
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignClick : IBusiness
	{
		ZGuid GCC_GCL { get; set; }
		ZGuid GCC_G8_Recipient { get; set; }
		ZBlob GCC_HostAddress { get; set; }
		ZDateTime GCC_ClickTimeUtc { get; set; }
	}
}
