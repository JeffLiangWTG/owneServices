using System;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public class YesNoWithReasonForNoTest : TestCase
	{
		public void TestYes()
		{
			AssertEquals(true, YesNoWithReasonForNo.Yes);
			AssertEquals("", YesNoWithReasonForNo.Yes.ReasonForNotAllowed);
		}

		public void TestNo()
		{
			AssertEquals(false, YesNoWithReasonForNo.No("reason"));
			AssertEquals("reason", YesNoWithReasonForNo.No("reason").ReasonForNotAllowed);
			AssertExceptionThrown(typeof(ArgumentException), () => YesNoWithReasonForNo.No(""));
		}

		public void TestEquality()
		{
			var yes1 = YesNoWithReasonForNo.Yes;
			var yes2 = YesNoWithReasonForNo.Yes;
			var no1 = YesNoWithReasonForNo.No("reason");
			var no2 = YesNoWithReasonForNo.No("reason");
			var no3 = YesNoWithReasonForNo.No("other reason");

			AssertEquals(true, yes1 == yes2);
			AssertEquals(false, yes1 == no1);
			AssertEquals(true, no1 == no2);
			AssertEquals(false, no1 == no3);

			AssertEquals(false, yes1 != yes2);
			AssertEquals(true, yes1 != no1);
			AssertEquals(false, no1 != no2);
			AssertEquals(true, no1 != no3);

			AssertEquals(true, yes1.Equals(yes2));
			AssertEquals(false, yes1.Equals(no1));
			AssertEquals(true, no1.Equals(no2));
			AssertEquals(false, no1.Equals(no3));
		}
	}
}
