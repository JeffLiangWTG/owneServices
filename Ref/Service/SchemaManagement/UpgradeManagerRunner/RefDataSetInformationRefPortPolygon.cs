using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefPortPolygon : RefDataSetInformationTransformation
	{
		public RefDataSetInformationRefPortPolygon(int version) : base(version)
		{
		}

		public override int DataSetId => 44;
		public override string TableName => nameof(RefPortPolygon);
		public override string DataSetName => nameof(RefPortPolygon);
		public override string TableCode => "RPP";
	}
}
