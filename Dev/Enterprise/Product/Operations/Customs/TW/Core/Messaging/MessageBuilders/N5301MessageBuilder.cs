using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.TW.MessageDefinitions.N5301;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class N5301MessageBuilder : BaseTWMessageBuilder<IN5301Declaration, Declaration>
	{
		public N5301MessageBuilder()
		{
		}

		public N5301MessageBuilder(IN5301Declaration declaration)
		{
			this.declaration = declaration;
		}

		readonly IN5301Declaration declaration;

		public ZString PopulateXml()
		{
			return declaration != null ? XmlHelper.Serializer(typeof(Declaration), PopulateDeclaration(declaration), true) : string.Empty;
		}

		public override Declaration PopulateDeclaration(IN5301Declaration obj, string functionCode = null)
		{
			var newItem = new Declaration();
			if (obj != null)
			{
				newItem.FunctionCode = new DeclarationFunctionCode { Value = MessageFunctionCode.Add };
				newItem.Id = new DeclarationId { Value = obj.ID };
				PopulateValueIfNodeValueIsNotEmpty(obj.TotalGrossMassMeasure, () => { newItem.TotalGrossMassMeasure = new DeclarationTotalGrossMassMeasure { Value = obj.TotalGrossMassMeasure }; });
				newItem.TotalPackageQuantity = new DeclarationTotalPackageQuantity { Value = obj.TotalPackageQuantity };
				newItem.TypeCode = new DeclarationTypeCode { Value = obj.TypeCode };
				PopulateAdditionalInformation(newItem, obj.AdditionalInformations);
				PopulateAgent(newItem, obj.Agent);
				PopulateBorderTransportMeans(newItem, obj.BorderTransportMeans);
				PopulateCarrier(newItem, obj.Carrier);
				PopulateConsignment(newItem, obj.Consignment);
				PopulateDeconsolidator(newItem, obj.Deconsolidator);
				PopulateDeclarationLoadingLocation(newItem, obj.LoadingLocation);
				PopulateRepresentativePerson(newItem, obj.RepresentativePersonName);
				PopulateApplicant(newItem, obj.Applicant);
				PopulateUnloadingLocation(newItem, obj.UnloadingLocation);
			}
			return newItem;
		}

		public void PopulateAdditionalInformation(Declaration bo, IEnumerable<IAdditionalInformation> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationAdditionalInformation>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationAdditionalInformation();
					newItem.StatementCode = new DeclarationAdditionalInformationStatementCode { Value = item.StatementCode };
					newItem.StatementDescription = new DeclarationAdditionalInformationStatementDescription { Value = item.StatementDescription };
					collection.Add(newItem);
				}
				bo.AdditionalInformation = collection;
			}
		}

		public void PopulateAgent(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Agent = new DeclarationAgent();
				var newItem = bo.Agent;
				newItem.Id = new DeclarationAgentId { Value = obj.ID };
				newItem.RoleCode = new DeclarationAgentRoleCode { Value = obj.RoleCode };
				newItem.TwSubBoxId = new DeclarationAgentTwSubBoxId { Value = obj.SubBoxID };
			}
		}

		public void PopulateBorderTransportMeans(Declaration bo, ITransportMeans obj)
		{
			if (obj != null)
			{
				bo.BorderTransportMeans = new DeclarationBorderTransportMeans();
				var newItem = bo.BorderTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ArrivalDateTime, () => { newItem.ArrivalDateTime = obj.ArrivalDateTime.ToISO8601ShortDateString(); });
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationBorderTransportMeansId { Value = obj.ID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.JourneyID, () => { newItem.JourneyId = new DeclarationBorderTransportMeansJourneyId { Value = obj.JourneyID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => { newItem.Name = new DeclarationBorderTransportMeansName { Value = obj.Name }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => { newItem.TwRegistration = new DeclarationBorderTransportMeansTwRegistration { Value = obj.Registration }; });
				newItem.TypeCode = new DeclarationBorderTransportMeansTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateCarrier(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { bo.Carrier = new DeclarationCarrier() { Id = new DeclarationCarrierId { Value = obj.ID } }; });
			}
		}

		public void PopulateConsignment(Declaration bo, IConsignment obj)
		{
			if (obj != null)
			{
				bo.Consignment = new DeclarationConsignment();
				var newItem = bo.Consignment;
				PopulateValueIfNodeValueIsNotEmpty(obj.ManifestSerialNumber, () => { newItem.TwManifestSerialNumber = new DeclarationConsignmentTwManifestSerialNumber { Value = obj.ManifestSerialNumber }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.ShippingOrderNumber, () => { newItem.TwShippingOrderNumber = new DeclarationConsignmentTwShippingOrderNumber { Value = obj.ShippingOrderNumber }; });
				PopulateConsignmentItem(newItem, obj.ConsignmentItem);
				PopulateDepartureTransportMeans(newItem, obj.DepartureTransportMeans);
				PopulateLoadingLocation(newItem, obj.LoadingLocation);
				PopulateTransitTransportMeans(newItem, obj.TransitTransportMeansTypeCode);
				PopulateConsignmentTransportContractDocument(newItem, obj.TransportContractDocuments);
				PopulateTransportEquipment(newItem, obj.TransportEquipments);
			}
		}

		public void PopulateConsignmentItem(DeclarationConsignment bo, IConsignmentItem obj)
		{
			if (obj != null)
			{
				bo.ConsignmentItem = new DeclarationConsignmentConsignmentItem();
				var newItem = bo.ConsignmentItem;
				PopulateCommodity(newItem, obj.Commodity);
				PopulateGoodsMeasure(newItem, obj.GoodsMeasure);
				PopulatePackaging(newItem, obj.Packaging);
				PopulateTransportContractDocument(newItem, obj.TransportContractDocuments);
			}
		}

		public void PopulateCommodity(DeclarationConsignmentConsignmentItem bo, ICommodity obj)
		{
			if (obj != null)
			{
				PopulateValueIfNodeValueIsNotEmpty(obj.CargoDescription, () => { bo.Commodity = new DeclarationConsignmentConsignmentItemCommodity() { CargoDescription = new DeclarationConsignmentConsignmentItemCommodityCargoDescription { Value = obj.CargoDescription } }; });
			}
		}

		public void PopulateGoodsMeasure(DeclarationConsignmentConsignmentItem bo, IGoodsMeasure obj)
		{
			if (obj != null && (!obj.TariffQuantity.IsEmpty && !obj.UnitCode.IsEmpty))
			{
				bo.GoodsMeasure = new DeclarationConsignmentConsignmentItemGoodsMeasure();
				var newItem = bo.GoodsMeasure;
				newItem.TariffQuantity = new DeclarationConsignmentConsignmentItemGoodsMeasureTariffQuantity { Value = obj.TariffQuantity };
				newItem.TwUnitCode = new DeclarationConsignmentConsignmentItemGoodsMeasureTwUnitCode { Value = obj.UnitCode };
			}
		}

		public void PopulatePackaging(DeclarationConsignmentConsignmentItem bo, IPackaging obj)
		{
			if (obj != null)
			{
				bo.Packaging = new DeclarationConsignmentConsignmentItemPackaging();
				var newItem = bo.Packaging;
				PopulateValueIfNodeValueIsNotEmpty(obj.MarksNumbers, () => { newItem.MarksNumbers = new DeclarationConsignmentConsignmentItemPackagingMarksNumbers { Value = obj.MarksNumbers }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.PackagingMaterialDescription, () => { newItem.PackagingMaterialDescription = new DeclarationConsignmentConsignmentItemPackagingPackagingMaterialDescription { Value = obj.PackagingMaterialDescription }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Combination, () => { newItem.TwCombination = new DeclarationConsignmentConsignmentItemPackagingTwCombination { Value = obj.Combination }; });
				newItem.TypeCode = new DeclarationConsignmentConsignmentItemPackagingTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateTransportContractDocument(DeclarationConsignmentConsignmentItem bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentConsignmentItemTransportContractDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentConsignmentItemTransportContractDocument();
					newItem.Id = new DeclarationConsignmentConsignmentItemTransportContractDocumentId { Value = item.ID };
					newItem.TypeCode = new DeclarationConsignmentConsignmentItemTransportContractDocumentTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TransportContractDocument = collection;
			}
		}

		public void PopulateDepartureTransportMeans(DeclarationConsignment bo, ITransportMeans obj)
		{
			if (obj != null && (!obj.ID.IsEmpty || !obj.Name.IsEmpty || !obj.JourneyID.IsEmpty || !obj.Registration.IsEmpty))
			{
				bo.DepartureTransportMeans = new DeclarationConsignmentDepartureTransportMeans();
				var newItem = bo.DepartureTransportMeans;
				PopulateValueIfNodeValueIsNotEmpty(obj.ID, () => { newItem.Id = new DeclarationConsignmentDepartureTransportMeansId { Value = obj.ID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Name, () => { newItem.Name = new DeclarationConsignmentDepartureTransportMeansName { Value = obj.Name }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.JourneyID, () => { newItem.TwJourneyId = new DeclarationConsignmentDepartureTransportMeansTwJourneyId { Value = obj.JourneyID }; });
				PopulateValueIfNodeValueIsNotEmpty(obj.Registration, () => { newItem.TwRegistration = new DeclarationConsignmentDepartureTransportMeansTwRegistration { Value = obj.Registration }; });
			}
		}

		public void PopulateLoadingLocation(DeclarationConsignment bo, ILocation obj)
		{
			var id = obj?.ID ?? ZString.Empty;
			if (!id.IsEmpty)
			{
				bo.LoadingLocation = new DeclarationConsignmentLoadingLocation() { Id = new DeclarationConsignmentLoadingLocationId { Value = id } };
			}
		}

		public void PopulateTransitTransportMeans(DeclarationConsignment bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.TransitTransportMeans = new DeclarationConsignmentTransitTransportMeans() { TypeCode = new DeclarationConsignmentTransitTransportMeansTypeCode { Value = obj } };
			}
		}

		public void PopulateConsignmentTransportContractDocument(DeclarationConsignment bo, IEnumerable<ITransportContractDocument> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentTransportContractDocument>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentTransportContractDocument();
					newItem.Id = new DeclarationConsignmentTransportContractDocumentId { Value = item.ID };
					newItem.TypeCode = new DeclarationConsignmentTransportContractDocumentTypeCode { Value = item.TypeCode };
					collection.Add(newItem);
				}
				bo.TransportContractDocument = collection;
			}
		}

		public void PopulateTransportEquipment(DeclarationConsignment bo, IEnumerable<ITransportEquipment> obj)
		{
			if (obj != null)
			{
				var collection = new Collection<DeclarationConsignmentTransportEquipment>();
				foreach (var item in obj)
				{
					var newItem = new DeclarationConsignmentTransportEquipment();
					newItem.CharacteristicCode = new DeclarationConsignmentTransportEquipmentCharacteristicCode { Value = item.CharacteristicCode };
					newItem.Id = new DeclarationConsignmentTransportEquipmentId { Value = item.ID };
					newItem.TwUsedCapacityCode = new DeclarationConsignmentTransportEquipmentTwUsedCapacityCode { Value = item.UsedCapacityCode };
					collection.Add(newItem);
				}
				bo.TransportEquipment = collection;
			}
		}

		public void PopulateDeconsolidator(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.Deconsolidator = new DeclarationDeconsolidator() { Id = new DeclarationDeconsolidatorId { Value = obj.ID } };
			}
		}

		public void PopulateDeclarationLoadingLocation(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.LoadingLocation = new DeclarationLoadingLocation() { Id = new DeclarationLoadingLocationId { Value = obj } };
			}
		}

		public void PopulateRepresentativePerson(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.RepresentativePerson = new DeclarationRepresentativePerson() { Name = new DeclarationRepresentativePersonName { Value = obj } };
			}
		}

		public void PopulateApplicant(Declaration bo, IPartyDetails obj)
		{
			if (obj != null)
			{
				bo.TwApplicant = new DeclarationTwApplicant();
				var newItem = bo.TwApplicant;
				newItem.TwChineseName = new DeclarationTwApplicantTwChineseName { Value = obj.ChineseName.SubstringSafe(0, 70) };
				newItem.TwCustomsControlId = new DeclarationTwApplicantTwCustomsControlId { Value = obj.CustomsControlID };
				newItem.TwId = new DeclarationTwApplicantTwId { Value = obj.ID };
				newItem.TwName = new DeclarationTwApplicantTwName { Value = obj.Name.SubstringSafe(0, 80) };
				newItem.TwTypeCode = new DeclarationTwApplicantTwTypeCode { Value = obj.TypeCode };
			}
		}

		public void PopulateUnloadingLocation(Declaration bo, ZString obj)
		{
			if (!obj.IsEmpty)
			{
				bo.UnloadingLocation = new DeclarationUnloadingLocation() { Id = new DeclarationUnloadingLocationId { Value = obj } };
			}
		}
	}
}
