using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(PreviousDocumentMaster))]
sealed class PreviousDocumentMasterTest	 : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentException>("When factory is null", () => new PreviousDocumentMaster(null, Mock.Of<IPreviousDocumentsProvider>()));
		AssertExceptionThrown<ArgumentException>("When parent is null", () => new PreviousDocumentMaster(Factory, null));

		var previousDocumentProviderMock = Mock.Of<IPreviousDocumentsProvider>();
		AssertExceptionThrown<ArgumentException>("When parent.PreviousDocuments is null", () => new PreviousDocumentMaster(Factory, previousDocumentProviderMock));

		AssertSame("PreviousDocuments", previousDocumentProviderMock.PreviousDocuments, Mock.Of<IPreviousDocumentsProvider>().PreviousDocuments);
	});

	public void TestCSI_Procedure()
	{
		var entryInstruction = Factory.New<JobDeclaration>()
			.CustomsEntryInstructions.AddNew();

		var previousDocuments = entryInstruction.PreviousDocuments;
		var documentMaster = new PreviousDocumentMaster(Factory, entryInstruction);

		CombineAssertions("With No Previous Documents are present", () =>
		{
			documentMaster.CSI_Procedure = "123";
			AssertEquals("Document Count", 1, previousDocuments.Count);
			AssertEquals("CSI_Procedure", "123", previousDocuments[0].CSI_Procedure);
		});

		previousDocuments.AddNew().CSI_Procedure = "456";
		AssertEquals("Previous Documents Count after adding new document", 2, previousDocuments.Count);

		CombineAssertions("With 2 Previous Documents and resetting the CSI_Procedure on Document Master", () =>
		{
			documentMaster.CSI_Procedure = "555";
			AssertEquals("Document Count", 1, previousDocuments.Count);
			AssertEquals("CSI_Procedure", "555", previousDocuments[0].CSI_Procedure);
		});
	}

	public void TestCSI_Procedure_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<PreviousDocumentMaster>()
			.HasProperty(p => p.CSI_Procedure)
			.WithCaption("Previous Procedure")
			.WithAttribute<MaxLengthAttribute>(m => m.MaxLength == 7)
			.WithAttribute<ListAttribute>(l => l.ListDataSourceMember == $"{nameof(PreviousDocumentMaster.Lookups)}.{nameof(PreviousDocumentMasterLookups.ProcedureList)}");
	});

	public void TestLookups() => CombineAssertions(() =>
	{
		var document = CreatePreviousDocumentMaster();
		AssertType<PreviousDocumentMasterLookups>(document.Lookups);
		AssertSame(document.Lookups, document.Lookups);
	});

	public void TestValidation_Type()
	{
		var document = CreatePreviousDocumentMaster();
		AssertType<PreviousDocumentMasterValidation>(document.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => CreatePreviousDocumentMaster();

	PreviousDocumentMaster CreatePreviousDocumentMaster()
	{
		var entryInstruction = Factory.New<JobDeclaration>()
			.CustomsEntryInstructions.AddNew();
		return new PreviousDocumentMaster(Factory, entryInstruction);
	}
}
