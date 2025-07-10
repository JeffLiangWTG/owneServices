using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(DpsStatusUpdateSettingRegistryControl))]
	class DpsStatusUpdateSettingRegistryControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestPhasesGridVisibleWhenOptionChanges()
		{
			var setting = new DpsStatusUpdateSetting();
			using (var form = new ZForm(setting))
			using (var control = new DpsStatusUpdateSettingRegistryControl())
			{
				form.Show();
				form.Controls.Add(control);

				control.DpsStatusUpdateOptionEdit.OnItemSelected(setting.OptionList[DpsStatusUpdateOptions.Codes.PHS], true);
				Assert("The grid is visible", control.PhasesGrid.Visible);

				control.DpsStatusUpdateOptionEdit.OnItemSelected(setting.OptionList[DpsStatusUpdateOptions.Codes.COM], true);
				Assert("The grid is visible", control.PhasesGrid.Visible);

				control.DpsStatusUpdateOptionEdit.OnItemSelected(setting.OptionList[DpsStatusUpdateOptions.Codes.DAB], true);
				Assert("The grid is invisible again", !control.PhasesGrid.Visible);
			}
		}

		#region Overrides
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DpsStatusUpdateSetting();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DpsStatusUpdateSettingRegistryControl)control).PhasesGrid.ReadOnly;
		}
		#endregion
	}
}
