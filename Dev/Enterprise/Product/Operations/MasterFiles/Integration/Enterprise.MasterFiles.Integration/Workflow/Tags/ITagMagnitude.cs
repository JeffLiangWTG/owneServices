using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagMagnitude : IBusiness
	{
		ZGuid PK { get; }
		ZGuid TGM_TGD_Tag { get; set; }
		ZString TGM_Code { get; set; }
		ZString TGM_Description { get; set; }
		ZInt TGM_NudgeAmount { get; set; }
		ZString TGM_VisualizationData { get; set; }
		ZGuid TGM_GG_OwnerGroup { get; set; }
		ZInt TGM_RuleRunSequence { get; set; }

		string DisplayText { get; }
		ITagDefinition TagDefinition { get; }
	}
}
