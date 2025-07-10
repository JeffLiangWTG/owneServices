using System;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public interface IGlowTrackingUrlGenerator
	{
		Uri GenerateURL(ZGuid contactPK);
	}
}
