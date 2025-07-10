using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class DependencyProcessResult
	{
		public DependencyProcessResult(ProcessAction action, Dependency depencency)
		{
			Action = action;
			Dependency = depencency;
		}
		public ProcessAction Action { get; set; }
		public Dependency Dependency { get; set; }
	}

	public enum ProcessAction
	{
		ReportError,
		WaitForDependency,
		ProcessPrimaryData
	}
}
