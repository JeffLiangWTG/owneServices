using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARTermsInstallmentCollection))]
	sealed class OrgARTermsInstallmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewElementReadonly()
		{
			OrgARTermsInstallmentCollection collection = (OrgARTermsInstallmentCollection)GetCollectionToTest();
			AssertEquals("Collection readonly", false, collection.ReadOnly);
			AssertEquals("New element readonly", false, collection.AddNew().ReadOnly);

			collection.SetReadOnlyIncludingChildren(true);
			AssertEquals("Collection readonly", true, collection.ReadOnly);
			AssertEquals("New element readonly", true, collection.AddNew().ReadOnly);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(OrgARTermsInstallmentCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgARTermsInstallmentCollection(defaultTerms);
		}

		OrgHeader organisation;
		OrgCompanyData companyData;
		OrgARTerms defaultTerms;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			companyData = organisation.CompanyData;
			companyData.OB_IsDebtor = true;

			defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", InvoiceTermsList.FromInvoiceDate.Code);
		}

		void SetupTermsInfo(OrgARTerms term, ZString jobType, ZGuid branchPK, ZGuid deptPK, ZString direction, ZString transportMode, ZString invoiceType, ZString invoiceTerm)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
			}
		}
	}
}
