using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentLookups_OLDTest : DtbTransportLookupsTest
	{
		#region TestBookingTemplates

		protected override DtbTransportTmplCollection GetExpectedTransportTmplCollection()
		{
			return new DtbConsignmentTmplCollection(Factory, new ZQuery(DtbBookingTmplSchema.KT_IsActive, true));
		}

		#endregion

		#region DistanceUnits

		public void TestDistanceUnits()
		{
			var consignment = Factory.New<DtbBookingConsignment>();
			AssertEquals(new DistanceUnitList(), consignment.Lookups.DistanceUnits);
		}

		#endregion

		#region Implementation

		protected override DtbTransport GetNewTransport()
		{
			return Helper.CreateBookingConsignment();
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
