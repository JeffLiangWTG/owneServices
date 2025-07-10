using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusConfiguration : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusConfiguration(int version) : base(version)
		{
		}

		public override int DataSetId => 52;
		public override string TableName => nameof(RefCusConfiguration);
		public override string DataSetName => nameof(RefCusConfiguration);
		public override string TableCode => "ZZJ";
	}
}
