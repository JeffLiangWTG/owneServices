using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitPackageStateBusinessObjectFinderForRCN : IWhsTransitPackageStateBusinessObjectFinder
	{
		public WhsTransitPackageStateBusinessObjectFinderForRCN(WhsItemReceiveConsignment consignment, UniversalObjectFactory factory, IXmlImportLogger logger, UniversalShipment sourceDataObject, UniversalShipment dataObject)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.sourceDataObject = Argument.NotNull(sourceDataObject, nameof(sourceDataObject));
			this.dataObject = Argument.NotNull(dataObject, nameof(dataObject));

			matchOnPackageIDs = sourceDataObject.PackingLineCollection != null && sourceDataObject.PackingLineCollection.Any(p => !p.ReferenceNumber.GetValueOrDefault().IsEmpty);
			packingLinesHasPackageID = GetPackingLineHasPackageID(sourceDataObject);
		}

		public WhsTransitPackageStateBusinessObjectFinderForRCN(WhsItemReceiveConsignment consignment, UniversalObjectFactory factory)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly UniversalObjectFactory factory;
		readonly IXmlImportLogger logger;
		readonly WhsItemReceiveConsignment consignment;
		readonly DataObjectList<PackingLine> packingLinesHasPackageID;
		readonly UniversalShipment sourceDataObject;
		readonly UniversalShipment dataObject;
		readonly bool matchOnPackageIDs;

		UniversalShipment topLeveDO;
		UniversalShipment TopLeveDO => topLeveDO ??= TransitUniversalExtensions.GetTopLevelDataObject(logger);
		bool IsArrivalTransitWarehouse => TopLeveDO.IsArrivalTransitWarehouse();
		bool IsDepartureTransitWarehouse => TopLeveDO.IsDepartureTransitWarehouse();

		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		PkgPackageJobDataObjectReader PackageJobDataObjectReader
		{
			get
			{
				if (packageJobDataObjectReader == null)
				{
					IPackageParentDataObject packageParentDataObject = new PackingSourceDataObject(null, packingLinesHasPackageID, sourceDataObject);
					var dataSourceCollection = dataObject?.DataContext?.DataSourceCollection;
					var isFromForwarding = dataSourceCollection.IsFromForwardingConsol() || dataSourceCollection.IsFromForwardingShipment();

					if ((packageParentDataObject.PackingLineCollection != null && packageParentDataObject.PackingLineCollection.Count > 0) || isFromForwarding)
					{
						packageParentDataObject.OuterPacks = null;
						packageParentDataObject.TotalNoOfPacks = null;
						packageParentDataObject.TotalWeight = null;
						packageParentDataObject.TotalVolume = null;
					}

					var isPackingLineLinkNeedsTobeCorrected = packingLinesHasPackageID.Any(p => !p.Link.HasValue) || packingLinesHasPackageID.Select(p => p.Link).Distinct().Count() != packingLinesHasPackageID.Count;
					if (isPackingLineLinkNeedsTobeCorrected)
					{
						MakeSureAllPackingLinesHasLinks(packingLinesHasPackageID.ToList());
					}

					var importOptions = matchOnPackageIDs ? ImportOption.KeepUnmatchedPackages : ImportOption.Default;

					packageJobDataObjectReader = new PkgPackageJobDataObjectReader(packageParentDataObject, logger, factory, consignment, importOptions);
				}

				return packageJobDataObjectReader;
			}
		}
		PkgPackageJobDataObjectReader packageJobDataObjectReader;

		List<PkgPackage> ProcessedPackages
		{
			get
			{
				if (processedPackages == null)
				{
					var pkgPackageJob = PackageJobDataObjectReader.ReadIntoBusinessObject();
					var query = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, pkgPackageJob.PK);
					processedPackages = factory.BOFactory.Load<PkgPackage>(query).ToList();
					var linksForPackages = new Dictionary<ZGuid, ZInt>();
					PopulateLinksForPackages(consignment, packageJobDataObjectReader, linksForPackages);
					GroupPackagesByContainerLink(processedPackages, packingLinesHasPackageID.ToList(), linksForPackages);
				}
				return processedPackages;
			}
		}
		List<PkgPackage> processedPackages;

		List<PkgPackage> NoLinkPackages
		{
			get
			{
				if (noLinkPackages == null)
				{
					noLinkPackages = ProcessedPackages.Except(PackageJobDataObjectReader.PackageLinks.Values).ToList();
				}
				return noLinkPackages;
			}
		}
		List<PkgPackage> noLinkPackages;

		IEnumerable<PkgPackageHandlingUnitDivot> Divots
		{
			get
			{
				if (divots == null)
				{
					var divotsQuery = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, ProcessedPackages.Select(p => p.PK));
					divotsQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);
					divots = factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(divotsQuery);
				}
				return divots;
			}
		}
		IEnumerable<PkgPackageHandlingUnitDivot> divots;

		ZGuid? DispatchConsignmentPK
		{
			get
			{
				if (dispatchConsignmentPK == null)
				{
					if (!matchOnPackageIDs)
					{
						var packageStates = new UniversalObjectFactory().BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, consignment.PK));
						var dispatchConsignmentPKs = packageStates.Select(p => p.WPS_WDC_TransitDispatchConsignment).Where(pk => !pk.IsEmpty).Distinct();
						if (dispatchConsignmentPKs.Count() <= 1)
						{
							dispatchConsignmentPK = dispatchConsignmentPKs.SingleOrDefault();
						}
						else
						{
							var dcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.PK, dispatchConsignmentPKs);
							var dcns = factory.BOFactory.Load<WhsItemDispatchConsignment>(dcnQuery);

							var logHelper = new DispatchConsignmentColumnIndexerTransitLogHelper(factory);
							var errorMessageStringBuilder = new ZStringBuilder();
							errorMessageStringBuilder.Append(
								logHelper.GetTable(
									Res.GetString("4e55810f-59be-48f2-aae5-041a73b71b31", "Found multiple Dispatch Consignments :"),
									dcns,
									TransitLogColumnIDs.DCNIndexerColumn.DCN)
								);
							errorMessageStringBuilder.AppendLine(System.Environment.NewLine + Res.GetString("067d6a84-f72d-47e6-8ff9-e5c167d02368", "Package IDs are required for imports to Receive Consignments with more than one Dispatch Consignment. Either add Package IDs or remove the Packages from the other Dispatch Consignments. Packages can be removed from via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse."));

							throw new DataObjectReadFailureException(errorMessageStringBuilder.ToString().TrimEnd());
						}
					}
				}

				return dispatchConsignmentPK;
			}
		}
		ZGuid? dispatchConsignmentPK;

		public IEnumerable<WhsItemPackageState> Find()
		{
			return factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, consignment.PK));
		}

		public WhsItemPackageStateDTO Find(PackingLine packingLine)
		{
			WhsItemPackageStateDTO result = null;
			if (ProcessedPackages.Any())
			{
				result = new WhsItemPackageStateDTO();
				result.IsNew = true;
				var package = MatchPackageForPackingLine(packingLine);

				result.WPS_KP_Package = package.PK;
				result.WPS_WRC_TransitReceiveConsignment = consignment.PK;
				result.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
				result.WPS_WW_Warehouse = consignment.WRC_WW_IntendedWarehouse;
				result.WPS_WDC_TransitDispatchConsignment = DispatchConsignmentPK;

				var isOutPackage = package.KP_KP_ParentPackage.IsEmpty && !Divots.Any(d => d.KPD_KP_Package == package.PK);

				if (isOutPackage)
				{
					var isParent = Divots.Any(d => d.KPD_KP_HandlingUnit == package.PK);
					var packageQty = package.KP_PackageQty;
					var hasPackageID = !package.KP_KPH_PackageHeader.IsEmpty;

					result.WPS_UnitType =
						packageQty == 1 && isParent && hasPackageID ? PackageStateUnitType.Codes.Overpack :
						packageQty > 1 ? PackageStateUnitType.Codes.PackLine :
						PackageStateUnitType.Codes.Package;
				}
				else
				{
					var parentPK = package.KP_KP_ParentPackage.IsEmpty ? Divots.Single(d => d.KPD_KP_Package == package.PK).KPD_KP_HandlingUnit : package.KP_KP_ParentPackage;
					result.KP_KP_ParentPackage = parentPK;

					var parentPackage = ProcessedPackages.Single(p => p.PK == parentPK);
					var parentPackageHasPackageID = !parentPackage.KP_KPH_PackageHeader.IsEmpty;
					if (parentPackageHasPackageID)
					{
						var packageQty = package.KP_PackageQty;
						result.WPS_UnitType = packageQty > 1 ? PackageStateUnitType.Codes.PackLine : PackageStateUnitType.Codes.Package;
					}
					else
					{
						result.IsNew = false;
					}
				}

				result.WPS_IsHighRisk = packingLine == null ? false : packingLine.IsHighRisk.GetValueOrDefault();
				result.WPS_IsHandlingUnit = result.WPS_UnitType == PackageStateUnitType.Codes.Overpack;
			}

			return result;
		}

		PkgPackage MatchPackageForPackingLine(PackingLine packingLine)
		{
			PkgPackage package = null;

			if (packingLine == null)
			{
				package = ProcessedPackages.FirstOrDefault();
				NoLinkPackages.Remove(package);
				ProcessedPackages.Remove(package);
			}
			else
			{
				package = PackageJobDataObjectReader.PackageLinks.FirstOrDefault(p => p.Key == packingLine.Link).Value;

				if (package == null)
				{
					package = NoLinkPackages.FirstOrDefault(p => packingLine.ReferenceNumber.HasValue && !packingLine.ReferenceNumber.Value.IsEmpty && p.KP_PackageID == packingLine.ReferenceNumber.Value);
					NoLinkPackages.Remove(package);
					ProcessedPackages.Remove(package);
				}

				if (package == null)
				{
					package = NoLinkPackages.FirstOrDefault(p => packingLine.PackType != null && p.KP_F3_NKPackType == (ZString)packingLine.PackType.Code);
					NoLinkPackages.Remove(package);
					ProcessedPackages.Remove(package);
				}

				if (package == null)
				{
					package = NoLinkPackages.FirstOrDefault(p => p.KP_Weight == packingLine.Weight && p.KP_Width == packingLine.Width && p.KP_Length == packingLine.Length) ?? NoLinkPackages.FirstOrDefault();
					NoLinkPackages.Remove(package);
					ProcessedPackages.Remove(package);
				}

				if (package == null)
				{
					package = ProcessedPackages.FirstOrDefault();
					ProcessedPackages.Remove(package);
				}
			}

			return package;
		}

		void GroupPackagesByContainerLink(List<PkgPackage> packages, List<PackingLine> filteredPackages, Dictionary<ZGuid, ZInt> linksForPackages)
		{
			if (IsDepartureTransitWarehouse || linksForPackages.Count == 0)
			{
				HandlerManager.AddPackagesByContainerLink(-1, new List<PkgPackage>(packages));
			}
			else if (IsArrivalTransitWarehouse)
			{
				foreach (var package in packages)
				{
					AssignPackageToContainer(package, linksForPackages, filteredPackages);
				}
			}
		}

		void AssignPackageToContainer(PkgPackage package, Dictionary<ZGuid, ZInt> linksForPackages, List<PackingLine> filteredPackages)
		{
			var packagePK = package.GetValue(PkgPackageSchema.PK);
			if (linksForPackages.ContainsKey(packagePK))
			{
				var packingLink = linksForPackages[packagePK];
				var packingDataObject = filteredPackages.SingleOrDefault(p => p.Link == packingLink);
				if (packingDataObject != null)
				{
					var containerLink = packingDataObject.ContainerLink ?? -1;
					HandlerManager.AddPackagesByContainerLink(containerLink, [package]);
				}
			}
		}

		void MakeSureAllPackingLinesHasLinks(List<PackingLine> packingLineCollection)
		{
			var existingLinks = packingLineCollection.Where(p => p.Link.HasValue).Select(p => p.Link.Value).Distinct();
			var linkCodes = new HashSet<ZInt>(existingLinks);
			var i = 1;

			foreach (var packingLine in packingLineCollection.ToArray())
			{
				while (linkCodes.Contains(i))
				{
					i++;
				}
				packingLine.Link = i;
				linkCodes.Add(i++);
			}
		}

		DataObjectList<PackingLine> GetPackingLineHasPackageID(UniversalShipment sourceDataObject)
		{
			var result = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };

			if (sourceDataObject.PackingLineCollection != null)
			{
				var hasLoosePackageIds = sourceDataObject.PackingLineCollection.Any(p => p.IsLoosePackageIDDataObject());
				foreach (var package in sourceDataObject.PackingLineCollection)
				{
					if (!hasLoosePackageIds
						|| (hasLoosePackageIds && package.ReferenceNumber.HasValue && !package.ReferenceNumber.Value.IsEmpty))
					{
						result.Add(package);
					}
				}
			}

			return result;
		}

		#region PopulateLinksForPackages

		void PopulateLinksForPackages(WhsItemReceiveConsignment consignment, PkgPackageJobDataObjectReader packageJobReader, Dictionary<ZGuid, ZInt> linksForPackages)
		{
			var duplicatePackageIDs = new HashSet<ZString>();
			foreach (var packageLinkKeyValuePair in packageJobReader.PackageLinks)
			{
				if (!linksForPackages.ContainsKey(packageLinkKeyValuePair.Value.PK))
				{
					linksForPackages.Add(packageLinkKeyValuePair.Value.PK, packageLinkKeyValuePair.Key);
				}
				else
				{
					duplicatePackageIDs.Add(packageLinkKeyValuePair.Value.KP_PackageID);
				}
			}

			ReportErrorForDuplicatePackagesIfNeeded(duplicatePackageIDs, consignment.WRC_ConsignmentID);
		}

		static void ReportErrorForDuplicatePackagesIfNeeded(IEnumerable<ZString> duplicatePackageIDs, ZString consignmentID)
		{
			if (duplicatePackageIDs.Any())
			{
				var errorTextBuilder = new ZStringBuilder();
				foreach (var duplicatePackageID in duplicatePackageIDs)
				{
					errorTextBuilder.Append(Res.GetString("394c83f6-1c0b-40fc-a7d7-d6c79c62c4aa", "Reference number {0} is used for more than one package on a shipment {1}. Please provide a unique reference number or leave it empty.", duplicatePackageID, consignmentID));
				}
				throw new DataObjectReadFailureException(errorTextBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion
	}
}
