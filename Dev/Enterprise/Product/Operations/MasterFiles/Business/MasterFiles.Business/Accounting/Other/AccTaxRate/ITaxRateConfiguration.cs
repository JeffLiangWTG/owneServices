using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ITaxRateConfiguration
	{
		ZString TaxID { get; }
		ZString Type { get; }
		ZString Description { get; }
		ZString ExtraType { get; }
		ZBool IsDefaultGST { get; }
		ZBool IsDefaultFreeGST { get; }
		ZBool IsDefaultGSTReverse { get; }
		ZBool IsDefaultFreeGSTReverse { get; }
		ZBool IsDefaultNotReport { get; }
		ZShort PostingGroup { get; }
		ZString ReferenceRateType { get; }
		ZString ReferenceExtraRateType { get; }
		ZString TaxSystem { get; }
		ZString RateSource { get; }
	}
}
