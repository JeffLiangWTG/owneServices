using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefDamage : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefDamage(int version) : base(version)
		{
		}

		public override int DataSetId => 62;

		public override string TableName => nameof(RefDamage);

		public override string DataSetName => nameof(RefDamage);

		public override string TableCode => "RFM";
	}
}
