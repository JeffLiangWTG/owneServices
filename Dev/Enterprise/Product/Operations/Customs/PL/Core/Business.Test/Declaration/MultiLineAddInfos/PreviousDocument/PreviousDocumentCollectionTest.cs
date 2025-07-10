using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocumentCollection))]
class PreviousDocumentCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments;
	}

	public void TestChildDefaults()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
		AssertEquals(ExportPreviousDocSubTypeList.Codes.STD, previousDocument.CSI_SubType);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var previousDocument2 = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
		AssertEquals(ZString.Empty, previousDocument2.CSI_SubType);
	}
}
