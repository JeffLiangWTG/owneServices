using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface IDeduplicationBusinessObjectUserControl
	{
		FilterStripBusinessObject DeduplicationBusinessObjectFilterBizo { get; }
		ZFilterStripControl DeduplicationBusinessObjectFilterControl { get; }
		void ResetFilterStripAndPerformDefaultCodeSearch(FilterBusinessObjectDefaults defaults, bool waitforLoad = false);
	}
}
