using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsDocketContainerDataObjectWriterTest : TestCaseWithFactory
	{
		#region TestBasicContainerLevelFieldMappings

		public void TestBasicContainerLevelFieldMappings()
		{
			var containerBO = GetContainer(Factory);
			var containerData = new WhsDocketContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO))).GetDataObject(containerBO);

			AssertNotNull("containerData", containerData);

			CombineAssertions(delegate
			{
				AssertContents(containerData);
			});
		}

		#endregion

		#region TestContainersAlreadyOnParentCollectionAreDeDuplicated

		public void TestContainersAlreadyOnParentCollectionAreDeDuplicated()
		{
			var containerBO = GetContainer(Factory);
			var matchingContainer = new Container
			{
				ContainerNumber = "OOCCC1111",
				IsChargeable = false,
				IsPalletised = false,
				ItemCount = 1,
				PalletCount = 2,
				Seal = "s2222"
			};

			var containers = new DataObjectList<Container> { matchingContainer };
			var containerData = new WhsDocketContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)), containers).GetDataObject(containerBO);

			AssertNotNull("containerData", containerData);

			CombineAssertions(delegate
			{
				AssertContents(containerData);
				AssertEquals("Same Container is used", matchingContainer, containerData);
				AssertEquals("Container is removed from the collection", 0, containers.Count);
			});
		}

		#endregion

		#region Implementation

		internal static WhsDocketContainer GetContainer(BusinessObjectFactory factory)
		{
			var containerBO = factory.New<WhsDocketContainer>();
			containerBO.WC_ContainerNum = "OOCCC1111";
			containerBO.WC_IsChargeable = ZBool.True;
			containerBO.WC_IsPalletised = ZBool.True;
			containerBO.WC_ItemCount = 3;
			containerBO.WC_PalletCount = 4;
			containerBO.WC_SealNum = "S1111";

			var containerType = factory.New<RefContainer>();
			containerType.RC_Code = "ZW0W";
			containerType.RC_Description = "Container Type WOW!!";
			containerType.RC_ISOType = "21G5";
			containerBO.WC_RC = containerType.PK;

			return containerBO;
		}

		internal static void AssertContents(Container containerDataObject)
		{
			AssertEquals("containerData.ContainerNumber", "OOCCC1111", containerDataObject.ContainerNumber);
			AssertEquals("containerData.ContainerType.Code", "ZW0W", containerDataObject.ContainerType.Code);
			AssertEquals("containerData.ContainerType.Description", "Container Type WOW!!", containerDataObject.ContainerType.Description);
			AssertEquals("containerData.ContainerType.ISOCode", "21G5", containerDataObject.ContainerType.ISOCode);
			AssertEquals("containerData.Seal", "S1111", containerDataObject.Seal);
			AssertEquals("containerData.IsChargeable", ZBool.True, containerDataObject.IsChargeable);
			AssertEquals("containerData.IsPalletised", ZBool.True, containerDataObject.IsPalletised);
			AssertEquals("containerData.ItemCount", 3, containerDataObject.ItemCount);
			AssertEquals("containerData.PalletCount", 4, containerDataObject.PalletCount);
		}

		#endregion
	}
}