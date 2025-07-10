using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ValidationProviderTest : TestCaseWithFactory
	{
		public void TestValidateFormat()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			gNF = list.AddNew();
			gNF.NumberFormat = "3-2-2";
			gNF.CountryCode = ZString.Empty;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = false;

			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			Factory.Save();

			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;

			descriptor1.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.AJ_LocalAccountNumber = "44442201";

			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "4444221";

			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [8]"));

			descriptor1.AJ_LocalAccountNumber = "44442211";
			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "444422113";
			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [8]"));

			descriptor1.AJ_RN_NKCountryOfCompliance = null;
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.AJ_LocalAccountNumber = "33311";
			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "3";
			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [3 or 5 or 7]"));

			descriptor1.AJ_LocalAccountNumber = "333";
			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "33";
			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [3 or 5 or 7]"));

			descriptor1.AJ_LocalAccountNumber = "3332211";
			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "3332";
			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [3 or 5 or 7]"));

			descriptor1.AJ_LocalAccountNumber = "33311";
			AssertEquals("AccountNumber should have no error.", false, descriptor1.AccountNumInfo.HasErrors());

			descriptor1.AJ_LocalAccountNumber = "333221";
			Assert(descriptor1.AccountNumInfo.HasError("GL Account Number Length invalid. Please enter in the following Length: [3 or 5 or 7]"));
		}

		public void TestCheckAccountNumberAlreadyUsed()
		{
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			AccGLHeader accGLHeader = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader accGLHeader1 = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			descriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			descriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			descriptor1.AJ_LocalAccountNumber = "8888.888";
			descriptor2.AJ_LocalAccountNumber = "9999.888";
			descriptor1.AJ_RN_NKCountryOfCompliance = null;
			descriptor2.AJ_RN_NKCountryOfCompliance = null;
			descriptor1.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor2.AJ_Language = Constants.Languages.ChineseSimplified;
			descriptor1.ParentGLHeaderPK = accGLHeader.PK;
			Factory.Save();

			descriptor2.ParentGLHeaderPK = accGLHeader1.PK;
			AssertEquals("AccountNumber should have no error.", false, descriptor2.AccountNumInfo.HasErrors());

			descriptor2.ParentGLHeaderPK = accGLHeader.PK;
			Assert(descriptor2.ParentGLHeaderPKInfo.HasError("Please choose another GL Account as this one is already referenced by another Local Account for the current language."));

			descriptor2.ParentGLHeaderPK = accGLHeader1.PK;
			descriptor2.AJ_LocalAccountNumber = "8888.888";
			Assert(descriptor2.AccountNumInfo.HasError("Please enter another account number as this one is already used by another Local account"));

			descriptor2.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.Australia;
			descriptor2.ParentGLHeaderPK = accGLHeader1.PK;
			descriptor2.AJ_LocalAccountNumber = "8888.888";
			AssertEquals("AccountNumber should have no error.", false, descriptor2.AccountNumInfo.HasErrors());
		}

		public void TestThisAccountReferredByMoreThanOneAccount()
		{
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			AccGLAccountDescriptor accountToBeReferred = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;

			descriptor1.AJ_AJ_CarriedForwardAccount = accountToBeReferred.PK;
			descriptor2.AJ_AJ_CarriedForwardAccount = accountToBeReferred.PK;

			AccGLAccountDescriptorValidationHelper validatorHelper = new AccGLAccountDescriptorValidationHelper(descriptor2, "", "", Factory);
			Assert(validatorHelper.ColumnReferredByMoreThanOneAccount(AccGLAccountDescriptorSchema.AJ_AJ_CarriedForwardAccount, accountToBeReferred.PK));
		}

		public void TestThisAccountNotReferredByAnyAccount()
		{
			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			AccGLAccountDescriptor accountToBeReferred = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;

			descriptor2.AJ_AJ_CarriedForwardAccount = accountToBeReferred.PK;

			AccGLAccountDescriptorValidationHelper validatorHelper = new AccGLAccountDescriptorValidationHelper(descriptor2, "", "", Factory);

			Assert(!validatorHelper.ColumnReferredByMoreThanOneAccount(AccGLAccountDescriptorSchema.AJ_AJ_CarriedForwardAccount, accountToBeReferred.PK));
		}

		public void TestValidateAccountNumberFormat()
		{
			AccGLAccountDescriptor testDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			testDescriptor.AJ_LocalAccountNumber = "123";
			AssertEquals("Should have no error. AJ_LocalAccountNumber can be any format", false, testDescriptor.AJ_LocalAccountNumberInfo.HasErrors());

			testDescriptor.AJ_LocalAccountNumber = "1234.5678.90";
			AssertEquals("Should have no error. AJ_LocalAccountNumber can be any format", false, testDescriptor.AJ_LocalAccountNumberInfo.HasErrors());
		}
	}
}
