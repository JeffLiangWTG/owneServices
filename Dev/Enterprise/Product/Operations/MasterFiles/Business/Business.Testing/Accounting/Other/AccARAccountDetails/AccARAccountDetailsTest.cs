using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccARAccountDetails))]
	sealed class AccARAccountDetailsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestA1_PaymentMethod_List_NettingDisabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var accountDetails = Header.CompanyData.ARAccountDetailsCollection.AddNew();
			var list = accountDetails.A1_PaymentMethodList;
			AssertEquals("Lookup Count should be 2", 2, list.Count);
			AssertEquals("List element should be TAX", AccARAccountDetails.ARBankAccPayment, list[0].Code);
			AssertEquals("List element should be CRQ", AccARAccountDetails.ARCollectionRequest, list[1].Code);
		}

		public void TestA1_PaymentMethod_List_NettingEnabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var accountDetails = Header.CompanyData.ARAccountDetailsCollection.AddNew();
			var list = accountDetails.A1_PaymentMethodList;

			AssertEquals("Lookup Count should be 3", 3, list.Count);
			AssertEquals("List element should be TAX", AccARAccountDetails.ARBankAccPayment, list[0].Code);
			AssertEquals("List element should be CRQ", AccARAccountDetails.ARCollectionRequest, list[1].Code);
			AssertEquals("List element should be NET", AccARAccountDetails.ARNettingBankAccount, list[2].Code);
		}

		public void TestPaymentMethod()
		{
			var accountDetails = Header.CompanyData.ARAccountDetailsCollection.AddNew();
			AssertEquals("Default PaymentMethod should be ARB", AccARAccountDetails.ARBankAccPayment, accountDetails.PaymentMethod);
		}

		public void TestReadOnlySecurityMembers()
		{
			var oldAPValue = Env.Security.OrgPayablesAccountDetailsModify.IsAllowed;
			var oldARValue = Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed;
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			var accountDetails = org.CompanyData.ARAccountDetailsCollection.AddNew();

			try
			{
				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = false;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !accountDetails.A1_PaymentMethodInfo.ReadOnly);

				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = true;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = false;
				Assert("Access Denied - ReadOnly", accountDetails.A1_PaymentMethodInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = oldAPValue;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = oldARValue;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.CompanyData.ARAccountDetailsCollection.AddNew();
		}

		OrgHeader Header;
		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.New<OrgHeader>();
			Header.CompanyData.OB_IsDebtor = true;
		}
	}
}
