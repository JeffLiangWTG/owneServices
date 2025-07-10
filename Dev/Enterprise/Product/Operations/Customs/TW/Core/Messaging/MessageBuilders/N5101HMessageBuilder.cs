using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5101H;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5101HMessageBuilder : BaseTWMessageBuilder<IN5101HDeclaration, Declaration>
	{
		public ZString PopulateXml(IN5101HDeclaration declaration)
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5101HDeclaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = obj.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode { Value = obj.FunctionCode };
				PopulateValueIfNodeValueIsNotEmpty(obj.StatusCode, () => { newItem.StatusCode = new DeclarationStatusCode { Value = obj.StatusCode }; });
				PopulateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateCarrier(newItem, obj.CarrierId);
				PopulateConsignments(newItem, obj.Consignments);
				PopulateConsignmentDeconsolidator(newItem, obj.DeconsolidatorId);
				PopulateConsignmentGoodsShipment(newItem, obj.GoodsShipment);
				PopulateConsignmentUnloadingLocation(newItem, obj.UnloadingLocationArrivalDateTime);
			}
			return newItem;
		}

		#region Declaration/BorderTransportMeans
		void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			var result = new DeclarationBorderTransportMeans();
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { result.Id = new DeclarationBorderTransportMeansId { Value = obj.ID }; });
				result.JourneyId = new DeclarationBorderTransportMeansJourneyId { Value = obj.JourneyID };
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => { result.TwRegistration = new DeclarationBorderTransportMeansTwRegistration { Value = obj.Registration }; });
				result.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
			}
			bo.BorderTransportMeans = result;
		}
		#endregion

		#region Declaration/Carrier
		void PopulateCarrier(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Carrier = new DeclarationCarrier() { Id = new DeclarationCarrierId() { Value = obj } };
			}
		}
		#endregion

		#region Declaration/Consignment
		void PopulateConsignments(Declaration bo, IEnumerable<IN5101HConsignment> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignment>();
				foreach (var item in obj)
				{
					result.Add(PopulateConsignment(item));
					if (result.Count > 9998)
					{ break; }
				}
				bo.Consignment = result;
			}
		}

		DeclarationConsignment PopulateConsignment(IN5101HConsignment obj)
		{
			var result = new DeclarationConsignment();
			PopulateValueIfNodeValueIsNotEmpty(obj.BoardedQuantity, () => { result.BoardedQuantity = new DeclarationConsignmentBoardedQuantity { Value = obj.BoardedQuantity }; });
			result.TotalPackageQuantity = new DeclarationConsignmentTotalPackageQuantity { Value = obj.TotalPackageQuantity };
			PopulateValueIfNodeValueIsNotEmpty(obj.EscortMark, () => { result.TwEscortMark = new DeclarationConsignmentTwEscortMark { Value = obj.EscortMark }; });
			result.TwTotalGrossMassMeasure = new DeclarationConsignmentTwTotalGrossMassMeasure { Value = ZArchitecture.Core.Utilities.Round(obj.TotalGrossMassMeasure, 6) };
			result.TwTypeCode = new DeclarationConsignmentTwTypeCode { Value = obj.TypeCode };
			PopulateConsignmentAssociatedTransportDocument(result, obj.AssociatedTransportDocumentId);
			PopulateConsignmentConsignee(result, obj.Consignee);
			PopulateConsignmentConsignmentItem(result, obj.ConsignmentItem);
			PopulateConsignmentConsignor(result, obj.Consignor);
			PopulateConsignmentGoodsLocation(result, obj.GoodsLocationId);
			PopulateConsignmentLoadingLocation(result, obj.LoadingLocation);
			PopulateConsignmentNotifyPartys(result, obj.NotifyParties);
			PopulateConsignmentTransportContractDocument(result, obj.TransportContractDocument);
			PopulateConsignmentTransportEquipments(result, obj.TransportEquipments);
			PopulateConsignmentUnloadingLocation(result, obj.UnloadingLocationId);
			return result;
		}

		#region Declaration/Consignment/AssociatedTransportDocument
		void PopulateConsignmentAssociatedTransportDocument(DeclarationConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.AssociatedTransportDocument = new DeclarationConsignmentAssociatedTransportDocument() { Id = new DeclarationConsignmentAssociatedTransportDocumentId() { Value = obj } };
			}
		}
		#endregion

		#region Declaration/Consignment/Consignee
		void PopulateConsignmentConsignee(DeclarationConsignment bo, IPartyDetails obj)
		{
			var result = new DeclarationConsignmentConsignee();
			var objIsNotNull = obj != null;
			if (objIsNotNull)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { result.Id = new DeclarationConsignmentConsigneeId { Value = obj.ID }; });
			}
			result.Name = new DeclarationConsignmentConsigneeName { Value = objIsNotNull ? obj.Name : ZString.Empty };
			if (objIsNotNull)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => { result.TwChineseName = new DeclarationConsignmentConsigneeTwChineseName { Value = obj.ChineseName }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => { result.TwTypeCode = new DeclarationConsignmentConsigneeTwTypeCode { Value = obj.TypeCode }; });
				PopulateConsignmentConsigneeAddress(result, obj.Address);
			}
			bo.Consignee = result;
		}

		void PopulateConsignmentConsigneeAddress(DeclarationConsignmentConsignee bo, IAddress obj)
		{
			if (IsNotNullNode(obj))
			{
				var result = new DeclarationConsignmentConsigneeAddress();
				PopulateValueIfNodeValueIsNotEmpty(obj.Line, () => { result.Line = new DeclarationConsignmentConsigneeAddressLine { Value = obj.Line }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => { result.TwChineseLine = new DeclarationConsignmentConsigneeAddressTwChineseLine { Value = obj.ChineseLine }; });
				bo.Address = result;
			}
		}
		#endregion

		#region Declaration/Consignment/ConsignmentItem 
		void PopulateConsignmentConsignmentItem(DeclarationConsignment bo, IN5101HConsignmentItem obj)
		{
			var result = new DeclarationConsignmentConsignmentItem();
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Split, () => { result.TwSplit = new DeclarationConsignmentConsignmentItemTwSplit { Value = obj.Split }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TotalPackageQuantity, () => { result.TwTotalPackageQuantity = new DeclarationConsignmentConsignmentItemTwTotalPackageQuantity { Value = obj.TotalPackageQuantity }; });
				PopulateConsignmentConsignmentItemAdditionalInformations(result, obj.AdditionalInformations);
				PopulateConsignmentConsignmentItemCommodity(result, obj.Commodity);
				PopulateConsignmentConsignmentItemGoodsMeasure(result, obj.GoodsMeasure);
				PopulateConsignmentConsignmentItemPackaging(result, obj.Packaging);
				PopulateConsignmentConsignmentItemUCR(result, obj.UCRId);
			}
			bo.ConsignmentItem = result;
		}

		#region Declaration/Consignment/ConsignmentItem/AdditionalInformation
		void PopulateConsignmentConsignmentItemAdditionalInformations(DeclarationConsignmentConsignmentItem bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignmentConsignmentItemAdditionalInformation>();
				foreach (var item in obj)
				{
					PopulateConsignmentConsignmentItemAdditionalInformation(result, item);
					if (result.Count > 4)
					{ break; }
				}
				if (result.Any())
				{
					bo.AdditionalInformation = result;
				}
			}
		}

		void PopulateConsignmentConsignmentItemAdditionalInformation(Collection<DeclarationConsignmentConsignmentItemAdditionalInformation> collection, IAdditionalInformation obj)
		{
			if (obj != null && (!obj.StatementCode.IsEmpty || !obj.StatementDescription.IsEmpty))
			{
				var result = new DeclarationConsignmentConsignmentItemAdditionalInformation();
				result.StatementCode = new DeclarationConsignmentConsignmentItemAdditionalInformationStatementCode() { Value = obj.StatementCode };
				result.StatementDescription = new DeclarationConsignmentConsignmentItemAdditionalInformationStatementDescription() { Value = obj.StatementDescription };
				collection.Add(result);
			}
		}
		#endregion

		#region Declaration/Consignment/ConsignmentItem/Commodity
		void PopulateConsignmentConsignmentItemCommodity(DeclarationConsignmentConsignmentItem bo, IN5101HCommodity obj)
		{
			var result = new DeclarationConsignmentConsignmentItemCommodity();
			result.CargoDescription = new DeclarationConsignmentConsignmentItemCommodityCargoDescription() { Value = obj.CargoDescription };
			PopulateConsignmentConsignmentItemCommodityClassifications(result, obj.Classifications);
			bo.Commodity = result;
		}

		#region Declaration/Consignment/ConsignmentItem/Commodity/Classification
		void PopulateConsignmentConsignmentItemCommodityClassifications(DeclarationConsignmentConsignmentItemCommodity bo, IEnumerable<IClassification> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignmentConsignmentItemCommodityClassification>();
				foreach (var item in obj)
				{
					PopulateConsignmentConsignmentItemCommodityClassification(result, item);
					if (result.Count > 1)
					{ break; }
				}
				if (result.Any())
				{
					bo.Classification = result;
				}
			}
		}

		void PopulateConsignmentConsignmentItemCommodityClassification(Collection<DeclarationConsignmentConsignmentItemCommodityClassification> collection, IClassification obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.IdentificationTypeCode.IsEmpty))
			{
				var result = new DeclarationConsignmentConsignmentItemCommodityClassification();
				result.Id = new DeclarationConsignmentConsignmentItemCommodityClassificationId() { Value = obj.ID };
				result.IdentificationTypeCode = new DeclarationConsignmentConsignmentItemCommodityClassificationIdentificationTypeCode() { Value = obj.IdentificationTypeCode };
				collection.Add(result);
			}
		}
		#endregion
		#endregion

		#region Declaration/Consignment/ConsignmentItem/GoodsMeasure
		void PopulateConsignmentConsignmentItemGoodsMeasure(DeclarationConsignmentConsignmentItem bo, IN5101HGoodsMeasure obj)
		{
			if (obj != null && (!obj.GrossVolumeMeasure.IsEmpty || !obj.VolumeUnitCode.IsEmpty))
			{
				var result = new DeclarationConsignmentConsignmentItemGoodsMeasure();
				result.TwGrossVolumeMeasure = new DeclarationConsignmentConsignmentItemGoodsMeasureTwGrossVolumeMeasure() { Value = ZArchitecture.Core.Utilities.Round(obj.GrossVolumeMeasure, 3) };
				result.TwVolumeUnitCode = new DeclarationConsignmentConsignmentItemGoodsMeasureTwVolumeUnitCode() { Value = obj.VolumeUnitCode };
				bo.GoodsMeasure = result;
			}
		}
		#endregion

		#region Declaration/Consignment/ConsignmentItem/Packaging
		void PopulateConsignmentConsignmentItemPackaging(DeclarationConsignmentConsignmentItem bo, IPackaging obj)
		{
			var result = new DeclarationConsignmentConsignmentItemPackaging();
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.MarksNumbers, () => { result.MarksNumbers = new DeclarationConsignmentConsignmentItemPackagingMarksNumbers { Value = obj.MarksNumbers }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.PackagingMaterialDescription, () => { result.PackagingMaterialDescription = new DeclarationConsignmentConsignmentItemPackagingPackagingMaterialDescription { Value = obj.PackagingMaterialDescription }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Combination, () => { result.TwCombination = new DeclarationConsignmentConsignmentItemPackagingTwCombination { Value = obj.Combination }; });
			}
			result.TypeCode = new DeclarationConsignmentConsignmentItemPackagingTypeCode() { Value = obj?.TypeCode ?? ZString.Empty };
			bo.Packaging = result;
		}
		#endregion

		#region Declaration/Consignment/ConsignmentItem/UCR
		void PopulateConsignmentConsignmentItemUCR(DeclarationConsignmentConsignmentItem bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Ucr = new DeclarationConsignmentConsignmentItemUcr() { Id = new DeclarationConsignmentConsignmentItemUcrId() { Value = obj } };
			}
		}
		#endregion
		#endregion

		#region Declaration/Consignment/Consignor
		void PopulateConsignmentConsignor(DeclarationConsignment bo, IPartyDetails obj)
		{
			var address = obj?.Address;
			if (IsNotNullNode(obj) || IsNotNullNode(address))
			{
				var result = new DeclarationConsignmentConsignor();
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { result.Id = new DeclarationConsignmentConsignorId { Value = obj.ID }; });
				result.Name = new DeclarationConsignmentConsignorName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => { result.TwChineseName = new DeclarationConsignmentConsignorTwChineseName { Value = obj.ChineseName }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => { result.TwTypeCode = new DeclarationConsignmentConsignorTwTypeCode { Value = obj.TypeCode }; });
				PopulateConsignmentConsignorAddress(result, address);
				bo.Consignor = result;
			}
		}

		void PopulateConsignmentConsignorAddress(DeclarationConsignmentConsignor bo, IAddress obj)
		{
			if (IsNotNullNode(obj))
			{
				var result = new DeclarationConsignmentConsignorAddress();
				result.Line = new DeclarationConsignmentConsignorAddressLine { Value = obj.Line };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => { result.TwChineseLine = new DeclarationConsignmentConsignorAddressTwChineseLine { Value = obj.ChineseLine }; });
				bo.Address = result;
			}
		}
		#endregion

		#region Declaration/Consignment/GoodsLocation
		void PopulateConsignmentGoodsLocation(DeclarationConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.GoodsLocation = new DeclarationConsignmentGoodsLocation() { Id = new DeclarationConsignmentGoodsLocationId() { Value = obj } };
			}
		}
		#endregion

		#region Declaration/Consignment/LoadingLocation
		void PopulateConsignmentLoadingLocation(DeclarationConsignment bo, ILocation obj)
		{
			var result = new DeclarationConsignmentLoadingLocation();
			result.Id = new DeclarationConsignmentLoadingLocationId() { Value = obj?.ID ?? ZString.Empty };
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => { result.Name = new DeclarationConsignmentLoadingLocationName() { Value = obj.Name }; });
			}
			bo.LoadingLocation = result;
		}
		#endregion

		#region Declaration/Consignment/NotifyParty
		void PopulateConsignmentNotifyPartys(DeclarationConsignment bo, IEnumerable<IPartyDetails> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignmentNotifyParty>();
				foreach (var item in obj)
				{
					PopulateConsignmentNotifyParty(result, item);
					if (result.Count > 2)
					{ break; }
				}
				if (result.Any())
				{
					bo.NotifyParty = result;
				}
			}
		}

		void PopulateConsignmentNotifyParty(Collection<DeclarationConsignmentNotifyParty> list, IPartyDetails obj)
		{
			var address = obj.Address;
			if (IsNotNullNode(obj) || IsNotNullNode(address))
			{
				var result = new DeclarationConsignmentNotifyParty();
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { result.Id = new DeclarationConsignmentNotifyPartyId { Value = obj.ID }; });
				result.Name = new DeclarationConsignmentNotifyPartyName { Value = obj.Name };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseName, () => { result.TwChineseName = new DeclarationConsignmentNotifyPartyTwChineseName { Value = obj.ChineseName }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.TypeCode, () => { result.TwTypeCode = new DeclarationConsignmentNotifyPartyTwTypeCode { Value = obj.TypeCode }; });
				PopulateConsignmentNotifyPartyAddress(result, address);
				list.Add(result);
			}
		}

		void PopulateConsignmentNotifyPartyAddress(DeclarationConsignmentNotifyParty bo, IAddress obj)
		{
			if (IsNotNullNode(obj))
			{
				var result = new DeclarationConsignmentNotifyPartyAddress();
				result.Line = new DeclarationConsignmentNotifyPartyAddressLine { Value = obj.Line };
				PopulateValueIfNodeValueIsNotEmpty(obj.ChineseLine, () => { result.TwChineseLine = new DeclarationConsignmentNotifyPartyAddressTwChineseLine { Value = obj.ChineseLine }; });
				bo.Address = result;
			}
		}
		#endregion

		#region Declaration/Consignment/TransportContractDocument
		void PopulateConsignmentTransportContractDocument(DeclarationConsignment bo, ITransportContractDocument obj)
		{
			bo.TransportContractDocument = new DeclarationConsignmentTransportContractDocument() { Id = new DeclarationConsignmentTransportContractDocumentId() { Value = obj?.ID ?? ZString.Empty } };
		}
		#endregion

		#region Declaration/Consignment/TransportEquipment
		void PopulateConsignmentTransportEquipments(DeclarationConsignment bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignmentTransportEquipment>();
				foreach (var item in obj)
				{
					PopulateConsignmentTransportEquipment(result, item);
					if (result.Count > 9998)
					{ break; }
				}
				if (result.Any())
				{
					bo.TransportEquipment = result;
				}
			}
		}

		void PopulateConsignmentTransportEquipment(Collection<DeclarationConsignmentTransportEquipment> collection, ITransportEquipment obj)
		{
			if (obj != null && (!obj.CharacteristicCode.IsEmpty || !obj.ID.IsEmpty || !obj.UsedCapacityCode.IsEmpty))
			{
				var result = new DeclarationConsignmentTransportEquipment();
				result.CharacteristicCode = new DeclarationConsignmentTransportEquipmentCharacteristicCode { Value = obj.CharacteristicCode };
				result.Id = new DeclarationConsignmentTransportEquipmentId { Value = obj.ID };
				result.TwUsedCapacityCode = new DeclarationConsignmentTransportEquipmentTwUsedCapacityCode { Value = obj.UsedCapacityCode };
				PopulateConsignmentTransportEquipmentTwSeal(result, obj.Seals);
				collection.Add(result);
			}
		}

		void PopulateConsignmentTransportEquipmentTwSeal(DeclarationConsignmentTransportEquipment bo, IEnumerable<ZString> obj)
		{
			if (obj != null && obj.Any())
			{
				var result = new Collection<DeclarationConsignmentTransportEquipmentTwSeal>();
				foreach (var item in obj)
				{
					if (!item.IsEmpty)
					{
						result.Add(new DeclarationConsignmentTransportEquipmentTwSeal() { TwSealId = new DeclarationConsignmentTransportEquipmentTwSealTwSealId() { Value = item } });
					}
					if (result.Count > 9)
					{ break; }
				}
				if (result.Any())
				{
					bo.TwSeal = result;
				}
			}
		}
		#endregion

		#region Declaration/Consignment/GoodsLocation
		void PopulateConsignmentUnloadingLocation(DeclarationConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.UnloadingLocation = new DeclarationConsignmentUnloadingLocation() { Id = new DeclarationConsignmentUnloadingLocationId() { Value = obj } };
			}
		}
		#endregion
		#endregion

		#region Declaration/Deconsolidator
		void PopulateConsignmentDeconsolidator(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.Deconsolidator = new DeclarationDeconsolidator() { Id = new DeclarationDeconsolidatorId() { Value = obj } };
			}
		}
		#endregion

		#region Declaration/GoodsShipment
		void PopulateConsignmentGoodsShipment(Declaration bo, IN5101HGoodsShipment obj)
		{
			if (obj != null)
			{
				var result = new DeclarationGoodsShipment();
				PopulateConsignmentGoodsShipmentConsignment(result, obj.Consignment);
				result.EntryOffice = new DeclarationGoodsShipmentEntryOffice() { Id = new DeclarationGoodsShipmentEntryOfficeId() { Value = obj.EntryOfficeId } };
				bo.GoodsShipment = result;
			}
		}

		#region Declaration/GoodsShipment/Consignment
		void PopulateConsignmentGoodsShipmentConsignment(DeclarationGoodsShipment bo, IN5101HConsignment obj)
		{
			var result = new DeclarationGoodsShipmentConsignment();
			result.GoodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation() { Id = new DeclarationGoodsShipmentConsignmentGoodsLocationId() { Value = obj?.GoodsLocationId ?? ZString.Empty } };
			result.TransportContractDocument = new DeclarationGoodsShipmentConsignmentTransportContractDocument() { Id = new DeclarationGoodsShipmentConsignmentTransportContractDocumentId() { Value = obj?.TransportContractDocument?.ID ?? ZString.Empty } };
			bo.Consignment = result;
		}
		#endregion
		#endregion

		#region Declaration/UnloadingLocation
		void PopulateConsignmentUnloadingLocation(Declaration bo, ZDateTime obj)
		{
			if (obj.IsValid)
			{
				bo.UnloadingLocation = new DeclarationUnloadingLocation() { ArrivalDateTime = obj.ToISO8601ShortDateString() };
			}
		}
		#endregion

		#region Common
		bool IsNotNullNode(IPartyDetails obj) => obj != null && (!obj.ID.IsEmpty || !obj.Name.IsEmpty || !obj.ChineseName.IsEmpty || !obj.TypeCode.IsEmpty);

		bool IsNotNullNode(IAddress obj) => obj != null && (!obj.Line.IsEmpty || !obj.ChineseLine.IsEmpty);
		#endregion
	}
}
