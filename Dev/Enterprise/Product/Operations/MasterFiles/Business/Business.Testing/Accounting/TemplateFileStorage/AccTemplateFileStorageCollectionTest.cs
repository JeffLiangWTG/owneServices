using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTemplateFileStorageCollection))]
	sealed class AccTemplateFileStorageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionReturnsCorrectSet()
		{
			using (Company.TemporarilySetCountry(CountryCodes.Turkey))
			{
				var template1 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
				template1.TFS_GC = Company.PK;
				template1.TFS_ExternalReference = ZGuid.NewZGuid();

				var template2 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
				template2.TFS_GC = Company.PK;
				template2.TFS_ExternalReference = ZGuid.NewZGuid();

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(CountryCodes.Turkey);

				var template3 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
				template3.TFS_GC = company.PK;
				template3.TFS_ExternalReference = ZGuid.NewZGuid();

				Factory.Save();

				var collection = GetCollectionToTest();
				collection.Load();
				AssertEquals(2, collection.Count);
				Assert(collection.Contains(template1));
				Assert(collection.Contains(template2));
				Assert(!collection.Contains(template3));

				collection.RemoveAndDeleteAll();
				Factory.Save();
				collection.Load();
				AssertEquals(0, collection.Count);
			}
		}

		public void TestCollectionDeleteIsNotAllowed()
		{
			var template1 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			template1.TFS_GC = Company.PK;
			template1.TFS_ExternalReference = ZGuid.NewZGuid();

			var template2 = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			template2.TFS_GC = Company.PK;
			template2.TFS_ExternalReference = ZGuid.NewZGuid();

			var config = Factory.NewWithValidTestData<AccEInvoicingTemplateFileView>();
			config.ETF_GC = Company.PK;
			config.ETF_TemplateCode = template2.TFS_Code;
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Load();
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(template1));
			Assert(collection.Contains(template2));

			Assert("Template1 can be deleted.", template1.CanDelete);
			collection.RemoveAndDelete(template1);
			Assert("Template2 cannot be deleted.", !template2.CanDelete);
			collection.RemoveAndDelete(template2);
			AssertExceptionThrown(typeof(ZSaveException), () => Factory.Save());
		}

		public void TestCheckForOrganizationsUsingSelectedTemplate()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var template = Factory.NewWithValidTestData<AccTemplateFileStorage>();
			template.TFS_GC = Company.PK;
			template.TFS_ExternalReference = ZGuid.NewZGuid();
			Factory.Save();

			var configOrg = Factory.NewWithValidTestData<AccEInvoicingTemplateFileView>();
			configOrg.ETF_GC = Company.PK;
			configOrg.ETF_TemplateCode = template.TFS_Code;
			configOrg.ETF_ParentID = organisation.PK;
			configOrg.ETF_ParentTableCode = "OH";
			Factory.Save();

			template.TFS_IsActive = false;
			var expectedErrorMessageForOrganization = $@"The XSLT template is still being used by at least one organization or company: 
{organisation.OH_Code} - {organisation.OH_FullName}";
			AssertHasError(template.TFS_IsActiveInfo, expectedErrorMessageForOrganization);

			template.TFS_IsActive = true;

			var configCompany = Factory.NewWithValidTestData<AccEInvoicingTemplateFileView>();
			configCompany.ETF_GC = Company.PK;
			configCompany.ETF_TemplateCode = template.TFS_Code;
			configCompany.ETF_ParentID = ZGuid.Empty;
			configCompany.ETF_ParentTableCode = "";
			Factory.Save();

			template.TFS_IsActive = false;
			var expectedErrorMessageForOrganizationAndCompany = $@"The XSLT template is still being used by at least one organization or company: 
{Company.GC_Code} - {Company.GC_Name}
{organisation.OH_Code} - {organisation.OH_FullName}";
			AssertHasError(template.TFS_IsActiveInfo, expectedErrorMessageForOrganizationAndCompany);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Company = Factory.NewWithValidTestData<GlbCompany>();
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AccTemplateFileStorageCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTemplateFileStorageCollection(Company);
		}

		GlbCompany Company;
	}
}
