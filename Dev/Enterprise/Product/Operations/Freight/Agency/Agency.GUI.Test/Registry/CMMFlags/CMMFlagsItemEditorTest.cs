using System;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(CMMFlagsRegistryItemEditor))]
	internal class CMMFlagsItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CMMFlagsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CMMFlagsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CMMFlagsRegistryItem("", null, null, null, RegistryStorageFlags.System, new CMMFlags());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CMMFlagsRegistryItemEditor(new CMMFlagsRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CMMFlags() };
		}
		#endregion
	}
}
