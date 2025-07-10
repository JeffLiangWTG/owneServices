using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DefaultOSMGControl : RegistryZUserControl
	{
		public DefaultOSMGControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var defaultOSMG = BoundBusinessObject as DefaultOSMG;
			if (defaultOSMG != null && defaultOSMG.CurrentFallbackLevel != null && defaultOSMG.CurrentFallbackLevel.Level != Enterprise.Integration.RegistryStorageFlags.System)
			{
				ActionGroupBox.Visible = false;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OSMGGuidFindBox.ReadOnly = readOnly;
			BulkUpdateButton.ReadOnly = readOnly;
		}

		void BulkUpdateButton_Click(object sender, EventArgs e)
		{
			var bulkUpdateOSMGForm = new OSMGBulkUpdateForm(new OSMGBulkUpdater());
			ZFormModaliser.ShowDialogAndDispose(bulkUpdateOSMGForm);
		}
	}
}
