using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryHeader7501ExcessFee : EntrySummary7501ExcessFee, IObsoleteValidation
	{
		public EntryHeader7501ExcessFee(CusEntryHeader entry, IFee fee)
			: base(entry, fee)
		{
		}

		#region EntrySummary7501ExcessFee methods

		public override ZString SummaryFeeDesc
		{
			get { return fee.Code + " " + FeeCodeList.GetDescriptionFromCode(fee.Code); }
		}

		public override ZDecimal SummaryFee
		{
			get { return fee.Amount; }
		}

		#endregion

		CodeDescriptionPairList FeeCodeList => CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory);
	}
}
