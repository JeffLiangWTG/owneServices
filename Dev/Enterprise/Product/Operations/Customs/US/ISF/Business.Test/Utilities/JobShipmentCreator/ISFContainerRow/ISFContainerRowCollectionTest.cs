using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFContainerRowCollection))]
	sealed class ISFContainerRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ISFContainerRowCollection>
	{
		public void TestHasContainerMarkedForCopy()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "CNT1";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = ZString.Empty;
			CusISFEquip container3 = header.Equipments.AddNew();
			container3.BE_ContainerNum = "CNT3";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFContainerRowCollection collection = new ISFContainerRowCollection(headerRow);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.HasContainerMarkedForCopy);
			ISFContainerRow containerRow1 = collection[0];
			containerRow1.ShouldCopy = false;
			ISFContainerRow containerRow2 = collection[1];
			containerRow2.ShouldCopy = false;
			AssertEquals(false, collection.HasContainerMarkedForCopy);
			containerRow2.ShouldCopy = true;
			AssertEquals(true, collection.HasContainerMarkedForCopy);
			containerRow2.ShouldCopy = false;
			containerRow1.ShouldCopy = true;
			AssertEquals(true, collection.HasContainerMarkedForCopy);
		}

		public void TestAllowFeature()
		{
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestFindBoxListProvider()
		{
			AssertEquals("CNT1 ISOType:40FR USCode:40", listProvider.DescriptionFromCode("CNT1"));
			AssertEquals("CNT1 ISOType:40FR USCode:40", listProvider.DescriptionFromPrimaryKey(containerRow.PK));
			AssertEquals(containerRow.PK, listProvider.PrimaryKeyFromCode("CNT1"));
			AssertEquals("CNT1", listProvider.CodeFromPrimaryKey(containerRow.PK));
		}

		protected override ISFContainerRowCollection GetCollectionToTest()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			return new ISFContainerRowCollection(headerRow);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusISFHeader>();
			return new ISFContainerRow(header.Equipments.AddNew(), new ISFHeaderRow(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusISFHeader>();
			var container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "CNT1";
			container1.BE_ContainerISO = "40FR";
			container1.BE_EquipCode = "40";
			var container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = ZString.Empty;
			var headerRow = new ISFHeaderRow(header);
			collection = new ISFContainerRowCollection(headerRow);
			containerRow = collection[0];
			listProvider = collection;
		}
		ISFContainerRowCollection collection;
		ISFContainerRow containerRow;
		IFindBoxListProvider listProvider;
	}
}
