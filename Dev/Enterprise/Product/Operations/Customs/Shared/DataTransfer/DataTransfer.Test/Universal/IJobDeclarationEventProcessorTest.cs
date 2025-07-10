using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class IJobDeclarationEventProcessorTest : TestCaseWithFactory
	{
		public void TestJobDeclarationEventProcessorsFactory_All()
		{
			var result = JobDeclarationEventProcessorsFactory.All;
			AssertEquals(3, result.Count());

			var processor1 = ObjectFactory.Get("CSWResponseMessageEventProcessor");
			var processor2 = ObjectFactory.Get("eBondMessageToSuretyAgent");
			var processor3 = ObjectFactory.Get("AUNEXDOCEventProcessor");
			Assert(result.Any(x => x.GetType() == processor1.GetType()));
			Assert(result.Any(x => x.GetType() == processor2.GetType()));
			Assert(result.Any(x => x.GetType() == processor3.GetType()));
		}
	}
}
