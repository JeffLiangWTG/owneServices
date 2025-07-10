using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefMessagingBussPackageInfo : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefMessagingBussPackageInfo(int version) : base(version)
		{
		}

		public override int DataSetId => 60;

		public override string TableName => nameof(RefMessagingBussPackageInfo);

		public override string DataSetName => nameof(RefMessagingBussPackageInfo);

		public override string TableCode => "ZMP";
	}
}
