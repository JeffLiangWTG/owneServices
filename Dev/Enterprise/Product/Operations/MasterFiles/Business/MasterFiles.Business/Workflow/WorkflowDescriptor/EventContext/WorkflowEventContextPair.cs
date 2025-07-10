
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowEventContextPair
	{
		public WorkflowEventContextPair(ICodeDescription classifier, ICodeDescription masterType)
		{
			MasterClassifier = classifier;
			MasterType = masterType;
		}

		public ICodeDescription MasterClassifier { get; private set; }
		public ICodeDescription MasterType { get; private set; }
	}
}
