using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TestRangeJobMawbValidation : TestJobMawbValidation
	{
		public void TestMawbCount()
		{
			RangeMawb.NumberRangeStart = "10000001";
			RangeMawb.MawbCount = 10;
			AssertEquals("10000093", RangeMawb.NumberRangeEnd);
			AssertEquals(10, RangeMawb.MawbCount);

			RangeMawb.MawbCount = -1;
			AssertEquals(0, RangeMawb.MawbCount);

			RangeMawb.MawbCount = 10001;
			Assert("Expecting Mawb Count to have errors, mawb count must be less than 10000.", RangeMawb.MawbCountInfo.HasErrors());
			RangeMawb.MawbCount = 1001;
			Assert("Expecting Mawb Count to have wanrings, as mawb count is greater than 1000.", RangeMawb.MawbCountInfo.HasWarnings());
		}

		public void TestNumberRangeStart()
		{
			RangeMawb.MawbCount = 10;
			RangeMawb.NumberRangeStart = "10000001";
			AssertEquals("10000093", RangeMawb.NumberRangeEnd);
			AssertEquals(10, RangeMawb.MawbCount);

			RangeMawb.NumberRangeStart = "10000002";
			Assert(RangeMawb.NumberRangeStartInfo.HasErrors());

			RangeMawb.NumberRangeStart = "10002";
			Assert(RangeMawb.NumberRangeStartInfo.HasErrors());

			RangeMawb.NumberRangeStart = "1000wq22";
			Assert(RangeMawb.NumberRangeStartInfo.HasErrors());

			RangeMawb.NumberRangeStart = "";
			Assert(RangeMawb.NumberRangeStartInfo.HasErrors());

			RangeMawb.NumberRangeStart = "10000001";
			Assert(!RangeMawb.NumberRangeStartInfo.HasErrors());

			RangeMawb.MawbCount = 10;
			RangeMawb.NumberRangeStart = "99999992";
			Assert(RangeMawb.NumberRangeStartInfo.HasErrors());
			AssertEquals("The From MAWB will generate a To MAWB that is out of range.  The MAWB generated can be no more than 8 digits.", RangeMawb.NumberRangeStartInfo.GetErrors().GetFirstMessage());
		}

		public void TestNumberRangeEnd()
		{
			RangeMawb.MawbCount = 0;
			RangeMawb.NumberRangeStart = "10000001";
			RangeMawb.NumberRangeEnd = "10000093";
			AssertEquals(10, RangeMawb.MawbCount);

			RangeMawb.NumberRangeEnd = "10000002";
			Assert(RangeMawb.NumberRangeEndInfo.HasErrors());

			RangeMawb.NumberRangeEnd = "10002";
			Assert(RangeMawb.NumberRangeEndInfo.HasErrors());

			RangeMawb.NumberRangeEnd = "1000wq22";
			Assert(RangeMawb.NumberRangeEndInfo.HasErrors());

			RangeMawb.NumberRangeEnd = "10000093";
			Assert(!RangeMawb.NumberRangeEndInfo.HasErrors());

			RangeMawb.NumberRangeEnd = "";
			Assert(RangeMawb.NumberRangeEndInfo.HasErrors());
		}

		public void TestJM_MAWB()
		{
			AssertEquals("Pre-condition: MAWB should be empty", "", RangeMawb.JM_MAWB);
			AssertNoErrors(RangeMawb.JM_MAWBInfo);

			RangeMawb.Validation.ValidateJM_MAWB();

			AssertNoErrors("Expected MAWB still not to have an errors as it's not applicable to the RangeMawb", RangeMawb.JM_MAWBInfo);
		}

		#region Implementation

		RangeJobMawb RangeMawb
		{
			get { return rangeMawb ?? (rangeMawb = Factory.New<RangeJobMawb>()); }
		}

		RangeJobMawb rangeMawb;

		#endregion
	}
}
