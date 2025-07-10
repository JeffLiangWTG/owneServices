using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DebtorCollection))]
	sealed class DebtorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new DebtorCollection(new BusinessObjectFactory(), orgDefaults);
		}

		public void TestCollectionReturnsDebtor()
		{
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;

			OrgHeader notDebtor = Factory.NewWithValidTestData<OrgHeader>();
			notDebtor.CompanyData.OB_IsDebtor = false;

			Factory.Save();

			DebtorCollection collection = new DebtorCollection(new BusinessObjectFactory());
			AssertEquals("Collection.Count", 0, collection.Count);

			collection.Load();

			Assert("Collection.Count > 0", collection.Count > 0);
			Assert("Collection contains Debtor", collection.Contains(debtor.PK));
			Assert("Collection does not contain NotDebtor", !collection.Contains(notDebtor.PK));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Debtors.AddNew();
			AssertEquals("Debtor is selected", true, org1.CompanyData.OB_IsDebtor);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = false;

			OrgHeader org2 = Debtors.AddNew();
			AssertEquals("Debtor is not selected", false, org2.CompanyData.OB_IsDebtor);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Debtors.AddNew();
			organisation.OH_IsDebtor = false;
			using (organisation.SuspendValidationTesting())
			using (organisation.CompanyData.SuspendValidationTesting())
			{
				Debtors.ValidateEntityOnSaving(organisation);
				Assert("Error - Debtor not selected", organisation.OH_IsDebtorInfo.HasErrors());

				organisation.OH_IsDebtor = true;
				Debtors.ValidateEntityOnSaving(organisation);
				Assert("No error - Debtor selected", !organisation.OH_IsDebtorInfo.HasErrors());
			}
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Debtors.AllowNewTemporaryOrganisations);
		}

		#region Implementation

		DebtorCollection Debtors;

		protected override void SetUp()
		{
			base.SetUp();
			Debtors = new DebtorCollection(new BusinessObjectFactory());
		}

		#endregion
	}
}
