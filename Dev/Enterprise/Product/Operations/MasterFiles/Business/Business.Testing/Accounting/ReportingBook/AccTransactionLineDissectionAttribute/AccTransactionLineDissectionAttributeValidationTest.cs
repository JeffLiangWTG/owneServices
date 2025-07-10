using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTransactionLineDissectionAttributeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckALD_AttributeValue()
		{
			var accTransactionLineDissectionAttribute = accountingTestObjectCreator.CreateAccTransactionLineDissectionAttribute(attribute: AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, attributeValue: "XXX");
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValue();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.HasError("Enter a valid selection."));

			accTransactionLineDissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFOCodes.FOR;
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValue();
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.HasErrors());

			accTransactionLineDissectionAttribute.ALD_AttributeValue = string.Empty;
			accTransactionLineDissectionAttribute.ALD_AL_TransactionLine = TransactionLine.PK;

			var dissection1 = accountingTestObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart1.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			var dissection2 = accountingTestObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);

			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValue();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.HasError("A value must be recorded for attribute 'LFO' as this is needed to locate the Alternate Account Number for 'CT1' Chart."));

			dissection2.ADC_SeparateNumbering = true;
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValue();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.HasError("A value must be recorded for attribute 'LFO' as this is needed to locate the Alternate Account Number for 'CT1', 'CT2' Chart."));

			dissection1.ADC_SeparateNumbering = false;
			dissection2.ADC_SeparateNumbering = false;

			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValue();
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.HasErrors());
		}

		public void TestCheckAD_AttributeValueID()
		{
			var accTransactionLineDissectionAttribute = accountingTestObjectCreator.CreateAccTransactionLineDissectionAttribute(attribute: AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, attributeValueId: Guid.NewGuid());
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValueID();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.HasError("Enter a valid selection."));

			accTransactionLineDissectionAttribute.ALD_AttributeValueID = OrgHeader.PK;
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValueID();
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.HasErrors());

			accTransactionLineDissectionAttribute.ALD_AttributeValueID = Guid.Empty;
			accTransactionLineDissectionAttribute.ALD_AL_TransactionLine = TransactionLine.PK;

			var dissection1 = accountingTestObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart1.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			var dissection2 = accountingTestObjectCreator.CreateAccAlternateGLAccountDissection(GlHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);

			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValueID();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.HasError("A value must be recorded for attribute 'ORG' as this is needed to locate the Alternate Account Number for 'CT1' Chart."));

			dissection2.ADC_SeparateNumbering = true;
			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValueID();
			Assert(accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.HasError("A value must be recorded for attribute 'ORG' as this is needed to locate the Alternate Account Number for 'CT1', 'CT2' Chart."));

			dissection1.ADC_SeparateNumbering = false;
			dissection2.ADC_SeparateNumbering = false;

			accTransactionLineDissectionAttribute.Validation.ValidateALD_AttributeValueID();
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.HasErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();

			TransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			GlHeader = Factory.NewWithValidTestData<AccGLHeader>();
			TransactionLine.AL_AG = GlHeader.PK;
			Chart1 = Factory.NewWithValidTestData<AccAlternateChart>();
			Chart1.AAC_Code = "CT1";
			Chart2 = Factory.NewWithValidTestData<AccAlternateChart>();
			Chart2.AAC_Code = "CT2";
			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();

			accountingTestObjectCreator = new AccountingTestObjectCreator(Factory);
		}

		AccTransactionLines TransactionLine;
		AccGLHeader GlHeader;
		AccAlternateChart Chart1;
		AccAlternateChart Chart2;
		OrgHeader OrgHeader;
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
