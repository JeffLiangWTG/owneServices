using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentPackage_PackageView))]
	sealed class DtbConsignmentPackage_PackageViewTest : Package_PackageViewTest<DtbBookingConsignment, DtbConsignmentPackage_PackageView>
	{
		#region TestTotalsFromInstructions

		protected override PkgPackageJob GetPackageJob(DtbTransport transport)
		{
			return ((DtbBookingConsignment)transport).PackageJob;
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentPackage_PackageView GetPackageView(BusinessObjectFactory factory)
		{
			return new DtbConsignmentPackage_PackageView(Factory);
		}

		protected override DtbConsignmentPackage_PackageView GetPackageView(PkgPackage package, DtbBookingConsignment transport)
		{
			return new DtbConsignmentPackage_PackageView(package, transport);
		}

		protected override DtbBookingConsignment GetTransportBizO()
		{
			return Helper.CreateBookingConsignment();
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
