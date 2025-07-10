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
	public class ContainerLoadListControlBashFetchTest : FilterControlBashFetchHintTest<CYContainerLoadList>
	{
		protected override SchemaPKColumn PkColumn => ContainerLoadListHeaderSchema.PK;
		#region BashFetchTest

		public void TestBashFetchForView_CLH_LoadListId()
		{
			BashFetchForView("CLH_LoadListId", 0);
		}

		public void TestBashFetchForView_Booking_JSB_BookingId()
		{
			BashFetchForView("Booking+JSB_BookingId", 12);
		}

		public void TestBashFetchForView_CLH_OH_LoadListParty()
		{
			BashFetchForView("CLH_OH_LoadListParty", 0);
		}

		public void TestBashFetchForView_CLH_Status()
		{
			BashFetchForView("CLH_Status", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateUser()
		{
			BashFetchForView("CLH_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateBranch()
		{
			BashFetchForView("CLH_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateDepartment()
		{
			BashFetchForView("CLH_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateTimeUtc()
		{
			BashFetchForView("CLH_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_CLH_SystemLastEditUser()
		{
			BashFetchForView("CLH_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_CLH_SystemLastEditTimeUtc()
		{
			BashFetchForView("CLH_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_Booking_JSB_TransportMode()
		{
			BashFetchForView("Booking+JSB_TransportMode", 12);
		}

		public void TestBashFetchForView_Booking_JSB_RL_NKLoadPort()
		{
			BashFetchForView("Booking+JSB_RL_NKLoadPort", 12);
		}

		public void TestBashFetchForView_Booking_JSB_RL_NKDischargePort()
		{
			BashFetchForView("Booking+JSB_RL_NKDischargePort", 12);
		}

		public void TestBashFetchForView_Booking_JSB_IncoTerm()
		{
			BashFetchForView("Booking+JSB_IncoTerm", 12);
		}

		public void TestBashFetchForView_Booking_SupplierNameOrPK()
		{
			BashFetchForView("Booking+SupplierAddress+OrganisationNameOrPK", 36);
		}

		public void TestBashFetchForView_Booking_SupplierAddress_E2_Address1()
		{
			BashFetchForView("Booking+SupplierAddress+E2_Address1", 25);
		}

		public void TestBashFetchForView_Booking_ControllingCustomerNameOrPK()
		{
			BashFetchForView("Booking+ControllingCustomerAddress+OrganisationNameOrPK", 36);
		}

		public void TestBashFetchForView_Booking_ControllingCustomerAddress_E2_Address1()
		{
			BashFetchForView("Booking+ControllingCustomerAddress+E2_Address1", 25);
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			var bookingParty = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = factory.NewWithValidTestData<OrgHeader>();

			for (var i = 0; i < 12; i++)
			{
				var booking = factory.New<JobSupplierBooking>();
				booking.JSB_TransportMode = Core.Constants.TransportModes.Sea;
				booking.JSB_BookingId = "JSB0000" + i;
				booking.JSB_LoadMode = LoadMode;
				booking.JSB_RL_NKLoadPort = "AUSYD";
				booking.JSB_RL_NKDischargePort = "CNCAN";
				booking.JSB_OH_BookingParty = bookingParty.PK;
				booking.JSB_BookedOnDate = ZDate.Today;
				booking.JSB_Status = "PLC";
				booking.JSB_IncoTerm = Core.Constants.IncoTerms.ExWorks;
				booking.SupplierAddress.OrganisationPK = supplier.PK;
				booking.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

				var loadListHeader = factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = booking.PK;
				loadListHeader.CLH_LoadListId = "CLL0000" + i;
				loadListHeader.CLH_OH_LoadListParty = bookingParty.PK;
				loadListHeader.CLH_Status = "SHP";
				loadListHeader.CLH_SystemCreateTimeUtc = ZDateTime.Now;
				loadListHeader.CLH_SystemLastEditTimeUtc = ZDateTime.Now;
				loadListHeader.CLH_LoadMode = LoadMode;

				result.Add(loadListHeader.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new ContainerLoadListCollection(Factory);
			var filterBusinessObject = new ContainerLoadListFilterBusinessObject();
			return new ContainerLoadListFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ContainerLoadListCollection(Factory);
		}

		protected virtual ZString LoadMode => Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard;

		#endregion
	}
}
