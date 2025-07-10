using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	partial class AutoJobComInvoiceLine
	{
		#region New Properties

		public virtual ZGuid JI_RX_InvoiceCurrencyReadonly
		{
			get
			{
				RefCurrency curr = RefCurrency.LoadFromCurrencyCode(Factory, InvoiceHeader.JZ_RX_NKInvoice_Currency);
				return (curr == null) ? ZGuid.Empty : curr.PK;
			}
		}

		public virtual ZPropertyInfo JI_RX_InvoiceCurrencyReadonlyInfo
		{
			get { return GetZPropertyInfo(nameof(JI_RX_InvoiceCurrencyReadonly)); }
		}

		#region JI_OA_ManufacturerAddress_ZAddress

		protected void SetJI_OA_ManufacturerAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingManufacturerDefaults = true;
				JI_OA_ManufacturerAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingManufacturerDefaults = false;
			}
		}
		protected bool disableSettingManufacturerDefaults;

		protected override ZAddress GetNewJI_OA_ManufacturerAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_ManufacturerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;

			return zAddress;
		}

		#endregion

		#region JI_OA_ExporterAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override ZAddress GetNewJI_OA_ExporterAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_ExporterAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;

			return zAddress;
		}

		protected void SetJI_OA_ExporterAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingForeignExporterDetails = true;
				JI_OA_ExporterAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingForeignExporterDetails = false;
			}
		}
		protected bool disableSettingForeignExporterDetails;

		#endregion

		#region JI_OA_Seller_ZAddress

		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

		protected override ZAddress GetNewJI_OA_Seller_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_Seller_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);

			return zAddress;
		}

		public OrgAddress SellerAddress
		{
			get { return Factory.Load<OrgAddress>(JI_OA_Seller); }
		}

		protected void SetJI_OA_SellerAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingSellerDefaults = true;
				JI_OA_Seller_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingSellerDefaults = false;
			}
		}
		protected bool disableSettingSellerDefaults;

		#endregion

		#region JI_OA_ShipToPartyAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void SetJI_OA_ShipToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingShipToPartyDefaults = true;
				JI_OA_ShipToPartyAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingShipToPartyDefaults = false;
			}
		}
		protected bool disableSettingShipToPartyDefaults;

		protected override ZAddress GetNewJI_OA_ShipToPartyAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_ShipToPartyAddress_ZAddress();
			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);

			return zAddress;
		}

		#endregion

		#region JI_OA_SoldToParty_ZAddress

		protected void SetJI_OA_SoldToParty_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingSoldToPartyDefaults = true;
				JI_OA_SoldToPartyAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingSoldToPartyDefaults = false;
			}
		}
		protected bool disableSettingSoldToPartyDefaults;

		protected override ZAddress GetNewJI_OA_SoldToPartyAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_SoldToPartyAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress);

			return zAddress;
		}

		#endregion

		#region JI_OA_ConsigneeAddress_ZAddress

		protected void SetJI_OA_ConsigneeAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingConsigneeDefaults = true;
				JI_OA_ConsigneeAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingConsigneeDefaults = false;
			}
		}
		protected bool disableSettingConsigneeDefaults;

		protected override ZAddress GetNewJI_OA_ConsigneeAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_ConsigneeAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress);

			return zAddress;
		}
		#endregion

		#endregion

		public bool US_98GoodsValue_ReadOnly
		{
			get { return US_98GoodsValue_ReadOnlyCore; }
		}

		protected virtual bool US_98GoodsValue_ReadOnlyCore
		{
			get { return false; }
		}

		public bool HasSupplementary
		{
			get { return !HasEmptySupTariff; }
		}

		public bool HasEmptySupTariff
		{
			get
			{
				return US_SupTariff.IsEmpty || US_SupTariff == TariffViewAsCodeDescription.NotApplicableCode;
			}
		}

		public bool IsACE
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration != null && declaration.IsACE;
			}
		}

		public bool ShouldDeclareACEDDTCData
		{
			get { return IsACE && OGAIndicatorList.IsToBeDeclared(US_DDTCInd); }
		}

		protected virtual bool IsACEDDTCDataDeclarationReadonly
		{
			get { return !ShouldDeclareACEDDTCData; }
		}

		protected virtual bool IsACETSCADataDeclarationReadonly
		{
			get { return !IsACE || !(OGAIndicatorList.IsToBeDeclared(US_TSCAInd) || OGAIndicatorList.IsToBeDeclared(US_ODSInd)); }
		}

		protected bool IsNeitherACEDDTCDataDeclarationtNorExport
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration == null || (IsACEDDTCDataDeclarationReadonly && !declaration.IsExport);
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			AutoJobComInvoiceLine result = (AutoJobComInvoiceLine)base.CloneInternal(args);
			result.AddInfo.CopyPersistentValuesFrom(AddInfo, args);

			return result;
		}

		internal AddInfoJobComInvoiceLine GetAddInfo()
		{
			return AddInfo;
		}

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobUSComInvoiceLineSchema.USI_JI;

		#endregion
	}
}
