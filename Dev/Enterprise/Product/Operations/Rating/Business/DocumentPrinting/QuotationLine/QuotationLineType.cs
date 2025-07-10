using System;

namespace Enterprise.Rating.Business
{
	[Flags]
	public enum QuotationLineType
	{
		Mandatory = 0x01,
		UseRateLineDescription = 0x02,
		UseChargeDescription = 0x04,
		NoCurrency = 0x08,
		MergeWithRateLineDescription = 0x10,
		AlternativeFormat = 0x20,
		f0 = 0x0100,
		f4 = 0x0200,
		UseOverrideDescription = 0x0300,

		UseDescriptionMask = UseRateLineDescription | UseChargeDescription,
	}
}
