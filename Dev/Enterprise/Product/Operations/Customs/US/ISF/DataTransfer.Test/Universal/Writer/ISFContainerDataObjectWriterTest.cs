using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer.Testing
{
	sealed class ISFContainerDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestISFLineMapping()
		{
			var equipBO = SetupEquipment();
			var writer = new ISFContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, equipBO)));
			var equipDataObject = writer.GetDataObject(equipBO);
			AssertEquals(equipBO.BE_ContainerNum, equipDataObject.ContainerNumber);
			AssertEquals("equipDataObject.AddInfoCollection.Count", 2, equipDataObject.AddInfoCollection.Count);
			var containerType = equipDataObject.AddInfoCollection.GetZStringValue(ISFConstants.ContainerConstants.USContainerType);
			AssertEquals(equipBO.BE_EquipCode, containerType.GetValueOrDefault());
			var isoCodeData = equipDataObject.AddInfoCollection.GetZStringValue(ISFConstants.ContainerConstants.USContainerType);
			AssertEquals(equipBO.BE_EquipCode, isoCodeData.GetValueOrDefault());
		}

		CusISFEquip SetupEquipment()
		{
			var equip = Factory.New<CusISFEquip>();
			equip.BE_EquipCode = "20";
			equip.BE_ContainerNum = "TURE2345321";
			equip.BE_ContainerISO = "40FR";
			return equip;
		}
	}
}
