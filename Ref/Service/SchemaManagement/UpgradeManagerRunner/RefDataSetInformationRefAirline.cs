using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefAirline : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefAirline(int version) : base(version)
		{
		}

		public override int DataSetId => 43;
		public override string TableName => nameof(RefAirline);
		public override string DataSetName => nameof(RefAirline);
		public override string TableCode => "RM";
	}
}
