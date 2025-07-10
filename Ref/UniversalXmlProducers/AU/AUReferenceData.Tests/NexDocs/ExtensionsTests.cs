using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	class ExtensionsTests
	{
		[Test]
		public void TestGetStringCodeValue()
		{
			var itemCodeSet = new ItemCodeSet[] { TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, "BG "), TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Description, CodeValueType.@string, " US BEEF FOR GRINDING") };
			Assert.AreEqual("BG", itemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code));
			Assert.AreEqual("US BEEF FOR GRINDING", itemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description));
			Assert.AreEqual(null, itemCodeSet.GetStringCodeValue("INVALID"));

			var invalidItemCodeSet = new ItemCodeSet[] { TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, "BG"), TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.Code, CodeValueType.@string, "AM") };
			Assert.Throws<InvalidOperationException>(() => invalidItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code));
		}

		[Test]
		public void TestGetDateCodeValue()
		{
			var itemCodeSet = new ItemCodeSet[] { TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, "1983-01-01T00:00:00.000"), TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.EndDate, CodeValueType.dateTime, "null") };
			var startDateDetails = itemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			Assert.True(startDateDetails.SuccessfullyParsed);
			Assert.AreEqual(new DateTime(1983, 1, 1, 0, 0, 0), startDateDetails.DateTime);
			var endDateDetails = itemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate);
			Assert.False(endDateDetails.SuccessfullyParsed);
			var invalidDateDetails = itemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.EndDate);
			Assert.False(invalidDateDetails.SuccessfullyParsed);

			var invalidItemCodeSet = new ItemCodeSet[] { TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, "1983-01-01T00:00:00.000"), TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, "1983-01-01T00:00:00.000") };
			Assert.Throws<InvalidOperationException>(() => invalidItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate));
		}

		[Test]
		public void TestLargeFutureDateCodeValue()
		{
			var itemCodeSet = new ItemCodeSet[] { TestHelperClass.CreateItemCodeSet(NexDocConstants.ItemCodeSetKeys.StartDate, CodeValueType.dateTime, "2118-09-22T00:00:00.000") };
			var startDateDetails = itemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			Assert.True(startDateDetails.SuccessfullyParsed);
			Assert.AreEqual(Constants.RefData_Common.MaximumDateTime, startDateDetails.DateTime);
		}
	}
}
