using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class CustomAttributeComparerTest : WhsTestCaseWithFactory
	{
		public void TestCompare()
		{
			CustomAttributeComparer comparer = new CustomAttributeComparer();
			TestILineCustomAttributes src = new TestILineCustomAttributes();
			TestILineCustomAttributes with = new TestILineCustomAttributes();
			ZDateTime today = ZDateTime.Today;

			AssertEquals(true, comparer.Compare(src, with));

			with = new TestILineCustomAttributes("CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, today.AddDays(1), today.AddDays(2), today.AddDays(3), today.AddDays(4), today.AddDays(5), true, true, true, true, true, "TEXTBLOB1");
			AssertEquals(false, comparer.Compare(src, with));

			src = new TestILineCustomAttributes("CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, today.AddDays(1), today.AddDays(2), today.AddDays(3), today.AddDays(4), today.AddDays(5), true, true, true, true, true, "TEXTBLOB1");
			AssertEquals(true, comparer.Compare(src, with));

			with.CustomAttrib1 = "C1";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib1 = "CA1";

			with.CustomAttrib2 = "C2";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib2 = "CA2";

			with.CustomAttrib3 = "C3";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib3 = "CA3";

			with.CustomAttrib4 = "C4";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib4 = "CA4";

			with.CustomAttrib5 = "C5";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib5 = "CA5";

			with.CustomAttrib6 = "C6";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomAttrib6 = "CA6";

			with.CustomDecimal1 = 11.1m;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDecimal1 = 1.1m;

			with.CustomDecimal2 = 12.1m;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDecimal2 = 2.2m;

			with.CustomDecimal3 = 13.1m;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDecimal3 = 3.3m;

			with.CustomDecimal4 = 14.1m;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDecimal4 = 4.4m;

			with.CustomDecimal5 = 15.1m;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDecimal5 = 5.5m;

			with.CustomDate1 = today;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDate1 = today.AddDays(1);

			with.CustomDate2 = today;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDate2 = today.AddDays(2);

			with.CustomDate3 = today;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDate3 = today.AddDays(3);

			with.CustomDate4 = today;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDate4 = today.AddDays(4);

			with.CustomDate5 = today;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomDate5 = today.AddDays(5);

			with.CustomFlag1 = false;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomFlag1 = true;

			with.CustomFlag2 = false;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomFlag2 = true;

			with.CustomFlag3 = false;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomFlag3 = true;

			with.CustomFlag4 = false;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomFlag4 = true;

			with.CustomFlag5 = false;
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomFlag5 = true;

			with.CustomTextBlob1 = "BLOBBY";
			AssertEquals(false, comparer.Compare(src, with));
			with.CustomTextBlob1 = "TEXTBLOB1";
		}
	}
}
