using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AutoRatingDateFilteringIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestAutoRatingWithStandardJobDateRegistryMode()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Standard
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-2);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(2);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			shipment.JS_E_DEP = new ZDateTime(2013, 1, 1);

			AutorateAndAssert(new List<AssertionCharge>(), shipment, localClient);
		}

		public void TestAutoRatingWithArrivalJobDateRegistryMode()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Arrival
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-2);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(2);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20, 2);
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			shipment.JS_E_DEP = ZDateTime.Empty;
			AutorateAndAssert("Arrival date is empty and so Departure Date as standard date filtering is applied", expected, shipment, localClient);

			shipment.JS_E_DEP = new ZDateTime(2013, 1, 1);
			AutorateAndAssert("Arrival date is empty and so Departure Date as standard date filtering is applied", new List<AssertionCharge>(), shipment, localClient);

			shipment.JS_E_ARV = new ZDateTime(2013, 1, 1);
			AutorateAndAssert(new List<AssertionCharge>(), shipment, localClient);

			shipment.JS_E_ARV = ZDateTime.Empty;
			shipment.JS_E_DEP = ZDateTime.Empty;
			AutorateAndAssert("No charge should be filtered since both arrival and departure are empty", expected, shipment, localClient);
		}

		public void TestAutoRatingWithDepartureJobDateRegistryMode()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Departure
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-2);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(2);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20, 2);
			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			shipment.JS_E_ARV = new ZDateTime(2013, 1, 1);
			AutorateAndAssert(expected, shipment, localClient);

			shipment.JS_E_DEP = new ZDateTime(2013, 1, 1);
			AutorateAndAssert(new List<AssertionCharge>(), shipment, localClient);
		}

		[TestDate(2014, 2, 18)]
		public void TestAutoRatingWithCustomJobDateRegistryMode()
		{
			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Mode = "AIR",
				DirectionCode = FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.DepartureDate
			};

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
					.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);

			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(localClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX", ZString.Empty, ZString.Empty);
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-2);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(2);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 20, 2, consol);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  6666.66m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSSellAmt = 20.00m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);

			var oldDate = new ZDateTime(2014, 2, 1);
			shipment.JS_E_ARV = oldDate;
			AutorateAndAssert(expected, shipment, localClient);

			shipment.JS_E_DEP = oldDate;
			var emptyAssertionCharges = new List<AssertionCharge>();
			AutorateAndAssert(emptyAssertionCharges, shipment, localClient);

			autoRateDate.DateType = JobDateTypes.Codes.AWBIssueDate;
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			AutorateAndAssert(emptyAssertionCharges, shipment, localClient);

			consol.JK_MasterBillIssueDate = oldDate;
			AutorateAndAssert(emptyAssertionCharges, shipment, localClient);

			consol.JK_MasterBillIssueDate = ZDateTime.Today.AddDays(-1);
			AutorateAndAssert(expected, shipment, localClient);
		}

		[TestDate(2018, 2, 7)]
		public void TestAutoRatingWithJobOpenDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.JobOpenDate
			};

			var originChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Origin);
			originChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "NZAKL", "AUSYD");
			rateEntryA.TI_RateStartDate = new ZDate(2018, 2, 1);
			rateEntryA.TI_RateEndDate = new ZDate(2018, 2, 4);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("ODOC", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "NZAKL", "AUSYD");
			rateEntryB.TI_RateStartDate = new ZDate(2018, 2, 5);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, ZGuid.Empty, "NZAKL", "AUSYD", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000140";
			shipment.JS_E_DEP = new ZDateTime(2018, 2, 5);
			shipment.JS_E_ARV = new ZDateTime(2018, 2, 9);
			shipment.JS_INCO = "EXW";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2015, 4, 1);

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Found ODOC-FLT-Client Rate WTGAX",
"Information: RateLine Filtered ODOC-FLT-Client Rate WTGAX	reason:	Job Open Date (07-Feb-18) is outside of the date range of this rate (01-Feb-18 to 04-Feb-18)"
			};

			var expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  50m
						}
				};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "The ODOC charge should be filtered as it doesn't match the Job Open Date", expectedLogLines);
			}
		}

		[TestDate(2018, 3, 11)]
		public void TestAutoRatingWithFirstContainerGateInDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.CFSLoadListCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.FirstContainerGateInDate
			};

			var cfsChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.CFSLoadList);
			cfsChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Client Rate Setup

			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var chargeCode1 = Helper.ChargeCodes.New("CLL1", "CLL 1 Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList);
			var chargeCode2 = Helper.ChargeCodes.New("CLL2", "CLL 2 Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList);

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AUSYD", "NZAKL");
			rateEntryA.TI_RateStartDate = new ZDate(2018, 3, 11);
			rateEntryA.TI_RateEndDate = new ZDate(2018, 3, 13);
			rateEntryA.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine(chargeCode1, FlatCalculator.Code, "CN", CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 35m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AUSYD", "NZAKL");
			rateEntryB.TI_RateStartDate = new ZDate(2018, 3, 14);
			rateEntryB.TI_RateEndDate = new ZDate(2018, 3, 20);
			rateEntryB.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine(chargeCode2, FlatCalculator.Code, "CN", CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			#endregion

			#region Shipment and Consol Setup

			var cfsConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			cfsConsol.JK_UniqueConsignRef = "LOADLIST1";
			cfsConsol.JK_TransportMode = TransportModes.Sea;
			cfsConsol.JK_ConsolMode = ContainerModes.FCL;
			cfsConsol.JK_RL_NKLoadPort = "AUSYD";
			cfsConsol.JK_RL_NKDischargePort = "NZAKL";
			cfsConsol.JK_OH_Forwarder = localClient.PK;

			var transport = cfsConsol.Transports[0];
			transport.JW_ETD = new ZDateTime(2018, 3, 11);
			transport.JW_ETA = new ZDateTime(2018, 3, 20);

			cfsConsol.Containers.RemoveAndDeleteAll();

			var container1 = cfsConsol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerNum = "CONT00001";
			container1.JC_FCLWharfGateIn = new ZDateTime(2018, 3, 14);

			var container2 = cfsConsol.Containers.AddNew();
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerNum = "CONT00002";
			container2.JC_FCLWharfGateIn = new ZDateTime(2018, 3, 15);

			var container3 = cfsConsol.Containers.AddNew();
			container3.JC_RC = GP20.PK;
			container3.JC_ContainerNum = "CONT00003";
			container3.JC_FCLWharfGateIn = new ZDateTime(2018, 3, 16);

			Factory.Save();

			CreateJob(cfsConsol, "LOADLIST1");

			#endregion

			var expectedLogLines = new[] {
"Information: RateLine Found CLL1-FLT-CN-Client Rate WTGAX",
"Information: RateLine Found CLL2-FLT-CN-Client Rate WTGAX",
"Information: RateLine Filtered CLL1-FLT-CN-Client Rate WTGAX	reason:	First Container Gate In Date (14-Mar-18) is outside of the date range of this rate (11-Mar-18 to 13-Mar-18)"
			};

			var expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "CLL2",
							JR_OSCostAmt =  50m
						}
				};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(expected, cfsConsol, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(cfsConsol, "The CLL1 charge should be filtered as it doesn't match the job First Container Gate In Date", expectedLogLines);
			}
		}

		[TestDate(2018, 1, 15)]
		public void TestAutoRatingWithLastContainerGateInDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ForwardingConsolCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.LastContainerGateInDate
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var costing = Helper.NewCosting(null);

			var rateEntryA = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "NZAKL", "", "20GP");
			rateEntryA.TI_RateStartDate = new ZDate(2018, 1, 15);
			rateEntryA.TI_RateEndDate = new ZDate(2018, 1, 17);
			rateEntryA.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 35m;

			var rateEntryB = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "NZAKL", "", "20GP");
			rateEntryB.TI_RateStartDate = new ZDate(2018, 1, 18);
			rateEntryB.TI_RateEndDate = new ZDate(2018, 1, 24);
			rateEntryB.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("BAF", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			#endregion

			#region Consol Setup

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_UniqueConsignRef = "FWConsol1";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_ETD = new ZDateTime(2018, 1, 15);
			consol.Transports[0].JW_ETA = new ZDateTime(2018, 1, 24);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			consol.Containers.RemoveAndDeleteAll();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerNum = "CONT00001";
			container1.JC_FCLWharfGateIn = new ZDateTime(2018, 1, 18);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerNum = "CONT00002";
			container2.JC_FCLWharfGateIn = new ZDateTime(2018, 1, 19);

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = GP20.PK;
			container3.JC_ContainerNum = "CONT00003";
			container3.JC_FCLWharfGateIn = new ZDateTime(2018, 1, 20);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "NZAKL", 30m);
			consol.Shipments.Add(shipment);

			Factory.Save();

			#endregion

			var expectedLogLines = new[] {
"Information: RateLine Found BAF-FLT-20GP-Standard Costs (TACT/General Rates)",
"Information: RateLine Found FRT-FLT-20GP-Standard Costs (TACT/General Rates)",
"Information: RateLine Filtered FRT-FLT-20GP-Standard Costs (TACT/General Rates)	reason:	Last Container Gate In Date (20-Jan-18) is outside of the date range of this rate (15-Jan-18 to 17-Jan-18)"
			};

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount =  50m,
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					job.JH_OA_LocalChargesAddr = shipment.ConsigneeDocumentaryAddress.E2_OA_Address;
					Factory.Save();

					AutoCostAndAssert("", null, expected, consol, autorateRevenue: false);
					AssertAutoratingAuditLogNoteContainsLines(consol, "FRT charge should be filtered as it does not match the job Last Container Gate In Date", expectedLogLines);
				}
			}
		}

		[TestDate(2018, 4, 1)]
		public void TestAutoRatingWithCFSReceivalStartDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.CFSLoadListCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.CFSReceivalStartDate
			};

			var destinationChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.CFSLoadList);
			destinationChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			var chargeCode1 = Helper.ChargeCodes.New("CFS1", "Destination Charge A", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList);
			var chargeCode2 = Helper.ChargeCodes.New("CFS2", "Destination Charge B", FlatCalculator.Code, ChargeCodeGroupList.Codes.CFSLoadList);

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";
			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AU", "NZAKL");
			rateEntryA.TI_RateStartDate = new ZDate(2018, 4, 1);
			rateEntryA.TI_RateEndDate = new ZDate(2018, 4, 3);
			rateEntryA.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine(chargeCode1, FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AU", "NZAKL");
			rateEntryB.TI_RateStartDate = new ZDate(2018, 4, 4);
			rateEntryB.TI_RateEndDate = new ZDate(2018, 4, 10);
			rateEntryB.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine(chargeCode2, FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 40m;

			Factory.Save();

			#endregion

			#region Consol and routes setup

			var cfsConsol = Factory.New<CFSLoadListConsol>();
			cfsConsol.JK_UniqueConsignRef = "LOADLIST1";
			cfsConsol.JK_TransportMode = TransportModes.Sea;
			cfsConsol.JK_ConsolMode = ContainerModes.FCL;
			cfsConsol.JK_RL_NKLoadPort = "AUSYD";
			cfsConsol.JK_RL_NKDischargePort = "NZAKL";
			cfsConsol.JK_OH_Forwarder = localClient.PK;

			cfsConsol.Containers.RemoveAndDeleteAll();
			var container = cfsConsol.Containers.AddNew();
			container.JC_RC = GP20.PK;
			container.JC_ContainerNum = "CONT00001";
			container.JC_ContainerMode = "FCL";

			var transport1 = cfsConsol.Transports[0];
			transport1.JW_ETD = new ZDateTime(2018, 4, 1);
			transport1.JW_ETA = new ZDateTime(2018, 4, 7);
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_VoyageFlight = "MainVoy";
			transport1.JW_CarrierBookingReference = "1243";
			transport1.JW_TransportType = TransportPlanningType.MainVessel;
			transport1.JW_DepotReceivalCommences = new ZDateTime(2018, 4, 1);

			var transport2 = cfsConsol.Transports.AddNew();
			transport2.JW_ETD = new ZDate(2018, 4, 7);
			transport2.JW_ETA = new ZDate(2018, 4, 9);
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_VoyageFlight = "PreVoy";
			transport2.JW_CarrierBookingReference = "1245";
			transport2.JW_TransportType = TransportPlanningType.PreCarriage;
			transport2.JW_DepotReceivalCommences = new ZDateTime(2018, 4, 6);

			Factory.Save();

			CreateJob(cfsConsol, "LOADLIST1");
			#endregion

			var expectedLogLines = new[] {
"Information: RateLine Found CFS1-FLT-Client Rate WTGAX",
"Information: RateLine Found CFS2-FLT-Client Rate WTGAX",
"Information: RateLine Filtered CFS2-FLT-Client Rate WTGAX	reason:	CFS Receival Start Date (01-Apr-18) is outside of the date range of this rate (04-Apr-18 to 10-Apr-18)"
			};

			var expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "CFS1",
							JR_OSCostAmt = 20m
						}
				};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(expected, cfsConsol, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(cfsConsol, "The CFS2 charge should be filtered as it doesn't match the job CFS Receival Start Date", expectedLogLines);
			}
		}

		public void TestAutoRatingWithHBLPlaceOfReceiptArrivalDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD");
			rateEntryA.TI_RateStartDate = new ZDate(2018, 7, 16);
			rateEntryA.TI_RateEndDate = new ZDate(2018, 7, 30);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD");
			rateEntryB.TI_RateStartDate = new ZDate(2018, 7, 1);
			rateEntryB.TI_RateEndDate = new ZDate(2018, 7, 15);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 40m;

			var rateEntryC = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD");
			rateEntryC.TI_RateStartDate = new ZDate(2018, 6, 1);
			rateEntryC.TI_RateEndDate = new ZDate(2018, 6, 30);
			rateEntryC.RateLines.RemoveAndDeleteAll();

			var rateLineC = rateEntryC.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineC.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, ZGuid.Empty, "NZAKL", "AUSYD", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000140";
			shipment.JS_E_DEP = new ZDateTime(2018, 6, 15);
			shipment.JS_E_ARV = new ZDateTime(2018, 7, 25);
			shipment.JS_INCO = "EXW";
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			var consol = CreateForwardingConsol(TransportModes.Sea, "NZAKL", "AUSYD", TransportProvider1, shipment);
			shipment.JS_PackingMode = "FCL";
			consol.JK_AgentType = "AGT";
			consol.JK_UniqueConsignRef = "FWConsol1";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_ETD = new ZDateTime(2018, 6, 15);
			consol.Transports[0].JW_ETA = new ZDateTime(2018, 7, 25);

			var expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  50m
						}
				};

			var expectedLogLines = new[] {
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	HBL Place Of Receipt Arrival Date was empty and so Standard Job filtering was applied. Departure Date (15-Jun-18) is outside of the date range of this rate (01-Jul-18 to 15-Jul-18)",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	HBL Place Of Receipt Arrival Date was empty and so Standard Job filtering was applied. Departure Date (15-Jun-18) is outside of the date range of this rate (16-Jul-18 to 30-Jul-18)"
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Standard date filtering should be applied", expectedLogLines);
			}

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2018, 07, 10);

			expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  40m
						}
				};

			expectedLogLines = new[] {
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	HBL Place Of Receipt Arrival Date (10-Jul-18) is outside of the date range of this rate (01-Jun-18 to 30-Jun-18)",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	HBL Place Of Receipt Arrival Date (10-Jul-18) is outside of the date range of this rate (16-Jul-18 to 30-Jul-18)"
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "HBL Place Of Receipt Arrival Date should be applied", expectedLogLines);
			}
		}

		[TestDate(2017, 1, 10)]
		public void TestAutoRatingWithCustomJobDateRegistry_ImportDirection_MatchingLocationWithJobOrigin()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.Import,
				DateType = JobDateTypes.Codes.JobOpenDate,
				Location = "NZAKL"
			};

			var originChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Origin);
			originChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Enty Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "NZAKL", "AUSYD");
			rateEntryA.TI_RateStartDate = new ZDate(2017, 1, 2);
			rateEntryA.TI_RateEndDate = new ZDate(2017, 1, 20);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("ODOC", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "NZDEV", "AUSYD");
			rateEntryB.TI_RateStartDate = new ZDate(2017, 1, 22);
			rateEntryB.TI_RateEndDate = new ZDate(2017, 1, 27);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("OCART", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, ZGuid.Empty, "NZAKL", "AUSYD", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000210";
			shipment.JS_E_DEP = new ZDateTime(2017, 1, 5);
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 30);
			shipment.JS_INCO = "EXW";

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 1",
"Information: RateLine Found ODOC-FLT-Client Rate WTGAX"
			};

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 20m
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Only ODOC is applicable since JOB is within the ODOC date range", expectedLogLines);
			}
		}

		[TestDate(2017, 1, 10)]
		public void TestAutoRatingWithCustomJobDateRegistry_ExportDirection_MatchingLocationWithJobDestination()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "AIR",
				DirectionCode = FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.JobOpenDate,
				Location = "USLAX"
			};

			var destinationChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Destination);
			destinationChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Enty Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUMEL", "USLAX");
			rateEntryA.TI_RateStartDate = new ZDate(2017, 1, 2);
			rateEntryA.TI_RateEndDate = new ZDate(2017, 1, 20);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("DDOC", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "USLAX");
			rateEntryB.TI_RateStartDate = new ZDate(2017, 1, 22);
			rateEntryB.TI_RateEndDate = new ZDate(2017, 2, 1);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("DCART", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, ZGuid.Empty, "AUMEL", "USLAX", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000777";
			shipment.JS_E_DEP = new ZDateTime(2017, 1, 5);
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 30);
			shipment.JS_INCO = "CLT";

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 2",
"Information: RateLine Found DDOC-FLT-Client Rate WTGAX",
"Information: RateLine Found DCART-FLT-Client Rate WTGAX"
			};

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 20m
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "DCART should be filtered", expectedLogLines);
			}
		}

		public void TestAutoRatingWithCustomJobDateRegistry_DomesticDirection_MatchingLocationWithJobOriginAndDestination()
		{
			var zone = Helper.NewInternationalZone("AUIN", null, "AUMEL", CountryCodes.Australia);
			zone.FZ_ZoneMode = RateMode.SEA;

			Factory.Save();

			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.Domestic,
				DateType = JobDateTypes.Codes.DepartureDate,
				Location = "AU"
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Enty Setup

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, zone.Code, "AUSYD");
			rateEntryA.TI_RateStartDate = new ZDate(2017, 1, 2);
			rateEntryA.TI_RateEndDate = new ZDate(2017, 1, 20);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUMEL", "AUSYD");
			rateEntryB.TI_RateStartDate = new ZDate(2017, 1, 22);
			rateEntryB.TI_RateEndDate = new ZDate(2017, 2, 1);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("BAF", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 30m;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, ZGuid.Empty, "AUMEL", "AUSYD", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000AXA";
			shipment.JS_E_DEP = new ZDateTime(2017, 1, 5);
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 30);
			shipment.JS_INCO = "CLT";

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 2",
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered BAF-FLT-Client Rate WTGAX	reason:	Departure Date (05-Jan-17) is outside of the date range of this rate (22-Jan-17 to 01-Feb-17)"
			};

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "BAF should be filtered", expectedLogLines);
			}
		}

		[TestDate(2017, 1, 15)]
		public void TestAutoRatingWithCustomJobDateRegistry_OtherDirection_MatchingLocationWithJobOriginOrDestination()
		{
			var zone = Helper.NewInternationalZone("CNIN", null, CountryCodes.China, "CNSHA");
			zone.FZ_ZoneMode = RateMode.SEA;

			Factory.Save();

			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.Other,
				DateType = JobDateTypes.Codes.JobOpenDate,
				Location = "AU"
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Enty Setup

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "CN", "USLAX");
			rateEntryA.TI_RateStartDate = new ZDate(2017, 1, 2);
			rateEntryA.TI_RateEndDate = new ZDate(2017, 1, 10);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, zone.Code, "");
			rateEntryB.TI_RateStartDate = new ZDate(2017, 1, 11);
			rateEntryB.TI_RateEndDate = new ZDate(2017, 2, 1);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("BAF", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 30m;

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, ZGuid.Empty, "CNSHA", "USLAX", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000434";
			shipment.JS_E_DEP = new ZDateTime(2017, 1, 5);
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 30);
			shipment.JS_INCO = "CLT";

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 2",
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered BAF-FLT-Client Rate WTGAX	reason:	Departure Date (05-Jan-17) is outside of the date range of this rate (11-Jan-17 to 01-Feb-17)"
			};

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Since Location on Custom registry does not cover the Origin/Location on job, we fall back to STD registry which is Departure Date and so BAF will be filtered", expectedLogLines);
			}
		}

		public void TestAutoRatingCustomJobDateRegistry_FallBackToSameGroupOfSettings()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var setting1 = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.Export,
				Location = "US",
				DateType = JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate
			};

			var setting2 = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.DepartureDate
			};

			var setting3 = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "SEA",
				DirectionCode = FreightShipmentDirection.Code.Export,
				Location = "USLAX",
				DateType = JobDateTypes.Codes.JobOpenDate
			};

			var destinationChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Destination);
			destinationChargeGroup.ChargeGroupSettings.Add(setting1);
			destinationChargeGroup.ChargeGroupSettings.Add(setting2);
			destinationChargeGroup.ChargeGroupSettings.Add(setting3);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Enty Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX");
			rateEntryA.TI_RateStartDate = new ZDate(2017, 1, 2);
			rateEntryA.TI_RateEndDate = new ZDate(2017, 1, 20);
			rateEntryA.RateLines.RemoveAndDeleteAll();

			var rateLineA = rateEntryA.AddRateLine("DDOC", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX");
			rateEntryB.TI_RateStartDate = new ZDate(2017, 1, 22);
			rateEntryB.TI_RateEndDate = new ZDate(2017, 2, 1);
			rateEntryB.RateLines.RemoveAndDeleteAll();

			var rateLineB = rateEntryB.AddRateLine("DCART", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, ZGuid.Empty, "AUSYD", "USLAX", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000232";
			shipment.JS_E_DEP = new ZDateTime(2017, 1, 5);
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 30);
			shipment.JS_INCO = "PPD";
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.JK_AgentType = "AGT";
			consol.JK_UniqueConsignRef = "FWConsol1";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_ETD = new ZDateTime(2017, 1, 5);
			consol.Transports[0].JW_ETA = new ZDateTime(2017, 1, 30);

			Factory.Save();

			var expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 2",
"Information: RateLine Found DDOC-FLT-Client Rate WTGAX",
"Information: RateLine Found DCART-FLT-Client Rate WTGAX",
"Information: RateLine Filtered DCART-FLT-Client Rate WTGAX	reason:	Departure Date (05-Jan-17) is outside of the date range of this rate (22-Jan-17 to 01-Feb-17)"
			};

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 20m
				}
			};

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				shipmentJob.LocalChargesPK = localClient.PK;
				shipmentJob.JH_A_JOP = ZDateTime.Empty;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Since Job Date for both Setting 1 and Setting2 was empty, we fall back to third custom registry", expectedLogLines);
			}

			shipment.JS_HouseBillIssueDate = new ZDateTime(2017, 1, 8);
			expectedLogLines = new[] {
"Information: RatingHeader Found Client Rate WTGAX Entries: 2",
"Information: RateLine Filtered DDOC-FLT-Client Rate WTGAX	reason:	Job Open Date (24-Jan-17) is outside of the date range of this rate (02-Jan-17 to 20-Jan-17)"
			};

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 50m
				}
			};

			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				shipmentJob.LocalChargesPK = localClient.PK;
				shipmentJob.JH_A_JOP = new ZDateTime(2017, 1, 24);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "JOB should be applied", expectedLogLines);
			}
		}

		public void TestAutoRatingWithInterimReceiptDateRegistryMode()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.InterimReceiptDate
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "WTGAX";

			var rate = Helper.NewClientRate(localClient);

			var rateEntryA = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD", removeLines: true);
			rateEntryA.TI_RateStartDate = new ZDate(2023, 2, 16);
			rateEntryA.TI_RateEndDate = new ZDate(2023, 2, 28);

			var rateLineA = rateEntryA.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineA.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntryB = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD", removeLines: true);
			rateEntryB.TI_RateStartDate = new ZDate(2023, 2, 1);
			rateEntryB.TI_RateEndDate = new ZDate(2023, 2, 15);

			var rateLineB = rateEntryB.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineB.GetCalculator<FlatCalculator>().BaseRate = 40m;

			var rateEntryC = rate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "NZAKL", "AUSYD", removeLines: true);
			rateEntryC.TI_RateStartDate = new ZDate(2023, 1, 1);
			rateEntryC.TI_RateEndDate = new ZDate(2023, 1, 31);

			var rateLineC = rateEntryC.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.Australia);
			rateLineC.GetCalculator<FlatCalculator>().BaseRate = 50m;

			Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, localClient.PK, ZGuid.Empty, "NZAKL", "AUSYD", 200, 3);
			shipment.JS_UniqueConsignRef = "SHP000140";
			shipment.JS_E_DEP = new ZDateTime(2023, 1, 15);
			shipment.JS_E_ARV = new ZDateTime(2023, 2, 25);
			shipment.JS_INCO = "EXW";
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			var consol = CreateForwardingConsol(TransportModes.Sea, "NZAKL", "AUSYD", TransportProvider1, shipment);
			shipment.JS_PackingMode = "FCL";
			consol.JK_AgentType = "AGT";
			consol.JK_UniqueConsignRef = "FWConsol1";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_ETD = new ZDateTime(2023, 1, 15);
			consol.Transports[0].JW_ETA = new ZDateTime(2023, 2, 25);

			var expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  50m
						}
				};

			var expectedLogLines = new[] {
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	Interim Receipt Date was empty and so Standard Job filtering was applied. Departure Date (15-Jan-23) is outside of the date range of this rate (01-Feb-23 to 15-Feb-23)",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	Interim Receipt Date was empty and so Standard Job filtering was applied. Departure Date (15-Jan-23) is outside of the date range of this rate (16-Feb-23 to 28-Feb-23)"
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Standard date filtering should be applied", expectedLogLines);
			}

			shipment.JS_A_RCV = new ZDateTime(2023, 2, 10);

			expected = new[]
			{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  40m
						}
				};

			expectedLogLines = new[] {
"Information: RateLine Found FRT-FLT-Client Rate WTGAX",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	Interim Receipt Date (10-Feb-23) is outside of the date range of this rate (01-Jan-23 to 31-Jan-23)",
"Information: RateLine Filtered FRT-FLT-Client Rate WTGAX	reason:	Interim Receipt Date (10-Feb-23) is outside of the date range of this rate (16-Feb-23 to 28-Feb-23)"
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Interim Receipt Date should be applied", expectedLogLines);
			}
		}

		public void TestGivenInterimReceiptDateRegistryModeIsEnabled_WhenAutoRatingCost_ThenInterimReceiptDateShouldBeDetected()
		{
			#region Registry Setup

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};

			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ForwardingConsolCode,
				Mode = "ALL",
				DirectionCode = FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.InterimReceiptDate
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			#endregion

			#region Rate Setup

			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m, currency: "AUD");
			rateEntry1.TI_RateStartDate = new ZDate(2023, 1, 15);
			rateEntry1.TI_RateEndDate = new ZDate(2023, 1, 20);
			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "NZAKL", "FRT", 20m, currency: "AUD");
			rateEntry2.TI_RateStartDate = new ZDate(2023, 1, 25);
			rateEntry2.TI_RateEndDate = new ZDate(2023, 1, 30);

			#endregion

			#region Consol Setup

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "NZAKL");
			shipment.JS_E_DEP = new ZDateTime(2023, 1, 15);
			shipment.JS_E_ARV = new ZDateTime(2023, 2, 25);

			var consol = CreateConsol(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "NZAKL");
			consol.Transports[0].JW_ETD = new ZDateTime(2023, 1, 15);
			consol.Transports[0].JW_ETA = new ZDateTime(2023, 2, 25);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.Shipments.Add(shipment);

			Factory.Save();

			#endregion

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount =  10m,
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					AutoCostAndAssert
					(
						"Given Interim Receipt Date is not set, when auto rating cost, then Department Date should be used for Autorating",
						expectedInvoicingCharges: null,
						expectedCosts: expectedCosts,
						consol,
						autorateRevenue: false
					);
				}
			}

			shipment.JS_A_RCV = new ZDateTime(2023, 1, 26);

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount =  20m,
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					AutoCostAndAssert
					(
						"Given Interim Receipt Date is set, when auto rating cost, then Interim Receipt Date should be used for Autorating",
						expectedInvoicingCharges: null,
						expectedCosts: expectedCosts,
						consol,
						autorateRevenue: false
					);
				}
			}
		}
	}
}
