using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AffirmationCode))]
	sealed class AffirmationCodeTest : Customs.Business.Testing.CusCodeDataTest<AffirmationCode>
	{
		public void TestHumanReadableNameForCY_Data()
		{
			AffirmationCode uscAffirmCode = Factory.New<AffirmationCode>();
			uscAffirmCode.CY_Code = "AAA";
			AssertEquals("FDA (AAA)", uscAffirmCode.CY_DataInfo.HumanReadableName);
			uscAffirmCode.CY_Code = "";
			AssertEquals("FDA", uscAffirmCode.CY_DataInfo.HumanReadableName);
			uscAffirmCode.CY_Code = "BBB";
			AssertEquals("FDA (BBB)", uscAffirmCode.CY_DataInfo.HumanReadableName);
			AffirmationCode copyCode = Factory.New<AffirmationCode>();
			copyCode.CopyPersistentValuesFrom(uscAffirmCode);
			AssertEquals("FDA (BBB)", copyCode.CY_DataInfo.HumanReadableName);
			copyCode = (AffirmationCode)uscAffirmCode.Clone();
			AssertEquals("FDA (BBB)", copyCode.CY_DataInfo.HumanReadableName);
		}

		public void TestDescription()
		{
			USCAffirmationOfCompliance uscAffirmCode = Factory.New<USCAffirmationOfCompliance>();
			uscAffirmCode.UL_Code = "AAA";
			uscAffirmCode.UL_Description = "AAAA";
			AffirmationCode affirmationCode = Factory.New<AffirmationCode>();
			AssertEquals("Description", "", affirmationCode.Description);
			affirmationCode.CY_Code = "AAA";
			AssertEquals("Description", "AAAA", affirmationCode.Description);
		}

		public void TestCY_Data()
		{
			USCAffirmationOfCompliance uscAffirmCode = Factory.New<USCAffirmationOfCompliance>();
			uscAffirmCode.UL_Code = "COP";
			uscAffirmCode.UL_Description = "aaa";
			USCAffirmationOfCompliance uscAffirmCode2 = Factory.New<USCAffirmationOfCompliance>();
			uscAffirmCode2.UL_Code = "SFX";
			uscAffirmCode2.UL_Description = "BBBB";
			AffirmationCode affirmationCode = Factory.New<AffirmationCode>();
			affirmationCode.CY_Data = "dsfsdf";
			AssertEquals("DSFSDF", affirmationCode.CY_Data);
		}

		public void TestUSCAffirmationCode()
		{
			USCAffirmationOfCompliance uscAffirmCode = Factory.New<USCAffirmationOfCompliance>();
			uscAffirmCode.UL_Code = "AAA";
			uscAffirmCode.UL_Description = "AAAA";
			USCAffirmationOfCompliance uscAffirmCode2 = Factory.New<USCAffirmationOfCompliance>();
			uscAffirmCode2.UL_Code = "BBB";
			uscAffirmCode2.UL_Description = "BBBB";
			AffirmationCode affirmationCode = Factory.New<AffirmationCode>();
			AssertNull(affirmationCode.USCAffirmationCode);
			affirmationCode.CY_Code = "AAA";
			AssertEquals(uscAffirmCode, affirmationCode.USCAffirmationCode);
			affirmationCode.CY_Code = "BBB";
			AssertEquals(uscAffirmCode2, affirmationCode.USCAffirmationCode);
		}

		public void TestSetDefaultValues()
		{
			AffirmationCode affirmationCode = Factory.New<AffirmationCode>();
			AssertEquals("Type", CusCodeDataTypeList.Codes.AffirmationCode, affirmationCode.CY_Type);
		}

		public void TestAffirmationCodeValidation()
		{
			AffirmationCode affirmationCode = Factory.New<AffirmationCode>();
			AssertEquals("Validation", typeof(AffirmationCodeValidation), affirmationCode.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.FDAs.AddNew().AffirmationCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var affirmationCodes = new AffirmationCodeCollection(invoiceLine.FDAs.AddNew());
			return affirmationCodes.AddNew();
		}
	}
}
