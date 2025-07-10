using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.GUI;

public class FallbackRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public FallbackRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	: base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane()
	{
		return new FallbackControl();
	}

	protected override EditorPaneAnchor Anchor
	{
		get { return EditorPaneAnchor.All; }
	}
}
