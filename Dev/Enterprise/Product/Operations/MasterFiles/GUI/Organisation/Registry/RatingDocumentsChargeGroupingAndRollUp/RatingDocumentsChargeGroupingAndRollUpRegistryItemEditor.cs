using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	class RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new RatingDocumentsChargeGroupingAndRollUpControl();
	}
}