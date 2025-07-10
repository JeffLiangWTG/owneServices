using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.OnlineSailingSchedules.GSSApiHelpers;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class OnlineSailingSchedulesWebApiImportMany
	{
		readonly BusinessObjectFactory factory;
		readonly IImportManyGSSPayload importManyPayload;
		readonly OnlineSchedules onlineSchedules;

		public OnlineSailingSchedulesWebApiImportMany(IImportManyGSSPayload importManyPayload, OnlineSchedules onlineSchedules = null)
		{
			this.importManyPayload = importManyPayload;
			factory = onlineSchedules?.Factory ?? new BusinessObjectFactory();
			this.onlineSchedules = onlineSchedules ?? new OnlineSchedules(factory, new RoutesProvider(factory));
		}

		public IImportManyGSSResponse Import()
		{
			var payloadError = ValidatePayload();
			if (payloadError != null)
			{
				return payloadError;
			}

			var sailingsOfAllQueries = new List<List<List<ISailingModel>>>();
			foreach (var connectionQuery in importManyPayload.ConnectionQueries)
			{
				onlineSchedules.Request = BuildFilterRequest(connectionQuery);

				var loadRoutesNotifications = new NotificationBuffer();
				onlineSchedules.LoadRoutes(loadRoutesNotifications);
				if (loadRoutesNotifications.HasErrors)
				{
					return Error(connectionQuery, ImportGSSResponseCode.LoadRoutesValidation, Res.GetString("6778b618-dafb-baad-4dd0-ff4fbc3975c0", "Load routes errors"), loadRoutesNotifications.Events.Select(e => e.Message));
				}

				var routes = onlineSchedules.Routes.OfType<Route>()
					.Where(route => route.Legs.OfType<Leg>()
						.Any(leg => leg.TradeLaneName == connectionQuery.TradeLane.Name))
					.ToArray();
				var clonedOnlineSchedules = onlineSchedules.CloneSelectedSchedules(routes, factoryToUse: factory);
				clonedOnlineSchedules.RunPreSaveValidation();
				if (clonedOnlineSchedules.HasErrors)
				{
					return Error(connectionQuery, ImportGSSResponseCode.OnlineSchedulesValidation, Res.GetString("c0224863-4e0d-6780-4e94-efefb11a2d64", "Online Schedules validation errors"), GetOnlineSchedulesErrors(onlineSchedules));
				}

				if (routes.Length == 0)
				{
					return Error(connectionQuery, ImportGSSResponseCode.ParameterError, Res.GetString("26919202-99b0-cdb3-47cc-a99bd9064a9e", "Found zero routes for connection."));
				}

				clonedOnlineSchedules.CreateEnterpriseVoyages();

				var sailingsOfConnectionQuery = new List<List<ISailingModel>>();
				foreach (var route in routes)
				{
					var allSailings = route.Legs.Select(leg => leg.FindMatchingJobSailing(true)).ToList();
					var newSailings = allSailings.Where(sailing => sailing != null && !sailing.IsInDatabase).ToList();
					sailingsOfConnectionQuery.Add(BuildSailingModels(allSailings, newSailings, connectionQuery));
				}
				sailingsOfAllQueries.Add(sailingsOfConnectionQuery);
			}

			factory.Save();

			return new ImportManyGSSResponse
			{
				Success = true,
				Sailings = sailingsOfAllQueries
			};
		}

		ImportManyGSSResponse ValidatePayload()
		{
			if (importManyPayload.ConnectionQueries == null)
			{
				return Error(null, ImportGSSResponseCode.ParameterError, (NoResString)"ConnectionQueries must not be null.");
			}

			if (!importManyPayload.ConnectionQueries.Any())
			{
				return Error(null, ImportGSSResponseCode.ParameterError, (NoResString)"ConnectionQueries must have at least one connection.");
			}

			foreach (var connectionQuery in importManyPayload.ConnectionQueries)
			{
				if (connectionQuery == null
					|| connectionQuery.DischargePort == null
					|| connectionQuery.LoadPort == null
					|| connectionQuery.TradeLane == null)
				{
					return Error(connectionQuery, ImportGSSResponseCode.ParameterError, (NoResString)"Invalid connectionQuery in payload.");
				}
			}

			return null;
		}

		OnlineSchedulesFilterRequest BuildFilterRequest(IConnectionQuery connectionQuery)
		{
			var serviceString = connectionQuery.TradeLane.Name.Split(new[] { " - " }, StringSplitOptions.None)[0];

			return new OnlineSchedulesFilterRequest
			{
				EtdFrom = connectionQuery.StartDate.ToString("yyyy-MM-dd"),
				EtdTo = connectionQuery.ExpiryDate.ToString("yyyy-MM-dd"),
				LoadPort = connectionQuery.LoadPort,
				DischargePort = connectionQuery.DischargePort,
				IncludeRelatedPorts = connectionQuery.AllowRelatedUNLOCOs.ToString(),
				ServiceString = serviceString,
				LegsCount = (connectionQuery.DirectRoutesOnly ? "1" : string.Empty),
			};
		}

		static ImportManyGSSResponse Error(IConnectionQuery conectionQuery, ImportGSSResponseCode errorCode, string errorMessage, IEnumerable<string> errorNotifications = null)
		{
			return new ImportManyGSSResponse
			{
				Success = false,
				Error = new ImportManyGSSError
				{
					Code = errorCode,
					ErrorMessage = errorMessage,
					ConnectionQuery = conectionQuery,
					ErrorNotifications = errorNotifications
				}
			};
		}
	}
}
