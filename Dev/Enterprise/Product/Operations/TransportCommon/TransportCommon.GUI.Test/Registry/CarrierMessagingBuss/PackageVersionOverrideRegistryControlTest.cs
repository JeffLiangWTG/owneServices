using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(PackageVersionOverrideRegistryControl))]
	class PackageVersionOverrideRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestGridControlBinding()
		{
			using (var control = new PackageVersionOverrideRegistryControl())
			{
				var grid = control.Controls.Find("packageVersionOverrideGrid", searchAllChildren: false)[0] as ZGrid;

				var expectedListOfColumns = new[]
				{
					$"{PackageVersionOverride.Schema.Code} (ZTextBoxColumnStyleInfo) CharacterCasing:Upper Caption:Carrier Code",
					$"{PackageVersionOverride.Schema.AccountNumber} (ZTextBoxColumnStyleInfo) CharacterCasing:Normal Caption:Account Number",
					$"{PackageVersionOverride.Schema.PackageName} (ZTextBoxColumnStyleInfo) CharacterCasing:Normal Caption:Package Name",
					$"{PackageVersionOverride.Schema.PackageVersion} (ZTextBoxColumnStyleInfo) CharacterCasing:Normal Caption:Package Version"
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} CharacterCasing:{x.CharacterCasing} Caption:{x.CaptionResourceString.Caption}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		#region Implementation

		protected override void AssertAdditionalObjectsAreReadOnly(RegistryZUserControl control, IBusiness businessEntity, bool readOnly)
		{
			var grid = control.Controls.Find("packageVersionOverrideGrid", searchAllChildren: false)[0] as ZGrid;
			AssertEquals(readOnly, grid.ReadOnly);
		}

		protected override IBusiness GetNewBusinessEntity() => new PackageVersionOverrideCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((PackageVersionOverrideRegistryControl)control).ReadOnly;

		#endregion
	}
}
