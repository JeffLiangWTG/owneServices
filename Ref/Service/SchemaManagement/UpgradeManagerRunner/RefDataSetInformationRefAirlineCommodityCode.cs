using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefAirlineCommodityCode : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefAirlineCommodityCode(int version) : base(version)
		{
		}

		public override int DataSetId => 49;
		public override string TableName => nameof(RefAirlineCommodityCode);
		public override string DataSetName => nameof(RefAirlineCommodityCode);
		public override string TableCode => "RAC";
	}
}
