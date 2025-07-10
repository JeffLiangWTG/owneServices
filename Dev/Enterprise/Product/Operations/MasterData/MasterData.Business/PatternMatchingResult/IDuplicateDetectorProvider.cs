using System.Collections.Generic;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public interface IDuplicateDetectorProvider
	{
		IEnumerable<DuplicateDetectorProviderOutput> DetectDuplicates(OrgHeader header);
		IEnumerable<DuplicateDetectorProviderOutput> DetectDuplicates(GlbPerson person);
		DuplicationStatus LastRunStatus { get; }
	}
}
