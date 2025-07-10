using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class StatementServiceTest : TestCaseWithFactory
	{
		public void TestPrint_NoContact()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			AssertNull(service.Print(Guid.NewGuid(), company.PK.ToGuid()));
		}

		public void TestPrint_NoCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			Factory.Save();

			AssertNull(service.Print(contact.PK.ToGuid(), Guid.NewGuid()));
		}

		public void TestPrint_ContactNotRelatedToCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "ORG2";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_GC = company.PK;
			companyData1.OB_OH = otherOrg.PK;
			companyData1.OB_IsDebtor = true;
			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_GC = company.PK;
			companyData2.OB_OH = org.PK;
			companyData2.OB_IsDebtor = false;
			SetupStatement(org, branch);
			Factory.Save();

			AssertNull(service.Print(contact.PK.ToGuid(), company.PK.ToGuid()));
		}

		public void TestPrint_NoTransactions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = org.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			var result = service.Print(contact.PK.ToGuid(), company.PK.ToGuid());

			AssertEquals("Cannot generate a statement because there are no transactions issued to your organisation.", result.ErrorMessage);
		}

		public void TestPrint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = org.PK;
			companyData.OB_IsDebtor = true;

			SetupStatement(org, branch);
			Factory.Save();

			var result = service.Print(contact.PK.ToGuid(), company.PK.ToGuid());

			CombineAssertions(() =>
			{
				AssertEquals("StatementOfAccount.pdf", result.FileName);
				Assert(result.FileContents.Length > 0);
			});
		}

		public void TestPrint_ControllingBranch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var controllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			controllingBranch.GB_GC = company.PK;
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GB_ControllingBranch = controllingBranch.PK;
			companyData.OB_GC = company.PK;
			companyData.OB_OH = org.PK;
			companyData.OB_IsDebtor = true;

			SetupStatement(org, controllingBranch);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, controllingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var result = service.Print(contact.PK.ToGuid(), company.PK.ToGuid());

				CombineAssertions(() =>
				{
					AssertEquals("StatementOfAccount.pdf", result.FileName);
					Assert(result.FileContents.Length > 0);
				});
			}
		}

		public void TestPrint_ControllingBranchNotInCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var controllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			controllingBranch.GB_GC = otherCompany.PK;
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = org.PK;
			companyData.OB_IsDebtor = true;
			var otherCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			otherCompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
			otherCompanyData.OB_GC = otherCompany.PK;
			otherCompanyData.OB_OH = org.PK;
			otherCompanyData.OB_IsDebtor = true;

			SetupStatement(org, branch);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, controllingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var result = service.Print(contact.PK.ToGuid(), company.PK.ToGuid());

				CombineAssertions(() =>
				{
					AssertEquals("StatementOfAccount.pdf", result.FileName);
					Assert(result.FileContents.Length > 0);
				});
			}
		}

		void SetupStatement(OrgHeader org, GlbBranch branch)
		{
			var accObjCreator = new TestObjectCreator(Factory);
			var invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org.PK;
			invoice1.AH_GB = branch.PK;
			accObjCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, invoice1.AH_ExchangeRate, 100m, 0m, 0m);

			var invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org.PK;
			invoice2.AH_GB = branch.PK;
			accObjCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 200m, 0m, 0m);

			var creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org.PK;
			creditNote1.AH_GB = branch.PK;
			accObjCreator.CreateInvoiceLine(creditNote1, creditNote1.TransactionCurrency, creditNote1.AH_ExchangeRate, -300m, 0m, 0m);
		}

		protected override void SetUp()
		{
			base.SetUp();

			service = new StatementService();
		}
		StatementService service;
	}
}
