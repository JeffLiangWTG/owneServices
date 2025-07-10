using System;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyBookingPackLineTest : BaseFreightTest
	{
		public void TestSetDefaultValues_UnitOfDimensionIsDefaultedFromRegistry()
		{
			var booking = Factory.New<AgencyBooking>();
			AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Inches, booking.OuterPackLines.AddNew().JL_UnitOfDimension);
			AgencyRegistry.Instance.DefaultBookingDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, booking.OuterPackLines.AddNew().JL_UnitOfDimension);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = HomePort.SubstringSafe(0, 2);
			}
		}
		#endregion
	}
}
