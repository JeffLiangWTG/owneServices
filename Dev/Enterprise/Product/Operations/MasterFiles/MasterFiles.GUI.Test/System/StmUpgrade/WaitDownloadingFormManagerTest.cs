using System.Threading;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class WaitDownloadingFormManagerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestProcessStatusFormManager_UsesDbSafelyFromAnotherThread()
		{
			using (var psfManager = new WaitDownloadingFormManager())
			{
				psfManager.Start();
				Thread.Sleep(5 * 1000);
			}
		}
	}
}
