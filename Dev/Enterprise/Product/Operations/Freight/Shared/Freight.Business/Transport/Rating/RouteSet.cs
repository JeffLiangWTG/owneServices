namespace Enterprise.Freight.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using ZArchitecture.Schema;
	public class RouteSet
	{
		public RouteSet(BusinessObjectFactory factory, ZInt routeSetNumber)
		{
			this.routeSetNumber = routeSetNumber;
			this.transportLegs = new List<Transport>();
			this.factory = factory;
		}

		public RouteSet(BusinessObjectFactory factory, ZInt routeSetNumber, params Transport[] legs) : this(factory, routeSetNumber)
		{
			transportLegs.AddRange(legs);
		}

		readonly BusinessObjectFactory factory;
		ZInt routeSetNumber;
		readonly List<Transport> transportLegs;

		public ZInt RouteSetNumber
		{
			get { return routeSetNumber; }
			set { routeSetNumber = value; }
		}

		public IReadOnlyList<Transport> Legs => transportLegs.AsReadOnly();

		public int NumberOfLegs => transportLegs?.Count ?? 0;

		public ILocation Origin
		{
			get { return LocationHelper.GetLocationFromString(OriginUNLOCO, factory); }
		}

		public ILocation Destination
		{
			get { return LocationHelper.GetLocationFromString(DestinationUNLOCO, factory); }
		}

		public ILocation Via
		{
			get
			{
				ZString via = ZString.Empty;

				if (transportLegs.Count > 1)
				{
					var internationalVias = transportLegs
											.Where(x => x.JW_RL_NKLoadPort.SubstringSafe(0, 2) != OriginUNLOCO.SubstringSafe(0, 2)
													 && x.JW_RL_NKLoadPort.SubstringSafe(0, 2) != DestinationUNLOCO.SubstringSafe(0, 2))
											.Select(x => x.JW_RL_NKLoadPort)
											.ToArray();

					if (!internationalVias.Any())
					{
						var domesticVias = transportLegs
											.Where(x => x.JW_RL_NKLoadPort != OriginUNLOCO && x.JW_RL_NKLoadPort != DestinationUNLOCO)
											.Select(x => x.JW_RL_NKLoadPort)
											.ToArray();

						if (domesticVias.Any() && domesticVias.Length == 1)
						{
							via = domesticVias[0];
						}
					}
					else if (internationalVias.Length == 1)
					{
						via = internationalVias[0];
					}
				}

				return LocationHelper.GetLocationFromString(via, factory);
			}
		}

		public ZString TransportMode
		{
			get { return ReferenceLeg?.TransportMode ?? ZString.Empty; }
		}

		public OrgHeader Carrier
		{
			get { return ReferenceLeg?.Carrier; }
		}

		public ZDateTime ATA
		{
			get
			{
				var lastLeg = transportLegs.LastOrDefault();
				return lastLeg != null ? lastLeg.JW_ATA : ZDateTime.Invalid;
			}
		}

		public ZDateTime ETA
		{
			get
			{
				var lastLeg = transportLegs.LastOrDefault();
				return lastLeg != null ? lastLeg.JW_ETA : ZDateTime.Invalid;
			}
		}

		public ZDateTime ATD
		{
			get
			{
				var firstLeg = transportLegs.FirstOrDefault();
				return firstLeg != null ? firstLeg.JW_ATD : ZDateTime.Invalid;
			}
		}

		public ZDateTime ETD
		{
			get
			{
				var firstLeg = transportLegs.FirstOrDefault();
				return firstLeg != null ? firstLeg.JW_ETD : ZDateTime.Invalid;
			}
		}

		ZString OriginUNLOCO
		{
			get { return transportLegs.Any() ? transportLegs.First().JW_RL_NKLoadPort : ZString.Empty; }
		}

		ZString DestinationUNLOCO
		{
			get { return transportLegs.Any() ? transportLegs.Last().JW_RL_NKDiscPort : ZString.Empty; }
		}

		public bool TryAddTransportLegToRouteSet(Transport leg)
		{
			var result = false;

			if (!transportLegs.Any())
			{
				transportLegs.Add(leg);
				result = true;
			}
			else
			{
				var lastLeg = transportLegs.Last();

				if (CarrierBookingReferenceMatches(leg, lastLeg) && CountryMatches(leg, lastLeg) && ModeAndCarriersMatch(leg, lastLeg) && !SeaAndAirMixed(leg))
				{
					transportLegs.Add(leg);
					result = true;
				}
			}

			return result;
		}

		#region Rules

		bool CarrierBookingReferenceMatches(Transport leg, Transport lastLeg)
		{
			return lastLeg.JW_CarrierBookingReference == leg.JW_CarrierBookingReference;
		}

		bool CountryMatches(Transport leg, Transport lastLeg)
		{
			return lastLeg.JW_RL_NKDiscPort.SubstringSafe(0, 2) == leg.JW_RL_NKLoadPort.SubstringSafe(0, 2);
		}

		bool ModeAndCarriersMatch(Transport leg, Transport lastLeg)
		{
			return MainLeg != null ||
				   leg.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel ||
				   leg.JW_TransportMode == Core.Constants.TransportModes.Air ||
				   (lastLeg.JW_TransportMode == leg.JW_TransportMode && lastLeg.CarrierPK == leg.CarrierPK);
		}

		bool SeaAndAirMixed(Transport leg)
		{
			return (transportLegs.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Air) && leg.JW_TransportMode == Core.Constants.TransportModes.Sea) ||
				   (transportLegs.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea) && leg.JW_TransportMode == Core.Constants.TransportModes.Air);
		}

		#endregion

		public bool ContainsLeg(ZGuid legPK)
		{
			return transportLegs.Any(x => x.PK == legPK);
		}

		public Transport ReferenceLeg
		{
			get { return MainLeg ?? transportLegs.FirstOrDefault(); }
		}

		Transport MainLeg
		{
			get
			{
				return transportLegs.FirstOrDefault(x => x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel) ??
					   transportLegs.FirstOrDefault(x => x.JW_TransportMode == Core.Constants.TransportModes.Air);
			}
		}
	}

	public class RouteSets : List<RouteSet>
	{
		readonly RoutingCollection legsCollection;
		readonly BusinessObjectFactory factory;

		public RouteSets(BusinessObjectFactory factory, RoutingCollection legs)
		{
			this.legsCollection = legs;
			var fieldsToSubscibe = new[] { JobConsolTransportSchema.Constants.JW_CarrierBookingReference,
										   JobConsolTransportSchema.Constants.JW_VoyageFlight,
										   JobConsolTransportSchema.Constants.JW_RL_NKLoadPort,
										   JobConsolTransportSchema.Constants.JW_RL_NKDiscPort,
										   Transport.Schema.CarrierPK,
										   JobConsolTransportSchema.Constants.JW_TransportMode,
										   JobConsolTransportSchema.Constants.JW_TransportType };

			this.legsCollection.SubscribeToChildrenChanges(RecalculateRouteSets, fieldsToSubscibe);
			this.factory = factory;

			RecalculateRouteSets();
		}

		public ZInt GetRouteSetNumberForTransport(Transport leg)
		{
			return this.Where(x => x.ContainsLeg(leg.PK)).Select(x => x.RouteSetNumber).SingleOrDefault();
		}

		void RecalculateRouteSets()
		{
			if (!isRecalculateRouteSets)
			{
				isRecalculateRouteSets = true;

				var sortedLegs = this.legsCollection.Cast<Transport>().ToArray();
				this.Clear();

				if (sortedLegs.Any() && sortedLegs.All(x => !x.JW_RL_NKLoadPort.IsEmpty && !x.JW_RL_NKDiscPort.IsEmpty))
				{
					MovementLegComparer.SortMovementLegsByPorts(sortedLegs);

					RouteSet currentRouteSet = null;

					foreach (var leg in sortedLegs)
					{
						currentRouteSet = CreateOrUpdateRouteSet(currentRouteSet, leg);
						if (!this.Contains(currentRouteSet))
						{
							this.Add(currentRouteSet);
						}
					}
				}

				foreach (var t in this.legsCollection.Cast<Transport>().ToArray())
				{
					t.RouteSetNumberInfo.RefreshBinding();
				}

				isRecalculateRouteSets = false;
			}
		}

		bool isRecalculateRouteSets;

		RouteSet CreateOrUpdateRouteSet(RouteSet currentRouteSet, Transport leg)
		{
			if (currentRouteSet == null)
			{
				return new RouteSet(factory, 1, leg);
			}
			else
			{
				if (currentRouteSet.TryAddTransportLegToRouteSet(leg))
				{
					return currentRouteSet;
				}
				else
				{
					return new RouteSet(factory, currentRouteSet.RouteSetNumber + 1, leg);
				}
			}
		}
	}

	public class RouteSetJobDatesProvider : JobDatesProvider<RouteSet>
	{
		public RouteSetJobDatesProvider(RouteSet routeSet, IRoutingSupport routeSetParent)
			: base(routeSet)
		{
			this.routeSetParent = routeSetParent;
			parentJobDatesProvider = (routeSetParent as IRatingSupporterWithAdapter)?.RatingAdapter?.JobDatesProvider;
		}
		readonly IRoutingSupport routeSetParent;
		readonly IJobDatesProvider parentJobDatesProvider;

		protected override ZDateTime GetArrivalDateCore()
		{
			var result = ZDateTime.Empty;
			if (Parent != null)
			{
				result = Parent.ATA.IsValid ? Parent.ATA : Parent.ETA.IsValid ? Parent.ETA : ZDateTime.Empty;
			}

			return result.IsEmpty
				? parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.ArrivalDate) ?? base.GetArrivalDateCore()
				: result;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			var result = ZDateTime.Empty;
			if (Parent != null)
			{
				result = Parent.ATD.IsValid ? Parent.ATD : Parent.ETD.IsValid ? Parent.ETD : ZDateTime.Empty;
			}

			return result.IsEmpty
				? parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.DepartureDate) ?? base.GetDepartureDateCore()
				: result;
		}

		protected override ZDateTime GetCFSReceivalStartDateCore()
		{
			var result = ZDateTime.Empty;
			var transport = routeSetParent.TransportsIncludingRelated.RouteSets
				.Where(x => !x.ReferenceLeg.IsDeleted)
				.Select(x => x.ReferenceLeg)
				.FirstOrDefault();

			if (transport != null)
			{
				result = transport.JW_DepotReceivalCommences;
			}

			return result.IsEmpty
				? parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate) ?? base.GetCFSReceivalStartDateCore()
				: result;
		}

		protected override ZDateTime GetEstimatedArrivalDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate) ?? base.GetEstimatedArrivalDateCore();

		protected override ZDateTime GetEstimatedDepartureDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate) ?? base.GetEstimatedDepartureDateCore();

		protected override ZDateTime GetAWBIssueDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate) ?? base.GetAWBIssueDateCore();

		protected override ZDateTime GetCustomsClearanceDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate) ?? base.GetCustomsClearanceDateCore();

		protected override ZDateTime GetCustomsClearanceDateByDirectionCore(string direction) =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, direction) ?? base.GetCustomsClearanceDateByDirectionCore(direction);

		protected override ZDateTime GetPickupDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.PickupDate) ?? base.GetPickupDateCore();

		protected override ZDateTime GetDeliveryDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.DeliveryDate) ?? base.GetDeliveryDateCore();

		protected override ZDateTime GetVesselArrivalDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate) ?? base.GetVesselArrivalDateCore();

		protected override ZDateTime GetVesselDepartureDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate) ?? base.GetVesselDepartureDateCore();

		protected override ZDateTime GetHouseBillIssueDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.HouseBillIssueDate) ?? base.GetHouseBillIssueDateCore();

		protected override ZDateTime GetJobOpenDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.JobOpenDate) ?? base.GetJobOpenDateCore();

		protected override ZDateTime GetFirstContainerGateInDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate) ?? base.GetFirstContainerGateInDateCore();

		protected override ZDateTime GetLastContainerGateInDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate) ?? base.GetLastContainerGateInDateCore();

		protected override ZDateTime GetHBLPlaceOfReceiptArrivalDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate) ?? base.GetHBLPlaceOfReceiptArrivalDateCore();

		protected override ZDateTime GetCostingAutoratingDateOverrideCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride) ?? base.GetCostingAutoratingDateOverrideCore();

		protected override ZDateTime InterimReceiptDateCore() =>
			parentJobDatesProvider?.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate) ?? base.InterimReceiptDateCore();
	}
}
