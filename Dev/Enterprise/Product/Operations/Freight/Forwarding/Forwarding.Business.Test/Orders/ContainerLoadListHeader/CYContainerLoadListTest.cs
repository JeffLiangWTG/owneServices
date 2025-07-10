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
	[TestedType(typeof(CYContainerLoadList))]
	sealed class CYContainerLoadListTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.CommonContainerLoadList);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Factory.NewWithValidTestData<CYContainerLoadList>();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		[TestedType(typeof(CYContainerLoadList))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#region ICustomFieldProvider

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = Factory.NewWithValidTestData<CYContainerLoadList>() as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);

			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var loadList = Factory.New<CYContainerLoadList>();
			loadList.CLH_LoadListId = "C125";

			AssertEquals("C125", loadList.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var loadList = Factory.New<CYContainerLoadList>();
			AssertEquals(ExternalRequestTypes.Codes.ContainerLoadList, loadList.GetRequestTypeCode());
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
			var loadListPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);

			var loadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadList.CLH_OH_LoadListParty = loadListPartyOrg.PK;
			loadList.CLH_JSB_Booking = booking.PK;
			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			loadList.LoadListLines.Add(loadListLine);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
			AssertEquals(loadListPartyOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.LoadListParty).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.LoadListParty).ContactPK);
		}

		public void TestGetRequestSupportedAddressInfo_DifferentManufacturesAndDifferentBuyers()
		{
			var assigneeOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg1.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var assigneeOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg2.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

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
			var booking2 = Factory.NewWithValidTestData<JobSupplierBooking>();

			var bookingLine1 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			booking1.SupplierBookingLines.Add(bookingLine1);
			var bookingLine2 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;
			booking2.SupplierBookingLines.Add(bookingLine2);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine1, (DocAddressType.Manufacturer), manufactureOrg1.MainAddress.PK, manufactureOrg1.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine2, (DocAddressType.Manufacturer), manufactureOrg2.MainAddress.PK, manufactureOrg2.Contacts[0].OC_ContactName);

			var loadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadList.CLH_OH_LoadListParty = loadListPartyOrg.PK;
			loadList.CLH_JSB_Booking = booking1.PK;
			var loadListLine1 = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine1.CLL_JSL_BookingLine = bookingLine1.PK;
			loadListLine1.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			loadList.LoadListLines.Add(loadListLine1);
			var loadListLine2 = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine2.CLL_JSL_BookingLine = bookingLine2.PK;
			loadListLine2.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			loadList.LoadListLines.Add(loadListLine2);
			Factory.Save();

			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);

			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion

	}
}
