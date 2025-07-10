using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Module.Testing
{
	sealed class TrackingQuotationsModuleForTest : TrackingQuotationsModule
	{
		public TrackingQuotationsModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }
	}
}
