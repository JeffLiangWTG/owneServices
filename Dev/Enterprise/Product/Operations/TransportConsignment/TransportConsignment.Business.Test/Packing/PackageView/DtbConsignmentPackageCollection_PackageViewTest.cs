using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentPackageCollection_PackageView))]
	sealed class DtbConsignmentPackageCollection_PackageViewTest : PackageCollection_PackageViewTest<DtbBookingConsignment, DtbConsignmentPackageCollection_PackageView>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DtbConsignmentPackage_PackageView(Factory.New<PkgPackage>(), Transport);
		}

		protected override DtbBookingConsignment GetNewTransportBizO()
		{
			return Helper.CreateBookingConsignment();
		}

		protected override DtbConsignmentPackageCollection_PackageView GetPackageCollection_PackageView(DtbBookingConsignment transport)
		{
			return new DtbConsignmentPackageCollection_PackageView(transport);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
