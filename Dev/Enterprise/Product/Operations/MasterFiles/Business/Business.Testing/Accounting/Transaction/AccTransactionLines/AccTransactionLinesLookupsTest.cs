using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionLinesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBSHGLHeaders()
		{
			var transactionLines = Factory.New<AccTransactionLines>();
			var bshGLHeaders = transactionLines.Lookups.BSHGLHeaders;
			AssertEquals(1, bshGLHeaders.FilterBusinessObjectDefaults.Count);
			var defaultFilter = bshGLHeaders.FilterBusinessObjectDefaults["Account Type:Property"];
			AssertEquals("Account Type", defaultFilter.FilterName);
			AssertEquals("BSH", defaultFilter.Value);
		}

		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "MSG";
			taxMsg.A9_EnglishMsg = "ENG msg";
			taxMsg.A9_LocalMsg = "Local msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var transactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLines.AL_GC = uSCompany.PK;
			transactionLines.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();

			var lookup = new AccTransactionLinesLookups(transactionLines);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				Assert("The TaxMessage should be found if the Parent Company and message country are the same", lookup.VATClasses.Contains(taxMsg));
				transactionLines.AL_GC = ZGuid.Empty;
				Assert("The TaxMessage should not be found if the Parent company is null and the Current Company is different from the message country", !lookup.VATClasses.Contains(taxMsg));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Assert("The TaxMessage should be found if the Parent company is null and the Current Company and message country are the same", lookup.VATClasses.Contains(taxMsg));
			}
		}

		public void TestTaxRates()
		{
			AccTaxRate activeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate otherCountryActiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryActiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia ? Constants.CountryCodes.UnitedKingdom : Constants.CountryCodes.Australia;
			AccTaxRate inactiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			inactiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			inactiveTaxRate.AT_IsActive = false;

			AccTransactionLines transactionLines = Factory.New<AccTransactionLines>();
			transactionLines.Lookups.TaxRates.Load();
			AssertEquals("Should contain active tax rate", true, transactionLines.Lookups.TaxRates.Contains(activeTaxRate));
			AssertEquals("Should not contain other country tax rate", false, transactionLines.Lookups.TaxRates.Contains(otherCountryActiveTaxRate));
			AssertEquals("Should not contain inactive tax rate", false, transactionLines.Lookups.TaxRates.Contains(inactiveTaxRate));
		}

		public void TestTaxRateCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			AccTransactionLines transactionLines = Factory.New<AccTransactionLines>();
			transactionLines.Lookups.TaxRates.Load();
			Assert("Collection should present only the VAT Tax System", transactionLines.Lookups.TaxRates.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestBranches()
		{
			GlbBranch currentCompanybranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanybranch.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch currentCompanyInActivebranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyInActivebranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyInActivebranch.GB_IsActive = false;

			GlbCompany nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch nonCurrentCompanyBranch = nonCurrentCompany.Branches.AddNew();

			Factory.Save();

			AccTransactionLines transactionLines = Factory.New<AccTransactionLines>();
			transactionLines.Lookups.Branches.Load();

			Assert("Should contain current company branches", transactionLines.Lookups.Branches.Contains(currentCompanybranch));
			Assert("Should not contain inactive branches", !transactionLines.Lookups.Branches.Contains(currentCompanyInActivebranch));
			Assert("Should not contain current company branches", !transactionLines.Lookups.Branches.Contains(nonCurrentCompanyBranch));
		}

		public void TestPlacesOfSupplyLookup()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Australia })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var placesOfSupply = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);

					var transactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
					var lookups = new AccTransactionLinesLookups(transactionLines);
					AssertContainsExactElementsInAnyOrder(placesOfSupply, lookups.PlacesOfSupply);
				}
			}
		}

		public void TestPlaceOfSupplyTypesLookup()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Australia })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var placeOfSupplyTypes = PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(GlbCompany.CurrentCompany);

					var transactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
					var lookups = new AccTransactionLinesLookups(transactionLines);
					AssertContainsExactElementsInAnyOrder(placeOfSupplyTypes, lookups.PlaceOfSupplyTypes);
				}
			}
		}

		public void TestSupplyType()
		{
			var supplyTypes = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;

			var transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_GC = GlbCompany.CurrentCompany.PK;
			AssertContainsExactElementsInAnyOrder(supplyTypes.GetActiveCodeDescriptionPairList(), transactionLine.Lookups.SupplyTypes);

			transactionLine.AL_GC = ZGuid.Empty;
			AssertNoExceptionThrown(() => _ = transactionLine.Lookups.SupplyTypes);
		}
	}
}
