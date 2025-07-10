using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class Route : NonPersistentBusinessObject
	{
		public Route(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
		}

		#region Schema

		public static class Schema
		{
			public const string OriginPortUnloco = "OriginPortUnloco";
			public const string OriginPortName = "OriginPortName";
			public const string DestinationPortUnloco = "DestinationPortUnloco";
			public const string DestinationPortName = "DestinationPortName";
			public const string Departure = "Departure";
			public const string Arrival = "Arrival";
			public const string CarrierSCAC = "CarrierSCAC";
			public const string CarrierCode = "CarrierCode";
			public const string LegsCount = "LegsCount";
			public const string TransitTime = "TransitTime";
			public const string IsGateway = "IsGateway";
		}

		#endregion

		#region Business Object Overrides

		public RouteValidation Validation => new RouteValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("15cdf530-294e-4b7a-bcdf-8e1f3bed66f4", "Sailing Schedule"); }
		}

		#endregion

		#region Properties

		#region OriginPort

		[ReadOnly(true)]
		public ZString OriginPortUnloco
		{
			get { return originPortUnloco; }
			private set
			{
				SetNonPersistentPropertyValue(OriginPortUnlocoInfo, ref originPortUnloco, value);
			}
		}
		ZString originPortUnloco;

		public ZPropertyInfo OriginPortUnlocoInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OriginPortUnloco); }
		}

		public ZString OriginPortName
		{
			get { return OriginPort == null ? ZString.Empty : OriginPort.RL_PortName; }
		}

		#endregion

		#region DestinationPort

		[ReadOnly(true)]
		public ZString DestinationPortUnloco
		{
			get { return destinationPortUnloco; }
			private set
			{
				SetNonPersistentPropertyValue(DestinationPortUnlocoInfo, ref destinationPortUnloco, value);
			}
		}
		ZString destinationPortUnloco;

		public ZPropertyInfo DestinationPortUnlocoInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.DestinationPortUnloco); }
		}

		public ZString DestinationPortName
		{
			get { return DestinationPort == null ? ZString.Empty : DestinationPort.RL_PortName; }
		}

		#endregion

		#region Departure

		[ReadOnly(true)]
		public ZDateTime Departure
		{
			get { return departure; }
			private set
			{
				SetNonPersistentPropertyValue(DepartureInfo, ref departure, value);
				Validation.ValidateDeparture();
			}
		}
		ZDateTime departure;

		public ZPropertyInfo DepartureInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Departure); }
		}

		#endregion

		#region Arrival

		[ReadOnly(true)]
		public ZDateTime Arrival
		{
			get { return arrival; }
			private set
			{
				SetNonPersistentPropertyValue(ArrivalInfo, ref arrival, value);
				Validation.ValidateArrival();
			}
		}
		ZDateTime arrival;

		public ZPropertyInfo ArrivalInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Arrival); }
		}

		#endregion

		#region CarrierSCAC

		[ReadOnly(true)]
		public ZString CarrierSCAC
		{
			get { return carrierSCAC; }
			private set
			{
				SetNonPersistentPropertyValue(CarrierSCACInfo, ref carrierSCAC, value);
				Validation.ValidateCarrierSCAC();
			}
		}
		ZString carrierSCAC;

		public ZPropertyInfo CarrierSCACInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.CarrierSCAC); }
		}

		#endregion

		#region CarrierCode

		[ReadOnly(true)]
		public ZString CarrierCode
		{
			get { return Carrier == null ? ZString.Empty : Carrier.OH_Code; }
		}

		#endregion

		#region  LegsCount

		[ReadOnly(true)]
		public ZInt LegsCount
		{
			get { return legsCount; }
			private set { SetNonPersistentPropertyValue(LegsCountInfo, ref legsCount, value); }
		}
		ZInt legsCount;

		public ZPropertyInfo LegsCountInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.LegsCount); }
		}

		#endregion

		#region Transit Time

		[ReadOnly(true)]
		public ZInt TransitTime
		{
			get { return transitTime; }
			private set { SetNonPersistentPropertyValue(TransitTimeInfo, ref transitTime, value); }
		}
		ZInt transitTime;

		public ZPropertyInfo TransitTimeInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.TransitTime); }
		}

		#endregion

		#region IsGateway

		[ReadOnly(true)]
		public ZBool IsGateway
		{
			get
			{
				return Legs.Cast<Leg>()
					.Where(leg => !leg.CarrierSCAC.IsEmpty)
					.GroupBy(leg => leg.CarrierSCAC)
					.Count() > 1;
			}
		}

		#endregion

		#region Co2eKgPerTeu

		[ReadOnly(true)]
		[DecimalPlaces(3)]
		public ZDecimal Co2eKgPerTeu
		{
			get
			{
				if (Legs.Cast<Leg>().Any(leg => leg.Co2eKgPerTeu == ZDecimal.Zero))
				{
					return ZDecimal.Zero;
				}
				return Legs.Cast<Leg>().Sum(leg => leg.Co2eKgPerTeu);
			}
		}

		#endregion

		#region Co2eKgPerTonne

		[ReadOnly(true)]
		[DecimalPlaces(3)]
		public ZDecimal Co2eKgPerTonne
		{
			get
			{
				if (Legs.Cast<Leg>().Any(leg => leg.Co2eKgPerTonne == ZDecimal.Zero))
				{
					return ZDecimal.Zero;
				}
				return Legs.Cast<Leg>().Sum(leg => leg.Co2eKgPerTonne);
			}
		}

		#endregion

		#region Legs

		[ChildEditable]
		public LegsCollection Legs
		{
			get
			{
				if (legs == null)
				{
					legs = new LegsCollection(Factory);
					RegisterEditableChildObject(legs);
				}

				return legs;
			}
		}
		LegsCollection legs;

		#endregion

		#endregion

		public RefUNLOCO OriginPort
		{
			get
			{
				return string.IsNullOrEmpty(OriginPortUnloco)
					? null
					: Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, OriginPortUnloco)).FirstOrDefault();
			}
		}

		public RefUNLOCO DestinationPort
		{
			get
			{
				return string.IsNullOrEmpty(DestinationPortUnloco)
					? null
					: Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, DestinationPortUnloco)).FirstOrDefault();
			}
		}

		public OrgHeader Carrier
		{
			get
			{
				var orgCusCodes = CarrierSCACHelper.GetExistingOrgCustCodes(carrierSCAC, Factory);
				return orgCusCodes.Length == 1 ? orgCusCodes[0].Header : null;
			}
		}

		public void SetValues(ServiceModel.Route route)
		{
			Argument.NotNull(route, "route");
			Argument.NotNull(route.Carrier, "route.Carrier");
			Argument.NotNull(route.Legs, "route.Legs");

			var firstLeg = route.Legs
				.FirstOrDefault();
			var lastLeg = route.Legs
				.LastOrDefault();

			if (firstLeg == null || lastLeg == null)
			{
				throw new InvalidOperationException("Route must have at least one leg.");
			}

			OriginPortUnloco = firstLeg.LoadPort.Unloco;
			Departure = firstLeg.Etd ?? ZDateTime.Empty;

			DestinationPortUnloco = lastLeg.DischargePort.Unloco;
			Arrival = lastLeg.Eta ?? ZDateTime.Empty;

			CarrierSCAC = route.Carrier.Code;
			LegsCount = route.Legs.Length;
			TransitTime = route.TransitTime;

			foreach (var serviceLeg in route.Legs)
			{
				var leg = new Leg(Factory);
				leg.SetValues(serviceLeg);
				Legs.Add(leg);
			}
		}

		public Route Clone(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			var route = new Route(factory);

			using (route.GetValidationSuspender())
			using (route.SuspendSettingHasChanges())
			{
				route.OriginPortUnloco = OriginPortUnloco;
				route.DestinationPortUnloco = DestinationPortUnloco;
				route.Departure = Departure;
				route.Arrival = Arrival;
				route.CarrierSCAC = CarrierSCAC;
				route.LegsCount = LegsCount;
				route.TransitTime = TransitTime;

				foreach (Leg leg in Legs)
				{
					if (leg.IsSea)
					{
						route.Legs.Add(leg.Clone(factory));
					}
				}
			}

			return route;
		}

		#region Link Transports

		public void MergeWithTransports(Transport currentTransport, ITransportCollection transportCollectionForExtraLegs)
		{
			var firstLeg = Legs.Cast<Leg>().FirstOrDefault();

			if (firstLeg != null)
			{
				if (currentTransport == null && transportCollectionForExtraLegs != null)
				{
					currentTransport = transportCollectionForExtraLegs.AddNew() as Transport;
				}

				if (currentTransport == null)
				{
					return;
				}

				SetTransportData(currentTransport, firstLeg);

				if (transportCollectionForExtraLegs != null && Legs.Count > 1)
				{
					foreach (var extraLeg in Legs.Skip(1).Cast<Leg>())
					{
						if (extraLeg.OriginPortUnloco == extraLeg.DestinationPortUnloco)
						{
							continue;
						}

						var extraTransport = transportCollectionForExtraLegs.AddNew() as Transport;
						if (extraTransport != null)
						{
							SetTransportData(extraTransport, extraLeg);
						}
					}
				}

				UpdateConsolCarrierAndCoLoad(currentTransport.Parent);
				UpdateConsolCarrierContractAndAllocationDetails(currentTransport.Parent);
			}

			transportCollectionForExtraLegs.UpdateTransportTypes(true);
		}

		void UpdateConsolCarrierContractAndAllocationDetails(ITransportParentCommon parent)
		{
			var routeSelector = parent.Factory.GetValue<IMultiAllocationRouteSelectorProvider>();
			var overrideDialog = parent.Factory.GetValue<IOverrideAllocationRouteDialogProvider>();
			if (routeSelector == null || overrideDialog == null || parent is not CommonConsol consol)
			{
				return;
			}

			var allSailingPKs = Legs.Select(leg => leg.FindMatchingJobSailing()).WhereNotNull().Select(sailing => sailing.PK).ToArray();
			if (allSailingPKs.Length == 0)
			{
				return;
			}

			var allocationRouteQuery = new ZQuery(RatingContractAllocationLineSchema.RCA_JX_SailingSchedule, allSailingPKs);
			var allocationRoutes = Factory.Load<IRatingContractAllocationLine>(allocationRouteQuery);

			if (allocationRoutes.Length == 0)
			{
				return;
			}

			var routeAssignable = parent as IAllocationRouteAssignable;

			if (allocationRoutes.Length == 1)
			{
				var route = allocationRoutes[0];

				var currentAllocationRoute = Factory.Load<IRatingContractAllocationLine>(consol.JK_RCA_AllocationLine);
				var consolAllocationLineID = currentAllocationRoute?.RCA_AllocationLineID ?? ZString.Empty;

				if (!routeAssignable.HasCarrierOrRouteDifferentToOverride(route) || overrideDialog.PromptUserForConfirmingOverride(route, routeAssignable))
				{
					routeAssignable.UpdateCarrierContractAndAllocationDetails(route);
				}
				return;
			}

			var isAssignedRoute = allocationRoutes.Any(routeAssignable.IsAssignedAllocationRoute);

			if (!isAssignedRoute)
			{
				routeSelector.PromptUserForSelectingAllocationRoute(routeAssignable, allocationRoutes);
			}
		}

		void UpdateConsolCarrierAndCoLoad(ITransportParentCommon transportParent)
		{
			if (transportParent is CommonConsol commonConsol)
			{
				if (Carrier == null)
				{
					ErrorReporter.ReportOnce("ImportRouteCarrierNotResolved", $"Could not resolve carrier '{CarrierSCAC}'");
					return;
				}

				if (commonConsol.IsCoLoad && Carrier.OH_IsSeaWholesaler)
				{
					var firstSeaLeg = Legs.Cast<Leg>().FirstOrDefault(leg => leg.IsSea);
					if (firstSeaLeg != null)
					{
						if (firstSeaLeg.Carrier == null)
						{
							ErrorReporter.ReportOnce("ImportRouteCarrierNotResolved", $"Could not resolve carrier '{firstSeaLeg.CarrierSCAC}'");
							return;
						}

						SetShippingLineAddress(commonConsol, firstSeaLeg.Carrier, firstSeaLeg.CarrierSCAC);
					}

					if (GetSCAC(commonConsol.Creditor) != CarrierSCAC)
					{
						commonConsol.CreditorPK = Carrier?.PK ?? ZGuid.Empty;
					}
				}
				else
				{
					SetShippingLineAddress(commonConsol, Carrier, CarrierSCAC);
				}
			}
		}

		static void SetShippingLineAddress(CommonConsol commonConsol, OrgHeader orgHeader, ZString orgSCAC)
		{
			if (GetSCAC(commonConsol.ShippingLine) != orgSCAC)
			{
				commonConsol.SetDefaultShippingLineAddress(orgHeader);
			}
		}

		static ZString GetSCAC(OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				return ZString.Empty;
			}

			return orgHeader.CustomsCodes.GetCustomsRegNo(
				OrgCusCode.CodeTypes.CarrierCode,
				Core.Constants.CountryCodes.UnitedStates);
		}

		void SetTransportData(Transport transport, Leg leg)
		{
			transport.JW_TransportMode = MapTransportMode(leg.LegType, leg.VesselName);

			if (leg.LegType == GssConstants.WaterLegType)
			{
				transport.IsFeeder = true;
			}

			if (transport.IsSea && !transport.IsFeeder)
			{
				var sailing = leg.FindMatchingJobSailing();

				if (sailing != null)
				{
					transport.JW_IsLinked = true;
					transport.JW_JX = sailing.PK; // causes JW_ServiceString to be set from the sailing
					transport.RedefaultCreditor(shouldOverrideExistingCreditor: true);
					return;
				}
			}

			transport.JW_RL_NKLoadPort = leg.OriginPortUnloco;
			transport.JW_RL_NKDiscPort = leg.DestinationPortUnloco;
			transport.JW_VoyageFlight = leg.VoyageCode;
			transport.CarrierPK = leg.Carrier?.PK ?? ZGuid.Empty;
			transport.JW_Vessel = leg.VesselName;
			transport.JW_ETD = leg.Departure;
			transport.JW_ETA = leg.Arrival;
			transport.JW_IsLinked = false;

			if (transport.IsSea)
			{
				transport.JW_ServiceString = leg.TradeLaneName;
			}
		}

		string MapTransportMode(string externalTransportMode, string vesselName)
		{
			if (string.Equals(externalTransportMode, GssConstants.SeaLegType, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(externalTransportMode, GssConstants.FeederLegType, StringComparison.OrdinalIgnoreCase))
			{
				return Enterprise.Core.Constants.TransportModes.Sea;
			}

			if (string.Equals(externalTransportMode, GssConstants.WaterLegType, StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(vesselName) && VesselNameSuggestsInlandWaterwayTransportMode(vesselName))
				{
					return Enterprise.Core.Constants.TransportModes.InlandWaterwayTransport;
				}

				return Enterprise.Core.Constants.TransportModes.Sea;
			}

			if (string.Equals(externalTransportMode, GssConstants.RailLegType, StringComparison.OrdinalIgnoreCase))
			{
				return Enterprise.Core.Constants.TransportModes.Rail;
			}

			if (string.Equals(externalTransportMode, GssConstants.RoadLegType, StringComparison.OrdinalIgnoreCase))
			{
				return Enterprise.Core.Constants.TransportModes.Road;
			}

			return externalTransportMode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		static bool VesselNameSuggestsInlandWaterwayTransportMode(string vesselName)
		{
			return Regex.IsMatch(vesselName, @"\bBARGE\b", RegexOptions.IgnoreCase)
				|| string.Equals(vesselName, "WATER", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(vesselName, "INLAND WATERWAY", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(vesselName, "COMBINED WATERWAY", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
