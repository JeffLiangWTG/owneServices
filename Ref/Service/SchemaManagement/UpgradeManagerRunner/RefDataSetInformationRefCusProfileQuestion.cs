using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusProfileQuestion : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusProfileQuestion(int version) : base(version)
		{
		}

		public override int DataSetId => 68;
		public override string TableName => nameof(RefCusProfileQuestion);
		public override string DataSetName => nameof(RefCusProfileQuestion);
		public override string TableCode => "XQ2";
	}
}
