using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business
{
	public interface ITransmitDateSourceData
	{
		ZString JE_MessageType { get; }
		ZString JE_TransportMode { get; }
		ZDateTime BarrierDate { get; }
		ZDateTime CachedTodaysDate { get; }
		ZDateTime JE_EDITransmitDate { get; }
	}

	public class EDITransmitDateManager
	{
		public EDITransmitDateManager(ITransmitDateSourceData sourceData)
		{
			this.sourceData = sourceData;
			cachedTodaysDate = sourceData.CachedTodaysDate.Date;
		}
		readonly ITransmitDateSourceData sourceData;
		readonly ZDateTime cachedTodaysDate;

		void UpdateDatesFromDeclarationSettingsIfSourceItemsHaveChanged()
		{
			ZString messageType = sourceData.JE_MessageType;
			ZString transportMode = sourceData.JE_TransportMode;
			ZDateTime barrierDate = sourceData.BarrierDate.Date;
			ZDateTime eDITransmitDate = sourceData.JE_EDITransmitDate.Date;
			if (messageType != this.messageType
				|| transportMode != this.transportMode
				|| barrierDate != this.barrierDate
				|| eDITransmitDate != this.eDITransmitDate)
			{
				this.messageType = messageType;
				this.transportMode = transportMode;
				this.barrierDate = barrierDate;
				this.eDITransmitDate = eDITransmitDate;
				UpdateDatesFromDeclarationSettings();
			}
		}
		ZString messageType;
		ZString transportMode;
		ZDateTime barrierDate;
		ZDateTime eDITransmitDate;

		void UpdateDatesFromDeclarationSettings()
		{
			fRecommendedDate = ZDateTime.Empty;
			earliestDatePossible = ZDateTime.Empty;
			if (barrierDate.IsValid)
			{
				if (messageType == JobMessageTypeList.Codes.Export)
				{
					earliestDatePossible = barrierDate.AddDays(-30);
				}
			}
			fRecommendedDate = earliestDatePossible.IsEmpty || earliestDatePossible > cachedTodaysDate ? earliestDatePossible : cachedTodaysDate;
		}
		ZDateTime earliestDatePossible;

		ZDateTime fRecommendedDate;
		public ZDateTime RecommendedDate
		{
			get
			{
				UpdateDatesFromDeclarationSettingsIfSourceItemsHaveChanged();
				return fRecommendedDate;
			}
		}

		string RecommendedDateNotificationSuffix
		{
			get { return fRecommendedDate.IsEmpty ? "" : NotificationRecommendedDatePrefix + fRecommendedDate.ToString() + ")"; }
		}

		public ValidationResult CheckEDITransmitDate()
		{
			UpdateDatesFromDeclarationSettingsIfSourceItemsHaveChanged();

			if (eDITransmitDate.IsValid)
			{
				if (eDITransmitDate < cachedTodaysDate)
				{
					return new ValidationResult(ErrorEDITransmitDateInThePast + RecommendedDateNotificationSuffix, true);
				}
				else if (eDITransmitDate < earliestDatePossible)
				{
					return AddMessageErrorWhenEDITranmitDateTooEarly();
				}
				else if (messageType == JobMessageTypeList.Codes.Export && eDITransmitDate != fRecommendedDate && fRecommendedDate.IsValid)
				{
					return new ValidationResult(WarningNotEarliestDatePossible + RecommendedDateNotificationSuffix, false);
				}
			}
			return null;
		}

		public class ValidationResult
		{
			public ValidationResult(ZString messageText, bool isError)
			{
				fMessageText = messageText;
				fIsError = isError;
			}
			readonly ZString fMessageText;
			readonly bool fIsError;

			public ZString MessageText
			{
				get { return fMessageText; }
			}

			public bool IsError
			{
				get { return fIsError; }
			}
		}

		ValidationResult AddMessageErrorWhenEDITranmitDateTooEarly()
		{
			if (messageType == JobMessageTypeList.Codes.Export)
			{
				return new ValidationResult(ErrorExportTooEarly + RecommendedDateNotificationSuffix, true);
			}
			return null;
		}

		public const string ErrorExportTooEarly = "Cannot lodge an Export Declaration more than 30 days before the date of departure.";

		public static string ErrorEDITransmitDateInThePast { get { return "Cannot have an EDI Transmit Date earlier than the current date."; } }

		public static string WarningNotEarliestDatePossible { get { return "EDI Transmit Date not set to the earliest date possible."; } }

		public const string NotificationRecommendedDatePrefix = " (Recommended Date: ";
	}
}
