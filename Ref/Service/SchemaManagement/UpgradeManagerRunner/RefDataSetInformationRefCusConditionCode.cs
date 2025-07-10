using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusConditionCode : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusConditionCode(int version) : base(version)
		{
		}

		public override int DataSetId => 59;
		public override string TableName => nameof(RefCusConditionCode);
		public override string DataSetName => nameof(RefCusConditionCode);
		public override string TableCode => "ZY7";
	}
}
