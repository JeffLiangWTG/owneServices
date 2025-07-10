using System;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	class HelperTest
	{
		[Test]
		public void TestGetDateTime()
		{
			Assert.AreEqual(false, Helper.GetDateTime(string.Empty, "yyyyMMdd").SuccessfullyParsed);
			Assert.AreEqual(false, Helper.GetDateTime("20221004", string.Empty).SuccessfullyParsed);
			Assert.DoesNotThrow(() => Helper.GetDateTime("20201112", "ggggDFLK"));

			var invalidDateDetails = Helper.GetDateTime("20202020", "yyyyMMdd");
			Assert.AreEqual(false, invalidDateDetails.SuccessfullyParsed);
			Assert.AreEqual(DateTime.MinValue, invalidDateDetails.DateTime);
		}
	}
}
