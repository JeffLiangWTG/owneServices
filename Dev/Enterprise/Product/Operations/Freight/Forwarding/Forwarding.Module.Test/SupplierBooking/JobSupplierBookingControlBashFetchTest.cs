using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobSupplierBookingControlBashFetchTest : FilterControlBashFetchHintTest<JobSupplierBooking>
	{
		protected override SchemaPKColumn PkColumn => JobSupplierBookingSchema.PK;
		#region BashFetchTest

		public void TestBashFetchForView_JSB_Status()
		{
			BashFetchForView("JSB_Status", 0);
		}

		public void TestBashFetchForView_JSB_LoadMode()
		{
			BashFetchForView("JSB_LoadMode", 0);
		}

		public void TestBashFetchForView_JSB_TransportMode()
		{
			BashFetchForView("JSB_TransportMode", 0);
		}

		public void TestBashFetchForView_JSB_RL_NKLoadPort()
		{
			BashFetchForView("JSB_RL_NKLoadPort", 0);
		}

		public void TestBashFetchForView_JSB_RL_NKDischargePort()
		{
			BashFetchForView("JSB_RL_NKDischargePort", 0);
		}

		public void TestBashFetchForView_JSB_IncoTerm()
		{
			BashFetchForView("JSB_IncoTerm", 0);
		}

		public void TestBashFetchForView_JSB_OH_BookingParty()
		{
			BashFetchForView("JSB_OH_BookingParty", 0);
		}

		public void TestBashFetchForView_JSB_BookingId()
		{
			BashFetchForView("JSB_BookingId", 0);
		}

		public void TestBashFetchForView_JSB_SystemCreateUser()
		{
			BashFetchForView("JSB_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JSB_SystemCreateBranch()
		{
			BashFetchForView("JSB_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JSB_SystemCreateDepartment()
		{
			BashFetchForView("JSB_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JSB_SystemCreateTimeUtc()
		{
			BashFetchForView("JSB_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JSB_SystemLastEditUser()
		{
			BashFetchForView("JSB_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JSB_SystemLastEditTimeUtc()
		{
			BashFetchForView("JSB_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JSB_BookedOnDate()
		{
			BashFetchForView("JSB_BookedOnDate", 0);
		}

		public void TestBashFetchForView_JSB_CargoAvailableDate()
		{
			BashFetchForView("JSB_CargoAvailableDate", 0);
		}

		public void TestBashFetchForView_SupplierNameOrPK()
		{
			BashFetchForView("SupplierAddress+OrganisationNameOrPK", 36);
		}

		public void TestBashFetchForView_SupplierAddress_E2_Address1()
		{
			BashFetchForView("SupplierAddress.E2_Address1", 24);
		}

		public void TestBashFetchForView_ControllingCustomerNameOrPK()
		{
			BashFetchForView("ControllingCustomerAddress+OrganisationNameOrPK", 36);
		}

		public void TestBashFetchForView_ControllingCustomerAddress_E2_Address1()
		{
			BashFetchForView("ControllingCustomerAddress.E2_Address1", 24);
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 12; i++)
			{
				var booking = factory.NewWithValidTestData<JobSupplierBooking>();
				booking.JSB_TransportMode = "SEA";
				booking.JSB_Status = "INC";
				booking.JSB_LoadMode = "CY";
				booking.JSB_RL_NKLoadPort = "SGSIN";
				booking.JSB_RL_NKDischargePort = "AUSYD";
				booking.JSB_SystemCreateTimeUtc = ZDateTime.Now;
				booking.JSB_SystemLastEditTimeUtc = ZDateTime.Now;
				booking.ControllingCustomerAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				booking.SupplierAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

				result.Add(booking.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new JobSupplierBookingCollection(Factory);
			var filterBusinessObject = new JobSupplierBookingFilterBusinessObject();
			return new JobSupplierBookingFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new JobSupplierBookingCollection(Factory);
		}

		#endregion
	}
}
