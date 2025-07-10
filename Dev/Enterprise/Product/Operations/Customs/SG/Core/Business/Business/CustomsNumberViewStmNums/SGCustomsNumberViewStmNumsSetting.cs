using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
	{
		public SGCustomsNumberViewStmNumsSetting(GlbCompany company, ZString rangeType)
			: base(company, rangeType)
		{
		}

		protected override ZLong? DefaultTypeRangeMaxCore()
		{
			return IsSingaporeMessageNumber ? 9999L : 99999999L;
		}

		protected override bool CanRolloverCore()
		{
			return IsSingaporeMessageNumber;
		}

		protected override bool AllowDuplicateCore()
		{
			return !IsSingaporeMessageNumber;
		}

		bool IsSingaporeMessageNumber => RangeType == NumberRangeTypeList.Codes.SingaporeMessageNumber;
	}
}
