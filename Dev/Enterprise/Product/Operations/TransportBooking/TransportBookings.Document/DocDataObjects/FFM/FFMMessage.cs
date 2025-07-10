using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.TransportBookings.Document
{
	public sealed class FFMMessage : DocDataObject
	{
		public ZString VoyageFlightNo
		{
			get => voyageFlightNo;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNoInfo, ref voyageFlightNo, value))
				{
					Validate(VoyageFlightNoInfo);
				}
			}
		}
		ZString voyageFlightNo;
		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		public ZDateTime FlightDate
		{
			get => flightDate;
			set
			{
				if (SetNonPersistentPropertyValue(FlightDateInfo, ref flightDate, value))
				{
					Validate(FlightDateInfo);
				}
			}
		}
		ZDateTime flightDate;
		public ZPropertyInfo FlightDateInfo => GetZPropertyInfo(nameof(FlightDate));

		public ZString RegulatedAgentID
		{
			get => regulatedAgentID;
			set
			{
				if (SetNonPersistentPropertyValue(RegulatedAgentIDInfo, ref regulatedAgentID, value))
				{
					Validate(RegulatedAgentIDInfo);
				}
			}
		}
		ZString regulatedAgentID;
		public ZPropertyInfo RegulatedAgentIDInfo => GetZPropertyInfo(nameof(RegulatedAgentID));

		public ZString RegulatedAgentCountry
		{
			get => regulatedAgentCountry;
			set
			{
				if (SetNonPersistentPropertyValue(RegulatedAgentCountryInfo, ref regulatedAgentCountry, value))
				{
					Validate(RegulatedAgentCountryInfo);
				}
			}
		}
		ZString regulatedAgentCountry;
		public ZPropertyInfo RegulatedAgentCountryInfo => GetZPropertyInfo(nameof(RegulatedAgentCountry));

		public ZString SecurityStatusCode
		{
			get => securityStatusCode;
			set
			{
				if (SetNonPersistentPropertyValue(SecurityStatusCodeInfo, ref securityStatusCode, value))
				{
					Validate(SecurityStatusCodeInfo);
				}
			}
		}
		ZString securityStatusCode;
		public ZPropertyInfo SecurityStatusCodeInfo => GetZPropertyInfo(nameof(SecurityStatusCode));

		public IUnloco AirportOfDestinationCode
		{
			get => airportOfDestinationCode;
			set => airportOfDestinationCode = SetChild(airportOfDestinationCode, value);
		}
		IUnloco airportOfDestinationCode;

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

		public ZString DriverDocumentID
		{
			get => driverDocumentIDInfoID;
			set
			{
				if (SetNonPersistentPropertyValue(DriverDocumentIDInfo, ref driverDocumentIDInfoID, value))
				{
					Validate(DriverDocumentIDInfo);
				}
			}
		}
		ZString driverDocumentIDInfoID;
		public ZPropertyInfo DriverDocumentIDInfo => GetZPropertyInfo(nameof(DriverDocumentID));

		public IReadOnlyCollection<FFMMessagePackage> Packages
		{
			get => packages;
			set => packages = SetChildCollection(packages, value);
		}
		IReadOnlyCollection<FFMMessagePackage> packages;
	}
}
