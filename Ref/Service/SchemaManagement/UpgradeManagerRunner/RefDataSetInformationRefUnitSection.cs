using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefUnitSection : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefUnitSection(int version) : base(version)
		{
		}

		public override int DataSetId => 66;

		public override string TableName => nameof(RefUnitSection);

		public override string DataSetName => nameof(RefUnitSection);

		public override string TableCode => "RUS";
	}
}
