using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class CALINFMessageManager : EDIFACTMessageManager
	{
		public CALINFMessageManager(CALINFMessageData source, IMessageNotificationCollector notification)
			: base(source, new EDIFACTStatusCalculator(SARSEDIMessage.MessageTypeNames.CALINF), notification)
		{
			this.source = Argument.NotNull(source, nameof(source));
		}

		public override string MessageFriendlyName => SARSEDIMessage.MessageTypeNames.CALINF;

		protected override bool ShouldSendMessagesInTestMode => Env.Registry.ZACustoms.GetIsTestMode(Env.CurrentBranch);

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new CALINFMessageBuilder(source, actionCode);
		}

		public new JobVoyage BusinessObject => (JobVoyage)base.BusinessObject;

		readonly CALINFMessageData source;
	}
}
