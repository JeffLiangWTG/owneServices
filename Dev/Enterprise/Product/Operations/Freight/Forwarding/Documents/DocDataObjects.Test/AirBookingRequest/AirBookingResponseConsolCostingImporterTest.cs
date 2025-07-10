using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	public sealed class AirBookingResponseConsolCostingImporterTest : TestCaseWithFactory
	{
		public void TestImportConsolCosting()
		{
			var consol = CreateConsol("FRCDG", "USJFK");

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var consolCosts = GetSampleConsolCosts();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
			var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

			AssertMultilineASCIIEquals("there's no messages for user",
				string.Empty,
				string.Join("/r/n", messagesForUser));

			costs.Reload(true);
			AssertEquals("created cost from ABE response have been created", 1, costs.Count);

			AssertEquals("Consol Cost charge code", "FRT", costs[0].ChargeCode.AC_Code);
			AssertEquals("Consol Cost charge code", "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55", costs[0].E6_CostReference);
			AssertEquals("Consol Cost charge code", 1251.9m, costs[0].E6_LocalCostAmount);
			AssertEquals("Consol Cost charge code", "AUD", costs[0].E6_RX_NKCurrency);
			AssertEquals("Consol Cost charge code", "CHG", costs[0].E6_ApportionmentMethod);
			AssertEquals("Consol Cost charge code", "SPT", costs[0].E6_RatingBehaviour);
		}

		[TestDate(2024, 12, 02, 10, 02, 03)]
		public void TestImportConsolCosting_ConstructCostRateAuditDescription_FromDefaultFreightCharge()
		{
			var origin = "FRCDG";
			var destination = "USJFK";
			var consol = CreateConsol(origin, destination);

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var consolCosts = GetSampleConsolCosts();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
			var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

			Assert(messagesForUser.IsNullOrEmpty());

			costs.Reload(true);

			AssertEquals("created cost from ABE response have been created", 1, costs.Count);

			var loadedCost = costs[0];
			var costLine = consolCosts.ConsolCostLineCollection[0];

			var costCalculationDescription = loadedCost.CostCalculationDescription.ToAscii();

			var expectedDescriptionLines = $@"FRT
International Freight

Charge located in (AirlineConnect) Wise Costing - General Costing with the following details:
Mode:					AIR
Charge Code Group:			FRT
Rate Provider:				XABE
Origin:					FRCDG
Destination:				USJFK
Carrier Charge Codes:			FRT
Autorated for:				CON COBA0000690080
(Master bill=""607"")
Leg:					FRCDG-USJFK


Time:					02-Dec-24 10:02:03
";

			AssertContainsExactLinesInAnyOrder(expectedDescriptionLines, costCalculationDescription);
		}

		[TestDate(2024, 12, 02, 10, 02, 03)]
		public void TestImportConsolCosting_ConstructCostRateAuditDescription_FromUnicersalCharge()
		{
			var origin = "FRCDG";
			var destination = "USJFK";
			var consol = CreateConsol(origin, destination);

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_GC = Env.CurrentCompanyPK;
			newChargeCode.AC_Code = "CHW";
			newChargeCode.AC_Desc = "War is hell";
			newChargeCode.AC_ChargeType = ChargeType.Margin;
			newChargeCode.AC_RateCalculator = "FLT";
			newChargeCode.AC_ChargeGroup = "INS";

			var mapping = newChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = "CBT";

			Factory.Save();

			var consolCosts = GetSampleConsolCosts();
			var costLine = consolCosts.ConsolCostLineCollection[0];
			costLine.UniversalChargeCode = new UniversalChargeCode { Code = "CBT", Description = "Combat" };
			costLine.ChargeCode = new ChargeCode { Code = "CCC", Description = "Carrier Charge Code" };

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
			var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

			Assert(messagesForUser.IsNullOrEmpty());

			costs.Reload(true);

			AssertEquals("created cost from ABE response have been created", 1, costs.Count);

			var loadedCost = costs[0];
			var costCalculationDescription = loadedCost.CostCalculationDescription.ToAscii();

			var expectedDescriptionLines = $@"CHW
War is hell

Charge located in (AirlineConnect) Wise Costing - General Costing with the following details:
Mode:					AIR
Charge Code Group:			INS
Rate Provider:				XABE
Origin:					FRCDG
Destination:				USJFK
Universal Charge Codes:		CBT
Carrier Charge Codes:			CCC
Autorated for:				CON COBA0000690080
(Master bill=""607"")
Leg:					FRCDG-USJFK


Time:					02-Dec-24 10:02:03
";

			AssertContainsExactLinesInAnyOrder(expectedDescriptionLines, costCalculationDescription);
		}

		public void TestImportGatewayConsol_WhenLoginAsSendingAgentAndItIsGTT_ShouldLoadInGatewayBillingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var origin = "FRCDG";
				var destination = "USJFK";

				var sendingAgent = CreateBranchProxy(origin);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingAgent.branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					sendingAgent.proxy.OH_IsCreditor = true;
					var consol = CreateConsol(origin, destination);

					consol.JK_OA_SendingForwarderAddress = sendingAgent.proxy.MainAddress.PK;
					consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
					consol.JK_OA_ShippingLineAddress = sendingAgent.proxy.MainAddress.PK;
					SetupAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);

					Assert("Pre-condition: consol is Gateway", consol.IsGateway());

					Factory.Save();

					var consolCosts = GetSampleConsolCosts();

					var logger = new Mock<IXmlImportLogger>();
					var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
					var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

					var job = consol.Job as Job;
					var jobCharges = job.Charges;

					AssertMultilineASCIIEquals("there's no messages for user",
						string.Empty,
						string.Join("/r/n", messagesForUser));

					AssertEquals("Job charge from ABE response have been created", 1, jobCharges.Count);
					AssertEquals("Job charge code", "FRT", jobCharges[0].ChargeCode.AC_Code);
					AssertEquals("Job charge cost amount", 1251.9m, jobCharges[0].JR_LocalCostAmt);
					AssertEquals("Job charge currency", "AUD", jobCharges[0].JR_RX_NKCostCurrency);
					AssertEquals("Spot cost should be true", true, jobCharges[0].JR_IsSpotCost);
					var mutex = JobHeader.GetMutex_ForTestOnly(consol.PK);
					AssertEquals("Job should not release lock for now", true, mutex.IsLocked);
					AssertNoExceptionThrown("Factory can save successfully.", () => Factory.Save());
					AssertEquals("Job should release lock for now", false, mutex.IsLocked);
				}
			}
		}

		public void TestImportGatewayConsol_WhenLoginAsReceivingAgentAndItIsGTT_ShouldLoadInGatewayBillingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var origin = "FRCDG";
				var destination = "USJFK";

				var receivingAgent = CreateBranchProxy(destination);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, receivingAgent.branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					receivingAgent.proxy.OH_IsCreditor = true;
					var consol = CreateConsol(origin, destination);

					consol.JK_OA_ReceivingForwarderAddress = receivingAgent.proxy.MainAddress.PK;
					consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					consol.JK_OA_ShippingLineAddress = receivingAgent.proxy.MainAddress.PK;
					SetupAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

					Assert("Pre-condition: consol is Gateway", consol.IsGateway());

					Factory.Save();

					var consolCosts = GetSampleConsolCosts();

					var logger = new Mock<IXmlImportLogger>();
					var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
					var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

					AssertMultilineASCIIEquals("there's no messages for user",
						string.Empty,
						string.Join("/r/n", messagesForUser));

					var job = consol.Job as Job;
					var jobCharges = job.Charges;

					AssertEquals("Job charge from ABE response have been created", 1, jobCharges.Count);
					AssertEquals("Job charge code", "FRT", jobCharges[0].ChargeCode.AC_Code);
					AssertEquals("Job charge cost amount", 1251.9m, jobCharges[0].JR_LocalCostAmt);
					AssertEquals("Job charge currency", "AUD", jobCharges[0].JR_RX_NKCostCurrency);
					AssertEquals("Spot cost should be true", true, jobCharges[0].JR_IsSpotCost);
					var mutex = JobHeader.GetMutex_ForTestOnly(consol.PK);
					AssertEquals("Job should not release lock for now", true, mutex.IsLocked);
					AssertNoExceptionThrown("Factory can save successfully.", () => Factory.Save());
					AssertEquals("Job should release lock for now", false, mutex.IsLocked);
				}
			}
		}

		public void TestImportGatewayConsol_WhenLoginAsReceivingAgentAndItIsGTT_ShouldLoadInGatewayBillingTab_NoException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var origin = "FRCDG";
				var destination = "USJFK";

				var receivingAgent = CreateBranchProxy(destination);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, receivingAgent.branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					receivingAgent.proxy.OH_IsCreditor = true;
					var consol = CreateConsol(origin, destination);

					consol.JK_OA_ReceivingForwarderAddress = receivingAgent.proxy.MainAddress.PK;
					consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					consol.JK_OA_ShippingLineAddress = receivingAgent.proxy.MainAddress.PK;
					SetupAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

					Assert("Pre-condition: consol is Gateway", consol.IsGateway());

					Factory.Save();

					var consolCosts = GetSampleConsolCosts();

					var logger = new Mock<IXmlImportLogger>();
					var importer = new AirBookingResponseConsolCostingTestImporter(logger.Object, Factory);
					string[] messagesForUser = Array.Empty<string>();

					AssertNoExceptionThrown("Import without exception.", () =>
					{
						messagesForUser = importer.ImportConsolCosting(consolCosts, consol);
					});

					AssertMultilineASCIIEquals("Could not create job.",
					"Could not create job.",
					string.Join("/r/n", messagesForUser));
				}
			}
		}

		public void TestImportConsolCostingWithUniversalChargeCode()
		{
			var consol = CreateConsol("FRCDG", "USJFK");

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var consolCosts = GetSampleConsolCosts();
			consolCosts.ConsolCostLineCollection.Add(
				new ConsolCostLine
				{
					ChargeCode = new ChargeCode { Code = "WAR", Description = "War is hell" },
					UniversalChargeCode = new UniversalChargeCode { Code = "CBT", Description = "Combat" },
					SupplierReference = "4278d390-621c-475f-b42f-65ad536e15e7",
					CostOSAmount = 500.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				});

			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_GC = Env.CurrentCompanyPK;
			newChargeCode.AC_Code = "CHW";
			newChargeCode.AC_Desc = "War is hell";
			newChargeCode.AC_ChargeType = ChargeType.Margin;
			newChargeCode.AC_RateCalculator = "FLT";
			newChargeCode.AC_ChargeGroup = "INS";
			var mapping = newChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = "CBT";

			Factory.Save();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
			var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

			AssertMultilineASCIIEquals("there's no messages for user",
				string.Empty,
				string.Join("/r/n", messagesForUser));

			costs.Reload(true);
			AssertEquals("created cost from ABE response have been created", 2, costs.Count);

			AssertEquals("Consol Cost charge code", "FRT", costs[0].ChargeCode.AC_Code); // no universal charge code, works as before
			AssertEquals("Consol Cost charge code", "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55", costs[0].E6_CostReference);
			AssertEquals("Consol Cost charge code", 1251.9m, costs[0].E6_LocalCostAmount);
			AssertEquals("Consol Cost charge code", "AUD", costs[0].E6_RX_NKCurrency);
			AssertEquals("Consol Cost charge code", "CHG", costs[0].E6_ApportionmentMethod);
			AssertEquals("Consol Cost charge code", "SPT", costs[0].E6_RatingBehaviour);

			AssertEquals("Consol Cost charge code", "CHW", costs[1].ChargeCode.AC_Code); //universal charge code CBT mapped to CHW (in XML we have CBT as universal charge code)
			AssertEquals("Consol Cost charge code", "4278d390-621c-475f-b42f-65ad536e15e7", costs[1].E6_CostReference);
			AssertEquals("Consol Cost charge code", 500.9m, costs[1].E6_LocalCostAmount);
			AssertEquals("Consol Cost charge code", "AUD", costs[1].E6_RX_NKCurrency);
		}

		public void TestImportConsolCostingWithUniversalChargeCode_NoException()
		{
			var consol = CreateConsol("FRCDG", "USJFK");

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var consolCosts = GetSampleConsolCosts();
			consolCosts.ConsolCostLineCollection.Add(
				new ConsolCostLine
				{
					ChargeCode = new ChargeCode { Code = "WAR", Description = "War is hell" },
					UniversalChargeCode = new UniversalChargeCode { Code = "CBT", Description = "Combat" },
					SupplierReference = "4278d390-621c-475f-b42f-65ad536e15e7",
					CostOSAmount = 500.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				});

			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_GC = Env.CurrentCompanyPK;
			newChargeCode.AC_Code = "CHW";
			newChargeCode.AC_Desc = "War is hell";
			newChargeCode.AC_ChargeType = ChargeType.Margin;
			newChargeCode.AC_RateCalculator = "FLT";
			newChargeCode.AC_ChargeGroup = "INS";
			var mapping = newChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
			mapping.AUP_Code = "CBT";

			Factory.Save();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingTestImporter(logger.Object, Factory);
			string[] messagesForUser = Array.Empty<string>();

			AssertNoExceptionThrown("Import without exception.", () =>
			{
				messagesForUser = importer.ImportConsolCosting(consolCosts, consol);
			});

			AssertMultilineASCIIEquals("Could not import cost",
				"Could not import consol costing./r/nCould not import consol costing.",
				string.Join("/r/n", messagesForUser));
			costs.Reload(true);
			AssertEquals("created cost from ABE response have been created", 0, costs.Count);
		}

		public void TestImportConsolCostingWhenNonFreight_SelectsCurrentCompanyChargeCode()
		{
			var consol = CreateConsol("FRCDG", "USJFK");

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);

			consolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
			{
				new ConsolCostLine
				{
					ChargeCode = new ChargeCode { Code = "CHW" },
					UniversalChargeCode = new UniversalChargeCode { Code = "CBT", Description = "Combat" },
					SupplierReference = "4278d390-621c-475f-b42f-65ad536e15e7",
					CostOSAmount = 500.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				}
			});

			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			newChargeCode.AC_Code = "CHW";
			newChargeCode.AC_Desc = "Low Water Fee";

			var newChargeCode2 = Factory.New<AccChargeCode>();
			newChargeCode2.AC_GC = Env.CurrentCompanyPK;
			newChargeCode2.AC_Code = "CHW";
			newChargeCode2.AC_Desc = "Fixed Fee";

			var newChargeCode3 = Factory.New<AccChargeCode>();
			newChargeCode3.AC_GC = Env.CurrentCompanyPK;
			newChargeCode3.AC_Code = "WRMS";
			newChargeCode3.AC_Desc = "Miscellaneous Warehouse Fee";

			Factory.Save();

			var logger = new Mock<IXmlImportLogger>();
			var importer = new AirBookingResponseConsolCostingImporter(logger.Object, Factory);
			var messagesForUser = importer.ImportConsolCosting(consolCosts, consol);

			AssertMultilineASCIIEquals("there's no messages for user", string.Empty, string.Join("/r/n", messagesForUser));

			costs.Reload(true);

			AssertEquals("created cost from ABE response have been created", 1, costs.Count);
			AssertEquals("Consol Cost charge code", "CHW", costs[0].ChargeCode.AC_Code);
			AssertEquals("Consol Cost charge description", "Fixed Fee", costs[0].ChargeCode.AC_Desc);
		}

		ForwardingConsol CreateConsol(string origin, string destination)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Air;
			transport.JW_Status = "PLN";
			transport.JW_TransportType = "FL1";
			transport.JW_VoyageFlightForBinding = "EY345";
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_IsLinked = false;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = origin;
			shipment.JS_RL_NKDischargePort = destination;

			var forwardingExportAirDept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var shipmentJob = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
			shipmentJob.JH_GE = forwardingExportAirDept.PK;

			Factory.Save();

			return consol;
		}

		ConsolCosts GetSampleConsolCosts()
		{
			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			consolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
					{
						new ConsolCostLine
						{
							ChargeCode = new ChargeCode { Code = "FRT", Description = "International Freight" },
							SupplierReference = "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55",
							CostOSAmount = 1251.9m,
							CostOSCurrency = new Currency { Code = "AUD" },
							RatingBehaviour = new RatingBehaviour { Code = "SPT" },
							ApportionmentMethod = "CHG",
							ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
						}
					});

			return consolCosts;
		}

		void SetupAppointedGatewayAgentPorts(OrgAddress gatewayAgentAddress, ZString location, string handlingType)
		{
			var gatewayAgent = gatewayAgentAddress.Header;
			var agentPorts = gatewayAgent.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().FirstOrDefault(x => x.O5_PortOrCountry == location)
							?? gatewayAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = gatewayAgentAddress.PK;
			agentPorts.O5_PortOrCountry = location;
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_SeaAgentStatus = handlingType;
			agentPorts.O5_AirAgentStatus = handlingType;
			agentPorts.O5_RailAgentStatus = handlingType;
			agentPorts.O5_RoadAgentStatus = handlingType;
		}

		(OrgHeader proxy, GlbBranch branch) CreateBranchProxy(string unloco, GlbCompany company = null)
		{
			var proxy = CreateCreditor("PROXY" + unloco);
			proxy.OH_IsDebtor = true;
			proxy.MainAddress.OA_RL_NKRelatedPortCode = unloco;
			Factory.Save();

			var newBranch = company?.Branches.AddNew() ?? GlbCompany.CurrentCompany.Branches.AddNew();
			newBranch.GB_Code = unloco.Substring(1, 3);
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_OH_OrgProxy = proxy.PK;
			newBranch.GB_RL_NKHomePort = unloco;
			newBranch.Factory.Save();

			return (proxy, newBranch);
		}

		OrgHeader CreateCreditor(string code)
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = code;
			creditor.OH_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(false);

			return creditor;
		}

		class AirBookingResponseConsolCostingTestImporter : AirBookingResponseConsolCostingImporter
		{
			public AirBookingResponseConsolCostingTestImporter(IXmlImportLogger logger, BusinessObjectFactory factory) : base(logger, factory) { }

			internal override Job LoadOrCreateJob(ForwardingConsol consol)
			{
				throw new JobCreationException("It is just for test");
			}

			internal override JobConsolCost AddApportionment(ApportionmentListing apportionmentListing)
			{
				throw new JobCreationException("It is just for test");
			}
		}
	}
}
