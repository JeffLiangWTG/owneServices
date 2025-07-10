using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSContainerProviderTest : TestCaseWithFactory
	{
		public void TestContainerMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var goodsItems = nctsHeaderProvider.GoodsItems.ToArray();
				var containers = goodsItems[0].Containers.ToArray();

				CombineAssertions("Container members for first goodsItems with different levels", () =>
				{
					AssertEquals("ContainerNumber1", "ABCD1234560", containers[0].ContainerNumber);
					AssertEquals("ContainerNumber2", "ABCD1234562", containers[1].ContainerNumber);
				});

				containers = goodsItems[1].Containers.ToArray();

				CombineAssertions("Container members for second goodsItems with different levels", () =>
				{
					AssertEquals("ContainerNumber1", "ABCD1234561", containers[0].ContainerNumber);
					AssertEquals("ContainerNumber2", "ABCD1234562", containers[1].ContainerNumber);
				});
			}
		}
	}
}
