using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class CostingTest : RatingTestCase
	{
		public void TestCostingSaving_RegistryEnableWorkflowTemplateScopeRestrictionsIsFalse_SavedWithoutException()
		{
			using (WorkflowDataRegistry.Instance.EnableWorkflowTemplateScopeRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var existingCost = Factory.New<Costing>();
				existingCost.TH_OH = TestSupplier.PK;

				AssertNoExceptionThrown(Factory.Save);
			}
		}

		#region Existing Cost Validation

		OrgHeader TestSupplier
		{
			get
			{
				if (fTestSupplier == null)
				{
					fTestSupplier = Factory.New<OrgHeader>();
					fTestSupplier.OH_IsCreditor = true;
					fTestSupplier.OH_Code = "ZUB12Z";
					fTestSupplier.OH_FullName = "Some Test Sup";
					fTestSupplier.MainAddress.OA_Address1 = "123 Street";

					Factory.Save();
				}
				return fTestSupplier;
			}
		}

		OrgHeader fTestSupplier;

		public void TestExistingCostValidation()
		{
			var existingCost = Factory.New<Costing>();
			existingCost.TH_OH = TestSupplier.PK;
			existingCost.RunPreSaveValidation();
			AssertEquals("No Error about Existing Rate", false, existingCost.TH_OHInfo.HasErrors());
			Factory.Save();

			var newCost = Factory.New<Costing>();
			newCost.TH_OH = TestSupplier.PK;
			newCost.RunPreSaveValidation();

			AssertEquals("Error about Existing Rate", true, newCost.TH_OHInfo.HasErrors());
		}

		public void TestExistingCostValidationAcrossCompanies()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newCompany.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var existingCost = Factory.New<Costing>();
			existingCost.TH_GC = newCompany.PK; // ie in a different company
			existingCost.TH_OH = TestSupplier.PK;
			existingCost.RunPreSaveValidation();
			AssertEquals("No Error about Existing Rate", false, existingCost.TH_OHInfo.HasErrors());
			Factory.Save();

			var newCost = Factory.New<Costing>();       // current company
			newCost.TH_OH = TestSupplier.PK;
			newCost.RunPreSaveValidation();
			AssertEquals("No Error about Existing Cost as cost is in different company", false, newCost.TH_OHInfo.HasErrors());
		}

		#endregion

		#region Costing Is

		public void TestCostingIs()
		{
			var costing = Helper.NewCosting(null);

			AssertEquals(false, costing.IsClientRate());
			AssertEquals(false, costing.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, costing.IsTariff());
			AssertEquals(false, costing.IsLevelOneTariff());
			AssertEquals(false, costing.IsAdditionalTariff());

			AssertEquals(false, costing.IsQuote());

			AssertEquals(true, costing.IsCosting());
			AssertEquals(false, costing.IsWiseCostRate());
			AssertEquals(true, costing.IsStandardCostRate());

			AssertEquals(false, costing.IsClientRate());
			AssertEquals(false, costing.IsClientRateHavingSubsidiaryRelations());

			costing.TH_OH = Helper.NewOrgHeader().PK;

			AssertEquals(true, costing.IsCosting());
			AssertEquals(false, costing.IsWiseCostRate());
			AssertEquals("No longer a standard cost once an supplier has been added", false, costing.IsStandardCostRate());
		}

		#endregion

		#region Copy / Clone

		public void TestCopyRate()
		{
			var originalRate = Helper.NewCosting(Helper.NewOrgHeader());
			originalRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("WAR");
			originalRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").AddRateLine("WAR");

			var originalAirRateEntries = originalRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var originalLclRateEntries = originalRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			Factory.Save();
			var newRate = (Costing)originalRate.CopyIncludingChildren();
			var newAirRateEntries = newRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var newLclRateEntries = newRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;

			originalAirRateEntries.Load();
			originalLclRateEntries.Load();
			newAirRateEntries.Load();
			newLclRateEntries.Load();

			AssertEquals("Client PK", ZGuid.Empty, newRate.TH_OH);

			AssertEquals("AIR  Collection Count", originalAirRateEntries.Count, newAirRateEntries.Count);
			AssertEquals("LCL  Collection Count", originalLclRateEntries.Count, newLclRateEntries.Count);

			AssertEquals("AIR Origin", originalAirRateEntries[0].TI_OriginLRC, newAirRateEntries[0].TI_OriginLRC);
			AssertEquals("AIR Destination", originalAirRateEntries[0].TI_DestinationLRC, newAirRateEntries[0].TI_DestinationLRC);

			AssertEquals("LCL Origin", originalLclRateEntries[0].TI_OriginLRC, newLclRateEntries[0].TI_OriginLRC);
			AssertEquals("LCL Destination", originalLclRateEntries[0].TI_DestinationLRC, newLclRateEntries[0].TI_DestinationLRC);

			AssertEquals("AIR Rate Lines Count", originalAirRateEntries[0].RateLines.Count, newAirRateEntries[0].RateLines.Count);
			AssertEquals("LCL Rate Lines Count", originalLclRateEntries[0].RateLines.Count, newLclRateEntries[0].RateLines.Count);
		}

		public void TestClone()
		{
			var rate = Factory.NewWithValidTestData<Costing>();
			var airEntry = rate.AddRateEntry("AIR");

			var clonedRate = (Costing)rate.Clone();

			Assert("Different Objects", rate.PK != clonedRate.PK);
			var collection = clonedRate.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;
			AssertEquals("No child Rate Entry", 0, collection.Count);
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var rate = Factory.New<Costing>();
			((IImportParentRelatedActivityInfoOnNew)rate).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals(org.PK, rate.TH_OH);
		}

		#endregion
	}

	#region Business Object TestCase

	[TestedType(typeof(Costing))]
	public class CostingBizObjTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetOrgHeader_TheValueIsEmpty_SetCostAsStandardCost()
		{
			var costToTest = Factory.New<Costing>();
			costToTest.TH_OH = ZGuid.Empty;

			Factory.Save();

			AssertEquals("The cost is standard", true, costToTest.IsStandardCostRate());
			AssertEquals("The client name should be set to standard cost name", "Standard Costs (TACT/General Rates)", costToTest.TH_ClientFullName);
			AssertEquals("The description should be set to standard cost description", "Standard Costs (TACT/General Rates)", costToTest.TH_GlobalRateDescription);
			AssertEquals("The description should not be readonly", false, costToTest.TH_GlobalRateDescriptionInfo.ReadOnly);
		}

		public void TestSetOrgHeader_TheValueContainsValidOrganization_SetCostAsSpecificCost()
		{
			var costToTest = Factory.New<Costing>();
			costToTest.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			AssertEquals("The cost is specific", false, costToTest.IsStandardCostRate());
			AssertEquals("The client name should be set to organisation name", costToTest.Header.OH_FullName, costToTest.TH_ClientFullName);
			AssertEquals("The description should be set to empty", "", costToTest.TH_GlobalRateDescription);
			AssertEquals("The description should be readonly", true, costToTest.TH_GlobalRateDescriptionInfo.ReadOnly);
		}

		public void TestStandardCostDescriptionLocalization()
		{
			using (var mockResData = Res.UseMockData())
			{
				mockResData.SetResourceGetter(key =>
				{
					return new ResourceStringData(key, "Mock Translation");
				});

				var costToTest = Factory.New<Costing>();
				costToTest.TH_OH = ZGuid.Empty;
				Factory.Save();

				AssertEquals("Description should always be in English", "Standard Costs (TACT/General Rates)", costToTest.TH_GlobalRateDescription);
				AssertEquals("MultilingualDescription should be translated", "Mock Translation", costToTest.TH_GlobalRateDescriptionMultilingual.ToString());
				AssertCollectionContains("Standard Costs (TACT/General Rates)", costToTest.TH_GlobalRateDescriptionInfo.CustomizableDataResourceStrings.Source.GetCompileTimeSystemCaptions().Select(r => r.EnglishText));
			}
		}

		protected override bool IsSuppressedForTestDbHits => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<Costing>();
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}

	#endregion

	#region RelatableActivityTestCase

	[TestedType(typeof(Costing))]
	sealed class CostingRelatableActivityTest : RelatableActivityTestCase<Costing>
	{
		protected override Costing GetNewActivity()
		{
			return Factory.NewWithValidTestData<Costing>();
		}
	}

	#endregion
}
