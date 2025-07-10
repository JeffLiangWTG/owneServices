using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class AgencyBookingFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<AgencyBooking>
	{
		protected override AgencyBooking GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<AgencyBooking>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.AgencyBooking;
			}
		}
	}
}
