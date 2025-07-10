using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemScheduleValidation : ZValidation
	{
		public GlbCompanyCampaignItemScheduleValidation(GlbCompanyCampaignItemSchedule parent)
			: base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
			this.Parent = parent;
			this.ParentListInternals = parent;
			this.ZValidationInternals = this;
		}

		public override Type AutoValidationType
		{
			get { return typeof(GlbCompanyCampaignItemScheduleValidation); }
		}

		#region ScheduleSendTime

		public void ValidateScheduleSendTimeLocal()
		{
			ZValidationInternals.Validate(Parent.ScheduleSendTimeLocalInfo, GetScheduleSendTimeValidationInvoker());
		}

		RunValidationInvoker GetScheduleSendTimeValidationInvoker()
		{
			return delegate
			{
				CheckScheduleSendTimeIsValidZDateTime();
				CheckScheduleSendTimeIsValidZDateTimeRange();
				CheckScheduleSendTimeLocal();
			};
		}

		protected virtual void CheckScheduleSendTimeIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.ScheduleSendTimeLocalInfo);
		}

		protected virtual void CheckScheduleSendTimeIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.ScheduleSendTimeLocalInfo);
		}

		protected virtual void CheckScheduleSendTimeLocal()
		{
			if (Parent.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.ScheduleSendTimeLocalInfo);
			}
		}

		#endregion

		#region SenderTimeZone

		public void ValidateSenderTimeZone()
		{
			ZValidationInternals.Validate(Parent.SenderTimeZoneInfo, GetSenderTimeZoneValidationInvoker());
		}

		RunValidationInvoker GetSenderTimeZoneValidationInvoker()
		{
			return delegate
			{
				CheckSenderTimeZone();
			};
		}

		protected virtual void CheckSenderTimeZone()
		{
			MandatoryValidation.CheckEntered(Parent.SenderTimeZoneInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SenderTimeZoneInfo, Parent.UNLOCOs);
		}

		#endregion

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateScheduleSendTimeLocal();
				ValidateSenderTimeZone();
			}
		}

		#region Implementation

		protected readonly GlbCompanyCampaignItemSchedule Parent;
		readonly ISingleElementListInternal ParentListInternals;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
