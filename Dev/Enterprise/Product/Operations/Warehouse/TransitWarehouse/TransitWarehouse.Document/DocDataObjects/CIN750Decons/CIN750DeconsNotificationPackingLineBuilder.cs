using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750DeconsNotificationPackingLineBuilder : DocPackingLineBuilder
	{
		public CIN750DeconsNotificationPackingLineBuilder(CIN750DeconsNotification notification)
		{
			Notification = notification;
			DispatchConsignment = Argument.NotNull(notification.SourceBusinessObject as WhsItemDispatchConsignment, nameof(notification.SourceBusinessObject));
		}

		readonly CIN750DeconsNotification Notification;
		readonly WhsItemDispatchConsignment DispatchConsignment;

		public IEnumerable<Tuple<DocPackingLine, DocPackingLine>> BuildDocPackingLines()
		{
			var originalPackagesInfo = new List<NotificationHistoryInfo>();
			var dcnHistoryManager = new DispatchConsignmentCIN750NotificationHistoryManager(DispatchConsignment);
			Notification.DeconsHistoryInfo = dcnHistoryManager.DeconsHistory;
			var inHistoryInfo = new Dictionary<ZString, List<NotificationHistoryInfo>>();
			var corHistoryInfo = new Dictionary<ZString, List<NotificationHistoryInfo>>();

			var dcnHouseBill = DispatchConsignment.HouseBillNumber;
			var dcnMasterBill = DispatchConsignment.MasterBillNumber;
			var goodsPairs = new List<Tuple<DocPackingLine, DocPackingLine>>();

			var packageStates = DispatchConsignment.PackageStates.Where(p => p.WPS_Status != TransitWarehouseStatuses.Codes.AdjustedOut
				&& (p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.PackedPackage
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.PackedPackline
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.PackedNonTracked
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.ScannedIn
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.ScannedInAsInner
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.SkipScanMode
					|| p.WPS_ReceivedAs == TransitWarehouseReceiveAs.Codes.Adhoc));
			var packageStateGroups = packageStates.Select(p => p.BreakDownParentPackageState ?? p).Distinct().GroupBy(p => p.ReceiveConsignmentWithInnersAndBreakDownInnersFallBack).OrderBy(p => p.Key.WRC_JobID);

			foreach (var packageStateGroup in packageStateGroups)
			{
				var rcn = packageStateGroup.Key;
				var rcnMasterBill = rcn.MasterBillNumber;
				var fromPackageStates = packageStateGroup.AsEnumerable();
				var fromPackageStatePKs = fromPackageStates.Select(p => p.PK).ToList();
				var toPackageStates = packageStates;

				if (rcnMasterBill.IsEmpty || rcnMasterBill == dcnMasterBill)
				{
					toPackageStates = packageStates.Where(p => p.BreakDownParentPackageState != null && fromPackageStatePKs.Contains(p.BreakDownParentPackageState.PK));
					fromPackageStates = toPackageStates.Select(p => p.BreakDownParentPackageState).Distinct();
				}
				else
				{
					toPackageStates = packageStates.Where(p => p.BreakDownParentPackageState == null ? fromPackageStatePKs.Contains(p.PK) : fromPackageStatePKs.Contains(p.BreakDownParentPackageState.PK));
				}

				var rcnHistoryManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);

				var fromDocPackingLine = CreatePackingLineFromPackageStates(fromPackageStates);
				fromDocPackingLine.DeconsMessageID = ZGuid.NewZGuid().ToString();
				fromDocPackingLine.SourceReceiveConsignment = rcn;
				originalPackagesInfo.Add(new NotificationHistoryInfo()
				{
					LogType = nameof(CIN750NotificationMessageTypes.CIN750DeconsNotification) + NotificationHistoryInfo.FromHistorySuffix,
					JobID = rcn.WRC_JobID,
					Quantity = fromDocPackingLine.AmountQuantity,
					Weight = fromDocPackingLine.AmountWeight,
				});

				var rcnDeconsHistoryInfo = Notification.DeconsHistoryInfo.Where(h => h.IsFromRCN && h.JobID == rcn.WRC_JobID).ToList();
				var rcnInHistoryInfo = rcnHistoryManager.InHistory;
				var rcnCorHistoryInfo = rcnHistoryManager.CorHistory;
				var totalQuantity = rcnInHistoryInfo.Sum(q => q.Quantity) + rcnCorHistoryInfo.Sum(q => q.Quantity);
				var totalWeight = rcnInHistoryInfo.Sum(q => q.Weight) + rcnCorHistoryInfo.Sum(q => q.Weight);
				inHistoryInfo[rcn.WRC_JobID] = rcnInHistoryInfo;
				corHistoryInfo[rcn.WRC_JobID] = rcnCorHistoryInfo;

				fromDocPackingLine.AmountQuantity = Math.Max(Math.Min(fromDocPackingLine.AmountQuantity, totalQuantity) - rcnDeconsHistoryInfo.Sum(h => h.Quantity), 0);
				fromDocPackingLine.AmountWeight = Math.Max(Math.Min(fromDocPackingLine.AmountWeight, totalWeight) - rcnDeconsHistoryInfo.Sum(h => h.Weight), 0);

				var toDocPackingLine = CreatePackingLineFromPackageStates(toPackageStates);
				originalPackagesInfo.Add(new NotificationHistoryInfo()
				{
					LogType = nameof(CIN750NotificationMessageTypes.CIN750DeconsNotification) + NotificationHistoryInfo.ToHistorySuffix,
					JobID = rcn.WRC_JobID,
					Quantity = toDocPackingLine.AmountQuantity,
					Weight = toDocPackingLine.AmountWeight,
				});

				var dcnDeconsHistoryInfo = Notification.DeconsHistoryInfo.Where(h => h.IsTo && h.JobID == rcn.WRC_JobID);
				toDocPackingLine.AmountQuantity -= dcnDeconsHistoryInfo.Sum(h => h.Quantity);
				toDocPackingLine.AmountWeight -= dcnDeconsHistoryInfo.Sum(h => h.Weight);
				toDocPackingLine.SourceID = rcn.WRC_JobID;

				fromDocPackingLine.SourceType = nameof(DataContextType.TransitReceive);
				fromDocPackingLine.SourceID = rcn.WRC_JobID;

				var ovps = packageStates.Where(p => p.WPS_UnitType == PackageStateUnitType.Codes.Overpack);
				SetShipmentDescriptionIfNotEmpty(fromDocPackingLine, rcn, ovps);
				SetShipmentDescriptionIfNotEmpty(toDocPackingLine, DispatchConsignment);

				if (rcnMasterBill.IsEmpty || rcnMasterBill == dcnMasterBill)
				{
					if (rcn.HouseBillNumber.IsEmpty)
					{
						fromDocPackingLine.RefType = new CodeDescription(new CIN750RefTypes())
						{
							Code = CIN750RefTypes.Codes.Reference
						};
						fromDocPackingLine.RefCode = TransitDocumentHelper.GetEnterpiseAndServerCode() + rcn.WRC_ConsignmentID;
					}
					else
					{
						fromDocPackingLine.RefType = new CodeDescription(new CIN750RefTypes())
						{
							Code = CIN750RefTypes.Codes.HouseAirWaybill
						};
						fromDocPackingLine.RefCode = rcn.HouseBillNumber;
					}
				}
				else
				{
					fromDocPackingLine.RefType = new CodeDescription(new CIN750RefTypes())
					{
						Code = CIN750RefTypes.Codes.MasterAirWaybill
					};
					fromDocPackingLine.RefCode = rcnMasterBill;

					var fromGoodsTSD = rcn.CustomsReferenceNumbers?.GetTempStorageDeclaration() ?? ZString.Empty;
					var toGoodsTSD = toPackageStates
						.Select(p => p.ReceiveConsignmentWithInnersAndBreakDownInnersFallBack?.CustomsReferenceNumbers?.GetTempStorageDeclaration() ?? ZString.Empty)
						.FirstOrDefault(p => !p.IsEmpty);

					fromDocPackingLine.TemporaryStorageDeclaration = fromGoodsTSD;
					toDocPackingLine.TemporaryStorageDeclaration = toGoodsTSD;
				}
				toDocPackingLine.RefType = new CodeDescription(new CIN750RefTypes())
				{
					Code = CIN750RefTypes.Codes.HouseAirWaybill
				};
				toDocPackingLine.RefCode = dcnHouseBill;
				if (fromDocPackingLine.AmountQuantity > 0 && toDocPackingLine.AmountQuantity > 0)
				{
					goodsPairs.Add(new Tuple<DocPackingLine, DocPackingLine>(fromDocPackingLine, toDocPackingLine));
				}
			}

			Notification.OriginalPackagesInfo = originalPackagesInfo;
			Notification.InHistoryInfo = new ReadOnlyDictionary<ZString, List<NotificationHistoryInfo>>(inHistoryInfo);
			Notification.CorHistoryInfo = new ReadOnlyDictionary<ZString, List<NotificationHistoryInfo>>(corHistoryInfo);
			return goodsPairs;
		}

		protected override void AddValidation(DocPackingLine docPackingLine)
		{
			base.AddValidation(docPackingLine);
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity < 0, Res.GetString("2df9e13d-4129-47c1-be48-65821119ccd2", "Amount Quantity must be greater than 0."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight < 0, Res.GetString("c5b283b9-7622-4ca0-8613-d1b5f53ee0da", "Amount Weight must be greater than 0."));
		}
	}
}
