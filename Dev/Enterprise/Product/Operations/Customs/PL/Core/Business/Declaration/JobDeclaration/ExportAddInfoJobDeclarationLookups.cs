using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobDeclarationLookups : JobDeclarationLookups
{
	public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue("PL_Export_SpecificCircumstanceIndicatorList", () =>
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(SpecificCircumstanceIndicatorForUCCList.Codes.A20, SpecificCircumstanceIndicatorForUCCList.Descriptions.A20);
		return result;
	});
}
