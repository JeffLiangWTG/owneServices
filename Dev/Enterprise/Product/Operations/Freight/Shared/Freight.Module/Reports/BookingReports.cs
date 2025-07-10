using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class BookingReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BookingsReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ScheduleReports; }
		}
	}
}
