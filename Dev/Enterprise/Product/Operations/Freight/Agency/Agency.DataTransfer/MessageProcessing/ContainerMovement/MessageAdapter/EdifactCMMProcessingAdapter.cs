using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public class EdifactCMMProcessingAdapter : CMMProcessingAdapter
	{
		public EdifactCMMProcessingAdapter(EDIMessage message)
		{
			this.message = Argument.NotNull(message, "message");
			Factory = message.Factory;
		}

		protected override void LoadCore()
		{
			messageData = CMMMessageDecoder.Parse(message.EM_MessageText);

			ICMMOrganisationData senderData = messageData.MessageSender;
			if (senderData == null && Message.Interchange != null)
			{
				senderData = new InterchangeSender(Message.Interchange);
			}

			var fallbackAddressType = AddressHelper.DefaultShipperAddressType(MessageType);

			senderAddress = senderData == null ? null : AddressHelper.FindOrganisationAddressByOrgCodeType(Factory, fallbackAddressType, senderData.CodeType, senderData.Code);
		}

		ICMMMessagingData MessageData
		{
			get { return messageData; }
		}
		ICMMMessagingData messageData;

		public override EDIMessage Message
		{
			get { return message; }
		}
		readonly EDIMessage message;

		public override void AttachMessage(ContainerMovement movement, string status)
		{
			if (Message != null)
			{
				EDIMessage newMessage = Message;
				if (!newMessage.EM_LinkUniqueID.IsEmpty)
				{
					var args = new BusinessObjectCloneArgs(new[]
					{
						EDIMessageSchema.Constants.EM_LinkUniqueID,
						EDIMessageSchema.Constants.EM_LinkTable,
						EDIMessageSchema.Constants.EM_Status,
					});

					newMessage = (EDIMessage)Message.Clone(args);
				}

				newMessage.EM_Status = status;
				movement.Messages.Add(newMessage);
			}
		}

		#region IContainerMovementMessageProcessingAdapter Members

		public override string LloydsNumber
		{
			get { return MessageData == null ? String.Empty : MessageData.LloydsNumber; }
		}

		public override string VoyageNumber
		{
			get { return MessageData == null ? String.Empty : MessageData.VoyageNumber; }
		}

		public override string TransportMode
		{
			get { return MessageData == null ? String.Empty : MessageData.TransportMode; }
		}

		public override OrgAddress SenderAddress
		{
			get { return senderAddress; }
		}
		OrgAddress senderAddress;

		#endregion

		#region Properties

		public override CMMMessageType MessageType
		{
			get { return MessageData == null ? CMMMessageType.Unknown : MessageData.Type; }
		}

		public override string MessageSenderCode
		{
			get
			{
				string messageSenderCode = string.Empty;
				if (MessageData != null && MessageData.MessageSender != null)
				{
					messageSenderCode = MessageData.MessageSender.Code;
				}
				return messageSenderCode;
			}
		}

		public override CMMOrganisationType MessageSenderCodeType
		{
			get
			{
				var messageSenderCodeType = CMMOrganisationType.Unknown;
				if (MessageData != null && MessageData.MessageSender != null)
				{
					messageSenderCodeType = MessageData.MessageSender.CodeType;
				}
				return messageSenderCodeType;
			}
		}

		public override string MessageText
		{
			get { return Message == null ? String.Empty : (string)Message.EM_MessageText; }
		}

		#endregion

		#region Containers

		public override List<CMMMessageContainer> Containers
		{
			get
			{
				if (containers == null && IsLoaded)
				{
					containers = GetContainers();
				}

				return containers ?? new List<CMMMessageContainer>();
			}
		}
		List<CMMMessageContainer> containers;

		List<CMMMessageContainer> GetContainers()
		{
			List<CMMMessageContainer> result = new List<CMMMessageContainer>();

			foreach (ICMMEquipmentData cmmContainer in MessageData.Equipment)
			{
				var messageContainer = new CMMMessageContainer
				{
					BillOfLading = cmmContainer.BillOfLading,
					BookingReference = cmmContainer.BookingReference,
					ContainerNumber = cmmContainer.ContainerNumber,
					GoodsDeclarationNumber = cmmContainer.GoodsDeclarationNumber,
					GrossWeightKG = cmmContainer.GrossWeightKG,
					ISOType = MapContainerType(Sender, cmmContainer.ISOType),
					IsEmpty = cmmContainer.IsEmpty,
					PositioningDateTime = cmmContainer.PositioningDateTime,
					SealNumbers = cmmContainer.SealNumbers.ToList(),
				};

				CMMEquipmentSupplier cmmEquipmentSupplier;
				if (Enum.TryParse(cmmContainer.EquipmentSupplier.ToString(), out cmmEquipmentSupplier))
				{
					messageContainer.CmmEquipmentSupplier = cmmEquipmentSupplier;
				}

				result.Add(messageContainer);
			}

			return result;
		}

		/// <summary>
		/// Map the Container Type according to the sender override relationships
		/// </summary>
		string MapContainerType(OrgHeader sender, string isoContainerType)
		{
			ZQuery overrideFilter = new ZQuery();
			overrideFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, isoContainerType);
			overrideFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, sender.PK);
			overrideFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.ContainerType);

			var pOverride = Factory.LoadTop1<OrgPatternMatchOverride>(overrideFilter);
			if (pOverride == null)
			{
				return isoContainerType;
			}

			var refContainer = Factory.Load<RefContainer>(pOverride.OO_LocalGuid);
			return refContainer == null ? isoContainerType : refContainer.RC_ISOType.ToString();
		}

		#endregion
	}
}



