using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderContainerDataObjectWriterTest : OrganizationAddressTestHelper
	{
		#region TestBasicFieldLevelMappings

		public void TestBasicFieldLevelMappings()
		{
			var containerBO = SetupContainer(Factory.BOFactory);
			var writer = new OrderContainerDataObjectWriter<OrderContainer>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerData = writer.GetDataObject(containerBO);
			AssertNotNull(containerData);

			CombineAssertions(delegate
			{
				AssertContents(containerData);
			});
		}

		internal static OrderContainer SetupContainer(BusinessObjectFactory factory)
		{
			var container = factory.New<OrderContainer>();
			container.J1_Additional2SealNum = "SEAL3";
			container.J1_AdditionalSealNum = "SEAL2";
			container.J1_ContainerNumber = "OC112234";
			container.J1_ContainerCount = 3;
			container.J1_SealNum = "SEAL1";
			container.J1_RC = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20NOR")).PK;
			return container;
		}

		internal static void AssertContents(Container container)
		{
			AssertEquals("container.Seal", "SEAL1", container.Seal);
			AssertEquals("container.SecondSeal", "SEAL2", container.SecondSeal);
			AssertEquals("container.ThirdSeal", "SEAL3", container.ThirdSeal);
			AssertEquals("container.ContainerCount", 3, container.ContainerCount);
			AssertEquals("container.ContainerNumber", "OC112234", container.ContainerNumber);
			AssertEquals("container.ContainerType.Code", "20NOR", container.ContainerType.Code);
			AssertEquals("container.ContainerType.Description", "Twenty foot non-operating reefer", container.ContainerType.Description);
			AssertEquals("container.ContainerType.ISOCode", "22R0", container.ContainerType.ISOCode);
		}

		#endregion
	}
}
