using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(US7501DocPrinting))]
	sealed class US7501DocPrintingTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<US7501DocPrinting>
	{
		public void TestDefault()
		{
			US7501DocPrinting docData = Factory.New<US7501DocPrinting>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.US7501DocPrinting, docData.B7_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = declaration.InvoiceLines.AddNew();
			invLine.JI_JZ = invHeader.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			entry.CreateDocPrintingDetails(new ZGuid());
			var docData = entry.US7501DocPrintingData[0];
			return docData;
		}
	}
}
