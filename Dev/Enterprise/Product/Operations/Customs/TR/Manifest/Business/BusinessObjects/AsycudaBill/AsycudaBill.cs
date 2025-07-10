using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill
		, Integration.Customs.ASYCUDA.TRManifest.IAsycudaBill
		, IVisitedPortParent
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, IAdditionalBusinessObjectFetchStrategyProvider
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string IsToOrder = "IsToOrder";
			public const string NotOwned = "NotOwned";
			public const string PaymentType = "PaymentType";
			public const int PaymentTypeMaxLength = 1;
			public const string RoRo = "RoRo";
			public const string TransshipmentType = "TransshipmentType";
			public const int TransshipmentTypeMaxLength = 1;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Turkey;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);
		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		public new ASYCUDA.Business.IAsycudaTaxCollection<AsycudaTax, AsycudaBill> AsycudaTaxes => (ASYCUDA.Business.IAsycudaTaxCollection<AsycudaTax, AsycudaBill>)base.AsycudaTaxes;
		protected override ASYCUDA.Business.IAsycudaTaxCollection<ASYCUDA.Business.AsycudaTax, ASYCUDA.Business.AsycudaBill> CreateNewAsycudaTaxCollection() => new ASYCUDA.Business.AsycudaTaxCollection<AsycudaTax, AsycudaBill>(this);
		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(CusCodeDataTypeList.Codes.TRVisitedPort, typeof(VisitedPort));
				return result;
			}
		}

		protected override IEnumerable<KeyValuePair<ZString, int>> GetCustomsEntryNumberMaxLengthConfig()
		{
			yield return new KeyValuePair<ZString, int>(CusEntryNumberTypes.Turkey.PRV, 20);
		}

		public override ZString[] ShipperRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.VATCode };
		public override ZString[] ConsigneeRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.VATCode };
		public override ZString[] NotifyPartyRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.VATCode };

		public override ZBool ShipperRegNoReadOnly => Header.IsExport && ShipperUseRealOrg && !ABL_ShipperRegNoType.IsEmpty;
		public override ZBool ConsigneeRegNoReadOnly => Header.IsImport && ConsigneeUseRealOrg && !ABL_ConsigneeRegNoType.IsEmpty || ABL_ConsigneeRegNo_ReadOnly;
		public override ZBool NotifyPartyRegNoReadOnly => Header.IsImport && NotifyPartyUseRealOrg && !ABL_NotifyPartyRegNoType.IsEmpty;

		[MaxLength(9)]
		public override ZString ABL_GoodsLocation
		{
			get => base.ABL_GoodsLocation;
			set
			{
				var oldValue = ABL_GoodsLocation;
				if (oldValue != value)
				{
					base.ABL_GoodsLocation = value;
					UpdateABL_LocationInformation();
				}
			}
		}

		#region New Properties

		[ResourceStringData("TRAsycudaBill.IsToOrder", Caption = "To Order")]
		public ZBool IsToOrder
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.IsToOrder); }
			set
			{
				var oldValue = IsToOrder;
				this.SetSystemDefinedValue(Schema.IsToOrder, value);
				if (!IsCopying && oldValue != IsToOrder)
				{
					if (IsToOrder)
					{
						NotOwned = ZBool.False;
					}
					SetValueToABLConsigneeRegNo();
				}

				ValidateIsToOrderAndNotOwned();
				IsToOrderInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsToOrderInfo => GetZPropertyInfo(Schema.IsToOrder);

		[ResourceStringData("TRAsycudaBill.NotOwned", Caption = "Not Owned")]
		public ZBool NotOwned
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.NotOwned); }
			set
			{
				var oldValue = NotOwned;
				this.SetSystemDefinedValue(Schema.NotOwned, value);
				if (!IsCopying && oldValue != NotOwned)
				{
					if (NotOwned)
					{
						IsToOrder = ZBool.False;
					}

					SetValueToABLConsigneeRegNo();
				}

				ValidateIsToOrderAndNotOwned();
				NotOwnedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo NotOwnedInfo => GetZPropertyInfo(Schema.NotOwned);

		void ValidateIsToOrderAndNotOwned()
		{
			if (!IsValidationSuspended)
			{
				if (Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
				{
					validationForRegularBill.ValidateIsToOrder();
					validationForRegularBill.ValidateNotOwned();
					validationForRegularBill.ValidateABL_ConsigneeRegNo();
					validationForRegularBill.ValidateABL_NotifyPartyRegNo();
				}
			}
		}

		#region Related Declarations For Exports

		[ChildEditable(true)]
		public RelatedDeclarationForExportCollection RelatedDeclarationForExports
		{
			get
			{
				if (relatedDeclarationForExports == null)
				{
					relatedDeclarationForExports = new RelatedDeclarationForExportCollection(this);
					relatedDeclarationForExports.Load();
					RegisterEditableChildObject(relatedDeclarationForExports);
				}
				return relatedDeclarationForExports;
			}
		}

		RelatedDeclarationForExportCollection relatedDeclarationForExports;

		#endregion

		#region Payment Type

		[ResourceStringData("AsycudaBill.PaymentType", Caption = "Payment Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PaymentTypeList))]
		[MaxLength(Schema.PaymentTypeMaxLength)]
		public ZString PaymentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PaymentType);
			set
			{
				var oldValue = PaymentType;
				CheckMaximumLength(PaymentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.PaymentType, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidatePaymentType();
				}
				PaymentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PaymentTypeInfo => GetZPropertyInfo(Schema.PaymentType);

		#endregion

		#region Ro-Ro

		[ResourceStringData("AsycudaBill.RoRo", Caption = "Ro-Ro")]
		public ZBool RoRo
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.RoRo);
			set
			{
				var oldValue = RoRo;
				this.SetSystemDefinedValue(Schema.RoRo, value);
				RoRoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo RoRoInfo => GetZPropertyInfo(Schema.RoRo);

		#endregion

		#region Transshipment Type

		[ResourceStringData("AsycudaPack.TransshipmentType", Caption = "Transshipment Type", ShortCaption = "Transship. Type")]
		[MaxLength(Schema.TransshipmentTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TransshipmentTypeList))]
		public ZString TransshipmentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransshipmentType);
			set
			{
				var oldValue = TransshipmentType;
				CheckMaximumLength(TransshipmentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.TransshipmentType, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateTransshipmentType();
				}
				TransshipmentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TransshipmentTypeInfo => GetZPropertyInfo(Schema.TransshipmentType);
		#endregion

		#endregion

		#region PropertiesForDocWrapper

		public ZString CompanyNameOfAgent => ContainerAgent?.CompanyName ?? ZString.Empty;

		public ZString RegNoOfAgent => ContainerAgent?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, CountryCode) ?? ZString.Empty;

		public ZString ContainerInformation => Packs.Cast<AsycudaPack>().Any(x => x.Container != null) ? Res.GetString("06FA10B8-49D2-43EE-88FF-86867B912662", "E") : Res.GetString("4ED5D416-254E-491E-8075-BAD9B976B131", "H");
		public ZString IsTransshipment => TransshipmentType == TransshipmentTypeList.Codes.TT4 ? Res.GetString("C953FB27-2F98-45BF-9667-055756D7625B", "E") : Res.GetString("20BACD21-B626-4079-A346-2DBEB2E8162E", "H");

		#endregion

		#region ICusSupportingInfoTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> { { RelatedDeclarationForExport.RelatedDeclarationForExportType, typeof(RelatedDeclarationForExport) } };
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region IVisitedPortParent

		[ChildEditable(true)]
		public VisitedPortCollection VisitedPorts
		{
			get
			{
				if (visitedPorts == null)
				{
					visitedPorts = new VisitedPortCollection(this);
					visitedPorts.Load();
					RegisterEditableChildObject(visitedPorts);
				}
				return visitedPorts;
			}
		}

		VisitedPortCollection visitedPorts;

		AsycudaManifestHeader IVisitedPortParent.ManifestHeader
		{
			get { return Header; }
		}

		ZBool IVisitedPortParent.SupportsCustomsPorts
		{
			get
			{
				var header = Header;
				return header?.FeatureProvider?.SupportsCustomsPorts(header) ?? false;
			}
		}

		#endregion

		#region StampDutyValue

		public ZDecimal GlobalManifestStampDutyValue
		{
			get => IsChildMasterBill ? (TaxForGlobalManifestStampDutyValue?.AET_ChargeAmount ?? ZDecimal.Zero) : ZDecimal.Zero;
			set
			{
				if (IsChildMasterBill)
				{
					var oldValue = GlobalManifestStampDutyValue;
					if (oldValue != value)
					{
						var manifestStampDuty = TaxForGlobalManifestStampDutyValue ?? CreateStampDuty(TaxCodeList.Codes.GMS);
						manifestStampDuty.AET_ChargeAmount = value;
						Header.GlobalManifestStampDutyValueInfo.RefreshBinding();
					}
				}
			}
		}
		AsycudaTax TaxForGlobalManifestStampDutyValue
		{
			get
			{
				if (fTaxForGlobalManifestStampDutyValue == null || fTaxForGlobalManifestStampDutyValue.IsDeleted)
				{
					fTaxForGlobalManifestStampDutyValue = LoadStampDuty(TaxCodeList.Codes.GMS);
				}
				return fTaxForGlobalManifestStampDutyValue;
			}
		}
		AsycudaTax fTaxForGlobalManifestStampDutyValue;

		public ZDecimal MasterBillStampDutyValue
		{
			get => IsChildMasterBill ? (TaxForMasterBillStampDutyValue?.AET_ChargeAmount ?? ZDecimal.Zero) : ZDecimal.Zero;
			set
			{
				if (IsChildMasterBill)
				{
					var masterBillStampDuty = TaxForMasterBillStampDutyValue ?? CreateStampDuty(MasterBillStampDutyValueChargeType);
					masterBillStampDuty.AET_ChargeAmount = value;
					Header.MasterBillStampDutyValueInfo.RefreshBinding();
				}
			}
		}

		public void RefreshMasterBillStampDutyValue()
		{
			if (IsChildMasterBill)
			{
				if (TaxForMasterBillStampDutyValue != null)
				{
					if (TaxForMasterBillStampDutyValue.AET_ChargeType != MasterBillStampDutyValueChargeType)
					{
						TaxForMasterBillStampDutyValue.AET_ChargeType = MasterBillStampDutyValueChargeType;
						Header.MasterBillStampDutyValueInfo.RefreshBinding();
					}
				}
			}
		}

		AsycudaTax TaxForMasterBillStampDutyValue
		{
			get
			{
				if (fTaxForMasterBillStampDutyValue == null || fTaxForMasterBillStampDutyValue.IsDeleted)
				{
					fTaxForMasterBillStampDutyValue = LoadStampDuty(MasterBillStampDutyValueChargeType);
				}
				return fTaxForMasterBillStampDutyValue;
			}
		}
		AsycudaTax fTaxForMasterBillStampDutyValue;

		ZString MasterBillStampDutyValueChargeType
		{
			get
			{
				var chargeType = ZString.Empty;

				if (Header.IsAir)
				{
					chargeType = TaxCodeList.Codes.ABS;
				}
				else if (Header.IsSea)
				{
					chargeType = TaxCodeList.Codes.SBS;
				}
				return chargeType;
			}
		}

		AsycudaTax LoadStampDuty(ZString chargeType)
		{
			AsycudaTax result = null;

			if (!chargeType.IsEmpty)
			{
				result = AsycudaTaxes.FirstOrDefault(x => !x.IsDeleted && x.AET_ChargeType == chargeType);
			}

			return result;
		}

		[ResourceStringData("482506D5-E288-4833-9069-33E2DCB3BE0F", Caption = "Stamp Duty", ShortCaption = "Stamp Duty")]
		public ZDecimal BillStampDutyValue
		{
			get => TaxForBillStampDutyValue?.AET_ChargeAmount ?? ZDecimal.Zero;
			set
			{
				var oldValue = BillStampDutyValue;
				if (oldValue != value)
				{
					var billStampDuty = TaxForBillStampDutyValue ?? CreateStampDuty(TaxCodeList.Codes.OBS);
					billStampDuty.AET_ChargeAmount = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation?.ValidateBillStampDutyValue();
					}
					BillStampDutyValueInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo BillStampDutyValueInfo => GetZPropertyInfo(nameof(BillStampDutyValue));

		public void RefreshBillStampDutyValue()
		{
			if (!IsChildMasterBill)
			{
				fTaxForBillStampDutyValue = null;
				fTaxForAirBillStampDutyABSValue = null;

				if (!IsSea && !IsAir)
				{
					AsycudaTaxes.RemoveAndDeleteAll();
				}
				else if (IsSea)
				{
					if (TaxForAirBillStampDutyABSValue != null)
					{
						AsycudaTaxes.RemoveAndDelete(TaxForAirBillStampDutyABSValue);
					}
				}

				Header.TotalStampDutyValueInfo.RefreshBinding();
			}
		}

		AsycudaTax TaxForBillStampDutyValue
		{
			get
			{
				if (fTaxForBillStampDutyValue == null || fTaxForBillStampDutyValue.IsDeleted)
				{
					fTaxForBillStampDutyValue = AsycudaTaxes.FirstOrDefault(x => !x.IsDeleted && x.AET_ChargeType == TaxCodeList.Codes.OBS);
				}
				return fTaxForBillStampDutyValue;
			}
		}
		AsycudaTax fTaxForBillStampDutyValue;

		[ResourceStringData("05057F16-5E7D-437B-8BFF-346A012FA84D", Caption = "Stamp Duty (ABS)", ShortCaption = "Stamp Duty (ABS)")]
		public ZDecimal AirBillStampDutyABSValue
		{
			get => TaxForAirBillStampDutyABSValue?.AET_ChargeAmount ?? ZDecimal.Zero;
			set
			{
				var oldValue = AirBillStampDutyABSValue;
				if (oldValue != value)
				{
					var airBillStampDutyABS = TaxForAirBillStampDutyABSValue ?? CreateStampDuty(TaxCodeList.Codes.ABS);
					airBillStampDutyABS.AET_ChargeAmount = value;
					AirBillStampDutyABSValueInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo AirBillStampDutyABSValueInfo => GetZPropertyInfo(nameof(AirBillStampDutyABSValue));

		AsycudaTax TaxForAirBillStampDutyABSValue
		{
			get
			{
				if (fTaxForAirBillStampDutyABSValue == null || fTaxForAirBillStampDutyABSValue.IsDeleted)
				{
					fTaxForAirBillStampDutyABSValue = AsycudaTaxes.FirstOrDefault(x => !x.IsDeleted && x.AET_ChargeType == TaxCodeList.Codes.ABS);
				}
				return fTaxForAirBillStampDutyABSValue;
			}
		}
		AsycudaTax fTaxForAirBillStampDutyABSValue;

		AsycudaTax CreateStampDuty(ZString chargeType)
		{
			AsycudaTax result;
			using (SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				result = AsycudaTaxes.AddNew();
				result.AET_ChargeType = chargeType;
			}

			return result;
		}

		#endregion

		void SetValueToABLConsigneeRegNo()
		{
			if (IsToOrder)
			{
				ABL_ConsigneeRegNo = ConsigneeRegNoTextEmre;
			}
			else if (NotOwned)
			{
				ABL_ConsigneeRegNo = ConsigneeRegNoTextSahipDegil;
			}
		}

		const string ConsigneeRegNoTextEmre = "EMRE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localised strings")]
		const string ConsigneeRegNoTextSahipDegil = "SAHIP DEGIL";

		bool ABL_ConsigneeRegNo_ReadOnly => (IsToOrder && !NotOwned) || (!IsToOrder && NotOwned);

		void UpdateABL_LocationInformation()
		{
			if (Header.IsAir && Header.AMA_ManifestType == TRManifestTypes.Codes.HAVITH || Header.AMA_ManifestType == TRManifestTypes.Codes.HAVIHR)
			{
				ABL_LocationInformation = ABL_GoodsLocation;
			}
		}

		[ResourceStringData("76F27D84-17B0-4D99-B5C1-3EED05B2DDD7", Caption = "Out of Warehouse")]
		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.YesNoList))]
		public override ZString ABL_SpecialCargoCode { get => base.ABL_SpecialCargoCode; set => base.ABL_SpecialCargoCode = value; }

		public ResourceStringData PreviousDeclarationNoCaption => Res.GetData("D9E4816C-71AE-4145-B2F2-F4C8CE42EBC6", "Previous Declaration No");

		public ResourceStringData BillStampDutyValueCaption => IsAir ? Res.GetData("47E49092-FF5A-43C7-B07F-93A4584B4755", "Stamp Duty (OBS)") : Res.GetData("C3473E7F-B16A-49E0-9C33-FE67BD0D6DEA", "Stamp Duty");
	}
}
