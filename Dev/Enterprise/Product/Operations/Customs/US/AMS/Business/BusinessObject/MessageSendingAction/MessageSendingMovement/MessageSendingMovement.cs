using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingMovement : NonPersistentBusinessObject
	{
		#region Schema

		public static class Schema
		{
			public const string MM_RelatedDetails = "MM_RelatedDetails";
			public const string MM_Send = "MM_Send";
			public const string MM_Date = "MM_Date";
			public const string MM_ForeignDeparturePort = "MM_ForeignDeparturePort";
			public const int MM_ForeignDeparturePortMaxLength = 5;
			public const string MM_PTTFiler = "MM_PTTFiler";
		}

		#endregion

		public MessageSendingMovement(CusInBondMoveHeader moveHeader, ActionCode actionCode)
			: base(moveHeader.Factory)
		{
			this.moveHeader = moveHeader;
			this.actionCode = actionCode;
			this.MM_Send = !ActionCodeTool.IsInBondType(ActionCode);
		}
		readonly ActionCode actionCode;

		public MessageSendingMovement(CusInBondMoveDetail movementDetail, ActionCode actionCode)
			: this(movementDetail.MoveHeader, actionCode)
		{
			this.movementDetail = movementDetail;
		}
		readonly CusInBondMoveDetail movementDetail;

		internal ActionCode ActionCode
		{
			get { return actionCode; }
		}

		#region MM_RelatedDetails
		[ResourceStringData("NPBO:Enterprise.Customs.US.AMS.Business.MessageSendingMovement|MM_RelatedDetails", Caption = "Related Details")]
		public ZString MM_RelatedDetails
		{
			get { return ((IMessageAttacheeInHeader)moveHeader).RecordIdentifier; }
		}

		public ZPropertyInfo MM_RelatedDetailsInfo
		{
			get { return this.GetZPropertyInfo(Schema.MM_RelatedDetails); }
		}
		#endregion

		#region MM_PTTFiler
		[ResourceStringData("NPBO:Enterprise.Customs.US.AMS.Business.MessageSendingMovement|MM_PTTFiler", Caption = "PTT Filer")]
		public ZString MM_PTTFiler
		{
			get { return moveHeader.CarrierCodeForPTT; }
		}

		public ZPropertyInfo MM_PTTFilerInfo
		{
			get { return this.GetZPropertyInfo(Schema.MM_PTTFiler); }
		}
		#endregion

		#region MM_Send

		[ResourceStringData("NPBO:Enterprise.Customs.US.AMS.Business.MessageSendingMovement|MM_Send", Caption = "Send?")]
		public ZBool MM_Send
		{
			get { return mM_Send; }
			set
			{
				var oldValue = MM_Send;
				SetNonPersistentPropertyValue(MM_SendInfo, ref mM_Send, value);
				if (!IsCopying && oldValue != MM_Send)
				{
					if (ActionCode == ActionCode.SubsequentInBondOriginal || ActionCode == ActionCode.SubsequentInBondAmendment)
					{
						moveHeader.ReloadInBondNumber();
						if (InBondNumber.IsEmpty)
						{
							if (MM_Send)
							{
								moveHeader.LockInBondNumberAllocationMutex();
							}
							else
							{
								moveHeader.UnLockInBondNumberAllocationMutex();
							}
						}
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateMM_Send();
				}
				if (oldValue != MM_Send)
				{
					UpdateValidationMode();
				}
			}
		}

		public ZPropertyInfo MM_SendInfo
		{
			get { return this.GetZPropertyInfo(Schema.MM_Send); }
		}
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZBool mM_Send;

		#endregion

		#region MM_Date

		[BusinessObjectTestExclude]
		public ZDateTime MM_Date
		{
			get
			{
				var firstSendingObject = FirstSendingObject;
				return firstSendingObject == null ? ZDateTime.Empty : firstSendingObject.MB_Date;
			}
			set
			{
				var firstSendingObject = FirstSendingObject;
				if (firstSendingObject != null)
				{
					firstSendingObject.MB_Date = value;
				}
			}
		}

		public ZPropertyInfo MM_DateInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.MM_Date, x =>
				{
					var firstSendingObject = FirstSendingObject;
					return firstSendingObject != null ? firstSendingObject.MB_DateInfo : GetZPropertyInfo(Schema.MM_Date);
				});
			}
		}

		#endregion

		#region MM_ForeignDeparturePort

		[ResourceStringData("NPBO:Enterprise.Customs.US.AMS.Business.MessageSendingMovement|MM_ForeignDeparturePort", Caption = "Foreign Departure Port")]
		[List(nameof(ScheduleKList))]
		[MaxLength(Schema.MM_ForeignDeparturePortMaxLength)]
		public ZString MM_ForeignDeparturePort
		{
			get { return mM_ForeignDeparturePort; }
			set
			{
				SetNonPersistentPropertyValue(MM_ForeignDeparturePortInfo, ref mM_ForeignDeparturePort, value);
				if (FirstSendingObject != null)
				{
					FirstSendingObject.MB_ForeignDeparturePort = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateMM_ForeignDeparturePort();
				}
			}
		}

		public ZPropertyInfo MM_ForeignDeparturePortInfo
		{
			get { return this.GetZPropertyInfo(Schema.MM_ForeignDeparturePort); }
		}
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString mM_ForeignDeparturePort;

		public ZZRefCusCodeListCombinedCollection ScheduleKList
		{
			get { return scheduleKList ?? (scheduleKList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today)); }
		}
		ZZRefCusCodeListCombinedCollection scheduleKList;

		#endregion

		public CusInBondMoveDetail MovementDetail
		{
			get { return movementDetail; }
		}

		public IEnumerable<CusInBondMoveDetail> MovementDetails
		{
			get { return moveHeader.MovementDetails; }
		}

		public MessageSendingObject FirstSendingObject
		{
			get
			{
				if (firstSendingObject == null)
				{
					if (movementDetail != null || moveHeader != null)
					{
						var moveDetail = movementDetail ?? (CusInBondMoveDetail)moveHeader.FirstMoveDetail;
						if (moveDetail != null)
						{
							firstSendingObject = new MessageSendingObject.Loader(Factory).LoadOrNew(moveDetail, actionCode);
							if (firstSendingObject != null)
							{
								firstSendingObject.ShouldValidateDate = () => MM_Send;
							}
						}
					}
				}
				return firstSendingObject;
			}
		}
		MessageSendingObject firstSendingObject;

		readonly CusInBondMoveHeader moveHeader;

		internal ZString InBondNumber
		{
			get { return moveHeader.InBondNumber; }
		}

		internal string GetInBondNumberAllocationMutexLockInfo()
		{
			return moveHeader.GetInBondNumberAllocationMutexLockInfo();
		}

		internal bool InBondNumberAllocationMutexHasLock()
		{
			return moveHeader.InBondNumberAllocationMutexHasLock();
		}

		internal void UnLockInBondNumberAllocationMutex()
		{
			moveHeader.UnLockInBondNumberAllocationMutex();
		}

		public ZDate OriginalEstimatedDate
		{
			get
			{
				var header = moveHeader != null ? moveHeader.Header : null;
				return header != null && header.BH_ETA.IsValid ? header.BH_ETA.Date : ZDate.Empty;
			}
		}

		void UpdateBillValidationMode()
		{
			foreach (var moveDetail in MovementDetails)
			{
				var bill = moveDetail.Bill;
				if (bill != null)
				{
					bill.ValidationModes = MM_Send ? ValidationModes.UseParentValidateMode : ValidationModes.None;
				}
			}
		}

		void UpdateValidationMode()
		{
			moveHeader.ValidationModes = MM_Send ? ValidationModes.UseParentValidateMode : ValidationModes.None;
			UpdateBillValidationMode();
		}

		#region Validation

		public MessageSendingMovementValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}
		protected virtual MessageSendingMovementValidation GetNewValidation()
		{
			return new MessageSendingMovementValidation(this);
		}

		#endregion

	}
}
