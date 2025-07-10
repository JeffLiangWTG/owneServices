using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class ChangeReport : RefDataRepoModelEntityType
	{
		public string ActionIndicator { get; set; }
		public RefDataRepoModelEntityType Entity { get; set; }
	}
}
