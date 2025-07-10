using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefComplianceCommodityAlert : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefComplianceCommodityAlert(int version) : base(version)
		{
		}

		public override int DataSetId => 73;
		public override string TableName => nameof(RefComplianceCommodityAlert);
		public override string DataSetName => nameof(RefComplianceCommodityAlert);
		public override string TableCode => "RCR";
	}
}
