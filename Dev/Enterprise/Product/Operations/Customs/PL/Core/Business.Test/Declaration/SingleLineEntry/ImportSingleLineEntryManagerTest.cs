using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ImportSingleLineEntryManagerTest : TestCaseWithFactory
{
	public void TestDefaultJI_CountryOfOrigin()
	{
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager();
		singleLineEntryManager.SingleLineEntry.InvoiceNumber = "INumber";
		singleLineEntryManager.SingleLineEntry.GoodsOrigin = Core.Constants.CountryCodes.Egypt;
		singleLineEntryManager.Execute();
		var addedInvoice = declaration.Invoices.Cast<JobComInvoiceHeader>().First();
		var addedInvoiceLine = addedInvoice.InvoiceLines.Cast<JobComInvoiceLine>().First();
		AssertEquals(Core.Constants.CountryCodes.Egypt, addedInvoiceLine.JI_CountryOfOrigin);
	}

	public void TestDefaultCusSupportingInfo()
	{
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager();
		singleLineEntryManager.SingleLineEntry.InvoiceNumber = "INumber";
		singleLineEntryManager.SingleLineEntry.PreviousDocument = "DC1";
		singleLineEntryManager.SingleLineEntry.PreviousDocumentNumber = "PrevDocNum";
		singleLineEntryManager.Execute();
		var previousDocument = declaration.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x =>
			x.CSI_LineNo == 1 && x.CSI_ReferenceNumber == "PrevDocNum" && x.CSI_Code == "DC1" && x.CSI_SubType == "Z");
		AssertNotNull(previousDocument);
	}

	(JobDeclaration, ImportSingleLineEntryManager) GetSingleLineEntryManager()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var singleLineEntryManager = new ImportSingleLineEntryManager(declaration);
		return (declaration, singleLineEntryManager);
	}
}
