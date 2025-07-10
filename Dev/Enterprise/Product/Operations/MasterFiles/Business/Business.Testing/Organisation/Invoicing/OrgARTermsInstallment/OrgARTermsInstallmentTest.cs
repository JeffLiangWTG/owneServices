using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARTermsInstallment))]
	sealed class OrgARTermsInstallmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals(arTerms.PK, arTermsInstallment.ML_PY_Terms);
			AssertEquals((ZByte)arTerms.ARTermsInstallments.Count, arTermsInstallment.ML_SequenceNumber);
			AssertEquals((ZByte)0, arTermsInstallment.ML_DaysFromInvoiceDate);
			AssertEquals(Guid.Empty, arTermsInstallment.ML_P5_TermsCycle);
			AssertEquals(arTerms.PY_AgreedPaymentMethod, arTermsInstallment.ML_AgreedPaymentMethod);
		}

		public void TestML_SequenceNumber_ReadOnly()
		{
			AssertEquals(arTermsInstallment.ML_SequenceNumberInfo.ReadOnly, true);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This method currently performs a saving process on default values that fire DB Exceptions", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			arTerms = org.CompanyData.ARTerms.AddNew();
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			arTermsInstallment = arTerms.ARTermsInstallments.AddNew();
		}

		OrgARTerms arTerms;

		OrgARTermsInstallment arTermsInstallment;
	}
}
