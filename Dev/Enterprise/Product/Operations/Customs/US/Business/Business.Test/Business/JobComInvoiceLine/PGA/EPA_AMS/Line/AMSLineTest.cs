using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSLine))]
	class AMSLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<AMSLine>
	{
		public void TestUS_CertTypeMaxLength()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertEquals(1, amsLine.US_CertTypeInfo.MaxLength);
			amsHeader.US_Program = AMSProgramList.Codes.OR1;
			amsLine = amsHeader.AMSLines.AddNew();
			AssertEquals(AutoUSAMSLineAddInfo.Schema.US_CertTypeMaxLength, amsLine.US_CertTypeInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_CommercialDescription = "AMS";
			var result = ams.AMSLines.AddNew();
			result.US_ProductNumber = "SD";
			return result;
		}

		public void TestIAMSLineCertType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR2;
			var amsline = ams.AMSLines.AddNew();
			amsline.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			AssertEquals(LPCOTransactionTypeList.Codes.Continuous, ((IAMSLine)amsline).CertType);
			ams.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals(ZString.Empty, ((IAMSLine)amsline).CertType);
			ams.US_Program = AMSProgramList.Codes.MO2;
			AssertEquals(LPCOTransactionTypeList.Codes.Continuous, ((IAMSLine)amsline).CertType);
		}
	}
}
