using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class EntryNumberBizObj : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EntryNumberBizObj(EntrySummaryQueryBizObj queryData)
			: base(new BusinessObjectFactory())
		{
			this.queryData = queryData;
		}
		readonly EntrySummaryQueryBizObj queryData;

		public static class Schema
		{
			public const string EntryFilerCode = "EntryFilerCode";
			public const int EntryFilerCodeMaxLength = 3;
			public const string EntryNumber = "EntryNumber";
			public const int EntryNumberMaxLength = 9;
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
				queryData.ValidateCriteriaCode();
				queryData.ValidateDateRange();
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

				var isCriteriaCodeEmpty = queryData.CriteriaCode.IsEmpty;
				var isEntryNumberEmpty = EntryNumber.IsEmpty;

				if (isCriteriaCodeEmpty && isEntryNumberEmpty)
				{
					EntryNumberInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.ValueRequired);
				}

				if (!isEntryNumberEmpty && !isCriteriaCodeEmpty)
				{
					EntryNumberInfo.AddMessageError(ValidationConstants.EntrySummaryQuery.NotRequired);
				}

				if (!isEntryNumberEmpty && EntryNumber.Length != 8)
				{
					EntryNumberInfo.AddWarning(ValidationConstants.EntrySummaryQuery.EntryNumberLength);
				}
			}
		}

		#endregion

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateEntryNumber();
		}

		#endregion
	}
}
