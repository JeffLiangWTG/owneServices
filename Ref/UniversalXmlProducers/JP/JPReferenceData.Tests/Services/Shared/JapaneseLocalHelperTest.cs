using System;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class JapaneseLocalHelperTest
	{
		[Test]
		public void TestRetrieveTimeFromDescritpionWithJapaneseEras()
		{
			var testDescription1 = "1234567890";
			(var isFromNowOn, var result) = JapaneseLocalHelper.RetrieveTimeFromDescritpionWithJapaneseEras(testDescription1);
			Assert.AreEqual(false, isFromNowOn);
			Assert.AreEqual(DateTime.MaxValue, result);

			var testDescription2 = "平成1年2月3日以前";
			(isFromNowOn, result) = JapaneseLocalHelper.RetrieveTimeFromDescritpionWithJapaneseEras(testDescription2);
			Assert.AreEqual(false, isFromNowOn);
			Assert.AreEqual(new DateTime(1990, 2, 3), result);

			var testDescription3 = "令和1年2月3日以降";
			(isFromNowOn, result) = JapaneseLocalHelper.RetrieveTimeFromDescritpionWithJapaneseEras(testDescription3);
			Assert.AreEqual(true, isFromNowOn);
			Assert.AreEqual(new DateTime(2020, 2, 3), result);
		}
	}
}
