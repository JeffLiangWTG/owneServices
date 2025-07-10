using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DIS.Business
{
	/// <summary>
	/// Package Queued EDIMessages into EDIInterchanges
	/// </summary>
	public class OutgoingMessageProcessor : Enterprise.Messaging.Business.MessageProcessor.OutgoingMessageProcessor
	{
		public OutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new InterchangeProvider(readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get
			{
				return messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsDIS));
			}
		}
		ZQuery messageFilter;

		class InterchangeProvider : Customs.Business.BatchProcessor.OneInterchangeToOneMessageInterchangeProvider
		{
			public InterchangeProvider(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			#region Overrides

			protected override System.Type InterchangeType
			{
				get { return typeof(EDIInterchange); }
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, Enterprise.Messaging.Business.EDIInterchange interchange)
			{
				var message = messages[0];
				var recipient = "USC";
				var sender = "";

				var messageContent = XElement.Parse(message.EM_MessageText);
				var messageHeader = messageContent.Elements().FirstOrDefault(x => x.Name.LocalName == "MessageHeader");

				if (messageHeader != null)
				{
					var preparer = messageHeader.Elements().FirstOrDefault(x => x.Name.LocalName == "PreparerID");
					sender = preparer != null ? preparer.Value : string.Empty;
				}
				SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, recipient, sender);
			}

			protected override CargoWise.Types.ZString GetInterchangeFooter(int messageCount)
			{
				return "";
			}

			protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message)
			{
				return DoNotCollateType;
			}

			protected override string InstructionHowToSetInterchangeSenderID
			{
				get { return "Registry > Customs > United States of America > Import > ABI > Entry Filer"; }
			}

			#endregion
		}
	}
}
