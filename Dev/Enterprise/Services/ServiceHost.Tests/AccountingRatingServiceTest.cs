using System;
using System.Threading;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class AccountingRatingServiceTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAutoRate_UsesDbSafelyFromAnotherThread()
		{
			var accountingRating = new AccountingRatingController();

			var threadstart1 = new ThreadStart(delegate
			{
				var operationsJobPk = Guid.Empty;
				var operationsJobTableCode = "";

				accountingRating.AutoRateAndCreateJobHeader(operationsJobPk, operationsJobTableCode, Guid.Empty, true, true, LogType.Information);
			});

			var thread1 = new Thread(threadstart1);
			thread1.Start();
			thread1.Join(1000);
		}
	}
}
