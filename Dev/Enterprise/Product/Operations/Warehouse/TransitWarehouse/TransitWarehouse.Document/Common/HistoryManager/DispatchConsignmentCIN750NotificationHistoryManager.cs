using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Document
{
	public class DispatchConsignmentCIN750NotificationHistoryManager : CIN750NotificationHistoryManager<WhsItemDispatchConsignment>
	{
		public DispatchConsignmentCIN750NotificationHistoryManager(WhsItemDispatchConsignment dcn)
		{
			Parent = dcn;
			HistoryInfo = GetCIN750NotificationHistory(dcn);
		}

		public OutNotificationAdditionalData AdditionalDataForOut { get; private set; }

		public ConsNotificationAdditionalData AdditionalDataForCons { get; private set; }

		public override (CIN750NotificationMessageTypes? Type, string ErrorMessage) GetNextMessageTypeCore(CIN750NotificationMessageTypes? messageType = null)
		{
			var errorMessage = string.Empty;
			if (!Parent.MasterBillNumber.IsEmpty && Parent.HouseBillNumber.IsEmpty)
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("91b400d8-742f-40b6-aa7a-8ebd6f2b2f22", "the Dispatch Consignment does not have a House Bill to do a Consolidation into Master Bill"));
				return (null, errorMessage);
			}
			var isOutByReference = Parent.MasterBillNumber.IsEmpty && Parent.HouseBillNumber.IsEmpty;
			var isOutByMasterBill = !Parent.MasterBillNumber.IsEmpty && !Parent.HouseBillNumber.IsEmpty;
			var isOutByHouseBill = Parent.MasterBillNumber.IsEmpty && !Parent.HouseBillNumber.IsEmpty;

			var dcnRelatedPackages = GetDCNRelatedPackageStates(Parent);
			var dcnRelatedPackagesGroupedByRCN = dcnRelatedPackages.GroupBy(p => p.ReceiveConsignmentWithInnersAndBreakDownInnersFallBack);
			var relatedRCNs = dcnRelatedPackagesGroupedByRCN.Where(g => g.Key != null)
				.Select(p => p.Key);

			var rcnHistoryManagers = relatedRCNs.Select(rcn => new ReceiveConsignmentCIN750NotificationHistoryManager(rcn)).ToList();
			foreach (var manager in rcnHistoryManagers)
			{
				var rcnNextMessageType = manager.GetNextMessageType();
				if (rcnNextMessageType.HasValue)
				{
					errorMessage = GetErrorMessage(messageType, rcnNextMessageType, Res.GetString("9041f404-a6b0-4eff-9732-490c0fa6ddaf", "one or more related Receive Consignment do not have all Packages reported by CIN Notification"));
					return (rcnNextMessageType, errorMessage);
				}
			}

			var rcnPackageGroups = rcnHistoryManagers.Where(manager => manager.InHistory.Any()).Select(manager =>
			{
				return new HistoryPackageGroup()
				{
					Quantity = manager.InHistory.Sum(h => h.Quantity) + manager.CorHistory.Sum(h => h.Quantity),
					Weight = manager.InHistory.Sum(h => h.Weight) + manager.CorHistory.Sum(h => h.Weight),
					RefType = manager.InHistory.FirstOrDefault().RefType,
					RefCode = manager.InHistory.FirstOrDefault().RefCode,
					ParentRCN = manager.Parent
				};
			}).ToList();

			var dcnPackageGroups = new List<HistoryPackageGroup>();

			foreach (var history in HistoryInfo.OrderBy(h => h.LogCreatedTime))
			{
				if (history.LogType.StartsWith(nameof(CIN750NotificationMessageTypes.CIN750DeconsNotification)))
				{
					if (history.IsFromRCN)
					{
						var matchedGroup = MatchPackageGroup(rcnPackageGroups, history);
						if (matchedGroup != null)
						{
							if (matchedGroup.Quantity == history.Quantity)
							{
								rcnPackageGroups.Remove(matchedGroup);
							}
							else
							{
								matchedGroup.Quantity -= history.Quantity;
								matchedGroup.Weight -= history.Weight;
							}
						}
					}
					else if (history.IsTo)
					{
						var matchedGroup = MatchPackageGroup(dcnPackageGroups, history);
						if (matchedGroup != null)
						{
							matchedGroup.Quantity += history.Quantity;
							matchedGroup.Weight += history.Weight;
						}
						else
						{
							var newGroup = new HistoryPackageGroup()
							{
								RefType = history.RefType,
								RefCode = history.RefCode,
								Quantity = history.Quantity,
								Weight = history.Weight
							};
							dcnPackageGroups.Add(newGroup);
						}
					}
				}
				else if (history.LogType.StartsWith(nameof(CIN750NotificationMessageTypes.CIN750ConsNotification)))
				{
					if (history.IsFromRCN)
					{
						var matchedGroup = MatchPackageGroup(rcnPackageGroups, history);
						if (matchedGroup != null)
						{
							if (matchedGroup.Quantity == history.Quantity)
							{
								rcnPackageGroups.Remove(matchedGroup);
							}
							else
							{
								matchedGroup.Quantity -= history.Quantity;
								matchedGroup.Weight -= history.Weight;
							}
						}
					}
					if (history.IsFromDCN)
					{
						var matchedGroup = MatchPackageGroup(dcnPackageGroups, history);
						if (matchedGroup != null)
						{
							if (matchedGroup.Quantity == history.Quantity)
							{
								dcnPackageGroups.Remove(matchedGroup);
							}
							else
							{
								matchedGroup.Quantity -= history.Quantity;
								matchedGroup.Weight -= history.Weight;
							}
						}
					}
					else if (history.IsTo)
					{
						var matchedGroup = MatchPackageGroup(dcnPackageGroups, history);
						if (matchedGroup != null)
						{
							matchedGroup.Quantity += history.Quantity;
							matchedGroup.Weight += history.Weight;
						}
						else
						{
							var newGroup = new HistoryPackageGroup()
							{
								RefType = history.RefType,
								RefCode = history.RefCode,
								Quantity = history.Quantity,
								Weight = history.Weight
							};
							dcnPackageGroups.Add(newGroup);
						}
					}
				}
			}
			if (isOutByReference)
			{
				if (rcnPackageGroups.All(g => g.RefType == RefTypeStrings.ReferenceTypeStr))
				{
					return CheckAndCalculateSendingOutNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType);
				}
				else
				{
					errorMessage = GetErrorMessage(messageType, null, Res.GetString("ab99b782-03d6-4c25-8514-2e635c60fc3f", "the Dispatch Consignment does not have a House Bill to do a Consolidation"));
					return (null, errorMessage);
				}
			}
			else if (isOutByHouseBill)
			{
				if (rcnPackageGroups.Any())
				{
					if (rcnPackageGroups.Any(g => g.RefType == RefTypeStrings.MasterBillTypeStr))
					{
						errorMessage = GetErrorMessage(messageType, CIN750NotificationMessageTypes.CIN750DeconsNotification, Res.GetString("d5501c5d-eee9-4072-a4d1-fe5e8ce824ad", "the Packages on the Dispatch Consignment haven't been reported De-consolidations to House Bill yet"));
						return (CIN750NotificationMessageTypes.CIN750DeconsNotification, errorMessage);
					}
					else if (rcnPackageGroups.Any(g => g.RefType == RefTypeStrings.HouseBillTypeStr && g.RefCode != Parent.HouseBillNumber || rcnPackageGroups.Any(g => g.RefType == RefTypeStrings.ReferenceTypeStr)))
					{
						return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("80fec2cc-fecb-40f6-8f40-25109f9039e8", "the Dispatch Consignment's House Bill does not match the reported Packages"));
					}
					else
					{
						return CheckAndCalculateSendingOutNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType);
					}
				}
				else
				{
					if (dcnPackageGroups.Any(g => g.RefType == RefTypeStrings.HouseBillTypeStr && g.RefCode != Parent.HouseBillNumber))
					{
						return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("a9c7017d-799a-400c-bc3c-e3eb019c6747", "the Dispatch Consignment's House Bill does not match the reported Packages"));
					}
					else
					{
						return CheckAndCalculateSendingOutNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType);
					}
				}
			}
			else if (isOutByMasterBill)
			{
				if (rcnPackageGroups.Any())
				{
					if (rcnPackageGroups.All(g => g.RefType == RefTypeStrings.MasterBillTypeStr && g.RefCode == Parent.MasterBillNumber))
					{
						return CheckAndCalculateSendingOutNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType);
					}
					else if (rcnPackageGroups.Any(g => g.RefType == RefTypeStrings.MasterBillTypeStr))
					{
						errorMessage = GetErrorMessage(messageType, CIN750NotificationMessageTypes.CIN750DeconsNotification, Res.GetString("2ab42e2b-d656-46d7-a4ed-0a45caee532f", "the Dispatch Consignment's Master Bill does not match the reported Packages"));
						return (CIN750NotificationMessageTypes.CIN750DeconsNotification, errorMessage);
					}
					else if (rcnPackageGroups.Any(g => g.RefType == RefTypeStrings.HouseBillTypeStr && g.RefCode != Parent.HouseBillNumber))
					{
						return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("3ea54fe5-740c-4608-80cf-0841044ddec3", "the Dispatch Consignment's House Bill does not match the reported Packages"));
					}
					else
					{
						return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("fd361708-ba74-42b5-a72c-261dbeb64cca", "the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill"));
					}
				}
				else
				{
					if (dcnPackageGroups.Any(g => g.RefType == RefTypeStrings.HouseBillTypeStr))
					{
						return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("fd361708-ba74-42b5-a72c-261dbeb64cca", "the Packages on the Dispatch Consignment need to be reported Consolidation into its Master Bill"));
					}
					else
					{
						return CheckAndCalculateSendingOutNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType);
					}
				}
			}
			return (null, errorMessage);
		}

		(CIN750NotificationMessageTypes? Type, string ErrorMessage) CheckAndCalculateSendingOutNotification(List<HistoryPackageGroup> rcnPackageGroups, List<HistoryPackageGroup> dcnPackageGroups, List<WhsItemPackageState> dcnRelatedPackages, CIN750NotificationMessageTypes? messageType)
		{
			var errorMessage = string.Empty;
			var reportedPackageQuantity = rcnPackageGroups.Sum(g => g.Quantity) + dcnPackageGroups.Sum(g => g.Quantity);
			var dcnRelatedPackagesQuantity = dcnRelatedPackages.Sum(p => p.Package.KP_PackageQty);

			if (reportedPackageQuantity < dcnRelatedPackagesQuantity)
			{
				errorMessage = GetErrorMessage(messageType, CIN750NotificationMessageTypes.CIN750DeconsNotification, Res.GetString("0b140d6a-bfde-49e6-9b1b-41aa1eacb500", "the reported Quantity of Packages on the Dispatch Consignment does not match the Quantity in the Warehouse"));
				return (CIN750NotificationMessageTypes.CIN750DeconsNotification, errorMessage);
			}
			else if (reportedPackageQuantity > dcnRelatedPackagesQuantity)
			{
				return CheckAndCalculateSendingConsNotification(rcnPackageGroups, dcnPackageGroups, dcnRelatedPackages, messageType, Res.GetString("5214d846-19dd-4fe2-b48e-12407c3bb9df", "the reported Quantity of Packages on the Dispatch Consignment does not match the Quantity in the Warehouse"));
			}

			var packagesToOut = dcnRelatedPackages.FindAll(p => !p.WPS_WDH_TransitDispatchHeader.IsEmpty);
			var packageQuantityToOut = packagesToOut.Sum(p => p.Package.KP_PackageQty);

			var historyOutQuantity = OutHistory.Sum(h => h.Quantity);
			if (packageQuantityToOut == 0)
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("d4276ecd-a446-4e71-9942-f6cd783b5964", "the Packages on the Dispatch Consignment have not been Departed yet"));
				return (null, errorMessage);
			}
			else if (historyOutQuantity < packageQuantityToOut)
			{
				errorMessage = GetErrorMessage(messageType, CIN750NotificationMessageTypes.CIN750OutNotification, Res.GetString("b28872a8-d740-4937-9d15-4a2420515a4d", "the Consolidation and Deconsolidation of the Dispatch Consignment are both reported completely"));

				var historyOutWeight = OutHistory.Sum(h => h.Weight);
				var packageWeightToOut = packagesToOut.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms));

				AdditionalDataForOut = new OutNotificationAdditionalData()
				{
					PackageQuantityReadyToOut = packageQuantityToOut - historyOutQuantity,
					PackageWeightReadyToOut = packageWeightToOut - historyOutWeight
				};

				return (CIN750NotificationMessageTypes.CIN750OutNotification, errorMessage);
			}
			else if (historyOutQuantity == dcnRelatedPackagesQuantity)
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("1b9e5c56-e78f-4b01-bb04-02c6a79d143e", "the Packages on the Dispatch Consignment are reported Out completely"));
				return (null, errorMessage);
			}
			else if (historyOutQuantity == packageQuantityToOut)
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("bf0f8d41-3dc8-43fe-be30-f8e525c29d28", "some Packages on the Dispatch Consignment have not been Departed yet"));
				return (null, errorMessage);
			}
			else
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("d2845ee7-9e84-4233-a80e-3fbb68a9cba3", "the reported Out Quantity of Packages on the Dispatch Consignment is greater than the actual Quantity"));
				return (null, errorMessage);
			}
		}

		(CIN750NotificationMessageTypes? Type, string ErrorMessage) CheckAndCalculateSendingConsNotification(List<HistoryPackageGroup> rcnPackageGroups, List<HistoryPackageGroup> dcnPackageGroups, List<WhsItemPackageState> dcnRelatedPackages, CIN750NotificationMessageTypes? messageType, string baseMessage)
		{
			var errorMessage = GetErrorMessage(messageType, CIN750NotificationMessageTypes.CIN750ConsNotification, baseMessage);
			AdditionalDataForCons = new ConsNotificationAdditionalData()
			{
				RCNHistoryPackageGroups = rcnPackageGroups,
				DCNPackageGroupForHWB = dcnPackageGroups.FirstOrDefault(g => g.RefType == RefTypeStrings.HouseBillTypeStr),
				DCNPackageGroupForAWB = dcnPackageGroups.FirstOrDefault(g => g.RefType == RefTypeStrings.MasterBillTypeStr),
				PackageQuantityToCons = dcnRelatedPackages.Sum(p => p.Package.KP_PackageQty),
				PackageWeightToCons = dcnRelatedPackages.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms))
			};
			return (CIN750NotificationMessageTypes.CIN750ConsNotification, errorMessage);
		}

		HistoryPackageGroup MatchPackageGroup(IEnumerable<HistoryPackageGroup> source, NotificationHistoryInfo history)
		{
			if (history.RefType == RefTypeStrings.ReferenceTypeStr)
			{
				return source.FirstOrDefault(g => g.RefType == RefTypeStrings.ReferenceTypeStr && g.RefCode == history.Reference);
			}
			else if (history.RefType == RefTypeStrings.MasterBillTypeStr)
			{
				return source.FirstOrDefault(g => g.RefType == RefTypeStrings.MasterBillTypeStr && g.RefCode == history.MasterBill);
			}
			else if (history.RefType == RefTypeStrings.HouseBillTypeStr)
			{
				return source.FirstOrDefault(g => g.RefType == RefTypeStrings.HouseBillTypeStr && g.RefCode == history.HouseBill);
			}
			return null;
		}

		List<WhsItemPackageState> GetDCNRelatedPackageStates(WhsItemDispatchConsignment dcn)
		{
			var packageStateQuery = @"
WPS_PK IN
(
	SELECT
		PkgState.WPS_PK
	FROM
		dbo.WhsItemPackageState AS PkgState
		JOIN dbo.PkgPackage AS Package ON Package.KP_PK = PkgState.WPS_KP_Package
		LEFT JOIN dbo.PkgPackage TopPkg ON Package.KP_KP_TopHandlingUnitPackage = TopPkg.KP_PK
		LEFT JOIN dbo.WhsItemPackageState TopPkgState ON TopPkg.KP_PK = TopPkgState.WPS_KP_Package
	WHERE
		PkgState.WPS_WDC_TransitDispatchConsignment = @DispatchConsignmentPK
		AND PkgState.WPS_Status != 'ADJ'
		AND (Package.KP_KP_TopHandlingUnitPackage IS NULL
		OR (TopPkgState.WPS_UnitType IN ('CNT', 'ULD', 'HU') AND Package.KP_KP_ParentPackage IS NULL)
		OR TopPkgState.WPS_WDC_TransitDispatchConsignment != @DispatchConsignmentPK)
)";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@DispatchConsignmentPK", dcn.PK, WhsItemDispatchConsignmentSchema.PK },
			};

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddFilterAndZSQLParameterCollection(packageStateQuery, sqlParams);

			var dcnRelatedPackageStates = dcn.Factory.Load<WhsItemPackageState>(query).ToList();
			return dcnRelatedPackageStates;
		}
	}
}
