using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public enum InBondMessageType
	{
		DepartureAdd,
		DepartureDelete,
		DepartureAmend,
		DepartureBillDelete,

		InBondLevelArrival,
		BillOfLadingLevelArrival,
		ContainerLevelArrival,

		InBondLevelExportation,
		BillOfLadingLevelExportation,
		ContainerLevelExportation,

		InBondLevelTransferOfLiability,
		AirInBondAdd,
		AirInBondDelete,
		AirInBondAmend,
		AirBillDelete,

		AirEntireInBondArrival,
		AirBillOfLadingInBondArrival,
		AirContainernBondArrival,

		AirEntireInBondExportation,
		AirBillOfLadingInBondExportation,
		AirContainernBondExportation,

		AirBillOfLadingInBondExporation,
		BondedWarehouseUpdate,
		BondedWarehouseCancel,
		DiversionRequest
	}

	public class InBondMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<InBondMessageSendingObject>
	{
		public InBondMessageSendingObjectCollection(InBondMessageSendingHeaderObject master)
			: base(master.Factory)
		{
			this.master = master;
			PopulateObjects();
			Sort(InBondMessageSendingObject.Schema.US_InBondNumber);
		}

		readonly InBondMessageSendingHeaderObject master;
		public CusInBondHeader Header
		{
			get { return master.Header; }
		}

		public InBondMessageType MessageType
		{
			get { return master.MessageType; }
		}

		public bool HasAtLeastOneMarkedForSending
		{
			get
			{
				bool result = false;
				foreach (InBondMessageSendingObject obj in this)
				{
					if (obj.US_ShouldSend)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public void ReleaseAllInBondNumberMutex()
		{
			foreach (InBondMessageSendingObject sendingObject in this)
			{
				sendingObject.moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateObjects()
		{
			var shouldAddMovement = GetAddMovementCheck(MessageType);

			foreach (CusInBondMoveHeader moveHeader in master.GetMovementHeadersForSending())
			{
				switch (MessageType)
				{
					case InBondMessageType.BillOfLadingLevelArrival:
					case InBondMessageType.BillOfLadingLevelExportation:
						foreach (var moveDetail in moveHeader.MovementDetails.Where(x => !x.Bill.IsWaitingForResponse && x.Bill.MoveDetails.Count == 1))
						{
							var billObj = new InBondMessageSendingObject(moveHeader, moveDetail.Bill, MessageType);
							billObj.Master = master;
							Add(billObj);
						}
						break;
					case InBondMessageType.ContainerLevelArrival:
					case InBondMessageType.ContainerLevelExportation:
						foreach (var moveDetail in moveHeader.MovementDetails.Where(x => x.Bill.MoveDetails.Count == 1))
						{
							foreach (var container in moveDetail.Containers.Where(x => x.BC_ContainerNum != "NC" && !x.IsWaitingForResponse))
							{
								var containerObj = new InBondMessageSendingObject(moveHeader, moveDetail.Bill, container, MessageType);
								Add(containerObj);
							}
						}
						break;
					case InBondMessageType.AirBillDelete:
					case InBondMessageType.DepartureBillDelete:
						foreach (var moveDetail in moveHeader.MovementDetails.Where(x => x.Bill.HasDepartureBeenLodgedAtCustoms && x.Bill.MoveDetails.Count == 1))
						{
							var billObj = new InBondMessageSendingObject(moveHeader, moveDetail.Bill, MessageType);
							billObj.Master = master;
							Add(billObj);
						}
						break;
					default:
						if (shouldAddMovement(moveHeader))
						{
							var obj = new InBondMessageSendingObject(moveHeader, MessageType);
							Add(obj);
						}
						break;
				}
			}
		}

		Predicate<CusInBondMoveHeader> GetAddMovementCheck(InBondMessageType messageType)
		{
			Predicate<CusInBondMoveHeader> result = null;
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
				case InBondMessageType.AirInBondAdd:
					result = IsValidForAddMessaging;
					break;
				case InBondMessageType.DepartureDelete:
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.DepartureAmend:
				case InBondMessageType.AirInBondAmend:
				case InBondMessageType.DiversionRequest:
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					result = IsValidForDeleteOrAmendmentMessaging;
					break;
				case InBondMessageType.InBondLevelArrival:
				case InBondMessageType.AirEntireInBondArrival:
				case InBondMessageType.InBondLevelTransferOfLiability:
					result = IsValidForMessaging;
					break;
				case InBondMessageType.InBondLevelExportation:
				case InBondMessageType.AirEntireInBondExportation:
					result = IsValidForExportation;
					break;
				case InBondMessageType.BondedWarehouseUpdate:
					result = IsValidForBondedWarehouseUpdate;
					break;
				case InBondMessageType.BondedWarehouseCancel:
					result = IsValidForBondedWarehouseCancel;
					break;
				case InBondMessageType.BillOfLadingLevelArrival:
				case InBondMessageType.BillOfLadingLevelExportation:
				case InBondMessageType.ContainerLevelArrival:
				case InBondMessageType.ContainerLevelExportation:
					break;
				default:
					throw new NotSupportedException();
			}
			return result;
		}

		bool IsValidForBondedWarehouseUpdate(CusInBondMoveHeader moveHeader)
		{
			return moveHeader.HasWHSTransaction || moveHeader.WhsWarehouse != null;
		}

		bool IsValidForBondedWarehouseCancel(CusInBondMoveHeader moveHeader)
		{
			return moveHeader.HasWHSTransaction;
		}

		bool IsValidForAddMessaging(CusInBondMoveHeader moveHeader)
		{
			return !moveHeader.IsWaitingForResponse && !moveHeader.LogManager.HasAClearLog;
		}

		bool IsValidForDeleteOrAmendmentMessaging(CusInBondMoveHeader moveHeader)
		{
			return !moveHeader.IsWaitingForResponse && (moveHeader.LogManager.HasAClearLog
				|| !moveHeader.InBondNumber.IsEmpty);  // for movement created in legacy system
		}

		bool IsValidForExportation(CusInBondMoveHeader moveHeader)
		{
			return !moveHeader.IsWaitingForResponse && (moveHeader.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || moveHeader.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport);
		}

		bool IsValidForMessaging(CusInBondMoveHeader moveHeader)
		{
			return !moveHeader.IsWaitingForResponse;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
