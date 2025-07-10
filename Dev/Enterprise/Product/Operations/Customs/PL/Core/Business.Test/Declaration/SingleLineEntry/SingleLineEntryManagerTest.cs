using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using SupportingDocument = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class SingleLineEntryManagerTest : TestCaseWithFactory
{
	public void TestDefaultSupportingDocument_Export()
	{
		const string invoiceNumber = "ABCD";
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager(MessageTypeList.Codes.Export);
		singleLineEntryManager.SingleLineEntry.InvoiceNumber = invoiceNumber;
		singleLineEntryManager.Execute();

		declaration.SupportingDocuments.Load();

		var supportingDocument = declaration.Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())
			.First(x => x.CSI_Code == EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380);

		AssertEquals("ReferenceNumber", invoiceNumber, supportingDocument.CSI_ReferenceNumber);
	}

	public void TestDefaultSupportingDocument_Import()
	{
		const string invoiceNumber = "ABCD";
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager(MessageTypeList.Codes.Import);
		singleLineEntryManager.SingleLineEntry.InvoiceNumber = invoiceNumber;
		singleLineEntryManager.Execute();

		var supportingDocument = declaration.Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())
			.First(x => x.CSI_Code == EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935);

		AssertEquals("ReferenceNumber", invoiceNumber, supportingDocument.CSI_ReferenceNumber);
	}

	public void TestDefaultEntryInstruction()
	{
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager();
		singleLineEntryManager.SingleLineEntry.CPCCode = "4000";
		singleLineEntryManager.Execute();

		var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault(x =>
			x.CEI_DateForDuty == ZDateTime.Today && x.CEI_Procedure == "40" && x.CEI_SubStyle == SubStyleCodes.A);

		AssertNotNull("Entry instruction exists", entryInstruction);
	}

	public void TestDefaultInstruction_PK()
	{
		var (declaration, singleLineEntryManager) = GetSingleLineEntryManager();
		singleLineEntryManager.SingleLineEntry.CPCCode = "4000";
		singleLineEntryManager.Execute();

		var entryInstruction = declaration.CustomsEntryInstructions.First(x =>
			x.CEI_DateForDuty == ZDateTime.Today && x.CEI_Procedure == "40" && x.CEI_SubStyle == SubStyleCodes.A);

		var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().First();
		AssertEquals("InvoiceLine has instruction assigned", entryInstruction.PK, invoiceLine.JI_CEI);
	}

	public void TestDefaultCPC_Export()
	{
		var (_, singleLineEntryManager) = GetSingleLineEntryManager(MessageTypeList.Codes.Export);
		AssertEquals("1000", singleLineEntryManager.SingleLineEntry.CPCCode);
	}

	public void TestDefaultCPC_Import()
	{
		var (_, singleLineEntryManager) = GetSingleLineEntryManager(MessageTypeList.Codes.Import);
		AssertEquals("4000", singleLineEntryManager.SingleLineEntry.CPCCode);
	}

	(JobDeclaration, SingleLineEntryManager) GetSingleLineEntryManager(string messageType = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (messageType != null)
		{
			declaration.JE_MessageType = messageType;
		}

		var singleLineEntryManager = new SingleLineEntryManager(declaration);
		return (declaration, singleLineEntryManager);
	}
}
