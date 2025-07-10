using System;
using System.Data.Odbc;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.DataAmendment;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
	[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
	public class NKCodeAmendStrategyFixtures
	{
		IUNLOCOAmendStrategy<UNLOCO> nkCodeAmendStrategy;
		[Test]
		public void TestVerifyStrategy()
		{
			Exception caughtException = null;
			try
			{
				Assert.IsTrue(nkCodeAmendStrategy.VerifyStrategy(), $"NKCode amendment strategy verification failed");
			}
			catch (OdbcException ex)
			{
				caughtException = ex;
			}
			Assert.IsNull(caughtException);
		}

		[Test]
		public void TestLoadAmendmentData()
		{
			Exception caughtException = null;
			try
			{
				nkCodeAmendStrategy.LoadAmendmentData();
			}
			catch (OdbcException ex)
			{
				caughtException = ex;
			}
			Assert.IsNull(caughtException);
		}

		[Test]
		public void TestAmend()
		{
			UNLOCO uNLOCO = new UNLOCO() { Country = "CH",Subdivision = "22"};
			nkCodeAmendStrategy.Amend(uNLOCO);
			Assert.AreEqual(uNLOCO.Subdivision, "JU");
		}

		[OneTimeSetUp]
		public void SetUp()
		{
			nkCodeAmendStrategy = new NKCodeAmendStrategy<UNLOCO>();
			if (nkCodeAmendStrategy.VerifyStrategy())
			{
				nkCodeAmendStrategy.LoadAmendmentData();
			}
		}
	}
}
