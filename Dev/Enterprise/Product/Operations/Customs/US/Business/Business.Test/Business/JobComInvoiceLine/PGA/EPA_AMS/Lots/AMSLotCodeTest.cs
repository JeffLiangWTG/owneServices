using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSLotCode))]
	public class AMSLotCodeTest : Customs.Business.Testing.CusCodeDataTest<AMSLotCode>
	{
		public void TestCY_CodeMaxLength()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			var lotCode = amsHeader.LotCodes.AddNew();
			AssertEquals(1, lotCode.CY_CodeInfo.MaxLength);
			amsHeader.US_Program = AMSProgramList.Codes.OR1;
			lotCode = amsHeader.LotCodes.AddNew();
			AssertEquals(AutoCusCodeData.Schema.CY_CodeMaxLength, lotCode.CY_CodeInfo.MaxLength);
		}

		public void TestProperties()
		{
			var amsLotCode = AMSLotCodes.AddNew();
			AssertEquals("Type", CusCodeDataTypeList.Codes.AMSLotCode, amsLotCode.CY_Type);
			AssertEquals("Validation", typeof(AMSLotCodeValidation), amsLotCode.Validation.GetType());
			amsLotCode.CY_Code = "KNZ";
			AssertEquals("AML (KNZ)", amsLotCode.CY_DataInfo.HumanReadableName);

			amsLotCode.CY_Code = "";
			AssertEquals("AML", amsLotCode.CY_DataInfo.HumanReadableName);

			amsLotCode.CY_Code = LotNumberQualifierList.Codes._3;
			AssertEquals("Description", LotNumberQualifierList.Descriptions._3, amsLotCode.Description);
			amsLotCode.CY_Data = "testams";
			AssertEquals("TESTAMS", amsLotCode.CY_Data);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AMSLotCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().AMSLines.AddNew().AMSLines.AddNew().LotCodes.AddNew();
		}

		AMSLotCodeCollection AMSLotCodes
		{
			get
			{
				if (amsLotCodeCollection == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var ams = invoiceLine.AMSLines.AddNew();
					ams.US_Program = AMSProgramList.Codes.PN1;
					var amsLine = ams.AMSLines.AddNew();
					amsLotCodeCollection = amsLine.LotCodes;
				}
				return amsLotCodeCollection;
			}
		}
		AMSLotCodeCollection amsLotCodeCollection;
	}
}
