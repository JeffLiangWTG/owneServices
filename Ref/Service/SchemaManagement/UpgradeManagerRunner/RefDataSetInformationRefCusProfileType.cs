using CargoWise.RefDbRepo.Service.Schema_0_9_New;
namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusProfileType : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusProfileType(int version) : base(version)
		{
		}
		public override int DataSetId => 64;
		public override string TableName => nameof(RefCusProfileType);
		public override string DataSetName => nameof(RefCusProfileType);
		public override string TableCode => "XXX";
	}
}
