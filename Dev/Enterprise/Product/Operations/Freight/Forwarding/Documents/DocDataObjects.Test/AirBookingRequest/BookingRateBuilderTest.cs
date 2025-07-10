using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using ChargeLine = Enterprise.UniversalDataBuss.DataObjects.Accounting.ChargeLine;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class BookingRateBuilderTest : TestCase
	{
		#region TestBuild

		public void TestBuild()
		{
			var shipment = new UShipment();
			shipment.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.ConsolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ConsolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
				{
					new ConsolCostLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ChargeCode = new ChargeCode
						{
							Code = "STANDARD",
							Description = "BOOKABLE"
						},
						SupplierReference = "2847abac-e09e-44b8-a43f-74b51973b008",
						CostOSAmount = 641.38,
						CostOSCurrency = new Currency
						{
							Code = "EUR"
						}
					}
				});

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
				{
					new TransportLeg
					{
						LegOrder = 1,
						PortOfLoading = new UNLOCO
						{
							Code = "CDG"
						},
						PortOfDischarge = new UNLOCO
						{
							Code = "JFK"
						},
						EstimatedDeparture = new ZDateTime(2020, 12, 1),
						EstimatedArrival = new ZDateTime(2020, 12, 2),
						VoyageFlightNo = "DL263",
						BookingStatus = new CodeDescriptionPair
						{
							Code = "PLN"
						},
						LegType = LegType.Other,
						AircraftType = new CodeDescriptionPair
						{
							Code = "333",
							Description = "Airbus A330-300 Passenger"
						}
					}
				});

			shipment.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo
					{
						Key = "RateDescription",
						Value = "Online Confirmation - Booking will be confirmed online",
					},
					new AddInfo
					{
						Key = "Customer",
						Value = "132812",
					},
					new AddInfo
					{
						Key = "Shipment",
						Value = "31635553-eb00-4896-b11b-416e6158f442",
					}
				});

			shipment.SetCustomizedFieldCollection(() => new List<CustomizedField>
				{
					new CustomizedField
					{
						Key = "RateDescription",
						Value = "Online Confirmation - Booking will be confirmed online",
					},
					new CustomizedField
					{
						Key = "Customer",
						Value = "132812",
					},
					new CustomizedField
					{
						Key = "Customer",
						Value = "31635553-eb00-4896-b11b-416e6158f442",
					},
					new CustomizedField
					{
						Key = "Customer",
						Value = null,
					},
					new CustomizedField
					{
						Key = null,
						Value = "31635553-eb00-4896-b11b-416e6158f442",
					},
					null
				});

			shipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.Instance);
			shipment.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>
				{
					new ChargeLine
					{
						ChargeCode = new ChargeCode
						{
							Code = "FRC",
							Description = "Freight Charge"
						},
						CostOSAmount = 100m,
						CostOSCurrency = new Currency
						{
							Code = "USD"
						}
					},
					new ChargeLine
					{
						ChargeCode = new ChargeCode
						{
							Code = "VAL",
							Description = "Valuation"
						},
						CostOSAmount = 0m,
						CostOSCurrency = new Currency
						{
							Code = "USD"
						}
					},
					new ChargeLine
					{
						ChargeCode = new ChargeCode
						{
							Code = "XX",
							Description = "Other (XX)"
						},
						CostOSAmount = 0.40m,
						CostOSCurrency = new Currency
						{
							Code = "USD"
						}
					}
				});

			var rate = BookingRateBuilder.Build("081", shipment);

			CombineAssertions(() =>
			{
				AssertEquals(nameof(rate.ChargeCode), "STANDARD", rate.ChargeCode);
				AssertEquals(nameof(rate.ChargeCodeDescription), "BOOKABLE", rate.ChargeCodeDescription);
				AssertEquals(nameof(rate.Amount), 641.38m, rate.Amount);
				AssertEquals(nameof(rate.Currency), "EUR", rate.Currency);
				AssertEquals(nameof(rate.Currency), "081", rate.CarrierPrefix);
				AssertEquals(nameof(rate.Remarks), "Online Confirmation - Booking will be confirmed online", rate.Remarks);

				var transportLeg = rate.TransportLegs.Single();
				AssertEquals(nameof(transportLeg.LegOrder), 1, transportLeg.LegOrder);
				AssertEquals(nameof(transportLeg.PortOfLoadingCode), "CDG", transportLeg.PortOfLoadingCode);
				AssertEquals(nameof(transportLeg.PortOfDischargeCode), "JFK", transportLeg.PortOfDischargeCode);
				AssertEquals(nameof(transportLeg.EstimatedDeparture), new ZDateTime(2020, 12, 1), transportLeg.EstimatedDeparture);
				AssertEquals(nameof(transportLeg.EstimatedArrival), new ZDateTime(2020, 12, 2), transportLeg.EstimatedArrival);
				AssertEquals(nameof(transportLeg.VesselName), "Airbus A330-300 Passenger", transportLeg.VesselName);
				AssertEquals(nameof(transportLeg.VesselType), "333", transportLeg.VesselType);

				AssertEquals(nameof(rate.AdditionalDetails), 3, rate.AdditionalDetails.Count);
				AssertEquals(nameof(rate.AdditionalDetails), "RateDescription", rate.AdditionalDetails.First().Code);
				AssertEquals(nameof(rate.AdditionalDetails), "Online Confirmation - Booking will be confirmed online", rate.AdditionalDetails.First().Description);
				AssertEquals(nameof(rate.AdditionalDetails), "Customer", rate.AdditionalDetails.Last().Code);
				AssertEquals(nameof(rate.AdditionalDetails), "31635553-eb00-4896-b11b-416e6158f442", rate.AdditionalDetails.Last().Description);

				AssertEquals(nameof(rate.CostBreakdownCharges), 3, rate.CostBreakdownCharges.Count);
				AssertContainsExactElementsInAnyOrder("Cost Breakdown codes",
					new[] { "FRC", "VAL", "XX" },
					rate.CostBreakdownCharges.Select(c => c.ChargeCode));
				AssertContainsExactElementsInAnyOrder("Cost Breakdown descriptions",
					new[] { "Freight Charge", "Valuation", "Other (XX)" },
					rate.CostBreakdownCharges.Select(c => c.ChargeCodeDescription));
				AssertContainsExactElementsInAnyOrder("Cost Breakdown amount",
					new[] { 100m, 0.00m, 0.40m },
					rate.CostBreakdownCharges.Select(c => c.Amount));
				AssertContainsExactElementsInAnyOrder("Cost Breakdown currency",
					new[] { "USD", "USD", "USD" },
					rate.CostBreakdownCharges.Select(c => c.Currency));
			});
		}

		#endregion

		#region TestDoNotProcessIncorrectShipment

		public void TestDoNotProcessIncorrectShipment()
		{
			AssertNull("null UXml", BookingRateBuilder.Build(null, null));
			AssertNull("empty UXml", BookingRateBuilder.Build(null, new UShipment()));
		}

		#endregion
	}
}
