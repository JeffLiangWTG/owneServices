using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;

namespace Enterprise.Tracking.Web
{
	public class ModulePreloadRequestHandler : IHttpHandler, IRequiresSessionState
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var modules = WebDataRegistry.Instance.WebTrackerPreloadModules.Value;
			var moduleCodes = modules.OfType<CodeDescriptionBool>().Where(x => x.Bool).Select(x => x.Code);
			var preloadPaths = GetModulePreloadPaths(moduleCodes);

			var preloadService = ObjectFactory.Get<IWebTrackerPreloadService>();

			foreach (var path in preloadPaths)
			{
				preloadService.PreloadModule(context, path);
			}
		}

		static IEnumerable<string> GetModulePreloadPaths(IEnumerable<ZString> codes)
		{
			foreach (var code in codes)
			{
				switch (code)
				{
					case WebTrackerPreloadModulesList.Codes.Accounts:
						yield return TrackingConstants.PreloadRelativePath.Accounts;
						break;
					case WebTrackerPreloadModulesList.Codes.Admin:
						yield return TrackingConstants.PreloadRelativePath.Admin;
						break;
					case WebTrackerPreloadModulesList.Codes.Bookings:
						yield return TrackingConstants.PreloadRelativePath.Bookings;
						break;
					case WebTrackerPreloadModulesList.Codes.Cartage:
						yield return TrackingConstants.PreloadRelativePath.Cartage;
						break;
					case WebTrackerPreloadModulesList.Codes.CFSShipments:
						yield return TrackingConstants.PreloadRelativePath.CFSShipments;
						break;
					case WebTrackerPreloadModulesList.Codes.Containers:
						yield return TrackingConstants.PreloadRelativePath.Containers;
						break;
					case WebTrackerPreloadModulesList.Codes.Declaration:
						yield return TrackingConstants.PreloadRelativePath.Declaration;
						break;
					case WebTrackerPreloadModulesList.Codes.ImporterSecurityFiling:
						yield return TrackingConstants.PreloadRelativePath.ImporterSecurityFiling;
						break;
					case WebTrackerPreloadModulesList.Codes.LinerAndAgency:
						yield return TrackingConstants.PreloadRelativePath.LinerAndAgencyBillsOfLading;
						yield return TrackingConstants.PreloadRelativePath.LinerAndAgencyBookings;
						yield return TrackingConstants.PreloadRelativePath.LinerAndAgencyContainers;
						break;
					case WebTrackerPreloadModulesList.Codes.Orders:
						yield return TrackingConstants.PreloadRelativePath.Orders;
						break;
					case WebTrackerPreloadModulesList.Codes.Quotes:
						yield return TrackingConstants.PreloadRelativePath.Quotes;
						break;
					case WebTrackerPreloadModulesList.Codes.Reports:
						yield return TrackingConstants.PreloadRelativePath.Reports;
						break;
					case WebTrackerPreloadModulesList.Codes.Schedules:
						yield return TrackingConstants.PreloadRelativePath.Schedules;
						break;
					case WebTrackerPreloadModulesList.Codes.Shipments:
						yield return TrackingConstants.PreloadRelativePath.Shipments;
						break;
					case WebTrackerPreloadModulesList.Codes.Terms:
						yield return TrackingConstants.PreloadRelativePath.Terms;
						break;
					case WebTrackerPreloadModulesList.Codes.Warehousing:
						yield return TrackingConstants.PreloadRelativePath.Warehousing;
						break;
					default:
						throw new ArgumentException($"Module {code} could not be mapped to a preload path");
				}
			}
		}
	}
}
