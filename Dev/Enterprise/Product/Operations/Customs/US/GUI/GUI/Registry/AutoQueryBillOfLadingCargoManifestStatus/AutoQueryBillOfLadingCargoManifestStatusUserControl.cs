using System;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class AutoQueryBillOfLadingCargoManifestStatusUserControl : RegistryZUserControl
	{
		public AutoQueryBillOfLadingCargoManifestStatusUserControl()
		{
			InitializeComponent();

			UpdateEntryWithResultsCheckBox.Enabled = false;

			SendOnFirstSaveCheckBox.CheckedChanged += new EventHandler(SendOnFirstSaveCheckBox_CheckedChanged);
		}

		void SendOnFirstSaveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateEntryWithResultsCheckBox.Enabled = SendOnFirstSaveCheckBox.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			SendBasedOnETACheckBox.Enabled = !readOnly;
			SendOnFirstSaveCheckBox.Enabled = !readOnly;
		}
	}
}
