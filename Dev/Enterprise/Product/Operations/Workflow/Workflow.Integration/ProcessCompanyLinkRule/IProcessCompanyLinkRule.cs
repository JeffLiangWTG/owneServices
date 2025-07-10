using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessCompanyLinkRule : IBusiness
	{
		ZGuid PCR_GC_Company { get; set; }
		ZBool PCR_IsActive { get; set; }
		ZString PCR_Macro { get; set; }
		ZDateTime PCR_SystemCreateTimeUtc { get; set; }
		ZString PCR_SystemCreateUser { get; set; }
		ZDateTime PCR_SystemLastEditTimeUtc { get; set; }
		ZString PCR_SystemLastEditUser { get; set; }
		ZString PCR_Type { get; set; }
	}
}
