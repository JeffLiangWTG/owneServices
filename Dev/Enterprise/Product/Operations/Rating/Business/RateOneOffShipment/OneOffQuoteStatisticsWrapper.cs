using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class OneOffQuoteStatisticsWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string OneOffQuoteKPI = "OneOffQuoteKPI";
			public const string OneOffQuoteSource = "OneOffQuoteSource";
			public const string OneOffQuoteRevisionReason = "OneOffQuoteRevisionReason";
		}

		public OneOffQuoteStatisticsWrapper(RateOneOffShipment oneOffShipment)
		{
			rateOneOffShipment = oneOffShipment;
		}

		readonly RateOneOffShipment rateOneOffShipment;

		public OneOffQuoteKPIList OneOffQuoteKPIList
		{
			get { return rateOneOffShipment.Lookups.OneOffQuoteKPIList; }
		}

		public OneOffQuoteSourceList OneOffQuoteSourceList
		{
			get { return rateOneOffShipment.Lookups.OneOffQuoteSourceList; }
		}

		public OneOffQuoteRevisionReasonList OneOffQuoteRevisionReasonList
		{
			get { return rateOneOffShipment.Lookups.OneOffQuoteRevisionReasonList; }
		}

		#region One Off Quote KPI

		[List("OneOffQuoteKPIList")]
		[MaxLength(AutoRateOneOffShipment.Schema.TT_QuoteKPIMaxLength)]
		public ZString OneOffQuoteKPI
		{
			get => rateOneOffShipment?.TT_QuoteKPI ?? ZString.Empty;
			set
			{
				rateOneOffShipment.TT_QuoteKPI = value;
				if (!rateOneOffShipment.IsValidationSuspended)
				{
					OneOffQuoteKPIInfo.ClearAllNotifications();
					OneOffQuoteKPIInfo.AddAllNotificationsFrom(rateOneOffShipment.TT_QuoteKPIInfo);
				}
				OneOffQuoteKPIInfo.RefreshBinding();
				HasChanges = rateOneOffShipment.HasChanges;
			}
		}
		public ZPropertyInfo OneOffQuoteKPIInfo { get { return GetZPropertyInfo(Schema.OneOffQuoteKPI); } }

		#endregion

		#region One Off Quote Source

		[List("OneOffQuoteSourceList")]
		[MaxLength(AutoRateOneOffShipment.Schema.TT_QuoteSourceMaxLength)]
		public ZString OneOffQuoteSource
		{
			get => rateOneOffShipment?.TT_QuoteSource ?? ZString.Empty;
			set
			{
				rateOneOffShipment.TT_QuoteSource = value;
				if (!rateOneOffShipment.IsValidationSuspended)
				{
					OneOffQuoteSourceInfo.ClearAllNotifications();
					OneOffQuoteSourceInfo.AddAllNotificationsFrom(rateOneOffShipment.TT_QuoteSourceInfo);
				}
				OneOffQuoteSourceInfo.RefreshBinding();
				HasChanges = rateOneOffShipment.HasChanges;
			}
		}

		public ZPropertyInfo OneOffQuoteSourceInfo { get { return GetZPropertyInfo(Schema.OneOffQuoteSource); } }

		#endregion

		#region One Off Quote Revision Reason

		[List("OneOffQuoteRevisionReasonList")]
		[MaxLength(AutoRateOneOffShipment.Schema.TT_RevisionReasonMaxLength)]
		public ZString OneOffQuoteRevisionReason
		{
			get => rateOneOffShipment?.TT_RevisionReason ?? ZString.Empty;
			set
			{
				rateOneOffShipment.TT_RevisionReason = value;
				if (!rateOneOffShipment.IsValidationSuspended)
				{
					OneOffQuoteRevisionReasonInfo.ClearAllNotifications();
					OneOffQuoteRevisionReasonInfo.AddAllNotificationsFrom(rateOneOffShipment.TT_RevisionReasonInfo);
				}
				OneOffQuoteRevisionReasonInfo.RefreshBinding();
				HasChanges = rateOneOffShipment.HasChanges;
			}
		}

		public ZPropertyInfo OneOffQuoteRevisionReasonInfo { get { return GetZPropertyInfo(Schema.OneOffQuoteRevisionReason); } }

		#endregion
	}

	public class OneOffQuoteKPIList : CodeDescriptionPairList
	{
		public OneOffQuoteKPIList() : base()
		{
			AddRange(DataRegistryRating.Instance.OneOffQuoteKPISettings.Value);
			Sort();
		}
	}

	public class OneOffQuoteSourceList : CodeDescriptionPairList
	{
		public OneOffQuoteSourceList() : base()
		{
			AddRange(DataRegistryRating.Instance.OneOffQuoteSourceSettings.Value);
			Sort();
		}
	}

	public class OneOffQuoteRevisionReasonList : CodeDescriptionPairList
	{
		public OneOffQuoteRevisionReasonList() : base()
		{
			AddRange(DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.Value);
			Sort();
		}
	}
}
