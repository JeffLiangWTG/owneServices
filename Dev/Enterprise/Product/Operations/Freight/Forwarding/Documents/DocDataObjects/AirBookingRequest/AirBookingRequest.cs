using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingRequest : DocDataObject, IDataSourceProvider
	{
		public AirBookingRequest(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region EBookingApiUrlCode

		public ZString EBookingApiUrlCode
		{
			get => eBookingApiUrlCode;
			set
			{
				if (SetNonPersistentPropertyValue(EBookingApiUrlCodeInfo, ref eBookingApiUrlCode, value))
				{
					Validate(EBookingApiUrlCodeInfo);
				}
			}
		}
		ZString eBookingApiUrlCode;

		public ZPropertyInfo EBookingApiUrlCodeInfo => GetZPropertyInfo(nameof(EBookingApiUrlCode));

		#endregion

		#region TermsAndConditions

		public ZString[] TermsAndConditions
		{
			get => termsAndConditionsUrls ?? Array.Empty<ZString>();
			internal set => termsAndConditionsUrls = value;
		}

		ZString[] termsAndConditionsUrls;

		#endregion

		#region Carrier

		public ICodeDescription Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		ICodeDescription carrier;

		#endregion Carrier

		#region CASS

		public ZString CASS
		{
			get => cass;
			set
			{
				if (SetNonPersistentPropertyValue(CASSInfo, ref cass, value))
				{
					Validate(CASSInfo);
				}
			}
		}
		ZString cass;

		public ZPropertyInfo CASSInfo => GetZPropertyInfo(nameof(CASS));

		#endregion

		#region Agent

		public ZString Agent
		{
			get => agent;
			set
			{
				if (SetNonPersistentPropertyValue(AgentInfo, ref agent, value))
				{
					Validate(AgentInfo);
				}
			}
		}
		ZString agent;

		public ZPropertyInfo AgentInfo => GetZPropertyInfo(nameof(Agent));

		#endregion

		#region MasterAirWaybillNumber

		public ZString MasterAirWaybillNumber
		{
			get => masterAirWaybillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(MasterAirWaybillNumberInfo, ref masterAirWaybillNumber, value))
				{
					Validate(MasterAirWaybillNumberInfo);
				}
			}
		}

		ZString masterAirWaybillNumber;

		public ZPropertyInfo MasterAirWaybillNumberInfo => GetZPropertyInfo(nameof(MasterAirWaybillNumber));

		#endregion

		#region BookingReferenceNumber

		public ZString BookingReferenceNumber
		{
			get => bookingReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BookingReferenceNumberInfo, ref bookingReferenceNumber, value))
				{
					Validate(BookingReferenceNumberInfo);
				}
			}
		}

		ZString bookingReferenceNumber;

		public ZPropertyInfo BookingReferenceNumberInfo => GetZPropertyInfo(nameof(BookingReferenceNumber));

		#endregion BookingReferenceNumber

		#region OriginAirport

		public IUnloco OriginAirport
		{
			get => originAirport;
			set => originAirport = SetChild(originAirport, value);
		}

		IUnloco originAirport;

		#endregion OriginAirport

		#region DestinationAirport

		public IUnloco DestinationAirport
		{
			get => destinationAirport;
			set => destinationAirport = SetChild(destinationAirport, value);
		}

		IUnloco destinationAirport;

		#endregion DestinationAirport

		#region TotalPieces

		public ZInt TotalPieces
		{
			get => totalPieces;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPiecesInfo, ref totalPieces, value))
				{
					Validate(TotalPiecesInfo);
				}
			}
		}

		ZInt totalPieces;

		public ZPropertyInfo TotalPiecesInfo => GetZPropertyInfo(nameof(TotalPieces));

		#endregion TotalPieces

		#region TotalWeight

		public IMeasurement TotalWeight
		{
			get => totalWeight;
			set => totalWeight = SetChild(totalWeight, value);
		}

		IMeasurement totalWeight;

		#endregion TotalWeight

		#region TotalVolume

		public IMeasurement TotalVolume
		{
			get => totalVolume;
			set => totalVolume = SetChild(totalVolume, value);
		}

		IMeasurement totalVolume;

		#endregion TotalVolume

		#region GoodsDescription

		[List(nameof(GoodsDescriptionCollection))]
		[CustomFindBoxPopup(typeof(IAirlineCommodityFindBoxPopup))]
		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		[BusinessObjectTestExclude]
		public AirlineConfigCommodityBusinessObjectCollection GoodsDescriptionCollection { get; set; }

		#endregion

		#region Product

		[List(nameof(ProductList))]
		public ZString Product
		{
			get => product;
			set
			{
				if (SetNonPersistentPropertyValue(ProductInfo, ref product, value))
				{
					Validate(ProductInfo);
				}
			}
		}

		ZString product;

		public ZPropertyInfo ProductInfo => GetZPropertyInfo(nameof(Product));

		public CodeDescriptionPairList ProductList { get; set; }

		public Dictionary<string, RefAirlineProductCode> ProductDescriptionAndDetailsMap { get; set; }

		#endregion

		#region Commodity

		[List(nameof(CommodityCollection))]
		[CustomFindBoxPopup(typeof(IAirlineCommodityFindBoxPopup), false)]
		public ZString Commodity
		{
			get => commodity;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityInfo, ref commodity, value))
				{
					Validate(CommodityInfo);
				}
			}
		}

		ZString commodity;

		public ZPropertyInfo CommodityInfo => GetZPropertyInfo(nameof(Commodity));

		[BusinessObjectTestExclude]
		public AirlineConfigCommodityBusinessObjectCollection CommodityCollection { get; set; }

		public ZBool CommodityIsVisible
		{
			get => commodityIsVisible;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityIsVisibleInfo, ref commodityIsVisible, value))
				{
					Validate(CommodityIsVisibleInfo);
				}
			}
		}

		ZBool commodityIsVisible;

		public ZPropertyInfo CommodityIsVisibleInfo => GetZPropertyInfo(nameof(CommodityIsVisible));

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion

		#region FlightDetails

		public IReadOnlyCollection<FlightDetail> FlightDetails
		{
			get => flightDetails;
			set => flightDetails = SetChildCollection(flightDetails, value);
		}
		IReadOnlyCollection<FlightDetail> flightDetails;

		#endregion

		#region FlightDetailsNote

		public ZString FlightDetailsNote
		{
			get => flightDetailsNote;
			set
			{
				if (SetNonPersistentPropertyValue(FlightDetailsNoteInfo, ref flightDetailsNote, value))
				{
					Validate(FlightDetailsNoteInfo);
				}
			}
		}

		ZString flightDetailsNote;

		public ZPropertyInfo FlightDetailsNoteInfo => GetZPropertyInfo(nameof(FlightDetailsNote));

		#endregion

		#region Dimensions

		public IReadOnlyCollection<PackingLine> Dimensions
		{
			get => dimensions;
			set => dimensions = SetChildCollection(dimensions, value);
		}
		IReadOnlyCollection<PackingLine> dimensions;

		#endregion

		#region Ulds

		public IReadOnlyCollection<ULD> Ulds
		{
			get => ulds;
			set => ulds = SetChildCollection(ulds, value);
		}
		IReadOnlyCollection<ULD> ulds;

		#endregion

		#region CarrierContractNumbers

		public IReadOnlyCollection<ReferenceNumber> CarrierContractNumbers
		{
			get => carrierContractNumbers;
			set => carrierContractNumbers = SetChild(carrierContractNumbers, value);
		}

		IReadOnlyCollection<ReferenceNumber> carrierContractNumbers;

		#endregion

		#region SpecialInstructions

		public ZString SpecialInstructions
		{
			get => specialInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialInstructionsInfo, ref specialInstructions, value))
				{
					Validate(SpecialInstructionsInfo);
				}
			}
		}

		ZString specialInstructions;

		public ZPropertyInfo SpecialInstructionsInfo => GetZPropertyInfo(nameof(SpecialInstructions));

		#endregion

		#region DangerousGoodsHandlingInformation

		public ZString DangerousGoodsHandlingInformation
		{
			get => dangerousGoodsHandlingInformation;
			set
			{
				if (SetNonPersistentPropertyValue(DangerousGoodsHandlingInformationInfo, ref dangerousGoodsHandlingInformation, value))
				{
					Validate(DangerousGoodsHandlingInformationInfo);
				}
			}
		}

		ZString dangerousGoodsHandlingInformation;

		public ZPropertyInfo DangerousGoodsHandlingInformationInfo => GetZPropertyInfo(nameof(DangerousGoodsHandlingInformation));

		#endregion

		#region GoodsHandlingInstructions

		public ZString GoodsHandlingInstructions
		{
			get => goodsHandlingInstructions;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsHandlingInstructionsInfo, ref goodsHandlingInstructions, value))
				{
					Validate(GoodsHandlingInstructionsInfo);
				}
			}
		}

		ZString goodsHandlingInstructions;

		public ZPropertyInfo GoodsHandlingInstructionsInfo => GetZPropertyInfo(nameof(GoodsHandlingInstructions));

		#endregion

		#region BookingConfirmationNotes

		public ZString BookingConfirmationNotes
		{
			get => bookingConfirmationNotes;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationNotesInfo, ref bookingConfirmationNotes, value))
				{
					Validate(BookingConfirmationNotesInfo);
				}
			}
		}

		ZString bookingConfirmationNotes;

		public ZPropertyInfo BookingConfirmationNotesInfo => GetZPropertyInfo(nameof(BookingConfirmationNotes));

		#endregion

		#region SpecialHandling

		public ZString SpecialHandling
		{
			get => specialHandling;
			internal set
			{
				if (SetNonPersistentPropertyValue(SpecialHandlingInfo, ref specialHandling, value))
				{
					Validate(SpecialHandlingInfo);
				}
			}
		}

		ZString specialHandling;

		public ZPropertyInfo SpecialHandlingInfo => GetZPropertyInfo(nameof(SpecialHandling));

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			internal set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
					Validate(CarrierBookingReferenceInfo);
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region SpecialHandlingItems

		public IReadOnlyCollection<ICodeDescription> SpecialHandlingItems
		{
			get => specialHandlingItems;
			set => specialHandlingItems = SetChild(specialHandlingItems, value);
		}

		IReadOnlyCollection<ICodeDescription> specialHandlingItems;

		#endregion

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion IDataSourceProvider members

		#region RequiresTemperatureControl

		public ZBool RequiresTemperatureControl
		{
			get => requiresTemperatureControl;
			set
			{
				if (SetNonPersistentPropertyValue(RequiresTemperatureControlInfo, ref requiresTemperatureControl, value))
				{
					Validate(RequiresTemperatureControlInfo);
				}
			}
		}

		ZBool requiresTemperatureControl;

		public ZPropertyInfo RequiresTemperatureControlInfo => GetZPropertyInfo(nameof(RequiresTemperatureControl));

		#endregion

		#region TemperatureMinimum

		public IMeasurement TemperatureMinimum
		{
			get => temperatureMinimum;
			set => temperatureMinimum = SetChild(temperatureMinimum, value);
		}

		IMeasurement temperatureMinimum;

		#endregion

		#region TemperatureMaximum

		public IMeasurement TemperatureMaximum
		{
			get => temperatureMaximum;
			set => temperatureMaximum = SetChild(temperatureMaximum, value);
		}

		IMeasurement temperatureMaximum;

		#endregion
	}
}
