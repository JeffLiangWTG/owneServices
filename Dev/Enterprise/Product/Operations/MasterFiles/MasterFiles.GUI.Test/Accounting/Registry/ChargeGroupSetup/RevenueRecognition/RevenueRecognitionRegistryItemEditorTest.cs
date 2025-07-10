using System;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RevenueRecognitionRegistryItemEditor))]
	sealed class RevenueRecognitionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new RevenueRecognitionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RevenueRecognitionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RevenueRecognitionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RevenueRecognitionRegistryItem("", null, null, null, RegistryStorageFlags.System, new RevenueRecognitionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			RevenueRecognitionCollection collection = new RevenueRecognitionCollection();
			RevenueRecognition copy = collection.AddNew();

			copy.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			copy.Mode = Constants.TransportModes.Rail;
			copy.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
