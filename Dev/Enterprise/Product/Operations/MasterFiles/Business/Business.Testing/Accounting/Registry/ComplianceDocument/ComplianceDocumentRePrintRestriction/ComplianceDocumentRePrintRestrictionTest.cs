using CargoWise.Definitions;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentRePrintRestriction))]
	class ComplianceDocumentRePrintRestrictionTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestOrganizationCategoryList()
		{
			AssertEquals("OrganizationCategoryList count", 4, BizObj.OrganizationCategoryList.Count);
			AssertEquals("should contain BUS", true, BizObj.OrganizationCategoryList.ContainsCode(OrgConstants.Category.Business));
			AssertEquals("should contain GOV", true, BizObj.OrganizationCategoryList.ContainsCode(OrgConstants.Category.Government));
			AssertEquals("should contain NAT", true, BizObj.OrganizationCategoryList.ContainsCode(OrgConstants.Category.NaturalPersonIndividual));
			AssertEquals("should contain NGO", true, BizObj.OrganizationCategoryList.ContainsCode(OrgConstants.Category.NonGovernmentOrganisation));
		}

		public void TestComplianceDocumentMenuList()
		{
			BizObj.ComplianceDocumentMenuList.Load();
			int originalCount = BizObj.ComplianceDocumentMenuList.Count;

			var menuItem1 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem1.SU_BusinessContext = nameof(BusinessContext.ARComplianceDocument);
			menuItem1.SU_MenuName = "Label1";
			var menuItem2 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem2.SU_BusinessContext = nameof(BusinessContext.APComplianceDocument);
			menuItem2.SU_MenuName = "Label2";
			Factory.Save();

			BizObj.ComplianceDocumentMenuList.Load();
			AssertEquals("ComplianceDocumentMenuList count", originalCount + 2, BizObj.ComplianceDocumentMenuList.Count);
			Assert("should contain Label1", BizObj.ComplianceDocumentMenuList.Contains(menuItem1));
			Assert("should contain Label2", BizObj.ComplianceDocumentMenuList.Contains(menuItem2));
		}

		public void TestShouldNotBeEmpty()
		{
			var item = new ComplianceDocumentRePrintRestriction();
			item.OrganizationCategory = "NGO";
			item.NumberOfReprintAllowed = 1;
			item.ComplianceDocumentMenu = "Test";

			AssertNoErrors("should not have errors", item.OrganizationCategoryInfo);
			AssertNoErrors("should not have errors", item.NumberOfReprintAllowedInfo);
			AssertNoErrors("should not have errors", item.ComplianceDocumentMenuInfo);

			item.OrganizationCategory = "";
			AssertHasErrors("should have errors", item.OrganizationCategoryInfo);

			item.ComplianceDocumentMenu = "";
			AssertHasErrors("should have errors", item.ComplianceDocumentMenuInfo);
		}

		public void TestNumberOfReprintAllowedExceed()
		{
			var item = new ComplianceDocumentRePrintRestriction();
			item.NumberOfReprintAllowed = -1;
			var expectedMessage = @"Number of re-print allowed must be between 0 and 9.";
			AssertHasError("should have error", item.NumberOfReprintAllowedInfo, expectedMessage);

			item.NumberOfReprintAllowed = 1;
			AssertNoErrors("should not have errors", item.NumberOfReprintAllowedInfo);

			item.NumberOfReprintAllowed = 10;
			AssertHasError("should have error", item.NumberOfReprintAllowedInfo, expectedMessage);
		}

		public void TestSameOrgCategoryAndDocMenuExist()
		{
			var collection = new ComplianceDocumentRePrintRestrictionCollection();
			var item1 = collection.AddNew();
			item1.OrganizationCategory = "NGO";
			item1.NumberOfReprintAllowed = 1;
			item1.ComplianceDocumentMenu = "Test";

			var item2 = collection.AddNew();
			item2.OrganizationCategory = "NGO";
			item2.NumberOfReprintAllowed = 2;
			item2.ComplianceDocumentMenu = "Test";

			item2.RunPreSaveValidation();
			var expectedMessage = @"The same organization category and compliance document menu already exists.";
			AssertHasError("should have error", item2.ComplianceDocumentMenuInfo, expectedMessage);
		}

		public void TestOrgcategoryInvalid()
		{
			var collection = new ComplianceDocumentRePrintRestrictionCollection();
			var item1 = collection.AddNew();
			item1.OrganizationCategory = "NGO";
			item1.NumberOfReprintAllowed = 1;
			item1.ComplianceDocumentMenu = "Test";

			item1.RunPreSaveValidation();
			AssertNoErrors(item1.OrganizationCategoryInfo);

			var item2 = collection.AddNew();
			item2.OrganizationCategory = "BUS";
			item2.NumberOfReprintAllowed = 2;
			item2.ComplianceDocumentMenu = "Test";

			item2.RunPreSaveValidation();
			AssertNoErrors(item2.OrganizationCategoryInfo);

			var item3 = collection.AddNew();
			item3.OrganizationCategory = "GOV";
			item3.NumberOfReprintAllowed = 2;
			item3.ComplianceDocumentMenu = "Test";

			item3.RunPreSaveValidation();
			AssertNoErrors(item3.OrganizationCategoryInfo);

			var item4 = collection.AddNew();
			item4.OrganizationCategory = "NAT";
			item4.NumberOfReprintAllowed = 2;
			item4.ComplianceDocumentMenu = "Test";

			item4.RunPreSaveValidation();
			AssertNoErrors(item4.OrganizationCategoryInfo);

			var item5 = collection.AddNew();
			item5.OrganizationCategory = "XXX";
			item5.NumberOfReprintAllowed = 2;
			item5.ComplianceDocumentMenu = "Test";

			item5.RunPreSaveValidation();
			var expectedMessage = @"Organization category is invalid.";
			AssertHasError("should have error", item5.OrganizationCategoryInfo, expectedMessage);
		}

		#region Implementation
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var item = new ComplianceDocumentRePrintRestriction();
			item.OrganizationCategory = "NGO";
			item.NumberOfReprintAllowed = 1;
			item.ComplianceDocumentMenu = "Test";
			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected new ComplianceDocumentRePrintRestriction BizObj
		{
			get { return (ComplianceDocumentRePrintRestriction)base.BizObj; }
		}

		#endregion
	}
}
