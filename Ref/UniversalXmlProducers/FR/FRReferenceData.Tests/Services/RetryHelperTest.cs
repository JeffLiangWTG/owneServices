using System;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	sealed class RetryHelperTest
	{
		[Test]
		public void TestRetryWithDelay()
		{
			var result = false;
			var howManyTimesIHaveTried = 0;
			var action = new Action(() =>
			{
				howManyTimesIHaveTried++;
				result = true;
			});
			RetryHelper.RetryWithDelay(action, TimeSpan.FromMilliseconds(10), 10);
			Assert.That(result, Is.True);
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(1));
		}

		[Test]
		public void TestRetryWithDelay_WithException()
		{
			var result = false;
			var howManyTimesIHaveTried = 0;
			var action = new Action(() =>
			{
				howManyTimesIHaveTried++;
				if (howManyTimesIHaveTried < 10)
				{
					throw new Exception("blah blah");
				}
				else
				{
					result = true;
				}
			});
			RetryHelper.RetryWithDelay(action, TimeSpan.FromMilliseconds(10), 15);
			Assert.That(result, Is.True);
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(10));
		}

		[Test]
		public void TestRetryWithDelay_WithException_ExceedMaxRetryTimes()
		{
			var result = false;
			var howManyTimesIHaveTried = 0;
			var action = new Action(() =>
			{
				howManyTimesIHaveTried++;
				if (howManyTimesIHaveTried < 10)
				{
					throw new Exception("blah blah");
				}
				else
				{
					result = true;
				}
			});
			var exception = Assert.Throws<Exception>(() => { RetryHelper.RetryWithDelay(action, TimeSpan.FromMilliseconds(10), 5); });
			Assert.That(exception.Message, Is.EqualTo("blah blah"));
			Assert.That(result, Is.False);
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(5));
		}
	}
}
