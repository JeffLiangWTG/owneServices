using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefFacility : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefFacility(int version) : base(version)
		{
		}

		public override int DataSetId => 55;

		public override string TableName => nameof(RefFacility);

		public override string DataSetName => nameof(RefFacility);

		public override string TableCode => "RFT";
	}
}
