using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	class ZDateTimeExtensionsTest : TestCase
	{
		public void TestToUnspecifiedDateTime()
		{
			AssertEquals(new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), new ZDateTime(2012, 1, 1).ToUnspecifiedDateTime());
			AssertEquals(new DateTime(2013, 2, 2, 0, 0, 0, DateTimeKind.Unspecified), new ZDateTime(2013, 2, 2).ToUnspecifiedDateTime());
		}
	}
}
