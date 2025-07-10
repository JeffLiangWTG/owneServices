using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class GOVGIOMessageManager : EDIFACTMessageManager
	{
		public GOVGIOMessageManager(GOVGIOMessageHeader source, IMessageNotificationCollector notification) : base(source, new EDIFACTStatusCalculator(SARSEDIMessage.MessageTypeNames.GOVGIO), notification)
		{
			this.source = Argument.NotNull(source, nameof(source));
		}

		public override string MessageFriendlyName => "GOVGIO";

		protected override bool ShouldSendMessagesInTestMode => Env.Registry.ZACustoms.GetIsTestMode(Env.CurrentBranch);

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new GOVGIOMessageBuilder(source, actionCode);
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			if (source.ManifestHeader.GateInOutMessageType.IsEmpty)
			{
				messageText = Res.GetString("7469d1c1-db54-4ac5-b10a-79c805d98915", "Gate In/Out Message Type is required.");
				return false;
			}

			if (!source.ManifestHeader.Lookups.GateInOutMessageTypeList.ContainsCode(source.ManifestHeader.GateInOutMessageType))
			{
				messageText = Res.GetString("7051796a-4a7c-4bd6-9d7e-f7eb7c2402ef", "'{0}' is not a valid Gate In/Out Message Type.", source.ManifestHeader.GateInOutMessageType);
				return false;
			}

			return base.CanSendThisMessage(actionCode, out messageText);
		}

		public new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		readonly GOVGIOMessageHeader source;
	}
}
