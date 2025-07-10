using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CreditorCollection))]
	sealed class CreditorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new CreditorCollection(new BusinessObjectFactory(), orgDefaults);
		}

		public void TestCollectionReturnsCreditor()
		{
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_IsCreditor = true;

			OrgHeader notCreditor = Factory.NewWithValidTestData<OrgHeader>();
			notCreditor.CompanyData.OB_IsCreditor = false;

			Factory.Save();

			CreditorCollection collection = new CreditorCollection(new BusinessObjectFactory());
			AssertEquals("Collection.Count", 0, collection.Count);

			collection.Load();

			Assert("Collection.Count > 0", collection.Count > 0);
			Assert("Collection contains Creditor", collection.Contains(creditor.PK));
			Assert("Collection does not contain NotCreditor", !collection.Contains(notCreditor.PK));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Creditors.AddNew();
			AssertEquals("Creditor is selected", true, org1.CompanyData.OB_IsCreditor);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = false;

			OrgHeader org2 = Creditors.AddNew();
			AssertEquals("Creditor is not selected", false, org2.CompanyData.OB_IsCreditor);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Creditors.AddNew();
			using (organisation.SuspendValidationTesting())
			using (organisation.CompanyData.SuspendValidationTesting())
			{
				organisation.OH_IsCreditor = false;
				Creditors.ValidateEntityOnSaving(organisation);
				Assert("Error - Creditor not selected", organisation.OH_IsCreditorInfo.HasErrors());

				organisation.OH_IsCreditor = true;
				Creditors.ValidateEntityOnSaving(organisation);
				Assert("No error - Creditor selected", !organisation.OH_IsCreditorInfo.HasErrors());
			}
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Creditors.AllowNewTemporaryOrganisations);
		}

		public void TestIgnoreCurrentCompanyInFilter()
		{
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var myOrgCompanyData = creditor.CompanyDataCollection.AddNew();
			myOrgCompanyData.OB_IsCreditor = false;
			myOrgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var otherOrgCompanyData = creditor.CompanyDataCollection.AddNew();
			otherOrgCompanyData.OB_IsCreditor = true;
			otherOrgCompanyData.OB_GC = otherCompany.PK;

			Factory.Save();

			AssertNotEquals("Precondition: must have a current company", ZGuid.Empty, GlbCompany.CurrentCompany.PK);

			CreditorCollection collection = new CreditorCollection(new BusinessObjectFactory());
			collection.Load();
			Assert("collection has flag disabled by default ==> no creditor", !collection.Contains(creditor.PK));

			collection = new CreditorCollection(new BusinessObjectFactory(), ignoreCurrentCompanyInFilter: true);
			collection.Load();
			Assert("collection has flag enabled ==> finds creditor that is valid in otherCompany", collection.Contains(creditor.PK));
		}

		#region Implementation

		CreditorCollection Creditors;

		protected override void SetUp()
		{
			base.SetUp();
			Creditors = new CreditorCollection(new BusinessObjectFactory());
		}

		#endregion
	}
}
