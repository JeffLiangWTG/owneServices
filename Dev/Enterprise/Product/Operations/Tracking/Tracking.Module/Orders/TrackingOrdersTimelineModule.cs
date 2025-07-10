using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	/// <summary>
	/// Summary description for TrackingOrderTimelineModule.
	/// </summary>
	public class TrackingOrdersTimelineModule : TrackingOrdersModule
	{
		public TrackingOrdersTimelineModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingOrdersTimeline; }
		}
	}
}
