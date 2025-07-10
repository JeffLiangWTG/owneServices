using System;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class CollectionDateHelperFixture
	{
		[Test]
		public void CollectionDateIsReturningValidatingHolidays()
		{
			var collectionTime = new DateTime(2018, 2, 5);
			var result = CollectionDateHelper.DefineCollectionDate(collectionTime);

			Assert.That(result == new DateTime(2018, 2, 28));
		}

		[Test]
		public void CollectionDateIsThrowingErrorWhenExcessTheAcceptableYear()
		{
			var collectionTime = new DateTime(2026, 2, 5);

			Assert.Throws<InvalidOperationException>(() => CollectionDateHelper.DefineCollectionDate(collectionTime));
		}

		[Test]
		public void IsReturningThirdWednesdayWhenInTheSameMonthAnyDayAfter()
		{
			var collectionTime = new DateTime(2018, 4, 25);
			var result = CollectionDateHelper.DefineCollectionDate(collectionTime);

			Assert.That(result == new DateTime(2018, 4, 18));
		}
	}
}
