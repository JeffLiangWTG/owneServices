using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobOrderJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAttachOrderToShipmentWithNonMatchingControllingCustomerForShipment()
		{
			var origin = Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;
			var order = Factory.New<Order>();
			var shipment = Factory.New<ForwardingShipment>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";
			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";
			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
			order.JD_JS = shipment.PK;

			order.ControllingCustomerDocAddress.RunPreSaveValidation();
			AssertHasError(order.ControllingCustomerDocAddress.E2_OA_AddressInfo, "You do not have security rights to allow order and its shipment have non-matching Controlling Customers.");

			Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}

		public void TestAttachOrderToShipmentWithNonMatchingControllingCustomerForBooking()
		{
			var origin = Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed;
			Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = false;
			var order = Factory.New<Order>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";
			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";
			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
			order.JD_JS = shipment.PK;

			order.ControllingCustomerDocAddress.RunPreSaveValidation();
			AssertHasError(order.ControllingCustomerDocAddress.E2_OA_AddressInfo, "You do not have security rights to allow order and its booking have non-matching Controlling Customers.");

			Environment.Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForBooking.IsAllowed = origin;
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsChanged_OrderLineAttachedToSupplierBooking()
		{
			SetupTestDataForCheckOrganisationPK(addressOverride: false, attachOrderLines: true);
			AssertControllingCustomerCanOnlyBeUpdatedWhenCancelled();
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsAdded_OrderLineAttachedToSupplierBooking()
		{
			var bookingStatuses = typeof(Constants.SupplierBookingStatus).GetFields();

			OrderBO = Factory.NewWithValidTestData<Order>();
			OrderBO.ControllingCustomerDocAddress.E2_AddressOverride = false;
			OrderBO.ControllingCustomerDocAddress.E2_OA_Address = ZGuid.Empty;
			var orderLine = OrderBO.OrderLines.AddNew();

			BookingBO = Factory.NewWithValidTestData<JobSupplierBooking>();

			var jobSupplierBookingLine = BookingBO.SupplierBookingLines.AddNew();
			jobSupplierBookingLine.JSL_JO_OrderLine = orderLine.PK;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = controllingCustomer.Addresses.AddNew();
			controllingCustomerAddress.OA_Address1 = "Controlling Customer Address";

			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				BookingBO.JSB_Status = bookingStatus;
				Factory.Save();

				var order = new BusinessObjectFactory().Load<Order>(OrderBO.PK);
				order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				order.ControllingCustomerDocAddress.RunPreSaveValidation();

				if (bookingStatus != Constants.SupplierBookingStatus.Cancelled && bookingStatus != Constants.SupplierBookingStatus.Converted)
				{
					AssertHasError(
						$"{status} should have this error",
						order.ControllingCustomerDocAddress.OrganisationPKInfo,
						"Controlling customer cannot be added because this order or at least one of its order lines is linked to a supplier booking in progress."
					);
				}
				else
				{
					AssertNoErrors($"Should have no errors for {status}", order.ControllingCustomerDocAddress.OrganisationPKInfo);
				}
			}
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsChanged_NoOrderLineAttachedToSupplierBooking()
		{
			SetupTestDataForCheckOrganisationPK(addressOverride: false, attachOrderLines: false);

			OrderBO.ControllingCustomerDocAddress.RunPreSaveValidation();
			AssertNoErrors(OrderBO.ControllingCustomerDocAddress.OrganisationPKInfo);
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsChangedAddressOverrideOn_OrderLineAttachedToSupplierBooking()
		{
			SetupTestDataForCheckOrganisationPK(addressOverride: true, attachOrderLines: true);
			AssertControllingCustomerCanOnlyBeUpdatedWhenCancelled();
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsAddedAddressOverrideOn_OrderLineAttachedToSupplierBooking()
		{
			var bookingStatuses = typeof(Constants.SupplierBookingStatus).GetFields();

			OrderBO = Factory.NewWithValidTestData<Order>();
			OrderBO.ControllingCustomerDocAddress.E2_AddressOverride = false;
			OrderBO.ControllingCustomerDocAddress.E2_OA_Address = ZGuid.Empty;
			var orderLine = OrderBO.OrderLines.AddNew();

			BookingBO = Factory.NewWithValidTestData<JobSupplierBooking>();

			var jobSupplierBookingLine = BookingBO.SupplierBookingLines.AddNew();
			jobSupplierBookingLine.JSL_JO_OrderLine = orderLine.PK;

			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				BookingBO.JSB_Status = bookingStatus;

				Factory.Save();

				var order = new BusinessObjectFactory().Load<Order>(OrderBO.PK);

				order.ControllingCustomerDocAddress.E2_AddressOverride = true;
				order.ControllingCustomerDocAddress.E2_Address1 = "Magic School";
				order.ControllingCustomerDocAddress.E2_CompanyName = "Magic Company";
				order.ControllingCustomerDocAddress.E2_City = "Magic City";
				order.ControllingCustomerDocAddress.E2_State = "NSW";
				order.ControllingCustomerDocAddress.E2_Postcode = "1234";

				order.ControllingCustomerDocAddress.RunPreSaveValidation();

				if (bookingStatus != Constants.SupplierBookingStatus.Cancelled && bookingStatus != Constants.SupplierBookingStatus.Converted)
				{
					AssertHasError(
						$"{status} should have this error",
						order.ControllingCustomerDocAddress.OrganisationPKInfo,
						"Controlling customer cannot be added because this order or at least one of its order lines is linked to a supplier booking in progress."
					);
				}
				else
				{
					AssertNoErrors($"Should have no errors for {status}", order.ControllingCustomerDocAddress.OrganisationPKInfo);
				}
			}
		}

		public void TestCheckOrganisationPK_WhenControllingCustomerIsChangedAddressOverrideOn_NoOrderLineAttachedToSupplierBooking()
		{
			SetupTestDataForCheckOrganisationPK(addressOverride: true, attachOrderLines: false);

			OrderBO.ControllingCustomerDocAddress.RunPreSaveValidation();
			AssertNoErrors(OrderBO.ControllingCustomerDocAddress.OrganisationPKInfo);
		}

		#region Implementation

		void SetupTestDataForCheckOrganisationPK(bool addressOverride, bool attachOrderLines)
		{
			OrderBO = Factory.NewWithValidTestData<Order>();
			var orderLine = OrderBO.OrderLines.AddNew();

			var someControllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			someControllingCustomer.OH_Code = "SCM";

			var someControllingCustomerAddress = someControllingCustomer.Addresses.AddNew();
			someControllingCustomerAddress.OA_Address1 = "Somewhere in the world";

			OrderBO.ControllingCustomerDocAddress.E2_OA_Address = someControllingCustomerAddress.PK;

			if (attachOrderLines)
			{
				BookingBO = Factory.NewWithValidTestData<JobSupplierBooking>();

				var jobSupplierBookingLine = BookingBO.SupplierBookingLines.AddNew();
				jobSupplierBookingLine.JSL_JO_OrderLine = orderLine.PK;
			}

			Factory.Save();

			if (addressOverride)
			{
				OrderBO.ControllingCustomerDocAddress.E2_AddressOverride = true;
				OrderBO.ControllingCustomerDocAddress.E2_Address1 = "Magic School";
				OrderBO.ControllingCustomerDocAddress.E2_CompanyName = "Magic Company";
				OrderBO.ControllingCustomerDocAddress.E2_City = "Magic City";
				OrderBO.ControllingCustomerDocAddress.E2_State = "NSW";
				OrderBO.ControllingCustomerDocAddress.E2_Postcode = "1234";
			}
		}

		void AssertControllingCustomerCanOnlyBeUpdatedWhenCancelled()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = controllingCustomer.Addresses.AddNew();
			controllingCustomerAddress.OA_Address1 = "Controlling Customer Address";

			var bookingStatuses = typeof(Constants.SupplierBookingStatus).GetFields();

			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				BookingBO.JSB_Status = bookingStatus;

				Factory.Save();

				OrderBO.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
				OrderBO.ControllingCustomerDocAddress.RunPreSaveValidation();

				if (bookingStatus != Constants.SupplierBookingStatus.Cancelled && bookingStatus != Constants.SupplierBookingStatus.Converted)
				{
					AssertHasError(
						$"{status} should have this error",
						OrderBO.ControllingCustomerDocAddress.OrganisationPKInfo,
						"Controlling customer cannot be updated because this order or at least one of its order lines is linked to a supplier booking in progress."
					);
				}
				else
				{
					AssertNoErrors($"Should have no errors for {status}", OrderBO.ControllingCustomerDocAddress.OrganisationPKInfo);
				}

				OrderBO.ControllingCustomerDocAddress.E2_OA_Address = (ZGuid)OrderBO.ControllingCustomerDocAddress.E2_OA_AddressInfo.OriginalValue;
				AssertNoErrors(OrderBO.ControllingCustomerDocAddress.OrganisationPKInfo);
			}
		}

		Order OrderBO;
		JobSupplierBooking BookingBO;

		#endregion
	}
}
