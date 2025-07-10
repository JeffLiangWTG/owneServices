using System.IO;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	sealed class UnclosableStreamTest : TestCaseWithFactory
	{
		public void TestCloseStream()
		{
			using (var stream = new MemoryStream())
			{
				var uncloseSteam = new UnclosableStream(stream);
				uncloseSteam.Close();
				Assert("Stream should not be closed", stream.CanRead);
			}
		}
	}
}
