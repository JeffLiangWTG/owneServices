using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	static class OrderHelpers
	{
		public static Order CreateOrder(BusinessObjectFactory factory, string transportMode = TransportModes.Sea, OrgHeader supplier = null, OrgHeader buyer = null)
		{
			var order = factory.NewWithValidTestData<Order>();
			order.JD_TransportMode = transportMode;
			buyer = buyer ?? OrderManagerTestHelper.CreateOrgWithAddressesAndContacts(factory, GetNextId());
			order.JD_OA_BuyerAddress = buyer.Addresses[1].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[1].PK;

			order.GoodsAvailableAtAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			order.GoodsDeliveredToAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			order.ControllingAgentDocAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;

			supplier = supplier ?? OrderManagerTestHelper.CreateOrgWithAddressesAndContacts(factory, GetNextId());
			order.JD_OA_SupplierAddress = supplier.MainAddress.PK;
			order.JD_OC_SupplierContact = supplier.Contacts[0].PK;

			FillDocAddressWithContact(order.ConsigneeDocumentaryAddress, "Consignee Documentary Address");
			FillDocAddressWithContact(order.NotifyPartyDocAddress, "Notify Party Doc Address");
			FillDocAddressWithContact(order.NotifyParty2DocAddress, "Notify Party2 Doc Address");
			FillDocAddressWithContact(order.NotifyParty3DocAddress, "Notify Party3 Doc Address");
			FillDocAddressWithContact(order.ControllingCustomerDocAddress, "Controlling Customer Doc Address");
			return order;
		}

		static void FillDocAddressWithContact(JobDocAddress docAddress, string fullName)
		{
			var org = docAddress.Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = fullName;
			docAddress.OrganisationPK = org.PK;
			docAddress.Address1 = "Address 1";
			docAddress.Address2 = "Address 2";
			docAddress.ContactPK = org.Contacts.AddNew().With(contact => contact.FillWithValidTestData()).PK;
		}

		static long idSeed;
		public static string GetNextId() => (++idSeed).ToString("D10");
	}
}
