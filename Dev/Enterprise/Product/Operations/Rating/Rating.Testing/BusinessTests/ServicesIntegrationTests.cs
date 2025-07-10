using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class ServicesIntegrationTests : BaseRatingIntegrationTest
	{
		//GIVEN rate with service sub-group ChargeCode, UNT calculator, CN(Container) unit
		//WHEN autorate shipment+consol with containers and service without container
		//THEN the rate should be calculated per shipment+consol containers, even the service has no container
		//This is limitation in CW1 i.e.we can’t specify container per service.
		//Hence if we don't autorate per containers, users has no way to autorate those rates with service subgroup.
		public void TestRateLineWithCTNUnitFactor()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, RatingConstants.TransportMode.SEA, "AUSYD", "USLAX", "OFUMI", 10, QuantityUnit.CN);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.CTN;

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX");
			shipment.JS_INCO = ZString.Empty;
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX");
			consol.Shipments.Add(shipment);

			var shipmentCustomizedService = shipment.DocsAndCartage.Services.AddNew();
			shipmentCustomizedService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			shipmentCustomizedService.ES_ServiceCount = 2;
			shipmentCustomizedService.ES_Duration = new TimeSpan(1, 0, 0);
			shipmentCustomizedService.ES_Completed = ZDateTime.Today;

			consol.AddContainer("20GP", count: 3, number: "CONT00001", packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("20GP", count: 4, packLines: new[] { shipment.AddPackLine(weight: 40) });

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "OFUMI",
						JR_Desc = "Origin Fumigation - Container #CONT00001",
						JR_OSSellAmt = 30.00m,
						RevenueCalculationDescription = "OFUMI: CONT00001 (20GP) - 3 Container(s) @ AUD 10.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "OFUMI",
						JR_Desc = "Origin Fumigation",
						JR_OSSellAmt = 40.00m,
						RevenueCalculationDescription = "OFUMI: 4 Container(s) @ AUD 10.00/Container"
					},
				},
				shipment,
				Consignee,
				autorateCosts: false
			);
		}

		#region Logging Services

		[TestDate(2018, 09, 10)]
		public void TestAutoratingServices_NoCompletionDate_ServiceChargeFilteredOut()
		{
			var orgCleaning = Helper.ChargeCodes.New("TSTSVY", "Survey Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Cleaning);
			var orgFumigation = Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Fumigation);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AU", "");
			var rateLine = rateEntry.AddRateLine(orgFumigation, FlatCalculator.Code);

			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			rateLine = rateEntry.AddRateLine(orgCleaning, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 20m);
			shipment.JS_UniqueConsignRef = "S100116";

			var fumigationService = shipment.DocsAndCartage.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 1;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Today;

			var cleaningService = shipment.DocsAndCartage.Services.AddNew();
			cleaningService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			cleaningService.ES_ServiceCount = 1;

			var tailgate = shipment.DocsAndCartage.Services.AddNew();
			tailgate.ES_ServiceCode = FreightServiceType.Codes.Tailgate;
			tailgate.ES_Booked = ZDateTime.Today;
			tailgate.ES_Completed = ZDateTime.Today;
			tailgate.ES_ServiceCount = 1;
			tailgate.ES_OH_Contractor = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = orgFumigation.AC_Code,
					JR_OSSellAmt = 100m
				}
			};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);

			var expectedLogLines = new string[]
			{
				"Information: RateLine Filtered TSTSVY-FLT-Client Rate TESTORG1	reason:	CLN Service was either not present or completion date was not applicable for LOD charge code group.",
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		[TestDate(2016, 06, 06)]
		public void TestAutoratingServices_RateEntriesFilterLog()
		{
			var dstSurvey = Helper.ChargeCodes.New("TSTSVY", "Survey Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Survey);
			var dstFumigation = Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Fumigation);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "");
			rateEntry.TI_ContractNumber = "CTR008";
			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "", "STD", "");
			rateEntry2.TI_ContractNumber = "CTR007";
			var rateLine = rateEntry.AddRateLine(dstFumigation, FlatCalculator.Code);

			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			rateLine = rateEntry.AddRateLine(dstSurvey, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZCFT";
			var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_ClientContractNumber = "CTR009";

			consol1.JK_ConsolMode = ContainerModes.Loose;
			consol1.JK_TransportMode = TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZCFT";

			Factory.Save();

			AutorateAndAssert(null, shipment, client, autorateCosts: false);

			// before WI00425393, we expected to have message: "Information: RateEntry Filtered Client Rate TESTORG1 reason: Contract Number didn't match job CTR009. (x2)"
			// with the performance enhancement, we filter the entry at db level so no filter message.
		}

		[TestDate(2016, 06, 06)]
		public void TestAutoratingServices_RateEntriesFilterLog_ExpiredDatetimeCatergoryContainerType()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_FreightRateClass = "20GN";
			container.RC_Code = "20PP";

			var container1 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_FreightRateClass = "20GN";
			container1.RC_Code = "20NN";

			GP20.RC_FreightRateClass = "20GN";

			var dstSurvey = Helper.ChargeCodes.New("TSTSVY", "Survey Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Survey);
			var dstFumigation = Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Fumigation);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "NZCFT", "AUSYD");
			var rateLine = rateEntry.AddRateLine(dstFumigation, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "NZCFT", "AUSYD");
			rateEntry2.TI_RateStartDate = new ZDate(2016, 6, 6);
			rateEntry2.TI_RC = GP40.PK;
			var rateLine2 = rateEntry.AddRateLine(dstFumigation, FlatCalculator.Code);

			var rateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "NZCFT", "AUSYD");
			rateEntry3.TI_RateStartDate = new ZDate(2016, 6, 10);
			var rateLine3 = rateEntry.AddRateLine(dstFumigation, FlatCalculator.Code);

			var rateEntry4 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "NZCFT", "AUSYD");
			rateEntry4.TI_RateStartDate = new ZDate(2016, 6, 4);
			rateEntry4.TI_RateEndDate = new ZDate(2016, 6, 5);
			var rateLine4 = rateEntry.AddRateLine(dstFumigation, FlatCalculator.Code);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			var consol1 = shipment.Consols.AddNew();

			consol1.JK_ConsolMode = ContainerModes.FCL;
			consol1.JK_TransportMode = TransportModes.Road;
			consol1.JK_RL_NKLoadPort = "NZCFT";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			var containerConsol = consol1.Containers.AddNew();
			containerConsol.JC_RC = GP20.PK;
			Factory.Save();

			AutorateAndAssert(null, shipment, client, autorateCosts: false);

			var expectedLogLines = new string[]
			{
				"Information: RateEntry Filtered Client Rate TESTORG1 reason: Transport Mode didn't match job ROA. (x3)",
				"Information: RateEntry Filtered Client Rate TESTORG1 reason: Start Date is after 06-Jun-16. (x3)",
				"Information: RateEntry Filtered Client Rate TESTORG1 reason: Expiry Date is before 06-Jun-16. (x3)"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		[TestDate(2018, 07, 10)]
		public void TestAutoratingServices_Dates_ServiceChargeFilter()
		{
			var registryDefinedJobServices = FreightDataRegistry.Instance.JobServices.Value;
			registryDefinedJobServices.Add(ChargeCodeSubGroupList.Labor, (NoResString)"Labor", false);

			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDefinedJobServices))
			{
				var dstTailgate = Helper.ChargeCodes.New("TSTTLG", "Tailgate Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Tailgate);
				var dstCleaning = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

				var orgSurvey = Helper.ChargeCodes.New("TSTSVY", "Survey Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Survey);
				var orgFumigation = Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading, FreightServiceType.Codes.Fumigation);

				var dstLabor = Helper.ChargeCodes.New("TSTDLBR", "Labor Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor);
				var orgLabor = Helper.ChargeCodes.New("TSTOLBR", "Labor Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor);

				Factory.Save();

				var client = Helper.NewOrgHeader();
				var clientRate = Helper.NewClientRate(client);
				var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AU", "");
				var rateLine1 = rateEntry.AddRateLine(dstCleaning, FlatCalculator.Code);
				var rateLine2 = rateEntry.AddRateLine(orgFumigation, FlatCalculator.Code);
				var rateLine3 = rateEntry.AddRateLine(dstTailgate, FlatCalculator.Code);
				var rateLine4 = rateEntry.AddRateLine(orgSurvey, FlatCalculator.Code);
				var rateLine5 = rateEntry.AddRateLine(dstLabor, FlatCalculator.Code);
				var rateLine6 = rateEntry.AddRateLine(orgLabor, FlatCalculator.Code);

				rateLine1.GetCalculator<FlatCalculator>().BaseRate = 80m;
				rateLine2.GetCalculator<FlatCalculator>().BaseRate = 100m;
				rateLine3.GetCalculator<FlatCalculator>().BaseRate = 180m;
				rateLine4.GetCalculator<FlatCalculator>().BaseRate = 10m;
				rateLine5.GetCalculator<FlatCalculator>().BaseRate = 120m;
				rateLine6.GetCalculator<FlatCalculator>().BaseRate = 121m;

				var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 20m);
				shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
				shipment.JS_UniqueConsignRef = "S100116";
				shipment.JS_E_DEP = new ZDateTime(2018, 09, 1); // Origin services happen before this date whereas Destination services occur after it.
				shipment.JS_E_ARV = new ZDateTime(2018, 09, 20);

				var orgFumigationService = shipment.DocsAndCartage.Services.AddNew();
				orgFumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
				orgFumigationService.ES_ServiceCount = 1;
				orgFumigationService.ES_Duration = new TimeSpan(2, 30, 0);
				orgFumigationService.ES_Completed = shipment.JS_E_DEP.AddDays(-2);

				var dstCleaningService = shipment.DocsAndCartage.Services.AddNew();
				dstCleaningService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
				dstCleaningService.ES_ServiceCount = 1;
				dstCleaningService.ES_Duration = new TimeSpan(2, 30, 0);
				dstCleaningService.ES_Completed = shipment.JS_E_DEP.AddDays(2);

				var dstTailgateService = shipment.DocsAndCartage.Services.AddNew();
				dstTailgateService.ES_ServiceCode = FreightServiceType.Codes.Tailgate;
				dstTailgateService.ES_ServiceCount = 1;
				dstTailgateService.ES_Duration = new TimeSpan(2, 30, 0);
				dstTailgateService.ES_Completed = shipment.JS_E_ARV.AddDays(2);

				var dstLaborService = shipment.DocsAndCartage.Services.AddNew();
				dstLaborService.ES_ServiceCode = ChargeCodeSubGroupList.Labor;
				dstLaborService.ES_ServiceCount = 1;
				dstLaborService.ES_Duration = new TimeSpan(2, 30, 0);
				dstLaborService.ES_Completed = shipment.JS_E_ARV.AddDays(2);

				var orgSurveyService = shipment.DocsAndCartage.Services.AddNew();
				orgSurveyService.ES_ServiceCode = FreightServiceType.Codes.Survey;
				orgSurveyService.ES_ServiceCount = 1;
				orgSurveyService.ES_Duration = new TimeSpan(2, 30, 0);
				orgSurveyService.ES_Completed = shipment.JS_E_ARV.AddDays(2);
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = orgFumigation.AC_Code,
						JR_OSSellAmt = 100m
					},
					new AssertionCharge
					{
						ChargeCode = dstTailgate.AC_Code,
						JR_OSSellAmt = 180m
					},
					new AssertionCharge
					{
						ChargeCode = dstCleaning.AC_Code,
						JR_OSSellAmt = 80m
					},
					new AssertionCharge
					{
						ChargeCode = dstLabor.AC_Code,
						JR_OSSellAmt = 120m
					}
				};

				AutorateAndAssert(expected, shipment, client, autorateCosts: false);

				var expectedLogLines = new string[]
				{
					"LBR Service was either not present or completion date was not applicable for ORG charge code group.",
					"SVY Service was either not present or completion date was not applicable for LOD charge code group."
				};

				AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
			}
		}

		[TestDate(2018, 07, 10)]
		public void TestAutoratingServices_Location_ServiceChargeFilter()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.MainAddress.OA_RN_NKCountryCode = "CN";
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.MainAddress.OA_RN_NKCountryCode = "AU";
			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3.MainAddress.OA_RN_NKCountryCode = "NZ";

			var dstTailgate = Helper.ChargeCodes.New("TSTTLG", "Tailgate Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Tailgate);
			var dstCleaning = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Cleaning);
			var orgFumigation = Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "AU", "");
			var rateLine1 = rateEntry.AddRateLine(dstCleaning, FlatCalculator.Code);
			var rateLine2 = rateEntry.AddRateLine(orgFumigation, FlatCalculator.Code);
			var rateLine3 = rateEntry.AddRateLine(dstTailgate, FlatCalculator.Code);

			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 80m;
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 180m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "AUSYD", "CNSHA", 20m);
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			shipment.JS_UniqueConsignRef = "S100116";
			shipment.JS_E_DEP = new ZDateTime(2018, 09, 1);
			shipment.JS_E_ARV = new ZDateTime(2018, 09, 5);

			var orgFumigationService = shipment.DocsAndCartage.Services.AddNew();
			orgFumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			orgFumigationService.ES_ServiceCount = 1;
			orgFumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			orgFumigationService.ES_Completed = shipment.JS_E_ARV.AddDays(-1);
			orgFumigationService.ES_OA_Location = organisation1.MainAddress.PK;

			var dstCleaningService = shipment.DocsAndCartage.Services.AddNew();
			dstCleaningService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			dstCleaningService.ES_ServiceCount = 1;
			dstCleaningService.ES_Duration = new TimeSpan(2, 30, 0);
			dstCleaningService.ES_Completed = shipment.JS_E_ARV.AddDays(-2);
			dstCleaningService.ES_OA_Location = organisation2.MainAddress.PK;

			var dstTailgateService = shipment.DocsAndCartage.Services.AddNew();
			dstTailgateService.ES_ServiceCode = FreightServiceType.Codes.Tailgate;
			dstTailgateService.ES_ServiceCount = 1;
			dstTailgateService.ES_Duration = new TimeSpan(2, 30, 0);
			dstTailgateService.ES_Completed = shipment.JS_E_ARV.AddDays(2);
			dstTailgateService.ES_OA_Location = organisation3.MainAddress.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = orgFumigation.AC_Code,
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = dstCleaning.AC_Code,
					JR_OSSellAmt = 80m
				}
			};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);

			var expectedLogLines = new string[]
			{
				"Information: RateLine Filtered TSTTLG-FLT-Client Rate TESTORG1	reason:	Service country/region NZ did not match Job Destination CN"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		[TestDate(2018, 07, 10)]
		public void TestAutoratingServices_WhenJobOriginIsEmpty_ShoulsFilterCharge()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTLBR", "Labor Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.SEA, "", "HK", chargeCode.AC_Code, 80);

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "", "HKHKG", 20m);
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			shipment.JS_UniqueConsignRef = "S100116";
			shipment.JS_E_DEP = new ZDateTime(2018, 09, 1);
			shipment.JS_E_ARV = new ZDateTime(2018, 09, 5);
			// condition to make the service enabled
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Today.AddHours(3);

			var serviceOrg = Factory.NewWithValidTestData<OrgHeader>();
			serviceOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			var service = shipment.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = ChargeCodeSubGroupList.Labor;
			service.ES_ServiceCount = 1;
			service.ES_Duration = new TimeSpan(2, 30, 0);
			service.ES_Completed = shipment.JS_E_DEP.AddDays(-2);
			service.ES_OA_Location = serviceOrg.MainAddress.PK;

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;
				AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, client, autorateCosts: false);
			}
			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered TSTLBR-FLT-Client Rate TESTORG1	reason:	job origin and/or destination are blank."
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		[TestDate(2018, 07, 10)]
		public void TestAutoratingServices_WhenJobDestinationIsEmpty_ShouldFilterCharge()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTSTG", "Storage Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.SEA, "US", "", chargeCode.AC_Code, 80);

			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "USLAX", "", 20m);
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			shipment.JS_UniqueConsignRef = "S100116";
			shipment.JS_E_DEP = new ZDateTime(2018, 09, 1);
			shipment.JS_E_ARV = new ZDateTime(2018, 09, 5);
			// condition to make the service enabled
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 3;

			var serviceOrg = Factory.NewWithValidTestData<OrgHeader>();
			serviceOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			var service = shipment.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = ChargeCodeSubGroupList.Storage;
			service.ES_ServiceCount = 1;
			service.ES_Duration = new TimeSpan(2, 30, 0);
			service.ES_Completed = shipment.JS_E_ARV.AddDays(1);
			service.ES_OA_Location = serviceOrg.MainAddress.PK;

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;
				AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, client, autorateCosts: false);
			}
			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered TSTSTG-FLT-Client Rate TESTORG1	reason:	job origin and/or destination are blank."
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLines);
		}

		#endregion

		#region Unit Calculator

		#region Forwarding Services

		#region Ad-Hoc Job Service Rates

		public void TestAdHocJobServiceRates_Container()
		{
			const string newServiceCode = "NUU";

			var registryDefinedJobServices = FreightDataRegistry.Instance.JobServices.Value;
			registryDefinedJobServices.Add(newServiceCode, (NoResString)"Registry Defined Service", false);

			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDefinedJobServices))
			{
				var fumChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
				var steChargeCode = CreateAdHocChargeCode("STEORGSRV", "Steamy Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.SteamCleaning);
				var xinChargeCode = CreateAdHocChargeCode("XINORGSRV", "Extra Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.ExtraInspection);
				var nuuChargeCode = CreateAdHocChargeCode("NUUORGSRV", "Origin Service", ChargeCodeGroupList.Codes.Origin, newServiceCode);

				AssertNoExceptionThrown("There should be no Ad Hoc service charge codes existing in clean db", () => Factory.Save());

				var transportCreditor = Helper.CreateCreditor("TC01");
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.LCL;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "GBSUN";
				consol.JK_PrepaidCollect = PaymentType.Prepaid;

				var shipment1 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 85m);
				var shipment2 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 85m);
				consol.Shipments.Add(shipment1);
				consol.Shipments.Add(shipment2);

				var container = consol.Containers.AddNew();
				container.JC_RC = GP20.PK;
				container.PackLines.Add(shipment1.OuterPackLines.AddNew());
				container.PackLines.Add(shipment2.OuterPackLines.AddNew());

				var fumService = Helper.CreateAdHocJobService(container, FreightServiceType.Codes.Fumigation, 10m, JobServiceInfo.Constants.Codes.Hour, transportCreditor.PK);
				fumService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(3).AddMinutes(15);

				Helper.CreateAdHocJobService(container, FreightServiceType.Codes.SteamCleaning, 120m, JobServiceInfo.Constants.Codes.ServiceOccurrence, transportCreditor.PK);
				Helper.CreateAdHocJobService(container, FreightServiceType.Codes.ExtraInspection, 120m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
				Helper.CreateAdHocJobService(container, newServiceCode, 150m, JobServiceInfo.Constants.Codes.ServiceOccurrence, transportCreditor.PK);

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FUMORGSRV",
						E6_LocalCostAmount = 32.5m,//3.25 Hour(s) @ AUD 10.00/Hour
					},
					new AssertionCost
					{
						ChargeCode = "STEORGSRV",
						E6_LocalCostAmount = 120m,
					},
					new AssertionCost
					{
						ChargeCode = "NUUORGSRV",
						E6_LocalCostAmount = 150m
					},
				};

				AutoCostAndAssert("Container charges should come through as costs when there is a transport provider", null, expectedCosts, consol);
			}
		}

		public void TestAdHocJobServiceRates_Shipment()
		{
			const string newServiceCode = "NUU";

			var registryDefinedJobServices = FreightDataRegistry.Instance.JobServices.Value;
			registryDefinedJobServices.Add(newServiceCode, (NoResString)"Registry Defined Service", false);

			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDefinedJobServices))
			{
				var fumChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
				var qinChargeCode = CreateAdHocChargeCode("QINORGSRV", "Quarantine Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.QuarantineInspection);
				var nuuChargeCode = CreateAdHocChargeCode("NUUORGSRV", "Origin Service", ChargeCodeGroupList.Codes.Origin, newServiceCode);

				AssertNoExceptionThrown("There should be no Ad Hoc service charge codes existing in clean db", () => Factory.Save());

				var consignor = Helper.NewOrgHeader();
				var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 85m);
				var parent = shipment.DocsAndCartage;

				var fumService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Fumigation, 11.111m, JobServiceInfo.Constants.Codes.Hour);
				fumService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(3).AddMinutes(15);

				Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Tailgate, 1.5m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
				Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.QuarantineInspection, 150m, JobServiceInfo.Constants.Codes.ServiceOccurrence, TransportProvider2.PK);
				Helper.CreateAdHocJobService(parent, newServiceCode, 10m, JobServiceInfo.Constants.Codes.ServiceOccurrence);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = fumChargeCode.AC_Code,
						JR_OSSellAmt = 36.11m,
						JR_Desc = "Fumigation Service {REFFUM11.111}",
						RevenueCalculationDescription = "3.25 Hour(s) @ AUD 11.111/Hour"
					},
					new AssertionCharge
					{
						ChargeCode = qinChargeCode.AC_Code,
						JR_OSCostAmt = 150m,
						JR_Desc = "Quarantine Service {REFQIN150}",
						CostCalculationDescription = "1 Origin Quarantine Inspection @ AUD 150.00/Origin Quarantine Inspection"
					},
					new AssertionCharge
					{
						ChargeCode = nuuChargeCode.AC_Code,
						JR_OSSellAmt = 10m,
						JR_Desc = "Origin Service {REFNUU10}",
						RevenueCalculationDescription = "1 Origin Registry Defined Service @ AUD 10.00/Origin Registry Defined Service"
					},
				};

				var message = "Expected three rates as there is no AC_IsAdHocServiceCharge for Tailgate. QuarantineInspection should autorate as cost as it has a service provider";
				AutorateAndAssert(message, expected, shipment, consignor);
			}
		}

		public void TestAdHocJobServiceRates_ServiceLocationDoesNotMatchShipment_FallbackToOriginAndDestinationCosts()
		{
			var fumOrgChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Origin Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			var clnDstChargeCode = CreateAdHocChargeCode("CLNDSTSRV", "Cleaning Destination Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			AssertNoExceptionThrown("There should be no Ad Hoc service charge codes existing in clean db", () => Factory.Save());

			var consignor = Helper.NewOrgHeader();
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "NZAKL", "USLAX", 85m);
			shipment.JS_E_DEP = new ZDateTime(ZDateTime.Today.Year, 6, 1);
			var parent = shipment.DocsAndCartage;

			var fumService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Fumigation, 0, JobServiceInfo.Constants.Codes.Hour);
			fumService.ES_Completed = new ZDate(ZDateTime.Today.Year, 1, 1);
			fumService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(3).AddMinutes(15);
			fumService.ES_OA_Location = consignor.MainAddress.PK;
			fumService.ES_OH_Contractor = consignor.PK;

			var clnService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Cleaning, 0, JobServiceInfo.Constants.Codes.Hour);
			clnService.ES_Completed = new ZDateTime(ZDateTime.Today.Year, 12, 1);
			clnService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(2);
			clnService.ES_OA_Location = consignor.MainAddress.PK;
			clnService.ES_OH_Contractor = consignor.PK;

			var costing = Helper.NewCosting(consignor);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "NZAKL", "");
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			var rateLine = rateEntry.AddRateLine(fumOrgChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 15m;

			rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", "USLAX");
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			rateLine = rateEntry.AddRateLine(clnDstChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;
				var expectedCosts = new[]
				{
					new AssertionCharge
					{
						ChargeCode = clnDstChargeCode.AC_Code,
						JR_Desc = "Cleaning Destination Service",
						JR_OSCostAmt = 50.00m,
					},
					new AssertionCharge
					{
						ChargeCode = fumOrgChargeCode.AC_Code,
						JR_Desc = "Fumigation Origin Service",
						JR_OSCostAmt = 48.75m,
					}
				};
				var message = "Expected service provider costs to be applied for origin and destination locations";
				AutorateAndAssert(message, expectedCosts, shipment, consignor);
			}
		}

		public void TestAdHocJobServiceRates_ServiceLocationDoesNotMatchShipment_CostsFoundMatchingServiceLocation()
		{
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FUMORGSRV",
					JR_Desc = "Fumigation Origin Service",
					JR_OSCostAmt = 48.75m,
				},
				new AssertionCharge
				{
					ChargeCode = "CLNDSTSRV",
					JR_Desc = "Cleaning Destination Service",
					JR_OSCostAmt = 50.00m,
				}
			};
			var message = "Expected service provider costs to be applied for service location";
			AssertChargesFoundUsingServiceLocation(message, "CNSHA", expected);
		}

		public void TestAdHocJobServiceRates_ServiceLocationDoesNotMatchShipment_CostsNotFoundForCountry()
		{
			AssertChargesFoundUsingServiceLocation("Expected no costs to be found when a country is the service location and no fallback costs are found", "CN", expected: null);
		}

		public void TestAdHocJobServiceRates_ServiceLocationDoesNotMatchShipment_CostsFoundWhenServiceLocationMatchesOrigin()
		{
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FUMORGSRV",
					JR_Desc = "Fumigation Origin Service",
					JR_OSCostAmt = 48.75m,
				}
			};
			var message = "Expected costs to be found for origin only when service location matches origin";
			AssertChargesFoundUsingServiceLocation(message, "NZAKL", expected);
		}

		public void TestAdHocJobServiceRates_ServiceLocationDoesNotMatchShipment_CostsFoundWhenServiceLocationMatchesDestination()
		{
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CLNDSTSRV",
					JR_Desc = "Cleaning Destination Service",
					JR_OSCostAmt = 50.00m,
				}
			};
			var message = "Expected costs to be found for destination only when service location matches destination";
			AssertChargesFoundUsingServiceLocation(message, "USLAX", expected);
		}

		public void AssertChargesFoundUsingServiceLocation(string message, string location, AssertionCharge[] expected)
		{
			var fumOrgChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Origin Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			var clnDstChargeCode = CreateAdHocChargeCode("CLNDSTSRV", "Cleaning Destination Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			AssertNoExceptionThrown("There should be no Ad Hoc service charge codes existing in clean db", () => Factory.Save());

			var consignor = Helper.NewOrgHeader();
			consignor.MainAddress.OA_RL_NKRelatedPortCode = location;
			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "NZAKL", "USLAX", 85m);
			shipment.JS_E_DEP = new ZDateTime(ZDateTime.Today.Year, 6, 1);
			var parent = shipment.DocsAndCartage;

			var fumService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Fumigation, 0, JobServiceInfo.Constants.Codes.Hour);
			fumService.ES_Completed = new ZDate(ZDateTime.Today.Year, 1, 1);
			fumService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(3).AddMinutes(15);
			fumService.ES_OA_Location = consignor.MainAddress.PK;
			fumService.ES_OH_Contractor = consignor.PK;

			var clnService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Cleaning, 0, JobServiceInfo.Constants.Codes.Hour);
			clnService.ES_Completed = new ZDateTime(ZDateTime.Today.Year, 12, 1);
			clnService.ES_Duration = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddHours(2);
			clnService.ES_OA_Location = consignor.MainAddress.PK;
			clnService.ES_OH_Contractor = consignor.PK;

			var costing = Helper.NewCosting(consignor);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, location, "");
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			var rateLine = rateEntry.AddRateLine(fumOrgChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 15m;

			rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", location);
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			rateLine = rateEntry.AddRateLine(clnDstChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 25m;

			var standardCosting = Helper.NewCosting(null);
			rateEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, location, "");
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			rateLine = rateEntry.AddRateLine(fumOrgChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10m;
			rateEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.AIR, "", location);
			rateEntry.TI_RateStartDate = new ZDate(ZDateTime.Today.Year, 1, 1);
			rateLine = rateEntry.AddRateLine(fumOrgChargeCode, UnitCalculator.Code, QuantityUnit.HR, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 40m;

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;
				AutorateAndAssert(message, expected, shipment, consignor);
			}
		}

		public void TestAdHocJobServiceRates_GetsMeasuresFromShipment()
		{
			var fumChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);
			var taiChargeCode = CreateAdHocChargeCode("TAIORGSRV", "Tailgate Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Tailgate);
			var clnChargeCode = CreateAdHocChargeCode("CLNORGSRV", "Cleaning Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);
			var qinChargeCode = CreateAdHocChargeCode("QINORGSRV", "Quarantine Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.QuarantineInspection);
			var qupChargeCode = CreateAdHocChargeCode("QUPORGSRV", "Quarantine Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.QuarantineUnpack);

			var consignor = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "USBOS", "AUMEL", 8500m, 1.5m);
			shipment.JS_PackingMode = "FCL";
			var packLine = shipment.OuterPackLines.AddNew();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;
			packLine.Containers.Add(container);

			Factory.Save();

			var parent = shipment.DocsAndCartage;
			var fumService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Fumigation, 750m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
			var taiService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Tailgate, 1.5m, JobServiceInfo.Constants.Codes.Chargeable);
			var clnService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Cleaning, 10m, JobServiceInfo.Constants.Codes.Hour);
			var qinService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.QuarantineInspection, 150m, JobServiceInfo.Constants.Codes.FlatRate);
			var qupService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.QuarantineUnpack, 100m, JobServiceInfo.Constants.Codes.Container);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 750m,
					RevenueCalculationDescription = "FUMORGSRV: 1 Destination Fumigation @ AUD 750.00/Destination Fumigation"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 12.75m,
					RevenueCalculationDescription = "TAIORGSRV: 8.5 Cubic Meter(s) @ AUD 1.50/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 0m,
					RevenueCalculationDescription = "CLNORGSRV: 0 Hour(s) @ AUD 10.00/Hour"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 150m,
					RevenueCalculationDescription = "QINORGSRV: Base Rate AUD 150.00"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "QUPORGSRV: 1 Container(s) @ AUD 100.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, consignor);
		}

		public void TestAdHocJobServiceRates_ZeroRated()
		{
			var fumChargeCode = CreateAdHocChargeCode("FUMORGSRV", "Fumigation Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			var taiChargeCode = CreateAdHocChargeCode("TAIORGSRV", "Tailgate Service", ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Tailgate);

			var client = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, client.PK, "AUMEL", "USBOS", 850m);
			shipment.JS_UniqueConsignRef = "S00029735";

			Factory.Save();

			var parent = shipment.DocsAndCartage;
			var fumService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Fumigation, 0m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
			var taiService = Helper.CreateAdHocJobService(parent, FreightServiceType.Codes.Tailgate, 0m, ZString.Empty);

			var message = "The fumigation service charge should appear rated at zero. We do not expect to see the tail gate service as there is no measurement basis on the service";
			AutorateAndAssert(message, Array.Empty<AssertionCharge>(), shipment, client);

			var expectedLogLines = new[]
			{
				"Warning: Autorating Notifications: Rates for the below job services were not found. Please either create these charge codes, or ensure that your rate contains these charges and have the correct commodity code, service level and validity dates.",
				"Until you do this, these charges will not be rated.",
				"  • Fumigation: ORG / FUM for Shipment S00029735",
				"  • Tailgate: ORG / TAI for Shipment S00029735"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain information about ignored job service", expectedLogLines);
		}

		#region Chargeable Rating for Spot Rates and Ad Hoc Job Services

		public void TestChargeableForSpotRatesAndAdHocJobServices_Air()
		{
			AssertEquals("Pre-condition: this the unit we expect for loose air units", Weight.Kilograms, Env.Registry.FreightWeightUnit);

			var serviceChargeCode = CreateAdHocChargeCode("DSRV", "Service", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Survey);

			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "US", "AU");
			var chargeableLine = rateEntry.AddRateLine("DDOC", UnitCalculator.Code, Weight.Kilograms);
			chargeableLine.TL_Rounding = RatingRoundingTypes.Chargeable;
			chargeableLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var noRoundingLine = rateEntry.AddRateLine("DAWB", UnitCalculator.Code, Weight.Pounds);
			noRoundingLine.TL_Rounding = RatingRoundingTypes.NoRounding;
			noRoundingLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			var airShipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "USBOS", "AUMEL", 100m, 3m);
			airShipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			airShipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			airShipment.JS_UnitFreightRate = 20;
			airShipment.JS_UnitOfVolume = Volume.CubicFeet;
			airShipment.JS_UnitOfWeight = Weight.Pounds;

			Helper.CreateAdHocJobService(airShipment.DocsAndCartage, FreightServiceType.Codes.Survey, 1m, JobServiceInfo.Constants.Codes.Chargeable);

			Factory.Save();

			AssertEquals(100m, airShipment.JS_ActualWeight);
			AssertEquals(3m, airShipment.JS_ActualVolume);
			AssertEquals(Weight.Pounds, airShipment.JS_ChargeableUnit);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 907.18m,
					RevenueCalculationDescription = "FRT: 45.3592 Kilogram(s) @ AUD 20.00/KG"//spot rate freight charges should use the unit the registyr
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 453.59m,
					RevenueCalculationDescription = "DDOC: 45.3592 Kilogram(s) @ AUD 10.00/KG"//rate lines from client rates always use their unit
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					RevenueCalculationDescription = "DAWB: 100 Pound(s) @ AUD 2.00/LB"//rate lines from client rates always use their unit
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "DSRV: 100 Pound(s) @ AUD 1.00/LB"//ad-hoc services should use JS_ChargeableUnit
				}
			};

			AutorateAndAssert("Loose air freight chargeable weight", expected, airShipment, consignor);
		}

		public void TestChargeableForSpotRatesAndAdHocJobServices_Sea()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DAWB");

			var serviceChargeCode = Helper.ChargeCodes.New("DSRV", "Service", "", ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Survey);
			serviceChargeCode.AC_IsAdhocServiceCharge = true;

			var consignor = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "US", "AU");
			var chargeableLine = rateEntry.AddRateLine("DDOC", UnitCalculator.Code, Volume.CubicMetres);
			chargeableLine.TL_Rounding = RatingRoundingTypes.Chargeable;
			chargeableLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var noRoundingLine = rateEntry.AddRateLine("DAWB", UnitCalculator.Code, Volume.CubicMetres);
			noRoundingLine.TL_Rounding = RatingRoundingTypes.NoRounding;
			noRoundingLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			AssertEquals("Pre-condition: should be false by default", false, chargeableLine.UseOnlyActualWeightMeasure);
			AssertEquals("Pre-condition: should be false by default", false, noRoundingLine.UseOnlyActualWeightMeasure);
			var seaShipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "USBOS", "AUMEL", 1500m, 1.5m);
			seaShipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			seaShipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			seaShipment.JS_UnitFreightRate = 20;

			Factory.Save();

			Helper.CreateAdHocJobService(seaShipment.DocsAndCartage, FreightServiceType.Codes.Survey, 1m, JobServiceInfo.Constants.Codes.Chargeable);

			AssertEquals(1500m, seaShipment.JS_ActualWeight);
			AssertEquals(1.5m, seaShipment.JS_ActualVolume);
			AssertEquals("Sea shipments default the volume unit as the chargeable unit", seaShipment.JS_ActualVolume, seaShipment.JS_ActualChargeable);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 30m,
					RevenueCalculationDescription = "FRT: 1.5 Cubic Meter(s) @ AUD 20.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = "DDOC: 1.5 Cubic Meter(s) @ AUD 10.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 3m,
					RevenueCalculationDescription = "DAWB: 1.5 Cubic Meter(s) @ AUD 2.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 1.5m,
					RevenueCalculationDescription = "DSRV: 1.5 Cubic Meter(s) @ AUD 1.00/M3"
				}
			};

			AutorateAndAssert("Should autorate chargeable", expected, seaShipment, consignor);

			seaShipment.JS_ActualChargeable = 2m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 40m,
					RevenueCalculationDescription = "FRT: 2 Cubic Meter(s) @ AUD 20.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = "DDOC: 2 Cubic Meter(s) @ AUD 10.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 3m,
					RevenueCalculationDescription = "DAWB: 1.5 Cubic Meter(s) @ AUD 2.00/M3"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 2m,
					RevenueCalculationDescription = "DSRV: 2 Cubic Meter(s) @ AUD 1.00/M3"
				}
			};

			AutorateAndAssert("Should prefer overriden chargeable to the calculated chargeable", expected, seaShipment, consignor);
		}

		[TestDate(2016, 06, 06)]
		public void TestAutoratingServicesSpotRates_WhenCreditorWithClientRelationshipSetForAttachedConsole_ShouldNotFilterFreightRate()
		{
			//Create shipment with attached consoles that its creditor has client relationship. then set spot rate and then perform
			//AutoRating.We should not filter fright rate and should not log organization has been marked to not use group client rates
			var dstDemurrage = Helper.ChargeCodes.NewConsolChargeCode("DSTDME", "DST Demurrage", UnitCalculator.Code,
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal).PK.ToGuid();
			RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty,
				Guid.Empty, dstDemurrage);

			var child = Factory.NewWithValidTestData<OrgHeader>();
			child.OH_FullName = "child 4 consigner";
			OrgManagementRelatedPartyTestHelper.Create(Factory, Consignor, child);

			var creditorChild = Factory.NewWithValidTestData<OrgHeader>();
			creditorChild.OH_FullName = "child 4 creditor";
			var creditor = Helper.CreateCreditor();
			creditor.OH_FullName = "Creditor";
			creditor.OH_Code = "CreditorCode";
			OrgManagementRelatedPartyTestHelper.Create(Factory, creditor, creditorChild);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "CNSHA", "AUSYD", 20m);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_UnitFreightRate = 20;
			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_HouseBill = "BURR";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			consol.CreditorPK = creditor.PK;
			consol.Shipments.Add(shipment);

			Factory.Save();

			var expectedDescription = @"FRT: 0.02 Cubic Meter(s) @ AUD 20.00/M3

International Freight

One Off Freight Rate is applicable for Shipment S100216 (House Bill='BURR').

Mode:			LCL
Charge Code Group:	FRT
Origin:			CNSHA
Destination:		AUSYD
Service Level:		STD
Autorated for:		Shipment S100216 (House Bill='BURR')
Leg:			CNSHA-AUSYD";

			var expected = new[]
			{
				new AssertionCharge()
				{
					RevenueCalculationDescription = expectedDescription
				}
			};

			AutorateAndAssert(
				"Should not filter FRT-Job One Off Freight Rate when Creditro for attached console was set",
				expected, shipment, Consignor);
		}
		#endregion

		#endregion

		#region Hidden Service Cost Spot Rates

		public void TestAutoratingContainerServicesSpotCostRates()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China);

			var dstDemurrage = Helper.ChargeCodes.NewConsolChargeCode("DSTDMEGRP", "DST Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal).PK.ToGuid();
			var dstStorage = Helper.ChargeCodes.NewConsolChargeCode("DSTSTGGRP", "DST Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage).PK.ToGuid();
			var dstDetention = Helper.ChargeCodes.NewConsolChargeCode("DSTDTNGRP", "DST Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention).PK.ToGuid();
			RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstDemurrage);
			RatingDataRegistry.Instance.DestinationStorageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstStorage);
			RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstDetention);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var consignor = Helper.NewOrgHeader();
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 200m));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 400m));

			var baseDate = ZDateTime.MinSmallDateTimeValue;
			var container = consol.Containers.AddNew();
			container.ArrivalTruckWaitTime = baseDate.AddHours(2);
			var truckWaitPenalty = container.FindArrivalTruckWaitPenalty();
			truckWaitPenalty.CPY_PerUnitCost = 125m;
			truckWaitPenalty.CPY_TotalCost = 0m;
			truckWaitPenalty.CPY_RX_NKCurrency = "AUD";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 250m,
					CostCalculationDescription = "DSTDMEGRP: 2 Hour(s) @ AUD 125.00/Hour"
				}
			};

			AutoCostAndAssert("Should be able to autorate spot service cost for demurrage", null, expected, consol, false);

			container = consol.Containers.AddNew();
			container.ArrivalCTOStorageDays = new ZByte(5);
			var storagePenalty = container.FindArrivalCTOStoragePenalty();
			storagePenalty.CPY_PerUnitCost = 180m;
			storagePenalty.CPY_TotalCost = 0m;
			storagePenalty.CPY_RX_NKCurrency = "AUD";

			container.ArrivalCarrierDetentionDays = new ZByte(30);
			var detentionPenalty = container.FindArrivalCarrierDetentionPenalty();
			detentionPenalty.CPY_PerUnitCost = 10m;
			detentionPenalty.CPY_TotalCost = 0m;
			detentionPenalty.CPY_RX_NKCurrency = "AUD";

			expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 250m,
					CostCalculationDescription = "DSTDMEGRP: 2 Hour(s) @ AUD 125.00/Hour"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 900m,
					CostCalculationDescription = "DSTSTGGRP: 5 Day(s) @ AUD 180.00/Day"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 300m,
					CostCalculationDescription = "DSTDTNGRP: 30 Day(s) @ AUD 10.00/Day"
				}
			};

			AutoCostAndAssert("Should always be destination charges for hidden container costs", null, expected, consol, false);

			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_RL_NKDischargePort = "AUMEL";
			truckWaitPenalty.CPY_RL_NKLocation = "AUMEL";
			storagePenalty.CPY_RL_NKLocation = "AUMEL";
			detentionPenalty.CPY_RL_NKLocation = "AUMEL";

			expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 250m,
					CostCalculationDescription = "DSTDMEGRP: 2 Hour(s) @ AUD 125.00/Hour"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 900m,
					CostCalculationDescription = "DSTSTGGRP: 5 Day(s) @ AUD 180.00/Day"
				},
				new AssertionCost
				{
					E6_OSCostAmount = 300m,
					CostCalculationDescription = "DSTDTNGRP: 30 Day(s) @ AUD 10.00/Day"
				}
			};

			AutoCostAndAssert("", null, expected, consol, false);
		}

		public void TestAutoratingShipmentServicesSpotSellRates()
		{
			Func<ZString, Guid> createChargeCode = chargeGroup =>
			{
				var chargeCode = Helper.ChargeCodes.New(chargeGroup + "DMET", "Test Demurrage Charge", UnitCalculator.Code, chargeGroup, "DME");
				Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(chargeCode);

				return chargeCode.PK.ToGuid();
			};

			RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, createChargeCode("DST"));
			RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, createChargeCode("ORG"));

			var client = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "GBSUN", 20m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			Factory.Save();

			var docsAndCartage = shipment.DocsAndCartage;
			var baseDate = ZDateTime.MinSmallDateTimeValue;

			docsAndCartage.PickupCartageCoPK = client.PK;
			docsAndCartage.DeliveryCartageCoPK = client.PK;
			docsAndCartage.JP_PickupLabourCharge = 100m;
			docsAndCartage.JP_PickupLabourTime = baseDate.AddHours(3).AddMinutes(15);
			docsAndCartage.JP_PickupTruckWaitCharge = 80m;
			docsAndCartage.JP_PickupTruckWaitTime = baseDate.AddHours(5);

			docsAndCartage.JP_DeliveryLabourCharge = 125m;
			docsAndCartage.JP_DeliveryLabourTime = baseDate.AddHours(2);
			docsAndCartage.JP_DeliveryTruckWaitCharge = 350m;
			docsAndCartage.JP_DeliveryTruckWaitTime = baseDate.AddHours(1);
			docsAndCartage.JP_LCLAirStorageCharge = 10m;
			docsAndCartage.JP_LCLAirStorageDaysOrHours = new ZByte(49);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 325m,
					RevenueCalculationDescription = "OLAB: 3.25 Hour(s) @ AUD 100.00/Hour"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 400m,
					RevenueCalculationDescription = "ORGDMET: 5 Hour(s) @ AUD 80.00/Hour"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 250m,
					RevenueCalculationDescription = "DLAB: 2 Hour(s) @ AUD 125.00/Hour"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 350m,
					RevenueCalculationDescription = "DSTDMET: 1 Hour(s) @ AUD 350.00/Hour"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 490m,
					RevenueCalculationDescription = "DSTOR: 49 Hour(s) @ AUD 10.00/Hour"
				}
			};

			AutorateAndAssert("Expected spot rates to work like unit calculators with charge codes taken from registry", expected, shipment, client);
		}

		public void TestAutoratingServicesSpotRates_ShipmentStorageHoursOrDays()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DSTOR");
			var client = Helper.NewOrgHeader();

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "GBSUN", 20m);
			shipment.DocsAndCartage.DeliveryCartageCoPK = client.PK;
			shipment.DocsAndCartage.JP_LCLAirStorageCharge = 110m;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = new ZByte(4);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 440m,
					RevenueCalculationDescription = "DSTOR: 4 Hour(s) @ AUD 110.00/Hour"
				}
			};

			AutorateAndAssert("For Air Shipments storage is rated per hour", expected, shipment, client);

			shipment = CreateForwardingShipment(TransportModes.Road, client.PK, ZGuid.Empty, "AUSYD", "GBSUN", 20m);
			shipment.DocsAndCartage.DeliveryCartageCoPK = client.PK;
			shipment.DocsAndCartage.JP_LCLAirStorageCharge = 200m;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = new ZByte(3);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 600m,
					RevenueCalculationDescription = "DSTOR: 3 Day(s) @ AUD 200.00/Day"
				}
			};

			AutorateAndAssert("For any other mode storage is rated per day", expected, shipment, client);
		}

		public void TestAutoratingServicesSpotRates_ContainerServiceOverridesCosting()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTSTO", "Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(chargeCode);
			Factory.Save();

			RatingDataRegistry.Instance.DestinationStorageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var arrivalCTO = Helper.CreateCreditor("DESTCTO");
			var costing = Helper.NewCosting(arrivalCTO);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "CNSHA", "AUBNE");
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY, CurrencyCodes.Australia);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, Consignor.PK, ZGuid.Empty, "CNSHA", "AUBNE", 200m));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, Consignor.PK, ZGuid.Empty, "CNSHA", "AUBNE", 200m));

			var container = consol.Containers.AddNew();
			container.ArrivalCTOStorageDays = new ZByte(5);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 500m,
					CostCalculationDescription = "DSTSTO: 5 Day(s) @ AUD 100.00/Day"
				}
			};

			AutoCostAndAssert("Should be able to autorate spot service cost for demurrage", null, expected, consol, false);

			var storagePenalty = container.FindArrivalCTOStoragePenalty();
			storagePenalty.CPY_PerUnitCost = 180m;
			storagePenalty.CPY_TotalCost = 0m;

			expected = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 900m,
					CostCalculationDescription = "DSTSTO: 5 Day(s) @ AUD 180.00/Day"
				}
			};

			AutoCostAndAssert("Should prefer spot rate", null, expected, consol, false);
		}

		public void TestAutoratingServicesSpotRates_ShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup_Origin()
		{
			AssertShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup(ChargeCodeGroupList.Codes.Origin);
		}

		public void TestAutoratingServicesSpotRates_ShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup_Destination()
		{
			AssertShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup(ChargeCodeGroupList.Codes.Destination);
		}

		void AssertShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup(string chargeGroup)
		{
			var chargeCode1 = Helper.ChargeCodes.New("FUM1", "Fumigation Service 1", UnitCalculator.Code, chargeGroup, FreightServiceType.Codes.Fumigation);
			chargeCode1.AC_IsAdhocServiceCharge = true;
			var chargeCode2 = Helper.ChargeCodes.New("FUM2", "Fumigation Service 2", UnitCalculator.Code, chargeGroup, FreightServiceType.Codes.Fumigation);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(chargeGroup, RateMode.LSE, "AUSYD", "AUMEL");
			CreateRateLineWithUnitCalculator(entry, chargeCode1, QuantityUnit.SV, 100m);
			CreateRateLineWithUnitCalculator(entry, chargeCode2, QuantityUnit.SV, 150m);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "AUMEL", 20m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			shipment.JS_E_DEP = ZDateTime.Today;

			var service = Helper.CreateAdHocJobService(shipment.DocsAndCartage, FreightServiceType.Codes.Fumigation, 0m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
			service.ES_OA_Location = ZGuid.Empty;

			var isDestinationServiceTest = chargeGroup == ChargeCodeGroupList.Codes.Destination;
			var serviceDesc = "";
			if (isDestinationServiceTest)
			{
				service.ES_Completed = ZDateTime.Today.AddDays(1);
				serviceDesc = "Destination Fumigation";
			}
			else
			{
				service.ES_Completed = ZDateTime.Today.AddDays(-1);
				serviceDesc = "Origin Fumigation";
			}

			var logs = new[] {
				$"Information: RateLine Filtered FUM1-UNT-SV-Client Rate TESTORG1	reason:	replaced by Job Service ({chargeGroup} / FUM)",
				$"Information: RateLine Filtered FUM2-UNT-SV-Client Rate TESTORG1	reason:	replaced by Job Service ({chargeGroup} / FUM)"
			};

			AutorateAndAssertWithLogs("should not override any client rate as there is no FUM rate.",
				expectedCharges: new[]
				{
					new AssertionCharge
					{
						ChargeCode = chargeCode1.AC_Code,
						JR_OSSellAmt = 100,
						RevenueCalculationDescription = $"FUM1: 1 {serviceDesc} @ AUD 100.00/{serviceDesc}"
					},
					new AssertionCharge
					{
						ChargeCode = chargeCode2.AC_Code,
						JR_OSSellAmt = 150,
						RevenueCalculationDescription = $"FUM2: 1 {serviceDesc} @ AUD 150.00/{serviceDesc}"
					}
				},
				notExpectedLogs: logs,
				forwardingShipment: shipment,
				localClient: client
			);

			service.ES_ServiceRate = 750;

			AutorateAndAssertWithLogs("should override the client rates based on Charge Group and Charge Sub-group",
				expectedCharges: new[]
				{
					new AssertionCharge
					{
						ChargeCode = chargeCode1.AC_Code,
						JR_OSSellAmt = 750,
						RevenueCalculationDescription = $"FUM1: 1 {serviceDesc} @ AUD 750.00/{serviceDesc}"
					}
				},
				expectedLogs: logs,
				forwardingShipment: shipment,
				localClient: client
			);
		}

		public void TestAutoratingServicesSpotRates_ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_Labor() =>
			AssertShipmentPenaltyService_OverridesClientRateBasedOnChargeCode(
				chargeSubGroup: ChargeCodeSubGroupList.Labor,
				orgRegistryItem: RatingDataRegistry.Instance.OriginLaborServiceChargeCode,
				dstRegistryItem: RatingDataRegistry.Instance.DestinationLaborServiceChargeCode,
				serviceName: "Labor",
				setPickupTime: (shipment, time) => shipment.DocsAndCartage.JP_PickupLabourTime = time,
				setDeliveryTime: (shipment, time) => shipment.DocsAndCartage.JP_DeliveryLabourTime = time,
				setPickupCharge: (shipment, charge) => shipment.DocsAndCartage.JP_PickupLabourCharge = charge,
				setDeliveryCharge: (shipment, charge) => shipment.DocsAndCartage.JP_DeliveryLabourCharge = charge);

		public void AssertShipmentPenaltyService_OverridesClientRateBasedOnChargeCode(
			string chargeSubGroup,
			ChargeCodeRegistryItem orgRegistryItem,
			ChargeCodeRegistryItem dstRegistryItem,
			string serviceName,
			Action<ForwardingShipment, ZDateTime> setPickupTime,
			Action<ForwardingShipment, ZDateTime> setDeliveryTime,
			Action<ForwardingShipment, ZDecimal> setPickupCharge,
			Action<ForwardingShipment, ZDecimal> setDeliveryCharge)
		{
			var orgChargeCode1 = Helper.ChargeCodes.New("ORG1", "ORG Charge 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, chargeSubGroup);
			var orgChargeCode2 = Helper.ChargeCodes.New("ORG2", "ORG Charge 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, chargeSubGroup);
			var dstChargeCode1 = Helper.ChargeCodes.New("DST1", "DST Charge 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, chargeSubGroup);
			var dstChargeCode2 = Helper.ChargeCodes.New("DST2", "DST Charge 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, chargeSubGroup);

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "GBSUN");
			CreateRateLineWithUnitCalculator(orgEntry, orgChargeCode1, QuantityUnit.SV, 100m);
			CreateRateLineWithUnitCalculator(orgEntry, orgChargeCode2, QuantityUnit.HR, 200m);

			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "GBSUN");
			CreateRateLineWithUnitCalculator(dstEntry, dstChargeCode1, QuantityUnit.SV, 50m);
			CreateRateLineWithUnitCalculator(dstEntry, dstChargeCode2, QuantityUnit.HR, 70m);

			Factory.Save();

			using (orgRegistryItem.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgChargeCode1.PK.ToGuid()))
			using (dstRegistryItem.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstChargeCode1.PK.ToGuid()))
			{
				var shipment = CreateForwardingShipment(TransportModes.Air, client.PK, ZGuid.Empty, "AUSYD", "GBSUN", 20m);
				shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
				var baseDate = ZDateTime.MinSmallDateTimeValue;
				setPickupTime(shipment, baseDate.AddHours(3));
				setDeliveryTime(shipment, baseDate.AddHours(2));

				var logs = new[] {
					$"Information: RateLine Filtered ORG1-UNT-SV-Client Rate TESTORG1	reason:	replaced by Job Service (ORG / {chargeSubGroup})",
					$"Information: RateLine Filtered DST1-UNT-SV-Client Rate TESTORG1	reason:	replaced by Job Service (DST / {chargeSubGroup})"
				};

				AutorateAndAssertWithLogs("Should not override any client rate.",
					expectedCharges: new[]
					{
						new AssertionCharge
						{
							ChargeCode = orgChargeCode1.AC_Code,
							JR_OSSellAmt = 100m,
							RevenueCalculationDescription = $"ORG1: 1 Origin {serviceName} @ AUD 100.00/Origin {serviceName}"
						},
						new AssertionCharge
						{
							ChargeCode = orgChargeCode2.AC_Code,
							JR_OSSellAmt = 600m,
							RevenueCalculationDescription = "ORG2: 3 Hour(s) @ AUD 200.00/Hour"
						},
						new AssertionCharge
						{
							ChargeCode = dstChargeCode1.AC_Code,
							JR_OSSellAmt = 50m,
							RevenueCalculationDescription = $"DST1: 1 Destination {serviceName} @ AUD 50.00/Destination {serviceName}"
						},
						new AssertionCharge
						{
							ChargeCode = dstChargeCode2.AC_Code,
							JR_OSSellAmt = 140m,
							RevenueCalculationDescription = "DST2: 2 Hour(s) @ AUD 70.00/Hour"
						}
					},
					notExpectedLogs: logs,
					forwardingShipment: shipment,
					localClient: client
				);

				setPickupCharge(shipment, 60m);
				setDeliveryCharge(shipment, 120m);

				AutorateAndAssertWithLogs("Should override client rates.",
					expectedCharges: new[]
					{
						new AssertionCharge
						{
							ChargeCode = orgChargeCode1.AC_Code,
							JR_OSSellAmt = 180m,
							RevenueCalculationDescription = "ORG1: 3 Hour(s) @ AUD 60.00/Hour"
						},
						new AssertionCharge
						{
							ChargeCode = orgChargeCode2.AC_Code,
							JR_OSSellAmt = 600m,
							RevenueCalculationDescription = "ORG2: 3 Hour(s) @ AUD 200.00/Hour"
						},
						new AssertionCharge
						{
							ChargeCode = dstChargeCode1.AC_Code,
							JR_OSSellAmt = 240m,
							RevenueCalculationDescription = "DST1: 2 Hour(s) @ AUD 120.00/Hour"
						},
						new AssertionCharge
						{
							ChargeCode = dstChargeCode2.AC_Code,
							JR_OSSellAmt = 140m,
							RevenueCalculationDescription = "DST2: 2 Hour(s) @ AUD 70.00/Hour"
						}
					},
					expectedLogs: logs,
					forwardingShipment: shipment,
					localClient: client
				);
			}
		}

		[TestDate(2016, 06, 06)]
		public void TestAutoratingServicesSpotRates_ShipmentLog()
		{
			var dstDemurrage = Helper.ChargeCodes.NewConsolChargeCode("DSTDME", "DST Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal).PK.ToGuid();
			RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstDemurrage);

			Factory.Save();

			var client = Helper.NewOrgHeader();
			var shipment = CreateForwardingShipment(TransportModes.Sea, client.PK, ZGuid.Empty, "CNSHA", "AUSYD", 20m);
			shipment.JS_UniqueConsignRef = "S100216";
			shipment.JS_HouseBill = "Clinton";
			var baseDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			shipment.DocsAndCartage.JP_DeliveryTruckWaitCharge = 125m;
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = baseDate.AddHours(2);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Shipments.Add(shipment);

			var container = consol.Containers.AddNew();
			container.ArrivalTruckWaitCost = 180m;
			container.ArrivalTruckWaitTime = baseDate.AddHours(1);

			Factory.Save();

			var expectedDescription = @"2 Hour(s) @ AUD 125.00/Hour
Delivery Demurrage Service Charge is applicable for Shipment S100216 (House Bill='CLINTON') Delivery.

Mode:			ALL
Charge Code Group:	DST / DME
Origin:			CNSHA
Destination:		AUSYD
Autorated for:		Shipment S100216 (House Bill='CLINTON')
Leg:			CNSHA-AUSYD";

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 250m,
					RevenueCalculationDescription = expectedDescription
				}
			};

			AutorateAndAssert("Spot Rate for shipment sell should both be included on the DSTDME rate. Can't apportion from shipment side", expected, shipment, client);

			var expectedLogLine = @"Information: RateLine Found DSTDME-Job Delivery Demurrage Service";

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected line", expectedLogLine);
		}

		public void TestAutoratingServicesSpotRates_ConsolCanAutoRateBothCostAndSellServiceSpotRates()
		{
			var dstDemurrage = Helper.ChargeCodes.NewConsolChargeCode("DSTDME", "DST Demurrage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal).PK.ToGuid();
			RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, dstDemurrage);

			var forwarder = Helper.NewOrgHeader(1);
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, client.PK, "CNSHA", "AUSYD", 20m);
			var baseDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			shipment.DocsAndCartage.JP_DeliveryTruckWaitCharge = 125m;
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = baseDate.AddHours(2);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			consol.Shipments.Add(shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.ArrivalTruckWaitTime = baseDate.AddHours(1);
			var truckWaitPenalty = container.FindArrivalTruckWaitPenalty();
			truckWaitPenalty.CPY_PerUnitCost = 180m;
			truckWaitPenalty.CPY_TotalCost = 0m;

			Factory.Save();

			var expectedChargesForShipment = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 180m,
					CostCalculationDescription = "DSTDME: 1 Hour(s) @ AUD 180.00/Hour",
					JR_OSSellAmt = 250m,
					RevenueCalculationDescription = "DSTDME: 2 Hour(s) @ AUD 125.00/Hour"
				}
			};

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, expectedChargesForShipment }
			};

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 180m,
					CostCalculationDescription = "DSTDME: 1 Hour(s) @ AUD 180.00/Hour"
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AutoCostAndAssert("Expected dst demurrage charges for both consol and shipment services", expectedCharges, expectedCosts, consol);
		}

		#endregion

		#region Special Services for Consol Container

		public void TestServicesAreCalculatedPerContainer()
		{
			var subgroup = ChargeCodeSubGroupList.Storage;
			var chargeGroup = ChargeCodeGroupList.Codes.Destination;
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CNTCN1", "", UnitCalculator.Code, chargeGroup, subgroup);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);
			var entry1 = costing.AddRateEntry(chargeGroup, RateMode.FCL, "NZ", "AUMEL");
			entry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			entry1.TI_RC = GP40.PK;

			var line1 = entry1.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line1.GetCalculator<UnitCalculator>().PerUnit = 150;

			var entry2 = costing.AddRateEntry(chargeGroup, RateMode.FCL, "NZ", "AUMEL");
			entry2.TI_RX_NKCurrency = CurrencyCodes.Australia;
			entry2.TI_RC = GP20.PK;

			var line2 = entry2.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line2.GetCalculator<UnitCalculator>().PerUnit = 999;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.CreditorPK = creditor.PK;
			consol.JK_OA_ArrivalCTOAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(3, 0, 0, 0), 3, 3, 3);
			CreateContainerWithServices(consol, GP20, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(3, 0, 0, 0), 3, 3, 3);
			CreateContainerWithServices(consol, GP20, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(1, 0, 0, 0), 1, 1, 1);
			CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(1, 0, 0, 0), 1, 1, 1);
			CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(4, 0, 0, 0), 4, 4, 4);
			CreateContainerWithServices(consol, Helper.Containers["40RE"], creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(1, 0, 0, 0), 1, 1, 1);

			foreach (ForwardingContainer container in consol.Containers)
			{
				container.PackLines.Add(shipment1.OuterPackLines.AddNew());
				container.PackLines.Add(shipment2.OuterPackLines.AddNew());
			}

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CNTCN1",
					E6_OSCostAmount = 450m,// (3x150)
				},
				new AssertionCost
				{
					ChargeCode = "CNTCN1",
					E6_OSCostAmount = 1998m,// (2x999)
				}
			};

			AutoCostAndAssert("Should merge the rates for the 2x40RE and 3x40GP containers", null, expectedCosts, consol, false);
		}

		public void TestRefContainerDifferentiatesOtherwiseIdenticalRates_ForServices()
		{
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM2", "Fumigation 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rc1 = GP40;
			var rc2 = Helper.Containers["40RE"];

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUMEL");
			entry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			entry1.TI_RC = rc1.PK;

			var line1SV = entry1.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV);
			line1SV.GetCalculator<UnitCalculator>().PerUnit = 25;

			var line1HR = entry1.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.HR);
			line1HR.GetCalculator<UnitCalculator>().PerUnit = 50;

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUMEL");
			entry2.TI_RX_NKCurrency = CurrencyCodes.Australia;
			entry2.TI_RC = rc2.PK;

			var line2SV = entry2.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV);
			line2SV.GetCalculator<UnitCalculator>().PerUnit = 100;

			var line2HR = entry2.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.HR);
			line2HR.GetCalculator<UnitCalculator>().PerUnit = 200;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.CreditorPK = creditor.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			CreateContainerWithServices(consol, rc1, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(4, 0, 0));
			CreateContainerWithServices(consol, rc2, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(4, 0, 0));

			foreach (ForwardingContainer container in consol.Containers)
			{
				container.PackLines.Add(shipment1.OuterPackLines.AddNew());
				container.PackLines.Add(shipment2.OuterPackLines.AddNew());
			}

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = chargeCode1.AC_Code,
					E6_OSCostAmount = 100m,
					CostCalculationDescription = "DSTFUM1: 1 Destination Fumigation @ AUD 100.00/Destination Fumigation"
				},
				new AssertionCost
				{
					ChargeCode = chargeCode2.AC_Code,
					E6_OSCostAmount = 800m,
					CostCalculationDescription = "DSTFUM2: 4 Hour(s) @ AUD 200.00/Hour"
				},
				new AssertionCost
				{
					ChargeCode = chargeCode1.AC_Code,
					E6_OSCostAmount = 25m,
					CostCalculationDescription = "DSTFUM1: 1 Destination Fumigation @ AUD 25.00/Destination Fumigation"
				},
				new AssertionCost
				{
					ChargeCode = chargeCode2.AC_Code,
					E6_OSCostAmount = 200m,
					CostCalculationDescription = "DSTFUM2: 4 Hour(s) @ AUD 50.00/Hour"
				}
			};

			AutoCostAndAssert("Should produce charges for 40RE and 40GP as there are two containers with fumigation services", null, expectedCosts, consol, false);
		}

		public void TestContainerPenalties_Import_ConsolCosting_ProviderPriorities()
		{
			// Testing rate search by provider priority and charge creditor.
			// Given
			// - the penalty does not have an explicit cost so a rate search is needed
			// - the penalty has a creditor defined
			// - the penalty creditor does not have a cost rate
			// then verify
			// - for Truck Wait, the rate search priority is Consol > Arrival > Transport provider -> Consol Creditor
			// - for CTO Storage, the rate search priority is Consol > Arrival > CTO -> Consol Creditor
			// - for Carrier Storage/Detention, the rate search priority is Consol > Creditor -> Consol > Carrier
			// - the creditor on all charges is the penalty creditor

			var truckWaitChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESTWT", "Dest Truck Wait", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var detentionChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESDEN", "Dest Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var ctoStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESCTOSTO", "Dest CTO Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var carrierStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESCARSTO", "Dest Carrier Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage);

			var truckWaitCreditor = Helper.CreateCreditor("TWTCREDIT");
			var ctoStorageCreditor = Helper.CreateCreditor("STGCTOCRD");
			var carrierStorageCreditor = Helper.CreateCreditor("STGCARCRD");
			var detentionCreditor = Helper.CreateCreditor("DTNCREDIT");
			var consolCreditor = Helper.CreateCreditor("CONCREDIT");
			var arrivalTransportProvider = Helper.CreateCreditor("DESTTRAN");
			var arrivalCTO = Helper.CreateCreditor("DESTCTO");
			var carrier = CreateCarrierOrg();

			var consol = NewImportConsolWithCollectPayment(consolCreditor, carrier);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransportProvider.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			var container = AddContainerAndShipment(consol);

			var truckWaitPenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
			truckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
			truckWaitPenalty.CPY_OH_Creditor = truckWaitCreditor.PK;

			var ctoStoragePenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			ctoStoragePenalty.CPY_Duration = TimeSpan.FromDays(4);
			ctoStoragePenalty.CPY_OH_Creditor = ctoStorageCreditor.PK;

			var carrierStoragePenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			carrierStoragePenalty.CPY_Duration = TimeSpan.FromDays(5);
			carrierStoragePenalty.CPY_OH_Creditor = carrierStorageCreditor.PK;

			var detentionPenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			detentionPenalty.CPY_Duration = TimeSpan.FromDays(6);
			detentionPenalty.CPY_OH_Creditor = detentionCreditor.PK;

			// 1. Costing on carrier only
			/////////////////////////////
			var carrierCosting = Helper.NewCosting(carrier);
			AddPenaltyEntryWithUnitCharges(carrierCosting, 400m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 402m,
					E6_OH_Creditor = carrierStorageCreditor.PK,
					ChargeCode = carrierStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 6 * 403,
					E6_OH_Creditor = detentionCreditor.PK,
					ChargeCode = detentionChargeCode.AC_Code,
				}
			};

			AutoCostAndAssert("Should rate only penalties with carrier creditor", null, expectedCosts, consol, false, true, false);
			DeleteExistingCosts(consol.CostSupporter.PK);

			// 2. Costing on carrier and consol > creditor
			//////////////////////////////////////////////
			var consolCreditorCosting = Helper.NewCosting(consolCreditor);
			AddPenaltyEntryWithUnitCharges(consolCreditorCosting, 100m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 102m,
					E6_OH_Creditor = carrierStorageCreditor.PK,
					ChargeCode = carrierStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 6 * 103,
					E6_OH_Creditor = detentionCreditor.PK,
					ChargeCode = detentionChargeCode.AC_Code,
				}
			};

			AutoCostAndAssert("Should rate only penalties with carrier creditor", null, expectedCosts, consol, false, true, false);
			DeleteExistingCosts(consol.CostSupporter.PK);

			// 3. Costing on Carrier, Creditor, Arrival PortTransport, Arrival CTO
			//////////////////////////////////////////////////////////////////////
			var arrivalTransportProviderCosting = Helper.NewCosting(arrivalTransportProvider);
			AddPenaltyEntryWithUnitCharges(arrivalTransportProviderCosting, 200m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);

			var arrivalCTOCosting = Helper.NewCosting(arrivalCTO);
			AddPenaltyEntryWithUnitCharges(arrivalCTOCosting, 300m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 3 * 200m,
					E6_OH_Creditor = truckWaitCreditor.PK,
					ChargeCode = truckWaitChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 4 * 301m,
					E6_OH_Creditor = ctoStorageCreditor.PK,
					ChargeCode = ctoStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 102m,
					E6_OH_Creditor = carrierStorageCreditor.PK,
					ChargeCode = carrierStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 6 * 103,
					E6_OH_Creditor = detentionCreditor.PK,
					ChargeCode = detentionChargeCode.AC_Code,
				}
			};
			AutoCostAndAssert("Should find cost on appropriate provider when none on penalty creditor", null, expectedCosts, consol, false, true, false);
			DeleteExistingCosts(consol.CostSupporter.PK);

			// 4. Costing on all providers and all grid creditors
			// then verify
			// - for all charges, the rate on the penalty creditor is chosen
			// - the creditor on all charges is the penalty creditor
			/////////////////////////////////////////////////////////
			var truckWaitCosting = Helper.NewCosting(truckWaitCreditor);
			var detentionCosting = Helper.NewCosting(detentionCreditor);
			var ctoStorageCosting = Helper.NewCosting(ctoStorageCreditor);
			var carrierStorageCosting = Helper.NewCosting(carrierStorageCreditor);
			AddPenaltyEntryWithUnitCharges(truckWaitCosting, 510m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);
			AddPenaltyEntryWithUnitCharges(ctoStorageCosting, 520m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);
			AddPenaltyEntryWithUnitCharges(carrierStorageCosting, 530m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);
			AddPenaltyEntryWithUnitCharges(detentionCosting, 540m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", truckWaitChargeCode, ctoStorageChargeCode, carrierStorageChargeCode, detentionChargeCode);

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 3 * 510m,
					E6_OH_Creditor = truckWaitCreditor.PK,
					ChargeCode = truckWaitChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 4 * 521m,
					E6_OH_Creditor = ctoStorageCreditor.PK,
					ChargeCode = ctoStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 532m,
					E6_OH_Creditor = carrierStorageCreditor.PK,
					ChargeCode = carrierStorageChargeCode.AC_Code,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 6 * 543,
					E6_OH_Creditor = detentionCreditor.PK,
					ChargeCode = detentionChargeCode.AC_Code,
				}
			};
			AutoCostAndAssert("Should find costs on penalty creditor over all other providers", null, expectedCosts, consol, false, true, deleteExistingCosts: false);
		}

		public void TestContainerPenalties_Import_ShipmentCosting()
		{
			// Testing container penalties apply when rating costs on a shipment
			// Given
			// - Truck Wait Time on both shipment and consol container
			// - Detention on container
			// - Storage on container only
			// - costing with non-consol charge codes for all services
			// then verify
			// - for Truck Wait, the duration on the container is chosen
			// - for Detention, the duration on the container is chosen
			// - for Storage, the duration on the container is chosen

			var truckWaitChargeCode = Helper.ChargeCodes.New("DESTWT", "Dest Truck Wait", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var detentionChargeCode = Helper.ChargeCodes.New("DESDEN", "Dest Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var storageChargeCode = Helper.ChargeCodes.New("DESTOR", "Dest Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);

			var carrier = CreateCarrierOrg();
			var costing = Helper.NewCosting(carrier);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AU");
			entry.AddRateLine(truckWaitChargeCode, UnitCalculator.Code, QuantityUnit.HR).GetCalculator<UnitCalculator>().PerUnit = 5;
			entry.AddRateLine(detentionChargeCode, UnitCalculator.Code, QuantityUnit.DY).GetCalculator<UnitCalculator>().PerUnit = 7;
			entry.AddRateLine(storageChargeCode, UnitCalculator.Code, QuantityUnit.DY).GetCalculator<UnitCalculator>().PerUnit = 11;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "NZAKL", "AUSYD", 0m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "NZAKL", "AUSYD", carrier, shipment1);
			consol.JK_PrepaidCollect = PaymentType.Collect;
			var container = AddContainer(shipment1);

			var shipment = consol.Shipments[0];
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = TimeSpan.FromHours(11);
			container.ArrivalTruckWaitTime = TimeSpan.FromHours(9);
			var truckWaitPenalty = container.FindArrivalTruckWaitPenalty();
			truckWaitPenalty.CPY_OH_Creditor = carrier.PK;

			container.ArrivalCarrierDetentionDays = 4;
			var detentionPenalty = container.FindArrivalCarrierDetentionPenalty();
			detentionPenalty.CPY_OH_Creditor = carrier.PK;

			container.ArrivalCTOStorageDays = 3;
			var storagePenalty = container.FindArrivalCTOStoragePenalty();
			storagePenalty.CPY_OH_Creditor = carrier.PK;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = (int)container.ArrivalTruckWaitTime.ToTimeSpan().TotalHours * 5m,
					ChargeCode = truckWaitChargeCode.AC_Code,
				},
				new AssertionCharge
				{
					JR_OSCostAmt = container.ArrivalCarrierDetentionDays * 7m,
					ChargeCode = detentionChargeCode.AC_Code,
				},
				new AssertionCharge
				{
					JR_OSCostAmt = container.ArrivalCTOStorageDays * 11m,
					ChargeCode = storageChargeCode.AC_Code,
				}
			};
			AutorateAndAssert(expectedCosts, shipment, null, autorateCosts: true, autorateRevenue: false);
		}

		public void TestContainerPenalties_Import_ConsolCosting_LocationPriorities()
		{
			// Testing rate search by location priority
			// Given
			// - the penalty does not have an explicit cost so a rate search is needed
			// - the penalty has a location different from the default
			// then verify
			// - the rate that matches the penalty location is chosen
			// - other destination rates (e.g., destination documention) at the penalty location are not charged

			var truckWaitChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESTWT", "Dest Truck Wait", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var detentionChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESDEN", "Dest Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var storageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESTOR", "Dest Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var ddoc1ChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DDOC1", "Dest Doco 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var ddoc2ChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DDOC2", "Dest Doco 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var penaltyCreditor = Helper.CreateCreditor("PENCREDIT");
			var consolCreditor = Helper.CreateCreditor("CONCREDIT");

			var consol = NewImportConsolWithCollectPayment(consolCreditor, null);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = consolCreditor.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = consolCreditor.MainAddress.PK;
			var container = AddContainerAndShipment(consol);

			container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
			var penalty = container.FindArrivalTruckWaitPenalty();
			penalty.CPY_OH_Creditor = penaltyCreditor.PK;
			penalty.CPY_RL_NKLocation = "AUMEL";

			container.ArrivalCarrierDetentionDays = 5;
			var detentionPenalty = container.FindArrivalCarrierDetentionPenalty();
			detentionPenalty.CPY_OH_Creditor = penaltyCreditor.PK;
			detentionPenalty.CPY_RL_NKLocation = "AUBNE";

			container.ArrivalCTOStorageDays = 3;
			var storagePenalty = container.FindArrivalCTOStoragePenalty();
			storagePenalty.CPY_OH_Creditor = penaltyCreditor.PK;
			storagePenalty.CPY_RL_NKLocation = "AUPER";

			var consolCreditorCosting = Helper.NewCosting(consolCreditor);
			var entry5 = consolCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUSYD");
			AddFlatCharges(entry5, 550m, ddoc2ChargeCode, truckWaitChargeCode, detentionChargeCode, storageChargeCode);
			var entry6 = consolCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUMEL");
			AddFlatCharges(entry6, 560m, ddoc1ChargeCode, truckWaitChargeCode, detentionChargeCode, storageChargeCode);
			var entry7 = consolCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUPER");
			AddFlatCharges(entry7, 570m, ddoc1ChargeCode, truckWaitChargeCode, detentionChargeCode, storageChargeCode);
			var entry8 = consolCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUBNE");
			AddFlatCharges(entry8, 580m, ddoc1ChargeCode, truckWaitChargeCode, detentionChargeCode, storageChargeCode);
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 561m,
					ChargeCode = "DESTWT",
					E6_OH_Creditor = penaltyCreditor.PK,
				},
				new AssertionCost
				{
					E6_OSCostAmount = 582m,
					ChargeCode = "DESDEN",
					E6_OH_Creditor = penaltyCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 573m,
					ChargeCode = "DESTOR",
					E6_OH_Creditor = penaltyCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 550m,
					ChargeCode = "DDOC2",
					E6_OH_Creditor = consolCreditor.PK
				}
			};
			AutoCostAndAssert("Should find cost matching penalty location", null, expectedCosts, consol, false, true, false);
			DeleteExistingCosts(consol.CostSupporter.PK);

			var penaltyCreditorCosting = Helper.NewCosting(penaltyCreditor);
			var entry1 = penaltyCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUSYD");
			AddFlatCharges(entry1, 100m, truckWaitChargeCode, detentionChargeCode, storageChargeCode);

			var entry2 = penaltyCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUMEL");
			AddFlatCharges(entry2, 200m, truckWaitChargeCode, detentionChargeCode, storageChargeCode);

			var entry3 = penaltyCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUPER");
			AddFlatCharges(entry3, 300m, truckWaitChargeCode, detentionChargeCode, storageChargeCode);

			var entry4 = penaltyCreditorCosting.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUBNE");
			AddFlatCharges(entry4, 400m, truckWaitChargeCode, detentionChargeCode, storageChargeCode);

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 200m,
					ChargeCode = "DESTWT",
					E6_OH_Creditor = penaltyCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 401m,
					ChargeCode = "DESDEN",
					E6_OH_Creditor = penaltyCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 302m,
					ChargeCode = "DESTOR",
					E6_OH_Creditor = penaltyCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 550m,
					ChargeCode = "DDOC2",
					E6_OH_Creditor = consolCreditor.PK
				},
			};
			AutoCostAndAssert("Should find cost matching penalty location", null, expectedCosts, consol, false, true, false);
		}

		public void TestContainerPenalties_Export_ConsolCosting()
		{
			// Testing export container penalties of all types will rate.
			// Given
			// - consol with all possible penalties
			// - a costing with rates for every penalty
			// then verify
			// - for each penalty a charge is added to consol

			var originTruckWaitChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGTWT", "Origin Truck Wait", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var originDetentionChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGDEN", "Origin Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention);
			var originCtoStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGSTG", "Origin CTO Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage);
			var originCarrierStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGSTC", "Origin Carrier Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CarrierStorage);

			var originTruckWaitCreditor = Helper.CreateCreditor("ORGTWT1");
			var originStorageCTO1Creditor = Helper.CreateCreditor("ORGSTGCTO1");
			var originStorageCTO2Creditor = Helper.CreateCreditor("ORGSTGCTO2");
			var originStorageCarrierCreditor = Helper.CreateCreditor("ORGSTGCAR1");
			var originDetentionCreditor = Helper.CreateCreditor("ORGDEN1");
			var consolCreditor = Helper.CreateCreditor("CONCREDIT");
			var originTransportProvider = Helper.CreateCreditor("ORGTRAN");
			var originCTO = Helper.CreateCreditor("ORGCTO");
			var carrier = CreateCarrierOrg();

			var consol = NewExportConsolWithPrepaidPayment("C00001111", consolCreditor, carrier);
			consol.JK_OA_DeparturePackCFSTransportAddress = originTransportProvider.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = originCTO.MainAddress.PK;
			var container = AddContainerAndShipment(consol);

			var originTruckWaitPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
			originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
			originTruckWaitPenalty.CPY_OH_Creditor = originTruckWaitCreditor.PK;

			var originStorageCTO1Penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			originStorageCTO1Penalty.CPY_Duration = TimeSpan.FromDays(4);
			originStorageCTO1Penalty.CPY_OH_Creditor = originStorageCTO1Creditor.PK;

			var originStorageCTO2Penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			originStorageCTO2Penalty.CPY_Duration = TimeSpan.FromDays(5);
			originStorageCTO2Penalty.CPY_OH_Creditor = originStorageCTO2Creditor.PK;

			var originStorageCarrierPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			originStorageCarrierPenalty.CPY_Duration = TimeSpan.FromDays(6);
			originStorageCarrierPenalty.CPY_OH_Creditor = originStorageCarrierCreditor.PK;

			var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(7);
			originDetentionPenalty.CPY_OH_Creditor = originDetentionCreditor.PK;

			CreatePenaltyCostingWithUnitCharges(originTransportProvider, 10m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(consolCreditor, 20m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(carrier, 30m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originCTO, 40m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originTruckWaitCreditor, 50m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originStorageCTO1Creditor, 60m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originStorageCTO2Creditor, 70m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originStorageCarrierCreditor, 80m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);
			CreatePenaltyCostingWithUnitCharges(originDetentionCreditor, 90m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", originTruckWaitChargeCode, originDetentionChargeCode, originCtoStorageChargeCode, originCarrierStorageChargeCode);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 3 * 50m,
					ChargeCode = originTruckWaitChargeCode.AC_Code,
					E6_OH_Creditor = originTruckWaitCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 7 * 91m,
					ChargeCode = originDetentionChargeCode.AC_Code,
					E6_OH_Creditor = originDetentionCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 4 * 62m,
					ChargeCode = originCtoStorageChargeCode.AC_Code,
					E6_OH_Creditor = originStorageCTO1Creditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 72m,
					ChargeCode = originCtoStorageChargeCode.AC_Code,
					E6_OH_Creditor = originStorageCTO2Creditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 6 * 83m,
					ChargeCode = originCarrierStorageChargeCode.AC_Code,
					E6_OH_Creditor = originStorageCarrierCreditor.PK
				}
			};

			AutoCostAndAssert("Should find cost on penalty creditor", null, expectedCosts, consol, false, true, false);
		}

		public void TestContainerPenalties_MultipleStoragePenaltiesAtDifferentLocations()
		{
			// Testing multiple container storage penalties are rated separately when at different locations
			// Given
			// - consol with multiple container storage penalties with different providers and different locations
			// - costing for each storage provider at every location
			// then verify
			// - for each penalty a charge is added to consol for the correct provider and location

			var originStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGSTG", "Origin Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage);

			var originStorageCTO1Creditor = Helper.CreateCreditor("ORGSTGCTO1");
			var originStorageCTO2Creditor = Helper.CreateCreditor("ORGSTGCTO2");

			var consol = NewExportConsolWithPrepaidPayment("C00001111", null, null);
			var container = AddContainerAndShipment(consol);

			var originStorageCTO1Penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			originStorageCTO1Penalty.CPY_Duration = TimeSpan.FromDays(4);
			originStorageCTO1Penalty.CPY_OH_Creditor = originStorageCTO1Creditor.PK;

			var originStorageCTO2Penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			originStorageCTO2Penalty.CPY_Duration = TimeSpan.FromDays(5);
			originStorageCTO2Penalty.CPY_OH_Creditor = originStorageCTO2Creditor.PK;
			originStorageCTO2Penalty.CPY_RL_NKLocation = "AUNTL";

			var costing1 = Helper.NewCosting(originStorageCTO1Creditor);
			AddPenaltyEntryWithUnitCharges(costing1, 62m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "", originStorageChargeCode);
			AddPenaltyEntryWithUnitCharges(costing1, 65m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AUNTL", "", originStorageChargeCode);
			var costing2 = Helper.NewCosting(originStorageCTO2Creditor);
			AddPenaltyEntryWithUnitCharges(costing2, 70m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "", originStorageChargeCode);
			AddPenaltyEntryWithUnitCharges(costing2, 72m, RatingConstants.RateCategory.ORG, RateMode.FCL, "AUNTL", "", originStorageChargeCode);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 4 * 62m,
					ChargeCode = originStorageChargeCode.AC_Code,
					E6_OH_Creditor = originStorageCTO1Creditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 5 * 72m,
					ChargeCode = originStorageChargeCode.AC_Code,
					E6_OH_Creditor = originStorageCTO2Creditor.PK
				}
			};

			AutoCostAndAssert("Should rate each penalty", null, expectedCosts, consol, false, true, false);
		}

		public void TestContainerPenalties_MultipleStoragePenalties_RatesWithLessSpecificLocations()
		{
			// Testing multiple container storage penalties with different penalty creditors are rated separately when
			// provider rate locations are a mix of country and port
			// Given
			// - consol with two container storage penalties with different penalty creditors and same location
			// - costing for one penalty creditor has a location of country
			// - costing for other penalty creditor has a location of port
			// then verify
			// - for each penalty a charge is added to consol for the correct penalty creditor
			// - That is, the rate with the more specific location does not override the rate with the generic country location
			//   since they are for different services.

			var destinationCtoStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTSTG", "Destination CTO Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var destinationCarrierStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTSTC", "Destination Carrier Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage);
			var someOtherDestinationCarrierStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ODTSTC", "Other Destination Carrier Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage);

			var destinationStorageCTOCreditor = Helper.CreateCreditor("DSTSTGCTO1");
			var carrier = CreateCarrierOrg();

			var consol = NewImportConsolWithCollectPayment(carrier, null);
			consol.JK_UniqueConsignRef = "C00001111";
			var container = AddContainerAndShipment(consol);

			var destinationStorageCTOPenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			destinationStorageCTOPenalty.CPY_Duration = TimeSpan.FromDays(2);
			destinationStorageCTOPenalty.CPY_OH_Creditor = destinationStorageCTOCreditor.PK;

			var carrierPenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			carrierPenalty.CPY_Duration = TimeSpan.FromDays(10);
			carrierPenalty.CPY_OH_Creditor = carrier.PK;

			var costing1 = Helper.NewCosting(destinationStorageCTOCreditor);
			AddPenaltyEntryWithUnitCharges(costing1, 60m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", destinationCtoStorageChargeCode, destinationCarrierStorageChargeCode);
			var costing2 = Helper.NewCosting(carrier);
			AddPenaltyEntryWithUnitCharges(costing2, 70m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AUSYD", destinationCtoStorageChargeCode, destinationCarrierStorageChargeCode);
			AddPenaltyEntryWithUnitCharges(costing2, 80m, RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", destinationCtoStorageChargeCode, destinationCarrierStorageChargeCode, someOtherDestinationCarrierStorageChargeCode);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 2 * 60m,
					ChargeCode = destinationCtoStorageChargeCode.AC_Code,
					E6_OH_Creditor = destinationStorageCTOCreditor.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 10 * 71m,
					ChargeCode = destinationCarrierStorageChargeCode.AC_Code,
					E6_OH_Creditor = carrier.PK
				},
				new AssertionCost
				{
					E6_OSCostAmount = 10 * 82m,
					ChargeCode = someOtherDestinationCarrierStorageChargeCode.AC_Code,
					E6_OH_Creditor = carrier.PK
				}
			};

			AutoCostAndAssert("Should rate each penalty", null, expectedCosts, consol, false, true, false);
		}

		Costing CreatePenaltyCostingWithUnitCharges(OrgHeader supplier, decimal startingPrice, string rateCategory, string rateMode, string origin, string dest, params AccChargeCode[] chargeCodes)
		{
			var costing = Helper.NewCosting(supplier);
			AddPenaltyEntryWithUnitCharges(costing, startingPrice, rateCategory, rateMode, origin, dest, chargeCodes);
			return costing;
		}

		RateEntry AddPenaltyEntryWithUnitCharges(Costing costing, decimal startingPrice, string rateCategory, string rateMode, string origin, string dest, params AccChargeCode[] chargeCodes)
		{
			var entry = costing.AddRateEntry(rateCategory, rateMode, origin, dest);
			foreach (var chargeCode in chargeCodes)
			{
				string lineUnit = chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.CartageDemurrageTotal
					? QuantityUnit.HR
					: QuantityUnit.DY;
				entry.AddRateLine(chargeCode, UnitCalculator.Code, lineUnit)
					.GetCalculator<UnitCalculator>().PerUnit = startingPrice++;
			}

			return entry;
		}

		void AddFlatCharges(RateEntry entry, decimal startingPrice, params AccChargeCode[] chargeCodes)
		{
			foreach (var chargeCode in chargeCodes)
			{
				entry.AddRateLine(chargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = startingPrice++;
			}
		}

		ForwardingConsol NewImportConsolWithCollectPayment(OrgHeader creditor, OrgHeader carrier)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			if (creditor != null)
			{
				consol.CreditorPK = creditor.PK;
			}
			if (carrier != null)
			{
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}
			return consol;
		}

		ForwardingConsol NewExportConsolWithPrepaidPayment(string consolId, OrgHeader creditor, OrgHeader carrier)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolId;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			if (creditor != null)
			{
				consol.CreditorPK = creditor.PK;
			}
			if (carrier != null)
			{
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}
			return consol;
		}

		CommonContainer AddContainerAndShipment(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = consol.JK_TransportMode;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_ContainerCount = new ZShort(1);
			container.JC_RC = GP40.PK;
			consol.Containers.Add(container);
			container.PackLines.Add(shipment.OuterPackLines.AddNew());
			return container;
		}

		CommonContainer AddContainer(ForwardingShipment shipment)
		{
			if (shipment.JS_PackingMode != ContainerModes.FCL)
			{
				shipment.JS_PackingMode = ContainerModes.FCL;
			}
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_ContainerCount = new ZShort(1);
			container.JC_RC = GP40.PK;
			shipment.Consols[0].Containers.Add(container);
			container.PackLines.Add(shipment.OuterPackLines.AddNew());
			return container;
		}

		OrgHeader CreateCarrierOrg(string scac = "SCAC")
		{
			var carrier = Helper.CreateCreditor(scac + "CARRIER");
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;

			if (string.IsNullOrWhiteSpace(scac))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = scac;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			return carrier;
		}

		public void TestDuplicatedPenaltiesAndServices_SumUpPenaltiesAndDuplicateServices()
		{
			var ccHRSV1 = Helper.ChargeCodes.NewConsolChargeCode("CCHRSV1", "", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal);
			var ccCNSV1 = Helper.ChargeCodes.NewConsolChargeCode("CCCNSV1", "", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			var ccSCSV1 = Helper.ChargeCodes.NewConsolChargeCode("CCSCSV1", "", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var ccSCFUM = Helper.ChargeCodes.NewConsolChargeCode("CCSCFUM", "", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "NZ", "AUSYD");
			rateEntry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntry1.TI_RC = GP40.PK;

			var rlHR = rateEntry1.AddRateLine(ccHRSV1, UnitCalculator.Code, QuantityUnit.HR);
			var rlCN = rateEntry1.AddRateLine(ccCNSV1, UnitCalculator.Code, QuantityUnit.CN);
			var rlSV1 = rateEntry1.AddRateLine(ccSCSV1, UnitCalculator.Code, QuantityUnit.SV);
			var rlSV2 = rateEntry1.AddRateLine(ccSCFUM, UnitCalculator.Code, QuantityUnit.SV);

			rlHR.GetCalculator<UnitCalculator>().PerUnit = 5;
			rlCN.GetCalculator<UnitCalculator>().PerUnit = 150;
			rlSV1.GetCalculator<UnitCalculator>().PerUnit = 50;
			rlSV2.GetCalculator<UnitCalculator>().PerUnit = 50;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = creditor.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ArrivalCTOAddress = creditor.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = creditor.MainAddress.PK;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var container = CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(3, 0, 0, 0), 3, 3, 3);
			container.PackLines.Add(shipment1.OuterPackLines.AddNew());
			container.PackLines.Add(shipment2.OuterPackLines.AddNew());

			var container1 = CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(2, 0, 0, 0), 2, 2, 2);
			var container2 = CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(1, 0, 0, 0), 1, 1, 1);

			container1.PackLines.Add(shipment1.OuterPackLines.AddNew());
			container2.PackLines.Add(shipment1.OuterPackLines.AddNew());

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 720,// 3 * 24 + 2 * 24 + 1 * 24
					CostCalculationDescription = "CCHRSV1: 144 Hour(s) @ AUD 5.00/Hour",
				},
				new AssertionCost
				{
					E6_OSCostAmount = 450m,
					CostCalculationDescription = "CCCNSV1: 3 40GP Container(s) @ AUD 150.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "CCSCSV1",
					E6_OSCostAmount = 150m,
					CostCalculationDescription = $"Destination CTO Storage (Penalty Type - STO) @ AUD 50.00/Destination CTO Storage (Penalty Type - STO)"
				},
				new AssertionCost
				{
					ChargeCode = "CCSCFUM",
					E6_OSCostAmount = 50,
					CostCalculationDescription = $@"CCSCFUM: 1 Destination Fumigation @ AUD 50.00/Destination Fumigation (Service ID {consol.Containers[0].Services[0].ES_ServiceId})"
				},
				new AssertionCost
				{
					ChargeCode = "CCSCFUM",
					E6_OSCostAmount = 50m,
					CostCalculationDescription = $@"CCSCFUM: 1 Destination Fumigation @ AUD 50.00/Destination Fumigation (Service ID {consol.Containers[1].Services[0].ES_ServiceId})"
				},
				new AssertionCost
				{
					ChargeCode = "CCSCFUM",
					E6_OSCostAmount = 50m,
					CostCalculationDescription = $@"CCSCFUM: 1 Destination Fumigation @ AUD 50.00/Destination Fumigation (Service ID {consol.Containers[2].Services[0].ES_ServiceId})"
				}
			};

			AutoCostAndAssert("Expected to add service counts and duration from other containers", null, expectedCosts, consol, false);

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = GP20.PK;
			container3.PackLines.Add(shipment1.OuterPackLines.AddNew());

			AutoCostAndAssert("RateLines all have TI_RC specificed so container of different type should be ignored", null, expectedCosts, consol, false);
		}

		[TestDate(2018, 1, 1)]
		public void TestJobServicesAreCalculatedOnlyOnce()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;

			#region Create Charges

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FSC");

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);
			var fumOrgCharge = Helper.ChargeCodes.NewConsolChargeCode("ORGSV", "Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var orgCostEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "", "", "20GP");
			orgCostEntry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var orgCostLine1 = orgCostEntry1.AddRateLine(fumOrgCharge, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			orgCostLine1.GetCalculator<UnitCalculator>().PerUnit = 30m;

			var orgCostEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "NZ", "", "", "20GP");
			orgCostEntry2.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var orgCostLine2 = orgCostEntry2.AddRateLine(fumOrgCharge, UnitCalculator.Code, QuantityUnit.SV, CurrencyCodes.Australia);
			orgCostLine2.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var fclCostEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "20GP");
			fclCostEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var fclCostLine1 = fclCostEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			fclCostLine1.GetCalculator<UnitCalculator>().PerUnit = 32m;

			var fclCostLine2 = fclCostEntry.AddRateLine("FSC", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			fclCostLine2.GetCalculator<UnitCalculator>().PerUnit = 35m;

			Factory.Save();

			#endregion

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001740";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.CreditorPK = creditor.PK;
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			Factory.Save();

			#region Routes Setup

			var transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_CarrierBookingReference = "Route1";
			transport1.JW_VoyageFlight = "123S";
			transport1.JW_ETD = new ZDateTime(2018, 01, 12);
			transport1.JW_ETA = new ZDateTime(2018, 01, 20);
			transport1.JW_TransportType = TransportPlanningType.MainVessel;
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_OA_CarrierAddress = creditor.MainAddress.PK;
			transport1.CreditorPK = creditor.PK;

			var transport2 = consol.Transports.AddNew("NZAKL", "SGSIN");
			transport2.JW_IsLinked = false;
			transport2.JW_CarrierBookingReference = "Route2";
			transport2.JW_VoyageFlight = "123S";
			transport2.JW_ETD = new ZDateTime(2018, 01, 21);
			transport2.JW_ETA = new ZDateTime(2018, 02, 11);
			transport2.JW_TransportType = TransportPlanningType.Other;
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_OA_CarrierAddress = creditor.MainAddress.PK;
			transport2.CreditorPK = creditor.PK;

			#endregion

			#region Containers and Services Setup

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_ContainerNum = "FAKE00008";
			container1.JC_RC = GP20.PK;
			container1.PackLines.Add(shipment1.OuterPackLines.AddNew());
			container1.PackLines.Add(shipment2.OuterPackLines.AddNew());

			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Booked = new ZDateTime(2018, 01, 11);
			service1.ES_Completed = new ZDateTime(2018, 01, 12);
			service1.ES_ServiceCount = 2;
			service1.ES_OH_Contractor = creditor.PK;
			service1.ES_ServiceId = "Norris";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_ContainerNum = "FAKE00007";
			container2.JC_RC = GP20.PK;
			container2.PackLines.Add(shipment1.OuterPackLines.AddNew());
			container2.PackLines.Add(shipment2.OuterPackLines.AddNew());

			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Booked = new ZDateTime(2018, 01, 20);
			service2.ES_Completed = new ZDateTime(2018, 01, 21);
			service2.ES_ServiceCount = 5;
			service2.ES_OH_Contractor = creditor.PK;
			service2.ES_ServiceId = "Piastri";

			#endregion

			var expectedCost = new[]
			{
				new AssertionCost
				{
					ChargeCode = fumOrgCharge.AC_Code,
					E6_OSCostAmount = 60m,
					CostCalculationDescription = "ORGSV: 2 Origin Fumigation @ AUD 30.00/Origin Fumigation (Service ID Norris)"
				},
				new AssertionCost
				{
					ChargeCode = fumOrgCharge.AC_Code,
					E6_OSCostAmount = 150m,
					CostCalculationDescription = "ORGSV: 5 Origin Fumigation @ AUD 30.00/Origin Fumigation (Service ID Piastri)"
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 64m
				},
				new AssertionCost
				{
					ChargeCode = "FSC",
					E6_OSCostAmount = 70m,
					CostCalculationDescription = "FSC: 2 20GP Container(s) @ AUD 35.00/Container"
				}
			};

			var expectedLogLines = new string[]
			{
				"Information: RateLine Filtered FRT-UNT-CN-20GP-Costing CREDITOR	reason:	Only Services are eligible with this adapter",
				"Information: RateLine Filtered FSC-UNT-CN-20GP-Costing CREDITOR	reason:	Only Services are eligible with this adapter",
			};

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("", null, expectedCost, consol, false);
				AssertAutoratingAuditLogNoteContainsLines(consol, "Log should contain expected line", expectedLogLines);
			}
		}

		#endregion

		#endregion

		#region CFS Services

		[TestDate(2014, 6, 24)]
		public void TestAutoRateLoadList()
		{
			var chargeCode = CreateAdHocChargeCode("CFSFMG", "Service", ChargeCodeGroupList.Codes.CFSLoadList, FreightServiceType.Codes.Cleaning);

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.FCL, "AU", "NZ");
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 1200m;

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "LOADLIST1";
			loadList.JK_TransportMode = TransportModes.Sea;
			loadList.JK_ConsolMode = ContainerModes.FCL;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "NZAKL";
			loadList.JK_OH_Forwarder = client.PK;

			var container = loadList.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var service = container.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service.ES_Booked = new ZDateTime(2014, 3, 23);
			service.ES_Completed = new ZDateTime(2014, 3, 23);
			service.ES_ServiceCount = 2;

			var job = new JobHeader.Loader(loadList).TryLoadOrCreate() as Job;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 1200m
				}
			};

			AutorateAndAssert(expected, loadList, client);
		}

		#endregion

		#region ForwardingConsol/CFSLoadListConsol Services

		public void TestServiceProvidedForMultipleContainersOfAForwardingConsol()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var creditor = Helper.CreateCreditor();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = "LCL";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.CreditorPK = creditor.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = GP20.PK;

			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Booked = new ZDateTime(2012, 10, 23);
			service1.ES_Completed = new ZDateTime(2012, 10, 23);
			service1.ES_ServiceCount = 2;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100027";
			container2.JC_RC = GP20.PK;

			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Booked = new ZDateTime(2012, 10, 23);
			service2.ES_Completed = new ZDateTime(2012, 10, 23);
			service2.ES_ServiceCount = 5;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "FAKE4100032";
			container3.JC_RC = GP20.PK;

			var service3 = container3.Services.AddNew();
			service3.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service3.ES_Booked = new ZDateTime(2012, 10, 23);
			service3.ES_Completed = new ZDateTime(2012, 10, 23);
			service3.ES_ServiceCount = 5;
			service3.ES_OH_Contractor = TransportProvider2.PK;

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGSV", "Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var costing = Helper.NewCosting(creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "");
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			foreach (ForwardingContainer container in consol.Containers)
			{
				container.PackLines.Add(shipment1.OuterPackLines.AddNew());
				container.PackLines.Add(shipment2.OuterPackLines.AddNew());
			}

			Factory.Save();

			var expectedCost = new[]
			{
				new AssertionCost
				{
					ChargeCode = chargeCode.AC_Code,
					E6_LocalCostAmount = 0m,
					CostCalculationDescription = "FUM service has been performed on multiple containers by multiple contractors which is not supported."
				}
			};

			AutoCostAndAssert("Multiple contractors invalidate services", null, expectedCost, consol, false);
		}

		public void TestServiceProvidedForMultipleContainersOfALoadList()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CFSFUM", "Service", "", ChargeCodeGroupList.Codes.CFSLoadList, FreightServiceType.Codes.Fumigation);

			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OH_Forwarder = Helper.NewOrgHeader().PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceId = "A";
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 2;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100027";
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerMode = ContainerModes.FCL;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceId = "B";
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 5;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "FAKE4100032";
			container3.JC_RC = GP20.PK;
			container3.JC_ContainerMode = ContainerModes.FCL;
			var service3 = container3.Services.AddNew();
			service3.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service3.ES_Booked = new ZDateTime(2012, 10, 23);
			service3.ES_Completed = new ZDateTime(2012, 10, 23);
			service3.ES_ServiceCount = 5;
			service3.ES_OH_Contractor = TransportProvider2.PK;

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.PAC, RateMode.SEA, "AUSYD", "");
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			Factory.Save();

			using (var job = new JobHeader.Loader(consol).TryLoadOrCreate() as Job)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "DES")).PK;

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = chargeCode.AC_Code,
						JR_OSCostAmt = 0m,
						CostCalculationDescription = "CFSFUM: 0 CFS Load List Charges Fumigation (FUM service has been performed on multiple containers by multiple contractors which is not supported.) @ AUD 1.00/CFS Load List Charges Fumigation"
					}
				};

				AutorateAndAssert(expectedCharges, consol, consol.Forwarder);
			}
		}

		#endregion

		#endregion

		#region Time Calculator

		public void TestJobServicesWithTimeCalculator()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("FUMSV", "Service Charge", TimeCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU");
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.SV);
			rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 2, 200, QuantityUnit.HR);
			rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 2, 150);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = creditor.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			var container = CreateContainerWithServices(consol, GP20, creditor, FreightServiceType.Codes.Fumigation, new TimeSpan(2, 30, 0));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, Consignee.PK, "GBSUN", "AUSYD", 10));
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, Consignee.PK, "GBSUN", "AUSYD", 10));
			container.PackLines.Add(consol.Shipments[0].OuterPackLines.AddNew());
			container.PackLines.Add(consol.Shipments[1].OuterPackLines.AddNew());

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = chargeCode.AC_Code,
					E6_OSCostAmount = 475m,
					CostCalculationDescription = "FUMSV: 2 Destination Fumigation x Hour (1 Destination Fumigation x 2 Hour(s)) @ AUD 200.00/Destination Fumigation x Hour + 0.5 Destination Fumigation x Hour (1 Destination Fumigation x 0.5 Hour(s)) @ AUD 150.00/Destination Fumigation x Hour"
				}
			};

			AutoCostAndAssert("", null, expected, consol, false);
		}

		public void TestJobServicesWIthTimecalculator_OneContainerDaysStorage_20GPCumulative()
		{
			var shipmentOneContainerRateFor20GPWithCumulativeBreak = CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(50, 5, 1, true, true, "AA");
			Factory.Save();

			AutorateAndAssert
			(
				"A shipment with 1 container and a rate with a container specified and the time calculator with cumulative breaks checked; with destination storage",
				new[]
				{
					new AssertionCharge
					{
						JR_LocalSellAmt = 250.00m,
						RevenueCalculationDescription =   "DSTOR: 5 Container x Day (1 20GP (CAA0000000) x 5 Day(s)) @ AUD 50.00/Container x Day"
					}
				},
				shipmentOneContainerRateFor20GPWithCumulativeBreak,
				shipmentOneContainerRateFor20GPWithCumulativeBreak.Consignee,
				autorateCosts: false, autorateRevenue: true
			);
		}

		public void TestJobServicesWIthTimecalculator_OneContainerDaysStorage_Cumulative()
		{
			var shipmentOneContainerWithCumulativeBreak = CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(50, 5, 1, false, true, "AB");
			Factory.Save();
			AutorateAndAssert
			(
				"A shipment with 1 container and a rate without a container specified and the time calculator with cumulative breaks checked; with destination storage",
				new[]
				{
					new AssertionCharge
					{
						JR_LocalSellAmt = 250.00m,
						RevenueCalculationDescription =   "DSTOR: 5 Container x Day (1 Container(s) (CAB0000000) x 5 Day(s)) @ AUD 50.00/Container x Day"
					}
				},
				shipmentOneContainerWithCumulativeBreak,
				shipmentOneContainerWithCumulativeBreak.Consignee,
				autorateCosts: false, autorateRevenue: true
			);
		}

		public void TestJobServicesWIthTimecalculator_OneContainerDaysStorage_20GP()
		{
			var shipmentOneContainerRateFor20GP = CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(50, 5, 1, true, false, "AC");
			Factory.Save();

			AutorateAndAssert
			(
				"A shipment with 1 container and a rate with a container specified and the time calculator with cumulative breaks un-checked; with destination storage",
				new[]
				{
					new AssertionCharge
					{
						JR_LocalSellAmt = 250.00m,
						RevenueCalculationDescription =   "DSTOR: 5 Container x Day (1 20GP (CAC0000000) x 5 Day(s)) @ AUD 50.00/Container x Day"
					}
				},
				shipmentOneContainerRateFor20GP,
				shipmentOneContainerRateFor20GP.Consignee,
				autorateCosts: false, autorateRevenue: true
			);
		}

		public void TestJobServicesWIthTimecalculator_OneContainerDaysStorage()
		{
			var shipmentOneContainer = CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(50, 5, 1, false, false, "AD");
			Factory.Save();

			AutorateAndAssert
			(
				"A shipment with 1 container and a rate without a container specified and the time calculator with cumulative breaks unchecked; with destination storage",
				new[]
				{
					new AssertionCharge
					{
						JR_LocalSellAmt = 250.00m,
						RevenueCalculationDescription =   "DSTOR: 5 Container x Day (1 Container(s) (CAD0000000) x 5 Day(s)) @ AUD 50.00/Container x Day"
					}
				},
				shipmentOneContainer,
				shipmentOneContainer.Consignee,
				autorateCosts: false, autorateRevenue: true
			);
		}

		public void TestJobServicesWIthTimecalculator_TwoContainerDaysStorage()
		{
			var shipment = CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(50, 5, 2, false, false);
			Factory.Save();

			AssertNotNull("Precondition", shipment.Consols[0].Containers[0].DeliveryPenalties.FindContainerPenalty("STO", "CTO"));
			AssertNotNull("Precondition", shipment.Consols[0].Containers[1].DeliveryPenalties.FindContainerPenalty("STO", "CTO"));

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = 500.00m,
					RevenueCalculationDescription =   "DSTOR: 5 Container x Day (1 Container(s) (CAB0000000) x 5 Day(s)) @ AUD 50.00/Container x Day + 5 Container x Day (1 Container(s) (CAB0000001) x 5 Day(s)) @ AUD 50.00/Container x Day"
				}
			};
			var message = "A shipment with 2 container and with destination storage";
			AutorateAndAssert(message, expected, shipment, shipment.Consignee, autorateCosts: false, autorateRevenue: true);
		}

		ForwardingShipment CreateShipmentWithJobServicesWithTimeCalculatorForDestinationStorage(int dollarsPerDayPerContainer, byte daysStorage, int quantityContainers, bool rateWithContainerType, bool isCumulativeBreak, string testCasePrefix = "AB")
		{
			var storageChargeCodePK = RatingDataRegistry.Instance.DestinationStorageServiceChargeCode.Value;
			var storageChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, storageChargeCodePK));
			storageChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			storageChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			var creditor = Helper.CreateCreditor(testCasePrefix + "CREDIT");
			var client = Helper.NewOrgHeader(testCasePrefix + "CLIENT");
			var clientRate = Helper.NewClientRate(client);

			var rateEntry20GP = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", container: rateWithContainerType ? "20GP" : string.Empty);
			rateEntry20GP.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var storage20RateLine = rateEntry20GP.AddRateLine(storageChargeCode, TimeCalculator.Code, QuantityUnit.CN);
			var storage20TimeCalculator = storage20RateLine.GetCalculator<TimeCalculator>();
			storage20TimeCalculator.IsAccumulated = isCumulativeBreak;
			storage20TimeCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 1);
			storage20TimeCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, dollarsPerDayPerContainer, QuantityUnit.DY);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, client.PK, "GBSUN", "AUSYD", 10);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = creditor.PK;
			consol.JK_OA_ArrivalCTOAddress = creditor.MainAddress.PK;
			consol.Shipments.Add(shipment);

			for (int i = 0; i < quantityContainers; i++)
			{
				var container = CreateContainer(GP20, $"C{testCasePrefix}000000{i}");
				consol.Containers.Add(container);
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.SetContainer(consol, container);

				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, container,
					ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO,
					ContainerPenaltyTimeUnit.Codes.Days, 0, daysStorage, 0, currency: "AUD");
			}

			return shipment;
		}

		ShipmentContainerPenalty AttachPenaltyToShipment(
			ShipmentContainerPenaltyCollection penaltyCollection,
			ForwardingShipment forwardingShipment,
			CommonContainer container,
			ZString penaltyType,
			ZString creditorType,
			ZString timeUnit,
			ZByte freeTimeAsDays,
			ZByte duration,
			ZDecimal perUnitCost,
			ZGuid creditorPK = default,
			string currency = "")
		{
			var penalty = penaltyCollection.AddNew();
			penalty.CPY_JS_Shipment = forwardingShipment?.PK ?? ZGuid.Empty;

			if (container != default)
			{
				penalty.CPY_JC_Container = container.PK;
			}

			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_CreditorType = creditorType;

			penalty.CPY_TimeUnit = timeUnit;
			penalty.FreeTimeAsDays = freeTimeAsDays;

			if (timeUnit == ContainerPenaltyTimeUnit.Codes.Hours)
			{
				penalty.CPY_Duration = TimeSpan.FromHours(duration);
			}
			else
			{
				penalty.DurationAsDays = duration;
			}

			penalty.CPY_PerUnitCost = perUnitCost;

			if (creditorPK != default)
			{
				penalty.CPY_OH_Creditor = creditorPK;
			}

			if (!string.IsNullOrEmpty(currency))
			{
				penalty.CPY_RX_NKCurrency = currency;
			}

			return penalty;
		}

		public void TestJobServicessWithTimeCalculator_StorageForMultipleContainersAndTypes()
		{
			var storageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("HIDSV", "Storage Service", TimeCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			// [Not Accumulated] Storage 20GP: 0-4 days free, 5-7 days = $20 per day, >7 days = $30 per day
			{
				var rateEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", container: "20GP");
				rateEntry20GP.TI_RX_NKCurrency = CurrencyCodes.Australia;
				var storage20RateLine = rateEntry20GP.AddRateLine(storageChargeCode, TimeCalculator.Code, QuantityUnit.CN);
				var storage20TimeCalculator = storage20RateLine.GetCalculator<TimeCalculator>();
				storage20TimeCalculator.IsAccumulated = false;
				storage20TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 4, 0, QuantityUnit.DY);
				storage20TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 4, 20);
				storage20TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 7, 30);
			}

			// [Accumulated] Storage 40GP: 0-4 days free, 5-7 days = $25 per day, > days = $35 per day
			{
				var rateEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", container: "40GP");
				rateEntry40GP.TI_RX_NKCurrency = CurrencyCodes.Australia;
				var storage40RateLine = rateEntry40GP.AddRateLine(storageChargeCode, TimeCalculator.Code, QuantityUnit.CN);
				var storage40TimeCalculator = storage40RateLine.GetCalculator<TimeCalculator>();
				storage40TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 4, 0, QuantityUnit.DY);
				storage40TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 4, 25);
				storage40TimeCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 7, 35);
			}

			// [Per Unit Rate] Storage 40HC: $5 per hour
			{
				var rateEntry40HC = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", container: "40HC");
				rateEntry40HC.TI_RX_NKCurrency = CurrencyCodes.Australia;
				var storageLine = rateEntry40HC.AddRateLine(storageChargeCode, TimeCalculator.Code, QuantityUnit.CN);
				var calculator = storageLine.GetCalculator<TimeCalculator>();
				calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 5, QuantityUnit.HR);
			}

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = creditor.PK;
			consol.JK_OA_ArrivalCTOAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Storage:
			//  1 x 20GP for 6 days
			//  1 x 20GP for 11 days
			//  1 x 40GP for 9 days
			//  3 x 40HC for 24 hours each
			var container1 = CreateContainerWithServices(consol, GP20, creditor, FreightServiceType.Codes.Cleaning, new TimeSpan(2, 0, 0), storageDays: 6, serviceCount: 0);
			container1.JC_ContainerNum = "CON1111111";

			var container2 = CreateContainerWithServices(consol, GP20, creditor, FreightServiceType.Codes.Cleaning, new TimeSpan(4, 0, 0), storageDays: 11, serviceCount: 0);
			container2.JC_ContainerNum = "CON2222222";

			var container3 = CreateContainerWithServices(consol, GP40, creditor, FreightServiceType.Codes.Cleaning, new TimeSpan(6, 0, 0), storageDays: 9, serviceCount: 0);
			container3.JC_ContainerNum = "CON3333333";

			var container4 = CreateContainerWithServices(consol, Helper.Containers["40HC"], creditor, FreightServiceType.Codes.Cleaning, new TimeSpan(8, 0, 0), storageDays: 1, serviceCount: 0);
			container4.JC_ContainerCount = 3;

			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, Consignee.PK, "GBSUN", "AUSYD", 10));

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = storageChargeCode.AC_Code,
					E6_OSCostAmount = 6 * 20m + 11 * 30m,
					CostCalculationDescription = "HIDSV: 11 Container x Day (1 20GP (CON2222222) x 11 Day(s)) @ AUD 30.00/Container x Day + 6 Container x Day (1 20GP (CON1111111) x 6 Day(s)) @ AUD 20.00/Container x Day"
				},
				new AssertionCost
				{
					ChargeCode = storageChargeCode.AC_Code,
					E6_OSCostAmount = (3 * 25m + 2 * 35m),
					CostCalculationDescription = "HIDSV: 2 Container x Day (1 40GP (CON3333333) x 2 Day(s)) @ AUD 35.00/Container x Day + 3 Container x Day (1 40GP (CON3333333) x 3 Day(s)) @ AUD 25.00/Container x Day"
				},
				new AssertionCost
				{
					ChargeCode = storageChargeCode.AC_Code,
					E6_OSCostAmount = 3 * 24 * 5m,
					CostCalculationDescription = "HIDSV: 72 Container x Hour (3 40HC x 24 Hour(s)) @ AUD 5.00/Container x Hour"
				}
			};

			var message = "The storage duration should be applied per container";
			AutoCostAndAssert(message, null, expected, consol, false);
		}

		public void TestJobServicesWithTimeCalculator_ShipmentStorageByWeight()
		{
			var storageChargeCode = Helper.ChargeCodes.New("HIDSV", "Storage Service", TimeCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			// Storage: $10 per day per 100KG
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AUSYD");
			{
				rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
				var line = rateEntry.AddRateLine(storageChargeCode, TimeCalculator.Code, QuantityUnit.KG);
				line.TL_WeightVolumeMultiple = 100;
				var calc = line.GetCalculator<TimeCalculator>();
				calc.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 10, QuantityUnit.DY);
			}

			var shipment = CreateForwardingShipment(TransportModes.Air, creditor.PK, ZGuid.Empty, "GBSUN", "AUSYD", 8000);
			var docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.DeliveryCartageCoPK = creditor.PK;
			docsAndCartage.JP_LCLAirStorageDaysOrHours = new ZByte(96);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "HIDSV",
						JR_OSCostAmt = 80m * 4 * 10m,
						CostCalculationDescription = "HIDSV: 32000 100 KG x Day (8000 Kilogram(s) x 4 Day(s)) @ AUD 10.00/100 KG x Day"
					},
			};

			AutorateAndAssert(expected, shipment, creditor, autorateRevenue: false);
		}

		#endregion

		#region Container/Containerless Services

		public void TestEachContainerServiceHasItsOwnRate()
		{
			var creditor = Helper.CreateCreditor();
			var contractor = Helper.CreateCreditor("CONTRACTOR");
			contractor.OH_IsShippingProvider = true;
			contractor.OH_IsShippingLine = true;
			contractor.OH_IsMiscFreightServices = true;
			contractor.OH_IsFumigationContractor = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var mainAddressPK = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = mainAddressPK;
			consol.JK_OA_ShippingLineAddress = mainAddressPK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("OFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			chargeCode.AC_IsAdhocServiceCharge = true;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerNum = "CON1111111";
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;
			service1.ES_Duration = new TimeSpan(3, 0, 0, 0);
			service1.ES_References = "REF1111111";
			service1.ES_Completed = ZDateTime.Now;
			service1.ES_ServiceRate = 150m;
			service1.ES_OH_Contractor = contractor.PK;
			service1.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Day;
			service1.ES_OA_Location = contractor.MainAddress.PK;
			service1.ES_RX_NKServiceRateCurrency = "AUD";

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerNum = "CON2222222";
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCount = 1;
			service2.ES_Duration = new TimeSpan(1, 0, 0, 0);
			service2.ES_References = "REF2222222";
			service2.ES_Completed = ZDateTime.Now;
			service2.ES_ServiceRate = 300m;
			service2.ES_OH_Contractor = contractor.PK;
			service2.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Day;
			service2.ES_OA_Location = contractor.MainAddress.PK;
			service2.ES_RX_NKServiceRateCurrency = "AUD";

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = GP40.PK;
			container3.JC_ContainerNum = "CON3333333";
			var service3 = container3.Services.AddNew();
			service3.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service3.ES_ServiceCount = 2;
			service3.ES_Duration = new TimeSpan(1, 0, 0, 0);
			service3.ES_References = "REF3333333";
			service3.ES_Completed = ZDateTime.Now;
			service3.ES_ServiceRate = 120m;
			service3.ES_OH_Contractor = contractor.PK;
			service3.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.ServiceOccurrence;
			service3.ES_OA_Location = contractor.MainAddress.PK;
			service3.ES_RX_NKServiceRateCurrency = "USD";

			AddParityExchangeRate(service3.ServiceRateCurrency);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			packline1.JL_ActualVolume = 1m;
			packline1.JL_ActualWeight = 1000m;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container2.PK;
			packline2.JL_ActualVolume = 1m;
			packline2.JL_ActualWeight = 1000m;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_JC = container3.PK;
			packline3.JL_ActualVolume = 1m;
			packline3.JL_ActualWeight = 1000m;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 450m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 3 Day(s) @ AUD 150.00/Day",
				},
				new AssertionCost
				{
					E6_OSCostAmount = 300m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 1 Day(s) @ AUD 300.00/Day",
				},
				new AssertionCost
				{
					E6_OSCostAmount = 240m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 2 Origin Fumigation @ USD 120.00/Origin Fumigation",
				}
			};

			AutoCostAndAssert("AutoRating on consol should produce charges", null, expectedCharges, consol, false);

			var paymentBases = (shipment.Job as Job).Charges.Cast<Charge>().SelectMany(x => x.CostPaymentBases).ToList();
			CombineAssertions("Chargeable descriptions and amounts", () =>
			{
				AssertEquals("Container 1 REF1111111", 450m, paymentBases.Single(x => x.PBS_ChargeableDescription == "CON1111111 REF1111111")?.Amount);
				AssertEquals("Container 2 REF2222222", 300m, paymentBases.Single(x => x.PBS_ChargeableDescription == "CON2222222 REF2222222")?.Amount);
				AssertEquals("Container 3 REF3333333", 240m, paymentBases.Single(x => x.PBS_ChargeableDescription == "REF3333333")?.Amount);
			});
		}

		public void TestContainerServiceHasItsOwnRate_ServiceUnitCN()
		{
			var creditor = Helper.CreateCreditor();
			var contractor = Helper.CreateCreditor("CONTRACTOR");
			contractor.OH_IsShippingProvider = true;
			contractor.OH_IsShippingLine = true;
			contractor.OH_IsMiscFreightServices = true;
			contractor.OH_IsFumigationContractor = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var mainAddressPK = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = mainAddressPK;
			consol.JK_OA_ShippingLineAddress = mainAddressPK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("OFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			chargeCode.AC_IsAdhocServiceCharge = true;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 3;
			var service1 = container1.Services.AddNew();
			service1.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;
			service1.ES_Duration = new TimeSpan(1, 0, 0, 0);
			service1.ES_References = "REF1111111";
			service1.ES_Completed = ZDateTime.Now;
			service1.ES_ServiceRate = 200m;
			service1.ES_OH_Contractor = contractor.PK;
			service1.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Container;
			service1.ES_OA_Location = contractor.MainAddress.PK;
			service1.ES_RX_NKServiceRateCurrency = "AUD";

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP40.PK;
			container2.JC_ContainerCount = 2;
			var service2 = container2.Services.AddNew();
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCount = 1;
			service2.ES_Duration = new TimeSpan(1, 0, 0, 0);
			service2.ES_References = "REF2222222";
			service2.ES_Completed = ZDateTime.Now;
			service2.ES_ServiceRate = 400m;
			service2.ES_OH_Contractor = contractor.PK;
			service2.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Container;
			service2.ES_OA_Location = contractor.MainAddress.PK;
			service2.ES_RX_NKServiceRateCurrency = "AUD";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			packline1.JL_ActualVolume = 1m;
			packline1.JL_ActualWeight = 1000m;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container2.PK;
			packline2.JL_ActualVolume = 1m;
			packline2.JL_ActualWeight = 1000m;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 600m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 3 20GP Container(s) @ AUD 200.00/Container",
				},
				new AssertionCost
				{
					E6_OSCostAmount = 800m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 2 40GP Container(s) @ AUD 400.00/Container",
				},
			};

			AutoCostAndAssert("AutoRating on consol should produce charges", null, expectedCharges, consol, false);
			var paymentBases = (shipment.Job as Job).Charges.Cast<Charge>().SelectMany(x => x.CostPaymentBases).ToList();
			CombineAssertions("Chargeable descriptions and amounts", () =>
			{
				AssertEquals("Container 1 REF1111111", 600m, paymentBases.Single(x => x.PBS_ChargeableDescription == "REF1111111")?.Amount);
				AssertEquals("Container 2 REF2222222", 800m, paymentBases.Single(x => x.PBS_ChargeableDescription == "REF2222222")?.Amount);
			});
		}

		public void TestContainerHasBothServiceWithUnitCN_AndFreightSpotRate_CreateDifferentEntries()
		{
			var creditor = Helper.CreateCreditor();
			var contractor = Helper.CreateCreditor("CONTRACTOR");
			contractor.OH_IsShippingProvider = true;
			contractor.OH_IsShippingLine = true;
			contractor.OH_IsMiscFreightServices = true;
			contractor.OH_IsFumigationContractor = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var mainAddressPK = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = mainAddressPK;
			consol.JK_OA_ShippingLineAddress = mainAddressPK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("OFUM", "Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			chargeCode.AC_IsAdhocServiceCharge = true;

			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "FRT20GP";
			container.JC_CostSpotRate = 500m;
			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			container.JC_RX_NKCostSpotRateCurrency = "USD";

			var service = container.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service.ES_ServiceCount = 1;
			service.ES_Duration = new TimeSpan(1, 0, 0, 0);
			service.ES_References = "REF1111111";
			service.ES_Completed = ZDateTime.Now;
			service.ES_ServiceRate = 200m;
			service.ES_OH_Contractor = contractor.PK;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Container;
			service.ES_OA_Location = contractor.MainAddress.PK;
			service.ES_RX_NKServiceRateCurrency = "USD";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.JL_ActualVolume = 1m;
			packLine.JL_ActualWeight = 1000m;

			RefCurrency usd = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			AddParityExchangeRate(usd);

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 500m,
					ChargeCode = "FRT",
					CostCalculationDescription = "FRT: 1 20GP Container(s) @ USD 500.00/Container",
				},
				new AssertionCost
				{
					E6_OSCostAmount = 200m,
					ChargeCode = "OFUM",
					CostCalculationDescription = "OFUM: 1 20GP Container(s) @ USD 200.00/Container",
				},
			};

			AutoCostAndAssert("AutoRating on consol should produce charges", null, expectedCharges, consol, false);
			var paymentBases = (shipment.Job as Job).Charges.Cast<Charge>().SelectMany(x => x.CostPaymentBases).ToList();

			var actual = paymentBases
				.Select(x => new
				{
					PBS_ChargeableDescription = x.PBS_ChargeableDescription,
					Amount = x.Amount
				})
				.Select(x => $"{x.PBS_ChargeableDescription}|{x.Amount}")
				.ToArray();

			var expected = new[]
			{
				"REF1111111|200",
				"FRT20GP|500",
			};

			AssertContainsExactElementsInAnyOrder(
				"AutoRating charges should match payment bases",
				expected,
				actual
			);
		}

		public void TestReceipt_ServicesIsolateFromContainers_AutorateWithContainerRates()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "WAQIS";
			chargeCode.AC_Desc = "Aqis Inspection";
			chargeCode.AC_ChargeGroup = "WIN";
			chargeCode.AC_ChargeType = ChargeType.Revenue;
			chargeCode.AC_ChargeSubGroup = "QIN";
			chargeCode.AC_RateCalculator = UnitCalculator.Code;

			var localClient = Helper.NewOrgHeader();
			var warehouseOrg = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(localClient);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, ZString.Empty, ZString.Empty, ZString.Empty, "20GP");
			entry1.RateLines.RemoveAndDeleteAll();
			var line1 = entry1.AddRateLine("WAQIS", UnitCalculator.Code, "SV");
			line1.GetCalculator<UnitCalculator>().PerUnit = 100.0m;

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, ZString.Empty, ZString.Empty, ZString.Empty, "40GP");
			entry2.RateLines.RemoveAndDeleteAll();
			var line2 = entry2.AddRateLine("WAQIS", UnitCalculator.Code, "SV");
			line2.GetCalculator<UnitCalculator>().PerUnit = 150.0m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouse = newFactory.New<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "AAA";
			warehouse.WW_WarehouseName = "AAA";
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = warehouseOrg.Addresses.MainAddress.PK;
			warehouse.WW_WarehouseType = Warehouse.Integration.CodeLists.WarehouseTypes.Codes.Product;

			var receipt = newFactory.New<WhsReceive>();
			receipt.WD_OH_Client = localClient.PK;
			receipt.WD_WW_Whs = warehouse.PK;
			receipt.WD_DocketSubType = ReceiveType.Codes.Receipt;

			// 2 containers of same type 20GP
			var container1 = receipt.Containers.AddNew();
			container1.WC_ContainerNum = "CONTAINER20GP1";
			container1.WC_RC = GP20.PK;
			var container2 = receipt.Containers.AddNew();
			container2.WC_ContainerNum = "CONTAINER20GP2";
			container2.WC_RC = GP20.PK;

			var service = receipt.Services.AddNew();
			service.ES_ServiceCode = "QIN";
			service.ES_ServiceCount = 6.0m;
			service.ES_Completed = ZDate.Today;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 600.0m
				},
			};

			var receiptJob = new Job.Loader(receipt).TryLoadOrCreate();
			AutorateAndAssert(expectedCharges, receipt, localClient, autorateCosts: false, job: receiptJob);

			// 2 containers of same type 40GP
			receipt.Containers.RemoveAllFromRelationship();
			var container3 = receipt.Containers.AddNew();
			container3.WC_ContainerNum = "CONTAINER40GP1";
			container3.WC_RC = GP40.PK;
			var container4 = receipt.Containers.AddNew();
			container4.WC_ContainerNum = "CONTAINER40GP2";
			container4.WC_RC = GP40.PK;

			receiptJob.Charges.RemoveAndDeleteAll();
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 900.0m
				},
			};
			AutorateAndAssert(expectedCharges, receipt, localClient, autorateCosts: false, job: receiptJob);

			// mixed container types 40GP and 20GP
			var container5 = receipt.Containers.AddNew();
			container5.WC_ContainerNum = "CONTAINER20GP3";
			container5.WC_RC = GP20.PK;

			receiptJob.Charges.RemoveAndDeleteAll();
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 600.0m
				},
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 900.0m
				},
			};
			AutorateAndAssert(expectedCharges, receipt, localClient, autorateCosts: false, job: receiptJob);

			var entry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL); // a super general entry
			entry3.RateLines.RemoveAndDeleteAll();
			var line3 = entry3.AddRateLine("WAQIS", UnitCalculator.Code, "SV");
			line3.GetCalculator<UnitCalculator>().PerUnit = 200.0m;
			Factory.Save();

			receiptJob.Charges.RemoveAndDeleteAll();
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 600.0m
				},
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSSellAmt = 900.0m
				},
			};
			AutorateAndAssert(expectedCharges, receipt, localClient, autorateCosts: false, job: receiptJob);
		}

		/// <summary>
		///		It is a special test for some ugly case specific to some warehouse jobs like ItemReceiveConsignment.
		///		They provide a measure with Unidentified measure part for some their purposes, but this Unidentified
		///		measure type was (may) causing some turbulence for autorating logic which filters lines and decides which line
		///		calculates which part or service.
		/// </summary>
		public void TestItemReceiveConsignment_ServiceOccurrence()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "WAQIS";
			chargeCode.AC_Desc = "Aqis Inspection";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TRWReceive;
			chargeCode.AC_ChargeType = ChargeType.NonAccrual;
			chargeCode.AC_ChargeSubGroup = "QIN";
			chargeCode.AC_RateCalculator = UnitCalculator.Code;

			var contractor1 = Helper.NewOrgHeader();
			var contractor2 = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();

			var costing1 = Helper.NewCosting(contractor1);
			var entry1 = costing1.AddRateEntry(RatingConstants.RateCategory.TRW, RateMode.ALL, ZString.Empty, ZString.Empty, ZString.Empty);
			entry1.AddPerUnitCharge("WAQIS", 100, "SV");

			var costing2 = Helper.NewCosting(contractor2);
			var entry2 = costing2.AddRateEntry(RatingConstants.RateCategory.TRW, RateMode.ALL, ZString.Empty, ZString.Empty, ZString.Empty);
			entry2.AddPerUnitCharge("WAQIS", 200, "SV");

			Factory.Save();

			var warehouseOrg = Helper.NewOrgHeader();

			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "AAA";
			warehouse.WW_WarehouseName = "AAA";
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = warehouseOrg.Addresses.MainAddress.PK;
			warehouse.WW_WarehouseType = Warehouse.Integration.CodeLists.WarehouseTypes.Codes.Transit;

			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;

			var service1 = consignment.Services.AddNew();
			service1.ES_ServiceCode = "QIN";
			service1.ES_Completed = ZDate.Today;
			service1.ES_OH_Contractor = contractor1.PK;
			service1.ES_ServiceCount = 2;
			service1.ES_ServiceId = "Service1";

			var service2 = consignment.Services.AddNew();
			service2.ES_ServiceCode = "QIN";
			service2.ES_Completed = ZDate.Today;
			service2.ES_OH_Contractor = contractor2.PK;
			service2.ES_ServiceCount = 5;
			service2.ES_ServiceId = "Service2";

			var service3 = consignment.Services.AddNew();
			service3.ES_ServiceCode = "QIN";
			service3.ES_Completed = ZDate.Today;
			service3.ES_OH_Contractor = contractor2.PK;
			service3.ES_ServiceCount = 8;
			service3.ES_ServiceId = "Service3";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSCostAmt = 200.0m,
					CostCalculationDescription = "WAQIS: 2 Transit Receive Quarantine Inspection @ AUD 100.00/Transit Receive Quarantine Inspection (Service ID Service1)"
				},
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSCostAmt = 1000.0m,
					CostCalculationDescription = "WAQIS: 5 Transit Receive Quarantine Inspection @ AUD 200.00/Transit Receive Quarantine Inspection (Service ID Service2)"
				},
				new AssertionCharge
				{
					ChargeCode = "WAQIS",
					JR_OSCostAmt = 1600.0m,
					CostCalculationDescription = "WAQIS: 8 Transit Receive Quarantine Inspection @ AUD 200.00/Transit Receive Quarantine Inspection (Service ID Service3)"
				},
			};

			var receiptJob = new Job.Loader(consignment).TryLoadOrCreate();
			receiptJob.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			AutorateAndAssert(expectedCharges, consignment, localClient, autorateRevenue: false, job: receiptJob);
		}

		/**
		 * Summary: This test is to ensure the functional test case in [WI00226833 Functional Review.xlsm].
		 * With an FCL shipment, containerless services should autorate with container specific rates.
		 */
		public void TestForwardingShipment_ServicesIsolateFromContainers_AutorateWithContainerRates()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "HKHKG", ZString.Empty, "20GP");
			entry1.AddRateLine("OFUMI", UnitCalculator.Code, "SV", "AUD").GetCalculator<UnitCalculator>().PerUnit = 5.5m;

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "HKHKG", ZString.Empty, "40GP");
			entry2.AddRateLine("OFUMI", UnitCalculator.Code, "SV", "AUD").GetCalculator<UnitCalculator>().PerUnit = 7.5m;

			var entry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "HKHKG");
			entry3.AddRateLine("OFUMI", UnitCalculator.Code, "SV", "AUD").GetCalculator<UnitCalculator>().PerUnit = 10.5m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "HKHKG", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;

			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);

			var service = shipment.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDate.Today;
			service.ES_ServiceCount = 10;

			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_RC = GP40.PK;
			container2.JC_ContainerCount = 1;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OFUMI",
					JR_OSSellAmt = 55m // => 10SV * $5.5/SV 20GP
				},
				new AssertionCharge
				{
					ChargeCode = "OFUMI",
					JR_OSSellAmt = 75m // => 10SV * $7.5/SV 40GP
				},
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);
		}

		#endregion

		public void TestRevenueRatesLoadedAtServiceLocation()
		{
			var serviceProvider = TransportProvider1;
			serviceProvider.MainAddress.OA_RL_NKRelatedPortCode = "ATVIE";

			var clientRate = Helper.NewClientRate(Consignor);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "ROBUH", "HKHKG");
			entry1.AddRateLine("OFUMI", UnitCalculator.Code, "SV", "AUD").GetCalculator<UnitCalculator>().PerUnit = 5.5m;

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "ATVIE", "HKHKG");
			entry2.AddRateLine("OFUMI", UnitCalculator.Code, "SV", "AUD").GetCalculator<UnitCalculator>().PerUnit = 7.5m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "ROBUH", "HKHKG", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);

			var service = shipment.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDate.Today;
			service.ES_ServiceCount = 10;
			service.ES_OA_Location = serviceProvider.MainAddress.PK;

			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OFUMI",
					JR_OSSellAmt = 75m // => 10SV * $7.5/SV 20GP
				}
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);
		}

		public void TestAutoratingServices_CompanyTariffCalculator()
		{
			Helper.ChargeCodes.New("ODTN", "Origin Detention Charge Code", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention);
			Helper.ChargeCodes.New("OTWT", "Origin Truck Wait Charge Code", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal);

			var serviceProvider = TransportProvider1;
			serviceProvider.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var localClient = NewClient;
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			Consignor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);

			var service = shipment.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDate.Today;
			service.ES_ServiceCount = 9;
			service.ES_OA_Location = serviceProvider.MainAddress.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_DeparturePackCFSTransportAddress = serviceProvider.MainAddress.PK;
			consol.JK_OA_CreditorAddress = serviceProvider.MainAddress.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;
			container1.PackLines.Add(shipment.OuterPackLines.AddNew());

			var originDetentionPenalty = container1.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_RC = GP40.PK;
			container2.JC_ContainerCount = 1;
			container2.PackLines.Add(shipment.OuterPackLines.AddNew());

			var originTruckWaitPenalty = container2.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
			originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(7);

			Factory.Save();

			var companyTariff1 = Helper.NewCompanyTariff();
			companyTariff1.TH_GlobalRateLevel = 1;
			var companyTariff2 = Helper.NewCompanyTariff();
			companyTariff2.TH_GlobalRateLevel = 2;

			var entry1 = companyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, CountryCodes.Australia);
			entry1.RateLines.RemoveAndDeleteAll();
			CreatePerServiceTariffLineWithOverride(entry1, "OFUMI", QuantityUnit.SV, 10m, +1m);

			var entry20GP = companyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, CountryCodes.Australia, container: "20GP");
			entry20GP.RateLines.RemoveAndDeleteAll();
			CreatePerServiceTariffLineWithOverride(entry20GP, "ODTN", QuantityUnit.DY, 20m, +2m);
			CreatePerServiceTariffLineWithOverride(entry20GP, "OTWT", QuantityUnit.HR, 23m, +3m);

			var entry40GP = companyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, CountryCodes.Australia, container: "40GP");
			entry40GP.RateLines.RemoveAndDeleteAll();
			CreatePerServiceTariffLineWithOverride(entry40GP, "ODTN", QuantityUnit.DY, 40m, +4m);
			CreatePerServiceTariffLineWithOverride(entry40GP, "OTWT", QuantityUnit.HR, 43m, +5m);

			companyTariff1.Factory.Save();
			companyTariff2.Factory.Save();

			// For tariff level Consignor as local client with tariff
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OFUMI",
					JR_OSSellAmt = 9 * 10m
				},
				new AssertionCharge
				{
					ChargeCode = "ODTN",
					JR_OSSellAmt = 3 * 20m
				},
				new AssertionCharge
				{
					ChargeCode = "OTWT",
					JR_OSSellAmt = 7 * 43m
				}
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedOverrideCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OFUMI",
					JR_OSSellAmt = 9 * (10m + 1m)
				},
				new AssertionCharge
				{
					ChargeCode = "ODTN",
					JR_OSSellAmt = 3 * (20m + 2m)
				},
				new AssertionCharge
				{
					ChargeCode = "OTWT",
					JR_OSSellAmt = 7 * (43m + 5m)
				}
			};
			AutorateAndAssert(expectedOverrideCharges, shipment, localClient, autorateCosts: false);
		}

		public void TestAutoratingServices_CompanyTariffCalculator_MultipleLinesWithSameBaseTariff()
		{
			Helper.ChargeCodes.New("QIN", "Inspection", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, "QIN");

			var serviceProvider = TransportProvider1;
			serviceProvider.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var localClient = NewClient;
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "CNSHG", "AUSYD", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var service = shipment.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.QuarantineInspection;
			service.ES_Completed = ZDate.Today;
			service.ES_ServiceCount = 2;
			service.ES_OA_Location = serviceProvider.MainAddress.PK;

			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;
			container1.PackLines.Add(shipment.OuterPackLines.AddNew());

			var clientRate1 = Helper.NewClientRate(localClient);

			var clientEntry1 = clientRate1.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", CountryCodes.Australia);
			// Create two rate lines with same charge code and CompanyTariffBased calculator so they refer to the same base tariff line.
			clientEntry1.AddRateLine("QIN", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.SV, "AUD")
				.GetCalculator<CompanyTariffOrCostBasedCalculator>()
				.PerUnit = 1;
			clientEntry1.AddRateLine("QIN", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.SV, "AUD")
				.GetCalculator<CompanyTariffOrCostBasedCalculator>()
				.PerUnit = 2;

			Factory.Save();

			var companyTariff1 = Helper.NewCompanyTariff();
			companyTariff1.TH_GlobalRateLevel = 1;

			var tariffEntry1 = companyTariff1.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", CountryCodes.Australia);
			tariffEntry1.RateLines.RemoveAndDeleteAll();
			CreatePerServiceTariffLineWithOverride(tariffEntry1, "QIN", QuantityUnit.SV, 10m, +1m, numOverrides: 1);

			companyTariff1.Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "QIN",
					JR_OSSellAmt = 2 * (10m + 1m + 10m + 2m)
				}
			};
			AutorateAndAssert(expectedCharges, shipment, localClient, Consignor, autorateCosts: false);
		}

		[TestDate(2021, 9, 1)]
		public void TestAutoratingServices_OneOffQuote()
		{
			var chargeCode = Helper.ChargeCodes.New("DDDD", "Service charge", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var clientRate = Helper.NewClientRate(Consignee);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.All, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine(chargeCode, CombinedCalculator.Code, lineUnit: "KG");
			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0, 20000, 0);
			calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 50, 0);
			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, .25, 0);

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", ZString.Empty, Consignee, Consignor, Consignee, null, "AUSYD", "USLAX", 87m, .315m, QuotedBookingState.QuoteOnly);
			oneOffQuote.StartDate = ZDate.Today;
			oneOffQuote.EndDate = ZDate.Today;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = 50,
					RevenueCalculationDescription = "DDDD: Minimum AUD 50.00"
				}
			};
			AutorateAndAssert(expectedCharges, oneOffQuote, Consignee, autorateCosts: false);

			AssertEquals("there should not be any error report", 0, ExceptionReporterTestListener.Instance.Count);

			var notExpectedNote = "Information: RateLine Filtered DDDD-CMB-SV-Client Rate CONSIGNEE1	reason:	FUM Service was not present";
			AssertAutoratingAuditLogNotContains(oneOffQuote, notExpectedNote, "should NOT have line filtered message");
		}

		public void TestAutoratingByServiceCount_DuplicateServiceType()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var contractor1 = Helper.NewOrgHeader();
			var costingRate1 = Helper.NewCosting(contractor1);
			costingRate1.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 100m, "SV", "AUD");

			var contractor2 = Helper.NewOrgHeader();
			var costingRate2 = Helper.NewCosting(contractor2);
			costingRate2.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 50m, "SV", "AUD");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_OH_Contractor = contractor1.PK;
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today;
			serviceA.ES_ServiceCount = 1;
			serviceA.ES_OA_Location = contractor1.MainAddress.PK;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_OH_Contractor = contractor1.PK;
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today;
			serviceB.ES_ServiceCount = 2;
			serviceB.ES_OA_Location = contractor1.MainAddress.PK;

			var serviceC = shipment.Services.AddNew();
			serviceC.ES_ServiceId = "ServiceC";
			serviceC.ES_OH_Contractor = contractor2.PK;
			serviceC.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceC.ES_Completed = ZDate.Today;
			serviceC.ES_ServiceCount = 3;
			serviceC.ES_OA_Location = contractor1.MainAddress.PK;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 200,
					CostCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 100.00/Origin Fumigation (Service ID ServiceA)",
					RevenueCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceA)",
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 200,
					JR_LocalSellAmt = 400,
					CostCalculationDescription = "OFUMI: 2 Origin Fumigation @ AUD 100.00/Origin Fumigation (Service ID ServiceB)",
					RevenueCalculationDescription = "OFUMI: 2 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceB)",
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 150,
					JR_LocalSellAmt = 600,
					CostCalculationDescription = "OFUMI: 3 Origin Fumigation @ AUD 50.00/Origin Fumigation (Service ID ServiceC)",
					RevenueCalculationDescription = "OFUMI: 3 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceC)",
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestAutoratingServices_DuplicateServiceType_AdhocOneServiceRate()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = true;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var contractor = Helper.NewOrgHeader();
			var costingRate = Helper.NewCosting(contractor);
			var rateEntry1 = costingRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "USLAX", "OFUMI", 100m, "SV", "AUD");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var rateEntry2 = costingRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 125m, "SV", "AUD");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_References = "A";
			serviceA.ES_OH_Contractor = contractor.PK;
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today.AddDays(-2);
			serviceA.ES_ServiceCount = 1;
			serviceA.ES_ServiceRate = 150m;
			serviceA.ES_MeasurementBasis = "SV";
			serviceA.ES_RX_NKServiceRateCurrency = "AUD";
			serviceA.ES_OA_Location = contractor.MainAddress.PK;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_References = "B";
			serviceB.ES_OH_Contractor = contractor.PK;
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today.AddDays(-2);
			serviceB.ES_ServiceCount = 2;
			serviceB.ES_ServiceRate = 0;
			serviceB.ES_OA_Location = contractor.MainAddress.PK;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = 150,
					JR_LocalSellAmt = 200,
					CostCalculationDescription = "1 Origin Fumigation @ AUD 150.00/Origin Fumigation (Service ID ServiceA)",
					RevenueCalculationDescription = "1 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceA)"
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 200,
					JR_LocalSellAmt = 400,
					CostCalculationDescription = "2 Origin Fumigation @ AUD 100.00/Origin Fumigation (Service ID ServiceB)",
					RevenueCalculationDescription = "2 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceB)"
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestAutoratingServices_DuplicateServiceType_AdhocWithSameServiceRate()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = true;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var contractor = Helper.NewOrgHeader();

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_References = "A";
			serviceA.ES_OH_Contractor = contractor.PK;
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today.AddDays(-2);
			serviceA.ES_ServiceCount = 1;
			serviceA.ES_ServiceRate = 150m;
			serviceA.ES_MeasurementBasis = "SV";
			serviceA.ES_RX_NKServiceRateCurrency = "AUD";
			serviceA.ES_OA_Location = contractor.MainAddress.PK;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_References = "B";
			serviceB.ES_OH_Contractor = contractor.PK;
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today.AddDays(-1);
			serviceB.ES_ServiceCount = 1;
			serviceB.ES_ServiceRate = 150m;
			serviceB.ES_MeasurementBasis = "SV";
			serviceB.ES_RX_NKServiceRateCurrency = "AUD";
			serviceB.ES_OA_Location = contractor.MainAddress.PK;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = 150,
					CostCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 150.00/Origin Fumigation (Service ID ServiceA)",
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 150,
					CostCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 150.00/Origin Fumigation (Service ID ServiceB)",
				},
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestAutoratingServices_DuplicateServiceType_AdhocWithDifferingServiceRate()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = true;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var contractor = Helper.NewOrgHeader();

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_References = "A";
			serviceA.ES_OH_Contractor = contractor.PK;
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today.AddDays(-2);
			serviceA.ES_ServiceCount = 1;
			serviceA.ES_ServiceRate = 150m;
			serviceA.ES_MeasurementBasis = "SV";
			serviceA.ES_RX_NKServiceRateCurrency = "AUD";
			serviceA.ES_OA_Location = contractor.MainAddress.PK;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_References = "B";
			serviceB.ES_OH_Contractor = contractor.PK;
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today.AddDays(-1);
			serviceB.ES_ServiceCount = 1;
			serviceB.ES_ServiceRate = 123m;
			serviceB.ES_MeasurementBasis = "SV";
			serviceB.ES_RX_NKServiceRateCurrency = "AUD";
			serviceB.ES_OA_Location = contractor.MainAddress.PK;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = 150,
					CostCalculationDescription = @"1 Origin Fumigation @ AUD 150.00/Origin Fumigation (Service ID ServiceA)",
				},
				new AssertionCharge
				{
					JR_LocalCostAmt = 123,
					CostCalculationDescription = @"1 Origin Fumigation @ AUD 123.00/Origin Fumigation (Service ID ServiceB)",
				},
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestAutoratingServices_DuplicateServiceType_AdhocWithNoContractor()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = true;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_References = "A";
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today.AddDays(-2);
			serviceA.ES_ServiceCount = 1;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_References = "B";
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today.AddDays(-1);
			serviceB.ES_ServiceCount = 1;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					RevenueCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceA)",
				},
				new AssertionCharge
				{
					RevenueCalculationDescription = "OFUMI: 1 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceB)",
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestAutoratingServices_DuplicateServiceType_DifferentContractors()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = false;
			chargeCode.Factory.Save();

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var contractorA = Helper.NewOrgHeader();
			var costingRateA = Helper.NewCosting(contractorA);
			costingRateA.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 100m, "SV", "AUD");

			var contractorB = Helper.NewOrgHeader();
			var costingRateB = Helper.NewCosting(contractorB);
			costingRateB.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "AUSYD", "", "OFUMI", 200m, "SV", "AUD");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 10);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_E_DEP = ZDate.Today;
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;

			var serviceA = shipment.Services.AddNew();
			serviceA.ES_ServiceId = "ServiceA";
			serviceA.ES_OH_Contractor = contractorA.PK;
			serviceA.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceA.ES_Completed = ZDate.Today.AddDays(-2);
			serviceA.ES_ServiceCount = 1;
			serviceA.ES_OA_Location = contractorA.MainAddress.PK;

			var serviceB = shipment.Services.AddNew();
			serviceB.ES_ServiceId = "ServiceB";
			serviceB.ES_OH_Contractor = contractorB.PK;
			serviceB.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			serviceB.ES_Completed = ZDate.Today.AddDays(-2);
			serviceB.ES_ServiceCount = 1;
			serviceB.ES_OA_Location = contractorB.MainAddress.PK;

			shipment.Consols.AddNew();

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					CostCalculationDescription = @"1 Origin Fumigation @ AUD 100.00/Origin Fumigation (Service ID ServiceA)",
				},
				new AssertionCharge
				{
					CostCalculationDescription = @"1 Origin Fumigation @ AUD 200.00/Origin Fumigation (Service ID ServiceB)",
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
		}

		public void TestGivenRateLineWithCTNUnitFactor_WhenAutorate_ThenServiceShouldNotBeFilteredByUnitFactor()
		{
			var client = Helper.NewOrgHeader();

			var rateEntry = CreateTestChargeCodeAndClientRate(client, "DDTM", "Destination Detention Charge Code", ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 200);

			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.CTN;

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, "USLAX", "AUSYD");
			shipment.JS_INCO = ZString.Empty;
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.FCL, "USLAX", "AUSYD");
			consol.Shipments.Add(shipment);

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RC = GP20.PK;
			container.JC_ContainerCount = 1;
			container.PackLines.Add(shipment.OuterPackLines.AddNew());

			var originDetentionPenalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);

			AssertServiceDoesNotBeFilteredByUnitFactor(client, ConsolInvoicingStyles.Apportion);
			AssertServiceDoesNotBeFilteredByUnitFactor(client, ConsolInvoicingStyles.ApportionInvoiceMaster);
			AssertServiceDoesNotBeFilteredByUnitFactor(client, ConsolInvoicingStyles.Master);

			void AssertServiceDoesNotBeFilteredByUnitFactor(OrgHeader client, ZString buyersConsolInvoicingStyle)
			{
				client.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

				Factory.Save();

				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge
						{
							ChargeCode = "DDTM",
							JR_Desc = "Destination Detention Charge Code",
							JR_OSSellAmt = 600m,
						},
					},
					shipment,
					client,
					autorateCosts: false
				);
			}
		}

		#region Implementation

		static void CreatePerServiceTariffLineWithOverride(RateEntry entry, string chargeCode, string lineUnit, decimal perUnit, decimal perUnitOverride, int numOverrides = 1)
		{
			entry.AddRateLine(chargeCode, UnitCalculator.Code, lineUnit, "AUD")
				.GetCalculator<UnitCalculator>()
				.PerUnit = perUnit;

			for (int i = 0; i < numOverrides; ++i)
			{
				var overrideLine = entry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, "", "AUD");
				overrideLine.TL_CompanyTariffLevel = (byte)(i + 2);
				overrideLine.GetCalculator<CompanyTariffOrCostBasedCalculator>()
					.PerUnit = perUnitOverride * (i + 1);
			}
		}

		AccChargeCode CreateAdHocChargeCode(string code, string description, string chargeGroup, string serviceType)
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var result = Helper.ChargeCodes.NewConsolChargeCode(code, description, "", chargeGroup, serviceType);
			result.AC_IsAdhocServiceCharge = true;

			return result;
		}

		ForwardingContainer CreateContainer(RefContainer refContainer, string containerNumber = null)
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_ContainerCount = new ZShort(1);
			container.JC_RC = refContainer.PK;
			container.JC_ContainerNum = containerNumber ?? string.Empty;

			return container;
		}

		ForwardingContainer CreateContainerWithServices(ForwardingConsol consolToAttach, RefContainer refContainer, OrgHeader contractor, string serviceCode, TimeSpan serviceDuration, byte storageDays = 0, byte detentionDays = 0, int demurrageDays = 0, int serviceCount = 1, string serviceId = null)
		{
			var container = CreateContainer(refContainer);

			if (serviceCount > 0)
			{
				var service = container.Services.AddNew();
				service.ES_ServiceId = serviceId;
				service.ES_ServiceCount = serviceCount;
				service.ES_ServiceCode = serviceCode;
				service.ES_Duration = serviceDuration;
				service.ES_Completed = ZDateTime.Now;
				service.ES_OH_Contractor = contractor.PK;
			}

			consolToAttach.Containers.Add(container);
			// override the default duration by setting it AFTER the container is added
			container.ArrivalCTOStorageDays = storageDays;
			container.ArrivalCarrierDetentionDays = detentionDays;
			if (demurrageDays > 0)
			{
				container.ArrivalTruckWaitTime = new TimeSpan(demurrageDays, 0, 0, 0);
			}

			return container;
		}

		void CreateRateLineWithUnitCalculator(RateEntry rateEntry, AccChargeCode chargeCode, string lineUnit, ZDecimal perUnitCharge)
		{
			var line = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, lineUnit, CurrencyCodes.Australia);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnitCharge;
		}

		RateEntry CreateTestChargeCodeAndClientRate(OrgHeader orgHeader, ZString chargeCodeCode, ZString chargeCodeDescription,
			string chargeCodeGroup, string chargeCodeSubGroup, ZString rateCategory, string origin, string destination,
			string rateQuantityUnit, ZDecimal ratePerUnitPrice)
		{
			var testChargeCode = Helper.ChargeCodes.New(chargeCodeCode, chargeCodeDescription, UnitCalculator.Code, chargeCodeGroup, chargeCodeSubGroup);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntry(rateCategory, RateMode.SEA, origin, destination);
			var rateLine = rateEntry.AddRateLine(testChargeCode, UnitCalculator.Code, rateQuantityUnit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = ratePerUnitPrice;

			Factory.Save();

			return rateEntry;
		}

		void AutorateAndAssertWithLogs(string message, AssertionCharge[] expectedCharges, ForwardingShipment forwardingShipment, OrgHeader localClient, string[] expectedLogs = default, string[] notExpectedLogs = default, bool autorateCost = true)
		{
			AutorateAndAssert(message, expectedCharges, forwardingShipment, localClient, autorateCosts: autorateCost);

			var note = forwardingShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).FirstOrDefault();

			if (expectedLogs != default)
			{
				foreach (var item in expectedLogs)
				{
					AssertContains(item, note.ST_NoteDataAsText);
				}
			}

			if (notExpectedLogs != default)
			{
				foreach (var item in notExpectedLogs)
				{
					AssertNotContains(item, note.ST_NoteDataAsText);
				}
			}
		}

		protected override void SetUp()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
			base.SetUp();
		}

		#endregion
	}
}
