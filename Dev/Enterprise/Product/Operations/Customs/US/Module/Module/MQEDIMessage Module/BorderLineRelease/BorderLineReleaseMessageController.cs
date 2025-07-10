using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class BorderLineReleaseMessageController : MQEDIMessageController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.BorderLineReleaseMessage;

		public override ControllerID ID => ControllerIDs.Customs.US.BorderLineReleaseMessage;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BorderLineReleaseMessageView;
	}
}
