using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class GatewayBillingLegacyIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestConsolGatewayBillingRatingWhenShipmentJobDepartmentIsFEAAndConsolDepartmentIsGEAGatewaySellWins_GatewayAgent()
		{
			TestConsolGatewayBillingRatingWhenShipmentJobDepartmentIsFEAAndConsolDepartmentIsGEAGatewaySellWins(AgentType.Agent);
		}

		public void TestConsolGatewayBillingRatingWhenShipmentJobDepartmentIsFEAAndConsolDepartmentIsGEAGatewaySellWins_GatewayCoLoad()
		{
			TestConsolGatewayBillingRatingWhenShipmentJobDepartmentIsFEAAndConsolDepartmentIsGEAGatewaySellWins(AgentType.CoLoad);
		}

		void TestConsolGatewayBillingRatingWhenShipmentJobDepartmentIsFEAAndConsolDepartmentIsGEAGatewaySellWins(string gatewayType)
		{
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, "NZAKL", "AUSYD", "WAR", 60);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "NZAKL", "AUSYD", "FRT", 33);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", gatewayType, true);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "NZAKL", "AUSYD", 150m, 3m);
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_FreightGatewaySellRateAutoratingMode = "AIN";
			consol.Shipments.Add(shipment);

			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
				var forwardingDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

				shipmentJob.JH_GE = forwardingDepartment.PK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 12500m,
							JR_Desc = "International Freight - Shipment EBM22Q33TU475BXH3P60"
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestConsolGatewayBillingRating_GatewayAgent()
		{
			TestConsolGatewayBillingRating(AgentType.Agent);
		}

		public void TestConsolGatewayBillingRating_GatewayCoLoad()
		{
			TestConsolGatewayBillingRating(AgentType.CoLoad);
		}

		void TestConsolGatewayBillingRating(string gatewayType)
		{
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, "NZAKL", "AUSYD", "WAR", 60);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "NZAKL", "AUSYD", "FRT", 33);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", gatewayType, true, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_HouseBill = "0012";
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 33m,
							JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
							RelatedJobNumber = "SHP00001",
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestAutorateRevenue_ChargeWithTheSameRelatedJobNumberExists_Update()
		{
			var rateLine = CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "NZAKL", "AUSYD", "FRT", 66);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", AgentType.Agent, true, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_UniqueConsignRef = "SHP00001";
			shipment.JS_HouseBill = "0012";
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 66m,
							JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
							RelatedJobNumber = "SHP00001",
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);

					rateLine.GetCalculator<FlatCalculator>().BaseRate = 99;

					Factory.Save();

					expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 99m,
							JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
							RelatedJobNumber = "SHP00001",
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestAutorateRevenue_RelatedJobNumberOnExistingChargeHasBeenManuallyUpdated_CreateNewCharge()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry = clientRate.AddRateEntry("AIR", "LSE", "NZAKL", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "KG", CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 1;
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(rateLine.ChargeCode);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", AgentType.Agent, true, PaymentType.Prepaid);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var shipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "NZAKL", "AUSYD", 5m, 0m);
			shipment1.JS_UniqueConsignRef = "SHP00001";
			shipment1.JS_HouseBill = "0012";

			var shipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "NZAKL", "AUSYD", 10, 0m);
			shipment2.JS_UniqueConsignRef = "SHP00002";
			shipment2.JS_HouseBill = "0013";

			var shipment3 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUMEL", "AUSYD", 10, 0m);
			shipment3.JS_UniqueConsignRef = "SHP00003";
			shipment3.JS_HouseBill = "0014";

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment1).TryCreateWithMutex());
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment2).TryCreateWithMutex());
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment3).TryCreateWithMutex());

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Env.CurrentBranch.PK, gatewayDepartment.PK.ToGuid()))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 5m,
						JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
						RelatedJobNumber = "SHP00001",
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						JR_Desc = "International Freight - Shipment SHP00002 (House Bill='0013')",
						RelatedJobNumber = "SHP00002",
					}
				};

				AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);

				var consolJob = new JobHeader.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly() as Job;
				var charge = consolJob.Charges.Cast<Charge>().First(c => c.JR_Calc_RelatedJobNumber == "SHP00001");
				charge.JR_Calc_RelatedJobNumber = "SHP00003";

				rateLine.GetCalculator<UnitCalculator>().PerUnit = 5;
				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						// Should not update the manually updated charge 
						ChargeCode = "FRT",
						JR_OSSellAmt = 5m,
						JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
						RelatedJobNumber = "SHP00003",
					},
					new AssertionCharge
					{
						// Should match the existing charge and update it
						ChargeCode = "FRT",
						JR_OSSellAmt = 50m,
						JR_Desc = "International Freight - Shipment SHP00002 (House Bill='0013')",
						RelatedJobNumber = "SHP00002",
					},
					new AssertionCharge
					{
						// Should create a new charge for the manually updated one 
						ChargeCode = "FRT",
						JR_OSSellAmt = 25m,
						JR_Desc = "International Freight - Shipment SHP00001 (House Bill='0012')",
						RelatedJobNumber = "SHP00001",
					},
				};

				AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy, job: consolJob, autorateCosts: false);
			}
		}

		public void TestConsolGatewayBillingWithGTWCondition_GatewayAgent()
		{
			TestConsolGatewayBillingWithGTWCondition(AgentType.Agent);
		}

		public void TestConsolGatewayBillingWithGTWCondition_GatewayCoLoad()
		{
			TestConsolGatewayBillingWithGTWCondition(AgentType.CoLoad);
		}

		void TestConsolGatewayBillingWithGTWCondition(string gatewayType)
		{
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, "DEFRA", "NZAKL", "WAR", 60);
			var rateLine = CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "DEFRA", "NZAKL", "FRT", 33);
			rateLine.TL_Condition = "GTW";

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, Consignee, "AUSYD", "NZAKL", "BAF", 15);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = TransportModes.Air;
			consol1.JK_ConsolMode = ContainerModes.Loose;
			consol1.JK_RL_NKLoadPort = "DEFRA";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol1.Transports[0].CarrierPK = Consignee.PK;
			consol1.JK_UniqueConsignRef = "AAAA";
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_OA_SendingForwarderAddress = Consignee.MainAddress.PK;

			var consol2 = CreateGatewayConsol("AUSYD", "NZAKL", gatewayType, true);
			consol2.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol2.Transports[0].CarrierPK = Consignee.PK;
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;

			var consolDocAddress = consol2.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "DEFRA", "NZAKL", 1m, 3m);

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			Factory.Save();

			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 33m,
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 15m,
						}
					};

					AutorateAndAssert(expected, consol2, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestGatewayBillingRatingPrepaidAgentWinsWhenConsolPaymentTypeCollectAndPrepaidFirstByPriority_GatewayAgent()
		{
			TestGatewayBillingRatingPrepaidAgentWinsWhenConsolPaymentTypeCollectAndPrepaidFirstByPriority(AgentType.Agent);
		}

		public void TestGatewayBillingRatingPrepaidAgentWinsWhenConsolPaymentTypeCollectAndPrepaidFirstByPriority_GatewayCoLoad()
		{
			TestGatewayBillingRatingPrepaidAgentWinsWhenConsolPaymentTypeCollectAndPrepaidFirstByPriority(AgentType.CoLoad);
		}

		void TestGatewayBillingRatingPrepaidAgentWinsWhenConsolPaymentTypeCollectAndPrepaidFirstByPriority(string gatewayType)
		{
			var priorities = new RatesPrioritiesCollection();
			priorities.AddNew(RatingDebtorOrgTypes.SAG);
			priorities.AddNew(RatingDebtorOrgTypes.CNE);
			priorities.AddNew(RatingDebtorOrgTypes.RAG);

			RatingDataRegistry.Instance.GatewayCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, "NZAKL", "AUSYD", "FRT", 23);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "NZAKL", "AUSYD", "FRT", 300);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", gatewayType, true, PaymentType.Collect);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_OH_DeliveryAgent = Consignee.PK;
			consol.Shipments.Add(shipment);

			using (new Job.Loader(shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 23m,
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestGatewaySellRatePrioritiesRegistryItemFiltersGatewaySellRates_GatewayAgent()
		{
			TestGatewaySellRatePrioritiesRegistryItemFiltersGatewaySellRates(AgentType.Agent);
		}

		public void TestGatewaySellRatePrioritiesRegistryItemFiltersGatewaySellRates_GatewayCoLoad()
		{
			TestGatewaySellRatePrioritiesRegistryItemFiltersGatewaySellRates(AgentType.CoLoad);
		}

		void TestGatewaySellRatePrioritiesRegistryItemFiltersGatewaySellRates(string gatewayType)
		{
			var priorities = new RatesPrioritiesCollection();
			priorities.AddNew(RatingDebtorOrgTypes.RAG);
			RatingDataRegistry.Instance.GatewayCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

			var origin = "SGSIN";
			var destination = "AUSYD";

			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, origin, destination, "FRT", 100, RateCategory.FCL, Constants.RateMode.SEA);

			var consol = CreateGatewayConsol(origin, destination, gatewayType, true, PaymentType.Collect, TransportModes.Sea, ContainerModes.Groupage);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, Consignee.PK, origin, destination, 850m);
			var shipment2 = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, Consignee.PK, origin, destination, 850m);
			consol.Shipments.AddRange(new[] { shipment, shipment2 });

			using (new Job.Loader(shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					var expected = Array.Empty<AssertionCharge>();
					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);

					var expectedLogLines = @"Information: RateLine Filtered FRT-FLT-Client Rate CONSIGNEE1	reason:	CNE organization is not listed in sell rates priority registry among Gateway Collect applicable organizations";
					AssertAutoratingAuditLogNoteContainsLines(consol, "Log should contain expected lines", expectedLogLines);
				}
			}

			DisposeJobShipmentForGatewayConsol(consol);
		}

		public void TestAutorateGatewaySellCostSetsGatewayAgentAsCreditor()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_TransportMode = TransportModes.Air;
			consol1.JK_ConsolMode = ContainerModes.Loose;
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "AUBNE";

			var notCurrentCompanyQuery = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code);
			var notCurrentCompany = Factory.LoadTop1<GlbCompany>(notCurrentCompanyQuery);
			AssertNotNull(notCurrentCompany);

			consol1.JK_OA_SendingForwarderAddress = notCurrentCompany.OrgProxy.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts = consol1.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = consol1.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var carrier = Helper.NewOrgHeader();
			consol1.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_ConsolMode = ContainerModes.Loose;
			consol2.JK_RL_NKLoadPort = "AUBNE";
			consol2.JK_RL_NKDischargePort = "ZAJNB";
			consol2.JK_Phase = "ALL";

			consol2.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			orgAppointedAgentPorts = consol2.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = consol2.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			carrier = Helper.NewOrgHeader();
			consol2.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			Assert("Sending agent must be the company/branch org proxy and appointed agent of the load/discharge port", !consol1.IsGateway());
			Assert(consol2.IsGateway());

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader().PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader().PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "ZAJNB";
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 20;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 10m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			Factory.Save();

			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var forwardingDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));

				shipmentJob.JH_GE = forwardingDepartment.PK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), forwardingDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							CostAccountCode = consol2.SendingForwarder.OH_Code,
							JR_OSCostAmt = 200m,
							JR_OSSellAmt = 200m,
						}
					};

					AutorateAndAssert(expected, shipment, GlbCompany.CurrentCompany.OrgProxy, null, null, false);
				}
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestConsolGatewayBillingChargeJobInternalInfo()
		{
			var branchA = CreateBranchProxy(NewClient, "AAA");
			var branchB = CreateBranchProxy(NewClient2, "BBB");

			NewClient2.OH_IsDebtor = true;
			Consignee.OH_IsCreditor = true;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("WAR");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			Helper.NewClientRateWithSingleRateLine(NewClient, "AIR", "LSE", "NZAKL", "AUSYD", "WAR", 60);
			Helper.NewClientRateWithSingleRateLine(Consignee, "AIR", "LSE", "NZAKL", "AUSYD", "FRT", 33);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine("AIR", "LSE", "NZAKL", "AUSYD", "BAF", 15, CurrencyCodes.Australia);

			Factory.Save();

			var consignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var shipment = CreateForwardingShipment(TransportModes.Air, consignorPK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_OH_DeliveryAgent = Consignee.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "NZAKL", "AUSYD", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.JK_UniqueConsignRef = "AAAA";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts.O5_PortOrCountry = "NZAKL";
			appointedAgentPorts.O5_OA_AgentOfficeAddress = orgAddress.PK;
			appointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts);

			AssertEquals(true, consol.IsGateway());

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = NewClient.MainAddress.PK;

			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchA.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				using (var consolJob = new Job.Loader(consol).TryLoadOrCreate())
				using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
				{
					consolJob.JH_GE = gatewayDepartment.PK;
					consolJob.JH_GB = branchA.PK;
					shipmentJob.JH_GB = branchB.PK;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 33m,
							JR_GB_InternalBranch = shipmentJob.JH_GB,
							JR_GE_InternalDept = shipmentJob.JH_GE,
							JR_JH_InternalJob = shipmentJob.PK,
							SellAccountCode = NewClient2.OH_Code,
							CostAccountCode = Consignee.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 15m,
							JR_GB_InternalBranch = consol.Job.JH_GB,
							JR_GE_InternalDept = consol.Job.JH_GE,
							JR_JH_InternalJob = consol.Job.PK,
							SellAccountCode = NewClient.OH_Code,
							CostAccountCode = TransportProvider1.OH_Code
						}
					};

					AutorateAndAssert(expected, consol, NewClient, null, consolJob);
				}
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestInternalJobDefaultingWithRegistrySettingWhenAutoRateRevenue()
		{
			var branchA = CreateBranchProxy(NewClient, "AAA");
			var branchB = CreateBranchProxy(NewClient2, "BBB");

			NewClient2.OH_IsDebtor = true;
			Consignee.OH_IsCreditor = true;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("WAR");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			Helper.NewClientRateWithSingleRateLine(NewClient, "AIR", "LSE", "NZAKL", "AUSYD", "WAR", 60);
			Helper.NewClientRateWithSingleRateLine(Consignee, "AIR", "LSE", "NZAKL", "AUSYD", "FRT", 33);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine("AIR", "LSE", "NZAKL", "AUSYD", "BAF", 15, CurrencyCodes.Australia);

			Factory.Save();

			var consignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var shipment1 = CreateForwardingShipment(TransportModes.Air, consignorPK, Consignee.PK, "NZCHC", "AUSYD", 1m, 3m);
			shipment1.JS_OH_DeliveryAgent = Consignee.PK;
			shipment1.JS_UniqueConsignRef = "S0001";

			var consol1 = CreateForwardingConsol(TransportModes.Air, "NZCHC", "NZAKL", TransportProvider1, shipment1);
			consol1.JK_PrepaidCollect = PaymentType.Prepaid;
			consol1.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var appointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts1.O5_PortOrCountry = "NZCHC";
			appointedAgentPorts1.O5_OA_AgentOfficeAddress = orgAddress1.PK;
			appointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol1.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts1);

			var shipment2 = CreateForwardingShipment(TransportModes.Air, consignorPK, Consignee.PK, "NZALR", "AUSYD", 1m, 3m);
			shipment2.JS_OH_DeliveryAgent = Consignee.PK;
			shipment2.JS_UniqueConsignRef = "S0002";

			var consol2 = CreateForwardingConsol(TransportModes.Air, "NZALR", "NZAKL", TransportProvider1, shipment2);
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;
			consol2.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedAgentPorts2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts2.O5_PortOrCountry = "NZALR";
			appointedAgentPorts2.O5_OA_AgentOfficeAddress = orgAddress2.PK;
			appointedAgentPorts2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol2.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts2);

			var shipment3 = CreateForwardingShipment(TransportModes.Air, consignorPK, Consignee.PK, "NZNPE", "AUSYD", 2m, 3m);
			shipment3.JS_OH_DeliveryAgent = Consignee.PK;
			shipment3.JS_UniqueConsignRef = "S0003";

			var consol3 = CreateForwardingConsol(TransportModes.Air, "NZNPE", "NZAKL", TransportProvider1, shipment3);
			consol3.JK_PrepaidCollect = PaymentType.Prepaid;
			consol3.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol3.JK_UniqueConsignRef = "C0003";
			consol3.JK_AgentType = AgentType.Agent;
			consol3.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;

			Factory.Save();

			var consolJob1 = new Job.Loader(consol1).TryLoadOrCreateWithoutMutexForTestOnly();
			var shipmentJob1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();

			var consolJob2 = new Job.Loader(consol2).TryLoadOrCreateWithoutMutexForTestOnly();
			var shipmentJob2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();

			var consolJob3 = new Job.Loader(consol3).TryLoadOrCreateWithoutMutexForTestOnly();
			var shipmentJob3 = new Job.Loader(shipment3).TryCreateWithoutMutexForTestOnly();

			var shipment4 = CreateForwardingShipment(TransportModes.Air, consignorPK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment4.JS_OH_DeliveryAgent = Consignee.PK;

			var consol4 = CreateForwardingConsol(TransportModes.Air, "NZAKL", "AUSYD", TransportProvider1, shipment4);
			consol4.JK_PrepaidCollect = PaymentType.Prepaid;
			consol4.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol4.JK_UniqueConsignRef = "C0004";
			consol4.JK_AgentType = AgentType.Agent;
			consol4.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol4.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol4.Shipments.Add(shipment1);
			consol4.Shipments.Add(shipment2);
			consol4.Shipments.Add(shipment3);

			var appointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts.O5_PortOrCountry = "NZAKL";
			appointedAgentPorts.O5_OA_AgentOfficeAddress = orgAddress.PK;
			appointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol4.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts);

			AssertEquals(true, consol4.IsGateway());

			var consolDocAddress = consol4.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = NewClient.MainAddress.PK;

			SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry();

			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchA.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				using (var consolJob4 = new Job.Loader(consol4).TryLoadOrCreateWithoutMutexForTestOnly())
				using (var shipmentJob4 = new Job.Loader(shipment4).TryCreateWithMutex())
				{
					consolJob4.JH_GE = gatewayDepartment.PK;
					consolJob4.JH_GB = branchA.PK;
					shipmentJob4.JH_GB = branchB.PK;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 33m,
							JR_GB_InternalBranch = shipmentJob4.JH_GB,
							JR_GE_InternalDept = shipmentJob4.JH_GE,
							JR_JH_InternalJob = shipmentJob4.PK,                // Previous sending agent is NON --> REL
							SellAccountCode = NewClient2.OH_Code,
							CostAccountCode = Consignee.OH_Code
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_JH_InternalJob = consolJob1.PK                   // Previous sending agent is GTT --> PCL
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_JH_InternalJob = consolJob2.PK                   // Previous sending agent is GTA --> PCL
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_JH_InternalJob = shipmentJob3.PK                 // Previous sending agent is SGT --> REL
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 15m,
							JR_GB_InternalBranch = consol4.Job.JH_GB,
							JR_GE_InternalDept = consol4.Job.JH_GE,
							JR_JH_InternalJob = consol4.Job.PK,
							SellAccountCode = NewClient.OH_Code,
							CostAccountCode = TransportProvider1.OH_Code
						}
					};

					AutorateAndAssert(expected, consol4, NewClient, null, consolJob4);
				}
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestConsolGatewayBillingChargeJobInternalInfoWhenChargeAlreadyExist()
		{
			var newBranch = CreateBranchProxy(NewClient, "AAA");

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("WAR");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			Helper.NewClientRateWithSingleRateLine(NewClient, "AIR", "LSE", "NZAKL", "AUSYD", "WAR", 60);
			Helper.NewClientRateWithSingleRateLine(Consignee, "AIR", "LSE", "NZAKL", "AUSYD", "FRT", 33);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine("AIR", "LSE", "NZAKL", "AUSYD", "BAF", 15, CurrencyCodes.Australia);

			Consignee.OH_IsCreditor = true;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_OH_DeliveryAgent = Consignee.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "NZAKL", "AUSYD", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.JK_UniqueConsignRef = "AAAA";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts.O5_PortOrCountry = "NZAKL";
			appointedAgentPorts.O5_OA_AgentOfficeAddress = orgAddress.PK;
			appointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts);

			AssertEquals(true, consol.IsGateway());

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = NewClient.MainAddress.PK;

			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				using (var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly())
				using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
				{
					consolJob.JH_GE = gatewayDepartment.PK;
					consolJob.JH_GB = newBranch.PK;
					shipmentJob.JH_GB = newBranch.PK;

					var existingCharge = consolJob.Charges.AddNew();
					existingCharge.JR_AC = Helper.ChargeCodes["FRT"].PK;
					existingCharge.JR_JH_InternalJob = consolJob.PK;
					existingCharge.JR_OrderReference = shipmentJob.JH_JobNum;
					existingCharge.JR_OH_SellAccount = ZGuid.Empty;
					existingCharge.ApplyCustomQuickCalculator(consol.RatingAdapter, 0, 33);

					AssertEquals(33m, existingCharge.JR_OSSellAmt);
					AssertEquals(true, existingCharge.JR_SellRatingOverride);
					AssertEquals(true, existingCharge.SellPaymentBases.Count > 0);

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							CostAccountCode = "CONSIGNEE1",
							SellAccountCode = "NEWTESSYD",
							JR_OSSellAmt = 33m,
							JR_GB_InternalBranch = shipmentJob.JH_GB,
							JR_GE_InternalDept = shipmentJob.JH_GE,
							JR_JH_InternalJob = shipmentJob.PK,
							JR_CostRatingOverride = false,
							JR_SellRatingOverride = false
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							CostAccountCode = "TRASPROV1",
							SellAccountCode = "NEWTESSYD",
							JR_OSSellAmt = 15m,
							JR_GB_InternalBranch = consol.Job.JH_GB,
							JR_GE_InternalDept = consol.Job.JH_GE,
							JR_JH_InternalJob = consol.Job.PK,
							JR_CostRatingOverride = false,
							JR_SellRatingOverride = false
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							CostAccountCode = "CONSIGNEE1",
							JR_OSSellAmt = 33m,
							JR_CostRatingOverride = true,
							JR_SellRatingOverride = true
						}
					};

					AutorateAndAssert(expected, consol, NewClient, null, consolJob);
				}
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestConsolGatewayBillingApportionmentChargeJobInternalInfo()
		{
			var newBranch = CreateBranchProxy(NewClient, "AAA");

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("WAR");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			Helper.NewClientRateWithSingleRateLine(NewClient, "AIR", "LSE", "USLAX", "AUSYD", "WAR", 60);
			Helper.NewClientRateWithSingleRateLine(Consignee, "AIR", "LSE", "USLAX", "AUSYD", "FRT", 33);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine("AIR", "LSE", "USLAX", "AUSYD", "BAF", 15, CurrencyCodes.Australia);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "USLAX", "AUSYD", 1, 3);
			shipment.JS_OH_DeliveryAgent = Consignee.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "USLAX", "AUSYD", TransportProvider1, shipment);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.JK_UniqueConsignRef = "AAAA";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = NewClient.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var appointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			appointedAgentPorts.O5_PortOrCountry = "USLAX";
			appointedAgentPorts.O5_OA_AgentOfficeAddress = orgAddress.PK;
			appointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPorts);

			AssertEquals(true, consol.IsGateway());

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = NewClient.MainAddress.PK;

			Factory.Save();

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				using (var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly(newBranch))
				using (var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly(newBranch))
				{
					consolJob.JH_GE = gatewayDepartment.PK;
					consolJob.JH_GB = newBranch.PK;
					shipmentJob.JH_GB = newBranch.PK;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 33m,
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 15m,
						}
					};

					AutorateAndAssert(expected, consol, NewClient);
					var charges = ((Job)consol.Job).Charges;
					AssertEquals(2, charges.Count);

					var chargeFromCost = charges.Cast<Charge>().First(x => x.ChargeCode.AC_Code == "BAF");
					AssertEquals(consol.Job.JH_GB, chargeFromCost.JR_GB_InternalBranch);
					AssertEquals(consol.Job.JH_GE, chargeFromCost.JR_GE_InternalDept);
					AssertEquals(consol.Job.PK, chargeFromCost.JR_JH_InternalJob);

					var chargeFromShipment = charges.Cast<Charge>().First(x => x.ChargeCode.AC_Code == "FRT");
					AssertEquals(shipmentJob.JH_GB, chargeFromShipment.JR_GB_InternalBranch);
					AssertEquals(shipmentJob.JH_GE, chargeFromShipment.JR_GE_InternalDept);
					AssertEquals(shipmentJob.PK, chargeFromShipment.JR_JH_InternalJob);

					Factory.Save();

					var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.CostSupporter.PK));
					AssertEquals(1, costs.Length);
					AssertEquals(1, costs[0].ApportionmentCharges.Count);

					var apportionmentCharge = costs[0].ApportionmentCharges[0];
					AssertEquals(apportionmentCharge.JR_GB_InternalBranch, consol.Job.JH_GB);
					AssertEquals(apportionmentCharge.JR_GE_InternalDept, consol.Job.JH_GE);
					AssertEquals(apportionmentCharge.JR_JH_InternalJob, consol.Job.PK);
				}
			}
		}

		#region RemoveChargesNotApplicableToPaymentTerm

		public void TestRemoveChargesNotApplicableToPaymentTerm_GivenNotUseIntercompanyTariffsToAutorateGatewayBilling_ThenShouldChargeClientRate()
		{
			AssertRemoveChargesNotApplicableToPaymentTerm
			(
				useIntercompanyTariffsToAutorateGatewayBilling: false,
				expectedCharges: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 100m,
					}
				},
				expectedLogLines: null,
				message: "GatewayPriorities is applicable hence ClientRate is not removed."
			);
		}

		public void TestRemoveChargesNotApplicableToPaymentTerm_UseIntercompanyTariffsToAutorateGatewayBilling_ThenShouldChargeInterCompanyTariff()
		{
			AssertRemoveChargesNotApplicableToPaymentTerm
			(
				useIntercompanyTariffsToAutorateGatewayBilling: true,
				expectedCharges: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 200m,
					}
				},
				expectedLogLines: null,
				message: "GatewayPriorities is not applicable hence InterCompanyTariff charge is not removed."
			);
		}

		void AssertRemoveChargesNotApplicableToPaymentTerm(bool useIntercompanyTariffsToAutorateGatewayBilling, AssertionCharge[] expectedCharges, string expectedLogLines, string message)
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useIntercompanyTariffsToAutorateGatewayBilling))
			{
				var priorities = new RatesPrioritiesCollection();
				priorities.AddNew(RatingDebtorOrgTypes.LC);
				RatingDataRegistry.Instance.GatewayCollectPriorities.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);

				var origin = "SGSIN";
				var destination = "AUSYD";

				CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, origin, destination, "FRT", 100m);
				CreateRateEntryWithRateLine(RatingHeaderTypes.IntercompanyTariff, GlbCompany.CurrentCompany.OrgProxy, origin, destination, "FRT", 200m);

				var consol = CreateGatewayConsol(origin, destination, AgentType.Agent, true, PaymentType.Collect);
				consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
				consol.Transports[0].CarrierPK = Consignee.PK;

				var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
				consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				Factory.Save();

				var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, origin, destination, 1m, 3m);
				shipment.JS_OH_DeliveryAgent = Consignee.PK;
				consol.Shipments.Add(shipment);

				using (new Job.Loader(shipment).TryCreateWithMutex())
				{
					var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

					using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
					{
						AutorateAndAssert(expectedCharges, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true);

						if (expectedLogLines != null)
						{
							AssertAutoratingAuditLogNoteContainsLines(consol, message, expectedLogLines);
						}
					}
				}
			}
		}

		#endregion

		public void TestWhenNoSellRatePrioritiesRegistryItemApplies_SellRatesAreFilteredOut()
		{
			var origin = "SGSIN";
			var destination = "AUSYD";
			var importer = Helper.NewOrgHeader();
			importer.OH_Code = "IMPORTER1";

			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, importer, "", destination, "DDOC", 100, RateCategory.DST, Constants.RateMode.ALL);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.NonContainerised;
			declaration.JE_RL_NKOrigin = origin;
			declaration.JE_RL_NKFinalDestination = destination;
			declaration.JE_ShipmentIncoTerm = IncoTerms.FreeOnBoard;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_EntryStatus = "DWC";
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Drawback;

			var declarationJob = new Job.Loader(declaration).TryCreateWithMutex();
			declarationJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CIS")).PK;

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, declaration, importer);

			var expectedLogLines = @"Information: RateLine Filtered DDOC-FLT-Client Rate IMPORTER1	reason:	could not establish sell rates priority registry for Unknown job direction";
			AssertAutoratingAuditLogNoteContainsLines(declaration, "Log should contain expected lines", expectedLogLines);
		}

		public void TestGatewayBillingRatingConsigneeWinsWhenConsolPaymentTypeCollect_GatewayAgent()
		{
			TestGatewayBillingRatingConsigneeWinsWhenConsolPaymentTypeCollect(AgentType.Agent);
		}

		public void TestGatewayBillingRatingConsigneeWinsWhenConsolPaymentTypeCollect_GatewayCoLoad()
		{
			TestGatewayBillingRatingConsigneeWinsWhenConsolPaymentTypeCollect(AgentType.CoLoad);
		}

		void TestGatewayBillingRatingConsigneeWinsWhenConsolPaymentTypeCollect(string gatewayType)
		{
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, GlbCompany.CurrentCompany.OrgProxy, "NZAKL", "AUSYD", "WAR", 60);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "NZAKL", "AUSYD", "FRT", 300);

			var consol = CreateGatewayConsol("NZAKL", "AUSYD", gatewayType, true, PaymentType.Collect);
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Factory.NewWithValidTestData<OrgHeader>().PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_PackingMode = ContainerModes.Loose;
			consol.Shipments.Add(shipment);

			using (new Job.Loader(shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 300m,
						}
					};

					AutorateAndAssert(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
				}
			}
		}

		public void TestJK_SendingForwarderHandlingType_GatewayErrorsOnCFSLoadList()
		{
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;
			if (consol.IsPackLoadList)
			{
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			}
			else
			{
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			}

			var gatewayAgentPort = consol.Forwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.Forwarder.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (var job = new JobHeader.Loader(Factory, consol).TryCreate())
			{
				job.JH_JobNum = ((IJobNumber)consol).JobNumber;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				consol.JK_AgentType = AgentType.Agent;

				AssertNoErrors("There should not be any messages about requiring agent type 'gtw' for pure CFS load lists attached to a job header", consol.JK_AgentTypeInfo);

				consol.JK_IsForwarding = true;
				var consolAsForwardingConsol = Factory.Load<ForwardingConsol>(consol.PK);

				var gateway = (IGateway)consolAsForwardingConsol;

				AssertNotEquals("Pre-condition: forwarder should be considered a valid gateway agent", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());

				consol.JK_SendingForwarderHandlingType = ZString.Empty;
				consolAsForwardingConsol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertHasErrorContaining(consolAsForwardingConsol.JK_SendingForwarderHandlingTypeInfo, "Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists");

				gatewayAgentPort.O5_PortOrCountry = "JPOSA";
				consolAsForwardingConsol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertEquals("Changing the port means this org proxy is no longer a gateway agent for this job", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());
			}
		}

		#region Gateway Sell Rate Priorities / Consol Sending and Receiving Agents are valid Debtors for Shipment

		public void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToClientRates_GatewayAgent()
		{
			TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToClientRates(AgentType.Agent);
		}

		public void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToClientRates_GatewayCoLoad()
		{
			TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToClientRates(AgentType.CoLoad);
		}

		void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToClientRates(string gatewayType)
		{
			var registryDefaults = RatingDataRegistry.Instance.GatewayCollectPriorities.Value;
			AssertEquals("Pre-condition", RatingDebtorOrgTypes.RAG, registryDefaults[0].RatingOrganizationType);
			AssertEquals("Pre-condition", RatingDebtorOrgTypes.CNE, registryDefaults[1].RatingOrganizationType);

			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, receivingAgent, "", "AU", "FRT", 10);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "", "AU", "FRT", 20);

			var consol = CreateGatewayConsol("CNSHA", "AUSYD", gatewayType, false);

			Assert("Precondition", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var shipment1 = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "CNSHA", "AUMEL", 850m);
			var shipment2 = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "CNSHA", "AUMEL", 850m);
			shipment1.JS_INCO = IncoTerms.ExWorks;
			shipment2.JS_INCO = IncoTerms.ExWorks;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.ToGuid()))
			{
				CreateJob(shipment1, shipment1.JS_UniqueConsignRef);
				CreateJob(shipment2, shipment2.JS_UniqueConsignRef);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in EDICUS client rate",
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in EDICUS client rate",
					}
				};

				var message = "Due to the sell rate priorties registry settings for Gateway Collect, we should prefer the Receiving Agent Rates to the Consignee Rates";
				AutorateAndAssert(message, expected, consol, null, receivingAgent);
			}
		}

		public void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToTariff_GatewayAgent()
		{
			TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToTariff(AgentType.Agent);
		}

		public void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToTariff_GatewayCoLoad()
		{
			TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToTariff(AgentType.CoLoad);
		}

		void TestConsolGatewayBilling_GatewayCollectPriorities_AppliesToTariff(string gatewayType)
		{
			var registryDefaults = RatingDataRegistry.Instance.GatewayCollectPriorities.Value;
			AssertEquals("Pre-condition", RatingDebtorOrgTypes.RAG, registryDefaults[0].RatingOrganizationType);
			AssertEquals("Pre-condition", RatingDebtorOrgTypes.CNE, registryDefaults[1].RatingOrganizationType);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Tariff, null, "", "AU", "FRT", 10);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignee, "", "AU", "FRT", 20);

			var consol = CreateGatewayConsol("CNSHA", "AUSYD", gatewayType, withSendingAgent: false);
			consol.ReceivingForwarder.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Assert("Precondition", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var shipment1 = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "CNSHA", "AUMEL", 850m);
			var shipment2 = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "CNSHA", "AUMEL", 850m);
			shipment1.JS_INCO = IncoTerms.ExWorks;
			shipment2.JS_INCO = IncoTerms.ExWorks;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.ToGuid()))
			{
				var job1 = CreateJob(shipment1, shipment1.JS_UniqueConsignRef);
				var job2 = CreateJob(shipment2, shipment2.JS_UniqueConsignRef);
				AssertEquals(Consignee, job1.LocalCharges);
				AssertEquals(Consignee, job2.LocalCharges);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: EDICUS)",
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: EDICUS)",
					}
				};

				var message = "Due to the sell rate priorties registry settings for Gateway Collect, we should prefer the Receiving Agent Rates to the Consignee Rates";
				AutorateAndAssert(message, expected, consol, null, consol.ReceivingForwarder);
			}
		}

		public void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToClientRates_GatewayAgent()
		{
			TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToClientRates(AgentType.Agent);
		}

		public void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToClientRates_GatewayCoLoad()
		{
			TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToClientRates(AgentType.CoLoad);
		}

		void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToClientRates(string gatewayType)
		{
			var registryDefaults = RatingDataRegistry.Instance.GatewayPrepaidPriorities.Value;
			AssertEquals("Precondition", RatingDebtorOrgTypes.SAG, registryDefaults[0].RatingOrganizationType);
			AssertEquals("Precondition", RatingDebtorOrgTypes.CNR, registryDefaults[1].RatingOrganizationType);

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, sendingAgent, "AU", "", "FRT", 10);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignor, "AU", "", "FRT", 20);

			var consol = CreateGatewayConsol("AUSYD", "CNSHA", gatewayType, true);

			Assert("Precondition", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var shipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUMEL", "CNSHA", 850m);
			var shipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUMEL", "CNSHA", 850m);
			shipment1.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment2.JS_INCO = IncoTerms.DeliveredDutyPaid;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.ToGuid()))
			{
				CreateJob(shipment1, shipment1.JS_UniqueConsignRef);
				CreateJob(shipment2, shipment2.JS_UniqueConsignRef);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in EDICUS client rate",
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in EDICUS client rate",
					}
				};

				var message = "Due to the sell rate priorties registry settings for Gateway Prepaid, we should prefer the Sending Agent Rates to the Consignor Rates";
				AutorateAndAssert(message, expected, consol, sendingAgent, null);
			}
		}

		public void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToTariffs_GatewayAgent()
		{
			TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToTariffs(AgentType.Agent);
		}

		public void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToTariffs_GatewayCoLoad()
		{
			TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToTariffs(AgentType.CoLoad);
		}

		void TestConsolGatewayBilling_GatewayPrepaidPriorities_AppliesToTariffs(string gatewayType)
		{
			var registryDefaults = RatingDataRegistry.Instance.GatewayPrepaidPriorities.Value;
			AssertEquals("Precondition", RatingDebtorOrgTypes.SAG, registryDefaults[0].RatingOrganizationType);
			AssertEquals("Precondition", RatingDebtorOrgTypes.CNR, registryDefaults[1].RatingOrganizationType);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Tariff, null, "AU", "", "FRT", 10);
			CreateRateEntryWithRateLine(RatingHeaderTypes.ClientRate, Consignor, "AU", "", "FRT", 20);

			var consol = CreateGatewayConsol("AUSYD", "CNSHA", gatewayType, true);
			consol.SendingForwarder.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Assert("Precondition", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var shipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUMEL", "CNSHA", 850m);
			var shipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, ZGuid.Empty, "AUMEL", "CNSHA", 850m);
			shipment1.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment2.JS_INCO = IncoTerms.DeliveredDutyPaid;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Factory.Save();

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.ToGuid()))
			{
				var job1 = CreateJob(shipment1, shipment1.JS_UniqueConsignRef);
				var job2 = CreateJob(shipment2, shipment2.JS_UniqueConsignRef);
				AssertEquals(Consignor, job1.LocalCharges);
				AssertEquals(Consignor, job2.LocalCharges);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: EDICUS)",
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = "Charge located in Company Tariff Level 1 (Linked to: EDICUS)",
					}
				};

				var message = "Due to the sell rate priorties registry settings for Gateway Prepaid, we should prefer the Sending Agent Rates to the Consignor Rates";
				AutorateAndAssert(message, expected, consol, consol.SendingForwarder, null);
			}
		}

		#endregion

		public void TestAutoRatingGatewayBillingOnConsol_JobIsNotSavedOnShipment_ShouldNotContinueAutoRating()
		{
			var consol = CreateGatewayConsol("SGSIN", "AUSYD", AgentType.Agent, true, PaymentType.Collect);
			consol.JK_UniqueConsignRef = "C00002222";
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "MYKUL", "AUSYD", 1m, 3m);
			shipment.JS_UniqueConsignRef = "SH0001112";

			consol.Shipments.Add(shipment);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment).Load());

			using (var shipmentJob = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			{
				var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, gatewayDepartment.PK.ToGuid()))
				{
					AutorateAndAssert("Autorating should not be executed", null, consol, GlbCompany.CurrentCompany.OrgProxy, autorateCosts: false,
						expectedErrors: new[]
						{
						@"Error Consol C00002222 (Master Bill='081') : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - Invoicing Job: You have created the job SH0001112 on another form, but haven't saved it yet.
	Please close or save other forms that use job SH0001112 to continue.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep."
						});
				}
			}
		}

		#region Charge Creditor

		public void TestAutorateGatewayConsol_HavingCarrierWithAppointedCarrierAgency_ShouldHaveRatesFromCarrier_ChargeCreditorShouldBeCarrier()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, carrier, "NZAKL", "AUSYD", "FRT", 100m);

			_ = NewAgencyCreditor(carrier);
			var consol = SetupGatewayConsolWithCarrierAndShipment(carrier, null);
			Factory.Save();

			// creditor should be carrier rate even if there is an agency
			var expectedResult = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					CostAccountCode = "CARRIER"
				}
			};

			using (var consolJob = new Job.Loader(consol).TryLoadOrCreate())
			{
				AutorateAndAssert(expectedResult, consol, NewClient, null, consolJob);
			}

			DisposeJobShipmentForGatewayConsol(consol);
		}

		public void TestAutorateGatewayConsol_HavingCarrierWithoutAppointedCarrierAgency_ShouldHaveRatesFromCarrier_ChargeCreditorShouldBeTheCarrier()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, carrier, "NZAKL", "AUSYD", "FRT", 100m);

			var consol = SetupGatewayConsolWithCarrierAndShipment(carrier, null);
			Factory.Save();

			// creditor should be the carrier for carrier rate if there is no agency
			var expectedResult = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					CostAccountCode = "CARRIER"
				}
			};

			using (var consolJob = new Job.Loader(consol).TryLoadOrCreate())
			{
				AutorateAndAssert(expectedResult, consol, NewClient, null, consolJob);
			}

			DisposeJobShipmentForGatewayConsol(consol);
		}

		public void TestAutorateGatewayConsol_CoLoad_ChargeCreditorShouldBeCoLoadWith()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var coloadOrg = NewCarrierCreditor("COLOADWITH");
			_ = NewAgencyCreditor(coloadOrg);
			var port1 = "AUSYD";
			var port2 = "USLAX";
			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, coloadOrg, port1, port2, "FRT", 100m);

			var consol = CreateGatewayConsol(port1, port2, AgentType.CoLoad, true, PaymentType.Prepaid);
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, port1, port2, 1000m);
			consol.Shipments.Add(shipment);
			shipment.Consols.AddNew();

			Factory.Save();

			Assert("Precondition", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					CostAccountCode = coloadOrg.OH_Code
				}
			};

			AutorateAndAssert("", expected, consol, null, null, autorateRevenue: false, autorateCosts: true);

			DisposeJobShipmentForGatewayConsol(consol);
		}

		#endregion

		void DisposeJobShipmentForGatewayConsol(ForwardingConsol consol)
		{
			foreach (var shipment in consol.CostSupporter?.ShipmentsList)
			{
				var jobToDisposes = Factory.Load<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));

				if (jobToDisposes.Length > 0)
				{
					foreach (var jobToDispose in jobToDisposes)
					{
						jobToDispose.Dispose();
					}
				}
			}
		}

		ForwardingConsol SetupGatewayConsolWithCarrierAndShipment(OrgHeader carrier, OrgHeader creditor)
		{
			var consol = CreateGatewayConsol("NZAKL", "AUSYD", AgentType.Agent, true, PaymentType.Collect);
			consol.SetDefaultShippingLineAddress(carrier);

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			shipment.JS_OH_DeliveryAgent = Consignee.PK;
			consol.Shipments.Add(shipment);

			if (creditor != null)
			{
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			}

			return consol;
		}

		ForwardingConsol CreateGatewayConsol(string origin
			, string destination
			, string agentType
			, bool withSendingAgent
			, string prepaidCollect = PaymentType.Prepaid
			, string transportMode = TransportModes.Air
			, string consolMode = ContainerModes.Loose)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = consolMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			if (!string.IsNullOrEmpty(prepaidCollect))
			{
				consol.JK_PrepaidCollect = prepaidCollect;
			}

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";

			consol.JK_AgentType = agentType;

			if (withSendingAgent)
			{
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				var appointedPortsForSendingAgent = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				appointedPortsForSendingAgent.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				appointedPortsForSendingAgent.O5_PortOrCountry = origin;
				appointedPortsForSendingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
				appointedPortsForSendingAgent.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForSendingAgent.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForSendingAgent.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForSendingAgent.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			}
			else
			{
				consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				var appointedPortsForReceivingAgent = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				appointedPortsForReceivingAgent.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
				appointedPortsForReceivingAgent.O5_PortOrCountry = destination;
				appointedPortsForReceivingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
				appointedPortsForReceivingAgent.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForReceivingAgent.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForReceivingAgent.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;
				appointedPortsForReceivingAgent.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			}

			Assert("consol.IsGateway", consol.IsGateway());

			return consol;
		}

		RateLine CreateRateEntryWithRateLine(string ratingHeaderType, OrgHeader client, ZString origin, ZString destination, ZString chargeCode, ZDecimal baseRate,
		string category = TransportModes.Air, string mode = ContainerModes.Loose)
		{
			RatingHeader ratingHeader = null;
			switch (ratingHeaderType)
			{
				case RatingHeaderTypes.ClientRate:
					ratingHeader = Helper.NewClientRate(client);
					break;
				case RatingHeaderTypes.Costing:
					ratingHeader = Helper.NewCosting(client);
					break;
				case RatingHeaderTypes.Tariff:
					ratingHeader = Factory.New<CompanyTariff>();
					break;
				case RatingHeaderTypes.IntercompanyTariff:
					ratingHeader = Helper.NewIntercompanyTariff(client);
					break;
				case RatingHeaderTypes.Quote:
					ratingHeader = Helper.NewQuote(client);
					break;
			}

			var rateEntry = ratingHeader.AddRateEntry(category, mode, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code, ZString.Empty, CurrencyCodes.Australia);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(rateLine.ChargeCode);

			return rateLine;
		}

		void SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry()
		{
			// Registry setting: Prev sending agent - Invoice Target Job
			// SGT - REL
			// GTA - PCL
			// GTT - PCL
			// NON - REL
			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting1 = collection.AddNew();
			setting1.ConsolDirection = "ALL";
			setting1.ConsolTransportMode = "ALL";
			setting1.PreviousSendingAgentType = "SGT";
			setting1.InvoiceTargetJobType = "REL";
			var setting2 = collection.AddNew();
			setting2.ConsolDirection = "ALL";
			setting2.ConsolTransportMode = "ALL";
			setting2.PreviousSendingAgentType = "GTA";
			setting2.InvoiceTargetJobType = "PCL";
			var setting3 = collection.AddNew();
			setting3.ConsolDirection = "ALL";
			setting3.ConsolTransportMode = "ALL";
			setting3.PreviousSendingAgentType = "GTT";
			setting3.InvoiceTargetJobType = "PCL";
			var setting4 = collection.AddNew();
			setting4.ConsolDirection = "ALL";
			setting4.ConsolTransportMode = "ALL";
			setting4.PreviousSendingAgentType = "NON";
			setting4.InvoiceTargetJobType = "REL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
	}
}
