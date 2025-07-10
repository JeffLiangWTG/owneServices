
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.MasterFiles.Business
{
	public interface ITrackingBusinessObject
	{
		TrackingConstants.BusinessContext TrackingBusinessContext { get; }
		ZGuid TrackingBusinessObjectPK { get; }
	}
}
