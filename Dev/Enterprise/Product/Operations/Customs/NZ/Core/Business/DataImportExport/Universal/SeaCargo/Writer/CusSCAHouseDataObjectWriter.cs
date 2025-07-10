using CargoWise.Integration;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAHouseDataObjectWriter : CusSCAHouseDataObjectWriter<CusSCAHouse, CusSCAPackingLine>
	{
		public CusSCAHouseDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ICodeDescriptionPairList GetShipmentStatuses()
		{
			return Factory.GetCachedValue<LowValueConsignmentStatusList>();
		}

		protected override ICodeDescriptionPairList GetMessageStatuses()
		{
			return Factory.GetCachedValue<LowValueConsignmentStatusList>();
		}

		protected override CusSCAPivotDataObjectWriter<CusSCAPackingLine> GetNewCusSCAPivotDataObjectWriter()
		{
			return new CusSCAPivotDataObjectWriter(writeManager);
		}
	}
}
