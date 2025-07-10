using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IGlbEmailAddress
	{
		ZDateTime GI_DeliveryReportTimeUtc { get; set; }
		ZString GI_DeliveryStatus { get; set; }
		ZString GI_EmailAddress { get; }
	}
}