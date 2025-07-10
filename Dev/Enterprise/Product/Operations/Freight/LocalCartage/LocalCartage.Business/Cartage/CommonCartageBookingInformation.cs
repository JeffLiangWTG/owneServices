using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using BookingCodes = Enterprise.Freight.Business.FreightConstants.LocalCartageBookingStatus.Codes;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageBookingInformation : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string BookingAccepted = "BookingAccepted";
			public const string BookingRejected = "BookingRejected";
			public const string BookingStatus = "BookingStatus";
			public const string BookingComment = "BookingComment";

			public const int BookingStatusMaxLength = 3;
			public const int BookingCommentMaxLength = 120;
		}

		public CommonCartageBookingInformation(CommonCartage cartage)
		{
			this.Cartage = cartage;
			fBookingAccepted = GetHasStatusUpdateEventOfType(BookingCodes.BookingAccepted);
			fBookingRejected = GetHasStatusUpdateEventOfType(BookingCodes.BookingRejected);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BookingStatus = ZString.Empty;
		}

		bool GetHasStatusUpdateEventOfType(ZString refType)
		{
			ZQuery logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			ZString statusDescription = FreightCodePairLists.CartageJobBookingActionList().GetDescriptionFromCode(refType);
			logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, refType + "-" + statusDescription);
			return Cartage.Logs.GetAllLogs().Find(logFilter).Length > 0;
		}

		public ZBool BookingAccepted
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fBookingAccepted; }
			set { SetNonPersistentPropertyValue(BookingAcceptedInfo, ref fBookingAccepted, value); }
		}
		ZBool fBookingAccepted;

		public ZPropertyInfo BookingAcceptedInfo
		{
			get { return GetZPropertyInfo(Schema.BookingAccepted); }
		}

		public ZBool BookingRejected
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fBookingRejected; }
			set { SetNonPersistentPropertyValue(BookingRejectedInfo, ref fBookingRejected, value); }
		}
		ZBool fBookingRejected;

		public ZPropertyInfo BookingRejectedInfo
		{
			get { return GetZPropertyInfo(Schema.BookingRejected); }
		}

		[List("BookingStatusList")]
		[MaxLength(Schema.BookingStatusMaxLength)]
		public ZString BookingStatus
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fBookingStatus; }
			set
			{
				CheckMaximumLength(BookingStatusInfo, value);
				SetNonPersistentPropertyValue(BookingStatusInfo, ref fBookingStatus, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookingStatus();
				}
			}
		}
		ZString fBookingStatus;

		public ZPropertyInfo BookingStatusInfo
		{
			get { return GetZPropertyInfo(Schema.BookingStatus); }
		}

		public ZString CalculatedBookingStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (!BookingStatus.IsEmpty)
				{
					result = BookingStatus;
				}
				else if (BookingAccepted)
				{
					result = BookingCodes.BookingAccepted;
				}
				else if (BookingRejected)
				{
					result = BookingCodes.BookingRejected;
				}
				return result;
			}
		}

		[MaxLength(Schema.BookingCommentMaxLength)]
		public ZString BookingComment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fBookingComment; }
			set
			{
				CheckMaximumLength(BookingCommentInfo, value);
				SetNonPersistentPropertyValue(BookingCommentInfo, ref fBookingComment, value);
			}
		}
		ZString fBookingComment;

		public ZPropertyInfo BookingCommentInfo
		{
			get { return GetZPropertyInfo(Schema.BookingComment); }
		}

		public bool CanSelectStatus
		{
			get
			{
				bool result = false;
				if (CartageHasParent)
				{
					result = true;
				}
				else
				{
					result = GetHasStatusUpdateEventOfType(BookingCodes.BookingAccepted)
						|| GetHasStatusUpdateEventOfType(BookingCodes.BookingRejected);
				}
				return result;
			}
		}

		public bool CanExposeBookingAction
		{
			get { return !CartageHasParent; }
		}

		public CommonCartageBookingInformationValidation Validation
		{
			get { return new CommonCartageBookingInformationValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public CodeDescriptionPairList BookingStatusList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (CartageHasParent)
				{
					result = FreightCodePairLists.CartageJobFWDBookingActionList();
				}
				else if (CanSelectStatus)
				{
					result = FreightCodePairLists.CartageJobBookingStatusList();
				}
				else
				{
					result = FreightCodePairLists.CartageJobBookingActionList();
				}
				return result;
			}
		}

		bool CartageHasParent
		{
			get { return Cartage.HasParent; }
		}

		readonly CommonCartage Cartage;
	}
}
