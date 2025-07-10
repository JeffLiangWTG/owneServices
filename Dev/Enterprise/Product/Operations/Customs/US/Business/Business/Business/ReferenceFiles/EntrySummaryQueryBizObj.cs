using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class EntrySummaryQueryBizObj : NonPersistentBusinessObject, IObsoleteValidation, IEntrySummaryQueryMessageAttachee
	{
		public EntrySummaryQueryBizObj(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string EntryFilerCode = "EntryFilerCode";
			public const int EntryFilerCodeMaxLength = 3;
			public const string EntryNumber = "EntryNumber";
			public const int EntryNumberMaxLength = 9;

			public const string CriteriaCode = "CriteriaCode";
			public const int CriteriaCodeMaxLength = 3;

			public const string DateFrom = "DateFrom";
			public const string TimeFrom = "TimeFrom";

			public const string DateTo = "DateTo";
			public const string TimeTo = "TimeTo";

			public const string ConsumptionEntrySummaries = "ConsumptionEntrySummaries";
			public const string FTAReconSummaries = "FTAReconSummaries";
			public const string OtherReconSummaries = "OtherReconSummaries";
			public const string DrawbackSummaries = "DrawbackSummaries";
			public const string NAFTADutyDeferralSummaries = "NAFTADutyDeferralSummaries";

			public const string CollectionBillInformationCode = "CollectionBillInformationCode";
		}

		#region Properties

		public bool SendMessage;

		#region EntryFilerCode

		[MaxLength(Schema.EntryFilerCodeMaxLength)]
		public ZString EntryFilerCode
		{
			get { return entryFilerCode; }
			set
			{
				SetNonPersistentPropertyValue(EntryFilerCodeInfo, ref entryFilerCode, value);
				ValidateEntryFilerCode();
			}
		}
		ZString entryFilerCode;

		public ZPropertyInfo EntryFilerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EntryFilerCode); }
		}

		public void ValidateEntryFilerCode()
		{
			if (!IsValidationSuspended)
			{
				EntryFilerCodeInfo.ClearAllNotifications();

				if (EntryFilerCode.IsEmpty)
				{
					EntryFilerCodeInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);
				}
			}
		}

		#endregion

		#region EntryNumber

		[MaxLength(Schema.EntryNumberMaxLength)]
		public ZString EntryNumber
		{
			get { return entryNumber; }
			set
			{
				SetNonPersistentPropertyValue(EntryNumberInfo, ref entryNumber, value);
				ValidateEntryNumber();
				ValidateCriteriaCode();
				ValidateDateRange();
			}
		}
		ZString entryNumber;

		public ZPropertyInfo EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EntryNumber); }
		}

		public void ValidateEntryNumber()
		{
			if (!IsValidationSuspended)
			{
				EntryNumberInfo.ClearAllNotifications();

				if (CriteriaCode.IsEmpty && EntryNumber.IsEmpty)
				{
					EntryNumberInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.ValueRequired);
				}

				if (!EntryNumber.IsEmpty && !CriteriaCode.IsEmpty)
				{
					EntryNumberInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.NotRequired);
				}

				if (!EntryNumber.IsEmpty && EntryNumber.Length != 8)
				{
					EntryNumberInfo.AddWarning(ValidationConstants.EntrySummaryQuery.EntryNumberLength);
				}
			}
		}

		void ValidateEntryNumberCalculate()
		{
			if (!ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective)
			{
				ValidateEntryNumber();
			}
			else
			{
				foreach (EntryNumberBizObj entryNumber in EntryNumbers)
				{
					entryNumber.ValidateEntryNumber();
				}
			}
		}

		#endregion

		#region ApplicationCode

		[List(nameof(ApplicationCodeList))]
		public ZString ApplicationCode
		{
			get { return JobApplicationCodeList.Codes.ACE; }
		}

		#endregion

		#region CriteriaCode

		[List(nameof(CriteriaCodeList))]
		[MaxLength(Schema.CriteriaCodeMaxLength)]
		public ZString CriteriaCode
		{
			get { return criteriaCode; }
			set
			{
				SetNonPersistentPropertyValue(CriteriaCodeInfo, ref criteriaCode, value);
				ValidateCriteriaCode();
				ValidateEntryNumberCalculate();
				ValidateDateRange();
			}
		}
		ZString criteriaCode;

		public ZPropertyInfo CriteriaCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CriteriaCode); }
		}

		public void ValidateCriteriaCode()
		{
			if (!IsValidationSuspended)
			{
				CriteriaCodeInfo.ClearAllNotifications();

				if (!CriteriaCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(CriteriaCodeInfo, CriteriaCodeList);

					if (IsEntryNumberExist)
					{
						CriteriaCodeInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.NotRequired);
					}
				}
				else if (!IsEntryNumberExist)
				{
					CriteriaCodeInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.ValueRequired);
				}
			}
		}

		#endregion

		#region DateFrom

		public ZDateTime DateFrom
		{
			get { return dateFrom; }
			set
			{
				SetNonPersistentPropertyValue(DateFromInfo, ref dateFrom, value);

				TimeFrom = DateFrom.IsValid ? new ZDateTime(DateFrom.Year, 1, 1, 0, 0, 0) : ZDateTime.Empty;

				ValidateDateFrom();
				ValidateDateRange();
			}
		}
		ZDateTime dateFrom;

		public ZPropertyInfo DateFromInfo
		{
			get { return GetZPropertyInfo(Schema.DateFrom); }
		}

		public void ValidateDateFrom()
		{
			if (!IsValidationSuspended)
			{
				DateFromInfo.ClearAllNotifications();

				if (!CriteriaCode.IsEmpty && DateFrom.IsEmpty)
				{
					DateFromInfo.AddMessageError(DateRangeRequired);
				}
				else if (CriteriaCode.IsEmpty && !DateFrom.IsEmpty)
				{
					DateFromInfo.AddMessageError(DateRangeNotRequired);
				}
				else if (FromDateTime > ZDateTime.Now)
				{
					DateFromInfo.AddMessageError(CannotBeFutureDate);
				}

				if (!CriteriaCode.IsEmpty && IsEntryNumberExist && !DateFrom.IsEmpty)
				{
					DateFromInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.NotRequired);
				}

				if (CriteriaCode.IsEmpty && !IsEntryNumberExist && DateFrom.IsEmpty)
				{
					DateFromInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.ValueRequired);
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZDateTime TimeFrom
		{
			get { return timeFrom; }
			set
			{
				SetNonPersistentPropertyValue(TimeFromInfo, ref timeFrom, value);
				ValidateTimeFrom();
				ValidateDateRange();
			}
		}
		ZDateTime timeFrom;

		public ZPropertyInfo TimeFromInfo
		{
			get { return GetZPropertyInfo(Schema.TimeFrom); }
		}

		public ZDateTime FromDateTime
		{
			get { return DateFrom.IsValid && TimeFrom.IsValid ? new ZDateTime(DateFrom.Year, DateFrom.Month, DateFrom.Day, TimeFrom.Hour, TimeFrom.Minute, 0) : ZDateTime.Empty; }
		}

		public void ValidateTimeFrom()
		{
			if (!IsValidationSuspended)
			{
				TimeFromInfo.ClearAllNotifications();
				if (!IsEntryNumberExist)
				{
					MandatoryValidation.MessageErrorIfNotEntered(TimeFromInfo, "From Time");
				}

				if (DateFrom == DateTo && TimeFrom > TimeTo)
				{
					TimeFromInfo.AddMessageError(TimeFromIsInvalid);
				}
			}
		}
		internal const string DateTimeRangeRequired = "An Outstanding Action Entry Summary Query Request must be specified with a date/time range.";
		internal const string TimeFromIsInvalid = "From Time cannot be greater than To Time.";

		#endregion

		#region DateTo

		public ZDateTime DateTo
		{
			get { return dateTo; }
			set
			{
				SetNonPersistentPropertyValue(DateToInfo, ref dateTo, value);

				if (value.IsToday)
				{
					ZDateTime current = ZDateTime.Now;
					TimeTo = DateTo.IsValid ? new ZDateTime(DateTo.Year, 1, 1, current.Hour, current.Minute, current.Second) : ZDateTime.Empty;
				}
				else
				{
					TimeTo = DateTo.IsValid ? new ZDateTime(DateTo.Year, 1, 1, 23, 59, 59) : ZDateTime.Empty;
				}

				ValidateDateTo();
				ValidateDateRange();
			}
		}
		ZDateTime dateTo;

		public ZPropertyInfo DateToInfo
		{
			get { return GetZPropertyInfo(Schema.DateTo); }
		}

		public void ValidateDateTo()
		{
			if (!IsValidationSuspended)
			{
				DateToInfo.ClearAllNotifications();

				if (!CriteriaCode.IsEmpty && DateTo.IsEmpty)
				{
					DateToInfo.AddMessageError(DateRangeRequired);
				}
				else if (CriteriaCode.IsEmpty && !DateTo.IsEmpty)
				{
					DateToInfo.AddMessageError(DateRangeNotRequired);
				}
				else if (ToDateTime > ZDateTime.Now)
				{
					DateToInfo.AddMessageError(CannotBeFutureDate);
				}
				else if (DateTo < DateFrom)
				{
					DateToInfo.AddMessageError(ToDateInvalid);
				}
				else if (DateFrom.IsValid && DateFrom.AddDays(31) < DateTo)
				{
					DateToInfo.AddMessageError(ToDateOutOfRange);
				}

				if (!CriteriaCode.IsEmpty && IsEntryNumberExist && !DateTo.IsEmpty)
				{
					DateToInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.NotRequired);
				}

				if (CriteriaCode.IsEmpty && !IsEntryNumberExist && DateTo.IsEmpty)
				{
					DateToInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.ValueRequired);
				}
			}
		}
		internal const string ToDateInvalid = "To Date cannot be prior to From Date.";
		internal const string ToDateOutOfRange = "The Date range cannot exceed 31 days.";
		internal const string DateRangeRequired = "An entry summary query with outstanding action should be requested with date range.";
		internal const string DateRangeNotRequired = "Date range is only required for an entry summary query with outstanding action.";
		internal const string CannotBeFutureDate = "Date cannot be in the future.";

		public ZDateTime TimeTo
		{
			get { return timeTo; }
			set
			{
				SetNonPersistentPropertyValue(TimeToInfo, ref timeTo, value);
				ValidateTimeTo();
				ValidateTimeFrom();
				ValidateDateRange();
			}
		}
		ZDateTime timeTo;

		public ZPropertyInfo TimeToInfo
		{
			get { return GetZPropertyInfo(Schema.TimeTo); }
		}

		public ZDateTime ToDateTime
		{
			get { return DateTo.IsValid && TimeTo.IsValid ? new ZDateTime(DateTo.Year, DateTo.Month, DateTo.Day, TimeTo.Hour, TimeTo.Minute, 59) : ZDateTime.Empty; }
		}

		public void ValidateTimeTo()
		{
			if (!IsValidationSuspended)
			{
				TimeToInfo.ClearAllNotifications();
				if (!IsEntryNumberExist)
				{
					MandatoryValidation.MessageErrorIfNotEntered(TimeToInfo, "To Time");
				}
			}
		}
		internal const string ToTimeInvalid = "To Date Time cannot be prior to From Date Time.";

		public void ValidateDateRange()
		{
			ValidateDateFrom();
			ValidateTimeFrom();

			ValidateDateTo();
			ValidateTimeTo();
		}

		#endregion

		#region ConsumptionEntrySummaries

		public ZBool ConsumptionEntrySummaries
		{
			get { return consumptionEntrySummaries; }
			set
			{
				SetNonPersistentPropertyValue(ConsumptionEntrySummariesInfo, ref consumptionEntrySummaries, value);
			}
		}
		ZBool consumptionEntrySummaries;

		public ZPropertyInfo ConsumptionEntrySummariesInfo
		{
			get { return GetZPropertyInfo(Schema.ConsumptionEntrySummaries); }
		}

		#endregion

		#region FTAReconSummaries

		public ZBool FTAReconSummaries
		{
			get { return fFTAReconSummaries; }
			set
			{
				SetNonPersistentPropertyValue(FTAReconSummariesInfo, ref fFTAReconSummaries, value);
			}
		}
		ZBool fFTAReconSummaries;

		public ZPropertyInfo FTAReconSummariesInfo
		{
			get { return GetZPropertyInfo(Schema.FTAReconSummaries); }
		}

		#endregion

		#region OtherReconSummaries

		public ZBool OtherReconSummaries
		{
			get { return otherReconSummaries; }
			set
			{
				SetNonPersistentPropertyValue(OtherReconSummariesInfo, ref otherReconSummaries, value);
			}
		}
		ZBool otherReconSummaries;

		public ZPropertyInfo OtherReconSummariesInfo
		{
			get { return GetZPropertyInfo(Schema.OtherReconSummaries); }
		}

		#endregion

		#region DrawbackSummaries

		public ZBool DrawbackSummaries
		{
			get { return drawbackSummaries; }
			set
			{
				SetNonPersistentPropertyValue(DrawbackSummariesInfo, ref drawbackSummaries, value);
			}
		}
		ZBool drawbackSummaries;

		public ZPropertyInfo DrawbackSummariesInfo
		{
			get { return GetZPropertyInfo(Schema.DrawbackSummaries); }
		}

		#endregion

		#region NAFTADutyDeferralSummaries

		public ZBool NAFTADutyDeferralSummaries
		{
			get { return fNAFTADutyDeferralSummaries; }
			set
			{
				SetNonPersistentPropertyValue(NAFTADutyDeferralSummariesInfo, ref fNAFTADutyDeferralSummaries, value);
			}
		}
		ZBool fNAFTADutyDeferralSummaries;

		public ZPropertyInfo NAFTADutyDeferralSummariesInfo
		{
			get { return GetZPropertyInfo(Schema.NAFTADutyDeferralSummaries); }
		}

		#endregion

		#region CollectionBillInformationCode

		[List(nameof(CollectionBillInformationCodeList))]
		public ZString CollectionBillInformationCode
		{
			get { return fCollectionBillInformationCode; }
			set
			{
				SetNonPersistentPropertyValue(CollectionBillInformationCodeInfo, ref fCollectionBillInformationCode, value);
				ValidateCollectionBillInformationCode();
			}
		}
		ZString fCollectionBillInformationCode;

		public ZPropertyInfo CollectionBillInformationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CollectionBillInformationCode); }
		}

		public void ValidateCollectionBillInformationCode()
		{
			if (!IsValidationSuspended)
			{
				CollectionBillInformationCodeInfo.ClearAllNotifications();

				if (!CollectionBillInformationCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(CollectionBillInformationCodeInfo, CollectionBillInformationCodeList);
				}
			}
		}

		#endregion

		#region IsEntryNumberExist

		bool IsEntryNumberExist
		{
			get
			{
				if (!ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective)
				{
					return !EntryNumber.IsEmpty;
				}
				return EntryNumbers.Cast<EntryNumberBizObj>().Any(x => !x.EntryNumber.IsEmpty);
			}
		}

		#endregion

		#endregion

		#region Implementation

		ZString IEntrySummaryQueryMessageAttachee.CollectionBillInformationCode
		{
			get
			{
				var result = ZString.Empty;
				if (ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective)
				{
					result = CollectionBillInformationCode.SubstringSafe(1);
				}
				return result;
			}
		}

		public ZString ProcessingDistrictPort
		{
			get { return ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(Branch); }
		}

		public GlbBranch Branch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		public ZString ProcessingOfficeCode
		{
			get { return ZString.Empty; }
		}

		public bool IsRemoteLocationFiling
		{
			get { return false; }
		}

		EDIMessageCollection IQueryMessageAttachee.Messages
		{
			get { return null; }
		}

		public ZString PreparerDistrictPort
		{
			get { return ZString.Empty; }
		}

		ZDateTime IQueryMessageAttachee.DateFrom
		{
			get { return FromDateTime; }
		}

		ZDateTime IQueryMessageAttachee.DateTo
		{
			get { return ToDateTime; }
		}

		Guid IQueryMessageAttachee.CompanyPK
		{
			get { return GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		IEnumerable<(ZString, ZString)> IQueryMessageAttachee.EntryFilerCodesAndNumbers
		{
			get
			{
				if (!ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective)
				{
					yield return (EntryFilerCode, EntryNumber);
				}
				else
				{
					foreach (EntryNumberBizObj entryNumber in EntryNumbers)
					{
						yield return (entryNumber.EntryFilerCode, entryNumber.EntryNumber);
					}
				}
			}
		}

		[ChildEditable(true)]
		public EntryNumberBizObjCollection EntryNumbers
		{
			get
			{
				if (entryNumbers == null)
				{
					entryNumbers = new EntryNumberBizObjCollection(this);
					RegisterEditableChildObject(entryNumbers);
				}
				return entryNumbers;
			}
		}
		EntryNumberBizObjCollection entryNumbers;

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEntryNumberCalculate();
			ValidateCriteriaCode();
			ValidateDateRange();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			using (SuspendSettingHasChanges())
			{
				EntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
				CriteriaCode = ZString.Empty;
			}
		}

		#endregion

		#region Lookups

		public JobApplicationCodeList ApplicationCodeList
		{
			get { return Factory.GetCachedValue<JobApplicationCodeList>(); }
		}

		public CriteriaCodeList CriteriaCodeList
		{
			get { return Factory.GetCachedValue<CriteriaCodeList>(); }
		}

		public CollectionBillInformationCodeList CollectionBillInformationCodeList
		{
			get { return Factory.GetCachedValue<CollectionBillInformationCodeList>(); }
		}

		#endregion
	}
}
