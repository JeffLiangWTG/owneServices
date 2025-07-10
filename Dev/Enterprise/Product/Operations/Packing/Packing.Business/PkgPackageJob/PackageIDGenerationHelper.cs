using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ZArchitecture;

namespace Enterprise.Packing.Business
{
	static class PackageIDGenerationHelper
	{
		public static void AddPackageForIDGenerationIfMarked(BusinessObjectFactory factory, ISupportPackageIDGenerationInternals packageIDGeneration)
		{
			if (packageIDGeneration.ShouldGenerateIDOnSaving && packageIDGeneration.KP_PackageID.IsEmpty)
			{
				var generateIDsService = factory.ServiceContainer.GetAfterOnSavingService<GenerateIDAfterOnSavingBOProcessingService>();
				if (generateIDsService == null)
				{
					generateIDsService = new GenerateIDAfterOnSavingBOProcessingService();
					factory.ServiceContainer.AddAfterOnSavingService(generateIDsService);
				}

				generateIDsService.AddPackageToGenerateIDFor(packageIDGeneration);
			}
		}

		public static void Clear(BusinessObjectFactory factory)
		{
			var generateIDsService = factory.ServiceContainer.GetAfterOnSavingService<GenerateIDAfterOnSavingBOProcessingService>();
			generateIDsService?.Clear();
		}

		class GenerateIDAfterOnSavingBOProcessingService : IAfterOnSavingBOProcessingService
		{
			internal void AddPackageToGenerateIDFor(ISupportPackageIDGenerationInternals entityForIDGeneration)
			{
				PackagesForIDGeneration.Add(entityForIDGeneration);
			}

			internal void Clear()
			{
				PackagesForIDGeneration.Clear();
			}

			readonly HashSet<ISupportPackageIDGenerationInternals> PackagesForIDGeneration = new HashSet<ISupportPackageIDGenerationInternals>();

			// interfaces

			#region IAfterOnSavingBOProcessingService Members

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				if (!GeneratingIDsInProgress)
				{
					GeneratingIDsInProgress = true;
					using (new DisposableAction(() => GeneratingIDsInProgress = false))
					{
						if (PackagesForIDGeneration.Any())
						{
							try
							{
								var notificationBuffer = new NotificationBuffer();
								PackageIDGenerator.GenerateIDsForAllPackages(PackagesForIDGeneration.Cast<ISupportPackageIDGeneration>().ToLookup(p => p.PackageJob), notificationBuffer, SSCCGenerationContext.GeneratingIDsOnSave);

								foreach (var package in PackagesForIDGeneration)
								{
									package.CallAfterIDGenerated();
								}

								if (notificationBuffer.HasErrors)
								{
									var headerText = Res.GetString("7338c191-4c74-4f87-994e-7670084d1662", "Package ID Generation Failed");
									var message = Res.GetString("5421b290-39b0-47b4-a87b-9df7bd0fdb89",
										"Package IDs could not be successfully generated. Errors below:\r\n{0}",
										string.Join("\r\n", notificationBuffer.Events.Select(e => e.Message)));
									throw new ZCannotSaveException(message, headerText);
								}
							}
							finally
							{
								PackagesForIDGeneration.Clear();
							}
						}
					}
				}
			}

			bool GeneratingIDsInProgress { get; set; }

			#endregion
		}
	}
}
