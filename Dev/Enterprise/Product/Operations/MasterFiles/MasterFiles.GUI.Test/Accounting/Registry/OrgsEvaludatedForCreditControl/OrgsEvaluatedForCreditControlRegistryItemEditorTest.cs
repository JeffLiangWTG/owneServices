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
	[TestedType(typeof(OrgsEvaluatedForCreditControlRegistryItemEditor))]
	sealed class OrgsEvaluatedForCreditControlRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgsEvaluatedForCreditControlRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgsEvaluatedForCreditControlControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgsEvaluatedForCreditControlControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OrgsEvaluatedForCreditControlRegistryItem("", null, null, null, RegistryStorageFlags.System, new OrgsEvaluatedForCreditControlCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgsEvaluatedForCreditControlCollection collection = new OrgsEvaluatedForCreditControlCollection();
			OrgsEvaluatedForCreditControl copy = collection.AddNew();

			copy.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			copy.Mode = Constants.TransportModes.Rail;
			copy.OrganizationType = AccountingMasterFilesConstants.OrganisationTypeCodes.LocalClient;
			copy.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
			copy.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
