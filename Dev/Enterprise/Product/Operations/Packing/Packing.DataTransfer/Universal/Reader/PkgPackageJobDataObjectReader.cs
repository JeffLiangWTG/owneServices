using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageJobDataObjectReader : DataObjectReader<IDataObject, PkgPackageJob>
	{
		public PkgPackageJobDataObjectReader(IPackageParentDataObject parentData, IXmlImportLogger logger, UniversalObjectFactory factory, IPackingParent parentBO, ImportOption importOptions = ImportOption.Default, IEnumerable<ZGuid> targetPackagePks = null)
			: base(new PackageParentDataObject(parentData), logger, factory)
		{
			ParentBO = Argument.NotNull(parentBO, "IPackingParent packingParent");
			TargetPackagePks = targetPackagePks ?? Enumerable.Empty<ZGuid>();
			ImportOptions = importOptions;
		}

		readonly IPackingParent ParentBO;
		readonly IEnumerable<ZGuid> TargetPackagePks;
		readonly ImportOption ImportOptions;

		bool MatchOnPackageIDs => ImportOptions.HasAnyFlag(ImportOption.MatchOnPackageIDs, ImportOption.KeepUnmatchedPackages, ImportOption.MatchAndRelabelOnPreviousPackageID);

		bool KeepUnmatchedPackages => ImportOptions.HasAnyFlag(ImportOption.KeepUnmatchedPackages, ImportOption.MatchAndRelabelOnPreviousPackageID);

		bool PartialMatch => ImportOptions.HasFlag(ImportOption.PartialMatch);

		#region DataObject

		protected new IPackageParentDataObject dataObject => ((PackageParentDataObject)base.dataObject).ParentData;

		class PackageParentDataObject : IDataObject
		{
			public PackageParentDataObject(IPackageParentDataObject parentData)
			{
				ParentData = parentData;
			}

			public readonly IPackageParentDataObject ParentData;
		}

		#endregion

		#region GetExistingBusinessObject

		protected override PkgPackageJob GetExistingBusinessObject()
		{
			return PkgPackageJob.LoadOrCreatePackageJob(ParentBO);
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(PkgPackageJob targetBO)
		{
			if (ImportOptions == ImportOption.MatchAndRelabelOnPreviousPackageID)
			{
				ThrowImportFailureBeforeImportChecks(targetBO, dataObject.PackingLineCollection);
			}

			HandlePackageCleanup(targetBO.Packages.ToArray());
			PopulatePackageContainers(targetBO, dataObject);
			PopulatePackages(targetBO, dataObject);

			if (ImportOptions == ImportOption.MatchAndRelabelOnPreviousPackageID)
			{
				ThrowImportFailureIfDuplicatePackageIDsInBusinessObject(targetBO);
			}
		}

		void HandlePackageCleanup(PkgPackage[] packagesToCleanUp)
		{
			if (!MatchOnPackageIDs)
			{
				var targettedPackages = !TargetPackagePks.Any() ? packagesToCleanUp : packagesToCleanUp.Where(package => TargetPackagePks.Contains(package.PK)).ToArray();

				ResetPackagesActionStrategy(targettedPackages);
				if (!PartialMatch)
				{
					targettedPackages.DeleteAll();
				}
			}
		}

		void ResetPackagesActionStrategy(PkgPackage[] packages)
		{
			packages.ForEach(p => p.ClearActionStrategyCacheIncludingChildren());
		}

		#endregion

		#region PopulatePackageContainers

		void PopulatePackageContainers(PkgPackageJob packageJobBO, IPackageParentDataObject packageJobParentData)
		{
			if (packageJobParentData.ContainerCollection != null)
			{
				if (PartialMatch)
				{
					var containerCollectionDataObjectReader = new PkgPackageContainerCollectionDataObjectReader(packageJobParentData.ContainerCollection, logger, factory, packageJobBO.Packages, this);
					containerCollectionDataObjectReader.ReadIntoCollection();
				}
				else
				{
					foreach (var containerDO in packageJobParentData.ContainerCollection)
					{
						var packageContainerReader = new PkgPackageContainerDataObjectReader(containerDO, logger, factory, packageJobBO.Packages);
						ReadIntoContainerAndPopulateContainerLinks(containerDO, packageContainerReader);
					}
				}
			}
		}

		#endregion

		#region ReadIntoContainerAndPopulateContainerLinks

		public PkgPackage ReadIntoContainerAndPopulateContainerLinks(Container containerDO, PkgPackageContainerDataObjectReader reader)
		{
			Argument.NotNull(containerDO, nameof(containerDO));
			Argument.NotNull(reader, nameof(reader));

			CheckDuplicateContainerLink(containerDO);
			var containerBO = reader.ReadIntoBusinessObject();
			AddContainerLink(containerDO, containerBO);

			return containerBO;
		}

		void CheckDuplicateContainerLink(Container containerData)
		{
			var link = containerData.Link;
			if (link.HasValue && PackageContainerLinks.ContainsKey(link.Value))
			{
				var errorMessage = Res.GetString("488f8d49-6ad0-4d57-b867-5be94d0c2bb7", "Cannot add Container {0}, has duplicate link value {1}.", containerData.ContainerNumber, link);
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		void AddContainerLink(Container containerDO, PkgPackage package)
		{
			var link = containerDO.Link;
			if (link.HasValue)
			{
				PackageContainerLinks.Add(link.Value, package);
			}
		}

		#endregion

		#region PopulatePackages

		void PopulatePackages(PkgPackageJob packageJobBO, IPackageParentDataObject packageJobParentData)
		{
			if (packageJobParentData.PackingLineCollection != null)
			{
				var processedPackages = new HashSet<PkgPackage>();

				var dataContext = logger.TopLevelDataContext;
				var shouldMapPackageType = false;
				IUniversalDataObjectReaderHelper readerHelper = null;

				if (dataContext != null && dataContext.DataSourceCollection != null)
				{
					var isCustomDeclaration = dataContext
						.DataSourceCollection
						.Where(source => source != null && source.Type.HasValue)
						.Any(source => source.Type.ToString() == nameof(DataContextType.CustomsDeclaration));
					readerHelper = ObjectFactory.Get<ICountrySpecificDataObjectReaderHelperProvider>().GetUniversalDataObjectReaderHelper(factory, dataContext.CountryCodeToImportInto);

					shouldMapPackageType = isCustomDeclaration && readerHelper != null;
				}

				if (PartialMatch)
				{
					PopulatePackingLinesForPartialMatch(packageJobBO, packageJobParentData, processedPackages, readerHelper, shouldMapPackageType);
				}
				else
				{
					PopulatePackingLines(packageJobBO, packageJobParentData, processedPackages, readerHelper, shouldMapPackageType);
				}

				if (MatchOnPackageIDs && !KeepUnmatchedPackages)
				{
					foreach (var package in packageJobBO.GetAllPackagesOnJob().ToArray())
					{
						if (!processedPackages.Contains(package))
						{
							package.Delete();
						}
					}
				}
			}

			PopulatePackagesFromJobTotalsIfRequired(packageJobBO, packageJobParentData);
		}

		void PopulatePackingLinesForPartialMatch(PkgPackageJob packageJobBO, IPackageParentDataObject packageJobParentData, HashSet<PkgPackage> processedPackages, IUniversalDataObjectReaderHelper readerHelper, bool shouldMapPackageType)
		{
			var reader = new LoosePackageCollectionDataObjectReader(packageJobParentData.PackingLineCollection, logger, factory, this, packageJobBO, (packageData, packageReader) =>
			{
				var linkedPackage = GetLinkedPackage(packageJobBO, packageData);

				return ProcessPackage(packageData, linkedPackage, packageReader, processedPackages, readerHelper, shouldMapPackageType);
			});
			reader.ReadIntoCollection();
		}

		void PopulatePackingLines(PkgPackageJob packageJobBO, IPackageParentDataObject packageJobParentData, HashSet<PkgPackage> processedPackages, IUniversalDataObjectReaderHelper readerHelper, bool shouldMapPackageType)
		{
			var packingLineCollection = packageJobParentData.PackingLineCollection;
			foreach (var packageData in packingLineCollection)
			{
				PkgPackageDataObjectReader packageReader;

				var linkedPackage = GetLinkedPackage(packageJobBO, packageData);
				if (linkedPackage != null)
				{
					packageReader = linkedPackage.IsContainer
							? new PkgPackageDataObjectReader(packageData, logger, factory, packageJobBO, linkedPackage.Packages, packingLineCollection: packingLineCollection)

						// If package is not a Container then the Container Link was used to split the package Data. Int'l do this for RORO to store
						// vehicle information. While this is technically not correct, there are live clients using it and eServices advises that this
						// cannot be changed. Therefore we need to handle this quirk and NOT create both a Container + Package.
						: new PkgPackageDataObjectReader(packageData, logger, factory, linkedPackage);
				}
				else
				{
					packageReader = new PkgPackageDataObjectReader(packageData, logger, factory, packageJobBO, packageJobBO.Packages, ImportOptions, isProcessingOuterPackLine: true, packingLineCollection: packingLineCollection);
				}

				ProcessPackage(packageData, linkedPackage, packageReader, processedPackages, readerHelper, shouldMapPackageType);
			}
		}

		PkgPackage ProcessPackage(PackingLine packageData, PkgPackage linkedPackage, PkgPackageDataObjectReader packageReader, HashSet<PkgPackage> processedPackages, IUniversalDataObjectReaderHelper readerHelper, bool shouldMapPackageType)
		{
			PkgPackage package = null;
			var parentContainer = linkedPackage != null && linkedPackage.IsContainer ? linkedPackage : null;
			if (parentContainer == null || packageData.PackQty > 0)
			{
				package = packageReader.ReadIntoBusinessObject();
				processedPackages.UnionWith(packageReader.ProcessedPackages);

				// When data source is Custom Declaration, we need to remap the Pack Type (if possible).
				if (shouldMapPackageType)
				{
					var packageType = packageData.PackType;
					ZString? packageTypeCode = packageType == null || !packageType.Code.HasValue ? null : packageType.GetCodeAsUpperCase();
					var mappedPackType = readerHelper.GetFreightUnitForPackType(packageTypeCode);

					if (mappedPackType.HasValue && mappedPackType.Value != ZString.Empty)
					{
						package.KP_F3_NKPackType = mappedPackType.Value;
					}
				}

				CheckIfContainerPackedInsideOtherPackage(package);
			}
			else if (parentContainer != null && packageData.PackQty <= 0 && !parentContainer.Container.K0_IsEmpty)
			{
				throw new DataObjectReadFailureException(Res.GetString("PkgPackageJobDataObjectReader|PopulatePackages|ZeroPacklineQuantityInNonEmptyContainer", "A container must be empty if its packline has quantity 0."));
			}
			// else the container is empty so we ignore packlines with qty <= 0 

			var addLink = new Action<KeyValuePair<ZInt, PkgPackage>>((kvp) =>
			{
				if (PackageLinks.ContainsKey(kvp.Key))
				{
					var errorMessage = Res.GetString("6b762c0d-2797-41f2-97bc-0abf1f36c66d", "Cannot add Package {0}, has duplicate link value {1}.", kvp.Value.KP_PackageID, kvp.Key);
					throw new DataObjectReadFailureException(errorMessage);
				}
				else
				{
					PackageLinks.Add(kvp.Key, kvp.Value);
				}
			});

			Array.ForEach(packageReader.PackageLinks.ToArray(), addLink);

			return package;
		}

		static void CheckIfContainerPackedInsideOtherPackage(PkgPackage package)
		{
			if (package.KP_F3_NKPackType == Constants.PkgUnit.Container && !package.KP_KP_ParentPackage.IsEmpty)
			{
				var errorMessage = Res.GetString("091B2A04-281A-44EC-ABFF-0DB4CF2AD387", "Package {0} must not have Pack Type 'CNT' as it is inside a container or package {1}.", package.KP_PackageID, package.ParentPackage.KP_PackageID);
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		PkgPackage GetLinkedPackage(PkgPackageJob packageJobBO, PackingLine packageData)
		{
			PkgPackage linkedPackage = null;

			var containerLink = packageData.ContainerLink;
			if (containerLink.HasValue)
			{
				PackageContainerLinks.TryGetValue(containerLink.Value, out linkedPackage);
			}

			if (linkedPackage == null)
			{
				linkedPackage = packageJobBO.Containers.FirstOrDefault(c => packageData.ContainerNumber.Equals(c.KP_PackageID));
			}
			return linkedPackage;
		}

		#endregion

		#region PopulatePackagesFromJob

		/// <summary>
		/// if there are no Outer Packages and there is a Job Total Quantity
		///   Create an Outer Package using Job Totals
		/// if there is 1 Outer Package that doesn't have Weights or Volumes but it's Quantity and Package Type matches the Job Total
		///   Add the Job Total Weight and Volume to it
		/// if there are many Outer Packages that don't have Weights or Volumes, and their parent is the same (ie for the same container, or all top level)
		///   Create a new Outer for them and add the existing as children of the new
		/// NOTE:
		///		We do not create a Totals Package if the loose packages are spread across Containers as there is no support to group them under a Totals Package.
		///		The Sender should add Goods Weight to each Container instead.  
		/// </summary>
		void PopulatePackagesFromJobTotalsIfRequired(PkgPackageJob packageJobBO, IPackageParentDataObject dataObject)
		{
			var outerPackages = packageJobBO.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers();

			var dataObjectHasNoContainers = dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count == 0;
			var parentTableCodesThatShouldNotHavePackagePopulatedWhenContainersAreAttached = new string[] { DtbBookingConsolidationSchema.Constants.Prefix };
			var parentBOShouldHavePackagePopulatedWhenContainersAreAttached = !parentTableCodesThatShouldNotHavePackagePopulatedWhenContainersAreAttached.Any(t => ParentBO.TablePrefix == t);

			if (!outerPackages.Any() && (NoOfPacksFromJob.GetValueOrDefault() > 0 || (isUnknownQty && (parentBOShouldHavePackagePopulatedWhenContainersAreAttached || dataObjectHasNoContainers))))
			{
				PopulatePackageFromJob(packageJobBO.Packages.AddNew(), dataObject);
			}
			else if (outerPackages.Any() && outerPackages.All(p => p.KP_Weight == 0m && p.KP_Volume == 0m) && (dataObject.TotalWeight.GetValueOrDefault() > 0 || dataObject.TotalVolume.GetValueOrDefault() > 0))
			{
				var firstLoosePackage = outerPackages.First();
				if (outerPackages.Count == 1 && DoesPackageMatchJobTotalsDetails(firstLoosePackage))
				{
					PopulatePackageFromJob(firstLoosePackage, dataObject);
				}
				else if (outerPackages.All(p => p.KP_KP_ParentPackage == firstLoosePackage.KP_KP_ParentPackage))
				{
					var newOuterPackage = InsertNewTotalsPackage(firstLoosePackage.ParentPackage?.Packages ?? packageJobBO.Packages);
					PopulatePackageFromJob(newOuterPackage, dataObject);
				}
			}
		}

		void PopulatePackageFromJob(PkgPackage packageBO, IPackageParentDataObject dataObject)
		{
			if (isUnknownQty)
			{
				SetValue(packageBO, PkgPackageSchema.KP_IsUnknownQty, true);
				SetValue(packageBO, PkgPackageSchema.KP_PackageQty, NoOfPacksFromJob.GetValueOrDefault());
				SetValue(packageBO, PkgPackageSchema.KP_F3_NKPackType, PackTypeFromJob);
			}
			else
			{
				var packQtyToSet = NoOfPacksFromJob.GetValueOrDefault() == 0 ? (ZInt)1 : NoOfPacksFromJob.Value;

				using (new SemaphoreManager(packageBO.SuspendAddWeightToParentPackageSemaphore))
				{
					// packs
					SetValue(packageBO, PkgPackageSchema.KP_PackageQty, packQtyToSet);
					SetValue(packageBO, PkgPackageSchema.KP_F3_NKPackType, PackTypeFromJob);

					// weights
					SetValue(packageBO, PkgPackageSchema.KP_Weight, dataObject.TotalWeight);
					SetValue(packageBO, PkgPackageSchema.KP_WeightUQ, dataObject.TotalWeightUnit);

					// volume
					SetValue(packageBO, PkgPackageSchema.KP_Volume, dataObject.TotalVolume);
					SetValue(packageBO, PkgPackageSchema.KP_VolumeUQ, dataObject.TotalVolumeUnit);

					// other
					SetValue(packageBO, PkgPackageSchema.KP_GoodsDescription, dataObject.GoodsDescription);
				}

				PopulatePackageBookedDimensionsIfSupported(packageBO, dataObject, packQtyToSet);
			}
			PkgPackageDataObjectReader.ThrowImportFailuresOnInvalidData(packageBO);
		}

		void PopulatePackageBookedDimensionsIfSupported(PkgPackage packageBO, IPackageParentDataObject dataObject, ZInt packQtyToSet)
		{
			if (packageBO.PackageJob?.ParentJob as IPackingParentSupportsImportingBookedDimensions != null)
			{
				// packs
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_PackageQty, packQtyToSet);

				// weights
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Weight, dataObject.TotalWeight);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_WeightUQ, dataObject.TotalWeightUnit);

				// volume
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Volume, dataObject.TotalVolume);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_VolumeUQ, dataObject.TotalVolumeUnit);
			}
		}

		bool DoesPackageMatchJobTotalsDetails(PkgPackage package)
		{
			return package.KP_PackageQty == NoOfPacksFromJob && (package.KP_F3_NKPackType.IsEmpty || package.KP_F3_NKPackType == PackTypeFromJob);
		}

		static PkgPackage InsertNewTotalsPackage(PkgPackageCollection packages)
		{
			var innersForNewPackage = packages.Where(p => !p.IsContainer).ToArray();
			var package = packages.AddNew();
			package.Packages.AddRange(innersForNewPackage);
			return package;
		}

		/// <summary>
		/// Returns a Pack Type of UNT if the *Job Level* Pack Type was CNT. A Job Level Pack Type of CNT is not valid
		/// as there are no container details (this can happen in ForwardingShipment if the user enters n x CNT).
		/// </summary>
		ZString PackTypeFromJob => NoOfPacksAndPackTypeFromJob.Value;

		ZInt? NoOfPacksFromJob => NoOfPacksAndPackTypeFromJob.Key;

		KeyValuePair<ZInt?, ZString> NoOfPacksAndPackTypeFromJob
		{
			get
			{
				if (fNoOfPacksAndPackTypeFromJob == null)
				{
					ZInt? noOfPacks = null;
					var packageType = ZString.Empty;

					if (dataObject.OuterPacks != null && !dataObject.OuterPacksPackageType.GetCodeAsUpperCase().IsEmpty)
					{
						noOfPacks = dataObject.OuterPacks;
						packageType = dataObject.OuterPacksPackageType.GetCodeAsUpperCase();
					}
					else if (dataObject.TotalNoOfPacks != null && !dataObject.TotalNoOfPacksPackageType.GetCodeAsUpperCase().IsEmpty)
					{
						noOfPacks = dataObject.TotalNoOfPacks;
						packageType = dataObject.TotalNoOfPacksPackageType.GetCodeAsUpperCase();
					}
					else if (dataObject.TotalNoOfPieces != null)
					{
						noOfPacks = dataObject.TotalNoOfPieces;
						packageType = Constants.PkgUnit.Piece;
					}
					isUnknownQty = noOfPacks.GetValueOrDefault() == 0 && dataObject.TotalNoOfPacks != null && dataObject.TotalNoOfPacks.Value == 0;

					if (packageType == Constants.PkgUnit.Container)
					{
						packageType = Constants.PkgUnit.Unit;
					}

					fNoOfPacksAndPackTypeFromJob = new KeyValuePair<ZInt?, ZString>(noOfPacks, packageType);
				}

				return fNoOfPacksAndPackTypeFromJob.Value;
			}
		}
		KeyValuePair<ZInt?, ZString>? fNoOfPacksAndPackTypeFromJob;
		bool isUnknownQty;

		#endregion

		#region PackageContainerLinks

		public Dictionary<ZInt, PkgPackage> PackageContainerLinks => packageContainerLinks ?? (packageContainerLinks = new Dictionary<ZInt, PkgPackage>());
		Dictionary<ZInt, PkgPackage> packageContainerLinks;

		#endregion

		#region PackageLinks

		public Dictionary<ZInt, PkgPackage> PackageLinks => packageLinks ?? (packageLinks = new Dictionary<ZInt, PkgPackage>());
		Dictionary<ZInt, PkgPackage> packageLinks;

		#endregion

		#region ThrowImportFailureBeforeImportChecks

		public static void ThrowImportFailureBeforeImportChecks(PkgPackageJob targetBO, DataObjectList<PackingLine> packingLineCollection)
		{
			if (packingLineCollection != null)
			{
				ThrowImportFailureIfDuplicateReferenceNumberInDataObject(packingLineCollection);
				ThrowImportFailureIfDuplicatePreviousPackageIDsInDataObject(targetBO, packingLineCollection);
			}
		}

		static void ThrowImportFailureIfDuplicateReferenceNumberInDataObject(DataObjectList<PackingLine> packingLineCollection)
		{
			var referenceNumbers = packingLineCollection.Select(x => x.ReferenceNumber ?? ZString.Empty).Where(x => !x.IsEmpty);
			var duplicates = GetDuplicateValues(referenceNumbers);
			if (duplicates.Any())
			{
				var duplicateReferenceNumberString = string.Join(",", duplicates);
				var message = Res.GetString("a73b2bf8-80ad-9883-4e80-663ac6f791a1", "There are Packing Line elements with the same Reference Number value. See Reference Number/s: {0}", duplicateReferenceNumberString);
				throw new DataObjectReadFailureException(message);
			}
		}

		static void ThrowImportFailureIfDuplicatePreviousPackageIDsInDataObject(PkgPackageJob targetBO, DataObjectList<PackingLine> packingLineCollection)
		{
			var previousPackageIDs = packingLineCollection.Select(x => PkgPackageDataObjectReader.GetPreviousPackageID(x)).Where(x => !x.IsEmpty);
			var existingReferenceNumbers = targetBO.Packages.Select(package => package.KP_PackageID);
			var duplicates = GetDuplicateValues(previousPackageIDs)
				.Where(duplicate => existingReferenceNumbers.Contains(duplicate));
			if (duplicates.Any())
			{
				var duplicatePreviousPackageIDString = string.Join(",", duplicates);
				var message = Res.GetString("1990968a-44c3-45a0-4dcf-7cc1beee4824", "There are Packing Line elements with the same Previous Package ID value. See Previous Package ID/s: {0}", duplicatePreviousPackageIDString);
				throw new DataObjectReadFailureException(message);
			}
		}

		static List<ZString> GetDuplicateValues(IEnumerable<ZString> values)
		{
			return values
				.GroupBy(x => x)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
				.ToList();
		}

		#endregion

		#region ThrowImportFailureIfDuplicatePackageIDsInBusinessObject

		public static void ThrowImportFailureIfDuplicatePackageIDsInBusinessObject(PkgPackageJob targetBO)
		{
			var packageIDs = targetBO.Packages.Select(t => t.KP_PackageID).Where(x => !x.IsEmpty);
			var duplicatePackageIDs = GetDuplicateValues(packageIDs);

			if (duplicatePackageIDs.Any())
			{
				var duplicatePackageIDString = string.Join(", ", duplicatePackageIDs);
				var message = Res.GetString("60061402-8836-e5b8-4fd4-1f922878ec82", "Package ID needs to be unique per job. See package ID/s: {0}", duplicatePackageIDString);
				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion
	}
}

