using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class AMSBrokerDownloadController : MQEDIMessageController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.AMSBrokerDownloadMessage;

		public override ControllerID ID => ControllerIDs.Customs.US.AMSBrokerDownloadMessages;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AMSBrokerDownloadMessagesView;
	}
}
