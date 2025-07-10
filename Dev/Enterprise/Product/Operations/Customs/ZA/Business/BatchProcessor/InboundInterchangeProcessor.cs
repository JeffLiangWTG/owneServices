using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class InboundInterchangeProcessor : Messaging.Business.InboundInterchangeProcessor
	{
		public InboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIMessage.ApplicationCodes.SouthAfricanCustoms }; }
		}

		protected override Type TypeOfInterchangeToCreate()
		{
			return typeof(ZACInterchange);
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new InboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;

		protected override bool IsNoBranchFilter => true;
		protected override bool SupportEnvironmentSwitch => true;

		#region InboundMessageCreator Class

		class InboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				GenerateMessageFromInterchange(interchange);
			}

			#region Generate Message

			void GenerateMessageFromInterchange(EDIInterchange interchange)
			{
				var interchangeText = interchange.EI_InterchangeText.Replace("\r", "").Replace("\n", "");
				var interchangeDetails = EDIInterchange.GetInterchangeDetailsFromString(interchange.Factory, interchangeText, EDIMessage.ApplicationCodes.SouthAfricanCustoms, false, true, new ZACharacterSet());
				interchange.EI_HeaderText = interchangeDetails.HeaderText;
				interchange.EI_BodyText = interchangeDetails.BodyText;
				interchange.EI_FooterText = interchangeDetails.FooterText;
				interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(false);
			}

			#endregion
		}

		#endregion
	}
}
