using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLineDissectionAttribute))]
	sealed class AccTransactionLineDissectionAttributeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestALD_AttributeValueIDAndALD_AttributeValue_ReadOnly()
		{
			var accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
			accTransactionLineDissectionAttribute.ALD_Attribute = "ORG";
			AssertEquals(true, accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.ReadOnly);
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.ReadOnly);

			accTransactionLineDissectionAttribute.ALD_Attribute = "OCG";
			AssertEquals(false, accTransactionLineDissectionAttribute.ALD_AttributeValueInfo.ReadOnly);
			AssertEquals(true, accTransactionLineDissectionAttribute.ALD_AttributeValueIDInfo.ReadOnly);
		}

		public void TestApplicableAlternateChart()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			transactionLine.AL_AG = glHeader.PK;
			var chart1 = Factory.NewWithValidTestData<AccAlternateChart>();
			chart1.AAC_Code = "CH1";
			chart1.AAC_Description = "Chart one Description";
			var chart2 = Factory.NewWithValidTestData<AccAlternateChart>();
			chart2.AAC_Code = "CH2";
			chart2.AAC_Description = "Chart two Description";
			var dissection1 = glHeader.AlternateGLAccountDissections.AddNew();
			dissection1.ADC_AAC_AlternateChart = chart1.PK;
			dissection1.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			dissection1.ADC_SeparateNumbering = true;
			var dissection2 = glHeader.AlternateGLAccountDissections.AddNew();
			dissection2.ADC_AAC_AlternateChart = chart2.PK;
			dissection2.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			dissection2.ADC_SeparateNumbering = false;
			var accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
			accTransactionLineDissectionAttribute.ALD_AL_TransactionLine = transactionLine.PK;
			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;

			AssertEquals("CH1 - Chart one Description, CH2 - Chart two Description", accTransactionLineDissectionAttribute.ApplicableAlternateChart);
		}
	}
}
