using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
	{
		public USCustomsNumberViewStmNumsSetting(GlbCompany company, ZString rangeType)
			: base(company, rangeType)
		{
		}

		public const long USMaximumFormalEntryNumber = 9999999; // US Entry Number consist of 7 digits follow by a check digit

		protected override int RequiredDigitCore()
		{
			var result = 8;
			switch (RangeType)
			{
				case NumberRangeTypeList.Codes.CustomsEntry:
					result = 7;
					break;
			}
			return result;
		}

		protected override ZLong? DefaultTypeRangeMaxCore()
		{
			ZLong result = 99999999L;
			switch (RangeType)
			{
				case NumberRangeTypeList.Codes.CustomsEntry:
					result = USMaximumFormalEntryNumber;
					break;
			}
			return result;
		}

		protected override ZLong GetThresholdRunOutWarningCore()
		{
			return 100L;
		}

		protected override bool IsNumberUsedCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			CusEntryNumber result = null;
			if (!number.IsEmpty)
			{
				switch (RangeType)
				{
					case NumberRangeTypeList.Codes.CustomsEntry:
						result = GetExistingEntrySummary(number, ((USCustomsNumberViewStmNumsWrapper)stmNums.Wrapper).AppliesTo);
						break;
				}
			}
			return result != null;
		}

		CusEntryNumber GetExistingEntrySummary(ZString number, ZString entryFilerCode)
		{
			CusEntryNumber result = null;
			var cusEntryNums = CusEntryNumber.Load(Parent.Factory, CusEntryHeaderMessageTypeList.Codes.EntrySummary, number, Core.Constants.CountryCodes.UnitedStates);
			foreach (CusEntryNumber entryNumObject in cusEntryNums)
			{
				if (entryFilerCode == entryNumObject.GetEntryFilerCode())
				{
					result = entryNumObject;
					break;
				}
			}
			return result;
		}

		protected override ZString GenerateCustomsNumberCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return number + ((USCustomsNumberViewStmNumsWrapper)stmNums.Wrapper).GetCheckDigit(number);
		}

		protected override void DefaultDataOnSettingOwnerCore(CustomsNumberViewStmNums stmNums)
		{
			if (stmNums.SN_Type.IsEmpty)
			{
				stmNums.SN_Type = RangeType;
			}
		}
	}
}
