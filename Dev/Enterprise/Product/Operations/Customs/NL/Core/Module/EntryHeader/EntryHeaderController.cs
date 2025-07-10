using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NL.Module;

public class EntryHeaderController : EU.Module.EntryHeaderController
{
	public override ControllerID ID => ControllerIDs.Customs.NL.EntryHeader;
}
