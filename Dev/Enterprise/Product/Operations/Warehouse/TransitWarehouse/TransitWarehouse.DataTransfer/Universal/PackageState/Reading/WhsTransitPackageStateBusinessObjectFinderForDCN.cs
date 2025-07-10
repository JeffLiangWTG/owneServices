using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitPackageStateBusinessObjectFinderForDCN : IWhsTransitPackageStateBusinessObjectFinder
	{
		public enum FindOption
		{
			ByDCN,
			ByPackageID,
			ByPackingLineID,
			ByRCN,
		}

		public WhsTransitPackageStateBusinessObjectFinderForDCN(WhsItemDispatchConsignment consignment, IXmlImportLogger logger, UniversalObjectFactory factory, FindOption findOption)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.findOption = findOption;
		}

		public WhsTransitPackageStateBusinessObjectFinderForDCN(UniversalShipment sourceDataObject, WhsItemDispatchConsignment consignment, IXmlImportLogger logger, UniversalObjectFactory factory, FindOption findOption) : this(consignment, logger, factory, findOption)
		{
			this.sourceDataObject = Argument.NotNull(sourceDataObject, nameof(sourceDataObject));
		}

		public WhsTransitPackageStateBusinessObjectFinderForDCN(UniversalShipment sourceDataObject, WhsItemDispatchConsignment consignment, IXmlImportLogger logger, UniversalObjectFactory factory, Func<UniversalShipment, WhsItemReceiveConsignment> findRCN, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo, FindOption findOption) : this(consignment, logger, factory, findOption)
		{
			this.sourceDataObject = Argument.NotNull(sourceDataObject, nameof(sourceDataObject));
			this.findRCN = findRCN;
			this.dcnWarningInfo = dcnWarningInfo;
		}

		readonly UniversalShipment sourceDataObject;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly WhsItemDispatchConsignment consignment;
		readonly Func<UniversalShipment, WhsItemReceiveConsignment> findRCN;
		readonly Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo;
		public FindOption findOption;

		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		#region FindPackageStates

		public IEnumerable<WhsItemPackageState> Find()
		{
			switch (findOption)
			{
				case FindOption.ByDCN:
					return FindExistingDCNPackageStates();
				case FindOption.ByPackageID:
					return FindPackageStatesForAttachByPackageID(sourceDataObject);
				case FindOption.ByPackingLineID:
					return FindPackageStatesForAttachByPackingLineID(sourceDataObject);
				case FindOption.ByRCN:
					return FindPackageStatesForAttachByRCN(sourceDataObject, findRCN, dcnWarningInfo);
				default:
					throw new ArgumentOutOfRangeException(nameof(findOption), findOption, null);
			}
		}

		IEnumerable<WhsItemPackageState> FindExistingDCNPackageStates()
		{
			var dispatchQuery = new ZQuery();
			dispatchQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignment.PK);
			var existingDCNPackageStates = factory.BOFactory.Load<WhsItemPackageState>(dispatchQuery);
			return existingDCNPackageStates;
		}

		IEnumerable<WhsItemPackageState> FindPackageStatesForAttachByPackageID(UniversalShipment sourceDataObject)
		{
			var packLinesWithPackageIds = sourceDataObject.PackingLineCollection?.Where(p =>
						p.PackQty.GetValueOrDefault() == 1 &&
						p.ReferenceNumber.HasValue && !p.ReferenceNumber.GetValueOrDefault().IsEmpty) ?? Enumerable.Empty<PackingLine>();

			if (packLinesWithPackageIds.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString("5f335804-281d-47c7-a2bc-3f2445e6982e",
					"Some Imported Packlines have Package IDs. Attempting to match by Package IDs."));

				var packagesToDispatch = GetPackagesToDispatchByPackageID(packLinesWithPackageIds);
				MatchInnerPacklines(packLinesWithPackageIds, packagesToDispatch.Select(c => c.GetValue(WhsItemPackageStateSchema.PK)), outerPacklineHasID: true);

				return packagesToDispatch;
			}

			return Enumerable.Empty<WhsItemPackageState>();
		}

		public IEnumerable<WhsItemPackageState> FindPackageStatesForAttachByPackingLineID(UniversalShipment sourceDataObject)
		{
			var packagesToDispatchByPacklineID = Enumerable.Empty<WhsItemPackageState>();

			var packLinesWithPackingLineIDs = sourceDataObject.PackingLineCollection?.Where(p =>
				p.PackType != null && !p.PackType.Code.GetValueOrDefault().IsEmpty
				&& p.PackQty.GetValueOrDefault() > 0
				&& p.PackingLineID.HasValue && !p.PackingLineID.GetValueOrDefault().IsEmpty
				&& p.ReferenceNumber.HasValue && p.ReferenceNumber.GetValueOrDefault().IsEmpty) ?? Enumerable.Empty<PackingLine>();
			if (packLinesWithPackingLineIDs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString("8364df57-2517-4acc-8f1b-50f159b8aab0",
					"Some Imported Packlines do not have Package IDs. Attempting to match with Packing Line ID."));

				packagesToDispatchByPacklineID = GetPackagesByPackingLineID(packLinesWithPackingLineIDs);
				if (packagesToDispatchByPacklineID.Any())
				{
					MatchInnerPacklines(packLinesWithPackingLineIDs, packagesToDispatchByPacklineID.Select(c => c.GetValue(WhsItemPackageStateSchema.PK)), outerPacklineHasID: false);
				}
			}

			return packagesToDispatchByPacklineID;
		}

		public IEnumerable<WhsItemPackageState> FindPackageStatesForAttachByRCN(UniversalShipment sourceDataObject, Func<UniversalShipment, WhsItemReceiveConsignment> findRCN, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo)
		{
			(var packLinesWithoutIds, var packLinesWithoutIdsAndTheirInnerGroupedByType) = GetPackLinesWithoutIdsAndGroupedByType(sourceDataObject, HandlerManager);

			if (packLinesWithoutIds.Any())
			{
				var warehouseBranchPK = consignment.Warehouse.WW_GB_RelatedCompanyBranch;
				var packageMatchingType = WarehouseDataRegistry.Instance.DispatchInstructionRCNPackageCountMatchingType.GetFallBackValueAtAllLevels(Guid.Empty, warehouseBranchPK.ToGuid(), Guid.Empty);

				logger.Log(LogType.Information, ResString.GetMultilingualString("89a49516-530a-4c89-af72-8a0a4d639588",
					"Some Imported Packlines do not have Package IDs. Registry setting set to {0}. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.", packageMatchingType));

				var rcn = findRCN(sourceDataObject);

				if (rcn != null)
				{
					return FindPackagesByRCNQtyAndTypeCore(packLinesWithoutIds, packLinesWithoutIdsAndTheirInnerGroupedByType, rcn, packageMatchingType, sourceDataObject.PackingLineCollection, dcnWarningInfo);
				}
			}

			return Enumerable.Empty<WhsItemPackageState>();
		}

		#endregion

		#region MatchInnerPacklines

		void MatchInnerPacklines(IEnumerable<PackingLine> packlines, IEnumerable<ZGuid> packageStatePKs, bool outerPacklineHasID)
		{
			var packlineWithInners = packlines.Where(p => p.PackingLineCollection != null && p.PackingLineCollection.Any());
			if (packlineWithInners.Any())
			{
				var packageMatchingData = GetPackageMatchingData(packageStatePKs);
				if (outerPacklineHasID)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("f6daefde-30ce-474a-b3d9-41e947c1748f",
						"Some Imported Packlines with Package IDs have Inner Packlines. Attempting to match Inner Packlines."));
					foreach (var packlineWithID in packlineWithInners)
					{
						var matchingDataWithID = packageMatchingData.FirstOrDefault(m => m.OuterPackageID == packlineWithID.ReferenceNumber.GetValueOrDefault());
						MatchInnerPacklinesCore(new List<PackingLine>() { packlineWithID }, new List<PackageMatchingData>() { matchingDataWithID }, outerPacklineHasID: true);
					}
				}
				else
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("e34798f3-67cb-402a-98f3-e34e02e1ad71",
						"Some Imported Packlines without Package IDs have Inner Packlines. Attempting to match Inner Packlines."));
					MatchInnerPacklinesCore(packlineWithInners, packageMatchingData, outerPacklineHasID: false);
				}
			}
		}

		List<PackageMatchingData> GetPackageMatchingData(IEnumerable<ZGuid> packageStatePKs)
		{
			// Use SQL because ZDBOnlySubQueries dont work without business objects (such as WhsItemPackageState)
			var sqlParams = new ZSqlParameterCollection();
			var packageCounter = 0;
			var packageParamsTextBuilder = new ZStringBuilder();

			foreach (var packageStatePK in packageStatePKs)
			{
				var packageIDParam = Invariant($"@PackagePK{packageCounter}"); // SQL Parameters - not localisable
				sqlParams.Add(packageIDParam, packageStatePK, WhsItemPackageStateSchema.PK);
				packageParamsTextBuilder.Append(packageIDParam);
				packageCounter++;
			}

			var packageParams = packageParamsTextBuilder.ToStringWithDelimiterBetweenAppends(", ");
			var sqlText = Invariant(
$@"
SELECT
	WPS_PK AS OuterPackageStatePK,
	WPS_UnitType AS OuterPackageStateUnitType,
	ISNULL(OuterHeader.KPH_PackageID, '') AS OuterPackageID,
	OuterPackage.KP_F3_NKPackType AS OuterPackType,
	OuterPackage.KP_PackageQty AS OuterPackQty,
	OuterPackage.KP_KJ_ParentPackageJob AS OuterPackageJob,
	OuterPackage.KP_PK AS OuterPackagePK,
	InnerPackagePK,
	ISNULL(InnerPackage.KPH_PackageID, '') AS InnerPackageID,
	InnerPackage.KP_F3_NKPackType AS InnerPackType,
	InnerPackage.KP_PackageQty AS InnerPackQty
FROM dbo.WhsItemPackageState
	JOIN dbo.PkgPackage OuterPackage ON WPS_KP_Package = KP_PK
	LEFT JOIN dbo.PkgPackageHeader OuterHeader ON KP_KPH_PackageHeader = KPH_PK
	LEFT JOIN 
	(
		SELECT
			KP_KP_ParentPackage AS ParentPK,
			KP_F3_NKPackType,
			KP_PackageQty,
			KP_PK AS InnerPackagePK,
			KPH_PackageID
		FROM dbo.PkgPackage
			LEFT JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
	) InnerPackage ON KP_PK = InnerPackage.ParentPK
WHERE WPS_PK IN ({packageParams})
");

			var packageStateData = new DynamicBusinessObjectCollection(factory.BOFactory);
			packageStateData.Load(sqlText, sqlParams);
			return packageStateData.GroupBy(p => (ZString)p[nameof(PackageMatchingData.OuterPackageID)]).Select(p => new PackageMatchingData()
			{
				OuterPackageID = p.Key,
				OuterPackagePK = (ZGuid)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackagePK)],
				OuterPackageJob = (ZGuid)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackageJob)],
				OuterPackQty = (ZInt)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackQty)],
				OuterPackType = (ZString)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackType)],
				OuterPackageStatePK = (ZGuid)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackageStatePK)],
				OuterPackageStateUnitType = (ZString)p.FirstOrDefault()[nameof(PackageMatchingData.OuterPackageStateUnitType)],
				InnerPackages = p.Select(innerp => new InnerPackageMatchingData()
				{
					InnerPackageID = (ZString)innerp[nameof(InnerPackageMatchingData.InnerPackageID)],
					InnerPackQty = (ZInt)innerp[nameof(InnerPackageMatchingData.InnerPackQty)],
					InnerPackType = (ZString)innerp[nameof(InnerPackageMatchingData.InnerPackType)],
					InnerPackagePK = (ZGuid)innerp[nameof(InnerPackageMatchingData.InnerPackagePK)]
				}).ToList()
			}).ToList();
		}

		void MatchInnerPacklinesCore(IEnumerable<PackingLine> packingLines, List<PackageMatchingData> packageMatchingData, bool outerPacklineHasID)
		{
			var errorMsg = new ZStringBuilder();
			if (outerPacklineHasID && packageMatchingData.Single().OuterPackageStateUnitType == PackageStateUnitType.Codes.Package)
			{
				MatchInnerPacklineWithPackageMatchingDataByQtyAndType(packingLines, packageMatchingData, errorMsg, true);
			}
			else
			{
				if (outerPacklineHasID)
				{
					var innerPackingLinesWithReference = packingLines.Single().PackingLineCollection.Where(p => !p.ReferenceNumber.GetValueOrDefault().IsEmpty && p.PackQty == 1);
					foreach (var innerPackingLine in innerPackingLinesWithReference)
					{
						var matchedInner = packageMatchingData.Single().InnerPackages.FirstOrDefault(imd => imd.InnerPackageID == innerPackingLine.ReferenceNumber.Value && imd.InnerPackQty == 1);
						if (matchedInner == null)
						{
							errorMsg.AppendLine(ResString.GetMultilingualString("d5b1ae75-e752-4ba3-80b2-325a6e0fb18f", "Could not import Dispatch Instruction because cannot find matched inner Package with ID {0}.", innerPackingLine.ReferenceNumber.Value));
						}
						else
						{
							var packagePK = matchedInner.InnerPackagePK;
							HandlerManager.AddMatchedPackagePKAndPackingLine(packagePK, innerPackingLine);
						}
					}
				}
				MatchInnerPacklineWithPackageMatchingDataByQtyAndType(packingLines, packageMatchingData, errorMsg, false);
			}
			if (errorMsg.Length > 0)
			{
				throw new DataObjectReadFailureException(errorMsg.ToString().TrimStart().TrimEnd());
			}
		}

		void MatchInnerPacklineWithPackageMatchingDataByQtyAndType(IEnumerable<PackingLine> packingLines, List<PackageMatchingData> packageMatchingData, ZStringBuilder errorMsg, bool matchingFromOverpack)
		{
			var needPopulatePackages = false;
			var innerPacklineTypeAndQty = packingLines.SelectMany(p => p.PackingLineCollection).GroupBy(p => p.PackType.Code.Value)
						.Select(g => (Type: g.Key, Qty: g.Sum(p => p.PackQty.Value)))
						.OrderBy(p => p.Type);
			var matchedTypeAndQty = packageMatchingData.SelectMany(md => md.InnerPackages).GroupBy(imd => imd.InnerPackType).Select(g => (Type: g.Key, Qty: g.Sum(p => p.InnerPackQty)))
						.OrderBy(p => p.Type);
			foreach (var typeGroup in innerPacklineTypeAndQty)
			{
				if (!matchedTypeAndQty.Any(m => m.Type == typeGroup.Type))
				{
					if (!matchingFromOverpack)
					{
						errorMsg.AppendLine(ResString.GetMultilingualString("e76a5bc9-daf6-48e5-8405-76de069f1024", "Could not import Dispatch Instruction because inner Packline of Type {0} cannot be matched.", typeGroup.Type));
					}
					else
					{
						needPopulatePackages = true;
					}
				}
				else
				{
					var matchedItem = matchedTypeAndQty.FirstOrDefault(m => m.Type == typeGroup.Type);
					if (typeGroup.Qty != matchedItem.Qty)
					{
						if (!matchingFromOverpack)
						{
							errorMsg.AppendLine(ResString.GetMultilingualString("c7fc671e-5fa2-4fde-a1e0-6eb95055a869", "Could not import Dispatch Instruction because inner Packline Qty of Type {0} is {1} which is different from the expected Qty {2}.", typeGroup.Type, matchedItem.Qty, typeGroup.Qty));
						}
						else
						{
							needPopulatePackages = true;
						}
					}
				}
			}
			if (needPopulatePackages)
			{
				var query = new ZQuery();
				query.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, packageMatchingData.Single().OuterPackagePK);

				var existingNonTrackedInners = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, query);
				var packagePks = new List<Guid>();
				foreach (var inner in existingNonTrackedInners)
				{
					var row = (IColumnIndexer)inner;
					packagePks.Add((Guid)inner[PkgPackageSchema.Constants.PK]);
					row.Delete();
				}

				if (packagePks.Any())
				{
					var packageBookedDetails = factory.Load<PkgPackageBookedDetail>(new ZQuery(PkgPackageBookedDetailSchema.KPB_KP_Package, packagePks));
					foreach (var detail in packageBookedDetails)
					{
						var row = (IColumnIndexer)detail;
						row.Delete();
					}
				}

				foreach (var packingLine in packingLines)
				{
					var packageJob = factory.Load<PkgPackageJob>(packageMatchingData.Single().OuterPackageJob);
					var targetPackage = factory.Load<PkgPackage>(packageMatchingData.Single().OuterPackagePK);
					var packageReader = new PkgPackageDataObjectReader(packingLine, logger, factory, packageJob, new PkgPackageCollection(packageJob), importOption: ImportOption.ImportInnersAsNonTrackableItem, targetPackage: targetPackage);
					packageReader.ReadIntoBusinessObject();
				}
			}
		}

		IEnumerable<WhsItemPackageState> GetPackagesToDispatchByPackageID(IEnumerable<PackingLine> packingLines)
		{
			if (!packingLines.Any())
			{
				return Enumerable.Empty<WhsItemPackageState>();
			}

			var packageStateData = GetMatchingPackageStates(packingLines);
			var sourcePackageIDs = packingLines.Select(pl => pl.ReferenceNumber.GetValueOrDefault().ToUpper());

			var validPackageStates = GetValidPackageStates(packageStateData, sourcePackageIDs);

			foreach (var packageState in packageStateData)
			{
				var packagePK = (ZGuid)packageState[WhsItemPackageStateSchema.WPS_KP_Package];
				var packageID = ((ZString)packageState[PkgPackageHeaderSchema.KPH_PackageID]).ToUpper();
				var packingLine = packingLines.FirstOrDefault(pl => pl.ReferenceNumber.GetValueOrDefault().ToUpper() == packageID);

				if (packingLine != null)
				{
					HandlerManager.AddMatchedPackagePKAndPackingLine(packagePK, packingLine);
				}
			}

			return validPackageStates;
		}

		DynamicBusinessObjectCollection GetMatchingPackageStates(IEnumerable<PackingLine> packLines, bool searchWithPackageID = true)
		{
			// Use SQL because ZDBOnlySubQueries dont work without business objects (such as WhsItemPackageState)
			var sqlParams = new ZSqlParameterCollection();
			var packageCounter = 0;
			var packageParamsTextBuilder = new ZStringBuilder();

			if (searchWithPackageID)
			{
				foreach (var packageID in packLines.Select(p => p.ReferenceNumber.GetValueOrDefault()).Where(p => !p.IsEmpty))
				{
					var packageIDParam = Invariant($"@PackageID{packageCounter}"); // SQL Parameters - not localisable
					sqlParams.Add(packageIDParam, packageID, PkgPackageHeaderSchema.KPH_PackageID);
					packageParamsTextBuilder.Append(packageIDParam);
					packageCounter++;
				}
			}
			else
			{
				foreach (var packageExternalReference in packLines.Select(p => p.PackingLineID.GetValueOrDefault()).Where(p => !p.IsEmpty))
				{
					var packageIDParam = Invariant($"@PackageExternalReference{packageCounter}"); // SQL Parameters - not localisable
					sqlParams.Add(packageIDParam, packageExternalReference, PkgPackageSchema.KP_ExternalReference);
					packageParamsTextBuilder.Append(packageIDParam);
					packageCounter++;
				}
			}

			sqlParams.Add("@DispatchPK", consignment.PK, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment);
			sqlParams.Add("@CurrentWhsPK", consignment.WDC_WW_Warehouse, WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse);

			var packageParams = packageParamsTextBuilder.ToStringWithDelimiterBetweenAppends(", ");
			var searchingColumn = searchWithPackageID ? PkgPackageHeaderSchema.KPH_PackageID.Name : PkgPackageSchema.KP_ExternalReference.Name;
			var indexSupportQuery = searchWithPackageID ? string.Empty : $"AND {PkgPackageSchema.KP_ExternalReference.Name} <> ''";
			var packageHeaderJoinParam = searchWithPackageID ? "JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK" : string.Empty;
			var sqlText = Invariant(
$@"
-- Macthing Packages booked or currently in Warehouse, or Matching Dispatch Consignment
SELECT
	WPS_PK,
	{searchingColumn},
	WPS_KP_Package,
	WPS_WDC_TransitDispatchConsignment,
	WDC_ConsignmentID
FROM
	dbo.WhsItemPackageState 
	JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
	{packageHeaderJoinParam}
	LEFT JOIN dbo.WhsItemDispatchConsignment ON WPS_WDC_TransitDispatchConsignment = WDC_PK
WHERE
	WPS_WDC_TransitDispatchConsignment = @DispatchPK
UNION
SELECT
	WPS_PK,
	{searchingColumn},
	WPS_KP_Package,
	WPS_WDC_TransitDispatchConsignment,
	WDC_ConsignmentID
FROM
	dbo.WhsItemPackageState 
	JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
	{packageHeaderJoinParam}
	LEFT JOIN dbo.WhsItemDispatchConsignment ON WPS_WDC_TransitDispatchConsignment = WDC_PK
WHERE
	{searchingColumn} IN ({packageParams})
    {indexSupportQuery}
	AND WPS_WW_Warehouse = @CurrentWhsPK
	AND WPS_Status NOT IN ('{TransitWarehouseStatuses.Codes.Departed}', '{TransitWarehouseStatuses.Codes.AdjustedOut}', '{TransitWarehouseStatuses.Codes.Finalized}')
");

			var packageStateData = new DynamicBusinessObjectCollection(factory.BOFactory);
			packageStateData.Load(sqlText, sqlParams);

			return packageStateData;
		}

		IEnumerable<WhsItemPackageState> GetValidPackageStates(DynamicBusinessObjectCollection packageStateData, IEnumerable<ZString> sourcePackageIDs)
		{
			var consignmentPK = consignment.PK;
			var validPackageStateData = packageStateData.Where(p => sourcePackageIDs.Contains(p[PkgPackageHeaderSchema.KPH_PackageID].ToString().ToUpper())
																	&& ((ZGuid)p[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] == ZGuid.Empty || (ZGuid)p[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] == consignmentPK));

			var groupedPackageIDs = validPackageStateData.ToLookup(p => ((ZString)p[PkgPackageHeaderSchema.KPH_PackageID]).ToUpper());
			var duplicatedIDs = groupedPackageIDs.Where(g => g.Count() > 1).Select(g => g.Key).Distinct();
			var invalidPackageIDs = sourcePackageIDs.Where(id => !groupedPackageIDs.Contains(id)).ToList();

			if (duplicatedIDs.Any() || invalidPackageIDs.Any())
			{
				var errorMessage = new ZStringBuilder();
				errorMessage.Append(Res.GetString("0c6ca781-7101-4baf-97ef-82c36cc02b30", "Failed to match valid Packages in Transit Warehouse '{0}'.", consignment.Warehouse.WW_WarehouseCode));

				AppendErrorForMatchedPackageIDDuplicated(duplicatedIDs, errorMessage);

				var packageDataWithDifferentDCNs = packageStateData.Where(p => (ZGuid)p[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] != ZGuid.Empty && (ZGuid)p[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] != consignmentPK);
				AppendErrorForPackagesOnOtherDCNs(packageDataWithDifferentDCNs, errorMessage);

				var notFoundPackageIDs = invalidPackageIDs.Where(id => !packageDataWithDifferentDCNs.Select(p => (ZString)p[PkgPackageHeaderSchema.KPH_PackageID]).Contains(id));
				AppendErrorForNotFoundPackages(notFoundPackageIDs, errorMessage);

				throw new DataObjectReadFailureException(errorMessage.ToString());
			}

			var packageStatePKs = validPackageStateData.Select(p => (ZGuid)p[WhsItemPackageStateSchema.PK]);

			return factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packageStatePKs));
		}

		void AppendErrorForMatchedPackageIDDuplicated(IEnumerable<ZString> duplicatedIDs, ZStringBuilder errorMessage)
		{
			if (duplicatedIDs.Any())
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine();
				errorMessage.Append(Res.GetString("a3c21f7e-04dd-4596-b227-d390ea048b6e",
@"The following Package IDs matched multiple Packages with the same ID: {0}
These Packages may be relabeled via Transit Warehouse.", string.Join(", ", duplicatedIDs.OrderBy(id => id))));
			}
		}

		void AppendErrorForPackagesOnOtherDCNs(IEnumerable<DynamicBusinessObject> packageDataWithDifferentDCNs, ZStringBuilder errorMessage)
		{
			if (packageDataWithDifferentDCNs.Any())
			{
				var packageIDsAndDCNIDs = packageDataWithDifferentDCNs.Select(p => (ZString)(p[PkgPackageHeaderSchema.KPH_PackageID].ToString() + " - " + p[WhsItemDispatchConsignmentSchema.WDC_ConsignmentID].ToString())).OrderBy(s => s).ToList();
				errorMessage.AppendLine();
				errorMessage.AppendLine();
				errorMessage.Append(Res.GetString("8a57db48-4a82-4abe-84de-cc4bfab4bfef",
@"The following Packages are currently assigned to a different Dispatch Consignment:
(Package ID - Dispatch Consignment ID): {0}
To dispatch these Packages on this Dispatch Consignment, first remove the Packages from the other Dispatch Consignments.
You can do this via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse.", FormatPackageIDsIfOverFive(packageIDsAndDCNIDs)));
			}
		}

		void AppendErrorForNotFoundPackages(IEnumerable<ZString> notFoundPackageIDs, ZStringBuilder errorMessage)
		{
			if (notFoundPackageIDs.Any())
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine();
				errorMessage.Append(Res.GetString("c6400c6c-dbf9-4fee-b6d4-63a11691ea23",
@"Could not find the following Package IDs: {0}
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.", string.Join(", ", notFoundPackageIDs)));
			}
		}

		ZString FormatPackageIDsIfOverFive(IEnumerable<ZString> list)
		{
			if (list.Count() > 5)
			{
				return string.Join(", ", list.Take(5)) + "...";
			}
			else
			{
				return string.Join(", ", list);
			}
		}

		#endregion

		#region FindByPackingLineID

		IEnumerable<WhsItemPackageState> GetPackagesByPackingLineID(IEnumerable<PackingLine> packingLines)
		{
			var result = new List<WhsItemPackageState>();
			var packageStateData = GetMatchingPackageStates(packingLines, false);
			var packageStatePKs = packageStateData.Select(p => (ZGuid)p[WhsItemPackageStateSchema.PK]);
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packageStatePKs));
			foreach (var packageState in packageStates)
			{
				var packagePK = packageState.WPS_KP_Package;
				var packingLineID = packageState.Package.KP_ExternalReference.ToUpper();
				var packingLine = packingLines.FirstOrDefault(pl => pl.PackingLineID.GetValueOrDefault().ToUpper() == packingLineID);

				if (packingLine != null)
				{
					HandlerManager.AddMatchedPackagePKAndPackingLine(packagePK, packingLine);
					result.Add(packageState);
					HandlerManager.AddMatchedPackingLinesByPackingLineID(packingLine);
				}
			}

			return result;
		}

		#endregion

		#region FindByRCNQtyAndType

		IEnumerable<WhsItemPackageState> FindPackagesByRCNQtyAndTypeCore(IEnumerable<PackingLine> packLinesWithoutIds, IEnumerable<(ZString Type, long Qty)> packLinesWithoutIdsByType, WhsItemReceiveConsignment receiveConsignment, string packageMatchingType, DataObjectList<PackingLine> packinglines, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo)
		{
			var rcnPackagesByType = Array.Empty<(ZString Type, long Qty)>();
			var rcnPackagesByPKTypeQty = Array.Empty<(ZGuid PK, ZString Type, long Qty)>();

			var rcnPackagesNotDispatchingOnAnotherDCN = GetRCNPackagesNotDispatchingOnAnotherDCN(receiveConsignment);
			if (rcnPackagesNotDispatchingOnAnotherDCN.Any())
			{
				var packageQuery = new ZQuery(PkgPackageSchema.PK, rcnPackagesNotDispatchingOnAnotherDCN.Select(p => p.WPS_KP_Package));
				var innerNontrackedQuery = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, rcnPackagesNotDispatchingOnAnotherDCN.Select(p => p.WPS_KP_Package));
				packageQuery.AddToFilter(innerNontrackedQuery, JoinCondition.Or);
				var rcnPackages = factory.BOFactory.Load<PkgPackage>(packageQuery);
				rcnPackagesByType = rcnPackages.GroupBy(p => p.KP_F3_NKPackType).Select(g => (Type: g.Key, Qty: (long)g.Sum(p => p.KP_PackageQty))).ToArray();
				rcnPackagesByPKTypeQty = rcnPackages.Select(p => (PK: p.PK, Type: p.KP_F3_NKPackType, Qty: (long)p.KP_PackageQty)).ToArray();
			}

			var (totalQtyMatched, qtyOfEachTypeMatched) = MatchPackline(packLinesWithoutIdsByType, rcnPackagesByType);

			var (canAttachPackage, message, warningNoteMessage) = GetPacklineMatchingResult(packLinesWithoutIdsByType, rcnPackagesByType, packageMatchingType, totalQtyMatched, qtyOfEachTypeMatched);

			MatchInnerPacklines(packLinesWithoutIds, rcnPackagesNotDispatchingOnAnotherDCN.Select(c => c.PK), outerPacklineHasID: false);

			if (canAttachPackage)
			{
				if (!string.IsNullOrEmpty(message))
				{
					logger.Log(LogType.Warning, message);
				}
				if (!string.IsNullOrEmpty(warningNoteMessage) && dcnWarningInfo != null)
				{
					var dcnID = consignment.WDC_ConsignmentID;
					foreach (var packingline in packinglines)
					{
						if (!dcnWarningInfo.ContainsKey(packingline))
						{
							var info = new DispatchInstructionWarningInfo();
							dcnWarningInfo.Add(packingline, info);
						}
						dcnWarningInfo[packingline].DCNIDsWithPackageMatchingDiscrepancy.Add(dcnID);
					}
				}

				foreach (var packingLine in packinglines)
				{
					if (packingLine.PackType.Code.HasValue)
					{
						var packagePK = rcnPackagesByPKTypeQty.Where(p => p.Type == packingLine.PackType.Code.Value && p.Qty == packingLine.PackQty).Select(p => p.PK).FirstOrDefault();

						HandlerManager.AddMatchedPackagePKAndPackingLine(packagePK, packingLine);
					}
				}
			}
			else
			{
				throw new DataObjectReadFailureException(message);
			}
			return rcnPackagesNotDispatchingOnAnotherDCN;
		}

		IEnumerable<WhsItemPackageState> GetRCNPackagesNotDispatchingOnAnotherDCN(WhsItemReceiveConsignment receiveConsignment)
		{
			IEnumerable<WhsItemPackageState> result;

			if (receiveConsignment != null)
			{
				var packageStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);

				// find packages attached to the receive consignment that are either attached to this dispatch or no dispatch
				var dispatchQuery = new ZQuery();
				dispatchQuery.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignment.PK);
				dispatchQuery.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, null);
				packageStateQuery.AddToFilter(dispatchQuery, JoinCondition.And);
				if (HandlerManager.GetPackagesAlreadyOnDCN().Any())
				{
					packageStateQuery.AddToFilter(WhsItemPackageStateSchema.PK, SQLComparisonOperator.NotEqual, HandlerManager.GetPackagesAlreadyOnDCN());
				}
				result = factory.BOFactory.Load<WhsItemPackageState>(packageStateQuery);

				// needs to return packagestate and package type
			}
			else
			{
				result = Enumerable.Empty<WhsItemPackageState>();
			}

			return result;
		}

		static (bool totalQtyMatched, bool qtyOfEachTypeMatched) MatchPackline(IEnumerable<(ZString Type, long Qty)> packLinesWithoutIdsByType, IEnumerable<(ZString Type, long Qty)> rcnPackagesByType)
		{
			var totalQtyMatched = rcnPackagesByType.Sum(p => p.Qty) == packLinesWithoutIdsByType.Sum(p => p.Qty);
			var qtyOfEachTypeMatched = rcnPackagesByType.Count() == packLinesWithoutIdsByType.Count();

			if (qtyOfEachTypeMatched)
			{
				foreach (var rcnTypeAndQty in rcnPackagesByType)
				{
					var packLineTypeAndQty = packLinesWithoutIdsByType.SingleOrDefault(p => p.Type == rcnTypeAndQty.Type);
					if (packLineTypeAndQty.Type.IsEmpty || rcnTypeAndQty.Qty != packLineTypeAndQty.Qty)
					{
						qtyOfEachTypeMatched = false;
						break;
					}
				}
			}

			return (totalQtyMatched, qtyOfEachTypeMatched);
		}

		(bool CanAttachPackage, ZString Message, ZString WarningNoteMessage) GetPacklineMatchingResult(IEnumerable<(ZString Type, long Qty)> packLinesGroupedByType,
			IEnumerable<(ZString Type, long Qty)> packagesGroupedByType,
			string packageMatchingType,
			bool totalQtyMatched,
			bool qtyOfEachTypeMatched)
		{
			if (totalQtyMatched && qtyOfEachTypeMatched)
			{
				return (true, "", "");
			}
			else
			{
				var packagesWithError = GetPackageTableMessage(packagesGroupedByType, packLinesGroupedByType);

				switch (packageMatchingType)
				{
					case DispatchInstructionRCNPackageCountMatchingTypesList.Codes.Typical:
						{
							return (false,
								Res.GetString("6a0762d5-151c-4965-ba61-d996806bb411",
								"Could not import Dispatch Instruction because RCN Packline Quantity and Pack Type discrepancy:{0}", packagesWithError) + System.Environment.NewLine +
								Res.GetString("abede6ef-1616-4004-959e-6b400162190a", "Ensure your Dispatch Instruction matches the RCN Quantity and Pack Type, or dispatch packages by package ID."),
								"");
						}
					case DispatchInstructionRCNPackageCountMatchingTypesList.Codes.Count:
						{
							if (totalQtyMatched)
							{
								return (true,
									Res.GetString("e759e6d8-8479-4691-870e-d4d086bd67ef",
									"RCN Packline Quantity and Pack Type discrepancy found:{0}", packagesWithError) + System.Environment.NewLine +
									Res.GetString("cab721f4-50e5-48a9-8c71-09622c9a2226", "Current Package Matching Type is Count and the Packline's total count matches, continue Dispatch Instruction import."),
									Res.GetString("f3006b91-29fc-4bc8-b739-32543f58638e", "Dispatch Instruction's Package Type and Qty is not fully matched with the RCN's packline when imported."));
							}
							else
							{
								return (false,
									Res.GetString("c2825c56-754c-446b-9749-3b319c6dd6ef",
									"RCN Packline Quantity and Pack Type discrepancy found:{0}", packagesWithError) + System.Environment.NewLine +
									Res.GetString("b3ed70eb-bcfd-4b9a-b12f-51942572cb02", "Current Package Matching Type is Count but the Packline's total count does not match, could not import Dispatch Instruction."),
									"");
							}
						}
					case DispatchInstructionRCNPackageCountMatchingTypesList.Codes.HBL:
						{
							return (true,
								Res.GetString("c62c2e2b-fff8-4018-9ea7-1353c6610bb0",
								"RCN Packline Quantity and Pack Type discrepancy found:{0}", packagesWithError) + System.Environment.NewLine +
								Res.GetString("300dacb4-0159-4b54-a508-548c500a49f8", "Current Package Matching Type is HBL, continue Dispatch Instruction import."),
								Res.GetString("f24e8ee3-219f-491d-b7cb-d37c75de597f", "Dispatch Instruction's Package Type and Qty is not fully matched with the RCN's packline when imported."));
						}
					default:
						{
							return (false, Res.GetString("106de51a-a7df-4d26-97b0-42b9e935b9bc", "Could not import Dispatch Instruction because of Undefined Package Matching Type. Please go to Registry -> Warehouse -> Transit Warehouse -> Dispatch Instruction RCN Package Count Matching Type and set the matching type."), "");
						}
				}
			}
		}

		public static (IEnumerable<PackingLine> packLinesWithoutIds, IEnumerable<(ZString Type, long Qty)> packLinesWithoutIdsByType) GetPackLinesWithoutIdsAndGroupedByType(UniversalShipment sourceDataObject, TransitDataObjectReaderHandlerManager handlerManager)
		{
			var packLinesWithoutIds = sourceDataObject.PackingLineCollection?.Where(p =>
				!handlerManager.GetMatchedPackingLinesByPackingLineID().Contains(p) &&
				p.PackType != null && !p.PackType.Code.GetValueOrDefault().IsEmpty
				&& p.PackQty.GetValueOrDefault() > 0
				&& (p.ReferenceNumber.GetValueOrDefault().IsEmpty || p.PackQty.GetValueOrDefault() > 1)
			) ?? Enumerable.Empty<PackingLine>();

			var packLinesWithoutIdsByType = packLinesWithoutIds
				.Concat(packLinesWithoutIds.Where(p => p.PackingLineCollection != null).SelectMany(p => p.PackingLineCollection))
				.GroupBy(p => p.PackType.Code.Value)
				.Select(g => (Type: g.Key, Qty: g.Sum(p => p.PackQty.Value)))
				.OrderBy(p => p.Type);

			return (packLinesWithoutIds, packLinesWithoutIdsByType);
		}

		static string GetPackageTableMessage(IEnumerable<(ZString Type, long Qty)> packagesGroupedByType, IEnumerable<(ZString Type, long Qty)> packLinesTypeAndQty)
		{
			var packLinesTypeAndQtyList = packLinesTypeAndQty.ToList();
			var packagesWithError = AddTableRow(new ZString("RCN"), new ZString((NoResString)"Dispatch Instruction"));// To log message for pack types and qtys
			foreach (var rcnPackageByTypeAndQty in packagesGroupedByType.OrderBy(p => p.Type))
			{
				var rcnPackageType = rcnPackageByTypeAndQty.Type;
				var rcnPackageQty = rcnPackageByTypeAndQty.Qty;
				var matchingPackLineTypeAndQty = packLinesTypeAndQtyList.FirstOrDefault(p => p.Type == rcnPackageType);
				if (!matchingPackLineTypeAndQty.Type.IsEmpty)
				{
					packagesWithError += AddTableRow($"{rcnPackageQty} {rcnPackageType}", $"{matchingPackLineTypeAndQty.Qty} {rcnPackageType}"); // To log message for pack types and qtys
					packLinesTypeAndQtyList.Remove(matchingPackLineTypeAndQty);
				}
				else
				{
					packagesWithError += AddTableRow($"{rcnPackageQty} {rcnPackageType}", $"0 {rcnPackageType}"); // To log message for pack types and qtys
				}
			}

			var remaining = packLinesTypeAndQtyList.OrderBy(p => p.Type);
			foreach (var packLineTypeAndQty in remaining)
			{
				packagesWithError += AddTableRow($"0 {packLineTypeAndQty.Type}", $"{packLineTypeAndQty.Qty} {packLineTypeAndQty.Type}");// To log message for pack types and qtys
			}

			return packagesWithError;
		}

		static string AddTableRow(ZString column1, ZString column2)
		{
			return $"{System.Environment.NewLine}{column1.PadRight(15)}{column2}";
		}

		#endregion

		#region Shared

		public WhsItemPackageStateDTO Find(PackingLine packingLine)
		{
			throw new NotImplementedException();
		}

		class PackageMatchingData
		{
			public ZGuid OuterPackageStatePK { get; set; }
			public ZString OuterPackageStateUnitType { get; set; }
			public ZString OuterPackageID { get; set; }
			public ZString OuterPackType { get; set; }
			public ZInt OuterPackQty { get; set; }
			public List<InnerPackageMatchingData> InnerPackages { get; set; }
			public ZGuid OuterPackageJob { get; set; }
			public ZGuid OuterPackagePK { get; set; }
		}

		class InnerPackageMatchingData
		{
			public ZString InnerPackageID { get; set; }
			public ZString InnerPackType { get; set; }
			public ZInt InnerPackQty { get; set; }
			public ZGuid InnerPackagePK { get; set; }
		}

		#endregion
	}
}
