using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IViewQuotedBooking
	{
		ZGuid PK { get; }
		ZGuid VB_JS { get; set; }
		ZGuid VB_TH { get; set; }
		ZGuid VB_GC { get; set; }
		IJobHeader Job { get; }
	}
}
