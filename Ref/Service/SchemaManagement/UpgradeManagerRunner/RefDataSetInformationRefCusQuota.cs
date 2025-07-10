using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusQuota : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusQuota(int version) : base(version)
		{
		}

		public override int DataSetId => 53;
		public override string TableName => nameof(RefCusQuota);
		public override string DataSetName => nameof(RefCusQuota);
		public override string TableCode => "ZXQ";	}
}
