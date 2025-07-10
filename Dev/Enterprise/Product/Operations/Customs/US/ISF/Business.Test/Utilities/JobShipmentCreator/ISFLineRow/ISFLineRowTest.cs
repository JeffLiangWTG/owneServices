using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFLineRow))]
	sealed class ISFLineRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHarmonisedNum()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var line = header.Lines.AddNew();
			var lineRow = new ISFLineRow(line, headerRow);
			AssertEquals(ZString.Empty, lineRow.HarmonisedNum);
			line.BL_HarmonisedNum = "10203040";
			AssertEquals("1020.30.40", lineRow.HarmonisedNum);
		}

		public void TestGoodsOrigin()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var line = header.Lines.AddNew();
			var lineRow = new ISFLineRow(line, headerRow);
			AssertEquals(ZString.Empty, lineRow.GoodsOrigin);
			line.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, lineRow.GoodsOrigin);
		}

		public void TestContainerPK()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var line = header.Lines.AddNew();
			var lineRow = new ISFLineRow(line, headerRow);
			AssertEquals(true, lineRow.ContainerPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, lineRow.ContainerPK);
			AssertNull(lineRow.Container);
			var equip = header.Equipments.AddNew();
			var containerRow = new ISFContainerRow(equip, headerRow);
			headerRow.Containers.Add(containerRow);
			AssertEquals(false, lineRow.ContainerPKInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, lineRow.ContainerPK);
			AssertNull(lineRow.Container);
			lineRow.ContainerPK = containerRow.PK;
			AssertEquals(false, lineRow.ContainerPKInfo.ReadOnly);
			AssertEquals(containerRow.PK, lineRow.ContainerPK);
			AssertEquals(equip, lineRow.Container);
		}

		public void TestContainers()
		{
			var header = Factory.New<CusISFHeader>();
			var line = header.Lines.AddNew();
			var headerRow = new ISFHeaderRow(header);
			var lineRow = new ISFLineRow(line, headerRow);
			AssertEquals(0, lineRow.Containers.Count);
			var equip = header.Equipments.AddNew();
			equip.BE_ContainerNum = "TURE23423";
			headerRow = new ISFHeaderRow(header);
			lineRow = new ISFLineRow(line, headerRow);
			AssertEquals(1, lineRow.Containers.Count);
			AssertEquals(equip, lineRow.Containers[0].Container);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusISFHeader>();
			var headerRow = new ISFHeaderRow(header);
			var line = header.Lines.AddNew();
			return new ISFLineRow(line, headerRow);
		}
	}
}
