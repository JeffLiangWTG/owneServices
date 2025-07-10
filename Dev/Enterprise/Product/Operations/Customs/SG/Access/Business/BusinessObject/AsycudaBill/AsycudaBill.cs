using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static System.FormattableString;

namespace Enterprise.Customs.SG.Access.Business
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), "Bills")]
	public partial class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set
			{
				var oldValue = ABL_BillStatus;
				base.ABL_BillStatus = value;
				if (!IsCopying && oldValue != ABL_BillStatus)
				{
					if (ABL_BillStatus == Common.SG.GlobalManifestStatusList.Codes.Cancelled)
					{
						var packItems = Packs.Cast<AsycudaPack>().Select(x => x.PackedItem);
						if (packItems != null)
						{
							foreach (var packItem in packItems)
							{
								var asyEntryNumbers = packItem.CustomsEntryNumbers.Cast<ASYCUDA.Business.AsycudaPackedItemEntryNum>().Where(x => x.CE_EntryType == ASYCUDA.Business.Constants.CustomsEntryType.ACCESSPermit);
								asyEntryNumbers.DeleteAll();
							}
						}
					}
				}
			}
		}

		public override ZDecimal ABL_FreightValue
		{
			get => base.ABL_FreightValue;
			set
			{
				var oldValue = ABL_FreightValue;
				base.ABL_FreightValue = value;
				if (!IsCopying && oldValue != ABL_FreightValue && !hasSetOwnCustomsValue)
				{
					isSettingCustomsValueViaFreightValue = true;
					ABL_CustomsValue = value;
					if (ABL_RX_NKCustomsValueCurrency != Core.Constants.CurrencyCodes.Singapore)
					{
						ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Singapore;
					}
					isSettingCustomsValueViaFreightValue = false;
				}
			}
		}

		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set
			{
				base.ABL_CustomsValue = value;
				if (!isSettingCustomsValueViaFreightValue)
				{
					hasSetOwnCustomsValue = true;
				}
			}
		}
		bool hasSetOwnCustomsValue;
		bool isSettingCustomsValueViaFreightValue;

		protected override bool ABL_CustomsValue_ReadOnly => true;
		protected override bool ABL_RX_NKCustomsValueCurrency_ReadOnly => true;

		protected override ZString GetAdditionalSelectionDescription(ASYCUDA.Business.AsycudaBill country)
		{
			var desc = ZString.Empty;
			var billSG = (AsycudaBill)country;
			if (billSG.IsImport)
			{
				desc = Invariant($" - Cycle: {billSG.CycleDate.ToShortDateString()}/{billSG.CycleNumber.ToString()}");
			}

			return desc;
		}

		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var oldValue = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && oldValue != ABL_OA_Shipper)
				{
					DefaultSGDataFromShipper(Shipper);
				}
			}
		}

		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee)
				{
					DefaultSG_PartyIDForImportFromOrganisationSG_UEN(Consignee);
					DefaultSG_PartyStatus(Consignee);
					DefaultSG_PayeeIndicatorFromOrganisationPaymentMethod(Consignee);
				}
			}
		}

		public override bool ABL_BillNumber_ReadOnly => !CustomsJobNumber.IsEmpty;

		void DefaultSGDataFromShipper(OrgAddress org)
		{
			if (org != null && IsExport)
			{
				DefaultSG_PartyIDFromOrganisationSG_UEN(org);
			}
		}

		void DefaultSG_PayeeIndicatorFromOrganisationPaymentMethod(OrgAddress org)
		{
			if (org != null && IsImport)
			{
				var paymentMethod = org.GetSGPayeeIndicator();
				if (!paymentMethod.IsEmpty && SG_PayeeIndicator != paymentMethod)
				{
					SG_PayeeIndicator = paymentMethod;
				}
			}
		}

		void DefaultSG_PartyIDForImportFromOrganisationSG_UEN(OrgAddress org)
		{
			if (org != null && IsImport)
			{
				DefaultSG_PartyIDFromOrganisationSG_UEN(org);
			}
		}

		void DefaultSG_PartyStatus(OrgAddress org)
		{
			if (org != null && IsImport)
			{
				var partyStatus = org.GetSGPartyStatusType();
				if (!partyStatus.IsEmpty && SG_PartyStatus != partyStatus)
				{
					SG_PartyStatus = partyStatus;
				}
			}
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Singapore;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);
		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override void CalculateBillApportionmentCalculator() => new SGBillApportionmentCalculator(this).Calculate();

		public override ZString ABL_RX_NKFreightValueCurrency
		{
			get => base.ABL_RX_NKFreightValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKFreightValueCurrency;
				base.ABL_RX_NKFreightValueCurrency = value;
				if (!IsCopying && oldValue != value)
				{
					UpdatePackLinePriceCurrency();
				}
			}
		}

		void UpdatePackLinePriceCurrency()
		{
			foreach (AsycudaPack pack in Packs)
			{
				pack.LinePriceCurrency = ABL_RX_NKFreightValueCurrency;
			}
		}

		protected override bool GetBillShouldBeLocked()
		{
			var result = false;
			var header = Header;
			if (header != null && header.LockedBills)
			{
				var isExport = IsExport; // The determination whether it is export should be changed to based on the ManifestType; it's wrong to base on the value on the BillCountry; will be fixed in another workitemm

				result = Packs.Cast<AsycudaPack>()
					.Any(x =>
					{
						var packedItem = x.PackedItem;
						return packedItem != null && !(isExport && packedItem.API_MessageStatus == ASYCUDA.Business.MessageStatusCodeList.Codes.Updated) && packedItem.HasManifestBeenSubmittedToCustoms;
					});
			}
			return result;
		}

		public bool IsIBGAccountLinked => Factory.GetValue(ref isIBGAccountLinkedCached, () =>
		{
			return IsImport
			&& ((!Consignee?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, Core.Constants.CountryCodes.Singapore).IsEmpty ?? false)
				|| (!Consignee?.Header?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, Core.Constants.CountryCodes.Singapore).IsEmpty ?? false));
		});

		CachedProperty<bool> isIBGAccountLinkedCached;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
	}
}
