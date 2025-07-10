#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IUNDGSubstanceWrapperSummaryProviderForTest
	{
		string GetSummary(IUNDGDataItem undgDataItem, BusinessObjectFactory factory);
	}
}

#endif
