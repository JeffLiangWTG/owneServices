using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefGlbReleaseNote : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefGlbReleaseNote(int version) : base(version)
		{
		}

		public override int DataSetId => 72;
		public override string TableName => nameof(RefGlbReleaseNote);
		public override string DataSetName => nameof(RefGlbReleaseNote);
		public override string TableCode => "ZGF";
	}
}
