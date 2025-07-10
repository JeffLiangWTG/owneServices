using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class ScheduleCampaignItemsValidation : ZValidation
	{
		public ScheduleCampaignItemsValidation(ScheduleCampaignItems parent)
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

		const int ScheduleLimit = 40;

		public override Type AutoValidationType
		{
			get { return typeof(ScheduleCampaignItemsValidation); }
		}

		#region ScheduleSendTimeUTC

		public void ValidateScheduleSendTimeUTC()
		{
			ZValidationInternals.Validate(Parent.ScheduleSendTimeUTCInfo, GetScheduleSendTimeUTCValidationInvoker());
		}

		RunValidationInvoker GetScheduleSendTimeUTCValidationInvoker()
		{
			return delegate
			{
				CheckScheduleSendTimeUTCIsValidZDateTime();
				CheckScheduleSendTimeUTCIsValidZDateTimeRange();
				CheckScheduleSendTimeUTC();
			};
		}

		protected virtual void CheckScheduleSendTimeUTCIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.ScheduleSendTimeUTCInfo);
		}

		protected virtual void CheckScheduleSendTimeUTCIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.ScheduleSendTimeUTCInfo);
		}

		protected virtual void CheckScheduleSendTimeUTC()
		{
			if (Parent.Status == TrackingStatusCodes.Codes.QUE)
			{
				MandatoryValidation.CheckEntered(Parent.ScheduleSendTimeUTCInfo);

				ZDateTime utcNow = ZDateTime.UtcNow;
				if (Parent.ScheduleSendTimeUTC > utcNow.AddDays(ScheduleLimit))
				{
					Parent.ScheduleSendTimeUTCInfo.AddError(Res.GetString("14ca44b9-d243-496e-ad96-c7684fb7be7e", "Date exceeds allowable limit of 40 days."));
				}
				else if (Parent.ScheduleSendTimeUTC < utcNow)
				{
					Parent.ScheduleSendTimeUTCInfo.AddError(Res.GetString("782a156b-1b56-45d5-bd04-8911fd177203", "Date cannot be set behind the current UTC time."));
				}
			}
		}

		#endregion

		#region ScheduleSendTimeLocal

		public void ValidateScheduleSendTimeLocal()
		{
			ZValidationInternals.Validate(Parent.ScheduleSendTimeLocalInfo, GetScheduleSendTimeLocalValidationInvoker());
		}

		RunValidationInvoker GetScheduleSendTimeLocalValidationInvoker()
		{
			return delegate
			{
				CheckScheduleSendTimeLocalIsValidZDateTime();
				CheckScheduleSendTimeLocalIsValidZDateTimeRange();
				CheckScheduleSendTimeLocal();
			};
		}

		protected virtual void CheckScheduleSendTimeLocalIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.ScheduleSendTimeLocalInfo);
		}

		protected virtual void CheckScheduleSendTimeLocalIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.ScheduleSendTimeLocalInfo);
		}

		protected virtual void CheckScheduleSendTimeLocal()
		{
			MandatoryValidation.CheckEntered(Parent.ScheduleSendTimeLocalInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateScheduleSendTimeUTC();
				ValidateScheduleSendTimeLocal();
			}
		}

		#region Implementation

		protected readonly ScheduleCampaignItems Parent;
		readonly ISingleElementListInternal ParentListInternals;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
