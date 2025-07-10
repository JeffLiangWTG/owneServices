using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationUNDGVersion : RefDataSetInformationTransformation
	{
		public RefDataSetInformationUNDGVersion(int version) : base(version)
		{
		}

		public override int DataSetId => 74;
		public override string TableName => nameof(UNDGVersion);
		public override string DataSetName => nameof(UNDGVersion);
		public override string TableCode => "DV";
	}
}
