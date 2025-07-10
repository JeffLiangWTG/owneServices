using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUNDG = Enterprise.UniversalDataBuss.DataObjects.Universal.UNDG;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentDataObjectSplitPackageHelper
	{
		readonly UniversalShipment shipmentDataObject;
		readonly ForwardingShipment shipmentBO;
		readonly OrgAddress transitWarehouseAddress;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly ShipmentDataObjectReadingHelper shipmentDataObjectReadingHelper;

		public ShipmentDataObjectSplitPackageHelper(UniversalShipment shipmentDataObject, ForwardingShipment shipmentBO, OrgAddress transitWarehouseAddress, IXmlImportLogger logger, UniversalObjectFactory factory, ShipmentDataObjectReadingHelper shipmentDataObjectReadingHelper)
		{
			this.shipmentDataObject = Argument.NotNull(shipmentDataObject, nameof(shipmentDataObject));
			this.shipmentBO = Argument.NotNull(shipmentBO, nameof(shipmentBO));
			this.transitWarehouseAddress = Argument.NotNull(transitWarehouseAddress, nameof(transitWarehouseAddress));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.shipmentDataObjectReadingHelper = Argument.NotNull(shipmentDataObjectReadingHelper, nameof(shipmentDataObjectReadingHelper));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		const string ReasonChangingSecurityInspectionStatus =
			"Inspection at Shipment level Changed by Transit Warehouse package update";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		const string ReasonChangingSecurityAdditionalInspectionStatus =
			"Additional Inspection at Package level Changed by Transit Warehouse package update";

		#region Dispatch

		public void ProcessTransitDispatch()
		{
			Process();
			AddPKCEvents();
		}

		void ProcessTransitDispatchPackages(IEnumerable<UniversalPackingLine> universalPackagesToBeImported, IEnumerable<ForwardingPackage> packagesToBeImported, IDictionary<ZGuid, IList<ZString>> packagePreviousPackLinesDictionary)
		{
			var unprocessedPacklines = GetUnprocessedPacklines();

			var existedPacklines = GetExistedPacklines();
			var isDepartureTransitWarehouse = CheckIsDepartureTransitWarehouse();

			var packageWrappers = PackageWrapper.WrapPackagesForDispatch(packagesToBeImported, existedPacklines.ToList(), universalPackagesToBeImported, isDepartureTransitWarehouse).ToList();
			RemovePackageWrappers(packageWrappers, p => (p.ForwardingPackLineBO?.JL_LastKnownTransitWarehouseStatus).GetValueOrDefault() == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, unprocessedPacklines);

			foreach (var packageWrapperGroup in packageWrappers.ToLookup(p => p.GroupKey))
			{
				DispatchSplitPackages(packageWrapperGroup.ToList(), unprocessedPacklines, packagePreviousPackLinesDictionary);
			}

			foreach (var packLineBO in unprocessedPacklines.Values)
			{
				ShortShipRemainingPacklines(packLineBO);
			}
		}

		internal IEnumerable<UniversalPackingLine> GetValidImportedPackages()
		{
			if (shipmentDataObject.PackingLineCollection == null)
			{
				return Enumerable.Empty<UniversalPackingLine>();
			}

			var localPackageLastKnownTimeMap = GetLocalPackageLastKnownTimeMap();

			return shipmentDataObject.PackingLineCollection.Where(packingLine =>
				IsValidImportedPackage(packingLine, isOuterPackingLine: true) &&
				(!localPackageLastKnownTimeMap.TryGetValue(packingLine.GetPackageIDWithFallbackToPacklineID(), out var localPackageLastKnownTime) ||
					CompareLocalAndUtcTime(
						localPackageLastKnownTime,
						(
							packingLine.GetLastKnownTransitWarehouseStatusDateTime(),
							ConvertToUtcDateTime(packingLine.GetLastKnownTransitWarehouseStatusDateTime(), transitWarehouseAddress))
						) <= 0
			));
		}

		static bool IsValidImportedPackage(UniversalPackingLine packingLine, bool isOuterPackingLine)
		{
			return (!isOuterPackingLine || !packingLine.GetPackageIDWithFallbackToPacklineID().IsEmpty) && packingLine.OutturnQty.GetValueOrDefault() > 0 && !packingLine.GetLastKnownTransitWarehouseStatusDateTime().IsEmpty;
		}

		Dictionary<ZString, (ZDateTime localTime, ZDateTime utcTime)> GetLocalPackageLastKnownTimeMap()
		{
			var localPackageLastKnownTimeMap = new Dictionary<ZString, (ZDateTime localTime, ZDateTime utcTime)>();

			foreach (var packLine in shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_LastKnownTransitWarehouseStatusDateTime.IsValid && p.JL_OA_LastKnownTransitWarehouseAddress != transitWarehouseAddress.PK))
			{
				var packLineOrgAddress = packLine.LastKnownTransitWarehouseAddress;
				var lastKnowTwLocalAndUtcTime = (packLine.JL_LastKnownTransitWarehouseStatusDateTime, ConvertToUtcDateTime(packLine.JL_LastKnownTransitWarehouseStatusDateTime, packLineOrgAddress));

				foreach (var package in packLine.PkgPackageCollection.Where(pkg => !pkg.PackageIDWithFallbackToExternalReference.IsEmpty))
				{
					if (!localPackageLastKnownTimeMap.ContainsKey(package.PackageIDWithFallbackToExternalReference))
					{
						localPackageLastKnownTimeMap.Add(package.PackageIDWithFallbackToExternalReference, lastKnowTwLocalAndUtcTime);
					}
					else if (CompareLocalAndUtcTime(localPackageLastKnownTimeMap[package.PackageIDWithFallbackToExternalReference], lastKnowTwLocalAndUtcTime) < 0)
					{
						localPackageLastKnownTimeMap[package.PackageIDWithFallbackToExternalReference] = lastKnowTwLocalAndUtcTime;
					}
				}
			}

			return localPackageLastKnownTimeMap;
		}

		static int CompareLocalAndUtcTime((ZDateTime localTime, ZDateTime utcTime) dateTime1, (ZDateTime localTime, ZDateTime utcTime) dateTime2)
		{
			return !dateTime1.utcTime.IsEmpty && !dateTime2.utcTime.IsEmpty ?
				DateTime.Compare(dateTime1.utcTime.ToDateTime(), dateTime2.utcTime.ToDateTime()) :
				DateTime.Compare(dateTime1.localTime.ToDateTime(), dateTime2.localTime.ToDateTime());
		}

		static ZDateTime ConvertToUtcDateTime(ZDateTime dateTime, OrgAddress orgAddress)
		{
			return orgAddress == null || orgAddress.OA_RL_NKRelatedPortCode.IsEmpty ? ZDateTime.Empty :
				Env.Time.GetUtcFromUnlocoTime(orgAddress.OA_RL_NKRelatedPortCode, dateTime.ToDateTime());
		}

		static IEnumerable<UniversalPackingLine> GetValidImportedInnerPackages(UniversalPackingLine packingLine)
		{
			return packingLine.HasInnerPackingLines()
				? packingLine.PackingLineCollection.Where(l => IsValidImportedPackage(l, isOuterPackingLine: false))
				: Enumerable.Empty<UniversalPackingLine>();
		}

		(ForwardingPackageJob PackageJob, IDictionary<ZGuid, IList<ZString>> PackagePreviousPackLinesDictionary) ReadPkgPackageJob(IEnumerable<UniversalPackingLine> universalPackagesToBeImported)
		{
			var wrapper = new PackagesWrapper(shipmentDataObject, new DataObjectList<UniversalPackingLine>(universalPackagesToBeImported));
			var isComplete = shipmentDataObject.PackingLineCollection?.Content == CollectionContent.Complete;
			var forwardingPkgPackageJobDataObjectReader = new ForwardingPkgPackageJobDataObjectReader(wrapper, logger, factory, shipmentBO, isComplete, transitWarehouseAddress, GetValidImportedInnerPackages);
			return (forwardingPkgPackageJobDataObjectReader.ReadIntoBusinessObject(), forwardingPkgPackageJobDataObjectReader.PackagePreviousPackLinesDictionary);
		}

		void DispatchSplitPackages(IList<PackageWrapper> packageWrappers, Dictionary<ZString, ForwardingPackLine> unprocessedPacklines, IDictionary<ZGuid, IList<ZString>> packagePreviousPackLinesDictionary)
		{
			if (packageWrappers == null || packageWrappers.Count == 0)
			{
				return;
			}

			var firstPackageWrapper = packageWrappers[0];
			if (firstPackageWrapper.ForwardingPackLineBO == null)
			{
				GroupAndImportUnmatchedPackages(packageWrappers, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, packagePreviousPackLinesDictionary);
				return;
			}

			unprocessedPacklines.Remove(firstPackageWrapper.GroupKey);

			var originalPackLine = firstPackageWrapper.ForwardingPackLineBO;
			if (originalPackLine.JL_LastKnownTransitWarehouseStatus != FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received)
			{
				return;
			}

			var importedPackageWrapperDic = packageWrappers.Where(p => p.IsImported)
				.GroupBy(p => GetAutoSplitKey(p.PackingLineDO, screeningMethod: GetScreeningMethodKey(p.PackingLineDO), isHighRisk: GetIsHighRisk(p.PackingLineDO), additionalScreeningMethod: GetAdditionalScreeningMethodKey(p.PackingLineDO)))
				.ToDictionary(p => p.Key, p => p.ToList());
			if (!importedPackageWrapperDic.Any())
			{
				return;
			}

			var isDispatchingAllPackages = packageWrappers.All(pkg => pkg.IsImported);
			var originalPackLineContainerNum = originalPackLine.GetContainer(shipmentDataObjectReadingHelper.LinkManager.Consol)?.JC_ContainerNum ?? string.Empty;

			var debugInfo = GetDebugOringinalPackLineInfo(packageWrappers);
			debugInfo.AppendLine($"originalPackLineContainerNum: {originalPackLineContainerNum}");

			var wrapperGroupKeyForOriginalPackLine = GetNotSplitPackageWrappersGroupKey(originalPackLine, importedPackageWrapperDic);
			if (isDispatchingAllPackages)
			{
				wrapperGroupKeyForOriginalPackLine = importedPackageWrapperDic.First().Key;
				var wrappersForOriginalPackline = importedPackageWrapperDic.First().Value;
				var newPackages = wrappersForOriginalPackline
					.Where(p => !originalPackLine.PkgPackageCollection.Contains(p.PackageBO)).Select(p => p.PackageBO).ToList();
				newPackages.ForEach(pkg => CheckIfPackageExistInAnyPacklines(pkg));
				originalPackLine.PkgPackageCollection.AddRange(newPackages);
				UpdatePackLineWithImportedPackage(originalPackLine, wrapperGroupKeyForOriginalPackLine.Value.ContainerNum, packageWrappers, wrappersForOriginalPackline);
				originalPackLine.JL_Damaged = originalPackLine.PkgPackageCollection_TotalDamagedPacks;
			}

			var wrapperGroupCollectionForSplitPackline = importedPackageWrapperDic.Where(p => wrapperGroupKeyForOriginalPackLine == null || p.Key != wrapperGroupKeyForOriginalPackLine.Value);
			foreach (var wrapperPairForSplitPackline in wrapperGroupCollectionForSplitPackline)
			{
				wrapperPairForSplitPackline.Value.ForEach(p => originalPackLine.PkgPackageCollection.RemoveFromRelationship(p.PackageBO));

				var splitPackline = (ForwardingPackLine)originalPackLine.Clone();
				PackLine.PopulatePackLineIdIfNeeded(splitPackline.Factory, new[] { splitPackline });
				splitPackline.JL_RefNumber = originalPackLine.JL_RefNumber;
				splitPackline.JL_OriginTransitWarehouseStatus = originalPackLine.JL_OriginTransitWarehouseStatus;

				shipmentBO.OuterPackLines.Add(splitPackline);

				var newPackages = wrapperPairForSplitPackline.Value.Select(p => p.PackageBO).ToList();
				newPackages.ForEach(pkg => CheckIfPackageExistInAnyPacklines(pkg));
				splitPackline.PkgPackageCollection.AddRange(newPackages);

				SyncPackLineFromPackage(splitPackline, wrapperPairForSplitPackline.Value.First().PackageBO);
				UpdatePackLineWithImportedPackage(splitPackline, wrapperPairForSplitPackline.Key.ContainerNum, packageWrappers, wrapperPairForSplitPackline.Value);

				if (!isDispatchingAllPackages && !originalPackLineContainerNum.IsEmpty && originalPackLineContainerNum == wrapperPairForSplitPackline.Key.ContainerNum)
				{
					originalPackLine.Containers.RemoveAll();
				}
				CheckPacklineActualWeightAndRelatedContainerGrossWeight(splitPackline);
			}

			UpdatePacklineInspectionTypeCodeFromPkgPackage(originalPackLine);
			UpdatePacklineAdditionalInspectionTypeCodeFromPkgPackage(originalPackLine);

			if (wrapperGroupCollectionForSplitPackline.Any())
			{
				try
				{
					SyncPackLineFromPackage(originalPackLine, originalPackLine.PkgPackageCollection[0]);
					TrySetDiscrepenciesStatus(originalPackLine, packageWrappers);
				}
				catch (ArgumentOutOfRangeException ex)
				{
					ErrorReporter.ReportOnce("Transit Warehouse dispatch packline index out of range", debugInfo.ToString(), ex);
				}
			}
		}

		ZStringBuilder GetDebugOringinalPackLineInfo(IList<PackageWrapper> packageWrappers)
		{
			var debugInfo = new ZStringBuilder();
			debugInfo.AppendLine($"shipment_id: {shipmentBO.JS_UniqueConsignRef}");
			for (var i = 0; i < packageWrappers.Count; i++)
			{
				debugInfo.AppendLine($"---PackageWrapper: {i}---");
				var packageWrapper = packageWrappers[i];
				if (packageWrapper.IsImported)
				{
					var packlineDO = packageWrapper.PackingLineDO;
					debugInfo.AppendLine($"packlineDO_{i}.ContainerNumber: {GetContainerNumber(packlineDO)}");
					debugInfo.AppendLine($"packlineDO_{i}.screeningMethod: {GetScreeningMethodKey(packlineDO)}");
					debugInfo.AppendLine($"packlineDO_{i}.isHighRisk: {GetIsHighRisk(packlineDO)}");
					debugInfo.AppendLine($"packlineDO_{i}.additionalScreeningMethod: {GetAdditionalScreeningMethodKey(packlineDO)}");
					debugInfo.AppendLine($"packageWrapper_{i}.IsImported: {packageWrapper.IsImported}");
				}
			}

			var originalPackline = packageWrappers[0].ForwardingPackLineBO;
			debugInfo.AppendLine($"originalPackline.PkgPackageCollection.Count: {originalPackline.PkgPackageCollection.Count}");
			for (var i = 0; i < originalPackline.PkgPackageCollection.Count; i++)
			{
				var package = originalPackline.PkgPackageCollection[i];
				debugInfo.AppendLine($"---OriginalPacklinePackageCollection: {i}---");
				debugInfo.AppendLine($"package.KP_F3_NKPackType_{i}: {package.KP_F3_NKPackType}");
				debugInfo.AppendLine($"package.KP_PackageID)_{i}: {package.KP_PackageID}");
			}

			return debugInfo;
		}

		bool IsScreeningMethodApplicable()
		{
			return (shipmentBO.JS_TransportMode == Core.Constants.TransportModes.Air || shipmentBO.JS_TransportMode == Core.Constants.TransportModes.AirSea)
				&& (shipmentDataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air || shipmentDataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.AirSea)
				&& (shipmentBO.OuterPackLines.Cast<PackLine>().Any(p => !p.JL_InspectionTypeCodeInfo.ReadOnly) || shipmentBO.RequiresSecuredCargoFromWarehouse);
		}

#if DEBUG
		public
#endif
		ZString GetScreeningMethodKey(ForwardingPackLine packLine)
		{
			return IsScreeningMethodApplicable()
				? packLine?.JL_InspectionTypeCode ?? ZString.Empty
				: ZString.Empty;
		}

		ZString GetScreeningMethodKey(UniversalPackingLine packLine)
		{
			return IsScreeningMethodApplicable()
				? packLine.ScreeningMethod.GetValueOrDefault()
				: ZString.Empty;
		}

		void UpdatePacklineInspectionTypeCodeFromPkgPackage(ForwardingPackLine packLine)
		{
			packLine.JL_InspectionTypeCode = PackLine.GetCalculatedInspectionTypeCode(packLine, packLine.PkgPackageCollection.FirstOrDefault());
		}

		bool IsAdditionalScreeningMethodApplicable
		{
			get
			{
				if (!isAdditionalScreeningMethodApplicable.HasValue)
				{
					isAdditionalScreeningMethodApplicable = (shipmentBO.JS_TransportMode == Core.Constants.TransportModes.Air || shipmentBO.JS_TransportMode == Core.Constants.TransportModes.AirSea)
						&& (shipmentDataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air || shipmentDataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.AirSea)
						&& shipmentBO.AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable
						&& shipmentBO.OuterPackLines.Cast<PackLine>().Any(p => !p.JL_IsHighRiskInfo.ReadOnly);
				}

				return isAdditionalScreeningMethodApplicable.Value;
			}
		}
		bool? isAdditionalScreeningMethodApplicable;

		ZString GetAdditionalScreeningMethodKey(UniversalPackingLine packLine)
		{
			if (IsAdditionalScreeningMethodApplicable)
			{
				var aviationSecurityAdditionalInspectionType = (packLine.AviationSecurityAdditionalInspectionType?.Code).GetValueOrDefault();
				return aviationSecurityAdditionalInspectionType.IsEmpty ? shipmentBO.GetAdditionalInspectionTypeCodeDefault() : aviationSecurityAdditionalInspectionType;
			}

			return ZString.Empty;
		}

		ZString GetAdditionalScreeningMethodKey(ForwardingPackLine packLine)
		{
			return IsAdditionalScreeningMethodApplicable
				? packLine.JL_AdditionalInspectionTypeCode
				: ZString.Empty;
		}

		ZBool GetIsHighRisk(UniversalPackingLine packLine)
		{
			return IsAdditionalScreeningMethodApplicable && packLine.IsHighRisk.GetValueOrDefault();
		}

		ZBool GetIsHighRisk(ForwardingPackLine packLine)
		{
			return IsAdditionalScreeningMethodApplicable && packLine.JL_IsHighRisk;
		}

		void UpdatePacklineAdditionalInspectionTypeCodeFromPkgPackage(ForwardingPackLine packLine)
		{
			packLine.JL_IsHighRisk = PackLine.GetPackageIsHighRisk(packLine, packLine.PkgPackageCollection.FirstOrDefault());
			packLine.JL_AdditionalInspectionTypeCode = PackLine.GetCalculatedAdditionalInspectionTypeCode(packLine, packLine.PkgPackageCollection.FirstOrDefault());
		}

		void UpdatePackLineWithImportedPackage(ForwardingPackLine packline, ZString newContainerNum, IList<PackageWrapper> totalPackageWrappers, List<PackageWrapper> wrapperGroupForOriginalPackline)
		{
			TrySetPacklineContainer(packline, newContainerNum);
			SetLastKnownReceiptInformation(packline, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, GetLastKnownTransitWarehouseStatusDateTime(wrapperGroupForOriginalPackline.Select(wrapper => wrapper.PackingLineDO).ToList()));
			TrySetDiscrepenciesStatus(packline, totalPackageWrappers);
		}

		void TrySetDiscrepenciesStatus(ForwardingPackLine matchedPackLine, IList<PackageWrapper> packageWrappers)
		{
			var packagesDOs = GetImportedPackingLines(matchedPackLine, packageWrappers).ToList();
			if (packagesDOs.Any())
			{
				SetDiscrepenciesStatus(packagesDOs, matchedPackLine);
			}
		}

		void TrySetPacklineContainer(ForwardingPackLine packline, ZString containerNum)
		{
			if (!containerNum.IsEmpty)
			{
				SetPacklineContainer(packline, containerNum);
			}
		}

		#endregion

		#region Receive

		public void ProcessTransitReceive()
		{
			Process();
			AddUPCEvents();
		}

		void GroupAndImportUnmatchedPackages(IList<PackageWrapper> unmatchedPackages, ZString lastKnownTransitWarehouseStatus, IDictionary<ZGuid, IList<ZString>> packagePreviousPackLinesDictionary)
		{
			var groupedPacklinePackagesDOs = GetGroupedPacklines(unmatchedPackages.Where(p => p.IsImported).Select(p => p.PackingLineDO).ToList(), IsScreeningMethodApplicable(), isOuterPackingLine: true)
						.WhereNotNull()
						.ToDictionary(e => e.Item1, e => e.Item2);
			if (groupedPacklinePackagesDOs.Count == 0)
			{
				return;
			}

			var packLineDOToPackLineBOMap = new Dictionary<UniversalPackingLine, ForwardingPackLine>();
			var readingContext = new ForwardingPackingLineCollectionReadingContext
			{
				PackingLineDataObjectCollection = new DataObjectList<UniversalPackingLine>(groupedPacklinePackagesDOs.Keys) { Content = CollectionContent.Partial },
				Logger = logger,
				Factory = factory,
				ShipmentBO = shipmentBO,
				ContainerLinkManager = shipmentDataObjectReadingHelper.LinkManager,
				OrderLineLinkManager = shipmentDataObjectReadingHelper.OrderLineLinkManager,
				PackLineBOToPackingLineDOMap = packLineDOToPackLineBOMap,
				DisableMatchOfExistingPackLine = true
			};

			var forwardingPackingLineCollectionReader = new ForwardingPackingLineCollectionReader(readingContext);
			forwardingPackingLineCollectionReader.ReadIntoCollection();
			shipmentBO.UpdateInspectionTypeFromPackLines();

			var importedPackagesDictionary = unmatchedPackages.Select(p => p.PackageBO).ToDictionary(p => p.PackageIDWithFallbackToExternalReference);
			var updateShipmentTotals = false;
			var packLinesToDelete = new List<ForwardingPackLine>();
			foreach (var packlinePair in groupedPacklinePackagesDOs)
			{
				var packlineBO = packLineDOToPackLineBOMap[packlinePair.Key];
				var newPackages = packlinePair.Value.Select(p => importedPackagesDictionary[p.GetPackageIDWithFallbackToPacklineID()]).ToList();
				newPackages.ForEach(pkg => CheckIfPackageExistInAnyPacklines(pkg));
				packlineBO.PkgPackageCollection.AddRange(newPackages);

				var (hasOverpack, originTransitWarehouseStatus, originalPackLines) = HandleOverpackIfNeeded(packlineBO, packagePreviousPackLinesDictionary);
				if (hasOverpack)
				{
					foreach (var package in packlineBO.PkgPackageCollection)
					{
						PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots(package)
							.ForEach(innerPackage => packlineBO.SetUNDGsFromPackage(innerPackage, package, false));
					}
				}
				updateShipmentTotals |= hasOverpack;
				packLinesToDelete.AddRange(originalPackLines);
				SetLastKnownReceiptInformation(packlineBO, lastKnownTransitWarehouseStatus, GetLastKnownTransitWarehouseStatusDateTime(packlinePair.Value));
				SetPacklineIDFromExternalReference(packlineBO, packlinePair.Key.ReferenceNumber, packlinePair.Key.PackingLineID);
				packlineBO.JL_OriginTransitWarehouseStatus = originTransitWarehouseStatus;

				if (IsTransitReceiveOnPickupCFS())
				{
					PopulatePANPortReferenceForPackLine(packlineBO);
					PopulateERCAdditionReferenceForPackLine(packlineBO);
				}

				packlineBO.JL_RefNumber = string.Empty;
				var containerNum = GetContainerNumber(packlinePair.Key);
				SetPacklineContainer(packlineBO, containerNum);
			}

			DeletePackLines(packLinesToDelete.Where(l => l.PkgPackageCollection.Count == 0));

			if (updateShipmentTotals && ShouldUpdateShipment(shipmentBO))
			{
				shipmentBO.CheckTotalsDiffer(false);
				shipmentBO.SyncMeasuresWithInnerPackLines(false);
			}
		}

		static bool ShouldUpdateShipment(ForwardingShipment shipmentBusinessObject)
		{
			if (shipmentBusinessObject.OuterPackLines.Cast<ForwardingPackLine>().Any(packLine =>
					packLine.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies
					|| packLine.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped
					|| packLine.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus
				))
			{
				return false;
			}

			return true;
		}

		(bool HasOverpack, ZString OriginTransitWarehouseStatus, IEnumerable<ForwardingPackLine> OriginalPackLines) HandleOverpackIfNeeded(ForwardingPackLine packlineBO, IDictionary<ZGuid, IList<ZString>> packagePreviousPackLinesDictionary)
		{
			if (packlineBO.InnerPackLines.Count == 0)
			{
				return (false, FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus, Enumerable.Empty<ForwardingPackLine>());
			}

			var originalPackLineIds = packlineBO.PkgPackageCollection
				.SelectMany(PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots)
				.Cast<ForwardingPackage>()
				.SelectMany(innerPackage =>
				{
					var previousPackLines = packagePreviousPackLinesDictionary.GetValueOrDefault(innerPackage.PK) ?? new List<ZString>();
					previousPackLines.Add(innerPackage.KP_ExternalReference);
					previousPackLines.Add(innerPackage.KP_PreviousPackLineID);
					return previousPackLines;
				})
				.Where(id => !id.IsEmpty)
				.ToHashSet();
			var originalPackLines = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().Where(l => originalPackLineIds.Contains(l.JL_PackLineId));
			return (true, FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, originalPackLines);
		}

		void DeletePackLines(IEnumerable<ForwardingPackLine> packLinesToDelete)
		{
			if (packLinesToDelete == null)
			{
				return;
			}

			foreach (var packLineToDelete in packLinesToDelete)
			{
				if (packLineToDelete.IsDeleted)
				{
					continue;
				}

				packLineToDelete.InnerPackLines.DeleteAll();
				shipmentBO.OuterPackLines.RemoveAndDelete(packLineToDelete);
			}
		}

		static IEnumerable<ForwardingPackage> GetAllOuterPackagesViaDivots(ForwardingPackageJob job)
		{
			if (job == null)
			{
				return Enumerable.Empty<ForwardingPackage>();
			}

			var allInnerPackages = job.Packages.Cast<ForwardingPackage>().SelectMany(PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots).Cast<ForwardingPackage>();
			return job.Packages.Cast<ForwardingPackage>().Except(allInnerPackages);
		}

		(IList<UniversalPackingLine> UniversalPackagesToBeImported, IList<ForwardingPackage> PackagesToBeImported, IDictionary<ZGuid, IList<ZString>> PackagePreviousPackLinesDictionary) GetPackagesToBeImported()
		{
			var universalPackagesToBeImported = GetValidImportedPackages().ToList();
			var (packageJob, packagePreviousPackLinesDictionary) = ReadPkgPackageJob(universalPackagesToBeImported);
			var packagesToBeImported = GetAllOuterPackagesViaDivots(packageJob).ToList();
			return (universalPackagesToBeImported, packagesToBeImported, packagePreviousPackLinesDictionary);
		}

		bool CheckIsDepartureTransitWarehouse()
		{
			return transitWarehouseAddress.PK == shipmentBO.ExportReceivingDepot?.PK || shipmentBO.Consols.Cast<CommonConsol>().Any(consol => transitWarehouseAddress.PK == consol.PackDepotAddress?.PK);
		}

		IEnumerable<ForwardingPackLine> GetExistedPacklines()
		{
			return shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().Where(p => !(p?.JL_PackLineId).GetValueOrDefault().IsEmpty);
		}

		Dictionary<ZString, ForwardingPackLine> GetUnprocessedPacklines()
		{
			var isDepartureTransitWarehouse = CheckIsDepartureTransitWarehouse();
			return GetExistedPacklines()
				.Where(p => !isDepartureTransitWarehouse || !p.JL_DepartureTransitWarehouseExcluded)
				.ToDictionary(k => k.JL_PackLineId);
		}

		void ProcessTransitReceivePackages(IEnumerable<UniversalPackingLine> universalPackagesToBeImported, IEnumerable<ForwardingPackage> packagesToBeImported, IDictionary<ZGuid, IList<ZString>> packagePreviousPackLinesDictionary)
		{
			var unmatchedPackages = new List<PackageWrapper>();
			var unprocessedPacklines = GetUnprocessedPacklines();
			var existedPacklines = GetExistedPacklines();
			var isDepartureTransitWarehouse = CheckIsDepartureTransitWarehouse();

			var packageWrappers = PackageWrapper.WrapPackagesReceive(packagesToBeImported, existedPacklines.ToList(), universalPackagesToBeImported, isDepartureTransitWarehouse).ToList();

			foreach (var packageWrappersGroup in packageWrappers.GroupBy(splitPackage => splitPackage.GroupKey).ToDictionary(p => p.Key, p => p.ToList()))
			{
				unmatchedPackages.AddRange(ImportPackagesIntoPackline(packageWrappersGroup.Value, unprocessedPacklines));
			}

			foreach (var packLineBO in unprocessedPacklines.Values)
			{
				ShortShipRemainingPacklines(packLineBO);
			}

			if (unmatchedPackages.Count > 0)
			{
				GroupAndImportUnmatchedPackages(unmatchedPackages, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, packagePreviousPackLinesDictionary);
			}
		}

		static ZDateTime GetLastKnownTransitWarehouseStatusDateTime(List<UniversalPackingLine> packingLineDOs)
		{
			return packingLineDOs == null || packingLineDOs.Count == 0
				? ZDateTime.Empty
				: packingLineDOs.Select(l => l.GetLastKnownTransitWarehouseStatusDateTime()).Max();
		}

		List<PackageWrapper> ImportPackagesIntoPackline(List<PackageWrapper> packageWrappers, Dictionary<ZString, ForwardingPackLine> unprocessedPacklines)
		{
			if (packageWrappers.Count == 0 || packageWrappers.All(pkg => pkg.ForwardingPackLineBO == null))
			{
				return packageWrappers;
			}

			unprocessedPacklines.Remove(packageWrappers[0].GroupKey);

			if (packageWrappers.All(pkg => !pkg.IsImported))
			{
				return new List<PackageWrapper>(0);
			}

			var originalPackLine = packageWrappers[0].ForwardingPackLineBO;
			var originalPackLineSplitKey = GetAutoSplitKey(originalPackLine, GetReceiveConsignmentNumberFromForwardingPackingLine(originalPackLine), screeningMethod: GetScreeningMethodKey(originalPackLine), isHighRisk: GetIsHighRisk(originalPackLine), additionalScreeningMethod: GetAdditionalScreeningMethodKey(originalPackLine));
			var receiveConsignmentNumberFromDataObject = originalPackLine.JL_OA_LastKnownTransitWarehouseAddress == transitWarehouseAddress.PK
				? GetReceiveConsignmentNumberFromDataObject()
				: originalPackLineSplitKey.ReceiveConsignmentNumber;

			var isTransitReceiveOnPickupCFS = IsTransitReceiveOnPickupCFS();
			var packageWrapperDic = packageWrappers.Where(p => p.IsImported).GroupBy(pkg => GetAutoSplitKey(pkg.PackingLineDO, receiveConsignmentNumberFromDataObject, screeningMethod: GetScreeningMethodKey(pkg.PackingLineDO), isHighRisk: GetIsHighRisk(pkg.PackingLineDO), additionalScreeningMethod: GetAdditionalScreeningMethodKey(pkg.PackingLineDO))).ToDictionary(p => p.Key, p => p.ToList());
			var isReceivingAllPackages = packageWrappers.All(pkg => pkg.IsImported);
			var remainingPackagesHasDifferentRCNs = isTransitReceiveOnPickupCFS
				&& Core.Constants.CountryCodes.IsFranceOrTerritory(transitWarehouseAddress.OA_RN_NKCountryCode)
				&& packageWrapperDic.Keys.Where(k => k != originalPackLineSplitKey).Any(k => k.ReceiveConsignmentNumber != originalPackLineSplitKey.ReceiveConsignmentNumber);

			var wrapperGroupKeyForOriginalPackLine = GetNotSplitPackageWrappersGroupKey(originalPackLine, originalPackLineSplitKey, packageWrapperDic, isReceivingAllPackages, remainingPackagesHasDifferentRCNs);

			if (wrapperGroupKeyForOriginalPackLine.HasValue)
			{
				var wrappersForOriginalPackLine = packageWrapperDic[wrapperGroupKeyForOriginalPackLine.Value];
				if (isTransitReceiveOnPickupCFS)
				{
					PopulateERCAdditionReferenceForPackLine(originalPackLine);
					PopulatePANPortReferenceForPackLine(originalPackLine);
				}

				var newPackages = wrappersForOriginalPackLine
					.Where(p => !originalPackLine.PkgPackageCollection.Contains(p.PackageBO)).Select(p => p.PackageBO).ToList();
				newPackages.ForEach(pkg => CheckIfPackageExistInAnyPacklines(pkg));
				originalPackLine.PkgPackageCollection.AddRange(newPackages);
				SetPacklineContainer(originalPackLine, wrapperGroupKeyForOriginalPackLine.Value.ContainerNum);

				SetLastKnownReceiptInformation(originalPackLine, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, GetLastKnownTransitWarehouseStatusDateTime(wrappersForOriginalPackLine.Select(x => x.PackingLineDO).ToList()));
				SetDiscrepenciesStatus(wrappersForOriginalPackLine.Select(x => x.PackingLineDO).ToList(), originalPackLine);
			}

			var autoSplitAllowed = !isTransitReceiveOnPickupCFS || remainingPackagesHasDifferentRCNs || !isReceivingAllPackages;
			var split = false;

			foreach (var wrapperGroupForSplitPackline in packageWrapperDic.Where(g => wrapperGroupKeyForOriginalPackLine == null || g.Key != wrapperGroupKeyForOriginalPackLine.Value))
			{
				var splitInNewPackLine = autoSplitAllowed ||
					(wrapperGroupKeyForOriginalPackLine != null && wrapperGroupForSplitPackline.Key.ContainerNum != wrapperGroupKeyForOriginalPackLine.Value.ContainerNum)
					|| (wrapperGroupKeyForOriginalPackLine != null && wrapperGroupForSplitPackline.Key.EqualsExcludingScreeningMethod(wrapperGroupKeyForOriginalPackLine.Value))
					|| (wrapperGroupKeyForOriginalPackLine != null && wrapperGroupForSplitPackline.Key.IsHighRisk != wrapperGroupKeyForOriginalPackLine.Value.IsHighRisk)
					|| (wrapperGroupKeyForOriginalPackLine != null && wrapperGroupForSplitPackline.Key.AdditionalScreeningMethod != wrapperGroupKeyForOriginalPackLine.Value.AdditionalScreeningMethod);

				wrapperGroupForSplitPackline.Value.ForEach(p => originalPackLine.PkgPackageCollection.RemoveFromRelationship(p.PackageBO));

				var splitPackline = splitInNewPackLine ? shipmentBO.OuterPackLines.AddNew() : originalPackLine;
				PackLine.PopulatePackLineIdIfNeeded(splitPackline.Factory, new[] { splitPackline });

				var newPackages = wrapperGroupForSplitPackline.Value.Select(p => p.PackageBO).ToList();
				newPackages.ForEach(pkg => CheckIfPackageExistInAnyPacklines(pkg));
				splitPackline.PkgPackageCollection.AddRange(newPackages);

				if (!splitInNewPackLine)
				{
					continue;
				}

				split = true;
				SetPacklineContainer(splitPackline, wrapperGroupForSplitPackline.Key.ContainerNum);
				SyncPackLineFromPackage(splitPackline, wrapperGroupForSplitPackline.Value.First().PackageBO);
				splitPackline.JL_RefNumber = originalPackLine.JL_RefNumber;

				if (isTransitReceiveOnPickupCFS)
				{
					if (remainingPackagesHasDifferentRCNs)
					{
						PopulateERCAdditionReferenceForPackLine(splitPackline);
					}
					else
					{
						CopyERCAdditionReferenceForPackLine(originalPackLine, splitPackline);
					}

					PopulatePANPortReferenceForPackLine(splitPackline);
				}

				SetLastKnownReceiptInformation(splitPackline, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, GetLastKnownTransitWarehouseStatusDateTime(wrapperGroupForSplitPackline.Value.Select(p => p.PackingLineDO).ToList()));
				SetDiscrepenciesStatus(wrapperGroupForSplitPackline.Value.Select(p => p.PackingLineDO).ToList(), splitPackline);

				if (!isReceivingAllPackages && !originalPackLineSplitKey.ContainerNum.IsEmpty && originalPackLineSplitKey.ContainerNum == wrapperGroupForSplitPackline.Key.ContainerNum)
				{
					originalPackLine.Containers.RemoveAll();
				}
				CheckPacklineActualWeightAndRelatedContainerGrossWeight(splitPackline);
			}

			if (split && packageWrapperDic.Any(g => wrapperGroupKeyForOriginalPackLine == null || g.Key != wrapperGroupKeyForOriginalPackLine.Value))
			{
				originalPackLine.SetUNDGsFromPackageCollection();
				originalPackLine.SetQuantityWeightAndVolumeFromPackageTotals();
				originalPackLine.SetInnerPackLinesFromPackageCollection();
				CheckPacklineActualWeightAndRelatedContainerGrossWeight(originalPackLine);
			}
			else
			{
				// we dont check for discrepancy on JL_Damaged, so it is always updated
				originalPackLine.JL_Damaged = originalPackLine.PkgPackageCollection_TotalDamagedPacks;
			}

			UpdatePacklineInspectionTypeCodeFromPkgPackage(originalPackLine);
			UpdatePacklineAdditionalInspectionTypeCodeFromPkgPackage(originalPackLine);

			var matchedPackLinePackagesDOs = GetImportedPackingLines(originalPackLine, packageWrappers).ToList();
			if (matchedPackLinePackagesDOs.Any())
			{
				SetLastKnownReceiptInformation(originalPackLine, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, GetLastKnownTransitWarehouseStatusDateTime(matchedPackLinePackagesDOs));
				SetDiscrepenciesStatus(matchedPackLinePackagesDOs, originalPackLine);
			}

			return new List<PackageWrapper>(0);
		}

		AutoSplitKey? GetNotSplitPackageWrappersGroupKey(ForwardingPackLine packLine, Dictionary<AutoSplitKey, List<PackageWrapper>> packageWrapperDic)
		{
			var packages = packageWrapperDic.Values.SelectMany(list => list).Select(wrapper => wrapper.PackageBO);
			return packLine.PkgPackageCollection.All(pkg => packages.Contains(pkg)) ? packageWrapperDic.First().Key : null;
		}

		AutoSplitKey? GetNotSplitPackageWrappersGroupKey(ForwardingPackLine packLine, AutoSplitKey matchedPackLineSplitKey, Dictionary<AutoSplitKey, List<PackageWrapper>> packageWrapperDic, bool isReceivingAllPackages, bool remainingPackagesHasDifferentRCNs)
		{
			if (!isReceivingAllPackages)
			{
				return GetNotSplitPackageWrappersGroupKey(packLine, packageWrapperDic);
			}

			if (remainingPackagesHasDifferentRCNs)
			{
				return packageWrapperDic.ContainsKey(matchedPackLineSplitKey) ? matchedPackLineSplitKey : null;
			}

			if (packageWrapperDic.Count == 1)
			{
				return packageWrapperDic.First().Key;
			}
			else
			{
				return packageWrapperDic.ContainsKey(matchedPackLineSplitKey) ? matchedPackLineSplitKey : packageWrapperDic.First().Key;
			}
		}

		#endregion

		void Process()
		{
			using (shipmentBO.OverrideChangingInspectionStatusReason(ReasonChangingSecurityInspectionStatus))
			using (shipmentBO.OverrideChangingAdditionalInspectionStatusReason(ReasonChangingSecurityAdditionalInspectionStatus))
			using (shipmentBO.SetIsMarkingPackLinesAsSecuredAllowed())
			{
				shipmentDataObjectReadingHelper.ImportAviationSecurityInspectionTypeIfNeeded(shipmentBO);
				shipmentDataObjectReadingHelper.ImportAviationSecurityAdditionalInspectionTypeIfNeeded(shipmentBO);

				var (universalPackagesToBeImported, packagesToBeImported, packagePreviousPackLinesDictionary) = GetPackagesToBeImported();
				var unloadedPackages = universalPackagesToBeImported.Where(p => p.LoadDate.GetValueOrDefault().IsEmpty).ToList();
				var loadedPackages = universalPackagesToBeImported.Where(p => !p.LoadDate.GetValueOrDefault().IsEmpty).ToList();

				if (unloadedPackages.Count > 0)
				{
					ProcessTransitReceivePackages(unloadedPackages, packagesToBeImported, packagePreviousPackLinesDictionary);
				}

				if (loadedPackages.Count > 0 || unloadedPackages.Count == 0)
				{
					ProcessTransitDispatchPackages(loadedPackages, packagesToBeImported, packagePreviousPackLinesDictionary);
				}
			}
		}

		static void RemovePackageWrappers(IList<PackageWrapper> packageWrappers, Func<PackageWrapper, bool> predicate, Dictionary<ZString, ForwardingPackLine> unprocessedPacklines)
		{
			if (packageWrappers == null || predicate == null)
			{
				return;
			}

			var packageWrappersToBeRemoved = packageWrappers.Where(predicate).ToList();
			foreach (var packageWrapperToBeRemoved in packageWrappersToBeRemoved)
			{
				var packLineId = PackageWrapper.GetPackLineId(packageWrapperToBeRemoved);
				unprocessedPacklines?.Remove(packLineId);
				packageWrappers.Remove(packageWrapperToBeRemoved);
			}
		}

		void SyncPackLineFromPackage(ForwardingPackLine packline, ForwardingPackage package)
		{
			packline.CopyValuesFromPackage(package);
			packline.SetQuantityWeightAndVolumeFromPackageTotals();
			packline.SetUNDGsFromPackageCollection();
			packline.SetInnerPackLinesFromPackageCollection();
			CheckPacklineActualWeightAndRelatedContainerGrossWeight(packline);
		}

		void CheckPacklineActualWeightAndRelatedContainerGrossWeight(ForwardingPackLine packline)
		{
			if (packline == null)
			{
				return;
			}

			var (packlineActualWeight, packlineMessage) = NumericalCheckHelper.GetValidDecimalValue(JobPackLinesSchema.JL_ActualWeight, packline.JL_ActualWeight);
			if (!packlineMessage.IsEmpty)
			{
				packline.JL_ActualWeight = packlineActualWeight;
				logger.Log(Enterprise.Integration.LogType.Warning, packlineMessage);
			}

			packline.Containers.Cast<CommonContainer>().ForEach(c =>
			{
				var (containerGrossWeight, containerMessage) = NumericalCheckHelper.GetValidDecimalValue(JobContainerSchema.JC_GrossWeight, c.JC_GrossWeight);
				if (!containerMessage.IsEmpty)
				{
					c.JC_GrossWeight = containerGrossWeight;
					logger.Log(Enterprise.Integration.LogType.Warning, containerMessage);
				}
			});
		}

		IEnumerable<UniversalPackingLine> GetImportedPackingLines(ForwardingPackLine forwardingPackLine, IList<PackageWrapper> splitPackages)
		{
			if (forwardingPackLine == null || forwardingPackLine.PkgPackageCollection.Count == 0)
			{
				yield break;
			}

			foreach (var pkgPackage in forwardingPackLine.PkgPackageCollection)
			{
				var found = splitPackages.FirstOrDefault(p => p.IsImported && p.PackageBO.PackageIDWithFallbackToExternalReference == pkgPackage.PackageIDWithFallbackToExternalReference);
				if (found != null)
				{
					yield return found.PackingLineDO;
				}
			}
		}

		#region SplitPackage

		class PackageWrapper
		{
			internal static IEnumerable<PackageWrapper> WrapPackagesForDispatch(IEnumerable<ForwardingPackage> pkgPackages, IList<ForwardingPackLine> forwardingPackLines, IEnumerable<UniversalPackingLine> packagesToBeImported, bool isDepartureTransitWarehouse)
			{
				var dic = packagesToBeImported.ToDictionary(x => x.GetPackageIDWithFallbackToPacklineID());

				foreach (var pkg in pkgPackages)
				{
					yield return WrapPackagesForDispatch(pkg, forwardingPackLines, dic, isDepartureTransitWarehouse);
				}
			}

			static PackageWrapper WrapPackagesForDispatch(ForwardingPackage pkg, IList<ForwardingPackLine> forwardingPackLines, Dictionary<ZString, UniversalPackingLine> packagesToBeImported, bool isDepartureTransitWarehouse)
			{
				var result = new PackageWrapper
				{
					PackageBO = pkg,
					PackingLineDO = (packagesToBeImported.ContainsKey(pkg.PackageIDWithFallbackToExternalReference) ? packagesToBeImported[pkg.PackageIDWithFallbackToExternalReference] : null)
				};

				result.ForwardingPackLineBO = TryToMatchForwardingPackLineBO(pkg, forwardingPackLines, isDepartureTransitWarehouse);

				result.GroupKey = result.ForwardingPackLineBO?.JL_PackLineId ?? ZString.Empty;

				return result;
			}

			internal static IEnumerable<PackageWrapper> WrapPackagesReceive(IEnumerable<ForwardingPackage> pkgPackages, IList<ForwardingPackLine> forwardingPackLines, IEnumerable<UniversalPackingLine> packagesToBeImported, bool isDepartureTransitWarehouse)
			{
				var packagesDictionaryToBeImported = packagesToBeImported.ToDictionary(p => p.GetPackageIDWithFallbackToPacklineID());

				foreach (var pkg in pkgPackages)
				{
					yield return WrapPackagesReceive(pkg, forwardingPackLines, packagesDictionaryToBeImported, isDepartureTransitWarehouse);
				}
			}

			static PackageWrapper WrapPackagesReceive(ForwardingPackage pkg, IList<ForwardingPackLine> forwardingPackLines, Dictionary<ZString, UniversalPackingLine> packagesToBeImported, bool isDepartureTransitWarehouse)
			{
				var result = new PackageWrapper
				{
					PackageBO = pkg,
					PackingLineDO = packagesToBeImported.ContainsKey(pkg.PackageIDWithFallbackToExternalReference) ? packagesToBeImported[pkg.PackageIDWithFallbackToExternalReference] : null
				};

				result.ForwardingPackLineBO = TryToMatchForwardingPackLineBO(pkg, forwardingPackLines, isDepartureTransitWarehouse);

				result.GroupKey = GetPackLineId(result);

				return result;
			}

			static ForwardingPackLine TryToMatchForwardingPackLineBO(ForwardingPackage pkg, IList<ForwardingPackLine> forwardingPackLines, bool isDepartureTransitWarehouse)
			{
				ForwardingPackLine res = null;

				if (!pkg.KP_PackageID.IsEmpty)
				{
					res = forwardingPackLines.FirstOrDefault(packLine => packLine.PkgPackageCollection.Any(package => pkg.KP_PackageID == package.KP_PackageID));
				}

				if (res == null)
				{
					res = forwardingPackLines.FirstOrDefault(packLine => packLine.JL_PackLineId == pkg.KP_ExternalReference)
							?? forwardingPackLines.FirstOrDefault(packLine => packLine.JL_PackLineId == pkg.KP_PreviousPackLineID);
				}

				if (res == null && pkg.KP_PackageID.IsEmpty)
				{
					res = forwardingPackLines.FirstOrDefault(packline => packline.PkgPackageCollection
							.Any(package => package.KP_PackageID.IsEmpty
							&& package.KP_ExternalReference == pkg.KP_ExternalReference
							&& package.KP_PreviousPackLineID == pkg.KP_PreviousPackLineID));
				}

				if (res != null && isDepartureTransitWarehouse && res.JL_DepartureTransitWarehouseExcluded)
				{
					var message = Res.GetString("51e88113-bb40-4fa3-ba50-0e56c7ef9942", "Package is already attached to an excluded pack line. See Package ID: {0}, Pack Line ID: {1}, Shipment ID: {2}",
						pkg.KP_PackageID, res.JL_PackLineId, res.Shipment.JS_UniqueConsignRef);
					throw new DataObjectReadFailureException(message);
				}
				return res;
			}

			internal static ZString GetPackLineId(PackageWrapper packageWrapper)
			{
				return packageWrapper == null
					? ZString.Empty
					: (packageWrapper.ForwardingPackLineBO?.JL_PackLineId.IsEmpty ?? true) ? packageWrapper.PackageBO.KP_ExternalReference : packageWrapper.ForwardingPackLineBO.JL_PackLineId;
			}

			internal ForwardingPackage PackageBO { get; private set; }

			internal ForwardingPackLine ForwardingPackLineBO { get; private set; }

			internal UniversalPackingLine PackingLineDO { get; private set; }

			internal bool IsImported => PackingLineDO != null;

			internal ZString GroupKey { get; private set; }
		}

		#endregion

		ZString GetContainerNumber(UniversalPackingLine groupedPackage)
		{
			if (!groupedPackage.ContainerLink.HasValue)
			{
				return ZString.Empty;
			}

			return shipmentDataObject.ContainerCollection.FirstOrDefault(c => c.Link == groupedPackage.ContainerLink.Value)?.ContainerNumber ?? ZString.Empty;
		}

		CommonContainer GetContainer(ForwardingPackLine packline, ZString containerNum)
			=> packline.Containers.Cast<ForwardingContainer>().FirstOrDefault(c =>
				c.JC_ContainerNum == containerNum &&
				(shipmentDataObjectReadingHelper.LinkManager.Consol is null || c.JC_JK == shipmentDataObjectReadingHelper.LinkManager.Consol.PK))
			?? shipmentDataObjectReadingHelper.LinkManager.Consol?.Containers.FindAnyByContainerNumber(containerNum);

		void SetPacklineContainer(ForwardingPackLine packline, ZString containerNum)
		{
			var container = GetContainer(packline, containerNum);
			if (container == null)
			{
				packline.Containers.RemoveAll();
			}
			else
			{
				packline.SetContainer(shipmentDataObjectReadingHelper.LinkManager.Consol, container);

				var (containerGrossWeight, containerMessage) = NumericalCheckHelper.GetValidDecimalValue(JobContainerSchema.JC_GrossWeight, container.JC_GrossWeight);
				if (!containerMessage.IsEmpty)
				{
					container.JC_GrossWeight = containerGrossWeight;
					logger.Log(Enterprise.Integration.LogType.Warning, containerMessage);
				}
			}
		}

		static void ShortShipRemainingPacklines(ForwardingPackLine unhandledPackline)
		{
			if (unhandledPackline.IsDeleted)
			{
				return;
			}

			unhandledPackline.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped;
			unhandledPackline.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
		}

		void SetLastKnownReceiptInformation(PackLine packline, ZString lastKnownTransitWarehouseStatus, ZDateTime lastKnownTransitWarehouseStatusDateTime)
		{
			packline.JL_LastKnownTransitWarehouseStatus = lastKnownTransitWarehouseStatus;
			packline.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			if (!lastKnownTransitWarehouseStatusDateTime.IsEmpty)
			{
				packline.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTransitWarehouseStatusDateTime;
			}
		}

		void SetPacklineIDFromExternalReference(PackLine packline, ZString? packageId, ZString? externalReference)
		{
			if (packageId.GetValueOrDefault().IsEmpty && !externalReference.GetValueOrDefault().IsEmpty)
			{
				packline.JL_PackLineId = externalReference.GetValueOrDefault();
			}
		}

		void CopyERCAdditionReferenceForPackLine(ForwardingPackLine packLineFrom, ForwardingPackLine packLineTo)
		{
			var existingERC = packLineFrom.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC);
			if (existingERC != null)
			{
				CreateERCAdditionalReferenceNumber(packLineTo, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, existingERC.CE_EntryNum, transitWarehouseAddress.OA_RN_NKCountryCode);
			}
		}

		#region PopulateERCAdditionReferenceForPackLine

		void PopulateERCAdditionReferenceForPackLine(ForwardingPackLine packLine)
		{
			var rcn = this.shipmentDataObject.GetMatchingDataSource(DataContextType.TransitReceive)?.Key;

			if (packLine.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC) == null)
			{
				CreateERCAdditionalReferenceNumber(packLine, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, rcn, transitWarehouseAddress.OA_RN_NKCountryCode);
			}
		}

		void CreateERCAdditionalReferenceNumber(ForwardingPackLine packLine, ZString type, ZString? number, string countryCode)
		{
			if (number.HasValue)
			{
				var referenceNumber = packLine.AdditionalReferenceNumbers.GetFirstReferenceNumberByTypeAndCountry(type, countryCode);

				if (referenceNumber == null)
				{
					referenceNumber = packLine.AdditionalReferenceNumbers.AddNew();

					referenceNumber.CE_EntryType = type;
					referenceNumber.CE_EntryNum = number.Value;
					referenceNumber.CE_EntryIsSystemGenerated = true;
					referenceNumber.CE_RN_NKCountryCode = countryCode;
				}
			}
		}

		bool IsTransitReceiveOnPickupCFS()
		{
			return shipmentDataObject.GetMatchingDataSource(DataContextType.TransitReceive) != null && shipmentBO.JS_OA_ExportReceivingDepot == transitWarehouseAddress.PK;
		}

		#region PopulatePANPortReferenceForPackLine

		void PopulatePANPortReferenceForPackLine(ForwardingPackLine packLine)
		{
			var panPortReference = shipmentDataObject.PortReferenceCollection?.FirstOrDefault(p => (p.Type?.Code.GetValueOrDefault() ?? string.Empty) == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN
									&& (p.Country?.Code ?? ZString.Empty) == transitWarehouseAddress.OA_RN_NKCountryCode);

			CreateOrUpdatePANPortReference(packLine, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN, panPortReference?.Reference, transitWarehouseAddress.OA_RN_NKCountryCode, panPortReference?.Status?.Code);

			if (panPortReference != null && panPortReference.Reference.HasValue && Core.Constants.CountryCodes.IsFranceOrTerritory(transitWarehouseAddress.OA_RN_NKCountryCode))
			{
				packLine.JL_ExportRefNumber = panPortReference.Reference.Value;
			}
		}

		void CreateOrUpdatePANPortReference(ForwardingPackLine packLine, ZString type, ZString? number, string countryCode, string status)
		{
			if (number.HasValue)
			{
				var panNumber = packLine.PortReferences.Cast<CusEntryNumber>().FirstOrDefault(c => c.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN && c.CE_RN_NKCountryCode == countryCode);

				if (panNumber == null)
				{
					panNumber = (CusEntryNumber)packLine.PortReferences.AddNew();
					panNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
					panNumber.CE_RN_NKCountryCode = countryCode;
				}

				panNumber.CE_EntryType = type;
				panNumber.CE_EntryNum = number.Value;
				panNumber.CE_EntryStatus = status;
				panNumber.CE_EntryIsSystemGenerated = true;
			}
		}

			#endregion

		ZString GetReceiveConsignmentNumberFromDataObject()
		{
			return IsTransitReceiveOnPickupCFS()
				? this.shipmentDataObject.GetMatchingDataSource(DataContextType.TransitReceive)?.Key ?? ZString.Empty
				: ZString.Empty;
		}

		ZString GetReceiveConsignmentNumberFromForwardingPackingLine(ForwardingPackLine packLine)
		{
			return IsTransitReceiveOnPickupCFS()
				? packLine.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(c => c.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC)?.CE_EntryNum ?? ZString.Empty
				: ZString.Empty;
		}

		#region AutoSplitKey

		internal struct AutoSplitKey
		{
			public ZString ContainerNum { get; }
			public ZString ReceiveConsignmentNumber { get; }
			public string PackType { get; }
			public ZString ScreeningMethod { get; }

			public ZBool IsHighRisk { get; }
			public ZString AdditionalScreeningMethod { get; }

			public AutoSplitKey(ZString containerNum, string packType, string rcn = "", ZString screeningMethod = default, ZBool isHighRisk = default, ZString additionalScreeningMethod = default)
			{
				ContainerNum = containerNum;
				PackType = packType;
				ReceiveConsignmentNumber = rcn;
				ScreeningMethod = screeningMethod;
				IsHighRisk = isHighRisk;
				AdditionalScreeningMethod = additionalScreeningMethod;
			}

			public override bool Equals(object obj)
			{
				return obj is AutoSplitKey identifier
					&& EqualityComparer<ZString>.Default.Equals(ContainerNum, identifier.ContainerNum)
					&& PackType == identifier.PackType
					&& EqualityComparer<ZString>.Default.Equals(ReceiveConsignmentNumber, identifier.ReceiveConsignmentNumber)
					&& EqualityComparer<ZString>.Default.Equals(ScreeningMethod, identifier.ScreeningMethod)
					&& EqualityComparer<ZBool>.Default.Equals(IsHighRisk, identifier.IsHighRisk)
					&& EqualityComparer<ZString>.Default.Equals(AdditionalScreeningMethod, identifier.AdditionalScreeningMethod);
			}

			public bool EqualsExcludingScreeningMethod(AutoSplitKey another)
			{
				return EqualityComparer<ZString>.Default.Equals(ContainerNum, another.ContainerNum)
					&& EqualityComparer<ZString>.Default.Equals(ReceiveConsignmentNumber, another.ReceiveConsignmentNumber)
					&& PackType == another.PackType;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = -2108129456;
					hashCode = hashCode * -1521134295 + ContainerNum.GetHashCode();
					hashCode = hashCode * -1521134295 + ReceiveConsignmentNumber.GetHashCode();
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PackType);
					hashCode = hashCode * -1521134295 + ScreeningMethod.GetHashCode();
					hashCode = hashCode * -1521134295 + IsHighRisk.GetHashCode();
					hashCode = hashCode * -1521134295 + AdditionalScreeningMethod.GetHashCode();
					return hashCode;
				}
			}

			public static bool operator ==(AutoSplitKey lhs, AutoSplitKey rhs) => lhs.Equals(rhs);
			public static bool operator !=(AutoSplitKey lhs, AutoSplitKey rhs) => !(lhs == rhs);
		}

		AutoSplitKey GetAutoSplitKey(UniversalPackingLine packingLine, string rcn = "", ZString screeningMethod = default, ZBool isHighRisk = default, ZString additionalScreeningMethod = default)
		{
			return new AutoSplitKey(GetContainerNumber(packingLine), packingLine.PackType.Code, rcn, screeningMethod, isHighRisk, additionalScreeningMethod);
		}

		AutoSplitKey GetAutoSplitKey(ForwardingPackLine packLine, string rcn = "", ZString screeningMethod = default, ZBool isHighRisk = default, ZString additionalScreeningMethod = default)
		{
			return new AutoSplitKey(packLine.GetContainer(shipmentDataObjectReadingHelper.LinkManager.Consol)?.JC_ContainerNum ?? string.Empty, packLine.JL_F3_NKPackType, rcn, screeningMethod, isHighRisk, additionalScreeningMethod);
		}

		#endregion

		void SetDiscrepenciesStatus(IList<UniversalPackingLine> packagesDOs, ForwardingPackLine matchedPackLine)
		{
			var groupedByAttributes = GetGroupedPacklines(packagesDOs, IsScreeningMethodApplicable(), isOuterPackingLine: true);
			if (groupedByAttributes.Count() > 1 || !IsPackagesSharesTheSameAttributes(groupedByAttributes.First().Item1, matchedPackLine))
			{
				matchedPackLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies;
			}
			else
			{
				matchedPackLine.SetUNDGsFromPackageCollection();
				matchedPackLine.SetInnerPackLinesFromPackageCollection();
				matchedPackLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
			}
		}

		#region IsPackagesSharesTheSameAttributes

		static bool IsPackagesSharesTheSameAttributes(UniversalPackingLine groupedPackage, ForwardingPackLine packline)
		{
			return (groupedPackage.Commodity?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_RH_NKCommodityCode
				&& groupedPackage.HarmonisedCode.GetValueOrDefault() == packline.JL_HarmonisedCode
				&& (groupedPackage.LengthUnit?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_Calc_HeightUnit
				&& (groupedPackage.PackType?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_F3_NKPackType
				&& groupedPackage.Height.GetValueOrDefault() == packline.JL_Height
				&& groupedPackage.Length.GetValueOrDefault() == packline.JL_Length
				&& groupedPackage.MarksAndNos.GetValueOrDefault() == packline.JL_MarksAndNumbers
				&& groupedPackage.PackQty.GetValueOrDefault() == packline.JL_PackageCount
				&& groupedPackage.Volume.GetValueOrDefault() == packline.JL_ActualVolume
				&& (groupedPackage.VolumeUnit?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_ActualVolumeUQ
				&& groupedPackage.Weight.GetValueOrDefault() == packline.JL_ActualWeight
				&& (groupedPackage.WeightUnit?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_ActualWeightUQ
				&& groupedPackage.Width.GetValueOrDefault() == packline.JL_Width
				&& groupedPackage.GetCleanSingleLineGoodsDescription().GetValueOrDefault() == GetPackageInfoHelper.GetCleanSingleLineText(packline.JL_Description)
				&& groupedPackage.RequiresTemperatureControl.GetValueOrDefault() == packline.JL_RequiresTemperatureControl
				&& (!packline.JL_RequiresTemperatureControl ||
					(groupedPackage.RequiredTemperatureMinimum.GetValueOrDefault() == packline.JL_RequiredTemperatureMinimum
					&& groupedPackage.RequiredTemperatureMaximum.GetValueOrDefault() == packline.JL_RequiredTemperatureMaximum
					&& (groupedPackage.RequiredTemperatureUnit?.Code.GetValueOrDefault() ?? ZString.Empty) == packline.JL_RequiredTemperatureUnit));
		}

		#endregion

		#region GetGroupedPacklines

		IEnumerable<(UniversalPackingLine, List<UniversalPackingLine>)> GetGroupedPacklines(IList<UniversalPackingLine> packlinesToGroup, bool isScreeningMethodApplicable, bool isOuterPackingLine)
		{
			if (packlinesToGroup == null || !packlinesToGroup.Any())
			{
				return Enumerable.Empty<(UniversalPackingLine, List<UniversalPackingLine>)>();
			}

			Dictionary<ZString, ZString> packLineIDDictionary = null;
			var getPackLineIDDictionary = () => packLineIDDictionary ??= PackLine.CreatePackLineIDDictionary(packlinesToGroup);
			return packlinesToGroup.GroupBy(p =>
			{
				var isLinkedToContainers = p.ContainerLink.GetValueOrDefault() > 0;
				var data = new
				{
					Commodity = p.Commodity?.Code.GetValueOrDefault() ?? ZString.Empty,
					ContainerLink = isLinkedToContainers ? (int)p.ContainerLink.GetValueOrDefault() : (int?)null,
					ContainerPackingOrder = isLinkedToContainers ? p.ContainerPackingOrder.GetValueOrDefault() : (int?)null,
					HarmonisedCode = p.HarmonisedCode.GetValueOrDefault(),
					ItemNo = p.ItemNo.GetValueOrDefault(),
					MarksAndNos = p.MarksAndNos.GetValueOrDefault(),
					CountryOfOrigin = p.CountryOfOrigin?.Code.GetValueOrDefault() ?? ZString.Empty,
					PackType = p.PackType?.Code.GetValueOrDefault() ?? ZString.Empty,
					ExportReferenceNumber = p.ExportReferenceNumber.GetValueOrDefault(),
					ImportReferenceNumber = p.ImportReferenceNumber.GetValueOrDefault(),
					GoodsDescription = p.GetCleanSingleLineGoodsDescription().GetValueOrDefault(),
					EndItemNo = p.EndItemNo.GetValueOrDefault(),
					LinePrice = p.LinePrice.GetValueOrDefault(),
					DetailedDescription = p.DetailedDescription.GetValueOrDefault(),
					Length = p.Length.GetValueOrDefault(),
					Width = p.Width.GetValueOrDefault(),
					Height = p.Height.GetValueOrDefault(),
					LengthUnit = p.LengthUnit?.Code.GetValueOrDefault() ?? ZString.Empty,
					VolumeUnit = p.VolumeUnit?.Code.GetValueOrDefault() ?? ZString.Empty,
					WeightUnit = p.WeightUnit?.Code.GetValueOrDefault() ?? ZString.Empty,
					RequiresTemperatureControl = p.RequiresTemperatureControl.GetValueOrDefault(),
					RequiredTemperatureMinimum = p.RequiredTemperatureMinimum.GetValueOrDefault(),
					RequiredTemperatureMaximum = p.RequiredTemperatureMaximum.GetValueOrDefault(),
					RequiredTemperatureUnit = p.RequiredTemperatureUnit?.Code.GetValueOrDefault() ?? ZString.Empty,
					ClassificationIdentifier = string.Join("|", p.ClassificationCollection?
						.Select(c => string.Join("-", c.Code.GetValueOrDefault(),
							c.Country?.Code.GetValueOrDefault() ?? string.Empty,
							c.Type?.Code.GetValueOrDefault() ?? string.Empty))
						.OrderBy(c => c) ?? Enumerable.Empty<string>()),
					CustomizedFieldIdentifier =
						string.Join("|",
							p.CustomizedFieldCollection?.Select(c => string.Join("-", c.Key.GetValueOrDefault(),
								c.Value.GetValueOrDefault(), c.DataType.GetValueOrDefault())).OrderBy(c => c) ??
							Enumerable.Empty<string>()),
					ScreeningMethod = isScreeningMethodApplicable ? (string)p.ScreeningMethod.GetValueOrDefault() : string.Empty,
					HasInnerPackingLines = isOuterPackingLine && p.HasInnerPackingLines(),
					PackingLineID = PackLine.GetOriginalPackLineID(isOuterPackingLine, p, getPackLineIDDictionary),
					IsHighRisk = GetIsHighRisk(p),
					AdditionalScreeningMethod = GetAdditionalScreeningMethodKey(p)
				};

				return data;
			}).Select(g => CreateGroupedPackline(g, isScreeningMethodApplicable, isOuterPackingLine, getPackLineIDDictionary));
		}

		(UniversalPackingLine, List<UniversalPackingLine>) CreateGroupedPackline(IGrouping<object, UniversalPackingLine> group, bool isScreeningMethodApplicable, bool isOuterPackingLine, Func<Dictionary<ZString, ZString>> getPackLineIDDictionary)
		{
			UniversalPackingLine result;

			if (group.Count() == 1)
			{
				result = (UniversalPackingLine)group.First().Clone();
				result.OutturnComment = null;
				result.OutturnedHeight = null;
				result.OutturnedLength = null;
				result.OutturnedVolume = null;
				result.OutturnedWeight = null;
				result.OutturnedWidth = null;
				result.OutturnPillagedQty = null;
				result.OutturnQty = null;
				result.SetWriterStrategy(UniversalDataBuss.DataObjects.DefaultDataObjectWriterStrategy.Instance);
				SetCollections(result, group, isScreeningMethodApplicable, isOuterPackingLine);
				return (result, group.ToList());
			}

			result = new UniversalPackingLine(UniversalDataBuss.DataObjects.DefaultDataObjectWriterStrategy.Instance)
			{
				Commodity = group.First().Commodity,
				ContainerPackingOrder = group.First().ContainerPackingOrder,
				ContainerLink = group.First().ContainerLink,
				ContainerNumber = group.First().ContainerNumber,
				HarmonisedCode = group.First().HarmonisedCode,
				ItemNo = group.First().ItemNo,
				MarksAndNos = group.First().MarksAndNos,
				CountryOfOrigin = group.First().CountryOfOrigin,
				PackType = group.First().PackType,
				ScreeningMethod = group.First().ScreeningMethod,
				ExportReferenceNumber = group.First().ExportReferenceNumber,
				ImportReferenceNumber = group.First().ImportReferenceNumber,
				GoodsDescription = group.First().GoodsDescription,
				EndItemNo = group.First().EndItemNo,
				LinePrice = group.First().LinePrice,
				DetailedDescription = group.First().DetailedDescription,
				LengthUnit = group.First().LengthUnit,
				Length = group.First().Length,
				Width = group.First().Width,
				Height = group.First().Height,
				VolumeUnit = group.First().VolumeUnit,
				WeightUnit = group.First().WeightUnit,
				RequiresTemperatureControl = group.First().RequiresTemperatureControl,
				RequiredTemperatureMinimum = group.First().RequiredTemperatureMinimum,
				RequiredTemperatureMaximum = group.First().RequiredTemperatureMaximum,
				RequiredTemperatureUnit = group.First().RequiredTemperatureUnit,
				LoadingMeters = group.Sum(e => e.LoadingMeters.GetValueOrDefault()),
				PackQty = group.Sum(e => e.PackQty.GetValueOrDefault()),
				Volume = group.Sum(e => e.Volume.GetValueOrDefault()),
				Weight = group.Sum(e => e.Weight.GetValueOrDefault()),
				OutturnDamagedQty = group.Sum(e => e.OutturnDamagedQty.GetValueOrDefault()),
				PackingLineID = PackLine.GetOriginalPackLineID(isOuterPackingLine, group.First(), getPackLineIDDictionary)
			};

			result.SetClassificationCollection(() => group.First()?.ClassificationCollection);
			result.SetCustomizedFieldCollection(() => group.First()?.CustomizedFieldCollection);
			SetCollections(result, group, isScreeningMethodApplicable, isOuterPackingLine);
			return (result, group.ToList());
		}

		void SetCollections(UniversalPackingLine packingLine, IGrouping<object, UniversalPackingLine> group, bool isScreeningMethodApplicable, bool isOuterPackingLine)
		{
			if (isOuterPackingLine)
			{
				packingLine.SetUNDGCollection(() => CreateGroupedUndg(group));
				packingLine.SetPackingLineCollection(() => CreateGroupedInnerPackingLines(group, isScreeningMethodApplicable));
			}
			else
			{
				packingLine.SetUNDGCollection(() => new List<UniversalUNDG>());
				packingLine.SetPackingLineCollection(() => new List<UniversalPackingLine>());
			}
		}

		List<UniversalPackingLine> CreateGroupedInnerPackingLines(IGrouping<object, UniversalPackingLine> group, bool isScreeningMethodApplicable)
		{
			if (group.First().HasInnerPackingLines())
			{
				var innerPackingLines = group.SelectMany(g => GetValidImportedInnerPackages(g)).ToList();
				return GetGroupedPacklines(innerPackingLines, isScreeningMethodApplicable, isOuterPackingLine: false).Select(g => g.Item1).ToList();
			}

			return new List<UniversalPackingLine>();
		}

		static List<UniversalUNDG> CreateGroupedUndg(IGrouping<object, UniversalPackingLine> group)
		{
			var packingLines = group.Select(g => g);
			if (group.First().HasInnerPackingLines())
			{
				packingLines = packingLines.Concat(group.SelectMany(g => GetValidImportedInnerPackages(g)));
			}
			var dgGroup = packingLines
				.SelectMany(g => g.UNDGCollection ?? Enumerable.Empty<UniversalUNDG>())
				.GroupBy(g => GetDGIdentifier(g));
			return dgGroup.Where(g => !g.Key.IsNullOrEmpty()).Select(g => new UniversalUNDG(UniversalDataBuss.DataObjects.DefaultDataObjectWriterStrategy.Instance)
			{
				UNDGCode = g.First().UNDGCode,
				IMOClass = g.First().IMOClass,
				FlashPoint = g.First().FlashPoint,
				ProperShippingName = g.First().ProperShippingName,
				TechicalName = g.First().TechicalName,
				MarinePollutant = g.First().MarinePollutant,
				PackingGroup = g.First().PackingGroup,
				PackedInLimitedQuantity = g.First().PackedInLimitedQuantity,
				Contact = g.First().Contact,
				Weight = g.Sum(e => e.Weight.GetValueOrDefault()),
				WeightUQ = g.First().WeightUQ,
				Volume = g.Sum(e => e.Volume.GetValueOrDefault()),
				VolumeUQ = g.First().VolumeUQ,
				SubLabel1 = g.First().SubLabel1,
				SubLabel2 = g.First().SubLabel2,
				PackQty = g.Sum(e => e.PackQty.GetValueOrDefault()),
				PackType = g.First().PackType,
				Standard = g.First().Standard,
				RadionuclideElementSuffix = g.First().RadionuclideElementSuffix,
				RadionuclideElement = g.First().RadionuclideElement,
				RadioactiveMaximumActivity = g.First().RadioactiveMaximumActivity,
				RadioactiveMaximumActivityUnit = g.First().RadioactiveMaximumActivityUnit,
				RadioactiveLabelCategory = g.First().RadioactiveLabelCategory,
				RadioactiveTransportIndex = g.First().RadioactiveTransportIndex,
				Description = g.First().Description,
				FissileExcepted = g.First().FissileExcepted,
				ExclusiveUse = g.First().ExclusiveUse,
				HighwayRouteControlledQuantity = g.First().HighwayRouteControlledQuantity,
				PackingInstructionSection = g.First().PackingInstructionSection,
			}).ToList();
		}

		static string GetDGIdentifier(UniversalUNDG dg, bool includeAmount = false)
		{
			if (dg == null)
			{
				return string.Empty;
			}

			var result = string.Join("-",
					dg.UNDGCode.GetValueOrDefault(),
					dg.FlashPoint.GetValueOrDefault(),
					dg.IMOClass.GetValueOrDefault(),
					dg.MarinePollutant?.Code.GetValueOrDefault() ?? string.Empty,
					dg.PackedInLimitedQuantity.GetValueOrDefault(),
					dg.TechicalName.GetValueOrDefault(),
					dg.VolumeUQ?.Code.GetValueOrDefault() ?? string.Empty,
					dg.WeightUQ?.Code.GetValueOrDefault() ?? string.Empty,
					dg.PackType?.Code.GetValueOrDefault() ?? string.Empty,
					dg.Contact?.FullName.GetValueOrDefault() ?? string.Empty,
					dg.Contact?.Phone.GetValueOrDefault() ?? string.Empty,
					dg.Contact?.Email.GetValueOrDefault() ?? string.Empty,
					dg.RadionuclideElementSuffix.GetValueOrDefault(),
					dg.RadionuclideElement.GetValueOrDefault(),
					dg.RadioactiveMaximumActivity.GetValueOrDefault(),
					dg.RadioactiveMaximumActivityUnit.GetValueOrDefault(),
					dg.RadioactiveLabelCategory.GetValueOrDefault(),
					dg.RadioactiveTransportIndex.GetValueOrDefault(),
					dg.Description.GetValueOrDefault(),
					dg.FissileExcepted.GetValueOrDefault(),
					dg.ExclusiveUse.GetValueOrDefault(),
					dg.HighwayRouteControlledQuantity.GetValueOrDefault(),
					dg.PackingInstructionSection.GetValueOrDefault());

			return includeAmount ? string.Join("-", result, dg.Weight.GetValueOrDefault(), dg.Volume.GetValueOrDefault(), dg.PackQty.GetValueOrDefault()) : result;
		}

		#endregion

		#endregion

		#region TW Events

		protected void AddUPCEvents()
		{
			AddPKCOrUPCEvent(Events.UnpackingCompleted);
		}

		protected void AddPKCEvents()
		{
			AddPKCOrUPCEvent(Events.PackingCompleted);
		}

		void AddPKCOrUPCEvent(ZArchitecture.Business.Event upcOrPkcEvent)
		{
			if (shipmentDataObject.ContainerCollection == null || shipmentDataObject.PackingLineCollection == null)
			{
				return;
			}

			var containerGroups = shipmentDataObject.ContainerCollection
				.Union(shipmentDataObject.RelatedShipmentCollection?.OfType<UniversalShipment>().Select(shipment => shipment?.ContainerCollection?.FirstOrDefault()).WhereNotNull() ?? Enumerable.Empty<Container>())
				.GroupBy(container => container.ContainerNumber);
			var (packQtyPerContainer, outturnQtyPerContainer) = GetPackQtyAndOutturnQtyPerContainer(upcOrPkcEvent);

			foreach (var containerGroup in containerGroups)
			{
				var containerNum = containerGroup.Key ?? ZString.Empty;
				if (containerNum.IsEmpty)
				{
					continue;
				}

				packQtyPerContainer.TryGetValue(containerNum, out long sumOfPackQtyForContainer);
				outturnQtyPerContainer.TryGetValue(containerNum, out long sumOfOutturnQtyForContainer);

				if ((upcOrPkcEvent == Events.PackingCompleted ? sumOfOutturnQtyForContainer : sumOfPackQtyForContainer) <= 0)
				{
					continue;
				}

				var parameters = upcOrPkcEvent == Events.PackingCompleted
					? GetPKCEventParams(sumOfOutturnQtyForContainer, containerNum)
					: GetUPCEventParams(sumOfPackQtyForContainer, sumOfOutturnQtyForContainer, containerNum);
				var location = parameters.First(param => param.Key == EventReferenceParameters.Codes.Location).Value;

				var eventVal = new EventValue(upcOrPkcEvent,
					isEstimate: false,
					eventTime: GetTimeForEvent(containerGroup, upcOrPkcEvent),
					parameters: parameters.ToImmutableDictionary()
				);
				var logToReplace = FindLogToReplaceByContainerNumAndLocation(upcOrPkcEvent, containerNum, location);

				shipmentBO.Logs.CreateRecreateOrUpdateEventLog(eventVal, logToReplace);
			}
		}

		Tuple<Dictionary<ZString, long>, Dictionary<ZString, long>> GetPackQtyAndOutturnQtyPerContainer(ZArchitecture.Business.Event upcOrPkcEvent)
		{
			if (shipmentDataObject.PackingLineCollection == null)
			{
				return Tuple.Create(new Dictionary<ZString, long>(), new Dictionary<ZString, long>());
			}

			var containerLinkLookup = shipmentDataObject.ContainerCollection.ToLookup(container => container.Link.GetValueOrDefault());
			var outturnQtyPerContainer = (from packingLine in shipmentDataObject.PackingLineCollection
										  let containerNumber = containerLinkLookup[packingLine.ContainerLink.GetValueOrDefault()].FirstOrDefault()?.ContainerNumber ?? ZString.Empty
										  group packingLine by containerNumber into g
										  select Tuple.Create(g.Key, g.Sum(x => x.OutturnQty.GetValueOrDefault()))).ToDictionary(x => x.Item1, x => (long)x.Item2);

			var containerASNLookup = (from shipment in shipmentDataObject.RelatedShipmentCollection ?? Enumerable.Empty<UniversalShipment>()
									  where (shipment?.ContainerCollection?.Any() ?? false) && (shipment?.DataContext?.DataSourceCollection?.Any() ?? false)
									  let transitReceiveASN = shipment.DataContext.DataSourceCollection.FirstOrDefault(source => (source.Type ?? ZString.Empty) == nameof(DataContextType.TransitReceiveASN))?.Key ?? ZString.Empty
									  select Tuple.Create(transitReceiveASN, shipment.ContainerCollection.FirstOrDefault()?.ContainerNumber ?? ZString.Empty)).ToLookup(x => x.Item1, x => x.Item2);

			var packQtyPerContainer = (upcOrPkcEvent == Events.PackingCompleted) ? new Dictionary<ZString, long>() :
				(from packingLine in shipmentDataObject.PackingLineCollection
				 let asn = packingLine.ReferenceNumberCollection?.FirstOrDefault(rn => (rn.Type?.Code ?? ZString.Empty) == Core.Constants.DocManagerCodes.TransitReceiveASN)?.ReferenceNumber ?? ZString.Empty
				 let containerNumber = containerASNLookup[asn].FirstOrDefault()
				 group packingLine by containerNumber into g
				 select Tuple.Create(g.Key, g.Sum(x => x.PackQty.GetValueOrDefault()))).ToDictionary(x => x.Item1, x => x.Item2);

			if (upcOrPkcEvent == Events.UnpackingCompleted)
			{
				foreach (var packingLine in shipmentDataObject.PackingLineCollection)
				{
					var asn = packingLine.ReferenceNumberCollection?.FirstOrDefault(rn => (rn.Type?.Code ?? ZString.Empty) == Core.Constants.DocManagerCodes.TransitReceiveASN)?.ReferenceNumber ?? ZString.Empty;
					var actualContainerNumber = containerLinkLookup[packingLine.ContainerLink.GetValueOrDefault()].FirstOrDefault()?.ContainerNumber ?? ZString.Empty;
					var expectedContainerNumber = containerASNLookup[asn].FirstOrDefault();
					if (!expectedContainerNumber.IsEmpty && !actualContainerNumber.IsEmpty && actualContainerNumber != expectedContainerNumber)
					{
						var message = Res.GetString("1be7d13d-1202-4f2f-975d-91f5c2229cb4", "On packing line {0}, the expected container {1} and the actual container {2} did not match.", asn, expectedContainerNumber, actualContainerNumber);
						logger.Log(Enterprise.Integration.LogType.Warning, message);
					}
				}
			}

			return Tuple.Create(packQtyPerContainer, outturnQtyPerContainer);
		}

		ZDateTimeOffset GetTimeForEvent(IEnumerable<Container> containers, ZArchitecture.Business.Event upcOrPkcEvent)
		{
			if (upcOrPkcEvent == Events.PackingCompleted)
			{
				return GetLatestDate(containers, container => container.PackDate);
			}
			return GetLatestDate(containers, container => container.LCLUnpack);
		}

		ZDateTimeOffset GetLatestDate(IEnumerable<Container> containers, Func<Container, ZDateTime?> selector)
		{
			var maxContainer = containers.MaxBySafe(selector);
			var maxDate = selector(maxContainer);
			return maxDate.HasValue ? maxDate.Value.ToOffset() : ZDateTimeOffset.Now;
		}

		StmALog FindLogToReplaceByContainerNumAndLocation(ZArchitecture.Business.Event upcOrPkcEvent, string containerNum, string location)
		{
			var events = shipmentBO.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(upcOrPkcEvent);
			return events.FirstOrDefault(evt =>
				evt.Parameters.TryGetValue(EventReferenceParameters.Codes.EquipmentReferenceNumber, out var eventContainerNum)
				&& eventContainerNum == containerNum
				&& evt.Parameters.TryGetValue(EventReferenceParameters.Codes.Location, out var eventLocation)
				&& eventLocation == location);
		}

		KeyValuePair<string, string>[] GetUPCEventParams(long ttl, long ptl, string containerNumber)
		{
			var result = GetPKCEventParams(ttl, containerNumber);

			if (ttl != ptl)
			{
				return result.Append(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Partial, ptl.ToString())).ToArray();
			}
			return result;
		}

		KeyValuePair<string, string>[] GetPKCEventParams(long ttl, string containerNumber)
		{
			var localCartageCFSOrg = shipmentDataObject.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.LocalCartageCFS));

			return new[]
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Facility, Core.Constants.FacilityType.Code.TransitWarehouse),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Location, localCartageCFSOrg.Port?.Code),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Total, ttl.ToString()),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.EquipmentReferenceNumber, containerNumber),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.Container),
			};
		}

		void CheckIfPackageExistInAnyPacklines(ForwardingPackage pkg)
		{
			var originalPackLines = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>();

			if (IsPackageIncludedInPackLines(originalPackLines, pkg))
			{
				var debugInfo = new ZStringBuilder();
				debugInfo.AppendLine(Res.GetString("221155ee-da6a-4673-b9e6-0bea3b8ed550", "There are Packing Line elements with the same Package."));
				debugInfo.AppendLine($"Package ID: {pkg.KP_PackageID}");
				debugInfo.AppendLine($"Package External Reference: {pkg.KP_ExternalReference}");
				debugInfo.AppendLine($"Shipment ID: {shipmentBO.JS_UniqueConsignRef}");

				for (var i = 0; i < shipmentDataObject.PackingLineCollection.Count; i++)
				{
					debugInfo.AppendLine($"---PackLineDO{i}---");
					var packLine = shipmentDataObject.PackingLineCollection[i];
					debugInfo.AppendLine($"packline.PackingLineID: {packLine.PackingLineID}");
					debugInfo.AppendLine($"packline.PackQty: {packLine.PackQty}");
					debugInfo.AppendLine($"packline.OutturnQty: {packLine.OutturnQty}");
					debugInfo.AppendLine($"packline.LoadDate: {packLine.LoadDate}");
					debugInfo.AppendLine($"packline.UnloadDate: {packLine.UnloadDate}");
					debugInfo.AppendLine($"packline.PackType: {packLine.PackType.Code}");

					debugInfo.AppendLine($"package.PackageID: {packLine.ReferenceNumber}");
					debugInfo.AppendLine($"package.ExternalReference: {packLine.PackingLineID}");
					debugInfo.AppendLine($"package.PreviousPackageID: {packLine.PreviousPackingLineID}");
				}

				for (var i = 0; i < originalPackLines.Count(); i++)
				{
					debugInfo.AppendLine($"---PackLineBO{i}---");
					var packLine = originalPackLines.ElementAt(i);
					debugInfo.AppendLine($"packline.PackLineID: {packLine.JL_PackLineId}");
					debugInfo.AppendLine($"packline.PackageCount: {packLine.JL_PackageCount}");
					debugInfo.AppendLine($"packline.PackType: {packLine.JL_F3_NKPackType}");
					debugInfo.AppendLine($"packline.LastKnownTransitWarehouseAddress: {packLine.JL_OA_LastKnownTransitWarehouseAddress}");
					debugInfo.AppendLine($"packline.LastKnownTransitWarehouseStatus: {packLine.JL_LastKnownTransitWarehouseStatus}");
					debugInfo.AppendLine($"packline.LastKnownTransitWarehouseStatusDateTime: {packLine.JL_LastKnownTransitWarehouseStatusDateTime}");
					debugInfo.AppendLine($"packline.TransitWarehouseMatchingStatus: {packLine.JL_OriginTransitWarehouseStatus}");

					var packages = packLine.PkgPackageCollection;
					for (var j = 0; j < packages.Count; j++)
					{
						debugInfo.AppendLine($"package{j}.PackageID: {packages[j].KP_PackageID}");
						debugInfo.AppendLine($"package{j}.ExternalReference: {packages[j].KP_ExternalReference}");
						debugInfo.AppendLine($"package{j}.PreviousPackageID: {packages[j].KP_PreviousPackLineID}");
					}
				}

				ErrorReporter.ReportOnce("Package exist in any PackLines", debugInfo.ToString() + System.Environment.StackTrace);
			}
		}

		bool IsPackageIncludedInPackLines(IEnumerable<ForwardingPackLine> packLines, ForwardingPackage pkg)
		{
			return pkg.KP_PackageID.IsEmpty
				? packLines.Any(packLine => packLine.PkgPackageCollection.Any(package => package.KP_ExternalReference == pkg.KP_ExternalReference))
				: packLines.Any(packLine => packLine.PkgPackageCollection.Any(package => package.KP_PackageID == pkg.KP_PackageID));
		}

		#endregion
	}
}
