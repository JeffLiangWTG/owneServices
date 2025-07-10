using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SendingarnumerNumberFountainsTest : TestCase
	{
		public void TestSendingarnumerNumberFountainsInstances()
		{
			foreach (var numberFountain in new[]
				{
					SendingarnumerNumberFountains.Instance.SendingarnumerExportAir,
					SendingarnumerNumberFountains.Instance.SendingarnumerImportAir,
					SendingarnumerNumberFountains.Instance.SendingarnumerExportSea,
					SendingarnumerNumberFountains.Instance.SendingarnumerImportSea
				}
				)
			{
				numberFountain.Reset();
				for (var i = 1; i <= 999; i++)
				{
					AssertEquals("should be 1", i.ToString().PadLeft(3, '0'), numberFountain.GetNextCarrierNumber());
				}
				AssertEquals("001", numberFountain.GetNextCarrierNumber());
				AssertEquals("002", numberFountain.GetNextCarrierNumber());
			}
		}
	}
}
