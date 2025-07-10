using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskTemplateMatches : IProcessTaskTemplateMatches
	{
		public ProcessTaskTemplateMatches(params IProcessTaskTemplate[] templates)
			: this((IList<IProcessTaskTemplate>)templates)
		{
		}

		public ProcessTaskTemplateMatches(IList<IProcessTaskTemplate> templates)
		{
			Matches = templates;
		}

		public IList<IProcessTaskTemplate> Matches { get; }
	}
}
