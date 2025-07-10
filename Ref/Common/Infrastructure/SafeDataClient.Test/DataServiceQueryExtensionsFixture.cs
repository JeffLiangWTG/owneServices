using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.SafeDataClient.Default;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	public class DataServiceQueryExtensionsFixture
	{
		[Test]
		public async Task IsTimingOutAsyncOperations()
		{
			var timeout = 0;

			var container = new Container(new Uri("http://localhost"));
			var fakeQuery = container.CreateQuery<RefUNLOCO>("RefUNLOCOUpdate");

			Exception ex = null;
			try
			{
				await container.RefUNLOCOUpdate.ExecuteAsync(timeout);
			}
			catch (TimeoutException te)
			{
				ex = te;
			}
			finally
			{
				Assert.NotNull(ex);
			}
		}
	}
}
