using System.Collections;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
{
	public CusAuthorizationUsageLookups(AutoCusAuthorizationUsage parent) : base(parent)
	{
	}

	public override ICollection CodeList =>
		Parent.Instruction != null
			? base.CodeList
			: GetAuthorizationUsageCodesForInvoiceLine();

	ICollection GetAuthorizationUsageCodesForInvoiceLine() => Factory.GetCachedValue("PLAuthorizationUsageCodesForInvoiceLine", () =>
	{
		var result = new CodeDescriptionPairList();

		var data = new CusAuthorizationUsageInvoiceLineCodeList();
		foreach (CodeDescriptionPair item in data)
		{
			result.Add(CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, item));
		}

		result.SortByDescription();
		return result;
	});
}
