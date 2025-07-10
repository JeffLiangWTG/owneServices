using System.Linq;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(AdditionalInfosUserControlLayoutBuilder))]
sealed class AdditionalInfosUserControlLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalInfosUserControlLayoutBuilder, AdditionalInfo, AdditionalInformationDetailsControlBag>
{
	protected override AdditionalInfosUserControlLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		return new AdditionalInfosUserControlLayoutBuilder();
	}

	protected override int ExpectedMaxColumns => 2;

	public void TestDefaultVisibilities()
	{
		const string import = EU.Business.MessageTypeList.Codes.Import;
		const string export = EU.Business.MessageTypeList.Codes.Export;

		var declaration = Factory.New<JobDeclaration>();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionDocument = instruction.AdditionalInfos.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.AdditionalInfos.AddNew();

		var invoiceLine = invoice.InvoiceLines.AddNew();
		var lineDocument = invoiceLine.AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			var invoiceLineBindingMember = "FilteredInvoiceLines.AdditionalInfos";
			var invoiceHeaderBindingMember = "Invoices.AdditionalInfos";
			var entryInstructionBindingMember = "CustomsEntryInstructions.AdditionalInfos";
			var componentName = nameof(EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.KindDropEdit);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, export);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, export);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, export);
			AssertNotVisible("EntryInstruction", componentName, entryInstructionBindingMember, import);
			AssertNotVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, import);
			AssertNotVisible("InvoiceLine", componentName, invoiceLineBindingMember, import);

			componentName = nameof(EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.FullTypeCodeFindBox);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, export);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, export);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, export);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, import);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, import);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, import);

			componentName = nameof(EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.ReferenceTextBox);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, export);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, export);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, export);
			AssertNotVisible("EntryInstruction", componentName, entryInstructionBindingMember, import);
			AssertNotVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, import);
			AssertNotVisible("InvoiceLine", componentName, invoiceLineBindingMember, import);

			componentName = nameof(EU.GUI.PlugIn.AdditionalInformationDetailsControlBag.DescriptionTextBox);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, export);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, export);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, export);
			AssertIsVisible("EntryInstruction", componentName, entryInstructionBindingMember, import);
			AssertIsVisible("InvoiceHeader", componentName, invoiceHeaderBindingMember, import);
			AssertIsVisible("InvoiceLine", componentName, invoiceLineBindingMember, import);
		});

		void AssertIsVisible(string description, string componentName, string dataMemberName, string declarationType = import)
			=> AssertVisibility(description, componentName, dataMemberName, declarationType, true);

		void AssertNotVisible(string description, string componentName, string dataMemberName, string declarationType = import)
			=> AssertVisibility(description, componentName, dataMemberName, declarationType, false);

		void AssertVisibility(string description, string componentName, string dataMemberName, string declarationType, bool isVisible)
		{
			using (var form = new ZForm())
			using (var control = new PlugIn.AdditionalInfosUserControlWithGrid())
			{
				declaration.JE_MessageType = declarationType;
				form.Controls.Add(control);
				control.Grid.DataMember = dataMemberName;
				control.SetDataBinding(declaration, string.Empty);
				form.Show();

				var additionalInfosPanel = control.Controls.Find("AdditionalInfosPanel", true).First();
				var additionalInfosGroupBox = additionalInfosPanel.Controls.Find("AdditionalInfosGroupBox", true).First();
				var detailsLayoutControl = additionalInfosGroupBox.Controls.Find("DetailsLayoutControl", true).First();
				var detailsPanel = detailsLayoutControl.Controls.Find("DetailsPanel", true).First();
				var component = detailsPanel.Controls.Find(componentName, true).First();
				AssertEquals($"{description}: {componentName} visibility for {declarationType}", expected: isVisible, component.Visible);
			}
		}
	}
}
