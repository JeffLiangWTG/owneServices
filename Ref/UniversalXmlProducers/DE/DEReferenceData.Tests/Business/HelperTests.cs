using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.Testing
{
	[TestFixture]
	class HelperTests
	{
		[Test]
		public void TestKeepNumerics()
		{
			Assert.AreEqual("123456,789", "123.456,789".KeepNumerics());
			Assert.AreEqual("23,456", "ABC23,456K".KeepNumerics());
			Assert.AreEqual(string.Empty, "AB$%".KeepNumerics());
			Assert.AreEqual(string.Empty, string.Empty.KeepNumerics());
			Assert.AreEqual("001230056", "00123A0056".KeepNumerics());
		}

		[Test]
		public void TestGetDateTime()
		{
			Assert.AreEqual(false, Helper.GetDateTime(string.Empty, "dd.MM.yyyy").SuccessfullyParsed);
			Assert.AreEqual(false, Helper.GetDateTime("12.03.2019", string.Empty).SuccessfullyParsed);
			Assert.DoesNotThrow(() => Helper.GetDateTime("12.03.2019", "ZZ.DF.GGGG"));

			var largeDateDetails = Helper.GetDateTime("12.03.3010", "dd.MM.yyyy");
			Assert.AreEqual(true, largeDateDetails.SuccessfullyParsed);
			Assert.AreEqual(Constants.MaximumDateTime, largeDateDetails.DateTime);

			var invalidDateDetails = Helper.GetDateTime("12.15.2019", "dd.MM.yyyy");
			Assert.AreEqual(false, invalidDateDetails.SuccessfullyParsed);
			Assert.AreEqual(DateTime.MinValue, invalidDateDetails.DateTime);
		}

		[Test]
		public void CheckExpiredDateTimeTest()
		{
			var timeZoneID = Constants.GermanTimeZoneID;
			Assert.IsTrue(Helper.CheckExpiredDateTime(Constants.MinimumDateTime, timeZoneID));
			Assert.IsFalse(Helper.CheckExpiredDateTime(Constants.MaximumDateTime, timeZoneID));
		}
	}
}
