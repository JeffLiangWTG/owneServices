using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using AsycudaArrivalHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader,
		Integration.Customs.ASYCUDA.TWManifest.IAsycudaManifestHeader,
		IMessageManageableBizObj,
		ITWMessageInfoProvider
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string AMA_GoodsLocationFromMasterBill = nameof(AsycudaManifestHeader.AMA_GoodsLocationFromMasterBill);
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Companies))]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader|AMA_LoginCompanyPK", Caption = "Company")]
		public ZGuid AMA_LoginCompanyPK => loginCompanyPK ?? (loginCompanyPK = GlbCompany.CurrentCompany.PK).Value;
		ZGuid? loginCompanyPK;

		public ZPropertyInfo AMA_LoginCompanyPKInfo => GetZPropertyInfo(nameof(AMA_LoginCompanyPK));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader|AMA_MailBox", Caption = "Mail Box")]
		public ZString AMA_MailBox => mailBox ?? (mailBox = ForwarderCredential?.GP_MailBoxID ?? ZString.Empty).Value;
		ZString? mailBox;

		public ZPropertyInfo AMA_MailBoxInfo => GetZPropertyInfo(nameof(AMA_MailBox));

		public GlbCompanyCredential ForwarderCredential
		{
			get
			{
				var currentCompany = GlbCompany.CurrentCompany;
				return currentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan ? GlbCompanyWrapper.GetWrapper<TWGlbCompanyWrapper>(currentCompany)?.ForwarderCertificate : null;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader|AMA_DeconsolidateVAT", Caption = "De-consolidator VAT", ShortCaption = "VAT")]
		public ZString AMA_DeconsolidateVAT => Factory.GetValue(ref deconsolidateVATCached, () => DeconsolidateAddress?.Header?.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode) ?? ZString.Empty);
		CachedProperty<ZString> deconsolidateVATCached;

		public ZPropertyInfo AMA_DeconsolidateVATInfo => GetZPropertyInfo(nameof(AMA_DeconsolidateVAT));

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Taiwan;

		public new AsycudaMessageCollection Messages => (AsycudaMessageCollection)base.Messages;

		protected override EDIMessageCollectionNonDependent CreateNewEDIMessageCollection() => new AsycudaMessageCollection(Factory, this);

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public new AsycudaContainerCollection Containers => (AsycudaContainerCollection)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection(this);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		#region IMessageManageableBizObj

		public bool IsInAStatusAmendmentSendable => false;

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		#endregion

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		[MaxLength(8)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Locations))]
		[ResourceStringData("B5E98634-DC6D-425E-B844-B3034D0583C0", Caption = "Goods Location")]
		public ZString AMA_GoodsLocationFromMasterBill
		{
			get { return MasterBill?.ABL_GoodsLocation ?? ZString.Empty; }
			set
			{
				var oldValue = AMA_GoodsLocationFromMasterBill;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_GoodsLocation = value;
				}
				AMA_GoodsLocationFromMasterBillInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_GoodsLocationFromMasterBillInfo
		{
			get
			{
				var masterBill = MasterBill;
				return masterBill == null ? GetZPropertyInfo(Schema.AMA_GoodsLocationFromMasterBill) : GetWrappedZPropertyInfo(Schema.AMA_GoodsLocationFromMasterBill, x => masterBill.ABL_GoodsLocationInfo);
			}
		}

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		public AsycudaArrivalHeader ArrivalHeader
		{
			get
			{
				if (!IsDeleted && (arrivalHeader == null || arrivalHeader.IsDeleted))
				{
					arrivalHeader = ArrivalHeaders.Cast<AsycudaArrivalHeader>().FirstOrDefault();
					if (arrivalHeader == null)
					{
						arrivalHeader = ArrivalHeaders.AddNew();
					}
					RegisterEditableChildObject(arrivalHeader);
				}
				return arrivalHeader;
			}
		}

		AsycudaArrivalHeader arrivalHeader;

		#region ITWMessageInfoProvider
		ZString ITWMessageInfoProvider.EntryNumber => Bills.OfType<AsycudaBill>().FirstOrDefault()?.CustomsEntryNumber ?? ZString.Empty;

		ZString ITWMessageInfoProvider.EntryNumberType => CusEntryNumberTypes.Taiwan.ForwarderHouseManifest;

		ZString ITWMessageInfoProvider.StaffCode => ZString.Empty;

		ZString ITWMessageInfoProvider.CompanyID => GlbCompany.CurrentCompany.GC_Code;

		ZString ITWMessageInfoProvider.PasswordType => PasswordTypesList.Codes.TVF;
		#endregion

		[MaxLength(6)]
		public override ZString AMA_VehicleRegistration { get => base.AMA_VehicleRegistration; set => base.AMA_VehicleRegistration = value; }

		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set
			{
				var oldValue = AMA_CustomsOffice;
				base.AMA_CustomsOffice = value;
				if (!IsCopying && oldValue != AMA_CustomsOffice)
				{
					Bills.MarkAsNeedingValidation();

					if (AMA_GoodsLocationFromMasterBill.IsEmpty)
					{
						var defaultGoodsLocation = RegistryHelper.GetDefaultGoodsLocation(AMA_CustomsOffice, Common.Shared.SharedJobMessageTypeList.Codes.Import);
						if (!defaultGoodsLocation.IsEmpty)
						{
							AMA_GoodsLocationFromMasterBill = defaultGoodsLocation;
						}
					}
				}
			}
		}

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = AMA_OA_Carrier;
				base.AMA_OA_Carrier = value;
				if (!IsCopying && oldValue != AMA_OA_Carrier)
				{
					UpdateAMA_CarrierCodeOnAMA_OA_CarrierChanged();
				}
			}
		}

		void UpdateAMA_CarrierCodeOnAMA_OA_CarrierChanged()
		{
			var newCarrierCode = IsSea ? CarrierCCCCode : Carrier?.Header?.MiscServ?.Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? ZString.Empty;
			if (newCarrierCode != AMA_CarrierCode)
			{
				AMA_CarrierCode = newCarrierCode;
			}
		}

		const int AMA_CarrierCodeMaxLength = 14;

		[MaxLength(AMA_CarrierCodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader|AMA_CarrierCode", Caption = "Carrier ID")]
		public override ZString AMA_CarrierCode
		{
			get => base.AMA_CarrierCode;
			set => base.AMA_CarrierCode = value.Left(AMA_CarrierCodeMaxLength);
		}

		protected override ZBool IsDeconsolidatorEnabledCore => true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_CustomsOffice = RegistryHelper.DefaultCustomsOfficeCode;

			var proxyAddressPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			if (proxyAddressPK.IsEmpty)
			{
				proxyAddressPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			}
			AMA_OA_DeconsolidateAddress_ZAddress.OrgPK = proxyAddressPK;
			AMA_TransportMode = Core.Constants.TransportModes.Air;
		}

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(Freight.Forwarding.Business.ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(this);
			TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(this);
		}
	}
}
