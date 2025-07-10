using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Tracking.Business.Testing
{
	public class PropertyChangeInfoTest : NonTransactionedTestCase
	{
		public void TestFormat_IZType()
		{
			AssertEquals(PropertyChangeInfo.Empty, PropertyChangeInfo.Format(ZDateTime.Empty));
			AssertEquals(PropertyChangeInfo.Empty, PropertyChangeInfo.Format(ZDateTimeOffset.Empty));
			AssertEquals("05-Jan-24 03:04", PropertyChangeInfo.Format(new ZDateTime(2024, 1, 5, 3, 4, 5)));
			AssertEquals("05-Jan-24 04:05", PropertyChangeInfo.Format(new ZDateTimeOffset(2024, 1, 5, 4, 5, 5, TimeSpan.FromHours(8))));
			AssertEquals(PropertyChangeInfo.Changed, PropertyChangeInfo.Format(ZBool.True));
			AssertEquals("", PropertyChangeInfo.Format(ZBool.False));
			AssertEquals("abc", PropertyChangeInfo.Format(new ZString("abc")));
		}
	}
}
