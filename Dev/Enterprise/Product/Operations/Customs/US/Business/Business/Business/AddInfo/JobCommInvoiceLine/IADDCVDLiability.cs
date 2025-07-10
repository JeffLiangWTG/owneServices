using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IADDCVDLiability
	{
		ZBool IsSetXLine { get; }
		ZBool US_ADD_NA { get; }
		ZBool US_CVD_NA { get; }
		bool IsCountryOfOriginCanada { get; }
		ZString US_UC_NKCountryOfOrigin { get; }
		bool IsACE { get; }
		ZDate EffectiveDateForDutyRate { get; }
		bool IsEntrySummaryValidationMode { get; }
		bool IsLVS { get; }
	}
}
