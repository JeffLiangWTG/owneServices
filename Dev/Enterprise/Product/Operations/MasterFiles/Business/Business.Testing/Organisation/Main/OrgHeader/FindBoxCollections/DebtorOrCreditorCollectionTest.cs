using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DebtorOrCreditorCollection))]
	sealed class DebtorOrCreditorCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new DebtorOrCreditorCollection(new BusinessObjectFactory(), orgDefaults);
		}

		public void TestCollectionReturnsDebtor()
		{
			OrgHeader creditorAndDebtor = Factory.NewWithValidTestData<OrgHeader>();
			creditorAndDebtor.CompanyData.OB_IsCreditor = true;
			creditorAndDebtor.CompanyData.OB_IsDebtor = true;

			OrgHeader creditorOnly = Factory.NewWithValidTestData<OrgHeader>();
			creditorOnly.CompanyData.OB_IsCreditor = true;
			creditorOnly.CompanyData.OB_IsDebtor = false;

			OrgHeader debtorOnly = Factory.NewWithValidTestData<OrgHeader>();
			debtorOnly.CompanyData.OB_IsCreditor = false;
			debtorOnly.CompanyData.OB_IsDebtor = true;

			OrgHeader neither = Factory.NewWithValidTestData<OrgHeader>();
			neither.CompanyData.OB_IsCreditor = false;
			neither.CompanyData.OB_IsDebtor = false;

			Factory.Save();

			DebtorOrCreditorCollection collection = new DebtorOrCreditorCollection(new BusinessObjectFactory());
			AssertEquals("Collection.Count", 0, collection.Count);

			collection.Load();

			Assert("Collection.Count > 0", collection.Count > 0);
			Assert("Collection contains CreditorAndDebtor", collection.Contains(creditorAndDebtor.PK));
			Assert("Collection contains CreditorOnly", collection.Contains(creditorOnly.PK));
			Assert("Collection contains DebtorOnly", collection.Contains(debtorOnly.PK));
			Assert("Collection does not contain Neither", !collection.Contains(neither.PK));
		}

		public void TestValidateEntityOnSaving()
		{
			var org = Debtors.AddNew();
			org.OH_IsDebtor = false;
			org.OH_IsCreditor = false;
			using (org.SuspendValidationTesting())
			using (org.CompanyData.SuspendValidationTesting())
			{
				Debtors.ValidateEntityOnSaving(org);
				AssertHasError(org.OH_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertHasError(org.OH_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");

				org.OH_IsDebtor = true;
				Debtors.ValidateEntityOnSaving(org);
				AssertNoError(org.OH_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsDebtorInfo);
				AssertNoError(org.OH_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsCreditorInfo);

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = true;
				Debtors.ValidateEntityOnSaving(org);
				AssertNoError(org.OH_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsDebtorInfo);
				AssertNoError(org.OH_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsCreditorInfo);

				org.OH_IsDebtor = true;
				Debtors.ValidateEntityOnSaving(org);
				AssertNoError(org.OH_IsDebtorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsDebtorInfo);
				AssertNoError(org.OH_IsCreditorInfo, "An Organization selected from here must have an Organization type of either Receivables or Payables selected.");
				AssertNoErrors(org.OH_IsCreditorInfo);
			}
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(Debtors.AllowNewTemporaryOrganisations);
		}

		#region Implementation

		DebtorOrCreditorCollection Debtors;

		protected override void SetUp()
		{
			base.SetUp();
			Debtors = new DebtorOrCreditorCollection(new BusinessObjectFactory());
		}

		#endregion
	}
}
