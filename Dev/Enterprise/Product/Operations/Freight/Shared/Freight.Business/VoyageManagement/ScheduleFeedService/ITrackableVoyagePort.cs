using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public interface ITrackableVoyagePort
	{
		ZPropertyInfo Unloco { get; }
		ZPropertyInfo EstimatedDate { get; }

		Logs Logs { get; }
	}
}
