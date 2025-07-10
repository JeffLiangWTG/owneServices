using System.Collections.Generic;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	public class DummyCMMProcessingAdapter : CMMProcessingAdapter
	{
		public DummyCMMProcessingAdapter()
		{
			messageSenderCode = string.Empty;
			messageSenderCodeType = CMMOrganisationType.Unknown;
		}

		protected override void LoadCore()
		{
		}

		public override bool UpdateBookings
		{
			get
			{
				return updateBookings;
			}
		}

		public bool updateBookings;
		public override OrgAddress SenderAddress
		{
			get
			{
				return senderAddress;
			}
		}

		public OrgAddress senderAddress;
		public override string LloydsNumber
		{
			get
			{
				return lloydsNumber;
			}
		}

		public string lloydsNumber;
		public override string VoyageNumber
		{
			get
			{
				return voyageNumber;
			}
		}

		public string voyageNumber;
		public override string TransportMode
		{
			get
			{
				return transportMode;
			}
		}

		public string transportMode;
		public override CMMMessageType MessageType
		{
			get
			{
				return messageType;
			}
		}

		public CMMMessageType messageType;
		public override List<CMMMessageContainer> Containers
		{
			get
			{
				return containers;
			}
		}

		public List<CMMMessageContainer> containers;
		public override EDIMessage Message
		{
			get
			{
				return message;
			}
		}

		public EDIMessage message;
		public override string MessageSenderCode
		{
			get
			{
				return messageSenderCode;
			}
		}

		public string messageSenderCode;
		public override CMMOrganisationType MessageSenderCodeType
		{
			get
			{
				return messageSenderCodeType;
			}
		}

		public CMMOrganisationType messageSenderCodeType;
		public override string MessageText
		{
			get
			{
				return messageText;
			}
		}

		public string messageText;
		public override void AttachMessage(Business.ContainerMovement movement, string status)
		{
		}
	}
}
