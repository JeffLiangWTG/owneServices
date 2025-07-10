using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DefaultEPaymentReferenceRegistryItemEditor))]
	public class DefaultEPaymentReferenceRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DefaultEPaymentReferenceRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DefaultEPaymentReferenceControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DefaultEPaymentReferenceControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DefaultEPaymentReferenceRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var collection1 = new DefaultEPaymentReferenceCollection();
			var ref1 = collection1.AddNew();
			ref1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref1.ReferenceType = EPaymentReferenceTypes.FreeText;
			ref1.Reference = "Test";

			var collection2 = new DefaultEPaymentReferenceCollection();
			var ref2 = collection2.AddNew();
			ref2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			ref2.ReferenceType = EPaymentReferenceTypes.InvoiceNumbers;

			return new[] { collection1, collection2 };
		}
	}
}
