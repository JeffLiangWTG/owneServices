using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusTariffAttributeName : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusTariffAttributeName(int version) : base(version)
		{
		}

		public override int DataSetId => 58;
		public override string TableName => nameof(RefCusTariffAttributeName);
		public override string DataSetName => nameof(RefCusTariffAttributeName);
		public override string TableCode => "ZY6";
	}
}
