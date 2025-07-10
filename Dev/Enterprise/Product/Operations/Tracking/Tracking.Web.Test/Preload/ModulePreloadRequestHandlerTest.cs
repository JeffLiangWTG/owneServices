using System;
using System.Web;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ModulePreloadRequestHandlerTest : TestCaseWithFactory
	{
		public void TestModulePreload_All()
		{
			var preloadModules = GetPreloadModuleList(true);

			using (WebDataRegistry.Instance.WebTrackerPreloadModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preloadModules))
			{
				var mockPreloadService = new Mock<IWebTrackerPreloadService>();
				mockPreloadService.Setup(x => x.PreloadModule(HttpContext.Current, It.IsAny<string>()));
				ObjectFactory.Substitute(mockPreloadService.Object);

				var requestHandler = new ModulePreloadRequestHandler();
				requestHandler.ProcessRequest(HttpContext.Current);

				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Accounts));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Admin));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Bookings));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Cartage));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.CFSShipments));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Containers));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Declaration));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.ImporterSecurityFiling));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyBillsOfLading));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyBookings));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyContainers));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Orders));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Quotes));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Reports));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Schedules));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Shipments));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Terms));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.Warehousing));
			}

			Assert("This is not an empty test", true);
		}

		public void TestModulePreload_None()
		{
			var preloadModules = GetPreloadModuleList(false);

			using (WebDataRegistry.Instance.WebTrackerPreloadModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preloadModules))
			{
				var mockPreloadService = new Mock<IWebTrackerPreloadService>();
				mockPreloadService.Setup(x => x.PreloadModule(HttpContext.Current, It.IsAny<string>()));
				ObjectFactory.Substitute(mockPreloadService.Object);

				var requestHandler = new ModulePreloadRequestHandler();
				requestHandler.ProcessRequest(HttpContext.Current);

				mockPreloadService.Verify(x => x.PreloadModule(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Never);
			}

			Assert("This is not an empty test", true);
		}

		public void TestModulePreload_LinerAndAgency()
		{
			var preloadModules = new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = true, Code = WebTrackerPreloadModulesList.Codes.LinerAndAgency },
			};

			using (WebDataRegistry.Instance.WebTrackerPreloadModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preloadModules))
			{
				var mockPreloadService = new Mock<IWebTrackerPreloadService>();
				mockPreloadService.Setup(x => x.PreloadModule(HttpContext.Current, It.IsAny<string>()));
				ObjectFactory.Substitute(mockPreloadService.Object);

				var requestHandler = new ModulePreloadRequestHandler();
				requestHandler.ProcessRequest(HttpContext.Current);

				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyBillsOfLading));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyBookings));
				mockPreloadService.Verify(x => x.PreloadModule(HttpContext.Current, TrackingConstants.PreloadRelativePath.LinerAndAgencyContainers));
			}

			Assert("This is not an empty test", true);
		}

		object GetPreloadModuleList(bool enabled)
		{
			return new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Accounts },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Admin },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Bookings },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Cartage },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.CFSShipments },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Containers },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Declaration },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.ImporterSecurityFiling },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.LinerAndAgency },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Orders },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Quotes },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Reports },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Schedules },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Shipments },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Terms },
				new CodeDescriptionBoolDisallowNew { Bool = enabled, Code = WebTrackerPreloadModulesList.Codes.Warehousing },
			};
		}
	}
}
