using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.TWManifest.IAsycudaBill, ITariffFormatProvider
	{
		const string BagNumberEntryNumCategory = "CUS";

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string GoodsDescription = "GoodsDescription";
			public const string BagNumber = "BagNumber";
			public const string EntryNumber = "EntryNumber";
			public const string ABL_SplitQuantity = "ABL_SplitQuantity";
			public const string ABL_SplitQuantityUQ = "ABL_SplitQuantityUQ";
			public const string ABL_Tariff = "ABL_Tariff";
		}

		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		[ResourceStringData("A580741C-3856-42CE-B56F-C784FB7BC4A1", Caption = "Port of Loading")]
		public override ZString ABL_RL_NKPortOfLoading
		{
			get => base.ABL_RL_NKPortOfLoading;
			set
			{
				var oldValue = ABL_RL_NKPortOfLoading;
				base.ABL_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != ABL_RL_NKPortOfLoading)
				{
					if (ABL_LocationInformation_ReadOnly && !ABL_LocationInformation.IsEmpty)
					{
						ABL_LocationInformationInfo.ClearValue();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ABL_LocationInformation_ReadOnly))]
		[MaxLength(70)]
		[ResourceStringData("6E5D62F3-A9F1-4150-A46A-A55EDB269A52", Caption = "Port of Loading Description (Z99)", ShortCaption = "Z99 Description")]
		public override ZString ABL_LocationInformation { get => base.ABL_LocationInformation; set => base.ABL_LocationInformation = value; }

		public bool ABL_LocationInformation_ReadOnly => !IsZ99PortCode(ABL_RL_NKPortOfLoading);

		[MaxLength(8)]
		[ResourceStringData("6542986F-B70A-4F4A-9E18-504E16D63627", Caption = "Goods Location")]
		public override ZString ABL_GoodsLocation { get => base.ABL_GoodsLocation; set => base.ABL_GoodsLocation = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TWShipmentTypes))]
		[ResourceStringData("A5705890-BD43-422A-B40D-E1CC5BDF62E8", Caption = "Type")]
		public override ZString ABL_ShipmentType { get => base.ABL_ShipmentType; set => base.ABL_ShipmentType = value; }

		[ResourceStringData("82D0A245-0529-4818-910D-3F2357F6A032", Caption = "UCR")]
		public override ZString ABL_UCRNumber { get => base.ABL_UCRNumber; set => base.ABL_UCRNumber = value; }

		[MaxLength(35)]
		[ResourceStringData("0E34E731-A3AF-41FC-9692-2B9C43F5E7E7", Caption = "Package Description")]
		public override ZString ABL_Remarks { get => base.ABL_Remarks; set => base.ABL_Remarks = value; }

		[ResourceStringData("BF41A7A4-7785-4093-893F-4D82C42E322E", Caption = "Entry Number", ShortCaption = "Entry #")]
		public override ZString CustomsEntryNumber { get => base.CustomsEntryNumber; set => base.CustomsEntryNumber = value; }

		protected override bool CustomsEntryNumber_ReadOnly => true;

		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var oldValue = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && oldValue != ABL_OA_Shipper)
				{
					ClearValueIfNeeded(!ABL_OA_Shipper.IsEmpty, ShipperLocalAddressInfos);
					DefaultLocalAddressDetails(Shipper, ManifestBase.AsycudaBillAddress.AddressType.Shipper);
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
					ClearValueIfNeeded(!ABL_OA_Consignee.IsEmpty, ConsigneeLocalAddressInfos);
					DefaultLocalAddressDetails(Consignee, ManifestBase.AsycudaBillAddress.AddressType.Consignee);
				}
			}
		}

		public override ZGuid ABL_OA_NotifyParty
		{
			get => base.ABL_OA_NotifyParty;
			set
			{
				var oldValue = ABL_OA_NotifyParty;
				base.ABL_OA_NotifyParty = value;
				if (!IsCopying && oldValue != ABL_OA_NotifyParty)
				{
					ClearValueIfNeeded(!ABL_OA_NotifyParty.IsEmpty, NotifyPartyLocalAddressInfos);
					DefaultLocalAddressDetails(NotifyParty, ManifestBase.AsycudaBillAddress.AddressType.NotifyParty);
				}
			}
		}

		void ClearValueIfNeeded(bool shouldClear, IEnumerable<ZPropertyInfo> infos)
		{
			if (shouldClear)
			{
				infos.ForEach(x => x.ClearValue());
			}
		}

		IEnumerable<ZPropertyInfo> ShipperLocalAddressInfos
		{
			get
			{
				yield return ABL_ShipperLocalNameInfo;
				yield return ABL_ShipperLocalStreet1Info;
				yield return ABL_ShipperLocalStreet2Info;
				yield return ABL_ShipperLocalCityInfo;
				yield return ABL_ShipperLocalStateInfo;
			}
		}

		IEnumerable<ZPropertyInfo> ConsigneeLocalAddressInfos
		{
			get
			{
				yield return ABL_ConsigneeLocalNameInfo;
				yield return ABL_ConsigneeLocalStreet1Info;
				yield return ABL_ConsigneeLocalStreet2Info;
				yield return ABL_ConsigneeLocalCityInfo;
				yield return ABL_ConsigneeLocalStateInfo;
			}
		}

		IEnumerable<ZPropertyInfo> NotifyPartyLocalAddressInfos
		{
			get
			{
				yield return ABL_NotifyPartyLocalNameInfo;
				yield return ABL_NotifyPartyLocalStreet1Info;
				yield return ABL_NotifyPartyLocalStreet2Info;
				yield return ABL_NotifyPartyLocalCityInfo;
				yield return ABL_NotifyPartyLocalStateInfo;
			}
		}

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperLocalName { get => base.ABL_ShipperLocalName; set => base.ABL_ShipperLocalName = value; }

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperLocalStreet1 { get => base.ABL_ShipperLocalStreet1; set => base.ABL_ShipperLocalStreet1 = value; }

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperLocalStreet2 { get => base.ABL_ShipperLocalStreet2; set => base.ABL_ShipperLocalStreet2 = value; }

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperLocalCity { get => base.ABL_ShipperLocalCity; set => base.ABL_ShipperLocalCity = value; }

		[ReadOnlyMember(nameof(ShipperUseRealOrg))]
		public override ZString ABL_ShipperLocalState { get => base.ABL_ShipperLocalState; set => base.ABL_ShipperLocalState = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeLocalName { get => base.ABL_ConsigneeLocalName; set => base.ABL_ConsigneeLocalName = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeLocalStreet1 { get => base.ABL_ConsigneeLocalStreet1; set => base.ABL_ConsigneeLocalStreet1 = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeLocalStreet2 { get => base.ABL_ConsigneeLocalStreet2; set => base.ABL_ConsigneeLocalStreet2 = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeLocalCity { get => base.ABL_ConsigneeLocalCity; set => base.ABL_ConsigneeLocalCity = value; }

		[ReadOnlyMember(nameof(ConsigneeUseRealOrg))]
		public override ZString ABL_ConsigneeLocalState { get => base.ABL_ConsigneeLocalState; set => base.ABL_ConsigneeLocalState = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyLocalName { get => base.ABL_NotifyPartyLocalName; set => base.ABL_NotifyPartyLocalName = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyLocalStreet1 { get => base.ABL_NotifyPartyLocalStreet1; set => base.ABL_NotifyPartyLocalStreet1 = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyLocalStreet2 { get => base.ABL_NotifyPartyLocalStreet2; set => base.ABL_NotifyPartyLocalStreet2 = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyLocalCity { get => base.ABL_NotifyPartyLocalCity; set => base.ABL_NotifyPartyLocalCity = value; }

		[ReadOnlyMember(nameof(NotifyPartyUseRealOrg))]
		public override ZString ABL_NotifyPartyLocalState { get => base.ABL_NotifyPartyLocalState; set => base.ABL_NotifyPartyLocalState = value; }

		[MaxLength(8)]
		public override ZInt ABL_ManifestQty
		{
			get { return base.ABL_ManifestQty; }
			set
			{
				if (value > 0)
				{
					base.ABL_ManifestQty = value;
				}
				else
				{
					base.ABL_ManifestQty = ZInt.Zero;
				}
				ABL_ManifestQtyInfo.RefreshBinding();
			}
		}

		public override ZDecimal ABL_GrossWeight
		{
			get { return base.ABL_GrossWeight; }
			set
			{
				if (value > 0)
				{
					base.ABL_GrossWeight = value;
				}
				else
				{
					base.ABL_GrossWeight = ZDecimal.Zero;
				}
				ABL_ManifestQtyInfo.RefreshBinding();
			}
		}

		public override ZDecimal ABL_Volume
		{
			get { return base.ABL_Volume; }
			set
			{
				if (value > 0)
				{
					base.ABL_Volume = value;
				}
				else
				{
					base.ABL_Volume = ZDecimal.Zero;
				}
				ABL_VolumeInfo.RefreshBinding();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!IsChildMasterBill)
			{
				ABL_GrossWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
				ABL_VolumeUQ = Enterprise.Core.Constants.Volume.CubicMetres;
				ABL_ShipmentType = TWManifestShipmentTypes.Codes.Import;
			}
		}

		void DefaultLocalAddressDetails(OrgAddress address, ManifestBase.AsycudaBillAddress.AddressType type)
		{
			if (address != null)
			{
				var localAddress = address.GetTranslatedAddressInSpecificLanguage(Enterprise.Core.SharedConstants.Languages.ChineseTraditional);
				switch (type)
				{
					case ManifestBase.AsycudaBillAddress.AddressType.Shipper:
						SetLocalAddressDefaultValues(localAddress, ABL_ShipperLocalNameInfo, ABL_ShipperLocalStreet1Info, ABL_ShipperLocalStreet2Info, ABL_ShipperLocalCityInfo, ABL_ShipperLocalStateInfo);
						break;
					case ManifestBase.AsycudaBillAddress.AddressType.Consignee:
						SetLocalAddressDefaultValues(localAddress, ABL_ConsigneeLocalNameInfo, ABL_ConsigneeLocalStreet1Info, ABL_ConsigneeLocalStreet2Info, ABL_ConsigneeLocalCityInfo, ABL_ConsigneeLocalStateInfo);
						break;
					case ManifestBase.AsycudaBillAddress.AddressType.NotifyParty:
						SetLocalAddressDefaultValues(localAddress, ABL_NotifyPartyLocalNameInfo, ABL_NotifyPartyLocalStreet1Info, ABL_NotifyPartyLocalStreet2Info, ABL_NotifyPartyLocalCityInfo, ABL_NotifyPartyLocalStateInfo);
						break;
				}
			}
		}

		void SetLocalAddressDefaultValues(OrgTranslatedAddress localAddress, ZPropertyInfo nameInfo, ZPropertyInfo street1Info, ZPropertyInfo street2Info, ZPropertyInfo cityInfo, ZPropertyInfo stateInfo)
		{
			if (localAddress != null)
			{
				nameInfo.Value = localAddress.CompanyName.Left(nameInfo.MaxLength);
				street1Info.Value = localAddress.Address1;
				street2Info.Value = localAddress.Address2;
				cityInfo.Value = localAddress.City;
				stateInfo.Value = localAddress.StateCode;
			}
		}

		public override ZString[] ShipperRegNoTypes() => CommonRegNoTypes;

		public override ZString[] ConsigneeRegNoTypes() => CommonRegNoTypes;

		public override ZString[] NotifyPartyRegNoTypes() => CommonRegNoTypes;

		ZString[] CommonRegNoTypes => new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID };

		[BusinessObjectTestExclude]
		[MaxLength(35)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TariffCollection))]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaBill|ABL_Tariff", Caption = "HS Code")]
		public ZString ABL_Tariff
		{
			get => GetPack()?.PackedItem.API_FormattedTariff ?? ZString.Empty;
			set
			{
				if (ABL_Tariff != value)
				{
					CheckMaximumLength(ABL_TariffInfo, value);
					Pack.PackedItem.API_Tariff = value;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateABL_Tariff();
					}
					ABL_TariffInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ABL_TariffInfo => GetZPropertyInfo(Schema.ABL_Tariff);

		public TariffView UniversalTariff
		{
			get
			{
				var tariff = ((ITariffFormatProvider)this).TariffFormatter.Format(ABL_Tariff);
				return !tariff.IsEmpty ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem, tariff, ZDateTime.Today) : null;
			}
		}

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.UNDGSubstances))]
		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaBill|ABL_DG_UNNO", Caption = "DG Code")]
		public ZString ABL_DG_UNNO
		{
			get
			{
				return GetPack()?.UNDGs.UNDGSubstanceManager.Value ?? ZString.Empty;
			}
			set
			{
				if (ABL_DG_UNNO != value)
				{
					CheckMaximumLength(ABL_DG_UNNOInfo, value);
					Pack.UNDGs.UNDGSubstanceManager.Value = value;
					ABL_DG_UNNOInfo.RefreshBinding();
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateABL_DG_UNNO();
					}
				}
			}
		}

		public ZPropertyInfo ABL_DG_UNNOInfo => GetZPropertyInfo(nameof(ABL_DG_UNNO));

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaBill|ABL_RL_NKFinalDestination", Caption = "Final Destination")]
		public override ZString ABL_RL_NKFinalDestination { get => base.ABL_RL_NKFinalDestination; set => base.ABL_RL_NKFinalDestination = value; }

		[ResourceStringData("Enterprise.Customs.TW.Manifest.Business.AsycudaBill|ABL_SpecialCargoCode", Caption = "Is Escort Required?")]
		public ZBool IsEscortRequired
		{
			get => ABL_SpecialCargoCode == YesNoList.Codes.Yes;
			set
			{
				if (IsEscortRequired != value)
				{
					ABL_SpecialCargoCode = value ? YesNoList.Codes.Yes : string.Empty;
					IsEscortRequiredInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsEscortRequiredInfo => GetZPropertyInfo(nameof(IsEscortRequired));

		[MaxLength(256)]
		[ResourceStringData("EA3C743A-F8D8-49C3-956B-8D9B269BECD8", Caption = "Goods Description")]
		public ZString GoodsDescription
		{
			get => GetPack()?.PackedItem.API_GoodsDescription ?? ZString.Empty;
			set
			{
				if (GoodsDescription != value)
				{
					CheckMaximumLength(GoodsDescriptionInfo, value);
					Pack.PackedItem.API_GoodsDescription = value;
					GoodsDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo GoodsDescriptionInfo
		{
			get
			{
				var pack = GetPack();
				return pack == null ? GetZPropertyInfo(nameof(GoodsDescription)) : GetWrappedZPropertyInfo(nameof(GoodsDescription), x => pack.PackedItem.API_GoodsDescriptionInfo);
			}
		}

		public bool IsZ99PortCode(ZString portCode)
		{
			return portCode.Length == 5 && portCode.EndsWith(TW.Business.Constants.Z99, StringComparison.OrdinalIgnoreCase);
		}

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override Type GetPackTypeCore()
		{
			return typeof(AsycudaPack);
		}

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		internal AsycudaPack GetPack() => Packs.Cast<AsycudaPack>().FirstOrDefault();

		AsycudaPack Pack
		{
			get
			{
				if (!IsDeleted && (pack == null || pack.IsDeleted))
				{
					if (pack != null)
					{
						UnRegisterEditableChildObject(pack);
						pack = null;
					}
					if (!IsDeleting)
					{
						LoadPack();
					}
				}
				return pack;
			}
		}
		AsycudaPack pack;

		void LoadPack()
		{
			pack = GetPack();
			if (pack == null)
			{
				pack = Packs.AddNew();
			}
			RegisterEditableChildObject(pack);
		}

		ASYCUDA.Business.AsycudaArrivalLine ArrivalLine
		{
			get
			{
				if (!IsDeleting && !IsDeleted && (fArrivalLine == null || fArrivalLine.IsDeleted))
				{
					fArrivalLine = GetArrivalLine();
					if (Header != null && fArrivalLine == null)
					{
						fArrivalLine = Header.ArrivalHeader.ArrivalDetails.AddNew();
						fArrivalLine.ATL_ABL_AsycudaBill = PK;
					}
					RegisterEditableChildObject(fArrivalLine);
				}
				return fArrivalLine;
			}
		}
		ASYCUDA.Business.AsycudaArrivalLine fArrivalLine;

		[MaxLength(8)]
		[ResourceStringData("EF3088D2-B086-4792-A044-836125212ABA", Caption = "Split Quantity")]
		public ZInt ABL_SplitQuantity
		{
			get => GetArrivalLine()?.ATL_Quantity ?? ZInt.Zero;
			set
			{
				if (ABL_SplitQuantity != value && ArrivalLine != null)
				{
					if (value > 0)
					{
						ArrivalLine.ATL_Quantity = value;
						if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
						{
							validationForRegularBill.ValidateABL_SplitQuantity();
						}
					}
					else
					{
						ArrivalLine.ATL_Quantity = ZInt.Zero;
					}
					ABL_SplitQuantityInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ABL_SplitQuantityInfo => GetZPropertyInfo(nameof(ABL_SplitQuantity));

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PackageTypeList))]
		[ResourceStringData("9D52D0CE-BE2B-4B05-BC3F-BE7BD4793CD7", Caption = "UQ")]
		public ZString ABL_SplitQuantityUQ => ABL_ManifestUQ;

		#region AsycudaBillLinkAsycudaContainer

		public AsycudaBillLinkAsycudaContainerCollection AsycudaBillLinkAsycudaContainers
		{
			get
			{
				if (asycudaBillLinkAsycudaContainers == null)
				{
					asycudaBillLinkAsycudaContainers = new AsycudaBillLinkAsycudaContainerCollection(this);
					asycudaBillLinkAsycudaContainers.Load();
				}
				return asycudaBillLinkAsycudaContainers;
			}
		}
		AsycudaBillLinkAsycudaContainerCollection asycudaBillLinkAsycudaContainers;

		public bool IsAsycudaBillLinkAsycudaContainersLoaded => asycudaBillLinkAsycudaContainers != null;

		[ChildEditable]
		public AsycudaContainerBillLinkCollection<ASYCUDA.Business.AsycudaContainerBillOrPackageLink, AsycudaBill> BillLinkContainerDivots
		{
			get
			{
				if (billLinkContainerDivots == null)
				{
					billLinkContainerDivots = new AsycudaContainerBillLinkCollection<ASYCUDA.Business.AsycudaContainerBillOrPackageLink, AsycudaBill>(this);
					billLinkContainerDivots.Load();
					RegisterEditableChildObject(billLinkContainerDivots);
				}
				return billLinkContainerDivots;
			}
		}

		AsycudaContainerBillLinkCollection<ASYCUDA.Business.AsycudaContainerBillOrPackageLink, AsycudaBill> billLinkContainerDivots;

		public void LinkContainer(ZGuid containerPK)
		{
			var divot = GetDivot(containerPK);
			if (divot == null)
			{
				divot = BillLinkContainerDivots.AddNew();
				divot.APC_ACN_Container = containerPK;
				divot.APC_ABL_Bill = PK;
				divot.APC_ClusterKey = ABL_ClusterKey;
			}
		}

		public void UnlinkContainer(ZGuid containerPK)
		{
			var divot = GetDivot(containerPK);
			if (divot != null)
			{
				BillLinkContainerDivots.RemoveAndDelete(divot);
			}
		}

		internal ASYCUDA.Business.AsycudaContainerBillOrPackageLink GetDivot(ZGuid containerPK) => BillLinkContainerDivots.Cast<ASYCUDA.Business.AsycudaContainerBillOrPackageLink>().SingleOrDefault(x => x.APC_ACN_Container == containerPK);

		protected override Type GetPackageContainerLinkTypeCore()
		{
			return typeof(ASYCUDA.Business.AsycudaContainerBillOrPackageLink);
		}

		#endregion

		ASYCUDA.Business.AsycudaArrivalLine GetArrivalLine() => Factory.LoadTop1<ASYCUDA.Business.AsycudaArrivalLine>(DataHelper.GenerateClusterKeyQuery(ABL_ClusterKey, PK, AsycudaArrivalLineSchema.ATL_ClusterKey, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, !IsInDatabase));

		[MaxLength(16)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BagNumberList))]
		[ResourceStringData("18A00DAD-0253-4316-949F-B8EC1C8F9517", Caption = "Bag Number")]
		public ZString BagNumber
		{
			get => GetABLEntryNum()?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				if (BagNumber != value)
				{
					CheckMaximumLength(BagNumberInfo, value);
					ABLEntryNum.CE_EntryLineReference = value;
					BagNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BagNumberInfo
		{
			get
			{
				var entryNum = GetABLEntryNum();
				return entryNum == null ? GetZPropertyInfo(nameof(BagNumber)) : GetWrappedZPropertyInfo(nameof(BagNumber), x => entryNum.CE_EntryLineReferenceInfo);
			}
		}

		protected override bool ABL_BillStatus_ReadOnly => true;

		[MaxLength(20)]
		public ZString EntryNumber
		{
			get => GetABLEntryNum()?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (EntryNumber != value)
				{
					CheckMaximumLength(EntryNumberInfo, value);
					ABLEntryNum.CE_EntryNum = value;
					EntryNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EntryNumberInfo
		{
			get
			{
				var cusEntryNumber = GetABLEntryNum();
				return cusEntryNumber == null ? GetZPropertyInfo(nameof(EntryNumber)) : GetWrappedZPropertyInfo(nameof(EntryNumber), x => cusEntryNumber.CE_EntryNumInfo);
			}
		}

		ASYCUDA.Business.ABLEntryNum ABLEntryNum
		{
			get
			{
				if (!IsDeleted && (aBL_EntryNum == null || aBL_EntryNum.IsDeleted))
				{
					aBL_EntryNum = GetABLEntryNum();
					if (aBL_EntryNum == null)
					{
						aBL_EntryNum = CustomsEntryNumbers.AddNew();
						aBL_EntryNum.CE_Category = BagNumberEntryNumCategory;
						aBL_EntryNum.CE_EntryType = MessageTypeList.Codes.FHM;
					}
				}
				return aBL_EntryNum;
			}
		}
		ASYCUDA.Business.ABLEntryNum aBL_EntryNum;

		ASYCUDA.Business.ABLEntryNum GetABLEntryNum()
		{
			return CustomsEntryNumbers.OfType<ASYCUDA.Business.ABLEntryNum>()
						.Where(x => x.CE_EntryType == MessageTypeList.Codes.FHM && x.CE_Category == BagNumberEntryNumCategory)
						.OrderBy(x => x.CE_SystemCreateTimeUtc).FirstOrDefault();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				GetArrivalLine()?.Delete();
			}
			base.Delete();
		}
		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill)
		{
			// do not need a default value.
		}

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => new TaiwanTariffFormatter();

		protected override ManifestBase.AsycudaBillLookups GetNewLookups()
		{
			return new AsycudaBillLookups(this);
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
	}
}
