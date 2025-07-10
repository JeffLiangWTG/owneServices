using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAPAccountDetails))]
	sealed class AccAPAccountDetailsTest : EnterpriseBusinessObjectTestCase
	{
		#region IReadOnlySecurity

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					fOrgInDB = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		public void TestReadOnlySecurityMembers()
		{
			var oldAPValue = Env.Security.OrgPayablesModify.IsAllowed;
			var oldARValue = Env.Security.OrgReceivablesModify.IsAllowed;
			var accDetails = OrgInDB.CompanyData.AccountDetailsCollection.AddNew();

			try
			{
				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = true;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !accDetails.A1_AccountNameInfo.ReadOnly);

				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = false;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = true;
				Assert("Access Denied - ReadOnly", accDetails.A1_AccountNameInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesModify.IsAllowed = oldAPValue;
				Env.Security.OrgReceivablesModify.IsAllowed = oldARValue;
			}
		}

		#endregion

		public void TestEPaymentReferenceIsUpdatedWhenChangingEPaymentReferenceType()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var chequeAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
				chequeAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				chequeAccountDetail.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;
				chequeAccountDetail.A1_EPaymentReference = "EPayment Reference";

				chequeAccountDetail.A1_EPaymentReferenceType = EPaymentReferenceTypes.PaymentReferenceNum;
				AssertNullOrEmpty(chequeAccountDetail.A1_EPaymentReference);
			}
		}

		public void TestEPaymentReferencesIsUpdatedWhenChangingPaymentMethod()
		{
			var collection = new DefaultEPaymentReferenceCollection();
			var reference = collection.AddNew();
			reference.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			reference.Reference = "Test Reference";

			using (AccountingMasterFilesRegistry.Instance.DefaultPaymentReference.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEPaymentReferenceIsUpdatedWhenChangingPaymentMethod(true, EPaymentReferenceTypes.FreeText, "Test Reference");
				AssertEPaymentReferenceIsUpdatedWhenChangingPaymentMethod(false, ZString.Empty, ZString.Empty);
			}

			void AssertEPaymentReferenceIsUpdatedWhenChangingPaymentMethod(bool enableEPaymentFunctionality, ZString expectedReferenceType, ZString expectedReference)
			{
				using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, enableEPaymentFunctionality))
				{
					var chequeAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
					AssertNullOrEmpty(chequeAccountDetail.A1_EPaymentReference);
					AssertNullOrEmpty(chequeAccountDetail.A1_EPaymentReferenceType);

					chequeAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
					AssertEquals(expectedReferenceType, chequeAccountDetail.A1_EPaymentReferenceType);
					AssertEquals(expectedReference, chequeAccountDetail.A1_EPaymentReference);

					chequeAccountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
					AssertNullOrEmpty(chequeAccountDetail.A1_EPaymentReference);
					AssertNullOrEmpty(chequeAccountDetail.A1_EPaymentReferenceType);
				}
			}
		}

		public void TestA1_EPaymentReference_ReadOnly()
		{
			AssertA1_EPaymentReference_ReadOnly(true, false);
			AssertA1_EPaymentReference_ReadOnly(false, true);

			void AssertA1_EPaymentReference_ReadOnly(bool enableEPaymentFunctionality, bool shouldReadonly)
			{
				using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, enableEPaymentFunctionality))
				{
					var accountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();

					accountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
					AssertEquals(true, accountDetail.A1_EPaymentReferenceInfo.ReadOnly);

					accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
					AssertEquals(true, accountDetail.A1_EPaymentReferenceInfo.ReadOnly);

					accountDetail.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;
					AssertEquals(shouldReadonly, accountDetail.A1_EPaymentReferenceInfo.ReadOnly);
				}
			}
		}

		public void TestA1_EPaymentReferenceType_ReadOnly()
		{
			AssertA1_EPaymentReferenceType_ReadOnly(true, false);
			AssertA1_EPaymentReferenceType_ReadOnly(false, true);

			void AssertA1_EPaymentReferenceType_ReadOnly(bool enableEPaymentFunctionality, bool shouldReadonly)
			{
				using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, enableEPaymentFunctionality))
				{
					var accountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();

					accountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
					Assert(accountDetail.A1_EPaymentReferenceTypeInfo.ReadOnly);

					accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
					AssertEquals(shouldReadonly, accountDetail.A1_EPaymentReferenceTypeInfo.ReadOnly);
				}
			}
		}

		public void TestPropertyReadOnlyness()
		{
			Assert_Property_ReadOnly(true, true, EPaymentMethods.EPaymentViaOFX);
			Assert_Property_ReadOnly(false, false, ReceiptTypes.Cheque);
			Assert_Property_ReadOnly(false, true, EPaymentMethods.EPaymentViaOFX);
			Assert_Property_ReadOnly(true, false, ReceiptTypes.Cheque);

			void Assert_Property_ReadOnly(bool enableEPaymentFunctionality, bool shouldReadonly, string paymentMethod)
			{
				using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, enableEPaymentFunctionality))
				{
					var accountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
					accountDetail.A1_PaymentMethod = paymentMethod;
					AssertEquals(shouldReadonly, accountDetail.A1_PaymentMethodInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_RX_NKAccountCurrencyInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_AccountNameInfo.ReadOnly);
					AssertEquals("User should always be allowed to tick/untick the 'Default' checkbox - form validation is handled thereafter.", false, accountDetail.A1_IsDefaultAccountInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_BankNameInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_BankSwiftInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_BankBsbInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_BankAccountInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_IBANNumberInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_RN_NKCountryCodeInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_SystemCreateUserInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_SystemCreateTimeUtcInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_SystemLastEditUserInfo.ReadOnly);
					AssertEquals(shouldReadonly, accountDetail.A1_SystemLastEditTimeUtcInfo.ReadOnly);
				}
			}
		}

		public void TestPaymentReasonIsUpdatedWhenChangingPaymentMethod()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var chequeAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
				chequeAccountDetail.A1_IsDefaultAccount = true;
				chequeAccountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert("When payment method is not EPO, payment reason field should be empty.", chequeAccountDetail.A1_EPaymentReasonCode.IsEmpty);
				var ofxAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
				ofxAccountDetail.A1_IsDefaultAccount = true;
				ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AssertEquals("When payment method is EPO, payment reason field should be defaulted from 'Default Payment Reason' registry value.", EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, ofxAccountDetail.A1_EPaymentReasonCode);
				ofxAccountDetail.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				Header.OH_Code = "TSTORG12";
				Factory.Save();

				var newFactory1 = new BusinessObjectFactory();
				var reloadedChequeAccountDetail1 = newFactory1.Load<AccAPAccountDetails>(chequeAccountDetail.PK);
				Assert("Existing Account Detail Payment Reason should not be updated when loading it in new factory.", reloadedChequeAccountDetail1.A1_EPaymentReasonCode.IsEmpty);
				reloadedChequeAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AssertEquals("When existing account detail payment method is changed to EPO, payment reason field should be defaulted from 'Default Payment Reason' registry value.", EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, reloadedChequeAccountDetail1.A1_EPaymentReasonCode);
				var reloadedOFXAccountDetail1 = newFactory1.Load<AccAPAccountDetails>(ofxAccountDetail.PK);
				AssertEquals("Existing Account Detail Payment Reason should not be updated when loading it in new factory.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedOFXAccountDetail1.A1_EPaymentReasonCode);
				reloadedOFXAccountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert("When existing account detail payment method is changed from EPO to CHQ, payment reason field should be empty.", reloadedOFXAccountDetail1.A1_EPaymentReasonCode.IsEmpty);
				reloadedOFXAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AssertEquals("When existing account detail payment method is changed back to EPO from CHQ, original payment reason value should be populated.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedOFXAccountDetail1.A1_EPaymentReasonCode);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				var chequeAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
				chequeAccountDetail.A1_IsDefaultAccount = true;
				chequeAccountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert("When E-Payment Functionality is disabled, payment reason field should be empty.", chequeAccountDetail.A1_EPaymentReasonCode.IsEmpty);
				var ofxAccountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
				ofxAccountDetail.A1_IsDefaultAccount = true;
				ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				Assert("When E-Payment Functionality is disabled, payment reason field should be empty.", ofxAccountDetail.A1_EPaymentReasonCode.IsEmpty);
				ofxAccountDetail.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices; //mimick account detail created before disabling E-Payment functionality
				Header.OH_Code = "TSTORG12";
				Factory.Save();

				var newFactory1 = new BusinessObjectFactory();
				var reloadedChequeAccountDetail1 = newFactory1.Load<AccAPAccountDetails>(chequeAccountDetail.PK);
				Assert("When E-Payment Functionality is disabled, payment reason is not updated.", reloadedChequeAccountDetail1.A1_EPaymentReasonCode.IsEmpty);
				reloadedChequeAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				Assert("When E-Payment Functionality is disabled, payment reason is not updated.", reloadedChequeAccountDetail1.A1_EPaymentReasonCode.IsEmpty);
				var reloadedOFXAccountDetail1 = newFactory1.Load<AccAPAccountDetails>(ofxAccountDetail.PK);
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedOFXAccountDetail1.A1_EPaymentReasonCode);
				reloadedOFXAccountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedOFXAccountDetail1.A1_EPaymentReasonCode);
				reloadedOFXAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedOFXAccountDetail1.A1_EPaymentReasonCode);
			}
		}

		public void TestA1_EPaymentReasonCodeReadOnlyness()
		{
			var accountDetail = Header.CompanyData.AccountDetailsCollection.AddNew();
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				accountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert("When payment method is not EPO, payment reason field should not be enabled.", accountDetail.A1_EPaymentReasonCodeInfo.ReadOnly);
				accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				Assert("When payment method is EPO, payment reason field should be enabled.", !accountDetail.A1_EPaymentReasonCodeInfo.ReadOnly);
			}
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				accountDetail.A1_PaymentMethod = ReceiptTypes.Cheque;
				Assert("When E-Payment Functionality is disabled, payment reason field should not be enabled.", accountDetail.A1_EPaymentReasonCodeInfo.ReadOnly);
				accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				Assert("When E-Payment Functionality is disabled, payment reason field should not be enabled.", accountDetail.A1_EPaymentReasonCodeInfo.ReadOnly);
			}
		}

		public void TestSetAccountDetailsValuesFromBeneficiary()
		{
			var accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();
			AssertEquals(ZGuid.Empty, accountDetails.A1_EPaymentBeneficiaryId);
			AssertNull(accountDetails.EPaymentBeneficiary);

			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			accountDetails.SetAccountDetailsValuesFromBeneficiary(beneficiary);
			AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_AccountName.MaxLength, beneficiary.ABF_BeneficiaryFullName), accountDetails.A1_AccountName);
			AssertEquals(beneficiary.ABF_RX_NKAccountCurrency, accountDetails.A1_RX_NKAccountCurrency);
			AssertEquals(beneficiary.ABF_RN_NKCountryCode, accountDetails.A1_RN_NKCountryCode);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankName.MaxLength, beneficiary.ABF_BankName), accountDetails.A1_BankName);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankBranchName.MaxLength, beneficiary.ABF_BankBranchName), accountDetails.A1_BankBranchName);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankBsb.MaxLength, beneficiary.ABF_BankBsb), accountDetails.A1_BankBsb);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankAccount.MaxLength, beneficiary.ABF_BankAccount), accountDetails.A1_BankAccount);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankSwift.MaxLength, beneficiary.ABF_BankSwift), accountDetails.A1_BankSwift);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress1.MaxLength, beneficiary.ABF_BankAddress1), accountDetails.A1_BankAddress1);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress2.MaxLength, beneficiary.ABF_BankAddress2), accountDetails.A1_BankAddress2);
			AssertEquals(AccAPAccountDetails.NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress3.MaxLength, beneficiary.ABF_BankAddress3), accountDetails.A1_BankAddress3);
		}

		public void TestEPaymentBeneficiary()
		{
			var accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();
			AssertEquals(ZGuid.Empty, accountDetails.A1_EPaymentBeneficiaryId);
			AssertNull(accountDetails.EPaymentBeneficiary);

			var beneficiary = Factory.New<AccEPaymentBeneficiary>();
			accountDetails.A1_EPaymentBeneficiaryId = beneficiary.PK;
			AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
			AssertEquals(beneficiary.PK, accountDetails.EPaymentBeneficiary.PK);
		}

		public void TestReadOnlyness()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				Assert(!accountDetails.ReadOnly);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				var accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.Empty;
				Assert(!accountDetails.ReadOnly);

				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;
				Assert(!accountDetails.ReadOnly);
			}
		}

		public void TestGetDefaultPaymentMethod()
		{
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			AssertEquals("Should return InputPaymentMethod", ZArchitecture.Core.ReceiptTypes.Cheque, AccAPAccountDetails.GetDefaultPaymentMethod(ZArchitecture.Core.ReceiptTypes.Cheque));
			AssertEquals("Should return value from Registry", ZArchitecture.Core.ReceiptTypes.DirectDebit, AccAPAccountDetails.GetDefaultPaymentMethod(AccAPAccountDetailsLookups.DefaultPayment));
		}

		public void TestPaymentMethod()
		{
			AccAPAccountDetails accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			accountDetails.A1_PaymentMethod = AccAPAccountDetailsLookups.DefaultPayment;
			AssertEquals("Default PaymentMethod should be Direct Debit", ZArchitecture.Core.ReceiptTypes.DirectDebit, accountDetails.PaymentMethod);
			AssertEquals("Default A1_PaymentMethod should be Default", AccAPAccountDetailsLookups.DefaultPayment, accountDetails.A1_PaymentMethod);

			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Default PaymentMethod should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, accountDetails.PaymentMethod);
			AssertEquals("Default A1_PaymentMethod should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, accountDetails.A1_PaymentMethod);
		}

		public void TestAutoDirectDebit()
		{
			AccAPAccountDetails accountDetails = Header.CompanyData.AccountDetailsCollection.AddNew();

			accountDetails.A1_AccountName = "AccoutName";
			accountDetails.A1_BankBsb = "2168456";
			accountDetails.A1_BankAccount = "ANZ";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("Should be Auto Direct Debit", accountDetails.AutoDirectDebit);

			accountDetails.A1_AccountName = ZString.Empty;
			Assert("Should NOT be Auto Direct Debit, AccountName is Empty", !accountDetails.AutoDirectDebit);

			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_BankBsb = ZString.Empty;
			Assert("Should NOT be Auto Direct Debit, BankBsb is Empty", !accountDetails.AutoDirectDebit);

			accountDetails.A1_BankBsb = "216845";
			accountDetails.A1_BankAccount = ZString.Empty;
			Assert("Should NOT be Auto Direct Debit, BankAccount is Empty", !accountDetails.AutoDirectDebit);

			accountDetails.A1_BankAccount = "BankAccount";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Should NOT be Auto Direct Debit, PaymentMethod is not DDR", !accountDetails.AutoDirectDebit);

			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			accountDetails.A1_PaymentMethod = AccAPAccountDetailsLookups.DefaultPayment;
			Assert("Should be Auto Derict Debit, Default in Registry Item is DDR", accountDetails.AutoDirectDebit);
		}

		public void TestA1_IsDefaultAccount()
		{
			AccAPAccountDetails accountDetails1 = Header.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			AccAPAccountDetails accountDetails2 = Header.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			accountDetails1.A1_IsDefaultAccount = true;
			Assert("Account Details 1 should be default.", accountDetails1.A1_IsDefaultAccount);
			AssertNoErrors("Account Details 1 should have no errors", accountDetails1.A1_IsDefaultAccountInfo);

			accountDetails2.A1_IsDefaultAccount = true;
			Assert("Account Details 2 should be default.", accountDetails2.A1_IsDefaultAccount);
			AssertHasErrors("Account Details 2 should have errors, another Account exists with the same payment and curreny", accountDetails2.A1_IsDefaultAccountInfo);
			AssertHasErrors("Account Details 1 should have errors, another Account exists with the same payment and curreny", accountDetails1.A1_IsDefaultAccountInfo);
		}

		public void TestIsDataVersionsAutoLogged()
		{
			AssertEquals("Data versions auto logging should be always on.", true, ((IDataVersionLoggingSupported)GetNewBusinessObject()).IsDataVersionsAutoLogged);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.CompanyData.AccountDetailsCollection.AddNew();
		}

		OrgHeader Header;
		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.New<OrgHeader>();
			Header.CompanyData.OB_IsCreditor = true;
		}
	}
}
