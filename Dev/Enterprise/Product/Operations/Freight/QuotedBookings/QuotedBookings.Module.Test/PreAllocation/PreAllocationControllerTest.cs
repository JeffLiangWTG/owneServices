using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(PreAllocationController))]
	public class PreAllocationControllerTest : ZControllerBasherTest
	{
		#region Overrides

		protected override Type GetBusinessObjectType()
		{
			return typeof(ViewQuotedBooking);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PreAllocations;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			Factory.Save();
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = booking.PK;
			return viewQuotedBooking;
		}
		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
