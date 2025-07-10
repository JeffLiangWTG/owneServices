using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Telematics.ServiceTasks
{
	public interface IRequestPacker
	{
		Uri CreateUri(IEnumerable<ZGeography> deviceLocations);
	}
}
