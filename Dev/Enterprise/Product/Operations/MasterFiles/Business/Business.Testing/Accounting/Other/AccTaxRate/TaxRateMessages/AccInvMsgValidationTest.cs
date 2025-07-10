using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccInvMsgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCodeMandatory()
		{
			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_Code = "";
			AssertHasErrors(message.A9_CodeInfo);

			message.A9_Code = "Code 1";
			AssertNoErrors(message.A9_EnglishMsgInfo);
		}

		public void TestCheckEnglishMessageMandatory()
		{
			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_EnglishMsg = "";
			AssertHasErrors(message.A9_EnglishMsgInfo);

			message.A9_EnglishMsg = "ENGLISH MESSAGE";
			AssertNoErrors(message.A9_EnglishMsgInfo);
		}

		public void TestCheckLocalMessageNotMandatory()
		{
			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_LocalMsg = "";
			AssertNoErrors(message.A9_LocalMsgInfo);

			message.A9_LocalMsg = "LOCAL MESSAGE";
			AssertNoErrors(message.A9_LocalMsgInfo);
		}

		public void TestAccInvMsgIsUnique()
		{
			AccInvMsg message1 = Factory.New<AccInvMsg>();
			AccInvMsg message2 = Factory.New<AccInvMsg>();
			message1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message1.A9_Code = "CODE";
			message2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			message2.A9_Code = "CODE";
			AssertHasError("Tax Message with same code and country should have error.", message2.A9_CodeInfo, "An Invoice Tax Message with this code already exists. Please enter a new Code.");
			AccInvMsg message3 = Factory.New<AccInvMsg>();
			message3.A9_RN_NKCountryCode = "US";
			message3.A9_Code = "CODE";
			AssertNoErrors("Tax Message with same code and different country should not have error.", message3.A9_CodeInfo);
		}

		public void TestCheckA9_TaxGroupCodeMandatory()
		{
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			testValue.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, testValue);

			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_TaxGroupCode = "";
			AssertHasErrors(message.A9_TaxGroupCodeInfo);

			message.A9_TaxGroupCode = "N1";
			AssertNoErrors(message.A9_TaxGroupCodeInfo);
		}

		public void TestNoRegistryValuesTaxGroupCodeEmpty()
		{
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, testValue);

			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_TaxGroupCode = "";
			AssertNoErrors(message.A9_TaxGroupCodeInfo);
		}

		public void TestRegistryWithValuesTaxGroupCodeEmpty()
		{
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			testValue.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, testValue);

			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_TaxGroupCode = "";
			AssertHasErrors(message.A9_TaxGroupCodeInfo);
		}

		public void TestRegistryWithValuesTaxGroupCodeValid()
		{
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			testValue.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, testValue);

			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_TaxGroupCode = "N1";
			AssertNoErrors(message.A9_TaxGroupCodeInfo);
		}

		public void TestRegistryWithValuesTaxGroupCodeInvalid()
		{
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			testValue.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, testValue);

			AccInvMsg message = Factory.New<AccInvMsg>();
			message.A9_TaxGroupCode = "XX";
			AssertHasErrors(message.A9_TaxGroupCodeInfo);
		}
	}
}
