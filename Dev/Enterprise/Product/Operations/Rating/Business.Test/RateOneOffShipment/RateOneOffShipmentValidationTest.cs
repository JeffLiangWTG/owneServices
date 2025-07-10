using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateOneOffShipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIncoTerms()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;

			oneOff.TT_IncoTerm = "";
			AssertNoErrors(oneOff.TT_IncoTermInfo);

			oneOff.TT_IncoTerm = "WWW";
			AssertHasErrors(oneOff.TT_IncoTermInfo);

			oneOff.TT_IncoTerm = "EXW";
			AssertNoErrors(oneOff.TT_IncoTermInfo);
		}

		public void TestPickupEquipment()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;
			oneOff.TT_TransportMode = Core.Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Core.Constants.ContainerModes.Loose;

			oneOff.TT_PickupEquipment = "";
			AssertNoErrors(oneOff.TT_PickupEquipmentInfo);

			oneOff.TT_PickupEquipment = "WWW";
			AssertHasErrors(oneOff.TT_PickupEquipmentInfo);

			oneOff.TT_PickupEquipment = oneOff.Lookups.Equipments[0].Code;
			AssertNoErrors(oneOff.TT_PickupEquipmentInfo);
		}

		public void TestDeliveryEquipment()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;
			oneOff.TT_TransportMode = Core.Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Core.Constants.ContainerModes.Loose;

			oneOff.TT_DeliveryEquipment = "";
			AssertNoErrors(oneOff.TT_DeliveryEquipmentInfo);

			oneOff.TT_DeliveryEquipment = "WWW";
			AssertHasErrors(oneOff.TT_DeliveryEquipmentInfo);

			oneOff.TT_DeliveryEquipment = oneOff.Lookups.Equipments[0].Code;
			AssertNoErrors(oneOff.TT_DeliveryEquipmentInfo);
		}

		public void TestDeliveryLocation()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;

			oneOff.TT_RL_NKDeliveryLocation = "NZAKL";
			AssertNoErrors(oneOff.TT_RL_NKDeliveryLocationInfo);

			oneOff.TT_RL_NKDeliveryLocation = "";
			AssertHasErrors(oneOff.TT_RL_NKDeliveryLocationInfo);

			oneOff.TT_RL_NKDeliveryLocation = "AUSYD";
			AssertNoErrors(oneOff.TT_RL_NKDeliveryLocationInfo);

			oneOff.TT_RL_NKDeliveryLocation = "ZZZZZ";
			AssertHasErrors(oneOff.TT_RL_NKDeliveryLocationInfo);
		}

		public void TestReceivalLocation()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;

			oneOff.TT_RL_NKReceivalLocation = "NZAKL";
			AssertNoErrors(oneOff.TT_RL_NKReceivalLocationInfo);

			oneOff.TT_RL_NKReceivalLocation = "";
			AssertHasErrors(oneOff.TT_RL_NKReceivalLocationInfo);

			oneOff.TT_RL_NKReceivalLocation = "AUSYD";
			AssertNoErrors(oneOff.TT_RL_NKReceivalLocationInfo);

			oneOff.TT_RL_NKReceivalLocation = "ZZZZZ";
			AssertHasErrors(oneOff.TT_RL_NKReceivalLocationInfo);
		}

		public void TestViaLocation()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;

			oneOff.TT_RL_NKViaLocation = "";
			AssertNoErrors(oneOff.TT_RL_NKViaLocationInfo);

			oneOff.TT_RL_NKViaLocation = "ZZZZZ";
			AssertHasErrors(oneOff.TT_RL_NKViaLocationInfo);

			oneOff.TT_RL_NKViaLocation = "AUSYD";
			AssertNoErrors(oneOff.TT_RL_NKViaLocationInfo);
		}

		public void TestCheckTT_RX_NKGoodsCurrency()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_ValueOfGoods = 55m;

			testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "###";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo, "Enter a valid " + testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo.Description + ".");

			testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "";
			AssertHasErrors(testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo);

			testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "AUD";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo);

			testQuote.CurrentOneOffQuote.TT_ValueOfGoods = 0m;
			testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo);

			testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "###";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo, "Enter a valid " + testQuote.CurrentOneOffQuote.TT_RX_NKGoodsCurrencyInfo.Description + ".");
		}

		public void TestCheckTT_UnitOfVolume()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "##";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_UnitOfVolumeInfo, "Enter a valid Volume Unit.");

			testQuote.CurrentOneOffQuote.TT_ActualVolume = 10;
			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_UnitOfVolumeInfo, "Please enter a Volume Unit.");

			testQuote.CurrentOneOffQuote.TT_ActualVolume = 0;
			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_UnitOfVolumeInfo);

			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "M3";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_UnitOfVolumeInfo);
		}

		public void TestCheckTT_ActualVolume()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Air;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.Loose;
			testQuote.CurrentOneOffQuote.TT_ActualVolume = 0;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_ActualVolumeInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");

			testQuote.CurrentOneOffQuote.TT_ActualVolume = 10;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_ActualVolumeInfo);

			testQuote.CurrentOneOffQuote.TT_ActualVolume = 0;
			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.RateMode.SEA;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_ActualVolumeInfo);

			testQuote.CurrentOneOffQuote.TT_ActualVolume = 0;
			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_ActualVolumeInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");
		}

		public void TestCheckTT_UnitOfWeight()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "##";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_UnitOfWeightInfo, "Enter a valid Weight Unit.");

			testQuote.CurrentOneOffQuote.TT_ActualWeight = 10;
			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "";
			AssertHasError(testQuote.CurrentOneOffQuote.TT_UnitOfWeightInfo, "Please enter a Weight Unit.");

			testQuote.CurrentOneOffQuote.TT_ActualWeight = 0;
			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_UnitOfWeightInfo);

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "KG";
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_UnitOfWeightInfo);
		}

		public void TestCheckTT_ActualWeight()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.LCL;
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 0;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_ActualWeightInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");

			testQuote.CurrentOneOffQuote.TT_ActualWeight = 1000;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_ActualWeightInfo);

			testQuote.CurrentOneOffQuote.TT_ActualWeight = 0;
			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_ActualWeightInfo);

			testQuote.CurrentOneOffQuote.TT_ActualWeight = 0;
			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Air;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_ActualWeightInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");

			testQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Air;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_ActualWeightInfo);
		}

		public void TestCheckTT_NumberOfEntries()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_NumberOfEntries = 0;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_NumberOfEntriesInfo);
			testQuote.CurrentOneOffQuote.TT_NumberOfEntries = 1;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_NumberOfEntriesInfo);

			testQuote.CurrentOneOffQuote.TT_NumberOfEntries = -5;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_NumberOfEntriesInfo, "Number of Entries cannot be negative.");

			testQuote.CurrentOneOffQuote["TT_NumberOfEntries"] = (ZShort)(-5);
			AssertHasError(testQuote.CurrentOneOffQuote.TT_NumberOfEntriesInfo, "Number of Entries cannot be negative.");
		}

		public void TestCheckTT_NumberOfEntryLines()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;

			testQuote.CurrentOneOffQuote.TT_NumberOfEntryLines = 0;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo);
			testQuote.CurrentOneOffQuote.TT_NumberOfEntryLines = 1;
			AssertNoErrors(testQuote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo);

			testQuote.CurrentOneOffQuote.TT_NumberOfEntryLines = -5;
			AssertHasError(testQuote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo, "Number of Customs Entry/Invoice Lines cannot be negative.");

			testQuote.CurrentOneOffQuote["TT_NumberOfEntryLines"] = (ZShort)(-5);
			AssertHasError(testQuote.CurrentOneOffQuote.TT_NumberOfEntryLinesInfo, "Number of Customs Entry/Invoice Lines cannot be negative.");
		}

		public void TestCarrier()
		{
			var airLine = Helper.NewOrgHeader();
			airLine.OH_IsShippingProvider = true;
			airLine.OH_IsAirLine = true;

			var shippingLine = Helper.NewOrgHeader();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;

			var nonCarrier = Helper.NewOrgHeader();
			nonCarrier.OH_IsShippingProvider = false;

			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOff = testQuote.CurrentOneOffQuote;

			oneOff.TT_OH_Carrier = ZGuid.Empty;
			AssertNoErrors(oneOff.TT_OH_CarrierInfo);

			oneOff.TT_OH_Carrier = nonCarrier.PK;
			AssertHasErrors(oneOff.TT_OH_CarrierInfo);

			oneOff.TT_OH_Carrier = airLine.PK;
			AssertNoErrors(oneOff.TT_OH_CarrierInfo);

			oneOff.TT_OH_Carrier = shippingLine.PK;
			AssertNoErrors(oneOff.TT_OH_CarrierInfo);
		}

		public void TestCheckTT_CompanyTariffLevelOverride()
		{
			Factory.New<GlobalTariff>();
			Factory.New<GlobalTariff>();
			Factory.Save();
			var oneOffQuote = Factory.New<RateOneOffShipment>();
			oneOffQuote.TT_CompanyTariffLevelOverride = 2;
			AssertNoErrors(oneOffQuote.TT_CompanyTariffLevelOverrideInfo);
			oneOffQuote.TT_CompanyTariffLevelOverride = 3;
			AssertHasErrors(oneOffQuote.TT_CompanyTariffLevelOverrideInfo);
		}

		public void TestCheckTT_QuoteKPI()
		{
			var oneOffQuote = Factory.New<RateOneOffShipment>();
			var list = new CodeDescriptionPairList();
			list.AddPair("WIN", "Accept the One Off Quote");
			list.AddPair("LSE", "Unaccept the One Off Quote");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				oneOffQuote.TT_QuoteKPI = "WIN";
				AssertNoErrors(oneOffQuote.TT_QuoteKPIInfo);
				oneOffQuote.TT_QuoteKPI = "Q";
				AssertHasError(oneOffQuote.TT_QuoteKPIInfo, "Enter a valid selection.");
			}
		}

		public void TestCheckTT_QuoteSource()
		{
			var oneOffQuote = Factory.New<RateOneOffShipment>();
			var list = new CodeDescriptionPairList();
			list.AddPair("WIN", "Accept the One Off Quote");
			list.AddPair("LSE", "Unaccept the One Off Quote");

			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				oneOffQuote.TT_QuoteSource = "WIN";
				AssertNoErrors(oneOffQuote.TT_QuoteSourceInfo);
				oneOffQuote.TT_QuoteSource = "Q";
				AssertHasError(oneOffQuote.TT_QuoteSourceInfo, "Enter a valid selection.");
			}
		}

		public void TestCheckTT_RevisionReason()
		{
			var oneOffQuote = Factory.New<RateOneOffShipment>();
			var list = new CodeDescriptionPairList();
			list.AddPair("WIN", "Accept the One Off Quote");
			list.AddPair("LSE", "Unaccept the One Off Quote");

			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				oneOffQuote.TT_RevisionReason = "WIN";
				AssertNoErrors(oneOffQuote.TT_RevisionReasonInfo);
				oneOffQuote.TT_RevisionReason = "Q";
				AssertHasError(oneOffQuote.TT_RevisionReasonInfo, "Enter a valid selection.");
			}
		}

		#region Implementation

		protected TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
