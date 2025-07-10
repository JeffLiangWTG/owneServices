using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationUNDGSubstanceCFR : RefDataSetInformationTransformation, IDataTransformationTask
	{
		public RefDataSetInformationUNDGSubstanceCFR(int version) : base(version)
		{
		}



		public override int DataSetId => 47;
		public override string TableName => nameof(UNDGSubstanceCFR);
		public override string DataSetName => nameof(UNDGSubstanceCFR);
		public override string TableCode => "CFR";
	}
}
