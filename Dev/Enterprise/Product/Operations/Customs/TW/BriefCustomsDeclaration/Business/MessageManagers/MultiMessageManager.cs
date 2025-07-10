using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MultiMessageManager : Customs.Business.MultiMessageManager
	{
		public MultiMessageManager(MessageSendingObjectParent wrapper) : base()
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
		}

		readonly MessageSendingObjectParent wrapper;

		protected AsycudaManifestHeader ManifestHeader => (AsycudaManifestHeader)wrapper.TopLevelBusinessObject;

		public override IMessageManageableBizObj TopLevelBizObjToManage => wrapper.Header;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			return wrapper.SendingObjectsCollection.Where(x => x.ShouldSend).Select(x => new MessageManager(x)).ToArray();
		}

		public bool SendMessages(ISendsMessagesToCustoms sender)
		{
			var result = SendMessagesWithoutSaving(sender);
			if (result)
			{
				wrapper.Factory.Save();
			}
			return result;
		}

		bool SendMessagesWithoutSaving(ISendsMessagesToCustoms sender)
		{
			Initialise();
			var result = false;
			var singleMessageManagers = AllMessageManagers;
			if (singleMessageManagers.Length > 0)
			{
				result = SendOriginal(sender, singleMessageManagers).Any();
			}
			return result;
		}
	}
}
