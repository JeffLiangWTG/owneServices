using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(SupportingDocumentFieldsLayoutBuilder))]
sealed class SupportingDocumentFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupportingDocumentFieldsLayoutBuilder, SupportingDocument, EU.GUI.PlugIn.SupportingDocumentFieldsControlBag>
{
	protected override SupportingDocumentFieldsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		return new SupportingDocumentFieldsLayoutBuilder();
	}

	protected override int ExpectedMaxColumns => 1;

	public void TestDefaultVisibilities()
	{
		const string import = EU.Business.MessageTypeList.Codes.Import;
		const string export = EU.Business.MessageTypeList.Codes.Export;

		var declaration = Factory.New<JobDeclaration>();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionDocument = instruction.SupportingDocuments.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.SupportingDocuments.AddNew();

		var invoiceLine = invoice.InvoiceLines.AddNew();
		var lineDocument = invoiceLine.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			var componentName = nameof(EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.QuantityCalcDropEdit);
			AssertNotVisible("EntryInstructuion", componentName, instructionDocument);
			AssertNotVisible("InvoiceHeder", componentName, invoiceDocument);
			AssertIsVisible("InvoiceLine", componentName, lineDocument);

			componentName = nameof(PlugIn.SupportingDocumentFieldsControlBag.ValueConvertToLocalCurrencyControl);
			AssertNotVisible("EntryInstructuion", componentName, instructionDocument, import);
			AssertNotVisible("InvoiceHeder", componentName, invoiceDocument, import);
			AssertNotVisible("InvoiceLine", componentName, lineDocument, import);
			AssertNotVisible("EntryInstructuion", componentName, instructionDocument, export);
			AssertNotVisible("InvoiceHeder", componentName, invoiceDocument, export);
			AssertIsVisible("InvoiceLine", componentName, lineDocument, export);

			componentName = nameof(EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.DocumentLineNoCalcEdit);
			AssertNotVisible("InvoiceLine", componentName, lineDocument, import);
			AssertIsVisible("InvoiceLine", componentName, lineDocument, export);

			componentName = nameof(EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.AdditionalDescriptionTextBox);
			AssertNotVisible("InvoiceLine", componentName, lineDocument, import);
			AssertIsVisible("InvoiceLine", componentName, lineDocument, export);

			componentName = nameof(PlugIn.SupportingDocumentFieldsControlBag.SupDocDescriptionTextBox);
			AssertNotVisible("InvoiceLine", componentName, lineDocument, export);
			AssertIsVisible("InvoiceLine", componentName, lineDocument, import);

			componentName = nameof(PlugIn.SupportingDocumentFieldsControlBag.SupDocReference2TextBox);
			AssertNotVisible("InvoiceLine", componentName, lineDocument, export);
			AssertIsVisible("InvoiceLine", componentName, lineDocument, import);
		});

		void AssertIsVisible(string description, string componentName, SupportingDocument doc, string declarationType = import)
			=> AssertVisibility(description, doc, componentName, declarationType, true);

		void AssertNotVisible(string description, string componentName, SupportingDocument doc, string declarationType = import)
			=> AssertVisibility(description, doc, componentName, declarationType, false);

		void AssertVisibility(string description, SupportingDocument doc, string componentName, string declarationType, bool isVisible)
		{
			using (var form = new ZForm(doc))
			using (var fieldsControl = new LayoutSupportingDocumentsFieldsControl(declaration))
			{
				declaration.JE_MessageType = declarationType;
				form.Controls.Add(fieldsControl);
				form.Show();

				var referenceNumberCodeFindBox = fieldsControl.Controls.Find(componentName, true).FirstOrDefault();
				AssertEquals($"{description}: {componentName} visibility for {declarationType}", expected: isVisible, referenceNumberCodeFindBox.Visible);
			}
		}
	}
}
