using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefAccessorial : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefAccessorial(int version) : base(version)
		{
		}

		public override int DataSetId => 75;
		public override string TableName => nameof(RefAccessorial);
		public override string DataSetName => nameof(RefAccessorial);
		public override string TableCode => "ASI";
	}
}
