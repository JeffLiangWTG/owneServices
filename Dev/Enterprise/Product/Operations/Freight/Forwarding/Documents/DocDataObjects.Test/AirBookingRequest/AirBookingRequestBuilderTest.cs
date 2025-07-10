using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingRequestBuilderTest : TestCaseWithFactory
	{
		#region TestBuild

		public void TestBuild()
		{
			const string airlinePrefix = "057";

			if (!AirBookingTestHelper.SetAirlineTermsAndConditions(Factory, airlinePrefix, "By submitting or canceling this eBooking you confirm that you have read, understood and agree to all the terms and conditions for this Airline available here:\r\nhttp://some-link.com"))
			{
				Fail("Prereq: failed to set up T&Cs for test");
			}

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, airlinePrefix);
			PopulateSpecialHandlingCodes(consol, LivingHumanOrgansBlood);

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "goods1");

			var shipment2 = consol.Shipments.AddNew();
			PopulateShipment(shipment2, "081002", 14, 27, "goods2");

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);

			AssertArrayEqualsByElements(nameof(bookingRequest.TermsAndConditions),
				new ZString[]
				{
					"By submitting or canceling this eBooking you confirm that you have read, understood and agree to all the terms and conditions for this Airline",
					"available here:",
					"http://some-link.com"
				}, bookingRequest.TermsAndConditions);

			AssertContainsExactElementsInAnyOrder(nameof(bookingRequest.CarrierContractNumbers),
				new[]
				{
					"CON|12345",
					"CON|54321"
				},
				bookingRequest.CarrierContractNumbers.Select(n => $"{n.Type.Code}|{n.Value}"));

			AssertContainsExactElementsInAnyOrder(nameof(bookingRequest.SpecialHandlingItems),
				new[]
				{
					"LHO|Living Human Organs/Blood"
				},
				bookingRequest.SpecialHandlingItems.Select(shi => $"{shi.Code}|{shi.Description}"));

			AssertEquals(nameof(bookingRequest.SpecialHandling), "LHO", bookingRequest.SpecialHandling);
			AssertEquals(nameof(bookingRequest.BookingConfirmationNotes), "Carrier remarks", bookingRequest.BookingConfirmationNotes);
		}

		#endregion

		#region TestPopulateEBookingApiUrlCode

		public void TestPopulateEBookingApiUrlCode()
		{
			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode)))
				{
					var consol = Factory.New<ForwardingConsol>();
					PopulateConsol(consol);
					var bookingRequest = new AirBookingRequestBuilder(consol).Build();
					AssertEquals(nameof(bookingRequest.EBookingApiUrlCode), EBookingApiUrls.Constants.ProdCode, bookingRequest.EBookingApiUrlCode);
				}

				using (FreightDataRegistry.Instance.EBookingServicesApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EBookingApiUrls(EBookingApiUrls.Constants.TestCode)))
				{
					var consol = Factory.New<ForwardingConsol>();
					PopulateConsol(consol);

					var bookingRequest = new AirBookingRequestBuilder(consol).Build();
					AssertEquals(nameof(bookingRequest.EBookingApiUrlCode), EBookingApiUrls.Constants.TestCode, bookingRequest.EBookingApiUrlCode);
				}
			}

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://test.com"))
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol);

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertEquals(nameof(bookingRequest.EBookingApiUrlCode), "CUSTOM", bookingRequest.EBookingApiUrlCode);
			}
		}

		#endregion

		#region TestPopulateGoodsDescription

		public void TestPopulateGoodsDescription_NoShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals(nameof(bookingRequest.GoodsDescription), ZString.Empty, bookingRequest.GoodsDescription);
		}

		public void TestPopulateGoodsDescription_SingleShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals(nameof(bookingRequest.GoodsDescription), "123456789012345", bookingRequest.GoodsDescription);
		}

		public void TestPopulateGoodsDescription_ManyShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "goods1 description");

			var shipment2 = consol.Shipments.AddNew();
			PopulateShipment(shipment2, "081002", 12, 25, "goods2 description");

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals(nameof(bookingRequest.GoodsDescription), "Consolidation", bookingRequest.GoodsDescription);
		}

		public void TestPopulateGoodsDescriptionList_ForAirlineNotSupportingCommodities()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "074");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(nameof(bookingRequest.GoodsDescriptionCollection), bookingRequest.GoodsDescriptionCollection);
			}
		}

		public void TestPopulateGoodsDescriptionList_ForAirlineSupportingCommodities()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNotNull(nameof(bookingRequest.GoodsDescriptionCollection), bookingRequest.GoodsDescriptionCollection);

				var descriptions = bookingRequest.GoodsDescriptionCollection
					.Cast<AirlineConfigCommodityBusinessObject>()
					.Select(bizObj => bizObj.Description)
					.OrderBy(description => description);

				AssertMultilineASCIIEquals("GoodsDescriptionsList for Etihad Airways (607)",
@"ACCESSORIES
ADHESIVE
AEROSOL
AIRCRAFT ENGINES
AIRCRAFT PARTS
GENERAL CARGO
OTHER",
	string.Join("\r\n", descriptions));
			}
		}

		#endregion

		#region TestPopulateProduct

		public void TestPopulateProductList_ForAirlineNotSupportingProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "074");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(nameof(bookingRequest.ProductList), bookingRequest.ProductList);

				PopulateConsol(consol, "618");
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(nameof(bookingRequest.ProductList), bookingRequest.ProductList);
			}
		}

		public void TestPopulateProductList_ForAirlineSupportingProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNotNull(nameof(bookingRequest.ProductList), bookingRequest.ProductList);
				AssertMultilineASCIIEquals("GoodsDescriptionsList for Etihad Airways (607)",
					@"AIRCRAFT ENGINES
Emirates AOG
SkyWheels Premium Door-to-Door",
					string.Join("\r\n", bookingRequest.ProductList.GetAllCodes().OrderBy(product => product)));
			}
		}

		public void TestPopulateProductList_ForAirlineSupportingProductsFromRefDb_WhenCommodityPivotPresent()
		{
			PopulateProductCodeCommodityCodePivot("020");
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");
			Factory.Save();
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			Assert("CommodityIsVisible should be true", bookingRequest.CommodityIsVisible);
			AssertNotNull(nameof(bookingRequest.ProductList), bookingRequest.ProductList);
			AssertMultilineASCIIEquals("ProductDescriptionList for Airways (020)",
				@"Product 1
Product 2
Product 3",
				string.Join("\r\n", bookingRequest.ProductList.GetAllCodes().OrderBy(product => product)));
		}

		public void TestPopulateProductList_ForAirlineSupportingProductsFromRefDb_DefaultsFirstProduct_WhenThereIsJustOneProductInTheList()
		{
			PopulateProductCodeCommodityCodePivot("020", onlyPopulateOneProduct: true);

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var airTransports = consol
				.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				.ToArray();

			Assert(airTransports.Length > 0);

			airTransports.First().JW_RL_NKLoadPort = ZString.Empty;
			airTransports.Last().JW_RL_NKDiscPort = ZString.Empty;

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "12345678901234567890");
			Factory.Save();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			Assert("CommodityIsVisible should be true", bookingRequest.CommodityIsVisible);
			AssertNotNull(nameof(bookingRequest.ProductList), bookingRequest.ProductList);
			AssertNotNullOrEmpty(nameof(bookingRequest.Product), bookingRequest.Product);
			AssertNotNull(nameof(bookingRequest.CommodityCollection), bookingRequest.CommodityCollection);
			AssertMultilineASCIIEquals("ProductDescriptionList for Airways (020)",
				@"Product 2",
				string.Join("\r\n", bookingRequest.ProductList.GetAllCodes().OrderBy(product => product)));

			var commodityDescriptions = bookingRequest.CommodityCollection
					.Cast<AirlineConfigCommodityBusinessObject>()
					.Select(bizObj => bizObj.Description)
					.OrderBy(description => description);
			AssertMultilineASCIIEquals("CommodityDescriptionList for Airways (020)",
				@"Commodity 3
Commodity 4",
				string.Join("\r\n", commodityDescriptions));
		}

		#endregion

		#region TestPopulateCommodity

		public void TestCommodityIsVisible_IsTrue_ForAirlineSupportingIATACommodity()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "074");
				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				Assert("Should hide Commodity when airline does not support commodity", !bookingRequest.CommodityIsVisible);

				PopulateConsol(consol, "607");
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				Assert("Should show Commodity when airline CommodityCode option is Optional", bookingRequest.CommodityIsVisible);

				PopulateConsol(consol, "618");
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				Assert("Should show Commodity when airline CommodityCode option is Required", bookingRequest.CommodityIsVisible);
			}
		}

		public void TestPopulateCommodityCodeList_WhenProductCodeChanges_And_CommodityCodePivotPresent()
		{
			PopulateProductCodeCommodityCodePivot("020");
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "12345678901234567890");

			Factory.Save();
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			bookingRequest.Commodity = "ABC";
			bookingRequest.OriginAirport.Code = ZString.Empty;
			bookingRequest.DestinationAirport.Code = ZString.Empty;
			bookingRequest.Product = bookingRequest.ProductList[0].Code;
			var descriptions = bookingRequest.CommodityCollection
					.Cast<AirlineConfigCommodityBusinessObject>()
					.Select(bizObj => bizObj.Description)
					.OrderBy(description => description);

			AssertMultilineASCIIEquals("CommodityList for selected product loads from commodity codes in ref db",
@"Commodity 1
Commodity 2",
string.Join("\r\n", descriptions));
			bookingRequest.Product = bookingRequest.ProductList[1].Code;
			descriptions = bookingRequest.CommodityCollection
					.Cast<AirlineConfigCommodityBusinessObject>()
					.Select(bizObj => bizObj.Description)
					.OrderBy(description => description);

			AssertMultilineASCIIEquals("CommodityList for selected product loads from commodity codes in ref db",
@"Commodity 3
Commodity 4",
string.Join("\r\n", descriptions));

			bookingRequest.Product = bookingRequest.ProductList[2].Code;
			AssertNull("CommodityList should be null if no ProductCommodityPivot present for given product", bookingRequest.CommodityCollection);
			AssertNullOrEmpty("CommodityCode should be empty", bookingRequest.Commodity);
		}

		public void TestPopulateCommodityCodeList_ShouldDefaulfOverridenValue()
		{
			PopulateProductCodeCommodityCodePivot("020");

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var airTransports = consol.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				.ToArray();

			Assert(airTransports.Length > 0);

			airTransports.First().JW_RL_NKLoadPort = ZString.Empty;
			airTransports.Last().JW_RL_NKDiscPort = ZString.Empty;

			var bookingRequestOverride =
@"<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
	<Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
	<Property Name=""Product"">
		<Value>Product 2</Value>
	</Property>
</Entity>";
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = consol.TablePrefix;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.AirBookingRequest;
			documentData.JDD_OverriddenData = bookingRequestOverride;
			documentData.JDD_ParentID = consol.PK;

			Factory.Save();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			var descriptions = bookingRequest.CommodityCollection
					.Cast<AirlineConfigCommodityBusinessObject>()
					.Select(bizObj => bizObj.Description)
					.OrderBy(description => description);
			AssertMultilineASCIIEquals("CommodityList for overriden product loads from commodity codes in ref db",
@"Commodity 3
Commodity 4",
string.Join("\r\n", descriptions));
		}

		#endregion

		#region TestPopulateSpecialHandling

		public void TestPopulateSpecialHandlingFromConsol_ReEnteringRequestBuilder()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");

				var bookingRequestOverride =
@"<Entity DataMajorVersion=""3"" DataMinorVersion=""0"">
  <Id>3cd9b974-fed9-4f4b-88ec-e0268221ddc3</Id>
  <Property Name=""GoodsDescription"">
    <Value>ACCESRY</Value>
  </Property>
</Entity>";
				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentTableCode = consol.TablePrefix;
				documentData.JDD_Name = ConsolDocumentDataStoreNames.AirBookingRequest;
				documentData.JDD_OverriddenData = bookingRequestOverride;
				documentData.JDD_ParentID = consol.PK;

				Factory.Save();

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();

				AssertEquals("GEN, HVY, HUM", bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be filled", 3, bookingRequest.SpecialHandlingItems.Count);
				AssertEquals("GEN", bookingRequest.SpecialHandlingItems.ToArray()[0].Code);
				AssertEquals("HVY", bookingRequest.SpecialHandlingItems.ToArray()[1].Code);
				AssertEquals("HUM", bookingRequest.SpecialHandlingItems.ToArray()[2].Code);
			}
		}

		public void TestPopulateSpecialHandlingFromConsol_ConsolHasNoSpecialHandligCodes()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "081");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(bookingRequest.GoodsDescriptionCollection);

				AssertEquals($"{nameof(bookingRequest.SpecialHandling)}: NON security code has been ignored", ZString.Empty, bookingRequest.SpecialHandling);

				bookingRequest.GoodsDescription = "ACCESSORIES";
				AssertEquals(nameof(bookingRequest.SpecialHandling), ZString.Empty, bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be empty", 0, bookingRequest.SpecialHandlingItems.Count);
			}
		}

		public void TestPopulateSpecialHandlingFromConsol_ConsolHasSpecialHandlingCodes()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "081");
				PopulateSpecialHandlingCodes(consol, CargoAircraftOnly, CarbonDioxideSolidDryIce);
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(bookingRequest.GoodsDescriptionCollection);

				AssertEquals($"{nameof(bookingRequest.SpecialHandling)}: security code has been included", "CAO, ICE, NSC", bookingRequest.SpecialHandling);

				bookingRequest.GoodsDescription = "ACCESSORIES";
				AssertEquals(nameof(bookingRequest.SpecialHandling), "CAO, ICE, NSC", bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be filled", 3, bookingRequest.SpecialHandlingItems.Count);
				AssertEquals("CAO", bookingRequest.SpecialHandlingItems.ToArray()[0].Code);
				AssertEquals("ICE", bookingRequest.SpecialHandlingItems.ToArray()[1].Code);
				AssertEquals("NSC", bookingRequest.SpecialHandlingItems.ToArray()[2].Code);
			}
		}

		public void TestPopulateSpecialHandlingFromGoodsDescription_ConsolHasNoSpecialHandligCodes()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "607001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertEquals(7, bookingRequest.GoodsDescriptionCollection.Count);

				AssertEquals($"{nameof(bookingRequest.SpecialHandling)}: NON security code has been ignored", ZString.Empty, bookingRequest.SpecialHandling);

				bookingRequest.GoodsDescription = "ACCESRY";
				AssertEquals(nameof(bookingRequest.SpecialHandling), "GEN, HVY, HUM", bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be filled", 3, bookingRequest.SpecialHandlingItems.Count);
				AssertEquals("GEN", bookingRequest.SpecialHandlingItems.ToArray()[0].Code);
				AssertEquals("HVY", bookingRequest.SpecialHandlingItems.ToArray()[1].Code);
				AssertEquals("HUM", bookingRequest.SpecialHandlingItems.ToArray()[2].Code);
			}
		}

		public void TestPopulateSpecialHandlingFromGoodsDescription_ConsolHasSpecialHandligCodes()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");
				PopulateSpecialHandlingCodes(consol, CargoAircraftOnly, CarbonDioxideSolidDryIce);
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "607001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertEquals(7, bookingRequest.GoodsDescriptionCollection.Count);

				AssertEquals($"{nameof(bookingRequest.SpecialHandling)}: security code for EY has been included", "CAO, ICE, NSC", bookingRequest.SpecialHandling);

				bookingRequest.GoodsDescription = "ACCESRY";
				AssertEquals(nameof(bookingRequest.SpecialHandling), "CAO, ICE, NSC, GEN, HVY, HUM", bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be filled", 6, bookingRequest.SpecialHandlingItems.Count);
				AssertEquals("CAO", bookingRequest.SpecialHandlingItems.ToArray()[0].Code);
				AssertEquals("ICE", bookingRequest.SpecialHandlingItems.ToArray()[1].Code);
				AssertEquals("NSC", bookingRequest.SpecialHandlingItems.ToArray()[2].Code);
				AssertEquals("GEN", bookingRequest.SpecialHandlingItems.ToArray()[3].Code);
				AssertEquals("HVY", bookingRequest.SpecialHandlingItems.ToArray()[4].Code);
				AssertEquals("HUM", bookingRequest.SpecialHandlingItems.ToArray()[5].Code);
			}
		}

		public void TestPopulateSpecialHandlingFromGoodsDescription_CanUseRefAirlineCommodityCode()
		{
			// Arrange
			const string airlinePrefix = "057";
			PopulateProductCodeCommodityCodePivot(airlinePrefix);
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, airlinePrefix);
			var airBookingRequestBuilder = new AirBookingRequestBuilder(consol);
			var airBookingRequest = airBookingRequestBuilder.Build();

			var specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
			AssertEquals("Special Handling Items are not expected, because no Product is selected.",
				0, specialHandlingItems.Length);

			airBookingRequest.Product = "Product 2";
			airBookingRequest.Commodity = "CO1";
			specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
			AssertEquals("No Special Handling codes are expected, because \"CO1\" Commodity does not belong to \"Product 2\".",
				0, specialHandlingItems.Length);

			airBookingRequest.Commodity = "CO2";
			specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
			AssertEquals("No Special Handling codes are expected, because \"CO2\" Commodity does not belong to \"Product 2\".",
				0, specialHandlingItems.Length);

			airBookingRequest.Commodity = "CO3";
			specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
			AssertEquals("Three Special Handling Codes are expected, because \"CO3\" Commodity belongs to \"Product 2\" with empty and whitespace Codes stripped out.",
				3, specialHandlingItems.Length);
			AssertEquals("\"CO3\" 1st Special Handling Code is expected.", "SH.2", specialHandlingItems[0].Code);
			AssertEquals("\"CO3\" 2nd Special Handling Code is expected.", CargoAircraftOnly, specialHandlingItems[1].Code);
			AssertEquals("\"CO3\" 3rd Special Handling Code is expected.", "SH.3", specialHandlingItems[2].Code);

			airBookingRequest.Commodity = "CO4";
			specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
			AssertEquals("\"CO4\" Commodity belongs to \"Product 2\", but no Special Handling Codes are expected because there are just whitespaces.",
				0, specialHandlingItems.Length);
		}

		public void TestPopulateSpecialHandlingFromGoodsDescription_CanUseConsolAndRefAirlineCommodityCodes()
		{
			// Arrange
			const string airlinePrefix = "057";
			PopulateProductCodeCommodityCodePivot(airlinePrefix);
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, airlinePrefix);
			PopulateSpecialHandlingCodes(consol, CargoAircraftOnly);
			var airBookingRequestBuilder = new AirBookingRequestBuilder(consol);
			var airBookingRequest = airBookingRequestBuilder.Build();

			// Assert
			CombineAssertions(() =>
			{
				var specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
				AssertEquals("Product is not selected, but one Consol Special Handling Code is defined.",
					1, specialHandlingItems.Length);
				AssertEquals("Consol Special Handling Code is expected.",
					CargoAircraftOnly, specialHandlingItems.ElementAtOrDefault(0)?.Code);

				airBookingRequest.Product = "Product 2";
				airBookingRequest.Commodity = "CO3";
				specialHandlingItems = airBookingRequest.SpecialHandlingItems.ToArray();
				AssertEquals("The unique combination of Consol & Commodity Special Handling Codes is expected.", 3, specialHandlingItems.Length);
				AssertEquals("Consol Special Handling Code is expected first.", CargoAircraftOnly, specialHandlingItems[0].Code);
				AssertEquals("\"CO3\" 1st Commodity Special Handling Code is expected.", "SH.2", specialHandlingItems[1].Code);
				AssertEquals("\"CO3\" 3rd Commodity Special Handling Code is expected.", "SH.3", specialHandlingItems[2].Code);
			});
		}

		public void TestPopulateSpecialHandlingFromGoodsDescription_NoDuplicateSpecialHandlingCodes()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");
				PopulateSpecialHandlingCodes(consol, CargoAircraftOnly, HumanRemainsInCoffins);

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "607001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertEquals(7, bookingRequest.GoodsDescriptionCollection.Count);

				AssertEquals($"{nameof(bookingRequest.SpecialHandling)}: NON security code has been ignored", "CAO, HUM", bookingRequest.SpecialHandling);

				bookingRequest.GoodsDescription = "ACCESRY";
				AssertEquals(nameof(bookingRequest.SpecialHandling), "CAO, HUM, GEN, HVY", bookingRequest.SpecialHandling);
				AssertEquals("SpecialHandlingItems should be filled", 4, bookingRequest.SpecialHandlingItems.Count);
				AssertEquals("CAO", bookingRequest.SpecialHandlingItems.ToArray()[0].Code);
				AssertEquals("HUM", bookingRequest.SpecialHandlingItems.ToArray()[1].Code);
				AssertEquals("GEN", bookingRequest.SpecialHandlingItems.ToArray()[2].Code);
				AssertEquals("HVY", bookingRequest.SpecialHandlingItems.ToArray()[3].Code);
			}
		}

		public void TestPopulateSpecialHandling_ShouldNotContainSecurityStatusCode_WhenItIsNON()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			consol.JK_MasterBillNum = "020745678";
			consol.SecurityStatusCode = SecurityJobConsolAWBSpecialHandling.NotSecured;
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotContains(SecurityJobConsolAWBSpecialHandling.NotSecured, bookingRequest.SpecialHandling);
		}

		public void TestPopulateSpecialHandling_ShouldContainSecurityStatusCode_WhenItHasValueOtherThanNON()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			consol.JK_MasterBillNum = "607745678";
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertContains(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly, bookingRequest.SpecialHandling);
		}

		#endregion

		#region TestPackingLineDimensions

		public void TestPackingLineDimensions_DoesNotConvertUnit()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Feet;

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_UnitOfDimension = Core.Constants.Length.Metres;

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);

			CombineAssertions("Should not convert units, ABE should do unit conversion", () =>
			{
				AssertContainsExactElementsInAnyOrder("Length unit",
					new[] { "M", "FT" },
					bookingRequest.Dimensions.Select(d => d.Length.Unit.Code).Distinct()
				);

				AssertContainsExactElementsInAnyOrder("Width unit",
					new[] { "M", "FT" },
					bookingRequest.Dimensions.Select(d => d.Width.Unit.Code).Distinct()
				);

				AssertContainsExactElementsInAnyOrder("Height unit",
					new[] { "M", "FT" },
					bookingRequest.Dimensions.Select(d => d.Height.Unit.Code).Distinct()
				);
			});
		}

		public void TestPackingLineDimensions_DoesNotRoundValues()
		{
			var unroundedDecimal = (ZDecimal)1.124;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_Width = packingLine1.JL_Height = packingLine1.JL_Length = unroundedDecimal;
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Centimetres;

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);

			CombineAssertions("Should not round values to 0 decimal places, ABE should do rounding", () =>
			{
				AssertEquals("Length unit",
					unroundedDecimal,
					bookingRequest.Dimensions.First().Length.Value
				);

				AssertEquals("Width unit",
					unroundedDecimal,
					bookingRequest.Dimensions.First().Width.Value
				);

				AssertEquals("Height unit",
					unroundedDecimal,
					bookingRequest.Dimensions.First().Height.Value
				);
			});
		}

		public void TestPackingLineDimensions_DoesNotConvertTotalWeight()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;
			packingLine1.JL_ActualWeight = 100m;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Grams;

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 2;
			packingLine2.JL_ActualWeight = 100m;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);
			AssertEquals(2, bookingRequest.Dimensions.Count);

			var pieces1 = bookingRequest.Dimensions.First();
			var pieces2 = bookingRequest.Dimensions.ElementAt(1);

			CombineAssertions("First pack line - should not convert weight units", () =>
			{
				AssertEquals(1, pieces1.Quantity);
				AssertEquals(100m, pieces1.Weight.Value);
				AssertEquals(Core.Constants.Weight.Grams, pieces1.Weight.Unit.Code);
			});

			CombineAssertions("Second pack line - should not convert weight units", () =>
			{
				AssertEquals(2, pieces2.Quantity);
				AssertEquals(100m, pieces2.Weight.Value);
				AssertEquals(Core.Constants.Weight.Pounds, pieces2.Weight.Unit.Code);
			});
		}

		#endregion

		#region TestPopulateGoodsDetails

		#region TestPopulateGoodsDetails_TotalPieces

		public void TestPopulateGoodDetails_TotalPieces_UsesPreAllocation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100;
			consol.WeightVerificationUnit = Weight.Kilograms;

			var shipment1 = consol.Shipments.AddNew();
			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.SetContainer(consol, null);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);
			AssertEquals("Pieces is populated from PreAllocation as Weight is entered", 0, bookingRequest.TotalPieces);

			consol.JK_TotalShipmentCountCheck = 2;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Pieces is populated from PreAllocation as Shipments is entered", 2, bookingRequest.TotalPieces);
		}

		public void TestPopulateGoodsDetails_TotalPieces_IfPreAllocationHasNoValues_UsesContainersOrLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 4;

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.SetContainer(consol, null);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);
			AssertEquals("Summary of counts of Dimensions and ULDs", 18, bookingRequest.TotalPieces);
		}

		#endregion

		#region TestPopulateGoodsDetails_Dimensions

		public void TestPopulateGoodsDetails_Dimensions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";
			consol.JK_TotalShipmentCountCheck = 2;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 4;

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.SetContainer(consol, null);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);
			AssertEquals("Dimensions count with Non-empty pre-allocation", 1, bookingRequest.Dimensions.Count);

			consol.JK_MaximumAllowablePackageLength = 2m;
			consol.JK_MaximumAllowablePackageHeight = 4m;
			consol.JK_MaximumAllowablePackageWidth = 6m;
			consol.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Pre-allocation with dimensions", 1, bookingRequest.Dimensions.Count);

			var packingLine = bookingRequest.Dimensions.First();
			AssertEquals(2m, packingLine.Length.Value);
			AssertEquals(Core.Constants.Length.Metres, packingLine.Length.Unit.Code);
			AssertEquals(4m, packingLine.Height.Value);
			AssertEquals(Core.Constants.Length.Metres, packingLine.Height.Unit.Code);
			AssertEquals(6m, packingLine.Width.Value);
			AssertEquals(Core.Constants.Length.Metres, packingLine.Width.Unit.Code);

			consol.JK_TotalShipmentCountCheck = 0;
			consol.JK_MaximumAllowablePackageLength = 0m;
			consol.JK_MaximumAllowablePackageHeight = 0m;
			consol.JK_MaximumAllowablePackageWidth = 0m;

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Empty pre-allocation, should use shipments", 2, bookingRequest.Dimensions.Count);
		}

		#endregion

		#region TestPopulateGoodsDetails_TotalWeight

		public void TestPopulateGoodsDetails_TotalWeight_UsesPreAllocation()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers or lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalWeight.Value);
				AssertEquals("Default weight unit", Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 4;
			container2.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			// packingLine1 & 2 weight should be overwritten by pre-allocation
			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_ActualWeight = 2M;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_ActualWeight = 3.2M;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine2.SetContainer(consol, null);

			// packingLine3 weight should add up to container weight and total weight
			var packingLine3 = shipment1.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 6;
			packingLine3.JL_ActualWeight = 110M;
			packingLine3.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine3.SetContainer(consol, container1);

			#endregion

			consol.JK_TotalShipmentActWeightCheck = 1500;
			consol.WeightVerificationUnit = Core.Constants.Weight.Grams;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-allocation and container has different weight units", () =>
			{
				AssertEquals("1500G + 110LB×453.592 grams/pound = ", 51395.2m, bookingRequest.TotalWeight.Value);
				AssertEquals("Should use pre-allocation weight unit", Core.Constants.Weight.Grams, bookingRequest.TotalWeight.Unit.Code);
			});

			consol.WeightVerificationUnit = Weight.Pounds;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-allocation and container has same weight units", () =>
			{
				AssertEquals("1500LB + 110LB = ", 1610m, bookingRequest.TotalWeight.Value);
				AssertEquals(Weight.Pounds, bookingRequest.TotalWeight.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalWeight_ZeroWeightAndBlankWeightUnit_UsesPreAllocation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";
			consol.JK_TotalShipmentActWeightCheck = 0;
			consol.WeightVerificationUnit = string.Empty;

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown(() => new AirBookingRequestBuilder(consol).Build());
			}
		}

		public void TestPopulateGoodsDetails_TotalWeight_IfHasContainersOrLines_AllUnitsImperial()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers or lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalWeight.Value);
				AssertEquals("Default weight unit", Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 4;
			container2.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_ActualWeight = 2M;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_ActualWeight = 3.2M;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine2.SetContainer(consol, null);

			var packingLine3 = shipment1.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 6;
			packingLine3.JL_ActualWeight = 110M;
			packingLine3.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine3.SetContainer(consol, container1);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Empty pre-allocation, should calculate based on containers/lines with all imperial units", () =>
			{
				AssertEquals(115.2m, bookingRequest.TotalWeight.Value);
				AssertEquals(Core.Constants.Weight.Pounds, bookingRequest.TotalWeight.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalWeight_IfHasContainersOrLines_AllUnitsMetric()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers or lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalWeight.Value);
				AssertEquals("Default weight unit", Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 3;
			packingLine1.JL_ActualWeight = 2.222M;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 3;
			packingLine2.JL_ActualWeight = 2.301M;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packingLine2.SetContainer(consol, null);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Empty pre-allocation, should calculate based on containers/lines with all metric units", () =>
			{
				AssertEquals(4.5m, bookingRequest.TotalWeight.Value);
				AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalWeight_IfHasContainersOrLines_MixedUnits()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers or lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalWeight.Value);
				AssertEquals("Default weight unit", Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 4;
			container2.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_ActualWeight = 2M;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_ActualWeight = 3.2M;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine2.SetContainer(consol, null);

			var packingLine3 = shipment1.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 6;
			packingLine3.JL_ActualWeight = 110M;
			packingLine3.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packingLine3.SetContainer(consol, container1);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Should default to metric units if mixed units", () =>
			{
				AssertEquals(52.3m, bookingRequest.TotalWeight.Value);
				AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalWeight_UsesGoodsWeightInCalculationAndUnit()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers or lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalWeight.Value);
				AssertEquals("Default weight unit", Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "LD-7";
			refcontainer.RC_TareWeight = 200m;
			refcontainer.RC_GrossWeight = 907.684m;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = refcontainer.PK;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			var shipment1 = consol.Shipments.AddNew();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_ActualWeight = 123M;
			packingLine1.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 1;
			packingLine2.JL_ActualWeight = 321M;
			packingLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packingLine2.SetContainer(consol, container);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			var uld1 = bookingRequest.Ulds.First();
			var loose1 = bookingRequest.Dimensions.First();

			CombineAssertions("Uses goods weight in calculation and container units", () =>
			{
				AssertEquals("321kg", 707.684m, uld1.GoodsWeight.Value);
				AssertEquals(Core.Constants.Weight.Pounds, uld1.GoodsWeight.Unit.Code);

				AssertEquals("0.123kg", 123m, loose1.Weight.Value);
				AssertEquals(Core.Constants.Weight.Grams, loose1.Weight.Unit.Code);

				AssertEquals("200kg + 321kg + 0.123kg =", 521.1m, bookingRequest.TotalWeight.Value);
				AssertEquals(Core.Constants.Weight.Kilograms, bookingRequest.TotalWeight.Unit.Code);
			});
		}

		#endregion

		#region TestPopulateGoodsDetails_TotalVolume

		public void TestPopulateGoodsDetails_TotalVolume_UsesPreAllocation()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers/lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalVolume.Value);
				AssertEquals("Default volume unit", Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});

			var shipment = consol.Shipments.AddNew();

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_Length = 5M;
			packingLine1.JL_Width = 3M;
			packingLine1.JL_Height = 2M;
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_Length = 3M;
			packingLine2.JL_Width = 4M;
			packingLine2.JL_Height = 5M;
			packingLine2.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packingLine2.SetContainer(consol, null);

			#endregion

			consol.JK_TotalShipmentActVolumeCheck = 12.3m;
			consol.VolumeVerificationUnit = Core.Constants.Volume.Litre;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-allocation with valid value and unit", () =>
			{
				AssertEquals(12.3m, bookingRequest.TotalVolume.Value);
				AssertEquals("Should use pre-allocation volume unit", Core.Constants.Volume.Litre, bookingRequest.TotalVolume.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalVolume_IfHasContainersOrLines_AllUnitsImperial()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers/lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalVolume.Value);
				AssertEquals("Default volume unit", Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "20PP";
			refcontainer.RC_CubicCapacity = 16m;
			refcontainer.RC_Height = 3m;
			refcontainer.RC_Length = 0.6m;
			refcontainer.RC_Width = 2m;
			refcontainer.RC_TareWeight = 1000m;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 3;
			container.JC_RC = refcontainer.PK;

			var shipment = consol.Shipments.AddNew();

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_Length = 5M;
			packingLine1.JL_Width = 3M;
			packingLine1.JL_Height = 2M;
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_Length = 3M;
			packingLine2.JL_Width = 4M;
			packingLine2.JL_Height = 5M;
			packingLine2.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packingLine2.SetContainer(consol, null);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Empty pre-allocation, should calculate based on containers/lines with all imperial units", () =>
			{
				AssertEquals(510.306m, bookingRequest.TotalVolume.Value);
				AssertEquals(Core.Constants.Volume.CubicFeet, bookingRequest.TotalVolume.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalVolume_IfHasContainersOrLines_AllUnitsMetric()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers/lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalVolume.Value);
				AssertEquals("Default volume unit", Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});

			var shipment = consol.Shipments.AddNew();

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 3;
			packingLine1.JL_Length = 87m;
			packingLine1.JL_Width = 99m;
			packingLine1.JL_Height = 86m;
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Centimetres;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 3;
			packingLine2.JL_Length = 95m;
			packingLine2.JL_Width = 85m;
			packingLine2.JL_Height = 95m;
			packingLine2.JL_UnitOfDimension = Core.Constants.Length.Centimetres;
			packingLine2.SetContainer(consol, null);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Empty pre-allocation, should calculate based on containers/lines with all metric units", () =>
			{
				AssertEquals(4.523m, bookingRequest.TotalVolume.Value);
				AssertEquals(Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});
		}

		public void TestPopulateGoodsDetails_TotalVolume_IfHasContainersOrLines_MixedUnits()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Pre-Condition - Default when pre-allocation is empty, no containers/lines", () =>
			{
				AssertEquals(0m, bookingRequest.TotalVolume.Value);
				AssertEquals("Default volume unit", Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "20PP";
			refcontainer.RC_CubicCapacity = 16m;
			refcontainer.RC_Height = 3m;
			refcontainer.RC_Length = 0.6m;
			refcontainer.RC_Width = 2m;
			refcontainer.RC_TareWeight = 1000m;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 3;
			container.JC_RC = refcontainer.PK;
			container.JC_TotalLength = 0.6m;
			container.JC_TotalHeight = 3m;
			container.JC_TotalWidth = 2m;

			var shipment = consol.Shipments.AddNew();

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_Length = 5M;
			packingLine1.JL_Width = 3M;
			packingLine1.JL_Height = 2M;
			packingLine1.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packingLine1.SetContainer(consol, null);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 6;
			packingLine2.JL_Length = 3M;
			packingLine2.JL_Width = 4M;
			packingLine2.JL_Height = 5M;
			packingLine2.JL_UnitOfDimension = Core.Constants.Length.Metres;
			packingLine2.SetContainer(consol, null);

			#endregion

			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Should default to metric units if mixed units", () =>
			{
				AssertEquals(364.554m, bookingRequest.TotalVolume.Value);
				AssertEquals(Core.Constants.Volume.CubicMetres, bookingRequest.TotalVolume.Unit.Code);
			});

			packingLine2.SetContainer(consol, container);
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			CombineAssertions("Should only consider lenght on packinglines. Container dims reference data is always imperial", () =>
			{
				AssertEquals("Different value because it uses container dimensions", 150.306m, bookingRequest.TotalVolume.Value);
				AssertEquals(Core.Constants.Volume.CubicFeet, bookingRequest.TotalVolume.Unit.Code);
			});
		}

		#endregion

		#region TestPopulateGoodsDetails_CommodityCode

		public void TestPopulateGoodsDetails_CommodityCode_ShouldNotDefaultOverridenCommodityCode()
		{
			PopulateProductCodeCommodityCodePivot("020");

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var airTransports = consol.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				.ToArray();

			Assert(airTransports.Length > 0);

			airTransports.First().JW_RL_NKLoadPort = ZString.Empty;
			airTransports.Last().JW_RL_NKDiscPort = ZString.Empty;

			var bookingRequestOverride =
@"<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
	<Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
	<Property Name=""Product"">
		<Value>Product 1</Value>
	</Property>
	<Property Name=""Commodity"">
		<Value>Test</Value>
	</Property>
</Entity>";
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = consol.TablePrefix;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.AirBookingRequest;
			documentData.JDD_OverriddenData = bookingRequestOverride;
			documentData.JDD_ParentID = consol.PK;

			Factory.Save();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Test", bookingRequest.Commodity);
		}

		public void TestPopulateGoodsDetails_CommodityCode_IfExactOriginAndDestinationMatch()
		{
			var bookingRequest = SetupBookingRequestAndSetProduct("857", "AUSYD", "BRSAO");

			AssertEquals("CO3", bookingRequest.Commodity);
			AssertHasWarning(bookingRequest.CommodityInfo, "Please note a default Commodity Code has been applied from airline 857 reference file. This code can be overridden.");

			bookingRequest.Product = "Invalid Code";
			AssertEquals("", bookingRequest.Commodity);
			AssertNoWarning(bookingRequest.CommodityInfo, "Please note a default Commodity Code has been applied from airline 857 reference file. This code can be overridden.");
		}

		public void TestPopulateGoodsDetails_CommodityCode_IfMatchOrigin()
		{
			var bookingRequest = SetupBookingRequestAndSetProduct("857", "AUSYD", "BRSAO", false, true);

			AssertEquals("CO2", bookingRequest.Commodity);
			AssertHasWarning(bookingRequest.CommodityInfo, "Please note a default Commodity Code has been applied from airline 857 reference file. This code can be overridden.");
		}

		public void TestPopulateGoodsDetails_CommodityCode_IfMatchDestination()
		{
			var bookingRequest = SetupBookingRequestAndSetProduct("857", "AUSYD", "BRSAO", true);

			AssertEquals("CO4", bookingRequest.Commodity);
			AssertHasWarning(bookingRequest.CommodityInfo, "Please note a default Commodity Code has been applied from airline 857 reference file. This code can be overridden.");
		}

		public void TestPopulateGoodsDetails_CommodityCode_IfOriginOrDestinationIsBlank()
		{
			var bookingRequest = SetupBookingRequestAndSetProduct("857", "AUSYD", "BRSAO", true, true);

			AssertNotNull(nameof(bookingRequest.CommodityCollection), bookingRequest.CommodityCollection);

			AssertEquals("CO1", bookingRequest.Commodity);
			AssertHasWarning(bookingRequest.CommodityInfo, "Please note a default Commodity Code has been applied from airline 857 reference file. This code can be overridden.");
		}

		void PopulateRefAirlineDefaultCommodityCode(ZString airlinePrefix, ZString origin, ZString destination)
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = airlinePrefix;

			var defaultCommodityCode1 = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode1.RDC_RAC_NKCommodityCode = "CO1";
			defaultCommodityCode1.RDC_RAR_NKProductCode = "PR1";
			defaultCommodityCode1.RDC_RM = airline.PK;

			var defaultCommodityCode2 = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode2.RDC_RAC_NKCommodityCode = "CO2";
			defaultCommodityCode2.RDC_RAR_NKProductCode = "PR1";
			defaultCommodityCode2.RDC_RM = airline.PK;
			defaultCommodityCode2.RDC_RL_NKOrigin = origin;

			var defaultCommodityCode3 = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode3.RDC_RAC_NKCommodityCode = "CO3";
			defaultCommodityCode3.RDC_RAR_NKProductCode = "PR1";
			defaultCommodityCode3.RDC_RM = airline.PK;
			defaultCommodityCode3.RDC_RL_NKOrigin = origin;
			defaultCommodityCode3.RDC_RL_NKDestination = destination;

			var defaultCommodityCode4 = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode4.RDC_RAC_NKCommodityCode = "CO4";
			defaultCommodityCode4.RDC_RAR_NKProductCode = "PR1";
			defaultCommodityCode4.RDC_RM = airline.PK;
			defaultCommodityCode4.RDC_RL_NKDestination = destination;

			Factory.Save();
		}

		AirBookingRequest SetupBookingRequestAndSetProduct(ZString airlinePrefix, ZString origin, ZString destination, bool setLoadPortToEmpty = false, bool setDichargePortToEmpty = false)
		{
			PopulateRefAirlineDefaultCommodityCode(airlinePrefix, origin, destination);

			PopulateProductCodeCommodityCodePivot(airlinePrefix, true, true);

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, airlinePrefix);

			if (setLoadPortToEmpty || setDichargePortToEmpty)
			{
				var airTransports = consol
					.Transports
					.Cast<Freight.Business.Transport>()
					.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
					.ToArray();

				Assert(airTransports.Length > 0);

				if (setLoadPortToEmpty)
				{
					airTransports.First().JW_RL_NKLoadPort = ZString.Empty;
				}

				if (setDichargePortToEmpty)
				{
					airTransports.Last().JW_RL_NKDiscPort = ZString.Empty;
				}
			}

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			bookingRequest.Product = bookingRequest.ProductList[0].Code;

			return bookingRequest;
		}

		#endregion

		#endregion

		#region TestPopulateSpecialInstructions

		public void TestPopulateSpecialInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var note1 = consol.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note1.ST_NoteText = "Special instructions";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Special instructions", bookingRequest.SpecialInstructions);
		}

		#endregion

		#region TestPopulateDangerousGoodsHandlingInformation

		public void TestPopulateTestPopulateDangerousGoodsHandlingInformation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var note1 = consol.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			note1.ST_NoteText = "Dangerous Goods instructions";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Dangerous Goods instructions", bookingRequest.DangerousGoodsHandlingInformation);
		}

		#endregion

		#region TestPopulateGoodsHandlingInstructions

		public void TestGoodsHandlingInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var note1 = consol.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note1.ST_NoteText = "Handling instructions";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("Handling instructions", bookingRequest.GoodsHandlingInstructions);
		}

		#endregion

		#region TestPopulateCarrierBookingReference

		public void TestPopulateCarrierBookingReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			consol.JK_BookingReference = "CBkRef1234";

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertEquals("CBkRef1234", bookingRequest.CarrierBookingReference);
		}

		#endregion

		#region TestPopulateTemperature

		public void TestPopulateTemperature()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			Assert("RequiresTemperatureControl", bookingRequest.RequiresTemperatureControl);
			AssertEquals("TemperatureMinimum.Value", 15m, bookingRequest.TemperatureMinimum.Value);
			AssertEquals("TemperatureMinimum.Unit.Code", "C", bookingRequest.TemperatureMinimum.Unit.Code);
			AssertEquals("TemperatureMaximum.Value", 25m, bookingRequest.TemperatureMaximum.Value);
			AssertEquals("TemperatureMaximum.Unit.Code", "C", bookingRequest.TemperatureMaximum.Unit.Code);
		}

		#endregion

		#region Validations

		#region Header Validations

		public void TestValidateAirWaybillNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);
			consol.JK_MasterBillNum = string.Empty;

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertHasMessageError(bookingRequest.MasterAirWaybillNumberInfo, "MAWB is required");

			consol.JK_MasterBillNum = "123-1234567A";

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNoMessageError(bookingRequest.MasterAirWaybillNumberInfo, "MAWB is required");
			AssertHasMessageError(bookingRequest.MasterAirWaybillNumberInfo, "The MAWB number is invalid.");

			consol.JK_MasterBillNum = "123-12345678";

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNoMessageError(bookingRequest.MasterAirWaybillNumberInfo, "The MAWB number is invalid.");

			consol.JK_MasterBillNum = "12312345678";

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNoMessageError(bookingRequest.MasterAirWaybillNumberInfo, "The MAWB number is invalid.");
		}

		public void TestValidateCASS()
		{
			var consol = Factory.New<ForwardingConsol>();
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			bookingRequest.CASS = ZString.Empty;
			AssertHasMessageError(bookingRequest.CASSInfo, "IATA CASS number is required");

			bookingRequest.CASS = "1234567/0000";
			AssertNoMessageError(bookingRequest.CASSInfo, "IATA CASS number is required");
		}

		public void TestValidateAirports()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertUNLOCOIATAValidation((Unloco)bookingRequest.OriginAirport, "Origin airport");
			AssertUNLOCOIATAValidation((Unloco)bookingRequest.DestinationAirport, "Destination airport");
		}

		public void TestValidateAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertMessageErrorIfEmpty(bookingRequest.AgentInfo, "Agent is required");
			AssertMaximumLengthValidation(bookingRequest.AgentInfo, 200, "Agent must not exceed 200 characters.");
			AssertAsciiCharactersValidation("Agent", bookingRequest.AgentInfo);
		}

		public void TestValidateCarrier()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertNoMessageError(((CodeDescription)bookingRequest.Carrier).CodeInfo, "Carrier is required");

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertHasMessageError(((CodeDescription)bookingRequest.Carrier).CodeInfo, "Carrier is required");
		}

		#endregion

		#region Goods Details Validations

		public void TestValidateTotalPieces()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertMaximumLengthValidation(bookingRequest.TotalPiecesInfo, 4, "Total Pieces must not exceed the maximum 9999.");

			bookingRequest.TotalPieces = 0;
			AssertHasMessageError(bookingRequest.TotalPiecesInfo, "Total Pieces is required.");

			bookingRequest.TotalPieces = 6;
			AssertNoMessageError(bookingRequest.TotalPiecesInfo, "Total Pieces is required.");
		}

		public void TestValidateTotalWeight()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertMaximumLengthValidation(((Measurement)bookingRequest.TotalWeight).ValueInfo, 7, "Total Weight must not exceed the maximum length 7.");
			AssertMaximumLengthValidation(((Measurement)bookingRequest.TotalVolume).ValueInfo, 9, "Total Volume must not exceed the maximum length 9.");

			bookingRequest.TotalWeight.Value = 0;
			AssertHasMessageError(((Measurement)bookingRequest.TotalWeight).ValueInfo, "Total Weight is required.");

			bookingRequest.TotalWeight.Value = 6.3m;
			AssertNoMessageError(((Measurement)bookingRequest.TotalWeight).ValueInfo, "Total Weight is required.");
		}

		public void TestValidateGoodsDescription()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertMessageErrorIfEmpty(bookingRequest.GoodsDescriptionInfo, "Goods description must be provided.");
			AssertMaximumLengthValidation(bookingRequest.GoodsDescriptionInfo, 15, "Goods description must not exceed 15 characters.");
			AssertAsciiCharactersValidation("Goods Description", bookingRequest.GoodsDescriptionInfo);
		}

		#endregion

		#region Product Validation

		public void TestValidateProduct_ForAirlineNotSupportingProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "074");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertNull(bookingRequest.ProductList);
			}
		}

		public void TestValidateProduct_ForAirlineSupportingProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "607");

				var shipment1 = consol.Shipments.AddNew();
				PopulateShipment(shipment1, "081001", 12, 25, "12345678901234567890");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();

				AssertMessageErrorIfEmpty(bookingRequest.ProductInfo, "Product must be provided");

				var errorMessage = "Product should be selected from the list";
				bookingRequest.Product = "Magic Mashroom";
				AssertHasMessageError(bookingRequest.ProductInfo, errorMessage);

				bookingRequest.Product = bookingRequest.ProductList[0].Code;
				AssertNoMessageError(bookingRequest.ProductInfo, errorMessage);

				bookingRequest.Product = ZString.Empty;
				AssertHasMessageError(bookingRequest.ProductInfo, "Product must be provided");

				PopulateConsol(consol, "074");

				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				bookingRequest.Product = ZString.Empty;
				AssertNoMessageError(bookingRequest.ProductInfo, "Product must be provided");
			}
		}

		#endregion

		#region Commodity Code Validation

		public void TestValidateCommodityCode()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var consol = Factory.New<ForwardingConsol>();
				PopulateConsol(consol, "618");

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				AssertMaximumLengthValidation(bookingRequest.CommodityInfo, 7, "Commodity must not exceed 7 characters.");
				AssertAsciiCharactersValidation("Commodity", bookingRequest.CommodityInfo);

				bookingRequest.Commodity = ZString.Empty;
				AssertHasMessageError("Singapore Airline CommodityCode option is Required", bookingRequest.CommodityInfo, "Commodity Code is required.");

				bookingRequest.Commodity = "Device";
				AssertNoMessageError("Singapore Airline CommodityCode option is Required", bookingRequest.CommodityInfo, "Commodity Code is required.");

				PopulateConsol(consol, "607");
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				bookingRequest.Commodity = ZString.Empty;
				AssertNoMessageError("Etihad Airways CommodityCode option is Optional", bookingRequest.CommodityInfo, "Commodity Code is required.");

				PopulateConsol(consol, "057");
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				bookingRequest.Commodity = ZString.Empty;
				AssertNoMessageError("Air France CommodityCode option has not defined", bookingRequest.CommodityInfo, "Commodity Code is required.");
			}
		}

		public void TestValidateCommodityCode_ShouldBeFromTheList_WhenListHasBeenProvided()
		{
			PopulateProductCodeCommodityCodePivot("020");

			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "020");

			var airTransports = consol.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				.ToArray();

			Assert(airTransports.Length > 0);

			airTransports.First().JW_RL_NKLoadPort = ZString.Empty;
			airTransports.Last().JW_RL_NKDiscPort = ZString.Empty;

			var bookingRequestOverride =
@"<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
	<Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
	<Property Name=""Product"">
		<Value>Product 2</Value>
	</Property>
</Entity>";
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = consol.TablePrefix;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.AirBookingRequest;
			documentData.JDD_OverriddenData = bookingRequestOverride;
			documentData.JDD_ParentID = consol.PK;

			Factory.Save();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertEquals("", bookingRequest.Commodity);
			AssertEquals(2, bookingRequest.CommodityCollection.Count);
			AssertEquals("CO3", bookingRequest.CommodityCollection[0].Code);
			AssertEquals("CO4", bookingRequest.CommodityCollection[1].Code);
			AssertHasMessageError("Commodity should be selected from the list", bookingRequest.CommodityInfo, "Commodity should be selected from the list");

			bookingRequest.Commodity = "4924";
			AssertHasMessageError("Commodity should be selected from the list", bookingRequest.CommodityInfo, "Commodity should be selected from the list");

			bookingRequest.Commodity = "CO3";
			AssertNoMessageError("Commodity should be selected from the list", bookingRequest.CommodityInfo, "Commodity should be selected from the list");

			bookingRequest.CommodityCollection = null;
			AssertNoExceptionThrown(() => bookingRequest.Commodity = "CO4");
			AssertNoMessageError("Commodity should be selected from the list", bookingRequest.CommodityInfo, "Commodity should be selected from the list");
		}

		#endregion

		#region Flight Validations

		public void TestValidateFlightNumber()
		{
			var formatMessageError = "Flight Number must be entered and have the following format MMN(N)(N)(N)(A)";
			var consol = Factory.New<ForwardingConsol>();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, "At least one Flight Detail is required.");

			PopulateConsol(consol);

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, "At least one Flight Detail is required.");
			Assert(flightDetail.FlightNumber.IsEmpty);
			AssertMaximumLengthValidation(flightDetail.FlightNumberInfo, 15, "Flight Number must not exceed 15 characters.");
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			flightDetail.FlightNumber = "A$123Q";
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			flightDetail.FlightNumber = "QF123";
			AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			flightDetail.FlightNumber = "Q1123A";
			AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
		}

		public void TestValidateFlightNumber_HasAnError()
		{
			var formatMessageError = "Flight Number must be entered and have the following format MMN(N)(N)(N)(A)";
			var consol = Factory.New<ForwardingConsol>();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			PopulateConsol(consol);
			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();
			Assert(flightDetail.FlightNumber.IsEmpty);
			flightDetail.FlightNumber = "A1";
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			flightDetail.FlightNumber = "19";
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			flightDetail.FlightNumber = "91";
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			var flightNumberIncorrectPattern = "A19%";
			var incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
			foreach (var currentFlightNumber in incorrectFlightNumbers)
			{
				flightDetail.FlightNumber = currentFlightNumber;
				AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);
			}

			flightNumberIncorrectPattern = "123";
			incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
			foreach (var currentFlightNumber in incorrectFlightNumbers)
			{
				flightDetail.FlightNumber = currentFlightNumber;
				AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
			}
		}

		public void TestValidateFlightNumber_HasNoError()
		{
			var formatMessageError = "Flight Number must be entered and have the following format MMN(N)(N)(N)(A)";
			var consol = Factory.New<ForwardingConsol>();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertHasMessageError(bookingRequest.ErrorPlaceHolderInfo, "At least one Flight Detail is required.");

			PopulateConsol(consol);

			bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			AssertNoMessageError(bookingRequest.ErrorPlaceHolderInfo, "At least one Flight Detail is required.");
			Assert(flightDetail.FlightNumber.IsEmpty);
			AssertMaximumLengthValidation(flightDetail.FlightNumberInfo, 15, "Flight Number must not exceed 15 characters.");
			AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);

			var flightNumberIncorrectPattern = "1234";
			var incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
			foreach (var currentFlightNumber in incorrectFlightNumbers)
			{
				flightDetail.FlightNumber = currentFlightNumber;
				AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
			}

			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				flightNumberIncorrectPattern = "123";
				incorrectFlightNumbers =
					Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
				foreach (var currentFlightNumber in incorrectFlightNumbers)
				{
					flightDetail.FlightNumber = startChr + currentFlightNumber;
					AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
				}
			}

			flightNumberIncorrectPattern = "123";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			flightNumberIncorrectPattern = "1234";
			for (char chr = 'A'; chr <= 'Z'; chr++)
			{
				incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
				foreach (var currentFlightNumber in incorrectFlightNumbers)
				{
					flightDetail.FlightNumber = chr + currentFlightNumber;
					AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
				}
			}

			flightNumberIncorrectPattern = "1234";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			flightNumberIncorrectPattern = "12345";
			for (char chr = 'A'; chr <= 'Z'; chr++)
			{
				incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
				foreach (var currentFlightNumber in incorrectFlightNumbers)
				{
					flightDetail.FlightNumber = chr + currentFlightNumber;
					AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
				}
			}

			flightNumberIncorrectPattern = "12345";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					flightDetail.FlightNumber = "A" + startChr + "1" + endChr;
					AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
				}
			}

			flightNumberIncorrectPattern = "12";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = "A" + startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			flightNumberIncorrectPattern = "123";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = "A" + startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			flightNumberIncorrectPattern = "1234";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0,
						flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = "A" + startChr + currentFlightNumber + endChr;
						AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					flightDetail.FlightNumber = startChr + endChr + "1";
					AssertNoMessageError(flightDetail.FlightNumberInfo, formatMessageError);
				}
			}

			flightNumberIncorrectPattern = "12345";
			for (char startChr = 'A'; startChr <= 'Z'; startChr++)
			{
				for (char endChr = 'A'; endChr <= 'Z'; endChr++)
				{
					incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
					foreach (var currentFlightNumber in incorrectFlightNumbers)
					{
						flightDetail.FlightNumber = "A" + startChr + currentFlightNumber + endChr;
						AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);
					}
				}
			}

			var signs = new string[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "+", "=", "/", "\\" };
			flightNumberIncorrectPattern = "1234";
			foreach (var sign in signs)
			{
				for (char startChr = 'A'; startChr <= 'Z'; startChr++)
				{
					for (char endChr = 'A'; endChr <= 'Z'; endChr++)
					{
						incorrectFlightNumbers = Permutate(flightNumberIncorrectPattern, 0, flightNumberIncorrectPattern.Length - 1);
						foreach (var currentFlightNumber in incorrectFlightNumbers)
						{
							flightDetail.FlightNumber = "A" + startChr + currentFlightNumber + endChr + sign;
							AssertHasMessageError(flightDetail.FlightNumberInfo, formatMessageError);
						}
					}
				}
			}
		}

		IEnumerable<string> Permutate(string str, int left, int right)
		{
			var result = new List<string>();
			if (left == right)
			{
				result.Add(str);
			}
			else
			{
				for (var i = left; i <= right; i++)
				{
					str = Swap(str, left, i);
					result.AddRange(Permutate(str, left + 1, right));
					str = Swap(str, left, i);
				}
			}
			return result;
		}
		string Swap(string data, int startIdx, int endIdx)
		{
			char[] charArray = data.ToCharArray();
			var temp = charArray[startIdx];
			charArray[startIdx] = charArray[endIdx];
			charArray[endIdx] = temp;
			var result = new string(charArray);
			return result;
		}

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestValidateFlightNumber_OnlineScheduleStatusWarning()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;

			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Core.Constants.FlightScheduleStatus.Matched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var warning = @"This Air Routing Leg does not match any of the Global Flight Schedule flights. This flight may not be tracked correctly. You may want to check Global Flight Schedules for correct flight details by using ""Import Global Flights"" to search and import most appropriate flight.";
				var consol = Factory.New<ForwardingConsol>();

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.TryMatchAgainstOnlineFlights();

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				var flightDetail = bookingRequest.FlightDetails.First();

				AssertHasWarning(flightDetail.FlightNumberInfo, warning);

				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				transport.TryMatchAgainstOnlineFlights();
				bookingRequest = new AirBookingRequestBuilder(consol).Build();
				flightDetail = bookingRequest.FlightDetails.First();

				AssertEquals("Matching enabled: Transport should be matched", Core.Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
				AssertNoWarning(flightDetail.FlightNumberInfo, warning);
			}
		}

		public void TestValidateFlightDetails_Ports()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			AssertUNLOCOIATAValidation(flightDetail.PortOfLoading, "Departure airport");
			AssertUNLOCOIATAValidation(flightDetail.PortOfDischarge, "Arrival airport");
		}

		public void TestValidateFlightDetails_ETD()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			flightDetail.ETD = ZDateTime.Empty;
			AssertHasMessageError(flightDetail.ETDInfo, "Estimated date of departure is mandatory");

			flightDetail.ETD = ZDateTime.Now;
			AssertNoMessageError(flightDetail.ETDInfo, "Estimated date of departure is mandatory");
		}

		public void TestValidateFlightDetails_AllotmentId()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			AssertMaximumLengthValidation(flightDetail.AllotmentIdInfo, 14, "Allotment ID must not exceed 14 characters.");

			flightDetail.AllotmentId = "@1135!";
			AssertHasMessageError(flightDetail.AllotmentIdInfo, "Allotment ID must be alphanumeric characters.");

			flightDetail.AllotmentId = "Ball";
			AssertNoMessageError(flightDetail.AllotmentIdInfo, "Allotment ID must be alphanumeric characters.");
		}

		#endregion

		#region ULD Details Validations

		public void TestValidateULDDetails_Type()
		{
			var formatMessageError = "Container type must be entered and have the following format AMM.";
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var uld = bookingRequest.Ulds.First();

			uld.Type.Code = string.Empty;
			AssertHasMessageError(uld.Type.CodeInfo, formatMessageError);

			uld.Type.Code = "A1";
			AssertHasMessageError(uld.Type.CodeInfo, formatMessageError);

			uld.Type.Code = "A1@";
			AssertHasMessageError(uld.Type.CodeInfo, formatMessageError);

			uld.Type.Code = "123";
			AssertHasMessageError(uld.Type.CodeInfo, formatMessageError);

			uld.Type.Code = "A23";
			AssertNoMessageError(uld.Type.CodeInfo, formatMessageError);
		}

		public void TestValidateULDDetails_Number_Volume()
		{
			const string numWarning = "This is not a valid ULD number, expected format should be similar to 'AKE1234QF', 'PMC12345CX'.";
			const string volWarning = "ULD volume is mandatory, please review the container tab and enter the actual dimensions within the Measures sub tab.";
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			TestCase(numWarning, "A1");
			TestCase(numWarning, "A11");
			TestCase(numWarning, "A11!");
			TestCase(numWarning, "A12345");
			TestCase(volWarning, "AKE1234QF", 0);
			TestCase(volWarning, "AKE1234QF", 0, 0, 1);
			TestCase(volWarning, "AKE1234QF", 0, 1, 0);
			TestCase(volWarning, "AKE1234QF", 1, 1, 0);
			TestCase(numWarning + "\r\n" + volWarning, "A1", 0);
			TestCase(null, "AKE1234QF");
			TestCase(null, "PMC12345CX");

			void TestCase(string expectedWarning, string containerNumber, decimal height = 1, decimal width = 1, decimal length = 1)
			{
				var container = consol.Containers[0];
				container.JC_ContainerNum = containerNumber;
				container.JC_TotalHeight = height;
				container.JC_TotalWidth = width;
				container.JC_TotalLength = length;

				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				var uld = bookingRequest.Ulds.First();
				if (string.IsNullOrEmpty(expectedWarning))
				{
					AssertNoMessageError(uld.NumberInfo, numWarning);
					AssertNoMessageError(uld.NumberInfo, volWarning);
				}
				else
				{
					AssertHasMessageError(uld.NumberInfo, expectedWarning);
				}
			}
		}

		public void TestValidateULDDetails_Weights()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var uld = bookingRequest.Ulds.First();

			AssertMaximumLengthValidation(uld.GrossWeight.ValueInfo, 7, "Gross Weight must not exceed the maximum length 7.");
			AssertMaximumLengthValidation(uld.TareWeight.ValueInfo, 7, "Tare Weight must not exceed the maximum length 7.");
			AssertMaximumLengthValidation(uld.GoodsWeight.ValueInfo, 7, "Goods Weight must not exceed the maximum length 7.");
		}

		#endregion

		#region Dimensions Validations

		public void TestValidateDimensions()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");
			consol.Containers[0].PackLines.RemoveAll();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var dimension = bookingRequest.Dimensions.First();

			AssertMaximumLengthValidation(dimension.Weight.ValueInfo, 7, "Total Weight must not exceed the maximum length 7.");
			AssertMaximumLengthValidation(dimension.QuantityInfo, 4, "Pieces must not exceed the maximum 9999.");
		}

		public void TestEmptyQuantityValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081002", 12, 25, "goods");
			consol.Containers[0].PackLines.RemoveAll();

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var dimension = bookingRequest.Dimensions.First();

			dimension.Quantity = 0;
			AssertHasMessageError(dimension.QuantityInfo, "The value for the number of loose pieces cannot be zero.");

			dimension.Quantity = 1;
			AssertNoMessageError(dimension.QuantityInfo, "The value for the number of loose pieces cannot be zero.");
		}

		#endregion

		#region Additional Information Validations

		public void TestValidateAdditionalInformation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertAsciiCharactersValidation("Special instructions", bookingRequest.SpecialInstructionsInfo);
			AssertMaximumLengthValidation(bookingRequest.SpecialInstructionsInfo, 65, "Special Instructions must not exceed 65 characters.");

			AssertAsciiCharactersValidation("Dangerous Goods Handling information", bookingRequest.DangerousGoodsHandlingInformationInfo);
			AssertMaximumLengthValidation(bookingRequest.DangerousGoodsHandlingInformationInfo, 65, "Dangerous Goods Handling Information must not exceed 65 characters.");

			AssertAsciiCharactersValidation("Goods Handling Instructions", bookingRequest.GoodsHandlingInstructionsInfo);
			AssertMaximumLengthValidation(bookingRequest.GoodsHandlingInstructionsInfo, 65, "Goods Handling Instructions must not exceed 65 characters.");

			consol.JK_MasterBillNum = "176-98757411";
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			AssertAsciiCharactersValidation("Special instructions", bookingRequest.SpecialInstructionsInfo);
			AssertMaximumLengthValidation(bookingRequest.SpecialInstructionsInfo, 200, "Special Instructions must not exceed 200 characters.");

			AssertAsciiCharactersValidation("Dangerous Goods Handling information", bookingRequest.DangerousGoodsHandlingInformationInfo);
			AssertMaximumLengthValidation(bookingRequest.DangerousGoodsHandlingInformationInfo, 200, "Dangerous Goods Handling Information must not exceed 200 characters.");
		}

		#endregion

		#endregion

		#region TestFlightStatusDoesNotRegisterChanges

		public void TestFlightStatusDoesNotRegisterChanges()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			var dynamicBookingRequest = bookingRequest.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var dynamicFlights = dynamicBookingRequest.GetDynamicProperty(nameof(bookingRequest.FlightDetails)) as IDynamicDataCollection;

			Assert("dynamicBookingRequest doesn't have changes", !dynamicBookingRequest.HasChanges);
			Assert("dynamicFlights doesn't have changes", !dynamicFlights.HasChanges);

			AssertEquals("prerequisite; dynamicFlights has 3 elements", 3, dynamicFlights.Count());

			foreach (var dynamicFlight in dynamicFlights)
			{
				var dynamicStatus = dynamicFlight.GetDynamicProperty(nameof(FlightDetail.Status));
				var dynamicCode = dynamicStatus.GetDynamicProperty(nameof(ICodeDescription.Code));
				var dynamicDescription = dynamicStatus.GetDynamicProperty(nameof(ICodeDescription.Description));

				AssertEquals("Status.Code Value", "PLN", dynamicCode.Value);
				AssertEquals("Status.Description Value", "Planned", dynamicDescription.Value);
			}

			var flight1 = bookingRequest.FlightDetails.First();
			flight1.Status.Code = Core.Constants.TransportStatus.Queued;

			Assert("dynamicBookingRequest doesn't have changes", !dynamicBookingRequest.HasChanges);
		}

		#endregion

		public void TestRoundingOfWeightAndVolume()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";
			consol.JK_TotalShipmentActWeightCheck = 7.123;
			consol.WeightVerificationUnit = Core.Constants.Weight.Grams;
			consol.JK_TotalShipmentActVolumeCheck = 3.234;
			consol.VolumeVerificationUnit = Core.Constants.Volume.Litre;

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				CombineAssertions("No containers or lines, valid pre-allocation value and unit", () =>
				{
					AssertEquals(7.1m, bookingRequest.TotalWeight.Value);
					AssertEquals(3.234m, bookingRequest.TotalVolume.Value);
				});
			}

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingRequest = new AirBookingRequestBuilder(consol).Build();
				CombineAssertions("No containers or lines, valid pre-allocation value and unit", () =>
				{
					AssertEquals(7.123m, bookingRequest.TotalWeight.Value);
					AssertEquals(3.234m, bookingRequest.TotalVolume.Value);
				});
			}
		}

		public void TestFlightDetails_ETD_ETA()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "BRSAO";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C00001001";

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "BRSAO";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2020, 7, 25, 12, 13, 14);
			transport.JW_ETA = new ZDateTime(2020, 7, 26, 12, 13, 14);

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			var flightDetail = bookingRequest.FlightDetails.First();

			AssertEquals("ETD", new ZDateTime(2020, 7, 25, 12, 13, 14), flightDetail.ETD);
			AssertEquals("ETA", new ZDateTime(2020, 7, 26, 12, 13, 14), flightDetail.ETA);
		}

		public void TestFlightDetailsNote()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;

			consol.Transports.Cast<Freight.Business.Transport>().ForEach(t => t.JW_Status = TransportStatus.Confirmed);
			var bookingRequest = new AirBookingRequestBuilder(consol).Build();

			Assert("Pre-condition: No FlightDetail has PLN status.", bookingRequest.FlightDetails.All(f => f.Status.Code != TransportStatus.Planned));
			AssertEquals("FlightDetailsNote should be empty.", "", bookingRequest.FlightDetailsNote);

			consol.Transports[0].JW_Status = TransportStatus.Planned;
			bookingRequest = new AirBookingRequestBuilder(consol).Build();

			Assert("Pre-condition: At least 1 FlightDetail has PLN status.", bookingRequest.FlightDetails.Any(f => f.Status.Code == TransportStatus.Planned));
			AssertEquals("FlightDetailsNote should have note message.", "(Note: For ‘Planned’ status, flight details might not be sent if the airline returns offers based on available capacity at the time of query.)", bookingRequest.FlightDetailsNote);
		}

		public void TestEBookingRequestContainsSubShipmentPackLinesWhenMasterShipmentIsBCNType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 6;
			packingLine1.JL_ActualWeight = 600m;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 8;
			packingLine2.JL_ActualWeight = 800m;

			var bookingRequest = new AirBookingRequestBuilder(consol).Build();
			AssertNotNull(bookingRequest);
			AssertEquals(2, bookingRequest.Dimensions.Count);

			var pieces1 = bookingRequest.Dimensions.First();
			var pieces2 = bookingRequest.Dimensions.ElementAt(1);

			AssertEquals(packingLine1.JL_PackageCount, pieces1.Quantity);
			AssertEquals(packingLine1.JL_ActualWeight, pieces1.Weight.Value);

			AssertEquals(packingLine2.JL_PackageCount, pieces2.Quantity);
			AssertEquals(packingLine2.JL_ActualWeight, pieces2.Weight.Value);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			AirBookingCarrierConfigurationManager.ClearSupportedAirlinesApplicationCache();
		}

		void PopulateConsol(ForwardingConsol consol, string airlinePrefix = "057")
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = $"{airlinePrefix}-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_UniqueConsignRef = "C00001004";
			consol.SecurityStatusCode = SecurityJobConsolAWBSpecialHandling.NotSecured;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BRSAO";
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "CLSCL";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "CLSCL";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;

			var contractNumber1 = consol.Numbers.AddNew();
			contractNumber1.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			contractNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			contractNumber1.CE_EntryNum = "12345";

			var contractNumber2 = consol.Numbers.AddNew();
			contractNumber2.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			contractNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			contractNumber2.CE_EntryNum = "54321";

			var otherNumber = consol.Numbers.AddNew();
			otherNumber.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;
			otherNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			otherNumber.CE_EntryNum = "666";

			consol.Notes.AddNew(true, "Booking Confirmation Notes", "Carrier remarks");

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = "22P1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AKE12345AU";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = refContainer.PK;
			container.JC_IsNonOperativeReefer = false;
			container.JC_TotalHeight = 1;
			container.JC_TotalWidth = 1;
			container.JC_TotalLength = 1;
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "BRSAO";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "BR";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		void PopulateProductCodeCommodityCodePivot(ZString airlinePrefix, bool onlyPopulateOneProduct = false, bool populateMoreCommodity = false)
		{
			var commodityCode1 = Factory.New<RefAirlineCommodityCode>();
			commodityCode1.RAC_AirlineID = airlinePrefix;
			commodityCode1.RAC_Code = "CO1";
			commodityCode1.RAC_Description = "Commodity 1";
			commodityCode1.RAC_SpecialHandlingCodes = "SH.1";

			var commodityCode2 = Factory.New<RefAirlineCommodityCode>();
			commodityCode2.RAC_AirlineID = airlinePrefix;
			commodityCode2.RAC_Code = "CO2";
			commodityCode2.RAC_Description = "Commodity 2";

			var commodityCode3 = Factory.New<RefAirlineCommodityCode>();
			commodityCode3.RAC_AirlineID = airlinePrefix;
			commodityCode3.RAC_Code = "CO3";
			commodityCode3.RAC_Description = "Commodity 3";
			commodityCode3.RAC_SpecialHandlingCodes = ",  ,SH.2 , CAO, SH.3 ,,";

			var commodityCode4 = Factory.New<RefAirlineCommodityCode>();
			commodityCode4.RAC_AirlineID = airlinePrefix;
			commodityCode4.RAC_Code = "CO4";
			commodityCode4.RAC_Description = "Commodity 4";
			commodityCode4.RAC_SpecialHandlingCodes = "    ";

			var productCode1 = Factory.New<RefAirlineProductCode>();
			productCode1.RAR_AirlineID = airlinePrefix;
			productCode1.RAR_Code = "PR1";
			productCode1.RAR_Description = "Product 2";

			var productCommodityPivot3 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
			productCommodityPivot3.RPC_AirlineID = airlinePrefix;
			productCommodityPivot3.RPC_RAC = commodityCode3.PK;
			productCommodityPivot3.RPC_RAR = productCode1.PK;

			var productCommodityPivot4 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
			productCommodityPivot4.RPC_AirlineID = airlinePrefix;
			productCommodityPivot4.RPC_RAC = commodityCode4.PK;
			productCommodityPivot4.RPC_RAR = productCode1.PK;

			if (!onlyPopulateOneProduct)
			{
				var productCode2 = Factory.New<RefAirlineProductCode>();
				productCode2.RAR_AirlineID = airlinePrefix;
				productCode2.RAR_Code = "PR2";
				productCode2.RAR_Description = "Product 1";

				var productCode3 = Factory.New<RefAirlineProductCode>();
				productCode3.RAR_AirlineID = airlinePrefix;
				productCode3.RAR_Code = "PR3";
				productCode3.RAR_Description = "Product 3";

				var productCommodityPivot1 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
				productCommodityPivot1.RPC_AirlineID = airlinePrefix;
				productCommodityPivot1.RPC_RAC = commodityCode1.PK;
				productCommodityPivot1.RPC_RAR = productCode2.PK;

				var productCommodityPivot2 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
				productCommodityPivot2.RPC_AirlineID = airlinePrefix;
				productCommodityPivot2.RPC_RAC = commodityCode2.PK;
				productCommodityPivot2.RPC_RAR = productCode2.PK;
			}

			if (populateMoreCommodity)
			{
				var productCommodityPivot5 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
				productCommodityPivot5.RPC_AirlineID = airlinePrefix;
				productCommodityPivot5.RPC_RAC = commodityCode1.PK;
				productCommodityPivot5.RPC_RAR = productCode1.PK;

				var productCommodityPivot6 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
				productCommodityPivot6.RPC_AirlineID = airlinePrefix;
				productCommodityPivot6.RPC_RAC = commodityCode2.PK;
				productCommodityPivot6.RPC_RAR = productCode1.PK;
			}

			Factory.Save();
		}

		ForwardingConsol PopulateSpecialHandlingCodes(ForwardingConsol consol, params ZString[] specialHandlingCodes)
		{
			consol.AWBSpecialHandlingItems.RemoveAndDeleteAll();

			foreach (var code in specialHandlingCodes.Distinct())
			{
				var shi = consol.AWBSpecialHandlingItems.AddNew();
				shi.JKH_Code = code;
			}

			return consol;
		}

		#endregion Implementation
	}
}
