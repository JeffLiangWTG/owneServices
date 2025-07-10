using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefClient : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefClient(int version) : base(version)
		{
		}

		public override int DataSetId => 57;
		public override string TableName => nameof(RefClient);
		public override string DataSetName => nameof(RefClient);
		public override string TableCode => "RCT";	}
}
