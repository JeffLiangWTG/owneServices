using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class RetryExtensionsFixture
	{
		[Test]
		public async Task Retry()
		{
			var noOfCall = 0;

			Task<int> Action()
			{
				noOfCall++;
				switch (noOfCall)
				{
					case 1:
						throw new NotSupportedException("XXX");
					case 2:
						throw new NotImplementedException("YYY");
					default:
						return Task.FromResult(1000);
				}
			}

			await RetryExtensions.Retry<int, NotSupportedException, NotImplementedException>(
				Action,
				3,
				() => { },
				x => x.Message == "XXX",
				x => x.Message == "YYY"
			);
			Assert.AreEqual(3, noOfCall);
		}

		[Test]
		public void Retry_ThrowExceptionInLastTry()
		{
			var noOfCall = 0;
			Task<int> Action()
			{
				noOfCall++;
				throw new NotSupportedException($"Try {noOfCall} times");
			}

			var maxRetries = 3;
			var exception = Assert.ThrowsAsync<NotSupportedException>(async () =>
			await RetryExtensions.Retry<int, NotSupportedException, NotImplementedException>(
				Action,
				maxRetries,
				() => { },
				x => x.Message.StartsWith("Try", StringComparison.OrdinalIgnoreCase),
				x => x.Message == "XXX"
			));
			Assert.AreEqual("Try 3 times", exception.Message);
			Assert.AreEqual(maxRetries, noOfCall);
		}
	}
}
