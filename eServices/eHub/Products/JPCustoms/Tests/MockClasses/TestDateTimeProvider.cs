using System;
using CargoWise.eHub.Products.JPCustoms.Common;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class TestDateTimeProvider : IDateTimeProvider
	{
		public TestDateTimeProvider()
		{
		}

		public DateTime DateTimeNow { get; set; }
	}
}