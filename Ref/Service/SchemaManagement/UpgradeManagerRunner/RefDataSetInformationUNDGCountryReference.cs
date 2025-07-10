using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationUNDGCountryReference : RefDataSetInformationTransformation
	{
		public RefDataSetInformationUNDGCountryReference(int version) : base(version)
		{
		}

		public override int DataSetId => 48;
		public override string TableName => nameof(UNDGCountryReference);
		public override string DataSetName => nameof(UNDGCountryReference);
		public override string TableCode => "DCR";
	}
}
