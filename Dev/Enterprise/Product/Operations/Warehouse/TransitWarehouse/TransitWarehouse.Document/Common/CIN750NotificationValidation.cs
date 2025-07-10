using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public static class CIN750NotificationValidation
	{
		public static ZString GetInNotificationValidationMessage(CIN750InNotification docDataObject)
		{
			var notificationHistory = docDataObject.InHistoryInfo;
			var message = ZString.Empty;
			if (docDataObject.RefType.Code == CIN750RefTypes.Codes.MasterAirWaybill && notificationHistory.Any(h => h.RefType != CIN750RefTypes.Codes.MasterAirWaybill))
			{
				message = Res.GetString("6d2e4acb-29b5-403b-8422-d4450d82a08b", "Cannot Send CIN 750 In Notification with Reference Type Master Air Waybill because it does not match the history Notifications.");
			}
			else if (docDataObject.RefType.Code == CIN750RefTypes.Codes.Reference && notificationHistory.Any(h => h.RefType != CIN750RefTypes.Codes.Reference))
			{
				message = Res.GetString("7c25dbb6-581b-4262-a158-79fafa94e2a0", "Cannot Send CIN 750 In Notification with Reference Type Reference because it does not match the history Notifications.");
			}
			else if (notificationHistory.Any(h => (h.RefType == CIN750RefTypes.Codes.Reference && h.Reference != docDataObject.RefCode) ||
												  (h.RefType == CIN750RefTypes.Codes.MasterAirWaybill && h.MasterBillWithoutHyphen != docDataObject.RefCode) ||
												  (h.RefType == CIN750RefTypes.Codes.HouseAirWaybill && h.HouseBill != docDataObject.RefCode)))
			{
				message = Res.GetString("0bd97fc2-118f-4a0e-b6b1-01fa625f8c18", "Cannot Send CIN 750 In Notification with Reference Code {0} because it does not match the history Notifications.", docDataObject.RefCode);
			}
			else if (docDataObject.Goods.Count == 0)
			{
				message = Res.GetString("3156f8bf-6d07-4ce0-bb83-2fb89efa0e54", "Cannot Send CIN 750 In Notification without Goods Detail.");
			}
			else if (message.IsEmpty && docDataObject.Goods.Count == 1)
			{
				var packingLine = docDataObject.Goods.Single();
				var inNotificationHistoryQuantity = docDataObject.InHistoryInfo.Sum(h => h.Quantity) + docDataObject.CorHistoryInfo.Sum(h => h.Quantity);
				var inNotificationHistoryWeight = docDataObject.InHistoryInfo.Sum(h => h.Weight) + docDataObject.CorHistoryInfo.Sum(h => h.Weight);
				message = ValidateQuantityAndWeight(TransitDocDataConstants.NotificationTypes.CIN750InNotification, packingLine, docDataObject.ReceivedPackageQuantity, docDataObject.ReceivedPackageWeight, inNotificationHistoryQuantity, inNotificationHistoryWeight);
			}
			return message;
		}

		public static ZString GetDeconsNotificationValidationMessage(CIN750DeconsNotification docDataObject)
		{
			var message = ZString.Empty;
			if (docDataObject.GoodsPairs.Count == 0)
			{
				message = Res.GetString("9da373df-900e-4320-a536-61ab1640a835", "Cannot Send CIN 750 Deconsolidation Notification without Goods Detail.");
			}
			else
			{
				var fromDocPackingLinesHistory = docDataObject.DeconsHistoryInfo.Where(h => h.IsFromRCN);
				var toDocPackingLineHistory = docDataObject.DeconsHistoryInfo.Where(h => h.IsTo);
				foreach (var fromToGoods in docDataObject.GoodsPairs)
				{
					if (!message.IsEmpty)
					{
						break;
					}
					var fromGoods = fromToGoods.Item1;
					var toGoods = fromToGoods.Item2;

					var originalFromDocPackingLineInfo = docDataObject.OriginalPackagesInfo.SingleOrDefault(h => h.IsFromRCN && h.JobID == fromGoods.SourceID);
					var fromGoodsDeconsHistory = fromDocPackingLinesHistory.Where(h => h.JobID == fromGoods.SourceID) ?? new List<NotificationHistoryInfo>();
					var fromGoodsInHistory = docDataObject.InHistoryInfo.ContainsKey(fromGoods.SourceID) ? docDataObject.InHistoryInfo[fromGoods.SourceID] : new List<NotificationHistoryInfo>();
					var fromGoodsCorHistory = docDataObject.CorHistoryInfo.ContainsKey(fromGoods.SourceID) ? docDataObject.CorHistoryInfo[fromGoods.SourceID] : new List<NotificationHistoryInfo>();
					var fromGoodsTotalQuantity = fromGoodsInHistory.Sum(q => q.Quantity) + fromGoodsCorHistory.Sum(q => q.Quantity);
					var fromGoodsTotalWeight = fromGoodsInHistory.Sum(q => q.Weight) + fromGoodsCorHistory.Sum(q => q.Weight);
					if (fromGoods.AmountQuantity > originalFromDocPackingLineInfo.Quantity)
					{
						message = Res.GetString("768cbbe4-8c70-489e-9883-d9852b86c54c", "Cannot Send CIN 750 Deconsolidation Notification because from goods amount ({0}) is greater than original from goods amount ({1}).", fromGoods.AmountQuantity, originalFromDocPackingLineInfo.Quantity);
					}
					else if (fromGoods.AmountWeight > originalFromDocPackingLineInfo.Weight)
					{
						message = Res.GetString("a0bcbad0-5d50-4846-aab7-674d85c205c2", "Cannot Send CIN 750 Deconsolidation Notification because from goods weight ({0:F3}) is greater than original from goods weight ({1:F3}).", fromGoods.AmountWeight, originalFromDocPackingLineInfo.Weight);
					}
					else if (fromGoods.AmountQuantity > toGoods.AmountQuantity)
					{
						message = Res.GetString("3f1735c7-67de-453e-ad42-0c627d090014", "Cannot Send CIN 750 Deconsolidation Notification because from goods amount ({0}) is greater than to goods amount ({1}).", fromGoods.AmountQuantity, toGoods.AmountQuantity);
					}
					else if (fromGoodsTotalQuantity - fromGoodsDeconsHistory.Sum(h => h.Quantity) < fromGoods.AmountQuantity)
					{
						message = Res.GetString("75d4edba-98bc-47c6-a639-e04c02a9aa1b", "Cannot Send CIN 750 Deconsolidation Notification because the reported Package Quantity ({0}) is greater than reported In Packages that haven't been reported ({1}).", fromGoods.AmountQuantity, fromGoodsTotalQuantity - fromGoodsDeconsHistory.Sum(h => h.Quantity));
					}
					else if (fromGoodsTotalWeight - fromGoodsDeconsHistory.Sum(h => h.Weight) < fromGoods.AmountWeight)
					{
						message = Res.GetString("be7970b9-bf35-4dd4-939a-d4a43ccae6d3", "Cannot Send CIN 750 Deconsolidation Notification because the reported Package Weight ({0:F3}) is greater than reported In Packages that haven't been reported ({1:F3}).", fromGoods.AmountWeight, fromGoodsTotalWeight - fromGoodsDeconsHistory.Sum(h => h.Weight));
					}
					else
					{
						var receivedPackage = docDataObject.OriginalPackagesInfo.SingleOrDefault(h => h.IsTo && h.JobID == fromGoods.SourceID);
						var toGoodsDeconsHistory = toDocPackingLineHistory.SingleOrDefault(h => h.JobID == fromGoods.SourceID);
						if (receivedPackage != null)
						{
							message = ValidateQuantityAndWeight(TransitDocDataConstants.NotificationTypes.CIN750DeconsNotification, toGoods, receivedPackage.Quantity, receivedPackage.Weight, toGoodsDeconsHistory?.Quantity ?? 0, toGoodsDeconsHistory?.Weight ?? 0);
						}
					}
				}
			}

			return message;
		}

		public static ZString GetConsNotificationValidationMessage(CIN750ConsNotification docDataObject)
		{
			var message = ZString.Empty;
			var dcn = docDataObject.SourceBusinessObject as WhsItemDispatchConsignment;
			var dcnRefType = docDataObject.RefType.Code;
			var fromGoods = docDataObject.FromGoods;
			var toGoods = docDataObject.ToGoods;
			var inHistory = docDataObject.InHistoryInfo;
			var corHistory = docDataObject.CorHistoryInfo;
			var hasDecons = (docDataObject.DeconsHistoryInfo?.Count ?? 0) > 0;
			var deconsHistoryInfo = hasDecons ? GetHistoryTotalQuantityAndTotalWeight(docDataObject.DeconsHistoryInfo.Where(h => h.IsTo)) : (0, 0);
			if (docDataObject.FromGoods.Count == 0)
			{
				message = Res.GetString("db5664be-3013-444d-8f11-c34a498ece3c", "Cannot Send CIN 750 Consolidation Notification without Goods Detail.");
			}
			else if (docDataObject.FromGoods.Sum(f => f.AmountQuantity) < toGoods.AmountQuantity)
			{
				message = Res.GetString("2fc80d6c-5139-4d41-b8b1-d1cfa4d2b386", "Cannot Send CIN 750 Consolidation Notification because From Goods quantity cannot less than To Goods quantity.");
			}
			else if (docDataObject.FromGoods.Sum(f => f.AmountWeight) < toGoods.AmountWeight)
			{
				message = Res.GetString("1d2a216b-24af-42fe-a092-33051df0cea4", "Cannot Send CIN 750 Consolidation Notification because From Goods weight cannot less than To Goods weight.");
			}
			else if (dcnRefType != toGoods.RefType.Code)
			{
				message = Res.GetString("2a78985e-74b8-4738-a34f-7b9b963eac0f", "Cannot Send CIN 750 Consolidation Notification as reported Ref Type should as same as To Goods Ref Type.");
			}
			else if (hasDecons)
			{
				if (fromGoods.Sum(good => good.AmountQuantity) + docDataObject.ConsHistoryInfo.Where(good => !good.IsTo).Sum(good => good.Quantity) > deconsHistoryInfo.TotalQuantity || fromGoods.Sum(good => good.AmountWeight) + docDataObject.ConsHistoryInfo.Where(good => !good.IsTo).Sum(good => good.Weight) > deconsHistoryInfo.TotalWeight)
				{
					message = Res.GetString("5c074121-1cd0-45e1-9bc9-552ba8e64421", "Cannot Send CIN 750 Consolidation Notification as From Goods quantity or weight cannot be greater than Deconsolidation.");
				}
			}
			else
			{
				foreach (var fromGood in fromGoods)
				{
					var fromGoodInHistory = inHistory.ContainsKey(fromGood.SourceID) ? docDataObject.InHistoryInfo[fromGood.SourceID] : new List<NotificationHistoryInfo>();
					var fromGoodCorHistory = corHistory.ContainsKey(fromGood.SourceID) ? docDataObject.CorHistoryInfo[fromGood.SourceID] : new List<NotificationHistoryInfo>();
					if (!message.IsEmpty)
					{
						break;
					}
					var inHistoryInfo = GetHistoryTotalQuantityAndTotalWeight(fromGoodInHistory);
					var corHistoryInfo = GetHistoryTotalQuantityAndTotalWeight(fromGoodCorHistory);
					var awbCons = GetHistoryTotalQuantityAndTotalWeight(docDataObject.ConsHistoryInfo.Where(h => h.IsFromDCN && h.JobID == fromGood.SourceID));
					var hwbCons = GetHistoryTotalQuantityAndTotalWeight(docDataObject.ConsHistoryInfo.Where(h => h.IsFromRCN));
					var fromGoodsOriginalQty = docDataObject.OriginalPackagesInfo.Where(o => o.JobID == fromGood.SourceID).Sum(o => o.Quantity);
					var fromGoodsOriginalWeight = docDataObject.OriginalPackagesInfo.Where(o => o.JobID == fromGood.SourceID).Sum(o => o.Weight);
					var fromGoodsRefType = fromGood.RefType?.Code ?? ZString.Empty;

					if (dcnRefType == CIN750RefTypes.Codes.MasterAirWaybill)
					{
						if (dcn.HouseBillNumber == (fromGoodInHistory.FirstOrDefault(i => i.RefType == CIN750RefTypes.Codes.HouseAirWaybill)?.RefCode ?? string.Empty))
						{
							if (fromGood.AmountQuantity + awbCons.TotalQuantity > fromGoodsOriginalQty || fromGood.AmountWeight + awbCons.TotalWeight > fromGoodsOriginalWeight)
							{
								message = Res.GetString("0ecb93f6-adea-4acd-a5bd-facce162f5d1", "Master Bill quantity or weight cannot be greater than DCN quantity or weight.");
							}
							else if (fromGood.AmountQuantity + awbCons.TotalQuantity > inHistoryInfo.TotalQuantity + corHistoryInfo.TotalQuantity || fromGood.AmountWeight + awbCons.TotalWeight > inHistoryInfo.TotalWeight + corHistoryInfo.TotalWeight)
							{
								message = Res.GetString("81b89cd8-ab2a-4c27-98b3-c36b378d1ed7", "Master Bill quantity or weight cannot be greater than reported quantity or weight.");
							}
						}
						else if (hwbCons.TotalQuantity == 0 && !hasDecons)
						{
							message = Res.GetString("872f2464-f197-485a-b665-b4e6e602f557", "Unable to send CIN 750 Consolidation Notification for Master Bill as House Bill has not been consolidated.");
						}
						else if (fromGood.AmountQuantity + awbCons.TotalQuantity > hwbCons.TotalQuantity || fromGood.AmountWeight + awbCons.TotalWeight > hwbCons.TotalWeight)
						{
							message = Res.GetString("54ec9f50-a032-472b-8c1d-4d92a4755491", "Master Bill quantity or weight cannot be greater than House Bill quantity or weight.");
						}
					}
					else if (fromGood.AmountQuantity > fromGoodsOriginalQty || fromGood.AmountWeight > fromGoodsOriginalWeight)
					{
						message = Res.GetString("eca3a404-686f-432e-adfb-1290e2af1daa", "House Bill or Reference quantity or weight cannot be greater than DCN quantity or weight.");
					}
					else if (fromGood.AmountQuantity > inHistoryInfo.TotalQuantity + corHistoryInfo.TotalQuantity || fromGood.AmountWeight > inHistoryInfo.TotalWeight + corHistoryInfo.TotalWeight)
					{
						message = Res.GetString("2889e5f4-e3ee-4f22-ae96-fddfb12f8771", "House Bill or Reference quantity or weight cannot be greater than reported quantity or weight.");
					}

					if (!message.IsEmpty)
					{
						break;
					}

					if (dcn.MasterBillNumber.IsEmpty)
					{
						if (dcnRefType == CIN750RefTypes.Codes.Reference && fromGoodsRefType == CIN750RefTypes.Codes.Reference && !docDataObject.HasOvps)
						{
							message = Res.GetString("e9e16330-b222-4427-926b-fa4b25f9ed24", "Unable to Send CIN 750 Consolidation Notification for Reference as DCN has no OVPs.");
						}
						else if (dcnRefType != CIN750RefTypes.Codes.Reference && (fromGoodsRefType != fromGoodInHistory.FirstOrDefault().RefType || dcnRefType != CIN750RefTypes.Codes.HouseAirWaybill))
						{
							message = Res.GetString("66300d1c-3d6e-45b4-979a-6605a94cc9de", "Unable to Send CIN 750 Consolidation Notification as reported Ref Type is not HWB.");
						}
					}
					else
					{
						if (awbCons.TotalQuantity == 0 && awbCons.TotalWeight == 0)
						{
							if (dcn.HouseBillNumber == (fromGoodInHistory.FirstOrDefault(i => i.RefType == CIN750RefTypes.Codes.HouseAirWaybill)?.RefCode ?? string.Empty))
							{
								if (fromGoodsRefType != CIN750RefTypes.Codes.HouseAirWaybill || dcnRefType != CIN750RefTypes.Codes.MasterAirWaybill)
								{
									message = Res.GetString("9800641b-b8f9-41bc-a257-7e9b9199472f", "Unable to send CIN 750 Consolidation Notification as reported Ref Type should be Master Bill.");
								}
							}
							else if (hwbCons.TotalQuantity < fromGoodsOriginalQty || hwbCons.TotalWeight < fromGoodsOriginalWeight)
							{
								if (fromGoodsRefType != CIN750RefTypes.Codes.Reference && fromGoodsRefType != CIN750RefTypes.Codes.HouseAirWaybill || dcnRefType != CIN750RefTypes.Codes.HouseAirWaybill)
								{
									message = Res.GetString("872f2464-f197-485a-b665-b4e6e602f557", "Unable to send CIN 750 Consolidation Notification for Master Bill as House Bill has not been consolidated.");
								}
							}
							else if (fromGoodsRefType != CIN750RefTypes.Codes.HouseAirWaybill || dcnRefType != CIN750RefTypes.Codes.MasterAirWaybill)
							{
								message = Res.GetString("3ab08cbb-b3d7-407c-8832-4f20f15c108b", "CIN 750 Consolidation Notification Ref Type should be Master Bill as House Bill has been consolidated.");
							}
						}
						else if (awbCons.TotalQuantity > 0 && awbCons.TotalWeight > 0 && fromGoodsRefType != CIN750RefTypes.Codes.HouseAirWaybill || dcnRefType != CIN750RefTypes.Codes.MasterAirWaybill)
						{
							message = Res.GetString("3ab08cbb-b3d7-407c-8832-4f20f15c108b", "CIN 750 Consolidation Notification Ref Type should be Master Bill as House Bill has been consolidated.");
						}
					}
				}
			}
			return message;
		}

		static (int TotalQuantity, decimal TotalWeight) GetHistoryTotalQuantityAndTotalWeight(IEnumerable<NotificationHistoryInfo> historyInfo) => (historyInfo.Sum(c => c.Quantity), historyInfo.Sum(c => c.Weight));

		static ZString ValidateQuantityAndWeight(string messageType, DocPackingLine packingLine, ZInt quantityUpperBound, ZDecimal weightUpperBound, ZInt historyQuantity, ZDecimal historyWeight)
		{
			var message = ZString.Empty;

			if (historyQuantity == quantityUpperBound)
			{
				message = Res.GetString("bf300d7e-c534-490c-9d96-66d0a3c07a70", "Cannot Send CIN 750 {0} Notification because the reported Packages Quantity ({1}) is equal to the Quantity of Received Packages.", messageType, historyQuantity);
			}
			else if (historyQuantity > quantityUpperBound)
			{
				message = Res.GetString("104dc854-8106-4767-99ae-c739d2ccabe1", "Cannot Send CIN 750 {0} Notification because the reported Packages Quantity ({1}) is greater than the Quantity of Received Packages ({2}).", messageType, historyQuantity, quantityUpperBound);
			}
			else if (packingLine.AmountQuantity > quantityUpperBound - historyQuantity)
			{
				message = Res.GetString("aecc65ca-0c53-4ea4-8e6f-2b229c2fe1ab", "Cannot Send CIN 750 {0} Notification because Amount Quantity ({1}) is greater than the Quantity of Received Packages that haven't been reported ({2}).", messageType, packingLine.AmountQuantity, quantityUpperBound - historyQuantity);
			}
			else if (historyWeight == weightUpperBound)
			{
				message = Res.GetString("c6bdd502-00e9-4201-91e0-3c820006afbb", "Cannot Send CIN 750 {0} Notification because the reported Packages Weight ({1:F3}) is equal to the Weight of Received Packages.", messageType, historyWeight);
			}
			else if (historyWeight > weightUpperBound)
			{
				message = Res.GetString("82cd9491-4b5c-4510-8988-dfe3d3083ce1", "Cannot Send CIN 750 {0} Notification because the reported Packages Weight ({1:F3}) is greater than the Weight of Received Packages ({2:F3}).", messageType, historyWeight, weightUpperBound);
			}
			else if (packingLine.AmountWeight > weightUpperBound - historyWeight)
			{
				message = Res.GetString("846617ad-1f31-4eb7-adab-67c54befbaa8", "Cannot Send CIN 750 {0} Notification because Amount Weight ({1:F3}) is greater than the Weight of Received Packages that haven't been reported ({2:F3}).", messageType, packingLine.AmountWeight, weightUpperBound - historyWeight);
			}
			return message;
		}

		public static ZString GetCorNotificationValidationMessage(CIN750CorNotification docDataObject)
		{
			var corNotificationHistory = docDataObject.CorHistoryInfo;
			var inNotificationHistory = docDataObject.InHistoryInfo;
			var message = ZString.Empty;
			if (inNotificationHistory.Count == 0)
			{
				message = Res.GetString("f8b03a37-372a-4e6f-b4d7-4fcab7493b3e", "Cannot Send CIN 750 Cor Notification because it has not been sent In Notifications.");
			}
			else if (docDataObject.RefType.Code == CIN750RefTypes.Codes.MasterAirWaybill && corNotificationHistory.Any(h => h.RefType != CIN750RefTypes.Codes.MasterAirWaybill))
			{
				message = Res.GetString("cdbef6a3-d18a-4cde-b48d-4fe855fa8138", "Cannot Send CIN 750 Cor Notification with Reference Type Master Air Waybill because it does not match the history Notifications.");
			}
			else if (docDataObject.RefType.Code == CIN750RefTypes.Codes.Reference && corNotificationHistory.Any(h => h.RefType != CIN750RefTypes.Codes.Reference))
			{
				message = Res.GetString("6b0a0411-0525-413e-beed-8ab42eae79f4", "Cannot Send CIN 750 Cor Notification with Reference Type Reference because it does not match the history Notifications.");
			}
			else if (corNotificationHistory.Any(h => (h.RefType == CIN750RefTypes.Codes.Reference && h.Reference != docDataObject.RefCode) ||
													 (h.RefType == CIN750RefTypes.Codes.MasterAirWaybill && h.MasterBillWithoutHyphen != docDataObject.RefCode) ||
													 (h.RefType == CIN750RefTypes.Codes.HouseAirWaybill && h.HouseBill != docDataObject.RefCode)))
			{
				message = Res.GetString("33b3b577-ea2c-40a2-9539-95bc5f436f23", "Cannot Send CIN 750 Cor Notification with Reference Code {0} because it does not match the history Notifications.", docDataObject.RefCode);
			}
			else if (docDataObject.Goods.Count == 0)
			{
				message = Res.GetString("05be015d-d44e-47ff-87ca-cc293e059d78", "Cannot Send CIN 750 Cor Notification without Goods Detail.");
			}
			else if (docDataObject.Goods.Count == 1)
			{
				var packingLine = docDataObject.Goods.Single();
				var inNotificationHistoryQuantity = docDataObject.InHistoryInfo.Sum(h => h.Quantity);
				var inNotificationHistoryWeight = docDataObject.InHistoryInfo.Sum(h => h.Weight);
				var corNotificationHistoryQuantity = docDataObject.CorHistoryInfo.Sum(h => h.Quantity);
				var corNotificationHistoryWeight = docDataObject.CorHistoryInfo.Sum(h => h.Weight);
				var corNotificationQuantity = corNotificationHistoryQuantity + packingLine.AmountQuantity;
				var corNotificationWeight = corNotificationHistoryWeight + packingLine.AmountWeight;
				var quantityErrorMessage = Res.GetString("605a980a-d62c-4975-be7c-c8d6c26d5f29", "Cannot Send CIN 750 Cor Notification because the reported Packages Quantity adjustment ({0}) exceeds the Quantity of In Notification ({1}).");
				var weightErrorMessage = Res.GetString("3f453a50-576c-490e-b97a-20cdb21199e0", "Cannot Send CIN 750 Cor Notification because the reported Packages Weight adjustment ({0:F3}) exceeds the Weight of In Notification ({1:F3}).");

				if (packingLine.AmountQuantity == 0 && packingLine.AmountWeight == 0)
				{
					message = Res.GetString("19f53d92-e878-4652-8c12-f75279a365ef", "Cannot Send CIN 750 Cor Notification because the reported Packages Quantity and Weight are 0.");
				}
				else if (inNotificationHistoryQuantity + corNotificationQuantity < 0)
				{
					message = string.Format(quantityErrorMessage, corNotificationQuantity, inNotificationHistoryQuantity);
				}
				else if (inNotificationHistoryWeight + corNotificationWeight < 0)
				{
					message = string.Format(weightErrorMessage, corNotificationWeight, inNotificationHistoryWeight);
				}
			}
			return message;
		}

		public static ZString GetOutNotificationValidationMessage(CIN750OutNotification docDataObject)
		{
			var message = string.Empty;

			if (docDataObject.Goods == null || docDataObject.Goods.Count != 1)
			{
				message = Res.GetString("2a89082b-6e36-4dd4-905f-00680e70323c", "Cannot send CIN 750 Out Notification without Goods Detail or multiple Goods Detail.");
			}
			else
			{
			var goodsQty = docDataObject.Goods.First().AmountQuantity;
			var goodsWeight = docDataObject.Goods.First().AmountWeight;

				if (goodsQty > docDataObject.MaxQuantity && docDataObject.MaxQuantity != 0)
				{
					message = Res.GetString("763e208e-ec92-4126-8829-1c167f3403c7", "Cannot send CIN 750 Out Notification because Amount Quantity ({0}) is greater than the Quantity of Departed Packages that haven't been reported ({1}).", goodsQty, docDataObject.MaxQuantity);
				}
				else if (goodsWeight > docDataObject.MaxWeight && docDataObject.MaxWeight != 0)
				{
					message = Res.GetString("86d9051d-4bc7-4099-9082-c89f541735dc", "Cannot send CIN 750 Out Notification because Amount Weight ({0:F3}) is greater than the Weight of Departed Packages that haven't been reported ({1:F3}).", goodsWeight, docDataObject.MaxWeight);
				}

				if (docDataObject.CustomsDocuments?.Count == 0 ||
					(docDataObject.CustomsDocuments.Count == 1 && docDataObject.CustomsDocuments.First().RefType == ZString.Empty))
				{
					message = WithoutCustomsDocumentsErrorMessage;
				}
			}
			return message;
		}

		public static Either<string, object> GetDCNNotificationAdditionalData(WhsItemDispatchConsignment dcn, CIN750NotificationMessageTypes messageType)
		{
			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var errorMessage = historyManager.GetCheckSendingNotificationMessage(messageType);

			return string.IsNullOrEmpty(errorMessage) ? historyManager.AdditionalDataForOut : errorMessage;
		}

		public static Either<string, object> GetNotificationAdditionalData(WhsItemReceiveConsignment rcn, CIN750NotificationMessageTypes messageType)
		{
			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var errorMessage = historyManager.GetCheckSendingNotificationMessage(messageType);

			return string.IsNullOrEmpty(errorMessage) ? historyManager.AdditionalDataForRCN : errorMessage;
		}

		public static string WithoutCustomsDocumentsErrorMessage => Res.GetString("a3151c37-02ce-47b5-a952-6531a61b4677", "Customs Documents is required.");
	}
}
