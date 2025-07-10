using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderContainerDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region TestBasicFieldLevelContainerLevelMappings

		public void TestBasicFieldLevelContainerLevelMappings()
		{
			var containerDataObject = GetNewContainerDataObject();

			var reader = new OrderContainerDataObjectReader<OrderContainer>(containerDataObject, Logger, Factory);
			var plannedContainer = reader.ReadIntoBusinessObject();

			AssertNotNull(plannedContainer);
			CombineAssertions(delegate
			{
				AssertContents(plannedContainer);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		internal static Container GetNewContainerDataObject()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerCount = 2;
			containerDataObject.ContainerNumber = "OC12452";
			containerDataObject.ContainerType = new ContainerType { Code = "20NOR", ISOCode = "22R0" };
			containerDataObject.Seal = "Seal1";
			containerDataObject.SecondSeal = "Seal2";
			containerDataObject.ThirdSeal = "Seal3";
			return containerDataObject;
		}

		internal static void AssertContents(OrderContainer plannedContainer)
		{
			AssertEquals("plannedContainer.J1_Additional2SealNum", "Seal3", plannedContainer.J1_Additional2SealNum);
			AssertEquals("plannedContainer.J1_AdditionalSealNum", "Seal2", plannedContainer.J1_AdditionalSealNum);
			AssertEquals("plannedContainer.J1_ContainerCount", new ZShort(2), plannedContainer.J1_ContainerCount);
			AssertEquals("plannedContainer.J1_ContainerNumber", "OC12452", plannedContainer.J1_ContainerNumber);
			AssertEquals("plannedContainer.J1_SealNum", "Seal1", plannedContainer.J1_SealNum);
			AssertEquals("plannedContainer.Container.RC_Code", "20NOR", plannedContainer.Container.RC_Code);
		}

		#endregion
	}
}
