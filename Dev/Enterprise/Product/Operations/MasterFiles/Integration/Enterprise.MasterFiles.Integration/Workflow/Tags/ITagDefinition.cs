using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagDefinition
	{
		ZGuid PK { get; }
		ZString TGD_Code { get; set; }
		ZString TGD_Description { get; set; }
		ZBool TGD_IsExclusive { get; set; }
		ZString TGD_VisualizationData { get; set; }
		ZString TGD_UsageScope { get; set; }
	}
}
