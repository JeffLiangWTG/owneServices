using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoJobComInvoiceHeader
	{
		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(JZ_OH_Buyer); }
		}

		#region New Properties

		public ZBool US_IsScheduleKTermsOfDeliveryLocation
		{
			get { return AddInfo.US_IsScheduleKTermsOfDeliveryLocation; }
		}

		public ZBool US_IsScheduleDTermsOfDeliveryLocation
		{
			get { return AddInfo.US_IsScheduleDTermsOfDeliveryLocation; }
		}

		public ZBool US_IsOtherTermsOfDeliveryLocation
		{
			get { return AddInfo.US_IsOtherTermsOfDeliveryLocation; }
		}

		public ZBool US_IsISOCountryCodeTermsOfDeliveryLocation
		{
			get { return AddInfo.US_IsISOCountryCodeTermsOfDeliveryLocation; }
		}

		#region JZ_OA_ManufacturerAddress_ZAddress

		protected void DefaultCountryOfOriginFromOrganisationDetail(OrgAddress address)
		{
			if (!IsCopying)
			{
				if (USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
				{
					new CountryOfOriginDefaulter().Default(US_UC_NKCountryOfOriginInfo, address);
				}
			}
		}

		protected void SetJZ_OA_ManufacturerAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingManufacturerDefaults = true;
				JZ_OA_ManufacturerAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingManufacturerDefaults = false;
			}
		}
		protected bool disableSettingManufacturerDefaults;

		protected override ZAddress GetNewJZ_OA_ManufacturerAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_ManufacturerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMIDAddress;
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;

			return zAddress;
		}

		#endregion

		#region JZ_OA_SupplierAddress_ZAddress

		protected void SetJZ_OA_SupplierAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingSupplierDefaults = true;
				JZ_OA_SupplierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingSupplierDefaults = false;
			}
		}
		protected bool disableSettingSupplierDefaults;

		protected override ZAddress GetNewJZ_OA_SupplierAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_SupplierAddress_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
			zAddress.OnOrgChanged += new EventHandler(fJZ_OA_SupplierAddress_ZAddress_OnOrgChanged);
			return zAddress;
		}

		void fJZ_OA_SupplierAddress_ZAddress_OnOrgChanged(object sender, EventArgs e)
		{
			if (!disableSettingSupplierDefaults)
			{
				try
				{
					disableSettingSupplierDefaults = true;
					if (IsInDatabase || JobComInvoiceLines != null)
					{
						JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();
					}

					DefaultIncoTermAndCurrencyFromSupplier();
				}
				finally
				{
					disableSettingSupplierDefaults = false;
				}
			}
		}

		#endregion

		#region JZ_OA_ExorterAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override ZAddress GetNewJZ_OA_ExporterAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_ExporterAddress_ZAddress();
			zAddress.IsOrgVisible = true;
			var declaration = (JobDeclaration)JobDeclaration;
			if (declaration != null && declaration.IsACE)
			{
				zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
			}
			else
			{
				zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
			}
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
			return zAddress;
		}

		protected void SetJZ_OA_ExporterAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingExporterDefaults = true;
				base.JZ_OA_ExporterAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingExporterDefaults = false;
			}
		}
		protected bool disableSettingExporterDefaults;

		#endregion

		#region JZ_OA_Seller_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress JZ_OA_Seller_ZAddress
		{
			get
			{
				if (fJZ_OA_Seller_ZAddress == null)
				{
					fJZ_OA_Seller_ZAddress = GetNewJZ_OA_SellerAddress_ZAddress();
					fJZ_OA_Seller_ZAddress.IsOrgVisible = true;
					fJZ_OA_Seller_ZAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;
				}
				return fJZ_OA_Seller_ZAddress;
			}
		}
		ZAddress fJZ_OA_Seller_ZAddress;

		protected void SetJZ_OA_Seller_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingSellerDefaults = true;
				JZ_OA_Seller_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingSellerDefaults = false;
			}
		}
		protected bool disableSettingSellerDefaults;

		protected override ZAddress GetNewJZ_OA_SellerAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_SellerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_ShipToPartyAddress_ZAddress

		protected void SetJZ_OA_ShipToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingShipToPartyDefaults = true;
				base.JZ_OA_ShipToPartyAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingShipToPartyDefaults = false;
			}
		}
		protected bool disableSettingShipToPartyDefaults;

		protected override ZAddress GetNewJZ_OA_ShipToPartyAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_ShipToPartyAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_SoldToPartyAddress_ZAddress

		protected void SetJZ_OA_SoldToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingSoldToPartyDefaults = true;
				JZ_OA_SoldToPartyAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingSoldToPartyDefaults = false;
			}
		}
		protected bool disableSettingSoldToPartyDefaults;

		protected override ZAddress GetNewJZ_OA_SoldToPartyAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_SoldToPartyAddress_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_ConsigneeAddress_ZAddress

		protected void SetJZ_OA_ConsigneeAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingConsigneeDefaults = true;
				JZ_OA_ConsigneeAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingConsigneeDefaults = false;
			}
		}
		protected bool disableSettingConsigneeDefaults;

		protected override ZAddress GetNewJZ_OA_ConsigneeAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_ConsigneeAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_DistributorAddress_ZAddress

		protected void SetJZ_OA_DistributorAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingDistributorDefaults = true;
				base.JZ_OA_DistributorAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingDistributorDefaults = false;
			}
		}
		protected bool disableSettingDistributorDefaults;

		protected override ZAddress GetNewJZ_OA_DistributorAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_DistributorAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_PackagerAddress_ZAddress

		protected void SetJZ_OA_PackagerAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingPackagerDefaults = true;
				base.JZ_OA_PackagerAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingPackagerDefaults = false;
			}
		}
		protected bool disableSettingPackagerDefaults;

		protected override ZAddress GetNewJZ_OA_PackagerAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_PackagerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;

			return zAddress;
		}

		#endregion

		#region JZ_OA_ShipperAddress_ZAddress

		protected void SetJZ_OA_ShipperAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingShipperDefaults = true;
				base.JZ_OA_ShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingShipperDefaults = false;
			}
		}
		protected bool disableSettingShipperDefaults;

		protected override ZAddress GetNewJZ_OA_ShipperAddress_ZAddress()
		{
			var zAddress = base.GetNewJZ_OA_ShipperAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;

			return zAddress;
		}

		#endregion

		#endregion

		protected bool IsUSOrganisationSynchronizationEnable(ZBool isAddressOverride)
		{
			return IsExport && !isAddressOverride;
		}

		internal AddInfoJobComInvoiceHeader GetAddInfo()
		{
			return AddInfo;
		}
	}
}
