using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;

namespace Enterprise.eTail.Documents.Business.US
{
	public class HVLVAirCargoAdvanceScreeningBuilder : AirCargoAdvanceScreeningBuilder
	{
		public HVLVAirCargoAdvanceScreeningBuilder(ForwardingShipment shipment, HVLVConsignment consignment, IDocDataObjectParameters parameters)
			: base(shipment, parameters)
		{
			this.consignment = consignment;
		}

		readonly HVLVConsignment consignment;

		protected override AirCargoAdvanceScreening CreateACAS()
		{
			return new AirCargoAdvanceScreening(
					nameof(HVLVConsignment),
					consignment.HVC_ConsignmentId,
					DocumentNames.AdvancedCargoReport);
		}

		protected override void PopulateHarmonizedCodes(AirCargoAdvanceScreening acas)
		{
			var itemLines = consignment.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();
			if (itemLines.Any())
			{
				acas.HarmonizedCodes = itemLines.Select(x => x.HVS_FormattedDestinationTariff).ToList();
			}
		}

		protected override void PopulateGoodsDescription(AirCargoAdvanceScreening acas)
		{
			acas.GoodsDescription = consignment.ACASReportGoodsDescription;
		}

		protected override void PopulateNumberOfPacks(AirCargoAdvanceScreening acas)
		{
			acas.NumberOfPacks = consignment.HVC_ItemCount;
		}

		protected override void PopulateWeight(AirCargoAdvanceScreening acas)
		{
			var newWeightUnit = Core.Constants.Weight.IsImperial(consignment.HVC_WeightUQ)
				? Core.Constants.Weight.Pounds
				: Core.Constants.Weight.Kilograms;

			var newWeight = consignment.HVC_ActualWeight > 0m ? consignment.HVC_ActualWeight : consignment.HVC_ManifestedWeight;

			acas.Weight = new Measurement()
			{
				Value = Core.Constants.Weight.Convert(newWeight, consignment.HVC_WeightUQ, newWeightUnit),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = newWeightUnit
				}
			};
		}

		protected override void PopulateHAWB(AirCargoAdvanceScreening acas)
		{
			acas.HAWB = consignment.HVC_WaybillNumber;
		}

		protected override void PopulateShipper(AirCargoAdvanceScreening acas)
		{
			acas.Shipper = PopulateShipperAddress()
				.AddPartyNameAndAddressValidation(nameof(acas.Shipper))
				.AddSupportedCharactersValidationForUSCustoms();
		}

		protected override void PopulateConsignee(AirCargoAdvanceScreening acas)
		{
			acas.Consignee = PopulateConsigneeAddress()
				.AddPartyNameAndAddressValidation(nameof(acas.Consignee))
				.AddSupportedCharactersValidationForUSCustoms();
		}

		protected override void PopulateStateDetails(AirCargoAdvanceScreening acas)
		{
			switch (consignment.HVC_ACASMessageStatus)
			{
				case HVLVACASMessageStatusList.Codes.OriginalSent:
					acas.State = AcasState.OriginalSent;
					break;
				case HVLVACASMessageStatusList.Codes.AcknowledgementRequired:
					acas.State = AcasState.AcknowledgementRequired;
					break;
				case HVLVACASMessageStatusList.Codes.AcknowledgementSent:
					acas.State = AcasState.AcknowledgementSent;
					break;
				case HVLVACASMessageStatusList.Codes.AmendmentRequired:
					acas.State = AcasState.AmendmentRequired;
					break;
				case HVLVACASMessageStatusList.Codes.AmendmentSent:
					acas.State = AcasState.AmendmentSent;
					break;
				default:
					acas.State = AcasState.None;
					break;
			}
		}

		Address PopulateShipperAddress()
		{
			Address result;
			if (consignment.ShipperIsOrganisation)
			{
				result = AddressBuilder.Create(context, consignment.ShipperAddress);
				var header = consignment.ShipperAddress.Header;
				if (header.Contacts.Count > 0)
				{
					var contact = header.Contacts[0];
					result.Contact = contact.OC_ContactName;
					result.Fax = contact.OC_Fax;
					result.Phone = contact.OC_Phone;
					result.Email = contact.OC_Email;
				}
			}
			else
			{
				result = new Address(context.Factory);
				result.CompanyName = consignment.HVC_ShipperName;
				result.AddressLine1 = consignment.HVC_ShipperAddress1;
				result.AddressLine2 = consignment.HVC_ShipperAddress2;
				result.City = consignment.HVC_ShipperCity;
				result.State = consignment.HVC_ShipperState;
				result.Postcode = consignment.HVC_ShipperPostcode;
				result.Country = Country.Create(context, consignment.ShipperCountryCode);
				result.Contact = consignment.HVC_ShipperContact;
				result.Fax = consignment.HVC_ShipperFax;
				result.Phone = consignment.HVC_ShipperPhone;
				result.Email = consignment.HVC_ShipperEmail;
			}

			return result;
		}

		Address PopulateConsigneeAddress()
		{
			Address result;
			if (consignment.ConsigneeIsOrganisation)
			{
				result = AddressBuilder.Create(context, consignment.ConsigneeAddress);
				var header = consignment.ConsigneeAddress.Header;
				if (header.Contacts.Count > 0)
				{
					var contact = header.Contacts[0];
					result.Contact = contact.OC_ContactName;
					result.Fax = contact.OC_Fax;
					result.Phone = contact.OC_Phone;
					result.Email = contact.OC_Email;
				}
			}
			else
			{
				result = new Address(context.Factory);
				result.CompanyName = consignment.HVC_ConsigneeName;
				result.AddressLine1 = consignment.HVC_ConsigneeAddress1;
				result.AddressLine2 = consignment.HVC_ConsigneeAddress2;
				result.City = consignment.HVC_ConsigneeCity;
				result.State = consignment.HVC_ConsigneeState;
				result.Postcode = consignment.HVC_ConsigneePostcode;
				result.Country = Country.Create(context, consignment.ConsigneeCountryCode);
				result.Contact = consignment.HVC_ConsigneeContact;
				result.Fax = consignment.HVC_ConsigneeFax;
				result.Phone = consignment.HVC_ConsigneePhone;
				result.Email = consignment.HVC_ConsigneeEmail;
			}

			return result;
		}
	}
}
