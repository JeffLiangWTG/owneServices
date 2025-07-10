using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Test.DocumentScanning
{
	internal class NeutralMastersEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadNeutralMasters()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "081";
			jobMawb.JM_MAWB = "10000024";
			jobMawb.JM_ServiceLevel = "STD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			ZString houseBill = jobMawb.JM_Airline3DigitPrefix + jobMawb.JM_MAWB;

			var loader = new NeutralMastersEDocsViaUniversalXmlSupport();
			AssertEquals(jobMawb.PK, loader.LoadBusinessObjectFromCode(Factory, houseBill)?.PK);
		}

		public void TestTryNeutralMastersNotInDb()
		{
			var loader = new NeutralMastersEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "08199999999"));
		}
	}
}
