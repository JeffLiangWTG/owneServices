using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(PackageVersionOverrideRegistryItemEditor))]
	class PackageVersionOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
			=> RegistryItemEditor.EditorPaneAnchor.All;

		protected override RegistryItemEditor GetEditor()
			=> new PackageVersionOverrideRegistryItemEditor(new PackageVersionOverrideDataType(), null, Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane)
			=> !((PackageVersionOverrideRegistryControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType()
			=> typeof(PackageVersionOverrideRegistryControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new PackageVersionOverrideRegistryItem("TEST_REGISTRY_ITEM", null, null, null,
				RegistryStorageFlags.System, new PackageVersionOverrideCollection(), RegistryOptions.IsOnlyForSupport);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new PackageVersionOverrideCollection();
			collection.Add(new PackageVersionOverride()
			{
				Code = "ZZABC",
				PackageName = "Pkg1",
				PackageVersion = "1.0.1",
				AccountNumber = "131824"
			});
			collection.Add(new PackageVersionOverride()
			{
				Code = "ZZABC",
				PackageName = "Pkg1",
				PackageVersion = "1.0.3",
				AccountNumber = "235667"
			});
			collection.Add(new PackageVersionOverride()
			{
				Code = "ZZPQR",
				PackageName = "Pkg2",
				PackageVersion = "2.0.0",
				AccountNumber = "131824"
			});
			collection.Add(new PackageVersionOverride()
			{
				Code = "ZZPQR",
				PackageName = "Pkg2",
				PackageVersion = "2.0.0",
				AccountNumber = "235667"
			});
			return new[] { collection };
		}

		#endregion
	}
}
