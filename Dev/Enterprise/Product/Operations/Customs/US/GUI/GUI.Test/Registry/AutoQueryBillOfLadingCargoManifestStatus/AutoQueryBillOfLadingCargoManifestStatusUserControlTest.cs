using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoQueryBillOfLadingCargoManifestStatusUserControl))]
	sealed class AutoQueryBillOfLadingCargoManifestStatusUserControlTest : RegistryZUserControlTestCase
	{
		public void TestPostGroupBoxEnabled()
		{
			AutoQueryBillOfLadingCargoManifestStatus query = new AutoQueryBillOfLadingCargoManifestStatus();
			using (ZForm form = new ZForm())
			using (AutoQueryBillOfLadingCargoManifestStatusUserControl control = new AutoQueryBillOfLadingCargoManifestStatusUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(query, null);
				control.ReadOnly = true;
				AssertEquals(false, control.SendBasedOnETACheckBox.Enabled);
				AssertEquals(false, control.SendOnFirstSaveCheckBox.Enabled);
				AssertEquals(false, control.UpdateEntryWithResultsCheckBox.Enabled);
				control.ReadOnly = false;
				AssertEquals(true, control.SendBasedOnETACheckBox.Enabled);
				AssertEquals(true, control.SendOnFirstSaveCheckBox.Enabled);
				AssertEquals(false, control.UpdateEntryWithResultsCheckBox.Enabled);
				query.SendOnFirstSave = true;
				AssertEquals(true, control.UpdateEntryWithResultsCheckBox.Enabled);
			}

			using (ZForm form = new ZForm())
			using (AutoQueryBillOfLadingCargoManifestStatusUserControl control = new AutoQueryBillOfLadingCargoManifestStatusUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(query, null);
				AssertEquals(true, control.SendBasedOnETACheckBox.Enabled);
				AssertEquals(true, control.SendOnFirstSaveCheckBox.Enabled);
				AssertEquals(true, control.UpdateEntryWithResultsCheckBox.Enabled);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new AutoQueryBillOfLadingCargoManifestStatus();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var statusControl = (AutoQueryBillOfLadingCargoManifestStatusUserControl)control;
			return !statusControl.SendBasedOnETACheckBox.Enabled || !statusControl.SendOnFirstSaveCheckBox.Enabled;
		}
	}
}
