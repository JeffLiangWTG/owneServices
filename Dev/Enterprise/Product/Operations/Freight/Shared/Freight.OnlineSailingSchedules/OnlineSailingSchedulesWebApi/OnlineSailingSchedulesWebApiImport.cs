using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using static Enterprise.Freight.OnlineSailingSchedules.GSSApiHelpers;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	class OnlineSailingSchedulesWebApiImport
	{
		readonly BusinessObjectFactory factory;
		readonly IImportGSSPayload importGSSPayload;
		OnlineSchedules onlineSchedules;

		public OnlineSailingSchedulesWebApiImport(IImportGSSPayload importGSSPayload, OnlineSchedules onlineSchedules = null)
		{
			this.importGSSPayload = importGSSPayload;
			factory = onlineSchedules?.Factory ?? new BusinessObjectFactory();
			this.onlineSchedules = onlineSchedules ?? new OnlineSchedules(factory, new RoutesProvider(factory));
		}

		public IImportGSSResponse Import()
		{
			if (!TryLoadRouteFromGSSAndCheckValidations(out var loadRoutesAndCheckValidationError, out var route))
			{
				return loadRoutesAndCheckValidationError;
			}

			onlineSchedules.CreateEnterpriseVoyages();

			var allSailings = route.Legs.Select(leg => leg.FindMatchingJobSailing(true)).ToList();
			var newSailings = allSailings.Where(sailing => sailing != null && !sailing.IsInDatabase).ToList();

			factory.Save();

			return BuildSuccessResult(allSailings, newSailings);
		}

		#region TryLoadRoutesFromGSSAndCheckValidations

		/// Load routes from the API and filter in memory based on the natural key
		bool TryLoadRouteFromGSSAndCheckValidations(out IImportGSSResponse error, out Route route)
		{
			route = null;
			error = null;
			onlineSchedules.Request = BuildFilterRequest();
			var loadRoutesNotifications = new NotificationBuffer();
			onlineSchedules.LoadRoutes(loadRoutesNotifications);
			if (loadRoutesNotifications.HasErrors)
			{
				error = Error(ImportGSSResponseCode.LoadRoutesValidation, Res.GetString("6778b618-dafb-baad-4dd0-ff4fbc3975c0", "Load routes errors"), loadRoutesNotifications.Events.Select(e => e.Message));
				return false;
			}

			var routes = onlineSchedules.Routes.Where(AllLegsMatch).ToArray();
			var originalMatchCount = onlineSchedules.Routes.Count;

			if (routes.Length != 1)
			{
				error = Error(
					ImportGSSResponseCode.ParameterError,
					Res.GetString("ea3bb706-ad3c-d588-4719-c8fbae869822", "Expected 1 route to match the criteria, {0} routes matched ({1} matched before filtering by connections).", onlineSchedules.Routes.Count, originalMatchCount)
				);
				return false;
			}

			route = routes[0];
			onlineSchedules = onlineSchedules.CloneSelectedSchedules(routes, factoryToUse: factory);
			onlineSchedules.RunPreSaveValidation();
			if (onlineSchedules.HasErrors)
			{
				error = Error(ImportGSSResponseCode.OnlineSchedulesValidation, Res.GetString("c0224863-4e0d-6780-4e94-efefb11a2d64", "Online Schedules validation errors"), GetOnlineSchedulesErrors(onlineSchedules));
				return false;
			}

			return true;
		}

		OnlineSchedulesFilterRequest BuildFilterRequest()
		{
			var firstLeg = importGSSPayload.GSSNaturalKey.Legs.First();
			var lastLeg = importGSSPayload.GSSNaturalKey.Legs.Last();

			return new OnlineSchedulesFilterRequest()
			{
				LoadPort = firstLeg.Origin,
				DischargePort = lastLeg.Destination,
				EtdFrom = firstLeg.Departure.AddDays(-2).ToString("yyyy-MM-dd"),
				EtdTo = firstLeg.Departure.AddDays(2).ToString("yyyy-MM-dd"),
				EtaFrom = lastLeg.Arrival.AddDays(-2).ToString("yyyy-MM-dd"),
				EtaTo = lastLeg.Arrival.AddDays(2).ToString("yyyy-MM-dd"),
				CarrierCode = importGSSPayload.GSSNaturalKey.CarrierSCAC
			};
		}

		bool AllLegsMatch(Route route)
		{
			if (route.Legs.Count != importGSSPayload.GSSNaturalKey.Legs.Count())
			{
				return false;
			}

			for (var i = 0; i < route.Legs.Count; i++)
			{
				if (!LegMatches(route.Legs[i], importGSSPayload.GSSNaturalKey.Legs.ElementAt(i)))
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		static bool LegMatches(Leg leg, IConnectionNaturalKey connectionNaturalKey)
		{
			return leg.OriginPortUnloco == connectionNaturalKey.Origin
				&& leg.DestinationPortUnloco == connectionNaturalKey.Destination
				&& leg.VesselName == connectionNaturalKey.VesselName
				&& leg.VoyageCode == connectionNaturalKey.VoyageCode
				&& leg.CarrierSCAC == connectionNaturalKey.CarrierSCAC
				&& leg.Departure == connectionNaturalKey.Departure
				&& leg.Arrival == connectionNaturalKey.Arrival
				&& leg.LegType == connectionNaturalKey.LegType;
		}

		ImportGSSResponse BuildSuccessResult(List<JobSailing> allSailings, List<JobSailing> newSailings)
		{
			return new ImportGSSResponse
			{
				Code = ImportGSSResponseCode.Success,
				Message = Res.GetString("f9076406-8898-5aaa-45c2-fdb7290cecc2", "Successfully imported Schedule(s). {0} total schedule(s) and {1} new schedule(s).", allSailings.Count, newSailings.Count),
				Sailings = BuildSailingModels(allSailings, newSailings)
			};
		}

		static ImportGSSResponse Error(ImportGSSResponseCode errorCode, string errorMessage, IEnumerable<string> errorNotifications = null)
		{
			return new ImportGSSResponse
			{
				Code = errorCode,
				ErrorMessage = errorMessage,
				ErrorNotifications = errorNotifications
			};
		}
	}
}
