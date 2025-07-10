using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class MultimodalDangerousGoodsDeclaration : DocDataObject
	{
		public MultimodalDangerousGoodsDeclaration(object id)
			: base(id)
		{
		}

		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(Shipper, value);
		}
		IAddress shipper;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(Consignee, value);
		}
		IAddress consignee;

		#endregion

		#region TransportDocumentNumber

		public ZString TransportDocumentNumber
		{
			get => transportDocumentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TransportDocumentNumberInfo, ref transportDocumentNumber, value))
				{
				}
			}
		}
		ZString transportDocumentNumber;

		public ZPropertyInfo TransportDocumentNumberInfo => GetZPropertyInfo(nameof(TransportDocumentNumber));

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
				}
			}
		}
		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region FreightForwarderReference

		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
				}
			}
		}
		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion

		#region Vessel

		public IVessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}
		IVessel vessel;

		#endregion

		#region MostInterestingTransportReference

		public ZString MostInterestingTransportReference
		{
			get => mostInterestingTransportReference;
			set
			{
				if (SetNonPersistentPropertyValue(MostInterestingTransportReferenceInfo, ref mostInterestingTransportReference, value))
				{
					Validate(MostInterestingTransportReferenceInfo);
				}
			}
		}
		ZString mostInterestingTransportReference;

		public ZPropertyInfo MostInterestingTransportReferenceInfo => GetZPropertyInfo(nameof(MostInterestingTransportReference));

		#endregion

		#region ETD

		public ZDateTime ETD
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(ETDInfo, ref etd, value))
				{
					Validate(ETDInfo);
				}
			}
		}
		ZDateTime etd;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region PortOfLoading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		IUnloco portOfDischarge;

		#endregion

		#region Destination

		public IUnloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		IUnloco destination;

		#endregion

		#region CarrierName

		public ZString CarrierName
		{
			get => carrierName;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierNameInfo, ref carrierName, value))
				{
				}
			}
		}
		ZString carrierName;

		public ZPropertyInfo CarrierNameInfo => GetZPropertyInfo(nameof(CarrierName));

		#endregion

		#region AdditionalHandlingInformation

		public ZString AdditionalHandlingInformation
		{
			get => additionalHandlingInformation;
			set
			{
				if (SetNonPersistentPropertyValue(AdditionalHandlingInformationInfo, ref additionalHandlingInformation, value))
				{
					Validate(AdditionalHandlingInformationInfo);
				}
			}
		}
		ZString additionalHandlingInformation;

		public ZPropertyInfo AdditionalHandlingInformationInfo => GetZPropertyInfo(nameof(AdditionalHandlingInformation));

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
					Validate(ContainerNumberInfo);
				}
			}
		}
		ZString containerNumber;

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

		#endregion

		#region SealNumber

		public ZString SealNumber
		{
			get => sealNumber;
			set
			{
				if (SetNonPersistentPropertyValue(SealNumberInfo, ref sealNumber, value))
				{
					Validate(SealNumberInfo);
				}
			}
		}
		ZString sealNumber;

		public ZPropertyInfo SealNumberInfo => GetZPropertyInfo(nameof(SealNumber));

		#endregion

		#region ContainerType

		public IContainerType ContainerType
		{
			get => containerType;
			set => containerType = SetChild(containerType, value);
		}

		IContainerType containerType;

		#endregion

		#region TareWeight

		public IMeasurement TareWeight
		{
			get => tareWeight;
			set => tareWeight = SetChild(tareWeight, value);
		}
		IMeasurement tareWeight;

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}
		IMeasurement grossWeight;

		#endregion

		#region CompanyName

		public ZString CompanyName
		{
			get => companyName;
			set
			{
				if (SetNonPersistentPropertyValue(CompanyNameInfo, ref companyName, value))
				{
					Validate(CompanyNameInfo);
				}
			}
		}
		ZString companyName;

		public ZPropertyInfo CompanyNameInfo => GetZPropertyInfo(nameof(CompanyName));

		#endregion

		#region PlaceAndDate

		public ZString PlaceAndDate
		{
			get => placeAndDate;
			set
			{
				if (SetNonPersistentPropertyValue(PlaceAndDateInfo, ref placeAndDate, value))
				{
					Validate(PlaceAndDateInfo);
				}
			}
		}
		ZString placeAndDate;

		public ZPropertyInfo PlaceAndDateInfo => GetZPropertyInfo(nameof(PlaceAndDate));

		#endregion

		#region HaulierName

		public ZString HaulierName
		{
			get => haulierName;
			set
			{
				if (SetNonPersistentPropertyValue(HaulierNameInfo, ref haulierName, value))
				{
					Validate(HaulierNameInfo);
				}
			}
		}
		ZString haulierName;

		public ZPropertyInfo HaulierNameInfo => GetZPropertyInfo(nameof(HaulierName));

		#endregion

		#region VehicleRegistrationNo

		public ZString VehicleRegistrationNo
		{
			get => vehicleRegistrationNo;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleRegistrationNoInfo, ref vehicleRegistrationNo, value))
				{
					Validate(VehicleRegistrationNoInfo);
				}
			}
		}
		ZString vehicleRegistrationNo;

		public ZPropertyInfo VehicleRegistrationNoInfo => GetZPropertyInfo(nameof(VehicleRegistrationNo));

		#endregion

		#region CompanyNameForShipper

		public ZString CompanyNameForShipper
		{
			get => companyNameForShipper;
			set
			{
				if (SetNonPersistentPropertyValue(CompanyNameForShipperInfo, ref companyNameForShipper, value))
				{
					Validate(CompanyNameForShipperInfo);
				}
			}
		}
		ZString companyNameForShipper;

		public ZPropertyInfo CompanyNameForShipperInfo => GetZPropertyInfo(nameof(CompanyNameForShipper));

		#endregion

		#region PlaceAndDateForShipper

		public ZString PlaceAndDateForShipper
		{
			get => placeAndDateForShipper;
			set
			{
				if (SetNonPersistentPropertyValue(PlaceAndDateForShipperInfo, ref placeAndDateForShipper, value))
				{
					Validate(PlaceAndDateForShipperInfo);
				}
			}
		}
		ZString placeAndDateForShipper;

		public ZPropertyInfo PlaceAndDateForShipperInfo => GetZPropertyInfo(nameof(PlaceAndDateForShipper));

		#endregion

		#region GoodsDetails

		public ZString GoodsDetails
		{
			get => goodsDetails;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDetailsInfo, ref goodsDetails, value))
				{
					Validate(GoodsDetailsInfo);
				}
			}
		}
		ZString goodsDetails;

		public ZPropertyInfo GoodsDetailsInfo => GetZPropertyInfo(nameof(GoodsDetails));

		#endregion

		#region FollowOnPages

		public IReadOnlyCollection<MultimodalDangerousGoodsDeclarationFollowOn> FollowOnPages
		{
			get => followOnPages;
			set => followOnPages = SetChildCollection(followOnPages, value);
		}
		IReadOnlyCollection<MultimodalDangerousGoodsDeclarationFollowOn> followOnPages;

		#endregion
	}
}
