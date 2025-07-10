using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	partial class AutoJobDeclaration
	{
		public new OrgHeader Buyer
		{
			get { return Factory.Load<OrgHeader>(JE_OH_Buyer); }
		}

		public override OrgHeader NotifyParty
		{
			get { return Factory.Load<OrgHeader>(JE_OH_NotifyParty); }
		}

		/// <summary>
		/// Importer of Record in ENS 10 position 8-19. This will be the party that takes responsibility for the transaction with CBP, and will be the company whose bond details are sent to CBP. The Importer of Record can be a shipper, consignee, buyer, or broker.
		/// </summary>
		public OrgHeader IOR => (OrgHeader)JE_OA_DeclarantAddress_ZAddress?.OrgHeader;

		#region JE_OA_DeclarantAddress_ZAddress

		protected bool HasCreatedJE_OA_DeclarantAddress_ZAddress { get; private set; }

		protected override ZAddress GetNewJE_OA_DeclarantAddress_ZAddress()
		{
			var zAddress = base.GetNewJE_OA_DeclarantAddress_ZAddress();
			HasCreatedJE_OA_DeclarantAddress_ZAddress = true;
			zAddress.OrgPKInfo.HumanReadableName = "IOR";
			zAddress.IsOrgVisible = true;
			zAddress.OrgPKValidation = CheckIOROrgPK;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
			return zAddress;
		}

		protected virtual void CheckIOROrgPK(ZPropertyInfo info)
		{
		}

		#endregion

		public RefUNLOCO PortOfExport
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, US_RL_NKPortOfExport); }
		}

		#region JE_OA_ManufacturerAddress_ZAddress

		protected override ZAddress GetNewJE_OA_ManufacturerAddress_ZAddress()
		{
			var zAddress = base.GetNewJE_OA_ManufacturerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
			zAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;

			return zAddress;
		}

		#endregion

		#region JE_OA_SellerZAddress

		protected override ZAddress GetNewJE_OA_SellerAddress_ZAddress()
		{
			var zAddress = base.GetNewJE_OA_SellerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);

			return zAddress;
		}

		#endregion

		#region JE_OA_ShipToPartyAddress_ZAddress

		protected override ZAddress GetNewJE_OA_ShipToPartyAddress_ZAddress()
		{
			var address = base.GetNewJE_OA_ShipToPartyAddress_ZAddress();
			address.IsOrgVisible = true;
			address.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDeliveryAddress);

			return address;
		}

		#endregion

		#region JE_OA_SoldToParty_ZAddress

		protected override ZAddress GetNewJE_OA_SoldToPartyAddress_ZAddress()
		{
			var zAddress = base.GetNewJE_OA_SoldToPartyAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress);

			return zAddress;
		}
		#endregion

		#region JE_OA_ConsigneeAddress_ZAddress

		protected override ZAddress GetNewJE_OA_ConsigneeAddress_ZAddress()
		{
			var zAddress = base.GetNewJE_OA_ConsigneeAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetCustomsAddressOfRecordAddress;

			return zAddress;
		}

		#endregion

		protected bool US_IsInvoiceByRequest_ReadOnly
		{
			get { return !IsImport; }
		}

		public bool IsWeeklyEstimateFilingDate
		{
			get { return US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate; }
		}

		public bool IsNonWeeklyEstimateFilingDate
		{
			get { return US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate; }
		}

		public bool HasSimplifiedEntryBeenLodgedAtCustoms
		{
			get
			{
				if (hasSimplifiedEntryBeenLodgedAtCustomsCached == null)
				{
					hasSimplifiedEntryBeenLodgedAtCustomsCached = new CachedProperty<bool>(Factory, () =>
					{
						var collection = ActiveEntryHeaders as ActiveCusEntryHeaderCollection;
						var seEntry = collection == null ? null : collection.SimplifiedEntry;
						return seEntry != null && seEntry.HasBeenLodgedAtCustoms;
					});
				}
				return hasSimplifiedEntryBeenLodgedAtCustomsCached.Value;
			}
		}
		CachedProperty<bool> hasSimplifiedEntryBeenLodgedAtCustomsCached;

		internal AddInfoJobDeclaration GetAddInfo()
		{
			return AddInfo;
		}

		public object GetOriginalAddInfoValue(SchemaColumn column)
		{
			return AddInfo.GetOriginalValue(column);
		}

		public bool HasAddInfoChangesSinceLastSaving(SchemaColumn column)
		{
			return AddInfo.HasChangesSinceLastSaving(column);
		}

		public RefCountry CountryOfDestination
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, US_RN_NKCountryOfDestination); }
		}

		public RefCountry FirstPortOfCallCountry
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, US_RN_NKFirstPortOfCallCountry); }
		}
	}
}
