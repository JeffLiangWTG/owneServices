using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefAccElectronicProcessingFee : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefAccElectronicProcessingFee(int version) : base(version)
		{
		}

		public override int DataSetId => 70;
		public override string TableName => nameof(RefAccElectronicProcessingFee);
		public override string DataSetName => nameof(RefAccElectronicProcessingFee);
		public override string TableCode => "EPF";
	}
}
