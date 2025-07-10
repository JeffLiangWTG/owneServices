using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefMRComponentCode : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefMRComponentCode(int version) : base(version)
		{
		}

		public override int DataSetId => 63;

		public override string TableName => nameof(RefMRComponentCode);

		public override string DataSetName => nameof(RefMRComponentCode);

		public override string TableCode => "RCC";
	}
}
