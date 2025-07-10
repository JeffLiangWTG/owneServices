using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFEquip))]
	sealed class CusISFEquipTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIContainerDataMembers()
		{
			var container = Factory.New<CusISFEquip>();
			container.BE_ContainerISO = "2032";
			container.BE_ContainerNum = "TURE2342318";
			container.BE_EquipCode = "40";
			IContainerData containerData = container;
			AssertEquals("40", containerData.EquipmentDescriptionCode);
			AssertEquals("TURE", containerData.EquipmentInitial);
			AssertEquals("234231", containerData.EquipmentNumber);
			AssertEquals("8", containerData.EquipmentNumberCheckDigit);
			AssertEquals("2032", containerData.EquipmentSizeTypeCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return header.Equipments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusISFHeader>();
			header.BF_OH_Importer = factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return header.Equipments.AddNew();
		}
	}
}
