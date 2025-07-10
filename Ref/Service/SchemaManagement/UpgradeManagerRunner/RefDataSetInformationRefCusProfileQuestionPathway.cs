using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefCusProfileQuestionPathway : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefCusProfileQuestionPathway(int version) : base(version)
		{
		}

		public override int DataSetId => 69;
		public override string TableName => nameof(RefCusProfileQuestionPathway);
		public override string DataSetName => nameof(RefCusProfileQuestionPathway);
		public override string TableCode => "XQP";
	}
}
