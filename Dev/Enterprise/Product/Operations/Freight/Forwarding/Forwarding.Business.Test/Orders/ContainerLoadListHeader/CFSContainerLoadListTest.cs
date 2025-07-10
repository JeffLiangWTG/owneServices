using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CFSContainerLoadList))]
	sealed class CFSContainerLoadListTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.CommonContainerLoadList);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Factory.NewWithValidTestData<CFSContainerLoadList>();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		[TestedType(typeof(CFSContainerLoadList))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#region ICustomFieldProvider

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = Factory.NewWithValidTestData<CFSContainerLoadList>() as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);

			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var loadPlan = Factory.New<CFSContainerLoadList>();
			loadPlan.CLH_LoadListId = "C125";

			AssertEquals("C125", loadPlan.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var loadPlan = Factory.New<CFSContainerLoadList>();
			AssertEquals(ExternalRequestTypes.Codes.ContainerLoadPlan, loadPlan.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);

			var loadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(loadPlan, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			var loadPlanLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadPlanLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadPlanLine.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation;
			loadPlan.LoadListLines.Add(loadPlanLine);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, loadPlan.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		public void TestGetRequestSupportedAddressInfo_DifferentManufacturesAndBuyersAndSuppliers()
		{
			var assigneeOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg1.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var assigneeOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg2.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

			var reviewerOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg1.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg2.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

			var manufactureOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg1.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg2.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

			var loadListPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "X125";
			order1.JD_OA_BuyerAddress = assigneeOrg1.MainAddress.PK;
			order1.JD_OC_BuyerContact = assigneeOrg1.Contacts[0].PK;

			var orderLine1 = Factory.NewWithValidTestData<OrderLine>();
			order1.OrderLines.Add(orderLine1);

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "X126";
			order2.JD_OA_BuyerAddress = assigneeOrg2.MainAddress.PK;
			order2.JD_OC_BuyerContact = assigneeOrg2.Contacts[0].PK;

			var orderLine2 = Factory.NewWithValidTestData<OrderLine>();
			order2.OrderLines.Add(orderLine2);

			var booking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking1, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg1.MainAddress.PK, reviewerOrg1.Contacts[0].OC_ContactName);

			var booking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking2, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg2.MainAddress.PK, reviewerOrg2.Contacts[0].OC_ContactName);

			var bookingLine1 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			booking1.SupplierBookingLines.Add(bookingLine1);
			var bookingLine2 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;
			booking2.SupplierBookingLines.Add(bookingLine2);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine1, (DocAddressType.Manufacturer), manufactureOrg1.MainAddress.PK, manufactureOrg1.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine2, (DocAddressType.Manufacturer), manufactureOrg2.MainAddress.PK, manufactureOrg2.Contacts[0].OC_ContactName);

			var loadList = Factory.NewWithValidTestData<CFSContainerLoadList>();
			loadList.CLH_OH_LoadListParty = loadListPartyOrg.PK;
			loadList.CLH_JSB_Booking = booking1.PK;
			var loadListLine1 = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine1.CLL_JSL_BookingLine = bookingLine1.PK;
			loadListLine1.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation;
			loadList.LoadListLines.Add(loadListLine1);
			var loadListLine2 = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine2.CLL_JSL_BookingLine = bookingLine2.PK;
			loadListLine2.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation;
			loadList.LoadListLines.Add(loadListLine2);
			Factory.Save();

			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);

			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);

			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion
	}
}
