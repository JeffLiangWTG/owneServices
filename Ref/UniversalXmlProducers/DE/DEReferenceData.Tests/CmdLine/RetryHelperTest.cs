using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine.Testing
{
	[TestFixture]
	sealed class RetryHelperTest
	{
		[Test]
		public async Task TestRetryWithDelayAsync()
		{
			var howManyTimesIHaveTried = 0;
			var action = new Func<Task<string>>(async () =>
			{
				howManyTimesIHaveTried++;
				await Task.Delay(1);
				return "ok";
			});

			var result = await RetryHelper.RetryWithDelayAsync(action, TimeSpan.FromMilliseconds(10), 10);
			Assert.That(result, Is.EqualTo("ok"));
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(1));
		}

		[Test]
		public async Task TestRetryWithDelayAsync_WithException()
		{
			var howManyTimesIHaveTried = 0;
			var action = new Func<Task<string>>(async () =>
			{
				howManyTimesIHaveTried++;
				if (howManyTimesIHaveTried < 10)
				{
					throw new Exception("blah blah");
				}
				else
				{
					await Task.Delay(1);
				}
				return "ok";
			});
			var result = await RetryHelper.RetryWithDelayAsync(action);
			Assert.That(result, Is.EqualTo("ok"));
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(10));
		}

		[Test]
		public void TestRetryWithDelayAsync_WithException_ExceedMaxRetryTimes()
		{
			var howManyTimesIHaveTried = 0;
			var action = new Func<Task<string>>(async () =>
			{
				howManyTimesIHaveTried++;
				if (howManyTimesIHaveTried < 10)
				{
					throw new Exception("blah blah");
				}
				else
				{
					await Task.Delay(1);
				}
				return "ok";
			});
			var exception = Assert.ThrowsAsync<Exception>(async () => { await RetryHelper.RetryWithDelayAsync(action, TimeSpan.FromMilliseconds(10), 5); });
			Assert.That(exception.Message, Is.EqualTo("blah blah"));
			Assert.That(howManyTimesIHaveTried, Is.EqualTo(5));
		}
	}
}
