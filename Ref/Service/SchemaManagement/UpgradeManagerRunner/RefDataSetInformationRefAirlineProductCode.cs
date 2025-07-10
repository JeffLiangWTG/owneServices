using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefAirlineProductCode : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefAirlineProductCode(int version) : base(version)
		{
		}

		public override int DataSetId => 54;
		public override string TableName => nameof(RefAirlineProductCode);
		public override string DataSetName => nameof(RefAirlineProductCode);
		public override string TableCode => "RAR";
	}
}
