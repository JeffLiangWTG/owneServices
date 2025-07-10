using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippersDeclarationForDangerousGoodsHeader : DocDataObject
	{
		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		IAddress shipper;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		IAddress consignee;

		#endregion

		#region Company

		public IAddress Company
		{
			get => company;
			set => company = SetChild(company, value);
		}

		IAddress company;

		#endregion

		#region AirportOfDeparture

		public IUnloco AirportOfDeparture
		{
			get => airportOfDeparture;
			set => airportOfDeparture = SetChild(airportOfDeparture, value);
		}

		IUnloco airportOfDeparture;

		#endregion

		#region AirportOfDestination

		public IUnloco AirportOfDestination
		{
			get => airportOfDestination;
			set => airportOfDestination = SetChild(airportOfDestination, value);
		}

		IUnloco airportOfDestination;

		#endregion

		#region AirWaybillNumber

		public ZString AirWaybillNumber
		{
			get => airWaybillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(AirWaybillNumberInfo, ref airWaybillNumber, value))
				{
					Validate(AirWaybillNumberInfo);
				}
			}
		}

		ZString airWaybillNumber;

		public ZPropertyInfo AirWaybillNumberInfo => GetZPropertyInfo(nameof(AirWaybillNumber));

		#endregion

		#region ShippersReferenceNumber

		public ZString ShippersReferenceNumber
		{
			get => shippersReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersReferenceNumberInfo, ref shippersReferenceNumber, value))
				{
					Validate(ShippersReferenceNumberInfo);
				}
			}
		}

		ZString shippersReferenceNumber;

		public ZPropertyInfo ShippersReferenceNumberInfo => GetZPropertyInfo(nameof(ShippersReferenceNumber));

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		public ICodeDescription shipmentType;

		#endregion

		#region IsCargoOnly

		public ZBool IsCargoOnly
		{
			get => isCargoOnly;
			set
			{
				if (SetNonPersistentPropertyValue(IsCargoOnlyInfo, ref isCargoOnly, value))
				{
					Validate(IsCargoOnlyInfo);
				}
			}
		}

		ZBool isCargoOnly;

		public ZPropertyInfo IsCargoOnlyInfo => GetZPropertyInfo(nameof(IsCargoOnly));

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

		#region Signatory

		public ZString Signatory
		{
			get => signatory;
			set
			{
				if (SetNonPersistentPropertyValue(SignatoryInfo, ref signatory, value))
				{
					Validate(SignatoryInfo);
				}
			}
		}

		ZString signatory;

		public ZPropertyInfo SignatoryInfo => GetZPropertyInfo(nameof(Signatory));

		#endregion

		#region PlaceOfSignature

		public ZString PlaceOfSignature
		{
			get => placeOfSignature;
			set
			{
				if (SetNonPersistentPropertyValue(PlaceOfSignatureInfo, ref placeOfSignature, value))
				{
					Validate(PlaceOfSignatureInfo);
				}
			}
		}

		ZString placeOfSignature;

		public ZPropertyInfo PlaceOfSignatureInfo => GetZPropertyInfo(nameof(PlaceOfSignature));

		#endregion

		#region DateOfSignature

		public ZDateTime DateOfSignature
		{
			get => dateOfSignature;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfSignatureInfo, ref dateOfSignature, value))
				{
					Validate(DateOfSignatureInfo);
				}
			}
		}

		ZDateTime dateOfSignature;

		public ZPropertyInfo DateOfSignatureInfo => GetZPropertyInfo(nameof(DateOfSignature));

		#endregion

		#region EmergencyContact

		public IContact EmergencyContact
		{
			get => emergencyContact;
			set => emergencyContact = SetChild(emergencyContact, value);
		}

		IContact emergencyContact;

		#endregion

		#region PermittedTransportType

		public ZString PermittedTransportType
		{
			get => permittedTransportType;
			set
			{
				if (SetNonPersistentPropertyValue(PermittedTransportTypeInfo, ref permittedTransportType, value))
				{
					Validate(PermittedTransportTypeInfo);
				}
			}
		}

		ZString permittedTransportType;

		public ZPropertyInfo PermittedTransportTypeInfo => GetZPropertyInfo(nameof(PermittedTransportType));

		#endregion
	}
}
