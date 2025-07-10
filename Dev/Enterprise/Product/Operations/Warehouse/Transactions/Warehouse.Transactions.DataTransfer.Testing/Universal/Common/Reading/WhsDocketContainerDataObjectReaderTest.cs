using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsDocketContainerDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region TestBasicContainerLevelFieldMappings

		public void TestBasicContainerLevelFieldMappings()
		{
			var containerDataObject = SetupContainer();
			var reader = new WhsDocketContainerDataObjectReader(containerDataObject, Logger, Factory, null);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull("containerBO", containerBO);

			CombineAssertions(delegate
			{
				AssertContents(containerBO);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching WhsDocketContainer found, creating new WhsDocketContainer.
Information - Populating WhsDocketContainer...
Information - Successfully loaded matching Container Type.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region TestLoadingContainer

		public void TestLoadingContainer()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			var containerToLoad = whsOrder.Containers.AddNew();
			containerToLoad.WC_ContainerNum = "OOCCC1111";
			containerToLoad.WC_SealNum = "I WILL CHANGE";

			Factory.SaveForTesting();

			var containerDataObject = SetupContainer();
			var reader = new WhsDocketContainerDataObjectReader(containerDataObject, Logger, Factory, whsOrder);
			var containerBO = reader.ReadIntoBusinessObject();

			AssertNotNull("containerBO", containerBO);

			CombineAssertions(delegate
			{
				AssertContents(containerBO);
				AssertEquals("containerBO.PK", containerToLoad.PK, containerBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching WhsDocketContainer.
Information - Populating WhsDocketContainer...
Information - Successfully loaded matching Container Type.
".Trim(), Logger.Logs);
			});
		}

		#endregion

		#region Implementation

		internal static Container SetupContainer()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "OOCCC1111";
			containerDataObject.IsChargeable = true;
			containerDataObject.IsPalletised = true;
			containerDataObject.ItemCount = 3;
			containerDataObject.PalletCount = 2;
			containerDataObject.Seal = "s1111";
			containerDataObject.ContainerType = new ContainerType { Code = "20GP", Description = "Twenty foot general purpose", ISOCode = "22G0" };

			return containerDataObject;
		}

		internal static void AssertContents(WhsDocketContainer containerBO)
		{
			AssertEquals("containerBO.WC_ContainerNum", "OOCCC1111", containerBO.WC_ContainerNum);
			AssertEquals("containerBO.WC_IsChargeable", ZBool.True, containerBO.WC_IsChargeable);
			AssertEquals("containerBO.WC_IsPalletised", ZBool.True, containerBO.WC_IsPalletised);
			AssertEquals("containerBO.WC_ItemCount", 3, containerBO.WC_ItemCount);
			AssertEquals("containerBO.WC_PalletCount", 2, containerBO.WC_PalletCount);
			AssertEquals("containerBO.WC_SealNum", "s1111", containerBO.WC_SealNum);
			AssertEquals("containerBO.WC_RC", containerBO.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK, containerBO.WC_RC);
		}

		#endregion
	}
}