using System;

namespace CargoWise.eServices.Authentication.ServiceClient
{
	public class DateTimeWrapper : IDateTime
	{
		public virtual DateTime UtcNow => DateTime.UtcNow;
	}
}
