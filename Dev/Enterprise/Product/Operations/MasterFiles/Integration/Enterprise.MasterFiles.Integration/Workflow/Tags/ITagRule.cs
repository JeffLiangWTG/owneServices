using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagRule
	{
		ZGuid PK { get; }
		ZString TGR_Name { get; set; }
		ITagLink TagRuleTemplate { get; }
	}
}
