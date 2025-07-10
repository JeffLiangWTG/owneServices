using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(PackingForm))]
	public class PackingFormTest : PackingZFormBasherTest
	{
		#region TestFormHasDocumentPlugIn

		public void TestFormHasDocumentPlugIn()
		{
			using (var form = (PackingForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		#endregion

		#region TestPluginIsDisabledOnPackageJobDelete

		public void TestPluginIsDisabledOnPackageJobDelete()
		{
			using (var form = (PackingForm)GetFormToBash())
			{
				form.Show();
				AssertEquals("Precondition", true, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn).Enabled);

				var packageJob = (PkgPackageJob)form.DataSource;
				packageJob.Delete();
				AssertEquals("Document plugin should be disabled on delete to prevent exceptions when invoked by the user.", false, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn).Enabled);
			}
		}

		#endregion

		#region TestHandleSaveException

		[RequiresSTA]
		public void TestHandleSaveException()
		{
			using (var form = (PackingForm)GetFormToBash())
			{
				form.ValidatingForSave += delegate
				{
					throw new ZSaveException(new ZDataException(new Exception(WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID), null, null), Factory);
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
			}

			var expectedErrorMessage = WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser;
			AssertEquals("Error message is incorrect.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			Data.CreatePackingData();
			Factory.Save();

			return new PackingForm(Data.PackageJob);
		}

		#endregion
	}
}
