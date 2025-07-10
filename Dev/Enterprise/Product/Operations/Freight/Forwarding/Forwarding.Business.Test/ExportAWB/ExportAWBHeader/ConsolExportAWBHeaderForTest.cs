using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ConsolExportAWBHeaderForTest : ConsolExportAWBHeader
	{
		public ConsolExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString HandlingInformationForTest
		{
			get { return base.HandlingInformation; }
		}

		public new ZDecimal RateLineChargeableWeight
		{
			get { return base.RateLineChargeableWeight; }
		}

		public new ZDecimal RateLineGrossWeight
		{
			get { return base.RateLineGrossWeight; }
		}

		public new ZString RateLineWeightUnit
		{
			get { return base.RateLineWeightUnit; }
		}

		public new ZString BillNumber
		{
			get { return base.BillNumber; }
		}

		public new ZString ReferenceNumber
		{
			get { return base.ReferenceNumber; }
		}

		public new ZString ConsolNumber
		{
			get { return base.ConsolNumber; }
		}

		public new ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet
		{
			get { return base.IsConsigneeAdvanceCargoReportingSelfFilerSet; }
		}

		public new bool IsTaxAutoCalculated
		{
			get { return base.IsTaxAutoCalculated; }
		}

		public new CommonShipment DirectShipment
		{
			get { return base.DirectShipment; }
		}

		public new JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return base.ConsigneeDocumentaryAddress; }
		}

		public new JobDocAddress ShipperDocumentaryAddress
		{
			get { return base.ShipperDocumentaryAddress; }
		}

		public new ZString ExtraCarrierInfoLine2
		{
			get { return base.ExtraCarrierInfoLine2; }
		}

		public new ZString ExtraShipperInfoLine1
		{
			get { return base.ExtraShipperInfoLine1; }
		}

		public new ZString ExtraShipperInfoLine2
		{
			get { return base.ExtraShipperInfoLine2; }
		}

		public new OrgHeader ReceivingForwarder
		{
			get { return base.ReceivingForwarder; }
		}

		public new OrgHeader SendingForwarder
		{
			get { return base.SendingForwarder; }
		}

		public new ZDecimal RateLineTotal
		{
			get { return base.RateLineTotal; }
		}

		public new DefaultAddressTypes DefaultConsigneeAddressType
		{
			get { return base.DefaultConsigneeAddressType; }
		}

		public new DefaultAddressTypes DefaultShipperAddressType
		{
			get { return base.DefaultShipperAddressType; }
		}

		public new OrgAddress ConsigneeOfficeAddress
		{
			get { return base.ConsigneeOfficeAddress; }
		}

		public new OrgAddress ConsigneeDeliveryAddress
		{
			get { return base.ConsigneeDeliveryAddress; }
		}

		public new OrgAddress ShipperOfficeAddress
		{
			get { return base.ShipperOfficeAddress; }
		}

		public new OrgAddress ShipperPickupAddress
		{
			get { return base.ShipperPickupAddress; }
		}

		public new ZString DefaultConsigneeCompanyName
		{
			get { return base.DefaultConsigneeCompanyName; }
		}

		public new ZString DefaultShipperCompanyName
		{
			get { return base.DefaultShipperCompanyName; }
		}

		public new List<OrgAddress> GetConsigneeAddresses()
		{
			return base.GetConsigneeAddresses();
		}

		public new List<OrgAddress> GetShipperAddresses()
		{
			return base.GetShipperAddresses();
		}

		public new JobDocAddress NotifyPartyDocumentaryAddress
		{
			get { return base.NotifyPartyDocumentaryAddress; }
		}

		public new List<OrgAddress> GetAlsoNotifyAddresses()
		{
			return base.GetAlsoNotifyAddresses();
		}

		public override ForwardingConsol Consol
		{
			get { return consol ?? base.Consol; }
		}

		ForwardingConsol consol;

		public void SetConsol(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.OH_Code = "ACCT";
					AddAddresses(fOrganisation.Addresses);
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;

		void AddAddresses(IList addressList)
		{
			OrgAddress orgAddress = addressList.Count == 1 ? (OrgAddress)addressList[0] : Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "COMPANYNAME";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.OA_Address1 = "ADDRESSOFC";
			orgAddress.OA_Address2 = "ADDRESSOFC2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2006";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96(3)766";
			if (orgAddress.AddressCapability.IsEmpty)
			{
				orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			}

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "ADDRESSPIC";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "ADDRESSDLV";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "CONAMEDOC";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.OA_Address1 = "ADDRESSPAD";
			orgAddress.OA_Address2 = "ADDRESSPAD2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "1005";
			orgAddress.OA_RL_NKRelatedPortCode = "SGSIN";
			orgAddress.OA_State = "VICTORIA";
			orgAddress.OA_Phone = "+96(1)723";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);

			addressList.Add(orgAddress);
		}
	}
}
