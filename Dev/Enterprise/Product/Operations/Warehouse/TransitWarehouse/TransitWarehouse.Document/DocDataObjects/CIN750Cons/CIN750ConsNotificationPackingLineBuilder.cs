using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750ConsNotificationPackingLineBuilder : DocPackingLineBuilder
	{
		public CIN750ConsNotificationPackingLineBuilder(CIN750ConsNotification notification, ConsNotificationAdditionalData additionalData)
		{
			Notification = Argument.NotNull(notification, nameof(notification));
			DispatchConsignment = Argument.NotNull(notification.SourceBusinessObject as WhsItemDispatchConsignment, nameof(notification.SourceBusinessObject));
			DCNHouseBill = DispatchConsignment.HouseBillNumber;
			DCNMasterBill = DispatchConsignment.MasterBillNumber;
			AdditionalData = Argument.NotNull(additionalData, nameof(additionalData));
		}

		public (IEnumerable<DocPackingLine> FromPackingLine, DocPackingLine ToPackingLine) BuildDocPackingLines()
		{
			var fromPackingLines = new List<DocPackingLine>();
			var toPackingLine = Build();

			var inHistoryInfo = new Dictionary<ZString, List<NotificationHistoryInfo>>();
			var corHistoryInfo = new Dictionary<ZString, List<NotificationHistoryInfo>>();
			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(DispatchConsignment);
			var deconsHistory = historyManager.DeconsHistory;
			var consHistory = historyManager.ConsHistory;
			var originalPackagesInfo = new List<NotificationHistoryInfo>();
			var fromGoodRCNSourceId = new Dictionary<ZString, ZString>();

			if (AdditionalData.RCNHistoryPackageGroups.Any())
			{
				foreach (var packageGroup in AdditionalData.RCNHistoryPackageGroups.OrderBy(g => g.RefCode))
				{
					var fromPackingLine = Build();
					fromPackingLine.AmountQuantity = packageGroup.Quantity;
					fromPackingLine.AmountWeight = packageGroup.Weight;
					fromPackingLine.RefType = CreateCodeDescriptionPairFromRefType(packageGroup.RefType);
					fromPackingLine.RefCode = packageGroup.RefCode;
					fromPackingLine.Description = GetGoodsDescription(packageGroup.ParentRCN.PackageStates);
					fromPackingLine.SourceType = nameof(DataContextType.TransitReceive);
					fromPackingLine.SourceID = packageGroup.ParentRCN.WRC_JobID;
					SetShipmentDescriptionIfNotEmpty(fromPackingLine, packageGroup.ParentRCN);
					fromPackingLine.SourceReceiveConsignment = packageGroup.ParentRCN;

					if (!packageGroup.ParentRCN.MasterBillNumber.IsEmpty || !packageGroup.ParentRCN.HouseBillNumber.IsEmpty)
					{
						var fromGoodsTSD = packageGroup.ParentRCN.CustomsReferenceNumbers?.GetTempStorageDeclaration();
						fromGoodsTSD = fromGoodsTSD.HasValue && !fromGoodsTSD.Value.IsEmpty ? fromGoodsTSD : ZString.Empty;
						fromPackingLine.TemporaryStorageDeclaration = fromGoodsTSD ?? ZString.Empty;
					}

					fromPackingLines.Add(fromPackingLine);

					// wait for optimization
					var rcnHistoryManager = new ReceiveConsignmentCIN750NotificationHistoryManager(packageGroup.ParentRCN);
					inHistoryInfo[fromPackingLine.SourceID] = rcnHistoryManager.InHistory;
					corHistoryInfo[fromPackingLine.SourceID] = rcnHistoryManager.CorHistory;
					fromGoodRCNSourceId[fromPackingLine.SourceID] = packageGroup.ParentRCN.WRC_JobID;

					originalPackagesInfo.Add(CreateNotificationHistoryInfoFromPackingLine(fromPackingLine));
				}
				if (DCNHouseBill.IsEmpty)
				{
					PopulateToPackingLineForREF(toPackingLine);
				}
				else if (!DCNMasterBill.IsEmpty && AdditionalData.RCNHistoryPackageGroups.All(h => h.RefType == CIN750RefTypes.Codes.HouseAirWaybill && h.RefCode == DCNHouseBill))
				{
					PopulateToPackingLineForAWB(toPackingLine);
				}
				else
				{
					PopulateToPackingLineForHWB(toPackingLine);
				}
			}
			else if (AdditionalData.DCNPackageGroupForHWB != null)
			{
				var matchedHWBPackageGroup = AdditionalData.DCNPackageGroupForHWB;
				var fromPackingLine = Build();
				fromPackingLine.AmountQuantity = matchedHWBPackageGroup.Quantity;
				fromPackingLine.AmountWeight = matchedHWBPackageGroup.Weight;
				fromPackingLine.RefType = CreateCodeDescriptionPairFromRefType(matchedHWBPackageGroup.RefType);
				fromPackingLine.RefCode = matchedHWBPackageGroup.RefCode;
				fromPackingLine.Description = GetGoodsDescription(DispatchConsignment.PackageStates);
				fromPackingLine.SourceType = nameof(DataContextType.TransitDispatch);
				fromPackingLine.SourceID = DispatchConsignment.WDC_JobID;
				SetShipmentDescriptionIfNotEmpty(fromPackingLine, DispatchConsignment);
				fromPackingLines.Add(fromPackingLine);
				originalPackagesInfo.Add(CreateNotificationHistoryInfoFromPackingLine(fromPackingLine));

				if (DCNMasterBill.IsEmpty)
				{
					PopulateToPackingLineForHWB(toPackingLine, ignoreReported: true);
				}
				else
				{
					PopulateToPackingLineForAWB(toPackingLine);
				}
			}
			else if (AdditionalData.DCNPackageGroupForAWB != null)
			{
				var matchedHistory = AdditionalData.DCNPackageGroupForAWB;
				var fromPackingLine = Build();
				fromPackingLine.AmountQuantity = matchedHistory.Quantity;
				fromPackingLine.AmountWeight = matchedHistory.Weight;
				fromPackingLine.RefType = CreateCodeDescriptionPairFromRefType(matchedHistory.RefType);
				fromPackingLine.RefCode = matchedHistory.RefCode;
				fromPackingLine.Description = GetGoodsDescription(DispatchConsignment.PackageStates);
				SetShipmentDescriptionIfNotEmpty(fromPackingLine, DispatchConsignment);
				fromPackingLines.Add(fromPackingLine);
				originalPackagesInfo.Add(CreateNotificationHistoryInfoFromPackingLine(fromPackingLine));

				PopulateToPackingLineForAWB(toPackingLine, ignoreReported: true);
			}

			Notification.RefType = CreateCodeDescriptionPairFromRefType(toPackingLine.RefType.Code);
			Notification.RefCode = toPackingLine.RefCode;
			Notification.HasOvps = fromPackingLines.Sum(p => p.AmountQuantity) > toPackingLine.AmountQuantity;

			Notification.OriginalPackagesInfo = originalPackagesInfo;
			Notification.InHistoryInfo = inHistoryInfo;
			Notification.CorHistoryInfo = corHistoryInfo;
			Notification.DeconsHistoryInfo = deconsHistory;
			Notification.ConsHistoryInfo = consHistory;
			Notification.FromGoodRCNSourceId = fromGoodRCNSourceId;
			return (fromPackingLines, toPackingLine);
		}

		static NotificationHistoryInfo CreateNotificationHistoryInfoFromPackingLine(DocPackingLine fromDocPackingLine)
		{
			return new NotificationHistoryInfo()
			{
				RefType = fromDocPackingLine.SourceType,
				JobID = fromDocPackingLine.SourceID,
				Quantity = fromDocPackingLine.AmountQuantity,
				Weight = fromDocPackingLine.AmountWeight,
			};
		}

		void PopulateToPackingLineForREF(DocPackingLine toPackingLine)
		{
			toPackingLine.RefType = CreateCodeDescriptionPairFromRefType(CIN750RefTypes.Codes.Reference);
			toPackingLine.RefCode = TransitDocumentHelper.GetEnterpiseAndServerCode() + DispatchConsignment.WDC_ConsignmentID;

			FillToPackingLine(toPackingLine, 0, 0);
		}

		void PopulateToPackingLineForHWB(DocPackingLine toPackingLine, bool ignoreReported = false)
		{
			toPackingLine.RefType = CreateCodeDescriptionPairFromRefType(CIN750RefTypes.Codes.HouseAirWaybill);
			toPackingLine.RefCode = DCNHouseBill;

			var reportedQuantity = ignoreReported ? 0 : AdditionalData.DCNPackageGroupForHWB?.Quantity ?? 0 + AdditionalData.DCNPackageGroupForAWB?.Quantity ?? 0;
			var reportedWeight = ignoreReported ? 0 : AdditionalData.DCNPackageGroupForHWB?.Weight ?? 0 + AdditionalData.DCNPackageGroupForAWB?.Weight ?? 0;

			FillToPackingLine(toPackingLine, reportedQuantity, reportedWeight);
		}

		void PopulateToPackingLineForAWB(DocPackingLine toPackingLine, bool ignoreReported = false)
		{
			toPackingLine.RefType = CreateCodeDescriptionPairFromRefType(CIN750RefTypes.Codes.MasterAirWaybill);
			toPackingLine.RefCode = DCNMasterBill;

			var reportedQuantity = ignoreReported ? 0 : AdditionalData.DCNPackageGroupForAWB?.Quantity ?? 0;
			var reportedWeight = ignoreReported ? 0 : AdditionalData.DCNPackageGroupForAWB?.Weight ?? 0;

			FillToPackingLine(toPackingLine, reportedQuantity, reportedWeight);
		}

		void FillToPackingLine(DocPackingLine toPackingLine, int reportedQuantity, decimal reportedWeight)
		{
			toPackingLine.AmountQuantity = AdditionalData.PackageQuantityToCons - reportedQuantity;
			toPackingLine.AmountWeight = AdditionalData.PackageWeightToCons - reportedWeight;
			toPackingLine.Description = GetGoodsDescription(DispatchConsignment.PackageStates);
			toPackingLine.SourceType = nameof(DataContextType.TransitDispatch);
			toPackingLine.SourceID = DispatchConsignment.WDC_JobID;
			SetShipmentDescriptionIfNotEmpty(toPackingLine, DispatchConsignment);
		}

		CodeDescription CreateCodeDescriptionPairFromRefType(string refType) => new CodeDescription(new CIN750RefTypes())
		{
			Code = refType
		};

		readonly CIN750ConsNotification Notification;
		readonly WhsItemDispatchConsignment DispatchConsignment;
		readonly ZString DCNMasterBill;
		readonly ZString DCNHouseBill;

		ConsNotificationAdditionalData AdditionalData { get; }
	}
}
