using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class QueryMessageController : MQEDIMessageController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.QueryMessage;

		public override ControllerID ID => ControllerIDs.Customs.US.QueryMessages;

		protected override IZForm GetForm(IBusiness businessEntity) => new MQEDIMessageWithRelatedMessageDetailsForm((MQEDIMessage)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.QueryMessagesView;
	}
}
