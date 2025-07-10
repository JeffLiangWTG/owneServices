using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed class CMMFlagsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CMMFlagsRegistryItemEditor(CMMFlagsRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CMMFlagsControl();
		}
	}
}


