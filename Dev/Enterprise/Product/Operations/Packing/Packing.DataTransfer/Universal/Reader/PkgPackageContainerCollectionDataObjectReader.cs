using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageContainerCollectionDataObjectReader : DataObjectCollectionReader<Container, PkgPackage>
	{
		#region Constructor

		public PkgPackageContainerCollectionDataObjectReader(DataObjectList<Container> containers, IXmlImportLogger logger, UniversalObjectFactory factory,
			PkgPackageCollection packageCollection, PkgPackageJobDataObjectReader packageJobReader = null)
			: base(containers)
		{
			Logger = Argument.NotNull(logger, nameof(logger));
			Factory = Argument.NotNull(factory, nameof(factory));
			PackageCollection = Argument.NotNull(packageCollection, nameof(packageCollection));
			PackageJobReader = packageJobReader;
		}

		readonly IXmlImportLogger Logger;
		readonly UniversalObjectFactory Factory;
		readonly PkgPackageCollection PackageCollection;
		readonly PkgPackageJobDataObjectReader PackageJobReader;

		#endregion

		#region DefaultCollectionContent

		protected override CollectionContent DefaultCollectionContent => CollectionContent.Partial;

		#endregion

		#region BusinessObjects (Containers in scope)

		/// <summary>
		/// Containers returned but not in the UXML Containers collection will be deleted if CollectionContent.Complete.
		/// </summary>
		protected override PkgPackage[] BusinessObjects => ContainersInScope.ToArray();

		IEnumerable<PkgPackage> ContainersInScope => PackageCollection.Where(p => p.IsContainer);

		#endregion

		#region Matching

		#region SkipEntity

		/// <summary>
		/// If we do NOT Find an Existing Container Match, "Skip the Update", but process later and import as a NEW Container. 
		/// </summary>
		protected override bool SkipEntity(Container dataObject)
		{
			var matchedPackageBO = MatchContainer(dataObject);
			if (matchedPackageBO != null)
			{
				ProcessedContainerBOsByDO.Add(dataObject, matchedPackageBO);
			}

			return matchedPackageBO == null;
		}

		#endregion

		#region FindMatchingBusinessObject

		/// <summary>
		/// Return the existing Container that has already been Matched in 'SkipEntity'. This Container will be Updated with ContainerDO data.
		/// </summary>
		protected override PkgPackage FindMatchingBusinessObject(Container containerDO)
		{
			return ProcessedContainerBOsByDO.SingleOrDefault(c => c.Key == containerDO).Value;
		}

		#endregion

		#region MatchContainer

		PkgPackage MatchContainer(Container containerDO)
		{
			PkgPackage matchedContainer = null;

			var containerNumber = containerDO.ContainerNumber.GetValueOrDefault();
			if (!containerNumber.IsEmpty)
			{
				matchedContainer = ContainersInScope.SingleOrDefault(p => p.KP_PackageID == containerNumber);
			}

			if (matchedContainer == null)
			{
				var containerType = containerDO.ContainerType.GetCodeAsUpperCase();
				var containerQty = containerDO.ContainerCount;
				matchedContainer = ContainersInScope.FirstOrDefault(p =>
						p.KP_PackageID.IsEmpty
						&& p.Container.ContainerType.RC_Code == containerType
						&& p.KP_PackageQty == containerQty
						&& !HasContainerAlreadyBeenMatched(p));
			}

			return matchedContainer;
		}

		bool HasContainerAlreadyBeenMatched(PkgPackage container)
		{
			return ProcessedContainerBOsByDO.SingleOrDefault(c => c.Value.PK == container.PK).Value != null;
		}

		#endregion

		#endregion

		#region ProcessedContainerBOsByDO

		Dictionary<Container, PkgPackage> ProcessedContainerBOsByDO
		{
			get { return processedContainerBOsByDO ?? (processedContainerBOsByDO = new Dictionary<Container, PkgPackage>()); }
		}
		Dictionary<Container, PkgPackage> processedContainerBOsByDO;

		void AddProcessedContainer(Container dataObject, PkgPackage processedContainer)
		{
			if (processedContainer != null && !ProcessedContainerBOsByDO.ContainsKey(dataObject))
			{
				ProcessedContainerBOsByDO.Add(dataObject, processedContainer);
			}
		}

		#endregion

		#region ReadIntoBusinessObject

		/// <summary>
		/// Update matched Containers with ContainerDO
		/// </summary>
		protected override PkgPackage ReadIntoBusinessObject(Container dataObject, PkgPackage matchedContainer)
		{
			return ReadIntoContainer(dataObject, targetObject: matchedContainer);
		}

		PkgPackage ReadIntoContainer(Container dataObject, PkgPackage targetObject = null)
		{
			var reader = new PkgPackageContainerDataObjectReader(dataObject, Logger, Factory, PackageCollection, targetObject);
			var processedContainer = ReadInContainer(dataObject, reader);
			AddProcessedContainer(dataObject, processedContainer);

			return processedContainer;
		}

		PkgPackage ReadInContainer(Container dataObject, PkgPackageContainerDataObjectReader reader = null)
		{
			return PackageJobReader != null
				? PackageJobReader.ReadIntoContainerAndPopulateContainerLinks(dataObject, reader)
				: reader.ReadIntoBusinessObject();
		}

		#endregion

		#region ProcessSkippedEntities (unmatched when Partial/Complete)

		/// <summary>
		/// If CollectionContent.Partial: "SkippedEntities" are Containers that could not be matched to existing Containers. These are created as New Containers.
		/// </summary>
		protected override void ProcessSkippedEntities(IEnumerable<Container> skippedEntities, IEnumerable<PkgPackage> unmatchedBusinessObjects)
		{
			ProcessUnmatchedEntities(skippedEntities);
		}

		/// <summary>
		/// If CollectionContent.Complete: Remaining Containers that are not processed (unmatached) to be created as New Containers.
		/// </summary>
		protected override void ReadIntoCollectionCore()
		{
			var unmatchedContainersSkipped = DataObjects.Except(ProcessedContainerBOsByDO.Select(d => d.Key));
			ProcessUnmatchedEntities(unmatchedContainersSkipped);
		}

		void ProcessUnmatchedEntities(IEnumerable<Container> unmatchedEntities)
		{
			if (unmatchedEntities != null)
			{
				foreach (var skipedContainer in unmatchedEntities)
				{
					ReadIntoContainer(skipedContainer);
				}
			}
		}

		#endregion

		#region Add/Remove To/From Collection

		protected override void AddToCollection(PkgPackage package)
		{
			// the package/container is added to the correct PackageJob/Package in 'PkgPackageContainerDataObjectReader.cs'
		}

		protected override void RemoveFromCollection(PkgPackage package)
		{
			if (package.CanDelete) // ie. the Container is assigned to another Transport Booking's Instruction.
			{
				package.ClearActionStrategyCacheIncludingChildren();
				package.Delete();
			}
		}

		#endregion
	}
}
