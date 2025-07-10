using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusProfile : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusProfile(int version) : base(version)
		{
		}

		public override int DataSetId => 67;
		public override string TableName => nameof(RefCusProfile);
		public override string DataSetName => nameof(RefCusProfile);
		public override string TableCode => "XX0";
	}
}
