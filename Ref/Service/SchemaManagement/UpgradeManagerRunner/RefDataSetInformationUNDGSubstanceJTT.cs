using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationUNDGSubstanceJTT : RefDataSetInformationTransformation
	{
		public RefDataSetInformationUNDGSubstanceJTT(int version) : base(version)
		{
		}

		public override int DataSetId => 45;
		public override string TableName => nameof(UNDGSubstanceJTT);
		public override string DataSetName => nameof(UNDGSubstanceJTT);
		public override string TableCode => "JTT";
	}
}
