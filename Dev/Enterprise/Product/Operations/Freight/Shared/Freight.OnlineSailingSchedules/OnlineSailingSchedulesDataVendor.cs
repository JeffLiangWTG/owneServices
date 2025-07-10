using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class OnlineSailingSchedulesDataVendor : SailingScheduleDataVendor,
		Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor
	{
		protected override bool IsEnabledCore
		{
			get { return FreightDataRegistry.Instance.EnableScheduleFeedService.Value; }
		}

		protected override bool IsVendorDataCurrentCore
		{
			get { return true; }
		}

		public override string Status
		{
			get { return string.Empty; }
		}

		protected override void UpdateVoyageOriginCore(VoyageOrigin origin)
		{
			if (origin.JA_E_DEP.IsDefault && !origin.IsInDatabase)
			{
				var bestMatchingRoute = FindBestMatchingRoute(origin);

				if (bestMatchingRoute != null && bestMatchingRoute.Departure != origin.JA_E_DEP)
				{
					origin.JA_E_DEP = bestMatchingRoute.Departure;
				}
			}
		}

		protected override void UpdateVoyageDestinationCore(VoyageDestination destination)
		{
			if (destination.JB_E_ARV.IsDefault && !destination.IsInDatabase)
			{
				var bestMatchingRoute = FindBestMatchingRoute(destination);

				if (bestMatchingRoute != null && bestMatchingRoute.Arrival != destination.JB_E_ARV)
				{
					destination.JB_E_ARV = bestMatchingRoute.Arrival;
				}
			}
		}

		protected override SailingInformation TryFindSailingIncludingRelatedPortsCore(VoyageOrigin origin, VoyageDestination destination)
		{
			var routeForOrigin = FindBestMatchingRoute(origin, true);

			if (routeForOrigin == null)
			{
				return null;
			}

			var routeForDestination = FindBestMatchingRoute(destination, true);

			if (routeForDestination == null)
			{
				return null;
			}

			var updatedSailingInfo = new SailingInformation()
			{
				Load = routeForOrigin.OriginPortUnloco,
				ETD = routeForOrigin.Departure,
				Discharge = routeForDestination.DestinationPortUnloco,
				ETA = routeForDestination.Arrival
			};

			return updatedSailingInfo;
		}

		protected virtual Route FindBestMatchingRoute(VoyageOrigin origin, bool includeRelatedPorts = false)
		{
			if (origin.PortOfLoading == null || !VoyageHasRequiredData(origin.Voyage))
			{
				return null;
			}

			var serviceRequest = CreateServiceRequest(origin.Voyage);
			if (serviceRequest == null)
			{
				return null;
			}

			serviceRequest.LoadPort = origin.JA_RL_NKPortOfLoading;
			var validateResult = serviceRequest.Validate();

			if (!string.IsNullOrWhiteSpace(validateResult))
			{
				return null;
			}

			var searchParams = UrlHelper.ConvertToParams(serviceRequest);
			var matchingRoutes = TryRetrieveFromLocalCache(origin.Factory, searchParams);

			if (matchingRoutes == null)
			{
				matchingRoutes = TryRetrieveMatchingRoutes(origin.Factory, searchParams);
				UpdateLocalCache(origin.Factory, searchParams, matchingRoutes);
			}

			var refinedRoutes = matchingRoutes.Where(route => route.OriginPortUnloco == origin.JA_RL_NKPortOfLoading).ToArray();

			if (includeRelatedPorts && refinedRoutes.Length == 0)
			{
				refinedRoutes = matchingRoutes;
			}

			return refinedRoutes.OrderBy(route => route.Departure).FirstOrDefault();
		}

		protected virtual Route FindBestMatchingRoute(VoyageDestination destination, bool includeRelatedPorts = false)
		{
			if (destination.PortOfDischarge == null || !VoyageHasRequiredData(destination.Voyage))
			{
				return null;
			}

			var serviceRequest = CreateServiceRequest(destination.Voyage);
			if (serviceRequest == null)
			{
				return null;
			}

			serviceRequest.DischargePort = destination.JB_RL_NKPortOfDischarge;
			var validateResult = serviceRequest.Validate();

			if (!string.IsNullOrWhiteSpace(validateResult))
			{
				return null;
			}

			var searchParams = UrlHelper.ConvertToParams(serviceRequest);
			var matchingRoutes = TryRetrieveFromLocalCache(destination.Factory, searchParams);

			if (matchingRoutes == null)
			{
				matchingRoutes = TryRetrieveMatchingRoutes(destination.Factory, searchParams);
				UpdateLocalCache(destination.Factory, searchParams, matchingRoutes);
			}

			var refinedRoutes = matchingRoutes.Where(route => route.DestinationPortUnloco == destination.JB_RL_NKPortOfDischarge).ToArray();

			if (includeRelatedPorts && refinedRoutes.Length == 0)
			{
				refinedRoutes = matchingRoutes;
			}

			return refinedRoutes.OrderBy(route => route.Arrival).FirstOrDefault();
		}

		#region Implementation

		bool VoyageHasRequiredData(JobVoyage voyage)
		{
			return
				voyage != null
				&& voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea
				&& !voyage.JV_VoyageFlight.IsEmpty
				&& !voyage.JV_RV_NKVessel.IsEmpty;
		}

		OnlineSchedulesFilterRequest CreateServiceRequest(JobVoyage voyage)
		{
			ZString carrierSCAC = voyage.Line != null
				? voyage.Line.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates)
				: ZString.Empty;

			if (string.IsNullOrEmpty(carrierSCAC))
			{
				return null;
			}

			ZString lloydsNumber = voyage.Vessel != null
				? voyage.Vessel.RV_LloydsNumber
				: ZString.Empty;

			return new OnlineSchedulesFilterRequest()
			{
				VoyageNumber = voyage.JV_VoyageFlight,
				VesselName = voyage.JV_RV_NKVessel,
				ImoNumber = lloydsNumber,
				CarrierCode = carrierSCAC,
				LegsCount = "1", //TODO: Should be removed in later stages, when (if) we would know for sure how to process multiple legs
				IncludeRelatedPorts = true.ToString()
			};
		}

		Route[] TryRetrieveMatchingRoutes(BusinessObjectFactory factory, string searchParams)
		{
			var routesProvider = GetRoutesProvider(factory);
			var notifications = new NotificationBuffer();
			var routes = routesProvider.GetRoutes(searchParams, new AutoInitiatedServiceRequestManager(notifications));

			return routes ?? System.Array.Empty<Route>();
		}

		protected virtual IRoutesProvider GetRoutesProvider(BusinessObjectFactory factory)
		{
			return new RoutesProvider(factory);
		}

		#region Local route cache

		Dictionary<string, Route[]> GetLocalCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OnlineSailingSchedulesDataVendorRouteCache", () => new Dictionary<string, Route[]>());
		}

		Route[] TryRetrieveFromLocalCache(BusinessObjectFactory factory, string searchParams)
		{
			GetLocalCache(factory).TryGetValue(searchParams, out Route[] result);

			return result;
		}

		void UpdateLocalCache(BusinessObjectFactory factory, string searchParams, Route[] matchingRoutes)
		{
			GetLocalCache(factory)[searchParams] = matchingRoutes;
		}

		#endregion

		#endregion
	}
}
