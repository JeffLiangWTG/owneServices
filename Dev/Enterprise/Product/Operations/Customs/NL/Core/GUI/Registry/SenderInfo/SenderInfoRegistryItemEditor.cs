using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.GUI;

public class SenderInfoRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public SenderInfoRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane()
	{
		return new SenderInfoUserControl();
	}

	protected override EditorPaneAnchor Anchor
	{
		get { return EditorPaneAnchor.All; }
	}
}
