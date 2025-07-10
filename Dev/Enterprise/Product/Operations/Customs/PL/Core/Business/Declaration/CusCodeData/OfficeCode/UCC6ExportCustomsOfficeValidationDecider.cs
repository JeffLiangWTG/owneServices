using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business;

public class UCC6ExportCustomsOfficeValidationDecider : ICustomsOfficeValidationDecider
{
	public bool IsRuleR0676Active => true;
}
