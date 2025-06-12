using System;

namespace CargoWise.eServices.Authentication.ServiceClient
{
	public interface IDateTime
	{
		DateTime UtcNow { get; }
	}
}
