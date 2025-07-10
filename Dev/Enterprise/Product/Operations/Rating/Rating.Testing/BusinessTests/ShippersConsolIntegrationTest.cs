using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class ShippersConsolIntegrationTest : BaseRatingIntegrationTest
	{
		[TestDate(2023, 03, 15)]
		public void TestAutoRate_LeadShipment()
		{
			var leadShipment = CreateShipment(shipmentType: ContainerModes.ShippersConsol, weight: 200);
			var subShipment = CreateShipment(shipmentType: ShipmentTypes.StandardHouse, weight: 800);
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			AddRateEntry(ClientRate, RatingConstants.RateCategory.AIR).AddFlatRateLine("BAF", 100, description: "BAF");
			AddRateEntry(ClientRate, RatingConstants.RateCategory.ORG).AddFlatRateLine("ODOC", 200, description: "ODOC (SCN)", unitFactor: UnitFactorList.Codes.SCN);
			AddRateEntry(ClientRate, RatingConstants.RateCategory.DST).AddFlatRateLine("DDOC", 300, description: "DDOC");

			Factory.Save();

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.Master,
				leadShipment,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "BAF", JR_OSSellAmt = 100m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ODOC (SCN)", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
				},
				expectedErrors: Array.Empty<string>(),
				message: "ORG/FRT charges should be rated from LeadShipment and DST charges should be rated from LeadShipment and SubShipment."
			);

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipment,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "BAF", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ODOC (SCN)", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
				},
				expectedErrors: Array.Empty<string>(),
				message: "Freight charge (BAF) apportioned, Origin charge (ODOC) isn't apportion as it has SCN - No Apportion unit factor. Destination charges (DDOC) in SCN mode never apportion."
			);

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipment,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "BAF", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ODOC (SCN)", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
				},
				expectedErrors: Array.Empty<string>(),
				message: "Freight charge (BAF) apportioned, Origin charge (ODOC) isn't apportion as it has SCN - No Apportion unit factor. Destination charges (DDOC) in SCN mode never apportion."
			);
		}

		[TestDate(2023, 03, 15)]
		public void TestAutoRate_SubShipment()
		{
			var leadShipment = CreateShipment(shipmentType: ContainerModes.ShippersConsol, weight: 200);
			var subShipment = CreateShipment(shipmentType: ShipmentTypes.StandardHouse, weight: 800);
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			AddRateEntry(ClientRate, RatingConstants.RateCategory.AIR).AddFlatRateLine("BAF", 100, description: "BAF");
			AddRateEntry(ClientRate, RatingConstants.RateCategory.ORG).AddFlatRateLine("ODOC", 200, description: "ODOC (SCN)", unitFactor: UnitFactorList.Codes.SCN);
			AddRateEntry(ClientRate, RatingConstants.RateCategory.DST).AddFlatRateLine("DDOC", 300, description: "DDOC");

			Factory.Save();

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.Master,
				subShipment,
				expectedCharges: Array.Empty<AssertionCharge>(),
				expectedErrors: new[] { "Error This Shipment is part of a Shipper's Consol. Generally, you should Autorate and invoice from the Shipper's Consol Master - Shipment EBM22Q33TU475BXH3P60" },
				message: "Should not be able to autorate MAS from Sub-Shipment"
			);

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.Apportion,
				subShipment,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "BAF", JR_OSSellAmt = 80m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ODOC (SCN)", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
				},
				expectedErrors: Array.Empty<string>(),
				message: "Freight charge (BAF) apportioned, Origin charge (ODOC) isn't apportion as it has SCN - No Apportion unit factor. Destination charges (DDOC) in SCN mode never apportion."
			);

			AssertAutoRate
			(
				invoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				subShipment,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "BAF", JR_OSSellAmt = 80m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ODOC (SCN)", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DDOC", JR_OSSellAmt = 300m },
				},
				expectedErrors: Array.Empty<string>(),
				message: "Freight charge (BAF) apportioned, Origin charge (ODOC) isn't apportion as it has SCN - No Apportion unit factor. Destination charges (DDOC) in SCN mode never apportion."
			);
		}

		public void TestAutorateSCN_RateOrigin()
		{
			ClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "SCN", "DEBER", "", "FRT", 5000m);

			var leadShipment = CreateShipment(shipmentType: ContainerModes.ShippersConsol, weight: 200);
			leadShipment.JS_RL_NKFreightRateOrigin = "DEBER";
			var subShipment = CreateShipment(shipmentType: ShipmentTypes.StandardHouse, weight: 800);
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 5000m }
			};

			AutorateAndAssert("Should have a FRT charge via RateOrigin", expectedCharges, leadShipment, Consignee);
		}

		public void TestAutorateSCN_RateDestination()
		{
			ClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "SCN", "", "AUMEL", "FRT", 5000m);

			var leadShipment = CreateShipment(shipmentType: ContainerModes.ShippersConsol, weight: 200);
			leadShipment.JS_RL_NKFreightRateDestination = "AUMEL";
			var subShipment = CreateShipment(shipmentType: ShipmentTypes.StandardHouse, weight: 800);
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 5000m }
			};

			AutorateAndAssert("Should have a FRT charge via RateDestination", expectedCharges, leadShipment, Consignee);
		}

		#region Implementation

		protected ForwardingShipment CreateShipment(string shipmentType, decimal weight)
		{
			var shipment = CreateForwardingShipment(Factory, TransportModes.Air, ContainerModes.ShippersConsol, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", weight);
			shipment.JS_ShipmentType = shipmentType;
			shipment.JS_HouseBillIssueDate = ZDateTime.Today.AddDays(5); // avoid DST charges being filtered because NewWithValidTestData set JS_HouseBillIssueDate with old date
			return shipment;
		}

		RateEntry AddRateEntry(RatingHeader ratingHeader, string category) => ratingHeader.AddRateEntry(category, mode: RateMode.SCN, "AUSYD", "USLAX", removeLines: true);

		ClientRate ClientRate => clientRate ?? (clientRate = Helper.NewClientRate(Consignee));
		ClientRate clientRate;

		void AssertAutoRate(string invoicingStyle, ForwardingShipment shipment, AssertionCharge[] expectedCharges, string[] expectedErrors, string message)
		{
			Consignee.CompanyData.OB_ARShippersConsolInvoicingStyle = invoicingStyle;

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false, autorateRevenue: true, expectedErrors: expectedErrors);
			}
		}

		#endregion
	}
}
