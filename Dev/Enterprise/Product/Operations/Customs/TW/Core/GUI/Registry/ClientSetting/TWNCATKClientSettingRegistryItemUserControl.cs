using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TWNCATKClientSettingRegistryItemUserControl : RegistryZUserControl
	{
		public TWNCATKClientSettingRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MachineNameTextBox.ReadOnly = SendFolderTextBox.ReadOnly = RunningIntervalInSecondsCalcEdit.ReadOnly = readOnly;
		}
	}
}
