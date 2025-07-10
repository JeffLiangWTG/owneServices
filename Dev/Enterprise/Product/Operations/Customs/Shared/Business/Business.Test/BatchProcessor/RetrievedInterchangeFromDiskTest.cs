using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class RetrievedInterchangeFromDiskTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRetrievedTime()
		{
			RetrievedInterchangeFromDisk testInterchange = new RetrievedInterchangeFromDisk();
			Assert("RetrievedTimeEmpty", testInterchange.RetrievedTime == ZDateTime.Empty);
			testInterchange.Filename = BaseSourcePath + @"Build.xml";
			Assert("RetrievedTimeNotEmpty", testInterchange.RetrievedTime != ZDateTime.Empty);
		}
	}
}
