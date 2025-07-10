using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefVessel : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefVessel(int version) : base(version)
		{
		}

		public override int DataSetId => 46;
		public override string TableName => nameof(RefVessel);
		public override string DataSetName => nameof(RefVessel);
		public override string TableCode => "RV";
	}
}
