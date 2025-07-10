using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(ConsolidatedTransportBookingFilterBusinessObject))]
	public class ConsolidatedTransportBookingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ConsolidatedTransportBookingFilterBusinessObject();
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<CommonConsolidatedTransportBooking>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.ConsolidatedTransportBookingCRMSecurity);
		}
	}
}
