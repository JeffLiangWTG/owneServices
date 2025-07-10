using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public class PackageVersionOverrideRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public PackageVersionOverrideRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		#region Implementation

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new PackageVersionOverrideRegistryControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		#endregion
	}
}
