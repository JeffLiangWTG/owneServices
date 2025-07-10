using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5167;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5167MessageBuilder : BaseTWMessageBuilder<IN5167Declaration, Declaration>
	{
		public ZString PopulateXml(IN5167Declaration declaration)
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5167Declaration input, string functionCode = null)
		{
			var newItem = new Declaration();
			if (input != null)
			{
				newItem.DeclarationOfficeId = new DeclarationDeclarationOfficeId() { Value = input.DeclarationOfficeID };
				newItem.FunctionalReferenceId = new DeclarationFunctionalReferenceId() { Value = input.FunctionalReferenceID };
				newItem.FunctionCode = new DeclarationFunctionCode() { Value = MessageFunctionCode.Add };
				PopulateAgent(newItem, input.Agent);
				PopulateConsignment(newItem, input.Consignment);
				PopulateGoodsShipment(newItem, input.GoodsShipment);
				PopulateImporter(newItem, input.Importer);
			}
			return newItem;
		}

		public void PopulateAgent(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationAgentId() { Value = obj.ID };
				newItem.RoleCode = new DeclarationAgentRoleCode() { Value = obj.RoleCode };
				newItem.TwSubBoxId = new DeclarationAgentTwSubBoxId() { Value = obj.SubBoxID };
			}
		}

		public void PopulateConsignment(Declaration bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				PopulateTransportContractDocument(bo.Consignment, obj.TransportContractDocuments?.FirstOrDefault());
			}
		}

		public void PopulateTransportContractDocument(DeclarationConsignment bo, ITransportContractDocument obj)
		{
			if (obj != null)
			{
				bo.TransportContractDocument = new DeclarationConsignmentTransportContractDocument();
				PopulateDeconsolidator(bo.TransportContractDocument, obj.Deconsolidator);
			}
		}

		public void PopulateDeconsolidator(DeclarationConsignmentTransportContractDocument bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Deconsolidator = new DeclarationConsignmentTransportContractDocumentDeconsolidator() { Id = new DeclarationConsignmentTransportContractDocumentDeconsolidatorId() { Value = obj.ID } };
			}
		}

		public void PopulateGoodsShipment(Declaration bo, IGoodsShipment obj)
		{
			if (obj != null)
			{
				bo.GoodsShipment = new DeclarationGoodsShipment();
				var newItem = bo.GoodsShipment;
				PopulateGoodsShipmentConsignment(newItem, obj.Consignment);
				PopulateGovernmentAgencyGoodsItem(newItem, obj.GovernmentAgencyGoodsItems);
			}
		}

		public void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationGoodsShipmentConsignment();
				PopulateCarrier(bo.Consignment, obj.Carrier);
			}
		}

		public void PopulateCarrier(DeclarationGoodsShipmentConsignment bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Carrier = new DeclarationGoodsShipmentConsignmentCarrier() { Id = new DeclarationGoodsShipmentConsignmentCarrierId() { Value = obj.ID } };
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
					PopulateControl(newItem, item.ControlInspectionStartDateTime);
					PopulateExaminationPlace(newItem, item.ExaminationPlace);
					PopulateTransportEquipment(newItem, item.TransportEquipments);
					PopulateAdditionalDeclaration(newItem, item.AdditionalDeclaration);
					collection.Add(newItem);
				}
				bo.GovernmentAgencyGoodsItem = collection;
			}
		}

		public void PopulateControl(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ZDateTime obj)
		{
			bo.Control = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemControl() { InspectionStartDateTime = obj.ToISO8601String() };
		}

		public void PopulateExaminationPlace(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, ZString obj)
		{
			bo.ExaminationPlace = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExaminationPlace() { Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExaminationPlaceId() { Value = obj } };
		}

		public void PopulateTransportEquipment(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipment>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipment();
					newItem.CharacteristicCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentCharacteristicCode() { Value = item.CharacteristicCode };
					newItem.Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentId() { Value = item.ID };
					newItem.TwUsedCapacityCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentTwUsedCapacityCode() { Value = item.UsedCapacityCode };
					PopulateSeal(newItem, item.Seals);
					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		public void PopulateSeal(DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipment bo, IEnumerable<ZString> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentTwSeal>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentTwSeal() { TwSealId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTransportEquipmentTwSealTwSealId() { Value = item } };
					collection.Add(newItem);
				}
				bo.TwSeal = collection;
			}
		}

		public void PopulateAdditionalDeclaration(DeclarationGoodsShipmentGovernmentAgencyGoodsItem bo, IAdditionalDeclaration obj)
		{
			if (obj != null)
			{
				bo.TwAdditionalDeclaration = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwAdditionalDeclaration();
				var newItem = bo.TwAdditionalDeclaration;
				newItem.TwId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwAdditionalDeclarationTwId() { Value = obj.ID };
				newItem.TwTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwAdditionalDeclarationTwTypeCode() { Value = obj.TypeCode };
			}
		}

		public void PopulateImporter(Declaration bo, IPartyDetails obj)
		{
			if (obj != null && !obj.CustomsControlID.IsEmpty)
			{
				bo.Importer = new DeclarationImporter() { TwCustomsControlId = new DeclarationImporterTwCustomsControlId() { Value = obj.CustomsControlID } };
			}
		}
	}
}
