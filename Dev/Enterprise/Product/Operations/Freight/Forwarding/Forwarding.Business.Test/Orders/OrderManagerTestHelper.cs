using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	public static class OrderManagerTestHelper
	{
		public static void CreateDocAddressAndFillWithAddressAndContact(IDocAddresses entity, DocAddressType addressType, ZGuid addressPK, string contactName)
		{
			var docAddress = entity.DocAddresses.AddNew(addressType);
			docAddress.E2_OA_Address = addressPK;
			docAddress.E2_Contact = contactName;
		}

		public static void SetPlannedValues(ContainerLoadListLine containerLoadListLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			containerLoadListLine.CLL_PlannedQuantity = quantity;
			containerLoadListLine.CLL_PlannedPackages = packages;
			containerLoadListLine.CLL_PlannedWeight = weight;
			containerLoadListLine.CLL_PlannedVolume = volume;
		}

		public static void SetPackedValues(ContainerLoadListLine containerLoadListLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			containerLoadListLine.CLL_PackedQuantity = quantity;
			containerLoadListLine.CLL_Packages = packages;
			containerLoadListLine.CLL_Weight = weight;
			containerLoadListLine.CLL_Volume = volume;
		}

		public static void SetReceivedValues(JobSupplierBookingLine supplierBookingLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			supplierBookingLine.JSL_ReceivedQuantity = quantity;
			supplierBookingLine.JSL_ReceivedPackages = packages;
			supplierBookingLine.JSL_ReceivedWeight = weight;
			supplierBookingLine.JSL_ReceivedVolume = volume;
		}

		public static void SetToBePacked(JobSupplierBookingLine supplierBookingLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			supplierBookingLine.JSL_RemainingQuantityToBePacked = quantity;
			supplierBookingLine.JSL_RemainingPackagesToBePacked = packages;
			supplierBookingLine.JSL_RemainingWeightToBePacked = weight;
			supplierBookingLine.JSL_RemainingVolumeToBePacked = volume;
		}

		public static JobSupplierBookingLine PrepareSupplierBookingLineForQtyPacked(BusinessObjectFactory factory, string status = Constants.SupplierBookingStatus.Planned)
		{
			var order = factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 20;

			var booking = factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;
			booking.JSB_Status = status;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			return bookingLine;
		}

		public static CommonContainerLoadList PrepareContainerLoadListForQtyPacked(BusinessObjectFactory factory, JobSupplierBookingLine bookingLine, ZDecimal packedQuantity, string loadMode, string status = Constants.ContainerLoadListHeaderStatus.Incomplete)
		{
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_JSB_SupplierBooking = bookingLine.SupplierBooking.PK;

			var loadListHeader = factory.NewWithValidTestData<CommonContainerLoadList>();
			loadListHeader.CLH_LoadMode = loadMode;
			if (loadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				loadListHeader.CLH_JSB_Booking = bookingLine.SupplierBooking.PK;
			}
			loadListHeader.CLH_Status = status;
			var maxLength = loadListHeader.CLH_MarksAndNumbersInfo.MaxLength;
			loadListHeader.CLH_MarksAndNumbers = LongString.Substring(0, maxLength);

			var loadListLine = loadListHeader.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container.PK;
			loadListLine.CLL_PackedQuantity = packedQuantity;

			if (loadMode == ContainerLoadListHeaderLoadMode.ContainerFreightStation)
			{
				loadListLine.CLL_PlannedQuantity = packedQuantity;

				var loadListLinePlanned = loadListHeader.LoadListLines.AddNew();
				loadListLinePlanned.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLinePlanned.CLL_JC_Container = ZGuid.Empty;
				loadListLinePlanned.CLL_PackedQuantity = packedQuantity;
				loadListLinePlanned.CLL_PlannedQuantity = packedQuantity;
			}

			return loadListHeader;
		}

		public static JobSupplierBooking CreateSupplierBooking(BusinessObjectFactory factory, string bookingID, OrgHeader bookingParty, OrgHeader supplier, string loadMode = "CY")
		{
			var booking = factory.New<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;
			booking.JSB_BookingId = bookingID;
			booking.JSB_LoadMode = loadMode;
			booking.JSB_RL_NKLoadPort = "AUSYD";
			booking.JSB_RL_NKDischargePort = "CNCAN";
			booking.JSB_RL_NKOrigin = "AUMEL";
			booking.JSB_RL_NKDestination = "SGSIN";
			booking.JSB_OH_BookingParty = bookingParty.PK;
			booking.JSB_BookedOnDate = ZDate.Today;
			booking.JSB_Status = Constants.SupplierBookingStatus.Placed;
			booking.JSB_IncoTerm = Constants.IncoTerms.ExWorks;
			booking.JSB_GoodsDescription = "Goods Description";
			booking.JSB_DetailedGoodsDescription = "This is a more detailed goods description";
			booking.SupplierAddress.OrganisationPK = supplier.PK;
			booking.JSB_OH_BookingParty = bookingParty.PK;

			var maxLength = booking.JSB_MarksAndNumbersInfo.MaxLength;
			booking.JSB_MarksAndNumbers = LongString.Substring(0, maxLength);

			if (supplier.Addresses.Count > 0)
			{
				booking.SupplierAddress.E2_OA_Address = supplier.Addresses[0].PK;
			}

			if (supplier.Contacts.Count > 0)
			{
				booking.SupplierAddress.ContactPK = supplier.Contacts[0].PK;
			}

			return booking;
		}

		public static OrgHeader CreateOrgWithAddressesAndContacts(BusinessObjectFactory factory, string orgCode)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;
			org.OH_FullName = orgCode;
			org.Addresses.AddNew().FillWithValidTestData();
			org.Addresses.AddNew().FillWithValidTestData();
			org.Contacts.AddNew().FillWithValidTestData();
			org.Contacts.AddNew().FillWithValidTestData();

			return org;
		}

		public static Order CreateGenericOrder(BusinessObjectFactory factory)
		{
			var buyer = CreateOrgWithAddressesAndContacts(factory, "BUYERSYD");

			var order = factory.NewWithValidTestData<Order>();
			order.JD_OA_BuyerAddress = buyer.Addresses[1].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[1].PK;
			order.GoodsAvailableAtAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order.ControllingAgentDocAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			return order;
		}

		public static void SetupAddressForOrder(BusinessObjectFactory factory, Order order)
		{
			order.NotifyPartyDocAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			order.NotifyParty2DocAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			order.NotifyParty3DocAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			order.ControllingCustomerDocAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;
			SetupAddressValues(order.NotifyPartyDocAddress, "NPa1");
			SetupAddressValues(order.NotifyParty2DocAddress, "NPa2");
			SetupAddressValues(order.NotifyParty3DocAddress, "NPa3");
			SetupAddressValues(order.ControllingCustomerDocAddress, "CoCu");
		}

		static void SetupAddressValues(JobDocAddress address, string prefix)
		{
			address.BypassFireEventBeforeChange = true;
			address.E2_AddressOverride = true;
			address.AddressCode = prefix;
			address.Address1 = prefix + "Addr1";
			address.Address2 = prefix + "Addr2";
			address.Postcode = prefix + "Zip";
			address.City = prefix + "City";
			address.State = "New South Wales";
			address.E2_Contact = prefix + "Contact";
			address.E2_Phone = prefix + "Phone";
			address.BypassFireEventBeforeChange = true;
		}

		public static void AssertPopulatedShipmentToExpected(ForwardingShipment shipment, Order order, JobSupplierBooking booking, ZDateTime etd, ZDateTime eta, bool hasConsigneeDocumentaryAddress = false, JobDocAddress controllingCustomerAddress = null, string packingMode = "FCL", CommonContainerLoadList loadListHeader = null)
		{
			Assertion.CombineAssertions("Packed Shipment should match expected values", () =>
			{
				Assertion.AssertNotNullOrEmpty("House Bill", shipment.JS_HouseBill);
				Assertion.AssertEquals("Transport Mode", booking.JSB_TransportMode, shipment.JS_TransportMode);
				Assertion.AssertEquals("Packing Mode", packingMode, shipment.JS_PackingMode);
				Assertion.AssertEquals("Load Port", booking.JSB_RL_NKOrigin, shipment.JS_RL_NKOrigin);
				Assertion.AssertEquals("Discharge Port", booking.JSB_RL_NKDestination, shipment.JS_RL_NKDestination);
				Assertion.AssertEquals("ETD", etd, shipment.JS_E_DEP);
				Assertion.AssertEquals("ETA", eta, shipment.JS_E_ARV);
				if (hasConsigneeDocumentaryAddress)
				{
					Assertion.AssertEquals("Consignee Address PK", booking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					Assertion.AssertEquals("Consignee Contact PK", booking.ConsigneeDocumentaryAddress.ContactPK, shipment.ConsigneeDocumentaryAddress.ContactPK);
					Assertion.AssertEquals("Consignee Org PK", booking.ConsigneeDocumentaryAddress.OrganisationPK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
				}
				else
				{
					Assertion.AssertEquals("Consignee Address PK", order != null ? order.JD_OA_BuyerAddress : Guid.Empty, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					Assertion.AssertEquals("Consignee Contact PK", order != null ? order.JD_OC_BuyerContact : Guid.Empty, shipment.ConsigneeDocumentaryAddress.ContactPK);
					Assertion.AssertEquals("Consignee Org PK", order != null ? order.BuyerPK : Guid.Empty, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
				}
				Assertion.AssertEquals("Notify Party 1", order != null ? order.NotifyPartyDocAddress.OrganisationPK : Guid.Empty, shipment.NotifyPartyDocumentaryAddress.OrganisationPK);
				Assertion.AssertEquals("Notify Party 2", order != null ? order.NotifyParty2DocAddress.OrganisationPK : Guid.Empty, shipment.NotifyParty2DocumentaryAddress.OrganisationPK);
				Assertion.AssertEquals("Notify Party 3", order != null ? order.NotifyParty3DocAddress.OrganisationPK : Guid.Empty, shipment.NotifyParty3DocumentaryAddress.OrganisationPK);
				Assertion.AssertEquals("Consignor Address PK", booking.SupplierAddress.E2_OA_Address, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				Assertion.AssertEquals("Consignor Contact PK", booking.SupplierAddress.ContactPK, shipment.ConsignorDocumentaryAddress.ContactPK);
				Assertion.AssertEquals("Consignor Org PK", booking.SupplierAddress.OrganisationPK, shipment.ConsignorDocumentaryAddress.OrganisationPK);
				Assertion.AssertEquals("IncoTerm", booking.JSB_IncoTerm, shipment.JS_INCO);
				Assertion.AssertEquals("Goods Description", (loadListHeader?.CLH_GoodsDescription ?? booking.JSB_GoodsDescription), shipment.JS_GoodsDescription);
				Assertion.AssertEquals("Detailed Goods Description", (loadListHeader?.CLH_DetailedGoodsDescription ?? booking.JSB_DetailedGoodsDescription), shipment.DetailedGoodsDescriptionNoteText);
				Assertion.AssertEquals("Marks and Numbers", (loadListHeader?.CLH_MarksAndNumbers ?? booking.JSB_MarksAndNumbers.Truncate(shipment.JS_MarksAndNumbersInfo.MaxLength)), shipment.JS_MarksAndNumbers);
			});
			if (order != null)
			{
				AssertAddressEquals(order.NotifyPartyDocAddress, shipment.NotifyPartyDocumentaryAddress);
				AssertAddressEquals(order.NotifyParty2DocAddress, shipment.NotifyParty2DocumentaryAddress);
				AssertAddressEquals(order.NotifyParty3DocAddress, shipment.NotifyParty3DocumentaryAddress);
				AssertAddressEquals(booking.JSB_LoadMode == SupplierBookingLoadModeList.Codes.CY || booking.JSB_LoadMode == SupplierBookingLoadModeList.Codes.LSE ? booking.ControllingCustomerAddress : controllingCustomerAddress, shipment.ControllingCustomerAddress);
				AssertAddressEquals(order.ControllingAgentDocAddress, shipment.ControllingAgentDocumentaryAddress);
				Assertion.AssertEquals(order.GoodsAvailableAtAddress.E2_OA_Address, shipment.ConsignorPickupAddress.E2_OA_Address);
				Assertion.AssertEquals(order.GoodsDeliveredToAddress.E2_OA_Address, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
			}
		}

		public static void AssertAddressEquals(JobDocAddress expected, JobDocAddress actual)
		{
			Assertion.AssertNotNull(actual);

			AssertionWithHtml.CombineAssertions($"{expected.DocAddressType} should match exactly", () =>
			{
				Assertion.AssertEquals("DocAddressType", expected.DocAddressType, actual.DocAddressType);
				Assertion.AssertEquals($"{expected.DocAddressType}.Address1", expected.Address1, actual.Address1);
				Assertion.AssertEquals($"{expected.DocAddressType}.Address2", expected.Address2, actual.Address2);
				Assertion.AssertEquals($"{expected.DocAddressType}.Postcode", expected.Postcode, actual.Postcode);
				Assertion.AssertEquals($"{expected.DocAddressType}.Street", expected.Street, actual.Street);
				Assertion.AssertEquals($"{expected.DocAddressType}.StreetNumber", expected.StreetNumber, actual.StreetNumber);
				Assertion.AssertEquals($"{expected.DocAddressType}.City", expected.City, actual.City);
				Assertion.AssertEquals($"{expected.DocAddressType}.State", expected.State, actual.State);
				Assertion.AssertEquals($"{expected.DocAddressType}.StateCode", expected.StateCode, actual.StateCode);
				Assertion.AssertEquals($"{expected.DocAddressType}.E2_Contact", expected.E2_Contact, actual.E2_Contact);
				Assertion.AssertEquals($"{expected.DocAddressType}.E2_Phone", expected.E2_Phone, actual.E2_Phone);
				Assertion.AssertEquals($"{expected.DocAddressType}.E2_OA_Address", expected.E2_OA_Address, actual.E2_OA_Address);
			});
		}

		public static (Order, OrderLine, JobSupplierBooking, JobSupplierBookingLine, CommonContainerLoadList, ContainerLoadListLine, ForwardingConsol, ForwardingContainer) CreateBasicDataForUniversalObjectTest(BusinessObjectFactory factory, string orderNumber = "SBK ORDER ME")
		{
			var order = factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = orderNumber;
			order.JD_OrderNumberSplit = new ZByte(3);

			var orderLine = order.OrderLines.AddNew();

			var booking = factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;
			booking.JSB_RL_NKLoadPort = "AUSYD";
			booking.JSB_RL_NKDischargePort = "CNCAN";

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_JSB_SupplierBooking = booking.PK;

			var loadListHeader = factory.NewWithValidTestData<CYContainerLoadList>();
			loadListHeader.CLH_JSB_Booking = booking.PK;
			loadListHeader.CLH_Status = "SHP";

			var loadListLine = loadListHeader.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container.PK;
			return (order, orderLine, booking, bookingLine, loadListHeader, loadListLine, consol, container);
		}

		public static void CheckKeyValuePairCollectionForUXML<T>(List<T> entityKeyCollection, int count, string orderKey = null, string orderLineKey = null, string bookingKey = null, string bookingLineKey = null, string containerLoadListKey = null, string containerNumber = null, string packingLineKey = null, string forwardingShipmentKey = null)
		{
			Assertion.AssertEquals(count, entityKeyCollection.Count);
			var totalCount = 0;
			entityKeyCollection.ForEach(entityKey =>
			{
				CheckKeyValuePairForUXML(entityKey, orderKey, orderLineKey, bookingKey, bookingLineKey, containerLoadListKey, containerNumber, packingLineKey, forwardingShipmentKey, out var hasMatched);
				if (hasMatched)
				{
					totalCount++;
				}
			});
			Assertion.AssertEquals(totalCount, entityKeyCollection.Count);
		}

		static void CheckKeyValuePairForUXML<T>(T entityKey, string orderKey, string orderLineKey, string bookingKey, string bookingLineKey, string containerLoadListKey, string containerNumber, string packingLineKey, string forwardingShipmentKey, out bool hasMatched)
		{
			hasMatched = false;
			var key = "";
			var value = "";
			switch (entityKey)
			{
				case EntityKey keyValuePair:
					key = keyValuePair.Type;
					value = keyValuePair.Key;
					break;
				case AddInfo keyValuePair:
					key = keyValuePair.Key;
					value = keyValuePair.Value;
					break;
			}
			switch (key)
			{
				case "OrderContextKey":
					Assertion.AssertEquals(orderKey, value);
					hasMatched = true;
					break;
				case "OrderLineContextKey":
					Assertion.AssertEquals(orderLineKey, value);
					hasMatched = true;
					break;
				case "SupplierBooking":
					Assertion.AssertEquals(bookingKey, value);
					hasMatched = true;
					break;
				case "BookingLineID":
					Assertion.AssertEquals(bookingLineKey, value);
					hasMatched = true;
					break;
				case "ContainerLoadList":
					Assertion.AssertEquals(containerLoadListKey, value);
					hasMatched = true;
					break;
				case "ContainerNumber":
					Assertion.AssertEquals(containerNumber, value);
					hasMatched = true;
					break;
				case "PackingLineID":
					Assertion.AssertEquals(packingLineKey, value);
					hasMatched = true;
					break;
				case "ForwardingShipment":
					Assertion.AssertEquals(forwardingShipmentKey, value);
					hasMatched = true;
					break;
			}
		}

		public class Measure
		{
			public Measure(
				decimal volume = 0.0m,
				string volumeUnit = Constants.Volume.CubicMetres,
				decimal weight = 0.0m,
				string weightUnit = Constants.Weight.Kilograms,
				int packages = 0,
				string packagesUnit = PkgUnit.Package,
				int quantity = 0,
				decimal price = 1.0m)
			{
				Volume = volume;
				VolumeUnit = volumeUnit;
				Weight = weight;
				WeightUnit = weightUnit;
				Packages = packages;
				PackagesUnit = packagesUnit;
				Quantity = quantity;
				Price = price;
			}

			public decimal Volume { get; set; }
			public string VolumeUnit { get; set; }
			public decimal Weight { get; set; }
			public string WeightUnit { get; set; }
			public int Packages { get; set; }
			public string PackagesUnit { get; set; }
			public int Quantity { get; set; }
			public decimal Price { get; set; }
		}

		public static void CreateSupplierBookingLine(Order order, Measure measure, JobSupplierBooking booking)
		{
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = measure.Quantity;
			orderLine.JO_ItemPrice = measure.Price;
			orderLine.JO_LinePrice = orderLine.JO_ItemPrice * orderLine.JO_Quantity;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_MarksAndNumbers = "JSL_MarksAndNumbers";
			bookingLine.JSL_Volume = measure.Volume;
			bookingLine.JSL_VolumeUnit = measure.VolumeUnit;
			bookingLine.JSL_GrossWeight = measure.Weight;
			bookingLine.JSL_GrossWeightUnit = measure.WeightUnit;
			bookingLine.JSL_BookedPackages = measure.Packages;
			bookingLine.JSL_BookedQuantity = measure.Quantity;
			bookingLine.JSL_F3_NKBookedPackagesUnit = measure.PackagesUnit;
		}

		static readonly ZString LongString =
			@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse nec nisl et dolor posuere viverra sit amet et sem. Aenean nec lacus semper turpis imperdiet eleifend. In non sem vitae quam ornare commodo vel a leo. Fusce nec dui orci. Maecenas blandit ipsum id tortor viverra, hendrerit elementum lectus sagittis. Nulla vitae molestie tellus. Curabitur felis ipsum, ornare non euismod eget, mattis a justo. Phasellus augue lectus, gravida at nisl eu, ornare pulvinar tellus. Suspendisse a dolor mi. Sed rhoncus sem a nisi porttitor bibendum. Etiam at tempus velit. Duis eu enim suscipit, consequat purus ac, lobortis mi. Nulla vestibulum purus quis est finibus, vitae sagittis eros ultricies. Aenean quis purus sapien.
Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Maecenas vitae mauris in risus lacinia dapibus ut viverra felis. Curabitur vitae velit odio. Donec ut fermentum diam. Cras ultrices nibh nibh. Aenean porttitor justo eu elit tincidunt, a scelerisque leo maximus. Vestibulum in nisl vel mi posuere elementum. Fusce porta bibendum nulla, sed commodo est ultricies eget. Maecenas in ante turpis. In tincidunt porttitor diam in pulvinar.
Mauris mattis massa erat. Sed quis lacinia mi. Nam vehicula lectus ac massa facilisis congue. Cras iaculis elit et massa ultricies, ut posuere risus lacinia. Aliquam consectetur maximus imperdiet. Maecenas purus justo, blandit ac cursus vel, varius non diam. Integer maximus mi non egestas vestibulum. Integer quis tortor sed nunc cursus mollis egestas non nibh. Praesent imperdiet tellus et quam consectetur convallis at et dui. Nulla dictum elit in ultricies elementum. Morbi at ipsum iaculis, consectetur dui viverra, tempus odio. Pellentesque mi eros, euismod et nulla eu, maximus rhoncus lacus. Donec vel lorem efficitur, tempor dolor eget, gravida orci. Morbi ut purus eu mauris consequat placerat. Nullam faucibus sodales molestie. Morbi odio augue, pellentesque et rhoncus vel, viverra sit amet nibh.
Donec consectetur euismod felis lobortis suscipit. Interdum et malesuada fames ac ante ipsum primis in faucibus. Praesent fermentum sit amet velit ac euismod. Morbi vel est quis tortor pretium ultrices. Praesent maximus mi et quam fermentum sagittis. Nullam elementum diam ut bibendum mollis. Nullam in est ut erat pretium viverra vitae ac risus. Quisque tristique orci ac justo hendrerit, et varius tortor suscipit. Sed fringilla pellentesque tincidunt. Vestibulum purus velit, maximus nec libero et, suscipit condimentum ipsum. Vestibulum ut risus lectus.
Donec ex sapien, suscipit quis euismod vel, dignissim eu ex. Sed sagittis metus et leo pellentesque, nec convallis diam molestie. Vestibulum id laoreet dolor. Maecenas quis ante dignissim, suscipit libero eu, pellentesque arcu. Nam blandit eu mauris ut finibus. Vivamus quis varius lorem. Curabitur pulvinar nisi vel maximus cursus. Fusce eu iaculis felis. Aenean eu faucibus arcu. Morbi nisl velit, venenatis quis metus et, eleifend ornare orci.
Praesent aliquet imperdiet turpis non pretium. Mauris at felis a lacus consequat dignissim. Cras risus libero, molestie sit amet cursus sed, pharetra eget quam. Vivamus et nunc quis sem venenatis finibus sit amet ut ligula. Proin vel ligula at augue mattis imperdiet non quis arcu. Phasellus vitae enim id ipsum mattis feugiat eu mollis justo. Donec ut augue rhoncus, malesuada ligula ut, rutrum diam.
Pellentesque vehicula est et nisi vestibulum, nec maximus dolor fermentum. Ut id diam ante. Sed sit amet dui quam. In hac habitasse platea dictumst. Etiam varius aliquet erat a fringilla. Praesent nulla lorem, lacinia sit amet vehicula in, commodo vitae neque. Cras ultricies ut sem quis luctus. Phasellus ullamcorper cursus nisi, vitae varius dui tincidunt ut. Fusce ultricies, lorem eget placerat egestas, turpis elit laoreet est, sit amet accumsan ligula libero eu magna. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;
Suspendisse euismod, enim in viverra cursus, lorem sem mollis ligula, vitae lacinia justo arcu ac nisl. In id urna ac purus accumsan finibus. Curabitur malesuada eros sit amet ipsum fringilla, nec feugiat nisi vulputate. Aliquam a lacus eu lacus auctor commodo. Phasellus vitae quam quam. Aliquam semper, urna vel malesuada varius, justo nulla commodo turpis, eget scelerisque eros massa ac libero. Ut mi leo, malesuada ac enim vitae, aliquam malesuada massa. Pellentesque ac elit tincidunt, venenatis dolor eget, vulputate neque. Sed et dui rutrum, pretium orci quis, tempus nulla. Fusce sed dignissim metus, eget fringilla diam. Nunc sit amet metus eu libero posuere porta. Mauris volutpat pulvinar imperdiet. Etiam molestie ac neque nec congue. Aliquam non nulla a felis vulputate cursus. Nunc varius augue condimentum erat interdum, sit amet fermentum dui aliquet.
Ut ac facilisis arcu, eget egestas quam. Cras fermentum sagittis mattis. Vivamus vel neque at augue commodo dictum vel bibendum urna. Pellentesque quis quam nunc. Integer posuere felis ut rutrum ultrices. Ut consectetur mauris lacus, sit amet eleifend ipsum molestie ac. In ac lorem tincidunt, accumsan lacus non, dignissim urna. Nulla faucibus, est eu tristique ullamcorper, orci dolor commodo ante, sit amet hendrerit purus mauris ornare dui. Morbi ut lobortis libero, a porta urna. Duis ipsum eros, egestas vitae eros a, imperdiet malesuada augue. Aenean ultricies, nulla sed eleifend malesuada, lacus augue consequat justo, ac tincidunt mi nulla id nulla.
Aenean et commodo neque. Ut eleifend eros quam, a egestas nulla egestas quis. In nec nunc interdum, auctor ligula non, mollis purus. Nulla eleifend velit tristique, venenatis elit interdum, fringilla turpis. Integer placerat auctor ligula, auctor vestibulum augue fringilla eget. Phasellus a fermentum elit. Nulla volutpat, quam sollicitudin convallis commodo, lectus lacus eleifend lectus, a pellentesque massa mi in libero.
Mauris vitae faucibus neque, at cursus velit. Vestibulum aliquam viverra aliquam. Vestibulum ac ante enim. Curabitur venenatis nisi in tincidunt blandit. In lobortis eu risus eu cursus. Sed et ullamcorper diam. Nam urna sapien, scelerisque sit amet ullamcorper nec, aliquam sed lorem.
Morbi nibh purus, mattis eget nunc non, porttitor pharetra orci. Mauris porta ac odio blandit mattis. Suspendisse sed consequat lectus, eget vulputate lacus. Vivamus eget sagittis turpis. Quisque a volutpat elit. Quisque faucibus erat et egestas vulputate. Sed a ultricies mauris. Maecenas pellentesque facilisis varius. Nunc id quam in justo tristique tempus vitae condimentum erat. Aenean eu orci vel diam mattis tempus eu eget ipsum. Nunc eget volutpat sapien, sed egestas ex. Quisque imperdiet odio est, vitae pretium eros pellentesque dapibus. In a venenatis lorem, at eleifend urna. Mauris non mi lectus.
Suspendisse congue elit id suscipit dapibus. Mauris tempor massa dignissim imperdiet euismod. Sed eget feugiat lorem, non sodales tellus. Nunc ornare tortor a viverra ullamcorper. In pharetra elementum erat, sed vulputate dui volutpat eu. Etiam tristique ex eu ex cursus ullamcorper. Nulla in lobortis purus. Donec in orci quam. Quisque vel magna dui. Curabitur et facilisis enim. Proin mollis tincidunt consectetur. Praesent bibendum tortor et dictum mollis. Vivamus molestie erat nunc, vitae suscipit arcu egestas id. Sed in enim libero. Nam ullamcorper lectus massa, ut accumsan leo malesuada nec. Integer at augue ornare, congue erat at, lobortis velit.
Cras at erat mi. Etiam sit amet posuere nibh. Aenean lobortis finibus ligula, sit amet efficitur arcu malesuada id. Integer sed maximus purus, quis egestas ipsum. Quisque ut cursus urna, et bibendum nisl. Sed in tincidunt ligula. Phasellus sed aliquam nulla, quis luctus justo. Curabitur tincidunt ex eget velit consequat, eu aliquam est pellentesque. Curabitur accumsan luctus arcu ac venenatis. Duis imperdiet lacinia maximus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer sodales efficitur tortor, ac sagittis turpis dignissim sagittis. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Quisque rhoncus molestie ipsum, a scelerisque urna pretium vel. Duis malesuada purus quis dolor dapibus porttitor. Curabitur venenatis hendrerit efficitur.
Suspendisse ac faucibus urna, quis maximus urna. Sed gravida velit nec diam vulputate consectetur. Etiam tortor metus, semper eu felis vitae, condimentum imperdiet lectus. Fusce eu suscipit justo, a ultricies massa. Sed molestie eget risus id interdum. Cras eu nisl ut augue iaculis accumsan a in odio. Nullam finibus posuere eros. Aliquam quis nunc in diam lacinia bibendum. Nam porttitor ex quis tortor tempor volutpat non sed sapien.
Vivamus bibendum nisi ut tincidunt dictum. Ut in vulputate massa, eu mollis eros. Nullam mattis tellus nec nisl lacinia, a aliquam quam lacinia. Mauris nunc sapien, ornare quis augue bibendum, elementum placerat turpis. Curabitur eget massa orci. Vestibulum sed neque vitae mi viverra feugiat. Morbi consectetur dui quis lectus blandit, vel aliquam ante fringilla.
Curabitur sed dui dapibus, sodales felis sed, ornare tellus. Sed erat felis, tincidunt vitae ligula in, laoreet aliquet ante. Duis id consectetur metus. Integer pulvinar, eros et ornare facilisis, augue metus eleifend diam, ut pretium erat neque eget lorem. Suspendisse semper congue elit sed malesuada. Integer tristique massa non turpis aliquam pretium. Morbi pharetra neque dolor, quis pretium risus dignissim sit amet. Aenean sit amet augue dui. Aenean pharetra scelerisque bibendum. Donec lacinia lectus sit amet nulla venenatis pulvinar. Maecenas tincidunt, velit quis tempor sagittis, sapien libero bibendum metus, et pretium elit ligula sit amet augue. Aliquam et nisl rhoncus, porttitor sapien a, congue neque. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Etiam dapibus lacus eu congue porta.
Cras porttitor, leo id ornare finibus, velit metus laoreet quam, vel consequat lacus justo sit amet nunc. Nullam suscipit auctor turpis, id egestas neque vehicula nec. Nulla nec lectus iaculis, dictum ligula efficitur, accumsan diam. Donec id tempor sapien, non varius velit. Donec eget ante at nunc sodales egestas. In in efficitur metus. Etiam ut vestibulum magna, et porttitor sem. Donec eu dui nec libero hendrerit congue ac id tellus. Vestibulum feugiat orci vitae tempor blandit. Fusce porttitor risus vitae lobortis finibus. Pellentesque vel blandit justo. Vivamus ut est vitae tortor aliquam ultricies. Aliquam et sagittis lectus, sit amet tincidunt tellus.
Nulla facilisi. Pellentesque luctus vestibulum metus at porta. Suspendisse elementum dictum elementum. Mauris placerat purus et lobortis laoreet. Nam laoreet vitae diam sit amet mattis. Vestibulum posuere arcu in malesuada lobortis. Phasellus sem nibh, lobortis eu tincidunt ut, scelerisque eget purus. Integer vulputate ullamcorper faucibus. Vestibulum nec facilisis nisi. Mauris placerat mauris faucibus, ultrices massa ut, vehicula enim. Duis auctor id turpis eu dapibus. Aenean condimentum sed nisl quis elementum. In luctus bibendum nunc. Suspendisse ornare eros eu lacus posuere malesuada. Duis mauris nunc, auctor ac libero tempor, ultrices eleifend mauris. Fusce quis ipsum lobortis, elementum orci vel, ultrices eros.
Donec at pretium libero. Fusce faucibus placerat ligula eget tincidunt. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vivamus lobortis viverra nisl non porttitor. Donec mollis mauris urna, quis vestibulum sem aliquam id. Phasellus at ornare eros. Fusce congue felis quis arcu luctus, at elementum tortor aliquet. Maecenas gravida est quis neque porta, id ullamcorper orci hendrerit. Maecenas mattis fermentum dui eu consectetur. Morbi aliquet feugiat risus, in sodales ligula tempor id. Fusce elementum faucibus ligula et molestie. Aenean sit amet sagittis dui. Vivamus hendrerit iaculis enim, et fermentum nisi lacinia id. Nulla facilisi. Mauris eu felis varius justo egestas venenatis.";
	}
}
