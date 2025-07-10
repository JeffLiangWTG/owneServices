using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefShippingLineMessagingRequirementType : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefShippingLineMessagingRequirementType(int version) : base(version)
		{
		}

		public override int DataSetId => 50;
		public override string TableName => nameof(RefShippingLineMessagingRequirementType);
		public override string DataSetName => nameof(RefShippingLineMessagingRequirementType);
		public override string TableCode => "RST";
	}
}
