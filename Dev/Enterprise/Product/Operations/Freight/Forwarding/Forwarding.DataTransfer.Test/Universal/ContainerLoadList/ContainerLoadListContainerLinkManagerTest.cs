using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ContainerLoadListContainerLinkManagerTest : OrganizationAddressTestHelper
	{
		public void TestWrite()
		{
			var containerLinkManager = new ContainerLoadListContainerLinkManager();

			AssertEquals(1, containerLinkManager.GetContainerLink(container1));
			AssertEquals(2, containerLinkManager.GetContainerLink(container2));
			AssertEquals(3, containerLinkManager.GetContainerLink(container3));
			AssertEquals(1, containerLinkManager.GetContainerLink(container1));
			AssertEquals(2, containerLinkManager.GetContainerLink(container2));
			AssertEquals(3, containerLinkManager.GetContainerLink(container3));
		}

		public void TestRead()
		{
			var containerLinkManager = new ContainerLoadListContainerLinkManager();

			containerLinkManager.CollectContainerLink(container1, new UniversalContainer { Link = 1 });
			containerLinkManager.CollectContainerLink(container2, new UniversalContainer { Link = 2 });
			containerLinkManager.CollectContainerLink(container3, new UniversalContainer { Link = 3 });

			AssertEquals(container1, containerLinkManager.GetContainer(1));
			AssertEquals(container2, containerLinkManager.GetContainer(2));
			AssertEquals(container3, containerLinkManager.GetContainer(3));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var consol = ContainerLoadListDataObjectHelper.CreateConsol(Factory);
			container1 = ContainerLoadListDataObjectHelper.CreateContainer(Factory, consol, "CNT001");
			container2 = ContainerLoadListDataObjectHelper.CreateContainer(Factory, consol, "CNT002");
			container3 = ContainerLoadListDataObjectHelper.CreateContainer(Factory, consol, "CNT003");
		}

		ForwardingContainer container1, container2, container3;
	}
}
