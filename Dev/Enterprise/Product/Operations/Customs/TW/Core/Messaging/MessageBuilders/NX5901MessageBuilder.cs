using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX5901;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX5901MessageBuilder : BaseTWMessageBuilder<INX5901Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX5901Declaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = obj.FunctionalReferenceID };
				newItem.Id = new DeclarationId { Value = obj.ID };
				newItem.TypeCode = new DeclarationTypeCode { Value = obj.TypeCode };
				PopulateAdditionalDocument(newItem, obj.AdditionalDocument);
				PopulateContactOffice(newItem, obj.ContactOffice);
				PopulateGoodsShipment(newItem, obj.GoodsShipment);
				PopulateGovernmentProcedure(newItem, obj.GovernmentProcedure);
				PopulatePreviousDocument(newItem, obj.PreviousDocument);
				PopulateDeclarationResponsibleGovernmentAgency(newItem, obj.ResponsibleGovernmentAgency);
			}
			return newItem;
		}

		public void PopulateAdditionalDocument(Declaration bo, IAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationAdditionalDocument() { Id = new DeclarationAdditionalDocumentId { Value = obj.ID } };
			}
		}

		public void PopulateContactOffice(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ContactOffice = new DeclarationContactOffice() { Id = new DeclarationContactOfficeId { Value = obj } };
			}
		}

		public void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				PopulateGoodsShipmentAdditionalDocument(newItem, obj.AdditionalDocuments);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
			}
		}

		public void PopulateGoodsShipmentAdditionalDocument(DeclarationGoodsShipment bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentAdditionalDocument();
					newItem.Id = new DeclarationGoodsShipmentAdditionalDocumentId { Value = item.ID };
					newItem.TwContent = new DeclarationGoodsShipmentAdditionalDocumentTwContent { Value = item.Content };
					newItem.TwImageFileFormat = new DeclarationGoodsShipmentAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat };
					newItem.TwImageFileName = new DeclarationGoodsShipmentAdditionalDocumentTwImageFileName { Value = item.ImageFileName };
					newItem.TwSequenceNumeric = item.SequenceNumeric;
					newItem.TwSizeMeasure = new DeclarationGoodsShipmentAdditionalDocumentTwSizeMeasure { Value = item.SizeMeasure };
					newItem.TypeCode = new DeclarationGoodsShipmentAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.AdditionalDocument = collection;
			}
		}

		public void PopulateResponsibleGovernmentAgency(DeclarationGoodsShipmentAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationGoodsShipmentAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		public void PopulateGovernmentAgencyGoodsItem(DeclarationGoodsShipment bo, IEnumerable<IGovernmentAgencyGoodsItem> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
					newItem.SequenceNumeric = item.SequenceNumeric;
					PopulateGovernmentAgencyGoodsItemAdditionalDocument(newItem, item.AdditionalDocuments?.FirstOrDefault());
					PopulateCommodityAdditionalDocument(newItem, item.Commodity?.AdditionalDocuments);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		public void PopulateGovernmentAgencyGoodsItemAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IAdditionalDocument obj)
		{
			if (obj != null)
			{
				bo.AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument() { TwSequenceNumeric = obj.SequenceNumeric };
			}
		}

		public void PopulateCommodityAdditionalDocument(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<IAdditionalDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument();
					newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentId { Value = item.ID };
					newItem.TwContent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentTwContent { Value = item.Content };
					newItem.TwImageFileFormat = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentTwImageFileFormat { Value = item.ImageFileFormat };
					newItem.TwImageFileName = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentTwImageFileName { Value = item.ImageFileName };
					newItem.TwSizeMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentTwSizeMeasure { Value = item.SizeMeasure };
					newItem.TypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentTypeCode { Value = item.TypeCode };
					PopulateCommodityAdditionalDocumentResponsibleGovernmentAgency(newItem, item.ResponsibleGovernmentAgency);
					collection.Add(newItem);
				}
				bo.Commodity = collection;
			}
		}

		public void PopulateCommodityAdditionalDocumentResponsibleGovernmentAgency(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocument bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentResponsibleGovernmentAgency() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityAdditionalDocumentResponsibleGovernmentAgencyId { Value = obj } };
			}
		}

		public void PopulateGovernmentProcedure(Declaration bo, IGovernmentProcedure obj)
		{
			if (obj != null)
			{
				bo.GovernmentProcedure = new DeclarationGovernmentProcedure() { TwTransportTypeCode = new DeclarationGovernmentProcedureTwTransportTypeCode { Value = obj.TransportTypeCode } };
			}
		}

		public void PopulatePreviousDocument(Declaration bo, IPreviousDocument obj)
		{
			if (obj != null)
			{
				bo.PreviousDocument = new DeclarationPreviousDocument() { TwFunctionalReferenceId = new DeclarationPreviousDocumentTwFunctionalReferenceId { Value = obj.FunctionalReferenceID } };
			}
		}

		public void PopulateDeclarationResponsibleGovernmentAgency(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.ResponsibleGovernmentAgency = new DeclarationResponsibleGovernmentAgency() { Id = new DeclarationResponsibleGovernmentAgencyId { Value = obj } };
			}
		}
	}
}
