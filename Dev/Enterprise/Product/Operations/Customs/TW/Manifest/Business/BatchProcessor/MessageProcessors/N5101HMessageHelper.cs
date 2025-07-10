using System.Collections.ObjectModel;
using CargoWise.Customs.TW.MessageDefinitions.N5101H;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Manifest.MessageProcessors
{
	public class N5101HMessageHelper : TWManifestMessageHelper
	{
		public N5101HMessageHelper(AsycudaMessage message) : base(message)
		{
			Declaration = (Declaration)new TWManifestOutgoingMessageKeyInfomation(Message.EM_MessageType, Message.EM_MessageText).Result;
		}

		Declaration Declaration { get; }

		protected override void WriteTable(HtmlTableCreator table)
		{
			WriteTitle(table, Titles.HeaderTitle);
			WriteRow(table, Captions.FunctionalReferenceID, Declaration?.FunctionalReferenceId?.Value);
			WriteRow(table, Captions.FunctionCode, Declaration?.FunctionCode?.Value);
			WriteRow(table, Captions.StatusCode, Declaration?.StatusCode?.Value);
			WriteBorderTransportMeans(table, Declaration?.BorderTransportMeans);
			WriteRow(table, Captions.CarrierID, Declaration?.Carrier?.Id?.Value);
			WriteConsignment(table, Declaration?.Consignment);
			WriteRow(table, Captions.DeconsolidatorID, Declaration?.Deconsolidator?.Id?.Value);
			WriteGoodsShipment(table, Declaration?.GoodsShipment);
			WriteRow(table, Captions.UnloadingLocationArrivalDateTime, Declaration?.UnloadingLocation?.ArrivalDateTime);
		}

		void WriteBorderTransportMeans(HtmlTableCreator table, DeclarationBorderTransportMeans borderTransportMeans)
		{
			if (borderTransportMeans != null)
			{
				WriteRow(table, Captions.BorderTransportMeansID, borderTransportMeans.Id?.Value);
				WriteRow(table, Captions.BorderTransportMeansJourneyID, borderTransportMeans.JourneyId?.Value);
				WriteRow(table, Captions.BorderTransportMeansRegistration, borderTransportMeans.TwRegistration?.Value);
				WriteRow(table, Captions.BorderTransportMeansTypeCode, borderTransportMeans.TypeCode?.Value);
			}
		}

		#region Consignment

		void WriteConsignment(HtmlTableCreator table, Collection<DeclarationConsignment> consignments)
		{
			if (consignments != null)
			{
				var index = 0;
				foreach (var consignment in consignments)
				{
					WriteTitle(table, GetSubTtile(Titles.Consignment, ++index));
					WriteRow(table, Captions.ConsignmentBoardedQuantity, consignment.BoardedQuantity?.Value.ToString());
					WriteRow(table, Captions.ConsignmentTotalPackageQuantity, consignment.TotalPackageQuantity?.Value.ToString());
					WriteRow(table, Captions.ConsignmentEscortMark, consignment.TwEscortMark?.Value);
					WriteRow(table, Captions.ConsignmentTotalGrossMassMeasure, consignment.TwTotalGrossMassMeasure?.Value.ToString());
					WriteRow(table, Captions.ConsignmentTypeCode, consignment.TwTypeCode?.Value);
					WriteRow(table, Captions.ConsignmentAssociatedTransportDocumentID, consignment.AssociatedTransportDocument?.Id?.Value);
					WriteConsignee(table, consignment.Consignee);
					WriteConsignmentItem(table, consignment.ConsignmentItem);
					WriteConsignor(table, consignment.Consignor);
					WriteRow(table, Captions.ConsignmentGoodsLocationID, consignment.GoodsLocation?.Id?.Value);
					WriteConsignmentLoadingLocation(table, consignment.LoadingLocation);
					WriteConsignmentNotifyParty(table, consignment.NotifyParty);
					WriteRow(table, Captions.TransportContractDocumentID, consignment.TransportContractDocument?.Id?.Value);
					WriteTransportEquipment(table, consignment.TransportEquipment);
					WriteRow(table, Captions.UnloadingLocation, consignment.UnloadingLocation?.Id?.Value);
				}
			}
		}

		void WriteConsignee(HtmlTableCreator table, DeclarationConsignmentConsignee consignee)
		{
			WriteRow(table, Captions.ConsigneeID, consignee?.Id?.Value);
			WriteRow(table, Captions.ConsigneeName, consignee?.Name?.Value);
			WriteRow(table, Captions.ConsigneeChineseName, consignee?.TwChineseName?.Value);
			WriteRow(table, Captions.ConsigneeTypeCode, consignee?.TwTypeCode?.Value);
			var consigneeAddress = consignee?.Address;
			if (consigneeAddress != null)
			{
				WriteRow(table, Captions.ConsigneeAddressLine, consigneeAddress.Line?.Value);
				WriteRow(table, Captions.ConsigneeAddressChineseLine, consigneeAddress.TwChineseLine?.Value);
			}
		}

		#region ConsignmentItem

		void WriteConsignmentItem(HtmlTableCreator table, DeclarationConsignmentConsignmentItem consignmentItem)
		{
			WriteRow(table, Captions.ConsignmentItemSplit, consignmentItem?.TwSplit?.Value);
			WriteRow(table, Captions.ConsignmentItemTotalPackageQuantity, consignmentItem?.TwTotalPackageQuantity?.Value);
			WriteriteConsignmentItemAdditionalInformation(table, consignmentItem?.AdditionalInformation);
			WriteriteConsignmentItemCommodity(table, consignmentItem?.Commodity);
			WriteriteConsignmentItemGoodsMeasure(table, consignmentItem?.GoodsMeasure);
			WriteriteConsignmentItemPackaging(table, consignmentItem?.Packaging);
			WriteRow(table, Captions.UCRID, consignmentItem?.Ucr?.Id?.Value);
		}

		void WriteriteConsignmentItemAdditionalInformation(HtmlTableCreator table, Collection<DeclarationConsignmentConsignmentItemAdditionalInformation> additionalInformations)
		{
			if (additionalInformations != null)
			{
				var index = 0;
				foreach (var additionalInformation in additionalInformations)
				{
					WriteTitle(table, GetSubTtile(Titles.ConsignmentItemAdditionalInformation, ++index));
					WriteRow(table, Captions.ConsignmentItemAdditionalInformationStatementCode, additionalInformation.StatementCode?.Value);
					WriteRow(table, Captions.ConsignmentItemAdditionalInformationStatementDescription, additionalInformation.StatementDescription?.Value);
				}
			}
		}

		#region ConsignmentItemCommodity

		void WriteriteConsignmentItemCommodity(HtmlTableCreator table, DeclarationConsignmentConsignmentItemCommodity commodity)
		{
			WriteRow(table, Captions.ConsignmentItemCommodityCargoDescription, commodity?.CargoDescription?.Value);
			WriteriteConsignmentItemCommodityClassification(table, commodity?.Classification);
		}

		void WriteriteConsignmentItemCommodityClassification(HtmlTableCreator table, Collection<DeclarationConsignmentConsignmentItemCommodityClassification> classifications)
		{
			if (classifications != null)
			{
				var index = 0;
				foreach (var classification in classifications)
				{
					WriteTitle(table, GetSubTtile(Titles.ConsignmentItemCommodityClassification, ++index));
					WriteRow(table, Captions.ConsignmentItemCommodityClassificationID, classification.Id?.Value);
					WriteRow(table, Captions.ConsignmentItemCommodityIdentificationTypeCode, classification.IdentificationTypeCode?.Value);
				}
			}
		}

		#endregion

		void WriteriteConsignmentItemGoodsMeasure(HtmlTableCreator table, DeclarationConsignmentConsignmentItemGoodsMeasure goodsMeasure)
		{
			if (goodsMeasure != null)
			{
				WriteRow(table, Captions.ConsignmentItemGoodsMeasureGrossVolumeMeasure, goodsMeasure.TwGrossVolumeMeasure?.Value);
				WriteRow(table, Captions.ConsignmentItemGoodsMeasureVolumeUnitCode, goodsMeasure.TwVolumeUnitCode?.Value);
			}
		}

		void WriteriteConsignmentItemPackaging(HtmlTableCreator table, DeclarationConsignmentConsignmentItemPackaging packaging)
		{
			if (packaging != null)
			{
				WriteRow(table, Captions.ConsignmentItemPackagingMarksNumbers, packaging.MarksNumbers?.Value);
				WriteRow(table, Captions.ConsignmentItemPackagingPackagingMaterialDescription, packaging.PackagingMaterialDescription?.Value);
				WriteRow(table, Captions.ConsignmentItemPackagingCombination, packaging.TwCombination?.Value);
				WriteRow(table, Captions.ConsignmentItemPackagingTypeCode, packaging.TypeCode?.Value);
			}
		}

		#endregion

		void WriteConsignor(HtmlTableCreator table, DeclarationConsignmentConsignor consignor)
		{
			if (consignor != null)
			{
				WriteRow(table, Captions.ConsignorID, consignor.Id?.Value);
				WriteRow(table, Captions.ConsignorName, consignor.Name?.Value);
				WriteRow(table, Captions.ConsignorChineseName, consignor.TwChineseName?.Value);
				WriteRow(table, Captions.ConsignorTypeCode, consignor.TwTypeCode?.Value);
				var consignorAddress = consignor.Address;
				if (consignorAddress != null)
				{
					WriteRow(table, Captions.ConsignorAddressLine, consignorAddress.Line?.Value);
					WriteRow(table, Captions.ConsignorAddressChineseLine, consignorAddress.TwChineseLine?.Value);
				}
			}
		}

		void WriteConsignmentLoadingLocation(HtmlTableCreator table, DeclarationConsignmentLoadingLocation loadingLocation)
		{
			WriteRow(table, Captions.LoadingLocationID, loadingLocation?.Id?.Value);
			WriteRow(table, Captions.LoadingLocationName, loadingLocation?.Name?.Value);
		}

		void WriteConsignmentNotifyParty(HtmlTableCreator table, Collection<DeclarationConsignmentNotifyParty> notifyPartys)
		{
			if (notifyPartys != null)
			{
				var index = 0;
				foreach (var notifyParty in notifyPartys)
				{
					WriteTitle(table, GetSubTtile(Titles.NotifyParty, ++index));
					WriteRow(table, Captions.NotifyPartyID, notifyParty.Id?.Value);
					WriteRow(table, Captions.NotifyPartyName, notifyParty.Name?.Value);
					WriteRow(table, Captions.NotifyPartyChineseName, notifyParty.TwChineseName?.Value);
					WriteRow(table, Captions.NotifyPartyTypeCode, notifyParty.TwTypeCode?.Value);
					var notifyPartyAddress = notifyParty.Address;
					if (notifyPartyAddress != null)
					{
						WriteRow(table, Captions.NotifyPartyAddressLine, notifyPartyAddress.Line?.Value);
						WriteRow(table, Captions.NotifyPartyAddressChineseLine, notifyPartyAddress.TwChineseLine?.Value);
					}
				}
			}
		}

		#region TransportEquipment

		void WriteTransportEquipment(HtmlTableCreator table, Collection<DeclarationConsignmentTransportEquipment> transportEquipments)
		{
			if (transportEquipments != null)
			{
				int index = 0;
				foreach (var transportEquipment in transportEquipments)
				{
					WriteTitle(table, GetSubTtile(Titles.TransportEquipment, ++index));
					WriteRow(table, Captions.TransportEquipmentCharacteristicCode, transportEquipment.CharacteristicCode?.Value);
					WriteRow(table, Captions.TransportEquipmentID, transportEquipment.Id?.Value);
					WriteRow(table, Captions.TransportEquipmentUsedCapacityCode, transportEquipment.TwUsedCapacityCode?.Value);
					WriteTransportEquipmentSeal(table, transportEquipment.TwSeal);
				}
			}
		}

		void WriteTransportEquipmentSeal(HtmlTableCreator table, Collection<DeclarationConsignmentTransportEquipmentTwSeal> seals)
		{
			if (seals != null)
			{
				var index = 0;
				foreach (var seal in seals)
				{
					WriteTitle(table, GetSubTtile(Titles.TransportEquipmentSeal, ++index));
					WriteRow(table, Captions.TransportEquipmentSealID, seal.TwSealId?.Value);
				}
			}
		}

		#endregion

		#endregion

		#region GoodsShipment

		void WriteGoodsShipment(HtmlTableCreator table, DeclarationGoodsShipment goodsShipment)
		{
			var consignment = goodsShipment?.Consignment;
			WriteRow(table, Captions.ConsignmentGoodsLocationID, consignment?.GoodsLocation?.Id?.Value);
			WriteRow(table, Captions.GoodsShipmentConsignmentTransportContractDocumentID, consignment?.TransportContractDocument?.Id?.Value);
			WriteRow(table, Captions.EntryOfficeID, goodsShipment?.EntryOffice?.Id?.Value);
		}

		#endregion

		class Captions
		{
			#region SuppressResourceStringsCheckRegion

			internal const string HeaderTitle = "進（轉）口貨物分艙單(N5101H)";

			internal const string FunctionalReferenceID = "訊息編號";

			internal const string FunctionCode = "訊息功能代碼";

			internal const string StatusCode = "傳輸完成註記";

			internal const string BorderTransportMeansID = "船（機）代碼";

			internal const string BorderTransportMeansJourneyID = "船舶航次（海）/航機班次（空）";

			internal const string BorderTransportMeansRegistration = "海關通關號碼";

			internal const string BorderTransportMeansTypeCode = "海空運別";

			internal const string CarrierID = "運輸業者/代理行代碼";

			internal const string ConsignmentBoardedQuantity = "袋數";

			internal const string ConsignmentTotalPackageQuantity = "總件數";

			internal const string ConsignmentEscortMark = "押運註記";

			internal const string ConsignmentTotalGrossMassMeasure = "總毛重";

			internal const string ConsignmentTypeCode = "艙單類別";

			internal const string ConsignmentAssociatedTransportDocumentID = "袋號";

			internal const string ConsigneeID = "收貨人代碼";

			internal const string ConsigneeName = "收貨人英文名稱";

			internal const string ConsigneeChineseName = "收貨人中文名稱";

			internal const string ConsigneeTypeCode = "身分識別代碼";

			internal const string ConsigneeAddressLine = "收貨人英文地址";

			internal const string ConsigneeAddressChineseLine = "收貨人中文地址";

			internal const string ConsignmentItemSplit = "分批註記";

			internal const string ConsignmentItemTotalPackageQuantity = "本批件數";

			internal const string ConsignmentItemAdditionalInformationStatementCode = "備用欄位識別代碼";

			internal const string ConsignmentItemAdditionalInformationStatementDescription = "備用欄位";

			internal const string ConsignmentItemCommodityCargoDescription = "貨名";

			internal const string ConsignmentItemCommodityClassificationID = "危險貨物代碼";

			internal const string ConsignmentItemCommodityIdentificationTypeCode = "類別";

			internal const string ConsignmentItemGoodsMeasureGrossVolumeMeasure = "體積";

			internal const string ConsignmentItemGoodsMeasureVolumeUnitCode = "體積單位";

			internal const string ConsignmentItemPackagingMarksNumbers = "標記";

			internal const string ConsignmentItemPackagingPackagingMaterialDescription = "包裝說明";

			internal const string ConsignmentItemPackagingCombination = "合成註記";

			internal const string ConsignmentItemPackagingTypeCode = "件數單位";

			internal const string UCRID = "貨物唯一追蹤號碼";

			internal const string ConsignorID = "發貨人代碼";

			internal const string ConsignorName = "發貨人英文名稱";

			internal const string ConsignorChineseName = "發貨人中文名稱";

			internal const string ConsignorTypeCode = "身分識別代碼";

			internal const string ConsignorAddressLine = "發貨人英文地址";

			internal const string ConsignorAddressChineseLine = "發貨人中文地址";

			internal const string GoodsLocationID = "卸存地點代碼";

			internal const string LoadingLocationID = "裝貨港代碼";

			internal const string LoadingLocationName = "裝貨港";

			internal const string NotifyPartyID = "受通知人代碼";

			internal const string NotifyPartyName = "受通知人英文名稱";

			internal const string NotifyPartyChineseName = "受通知人中文名稱";

			internal const string NotifyPartyTypeCode = "身分識別代碼";

			internal const string NotifyPartyAddressLine = "受通知人英文地址";

			internal const string NotifyPartyAddressChineseLine = "受通知人中文地址";

			internal const string TransportContractDocumentID = "分提單號碼";

			internal const string TransportEquipmentCharacteristicCode = "實櫃種類";

			internal const string TransportEquipmentID = "實櫃號碼";

			internal const string TransportEquipmentUsedCapacityCode = "貨櫃裝運方式";

			internal const string TransportEquipmentSealID = "封條號碼";

			internal const string UnloadingLocation = "目的地代碼";

			internal const string DeconsolidatorID = "承攬運送業者代碼";

			internal const string ConsignmentGoodsLocationID = "卸存地點代碼";

			internal const string GoodsShipmentConsignmentTransportContractDocumentID = "主提單號碼";

			internal const string EntryOfficeID = "受理艙單關別代碼";

			internal const string UnloadingLocationArrivalDateTime = "（預定）到達日期";

			#endregion
		}

		class Titles
		{
			#region SuppressResourceStringsCheckRegion

			internal const string HeaderTitle = "進（轉）口貨物分艙單(N5101H)";

			internal const string Consignment = "貨物";

			internal const string ConsignmentItemAdditionalInformation = "其他資訊";

			internal const string ConsignmentItemCommodityClassification = "分類";

			internal const string NotifyParty = "受通知人";

			internal const string TransportEquipment = "貨櫃";

			internal const string TransportEquipmentSeal = "封條";

			#endregion
		}
	}
}
