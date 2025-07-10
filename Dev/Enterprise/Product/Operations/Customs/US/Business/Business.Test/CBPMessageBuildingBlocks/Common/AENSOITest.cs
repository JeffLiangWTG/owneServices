using Enterprise.Customs.US.Business.BIRD.ACS;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Common.Testing
{
	sealed class AENSOITest : TestCase
	{
		public void TestIBIRDOGAStartRecord()
		{
			var oi = new AENSOI();
			oi.CommercialDescriptionText = "Blah blah blah";
			AssertEquals("Blah blah blah", ((IBIRDOGAStartRecord)oi).CommercialDesc);
		}
	}
}
