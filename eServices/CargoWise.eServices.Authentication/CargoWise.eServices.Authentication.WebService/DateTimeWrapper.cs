using System;

namespace CargoWise.eServices.Authentication.WebService
{
	public class DateTimeWrapper : IDateTime
	{
		public DateTime Now()
		{
			return DateTime.Now;
		}
	}
}
