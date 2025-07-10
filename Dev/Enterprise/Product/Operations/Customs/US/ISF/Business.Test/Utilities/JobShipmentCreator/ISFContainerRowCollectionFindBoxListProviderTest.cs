using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFContainerRowCollectionFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestDescriptionFromCode()
		{
			AssertEquals("CNT1 ISOType:40FR USCode:40", listProvider.DescriptionFromCode("CNT1"));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			AssertEquals("CNT1 ISOType:40FR USCode:40", listProvider.DescriptionFromPrimaryKey(containerRow.PK));
		}

		public void TestPrimaryKeyFromCode()
		{
			AssertEquals(containerRow.PK, listProvider.PrimaryKeyFromCode("CNT1"));
		}

		public void TestCodeFromPrimaryKey()
		{
			AssertEquals("CNT1", listProvider.CodeFromPrimaryKey(containerRow.PK));
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
			var collection = new ISFContainerRowCollection(headerRow);
			containerRow = collection[0];
			listProvider = new ISFContainerRowCollectionFindBoxListProvider(collection);
		}
		ISFContainerRow containerRow;
		IFindBoxListProvider listProvider;
	}
}
