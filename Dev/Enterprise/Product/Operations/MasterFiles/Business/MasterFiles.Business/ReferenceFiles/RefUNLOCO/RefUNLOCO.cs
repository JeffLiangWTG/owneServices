using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(RefUNLOCO.Schema.RL_PortName)]
	[System.Diagnostics.DebuggerDisplay("UNLOCO = {RL_Code}")]
	public class RefUNLOCO : AutoRefUNLOCO,
		IRefUNLOCO,
		ILocationBiz,
		IDocManagerSupport
	{
		public RefUNLOCO(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fUtcDateTime = ZDateTime.UtcNow;
			fLocalDateTime = (ZDateTime)Env.Time.GetLocalTimeFromUtc(fUtcDateTime.ToDateTime());
		}

		public new class Schema : AutoRefUNLOCO.Schema
		{
			public const string CoordinateText = nameof(RefUNLOCO.CoordinateText);
			public const string CountryEconomicGroupDescription = nameof(RefUNLOCO.CountryEconomicGroupDescription);
			public const string HasDaylightSavingsZone = nameof(RefUNLOCO.HasDaylightSavingsZone);
			public const string IsInCurrentCompanysCountry = nameof(RefUNLOCO.IsInCurrentCompanysCountry);
			public const string IsInEU = nameof(RefUNLOCO.IsInEU);
			public const string StandardZoneUTCOffset = nameof(RefUNLOCO.StandardZoneUTCOffset);
			public const string StateDescription = nameof(RefUNLOCO.StateDescription);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RL_IsSystem = false;
		}

		#region Static Loaders

		public static RefUNLOCO GetPortFromNameAndCountryCode(BusinessObjectFactory factory, ZString portName, ZString countryCode)
		{
			return new RefUNLOCOLoader().GetPortFromNameAndCountryCode(factory, portName, countryCode);
		}

		public static RefUNLOCO GetPortFromNameAndCountryName(BusinessObjectFactory factory, ZString portName, ZString countryName)
		{
			return new RefUNLOCOLoader().GetPortFromNameAndCountryName(factory, portName, countryName);
		}

		public static RefUNLOCO LoadFromIATA(BusinessObjectFactory factory, ZString iataCode)
		{
			return new RefUNLOCOLoader().LoadFromIATA(factory, iataCode);
		}

		public static RefUNLOCO LoadFromLocalMap(BusinessObjectFactory factory, ZString localPortCode, ZString countryCode, ZString systemUsage)
		{
			return new RefUNLOCOLoader().LoadFromLocalMap(factory, localPortCode, countryCode, systemUsage);
		}

		public static RefUNLOCO LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, OrgHeader org)
		{
			return new RefUNLOCOLoader().LoadFromForeignCode(factory, foreignCode, org);
		}

		#endregion

		#region Loader
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefUNLOCO Load(string uNLOCO)
			{
				return (RefUNLOCO)Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, uNLOCO);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefUNLOCO);
			}
		}

		#endregion

		#region Properties

		public ZString CountryEconomicGroupDescription
		{
			get
			{
				if (Country == null)
				{ return ZString.Empty; } //CountryCode will be null when the country code is obsolete and removed from system, or the country is added by user and then deleted
				return new EconomicGroupList().GetDescriptionFromCode(Country.RN_EconomicGrouping);
			}
		}
		public ZPropertyInfo CountryEconomicGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CountryEconomicGroupDescription)); }
		}

		public ZBool IsInCurrentCompanysCountry
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == RL_RN_NKCountryCode; }
		}
		public ZPropertyInfo IsInCurrentCompanysCountryInfo
		{
			get { return GetZPropertyInfo(nameof(IsInCurrentCompanysCountry)); }
		}

		public ZBool IsInEU
		{
			get
			{
				if (Country == null)
				{ return false; } //CountryCode will be null when the country code is obsolete and removed from system, or the country is added by user and then deleted
				return Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion;
			}
		}
		public ZPropertyInfo IsInEUInfo
		{
			get { return GetZPropertyInfo(nameof(IsInEU)); }
		}

		#region RL_R3

		[List("Lookups+TimeZoneSets")]
		public override ZGuid RL_R3
		{
			get
			{
				return base.RL_R3;
			}
			set
			{
				base.RL_R3 = value;
			}
		}

		#endregion

		#region

		[ActionField(FieldType = ActionFieldType.Auto)]
		public override ZBool RL_IsUpdatable
		{
			get
			{
				return base.RL_IsUpdatable;
			}
			set
			{
				base.RL_IsUpdatable = value;
			}
		}

		#endregion

		#region RL_IsSystem

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool RL_IsSystem
		{
			get
			{
				return base.RL_IsSystem;
			}
			set
			{
				base.RL_IsSystem = value;
			}
		}

		#endregion

		#region RL_RW

		[List("Lookups.CountryStates")]
		public override ZGuid RL_RW
		{
			get
			{
				return base.RL_RW;
			}
			set
			{
				base.RL_RW = value;
			}
		}

		#endregion

		#region RL_Code

		[ReadOnlyMember(RefUNLOCOSchema.Constants.RL_IsSystem)]
		public override ZString RL_Code
		{
			get
			{
				return base.RL_Code;
			}
			set
			{
				base.RL_Code = value;
				if (value.Length > 2)
				{
					SetCountryCode(value.Substring(0, 2).ToUpper());
				}

				if (fRefLocoMaps != null && !RL_CodeInfo.HasErrors())
				{
					foreach (var locoMap in fRefLocoMaps.ToArray())
					{
						locoMap.RY_RL_NKLocoPort = value;
					}
					fRefLocoMaps.AdditionalFilter = new ZQuery(RefLocoMapSchema.RY_RL_NKLocoPort, RL_Code);
					fRefLocoMaps.RefreshBinding();
				}
			}
		}

		#endregion

		#region RL_PortName

		[ReadOnlyMember(nameof(NameShouldBeReadOnly))]
		public override ZString RL_PortName
		{
			get
			{
				return base.RL_PortName;
			}
			set
			{
				RL_NameWithDiacriticals = value;
				base.RL_PortName = value;
			}
		}

		protected bool NameShouldBeReadOnly
		{
			get { return GlbStaff.CurrentUser.GS_IsController ? (ZBool)false : RL_IsSystem; }
		}

		#endregion

		#region NameAndCountry

		public ZString NameAndCountry
		{
			get
			{
				if (Country != null)
				{
					return string.Concat(RL_PortName, ", ", Country.RN_Desc);
				}
				return RL_PortName;
			}
		}
		public ZPropertyInfo NameAndCountryInfo => GetZPropertyInfo(nameof(NameAndCountry));

		#endregion

		#region OrgRequiredLookups

		public static ZString LookupPortCodeFromCountryCityOrAUState(BusinessObjectFactory factory, ZString country, ZString city, ZString state)
		{
			return new PortCodeLoader().GenerateRequiredPortCode(factory, country, city, state);
		}

		#endregion

		#region RL_NameWithDiacriticals

		[ReadOnlyMember(nameof(NameShouldBeReadOnly))]
		public override ZString RL_NameWithDiacriticals
		{
			get { return base.RL_NameWithDiacriticals; }
			set { base.RL_NameWithDiacriticals = value; }
		}

		#endregion

		#region LocationDateTime

		public ZDateTime LocationDateTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (TimeZoneSet != null)
				{
					ITimeZone calculationTimeZone = TimeZoneSet.GetCalculationTimeZone();
					result = calculationTimeZone.ToLocalTime(fUtcDateTime.ToDateTime());
				}

				return result;
			}
		}

		public ZPropertyInfo LocationDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(LocationDateTime)); }
		}

		public ZDateTimeOffset LocationDateTimeOffset
		{
			get
			{
				var result = ZDateTimeOffset.Empty;

				if (TimeZoneSet != null)
				{
					var calculationTimeZone = TimeZoneSet.GetCalculationTimeZone();
					var zDateTime = calculationTimeZone.ToLocalTime(fUtcDateTime.ToDateTime());
					result = new ZDateTimeOffset(zDateTime, calculationTimeZone.GetUtcOffsetBasedOnUtc(fUtcDateTime.ToDateTime()));
				}

				return result;
			}
		}

		public ZPropertyInfo LocationDateTimeOffsetInfo
		{
			get { return GetZPropertyInfo(nameof(LocationDateTimeOffset)); }
		}

		#endregion

		#region LocalTime

		ZDateTime fUtcDateTime;
		ZDateTime fLocalDateTime;

		[BusinessObjectTestExclude]
		public ZDateTime LocalDateTime
		{
			get { return fLocalDateTime; }
			set
			{
				if (value.IsValid)
				{
					fLocalDateTime = value;
					fUtcDateTime = Env.Time.GetUtcFromLocalTime(fLocalDateTime.ToDateTime());
				}
				LocalDateTimeInfo.RefreshBinding();
				LocationDateTimeInfo.RefreshBinding();
				LocationDateTimeOffsetInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocalDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(LocalDateTime)); }
		}

		#endregion

		#region Has Daylight Savings Zone

		public ZBool HasDaylightSavingsZone
		{
			get { return TimeZoneSet != null && TimeZoneSet.DaylightSavingZone != null; }
		}

		public ZPropertyInfo HasDaylightSavingsZoneInfo
		{
			get { return GetZPropertyInfo(nameof(HasDaylightSavingsZone)); }
		}

		#endregion

		#region CurrentBranchCode

		public ZString CurrentBranchCode => GlbBranch.CurrentBranch.HomePort?.RL_Code ?? ZString.Empty;

		public ZPropertyInfo CurrentBranchCodeInfo => GetZPropertyInfo(nameof(CurrentBranchCode));

		#endregion

		#region StandardZoneUTCOffset

		public ZDecimal StandardZoneUTCOffset
		{
			get
			{
				ZDecimal result = 0;

				if (TimeZoneSet != null)
				{
					result = TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC / 60m;
				}

				return result;
			}
		}

		public ZPropertyInfo StandardZoneUTCOffsetInfo
		{
			get { return GetZPropertyInfo(Schema.StandardZoneUTCOffset); }
		}

		#endregion

		#region TimeZoneDescription

		public ZString TimeZoneDescription
		{
			get
			{
				if (TimeZoneSet != null)
				{
					var zone = TimeZoneSet.GetCalculationTimeZone();
					var utcOffset = zone.GetUtcOffsetBasedOnUtc(fUtcDateTime.ToDateTime());
					return string.Concat(
						TimeZoneSet.R3_TimeZoneSetName,
						" UTC",
						utcOffset < TimeSpan.Zero ? "-" : "+",
						utcOffset.ToString("hh\\:mm", CultureInfo.CurrentCulture));
				}
				return ZString.Empty;
			}
		}
		public ZPropertyInfo TimeZoneDescriptionInfo => GetZPropertyInfo(nameof(TimeZoneDescription));

		#endregion

		#region RL_RN_NKCountryCode

		[List("Lookups.Countries")]
		[ReadOnlyMember(RefUNLOCOSchema.Constants.RL_IsSystem)]
		public override ZString RL_RN_NKCountryCode
		{
			get { return base.RL_RN_NKCountryCode; }
			set
			{
				CheckMaximumLength(RL_RN_NKCountryCodeInfo, value);
				base.RL_RN_NKCountryCode = value;
			}
		}

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[MacroIgnore]
		public RefCountry CountryCode => Country;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return base.HumanReadableNameCore + " (" + RL_Code + ")";
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Country

		public RefCountry GetCountryFromCode(string code)
		{
			return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, code);
		}

		protected void SetCountryCode(string code)
		{
			RefCountry country = GetCountryFromCode(code);
			if (country != null)
			{
				RL_RN_NKCountryCode = country.Code;
			}
			else
			{
				RL_RN_NKCountryCode = ZString.Empty;
			}
		}

		#endregion

		#region RefLocoMaps

		[ChildEditable(true)]
		public RefLocoMapCollection RefLocoMaps
		{
			get
			{
				if (fRefLocoMaps == null)
				{
					fRefLocoMaps = new RefLocoMapCollection(this);
					fRefLocoMaps.AdditionalFilter = new ZQuery(RefLocoMapSchema.RY_RL_NKLocoPort, RL_Code);
					RegisterEditableChildObject(fRefLocoMaps);
				}
				return fRefLocoMaps;
			}
		}
		RefLocoMapCollection fRefLocoMaps;

		#endregion

		#region Published and Appointed Agents

		public OrgAddress GetPublishedAgent(ZString transportMode, ZString agentDirection)
		{
			return GetBestAgent(null, transportMode, agentDirection, new ZString[] { AgentStatusList.Codes.Published });
		}

		public OrgAddress GetBestAgent(ZString transportMode, ZString agentDirection)
		{
			return GetBestAgent(null, transportMode, agentDirection);
		}

		public OrgAddress GetBestAgent(OrgHeader header, ZString transportMode, ZString agentDirection, bool includeHandlesStatus = false)
		{
			var agentStatuses = includeHandlesStatus ? new ZString[] { AgentStatusList.Codes.Published, AgentStatusList.Codes.Appointed, AgentStatusList.Codes.Handles } : new ZString[] { AgentStatusList.Codes.Published, AgentStatusList.Codes.Appointed };

			return GetBestAgent(header, transportMode, agentDirection, agentStatuses);
		}

		OrgAddress GetBestAgent(OrgHeader header, ZString transportMode, ZString agentDirection, ZString[] agentStatuses)
		{
			OrgAppointedAgentPorts agentPort = null;

			if (CheckSupportedAgentDirection(agentDirection))
			{
				var transportModeColumns = GetColumnsByTransportMode(transportMode);
				if (transportModeColumns != null)
				{
					var filter = GetAgentFilter(header, agentDirection, agentStatuses, transportModeColumns);
					agentPort = Factory.LoadTop1<OrgAppointedAgentPorts>(filter);
				}
			}

			return agentPort == null ? null : agentPort.AgentOfficeAddress;
		}

		static SchemaStringColumn[] GetColumnsByTransportMode(ZString transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return new SchemaStringColumn[] { OrgAppointedAgentPortsSchema.O5_AirAgentStatus };

				case Core.Constants.TransportModes.Rail:
					return new SchemaStringColumn[] { OrgAppointedAgentPortsSchema.O5_RailAgentStatus };

				case Core.Constants.TransportModes.Road:
					return new SchemaStringColumn[] { OrgAppointedAgentPortsSchema.O5_RoadAgentStatus };

				case Core.Constants.TransportModes.Sea:
					return new SchemaStringColumn[] { OrgAppointedAgentPortsSchema.O5_SeaAgentStatus };

				case Core.Constants.TransportModes.All:
					return new SchemaStringColumn[] { OrgAppointedAgentPortsSchema.O5_AirAgentStatus, OrgAppointedAgentPortsSchema.O5_RailAgentStatus, OrgAppointedAgentPortsSchema.O5_RoadAgentStatus, OrgAppointedAgentPortsSchema.O5_SeaAgentStatus };

				default:
					return null;
			}
		}

		bool CheckSupportedAgentDirection(ZString direction)
		{
			switch (direction)
			{
				case AgentDirectionList.Codes.Export:
				case AgentDirectionList.Codes.Import:
					return true;
			}

			return false;
		}

		/// <summary>
		///		Generates the <see cref="ZQuery"/> instance which filters agents according to the arguments.
		/// </summary>
		/// <remarks>
		///		Priorities description:
		///
		///		Agent Statuses: Published (PUB), Appointed (APP), Handled (HAN), Gateway Agent (GTA), Gateway Agent with Tariff (GTT)
		///		Locations: Port (AUSYD, USLAX, ...), Country (AU, US, ...)
		///		Directions: Both (BTH), Export (EXP), Import (IMP)
		///
		///		Priorities from higher to lower:
		///			1. Port, Published, Import/Export
		///			2. Port, Published, Both
		///			3. Country, Published, Import/Export
		///			4. Country, Published, Both
		///			5. Port, Appointed, Import/Export
		///			6. Port, Appointed, Both
		///			7. Country, Appointed, Import/Export
		///			8. Country, Appointed, Both
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "OR statements over different columns cannot be written as an IN statement")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "OR statements over different columns cannot be written as an IN statement, May be SQL expression")]
		ZQuery GetAgentFilter(OrgHeader header, ZString agentDirection, IEnumerable<ZString> agentStatuses, SchemaStringColumn[] transportModeColumns)
		{
			var agentPortQuery = new ZDBOnlyQuery(typeof(OrgAppointedAgentPorts));

			agentPortQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_PortOrCountry, new string[] { RL_Code, RL_RN_NKCountryCode });
			agentPortQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_AgentDirection, new string[] { AgentDirectionList.Codes.Both, agentDirection });

			if (transportModeColumns.Length > 1)
			{
				ZQuery columns = new ZQuery();
				foreach (SchemaStringColumn column in transportModeColumns)
				{
					columns.AddToFilter(JoinCondition.Or, column, agentStatuses);
				}

				agentPortQuery.AddToFilter(columns);
			}
			else
			{
				agentPortQuery.AddToFilter(transportModeColumns[0], agentStatuses);
			}

			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAppointedAgentPortsSchema.O5_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsForwarder, true);

			if (header != null)
			{
				orgSubQuery.AddToFilter(OrgHeaderSchema.PK, header.PK);
			}

			agentPortQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			var columnsToOrderBy = new[]
			{
				string.Format(CultureInfo.InvariantCulture, (NoResString)"CASE {0} WHEN '{1}' THEN 1 WHEN '{2}' THEN 2 ELSE 3 END",
					transportModeColumns[0].Name,
					AgentStatusList.Codes.Published,
					AgentStatusList.Codes.Appointed
				),
				string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} DESC", OrgAppointedAgentPortsSchema.O5_PortOrCountry.Name),
				string.Format(CultureInfo.InvariantCulture, "{0} DESC", OrgAppointedAgentPortsSchema.O5_AgentDirection.Name)
			};

			agentPortQuery.OrderBy = string.Join(",", columnsToOrderBy);

			return agentPortQuery;
		}

		#endregion

		#region Location Coordinates

		ZGeography CreatePointSafe(double longitude, double latitude)
		{
			return longitude == 0 && latitude == 0 ? ZGeography.Empty : ZGeography.CreatePoint(longitude, latitude);
		}

		#region Latitude

		public ZDecimal Latitude
		{
			get => latitude ?? RL_GeoLocation.Latitude ?? 0;
			set
			{
				if (IsValidLatitude(value))
				{
					RL_GeoLocation = CreatePointSafe(RL_GeoLocation.Longitude ?? 0, (double)value);
					latitude = null;
				}
				else
				{
					latitude = value;
				}
				LatitudeInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
					RefreshBinding();
				}
			}
		}

		ZDecimal? latitude;

		public static bool IsValidLatitude(ZDecimal value)
		{
			return value <= 90 && value >= -90;
		}

		public ZPropertyInfo LatitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Latitude)); }
		}

		#endregion

		#region Longitude

		public ZDecimal Longitude
		{
			get => longitude ?? RL_GeoLocation.Longitude ?? 0;
			set
			{
				if (IsValidLongitude(value))
				{
					RL_GeoLocation = CreatePointSafe((double)value, RL_GeoLocation.Latitude ?? 0);
					longitude = null;
				}
				else
				{
					longitude = value;
				}
				LongitudeInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
					RefreshBinding();
				}
			}
		}

		ZDecimal? longitude;

		public static bool IsValidLongitude(ZDecimal value)
		{
			return value <= 180 && value >= -180;
		}

		public ZPropertyInfo LongitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Longitude)); }
		}

		#endregion

		public ZString CoordinateText
		{
			get
			{
				return RL_GeoLocation.IsEmpty ? string.Empty : string.Format(CultureInfo.InvariantCulture, "{0} {1}", RL_GeoLocation.Latitude, RL_GeoLocation.Longitude);
			}
		}

		public ZPropertyInfo CoordinateTextInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CoordinateText)); }
		}

		#endregion

		internal BusinessObjectFactory ReadOnlyFactory
		{
			get { return readOnlyFactory ?? (readOnlyFactory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory readOnlyFactory;

		#endregion

		#region ILocationBiz Members

		public ZString Code
		{
			get { return RL_Code; }
			set { RL_Code = value; }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return RL_CodeInfo; }
		}

		public ZString Description
		{
			get { return RL_PortName; }
			set { RL_PortName = value; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return RL_PortNameInfo; }
		}

		public ZBool IsActive
		{
			get { return RL_IsActive; }
		}

		public ZPropertyInfo IsActiveInfo
		{
			get { return RL_IsActiveInfo; }
		}

		RefCityTown ILocation.CityTown
		{
			get { return this.GetCityTown(RL_PortName, Factory); }
		}

		RefCountry ILocation.Country
		{
			get { return Country; }
		}

		RefCountryStates ILocation.State
		{
			get { return CountryStates; }
		}

		public ZString StateDescription
		{
			get { return CountryStates != null ? CountryStates.RW_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo StateDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StateDescription)); }
		}

		RefUNLOCO ILocation.UNLOCO
		{
			get { return this; }
		}

		public IATACityCode IATACityCode => IATACityCode.GetValidOrDefault(Factory, RL_IATARegionCode);

		RefZoneHeader[] ILocation.Zones
		{
			get
			{
				return Factory.GetCachedValue("RefUNLOCOZoneCollectionCache|" + Code, () =>
				{
					var result = new List<RefZoneHeader>();
					var unlocoZones = new RefUNLOCOZoneCollection(this);
					unlocoZones.Load();
					result.AddRange((RefZoneHeader[])unlocoZones.ToArray(typeof(RefZoneHeader)));

					if (Country != null)
					{
						foreach (var zone in ((ILocation)Country).Zones)
						{
							if (!result.Contains(zone))
							{
								result.Add(zone);
							}
						}
					}

					return result.ToArray();
				}, CacheStalenessPolicy.StaleWhenDataTableChanges(RefZoneHeaderSchema.Constants.TableName, Factory));
			}
		}

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			ILocation location = this;

			return location.Country != null && location.Country.ContainsUNLOCO(code);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.UNLOCO);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public ZBool IsInGreatBritain =>
			 (Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.UnitedKingdom && string.Compare(CountryStates?.RW_RegionName, Regions.NorthernIreland, true) != 0;

		public ZBool IsInNorthernIreland =>
			(Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.UnitedKingdom && string.Compare(CountryStates?.RW_RegionName, Regions.NorthernIreland, true) == 0;

		public bool IsInIcs2Zone =>
			(Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.UnitedKingdom
				? (bool)IsInNorthernIreland
				: (Country?.IsIcs2Member ?? false);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant here that matches data in the database")]
		public static class Regions
		{
			public const string NorthernIreland = "NORTHERN IRELAND";
		}
	}
}
