using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ContainerBusinessObjectFinder<>))]
	sealed class ContainerBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var dataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ContainerNumber = "AAA";

			var container1 = Factory.New<CommonContainer>();
			container1.JC_ContainerNum = "111";

			var container2 = Factory.New<CommonContainer>();
			container2.JC_ContainerNum = "222";

			var container3 = Factory.New<CommonContainer>();
			container3.JC_ContainerNum = "333";

			var finder = new ContainerBusinessObjectFinder<CommonContainer>(dataObject);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));

			dataObject.ContainerNumber = "222";

			finder = new ContainerBusinessObjectFinder<CommonContainer>(dataObject);
			AssertEquals(container2, finder.Find(new[] { container1, container2, container3 }));

			dataObject.ContainerNumber = ZString.Empty;

			finder = new ContainerBusinessObjectFinder<CommonContainer>(dataObject);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));

			dataObject.ContainerNumber = null;

			finder = new ContainerBusinessObjectFinder<CommonContainer>(dataObject);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
		}
	}
}
