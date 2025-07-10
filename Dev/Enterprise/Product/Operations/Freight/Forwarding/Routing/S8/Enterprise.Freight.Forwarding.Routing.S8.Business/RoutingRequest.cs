using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RoutingRequest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Origin UNLOCO

		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString OriginUNLOCOCode
		{
			get { return fOriginUNLOCOCode; }
			set { SetNonPersistentPropertyValue(OriginUNLOCOCodeInfo, ref fOriginUNLOCOCode, value); }
		}
		ZString fOriginUNLOCOCode;

		public ZPropertyInfo OriginUNLOCOCodeInfo { get { return GetZPropertyInfo(nameof(OriginUNLOCOCode)); } }

		public RefUNLOCO OriginUNLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, OriginUNLOCOCode); }
		}

		#endregion

		#region Destination UNLOCO

		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString DestinationUNLOCOCode
		{
			get { return fDestinationUNLOCOCode; }
			set { SetNonPersistentPropertyValue(DestinationUNLOCOCodeInfo, ref fDestinationUNLOCOCode, value); }
		}
		ZString fDestinationUNLOCOCode;

		public ZPropertyInfo DestinationUNLOCOCodeInfo { get { return GetZPropertyInfo(nameof(DestinationUNLOCOCode)); } }

		public RefUNLOCO DestinationUNLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, DestinationUNLOCOCode); }
		}

		#endregion

		#region Departure Date

		public ZDateTime DepartureDate
		{
			get { return fDepartureDate; }
			set { SetNonPersistentPropertyValue(DepartureDateInfo, ref fDepartureDate, value); }
		}
		ZDateTime fDepartureDate;
		public ZPropertyInfo DepartureDateInfo { get { return GetZPropertyInfo(nameof(DepartureDate)); } }

		#endregion

		#region Include Code Shares

		public ZBool IncludeCodeShares
		{
			get { return fIncludeCodeShares; }
			set { SetNonPersistentPropertyValue(IncludeCodeSharesInfo, ref fIncludeCodeShares, value); }
		}
		ZBool fIncludeCodeShares;
		public ZPropertyInfo IncludeCodeSharesInfo { get { return GetZPropertyInfo(nameof(IncludeCodeShares)); } }

		#endregion

		#region Airline Code

		[MaxLength(RefAirline.Schema.RM_TwoCharacterCodeMaxLength)]
		public ZString AirlineCode
		{
			get { return fAirlineCode; }
			set { SetNonPersistentPropertyValue(AirlineCodeInfo, ref fAirlineCode, value); }
		}
		ZString fAirlineCode;

		public ZPropertyInfo AirlineCodeInfo { get { return GetZPropertyInfo(nameof(AirlineCode)); } }

		#endregion

		#region Code Share / Interline Option

		[MaxLength(3)]
		public ZString CodeShareInterlineOption
		{
			get { return fCodeShareInterlineOption; }
			set { SetNonPersistentPropertyValue(CodeShareInterlineOptionInfo, ref fCodeShareInterlineOption, value); }
		}
		ZString fCodeShareInterlineOption;
		public ZPropertyInfo CodeShareInterlineOptionInfo { get { return GetZPropertyInfo(nameof(CodeShareInterlineOption)); } }

		#endregion

		#region Minimum Connection Time

		public ZInt MinimumConnectionTime
		{
			get { return fMinimumConnectionTime; }
			set { SetNonPersistentPropertyValue(MinimumConnectionTimeInfo, ref fMinimumConnectionTime, value); }
		}
		ZInt fMinimumConnectionTime;
		public ZPropertyInfo MinimumConnectionTimeInfo => GetZPropertyInfo(nameof(MinimumConnectionTime));

		#endregion

		#region Cargo/Passenger Flight Option

		[MaxLength(3)]
		public ZString CargoPassengerFlightOption
		{
			get { return fCargoPassengerFlightOption; }
			set { SetNonPersistentPropertyValue(CargoPassengerFlightOptionInfo, ref fCargoPassengerFlightOption, value); }
		}
		ZString fCargoPassengerFlightOption;
		public ZPropertyInfo CargoPassengerFlightOptionInfo { get { return GetZPropertyInfo(nameof(CargoPassengerFlightOption)); } }

		#endregion

		#region Equipment Type

		[MaxLength(3)]
		public ZString EquipmentType
		{
			get { return fEquipmentType; }
			set { SetNonPersistentPropertyValue(EquipmentTypeInfo, ref fEquipmentType, value); }
		}
		ZString fEquipmentType;
		public ZPropertyInfo EquipmentTypeInfo { get { return GetZPropertyInfo(nameof(EquipmentType)); } }

		#endregion

		#region Include Weekly Timetable

		public ZBool IncludeWeeklyTimetable
		{
			get => includeWeeklyTimetable;
			set => SetNonPersistentPropertyValue(IncludeWeeklyTimetableInfo, ref includeWeeklyTimetable, value);
		}
		ZBool includeWeeklyTimetable;

		public ZPropertyInfo IncludeWeeklyTimetableInfo => GetZPropertyInfo(nameof(IncludeWeeklyTimetable));

		#endregion

		#region Include CO2 Emission Value

		public ZBool IncludeCO2EmissionValue
		{
			get => includeCO2EmissionValue;
			set => SetNonPersistentPropertyValue(IncludeCO2EmissionValueInfo, ref includeCO2EmissionValue, value);
		}
		ZBool includeCO2EmissionValue;

		public ZPropertyInfo IncludeCO2EmissionValueInfo => GetZPropertyInfo(nameof(IncludeCO2EmissionValue));

		#endregion

		#region Connections Count

		[MaxLength(1)]
		public ZString ConnectionsCount
		{
			get => connectionsCount;
			set => SetNonPersistentPropertyValue(ConnectionsCountInfo, ref connectionsCount, value);
		}
		ZString connectionsCount;

		public ZPropertyInfo ConnectionsCountInfo => GetZPropertyInfo(nameof(ConnectionsCount));

		#endregion

		#endregion

		#region Build Message

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public string GenerateRequestString()       // Based on S8 API / Integration Guide June 26 2007
		{
			StringBuilder request = new StringBuilder();
			request.Append("R ");                                                                                                                                                           // Routing Request
			request.Append(ZDateTime.Now.ToString("yy/MM/dd hh:mm:ss") + " ");                                                              // Request Time
			request.Append("(CargoWise One) ");                                                                                                                             // Source Info
			request.Append("SSIM ");                                                                                                                                                    // Schedule Data Set
			request.Append((OriginUNLOCO != null ? OriginUNLOCO.RL_IATA : ZString.Empty) + " ");                            // Origin IATA
			request.Append((DestinationUNLOCO != null ? DestinationUNLOCO.RL_IATA : ZString.Empty) + " ");      // Dest IATA
			ZDateTime effectiveDate = DepartureDate.IsEmpty ? ZDateTime.Now : DepartureDate;
			request.Append(effectiveDate.ToString("yy/MM/dd") + " ");                                                                                   // Departure Date
			request.Append(".");                                                                                                                                                            // Primary / Alternate Engine
			request.Append(CodeShareInterlineOption.IsEmpty ? "." : CodeShareInterlineOption.ToString());           // Online/CodeShare/Interline
			request.Append("2");                                                                                                                                                            // Additional Layers
			request.Append(".");                                                                                                                                                            // Segment Reuse Limit
			request.Append(IncludeWeeklyTimetable ? "T" : "H");                                                                                             // Automatic Look Ahead
			request.Append(ConnectionsCount.IsEmpty ? "." : ConnectionsCount.ToString());                                           // Number of Connections
			request.Append(".");                                                                                                                                                            // Connection Point Exclusions
			request.Append(".");                                                                                                                                                            // Return Format
			request.Append(MinimumConnectionTime.ToString());                                                                                                   // Minimum Connection Time (hours)
			request.Append(CargoPassengerFlightOption.IsEmpty ? "." : CargoPassengerFlightOption.ToString());   // Cargo/Passenger Flights
			request.Append((EquipmentType.IsEmpty ? "." : EquipmentType.ToString()) + " ");                                     // Equipment Type (Wodeboyd/Freigther)
			request.Append((AirlineCode.IsEmpty ? "." : AirlineCode.ToString()) + " ");                                             // Airline
			request.Append(". ");                                                                                                                                                           // Equipment
			request.Append(". ");                                                                                                                                                           // Connection Point IATA Code
			request.Append(". ");                                                                                                                                                           // Max Circuity Limit
			request.Append(".");

			if (IncludeCO2EmissionValue)
			{
				request.Append(" C");   // CO2 Emission Value
			}

			return request.ToString();
		}

		#endregion

		#endregion
	}
}
