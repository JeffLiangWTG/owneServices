using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;

namespace Enterprise.Customs.US.AMS.Business
{
	public class PortArrivalDetailCollection : NonPersistentBusinessObjectCollection<PortArrivalDetail>
	{
		public PortArrivalDetailCollection(CusInBondHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly CusInBondHeader header;

		public void InitialisePortArrival()
		{
			RemoveAndDeleteAll();
			PopulatePortArrivalProperties();
		}

		void PopulatePortArrivalProperties()
		{
			foreach (var bill in header.Bills)
			{
				var movementDetail = bill.MovementDetail;
				var billPortOfUnlading = bill.B0_InBondPortOfDestDCode;

				if (movementDetail != null && movementDetail.IsBillAlreadyOnFile)
				{
					var moveHeader = movementDetail.MoveHeader;
					if (moveHeader != null)
					{
						AMSEDIMessage latestReceivedVesselMessage = null;

						foreach (var message in moveHeader.Messages.GetMatchingMessages(AMSEDIMessage.ApplicationCodes.AMS, new ZString[] { AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse }, AMSEDIMessage.Direction.Receive, true, ListSortDirection.Descending).OfType<AMSEDIMessage>())
						{
							if (AMSMessageSubTypeList.IsVesselArrivalEventRelevent(message.EM_MessageSubType))
							{
								var inpp01Block = message.MessageBlock.MessageBlocks.OfType<IINPP01>().FirstOrDefault();
								if (inpp01Block != null && inpp01Block.PortOfUnlading == billPortOfUnlading)
								{
									latestReceivedVesselMessage = message;
									break;
								}
							}
						}

						var originalMessage = latestReceivedVesselMessage?.OriginalMessage;
						if (originalMessage != null && latestReceivedVesselMessage.GetMessageBlocks<TARW01>().Count == 0)
						{
							var h01Block = (ICMH01)originalMessage.MessageBlock.MessageBlocks.Find(delegate(MessageBlock block)
							{ return block is ICMH01; });
							if (h01Block != null)
							{
								var dateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(h01Block.Date, h01Block.Time);
								if (!dateTime.IsEmpty)
								{
									if (latestReceivedVesselMessage.EM_MessageSubType == AMSMessageSubTypeList.Codes.VesselArrival)
									{
										AddNewIfNotExist(billPortOfUnlading, dateTime);
									}
								}
							}
						}
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public PortArrivalDetail AddNewIfNotExist(ZString portCode, ZDateTime actualArrivalDate)
		{
			var result = this.OfType<PortArrivalDetail>().FirstOrDefault(x => x.PortCode == portCode);
			if (result == null)
			{
				result = AddNew();
				result.PortCode = portCode;
			}

			result.ActualArrivalDate = actualArrivalDate;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortArrivalDetail(Factory);
		}

		public ZBool HasPortCode(ZString portCode)
		{
			var result = false;
			foreach (PortArrivalDetail portArrival in this)
			{
				if (portArrival.PortCode == portCode)
				{
					result = true;
					break;
				}
			}

			return result;
		}
	}
}
