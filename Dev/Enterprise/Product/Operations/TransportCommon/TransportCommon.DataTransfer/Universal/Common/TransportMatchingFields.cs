using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public class TransportMatchingFields
	{
		public TransportMatchingFields(UniversalEvent eventDataObject, BusinessObjectFactory factory, string containerNumber = "")
		{
			EventDataObject = eventDataObject;
			Factory = factory;
			this.containerNumber = containerNumber;
		}
		protected readonly UniversalEvent EventDataObject;
		protected readonly BusinessObjectFactory Factory;
		protected readonly ZString containerNumber;

		#region City

		public ZString City =>
			!string.IsNullOrEmpty(city)
				? city
				: (city = EventParameters.GetEventParameter(Constants.EventReferenceParameters.Codes.Location, Parameters, FallbackReference).GetValueOrDefault());
		ZString city;

		#endregion

		#region Context

		protected IXmlEventValueObjectContextValueList Context
		{
			get { return ((IXmlEventValueObject)EventDataObject).Context; }
		}

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get { return containerNumber; }
		}

		#endregion

		#region EventType

		public ZString EventType
		{
			get { return EventDataObject.EventType.GetValueOrDefault(); }
		}

		#endregion

		#region EventTime

		public ZDateTimeOffset EventTime
		{
			get { return EventDataObject.EventTime.GetValueOrDefault(); }
		}

		#endregion

		#region Facility

		public ZString Facility =>
			!string.IsNullOrEmpty(facility)
				? facility
				: (facility = EventParameters.GetEventParameter(Constants.EventReferenceParameters.Codes.Facility, Parameters, FallbackReference).GetValueOrDefault());
		ZString facility;

		#endregion

		#region FacilityAddressType

		public DocAddressType FacilityAddressType =>
			facilityAddressType != DocAddressType.None
			? facilityAddressType
			: facilityAddressType = DocAddressTypeExtensions.GetDocAddressTypeFromEventReferenceFacility(Facility);

		DocAddressType facilityAddressType = DocAddressType.None;

		#endregion

		#region FallbackReference

		protected ZString FallbackReference
		{
			get { return EventDataObject.EventReference.GetValueOrDefault(); }
		}

		#endregion

		#region IsMatchingAddress

		public bool IsMatchingAddress(JobDocAddress address, bool compareCity = true)
		{
			return (FacilityAddressType == DocAddressType.None || (address != null && address.DocAddressType == FacilityAddressType))
							&& (!compareCity || (City.IsEmpty || (address != null && address.E2_City.EqualsIgnoringCase(City))));
		}

		#endregion

		#region PackageID

		public ZString PackageID
		{
			get { return !Context.TransportBookingPackageID.IsEmpty ? Context.TransportBookingPackageID : ContainerNumber; }
		}

		#endregion

		#region Parameters

		protected EventParameters Parameters
		{
			get { return EventDataObject.EventParameters; }
		}

		#endregion

		#region TransportReference

		public ZString TransportReference
		{
			get { return Context.TransportReference; }
		}

		#endregion
	}
}
