using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class DependencyProcessInfo
	{
		public Dependency Dependency { get; set; }
		public ProcessStatus Status { get; set; }
	}

	public enum ProcessStatus
	{
		Missing,
		Error,
		Merged,
		NotMerged
	}
}
