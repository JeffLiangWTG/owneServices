using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingMovementValidation : ZValidation
	{
		public MessageSendingMovementValidation(MessageSendingMovement parent)
			: base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}

		public void Add(MessageSendingMovementValidation validation)
		{
			zValidationInternals.Add(validation);
		}

		public void Remove(MessageSendingMovementValidation validation)
		{
			zValidationInternals.Remove(validation);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			var suspender = parentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateMM_Send();
			var firstSendingObject = Parent.FirstSendingObject;
			if (firstSendingObject != null)
			{
				firstSendingObject.Validation.ValidateMB_Date();
			}
			ValidateMM_ForeignDeparturePort();
		}

		#endregion

		#region MM_Send

		public void ValidateMM_Send()
		{
			zValidationInternals.Validate(Parent.MM_SendInfo, new RunValidationInvoker(this.CheckMM_Send));
		}

		protected void CheckMM_Send()
		{
			if (Parent.MM_Send)
			{
				if (Parent.ActionCode == ActionCode.ChangeEstDateOfArrival)
				{
					if (Parent.OriginalEstimatedDate >= ZDate.Today.AddDays(-5))
					{
						Parent.MM_SendInfo.AddMessageError(ValidationConstants.MessageSending.NewEstimatedDateOfArrival);
					}
				}
				else if ((Parent.ActionCode == ActionCode.SubsequentInBondOriginal || Parent.ActionCode == ActionCode.SubsequentInBondAmendment)
					&& Parent.InBondNumber.IsEmpty && !Parent.InBondNumberAllocationMutexHasLock())
				{
					Parent.MM_SendInfo.AddError(ValidationConstants.MessageSending.InBondNumberAllocationIsInProgress(Parent.GetInBondNumberAllocationMutexLockInfo()));
				}
			}
		}

		#endregion

		#region MM_ForeignDeparturePort

		public void ValidateMM_ForeignDeparturePort()
		{
			zValidationInternals.Validate(Parent.MM_ForeignDeparturePortInfo, new RunValidationInvoker(this.CheckMM_ForeignDeparturePort));
		}

		protected void CheckMM_ForeignDeparturePort()
		{
			if (IsDepartureOrChangeDateEvent && Parent.MM_Send)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MM_ForeignDeparturePortInfo);
			}
		}

		bool IsDepartureOrChangeDateEvent
		{
			get { return Parent.ActionCode == ActionCode.VesselDeparture || Parent.ActionCode == ActionCode.ChangeEstDateOfArrival; }
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get
			{
				return typeof(MessageSendingMovementValidation);
			}
		}
		public MessageSendingMovement Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly MessageSendingMovement parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals zValidationInternals;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
