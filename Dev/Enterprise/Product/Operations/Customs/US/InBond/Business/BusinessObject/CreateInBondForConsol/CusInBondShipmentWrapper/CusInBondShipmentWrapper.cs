using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondShipmentWrapper : NonPersistentBusinessObject
	{
		public CusInBondShipmentWrapper(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			this.shipment = shipment;
			if (shipment.ConsigneeDocumentaryAddress is JobDocAddress docAddress && docAddress.HasRealAddress)
			{
				ConsigneeOrgPK = shipment.ConsigneePK;
				ConsigneeOrganizationAddress = docAddress.E2_OA_Address;
			}
		}
		readonly ForwardingShipment shipment;

		#region Shipment Number

		[ResourceStringData("14367C75-E965-486D-85F2-CE462C571DB6", Caption = "Shipment Number")]
		public ZString ShipmentNumber => shipment.JS_UniqueConsignRef;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region House Bill Number

		[ResourceStringData("6EC8E284-9D81-4B8E-8A8C-51C5428160D5", Caption = "House Bill Number")]
		public ZString HouseBillNumber => shipment.JS_HouseBill;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		#endregion

		public ZGuid ConsignorPK => shipment.ConsignorPK;

		public ZDecimal GoodsValue => shipment.JS_GoodsValue;

		public ZString GoodsValueCurrency => shipment.JS_RX_NKGoodsValueCurr;

		public ZDecimal ActualWeight => shipment.JS_ActualWeight;

		public ZString WeightUnit => shipment.JS_UnitOfWeight;

		public ZDecimal ActualVolume => shipment.JS_ActualVolume;

		public ZString VolumeUnit => shipment.JS_UnitOfVolume;

		public ForwardingShipment Shipment => shipment;

		#region Consignee Organization

		[List(nameof(Consignees))]
		[ResourceStringData("7B174CD5-06A6-400C-B6FA-640DBBCAE15C", Caption = "Consignee Organization")]
		public ZGuid ConsigneeOrgPK
		{
			get { return ConsigneeOrganizationAddress_ZAddress.OrgPK; }
			set { ConsigneeOrganizationAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ConsigneeOrgPKInfo => GetWrappedZPropertyInfo(nameof(ConsigneeOrgPK), x => ConsigneeOrganizationAddress_ZAddress.OrgPKInfo);

		[RelatedBusinessObject(nameof(ConsigneeAddress))]
		[List(nameof(Consignees))]
		[ResourceStringData("4C917612-CB3B-45FC-8000-1542A1FEDC72", Caption = "Consignee Organization Address")]
		public ZGuid ConsigneeOrganizationAddress
		{
			get => consigneeOrganizationAddress;
			set => SetNonPersistentPropertyValue(ConsigneeOrganizationAddressInfo, ref consigneeOrganizationAddress, value);
		}
		ZGuid consigneeOrganizationAddress;

		public ZPropertyInfo ConsigneeOrganizationAddressInfo => GetZPropertyInfo(nameof(ConsigneeOrganizationAddress));

		public OrgAddress ConsigneeAddress
		{
			get { return Factory.Load<OrgAddress>(ConsigneeOrganizationAddress); }
		}

		public OrganisationsFindBoxCollection Consignees
		{
			get
			{
				return shipment.Lookups.ConsigneeForwarder_List;
			}
		}

		#region ZAddress

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress ConsigneeOrganizationAddress_ZAddress
		{
			get
			{
				if (consigneeOrganizationAddress_ZAddress == null)
				{
					consigneeOrganizationAddress_ZAddress = new ZAddress(ConsigneeOrganizationAddressInfo);
					consigneeOrganizationAddress_ZAddress.IsOrgVisible = true;
					consigneeOrganizationAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return consigneeOrganizationAddress_ZAddress;
			}
		}
		ZAddress consigneeOrganizationAddress_ZAddress;

		#endregion

		#endregion
	}
}
