using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Packing.Business
{
	public static class PackageIDGenerator
	{
		#region GenerateIDs 

		internal static void GenerateNewIDsAndStoreOldIDsAsPreviousID(List<PkgPackage> packagesToRegenerateIDs, HashSet<ZString> existingIDs)
		{
			Argument.NotNull(packagesToRegenerateIDs, "packagesToGenerateIDs");
			Argument.NotNull(existingIDs, "existingIDs");

			var packageJob = packagesToRegenerateIDs.FirstOrDefault()?.PackageJob;
			var numberGenerator = packageJob != null ? GetNewNumberGenerator(packageJob, packageJob.ParentJob, packageJob.Factory) : null;
			GenerateNewIDsAndStoreOldIDsAsPreviousID(numberGenerator, packagesToRegenerateIDs, existingIDs);
		}

		static void GenerateNewIDsAndStoreOldIDsAsPreviousID(NumberGenerator numberGenerator, IEnumerable<PkgPackage> packages, HashSet<ZString> existingIDs)
		{
			packages.ForEach(package => package.KP_PreviousPackageID = package.KP_PackageID);
			var proposedIDs = GetNextPackageIDs(numberGenerator, existingIDs, packages.Count());
			AssignGeneratedIDs(packages, proposedIDs);
		}

		/// <summary>
		/// Performance increase over multiple calls of single PkgPackageJob variant if packages in packagesToGenerateIDs relate to multiple different PkgPackageJobs.
		/// Internal Only
		/// </summary>
		/// <param name="packagesToGenerateIDs"></param>
		/// <param name="notify"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		internal static bool GenerateIDsForAllPackages(IEnumerable<ISupportPackageIDGeneration> packagesToGenerateIDs, INotifications notify, SSCCGenerationContext context)
		{
			Argument.NotNull(packagesToGenerateIDs, "packagesToGenerateIDs");
			Argument.NotNull(notify, "notify");
			bool idHasBeenGenerated = false;

			// no packages
			if (!packagesToGenerateIDs.Any())
			{
				notify.AddError(Res.GetString("e0ee8926-480d-4b78-bc88-16de4d46b107", "No packages were selected."));
			}
			else
			{
				var packageJobCache = new Dictionary<ZGuid, (PkgPackageJob, IPackingParent)>();
				idHasBeenGenerated = GenerateIDsCore(packagesToGenerateIDs.ToLookup(p => GetPackageJob(p, packageJobCache)), notify, context);
			}

			return idHasBeenGenerated;

			(PkgPackageJob, IPackingParent) GetPackageJob(ISupportPackageIDGeneration package, Dictionary<ZGuid, (PkgPackageJob, IPackingParent)> packageJobCache)
			{
				var key = package.PackageJobPK;
				if (!packageJobCache.TryGetValue(key, out var value))
				{
					var packageJob = package.PackageJob;
					packageJobCache[key] = value = (packageJob, packageJob.ParentJob);
				}

				return value;
			}
		}

		/// <summary>
		/// Generates package IDs for supplied packages. Only public for Packing.GUI. All other use cases should set ISupportPackageIDGeneration.ShouldGenerateIDOnSaving to true and Save.
		/// </summary>
		/// <param name="packageJob"></param>
		/// <param name="packagesToGenerateIDs"></param>
		/// <param name="notify"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static bool GenerateIDsForAllPackagesAndConsumeFountainImmediately_DoNotUse(PkgPackageJob packageJob, IEnumerable<ISupportPackageIDGeneration> packagesToGenerateIDs, INotifications notify, SSCCGenerationContext context)
		{
			Argument.NotNull(packageJob, nameof(packageJob));
			Argument.NotNull(packagesToGenerateIDs, nameof(packagesToGenerateIDs));
			Argument.NotNull(notify, nameof(notify));
			var idHasBeenGenerated = false;

			// no packages
			var packagesEvaluated = packagesToGenerateIDs.ToArray();
			if (packagesEvaluated.Length == 0)
			{
				notify.AddError(Res.GetString("e0ee8926-480d-4b78-bc88-16de4d46b107", "No packages were selected."));
			}
			else
			{
				var parentJob = packageJob.ParentJob;
				idHasBeenGenerated = GenerateIDsCore(packagesEvaluated.ToLookup(p => (packageJob, parentJob)), notify, context);
			}

			return idHasBeenGenerated;
		}

		internal static bool GenerateIDsForAllPackages(ILookup<PkgPackageJob, ISupportPackageIDGeneration> packagesToGenerateIDs, INotifications notify, SSCCGenerationContext context)
		{
			Argument.NotNull(packagesToGenerateIDs, "packagesToGenerateIDs");
			Argument.NotNull(notify, "notify");

			var parentJobCache = new Dictionary<PkgPackageJob, IPackingParent>();
			var packagesWithPackageJobAndParentJob = packagesToGenerateIDs
				.SelectMany(p => p.Select(o => new { Package = o, PackageJob = p.Key }))
				.ToLookup(o => (o.PackageJob, GetParentJob(o.PackageJob)), o => o.Package);

			return GenerateIDsCore(packagesWithPackageJobAndParentJob, notify, context);

			IPackingParent GetParentJob(PkgPackageJob packageJob)
			{
				if (!parentJobCache.TryGetValue(packageJob, out var value))
				{
					parentJobCache[packageJob] = value = packageJob.ParentJob;
				}

				return value;
			}
		}

		static bool GenerateIDsCore(ILookup<(PkgPackageJob PackageJob, IPackingParent ParentJob), ISupportPackageIDGeneration> packageJobsWithPackagesToGenerateIDs, INotifications notify, SSCCGenerationContext context)
		{
			var idHasBeenGenerated = false;
			var hadSaveException = false;

			if (CheckPackageJobsForPackagesAndErrors(packageJobsWithPackagesToGenerateIDs, notify))
			{
				var pkgJobFactory = packageJobsWithPackagesToGenerateIDs.First().Key.PackageJob.Factory;
				var packageJobsAndSsccPrefixes = GetSSCCPrefixes(packageJobsWithPackagesToGenerateIDs, notify, context);

				GenerateAndAssignIDs();

				if (!idHasBeenGenerated)
				{
					notify.AddWarning(Res.GetString("91c67b9d-7823-44ad-b9d9-02472eeb895b",
						"Package IDs can only be created for non-Container Packages that have no Package ID and a Qty of one (1)."));
				}

				// Close Package & Generate IDs currently rely on the changes being saved immediately.
				if (context == SSCCGenerationContext.GeneratingIDsViaUser || context == SSCCGenerationContext.ScanPacking)
				{
					try
					{
						// There will be nothing else in the factory to save as users can't generate SSCC without the form being saved first
						pkgJobFactory.Save();
					}
					catch (ZSaveException ex)
					{
						hadSaveException = true;
						ZExceptionReporting.HandleSaveException(ex, new NotificationsNotificationHandler(notify));
					}
				}

				void GenerateAndAssignIDs()
				{
					pkgJobFactory.SuspendValidation();
					try
					{
						var connection = ((IDbConnected)pkgJobFactory).Connection;

						foreach (var packageJobAndPackages in packageJobsWithPackagesToGenerateIDs)
						{
							var (packageJob, parentJob) = packageJobAndPackages.Key;
							var ssccPrefix = packageJob != null ? packageJobsAndSsccPrefixes.GetValueSafe(packageJob.PK) : ZString.Empty;

							Func<DbConnection, int, IEnumerable<string>> generateIDsFunction;

							if (string.IsNullOrEmpty(ssccPrefix))
							{
								var existingIDs = new HashSet<ZString>(packageJob.GetAllPackageIDs());
								generateIDsFunction = (cxn, n) =>
								{
									var numberGenerator = new Lazy<NumberGenerator>(() => GetNewNumberGenerator(packageJob, parentJob, new BusinessObjectFactory(cxn)));
									return GetNextPackageIDs(numberGenerator.Value, existingIDs, n);
								};
							}
							else
							{
								generateIDsFunction = (cxn, n) => Env.NumberFountains.SSCCBarCode(ssccPrefix).GetNextsFormatted(cxn, n);
							}

							Func<int, IEnumerable<string>> generateIDsCheckingTransaction = n =>
							{
								IEnumerable<string> result;

								if (connection.IsInTransaction)
								{
									result = generateIDsFunction(connection, n);
								}
								else
								{
									using (var newConnection = Db.NewExtraConnectionToMainDb())
									{
										var ids = new List<string>(n);

										newConnection.RunTransactioned(() =>
										{
											ids.AddRange(generateIDsFunction(newConnection, n));
										});

										result = ids;
									}
								}

								return result;
							};

							idHasBeenGenerated |= GenerateIDs(packageJobAndPackages, generateIDsCheckingTransaction);
						}

						if (idHasBeenGenerated && connection.IsInTransaction)
						{
							pkgJobFactory.Saved += MarkFactoryWithGenerateIDFailure;
						}
					}
					finally
					{
						pkgJobFactory.ResumeValidation();
					}
				}
			}

			return idHasBeenGenerated && !hadSaveException;

			void MarkFactoryWithGenerateIDFailure(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				factory.Saved -= MarkFactoryWithGenerateIDFailure;

				if (!savedSuccessfully)
				{
					packageJobsWithPackagesToGenerateIDs.ForEach(p => p.Key.PackageJob.MarkAsPackageIDGenerationFailed());
				}
			}
		}

		static Dictionary<ZGuid, ZString> GetSSCCPrefixes(ILookup<(PkgPackageJob, IPackingParent), ISupportPackageIDGeneration> packageJobsWithPackagesToGenerateIDs, INotifications notify, SSCCGenerationContext context)
		{
			var packageJobsAndSsccPrefixes = new Dictionary<ZGuid, ZString>();
			foreach (var packageJobAndPackages in packageJobsWithPackagesToGenerateIDs)
			{
				var (packageJob, parentJob) = packageJobAndPackages.Key;
				if (parentJob != null && !packageJobsAndSsccPrefixes.ContainsKey(packageJob.PK))
				{
					packageJobsAndSsccPrefixes.Add(packageJob.PK, parentJob.GetSSCCPrefix(notify, context));
				}
			}

			return packageJobsAndSsccPrefixes;
		}

		static bool CheckPackageJobsForPackagesAndErrors(ILookup<(PkgPackageJob, IPackingParent), ISupportPackageIDGeneration> packageJobsWithPackagesToGenerateIDs, INotifications notify)
		{
			var canGenerateLabels = packageJobsWithPackagesToGenerateIDs.Any();
			foreach (var packageJobAndPackages in packageJobsWithPackagesToGenerateIDs.ToArray())
			{
				var (packageJob, parentJob) = packageJobAndPackages.Key;
				// job is attached but no JobNo (eg. DocketID)
				if (parentJob != null && parentJob.JobNo.IsEmpty)
				{
					var jobDesc = (parentJob.JobDescription.IsEmpty) ? new ZString(Res.GetString("4f9c8e9f-822c-437a-b36e-4d8bd9c91789", "Parent Job")) : parentJob.JobDescription;
					notify.AddError(Res.GetString("f2103be7-2a4b-49b6-b837-169c9c8cf1a5", "The {0} does not yet have a Job Number. Try saving the {0} first.", jobDesc));
					canGenerateLabels = false;
				}
				// no job attached and the PkgPackageJob was never saved (no ID)
				else if (parentJob == null && packageJob.KJ_JobID.IsEmpty)
				{
					notify.AddError(Res.GetString("a00cde84-d370-4c96-b849-333d95be3725", "The Packing Job does not yet have a Job Number. Try saving the Packing Job first."));
					canGenerateLabels = false;
				}
			}

			return canGenerateLabels;
		}

		#region NotificationsNotificationHandler

		class NotificationsNotificationHandler : INotificationHandler
		{
			public NotificationsNotificationHandler(INotifications notify)
			{
				Notify = notify;
			}
			readonly INotifications Notify;

			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				Notify.AddError(message);
			}

			public void ReportInformation(string message, string caption)
			{
				Notify.AddInformation(message);
			}
		}

		#endregion

		static bool GenerateIDs(IEnumerable<ISupportPackageIDGeneration> packages, Func<int, IEnumerable<string>> generateIDsFunction)
		{
			var packagesToGenerateIDsFor = new List<ISupportPackageIDGeneration>();
			GetAllPackages(packagesToGenerateIDsFor, packages);

			var idHasBeenGenerated = false;
			if (packagesToGenerateIDsFor.Any())
			{
				var generatedIDs = generateIDsFunction(packagesToGenerateIDsFor.Count);
				AssignGeneratedIDs(packagesToGenerateIDsFor, generatedIDs);
				idHasBeenGenerated = true;
			}

			return idHasBeenGenerated;
		}

		static void GetAllPackages(List<ISupportPackageIDGeneration> packagesToGenerateIDsFor, IEnumerable<ISupportPackageIDGeneration> packages)
		{
			foreach (var package in packages)
			{
				if (CanGenerateID(package))
				{
					packagesToGenerateIDsFor.Add(package);
				}

				GetAllPackages(packagesToGenerateIDsFor, package.Packages);
			}
		}

		static void AssignGeneratedIDs(IEnumerable<ISupportPackageIDGeneration> packagesToGenerateIDsFor, IEnumerable<string> generatedIDs)
		{
			generatedIDs.Zip(packagesToGenerateIDsFor, (packageId, package) => package.KP_PackageID = packageId).ToArray();
		}

		static bool CanGenerateID(ISupportPackageIDGeneration package)
		{
			return package.KP_PackageQty == 1 && package.KP_PackageID.IsEmpty && !package.IsContainer;
		}

		static IEnumerable<string> GetNextPackageIDs(NumberGenerator numberGenerator, HashSet<ZString> existingIDs, int packageIdsNeedToGenerateCount)
		{
			var generatedIdsToReturn = new HashSet<ZString>();
			var idsNeedToGenerateCount = packageIdsNeedToGenerateCount;
			do
			{
				var generatedIds = numberGenerator.GenerateNumbers(numberGenerator.PrimaryTarget, idsNeedToGenerateCount, true);
				generatedIdsToReturn.UnionWith(generatedIds.Except(existingIDs));
				idsNeedToGenerateCount = packageIdsNeedToGenerateCount - generatedIdsToReturn.Count;
				existingIDs.UnionWith(generatedIdsToReturn);
			}
			while (idsNeedToGenerateCount > 0);

			return generatedIdsToReturn.Select(id => id.ToString());
		}

		static NumberGenerator GetNewNumberGenerator(PkgPackageJob packageJob, IPackingParent parent, BusinessObjectFactory factory)
		{
			var numberGeneratorContext = new NumberGeneratorContext();
			var packageIDNumberGeneratorTarget = new PackageIDNumberGeneratorTarget() { Context = numberGeneratorContext };
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = numberGeneratorContext,
				BaseFountain = Env.NumberFountains.PackageID,
				PrimaryTarget = packageIDNumberGeneratorTarget
			};

			// Handling Units each have their own Package Job & a Harcoded JobNo, it should use the System wide Fountain.
			if (!(parent is PkgHandlingUnit) && IsJobNumberInPackageIdFountain())
			{
				generator.FountainGetter = (fountainKey) => Env.NumberFountains.GetPackageIDGeneratorFountainWithPackingParent(fountainKey, packageJob.PK.ToGuid());
			}
			else
			{
				generator.FountainGetter = Env.NumberFountains.GetPackageIDGeneratorFountain;
			}

			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new PackageIDValueSource(packageJob)));
			return generator;

			bool IsJobNumberInPackageIdFountain()
			{
				return (bool)packageIDNumberGeneratorTarget
					.NumberCustomisation
					.Elements
					.Cast<BillOfLadingNumberCustomisationElement>()
					.SingleOrDefault(element => element.Key == BillOfLadingNumberCustomisationElement.Keys.JobNo)?.Fountain;
			}
		}

		#endregion

		#region ClearIDs

		public static void ClearIDs(IEnumerable<ISupportPackageIDGeneration> parentPackagesToClear, INotifications notify)
		{
			Argument.NotNull(parentPackagesToClear, "parentPackagesToClear");
			Argument.NotNull(notify, "notify");

			if (!parentPackagesToClear.Any())
			{
				notify.AddError(Res.GetString("e0ee8926-480d-4b78-bc88-16de4d46b107", "No packages were selected."));
			}
			else
			{
				var atLeastOnePackageWasCleared = false;

				foreach (var package in parentPackagesToClear)
				{
					atLeastOnePackageWasCleared |= ClearIDs(package);
				}

				if (!atLeastOnePackageWasCleared)
				{
					notify.AddError(Res.GetString("dd735031-110e-4f77-8ac0-a71ddb7c6295", "All Selected Packages are Containers or have Tracking Numbers. Container should be manually cleared and Tracking Numbers cannot be cleared."));
				}
			}
		}

		static bool ClearIDs(ISupportPackageIDGeneration package)
		{
			var packageCleared = false;

			if (!package.IsContainer && !package.IsBookedViaCarrier)
			{
				package.KP_PackageID = "";
				packageCleared = true;
			}

			foreach (var childPackage in package.Packages)
			{
				packageCleared |= ClearIDs(childPackage);
			}

			return packageCleared;
		}

		#endregion
	}
}

