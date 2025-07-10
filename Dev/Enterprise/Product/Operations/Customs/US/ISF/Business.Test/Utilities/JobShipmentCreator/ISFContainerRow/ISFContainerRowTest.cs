using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFContainerRow))]
	sealed class ISFContainerRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerNumber()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var equip = header.Equipments.AddNew();
			var containerRow = new ISFContainerRow(equip, headerRow);
			AssertEquals(ZString.Empty, containerRow.ContainerNumber);
			equip.BE_ContainerNum = "TURE23423424";
			AssertEquals("TURE23423424", containerRow.ContainerNumber);
		}

		public void TestContainerISOType()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var equip = header.Equipments.AddNew();
			var containerRow = new ISFContainerRow(equip, headerRow);
			AssertEquals(ZString.Empty, containerRow.ContainerISOType);
			equip.BE_ContainerISO = "22PI";
			AssertEquals("22PI", containerRow.ContainerISOType);
		}

		public void TestContainerUSCode()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var equip = header.Equipments.AddNew();
			var containerRow = new ISFContainerRow(equip, headerRow);
			AssertEquals(ZString.Empty, containerRow.ContainerUSCode);
			equip.BE_EquipCode = "40";
			AssertEquals("40", containerRow.ContainerUSCode);
		}

		public void TestContainerDescription()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var equip = header.Equipments.AddNew();
			var containerRow = new ISFContainerRow(equip, headerRow);
			AssertEquals("", containerRow.ContainerDescription);
			equip.BE_ContainerNum = "TURE2345321";
			AssertEquals("TURE2345321", containerRow.ContainerDescription);
			equip.BE_ContainerISO = "40FR";
			AssertEquals("TURE2345321 ISOType:40FR", containerRow.ContainerDescription);
			equip.BE_EquipCode = "40";
			AssertEquals("TURE2345321 ISOType:40FR USCode:40", containerRow.ContainerDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			return new ISFContainerRow(header.Equipments.AddNew(), headerRow);
		}
	}
}
