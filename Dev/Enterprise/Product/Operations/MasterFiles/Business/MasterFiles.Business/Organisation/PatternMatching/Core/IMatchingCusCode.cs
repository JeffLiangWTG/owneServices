using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public interface IMatchingCusCode
	{
		ZGuid PK { get; }
		bool HasChanges { get; }
		bool IsInDatabase { get; }
		ZString OK_CustomsRegNo { get; }
		ZString OK_CodeType { get; }
		ZString OK_CodeTypeOriginalValue { get; }
		ZString OK_CustomsRegNoOriginalValue { get; }
		bool OK_CustomsRegNoHasChanges { get; }
		bool OK_CodeTypeHasChanges { get; }
		ZString OK_RN_NKCodeCountry { get; }
		ZGuid OK_OA_PremisesAddress { get; }
	}
}
