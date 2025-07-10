using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefStlScript : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefStlScript(int version) : base(version)
		{
		}

		public override int DataSetId => 51;
		public override string TableName => nameof(RefStlScript);
		public override string DataSetName => nameof(RefStlScript);
		public override string TableCode => "STL";
	}
}
