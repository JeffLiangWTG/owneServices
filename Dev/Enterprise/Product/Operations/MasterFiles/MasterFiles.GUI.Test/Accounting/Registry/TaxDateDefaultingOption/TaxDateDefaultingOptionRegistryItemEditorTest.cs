using System;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaxDateDefaultingOptionRegistryItemEditor))]
	sealed class TaxDateDefaultingOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new TaxDateDefaultingOptionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TaxDateDefaultingOptionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TaxDateDefaultingOptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TaxDateDefaultingOptionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new TaxDateDefaultingOptionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new TaxDateDefaultingOptionCollection();
			var copy = collection.AddNew();

			copy.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			copy.Mode = Constants.TransportModes.Rail;
			copy.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			copy.Ledger = LedgerTypes.AccountsReceivable;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
