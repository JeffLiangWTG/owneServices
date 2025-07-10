namespace Enterprise.MasterFiles.Business
{
	public interface IPostcodeFormattingRulesProvider
	{
		PostcodeFormattingRule GetRuleFromIso(string iso);
	}
}
