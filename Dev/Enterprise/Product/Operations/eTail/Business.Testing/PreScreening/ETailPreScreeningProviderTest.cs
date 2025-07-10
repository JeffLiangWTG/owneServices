using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class ETailPreScreeningProviderTest : TestCaseWithFactory
	{
		public void TestPreScreeningSingleConsignment_Response_PreScreeningHVLVDetailsNotEnabled()
		{
			SetUpPreScreeningConfiguration(false);

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var service = new ETailPreScreeningService(Factory);

				var response = service.PreScreenHVLVConsignment(TestConsignment.PK.ToGuid());
				Assert(response is ETailPreScreeningRegistryDisabledResponse);
			}
		}

		public void TestPreScreeningConsignmentCollection_BookingHeader_Response_PreScreeningHVLVDetailsNotEnabled()
		{
			SetUpPreScreeningConfiguration(false);

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var service = new ETailPreScreeningService(Factory);

				var response = service.PreScreenHVLVConsignmentCollection(HVLVBookingHeaderSchema.Constants.Prefix,
					bookingHeader.PK.ToGuid());
				Assert(response is ETailPreScreeningRegistryDisabledResponse);
			}
		}

		public void TestPreScreeningConsignmentCollection_Shipment_Response_PreScreeningHVLVDetailsNotEnabled()
		{
			SetUpPreScreeningConfiguration(false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(shipment.GetHVLVConsignmentHeader());

				provider.Screen();
				var response = provider.ScreeningResult;
				AssertType<ETailPreScreeningRegistryDisabledResponse>(response);
			}
		}

		public void TestGetPreScreeningRule_SHP_ConsignmentHasShipment()
		{
			SetUpPreScreeningConfiguration(true);

			var fieldHVH = rule.Fields.AddNew();
			fieldHVH.FieldDescription = "Consignee";
			fieldHVH.ValidationRule = "ERR";

			var screeningValueHVH = fieldHVH.ScreeningValues.AddNew();
			screeningValueHVH.ScreeningValue = "BBB";

			var ruleSHP = PreScreeningConfiguration.Rules.AddNew();
			ruleSHP.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			ruleSHP.TransportMode = TransportModes.Sea;

			var fieldSHP = ruleSHP.Fields.AddNew();
			fieldSHP.FieldDescription = "Consignee";
			fieldSHP.ValidationRule = "ERR";

			var screeningValueSHP = fieldSHP.ScreeningValues.AddNew();
			screeningValueSHP.ScreeningValue = "AAA";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			TestConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				TestConsignment.HVC_ConsigneeName = "BBB";
				new ETailPreScreeningProvider(TestConsignment).Screen();
				AssertNullOrEmpty("should not apply HVH Pre-Screening rule when consignment has shipment", TestConsignment.PreScreeningErrorDetails);

				TestConsignment.HVC_ConsigneeName = "AAA";
				new ETailPreScreeningProvider(TestConsignment).Screen();
				AssertNotNullOrEmpty("should apply SHP Pre-Screening rule when consignment has shipment", TestConsignment.PreScreeningErrorDetails);
			}
		}

		public void TestGetPreScreeningRule_HVH_ConsignmentHasNoShipment()
		{
			SetUpPreScreeningConfiguration(true);

			var fieldHVH = rule.Fields.AddNew();
			fieldHVH.FieldDescription = "Consignee";
			fieldHVH.ValidationRule = "ERR";

			var screeningValueHVH = fieldHVH.ScreeningValues.AddNew();
			screeningValueHVH.ScreeningValue = "BBB";

			var ruleSHP = PreScreeningConfiguration.Rules.AddNew();
			ruleSHP.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			ruleSHP.TransportMode = TransportModes.Sea;

			var fieldSHP = ruleSHP.Fields.AddNew();
			fieldSHP.FieldDescription = "Consignee";
			fieldSHP.ValidationRule = "ERR";

			var screeningValueSHP = fieldSHP.ScreeningValues.AddNew();
			screeningValueSHP.ScreeningValue = "AAA";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				TestConsignment.HVC_ConsigneeName = "AAA";
				new ETailPreScreeningProvider(TestConsignment).Screen();
				AssertNullOrEmpty("should not apply SHP Pre-Screening rule when consignment dosen't has shipment", TestConsignment.PreScreeningErrorDetails);

				TestConsignment.HVC_ConsigneeName = "BBB";
				new ETailPreScreeningProvider(TestConsignment).Screen();
				AssertNotNullOrEmpty("should apply HVH Pre-Screening rule when consignment dosen't has shipment", TestConsignment.PreScreeningErrorDetails);
			}
		}

		public void TestGetFromObjectFactory()
		{
			var provider = ObjectFactory.Get<IETailPreScreeningService>("IETailPreScreeningService", Factory);
			AssertType<ETailPreScreeningService>(provider);
		}

		public void TestIsPreScreeningHVLVDetailsEnabled()
		{
			SetUpPreScreeningConfiguration(false);

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				AssertEquals(false, ETailPreScreeningProvider.IsPreScreeningHVLVDetailsEnabled);
			}

			PreScreeningConfiguration.IsEnabled = true;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				AssertEquals(true, ETailPreScreeningProvider.IsPreScreeningHVLVDetailsEnabled);
			}
		}

		public void TestHasPreScreeningRules()
		{
			SetUpPreScreeningConfiguration(true);

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var provider = new ETailPreScreeningProvider(bookingHeader);

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				AssertEquals(true, ETailPreScreeningProvider.IsPreScreeningHVLVDetailsEnabled);
				AssertEquals(true, provider.HasPreScreeningRules);
			}

			PreScreeningConfiguration.Rules.RemoveAll();
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				AssertEquals(false, provider.HasPreScreeningRules);
			}
		}

		public void TestPreScreeningHVLVDetails_ExcludeInactiveConsignments()
		{
			SetUpPreScreeningConfiguration(true, false);
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Goods Description";
			field1.ValidationRule = "ERR";
			field1.MessageText = "Error Message On Field Level";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "PS5";
			screeningValue1.ScreeningComparisonOperatorCode = "Contains";
			screeningValue1.MessageTextPerValue = "Error Message On Value Level - PS5";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "PS5";

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(bookingHeader);
				provider.Screen();
				var preScreeningResult = provider.ScreeningResult.Results;

				AssertEquals("Error Message On Value Level - PS5", "Goods Description Screening Error - Error Message On Value Level - PS5.", preScreeningResult.First(n => n.ConsignmentPK == consignment.PK).PreScreeningErrorDetails[0].Message);
			}

			consignment.HVC_IsActive = false;
			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;

				var provider = new ETailPreScreeningProvider(bookingHeader);
				provider.Screen();
				provider.SyncScreeningResult();
				var preScreeningResponse = provider.ScreeningResult;

				AssertEquals("Response should be marked passed.", true, preScreeningResponse.Passed);
				AssertEquals("Should contain 1 result for consignment.", 0, preScreeningResponse.Results.Count);
				AssertEquals("HVC_PreScreeningStatus should be unchanged.", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
			}
		}

		public void TestPreScreeningHVLVDetails_PreScreeningStatus()
		{
			SetUpPreScreeningConfiguration(true);

			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Consignee";
			field1.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "AA";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "Shipper";
			field2.ValidationRule = "WRN";

			var screeningValue2 = field2.ScreeningValues.AddNew();
			screeningValue2.ScreeningValue = "BB";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				TestConsignment.HVC_ConsigneeName = "AA";
				TestConsignment.HVC_ShipperName = "BB";

				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				Assert(TestConsignment.HVC_PreScreeningStatusInfo.HasError("Consignee Screening Error - AA has been listed for screening.\r\n"));

				TestConsignment.HVC_ConsigneeName = "CC";
				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				Assert(TestConsignment.HVC_PreScreeningStatusInfo.HasWarning("Shipper Screening Error - BB has been listed for screening.\r\n"));

				TestConsignment.HVC_ShipperName = "DD";
				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				Assert(!TestConsignment.HVC_PreScreeningStatusInfo.HasErrors());
				Assert(!TestConsignment.HVC_PreScreeningStatusInfo.HasWarnings());
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, TestConsignment.HVC_PreScreeningStatus);
			}
		}

		public void TestPreScreeningHVLVDetails_GivenBookingHeader_HasTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var bookingHeader = newFactory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_GoodsDescription = "Xbox 360";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_GoodsDescription = "Xbox 720";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_GoodsDescription = "Xbox 1080";

			CombineAssertions("Pre-condition: all three consignments should have UNK code for prescreen status", () =>
			{
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment1.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment2.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment3.HVC_PreScreeningStatus);
			});

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(bookingHeader);

				provider.Screen();
				provider.AddLog();

				newFactory.Save();

				var log = bookingHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("The total number of consignments on the booking header log is equal to 3", "3", consignmentTotal);
				});
			}
		}

		public void TestPreScreeningHVLVDetails_GivenShipment_HasTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipment = newFactory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment1 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_GoodsDescription = "PlayStation 4";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consignment2 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_GoodsDescription = "PlayStation 5";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consignment3 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_GoodsDescription = "PlayStation 6";
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;

			newFactory.Save();

			CombineAssertions("Pre-condition: all three consignments should have UNK code for prescreen status", () =>
			{
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment1.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment2.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment3.HVC_PreScreeningStatus);
			});

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var provider = new ETailPreScreeningProvider(header);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var log = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);
				AssertEquals("The total number of consignments on the shipment log is equal to 3", "3", consignmentTotal);
			}
		}

		public void TestPreScreeningHVLVDetails_GivenConsignmentOnShipment_HasTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipment = newFactory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_GoodsDescription = "Nintendo Switch";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			newFactory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var provider = new ETailPreScreeningProvider(consignment);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var log = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("The total number of consignments on the shipment log is equal to 1", "1", consignmentTotal);
				});
			}
		}

		public void TestPreScreeningHVLVDetails_GivenConsignmentOnBookingHeader_HasTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var bookingHeader = newFactory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "Nintendo Wii";

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(consignment);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var log = bookingHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("The total number of consignments on the booking header log is equal to 1", "1", consignmentTotal);
				});
			}
		}

		public void TestPreScreeningHVLVDetails_GivenBookingHeader_WithNonUNKPreScreeningStatusConsignments_AreNotIncludedInTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var bookingHeader = newFactory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_GoodsDescription = "Final Fantasy VII";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_GoodsDescription = "Final Fantasy VIII";
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_GoodsDescription = "Final Fantasy IX";
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;

			var consignment4 = bookingHeader.Consignments.AddNew();
			consignment4.HVC_GoodsDescription = "Final Fantasy X";
			consignment4.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(bookingHeader);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var log = bookingHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("Only 1 consignment should have been included on the booking header log", "1", consignmentTotal);
				});
			}
		}

		public void TestPreScreeningHVLVDetails_GivenShipment_WithNonUNKPreScreeningStatusConsignments_AreNotIncludedInTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipment = newFactory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment1 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_GoodsDescription = "Persona 3";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consignment2 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_GoodsDescription = "Persona 4 Golden";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var consignment3 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_GoodsDescription = "Persona 5";
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;

			var consignment4 = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment4.HVC_GoodsDescription = "Persona 5 Royal";
			consignment4.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment4.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			newFactory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var provider = new ETailPreScreeningProvider(header);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var log = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode).Single();
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total, out var consignmentTotal);

				CombineAssertions(() =>
				{
					AssertEquals("Event reference should be HVLV Ready", EventReferenceParameterReasons.PreScreened, reason);
					AssertEquals("Only 1 consignment should have been included on the shipment log", "1", consignmentTotal);
				});
			}
		}

		public void TestPreScreeningHVLVDetails_GivenConsignmentOnShipment_WithNonUNKPreScreeningStatus_DoesNotMakeTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var shipment = newFactory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = newFactory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_GoodsDescription = "Dell XPS 13";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;

			newFactory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var provider = new ETailPreScreeningProvider(consignment);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var logs = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode);
				AssertEquals("There are no logs on the shipment", 0, logs.Count());
			}
		}

		public void TestPreScreeningHVLVDetails_GivenConsignmentOnBookingHeader_WithNonUNKPreScreeningStatus_DoesNotMakeTTLLog()
		{
			SetUpPreScreeningConfiguration(true);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var bookingHeader = newFactory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "Dell XPS 15";
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(consignment);
				provider.Screen();
				provider.AddLog();
				newFactory.Save();

				var logs = bookingHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode);
				AssertEquals("There are no logs on the booking header", 0, logs.Count());
			}
		}

		public void TestCustomsValueOnChange_WillUpdatePrescreeningStatusToUNK_WhenStatusIsFAL()
		{
			SetUpPreScreeningConfiguration(true);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			TestConsignment.HVC_GoodsValue = 2000;

			var item = TestConsignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				AssertEquals("consignment should fail prescreening as total goods value exceeds deminimus value", HVLVConsignmentPreScreeningStatusCodes.Codes.Failed, TestConsignment.HVC_PreScreeningStatus);

				itemLine.HVS_CustomsValue = 230;
				AssertEquals("Prescreening status should update when item line customs value is changed", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, TestConsignment.HVC_PreScreeningStatus);
			}
		}

		public void TestIntrinsicValueOnChange_WillUpdatePrescreeningStatusToUNK_WhenStatusIsFAL()
		{
			SetUpPreScreeningConfiguration(true);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			TestConsignment.HVC_GoodsValue = 2000;

			var item = TestConsignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				AssertEquals("consignment should fail prescreening as total goods value exceeds deminimus value", HVLVConsignmentPreScreeningStatusCodes.Codes.Failed, TestConsignment.HVC_PreScreeningStatus);

				itemLine.HVS_IntrinsicValue = 230;
				AssertEquals("Prescreening status should update when item line intrinsic value is changed", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, TestConsignment.HVC_PreScreeningStatus);
			}
		}

		public void TestCustomsValueOnChange_WillUpdatePrescreeningStatusToUNK_WhenStatusIsPAS()
		{
			SetUpPreScreeningConfiguration(true);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			TestConsignment.HVC_GoodsValue = 1000;

			var item = TestConsignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;
			itemLine.HVS_CustomsValue = 1000;

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				ProviderWithFactory.Screen();
				ProviderWithFactory.SyncScreeningResult();
				AssertEquals("consignment should pass prescreening as total goods value does not exceed deminimus value", HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, TestConsignment.HVC_PreScreeningStatus);

				itemLine.HVS_CustomsValue = 230;
				AssertEquals("Prescreening status should update when item line customs value is changed", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, TestConsignment.HVC_PreScreeningStatus);
			}
		}

		public void TestPreScreenHVLVConsignment_ResultsFromMultipleFieldsAreCombined()
		{
			SetUpPreScreeningConfiguration(true);

			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Origin HS Code";
			field1.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.FromHSCode = "1234.56.78";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "Special Characters";
			field2.ValidationRule = "ERR";

			var specialCharacter = field2.SpecialCharacters.AddNew();
			specialCharacter.CharacterValue = "Carriage Return";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_FormattedOriginTariff = "1234.56.78";

			TestConsignment.HVC_GoodsDescription = @"AAA
BB";

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				ProviderWithFactory.Screen();
				Assert(TestConsignment.PreScreeningErrorDetails.ToString().Contains(DataBoundResourceStrings.GetDataForProperty(line.HVS_FormattedOriginTariffInfo).Caption + " Screening Error -  has been listed for screening"));
				Assert(TestConsignment.PreScreeningErrorDetails.ToString().Contains(DataBoundResourceStrings.GetDataForProperty(TestConsignment.HVC_GoodsDescriptionInfo).Caption + " Screening Error - Carriage Return has been listed for screening."));
			}
		}

		public void TestPreScreenHVLVConsignment_ResultsFromMultipleRulesAreCombined()
		{
			SetUpPreScreeningConfiguration(true);

			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Origin HS Code";
			field1.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.FromHSCode = "1234.56.78";

			var rule2 = PreScreeningConfiguration.Rules.AddNew();
			rule2.ModuleType = rule.ModuleType;
			rule2.EmailNotificationType = EmailTo.StaffMember;

			var field2 = rule2.Fields.AddNew();
			field2.FieldDescription = "Special Characters";
			field2.ValidationRule = "ERR";

			var specialCharacter = field2.SpecialCharacters.AddNew();
			specialCharacter.CharacterValue = "Carriage Return";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_FormattedOriginTariff = "1234.56.78";

			TestConsignment.HVC_GoodsDescription = @"AAA
BB";

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				ProviderWithFactory.Screen();
				Assert(TestConsignment.PreScreeningErrorDetails.ToString().Contains(DataBoundResourceStrings.GetDataForProperty(line.HVS_FormattedOriginTariffInfo).Caption + " Screening Error -  has been listed for screening"));
				Assert(TestConsignment.PreScreeningErrorDetails.ToString().Contains(DataBoundResourceStrings.GetDataForProperty(TestConsignment.HVC_GoodsDescriptionInfo).Caption + " Screening Error - Carriage Return has been listed for screening."));
			}
		}

		ETailPreScreeningProvider ProviderWithFactory;

		HVLVDetailsPreScreeningConfiguration PreScreeningConfiguration;

		HVLVPreScreeningRule rule;

		HVLVConsignment TestConsignment;

		void SetUpPreScreeningConfiguration(bool isEnable, bool needSetUpTestConsignment = true, string moduleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader)
		{
			PreScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			PreScreeningConfiguration.IsEnabled = isEnable;

			rule = PreScreeningConfiguration.Rules.AddNew();
			rule.ModuleType = moduleType;

			if (needSetUpTestConsignment)
			{
				TestConsignment = Factory.New<HVLVConsignment>();
				ProviderWithFactory = new ETailPreScreeningProvider(TestConsignment);
			}
		}
	}
}
