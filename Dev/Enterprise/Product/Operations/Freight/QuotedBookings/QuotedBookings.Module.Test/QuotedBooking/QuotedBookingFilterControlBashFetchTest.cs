using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class QuotedBookingFilterControlBashFetchTest : FilterControlBashFetchHintTest<ViewQuotedBooking>
	{
		//JobDocAddress: 15
		//JobDeclaration: 13
		//JobShipment: 2
		//RateOneOffContainers: 2
		//RateOneOffPackLine: 1
		//JobContainer: 1
		//JobPackLines: 1
		//RateOneOffShipment: 1
		//RatingHeader: 1
		// StmALog: 1
		const int expectedBookingHits = 38;

		#region BashFetchTest

		public void TestBashFetchForView_QuotedBooking_Quote_TH_QuoteNumber()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+TH_QuoteNumber", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_QuoteStatus()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+QuoteStatus", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_UniqueConsignRef()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_UniqueConsignRef", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_OA_OH()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+OA_OH", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ClientFullName()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientFullName", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_Mode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Mode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ShipmentStatus()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// GenPivot: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// JobShipment: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ShipmentStatus", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_IsHazardous()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// UNDGDataItem: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			// RefCommodityCode: 1
			BashFetchForView("QuotedBooking+Booking+IsHazardous", 50);
		}

		public void TestBashFetchForView_QuotedBooking_Weight()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Weight", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_WeightUnit()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+WeightUnit", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Volume()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Volume", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_PickupReady()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobDocsAndCartage: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+PickupReady", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_DeliveryOpen()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobDocsAndCartage: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+DeliveryOpen", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_PickupClose()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobDocsAndCartage: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+PickupClose", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_DeliveryClose()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobDocsAndCartage: 1
			//JobPackLines: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+DeliveryClose", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_VolumeUnit()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+VolumeUnit", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Chargeable()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Chargeable", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Origin()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Origin", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Destination()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Destination", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_CurrentOneOffQuote_TT_RL_NKViaLocation()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+CurrentOneOffQuote+TT_RL_NKViaLocation", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OH_Carrier()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+OH_Carrier", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Creditor()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Creditor", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Packs()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Packs", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_PacksType()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+PacksType", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ServiceLevel()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ServiceLevel", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_FMCTariffID()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+FMCTariffID", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Commodity()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Commodity", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_CompanyTariffLevel()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+CompanyTariffLevel", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_TransportMode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+TransportMode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerMode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerMode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerPackModeOverride()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerPackModeOverride", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OneOffQuoteStatistics_OneOffQuoteKPI()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+OneOffQuoteStatistics+OneOffQuoteKPI", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_CurrentOneOffQuote_TT_TransportMode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+CurrentOneOffQuote+TT_TransportMode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_CurrentOneOffQuote_TT_ContainerMode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+CurrentOneOffQuote+TT_ContainerMode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OneOffQuoteStatistics_OneOffQuoteSource()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+OneOffQuoteStatistics+OneOffQuoteSource", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OneOffQuoteStatistics_OneOffQuoteRevisionReason()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+OneOffQuoteStatistics+OneOffQuoteRevisionReason", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_TotalCO2ForBinding()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobCO2e: 2
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobPackLines: 1
			//RateOneOffPackLine: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			BashFetchForView("QuotedBooking+TotalCO2eForBinding", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_TotalCO2eForSorting()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobCO2e: 2
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobPackLines: 1
			//RateOneOffPackLine: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			BashFetchForView("QuotedBooking+TotalCO2eForSorting", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_CO2eStatus()
		{
			//JobDocAddress: 15
			//JobDeclaration: 13
			//JobCO2e: 2
			//JobShipment: 2
			//RateOneOffContainers: 2
			//JobContainer: 1
			//JobPackLines: 1
			//RateOneOffPackLine: 1
			//RateOneOffShipment: 1
			//RatingHeader: 1
			BashFetchForView("QuotedBooking+CO2eStatus", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_PaymentTerms()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+PaymentTerms", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_AdditionalTerms()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+AdditionalTerms", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_HouseBill()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_HouseBill", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_TH_IsLocked()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+TH_IsLocked", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Quote_TH_IsOneOffQuoteConsumed()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Quote+TH_IsOneOffQuoteConsumed", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCount20GP()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCount20GP", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsignorNameOrPK()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsignorNameOrPK", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsignorContact()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffPackLine: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+ConsignorContact", 37);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeContact()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffPackLine: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+ConsigneeContact", 37);
		}

		public void TestBashFetchForView_QuotedBooking_ClientContact()
		{
			// JobDocAddress: 27
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffPackLine: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+ClientContact", 49);
		}

		public void TestBashFetchForView_QuotedBooking_GoodsValue()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+GoodsValue", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_GoodsCurrency()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+GoodsCurrency", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_GoodsDescription()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+GoodsDescription", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_Calc_CurrentVessel()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobConsolTransport: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+Booking+JS_Calc_CurrentVessel", expectedBookingHits + 15);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_Calc_CurrentVoyageFlight()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobConsolTransport: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+Booking+JS_Calc_CurrentVoyageFlight", expectedBookingHits + 15);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_E_DEP()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_E_DEP", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_E_ARV()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_E_ARV", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_LoadPort()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+LoadPort", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_DischargePort()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyDestination: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+LoadPort", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeNameOrPK()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeNameOrPK", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_DocsAndCartage_JP_FCLPickupEquipmentNeeded()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+DocsAndCartage+JP_FCLPickupEquipmentNeeded", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_BookingReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_BookingReference", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_InterimReceipt()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_InterimReceipt", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_A_RCV()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_A_RCV", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_Sailing_JX_DepotReceivalCommences()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+Sailing+JX_DepotReceivalCommences", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_Sailing_JX_DepotCutOff()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+Sailing+JX_DepotCutOff", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_Sailing_JX_JA_CTOReceivalCommences()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+Sailing+JX_JA_CTOReceivalCommences", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_Sailing_JX_JA_CTOCutOff()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+Sailing+JX_JA_CTOCutOff", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_Sailing_JX_JA_DocumentaryCutoff()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobConShipLink: 1
			// JobContainer: 1
			// JobPackLines: 1
			// JobSailing: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+Sailing+JX_JA_DocumentaryCutoff", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_A_BKD()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_A_BKD", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_CFSReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_CFSReference", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_TEUCount()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+TEUCount", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCount()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCount", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCount20RE()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCount20RE", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCount40GP()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCount40GP", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCount40RE()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCount40RE", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ContainerCountOther()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ContainerCountOther", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Job_RepSales_GS_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// GlbStaff: 1
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+RepSales+GS_Code", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_BookingPartyDocumentaryAddress_Organisation_MainAddress_OA_OH()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// OrgAddress: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+BookingPartyDocumentaryAddress+Organisation+MainAddress+OA_OH", expectedBookingHits + 4);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_OH_DeliveryAgent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// JobShipment: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_OH_DeliveryAgent", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_BookingPartyDocumentaryAddress_Organisation_OH_FullName()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+BookingPartyDocumentaryAddress+Organisation+OH_FullName", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_PickupAgentCompanyCode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+PickupAgentCompanyCode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_ReleaseType()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_ReleaseType", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_HBLAWBChargesDisplay()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+HBLAWBChargesDisplay", expectedBookingHits);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_ActualDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_P9_ActualDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+P9_ActualDateForBinding", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_ActualDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDateForBinding", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_P9_ActualDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDateForBinding", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+LastMilestone+DescriptionWithReference", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+DescriptionWithReference", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_P9_SE_NKMilestoneEvent()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+NextMilestone+DescriptionWithReference", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+DescriptionWithReference", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_DescriptionWithReference()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_ScheduledDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_P9_ScheduledDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+P9_ScheduledDateForBinding", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_ScheduledDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDateForBinding", expectedBookingHits + 12);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_P9_ScheduledDateForBinding()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// ProcessTasks: 3
			// JobShipment: 2
			// RateOneOffContainers: 2
			// AccTransactionHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPackLines: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyOrigin: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// WhsDocketJobPivot: 1
			// StmALog: 1
			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDateForBinding", expectedBookingHits + 20);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_NumbersAsString()
		{
			// Tested table indicated by: <-
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// CusEntryNum: 1 <-
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+NumbersAsString", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_IsTemperatureControlled()
		{
			// Tested table indicated by: <-
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// CusEntryNum: 1 <-
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+IsTemperatureControlled", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_ScreeningStatus()
		{
			// Tested table indicated by: <-
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// CusEntryNum: 1 <-
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_ScreeningStatus", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_Calc_DGClass()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// UNDGDataItem: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_Calc_DGClass", 49);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_Calc_DGSubstance()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13 => 28
			// JobPackLines: 12 => 40
			// JobShipment: 2 => 42
			// RateOneOffContainers: 2 => 44
			// JobContainer: 1 => 45
			// JobPackLines: 1 => 46
			// RateOneOffShipment: 1 => 47
			// RatingHeader: 1 => 48
			// UNDGDataItem: 1 => 49
			// UNDGSubstancePivot: 12 => 61
			BashFetchForView("QuotedBooking+Booking+JS_Calc_DGSubstance", 61);
		}

		public void TestBashFetchForView_QuotedBooking_Job_Branch_GB_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// GlbStaff: 1
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+Branch+GB_Code", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_AddressAsASingleLine()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+AddressAsASingleLine", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_AddressOverride()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_AddressOverride", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_Address1()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_Address1", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_Address2()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_Address2", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_City()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_City", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_State()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_State", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_RN_NKCountryCode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_RN_NKCountryCode", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_ConsigneeDocumentaryAddress_E2_CompanyName()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ConsigneeDocumentaryAddress+E2_CompanyName", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_ControllingAgent_OH_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+ControllingAgent+OH_Code", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_ControllingCustomer_OH_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+ControllingCustomer+OH_Code", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_Job_Department_GE_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+Department+GE_Code", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_NotesChecker_HasGoodsHandlingInstructions()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobCartage: 12
			// JobDocumentData: 12
			// JobOrderHeader: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// StmNote: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+NotesChecker+HasGoodsHandlingInstructions", expectedBookingHits + 40);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_NotesChecker_HasSpecialInstructions()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobCartage: 12
			// JobDocumentData: 12
			// JobOrderHeader: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// StmNote: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+NotesChecker+HasSpecialInstructions", expectedBookingHits + 40);
		}

		public void TestBashFetchForView_QuotedBooking_Job_JH_Status()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+JH_Status", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Job_JH_HoldReason()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+JH_HoldReason", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Job_JH_ProfitLossReasonCode()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+JH_ProfitLossReasonCode", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Job_JH_TotalProfitRevenueMargin()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+JH_TotalProfitRevenueMargin", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_Job_JH_GS_NKRepOps()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddressCapability: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Job+JH_GS_NKRepOps", expectedBookingHits + 1);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_AddressAsASingleLine()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// OrgAddress: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+AddressAsASingleLine", expectedBookingHits + 5);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_OA_Address1()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+OA_Address1", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_OA_Address2()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+OA_Address2", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_CityFallback()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+CityFallback", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_OA_State()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+OA_State", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_RelatedCountry_RN_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// RefCountry: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+RelatedCountry+RN_Code", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_ClientAddr_OA_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+ClientAddr+OA_Code", expectedBookingHits + 2);
		}

		public void TestBashFetchForView_QuotedBooking_JS_QuoteBookingGenericOrderNumber()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobOrderHeader: 12
			// WhsDocketJobPivot: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+JS_QuoteBookingGenericOrderNumber", expectedBookingHits + 24);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_ExportReceivingDepot_Header_OH_Code()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// OrgAddress: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+ExportReceivingDepot+Header+OH_Code", expectedBookingHits + 3);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_NotifyPartyDocumentaryAddress_Organisation_MainAddress_OA_OH()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// OrgAddress: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+NotifyPartyDocumentaryAddress+Organisation+MainAddress+OA_OH", expectedBookingHits + 4);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_NotifyPartyDocumentaryAddress_Organisation_OH_FullName()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// OrgAddress: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// OrgHeader: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+NotifyPartyDocumentaryAddress+Organisation+OH_FullName", expectedBookingHits + 4);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_DocsAndCartage_JP_OrderItemsAsString()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobOrderItem: 12
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+DocsAndCartage+JP_OrderItemsAsString", expectedBookingHits + 14);
		}

		public void TestBashFetchForView_QuotedBooking_Booking_JS_IsDirectBooking()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+Booking+JS_IsDirectBooking", expectedBookingHits);
		}

		public void TestBashFetchForView_VB_SystemCreateUser()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("VB_SystemCreateUser", expectedBookingHits);
		}

		public void TestBashFetchForView_VB_SystemCreateTimeUtc()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("VB_SystemCreateTimeUtc", expectedBookingHits);
		}

		public void TestBashFetchForView_VB_SystemLastEditUser()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("VB_SystemLastEditUser", expectedBookingHits);
		}

		public void TestBashFetchForView_VB_SystemLastEditTimeUtc()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("VB_SystemLastEditTimeUtc", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OneOffQuoteApprovalStatus()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+OneOffQuoteApprovalStatus", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_OneOffQuoteIsAmended()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// RatingHeader: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			BashFetchForView("QuotedBooking+OneOffQuoteIsAmended", 48);
		}

		public void TestBashFetchForView_QuotedBooking_CarrierContractNumber()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffPackLine: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+CarrierContractNumber", 37);
		}

		public void TestBashFetchForView_QuotedBooking_AllocationLinePK()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffPackLine: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			BashFetchForView("QuotedBooking+AllocationLinePK", 37);
		}

		public void TestBashFetchForView_QuotedBooking_StartDate()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+StartDate", expectedBookingHits);
		}

		public void TestBashFetchForView_QuotedBooking_EndDate()
		{
			// JobDocAddress: 15
			// JobDeclaration: 13
			// JobShipment: 2
			// RateOneOffContainers: 2
			// JobContainer: 1
			// JobPackLines: 1
			// RateOneOffShipment: 1
			// RatingHeader: 1
			// StmALog: 1
			BashFetchForView("QuotedBooking+EndDate", expectedBookingHits);
		}

		#endregion

		protected override SchemaPKColumn PkColumn => ViewQuotedBookingSchema.PK;
		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			var importReleaseDepotOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var notifyParty = factory.NewWithValidTestData<OrgHeader>();
			var contact = notifyParty.Contacts.AddNew();
			contact.FillWithValidTestData();
			notifyParty.Contacts.Add(contact);
			var importReleaseDepot = importReleaseDepotOrgHeader.Addresses.AddNew();
			importReleaseDepot.FillWithValidTestData();
			var exportReceivingDepotOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var exportReceivingDepot = exportReceivingDepotOrgHeader.Addresses.AddNew();
			exportReceivingDepot.FillWithValidTestData();
			var controllingAgent = factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = factory.NewWithValidTestData<OrgHeader>();
			var bookingParty = factory.NewWithValidTestData<OrgHeader>();
			var localCharges = factory.NewWithValidTestData<OrgHeader>();
			var localChargesAddr = localCharges.MainAddress;
			localChargesAddr.FillWithValidTestData();
			localChargesAddr.OA_RN_NKCountryCode = "AU";
			var agentCollect = factory.NewWithValidTestData<OrgHeader>();
			var agentCollectAddr = agentCollect.MainAddress;
			agentCollectAddr.FillWithValidTestData();
			agentCollectAddr.OA_RN_NKCountryCode = "AU";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vessel";

			var voyage = factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voyage";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sales = factory.NewWithValidTestData<GlbStaff>();
			sales.GS_Code = "XXX";
			factory.Save();
			for (var i = 0; i < 12; i++)
			{
				var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, factory);
				quotedBooking.FillWithValidTestData();
				var booking = quotedBooking.Booking;
				booking.JS_UniqueConsignRef = "shipment" + i;
				booking.JS_RL_NKOrigin = "AUBNE";
				booking.JS_RL_NKDestination = "SGSIN";
				booking.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
				booking.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
				booking.ControllingAgentNameOrPK = controllingAgent.PK.ToString();
				booking.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();
				booking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
				booking.NotifyPartyContactPK = notifyParty.Contacts[0].PK;
				booking.DocsAndCartage.JP_OrderItemsAsString = "ORDER1, ORDER2";
				var packline = booking.OuterPackLines.AddNew();
				packline.UNDGs.AddNew();
				var packline2 = booking.OuterPackLines.AddNew();
				packline2.UNDGs.AddNew();
				booking.JS_JX = voyage.Sailings[0].PK;
				var job = new JobHeader.Loader(quotedBooking).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GS_NKRepSales = sales.GS_Code;
				quotedBooking.Job.LocalChargesPK = localCharges.PK;
				quotedBooking.Job.AgentCollectPK = agentCollect.PK;
				job = new JobHeader.Loader(booking).TryLoadOrCreateWithMutex();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GS_NKRepSales = sales.GS_Code;
				job = new JobHeader.Loader(quotedBooking.Quote).TryLoadOrCreateWithMutex();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GS_NKRepSales = sales.GS_Code;
				var milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;
				milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;
				milestone = quotedBooking.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;
				milestone = quotedBooking.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;
				var view = ViewQuotedBooking.LoadOrCreate(quotedBooking);
				result.Add(view.PK);
			}

			factory.Save();
			return result.ToArray();
		}

		protected override string[] GetExcludedColumnNamesForTestFetchHint()
		{
			var columnNames = new List<string>();
			columnNames.Add(nameof(QuotedBooking.IsTemplate));
			columnNames.Add("TemplateRecord+STR_IsActive");
			columnNames.Add("TemplateRecord+STR_TemplateName");
			return columnNames.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			return new QuotedBookingFilterControlForTest(Factory);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ViewQuotedBookingCollection(Factory);
		}
	}
}
