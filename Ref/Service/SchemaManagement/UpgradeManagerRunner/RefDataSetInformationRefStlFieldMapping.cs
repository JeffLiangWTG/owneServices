using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefStlFieldMapping : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefStlFieldMapping(int version) : base(version)
		{
		}

		public override int DataSetId => 56;
		public override string TableName => nameof(RefStlFieldMapping);
		public override string DataSetName => nameof(RefStlFieldMapping);
		public override string TableCode => "SFM";
	}
}
