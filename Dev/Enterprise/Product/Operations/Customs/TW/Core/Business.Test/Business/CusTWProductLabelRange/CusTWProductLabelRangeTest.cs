using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWProductLabelRange))]
	sealed class CusTWProductLabelRangeTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestTW0_Status_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(productLabelRange.TW0_StatusInfo, "Status", "The code for the registration status of the product label.");
		}

		[ExpectNoExceptions]
		public void TestTW0_EndNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(productLabelRange.TW0_EndNumberInfo, "End Number", "End No.", "The ending number of the serial number of product label or the specified code.");
		}

		[ExpectNoExceptions]
		public void TestTW0_StartNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(productLabelRange.TW0_StartNumberInfo, "Start Number", "Start No.", "The starting number of the serial number of product label or the specified code.");
		}

		[ExpectNoExceptions]
		public void TestTW0_RunNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(productLabelRange.TW0_RunNumberInfo, "Run Number", "Run No.", "The track of the product label.");
		}

		[ExpectNoExceptions]
		public void TestTW0_Year_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(productLabelRange.TW0_YearInfo, "Year", "The year of the product label.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			productLabelRange = Factory.NewWithValidTestData<CusTWProductLabelRange>();
		}

		CusTWProductLabelRange productLabelRange;
	}
}
