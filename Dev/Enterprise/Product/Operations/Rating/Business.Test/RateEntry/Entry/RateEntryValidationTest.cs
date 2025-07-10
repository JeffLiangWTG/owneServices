using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateEntryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPageHeading_ShouldSupportUnicode()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.AIR);
			var jpPageHeading = "ページヘッディング";
			rateEntry.TI_PageHeading = jpPageHeading;
			AssertNoErrors("TI_PageHeading", rateEntry.TI_PageHeadingInfo);
			Factory.Save();
			var factoryTemp = new BusinessObjectFactory();
			var rateEntryTemp = factoryTemp.Load<RateEntry>(rateEntry.PK);
			AssertEquals("TI_PageHeading", jpPageHeading, rateEntryTemp.TI_PageHeading);
		}

		public void TestValidateTI_CYC_WW_Facility_WhenValueIsInvalid_ShouldShowErrorMessage()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CYD);
			var warehouse = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse.FillWithValidTestData();
			var whsWarehouse = warehouse as IWhsWarehouse;
			whsWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			rateEntry.TI_CYC_WW_Facility = warehouse.PK;
			AssertNoErrors("TI_CYC_WW_FacilityInfo should have no errors", rateEntry.TI_CYC_WW_FacilityInfo);

			rateEntry.TI_CYC_WW_Facility = Guid.Empty;
			AssertNoErrors("TI_CYC_WW_FacilityInfo should have no errors", rateEntry.TI_CYC_WW_FacilityInfo);

			rateEntry.TI_CYC_WW_Facility = Utilities.InvalidSelectionGuid;
			AssertHasError("TI_CYC_WW_FacilityInfo should have error", rateEntry.TI_CYC_WW_FacilityInfo, "Enter a valid Yard (Facility Code).");
		}

		public void TestValidateTI_CYC_WW_Facility_WhenValueIsInvalid_ShouldNotShowErrorMessage_WhenTypeIsNotCYD()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.TRW);
			var warehouse = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse.FillWithValidTestData();
			var whsWarehouse = warehouse as IWhsWarehouse;
			whsWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			rateEntry.TI_CYC_WW_Facility = warehouse.PK;
			AssertNoErrors("TI_CYC_WW_FacilityInfo should have no errors", rateEntry.TI_CYC_WW_FacilityInfo);

			rateEntry.TI_CYC_WW_Facility = Guid.Empty;
			AssertNoErrors("TI_CYC_WW_FacilityInfo should have no errors", rateEntry.TI_CYC_WW_FacilityInfo);

			rateEntry.TI_CYC_WW_Facility = Utilities.InvalidSelectionGuid;
			AssertNoErrors("TI_CYC_WW_FacilityInfo should have no errors", rateEntry.TI_CYC_WW_FacilityInfo);
		}

		public void TestValidateTI_YardUnitType_WhenValueIsInvalid_ShouldShowErrorMessage()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CYD);
			rateEntry.TI_YardUnitType = ContainerYardConstants.YardUnitType.Codes.CNT;
			AssertNoErrors("TI_YardUnitTypeInfo should have no errors", rateEntry.TI_YardUnitTypeInfo);

			rateEntry.TI_YardUnitType = "111";
			AssertHasError("TI_YardUnitTypeInfo should have error", rateEntry.TI_YardUnitTypeInfo, "Enter a valid Unit Type.");
		}

		public void TestValidateTI_YardUnitLoad_WhenValueIsInvalid_ShouldShowErrorMessage()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CYD);
			rateEntry.TI_YardUnitLoad = ContainerYardConstants.YardUnitLoad.Codes.EMP;
			AssertNoErrors("TI_YardUnitLoadInfo should have no errors", rateEntry.TI_YardUnitLoadInfo);

			rateEntry.TI_YardUnitLoad = "111";
			AssertHasError("TI_YardUnitLoadInfo should have error", rateEntry.TI_YardUnitLoadInfo, "Enter a valid Empty/Laden.");
		}

		public void TestOpeningText_ShouldSupportUnicode()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.AIR);
			var jpPageOpeningText = "オープニングテキスト";
			rateEntry.TI_PageOpeningText = jpPageOpeningText;
			AssertNoErrors("TI_PageOpeningText", rateEntry.TI_PageOpeningTextInfo);
			Factory.Save();
			var factoryTemp = new BusinessObjectFactory();
			var rateEntryTemp = factoryTemp.Load<RateEntry>(rateEntry.PK);
			AssertEquals("TI_PageOpeningText", jpPageOpeningText, rateEntry.TI_PageOpeningText);
		}

		public void TestClosingText_ShouldSupportUnicode()
		{
			var rateEntry = Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.AIR);
			var jpPageClosingText = "ページクローズテキスト";
			rateEntry.TI_PageClosingText = jpPageClosingText;
			AssertNoErrors("TI_PageClosingText", rateEntry.TI_PageClosingTextInfo);
			Factory.Save();
			var factoryTemp = new BusinessObjectFactory();
			var rateEntryTemp = factoryTemp.Load<RateEntry>(rateEntry.PK);
			AssertEquals("TI_PageClosingText", jpPageClosingText, rateEntry.TI_PageClosingText);
		}

		public void TestOrgHeader()
			=> AssertOrgHeader(RatingConstants.RateCategory.CAI);

		public void TestOrgHeader_Customs()
			=> AssertOrgHeader(RatingConstants.RateCategory.CAI);

		void AssertOrgHeader(string rateCategory)
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntry(rateCategory, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");

			using (rateEntry.GetValidationSuspender())
			{
				clientRate.TH_OH = ZGuid.Empty;
				rateEntry.TI_OriginLRC = "XX1";
				rateEntry.TI_DestinationLRC = "XX2";
				rateEntry.TI_ViaLRC = "XX3";

				AssertNoErrors("Origin", rateEntry.TI_OriginLRCInfo);
				AssertNoErrors("Destination", rateEntry.TI_DestinationLRCInfo);
				AssertNoErrors("Via", rateEntry.TI_ViaLRCInfo);
			}

			clientRate.TH_OH = Helper.NewOrgHeader().PK;
			AssertHasError("Origin", rateEntry.TI_OriginLRCInfo, "Enter a valid Origin.");
			AssertHasError("Destination", rateEntry.TI_DestinationLRCInfo, "Enter a valid Destination.");
			AssertHasError("Via", rateEntry.TI_ViaLRCInfo, "Enter a valid Via.");
		}

		[TestDate(2020, 1, 1)]
		public void TestIncoterm()
		{
			var tariff = Factory.New<CompanyTariff>();
			var rateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.CST, Core.Constants.RateMode.SEA, "", "");

			rateEntry.TI_QuotePageIncoTerm = "DPU";
			rateEntry.Validation.ValidateTI_QuotePageIncoTerm();
			AssertNoErrors("Precondition", rateEntry.TI_QuotePageIncoTermInfo);

			rateEntry.TI_QuotePageIncoTerm = "DAT";
			rateEntry.Validation.ValidateTI_QuotePageIncoTerm();
			AssertHasError
			(
				"DAT incoterm is obsolete",
				rateEntry.TI_QuotePageIncoTermInfo,
				"This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."
			);
		}

		public void TestContainerStorageWithMandatoryCommodityCode()
		{
			var requiredFields = new AutoRatingRequiredFields { RequireCommodityCode = false };
			RatingDataRegistry.Instance.CompanyTariffsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);

			var tariff = Factory.New<CompanyTariff>();
			var containerStorage = tariff.AddRateEntry(RatingConstants.RateCategory.CST, Core.Constants.RateMode.SEA, "", "");
			containerStorage.TI_RH_NKCommodityCode = "";

			AssertNoErrors(containerStorage.TI_RH_NKCommodityCodeInfo);

			requiredFields.RequireCommodityCode = true;
			RatingDataRegistry.Instance.CompanyTariffsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);

			containerStorage.Validation.ValidateTI_RH_NKCommodityCode();
			AssertNoErrors(containerStorage.TI_RH_NKCommodityCodeInfo);
		}

		public void TestPackingChargesWithMandatoryCommodityCode()
		{
			var requiredFields = new AutoRatingRequiredFields { RequireCommodityCode = false };
			RatingDataRegistry.Instance.CompanyTariffsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);

			var tariff = Factory.New<CompanyTariff>();
			var packingCharges = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, Core.Constants.RateMode.SEA, "AUSYD", "");
			packingCharges.TI_RH_NKCommodityCode = "";

			AssertNoErrors(packingCharges.TI_RH_NKCommodityCodeInfo);

			requiredFields.RequireCommodityCode = true;
			RatingDataRegistry.Instance.CompanyTariffsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);

			packingCharges.Validation.ValidateTI_RH_NKCommodityCode();
			AssertHasErrors(packingCharges.TI_RH_NKCommodityCodeInfo);
		}

		public void TestValidateLocation()
		{
			var australianSuburb = Factory.NewWithValidTestData<RefCityTown>();
			australianSuburb.R9_RN_NKCountry = "AU";

			var seychellesSuburb = Factory.NewWithValidTestData<RefCityTown>();
			seychellesSuburb.R9_RN_NKCountry = "SC";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertNoErrors(entry.OriginSuburbPKInfo);
			AssertNoErrors(entry.DestinationSuburbPKInfo);

			entry.TI_OriginLRC = "AU";
			AssertNoErrors(entry.OriginSuburbPKInfo);
			AssertNoErrors(entry.DestinationSuburbPKInfo);

			entry.OriginSuburbPK = seychellesSuburb.PK;
			AssertHasErrors(entry.OriginSuburbPKInfo);
			AssertNoErrors(entry.DestinationSuburbPKInfo);

			entry.DestinationSuburbPK = seychellesSuburb.PK;
			AssertHasErrors(entry.OriginSuburbPKInfo);
			AssertHasErrors(entry.DestinationSuburbPKInfo);

			entry.TI_OriginLRC = "SC";
			AssertNoErrors(entry.OriginSuburbPKInfo);
			AssertNoErrors(entry.DestinationSuburbPKInfo);

			entry.TI_OriginLRC = "AU";
			AssertHasErrors(entry.OriginSuburbPKInfo);
			AssertHasErrors(entry.DestinationSuburbPKInfo);
		}

		#region TestZonesValidation

		public void TestZonesValidationByLocation()
		{
			var auCityTown = Factory.NewWithValidTestData<RefCityTown>();
			auCityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			auCityTown.R9_InternationalName = "BeepBoop";

			var internationalZone = Factory.NewWithValidTestData<RefZoneHeader>();
			internationalZone.FZ_Code = "FAKE";

			var auZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var auZone = auZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var cnZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.China, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var cnZone = cnZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var sydZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			sydZoneSet.TP_R9_ZoneHubLocation = auCityTown.PK;
			var sydZone = sydZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AU", "");

			var errorMessage = "cannot be chosen as it belongs to a Transport Zone Set is not in the same country/region as the Location of this Rate Entry";

			AssertHasZonesValidationError(entry, auZone);
			AssertHasZonesValidationError(entry, sydZone);
			AssertHasZonesValidationError(entry, cnZone, errorMessage);

			entry.TI_OriginLRC = Constants.CountryCodes.China;
			AssertHasZonesValidationError(entry, auZone, errorMessage);
			AssertHasZonesValidationError(entry, sydZone, errorMessage);
			AssertHasZonesValidationError(entry, cnZone);

			entry.TI_OriginLRC = internationalZone.FZ_Code;
			AssertHasZonesValidationError(entry, auZone);
			AssertHasZonesValidationError(entry, sydZone);
			AssertHasZonesValidationError(entry, cnZone);
		}

		public void TestZonesValidationByZoneOwner()
		{
			var rateEntryHeader = Helper.NewOrgHeader();
			var diffZoneOwner = Helper.NewOrgHeader("diffOwner");

			var allOwnerZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var allOwnerRatZone = allOwnerZoneSet.CreateRateTransportZoneForTest("ALL Owner Zone");

			var sameOwnerZoneSet = Helper.CreateRateTransportZoneSet(rateEntryHeader, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var sameOwnerRatZone = sameOwnerZoneSet.CreateRateTransportZoneForTest("Same Owner Zone");

			var diffOwnerZoneSet = Helper.CreateRateTransportZoneSet(diffZoneOwner, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var diffOwnerRatZone = diffOwnerZoneSet.CreateRateTransportZoneForTest("Different Owner Zone");

			var clientRate = Helper.NewClientRate(rateEntryHeader);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AU", "");

			var errorMessage = "cannot be chosen as the Transport Zone Set's Zone Owner is different to the Service Provider / Client of this Rate Entry";

			AssertHasZonesValidationError(entry, allOwnerRatZone);
			AssertHasZonesValidationError(entry, sameOwnerRatZone);
			AssertHasZonesValidationError(entry, diffOwnerRatZone, errorMessage);
		}

		public void TestZonesValidationByType()
		{
			var allZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL);
			var allZone = allZoneSet.CreateRateTransportZoneForTest("ALL Zone");

			var ratZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);
			var ratZone = ratZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var rrcZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ROA);
			var rrcZone = rrcZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var rruZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.LRO);
			var rruZone = rruZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var rauZoneSet = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ULD);
			var rauZone = rauZoneSet.CreateRateTransportZoneForTest("RATE Zone");

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, "AU", "CN");
			var errorMessage = "cannot be chosen as the Transport Zone Set's Mode is incompatible with this Rate Mode.";

			AssertHasZonesValidationError(entry, allZone);
			AssertHasZonesValidationError(entry, ratZone);
			AssertHasZonesValidationError(entry, rrcZone);
			AssertHasZonesValidationError(entry, rruZone, errorMessage);
			AssertHasZonesValidationError(entry, rauZone, errorMessage);

			entry.TI_Mode = Core.Constants.RateMode.FTL;
			AssertHasZonesValidationError(entry, allZone);
			AssertHasZonesValidationError(entry, ratZone);
			AssertHasZonesValidationError(entry, rrcZone);
			AssertHasZonesValidationError(entry, rruZone, errorMessage);
			AssertHasZonesValidationError(entry, rauZone, errorMessage);

			entry.TI_Mode = Core.Constants.RateMode.LRO;
			AssertHasZonesValidationError(entry, ratZone);
			AssertHasZonesValidationError(entry, rrcZone);
			AssertHasZonesValidationError(entry, rruZone);
			AssertHasZonesValidationError(entry, rauZone, errorMessage);

			entry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertHasZonesValidationError(entry, ratZone);
			AssertHasZonesValidationError(entry, rrcZone, errorMessage);
			AssertHasZonesValidationError(entry, rruZone, errorMessage);
			AssertHasZonesValidationError(entry, rauZone, errorMessage);

			entry.TI_Mode = Core.Constants.RateMode.AIR;
			AssertHasZonesValidationError(entry, ratZone);
			AssertHasZonesValidationError(entry, rrcZone, errorMessage);
			AssertHasZonesValidationError(entry, rruZone, errorMessage);
			AssertHasZonesValidationError(entry, rauZone, errorMessage);
		}

		static void AssertHasZonesValidationError(RateEntry rateEntry, RateTransportZone zone, string expectedErrorMessage = "")
		{
			rateEntry.TI_TZ_DestinationZone = ZGuid.Empty;
			rateEntry.TI_TZ_OriginZone = zone.PK;

			if (string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertNoErrors("Origin Zone should not have any errors", rateEntry.TI_TZ_OriginZoneInfo);
			}
			else
			{
				AssertHasErrorContaining(rateEntry.TI_TZ_OriginZoneInfo, expectedErrorMessage);
			}

			rateEntry.TI_TZ_OriginZone = ZGuid.Empty;
			rateEntry.TI_TZ_DestinationZone = zone.PK;

			if (string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertNoErrors("Destination Zone should not have any errors", rateEntry.TI_TZ_DestinationZoneInfo);
			}
			else
			{
				AssertHasErrorContaining(rateEntry.TI_TZ_DestinationZoneInfo, expectedErrorMessage);
			}
		}

		#endregion

		public void TestAircraftTypeValidation()
		{
			var validAircraftTypes = new[] { Constants.AircraftType.CAO, Constants.AircraftType.PAX };
			var validModes = new[] { Constants.RateMode.AIR, Constants.RateMode.ULD, Constants.RateMode.LSE };

			var testHeader1 = Factory.New<Costing>();

			var newEntry = testHeader1.AddRateEntry(RatingConstants.RateCategory.AIR);

			foreach (var validAircraftType in validAircraftTypes)
			{
				newEntry.TI_Mode = "YYY";
				newEntry.TI_AircraftType = validAircraftType;
				newEntry.RunPreSaveValidation();
				AssertHasErrors("AircraftType Has Errors", newEntry.TI_AircraftTypeInfo);

				foreach (var validMode in validModes)
				{
					newEntry.TI_Mode = validMode;

					newEntry.RunPreSaveValidation();
					AssertNoErrors("AircraftType Has No Errors", newEntry.TI_AircraftTypeInfo);
				}
			}

			newEntry.TI_AircraftType = "XXX";
			newEntry.RunPreSaveValidation();
			AssertHasErrors("AircraftType Has Errors", newEntry.TI_AircraftTypeInfo);
		}

		public void TestCommodityCodeValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			AssertNoErrors(rateEntry.TI_RH_NKCommodityCodeInfo);

			rateEntry.TI_RH_NKCommodityCode = "ABC";
			AssertHasErrors(rateEntry.TI_RH_NKCommodityCodeInfo);

			rateEntry.TI_RH_NKCommodityCode = "GEN";
			AssertNoErrors(rateEntry.TI_RH_NKCommodityCodeInfo);
		}

		public void TestServiceLevelValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertNoErrors(rateEntry.TI_RS_NKServiceLevel_NIInfo);

			rateEntry.TI_RS_NKServiceLevel_NI = "ABC";
			AssertHasErrors(rateEntry.TI_RS_NKServiceLevel_NIInfo);

			rateEntry.TI_RS_NKServiceLevel_NI = "STD";
			AssertNoErrors(rateEntry.TI_RS_NKServiceLevel_NIInfo);
		}

		public void TestCarrierServiceLevelValidation()
		{
			AssertContainsExactElementsInAnyOrder("Precondition", Enumerable.Empty<OrgCarrierServiceLevel>(), Factory.Load<OrgCarrierServiceLevel>(new ZQuery()));
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			AssertNoErrors(entry.TI_PL_NKCarrierServiceLevelInfo);

			entry.RunPreSaveValidation();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("No OrgCarrierServiceLevels created during validation", Enumerable.Empty<OrgCarrierServiceLevel>(), Factory.Load<OrgCarrierServiceLevel>(new ZQuery()));

			entry.TI_PL_NKCarrierServiceLevel = "ABC";
			AssertHasErrors(entry.TI_PL_NKCarrierServiceLevelInfo);

			entry.TI_PL_NKCarrierServiceLevel = "STD";
			AssertNoErrors(entry.TI_PL_NKCarrierServiceLevelInfo);
		}

		public void TestGatewayServiceLevelValidation_IntercompanyTariff()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var rateEntry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "ABC";
			AssertHasErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "STD";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);
		}

		public void TestGatewayServiceLevelValidation_CompanyTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);
		}

		public void TestGatewayServiceLevelValidation_ClientRate()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);
		}

		public void TestGatewayServiceLevelValidation_Costing()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);
		}

		public void TestGatewayServiceLevelValidation_Quote()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKGatewayServiceLevelInfo);
		}
		public void TestShipmentGatewayServiceLevelValidation_IntercompanyTariff()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = false;

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var rateEntry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "ABC";
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "DIR"; // is not gateway service level
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);
		}

		public void TestShipmentGatewayServiceLevelValidation_CompanyTariff()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;

			var companyTariff = Helper.NewCompanyTariff();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);
		}

		public void TestShipmentGatewayServiceLevelValidation_ClientRate()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);
		}

		public void TestShipmentGatewayServiceLevelValidation_Costing()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);
		}

		public void TestShipmentGatewayServiceLevelValidation_Quote()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "";
			AssertNoErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);

			rateEntry.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertHasErrors(rateEntry.TI_RS_NKShipmentGatewayServiceLevelInfo);
		}

		public void TestWW_WarehouseValidation()
		{
			AssertWW_WarehouseValidation(RatingConstants.RateCategory.WHS);
			AssertWW_WarehouseValidation(RatingConstants.RateCategory.TRW);
			AssertWW_WarehouseValidation(RatingConstants.RateCategory.TWU);
		}

		void AssertWW_WarehouseValidation(string rateCategory)
		{
			var testHeader1 = Factory.New<ClientRate>();
			var newEntry = testHeader1.AddRateEntry(rateCategory);

			newEntry.AllWarehouses = false;
			newEntry.RunPreSaveValidation();
			AssertHasError("Should require Warehouse", newEntry.TI_WW_WarehouseInfo, "Please enter a Warehouse.");

			newEntry.AllWarehouses = true;
			newEntry.RunPreSaveValidation();
			AssertNoErrors("Should not require Warehouse", newEntry.TI_WW_WarehouseInfo);

			testHeader1 = Factory.New<ClientRate>();
			newEntry = testHeader1.AddRateEntry("ORG");

			newEntry.AllWarehouses = false;
			newEntry.RunPreSaveValidation();
			AssertNoErrors("Should not require Warehouse", newEntry.TI_WW_WarehouseInfo);
		}

		public void TestWW_WarehouseValidation_Costing()
		{
			AssertWW_WarehouseValidation_Costing(RatingConstants.RateCategory.WHS);
			AssertWW_WarehouseValidation_Costing(RatingConstants.RateCategory.TRW);
			AssertWW_WarehouseValidation_Costing(RatingConstants.RateCategory.TWU);
		}

		void AssertWW_WarehouseValidation_Costing(string category)
		{
			var testHeader1 = Factory.New<Costing>();
			var newEntry = testHeader1.AddRateEntry(category);

			newEntry.AllWarehouses = false;
			newEntry.RunPreSaveValidation();
			AssertHasError("Should require Warehouse", newEntry.TI_WW_WarehouseInfo, "Please enter a Warehouse.");

			newEntry.AllWarehouses = true;
			newEntry.RunPreSaveValidation();
			AssertHasError("Should still require Warehouse", newEntry.TI_WW_WarehouseInfo, "For Warehouse Costings, you must nominate a specific Warehouse. You cannot create Warehouse costs for 'All Warehouses'.");
		}

		public void TestOriginLRC()
		{
			var testHeader1 = Factory.New<Quote>();

			var newEntry = testHeader1.AddRateEntry("ORG");
			newEntry.TI_OriginLRC = "XXXXX";
			AssertHasErrors("Origin Has Errors", newEntry.TI_OriginLRCInfo);

			newEntry.TI_OriginLRC = "AUSYD";
			newEntry.RunPreSaveValidation();
			AssertNoErrors("Origin Has No Errors", newEntry.TI_OriginLRCInfo);
		}

		public void TestDestinationLRC()
		{
			var testHeader1 = Factory.New<ClientRate>();

			var newEntry = testHeader1.AddRateEntry("DST");
			newEntry.TI_DestinationLRC = "XXXXX";
			AssertHasErrors("Destination Has Errors", newEntry.TI_DestinationLRCInfo);

			newEntry.TI_DestinationLRC = "AUSYD";
			newEntry.RunPreSaveValidation();
			AssertNoErrors("Destination Has No Errors", newEntry.TI_DestinationLRCInfo);
		}

		public void TestPlannedLoadLRC()
		{
			var rateEntry = Factory.New<ClientRate>();

			var newEntry = rateEntry.AddRateEntry("DST");
			newEntry.TI_PlannedLoadLRC = "XXXXX";
			AssertHasErrors("PlannedLoad Has Errors", newEntry.TI_PlannedLoadLRCInfo);

			newEntry.TI_PlannedLoadLRC = "AUSYD";
			newEntry.RunPreSaveValidation();
			AssertNoErrors("PlannedLoad Has No Errors", newEntry.TI_PlannedLoadLRCInfo);

			newEntry.TI_PlannedLoadLRC = "US";
			newEntry.RunPreSaveValidation();
			AssertNoErrors("PlannedLoad Has No Errors", newEntry.TI_PlannedLoadLRCInfo);
		}

		public void TestPlannedDischargeLRC()
		{
			var rateEntry = Factory.New<ClientRate>();

			var newEntry = rateEntry.AddRateEntry("ORG");
			newEntry.TI_PlannedDischargeLRC = "NZAKL";
			AssertNoErrors("PlannedDischarge Has No Errors", newEntry.TI_PlannedDischargeLRCInfo);

			newEntry.TI_PlannedDischargeLRC = "X";
			newEntry.RunPreSaveValidation();
			AssertHasErrors("PlannedDischarge Has Errors", newEntry.TI_PlannedDischargeLRCInfo);

			newEntry.TI_PlannedDischargeLRC = "AU";
			newEntry.RunPreSaveValidation();
			AssertNoErrors("PlannedDischarge Has No Errors", newEntry.TI_PlannedDischargeLRCInfo);
		}

		public void TestValidateOriginAndDestinationWhenCrossTradeIsChanged()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry = clientRate.AddRateEntry("FCL");
			rateEntry.TI_IsCrossTrade = true;

			AssertNoErrors(rateEntry.TI_OriginLRCInfo);
			AssertNoErrors(rateEntry.TI_DestinationLRCInfo);

			Assert(rateEntry.TI_OriginLRCInfo.ReadOnly);
			Assert(rateEntry.TI_DestinationLRCInfo.ReadOnly);

			rateEntry.TI_IsCrossTrade = false;

			AssertNoErrors(rateEntry.TI_OriginLRCInfo);
			AssertNoErrors(rateEntry.TI_DestinationLRCInfo);

			Assert(!rateEntry.TI_OriginLRCInfo.ReadOnly);
			Assert(!rateEntry.TI_DestinationLRCInfo.ReadOnly);
		}

		public void TestInternationalZone_UseAsOriginOrDestination()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var australia = ausyd.Country;

			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "GT10";
			zone.FZ_Description = "GT10 Desc";
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			zone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			zone.Countries.Add(australia);

			Factory.Save();

			var clientRate = Helper.NewClientRate(null);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, zone.Code, "");

			rateEntry.Validation.ValidateTI_OriginLRC();
			AssertNoErrors(rateEntry.TI_OriginLRCInfo);

			rateEntry.TI_Mode = Core.Constants.RateMode.LCL;
			rateEntry.Validation.ValidateTI_OriginLRC();
			AssertHasErrors(rateEntry.TI_OriginLRCInfo);
		}

		public void TestEmptyOriginDestinationWithVia()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR);

			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);

			entry.TI_ViaLRC = "SGSIN";

			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);

			entry.TI_ViaLRC = "";
			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);

			entry.TI_RateCategory = RatingConstants.RateCategory.TBC;

			entry.TI_TZ_OriginZone = ZGuid.NewZGuid();
			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);

			entry.TI_TZ_OriginZone = ZGuid.Empty;
			entry.TI_TZ_DestinationZone = ZGuid.NewZGuid();

			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);

			entry.TI_TZ_DestinationZone = ZGuid.Empty;
			entry.TI_ViaLRC = "";
			entry.Validation.ValidateTI_OriginLRC();
			entry.Validation.ValidateTI_DestinationLRC();
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);
		}

		public void TestValidateRateContainerClass()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gP20.RC_FreightRateClass = "";
			gP20.RC_HandlingRateClass = "";

			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("SED");

			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no container specified", entry.TI_MatchContainerRateClassInfo, "You must specify a container type before you can choose to match the container class.");

			entry.TI_MatchContainerRateClass = false;
			AssertNoErrors(entry.TI_MatchContainerRateClassInfo);

			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no freight rate class on the 20GP container", entry.TI_MatchContainerRateClassInfo, "The 20GP container type does not have a Rate Class specified.");

			entry = rate.AddRateEntry("SID");

			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no container specified", entry.TI_MatchContainerRateClassInfo, "You must specify a container type before you can choose to match the container class.");

			entry.TI_MatchContainerRateClass = false;
			AssertNoErrors(entry.TI_MatchContainerRateClassInfo);

			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no freight rate class on the 20GP container", entry.TI_MatchContainerRateClassInfo, "The 20GP container type does not have a Rate Class specified.");

			entry = rate.AddRateEntry("SCO");

			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no container specified", entry.TI_MatchContainerRateClassInfo, "You must specify a container type before you can choose to match the container class.");

			entry.TI_MatchContainerRateClass = false;
			AssertNoErrors(entry.TI_MatchContainerRateClassInfo);

			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no freight rate class on the 20GP container", entry.TI_MatchContainerRateClassInfo, "The 20GP container type does not have a Rate Class specified.");

			entry = rate.AddRateEntry("FCL");

			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no container specified", entry.TI_MatchContainerRateClassInfo, "You must specify a container type before you can choose to match the container class.");

			entry.TI_MatchContainerRateClass = false;
			AssertNoErrors(entry.TI_MatchContainerRateClassInfo);

			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as no freight rate class on the 20GP container", entry.TI_MatchContainerRateClassInfo, "The 20GP container type does not have a Rate Class specified.");

			entry.TI_MatchContainerRateClass = false;
			AssertNoErrors(entry.TI_MatchContainerRateClassInfo);

			gP20.RC_HandlingRateClass = "HAN1";
			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertHasError("Error as rate class is for HANDLING not FREIGHT", entry.TI_MatchContainerRateClassInfo, "The 20GP container type does not have a Rate Class specified.");

			entry.TI_MatchContainerRateClass = false;

			gP20.RC_FreightRateClass = "FR1";
			entry.TI_RC = gP20.PK;
			entry.TI_MatchContainerRateClass = true;
			AssertNoErrors("No error as freight rate class specified on 20GP", entry.TI_MatchContainerRateClassInfo);
		}

		public void TestContainerRateClass()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_FreightRateClass = "FR";
			container.RC_HandlingRateClass = "HN";
			container.RC_StorageClass = "ST";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			Func<string, string, bool> hasCorrectRateClassMessage = (category, expectedClass) =>
			{
				var entry = clientRate.AddRateEntry(category, Core.Constants.RateMode.SEA, "AU", "");
				entry.TI_MatchContainerRateClass = true;
				entry.TI_RC = container.PK;

				entry = clientRate.AddRateEntry(category, Core.Constants.RateMode.SEA, "AU", "");
				entry.TI_MatchContainerRateClass = true;
				entry.TI_RC = container.PK;

				clientRate.EntryCollectionValidator.Validate();
				return entry.RowErrors.Contains("You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate.");
			};

			Assert(hasCorrectRateClassMessage(RatingConstants.RateCategory.FCL, container.RC_FreightRateClass));
			Assert(hasCorrectRateClassMessage(RatingConstants.RateCategory.ORG, container.RC_HandlingRateClass));
			Assert(hasCorrectRateClassMessage(RatingConstants.RateCategory.CST, container.RC_StorageClass));
		}

		public void TestValidateMatchingAddress()
		{
			var newClient = Factory.New<OrgHeader>();
			newClient.OH_FullName = "Test Client";
			newClient.MainAddress.OA_Address1 = "184 Bourke Road";
			newClient.MainAddress.OA_City = "Alexandria";
			newClient.MainAddress.OA_State = "NSW";
			newClient.MainAddress.OA_PostCode = "2015";
			newClient.OH_RL_NKClosestPort = "AUSYD";

			var newAddress = newClient.Addresses.AddNew();
			newAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			newAddress.OA_Address1 = "180 Oxford Street";
			newAddress.OA_City = "Darlinghurst";
			newAddress.OA_State = "NSW";
			newClient.MainAddress.OA_PostCode = "2010";
			newClient.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var rate = Helper.NewClientRate(newClient);
			var oRGEntry = rate.AddRateEntry("ORG", "LCL", "AUSYD", "");

			oRGEntry.TI_OH_Consignor = newClient.PK;
			oRGEntry.TI_OA_CartagePickupAddressOverride = ZGuid.NewZGuid();
			AssertHasErrors(oRGEntry.TI_OA_CartagePickupAddressOverrideInfo);

			oRGEntry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			AssertNoErrors(oRGEntry.TI_OA_CartagePickupAddressOverrideInfo);

			oRGEntry.TI_OH_Consignor = ZGuid.Empty;
			oRGEntry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			AssertNoErrors(oRGEntry.TI_OA_CartagePickupAddressOverrideInfo);

			var aIREntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			aIREntry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			AssertNoErrors(aIREntry.TI_OA_CartagePickupAddressOverrideInfo);

			aIREntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
			AssertNoErrors(aIREntry.TI_OA_CartagePickupAddressOverrideInfo);

			var dSTEntry = rate.AddRateEntry("DST", "LCL", "", "USLAX");
			dSTEntry.TI_OH_Consignee = newClient.PK;
			dSTEntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.NewZGuid();
			AssertHasErrors(dSTEntry.TI_OA_CartageDeliveryAddressOverrideInfo);

			dSTEntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
			AssertNoErrors(dSTEntry.TI_OA_CartageDeliveryAddressOverrideInfo);

			dSTEntry.TI_OH_Consignee = ZGuid.Empty;
			dSTEntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
			AssertNoErrors(dSTEntry.TI_OA_CartageDeliveryAddressOverrideInfo);
		}

		public void TestValidateFrequency()
		{
			var entry = Factory.New<QuoteEntry>();

			entry.TI_FrequencyUnit = "Daily";
			entry.TI_Frequency = 0;
			AssertEquals("Has Errors", true, entry.TI_FrequencyInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.NoFrequency, entry.TI_FrequencyInfo.GetErrors().GetFirstMessage());

			entry.TI_Frequency = 20;
			AssertEquals("Has Errors", false, entry.TI_FrequencyInfo.HasErrors());

			entry.TI_Frequency = -5;
			AssertEquals("Has Errors", true, entry.TI_FrequencyInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.FrequencyGreaterThanZero, entry.TI_FrequencyInfo.GetErrors().GetFirstMessage());

			entry.TI_FrequencyUnit = ZString.Empty;
			entry.TI_Frequency = 0;
			AssertEquals("Has Errors", false, entry.TI_FrequencyInfo.HasErrors());
		}

		public void TestValidateFrequencyUnit()
		{
			var entry = Factory.New<QuoteEntry>();
			entry.TI_Frequency = 5;
			AssertEquals("Has Errors", true, entry.TI_FrequencyUnitInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.NoFrequencyUnit, entry.TI_FrequencyUnitInfo.GetErrors().GetFirstMessage());

			entry.TI_FrequencyUnit = "XYZ";
			AssertEquals("Has Errors", true, entry.TI_FrequencyUnitInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidFrequencyUnit, entry.TI_FrequencyUnitInfo.GetErrors().GetFirstMessage());

			entry.TI_FrequencyUnit = "DAILY";
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_Frequency = 0;
			entry.TI_FrequencyUnit = ZString.Empty;
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_FrequencyUnit = "DAILY";
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_FrequencyUnit = "DAYS";
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_FrequencyUnit = "WEEK";
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_FrequencyUnit = "MONTHLY";
			AssertEquals("Has Errors", false, entry.TI_FrequencyUnitInfo.HasErrors());

			entry.TI_FrequencyUnit = "ZZZ";
			AssertEquals("Has Errors", true, entry.TI_FrequencyUnitInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidFrequencyUnit, entry.TI_FrequencyUnitInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidatePaymentTerm()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = quote.AddRateEntry("AIR");

			entry.TI_PaymentTerm = "CLT";
			Assert("Has error on invalid term", entry.TI_PaymentTermInfo.HasErrors());
			AssertEquals("Error message should be correct", ErrorMessages.InvalidPaymentTerm, entry.TI_PaymentTermInfo.GetErrors().GetFirstMessage());

			entry.TI_PaymentTerm = "XYZ";
			Assert("Has error on invalid term", entry.TI_PaymentTermInfo.HasErrors());
			AssertEquals("Error message should be correct", ErrorMessages.InvalidPaymentTerm, entry.TI_PaymentTermInfo.GetErrors().GetFirstMessage());

			entry.TI_PaymentTerm = "CCX";
			AssertEquals("Has no error on collect term", false, entry.TI_PaymentTermInfo.HasErrors());

			entry.TI_PaymentTerm = "PPD";
			AssertEquals("Has no error on prepaid term", false, entry.TI_PaymentTermInfo.HasErrors());

			entry.TI_PaymentTerm = string.Empty;
			AssertEquals("Has no error when empty", false, entry.TI_PaymentTermInfo.HasErrors());
		}

		public void TestValidateTransitTimeAir()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = quote.AddRateEntry("AIR");
			entry.TI_TransitTime = ZString.Empty;
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "XYZ";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());

			entry.TI_TransitTime = "OVN";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "SMD";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "6";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "61";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "120";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "121";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidateTransitTimeSea()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = testQuote.AddRateEntry("FCL");
			entry.TI_TransitTime = ZString.Empty;
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "XYZ";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());

			entry.TI_TransitTime = "OVN";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());

			entry.TI_TransitTime = "SMD";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());

			entry.TI_TransitTime = "6";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "61";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "120";
			AssertEquals("Has Errors", false, entry.TI_TransitTimeInfo.HasErrors());

			entry.TI_TransitTime = "121";
			AssertEquals("Has Errors", true, entry.TI_TransitTimeInfo.HasErrors());
			AssertEquals("Has Errors", ErrorMessages.InvalidTransitTime, entry.TI_TransitTimeInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidateContainerType()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var uLD = RefContainer.New(Factory);
			uLD.RC_ShippingMode = "AIR";

			var testEntry = Factory.New<QuoteEntry>();
			testEntry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", true, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = "LCL";
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RateCategory = RatingConstants.RateCategory.FCL;
			testEntry.TI_Mode = Core.Constants.RateMode.SEA;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("For FCL, empty container value is allowed.", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.SCO;
			testEntry.TI_Mode = Core.Constants.RateMode.SEA;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", true, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.ORG;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_Mode = "LCL";
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.DST;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_Mode = "LCL";
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertNoErrors(testEntry.TI_RCInfo);
		}

		public void TestValidateContainerTypeWithLinkedContract()
		{
			CombineAssertions(() =>
			{
				CheckContainerTypeAndContractContainerType(ContainerTypes.FlatRack, ContainerTypes.FlatRack, true);
				CheckContainerTypeAndContractContainerType(ContainerTypes.FlatRack, ContainerTypes.DryStorage, false,
					"Container Type (FLT) of Container/Equipment Type (20FR) does not match the Container Type (DRY) of the linked Carrier Contract 555.");
				CheckContainerTypeAndContractContainerType("", "", true);
				CheckContainerTypeAndContractContainerType(ContainerTypes.HorseStalls, "", true);
				CheckContainerTypeAndContractContainerType("", ContainerTypes.HorseStalls, true);
			});
		}

		void CheckContainerTypeAndContractContainerType(string rateEntryContainerType, string carrierContractContainerType, bool expectsValid, string errorMessage = "")
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();

			var contract = Helper.NewRatingContract(org, "555", RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			contract.RCT_ContainerType = carrierContractContainerType;

			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			container.RC_ContainerType = rateEntryContainerType;

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;
			entry.TI_RC = container.PK;

			entry.Validation.ValidateAll();

			if (expectsValid)
			{
				AssertNoErrors(entry.TI_RCInfo);
			}
			else
			{
				AssertHasError(
					message: errorMessage,
					entry.TI_RCInfo,
					notificationExpectedToBeFound: errorMessage);
			}
		}

		public void TestValidateContainerType_Customs()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var uLD = RefContainer.New(Factory);
			uLD.RC_ShippingMode = "AIR";

			var testEntry = Factory.New<QuoteEntry>();
			testEntry.TI_RateCategory = RatingConstants.RateCategory.CAI;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", true, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.CLC;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RateCategory = RatingConstants.RateCategory.CFC;
			testEntry.TI_Mode = Core.Constants.RateMode.SEA;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("For FCL, empty container value is allowed.", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.COR;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_Mode = "LCL";
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.CDS;
			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertNoErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_Mode = "LCL";
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			testEntry.TI_RC = ZGuid.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = gP20.PK;
			AssertEquals("Has Errors", false, testEntry.TI_RCInfo.HasErrors());

			testEntry.TI_RC = uLD.PK;
			AssertHasErrors(testEntry.TI_RCInfo);

			testEntry.TI_RC = gP20.PK;
			AssertNoErrors(testEntry.TI_RCInfo);
		}

		public void TestValidateMode()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = testQuote.AddRateEntry("ORG");

			entry.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Has Errors", false, entry.TI_ModeInfo.HasErrors());

			entry.TI_Mode = "LCL";
			AssertEquals("Has Errors", false, entry.TI_ModeInfo.HasErrors());

			entry.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Has Errors", false, entry.TI_ModeInfo.HasErrors());

			entry.TI_Mode = "ZZZ";
			AssertEquals("Has Errors", true, entry.TI_ModeInfo.HasErrors());
		}

		public void TestValidateTransportProvider()
		{
			var airShippingProvider = Factory.NewWithValidTestData<OrgHeader>();
			airShippingProvider.OH_IsShippingProvider = true;
			airShippingProvider.OH_IsAirLine = true;

			var seaShippingProvider = Factory.NewWithValidTestData<OrgHeader>();
			seaShippingProvider.OH_IsShippingProvider = true;
			seaShippingProvider.OH_IsShippingLine = true;

			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = testQuote.AddRateEntry("ORG");

			entry.TI_Mode = Core.Constants.RateMode.AIR;
			entry.TI_OH_TransportProvider = airShippingProvider.PK;

			entry.Validation.ValidateTI_OH_TransportProvider();

			AssertEquals("Has errors", false, entry.TI_OH_TransportProviderInfo.HasErrors());

			entry.TI_OH_TransportProvider = seaShippingProvider.PK;
			entry.Validation.ValidateTI_OH_TransportProvider();

			AssertEquals("Has no errors", true, entry.TI_OH_TransportProviderInfo.HasErrors());

			entry.TI_Mode = Core.Constants.RateMode.SEA;
			entry.Validation.ValidateTI_Mode();
			AssertEquals("Has errors", false, entry.TI_OH_TransportProviderInfo.HasErrors());

			entry.TI_Mode = Core.Constants.RateMode.AIR;
			entry.Validation.ValidateTI_Mode();
			AssertEquals("Has no errors", true, entry.TI_OH_TransportProviderInfo.HasErrors());
		}

		public void TestValidateVia()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");

			entry.TI_ViaLRC = "AUSYD";
			AssertEquals("Has Errors", true, entry.TI_ViaLRCInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.ViaError, entry.TI_ViaLRCInfo.GetErrors().GetFirstMessage());

			entry.TI_ViaLRC = "NZAKL";
			AssertEquals("No errors", false, entry.TI_ViaLRCInfo.HasErrors());

			entry.TI_ViaLRC = "USLAX";
			AssertEquals("Has Errors", true, entry.TI_ViaLRCInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.ViaError, entry.TI_ViaLRCInfo.GetErrors().GetFirstMessage());

			entry.TI_ViaLRC = "AUEC";
			AssertEquals("No errors", false, entry.TI_ViaLRCInfo.HasErrors());

			entry.TI_ViaLRC = "AU";
			AssertEquals("No errors", false, entry.TI_ViaLRCInfo.HasErrors());
		}

		public void TestValidSaleCurrency()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			rateEntry.TI_RX_NKCurrency = "";
			AssertEquals("Has Errors", true, rateEntry.TI_RX_NKCurrencyInfo.HasErrors());

			rateEntry.TI_RX_NKCurrency = "HJK";
			AssertEquals("Has Errors", true, rateEntry.TI_RX_NKCurrencyInfo.HasErrors());

			rateEntry.TI_RX_NKCurrency = "AUD";
			AssertEquals("Has Errors", false, rateEntry.TI_RX_NKCurrencyInfo.HasErrors());
		}

		public void TestValidateGatewayAgentType()
		{
			var orgHeader = Helper.NewOrgHeader();

			var clientRate = Helper.NewClientRate(orgHeader);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Client rate can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Client rate gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var globalClientRate = Helper.NewGlobalClientRate(orgHeader);
			entry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global client rate can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global client rate gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var companyTariff = Helper.NewCompanyTariff();
			entry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Company tariff can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Company tariff gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var globalTariff = Helper.NewGlobalTariff();
			entry = globalTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global tariff can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global tariff gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			entry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Costing can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "RAG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Costing gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			entry = globalCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global costing can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Global costing gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var quote = Helper.NewQuote(orgHeader);
			entry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Quote can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Quote gateway agent type should always be empty", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			var interCompanyTariff = Helper.NewIntercompanyTariff(orgHeader);
			entry = interCompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Intercompany tariff can have empty gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "SSA";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Invalid gateway agent type for intercompany tariff", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "FSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Gateway agent type FSG and SSG are Obsoleted", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "SAG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Intercompany tariff can have SAG gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "SSG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Gateway agent type FSG and SSG are Obsoleted", true, entry.TI_GatewayAgentTypeInfo.HasErrors());

			entry.TI_GatewayAgentType = "RAG";
			entry.Validation.ValidateTI_GatewayAgentType();
			AssertEquals("Intercompany tariff can have RAG gateway agent type", false, entry.TI_GatewayAgentTypeInfo.HasErrors());

			RateEntry entry1;
			using (interCompanyTariff.GetValidationSuspender())
			{
				entry.TI_GatewayAgentType = "SSG";
				Factory.Save();
				entry1 = interCompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX");
				entry1.TI_GatewayAgentType = "SAG";
			}
			interCompanyTariff.RunPreSaveValidation();
			AssertEquals("Gateway agent type FSG and SSG are Obsoleted", true, entry.TI_GatewayAgentTypeInfo.HasWarnings());
			AssertEquals("Intercompany tariff can have SAG gateway agent type", false, entry1.TI_GatewayAgentTypeInfo.HasErrors());
		}

		#region Contract number validation

		#region Client rate

		void TestClientContractNumberFound_SameOrgGlobalCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Client, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "1234");
		}

		void TestClientContractNumberFound_SameOrgSameCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Client, companyPK: Env.CurrentCompanyPK, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "1234");
		}

		void TestClientContractNumberNotFound_ButExistsInOtherOrg(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var orgOther = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(orgOther, "1234", Core.Constants.RatingContractTypes.Client, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberHasNotFoundWarning(rate, "1234", availableForOtherCarriers: true);
		}

		void TestClientContractNumberNotFound_ButExistsInOtherCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var rate = Helper.NewClientRate(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Client, companyPK: otherCompany.PK, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberHasNotFoundWarning(rate, "1234");
		}

		public void TestContractNumberValidation_ClientRateAIR_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_FoundContractNumber_SameOrgSameCompany() => TestClientContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_ClientRateAIR_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_FoundContractNumber_SameOrgGlobalCompany() => TestClientContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_ClientRateAIR_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_NotFound_ButExistsInOtherOrg() => TestClientContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_ClientRateAIR_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_NotFound_ButExistsInOtherCompany() => TestClientContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_ClientRateAIR_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_ClientRateAIR_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateFCL_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateLCL_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateORG_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_ClientRateDST_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_ClientRateCAI_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_ClientRateCFC_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_ClientRateCLC_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_ClientRateCOR_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_ClientRateCDS_EmptyContractNumber() => TestContractNumberBlank(Helper.NewClientRate(Helper.NewOrgHeader()), RatingConstants.RateCategory.CDS);

		#endregion

		#region Doesnt matter if costing or client

		public void TestValidateTI_ContractNumber_CanEnterNonWesternCharacters()
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "这是一串中文", Core.Constants.RatingContractTypes.Provider, transportMode: "SEA");
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "这是一串中文");
		}

		void TestRatingContract_TransportModeConversion(string rateMode, string transportModeForContract)
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Provider, transportMode: transportModeForContract);
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "1234");
		}

		public void TestRatingContract_TransportModeConversion_FclToSea() => TestRatingContract_TransportModeConversion(Constants.RateMode.FCL, Constants.TransportModes.Sea);
		public void TestRatingContract_TransportModeConversion_LclToSea() => TestRatingContract_TransportModeConversion(Constants.RateMode.LCL, Constants.TransportModes.Sea);
		public void TestRatingContract_TransportModeConversion_SeaToSea() => TestRatingContract_TransportModeConversion(Constants.RateMode.SEA, Constants.TransportModes.Sea);
		public void TestRatingContract_TransportModeConversion_AirToAir() => TestRatingContract_TransportModeConversion(Constants.RateMode.AIR, Constants.TransportModes.Air);
		public void TestRatingContract_TransportModeConversion_LseToAir() => TestRatingContract_TransportModeConversion(Constants.RateMode.LSE, Constants.TransportModes.Air);
		public void TestRatingContract_TransportModeConversion_UldToAir() => TestRatingContract_TransportModeConversion(Constants.RateMode.ULD, Constants.TransportModes.Air);

		#endregion

		#region Costing

		string GetTransportMode(string rateMode)
		{
			switch (rateMode)
			{
				case Constants.RateMode.LSE:
					return Constants.TransportModes.Air;
				case Constants.RateMode.LCL:
				case Constants.RateMode.FCL:
					return Constants.TransportModes.Sea;
				default:
					return rateMode;
			}
		}

		void TestCarrierContractNumberFound_SameOrgGlobalCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Provider, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "1234");
		}

		void TestCarrierContractNumberFound_SameOrgSameCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Provider, companyPK: Env.CurrentCompanyPK, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberValidatesOK(rate, "1234");
		}

		void TestCarrierContractNumberNotFound_ButExistsInOtherOrg(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var orgOther = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(orgOther, "1234", Core.Constants.RatingContractTypes.Provider, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberHasNotFoundWarning(rate, "1234", availableForOtherCarriers: true);
		}

		void TestCarrierContractNumberNotFound_ButExistsInOtherCompany(string category, string rateMode = Constants.RateMode.FCL)
		{
			var org = Helper.NewOrgHeader();
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var rate = Helper.NewCosting(org).AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Helper.NewRatingContract(org, "1234", Core.Constants.RatingContractTypes.Provider, companyPK: otherCompany.PK, transportMode: GetTransportMode(rateMode));
			Factory.Save();

			AssertContractNumberHasNotFoundWarning(rate, "1234");
		}

		void TestContractNumberNotFound_NoneExist(RatingHeader rateHeader, string category, string rateMode = Constants.RateMode.FCL)
		{
			var rate = rateHeader.AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);
			Factory.Save();

			AssertContractNumberHasNotFoundWarning(rate, "1234");
		}

		void TestContractNumberBlank(RatingHeader rateHeader, string category, string rateMode = Constants.RateMode.FCL)
		{
			var rate = rateHeader.AddRateEntryWithFlatRateLine(category, rateMode, "AU", "NZ", "BAF", 100);

			AssertContractNumberValidatesOK(rate, String.Empty);
		}

		public void TestContractNumberValidation_CostingRateAIR_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_FoundContractNumber_SameOrgSameCompany() => TestCarrierContractNumberFound_SameOrgSameCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_CostingRateAIR_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_FoundContractNumber_SameOrgGlobalCompany() => TestCarrierContractNumberFound_SameOrgGlobalCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_CostingRateAIR_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_NotFound_ButExistsInOtherOrg() => TestCarrierContractNumberNotFound_ButExistsInOtherOrg(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_CostingRateAIR_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_NotFound_ButExistsInOtherCompany() => TestCarrierContractNumberNotFound_ButExistsInOtherCompany(RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_CostingRateAIR_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_NotFoundContractNumber_NoneExist() => TestContractNumberNotFound_NoneExist(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CDS);

		public void TestContractNumberValidation_CostingRateAIR_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.AIR, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateFCL_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.FCL, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateLCL_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.LCL, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateORG_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.ORG);
		public void TestContractNumberValidation_CostingRateDST_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.DST);
		public void TestContractNumberValidation_CostingRateCAI_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CAI, Constants.RateMode.LSE);
		public void TestContractNumberValidation_CostingRateCFC_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CFC, Constants.RateMode.SEA);
		public void TestContractNumberValidation_CostingRateCLC_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CLC, Constants.RateMode.LCL);
		public void TestContractNumberValidation_CostingRateCOR_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.COR);
		public void TestContractNumberValidation_CostingRateCDS_EmptyContractNumber() => TestContractNumberBlank(Helper.NewCosting(Helper.NewOrgHeader()), RatingConstants.RateCategory.CDS);

		#endregion

		#region Not costing or client rate

		public void TestContractNumberValidation_QuoteAIR_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.AIR), "Potato");
		public void TestContractNumberValidation_QuoteFCL_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.FCL), "Potato");
		public void TestContractNumberValidation_QuoteLCL_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.LCL), "Potato");
		public void TestContractNumberValidation_QuoteORG_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.ORG), "Potato");
		public void TestContractNumberValidation_QuoteDST_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.DST), "Potato");
		public void TestContractNumberValidation_QuoteCAI_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CAI), "Potato");
		public void TestContractNumberValidation_QuoteCFC_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CFC), "Potato");
		public void TestContractNumberValidation_QuoteCLC_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CLC), "Potato");
		public void TestContractNumberValidation_QuoteCOR_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.COR), "Potato");
		public void TestContractNumberValidation_QuoteCDS_AnyContractNumberIsOK() => AssertContractNumberValidatesOK(Factory.New<Quote>().AddRateEntry(RatingConstants.RateCategory.CDS), "Potato");

		#endregion

		void AssertContractNumberValidatesOK(RateEntry rateEntry, string contractNumber)
		{
			rateEntry.TI_ContractNumber = contractNumber;
			rateEntry.Validation.ValidateTI_ContractNumber();

			Assert("There should be no errors", !rateEntry.TI_ContractNumberInfo.HasErrors());
			Assert("There should be no warnings", !rateEntry.TI_ContractNumberInfo.HasWarnings());
		}

		void AssertContractNumberHasNotFoundWarning(RateEntry rateEntry, string contractNumber, bool availableForOtherCarriers = false)
		{
			AssertContractNumberHasNotFoundWarning(rateEntry, contractNumber, availableForOtherCarriers, contractAndAllocationEnabledInRegistry: true);
			AssertContractNumberHasNotFoundWarning(rateEntry, contractNumber, availableForOtherCarriers, contractAndAllocationEnabledInRegistry: false);
		}

		void AssertContractNumberHasNotFoundWarning(RateEntry rateEntry, string contractNumber, bool availableForOtherCarriers, bool contractAndAllocationEnabledInRegistry)
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, contractAndAllocationEnabledInRegistry))
			{
				rateEntry.TI_ContractNumber = contractNumber;
				rateEntry.Validation.ValidateTI_ContractNumber();

				if (rateEntry.Parent.IsCosting() && availableForOtherCarriers)
				{
					if (contractAndAllocationEnabledInRegistry)
					{
						AssertHasWarning(
							$"Contract number '{contractNumber}' not in contracts module shows warning if ",
							rateEntry.TI_ContractNumberInfo,
							$"No Carrier Contract {contractNumber} found under Carrier {rateEntry.Organisation}. However, it is a valid Carrier Contract under other carriers.");
					}
					else
					{
						AssertNoWarning(
							"Carrier Contract and Client Contract module is disabled in the registry",
							rateEntry.TI_ContractNumberInfo,
							$"No Carrier Contract {contractNumber} found under Carrier {rateEntry.Organisation}. However, it is a valid Carrier Contract under other carriers.");
					}
				}
				else if (rateEntry.Parent.IsCosting() && !availableForOtherCarriers)
				{
					if (contractAndAllocationEnabledInRegistry)
					{
						AssertHasWarning(
							$"Contract number '{contractNumber}' not in contracts module shows warning if ",
							rateEntry.TI_ContractNumberInfo,
							$"No Carrier Contract {contractNumber} found under Carrier {rateEntry.Organisation}.");
					}
					else
					{
						AssertNoWarning(
							"Carrier Contract and Client Contract module is disabled in the registry",
							rateEntry.TI_ContractNumberInfo,
							$"No Carrier Contract {contractNumber} found under Carrier {rateEntry.Organisation}.");
					}
				}
				else if (rateEntry.Parent.IsClientRate())
				{
					if (contractAndAllocationEnabledInRegistry)
					{
						AssertHasWarning(
							$"Contract number '{contractNumber}' not in contracts module shows warning if ",
							rateEntry.TI_ContractNumberInfo,
							$"'{contractNumber}' does NOT have a corresponding Client Contract & Allocations record.");
					}
					else
					{
						AssertNoWarning(
							"Carrier Contract and Client Contract module is disabled in the registry",
							rateEntry.TI_ContractNumberInfo,
							$"'{contractNumber}' does NOT have a corresponding Client Contract & Allocations record.");
					}
				}
				else
				{
					Assert(false);
				}
			}
		}

		#endregion

		#region ContractNumberLinked Validation

		public void TestContractNumberLinkedValidation_WhenContractAllowsHazardous_AndHazardousCommodity_ValidateTrue()
		{
			TestContractNumberLinkedValidation_ContractHazardousnessVsCommodityHazardousnes(
				commodityIsHazardous: true,
				contractAllowHazardous: true,
				expectedValidationResult: true
			);
		}

		public void TestContractNumberLinkedValidation_WhenContractAllowsHazardous_AndSafeCommodity_ValidateTrue()
		{
			TestContractNumberLinkedValidation_ContractHazardousnessVsCommodityHazardousnes(
				commodityIsHazardous: false,
				contractAllowHazardous: true,
				expectedValidationResult: true
			);
		}

		public void TestContractNumberLinkedValidation_WhenContractDoesNotAllowsHazardous_AndSafeCommodity_ValidateTrue()
		{
			TestContractNumberLinkedValidation_ContractHazardousnessVsCommodityHazardousnes(
				commodityIsHazardous: false,
				contractAllowHazardous: false,
				expectedValidationResult: true
			);
		}

		public void TestContractNumberLinkedValidation_WhenContractDoesNotAllowsHazardous_AndHazardousCommodity_ValidateFalse()
		{
			TestContractNumberLinkedValidation_ContractHazardousnessVsCommodityHazardousnes(
				commodityIsHazardous: true,
				contractAllowHazardous: false,
				expectedValidationResult: false
			);
		}

		void TestContractNumberLinkedValidation_ContractHazardousnessVsCommodityHazardousnes(bool commodityIsHazardous, bool contractAllowHazardous, bool expectedValidationResult)
		{
			var hazCommodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "HAZ");
			hazCommodity.RH_IsHazardous = commodityIsHazardous;

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var someOrg = Helper.NewOrgHeader("some");
			var costing = Helper.NewCosting(someOrg);
			var contract = Helper.NewRatingContract(someOrg, "contract", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddYears(1), transportMode: Core.Constants.TransportModes.Sea);
			contract.RCT_AllowHazardousCommodities = contractAllowHazardous;

			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "AUMEL", "FRT", 369, commodity: "HAZ", contractNumber: "contract");
			entry.TI_ContractNumberLinked = true;
			entry.Validation.ValidateAll();

			if (expectedValidationResult)
			{
				AssertNoErrors(entry);
			}
			else
			{
				AssertHasError(entry.TI_RH_NKCommodityCodeInfo, "Commodity (HAZ) is a hazardous commodity but the linked Carrier Contract contract does not allow Hazardous Commodities.");
			}
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenNoMatchingRatingContract_AndChecked_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "5555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2), transportMode: Core.Constants.TransportModes.Air);
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			AssertEquals("No match due to wrong transport mode", true, entry.TI_ContractNumberLinkedInfo.HasErrors());
			AssertEquals("No warnings. Only errors", false, entry.TI_ContractNumberLinkedInfo.HasWarnings());

			// The ContractNumberLinked validation also performs the same validation that the TI_CarrierContractNumber does
			// except it turns it into a error for itself. This test is here as a reminder of that fact.
			AssertHasError(
				"Contract number '555' not in contracts module shows error",
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {entry.Organisation}.");
			AssertHasWarning(
				"Contract number '555' not in contracts module shows error",
				entry.TI_ContractNumberInfo,
				$"No Carrier Contract 555 found under Carrier {entry.Organisation}.");

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {entry.Organisation}.");
			AssertNoWarning(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberInfo,
				$"No Carrier Contract 555 found under Carrier {entry.Organisation}.");
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenCouldMatchOtherCarrier_AndChecked_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			var anotherOrg = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "666", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2), transportMode: Core.Constants.TransportModes.Sea);
			Helper.NewRatingContract(anotherOrg, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2), transportMode: Core.Constants.TransportModes.Sea);
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			AssertEquals("No warnings. Only errors", false, entry.TI_ContractNumberLinkedInfo.HasWarnings());

			// The ContractNumberLinked validation also performs the same validation that the TI_CarrierContractNumber does
			// except it turns it into a error for itself. This test is here as a reminder of that fact.
			AssertHasError(
				"Contract number '555' not in contracts module shows error",
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}. However, it is a valid Carrier Contract under other carriers.");
			AssertHasWarning(
				"Contract number '555' not in contracts module shows error",
				entry.TI_ContractNumberInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}. However, it is a valid Carrier Contract under other carriers.");

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}. However, it is a valid Carrier Contract under other carriers.");
			AssertNoWarning(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}. However, it is a valid Carrier Contract under other carriers.");
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_LocalCompanyPriorityOverGlobal_AndDateExpired_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			var localButExpired = Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(-1), companyPK: Env.CurrentCompanyPK);
			var globalButValid = Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2), companyPK: null);
			// Do not Factory.Save() this. As it will fail sql validation. Yes it's invalid input but I need this to prove that the local company takes priority over global company

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			Assert("It is an error, not a warning.", !entry.TI_ContractNumberLinkedInfo.HasWarnings());

			AssertHasError(
				"Contract number '555' is expired for the local company",
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Sep-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Jul-22). Relevant rates validity period should be within the Contract’s validity period."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Sep-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Jul-22). Relevant rates validity period should be within the Contract’s validity period."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenContractNumberSetIsCarrierContractAllocationNumber_AndChecked_ValidateTrue()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			AssertEquals(
				"Using a correct contract number from CCA means ticking the ContractNumberLinked is OK",
				false,
				entry.TI_ContractNumberLinkedInfo.HasErrors()
			);
			AssertEquals(
				"Using a correct contract number from CCA means ticking the ContractNumberLinked is OK",
				false,
				entry.TI_ContractNumberLinkedInfo.HasWarnings()
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenContractNumberSetIsNotCarrierContractAllocationNumber_AndChecked_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "123", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			// Below is client, not provider. So it won't match the 555 on the rate.
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Client, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			AssertEquals("We do an error - not a warning", false, entry.TI_ContractNumberLinkedInfo.HasWarnings());
			AssertHasError(
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberLinkedInfo,
				$"No Carrier Contract 555 found under Carrier {org.OH_Code}."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateStartBeforeCarrierContractAllocationStartDate_DateThenCheck_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-3); // before carrier contract start
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";

			// Date is already outside of contract number date validity when the link is made.
			entry.TI_ContractNumberLinked = true;

			AssertHasError(
				entry.TI_RateStartDateInfo,
				"Start Date (22-May-22) of Rates for the Carrier Contract 555 is earlier than the Contract’s Start Date (22-Jun-22). Relevant rates validity period should be within the Contract’s validity period.");

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_RateStartDateInfo,
				"Start Date (22-May-22) of Rates for the Carrier Contract 555 is earlier than the Contract’s Start Date (22-Jun-22). Relevant rates validity period should be within the Contract’s validity period.");
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateStartBeforeCarrierContractAllocationStartDate_CheckThenDate_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";

			// Date is inside of contract number date validity when the link is made.
			entry.TI_ContractNumberLinked = true;
			Assert("Using a correct contract number from CCA means ticking the ContractNumberLinked is OK", !entry.TI_RateStartDateInfo.HasErrors());

			// Date is now outside the contract number date validity
			entry.TI_RateStartDate = today.AddMonths(-3); // before carrier contract start

			AssertHasError(
				entry.TI_RateStartDateInfo,
				"Start Date (22-May-22) of Rates for the Carrier Contract 555 is earlier than the Contract’s Start Date (22-Jun-22). Relevant rates validity period should be within the Contract’s validity period."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_RateStartDateInfo,
				"Start Date (22-May-22) of Rates for the Carrier Contract 555 is earlier than the Contract’s Start Date (22-Jun-22). Relevant rates validity period should be within the Contract’s validity period."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateExpiresAFterCarrierContractAllocationExpiryDate_DateThenCheck_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(3); // after carrier contract expiry.
			entry.TI_ContractNumber = "555";

			// Date is already outside of contract number date validity when the link is made.
			entry.TI_ContractNumberLinked = true;
			AssertEquals(
				"Expected no warnings when contract number is linked",
				false,
				entry.TI_ContractNumberLinkedInfo.HasWarnings()
			);
			AssertHasError(
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Nov-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Oct-22). Relevant rates validity period should be within the Contract’s validity period."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Nov-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Oct-22). Relevant rates validity period should be within the Contract’s validity period."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateExpiresAFterCarrierContractAllocationExpiryDate_CheckThenDate_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";

			// Date is inside of contract number date validity when the link is made.
			entry.TI_ContractNumberLinked = true;
			Assert(
				"Using a correct contract number from CCA means ticking the ContractNumberLinked is OK",
				!entry.TI_RateEndDateInfo.HasWarnings()
			);

			// Date is now outside the contract number date validity
			entry.TI_RateEndDate = today.AddMonths(3); // after carrier contract expiry.

			Assert(
				"We do an error - not a warning",
				!entry.TI_RateEndDateInfo.HasWarnings()
			);

			AssertHasError(
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Nov-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Oct-22). Relevant rates validity period should be within the Contract’s validity period."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_RateEndDateInfo,
				"Expiry Date (22-Nov-22) of Rates for the Carrier Contract 555 is later than the Contract’s Expiry Date (22-Oct-22). Relevant rates validity period should be within the Contract’s validity period."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenNonCostingRate_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Client, today.AddMonths(-2), today.AddMonths(2));

			var clientRate = Helper.NewClientRate(org);
			var entry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			// Only clientrate is tested, (of all the non costing rates) because only the contract number
			// for costing and client can be searched in the contracts and allocations module. Yet
			// the linkeage of a contract number is only supported for costing rates.
			AssertEquals(
				"We do an error - not a warning",
				false,
				entry.TI_ContractNumberLinkedInfo.HasWarnings()
			);

			AssertHasError(
				entry.TI_ContractNumberLinkedInfo,
				"Only Carrier Contract Numbers may be linked"
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Carrier and client contract module disabled in the registry",
				entry.TI_ContractNumberLinkedInfo,
				"Only Carrier Contract Numbers may be linked"
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_LinkedWithoutContractNumber_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();

			var clientRate = Helper.NewCosting(org);
			var entry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);

			entry.TI_ContractNumber = string.Empty;
			entry.TI_ContractNumberLinked = true;

			AssertEquals(
				"We do an error - not a warning",
				false,
				entry.TI_ContractNumberLinkedInfo.HasWarnings()
			);

			AssertHasError(
				entry.TI_ContractNumberLinkedInfo,
				"The Carrier Contract Number cannot be blank. It must have a corresponding Carrier Contract & Allocations record."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				entry.TI_ContractNumberLinkedInfo,
				"The Carrier Contract Number cannot be blank. It must have a corresponding Carrier Contract & Allocations record."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateModeIsInvalidBeforeLinking_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2), transportMode: Core.Constants.TransportModes.Air);
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";
			entry.TI_ContractNumberLinked = true;

			AssertEquals("Contract number found ok", false, entry.TI_ContractNumberLinkedInfo.HasErrors());
			AssertEquals("No warnings. Only errors", false, entry.TI_ContractNumberLinkedInfo.HasWarnings());

			AssertHasError(
				"Mismatched mode",
				entry.TI_ModeInfo,
				"Transport Mode (AIR) of the Carrier Contract 555 does not match the Transport Mode (SEA) of this Rate. Rate and relevant Carrier Contract should have aligned Transport Mode."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Mismatched mode",
				entry.TI_ModeInfo,
				"Transport Mode (AIR) of the Carrier Contract 555 does not match the Transport Mode (SEA) of this Rate. Rate and relevant Carrier Contract should have aligned Transport Mode."
			);
		}

		[TestDate(2022, 08, 22)]
		public void TestContractNumberLinkedValidation_WhenRateModeBecomesInvalidAfterLinking_ValidateFalse()
		{
			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var today = ZDate.Today;
			var org = Helper.NewOrgHeader();
			Helper.NewRatingContract(org, "555", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddMonths(2));
			Factory.Save();

			var costing = Helper.NewCosting(org);
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			entry.TI_RateStartDate = today.AddMonths(-1);
			entry.TI_RateEndDate = today.AddMonths(1);
			entry.TI_ContractNumber = "555";

			// Initially the RateMode is valid.
			entry.TI_ContractNumberLinked = true;
			AssertEquals("Mode is matching", false, entry.TI_ModeInfo.HasErrors());
			AssertEquals(
				"Using a correct contract number from CCA means ticking the ContractNumberLinked is OK",
				false,
				entry.TI_ContractNumberLinkedInfo.HasErrors()
			);
			AssertEquals(
				"Using a correct contract number from CCA means ticking the ContractNumberLinked is OK",
				false,
				entry.TI_ContractNumberLinkedInfo.HasWarnings()
			);

			// Then the ratemode changes.
			entry.TI_Mode = Constants.RateMode.ROA;
			AssertHasError(
				"Mismatched mode",
				entry.TI_ModeInfo,
				"Transport Mode (SEA) of the Carrier Contract 555 does not match the Transport Mode (ROA) of this Rate. Rate and relevant Carrier Contract should have aligned Transport Mode."
			);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			entry.Validation.ValidateAll();
			AssertNoError(
				"Mismatched mode",
				entry.TI_ModeInfo,
				"Transport Mode (SEA) of the Carrier Contract 555 does not match the Transport Mode (ROA) of this Rate. Rate and relevant Carrier Contract should have aligned Transport Mode."
			);
		}

		#endregion

		#region Test Registry Dependant Optional Validation

		public void TestRegistryDependantOptionalValidation()
		{
			AssertRegistryDependantOptionalValidation(Helper.NewQuote(Helper.NewOrgHeader()));
			AssertRegistryDependantOptionalValidation(Helper.NewClientRate(Helper.NewOrgHeader()));
			AssertRegistryDependantOptionalValidation(Helper.NewCosting(Helper.NewOrgHeader()));
			AssertRegistryDependantOptionalValidation(Helper.NewCompanyTariff());
		}

		void AssertRegistryDependantOptionalValidation(RatingHeader header)
		{
			foreach (RateEntryCollection collection in header.EntryCollectionsExcludingSummary.Values)
			{
				collection.AddNew();
			}

			AssertCarrierServiceLevelValidation(header);
			AssertServiceLevelValidation(header);
			AssertFrequencyValidation(header);
			AssertTransitTimeValidation(header);
			AssertCommodityCodeValidation(header);
		}

		#region Assert Field Specific Validation

		#region Carrier Service Level

		void AssertCarrierServiceLevelValidation(RatingHeader header)
		{
			foreach (RateEntryCollection collection in header.EntryCollectionsExcludingSummary.Values)
			{
				AssertCarrierServiceLevelValidationForRateEntryType(collection[0], true);
			}
		}

		void AssertCarrierServiceLevelValidationForRateEntryType(RateEntry entryForTest, bool shouldBeEnforced)
		{
			entryForTest.TI_OriginLRC = "AUSYD";
			entryForTest.TI_DestinationLRC = "USLAX";
			SetRatingValidationFields(entryForTest, false, false, false, false);

			entryForTest.TI_PL_NKCarrierServiceLevel = "STD";
			AssertNoErrors("Field has no errors", entryForTest.TI_PL_NKCarrierServiceLevelInfo);

			SetRatingValidationFields(entryForTest, true, false, false, false);
			AssertNoErrors("Field has no errors", entryForTest.TI_PL_NKCarrierServiceLevelInfo);

			entryForTest.TI_PL_NKCarrierServiceLevel = "";

			if (shouldBeEnforced && entryForTest.IsAir() && entryForTest.IsCosting())
			{
				AssertHasErrors("Field should be enforced and invalid", entryForTest.TI_PL_NKCarrierServiceLevelInfo);
			}
			else
			{
				AssertNoErrors("Field should not be enforced and therefore be valid", entryForTest.TI_PL_NKCarrierServiceLevelInfo);
			}

			SetRatingValidationFields(entryForTest, false, false, false, false);
			entryForTest.RunPreSaveValidation();
			AssertNoErrors("Field has no errors", entryForTest.TI_PL_NKCarrierServiceLevelInfo);
		}

		#endregion

		#region Service Level

		void AssertServiceLevelValidation(RatingHeader header)
		{
			foreach (RateEntryCollection collection in header.EntryCollectionsExcludingSummary.Values)
			{
				AssertServiceLevelValidationForRateEntryType(collection[0], true);
			}
		}

		void AssertServiceLevelValidationForRateEntryType(RateEntry entryForTest, bool shouldBeEnforced)
		{
			entryForTest.TI_OriginLRC = "AUSYD";
			entryForTest.TI_DestinationLRC = "USLAX";
			SetRatingValidationFields(entryForTest, false, false, false, false);
			var collection = entryForTest.Lookups.ServiceLevel_NIs;

			entryForTest.TI_RS_NKServiceLevel_NI = collection[0].RS_Code;
			AssertNoErrors("Field has no errors", entryForTest.TI_RS_NKServiceLevel_NIInfo);

			SetRatingValidationFields(entryForTest, true, false, false, false);
			AssertNoErrors("Field has no errors", entryForTest.TI_RS_NKServiceLevel_NIInfo);

			entryForTest.TI_RS_NKServiceLevel_NI = "";

			if (shouldBeEnforced && !entryForTest.IsCFS() && !entryForTest.IsShippingExportDetention() && !entryForTest.IsShippingImportDetention())
			{
				AssertHasErrors("Field should be enforced and invalid", entryForTest.TI_RS_NKServiceLevel_NIInfo);
			}
			else
			{
				AssertNoErrors("Field should not be enforced and therefore be valid", entryForTest.TI_RS_NKServiceLevel_NIInfo);
			}

			SetRatingValidationFields(entryForTest, false, false, false, false);
			entryForTest.RunPreSaveValidation();
			AssertNoErrors("Field has no errors", entryForTest.TI_RS_NKServiceLevel_NIInfo);
		}

		#endregion

		#region Transit Time

		void AssertTransitTimeValidation(RatingHeader header)
		{
			foreach (RateEntryCollection collection in header.EntryCollectionsExcludingSummary.Values)
			{
				var shouldBeEnforced = collection[0].IsFreightEntry();
				var validCode = ZString.Empty;
				if (shouldBeEnforced)
				{
					if (collection.RateEntryType == RatingConstants.RateCategory.AIR)
					{
						validCode = collection[0].Lookups.AirTransitTimes[0].Code;
					}
					else
					{
						validCode = collection[0].Lookups.SeaTransitTimes[0].Code;
					}
				}
				AssertTransitTimeValidationForRateEntryType(collection[0], shouldBeEnforced, validCode);
			}
		}

		void AssertTransitTimeValidationForRateEntryType(RateEntry entryForTest, bool shouldBeEnforced, ZString validCode)
		{
			entryForTest.TI_OriginLRC = "AUSYD";
			entryForTest.TI_DestinationLRC = "USLAX";
			SetRatingValidationFields(entryForTest, false, false, false, false);

			entryForTest.TI_TransitTime = validCode;
			AssertNoErrors("Field has no errors", entryForTest.TI_TransitTimeInfo);

			SetRatingValidationFields(entryForTest, false, false, true, false);
			AssertNoErrors("Field has no errors", entryForTest.TI_TransitTimeInfo);

			entryForTest.TI_TransitTime = "";

			if (shouldBeEnforced)
			{
				AssertHasErrors("Field should be enforced and invalid", entryForTest.TI_TransitTimeInfo);
			}
			else
			{
				AssertNoErrors("Field should not be enforced and therefore be valid", entryForTest.TI_TransitTimeInfo);
			}

			SetRatingValidationFields(entryForTest, false, false, false, false);
			entryForTest.RunPreSaveValidation();
			AssertNoErrors("Field has no errors", entryForTest.TI_TransitTimeInfo);
		}

		#endregion

		#region Commodity Code

		void AssertCommodityCodeValidation(RatingHeader ratingHeader)
		{
			foreach (RateEntryCollection collection in ratingHeader.EntryCollectionsExcludingSummary.Values)
			{
				var shouldBeEnforced = collection[0].TI_RateCategory != RatingConstants.RateCategory.CST
					&& collection[0].TI_RateCategory != RatingConstants.RateCategory.SID
					&& collection[0].TI_RateCategory != RatingConstants.RateCategory.SED;

				AssertCommodityCodeValidationForRateEntryType(collection[0], shouldBeEnforced);
			}
		}

		void AssertCommodityCodeValidationForRateEntryType(RateEntry rateEntry, bool shouldBeEnforced)
		{
			rateEntry.TI_OriginLRC = "AUSYD";
			rateEntry.TI_DestinationLRC = "USLAX";
			SetRatingValidationFields(rateEntry, false, false, false, false);
			var collection = rateEntry.Lookups.CommodityCodes;

			rateEntry.TI_RH_NKCommodityCode = collection[0].RH_Code;
			AssertNoErrors("Field has no errors", rateEntry.TI_RH_NKCommodityCodeInfo);

			SetRatingValidationFields(rateEntry, false, true, false, false);
			AssertNoErrors("Field has no errors", rateEntry.TI_RH_NKCommodityCodeInfo);

			rateEntry.TI_RH_NKCommodityCode = "";

			if (shouldBeEnforced)
			{
				AssertHasErrors("Field should be enforced and invalid", rateEntry.TI_RH_NKCommodityCodeInfo);
			}
			else
			{
				AssertNoErrors("Field should not be enforced and therefore be valid: " + rateEntry.TI_RateCategory, rateEntry.TI_RH_NKCommodityCodeInfo);
			}

			SetRatingValidationFields(rateEntry, false, false, false, false);
			rateEntry.RunPreSaveValidation();
			AssertNoErrors("Field has no errors", rateEntry.TI_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region Frequency

		void AssertFrequencyValidation(RatingHeader header)
		{
			foreach (RateEntryCollection collection in header.EntryCollectionsExcludingSummary.Values)
			{
				AssertFrequencyValidationForRateEntryType(collection[0], collection[0].IsFreightEntry());
			}
		}

		void AssertFrequencyValidationForRateEntryType(RateEntry entryForTest, bool shouldBeEnforced)
		{
			entryForTest.TI_OriginLRC = "AUSYD";
			entryForTest.TI_DestinationLRC = "USLAX";
			SetRatingValidationFields(entryForTest, false, false, false, false);

			entryForTest.TI_Frequency = 2;
			AssertNoErrors("Field has no errors", entryForTest.TI_FrequencyInfo);

			SetRatingValidationFields(entryForTest, false, false, false, true);
			AssertNoErrors("Field has no errors", entryForTest.TI_FrequencyInfo);

			entryForTest.TI_Frequency = 0;

			if (shouldBeEnforced)
			{
				AssertHasErrors("Field should be enforced and invalid", entryForTest.TI_FrequencyInfo);
			}
			else
			{
				AssertNoErrors("Field should not be enforced and therefore be valid", entryForTest.TI_FrequencyInfo);
			}

			SetRatingValidationFields(entryForTest, false, false, false, false);
			entryForTest.RunPreSaveValidation();
			AssertNoErrors("Field has no errors", entryForTest.TI_FrequencyInfo);
		}

		#endregion

		void SetRatingValidationFields(RateEntry entryForTest, bool serviceLevel, bool commodityCode, bool transitTime, bool frequency)
		{
			var requiredFields = new AutoRatingRequiredFields();
			requiredFields.RequireServiceLevel = serviceLevel;
			requiredFields.RequireCommodityCode = commodityCode;
			requiredFields.RequireTransitTime = transitTime;
			requiredFields.RequireFrequency = frequency;

			var info = typeof(RateEntryValidation).GetProperty("RatingValidationRequiredFields", BindingFlags.NonPublic | BindingFlags.Instance);
			var registryItem = (AutoRatingRequiredFieldsRegistryItem)info.GetValue(entryForTest.Validation, null);

			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);
			entryForTest.MarkAsNeedingValidation();
		}

		#endregion

		#endregion

		#region Helpers

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}
}
