using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	internal class OneOffQuoteDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestCompanyTariffLevelOverride()
		{
			OneOffQuote.CompanyTariffLevel = "1";
			var shipmentData = GetShipmentData(OneOffQuote);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("CompanyTariffLevelOverride should be exported", (ZByte)1, shipmentData.CompanyTariffLevelOverride);
		}

		#region Organizations

		public void TestOrganizations()
		{
			OneOffQuote.Quote.QuotationClientAddress.E2_OA_Address = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;
			CurrentOneOffQuote.PickUpDocAddress.E2_OA_Address = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;
			CurrentOneOffQuote.DeliveryDocAddress.E2_OA_Address = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals(3, shipmentData.OrganizationAddressCollection.Count);
			OrganizationAddressTestHelper.AssertOrganizationBO_WUFSHIJNB("Client Address", shipmentData.OrganizationAddressCollection.Single(x => x.AddressType.Value == "QuotationClientAddress"), "QuotationClientAddress");
			OrganizationAddressTestHelper.AssertOrganizationBO_INTHEMSYD("Consignor Address", shipmentData.OrganizationAddressCollection.Single(x => x.AddressType.Value == "OneOffQuotePickupAddress"), "OneOffQuotePickupAddress");
			OrganizationAddressTestHelper.AssertOrganizationBO_CRAHOLSYD("Consignee Address", shipmentData.OrganizationAddressCollection.Single(x => x.AddressType.Value == "OneOffQuoteDeliveryAddress"), "OneOffQuoteDeliveryAddress");
		}

		public void TestClientAddress()
		{
			testHelper.AssertClientAddress(QuoteBookingType.SpotQuote);
		}

		#endregion

		#region Dates

		public void TestDates()
		{
			var startDate = new ZDate(2022, 10, 11);
			var endDate = new ZDate(2022, 10, 21);
			var acceptedDate = new ZDate(2022, 10, 8);
			var clientAcceptedDate = new ZDate(2022, 10, 9);
			var followUpDate = new ZDate(2022, 10, 10);

			OneOffQuote.StartDate = startDate;
			OneOffQuote.EndDate = endDate;

			OneOffQuote.Quote.TH_Accepted = acceptedDate;
			OneOffQuote.Quote.TH_ClientAccepted = clientAcceptedDate;
			OneOffQuote.Quote.TH_FollowUpDate = followUpDate;

			var shipmentData = GetShipmentData(OneOffQuote);

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.DateCollection", shipmentData.DateCollection);
			CombineAssertions("Checking all dates in DateCollection", () =>
			{
				AssertEquals("Checking dates found.", 5, shipmentData.DateCollection.Count);
				shipmentData.DateCollection.AssertDateExists(DateType.Start, ZBool.False, startDate);
				shipmentData.DateCollection.AssertDateExists(DateType.End, ZBool.False, endDate);

				shipmentData.DateCollection.AssertDateExists(DateType.Accepted, ZBool.False, acceptedDate);
				shipmentData.DateCollection.AssertDateExists(DateType.ClientAccepted, ZBool.False, clientAcceptedDate);
				shipmentData.DateCollection.AssertDateExists(DateType.FollowUp, ZBool.False, followUpDate);
				AssertEquals("Checking dates left, and found dates not expected.", 0, shipmentData.DateCollection.Count);
			});
		}

		#endregion

		#region General Fields

		public void TestWithFMCTariffID()
		{
			OneOffQuote.FMCTariffID = "abcd";
			var shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals((ZString)"abcd", shipmentData.FMCTariffID);

			OneOffQuote.FMCTariffID = "";
			shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals((ZString)"", shipmentData.FMCTariffID);
		}

		public void TestWithCommodity()
		{
			OneOffQuote.Commodity = "abcd";
			var shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals("abcd", shipmentData.RateCommodity.Code);

			OneOffQuote.Commodity = "";
			shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals((ZString)"", shipmentData.RateCommodity.Code);
		}
		public void TestMode()
		{
			AssertMode("LSE", "Air Freight", "Loose");
			AssertMode("ULD", "Air Freight", "Unit Load Device");
			AssertMode("SEA", "Sea Freight", "Sea Freight (LCL and FCL)");
			AssertMode("LCL", "Sea Freight", "Less Container Load");
			AssertMode("FCL", "Sea Freight", "Full Container Load");
			AssertMode("ROA", "Road Freight", "Road Freight (LCL/LTL, FCL and FTL)");
			AssertMode("LRO", "Road Freight", "Road Freight (LCL/LTL)");
			AssertMode("FRO", "Road Freight", "Full Container Load");
			AssertMode("FTL", "Road Freight", "Full Truck Load");
			AssertMode("COU", "Courier", "Courier");
			AssertMode("RAI", "Rail Freight", "Rail Freight (LCL, FCL and FWL)");
			AssertMode("LRA", "Rail Freight", "Less Container Load");
			AssertMode("FRA", "Rail Freight", "Full Container Load");
			AssertMode("FWL", "Rail Freight", "Rail Freight (FWL)");

			void AssertMode(string code, string transportDesc, string containerDesc)
			{
				OneOffQuote.Mode = code;
				var transportMode = RatingConstants.GetTransportModeFromMode(code);
				var containerMode = RatingConstants.GetOneOffQuoteContainerModeFromMode(code);
				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("TransportMode.Code", transportMode, shipmentData.TransportMode.Code);
					AssertEquals("TransportMode.Description", transportDesc, shipmentData.TransportMode.Description);
					AssertEquals("ContainerMode.Code", containerMode, shipmentData.ContainerMode.Code);
					AssertEquals("ContainerMode.Description", containerDesc, shipmentData.ContainerMode.Description);
				});
			}
		}

		public void TestPaymentTermAndShipmentIncoTerm()
		{
			AssertPaymentTerm(true, "PPD", "Prepaid", "domestic prepaid");
			AssertPaymentTerm(true, "C3P", "Collect 3rd Party", "");

			AssertPaymentTerm(false, "CIF", "Cost, Insurance And Freight", "international incoterm: CIF");
			AssertPaymentTerm(false, "CPT", "Carriage Paid To", "");

			void AssertPaymentTerm(bool isDomestic, string code, string desc, string additionalTerms)
			{
				OneOffQuote.IsDomesticFreight = isDomestic;
				OneOffQuote.PaymentTerms = code;
				OneOffQuote.AdditionalTerms = additionalTerms;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("IsDomesticFreight", isDomestic, shipmentData.IsDomesticFreight);
					AssertEquals("ShipmentIncoTerm.Code", code, shipmentData.ShipmentIncoTerm.Code);
					AssertEquals("ShipmentIncoTerm.Description", desc, shipmentData.ShipmentIncoTerm.Description);
					AssertEquals("AdditionalTerms", additionalTerms, shipmentData.AdditionalTerms);
				});
			}
		}

		public void TestServiceLevel()
		{
			AssertServiceLevel("STD", "Standard");
			AssertServiceLevel("TSP", "Transhipment");

			void AssertServiceLevel(string code, string desc)
			{
				OneOffQuote.ServiceLevel = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("ServiceLevel.Code", code, shipmentData.ServiceLevel.Code);
					AssertEquals("ServiceLevel.Description", desc, shipmentData.ServiceLevel.Description);
				});
			}
		}

		public void TestHBLDeliveryMode()
		{
			OneOffQuote.ContainerPackModeOverride = "DOOR/DOOR";

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertNotNull("shipmentData", shipmentData);

			AssertEquals("HBLDeliveryMode.Code", "DOOR/DOOR", shipmentData.HBLContainerPackModeOverride);
		}

		public void TestOrigin()
		{
			AssertOrigin("INIXE", "Mangalore");
			AssertOrigin("AUSYD", "Sydney");

			void AssertOrigin(string code, string name)
			{
				OneOffQuote.Origin = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("PortOfOrigin.Code", code, shipmentData.PortOfOrigin.Code);
					AssertEquals("PortOfOrigin.Name", name, shipmentData.PortOfOrigin.Name);
				});
			}
		}

		public void TestDestination()
		{
			AssertDestination("INIXE", "Mangalore");
			AssertDestination("AUSYD", "Sydney");

			void AssertDestination(string code, string name)
			{
				OneOffQuote.Destination = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("PortOfDestination.Code", code, shipmentData.PortOfDestination.Code);
					AssertEquals("PortOfDestination.Name", name, shipmentData.PortOfDestination.Name);
				});
			}
		}

		public void TestVia()
		{
			AssertVia("INIXE", "Mangalore");
			AssertVia("AUSYD", "Sydney");

			void AssertVia(string code, string name)
			{
				OneOffQuote.Via = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("PortFirstForeign.Code", code, shipmentData.PortFirstForeign.Code);
					AssertEquals("PortFirstForeign.Name", name, shipmentData.PortFirstForeign.Name);
				});
			}
		}

		public void TestCarrier()
		{
			testHelper.AssertCarrier(QuoteBookingType.SpotQuote, (quotedBooking) =>
			{
				var carrier = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				carrier.OH_IsShippingProvider = true;

				quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			});
		}

		public void TestCreditor()
		{
			testHelper.AssertCreditor(QuoteBookingType.SpotQuote, (quotedBooking) =>
			{
				var creditor = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				creditor.OH_IsCreditor = true;

				quotedBooking.Quote.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;
			});
		}

		public void TestCarrierServiceLevel()
		{
			var carrier = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			carrier.OH_IsShippingProvider = true;

			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "TSP";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "Transhipment";

			CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;

			AssertCarrierServiceLevel("STD", "Standard");
			AssertCarrierServiceLevel("TSP", "Transhipment");

			void AssertCarrierServiceLevel(string code, string desc)
			{
				OneOffQuote.CarrierServiceLevel = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("CarrierServiceLevel.Code", code, shipmentData.CarrierServiceLevel.Code);
					AssertEquals("CarrierServiceLevel.Description", desc, shipmentData.CarrierServiceLevel.Description);
				});
			}
		}

		public void TestTransitTime()
		{
			AssertTransitTime("1", "1 day");
			AssertTransitTime("3", "3 days");
			AssertTransitTime("12", "12 days");

			void AssertTransitTime(string code, string name)
			{
				OneOffQuote.TransitTime = code;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("TransitTime.Code", code, shipmentData.TransitTime.Code);
					AssertEquals("TransitTime.Description", name, shipmentData.TransitTime.Description);
				});
			}
		}

		public void TestFrequency()
		{
			AssertFrequency(1, "DAYS", "Every X days");
			AssertFrequency(3, "FORTNIGHT", "X per fortnight");

			void AssertFrequency(int frequency, string frequencyUnitCode, string frequencyUnitDesc)
			{
				OneOffQuote.Frequency = frequency;
				OneOffQuote.FrequencyUnit = frequencyUnitCode;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {frequencyUnitCode}", () =>
				{
					AssertEquals("Frequency", frequency, shipmentData.Frequency);
					AssertEquals("FrequencyUnit.Code", frequencyUnitCode, shipmentData.FrequencyUnit.Code);
					AssertEquals("FrequencyUnit.Description", frequencyUnitDesc, shipmentData.FrequencyUnit.Description);
				});
			}
		}

		public void TestOneOffQuoteStatistics()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "DesEEE");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				OneOffQuote.OneOffQuoteStatistics.OneOffQuoteKPI = "EEE";
				OneOffQuote.OneOffQuoteStatistics.OneOffQuoteSource = "EEE";
				OneOffQuote.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "EEE";

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"All three fields of OOQ statistics should be populated into XML", () =>
				{
					AssertEquals("QuoteKPI.Code", "EEE", shipmentData.QuoteKPI.Code);
					AssertEquals("QuoteKPI.Description", "DesEEE", shipmentData.QuoteKPI.Description);
					AssertEquals("QuoteSource.Code", "EEE", shipmentData.QuoteSource.Code);
					AssertEquals("QuoteSource.Description", "DesEEE", shipmentData.QuoteSource.Description);
					AssertEquals("QuoteRevisionReason.Code", "EEE", shipmentData.QuoteRevisionReason.Code);
					AssertEquals("QuoteRevisionReason.Description", "DesEEE", shipmentData.QuoteRevisionReason.Description);
				});
			}
		}

		public void TestGreenhouseGasEmission()
		{
			OneOffQuote.Mode = "LSE";
			var oneOffQuoteBO = OneOffQuote.Quote.CurrentOneOffQuote;
			OneOffQuote.SetTotalCO2e(1m);
			OneOffQuote.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertNotNull("shipmentData.GreenhouseGasEmission", shipmentData.GreenhouseGasEmission);
			AssertEquals(1m, shipmentData.GreenhouseGasEmission.CO2e);
			CombineAssertions("GreenhouseGasEmission.CO2eUnit", () =>
			{
				AssertEquals("Code", "KG", shipmentData.GreenhouseGasEmission.CO2eUnit.Code);
				AssertEquals("Description", "Kilograms", shipmentData.GreenhouseGasEmission.CO2eUnit.Description);
			});
			AssertNull("Should not write into GreenhouseGasEmission.CO2eStatus", shipmentData.GreenhouseGasEmission.CO2eStatus);
			CombineAssertions("GreenhouseGasEmission.CO2eDescriptiveStatus", () =>
			{
				AssertEquals("Code", CO2eStatusList.Codes.Current, shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Code);
				AssertEquals("Description", CO2eHelper.GetCO2eStatusShortDescription(CO2eStatusList.Codes.Current), shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Description);
			});
		}

		#endregion

		#region Brokerage Details

		public void TestBrokerageDetails()
		{
			AssertBrokerageDetails(1, 12);
			AssertBrokerageDetails(3, 33);

			void AssertBrokerageDetails(short numOfEntries, short numOfLines)
			{
				OneOffQuote.QuoteNumberOfEntries = numOfEntries;
				OneOffQuote.QuoteNumberOfEntryLines = numOfLines;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("QuoteNumberOfEntries", numOfEntries, shipmentData.QuoteNumberOfEntries);
					AssertEquals("QuoteNumberOfEntryLines", numOfLines, shipmentData.QuoteNumberOfEntryLines);
				});
			}
		}

		#endregion

		#region Goods Details & Monetory Values

		public void TestGoodsDetails_LSE()
		{
			OneOffQuote.Mode = "LSE";

			AssertGeneralGoodsDetails(5, "KG", "Kilograms", 0.3m, "M3", "Cubic Meters", 30);
			AssertGeneralGoodsDetails(50, "MG", "Milligrams", 0.5m, "CI", "Cubic Inches", 95);

			AssertEquipmentAndCommodity("HUL", "Hand Unload/Load by Premise", "HWL", "Hand Unload/Load by Haulier", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("HSL", "Haulier Supplies Lift", "PSL", "Premise Supplies Lift", "SUGR", "SUGAR");

			void AssertGeneralGoodsDetails(decimal weight, string weightUnitCode, string weightUnitDesc,
				decimal volume, string volumeUnitCode, string volumeUnitDesc,
				decimal chargeable)
			{
				OneOffQuote.Weight = weight;
				OneOffQuote.WeightUnit = weightUnitCode;

				OneOffQuote.Volume = volume;
				OneOffQuote.VolumeUnit = volumeUnitCode;

				OneOffQuote.Chargeable = chargeable;
				//OneOffQuote.ChargeableUnit = chargeableUnitCode; -- have doubt on this, we don't populate this for Shipment too

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("TotalWeight", weight, shipmentData.TotalWeight);
					AssertEquals("TotalWeightUnit.Code", weightUnitCode, shipmentData.TotalWeightUnit.Code);
					AssertEquals("TotalWeightUnit.Description", weightUnitDesc, shipmentData.TotalWeightUnit.Description);

					AssertEquals("TotalVolume", volume, shipmentData.TotalVolume);
					AssertEquals("TotalVolumeUnit.Code", volumeUnitCode, shipmentData.TotalVolumeUnit.Code);
					AssertEquals("TotalVolumeUnit.Description", volumeUnitDesc, shipmentData.TotalVolumeUnit.Description);

					AssertEquals("ActualChargeable", chargeable, shipmentData.ActualChargeable);
				});
			}
		}

		public void TestGoodsDetails_FCL()
		{
			OneOffQuote.Mode = "FCL";

			AssertEquipmentAndCommodity("LOF", "Drop Container - Premise supplies Lift", "SDL", "Drop Container with Sideloader", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("TRL", "Drop Trailer", "WUP", "Wait for Pack/Unpack", "SUGR", "SUGAR");
		}

		void AssertEquipmentAndCommodity(string pickupEquipmentCode, string pickupEquipmentDesc,
				string deliveryEquipmentCode, string deliveryEquipmentDesc,
				string commodityCode, string commodityDesc)
		{
			OneOffQuote.PickupEquipment = pickupEquipmentCode;
			OneOffQuote.DeliveryEquipment = deliveryEquipmentCode;
			OneOffQuote.Commodity = commodityCode;

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertNotNull("shipmentData", shipmentData);

			var goodsDetails = shipmentData.LocalProcessing;
			AssertNotNull("goodsDetails", goodsDetails);

			CombineAssertions(() =>
			{
				AssertEquals("PickupEquipmentNeeded.Code", pickupEquipmentCode, goodsDetails.PickupEquipmentNeeded.Code);
				AssertEquals("PickupEquipmentNeeded.Description", pickupEquipmentDesc, goodsDetails.PickupEquipmentNeeded.Description);

				AssertEquals("DeliveryEquipmentNeeded.Code", deliveryEquipmentCode, goodsDetails.DeliveryEquipmentNeeded.Code);
				AssertEquals("DeliveryEquipmentNeeded.Description", deliveryEquipmentDesc, goodsDetails.DeliveryEquipmentNeeded.Description);

				AssertEquals("Commodity.Code", commodityCode, goodsDetails.Commodity.Code);
				AssertEquals("Commodity.Description", commodityDesc, goodsDetails.Commodity.Description);
			});
		}

		public void TestMonetaryValues()
		{
			AssertMonetaryValues(10000, "AUD", "Australian Dollar", 200, "USD", "United States Dollar");
			AssertMonetaryValues(20000, "USD", "United States Dollar", 50, "AUD", "Australian Dollar");

			void AssertMonetaryValues(decimal goodsValue, string goodsCurrencyCode, string goodsCurrencyDesc,
				decimal insuranceValue, string insuranceCurrencyCode, string insuranceCurrencyDesc)
			{
				OneOffQuote.GoodsValue = goodsValue;
				OneOffQuote.GoodsCurrency = goodsCurrencyCode;

				OneOffQuote.InsuranceValue = insuranceValue;
				OneOffQuote.InsuranceCurrency = insuranceCurrencyCode;

				var shipmentData = GetShipmentData(OneOffQuote);

				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("GoodsValue", goodsValue, shipmentData.GoodsValue);
					AssertEquals("GoodsValueCurrency.Code", goodsCurrencyCode, shipmentData.GoodsValueCurrency.Code);
					AssertEquals("GoodsValueCurrency.Description", goodsCurrencyDesc, shipmentData.GoodsValueCurrency.Description);

					AssertEquals("InsuranceValue", insuranceValue, shipmentData.InsuranceValue);
					AssertEquals("InsuranceValueCurrency.Code", insuranceCurrencyCode, shipmentData.InsuranceValueCurrency.Code);
					AssertEquals("InsuranceValueCurrency.Description", insuranceCurrencyDesc, shipmentData.InsuranceValueCurrency.Description);
				});
			}
		}

		#endregion

		#region Collection

		public void TestPotentialCarriers()
		{
			TestPotentialCarriersWithCreditor(1, "MAECHISHA", "MAERSK (CHINA) SHIPPING CO. LTD", "ABC", "Alphabet Shipping");
			TestPotentialCarriers(2, "AIRCAN_WW", "AIR CANADA");

			void TestPotentialCarriers(int count, string carrierCode, string carrierDescription)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = carrierCode;
				org.OH_FullName = carrierDescription;
				org.OH_IsShippingProvider = true;

				var carrier = CurrentOneOffQuote.PossibleCarriers.AddNew();
				carrier.TTC_OH_Carrier = org.PK;

				var shipmentData = GetShipmentData(OneOffQuote);
				AssertNotNull("shipmentData", shipmentData);

				var potentialCarrierCollection = shipmentData.PotentialCarrierCollection;
				AssertNotNull("potentialCarrierCollection", potentialCarrierCollection);
				AssertEquals("potentialCarrierCollection.Count", count, potentialCarrierCollection.Count);

				var potentialCarrier = potentialCarrierCollection[count - 1];
				AssertEquals("potentialCarrier.Code", carrierCode, potentialCarrier.Code);
				AssertEquals("potentialCarrier.Name", carrierDescription, potentialCarrier.Name);
			}

			void TestPotentialCarriersWithCreditor(int count, string carrierCode, string carrierDescription, string creditorCode, string creditorDescription)
			{
				TestPotentialCarriers(count, carrierCode, carrierDescription);
				var carrier = CurrentOneOffQuote.PossibleCarriers[count - 1];

				var creditor = Factory.New<OrgHeader>();
				creditor.OH_Code = creditorCode;
				creditor.OH_FullName = creditorDescription;
				creditor.OH_IsCreditor = true;
				carrier.TTC_OH_Creditor = creditor.PK;

				var potentialCarrier = GetShipmentData(OneOffQuote).PotentialCarrierCollection[count - 1];
				AssertEquals("potentialCarrier.Creditor.Code", creditorCode, potentialCarrier.Creditor.Code);
				AssertEquals("potentialCarrier.Creditor.Name", creditorDescription, potentialCarrier.Creditor.Name);
			}
		}

		public void TestContainers()
		{
			OneOffQuote.Mode = "FCL";

			var containers = CurrentOneOffQuote.Containers;
			var container1 = containers.AddNew();
			container1.TC_ContainerCount = 2;
			container1.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var container2 = containers.AddNew();
			container2.TC_ContainerCount = 3;
			container2.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var containerCollection = shipmentData.ContainerCollection;
			AssertNotNull("containerCollection", containerCollection);
			AssertEquals("containerCollection.Count", 2, shipmentData.ContainerCollection.Count);

			var container1Data = containerCollection.SingleOrDefault(x => x.ContainerType.Code.HasValue && x.ContainerType.Code.Value.Equals("20GP"));
			AssertNotNull("20GP Container", container1Data);
			AssertEquals(2, container1Data.ContainerCount);

			var container2Data = containerCollection.SingleOrDefault(x => x.ContainerType.Code.HasValue && x.ContainerType.Code.Value.Equals("40GP"));
			AssertEquals(3, container2Data.ContainerCount);
		}

		public void TestLooseCargo()
		{
			OneOffQuote.Mode = "LCL";

			var looseCargo = CurrentOneOffQuote.LooseCargo;

			var looseCargo1 = looseCargo.AddNew();
			looseCargo1.TPL_PackLineCount = 2;
			looseCargo1.TPL_F3_NKPackType = "PLT";
			looseCargo1.TPL_DimensionUQ = "M";
			looseCargo1.TPL_Length = 10;
			looseCargo1.TPL_Width = 20;
			looseCargo1.TPL_Height = 30;
			looseCargo1.TPL_Weight = 23.5m;
			looseCargo1.TPL_WeightUQ = "KG";
			looseCargo1.TPL_Volume = 0.5;
			looseCargo1.TPL_VolumeUQ = "M3";

			var looseCargo2 = looseCargo.AddNew();
			looseCargo2.TPL_PackLineCount = 3;
			looseCargo2.TPL_F3_NKPackType = "PKG";
			looseCargo2.TPL_DimensionUQ = "MI";
			looseCargo2.TPL_Length = 40;
			looseCargo2.TPL_Width = 60;
			looseCargo2.TPL_Height = 80;
			looseCargo2.TPL_Weight = 13.5m;
			looseCargo2.TPL_WeightUQ = "MG";
			looseCargo2.TPL_Volume = 1.5;
			looseCargo2.TPL_VolumeUQ = "CI";

			var shipmentData = GetShipmentData(OneOffQuote);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var looseCargoCollection = shipmentData.PackingLineCollection;
			AssertNotNull(looseCargoCollection);
			AssertEquals("looseCargoCollection.Count", 2, looseCargoCollection.Count);

			AssertLooseCargoData("PLT", 2, 23.5m, "KG", 0.5m, "M3", 10, 20, 30, "M");
			AssertLooseCargoData("PKG", 3, 13.5m, "MG", 1.5m, "CI", 40, 60, 80, "MI");

			void AssertLooseCargoData(string packTypeCode, ZShort count,
				decimal weight, string weightUnitCode,
				decimal volume, string volumeUnitCode,
				decimal length, decimal width, decimal height, string unitOfDimCode)
			{
				var looseCargoData = looseCargoCollection.SingleOrDefault(x => x.PackType.Code.HasValue && x.PackType.Code.Value.Equals(packTypeCode));
				AssertNotNull("looseCargoData", looseCargoData);

				AssertEquals("PackQty", count, (ZShort)looseCargoData.PackQty);

				AssertEquals("Weight", weight, looseCargoData.Weight);
				AssertEquals("WeightUnit.Code", weightUnitCode, looseCargoData.WeightUnit.Code);

				AssertEquals("Volume", volume, looseCargoData.Volume);
				AssertEquals("VolumeUnit.Code", volumeUnitCode, looseCargoData.VolumeUnit.Code);

				AssertEquals("LengthUnit.Code", unitOfDimCode, looseCargoData.LengthUnit.Code);
				AssertEquals("Length", length, looseCargoData.Length);
				AssertEquals("Width", width, looseCargoData.Width);
				AssertEquals("Height", height, looseCargoData.Height);
			}
		}

		public void TestAdditionalReferenceNumbers()
		{
			CurrentOneOffQuote.Numbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory.BOFactory, "CQN", "C0Q0N1"));
			var shipmentData = GetShipmentData(OneOffQuote);

			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("Precondition: shipmentData.AdditionalReferenceCollection", shipmentData.AdditionalReferenceCollection);
			CombineAssertions(() =>
			{
				AssertEquals("shipmentData.AdditionalReferenceCollection.Count", 1, shipmentData.AdditionalReferenceCollection.Count);
				AdditionalReferenceDataObjectWriterTest.AssertContents(shipmentData.AdditionalReferenceCollection[0], "CQN", "Carrier Quote Number", "C0Q0N1");
			});
		}

		#endregion

		#region Workflow Custom Fields

		public void TestWorkflowCustomFields()
		{
			testHelper.AssertWorkflowCustomFields(QuoteBookingType.SpotQuote);
		}

		public void TestWorkflowCustomFields_FromMatchedWorkflow()
		{
			testHelper.AssertWorkflowCustomFields_FromMatchedWorkflow(QuoteBookingType.SpotQuote, QuotedBooking.SpotQuoteCode, (quotedBooking) => quotedBooking.Quote);
		}

		#endregion

		#region Quote Charges

		public void TestQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP()
		{
			testHelper.AssertQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP(OneOffQuote, GetShipmentData);
		}

		#endregion

		#region Notes

		public void TestNotes_OneOffQuote()
		{
			testHelper.AssertNotes(OneOffQuote);
		}

		#endregion

		#region Status Fields

		public void TestStatusFields()
		{
			OneOffQuote.Quote.TH_IsCancelled = true;
			var shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals("IsCancelled", true, shipmentData.IsCancelled);

			OneOffQuote.Quote.TH_IsCancelled = false;
			shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals("IsCancelled", false, shipmentData.IsCancelled);

			OneOffQuote.OneOffQuoteApprovalStatus = true;
			shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals("IsQuoteApprovedByManager", true, shipmentData.IsQuoteApprovedByManager);

			OneOffQuote.OneOffQuoteApprovalStatus = false;
			shipmentData = GetShipmentData(OneOffQuote);
			AssertEquals("IsQuoteApprovedByManager", false, shipmentData.IsQuoteApprovedByManager);
		}

		#endregion

		#region Implementation

		QuotedBooking OneOffQuote
		{
			get
			{
				if (oneOffQuote == null)
				{
					oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory.BOFactory);
				}
				return oneOffQuote;
			}
		}
		QuotedBooking oneOffQuote;

		RateOneOffShipment CurrentOneOffQuote => OneOffQuote.Quote.CurrentOneOffQuote;

		Shipment GetShipmentData(QuotedBooking quotedBooking)
		{
			return GetShipmentData(quotedBooking, RecipientRoleType.ORP);
		}

		Shipment GetShipmentData(QuotedBooking quotedBooking, RecipientRoleType recipientRoleType)
		{
			var writer = new OneOffQuoteDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, quotedBooking)), quotedBooking);
			return writer.GetDataObject(quotedBooking.Quote);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testHelper = new QuotedBookingDataObjectWriterTestHelper(Factory, GetShipmentData);
		}

		QuotedBookingDataObjectWriterTestHelper testHelper;

		#endregion
	}
}
