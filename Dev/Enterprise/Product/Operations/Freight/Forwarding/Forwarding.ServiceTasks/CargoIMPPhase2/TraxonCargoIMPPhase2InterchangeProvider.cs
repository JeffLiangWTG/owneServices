using CargoWise.Types;
using Enterprise.Edifact.Generic;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(CargoIMPPhase2MessageManager))]

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	class TraxonCargoIMPPhase2InterchangeProvider : InterchangeProviderBase
	{
		internal const string TraxonPIMAAddress = "REUAGT82YAS";

		public TraxonCargoIMPPhase2InterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		ZString messageReferenceNumber;

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get
			{
				return Res.GetString("1e567610-95f1-43da-90d7-ef7a1fe3750f", "Please set a value in Registry {0}/{1}.",
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Category,
					ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Caption);
			}
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			ZString from = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Value;
			SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, TraxonPIMAAddress, from);

			UNBSegment uNB = GetUNB(PreparedTime.ToZDateTime(), interchange.EI_To, "PIMA", interchange.EI_From, "PIMA",
					ZString.Empty, "TRXA", "1", ZString.Empty, false, false, "0");
			interchange.EI_HeaderText = uNB.ToString(CurrentUNCharacterSet) + GetUNHString(messages[0]);
			this.messageReferenceNumber = messages[0].EM_MessageNum;
		}

		ZString GetUNHString(EDIMessage message)
		{
			UNHSegment uNH = new UNHSegment();
			uNH.MessageReferenceNumber = message.EM_MessageNum;
			uNH.MessageIdentifier.MessageType = GetMessageType(message);

			switch (message.EM_MessageType)
			{
				case CargoIMPPhase2MessageManager.MilestoneStatusUpdateType:
				case CargoIMPPhase2MessageManager.RouteMapInformationType:
					uNH.MessageIdentifier.MessageVersionNumber = "3";
					break;

				case CargoIMPPhase2MessageManager.RouteMapCancellationType:
					uNH.MessageIdentifier.MessageVersionNumber = "2";
					break;

				default:
					uNH.MessageIdentifier.MessageVersionNumber = "3";
					break;
			}

			uNH.CommonAccessReference = uNH.MessageReferenceNumber;
			return uNH.ToString(CurrentUNCharacterSet);
		}

		ZString GetMessageType(EDIMessage message)
		{
			return "TRX" + message.EM_MessageType;
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return CurrentUNCharacterSet.SegmentDelimiter + GetUNTString() + base.GetInterchangeFooter(messageCount);
		}

		ZString GetUNTString()
		{
			UNTSegment uNT = new UNTSegment();
			uNT.NumberOfSegmentsInTheMessage = "3";
			uNT.MessageReferenceNumber = this.messageReferenceNumber;
			return uNT.ToString(CurrentUNCharacterSet);
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return DoNotCollateType;
		}
	}
}
