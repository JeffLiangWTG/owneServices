using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefRepairCode : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefRepairCode(int version) : base(version)
		{
		}

		public override int DataSetId => 65;

		public override string TableName => nameof(RefRepairCode);

		public override string DataSetName => nameof(RefRepairCode);

		public override string TableCode => "RRC";
	}
}
