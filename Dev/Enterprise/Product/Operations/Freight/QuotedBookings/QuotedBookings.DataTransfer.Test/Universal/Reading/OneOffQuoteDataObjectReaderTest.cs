using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AdditionalReference = Enterprise.UniversalDataBuss.DataObjects.Universal.AdditionalReference;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	internal class OneOffQuoteDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		#region Organization Address

		public void TestConsignorAddress()
			=> AssertAddress(nameof(DocAddressType.OneOffQuotePickupAddress), (quotedBookingBO) => quotedBookingBO.Quote.CurrentOneOffQuote.PickUpDocAddress.Address);

		public void TestConsigneeAddress()
			=> AssertAddress(nameof(DocAddressType.OneOffQuoteDeliveryAddress), (quotedBookingBO) => quotedBookingBO.Quote.CurrentOneOffQuote.DeliveryDocAddress.Address);

		public void TestClientAddress()
			=> AssertAddress(nameof(DocAddressType.QuotationClientAddress), (quotedBookingBO) => quotedBookingBO.Quote.QuotationClientAddress.Address);

		public void TestLocalClientAddress()
			=> AssertAddress(AddressTypes.SendersLocalClient, (quotedBookingBO) => quotedBookingBO.Job.LocalChargesAddr);

		void AssertAddress(string addressType, Func<QuotedBooking, OrgAddress> getAddress)
		{
			var address = GetNewAddressData_INTHEMSYD(addressType);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Pre-condition", addressBO);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			CombineAssertions(() =>
			{
				AssertAddressContentMatches_INTHEMSYD(getAddress(quotedBookingBO));
				AssertMultilineASCIIEquals("Logger.Logs", $@"
Warning - Matching '{addressType}':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching '{addressType}':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added One Off Quote from UniversalShipment.
Information - Successfully saved One Off Quote - Quote (00001000).
".Trim(), logger.Logs);
			});
		}

		public void TestConsignorAddressOverride() => AssertAddressOverride(nameof(DocAddressType.OneOffQuotePickupAddress), q => q.Quote.CurrentOneOffQuote.PickUpDocAddress);

		public void TestConsigneeAddressOverride() => AssertAddressOverride(nameof(DocAddressType.OneOffQuoteDeliveryAddress), q => q.Quote.CurrentOneOffQuote.DeliveryDocAddress);

		public void TestLocalClientAddressOverride() => AssertAddressOverride(nameof(DocAddressType.QuotationClientAddress), q => q.Quote.QuotationClientAddress);

		void AssertAddressOverride(string addressType, Func<QuotedBooking, JobDocAddress> getAddressFunc)
		{
			var address = GetNewAddressData_INTHEMSYD(addressType);
			address.AddressOverride = true;
			address.Address1 = "123 Override St";

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);
			var addressToCheck = getAddressFunc(quotedBookingBO);

			Assert("JobDocAddress.E2_AddressOverride", addressToCheck.E2_AddressOverride);
			AssertEquals("JobDocAddress.Address.Header.OH_Code", "MISC", addressToCheck.Address.Header.OH_Code);
			AssertEquals("JobDocAddress.Address1", "123 Override St", addressToCheck.Address1);

			AssertMultilineASCIIEquals("Logger.Logs", $@"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Matching '{addressType}':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: 123 Override St; Address 2: 233 Here St; City: ThereVille]'.
Information - Added One Off Quote from UniversalShipment.
Information - Successfully saved One Off Quote - Quote (00001000).
".Trim(), logger.Logs);
		}

		public void TestLocalClientAddress_ShouldNotCreateJobIfClientAddressIsNotIdentified()
		{
			AssertJob("should not create a job if there is no address in the XML.");

			SetAddress(AddressTypes.CTOAddress);
			AssertJob("should not create a job if there is no SendersLocalClient address in the address collection.");

			var localClientAddress = SetAddress(AddressTypes.SendersLocalClient);
			AssertJob("No job should be created if the Sender's Local Client address cannot be identified, as it is not in the database.");

			var addressBO = new OrganisationDataObjectReader(localClientAddress, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Pre-condition", addressBO);

			Factory.SaveForTesting();
			AssertJob("Should create job as SendersLocalClient address can be identified.", shouldNotCreateJob: false);

			OrganizationAddress SetAddress(string addressType)
			{
				var address = GetLocalClientAddress();
				address.AddressType = addressType;

				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				shipmentDataObject.OrganizationAddressCollection.Add(address);

				return address;
			}

			void AssertJob(string message, bool shouldNotCreateJob = true)
			{
				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				if (shouldNotCreateJob)
				{
					AssertNull(message, quotedBookingBO.Job);
				}
				else
				{
					using (var job = quotedBookingBO.Job)
					{
						AssertNotNull(message, job);
					}
				}
			}
		}

		#endregion

		#region Dates

		public void TestDates()
		{
			var startDate = new ZDate(2022, 10, 11);
			var endDate = new ZDate(2022, 10, 21);

			shipmentDataObject.SetDateCollection(() => new List<Date>());
			shipmentDataObject.DateCollection.Add(DateType.Start, false, startDate);
			shipmentDataObject.DateCollection.Add(DateType.End, false, endDate);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			CombineAssertions(() =>
			{
				AssertEquals(startDate, quotedBookingBO.StartDate);
				AssertEquals(endDate, quotedBookingBO.EndDate);
			});
		}

		#endregion

		#region General Fields

		public void TestWithFMCTariffID()
		{
			shipmentDataObject.FMCTariffID = "abcd";

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = GetQuotedBooking();

			AssertNotNull(bookingBO);
			AssertEquals("booking.FMCTariffID", (ZString)"abcd", bookingBO.FMCTariffID);
		}

		public void TestWithCommodity()
		{
			shipmentDataObject.RateCommodity = new Commodity { Code = "COM", Description = "Common" };

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("booking.Commodity", (ZString)"COM", bookingBO.Commodity);
		}

		public void TestMode()
		{
			AssertMode("LSE", "Air Freight (LSE)", "AIR", "LSE");
			AssertMode("ULD", "Air Freight (ULD)", "AIR", "ULD");
			AssertMode("SEA", "Sea Freight (LCL and FCL)", "SEA", "SEA");
			AssertMode("LCL", "Sea Freight (LCL)", "SEA", "LCL");
			AssertMode("FCL", "Sea Freight (FCL)", "SEA", "FCL");
			AssertMode("ROA", "Road Freight (LCL/LTL, FCL and FTL)", "ROA", "ROA");
			AssertMode("LRO", "Road Freight (LCL/LTL)", "ROA", "LRO");
			AssertMode("FRO", "Road Freight (FCL)", "ROA", "FCL");
			AssertMode("FTL", "Road Freight (FTL)", "ROA", "FTL");
			AssertMode("COU", "Courier", "COU", "COU");
			AssertMode("RAI", "Rail Freight (LCL, FCL and FWL)", "RAI", "RAI");
			AssertMode("LRA", "Rail Freight (LCL)", "RAI", "LCL");
			AssertMode("FRA", "Rail Freight (FCL)", "RAI", "FCL");
			AssertMode("FWL", "Rail Freight (FWL)", "RAI", "FWL");

			void AssertMode(string code, string desc, string transport, string container)
			{
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = transport, Description = desc };
				shipmentDataObject.ContainerMode = new ContainerMode { Code = container };

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.Mode);
			}
		}

		public void TestSetGeneralFields_GivenInputContainerModeIsEmpty_ThenAssignTransportModeAndContainerModeForRateOneOffQuoteCorrectly()
		{
			AssertMode("LSE", "Air Freight (LSE)");
			AssertMode("ULD", "Air Freight (ULD)");
			AssertMode("SEA", "Sea Freight (LCL and FCL)");
			AssertMode("LCL", "Sea Freight (LCL)");
			AssertMode("FCL", "Sea Freight (FCL)");
			AssertMode("ROA", "Road Freight (LCL/LTL, FCL and FTL)");
			AssertMode("LRO", "Road Freight (LCL/LTL)");
			AssertMode("FRO", "Road Freight (FCL)");
			AssertMode("FTL", "Road Freight (FTL)");
			AssertMode("COU", "Courier");
			AssertMode("RAI", "Rail Freight (LCL, FCL and FWL)");
			AssertMode("LRA", "Rail Freight (LCL)");
			AssertMode("FRA", "Rail Freight (FCL)");
			AssertMode("FWL", "Rail Freight (FWL)");

			void AssertMode(string code, string desc)
			{
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = code, Description = desc };

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.Mode);
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
				shipmentDataObject.IsDomesticFreight = isDomestic;
				shipmentDataObject.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm()
				{
					Code = code,
					Description = desc
				};
				shipmentDataObject.AdditionalTerms = additionalTerms;

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("IsDomesticFreight should not update as it's calculated prop", false, quotedBookingBO.IsDomesticFreight);
					AssertEquals("PaymentTerms/ShipmentIncoTerm", code, quotedBookingBO.PaymentTerms);
					AssertEquals("AdditionalTerms", additionalTerms, quotedBookingBO.AdditionalTerms);
				});
			}
		}

		public void TestHBLContainerPackModeOverride()
		{
			shipmentDataObject.HBLContainerPackModeOverride = "DOOR/DOOR";
			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);
			AssertEquals($"Failed for HBLContainerPackModeOverride", "DOOR/DOOR", quotedBookingBO.ContainerPackModeOverride);
		}

		public void TestServiceLevel()
		{
			AssertServiceLevel("STD", "Standard");
			AssertServiceLevel("TSP", "Transhipment");

			void AssertServiceLevel(string code, string desc)
			{
				shipmentDataObject.ServiceLevel = new ServiceLevel()
				{
					Code = code,
					Description = desc
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.ServiceLevel);
			}
		}

		public void TestOrigin()
		{
			AssertOrigin("INIXE", "Mangalore");
			AssertOrigin("AUSYD", "Sydney");

			void AssertOrigin(string code, string name)
			{
				shipmentDataObject.PortOfOrigin = new UNLOCO()
				{
					Code = code,
					Name = name
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.Origin);
			}
		}

		public void TestDestination()
		{
			AssertDestination("INIXE", "Mangalore");
			AssertDestination("AUSYD", "Sydney");

			void AssertDestination(string code, string name)
			{
				shipmentDataObject.PortOfDestination = new UNLOCO()
				{
					Code = code,
					Name = name
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.Destination);
			}
		}

		public void TestVia()
		{
			AssertVia("INIXE", "Mangalore");
			AssertVia("AUSYD", "Sydney");

			void AssertVia(string code, string name)
			{
				shipmentDataObject.PortFirstForeign = new UNLOCO()
				{
					Code = code,
					Name = name
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.Via);
			}
		}

		public void TestCarrier()
		{
			var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			carrier.OH_IsShippingProvider = true;

			var address = GetNewAddressData_INTHEMSYD(DocAddressType.ShippingLineAddress);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Pre-condition", addressBO);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);
			AssertEquals(carrier, quotedBookingBO.Carrier);
		}

		public void TestCreditor()
		{
			var creditor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			creditor.OH_IsCreditor = true;

			var address = GetNewAddressData_INTHEMSYD(DocAddressType.Creditor);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Pre-condition", addressBO);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);
			AssertEquals(creditor.PK, quotedBookingBO.Creditor);
		}

		public void TestCarrierServiceLevel()
		{
			var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			carrier.OH_IsShippingProvider = true;

			var address = GetNewAddressData_INTHEMSYD(DocAddressType.ShippingLineAddress);
			var addressBO = new OrganisationDataObjectReader(address, logger, Factory).GetMatchedOrNewForTesting();
			AssertNotNull("Pre-condition", addressBO);

			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "TSP";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "Transhipment";

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(address);

			AssertCarrierServiceLevel("STD", "Standard");
			AssertCarrierServiceLevel("TSP", "Transhipment");

			void AssertCarrierServiceLevel(string code, string desc)
			{
				shipmentDataObject.CarrierServiceLevel = new ServiceLevel()
				{
					Code = code,
					Description = desc
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.CarrierServiceLevel);
			}
		}

		public void TestTransitTime()
		{
			AssertTransitTime("1", "1 day");
			AssertTransitTime("3", "3 days");
			AssertTransitTime("12", "12 days");

			void AssertTransitTime(string code, string desc)
			{
				shipmentDataObject.TransitTime = new CodeDescriptionPair()
				{
					Code = code,
					Description = desc
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals($"Failed for {code}", code, quotedBookingBO.TransitTime);
			}
		}

		public void TestFrequency()
		{
			AssertFrequency(1, "DAYS", "Every X days");
			AssertFrequency(3, "FORTNIGHT", "X per fortnight");

			void AssertFrequency(int frequency, string frequencyUnitCode, string frequencyUnitDesc)
			{
				shipmentDataObject.Frequency = frequency;
				shipmentDataObject.FrequencyUnit = new CodeDescriptionPair()
				{
					Code = frequencyUnitCode,
					Description = frequencyUnitDesc
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions($"Failed for {frequencyUnitCode}", () =>
				{
					AssertEquals("Frequency", frequency, quotedBookingBO.Frequency);
					AssertEquals("FrequencyUnit", frequencyUnitCode, quotedBookingBO.FrequencyUnit);
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
				shipmentDataObject.QuoteKPI = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				shipmentDataObject.QuoteSource = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				shipmentDataObject.QuoteRevisionReason = new CodeDescriptionPair()
				{
					Code = "EEE",
					Description = "DesEEE"
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions($"Only Quote Source is assigned", () =>
				{
					AssertEquals("Only Quote Source is assigned", "EEE", quotedBookingBO.OneOffQuoteStatistics.OneOffQuoteSource);
					AssertEquals("Quote KPI should not be assigned", "", quotedBookingBO.OneOffQuoteStatistics.OneOffQuoteKPI);
					AssertEquals("Quote Revision Reason should not be ssigned", "", quotedBookingBO.OneOffQuoteStatistics.OneOffQuoteRevisionReason);
				});
			}
		}

		#endregion

		#region Brokerage Details

		public void TestBrokerageDetails()
		{
			AssertBrokerageDetails(1, 12);
			AssertBrokerageDetails(3, 33);

			void AssertBrokerageDetails(short numOfEntries, short numOfLines)
			{
				shipmentDataObject.QuoteNumberOfEntries = numOfEntries;
				shipmentDataObject.QuoteNumberOfEntryLines = numOfLines;

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions(() =>
				{
					AssertEquals("QuoteNumberOfEntries", numOfEntries, quotedBookingBO.QuoteNumberOfEntries);
					AssertEquals("QuoteNumberOfEntryLines", numOfLines, quotedBookingBO.QuoteNumberOfEntryLines);
				});
			}
		}

		#endregion

		#region Goods Details & Monetory Values

		public void TestGoodsDetails_LSE()
		{
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR", Description = "Air Freight" };
			shipmentDataObject.ContainerMode = new ContainerMode { Code = "LSE", Description = "Loose" };

			AssertGeneralGoodsDetails(5, "KG", "Kilograms", 0.3m, "M3", "Cubic Meters", 30);
			AssertGeneralGoodsDetails(50, "MG", "Milligrams", 0.5m, "CI", "Cubic Inches", 95);

			AssertEquipmentAndCommodity("HUL", "Hand Unload/Load by Premise", "HWL", "Hand Unload/Load by Haulier", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("HSL", "Haulier Supplies Lift", "PSL", "Premise Supplies Lift", "SUGR", "SUGAR");

			void AssertGeneralGoodsDetails(decimal weight, string weightUnitCode, string weightUnitDesc,
				decimal volume, string volumeUnitCode, string volumeUnitDesc,
				decimal chargeable)
			{
				shipmentDataObject.TotalWeight = weight;
				shipmentDataObject.TotalWeightUnit = new UnitOfWeight()
				{
					Code = weightUnitCode,
					Description = weightUnitDesc,
				};

				shipmentDataObject.TotalVolume = volume;
				shipmentDataObject.TotalVolumeUnit = new UnitOfVolume()
				{
					Code = volumeUnitCode,
					Description = volumeUnitDesc,
				};

				shipmentDataObject.ActualChargeable = chargeable;

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions(() =>
				{
					AssertEquals("Weight", weight, quotedBookingBO.Weight);
					AssertEquals("WeightUnit", weightUnitCode, quotedBookingBO.WeightUnit);

					AssertEquals("Volume", volume, quotedBookingBO.Volume);
					AssertEquals("VolumeUnit", volumeUnitCode, quotedBookingBO.VolumeUnit);

					AssertEquals("Chargeable", chargeable, quotedBookingBO.Chargeable);
				});
			}
		}

		public void TestGoodsDetails_FCL()
		{
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea Freight" };
			shipmentDataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };

			AssertEquipmentAndCommodity("LOF", "Drop Container - Premise supplies Lift", "SDL", "Drop Container with Sideloader", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("TRL", "Drop Trailer", "WUP", "Wait for Pack/Unpack", "SUGR", "SUGAR");
		}

		void AssertEquipmentAndCommodity(string pickupEquipmentCode, string pickupEquipmentDesc,
				string deliveryEquipmentCode, string deliveryEquipmentDesc,
				string commodityCode, string commodityDesc)
		{
			shipmentDataObject.LocalProcessing = new LocalProcessing()
			{
				PickupEquipmentNeeded = new CodeDescriptionPair()
				{
					Code = pickupEquipmentCode,
					Description = pickupEquipmentDesc,
				},
				DeliveryEquipmentNeeded = new CodeDescriptionPair()
				{
					Code = deliveryEquipmentCode,
					Description = deliveryEquipmentDesc,
				},
				Commodity = new Commodity()
				{
					Code = commodityCode,
					Description = commodityDesc
				}
			};

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			CombineAssertions(() =>
			{
				AssertEquals("PickupEquipment", pickupEquipmentCode, quotedBookingBO.PickupEquipment);
				AssertEquals("DeliveryEquipment", deliveryEquipmentCode, quotedBookingBO.DeliveryEquipment);
				AssertEquals("Commodity", commodityCode, quotedBookingBO.Commodity);
			});
		}

		public void TestMonetaryValues()
		{
			AssertMonetaryValues(10000, "AUD", "Australian Dollar", 200, "USD", "United States Dollar");
			AssertMonetaryValues(20000, "USD", "United States Dollar", 50, "AUD", "Australian Dollar");

			void AssertMonetaryValues(decimal goodsValue, string goodsCurrencyCode, string goodsCurrencyDesc,
				decimal insuranceValue, string insuranceCurrencyCode, string insuranceCurrencyDesc)
			{
				shipmentDataObject.GoodsValue = goodsValue;
				shipmentDataObject.GoodsValueCurrency = new Currency()
				{
					Code = goodsCurrencyCode,
					Description = goodsCurrencyDesc,
				};

				shipmentDataObject.InsuranceValue = insuranceValue;
				shipmentDataObject.InsuranceValueCurrency = new Currency()
				{
					Code = insuranceCurrencyCode,
					Description = insuranceCurrencyDesc,
				};

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				CombineAssertions(() =>
				{
					AssertEquals("GoodsValue", goodsValue, quotedBookingBO.GoodsValue);
					AssertEquals("GoodsValueCurr", goodsCurrencyCode, quotedBookingBO.GoodsCurrency);

					AssertEquals("InsuranceValue", insuranceValue, quotedBookingBO.InsuranceValue);
					AssertEquals("InsuranceCurrency", insuranceCurrencyCode, quotedBookingBO.InsuranceCurrency);
				});
			}
		}

		#endregion

		#region Collection

		public void TestPotentialCarriers()
		{
			AssertPotentialCarriers(new List<(ZString carrierCode, ZString carrierName, ZString creditorCode, ZString creditorName)>
			{
				("AA", "American Airlines", "", ""),
				("DL", "Delta Airlines", "BB", "Some Org"),
				("WN", "Southwest Airlines", "CC", "Caucasus Containers"),
				("UA", "United Airlines", "DD", "Delta Airlines")
			});

			void AssertPotentialCarriers(IEnumerable<(ZString carrierCode, ZString carrierName, ZString creditorCode, ZString creditorName)> carriers)
			{
				var orgData = new List<PotentialCarrier>();

				foreach (var (carrierCode, carrierName, creditorCode, creditorName) in carriers)
				{
					var carrier = Factory.New<OrgHeader>();
					carrier.OH_Code = carrierCode;
					carrier.OH_FullName = carrierName;
					carrier.OH_IsShippingProvider = true;

					OrgHeader creditor = null!;
					if (!string.IsNullOrEmpty(creditorCode))
					{
						creditor = Factory.New<OrgHeader>();
						creditor.OH_Code = creditorCode;
						creditor.OH_FullName = creditorName;
						creditor.OH_IsCreditor = true;
					}

					orgData.Add(new (carrier, creditor));
				}

				shipmentDataObject.SetPotentialCarrierCollection(() => orgData);

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				var potentialCarrierCollection = quotedBookingBO.Quote?.CurrentOneOffQuote?.PossibleCarriers;
				AssertNotNull("potentialCarrierCollection", potentialCarrierCollection);

				var carrierData = potentialCarrierCollection.Select(x => (x.Carrier.OH_Code, x.Carrier.OH_FullName, x.Creditor?.OH_Code ?? "", x.Creditor?.OH_FullName ?? ""));
				AssertContainsExactElementsInAnyOrder(carriers, carrierData);
			}
		}

		public void TestContainers()
		{
			AssertContainers(new List<(ZString ContainerType, ZShort ContainerCount)>
			{
				("Box", 5),
				("Bag", 2),
				("Barrel", 3)
			});

			void AssertContainers(IEnumerable<(ZString ContainerType, ZShort ContainerCount)> containers)
			{
				var containerData = new DataObjectList<Container>();

				foreach (var container in containers)
				{
					var refContainer = Factory.BOFactory.New<RefContainer>();
					refContainer.RC_Code = container.ContainerType;

					containerData.Add(new Container()
					{
						ContainerType = ContainerType.New(refContainer),
						ContainerCount = container.ContainerCount
					});
				}

				shipmentDataObject.SetContainerCollection(() => containerData);

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				var containerCollection = quotedBookingBO.Quote?.CurrentOneOffQuote?.Containers;
				AssertNotNull("containerCollection", containerCollection);

				var containerBOs = containerCollection.Select(x => (x.Container.RC_Code, x.TC_ContainerCount));
				AssertContainsExactElementsInAnyOrder(containers, containerBOs);
			}
		}

		public void TestLooseCargo()
		{
			var looseCargo = new DataObjectList<PackingLine>();
			shipmentDataObject.SetPackingLineCollection(() => looseCargo);

			AssertLooseCargoData("PLT", 2, 23.5m, "KG", 0.5m, "M3", 10, 20, 30, "M");
			AssertLooseCargoData("PKG", 3, 13.5m, "MG", 1.5m, "CI", 40, 60, 80, "MI");

			void AssertLooseCargoData(string packTypeCode, ZShort count,
				decimal weight, string weightUnitCode,
				decimal volume, string volumeUnitCode,
				decimal length, decimal width, decimal height, string unitOfDimCode)
			{
				looseCargo.Add(new PackingLine()
				{
					PackType = new PackageType() { Code = packTypeCode },
					PackQty = count,
					Weight = weight,
					WeightUnit = new UnitOfWeight() { Code = weightUnitCode },
					Volume = volume,
					VolumeUnit = new UnitOfVolume() { Code = volumeUnitCode },
					LengthUnit = new UnitOfLength() { Code = unitOfDimCode },
					Length = length,
					Width = width,
					Height = height
				});

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				var looseCargoCollection = quotedBookingBO.Quote?.CurrentOneOffQuote?.LooseCargo;
				AssertNotNull("looseCargoCollection", looseCargoCollection);

				var looseCargoData = looseCargoCollection.Cast<RateOneOffPackLine>().FirstOrDefault(x => x.TPL_F3_NKPackType == packTypeCode);
				AssertNotNull("looseCargoData", looseCargoData);

				CombineAssertions(() =>
				{
					AssertEquals("TPL_PackLineCount", count, looseCargoData.TPL_PackLineCount);
					AssertEquals("TPL_Weight", weight, looseCargoData.TPL_Weight);
					AssertEquals("TPL_WeightUQ", weightUnitCode, looseCargoData.TPL_WeightUQ);

					AssertEquals("TPL_Volume", volume, looseCargoData.TPL_Volume);
					AssertEquals("TPL_VolumeUQ", volumeUnitCode, looseCargoData.TPL_VolumeUQ);

					AssertEquals("TPL_DimensionUQ", unitOfDimCode, looseCargoData.TPL_DimensionUQ);
					AssertEquals("TPL_Length", length, looseCargoData.TPL_Length);
					AssertEquals("TPL_Width", width, looseCargoData.TPL_Width);
					AssertEquals("TPL_Height", height, looseCargoData.TPL_Height);
				});
			}
		}

		public void TestAdditionalReferenceNumbers()
		{
			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			AddAdditionalReference("CQN", "Carrier Quote Number", "C0Q0N1");
			AddAdditionalReference("CQN", "Carrier Quote Number", "C0Q0N2");
			AddAdditionalReference("CON", "Carrier Contract Number", "XYMN12");
			AddAdditionalReference("XYZ", "Type that doesn't exists", "TTDE");

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			AssertContents("CQN", "C0Q0N1");
			AssertContents("CQN", "C0Q0N2");
			AssertContents("CON", "XYMN12");

			var notes = quotedBookingBO.Quote.CurrentOneOffQuote.Notes.FindByDescription("Unrecognized Additional Reference Types");
			AssertEquals("Note Collection contains XYZ note.", 1, notes.Length);
			AssertMultilineASCIIEquals(notes[0].ST_NoteText, @"Type: XYZ - Type that doesn't exists
Number: TTDE");

			void AddAdditionalReference(string entryTypeCode, string entryTypeDesc, string referenceNumber)
			{
				shipmentDataObject.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					ContextInformation = "OTH",
					ReferenceNumber = referenceNumber,
					Type = new EntryType { Code = entryTypeCode, Description = entryTypeDesc }
				});
			}

			void AssertContents(string entryTypeCode, string referenceNumber)
			{
				var additionalReferenceBO = quotedBookingBO.Quote.CurrentOneOffQuote.Numbers.Cast<CusEntryNumber>()
					.FirstOrDefault(x => x.CE_EntryType == entryTypeCode && x.CE_EntryNum == referenceNumber);
				AssertNotNull("additionalReference", additionalReferenceBO);
			}
		}

		#endregion

		#region Workflow Custom Fields

		public void TestWorkflowCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = TriggerLineTypes.Codes.QuotedBooking;
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType2 = QuotedBooking.SpotQuoteCode;

			var customFieldStr = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customFieldStr.XC_Name = "Workflow Custom Field";
			customFieldStr.XC_Type = "STR";

			var customFieldBool = Factory.New<GenCustomColumnDefinition>();
			customFieldBool.XC_Name = "Workflow Flag This!";
			customFieldBool.XC_Type = "BOO";

			Factory.SaveForTesting();

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Workflow Custom String Field", new ZString("HELLO")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Custom Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Custom Bool", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Workflow Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			shipmentDataObject.CustomizedFieldCollection.Add(incorrectCustomField);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			CombineAssertions(delegate
			{
				var customFields = quotedBookingBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Workflow Custom String Field", customFieldsString.Contains("Workflow Custom String Field - HELLO"));
				Assert("Custom Date", customFieldsString.Contains("Custom Date - 01-Jan-11 00:00:00"));
				Assert("Custom Bool", customFieldsString.Contains("Custom Bool - Y"));

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added One Off Quote from UniversalShipment.
Information - Successfully saved One Off Quote - Quote (00001000).
".Trim(), logger.Logs);
			});
		}

		#endregion

		#region Quote Charges

		public void TestQuoteCharges_CostingData()
		{
			using (Factory.BOFactory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var localCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
				shipmentDataObject.DataContext.CodesMappedToTarget = true;
				shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = "SYD";
				shipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());

				var chargeLineFRT = GetChargeLine("SYD", "FRT", "001", ZDate.Today, null,
					ZDate.Today, 100.00m, 100.00m, localCurrency, null, "AALSHI", "ABIGAS",
					"FIS", "Charge Description 1", 1, null, "FIN", 100.00m, 100.00m, localCurrency, null);
				chargeLineFRT.CostInvoiceDate = ZDate.Today;
				chargeLineFRT.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Instruction = InstructionType.Insert
				};
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLineFRT);

				var chargeLineBAF = GetChargeLine("SYD", "BAF", "001", ZDate.Today, null,
					ZDate.Today, 200.00m, 200.00m, localCurrency, null, "AALSHI", "ABIGAS",
					"FIS", "Charge Description 1", 1, null, "FIN", 250.00m, 250.00m, localCurrency, null);
				chargeLineBAF.CostInvoiceDate = ZDate.Today;
				chargeLineBAF.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Instruction = InstructionType.Insert
				};
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLineBAF);

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Whilst importing Charge Line: Job Number= Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Whilst importing Charge Line: Job Number= Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Revenue Override Comment: You have not entered a Revenue Override Comment.
Information - Added One Off Quote from UniversalShipment.
Information - Successfully saved One Off Quote - Quote (00001000).
".Trim(), logger.Logs);

				using (var job = quotedBookingBO.Job)
				{
					AssertNotNull(job);

					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
					AssertEquals("charges.Length", 2, charges.Length);

					var fRTCharge = charges.FirstOrDefault(x => x.ChargeCode.AC_Code == "FRT");
					var bAFCharge = charges.FirstOrDefault(x => x.ChargeCode.AC_Code == "BAF");

					AssertCharge(fRTCharge, chargeLineFRT);
					AssertCharge(bAFCharge, chargeLineBAF);
				}
			}
		}

		#endregion

		#region Notes

		public void TestNotes()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);

			var quotedBookingBO = GetQuotedBooking();
			AssertNotNull(quotedBookingBO);

			var notes = quotedBookingBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, notes.Length);

			CombineAssertions(delegate
			{
				AssertNoteContents(notes[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, notes[0].ST_IsCustomDescription);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added One Off Quote from UniversalShipment.
Information - Successfully saved One Off Quote - Quote (00001000) with 1 x StmNote.
".Trim(), logger.Logs);
			});
		}

		#endregion

		#region Status Fields

		public void TestOneOffQuoteStatus()
		{
			AssertOneOffQuoteStatus(true);
			AssertOneOffQuoteStatus(false);

			void AssertOneOffQuoteStatus(bool isQuoteApprovedByManager)
			{
				shipmentDataObject.IsQuoteApprovedByManager = isQuoteApprovedByManager;

				var quotedBookingBO = GetQuotedBooking();
				AssertNotNull(quotedBookingBO);
				AssertEquals(false, quotedBookingBO.Quote.CurrentOneOffQuote.TT_QuoteApprovedByManager);
			}
		}

		#endregion

		#region CO2e

		public void TestImportGreenHouseGasEmission_OneOffQuote()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, Core.Constants.RateMode.SEA, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "VNVNH", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.Quote.TH_QuoteNumber = "Q0001";
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.OneOffQuote, quotedBooking.Quote.TH_QuoteNumber);

			var reader = new OneOffQuoteDataObjectReader(dataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();
			var viewBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking.PK));
			AssertEquals(10000m, quotedBookingBO.GetTotalCO2e());
			AssertEquals(10000m, viewBooking.QuotedBooking.GetTotalCO2e());
		}

		public void TestReaderDoesNotPopulateCO2eWhenOneOffQuoteDoesNotMatchCO2eCalculationParameters()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, Core.Constants.RateMode.SEA, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "VNVNH", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.Quote.TH_QuoteNumber = "Q0001";
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.OneOffQuote, quotedBooking.Quote.TH_QuoteNumber);

			quotedBooking.Origin = "HKHKG";
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			var reader = new OneOffQuoteDataObjectReader(dataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Warning - Cannot populate CO2e for QuotedBooking because CO2e Calculation input parameters have been changed
Information - Updated One Off Quote - Quote (Q0001/A) from UniversalShipment.", logger.Logs);

			logger.ClearLogs();

			quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, Core.Constants.RateMode.SEA, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "VNVNH", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.Quote.TH_QuoteNumber = "Q0002";

			Factory.SaveForTesting();

			dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.OneOffQuote, quotedBooking.Quote.TH_QuoteNumber);

			reader = new OneOffQuoteDataObjectReader(dataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Importing greenhouse gas emissions calculation result.
Information - CO2e is calculated for QuotedBooking: One Off Quote - Quote (Q0002/A)
Information - Updated One Off Quote - Quote (Q0002/A) from UniversalShipment.", logger.Logs);
		}

		#endregion

		public void TestCompanyTariffLevelOverride()
		{
			shipmentDataObject.CompanyTariffLevelOverride = 2;
			var quotedBookingBO = GetQuotedBooking();

			AssertNotNull(quotedBookingBO);
			AssertEquals("CompanyTariffLevelOverride should be imported", "2", quotedBookingBO.CompanyTariffLevel);
		}

		#region Implementation

		protected override void SetUp()
		{
			logger = new TestErrorLogger();
			shipmentDataObject = SetupShipment();
		}

		Shipment SetupShipment()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			IDataContextDataObject dataContextDataObject2 = (shipment.DataContext = DataContextFactory.New());
			IDataContextDataObject dataContextDataObject3 = dataContextDataObject2;
			dataContextDataObject3.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			return shipment;
		}

		internal static ChargeLine GetChargeLine(ZString? branchCode, ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZString? costGSTVATID, ZDateTime? costInvoiceDate,
				ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount, ZString? creditor, ZString? debtor, ZString? departmentCode,
				ZString? description, ZShort? displaySequence, ZString? sellGSTVATID, ZString? sellInvoiceType, ZDecimal? sellLocalAmount, ZDecimal? sellOSAmount,
				ZString? sellOSCurrency, ZDecimal? sellOSGSTVATAmount)
		{
			ChargeLine chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (branchCode.HasValue)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = branchCode;
				chargeLine.Branch.Name = "EDIHQ";
			}

			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			chargeLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				chargeLine.CostGSTVATID = new TaxID();
				chargeLine.CostGSTVATID.TaxCode = costGSTVATID;
				chargeLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			chargeLine.CostInvoiceDate = costInvoiceDate;
			chargeLine.CostLocalAmount = costLocalAmount;
			chargeLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				chargeLine.CostOSCurrency = new Currency();
				chargeLine.CostOSCurrency.Code = costOSCurrency;
				chargeLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			chargeLine.CostOSGSTVATAmount = costOSGSTVATAmount;

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			if (departmentCode.HasValue)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = departmentCode;
				chargeLine.Department.Name = "Test Department";
			}

			chargeLine.Description = description;
			chargeLine.DisplaySequence = displaySequence;

			if (sellGSTVATID.HasValue)
			{
				chargeLine.SellGSTVATID = new TaxID();
				chargeLine.SellGSTVATID.TaxCode = sellGSTVATID;
				chargeLine.SellGSTVATID.Description = "Sell Tax Description";
			}

			chargeLine.SellInvoiceType = sellInvoiceType;
			chargeLine.SellLocalAmount = sellLocalAmount;
			chargeLine.SellOSAmount = sellOSAmount;

			if (sellOSCurrency.HasValue)
			{
				chargeLine.SellOSCurrency = new Currency();
				chargeLine.SellOSCurrency.Code = sellOSCurrency;
				chargeLine.SellOSCurrency.Description = "Sell Currency Description";
			}

			chargeLine.SellOSGSTVATAmount = sellOSGSTVATAmount;

			return chargeLine;
		}

		internal static void AssertCharge(JobCharge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertEquals("Branch should be equal", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.ChargeCode != null && chargeLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("charge.ChargeCode should not be null", charge.ChargeCode);
				AssertEquals("ChargeCode should be equal", chargeLine.ChargeCode.Code, charge.ChargeCode.AC_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertEquals("CostGSTRate should be equal", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Creditor != null && chargeLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", charge.CostAccount);
				AssertEquals("Creditor should be equal", chargeLine.Creditor.Key, charge.CostAccount.OH_Code);
			}

			if (chargeLine.Debtor != null && chargeLine.Debtor.Key.HasValue)
			{
				AssertNotNull("SellAccount should not be null", charge.SellAccount);
				AssertEquals("Debtor should be equal", chargeLine.Debtor.Key, charge.SellAccount.OH_Code);
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertEquals("Department should be equal", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertEquals("Description should be equal", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.DisplaySequence.HasValue)
			{
				AssertEquals("DisplaySequence should be equal", chargeLine.DisplaySequence, charge.JR_DisplaySequence);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertEquals("SellGSTRate should be equal", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellInvoiceType.HasValue)
			{
				AssertEquals("InvoiceType should be equal", chargeLine.SellInvoiceType, charge.JR_InvoiceType);
			}

			if (chargeLine.SellLocalAmount.HasValue)
			{
				AssertEquals("Sell Local Amount should be equal", chargeLine.SellLocalAmount, charge.JR_LocalSellAmt);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertEquals("Sell OS Amount should be equal", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertEquals("Sell OS Currency should be equal", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertEquals("SellOSGSTVAT Amount should be equal", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}
		}

		QuotedBooking GetQuotedBooking()
		{
			var reader = new OneOffQuoteDataObjectReader(shipmentDataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			return quotedBookingBO;
		}

		#endregion
	}
}
