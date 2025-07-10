using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestedType(typeof(CusSCAOceanBillDataObjectWriter))]
	sealed class CusSCAOceanBillDataObjectWriterTest : CusSCAOceanBillDataObjectWriterTest<BaseCusSCAOceanBill, BaseCusSCAHouse, BaseCusSCAContainer, BaseCusSCAPivot>
	{
		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager writeManager) => new CusSCAOceanBillDataObjectWriter(writeManager);

		protected override string OceanBillShipmentXML => TestFileHelper.GetFileContents("OceanBillShipment");

		protected override BaseCusSCAOceanBill GetNewOceanBill() => Factory.New<TestCusSCAOceanBill>();

		protected override BaseCusSCAHouse GetNewHouseBill() => Factory.New<CusSCAHouseForTest>();

		protected override BaseCusSCAContainer GetNewContainer() => Factory.New<CusSCAContainerForTest>();

		protected override BaseCusSCAPivot GetNewPivot() => Factory.New<CusSCAPivotForTest>();
	}
}
