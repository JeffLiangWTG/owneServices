using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolGatewayBillingSupporterTest : TestCaseWithFactory
	{
		#region Forwarding Consol Gateway Rules (Intergration Tests)

		public void TestGatewayAgentIsEitherOrBothTheSendingAndReceivingAgent_GatewayAgent()
		{
			TestGatewayAgentIsEitherTheSendingOrReceivingAgent(Constants.AgentType.Agent);
		}

		public void TestGatewayAgentIsEitherOrBothTheSendingAndReceivingAgent_GatewayCoLoad()
		{
			TestGatewayAgentIsEitherTheSendingOrReceivingAgent(Constants.AgentType.CoLoad);
		}

		///<summary>
		/// 1. The Gateway Agent organisation, hence GA, is applicable (for company) when:
		///		- Either or both of the Sending Agent organisation, hence SA, or Receiving Agent organisation, hence RA.
		///		- Appointed to either consol's load port for SA or discharge port for RA as a GA.
		///</summary>
		void TestGatewayAgentIsEitherTheSendingOrReceivingAgent(string gatewayType)
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals(false, consol.IsGateway());

				consol.JK_AgentType = gatewayType;

				AssertEquals(false, consol.IsGateway());

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				AssertEquals(false, consol.IsGateway());

				SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

				AssertEquals("Consol Sending Agent is now gateway agent port for load port", true, consol.IsGateway());

				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_RL_NKLoadPort = ZString.Empty;
				consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				AssertEquals("Consol Receiving Agent can act as Gateway Agent within the Company", true, consol.IsGateway());

				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				AssertEquals("Sending and Receiving Agent can both act as Gateway Agent within the same Company", true, consol.IsGateway());

				consol.JK_RL_NKDischargePort = ZString.Empty;

				AssertEquals("Precondition: Consol is gateway", true, consol.IsGateway());

				var org = Factory.NewWithValidTestData<OrgHeader>();
				SetGatewayAgentPortOnConsolFromSendingAgent(consol, org);

				AssertEquals("Is not gateway for any org, must be an org proxy of current company or a branch on common company", false, consol.IsGateway());

				var anotherCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var anotherCompany = Factory.LoadTop1<GlbCompany>(anotherCompanyQuery);

				Assert("Precondition: Another company exists", anotherCompany != null);

				var anotherCompanyBranch = anotherCompany.Branches.AddNew();
				SetGatewayAgentPortOnConsolFromSendingAgent(consol, anotherCompanyBranch.OrgProxy);

				AssertEquals("Agent type is still flagged as gateway ", gatewayType, consol.JK_AgentType);

				var failureReason = "Although consol's agent type is gateway and sending agent has gateway port listed, it is not an org proxy of the current branch." +
					"Only the current company's org proxy or an org proxy of a current company branch is considered a valid sending agent";

				AssertEquals(failureReason, false, consol.IsGateway());

				var currentCompanyBranch = GlbCompany.CurrentCompany.Branches.AddNew();
				SetGatewayAgentPortOnConsolFromSendingAgent(consol, currentCompanyBranch.OrgProxy);

				AssertEquals("Consol should be gateway as Sending Agent is org proxy for a branch of the current company", true, consol.IsGateway());
			}
		}

		public void TestGatewayIsEnabled_GatewayAgent()
		{
			TestGatewayIsEnabled(Constants.AgentType.Agent);
		}

		public void TestGatewayIsEnabled_GatewayCoLoad()
		{
			TestGatewayIsEnabled(Constants.AgentType.CoLoad);
		}

		///<summary>
		/// 2. Gateway Billing is enabled (for a company) when either
		///		- Consol type is (GTW or GCL) AND there is a Consol Job.
		///		- Consol type is (GTW or GCL) AND GA is not null (for company) AND no non-gateway consol costs exist (for company).
		///</summary>
		void TestGatewayIsEnabled(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			using (var job = Factory.NewJobForTesting<JobHeader>())
			{
				job.JH_JobNum = ((IJobNumber)consol).JobNumber;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = consol.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				Assert("Is considered gateway even without a Gateway Agent as a job exists and the consol type is AGT or CLD", consol.IsGateway());
			}

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Courier;
			consol.JK_RL_NKLoadPort = "AUBNE";
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);
			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);

			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.Constants.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			Factory.Save();

			Assert("Pre-condition", !consol.IsGateway());

			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert("Is Gateway is always true as it doesn't matter even if non-gateway consol costs exist", consol.IsGateway());
			AssertNoErrors(consol.JK_SendingForwarderHandlingTypeInfo);

			consolCost.Delete();
			consol.Validation.ValidateJK_AgentType();

			Assert("Now that consol costs have been deleted it's valid for gateway", consol.IsGateway());
		}

		public void TestGatewayBillingSupporterGetConsolJob_GatewayAgent()
		{
			TestGatewayBillingSupporterGetConsolJob(Constants.AgentType.Agent);
		}

		public void TestGatewayBillingSupporterGetConsolJob_GatewayCoLoad()
		{
			TestGatewayBillingSupporterGetConsolJob(Constants.AgentType.CoLoad);
		}

		///<summary>
		///3. Consol Job (for a company) is a job which is:
		///		- Attached to a Forwarding Consol. Consol may also be CFS but must be Forwarding.
		///		- For the company.
		///		- Not Legacy Gateway job.
		///</summary>
		void TestGatewayBillingSupporterGetConsolJob(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1001";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Assert("Is not considered a gateway because a job does not exist", !consol.IsGateway());
			using (var gatewayJob = Factory.NewJobForTesting<JobHeader>())
			{
				gatewayJob.JH_JobNum = ((IJobNumber)consol).JobNumber;
				gatewayJob.JH_ParentID = consol.PK;
				gatewayJob.JH_ParentTableCode = consol.Prefix;
				gatewayJob.JH_GB = GlbBranch.CurrentBranch.PK;
				gatewayJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				gatewayJob.JH_GC = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				AssertEquals("Gateway agent should be null", ((IOrgHeader)null, (IOrgHeader)null), ((IGateway)consol).GatewayBillingSupporter.GatewayAgent());

				consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				Factory.Save();
				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

				Assert("Consol is CFS", consol.JK_IsCFS);
				Assert("Is considered gateway even without a Gateway Agent as a job exists", consol.IsGateway());
				Assert("Not a legacy Gateway Job", !consol.IsLegacyGateway);
			}

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(query);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Assert("Consol is not considered a gateway as no jobs are linked to this consol for this company.", !consol.IsGateway());
			}
		}

		public void TestGatewayValidationWhenConsolIsGatewayAgentType_GatewayAgent()
		{
			TestGatewayValidationWhenConsolIsGatewayAgentType(Constants.AgentType.Agent);
		}

		public void TestGatewayValidationWhenConsolIsGatewayAgentType_GatewayCoLoad()
		{
			TestGatewayValidationWhenConsolIsGatewayAgentType(Constants.AgentType.CoLoad);
		}

		/// <summary>
		/// 4. Gateway Agent or Gateway CoLoad in a Consol invalidates when either: 
		///		- Gateway Agent is null for all companies or Gateway Agent has been cancelled out by being both Sending Agent and Receiving Agent in any company.
		///		- There are non-gateway consol costs present for any company with a Gateway Agent.
		///		- There is an existing legacy gateway job.
		///		- There is no Gateway Agent but there is Gateway Sell job header.
		/// </summary>
		void TestGatewayValidationWhenConsolIsGatewayAgentType(string gatewayType)
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_AgentType = gatewayType;
				consol.JK_RL_NKLoadPort = "GBSUN";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.Transports[0].JW_IsLinked = false;
				consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				var expectedMessage = @"Sending Agent can be set as a Gateway Agent on a Consol if this Agent is:
- configured as a Gateway Agent for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy";

				AssertHasError("Has error as there are no sending/receiving agents to be gateway agent", consol.JK_SendingForwarderHandlingTypeInfo, expectedMessage);

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "C000094912";
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;
				var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				gatewayAgentPort.O5_PortOrCountry = "AUBNE";
				gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
				var freightChargeCode = Env.Registry.FreightChargeCode;
				consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);

				try
				{
					consolCost[JobConsolCostSchema.Constants.E6_ParentID] = consol.PK;
					consolCost[JobConsolCostSchema.Constants.E6_ParentTableCode] = consol.TablePrefix;
				}
				finally
				{
					consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}

				consolCost[JobConsolCostSchema.Constants.E6_GC] = GlbCompany.CurrentCompany.PK;
				consolCost[JobConsolCostSchema.Constants.E6_AC_ChargeCode] = freightChargeCode;
				consol.JK_AgentType = gatewayType;

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoErrors(consol.JK_SendingForwarderHandlingTypeInfo);
				consol.JK_SendingForwarderHandlingType = ZString.Empty;

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				AssertNoErrors(consol.JK_AgentTypeInfo);

				consol.JK_UniqueConsignRef = "C190505551";
				consol.JK_RL_NKLoadPort = "GBSUN";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.Transports[0].JW_IsLinked = false;
				consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;

				using (var job = Factory.NewJobForTesting<JobHeader>())
				{
					job.JH_JobNum = consol.JK_UniqueConsignRef + Constants.GatewaySuffixForJobHeaderDeprecated;
					job.JH_ParentID = consol.PK;
					job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job.JH_GC = GlbCompany.CurrentCompany.PK;
					Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
					Factory.Save();
					consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

					Assert("Precondition: Job is a GatewayLegacyJob", job.IsGatewayLegacyJob);
					Assert("Is not considered gateway because a legacy job exists", !consol.IsGateway());
					AssertNoError(consol.JK_AgentTypeInfo, expectedMessage);
				}

				var newCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var newCompany = Factory.LoadTop1<GlbCompany>(newCompanyQuery);
				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_AgentType = gatewayType;
				consol.JK_UniqueConsignRef = "C190251";
				consol.JK_RL_NKLoadPort = "GBSUN";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;
				consol.JK_OA_ReceivingForwarderAddress = newCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				using (var job = Factory.NewJobForTesting<JobHeader>())
				{
					job.JH_JobNum = ((IJobNumber)consol).JobNumber;
					job.JH_ParentID = consol.PK;
					job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job.JH_GC = GlbCompany.CurrentCompany.PK;
					Factory.Save();

					Assert("Precondition: Job is a GatewayBillingJob", job.IsGatewayBillingJob());
					AssertHasError(consol.JK_SendingForwarderHandlingTypeInfo, expectedMessage);
				}
			}
		}

		public void TestGatewayValidationWhenConsolIsNotGatewayAgentOrGatewayCoLoad_GatewayAgent()
		{
			TestGatewayValidationWhenConsolIsNotGatewayAgentOrGatewayCoLoad(Constants.AgentType.Agent);
		}

		public void TestGatewayValidationWhenConsolIsNotGatewayAgent_GatewayCoLoad()
		{
			TestGatewayValidationWhenConsolIsNotGatewayAgentOrGatewayCoLoad(Constants.AgentType.CoLoad);
		}

		/// <summary>
		/// 5. Consols that are not GTW or GCL invalidate when both:
		///		- Job for a Forwarding Consol exists for any company.
		///		- Parent Consol is not CFS OR there is a Gateway Agent for the Job company.
		/// </summary>
		void TestGatewayValidationWhenConsolIsNotGatewayAgentOrGatewayCoLoad(string gatewayType)
		{
			var newCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var newCompany = Factory.LoadTop1<GlbCompany>(newCompanyQuery);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = gatewayType;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = newCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

			Assert("Precondition: Consol is a legal GTW or GCL", consol.IsGateway());

			using (var job = Factory.NewJobForTesting<JobHeader>())
			{
				job.JH_JobNum = ((IJobNumber)consol).JobNumber;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
				Factory.Save();
				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

				Assert("Precondition: Job is a GatewayBillingJob", job.IsGatewayBillingJob());
				Assert("Precondition: Consol is a legal GTW or GCL", consol.IsGateway());
				AssertEquals("Precondition: Gateway has no errors", false, consol.HasRowErrors || consol.HasRowNotifications);

				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
				var expectedError = "Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company EDI.";
				consol.JK_OA_PackDepotAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
				AssertHasErrorContaining(consol.JK_ReceivingForwarderHandlingTypeInfo, expectedError);
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				SetGatewayAgentPortOnConsolFromReceivingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);
			}
		}

		//Tests 6 & 7 require GUI - See GatewayBillingForForwardingConsolTest

		public void TestLegacyConsol_GatewayAgent()
		{
			TestLegacyConsol(Constants.AgentType.Agent);
		}

		public void TestLegacyConsol_GatewayCoLoad()
		{
			TestLegacyConsol(Constants.AgentType.CoLoad);
		}

		/// <summary>
		/// 8. Legacy Gateway Consols cannot be converted into GTW or GCL Consols.
		/// </summary>
		void TestLegacyConsol(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			using (var job = Factory.NewJobForTesting<JobHeader>())
			{
				job.JH_JobNum = consol.JK_UniqueConsignRef + Constants.GatewaySuffixForJobHeaderDeprecated;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				consol.JK_AgentType = gatewayType;
				Factory.Save();
				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

				Assert("Precondition: Job is a GatewayLegacyJob", job.IsGatewayLegacyJob);
				Assert("Is not considered gateway because a legacy job exists", !consol.IsGateway());
			}
		}

		public void TestGatewayFreightSellRate_GatewayAgent()
		{
			TestGatewayFreightSellRate(Constants.AgentType.Agent);
		}

		public void TestGatewayFreightSellRate_GatewayCoLoad()
		{
			TestGatewayFreightSellRate(Constants.AgentType.CoLoad);
		}

		/// <summary>
		///9. Spot Shipment Gateway Sell Cost is enabled when a Shipment is attached to a Consol when the Sending Agent is the Gateway Agent or Gateway Co-load
		/// </summary>
		void TestGatewayFreightSellRate(string gatewayType)
		{
			var expectedNotification =
				@"There is a Gateway Sell amount entered for this shipment, but no Sending Agent identified as Gateway Agent on related consols.
Please setup the Sending Agent as the Gateway Agent for the transhipment port in Maintain>Reference Files>Organizations>Forwarder>Gateway Agent";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GatewayFreightSellRate = 10m;

			AssertNoWarnings(shipment.JS_GatewayFreightSellRateInfo);

			shipment.Consols.AddNew();
			shipment.RunPreSaveValidation();

			AssertHasWarning(shipment.JS_GatewayFreightSellRateInfo, expectedNotification);

			var newCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var newCompany = Factory.LoadTop1<GlbCompany>(newCompanyQuery);

			var consol = shipment.Consols[0];
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = gatewayType;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = newCompany.OrgProxy.MainAddress.PK;
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, consol.SendingForwarder);

			Assert("Precondition: Consol is a legal GTW or GCL", consol.IsGateway());

			shipment.RunPreSaveValidation();

			AssertNoWarning(shipment.JS_GatewayFreightSellRateInfo, expectedNotification);

			consol.SendingForwarder.AppointedGatewayAgentPorts.RemoveAll();
			consol.SetDefaultSendingForwarderAddress();
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, consol.ReceivingForwarder);
			shipment.RunPreSaveValidation();

			AssertHasWarning(shipment.JS_GatewayFreightSellRateInfo, expectedNotification);
		}

		#endregion

		#region Unit Tests

		public void TestGatewayAgent_SendingAgent_GatewayAgent()
		{
			TestGatewayAgent_SendingAgent(Constants.AgentType.Agent);
		}

		public void TestGatewayAgent_SendingAgent_GatewayCoLoad()
		{
			TestGatewayAgent_SendingAgent(Constants.AgentType.CoLoad);
		}

		void TestGatewayAgent_SendingAgent(string gatewayType)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_AgentType = gatewayType;
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

			var gateway = (IGateway)consol;

			AssertEquals("Sending agent must be appointed for load port", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_RL_NKLoadPort = "AUBNE";

			AssertEquals(((IOrgHeader)consol.SendingForwarder, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			var currentCompanyBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, currentCompanyBranch.OrgProxy);

			AssertEquals(((IOrgHeader)consol.SendingForwarder, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			var anotherCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(anotherCompanyQuery);

			Assert("Precondition: Another company exists", anotherCompany != null);
			var branchForAnotherCompany = anotherCompany.Branches.AddNew();
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, branchForAnotherCompany.OrgProxy);

			AssertEquals("Is not considered gateway for another company's orgproxy", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());
		}

		public void TestGatewayAgent_ReceivingAgent_GatewayAgent()
		{
			TestGatewayAgent_ReceivingAgent(Constants.AgentType.Agent);
		}

		public void TestGatewayAgent_GatewayCoLoad()
		{
			TestGatewayAgent_ReceivingAgent(Constants.AgentType.CoLoad);
		}

		void TestGatewayAgent_ReceivingAgent(string gatewayType)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_AgentType = gatewayType;
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

			var gateway = (IGateway)consol;

			AssertEquals("Receiving agent must be appointed for discharge port", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "DEFRA";

			AssertEquals(((IOrgHeader)null, (IOrgHeader)consol.ReceivingForwarder), gateway.GatewayBillingSupporter.GatewayAgent());

			var currentCompanyBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, currentCompanyBranch.OrgProxy);

			AssertEquals(((IOrgHeader)null, (IOrgHeader)consol.ReceivingForwarder), gateway.GatewayBillingSupporter.GatewayAgent());

			var anotherCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(anotherCompanyQuery);

			Assert("Precondition: Another company exists", anotherCompany != null);
			var branchForAnotherCompany = anotherCompany.Branches.AddNew();
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, branchForAnotherCompany.OrgProxy);

			AssertEquals("Is not considered gateway for another company's orgproxy", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());
		}

		public void TestGatewayAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var gateway = (IGateway)consol;

			AssertEquals(((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals(((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			consol.JK_RL_NKLoadPort = "AUBNE";
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);
			AssertEquals(((IOrgHeader)consol.SendingForwarder, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

			consol.JK_RL_NKDischargePort = "DEFRA";
			var currentCompanyBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, currentCompanyBranch.OrgProxy);
			AssertEquals(((IOrgHeader)consol.SendingForwarder, (IOrgHeader)consol.ReceivingForwarder), gateway.GatewayBillingSupporter.GatewayAgent());
		}

		public void TestIsConsolSendingAgentActingAsGateway()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(false, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			AssertEquals(false, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

			AssertEquals(true, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			var anotherCompanyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(anotherCompanyQuery);

			Assert("Precondition: Another company exists", anotherCompany != null);

			var anotherBranch = anotherCompany.Branches.AddNew();
			var anotherBranchOrg = Factory.NewWithValidTestData<OrgHeader>();
			anotherBranch.GB_OH_OrgProxy = anotherBranchOrg.PK;
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, anotherBranchOrg);

			AssertEquals(true, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbBranch.CurrentBranch.OrgProxy);

			AssertEquals(true, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));

			shipment.Consols.AddNew();

			AssertEquals("Shipment depends on any consol, not all to be gateway", true, FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(shipment));
		}

		public void TestIsLegacyGateway()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C11111111";

			Assert(!consol.IsLegacyGateway);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = consol;
			job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;

			Factory.Save();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK); // Reload Consol in a new Factory to avoid caching IsLegacyGateway value

			Assert(consol.IsLegacyGateway);
		}

		public void TestIsGatewayWithNoNonGatewayConsolCosts_GatewayAgent()
		{
			TestIsGatewayWithNoNonGatewayConsolCosts(Constants.AgentType.Agent);
		}

		public void TestIsGatewayWithNoNonGatewayConsolCosts_GatewayCoLoad()
		{
			TestIsGatewayWithNoNonGatewayConsolCosts(Constants.AgentType.CoLoad);
		}

		void TestIsGatewayWithNoNonGatewayConsolCosts(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Courier;
			consol.JK_UniqueConsignRef = "C1002";
			consol.JK_RL_NKLoadPort = "AUBNE";
			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy);

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.Constants.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;

			Factory.Save();

			Assert("Pre-condition", !consol.IsGateway());

			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Assert("Should be Gateway enabled even if non-gateway consol costs exist", consol.IsGateway());
			AssertNoErrors(consol.JK_SendingForwarderHandlingTypeInfo);
		}

		public void TestGatewayStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USJFK";

			var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;

			var port = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var gateway = (IGateway)consol;

			AssertEquals("Correct Gateway Type is returned", "GTA", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());

			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertEquals("Correct Gateway Type is returned", "GTT", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());
		}

		public void TestGatewayStatusMatchesAgainstAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USJFK";

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var otherAddress = orgProxy.Addresses.OfType<OrgAddress>().FirstOrDefault(address => !address.IsMainAddress);
			consol.JK_OA_SendingForwarderAddress = otherAddress.PK;

			Factory.Save();

			var port = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = orgProxy.MainAddress.PK;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var gateway = (IGateway)consol;

			AssertEquals("Should not find gateway type", string.Empty, ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());

			consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

			AssertEquals("Should find gateway type", "GTA", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());
		}

		public void TestGatewayStatus_Fallbacks()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USJFK";

			var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;

			var port1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port1.O5_PortOrCountry = "AU";
			port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var port2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port2.O5_PortOrCountry = "AUSYD";
			port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			var gateway = (IGateway)consol;

			AssertEquals("Correct Gateway Type is returned", "GTT", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());

			port2.O5_PortOrCountry = "AUBNE";

			AssertEquals("Correct Gateway Type is returned", "GTA", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());
		}

		public void TestGatewayStatus_FallbacksWhenPortMatches()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USJFK";

			var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;

			var port1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port1.O5_PortOrCountry = "AU";
			port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var port2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port2.O5_PortOrCountry = "AUSYD";
			port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port2.O5_SeaAgentStatus = string.Empty;

			var gateway = (IGateway)consol;

			AssertEquals("Correct Gateway Type is returned", "GTA", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());

			port2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertEquals("Correct Gateway Type is returned", "GTT", ((ForwardingConsolGatewayBillingSupporter)gateway.GatewayBillingSupporter).GetDefaultSendingForwarderAddressGatewayType());
		}

		#region Gateway Agent matches with consol's transport mode and direction

		public void TestSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection()
		{
			foreach (var consolTransportMode in new[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Road
			})
			{
				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode);
				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					agentStatus: AgentStatusList.Codes.GatewayAgentWithTariff);

				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					AgentDirectionList.Codes.Export);
				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					AgentDirectionList.Codes.Export,
					AgentStatusList.Codes.GatewayAgentWithTariff);

				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(false,
					consolTransportMode,
					AgentDirectionList.Codes.Import);
				AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(false,
					consolTransportMode,
					AgentDirectionList.Codes.Import,
					AgentStatusList.Codes.GatewayAgentWithTariff);
			}
		}

		public void TestReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection()
		{
			foreach (var consolTransportMode in new[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Road
			})
			{
				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode);
				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					agentStatus: AgentStatusList.Codes.GatewayAgentWithTariff);

				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					AgentDirectionList.Codes.Import);
				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(true,
					consolTransportMode,
					AgentDirectionList.Codes.Import,
					AgentStatusList.Codes.GatewayAgentWithTariff);

				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(false,
					consolTransportMode,
					AgentDirectionList.Codes.Export);
				AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(false,
					consolTransportMode,
					AgentDirectionList.Codes.Export,
					AgentStatusList.Codes.GatewayAgentWithTariff);
			}
		}

		void AssertSendingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(bool isGatewayAgentExpected,
			string transportMode,
			string agentDirection = AgentDirectionList.Codes.Both,
			string agentStatus = AgentStatusList.Codes.GatewayAgent)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";

			SetGatewayAgentPortOnConsolFromSendingAgent(consol, GlbCompany.CurrentCompany.OrgProxy, agentDirection, agentStatus, true);

			var expectedGatewayAgent = isGatewayAgentExpected ? consol.SendingForwarder : null;
			AssertEquals(((IOrgHeader)expectedGatewayAgent, (IOrgHeader)null), ((IGateway)consol).GatewayBillingSupporter.GatewayAgent());
		}

		void AssertReceivingAgentActsAsGatewayAgent_MatchConsolTransportModeAndDirection(bool isGatewayAgentExpected,
			string transportMode,
			string agentDirection = AgentDirectionList.Codes.Both,
			string agentStatus = AgentStatusList.Codes.GatewayAgent)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";

			SetGatewayAgentPortOnConsolFromReceivingAgent(consol, GlbCompany.CurrentCompany.OrgProxy, agentDirection, agentStatus, true);

			var expectedGatewayAgent = isGatewayAgentExpected ? consol.ReceivingForwarder : null;
			AssertEquals(((IOrgHeader)null, (IOrgHeader)expectedGatewayAgent), ((IGateway)consol).GatewayBillingSupporter.GatewayAgent());
		}

		#endregion

		#endregion

		#region Implementation

		public static void SetGatewayAgentPortOnConsolFromSendingAgent(ForwardingConsol consol,
			OrgHeader sendingAgent,
			string agentDirection = AgentDirectionList.Codes.Both,
			string agentStatus = AgentStatusList.Codes.GatewayAgent,
			bool removeExistedAgentPorts = false)
		{
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = agentStatus;

			if (removeExistedAgentPorts)
			{
				consol.SendingForwarder.AppointedGatewayAgentPorts.RemoveAndDeleteAll();
			}

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = agentDirection;

			switch (consol.JK_TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					gatewayAgentPort.O5_SeaAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Air:
					gatewayAgentPort.O5_AirAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Rail:
					gatewayAgentPort.O5_RailAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Road:
					gatewayAgentPort.O5_RoadAgentStatus = agentStatus;
					break;
			}
		}

		static void SetGatewayAgentPortOnConsolFromReceivingAgent(ForwardingConsol consol,
			OrgHeader receivingAgent,
			string agentDirection = AgentDirectionList.Codes.Both,
			string agentStatus = AgentStatusList.Codes.GatewayAgent,
			bool removeExistedAgentPorts = false)
		{
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = agentStatus;

			if (removeExistedAgentPorts)
			{
				consol.ReceivingForwarder.AppointedGatewayAgentPorts.RemoveAndDeleteAll();
			}

			var gatewayAgentPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "DEFRA";
			gatewayAgentPort.O5_AgentDirection = agentDirection;

			switch (consol.JK_TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					gatewayAgentPort.O5_SeaAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Air:
					gatewayAgentPort.O5_AirAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Rail:
					gatewayAgentPort.O5_RailAgentStatus = agentStatus;
					break;
				case Core.Constants.TransportModes.Road:
					gatewayAgentPort.O5_RoadAgentStatus = agentStatus;
					break;
			}
		}

		#endregion
	}
}
