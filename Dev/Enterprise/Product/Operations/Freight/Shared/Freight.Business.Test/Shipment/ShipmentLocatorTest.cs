using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentLocatorTest : TestCaseWithFactory
	{
		CommonShipment CreateShipment(ZString shipmentNumber, ZString houseBill, ZDateTime etd, string origin, string destination)
		{
			var shipment = CreateShipment(shipmentNumber, houseBill);
			shipment.JS_E_DEP = etd;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			return shipment;
		}

		CommonShipment CreateShipment(string shipmentNumber, string houseBill)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_HouseBill = houseBill;
			return shipment;
		}

		public void TestFind()
		{
			CommonShipment shipment1 = CreateShipment("number1", "houseBill1");
			CommonShipment shipment2 = CreateShipment("number2", "");
			CommonShipment shipment3 = CreateShipment("", "houseBill3");
			CommonShipment shipment4 = CreateShipment("", "houseBill3", new ZDateTime(2010, 1, 10), "AUSYD", "USLAX");
			CommonShipment shipment5 = CreateShipment("", "houseBill3", new ZDateTime(2010, 2, 10), "AUBNE", "USNYC");
			CommonShipment shipment6 = CreateShipment("", "houseBill3", new ZDateTime(2010, 3, 10), "CNSHA", "AUMEL");

			Action<string, string, CommonShipment, CommonShipment, CommonShipment> assertFind = (shipmentNumber, houseBill, matchedByNumber, matchedByNumberThenHouseBill, matchedByHouseBill) =>
			{
				var locator = new ShipmentLocator<CommonShipment>(Factory, null);

				SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
				AssertEquals(matchedByNumber, locator.Find(shipmentNumber, houseBill, null, ZDateTime.Empty));

				SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
				AssertEquals(matchedByNumberThenHouseBill, locator.Find(shipmentNumber, houseBill, null, ZDateTime.Empty));

				SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
				AssertEquals(matchedByHouseBill, locator.Find(shipmentNumber, houseBill, null, ZDateTime.Empty));
			};

			assertFind("", "", null, null, null);
			assertFind("random", "numbers", null, null, null);

			assertFind("", "houseBill1", null, shipment1, shipment1);
			assertFind("number2", "houseBill3", shipment2, shipment2, shipment3);
			assertFind("number2", "", shipment2, shipment2, null);

			var shipmentLocator = new ShipmentLocator<CommonShipment>(Factory, null);
			AssertEquals(shipment6, shipmentLocator.Find("", "houseBill3", null, new ZDateTime(2010, 1, 10), "CNSHA", "USLAX"));
			AssertEquals(shipment5, shipmentLocator.Find("", "houseBill3", null, new ZDateTime(2010, 1, 10), "UAIEV", "USNYC"));
			AssertEquals(shipment4, shipmentLocator.Find("", "houseBill3", null, new ZDateTime(2010, 1, 10), "USLAX", "UAIEV"));
			AssertEquals(shipment3, shipmentLocator.Find("", "houseBill3", null, new ZDateTime(2010, 4, 10), "USLAX", "UAIEV"));
		}

		public void TestFind_Consol()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_HouseBill = "hello";

			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "hello";

			CommonConsol anotherConsol = Factory.New<CommonConsol>();

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
			AssertEquals(shipment2, new ShipmentLocator<CommonShipment>(Factory, null, consol).Find("", "hello", null, ZDateTime.Empty));
			AssertNull(new ShipmentLocator<CommonShipment>(Factory, null, anotherConsol).Find("", "hello", null, ZDateTime.Empty));
		}

		public void TestFind_IncludeInactive()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "number";
			shipment.JS_IsCancelled = true;

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			var locator = new ShipmentLocator<CommonShipment>(Factory, null);
			AssertEquals(shipment, locator.Find("number", "", null, ZDateTime.Empty));

			locator = new ShipmentLocator<CommonShipment>(Factory, null, null, false);
			AssertNull(locator.Find("number", "", null, ZDateTime.Empty));
		}

		public void TestFind_AdditionalQuery()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "hello";
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsBooking = false;
			shipment.JS_IsShipping = false;

			CommonShipment booking = Factory.New<CommonShipment>();
			booking.JS_HouseBill = "hello";
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsBooking = true;
			booking.JS_IsShipping = false;

			CommonShipment agencyShipment = Factory.New<CommonShipment>();
			agencyShipment.JS_HouseBill = "hello";
			agencyShipment.JS_IsForwardRegistered = false;
			agencyShipment.JS_IsBooking = false;
			agencyShipment.JS_IsShipping = true;

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);

			Action<CommonShipment, ZQuery> assertFind = (expectedShipment, additionalQuery) =>
				{
					CommonShipment foundShipment = new ShipmentLocator<CommonShipment>(Factory, additionalQuery).Find("", "hello", null, ZDateTime.Empty);
					AssertEquals(expectedShipment, foundShipment);
				};

			assertFind(shipment, new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, true));
			assertFind(booking, new ZQuery(JobShipmentSchema.JS_IsBooking, true));
			assertFind(agencyShipment, new ZQuery(JobShipmentSchema.JS_IsShipping, true));
			assertFind(null, new ZQuery(JobShipmentSchema.JS_RL_NKOrigin, "XXX"));
		}

		public void TestFind_OtherAgentReferences()
		{
			string otherAgentReference = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;

			CommonShipment standAloneShipment1 = CreateShipment("number1", "house1");
			CommonShipment standAloneShipment2 = CreateShipment("number2", "house2");
			standAloneShipment2.Numbers.AddNewIfNotExist("XXX", "otherRef2");

			CommonShipment standAloneShipment3 = CreateShipment("number3", "house3");
			standAloneShipment3.Numbers.AddNewIfNotExist(otherAgentReference, "otherRef3");

			CommonShipment consolShipment11 = CreateShipment("number11", "house11");
			consolShipment11.Numbers.AddNewIfNotExist(otherAgentReference, "otherRef11");

			CommonShipment consolShipment12 = CreateShipment("number12", "house12");
			consolShipment12.Numbers.AddNewIfNotExist("XXX", "otherRef12");

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.Shipments.Add(consolShipment11);
			consol1.Shipments.Add(consolShipment12);

			CommonShipment consolShipment21 = CreateShipment("number21", "house21");
			consolShipment21.Numbers.AddNewIfNotExist(otherAgentReference, "otherRef21");

			CommonShipment consolShipment22 = CreateShipment("number22", "house22");
			consolShipment22.Numbers.AddNewIfNotExist("XXX", "otherRef22");

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.Shipments.Add(consolShipment21);
			consol2.Shipments.Add(consolShipment22);

			Factory.Save();

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);

			var locator = new ShipmentLocator<CommonShipment>(Factory, null);
			AssertEquals("Matched by housebill", standAloneShipment2, locator.Find("number1", "house2", new ZString[] { "number3" }, ZDateTime.Empty));
			AssertEquals("Matched by OAG number", standAloneShipment3, locator.Find("number1", "NOMATCH", new ZString[] { "number3" }, ZDateTime.Empty));

			Action<string, CommonShipment> assertRegistryOptionsWillAffectFind = (message, expectedShipment) =>
			{
				AssertEquals(message, expectedShipment, locator.Find("NOMATCH", "NOMATCH", new ZString[] { "number3" }, ZDateTime.Empty));
			};

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
			assertRegistryOptionsWillAffectFind("Not matched by OAG number due to registry setup", null);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
			assertRegistryOptionsWillAffectFind("Not matched by OAG number due to registry setup", null);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
			assertRegistryOptionsWillAffectFind("Matched by OAG number", standAloneShipment3);

			SystemDataRegistry.Instance.AllowMatchingByOtherAgentReferencesOnImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			assertRegistryOptionsWillAffectFind("Not matched when matching is disabled in registry", null);

			SystemDataRegistry.Instance.AllowMatchingByOtherAgentReferencesOnImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			assertRegistryOptionsWillAffectFind("Matched by OAG number", standAloneShipment3);

			AssertEquals("Not matched by HBL or OAG", null, locator.Find("number1", "NOMATCH", new ZString[] { "randomRef" }, ZDateTime.Empty));
			AssertEquals("Matched by shipment number, specified in OAG's of existing shipment", standAloneShipment3, locator.Find("otherRef3", "NOMATCH", new ZString[] { "randomRef" }, ZDateTime.Empty));

			locator = new ShipmentLocator<CommonShipment>(Factory, null, consol1);
			AssertEquals("Matched by housebill", consolShipment12, locator.Find("number11", "house12", new ZString[] { "randomRef" }, ZDateTime.Empty));
			AssertEquals("Matched by OAG number", consolShipment11, locator.Find("number11", "NOMATCH", new ZString[] { "number11" }, ZDateTime.Empty));
			AssertEquals("Matched by OAG number", consolShipment11, locator.Find("NOMATCH", "NOMATCH", new ZString[] { "number11" }, ZDateTime.Empty));

			AssertEquals("Not matched by HBL or OAG", null, locator.Find("number11", "NOMATCH", new ZString[] { "randomRef" }, ZDateTime.Empty));
			AssertEquals("Matched by shipment number, specified in OAG's of existing shipment", consolShipment11, locator.Find("otherRef11", "NOMATCH", new ZString[] { "randomRef" }, ZDateTime.Empty));
		}
	}
}
