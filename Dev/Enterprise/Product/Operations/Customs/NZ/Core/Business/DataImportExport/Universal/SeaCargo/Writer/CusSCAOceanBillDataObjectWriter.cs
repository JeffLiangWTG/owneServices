using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAOceanBillDataObjectWriter : CusSCAOceanBillDataObjectWriter<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPackingLine>
	{
		public CusSCAOceanBillDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override CusSCAHouseDataObjectWriter<CusSCAHouse, CusSCAPackingLine> GetNewCusSCAHouseDataObjectWriter()
		{
			return new CusSCAHouseDataObjectWriter(writeManager);
		}

		protected override CusSCAContainerDataObjectWriter<CusSCAContainer> GetNewCusSCAContainerDataObjectWriter()
		{
			return new CusSCAContainerDataObjectWriter(writeManager);
		}
	}
}
