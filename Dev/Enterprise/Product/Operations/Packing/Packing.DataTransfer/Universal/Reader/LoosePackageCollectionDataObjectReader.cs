using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class LoosePackageCollectionDataObjectReader : DataObjectCollectionReader<PackingLine, PkgPackage>
	{
		#region Constructor

		public LoosePackageCollectionDataObjectReader(DataObjectList<PackingLine> dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory,
			PkgPackageJobDataObjectReader packageJobReader, PkgPackageJob packageJob, Func<PackingLine, PkgPackageDataObjectReader, PkgPackage> processFunc)
			: this(dataObjects, logger, factory, packageJobReader, null, packageJob, processFunc)
		{
		}

		public LoosePackageCollectionDataObjectReader(DataObjectList<PackingLine> dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory,
			PkgPackageJobDataObjectReader packageJobReader, PkgPackage loosePackageParent, Func<PackingLine, PkgPackageDataObjectReader, PkgPackage> processFunc)
			: this(dataObjects, logger, factory, packageJobReader, loosePackageParent, loosePackageParent.PackageJob, processFunc)
		{
		}

		LoosePackageCollectionDataObjectReader(DataObjectList<PackingLine> dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, PkgPackageJobDataObjectReader packageJobReader,
			PkgPackage loosePackageParent,
			PkgPackageJob packageJob,
			Func<PackingLine, PkgPackageDataObjectReader, PkgPackage> processFunc)
			: base(dataObjects)
		{
			Logger = Argument.NotNull(logger, nameof(logger));
			Factory = Argument.NotNull(factory, nameof(factory));
			PackageJob = Argument.NotNull(packageJob, nameof(packageJob));
			PackageJobReader = Argument.NotNull(packageJobReader, nameof(packageJobReader));
			ProcessFunc = Argument.NotNull(processFunc, nameof(processFunc));

			LoosePackageParent = loosePackageParent;
			if (LoosePackageParent != null && LoosePackageParent.IsContainer)
			{
				throw new ArgumentException("LoosePackageParent should be a Non-Container (Loose) Package.");
			}
		}

		readonly Func<PackingLine, PkgPackageDataObjectReader, PkgPackage> ProcessFunc;
		readonly IXmlImportLogger Logger;
		readonly UniversalObjectFactory Factory;
		readonly PkgPackageJobDataObjectReader PackageJobReader;
		readonly PkgPackageJob PackageJob;
		readonly PkgPackage LoosePackageParent;

		#endregion

		#region DefaultCollectionContent

		protected override CollectionContent DefaultCollectionContent => CollectionContent.Partial;

		#endregion

		#region BusinessObjects (Packages in Scope)

		/// <summary>
		/// Packages returned but not in the UXML PackingLine collection will be deleted if CollectionContent.Complete.
		/// </summary>
		protected override PkgPackage[] BusinessObjects
		{
			get { return PackagesInScope.Where(p => !p.IsContainer).ToArray(); }
		}

		PkgPackageCollection PackagesInScope
		{
			get { return LoosePackageParent != null ? LoosePackageParent.Packages : PackageJob.Packages; }
		}

		/// <summary>
		/// If Package is packed into a Container, return Container.Packages.
		/// If Package is packed into a Package, return ParentPackage.Packages.
		/// Otherwise return Top Level Packages Only. (ie. We do not move Packages in Containers to Top Level)
		/// </summary>
		PkgPackageCollection GetParentPackages(PackingLine packLineDO)
		{
			var parentContainer = GetContainerParent(packLineDO);
			return parentContainer?.Packages ?? PackagesInScope;
		}

		#endregion

		#region GetContainerParent

		PkgPackage GetContainerParent(PackingLine packageData)
		{
			PkgPackage container = null;

			if (!IsPackedIntoLoosePackage)
			{
				var containerLink = packageData.ContainerLink;
				if (containerLink.HasValue)
				{
					PackageJobReader.PackageContainerLinks.TryGetValue(containerLink.Value, out container);
				}

				// fallback if no Container Link
				if (container == null)
				{
					container = PackageJob.Containers.FirstOrDefault(c => c.KP_PackageID.Equals(packageData.ContainerNumber));
				}
			}

			return container;
		}

		#endregion

		#region IsPackedIntoLoosePackage

		bool IsPackedIntoLoosePackage => LoosePackageParent != null;

		#endregion

		#region Matching

		/// <summary>
		/// Return the existing Package that has already been Matched in 'SkipEntity'. This Package will be Updated with PackingLine data.
		/// </summary>
		protected override bool SkipEntity(PackingLine dataObject)
		{
			PkgPackage matchedPackageBO;
			matchedPackageBO = MatchPackageByPackingLine(dataObject);

			if (matchedPackageBO != null)
			{
				ProcessedPackageBOsByDO.Add(dataObject, matchedPackageBO);
			}

			return matchedPackageBO == null;
		}

		/// <summary>
		/// If we do NOT Find an Existing Package Match, "Skip the Update", but process later and import as a NEW Package. 
		/// </summary>
		protected override PkgPackage FindMatchingBusinessObject(PackingLine dataObject)
		{
			return ProcessedPackageBOsByDO.SingleOrDefault(c => c.Key == dataObject).Value;
		}

		PkgPackage MatchPackageByPackingLine(PackingLine packLineDO)
		{
			PkgPackage matchedPackage = null;

			// Match on package by Package Number.
			var packageNumber = packLineDO.ReferenceNumber.GetValueOrDefault();
			if (!packageNumber.IsEmpty)
			{
				matchedPackage = PackageJob.GetAllPackagesOnJob().Where(p => !p.IsContainer && p.KP_PackageID == packageNumber).SingleOrDefault();
			}

			// Match on package by pack type & pack qty.
			if (matchedPackage == null)
			{
				var parentPackageCollection = GetParentPackages(packLineDO);
				matchedPackage = parentPackageCollection.FirstOrDefault(p =>
						!p.IsContainer && p.KP_PackageID.IsEmpty
						&& p.KP_F3_NKPackType == packLineDO.PackType.GetCodeAsUpperCase()
						&& p.KP_PackageQty == packLineDO.PackQty
						&& !HasContainerAlreadyBeenMatched(p));
			}

			return matchedPackage;
		}

		bool HasContainerAlreadyBeenMatched(PkgPackage package) => ProcessedPackageBOsByDO.SingleOrDefault(c => c.Value.PK == package.PK).Value != null;

		#endregion

		#region ProcessedPackageBOsByDO

		Dictionary<PackingLine, PkgPackage> ProcessedPackageBOsByDO => processedPackageBOsByDO ?? (processedPackageBOsByDO = new Dictionary<PackingLine, PkgPackage>());
		Dictionary<PackingLine, PkgPackage> processedPackageBOsByDO;

		void AddProcessPackage(PackingLine dataObject, PkgPackage processedPackage)
		{
			if (!ProcessedPackageBOsByDO.ContainsKey(dataObject))
			{
				ProcessedPackageBOsByDO.Add(dataObject, processedPackage);
			}
		}

		#endregion

		#region ReadIntoPackage

		/// <summary>
		/// Update the Matched Package with PackingLineDO
		/// </summary>
		protected override PkgPackage ReadIntoBusinessObject(PackingLine dataObject, PkgPackage businessObject) => ReadIntoPackage(dataObject, targetObject: businessObject);

		PkgPackage ReadIntoPackage(PackingLine dataObject, PkgPackage targetObject = null)
		{
			var reader = GetPackageReader(dataObject, targetObject);
			MovePackageToContainerOrTopLevelIfRequired(dataObject, targetObject);

			var processedPackage = ProcessFunc != null ? ProcessFunc(dataObject, reader) : reader.ReadIntoBusinessObject();

			if (processedPackage != null)
			{
				AddProcessPackage(dataObject, processedPackage);
				PopulateInnerPackages(dataObject, processedPackage);
			}

			return processedPackage;
		}

		PkgPackageDataObjectReader GetPackageReader(PackingLine dataObject, PkgPackage targetObject)
		{
			PkgPackageDataObjectReader reader;

			var parentContainer = GetContainerParent(dataObject);
			var isPackageRORO = parentContainer != null && !parentContainer.IsContainer;
			if (isPackageRORO)
			{
				reader = new PkgPackageDataObjectReader(dataObject, Logger, Factory, parentContainer, ImportOption.PartialMatch, false);
			}
			else
			{
				var parentPackageCollection = GetParentPackages(dataObject);
				reader = new PkgPackageDataObjectReader(dataObject, Logger, Factory, PackageJob, parentPackageCollection, importOption: ImportOption.PartialMatch, targetPackage: targetObject, false, false, null, false);
			}

			return reader;
		}

		/// <summary>
		/// If Package is packed into a Container but Existing Package was not before, move it.
		/// If Package is not packed into a Container but Existing Package was before, move it.
		/// In all other "Move" cases, we will leave the package where it is.
		/// </summary>
		void MovePackageToContainerOrTopLevelIfRequired(PackingLine dataObject, PkgPackage matchingPackage)
		{
			var isExistingPackage = matchingPackage != null;
			if (isExistingPackage && !IsPackedIntoLoosePackage)
			{
				var parentContainer = GetContainerParent(dataObject);
				if (parentContainer != null && parentContainer.IsContainer && !parentContainer.Packages.Contains(matchingPackage))
				{
					matchingPackage.KP_KP_ParentPackage = parentContainer.PK;
				}
				else if (parentContainer == null && !matchingPackage.KP_KP_ParentPackage.IsEmpty)
				{
					matchingPackage.KP_KP_ParentPackage = ZGuid.Empty;
				}
			}
		}

		/// <summary>
		/// dataObject.PackingLineCollection == null	=	leave existing (don't touch)
		/// !dataObject.PackingLineCollection.Any()		=	Remove all existing packages
		/// dataObject.PackingLineCollection.Count == 1	=	remove all and add that 1
		/// </summary>
		void PopulateInnerPackages(PackingLine dataObject, PkgPackage processedPackage)
		{
			if (dataObject.PackingLineCollection != null)
			{
				var childDataObjects = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
				if (dataObject.PackingLineCollection.Any())
				{
					childDataObjects.AddRange(dataObject.PackingLineCollection);
				}

				var loosePackageCollectionDataObjectReader = new LoosePackageCollectionDataObjectReader(childDataObjects, Logger, Factory, PackageJobReader, processedPackage, ProcessFunc);
				loosePackageCollectionDataObjectReader.ReadIntoCollection();
			}
		}

		#endregion

		#region ProcessSkippedEntities

		void ProcessSkippedEntities(IEnumerable<PackingLine> skippedEntities)
		{
			if (skippedEntities != null)
			{
				foreach (var skippedPackage in skippedEntities)
				{
					ReadIntoPackage(skippedPackage);
				}
			}
		}

		#endregion

		#region ProcessSkippedEntities (unmatched when Partial/Complete)

		/// <summary>
		/// "SkippedEntities" are Packages that could not be matched to existing Packages. These are created as New Packages.
		/// </summary>
		protected override void ProcessSkippedEntities(IEnumerable<PackingLine> skippedEntities, IEnumerable<PkgPackage> unmatchedBusinessObjects)
		{
			ProcessSkippedEntities(skippedEntities);
		}

		/// <summary>
		/// If CollectionContent.Complete, we should use all unmatched PackingLineDO to create New Packages.
		/// </summary>
		protected override void ReadIntoCollectionCore()
		{
			var skippedEntities = DataObjects.Except(ProcessedPackageBOsByDO.Select(d => d.Key));
			ProcessSkippedEntities(skippedEntities);
		}

		#endregion

		#region Add/Remove To/From Collection

		protected override void AddToCollection(PkgPackage package)
		{
			// the package/container is added to the correct PackageJob/Package in 'PkgPackageDataObjectReader.cs'
		}

		protected override void RemoveFromCollection(PkgPackage package)
		{
			if (package.CanDelete) // ie. the Package is assigned to another Transport Booking's Instruction.
			{
				package.ClearActionStrategyCacheIncludingChildren();
				package.Delete();
			}
		}

		#endregion
	}
}
