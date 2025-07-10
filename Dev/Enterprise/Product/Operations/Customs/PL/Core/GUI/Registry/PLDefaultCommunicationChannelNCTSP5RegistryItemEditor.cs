using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.GUI.Registry;

public class PLDefaultCommunicationChannelNCTSP5RegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public PLDefaultCommunicationChannelNCTSP5RegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new PLDefaultCommunicationChannelNCTSP5Control();
}
