using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	sealed class OrgSupplierPartFilterBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		public void TestTariffAndProvTariffQuery()
		{
			var tariff1 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "0101"));
			var tariff2 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "02"));
			var tariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "03"));
			var tariff4 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "04"));
			var tariff5 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "05"));
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			pivot1.CI_OP = product1.PK;
			pivot1.CI_TariffNum = tariff1.UE_Tariff;
			var pivot2 = product1.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = tariff2.UE_Tariff;
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot3 = product2.PivotsForBinding.AddNew();
			pivot3.CI_TariffNum = tariff3.UE_Tariff;
			var pivot4 = product2.PivotsForBinding.AddNew();
			pivot4.CI_TariffNum = tariff4.UE_Tariff;
			var pivot5 = pivot4.Children.AddNew();
			pivot5.CI_TariffNum = tariff5.UE_Tariff;
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var product3Pivot = product3.PivotsForBinding.AddNew();
			product3Pivot.CI_TariffNum = tariff3.UE_Tariff;
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var tariffFilter = (TariffProvTariffModuleFilter)filterBizO[OrgSupplierPartFilterConstants.Tariff.TariffProvTariff];
			tariffFilter.IsActive = true;
			tariffFilter.Property = "1";
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have this tariff", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this tariff", !product3.MatchesFilter(filterBizO.Filter));
			var product4 = Factory.New<OrgSupplierPart>();
			product4.OP_PartNum = "PartNum4";
			var product4Pivot = product4.PivotsForBinding.AddNew();
			product4Pivot.CI_TariffNum = tariff1.UE_Tariff;
			Factory.Save();
			tariffFilter.Property = tariff3.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 has this tariff number", product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 has this tariff", product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not have this tariff", !product4.MatchesFilter(filterBizO.Filter));
			tariffFilter.Property = tariff5.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Child pivot has this tariff number", product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this tariff", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not have this tariff", !product4.MatchesFilter(filterBizO.Filter));
			var product5 = Factory.New<OrgSupplierPart>();
			product5.OP_PartNum = "PartNum5";
			var product5Pivot = product5.PivotsForBinding.AddNew();
			product5Pivot.CI_TariffNum = tariff5.UE_Tariff;
			var product6 = Factory.New<OrgSupplierPart>();
			product6.OP_PartNum = "PartNum6";
			var product6Pivot = product6.PivotsForBinding.AddNew();
			product6Pivot.CI_TariffNum = tariff5.UE_Tariff;
			var product6Pivot2 = product6Pivot.Children.AddNew();
			product6Pivot2.CI_TariffNum = tariff1.UE_Tariff;
			Factory.Save();
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Child pivot has this tariff number", product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this tariff", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not have this tariff", !product4.MatchesFilter(filterBizO.Filter));
			Assert("New Product5 has just been created with this tariff - should be in filtered list", product5.MatchesFilter(filterBizO.Filter));
			Assert("New Product6 has just been created also with this tariff - should be in filtered list", product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.Property = tariff1.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have this tariff", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this tariff", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does have this tariff", product4.MatchesFilter(filterBizO.Filter));
			Assert("Product5 does not have this tariff", !product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does have this tariff on a child pivot", product6.MatchesFilter(filterBizO.Filter));
			var supTariff1 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "9801"));
			var supTariff2 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "9802"));
			var supTariff3 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, "99"));
			pivot1.CI_SupplementalTariff = supTariff1.UE_Tariff;
			product3Pivot.CI_SupplementalTariff = supTariff1.UE_Tariff;
			product4Pivot.CI_SupplementalTariff = supTariff2.UE_Tariff;
			product5Pivot.CI_SupplementalTariff = supTariff3.UE_Tariff;
			product6Pivot.CI_SupplementalTariff = supTariff1.UE_Tariff;
			Factory.Save();
			tariffFilter.Property = ZString.Empty;
			tariffFilter.ProvTariff = supTariff1.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have this supplementary tariff", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does have this supplementary tariff", product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not have this supplementary tariff", !product4.MatchesFilter(filterBizO.Filter));
			Assert("Product5 does not not have thissupplementary tariff", !product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does have this supplementary tariff", product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.ProvTariff = supTariff2.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have this supplementary tariff", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this supplementary tariff", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Only Product4 has this supplementary tariff", product4.MatchesFilter(filterBizO.Filter));
			Assert("Product5 does not not have thissupplementary tariff", !product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does not have this supplementary tariff", !product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.ProvTariff = supTariff3.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have this supplementary tariff", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have this supplementary tariff", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not not have thissupplementary tariff", !product4.MatchesFilter(filterBizO.Filter));
			Assert("Only Product5 has this supplementary tariff", product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does not have this supplementary tariff", !product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.Property = tariff3.UE_Tariff;
			tariffFilter.ProvTariff = supTariff3.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("No products have both these tariffs", !product2.MatchesFilter(filterBizO.Filter));
			Assert("No products have both these tariffs", !product3.MatchesFilter(filterBizO.Filter));
			Assert("No products have both these tariffs", !product4.MatchesFilter(filterBizO.Filter));
			Assert("No products have both these tariffs", !product5.MatchesFilter(filterBizO.Filter));
			Assert("No products have both these tariffs", !product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.Property = tariff5.UE_Tariff;
			tariffFilter.ProvTariff = supTariff3.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have both of these tariffs", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have both of these tariffs", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 does not have both of these tariffs", !product4.MatchesFilter(filterBizO.Filter));
			Assert("Product5 has this combination of tariffs", product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does not have both of these tariffs", !product6.MatchesFilter(filterBizO.Filter));
			tariffFilter.Property = tariff1.UE_Tariff;
			tariffFilter.ProvTariff = supTariff2.UE_Tariff;
			Assert("AU pivot should not be matched", !product1.MatchesFilter(filterBizO.Filter));
			Assert("Product2 does not have both of these tariffs", !product2.MatchesFilter(filterBizO.Filter));
			Assert("Product3 does not have both of these tariffs", !product3.MatchesFilter(filterBizO.Filter));
			Assert("Product4 has this combination of tariffs", product4.MatchesFilter(filterBizO.Filter));
			Assert("Product5 does not have both of these tariffs", !product5.MatchesFilter(filterBizO.Filter));
			Assert("Product6 does not have both of these tariffs", !product6.MatchesFilter(filterBizO.Filter));
		}

		public void TestMultiTariffQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_CI_Parent = ZGuid.Empty;
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var tariffFilter = filterBizO.GetMultiTariffIndicatorQuery(false);
			Assert("Filter does not match Multi Tariff", product1.MatchesFilter(tariffFilter));
			tariffFilter = filterBizO.GetMultiTariffIndicatorQuery(true);
			Assert("Filter matches Multi Tariff", !product1.MatchesFilter(tariffFilter));
		}

		public void TestGetAMSIndicatorQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = product.PK.ToString().Replace("-", "");
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_AMSIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetAMSIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter does not match AMS indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetAMSIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter matches AMS indicator", !product.MatchesFilter(indicatorFilter));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CD_AMSIndicator = "D";
			Factory.Save();
			indicatorFilter = filterBizO.GetAMSIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches Export AMS Indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetAMSIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match Export AMS Indicator", !product.MatchesFilter(indicatorFilter));
		}

		public void TestGetNOPIndicatorQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = product.PK.ToString().Replace("-", "");
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_NOPIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetNOPIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter does not match NOP indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetNOPIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter matches NOP indicator", !product.MatchesFilter(indicatorFilter));
		}

		public void TestGetOMCIndicatorQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_OMCIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetOMCIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product1.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetOMCIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product1.MatchesFilter(spiFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_OMCIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter2 = filterBizO2.GetOMCIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(spiFilter2));
			spiFilter2 = filterBizO2.GetOMCIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(spiFilter2));
		}

		public void TestTariffType()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = "HTI";
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = "SHB";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var tariffFilter = filterBizO.GetTariffTypeQuery("HTI");
			Assert("Filter does not match Tariff Type", product1.MatchesFilter(tariffFilter));
			tariffFilter = filterBizO.GetTariffTypeQuery("HTE");
			Assert("Filter does not match Tariff Type", product2.MatchesFilter(tariffFilter));
			tariffFilter = filterBizO.GetTariffTypeQuery("SHB");
			Assert("Filter does not match Tariff Type", product3.MatchesFilter(tariffFilter));
		}

		public void TestGetCountryOfQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_UC_NKCountryOfOrigin = "AU";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var countryFilter = filterBizO.GetCountryOfOriginQuery(SQLComparisonOperator.Equal, "AU");
			Assert("Filter does not match country of origin", product1.MatchesFilter(countryFilter));
			countryFilter = filterBizO.GetCountryOfOriginQuery(SQLComparisonOperator.Equal, "US");
			Assert("Filter matches country of origin", !product1.MatchesFilter(countryFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_UC_NKCountryOfExport = "AU";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			countryFilter = filterBizO.GetCountryOfExportQuery(SQLComparisonOperator.Equal, "AU");
			Assert("Filter does not match country of export", product2.MatchesFilter(countryFilter));
			countryFilter = filterBizO.GetCountryOfExportQuery(SQLComparisonOperator.Equal, "US");
			Assert("Filter matches country of export", !product2.MatchesFilter(countryFilter));
		}

		public void TestGetSPIQueries()
		{
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CD_SPI = "MX";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetSPIIndicatorQuery(SQLComparisonOperator.Equal, "MX");
			Assert("Filter does not match SPI indicator", product3.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetSPIIndicatorQuery(SQLComparisonOperator.Equal, "MY");
			Assert("Filter matches SPI indicator", !product3.MatchesFilter(spiFilter));
			var product4 = Factory.New<OrgSupplierPart>();
			product4.OP_PartNum = "PartNum4";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CD_ProductClaim = "X";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			spiFilter = filterBizO.GetProductClaimQuery(SQLComparisonOperator.Equal, "X");
			Assert("Filter does not match Product Claim indicator", product4.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetProductClaimQuery(SQLComparisonOperator.Equal, "Y");
			Assert("Filter matches Product Claim indicator", !product4.MatchesFilter(spiFilter));
			var product5 = Factory.New<OrgSupplierPart>();
			product5.OP_PartNum = "PartNum5";
			var pivot5 = product5.PivotsForBinding.AddNew();
			pivot5.CD_ProductClaim = "S";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			spiFilter = filterBizO.GetProductClaimQuery(SQLComparisonOperator.Equal, "S");
			Assert("Filter does not match Product Claim indicator", product5.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetProductClaimQuery(SQLComparisonOperator.Equal, "Y");
			Assert("Filter matches Product Claim indicator", !product5.MatchesFilter(spiFilter));
		}

		public void TestGetLaceyActIndicatorQuery()
		{
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CD_LaceyActIndicator = "X";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetLaceyActIndicatorQuery(SQLComparisonOperator.Equal, "X", true);
			Assert("Filter does not match LaceyAct indicator", product3.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetLaceyActIndicatorQuery(SQLComparisonOperator.Equal, "Y", true);
			Assert("Filter matches LaceyAct indicator", !product3.MatchesFilter(spiFilter));
		}

		public void TestGetFDAIndicatorQuery()
		{
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_ACEFDAIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter2 = filterBizO2.GetPGAFDAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(spiFilter2));
			spiFilter2 = filterBizO2.GetPGAFDAIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(spiFilter2));
		}

		public void TestGetATFIndicatorQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_ATFIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetATFIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches ATF indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetATFIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match ATF indicator", !product.MatchesFilter(indicatorFilter));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CD_ATFIndicator = "D";
			Factory.Save();
			indicatorFilter = filterBizO.GetATFIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches ATF indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetATFIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match ATF indicator", !product.MatchesFilter(indicatorFilter));
		}

		public void TestGetDDTCIndicatorQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.Details.CD_DDTCIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetDDTCIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter does not match DDTC indicator", product.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetLaceyActIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter matches DDTC indicator", !product.MatchesFilter(spiFilter));
		}

		public void TestGetTTBIndicatorQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_TTBIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetTTBIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches TTB indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetTTBIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match TTB Indicator", !product.MatchesFilter(indicatorFilter));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CD_TTBIndicator = "D";
			Factory.Save();
			indicatorFilter = filterBizO.GetTTBIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches TTB indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetTTBIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match TTB Indicator", !product.MatchesFilter(indicatorFilter));
		}

		public void TestGetPGAIndecators()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.Details.CD_NMFS370Indicator = "D";
			pivot.Details.CD_NMFSCOAIndicator = "D";
			pivot.Details.CD_NMFSAMRIndicator = "D";
			pivot.Details.CD_NMFSHMSIndicator = "D";
			pivot.Details.CD_NMFSSIMPIndicator = "D";
			pivot.Details.CD_FWSIndicator = "D";
			pivot.Details.CD_FWSIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilterNMFS370 = filterBizO.GetNMFS370IndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches NMFS370 indicator", product.MatchesFilter(spiFilterNMFS370));
			spiFilterNMFS370 = filterBizO.GetNMFS370IndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match NMFS370 Indicator", !product.MatchesFilter(spiFilterNMFS370));
			var spiFilterNMFSCOA = filterBizO.GetNMFSCOAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches NMFSCOA indicator", product.MatchesFilter(spiFilterNMFSCOA));
			var spiFilterNMFSAMR = filterBizO.GetNMFSAMRIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches NMFSAMR indicator", product.MatchesFilter(spiFilterNMFSAMR));
			spiFilterNMFSAMR = filterBizO.GetNMFSAMRIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match NMFSAMR Indicator", !product.MatchesFilter(spiFilterNMFSAMR));
			var spiFilterNMFSHMS = filterBizO.GetNMFSHMSIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches NMFSHMS indicator", product.MatchesFilter(spiFilterNMFSHMS));
			spiFilterNMFSHMS = filterBizO.GetNMFSHMSIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match NMFSHMS Indicator", !product.MatchesFilter(spiFilterNMFSHMS));
			var spiFilterNMFSSIMP = filterBizO.GetNMFSSIMPIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches NMFSHMS indicator", product.MatchesFilter(spiFilterNMFSSIMP));
			var spiFilterFWS = filterBizO.GetFWSIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matches FWS indicator", product.MatchesFilter(spiFilterFWS));
			spiFilterFWS = filterBizO.GetFWSIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not match FWS Indicator", !product.MatchesFilter(spiFilterFWS));
		}

		public void TestGetPermitQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_ADDCaseNo = "A428000000";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var permitFilter = filterBizO.GetADDCaseNumQuery(SQLComparisonOperator.Equal, "A428000000");
			Assert("Filter does not match ADD Case Number", product1.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetADDCaseNumQuery(SQLComparisonOperator.Equal, "A428000001");
			Assert("Filter matches ADD Case Number", !product1.MatchesFilter(permitFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_CVDCaseNo = "C582000000";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetCVDCaseNumQuery(SQLComparisonOperator.Equal, "C582000000");
			Assert("Filter does not match CVD Case Number", product2.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetCVDCaseNumQuery(SQLComparisonOperator.Equal, "C582000001");
			Assert("Filter matches CVD Case Number", !product2.MatchesFilter(permitFilter));
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CD_CBTPACertificate = "CBTPA";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetCBTPACertificateQuery(SQLComparisonOperator.Equal, "CBTPA");
			Assert("Filter does not match CBTPA Certificate", product3.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetCBTPACertificateQuery(SQLComparisonOperator.Equal, "DBTPB");
			Assert("Filter matches CBTPA Certificate", !product3.MatchesFilter(permitFilter));
			var product4 = Factory.New<OrgSupplierPart>();
			product4.OP_PartNum = "PartNum4";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CD_WoolLicenceNo = "WOOL";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetWoolLicenseNumQuery(SQLComparisonOperator.Equal, "WOOL");
			Assert("Filter does not match Wool Licence Number", product4.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetWoolLicenseNumQuery(SQLComparisonOperator.Equal, "HAIR");
			Assert("Filter matches Wool Licence Number", !product4.MatchesFilter(permitFilter));
			var product5 = Factory.New<OrgSupplierPart>();
			product5.OP_PartNum = "PartNum5";
			var pivot5 = product5.PivotsForBinding.AddNew();
			pivot5.CD_MiscLicenceNo = "MISC";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetMiscLicenseNumQuery(SQLComparisonOperator.Equal, "MISC");
			Assert("Filter does not match Miscellaneous License Number", product5.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetMiscLicenseNumQuery(SQLComparisonOperator.Equal, "SPURIOUS");
			Assert("Filter matches Miscellaneous License Number", !product5.MatchesFilter(permitFilter));
			var product6 = Factory.New<OrgSupplierPart>();
			product6.OP_PartNum = "PartNum6";
			var pivot6 = product6.PivotsForBinding.AddNew();
			pivot6.CD_SugarCertificate = "SUGAR";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetCASugarCertificateQuery(SQLComparisonOperator.Equal, "SUGAR");
			Assert("Filter does not match CA Sugar Certificate", product6.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetCASugarCertificateQuery(SQLComparisonOperator.Equal, "NUTRASWEET");
			Assert("Filter matches CA Sugar Certificate", !product6.MatchesFilter(permitFilter));
			var product7 = Factory.New<OrgSupplierPart>();
			product7.OP_PartNum = "PartNum7";
			var pivot7 = product7.PivotsForBinding.AddNew();
			pivot7.CD_AgricultureLicenceNo = "AGRI";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetAgricultureLicenseNumQuery(SQLComparisonOperator.Equal, "AGRI");
			Assert("Filter does not match Agriculture Licence Number", product7.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetAgricultureLicenseNumQuery(SQLComparisonOperator.Equal, "HORTI");
			Assert("Filter matches Agriculture Licence Number", !product7.MatchesFilter(permitFilter));
			var product8 = Factory.New<OrgSupplierPart>();
			product8.OP_PartNum = "PartNum8";
			var pivot8 = product8.PivotsForBinding.AddNew();
			pivot8.CD_CottonFeeExempt = "Y";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetCottonFeeExemptQuery(true);
			Assert("Filter does not match Cotton Fee Exempt", product8.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetCottonFeeExemptQuery(false);
			Assert("Filter matches Cotton Fee Exempt", !product8.MatchesFilter(permitFilter));
			var product9 = Factory.New<OrgSupplierPart>();
			product9.OP_PartNum = "PartNum9";
			var pivot9 = product9.PivotsForBinding.AddNew();
			pivot9.CD_RulingType = "R";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetRulingTypeQuery(SQLComparisonOperator.Equal, "R");
			Assert("Filter does not match Ruling Type", product9.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetRulingTypeQuery(SQLComparisonOperator.Equal, "P");
			Assert("Filter matches Ruling Type", !product9.MatchesFilter(permitFilter));
			var product10 = Factory.New<OrgSupplierPart>();
			product10.OP_PartNum = "PartNum10";
			var pivot10 = product10.PivotsForBinding.AddNew();
			pivot10.CD_RulingNumber = "123456";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			permitFilter = filterBizO.GetRulingNumQuery(SQLComparisonOperator.Equal, "123456");
			Assert("Filter does not match Ruling Number", product10.MatchesFilter(permitFilter));
			permitFilter = filterBizO.GetRulingNumQuery(SQLComparisonOperator.Equal, "654321");
			Assert("Filter matches Ruling Number", !product10.MatchesFilter(permitFilter));
		}

		public void TestGetExportQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = "HTE";
			pivot1.CD_ExportCode = "CH";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var exportFilter = filterBizO.GetExportCodeQuery(SQLComparisonOperator.Equal, "CH");
			Assert("Filter does not match Export Code", product1.MatchesFilter(exportFilter));
			exportFilter = filterBizO.GetExportCodeQuery(SQLComparisonOperator.Equal, "CI");
			Assert("Filter matches Export Code", !product1.MatchesFilter(exportFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";
			pivot2.CD_OriginIndicator = "F";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			exportFilter = filterBizO.GetOriginIndicatorQuery(SQLComparisonOperator.Equal, "F");
			Assert("Filter does not match Origin Indicator", product2.MatchesFilter(exportFilter));
			exportFilter = filterBizO.GetOriginIndicatorQuery(SQLComparisonOperator.Equal, "G");
			Assert("Filter matches Origin Indicator", !product2.MatchesFilter(exportFilter));
			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PartNum3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = "HTE";
			pivot3.CD_ECCN = "ECN";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			exportFilter = filterBizO.GetECCNQuery(SQLComparisonOperator.Equal, "ECN");
			Assert("Filter does not match ECCN", product3.MatchesFilter(exportFilter));
			exportFilter = filterBizO.GetECCNQuery(SQLComparisonOperator.Equal, "CNN");
			Assert("Filter matches ECCN", !product3.MatchesFilter(exportFilter));
			var product4 = Factory.New<OrgSupplierPart>();
			product4.OP_PartNum = "PartNum4";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = "HTE";
			pivot4.CD_ITARExemptionNo = "123.11B";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			exportFilter = filterBizO.GetITARExemptionNumQuery(SQLComparisonOperator.Equal, "123.11B");
			Assert("Filter does not match ITAR Exemption Number", product4.MatchesFilter(exportFilter));
			exportFilter = filterBizO.GetITARExemptionNumQuery(SQLComparisonOperator.Equal, "123.11C");
			Assert("Filter matches ITAR Exemption Number", !product4.MatchesFilter(exportFilter));
		}

		public void TestGetCWOQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum";
			var pivot1 = Factory.New<CusClassPartPivot>();
			var code1 = Factory.New<CensusWarningOverride>();
			code1.CY_Code = "27H";
			code1.CY_Data = "22";
			code1.CY_ParentID = pivot1.PK;
			code1.CY_ParentTableCode = "CI";
			pivot1.CI_OP = product1.PK;
			pivot1.CI_ChildType = "HTI";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var cwoFilter = filterBizO.GetCWOConditionCodeQuery(SQLComparisonOperator.Equal, "27H");
			Assert("Filter does not match CWO Condition Code", product1.MatchesFilter(cwoFilter));
			cwoFilter = filterBizO.GetCWOConditionCodeQuery(SQLComparisonOperator.Equal, "72J");
			Assert("Filter matches CWO Condition Code", !product1.MatchesFilter(cwoFilter));
			cwoFilter = filterBizO.GetCWOOverrideQuery(SQLComparisonOperator.Equal, "22");
			Assert("Filter does not match CWO Override Code", product1.MatchesFilter(cwoFilter));
			cwoFilter = filterBizO.GetCWOOverrideQuery(SQLComparisonOperator.Equal, "33");
			Assert("Filter matches CWO Override Code", !product1.MatchesFilter(cwoFilter));
		}

		public void TestGetManufacturerQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			var address1 = Factory.NewWithValidTestData<MasterFiles.Business.OrgAddress>();
			var header1 = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			pivot1.CI_OP = product1.PK;
			pivot1.CD_OA_Manufacturer = address1.PK;
			address1.OA_OH = header1.PK;
			address1.OA_Code = "PODUNKTX";
			header1.OH_Code = "ABCPQRXYZ";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var manFilter = filterBizO.GetManufacturerQuery(SQLComparisonOperator.Equal, "ABCPQRXYZ");
			Assert("Filter does not match Manufacturer", product1.MatchesFilter(manFilter));
			manFilter = filterBizO.GetManufacturerQuery(SQLComparisonOperator.Equal, "XYZPQRABC");
			Assert("Filter matches Manufacturer", !product1.MatchesFilter(manFilter));
		}

		public void TestGetAttributeQueries()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum";
			var pivot1 = Factory.New<CusClassPartPivot>();
			var attribute1 = Factory.New<CusAttributeFilter>();
			pivot1.CI_OP = product1.PK;
			attribute1.BG_CI = pivot1.PK;
			attribute1.BG_AttributeName = "AT1";
			attribute1.BG_AttributeValue1 = "AT1";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var attributeFilter = filterBizO.GetAttribute1Query(SQLComparisonOperator.Equal, "AT1");
			Assert("Filter does not match Attribute 1", product1.MatchesFilter(attributeFilter));
			attributeFilter = filterBizO.GetAttribute1Query(SQLComparisonOperator.Equal, "AT2");
			Assert("Filter matches Attribute 1", !product1.MatchesFilter(attributeFilter));
			attribute1.BG_AttributeName = "AT2";
			attribute1.BG_AttributeValue1 = "AT2";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			attributeFilter = filterBizO.GetAttribute2Query(SQLComparisonOperator.Equal, "AT2");
			Assert("Filter does not match Attribute 2", product1.MatchesFilter(attributeFilter));
			attributeFilter = filterBizO.GetAttribute2Query(SQLComparisonOperator.Equal, "AT3");
			Assert("Filter matches Attribute 2", !product1.MatchesFilter(attributeFilter));
			attribute1.BG_AttributeName = "AT3";
			attribute1.BG_AttributeValue1 = "AT3";
			Factory.Save();
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			attributeFilter = filterBizO.GetAttribute3Query(SQLComparisonOperator.Equal, "AT3");
			Assert("Filter does not match Attribute 3", product1.MatchesFilter(attributeFilter));
			attributeFilter = filterBizO.GetAttribute3Query(SQLComparisonOperator.Equal, "AT1");
			Assert("Filter matches Attribute 3", !product1.MatchesFilter(attributeFilter));
		}

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var manufacturerCode = Enterprise.Customs.US.Module.OrgSupplierPartFilterConstants.Manufacturer.Code;
			var template = Factory.NewWithValidTestData<MasterFiles.Business.ProcessTaskTemplate>();
			template.P0_ProcessType = "PRD";
			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = manufacturerCode;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;
			Factory.Save();
			var collection = new OrgSupplierPartFilterStripBusinessObject().ModuleFilters;
			AssertNotNull(collection[manufacturerCode]);
			AssertNotNull(collection[manufacturerCode + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestGetTariffInvalidQuery()
		{
			#region Setup Tariffs
			CreateUSCTariff("1102201230", ZDate.Today.AddMonths(-3), ZDate.Today.AddMonths(-1), false, false);
			CreateUSCTariff("2200331230", ZDate.Today.AddMonths(-3), ZDate.Today.AddMonths(1), false, false);
			CreateUSCTariff("3304402340", ZDate.Today.AddMonths(-3), ZDate.Today.AddMonths(1), false, false);
			#endregion
			var product1 = Factory.New<OrgSupplierPart>();
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_TariffNum = "1102201230";
			var product2 = Factory.New<OrgSupplierPart>();
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = "2200331230";
			var product3 = Factory.New<OrgSupplierPart>();
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_TariffNum = "2200331230";
			var pivot4 = product3.PivotsForBinding.AddNew();
			pivot4.CI_CI_Parent = pivot3.PK;
			pivot4.CI_TariffNum = "330440";
			var product4 = Factory.New<OrgSupplierPart>();
			var pivot5 = product4.PivotsForBinding.AddNew();
			pivot5.CI_TariffNum = "330440";
			var product5 = Factory.New<OrgSupplierPart>();
			var pivot6 = product5.PivotsForBinding.AddNew();
			var classification1 = Factory.New<CusClassification>();
			classification1.CC_TariffNum = "1102201230";
			classification1.CC_LookupCode = "ABCABC";
			classification1.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			pivot6.CI_CC = classification1.PK;
			var product6 = Factory.New<OrgSupplierPart>();
			var pivot7 = product6.PivotsForBinding.AddNew();
			var classification2 = Factory.New<CusClassification>();
			classification2.CC_TariffNum = "2200331230";
			classification2.CC_LookupCode = "BCDBCD";
			classification2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			pivot7.CI_CC = classification2.PK;
			product1.OP_PartNum = "PartNum1";
			product2.OP_PartNum = "PartNum2";
			product3.OP_PartNum = "PartNum3";
			product4.OP_PartNum = "PartNum4";
			product5.OP_PartNum = "PartNum5";
			product6.OP_PartNum = "PartNum6";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var attributeFilter = filterBizO.GetTariffInvalidFilter(ZDate.Today.AddDays(-1));
			Assert("Product 1 is invalid", product1.MatchesFilter(attributeFilter));
			Assert("Product 2 is valid", !product2.MatchesFilter(attributeFilter));
			Assert("Product 3 is invalid", product3.MatchesFilter(attributeFilter));
			Assert("Product 4 is invalid", product4.MatchesFilter(attributeFilter));
			Assert("Product 5 is invalid", product5.MatchesFilter(attributeFilter));
			Assert("Product 6 is valid", !product6.MatchesFilter(attributeFilter));
			filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			attributeFilter = filterBizO.GetTariffInvalidFilter(ZDate.Today.AddMonths(-2));
			Assert("Product 1 is valid", !product1.MatchesFilter(attributeFilter));
			Assert("Product 2 is valid", !product2.MatchesFilter(attributeFilter));
			Assert("Product 3 is invalid", product3.MatchesFilter(attributeFilter));
			Assert("Product 4 is invalid", product4.MatchesFilter(attributeFilter));
			Assert("Product 5 is valid", !product5.MatchesFilter(attributeFilter));
		}

		public void TestGetMissingADCVInfoQueryForUSCACCase()
		{
			#region Setup Tariffs
			CreateUSCTariff("1101101234", ZDate.Today.AddYears(-2), ZDate.Today.AddYears(-1), true, true);
			CreateUSCTariff("2202202345", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), false, false);
			CreateUSCTariff("3303303456", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), true, false);
			CreateUSCTariff("4404404567", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), false, true);
			CreateUSCTariff("5505505678", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), true, true);
			CreateUSCACCase("A100101001", Core.Constants.CountryCodes.Germany, "AC");
			CreateUSCACCase("C100101001", Core.Constants.CountryCodes.Germany, "AC");
			CreateUSCACCaseTariff("A100101001", "C100101001", "110110");
			CreateUSCACCaseTariff("A100101001", "C100101001", "220220");
			CreateUSCACCaseTariff("A100101001", "C100101001", "330330");
			CreateUSCACCaseTariff("A100101001", "C100101001", "440440");
			CreateUSCACCaseTariff("A100101001", "C100101001", "550550");
			#endregion
			var product1 = Factory.New<OrgSupplierPart>();
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "5505505678";
			pivot1.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var product2 = Factory.New<OrgSupplierPart>();
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "4404404567";
			pivot2.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var product3 = Factory.New<OrgSupplierPart>();
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.CI_TariffNum = "3303303456";
			pivot3.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var product4 = Factory.New<OrgSupplierPart>();
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot4.CI_TariffNum = "2202202345";
			pivot4.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var product5 = Factory.New<OrgSupplierPart>();
			var pivot5 = product5.PivotsForBinding.AddNew();
			pivot5.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot5.CI_TariffNum = "1101101234";
			pivot5.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var product6 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			product2.OP_PartNum = "PartNum2";
			product3.OP_PartNum = "PartNum3";
			product4.OP_PartNum = "PartNum4";
			product5.OP_PartNum = "PartNum5";
			product6.OP_PartNum = "PartNum6";
			var pivot6 = product6.PivotsForBinding.AddNew();
			pivot6.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot6.CI_TariffNum = "1101101234";
			pivot6.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var pivot7 = product6.PivotsForBinding.AddNew();
			pivot7.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot7.CI_TariffNum = "5505505678";
			pivot7.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
			pivot7.CI_CI_Parent = pivot6.PK;
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var attributeFilter = filterBizO.GetMissingADCVInfoQuery(true);
			Assert("Product 1 requires ADD/CVD info.", product1.MatchesFilter(attributeFilter));
			Assert("Product 2 requires CVD info.", product2.MatchesFilter(attributeFilter));
			Assert("Product 3 requires ADD info.", product3.MatchesFilter(attributeFilter));
			Assert("Product 4 doesn't require ADD/CVD info.", !product4.MatchesFilter(attributeFilter));
			Assert("Product 5 doesn't require ADD/CVD info.", !product5.MatchesFilter(attributeFilter));
			Assert("Product 6 requires ADD info.", product6.MatchesFilter(attributeFilter));
			pivot1.CD_ADDApplicable = true;
			pivot1.CD_CVDCaseNo = "12345";
			pivot2.CD_CVDApplicable = true;
			pivot3.CD_ADDCaseNo = "12345";
			pivot7.CD_CVDApplicable = true;
			pivot7.CD_ADDCaseNo = "12345";
			Factory.Save();
			Assert("Product 1 doesn't require ADD/CVD info.", !product1.MatchesFilter(attributeFilter));
			Assert("Product 2 doesn't require ADD/CVD info.", !product2.MatchesFilter(attributeFilter));
			Assert("Product 3 doesn't require ADD/CVD info.", !product3.MatchesFilter(attributeFilter));
			Assert("Product 6 doesn't require ADD/CVD info.", !product6.MatchesFilter(attributeFilter));
		}

		public void TestGetCPSCIndicatorQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_CPSCIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetCPSCIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product1.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetCPSCIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product1.MatchesFilter(spiFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_CPSCIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter2 = filterBizO2.GetCPSCIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(spiFilter2));
			spiFilter2 = filterBizO2.GetCPSCIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(spiFilter2));
		}

		public void TestGetNHTSAIndicatorQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_NHTSAIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetNHTSAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product1.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetNHTSAIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product1.MatchesFilter(spiFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_NHTSAIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter2 = filterBizO2.GetNHTSAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(spiFilter2));
			spiFilter2 = filterBizO2.GetNHTSAIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(spiFilter2));
		}

		public void TestGetDEAIndicatorQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CD_DEAIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product1.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product1.MatchesFilter(indicatorFilter));
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot1.CD_DEAIndicator = "D";
			Factory.Save();
			indicatorFilter = filterBizO.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matchs", product1.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not matche", !product1.MatchesFilter(indicatorFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CD_DEAIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter2 = filterBizO2.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(indicatorFilter2));
			indicatorFilter2 = filterBizO2.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(indicatorFilter2));
			pivot2.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot2.CD_DEAIndicator = "D";
			Factory.Save();
			indicatorFilter2 = filterBizO2.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matchs", product2.MatchesFilter(indicatorFilter2));
			indicatorFilter2 = filterBizO2.GetDEAIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not matche", !product2.MatchesFilter(indicatorFilter2));
		}

		public void TestGetAPHISIndicatorQuery()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PartNum1";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_APHISIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter = filterBizO.GetAPHISIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product1.MatchesFilter(spiFilter));
			spiFilter = filterBizO.GetAPHISIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product1.MatchesFilter(spiFilter));
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PartNum2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CD_APHISIndicator = "D";
			Factory.Save();
			var filterBizO2 = new OrgSupplierPartFilterStripBusinessObject();
			var spiFilter2 = filterBizO2.GetAPHISIndicatorQuery(SQLComparisonOperator.Equal, "D", true);
			Assert("Filter matchs", product2.MatchesFilter(spiFilter2));
			spiFilter2 = filterBizO2.GetAPHISIndicatorQuery(SQLComparisonOperator.Equal, "C", true);
			Assert("Filter does not matche", !product2.MatchesFilter(spiFilter2));
		}

		public void TestExportIndicatorsQuery()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "EXPEPA";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_PSTIndicator = "D";
			pivot.Details.CD_FWSIndicator = "D";
			pivot.Details.CD_NMFSHMSIndicator = "D";
			Factory.Save();
			var filterBizO = new OrgSupplierPartFilterStripBusinessObject();
			var indicatorFilter = filterBizO.GetPSTIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches Export EPA Indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetPSTIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match Export EPA Indicator", !product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetFWSIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches Export FWS Indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetFWSIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match Export FWS Indicator", !product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetNMFSHMSIndicatorQuery(SQLComparisonOperator.Equal, "D", false);
			Assert("Filter matches Export NMFS Indicator", product.MatchesFilter(indicatorFilter));
			indicatorFilter = filterBizO.GetNMFSHMSIndicatorQuery(SQLComparisonOperator.Equal, "C", false);
			Assert("Filter does not match Export NMFS Indicator", !product.MatchesFilter(indicatorFilter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

		void CreateUSCTariff(ZString tariffNumber, ZDate dateFrom, ZDate dateTo, ZBool antiDumping, ZBool countervailingDutyFlag)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = dateFrom;
			tariff.UE_DateTo = dateTo;
			tariff.UE_AntiDumping = antiDumping;
			tariff.UE_CountervailingDutyFlag = countervailingDutyFlag;
		}

		void CreateUSCACCase(ZString caseNumber, ZString countryCode, ZString caseStatus)
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = caseNumber;
			acCase.U5_ISOCountryCode = countryCode;
			acCase.U5_CaseStatus = caseStatus;
			acCase.U5_CaseStatusDate = ZDateTime.Today;
		}

		void CreateUSCACCaseTariff(ZString caseNumberA, ZString caseNumberC, ZString tariffNumber)
		{
			var acCaseTariffA = Factory.New<USCACCaseTariff>();
			acCaseTariffA.U9_CaseNumber = caseNumberA;
			acCaseTariffA.U9_TariffNumber = tariffNumber;
			var acCaseTariffC = Factory.New<USCACCaseTariff>();
			acCaseTariffC.U9_CaseNumber = caseNumberC;
			acCaseTariffC.U9_TariffNumber = tariffNumber;
		}
	}
}
