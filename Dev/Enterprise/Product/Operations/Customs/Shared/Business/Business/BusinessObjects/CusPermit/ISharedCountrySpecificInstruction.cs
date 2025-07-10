using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public interface ISharedCountrySpecificInstruction
	{
		PermitTypeList GetTypeList();
		CodeDescriptionPairList GetSubTypeList(ZString typeCode);
		PermitQtyValIndicatorList GetQtyValIndicatorList(ZString permitType, ZString permitSubType);
		PermitTransactionTypeList GetTransactionTypeList(ZString permitType, ZString permitSubType);
		PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType);
		PermitRuleCodeList GetRuleCodeListForModule();
		ICollection GetLookupList(SharedCusPermitHeader permitHeader, ZString ruleCode);
		ICollection GetPermitNumberCollection(SharedCusPermitHeader permitHeader);
		AppliesToIndicator GetAppliesToIndicator(ZString permitType, ZString permitSubType);
		PermitMatchingType GetMatchingType(ZString ruleCode);
		IComparer GetRangeComparer(ZString ruleCode);
		ZString GetValueFromFieldType(ZString ruleCode);
		ZString GetValueToFieldType(ZString ruleCode);
		bool IsQtyValIndicatorMandatory { get; }
	}
}
