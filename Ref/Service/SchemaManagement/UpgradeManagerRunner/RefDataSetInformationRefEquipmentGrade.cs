using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefEquipmentGrade : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefEquipmentGrade(int version) : base(version)
		{
		}

		public override int DataSetId => 71;
		public override string TableName => nameof(RefEquipmentGrade);
		public override string DataSetName => nameof(RefEquipmentGrade);
		public override string TableCode => "REG";
	}
}
