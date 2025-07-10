using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
	{
		public TWCustomsNumberViewStmNumsSetting(GlbCompany company, ZString rangeType)
			: base(company, rangeType)
		{
		}

		protected override void DefaultDataOnSettingOwnerCore(CustomsNumberViewStmNums stmNums)
		{
			if (stmNums.SN_Type.IsEmpty)
			{
				stmNums.SN_Type = BaseEntryNumberGenerator.CustomsStmNumsType;
			}
		}

		protected override bool AllowDuplicateCore() => false;

		protected override bool IsNumberUsedCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return ((TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper).ExistingEntry(number);
		}

		protected override ZString GenerateCustomsNumberCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return number.TrimStart(new char[] { '0' });
		}
	}
}
