using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefMaterial : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefMaterial(int version) : base(version)
		{
		}

		public override int DataSetId => 61;

		public override string TableName => nameof(RefMaterial);

		public override string DataSetName => nameof(RefMaterial);

		public override string TableCode => "RMC";
	}
}
