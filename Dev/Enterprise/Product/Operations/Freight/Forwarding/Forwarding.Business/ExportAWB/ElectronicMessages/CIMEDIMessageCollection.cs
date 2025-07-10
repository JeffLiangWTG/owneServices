using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.AWB.Messaging.CargoIMP;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class CIMEDIMessageCollection : DependentBusinessObjectCollection<CIMEDIMessage, ForwardingConsol>
	{
		public CIMEDIMessageCollection(ForwardingConsol master)
			: base(master)
		{
			SetReadOnlyIncludingChildren(true);
		}

		#region Last Message

		public CIMEDIMessage GetLatestTransmittedMessage()
		{
			return this.Cast<CIMEDIMessage>()
				.OrderBy((message) => message, new AutoEDIMessageComparer<CIMEDIMessage>(ListSortDirection.Descending))
				.FirstOrDefault((message) => message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
		}

		#endregion

		#region Current Status

		public ZString CurrentStatus
		{
			get
			{
				bool lastMessageIsTransmit = true;

				foreach (CIMEDIMessage message in this.Cast<CIMEDIMessage>().OrderBy((message) => message, new AutoEDIMessageComparer<CIMEDIMessage>(ListSortDirection.Descending)))
				{
					if (message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					{
						if (lastMessageIsTransmit)
						{
							if (message.EM_Status == EDIMessage.Status.Queued)
							{
								return Res.GetString("e71e616f-1e20-4f6f-80b5-89b3e85242a4", "Message Queued for Sending");
							}
							else if (message.EM_Status == EDIMessage.Status.Sent)
							{
								return Res.GetString("1afb2d18-0f02-43b3-80ad-07d65d1cee2a", "Message Sent");
							}
						}

						return Res.GetString("c5a9e2e8-a8e5-466b-bc07-300c563b07d9", "Message Receipt Acknowledged");
					}

					lastMessageIsTransmit = false;

					if (message.EM_MessageType == CargoIMPMessageTypeList.Codes.FNA)
					{
						if (message.EM_MessageText.Contains((NoResString)"ACK/AWB REJECTED", StringComparison.Ordinal) && message.EM_MessageText.Contains((NoResString)"/DUPLICATE AWB", StringComparison.Ordinal)) // Hard-coded constant
						{
							// this is really more of a status report than an error
						}
						else
						{
							return Res.GetString("25826248-a879-48c2-ab1b-8f95eda22075", "Message Error Received");
						}
					}
				}

				return Res.GetString("80553fcb-5f0d-4dfb-976d-8c3cc314f13c", "No messages have been sent");
			}
		}

		#endregion

		#region Filter

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (!IsLoading)
			{
				Master.AWBCurrentStatusInfo.RefreshBinding();
			}

			if (!IsLoaded)
			{
				Master.Validation.ValidateAWBCurrentStatus();
			}

			base.OnAdded(bizOAdded);
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			Master.AWBCurrentStatusInfo.RefreshBinding();
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return EDIMessageSchema.EM_LinkUniqueID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (child is CIMEDIMessage cIMEDIMessage && cIMEDIMessage.EM_LinkUniqueID.IsEmpty)
			{
				base.SetCollectionRelationships(child);
				cIMEDIMessage.EM_LinkTable = ForwardingConsol.Schema.TableName;
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.CIM);
			return query;
		}

		#endregion

		#region Load

		protected override void AddItemsToCollectionForLoad(ZQuery filter)
		{
			base.AddItemsToCollectionForLoad(filter);
			LoadHVLVConsignmentFHLMessages();
		}

		void LoadHVLVConsignmentFHLMessages()
		{
			var hvlvShipments = Master.Shipments.Where(shipment => ((ForwardingShipment)shipment).IsHighVolumeLowValue);

			var consignmentHeaderSubQuery = new ZDBOnlySubQuery(typeof(IHVLVConsignmentHeader), HVLVConsignmentHeaderSchema.PK);
			consignmentHeaderSubQuery.AddToFilter(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, hvlvShipments.Select(x => x.PK));

			var consignmentSubQuery = new ZDBOnlySubQuery(typeof(IHVLVConsignment), HVLVConsignmentSchema.PK);
			consignmentSubQuery.AddSubQuery(HVLVConsignmentSchema.HVC_HCH_Header, consignmentHeaderSubQuery, JoinCondition.And);

			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, HVLVConsignmentSchema.Constants.TableName);
			messageQuery.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, consignmentSubQuery, JoinCondition.And);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CIM);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypes.FHL);

			AddRange(Factory.Load<EDIMessage>(messageQuery));
		}

		#endregion
	}
}
