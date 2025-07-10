using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class ShipmentDocAddressValidation : JobDocAddressValidation
	{
		readonly CommonShipment booking;
		public ShipmentDocAddressValidation(JobDocAddress addressToValidate) : base(addressToValidate)
		{
		}

		public ShipmentDocAddressValidation(JobDocAddress addressToValidate, CommonShipment booking) : base(addressToValidate)
		{
			this.booking = booking;
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (!Parent.OrganisationPKInfo.HasErrors() && (Shipment == null || !Shipment.JS_IsShipping))
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
						ValidateConsignorVsConsignee();
						ValidateSameConsigneeForBCN();
						break;
					case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
						ValidateConsignorVsConsignee();
						break;
					case DocAddressTypes.Codes.Manufacturer:
						ValidateManufacturer();
						break;
				}
			}
		}

		void ValidateConsignorVsConsignee()
		{
			if (!HasAddress(DocAddressType.ConsignorDocumentaryAddress) &&
				!HasAddress(DocAddressType.ConsigneeDocumentaryAddress))
			{
				Parent.OrganisationPKInfo.AddError(OrgMissingError);
			}
		}

		void ValidateSameConsigneeForBCN()
		{
			foreach (CommonConsol consol in Shipment.Consols)
			{
				if (consol.JK_TransportMode == Core.Constants.TransportModes.Sea &&
					consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol &&
					consol.Shipments.Count > 1)
				{
					ZGuid defaultConsignee = consol.Shipments[0].ConsigneePK;
					if (Parent.OrganisationPK != defaultConsignee)
					{
						Parent.OrganisationPKInfo.AddWarning(Res.GetString("7bcc1c53-9857-4a8a-a867-73fc31597af6", "Shipments attached to a BCN consol should have the same consignee"));
						break;
					}
				}
			}
		}

		void ValidateManufacturer()
		{
			if (Shipment.IsThirdPartyOwnershipHouse && !HasAddress(DocAddressType.Manufacturer))
			{
				Parent.OrganisationPKInfo.AddError(OrgMissingError);
			}
		}

		bool HasAddress(DocAddressType addressType)
		{
			JobDocAddress address = Shipment.DocAddresses.FindByDocAddressType(addressType);
			return address != null && address.IsValidAddress;
		}

		string OrgMissingError
		{
			get { return Res.GetString("0c27fff7-7d34-4eca-9bbf-e3a3de93f524", "This Organization has no address entered."); }
		}

		CommonShipment Shipment
		{
			get { return booking ?? (CommonShipment)Parent.Parent; }
		}
	}
}
