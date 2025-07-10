using System;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	internal partial class DpsStatusUpdateSettingRegistryControl : RegistryZUserControl
	{
		public DpsStatusUpdateSettingRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			DpsStatusUpdateOptionEdit.ReadOnly = readOnly;
			PhasesGrid.ReadOnly = readOnly;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var setting = DataSource as DpsStatusUpdateSetting;
			setting.OptionInfo.ValueChanged += OptionInfo_ValueChanged;
			OptionInfo_ValueChanged(setting, null);
		}

		void OptionInfo_ValueChanged(object sender, EventArgs e)
		{
			PhasesGrid.Visible = (sender as DpsStatusUpdateSetting).Option != DpsStatusUpdateOptions.Codes.DAB;
		}
	}
}
