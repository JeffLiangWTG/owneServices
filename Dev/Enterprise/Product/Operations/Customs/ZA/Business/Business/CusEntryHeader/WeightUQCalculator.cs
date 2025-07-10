using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Business
{
	public class WeightUQCalculator : Customs.Business.WeightUQCalculator
	{
		public WeightUQCalculator(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected CusEntryHeader Header => (CusEntryHeader)base.EntryHeader;

		public ZWeight GrossWeight
		{
			get
			{
				ZWeight result = ZWeight.Empty;
				foreach (CusEntryLine mergedLine in EntryHeader.MergedLines)
				{
					result += mergedLine.EffectiveGrossWeight;
				}
				return result;
			}
		}

		public override CargoWise.Types.ZDecimal Weight
		{
			get { return GrossWeight.Amount; }
		}

		public override string UQ
		{
			get { return GrossWeight.Unit; }
		}
	}
}
