using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgTaxConfigurationFilterControl))]
	sealed class OrgTaxConfigurationFilterControlTest : ZFormBasherForControlTest
	{
		public void TestStatusDropEditCharacterCasing()
		{
			using (var control = GetControlForTest())
			{
				var statusDropEdit = control.GetControl<ZDropEdit>("statusDropEdit");
				AssertEquals("Must match related lookup character casing", CharacterCasing.Normal, statusDropEdit.CharacterCasing);
			}
		}

		protected override IBusiness GetBusinessEntityForBinding() => new OrgTaxConfigurationModuleFilter("Tax Configuration");

		protected override Control GetControlForTest() => new OrgTaxConfigurationFilterControl();

		public override Type FormToBashType => typeof(ZChildForm);
	}
}
