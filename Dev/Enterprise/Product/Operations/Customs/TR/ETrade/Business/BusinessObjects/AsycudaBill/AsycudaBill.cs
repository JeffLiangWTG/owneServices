using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.Business
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public partial class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.TRETrade.IAsycudaBill, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string NatureOfBusiness = "NatureOfBusiness";
			public const int NatureOfBusinessMaxLength = 2;
			public const string ExemptionCode1 = "ExemptionCode1";
			public const int ExemptionCode1MaxLength = 10;
			public const string ExemptionCode2 = "ExemptionCode2";
			public const int ExemptionCode2MaxLength = 10;
			public const string GuaranteeType = "GuaranteeType";
			public const int GuaranteeTypeMaxLength = 8;
			public const string GuaranteeRefNo = "GuaranteeRefNo";
			public const int GuaranteeRefNoMaxLength = 15;
			public const string GuaranteeAmount = "GuaranteeAmount";
			public const string DepartureCountry = "DepartureCountry";
			public const int DepartureCountryMaxLength = 2;
			public const string TradeCountry = "TradeCountry";
			public const int TradeCountryMaxLength = 2;
			public const string ExportCountry = "ExportCountry";
			public const int ExportCountryMaxLength = 2;
			public const string ArrivalCountry = "ArrivalCountry";
			public const int ArrivalCountryMaxLength = 2;
			public new const int ABL_RL_NKOriginMaxLength = 2;
			public const string PaymentMethod = "PaymentMethod";
			public const int PaymentMethodMaxLength = 2;
			public const string AccountantName = "AccountantName";
			public const int AccountantNameMaxLength = 50;
			public const string AccountantVAT = "AccountantVAT";
			public const int AccountantVATMaxLength = 11;
			public new const int ABL_RX_NKOtherValueCurrencyMaxLength = 3;
			public const string SupplementaryDeclarationName = "SupplementaryDeclarationName";
			public const int SupplementaryDeclarationNameMaxLength = 100;
			public const string SupplementaryDeclarationRegNoIdNo = "SupplementaryDeclarationRegNoIdNo";
			public const int SupplementaryDeclarationRegNoIdNoMaxLength = 11;
			public const string SupplementaryDeclarationDeliveryDate = "SupplementaryDeclarationDeliveryDate";
			public new const int ABL_ProcedureMaxLength = 4;
			public const string ContainerNumber = "ContainerNumber";
			public const int ContainerNumberMaxLength = 20;
			public const string Separated = "Separated";
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Turkey;
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		#region Properties
		[ResourceStringData("AsycudaBill|ContainerNumber", Caption = "Container No")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Containers))]
		[MaxLength(Schema.ContainerNumberMaxLength)]
		public ZString ContainerNumber
		{
			get
			{
				return Pivot?.Container?.ACN_ContainerNumber ?? ZString.Empty;
			}
			set
			{
				var newValue = value.Trim();

				var existPivot = Pivot;
				var existContainer = existPivot?.Container;
				var oldValue = existContainer?.ACN_ContainerNumber ?? ZString.Empty;

				var shouldDeleteOldConatiner = false;
				if (oldValue != newValue && existContainer != null)
				{
					shouldDeleteOldConatiner = !IsThisContainerLinkedToOtherBill(existContainer.PK);
				}

				if (newValue.IsEmpty)
				{
					existPivot?.Delete();
					if (shouldDeleteOldConatiner)
					{
						existContainer?.Delete();
					}
				}
				else
				{
					CheckMaximumLength(ContainerNumberInfo, newValue);

					if (existPivot == null)
					{
						existPivot = (ASYCUDA.Business.AsycudaContainerBillOrPackageLink)Factory.New(typeof(ASYCUDA.Business.AsycudaContainerBillOrPackageLink));
						existPivot.APC_ABL_Bill = PK;
						existPivot.APC_ClusterKey = ABL_ClusterKey;
					}

					var existingContainer = Header.Containers.Cast<AsycudaContainer>().FirstOrDefault(x => x.ACN_ContainerNumber == newValue && x.ContainerLevel == BillContainerCode);
					if (existingContainer != null)
					{
						existPivot.APC_ACN_Container = existingContainer.PK;
					}
					else
					{
						var newContainer = Header.Containers.AddNew();
						newContainer.ACN_ContainerNumber = newValue;
						newContainer.ContainerLevel = BillContainerCode;

						existPivot.APC_ACN_Container = newContainer.PK;
					}

					if (shouldDeleteOldConatiner)
					{
						existContainer?.Delete();
					}

					RegisterEditableChildObject(existPivot);
				}
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateContainerNumber();
				}
				ContainerNumberInfo.RefreshBinding(oldValue);
			}
		}

		bool IsThisContainerLinkedToOtherBill(ZGuid containerPK)
		{
			var result = false;
			if (containerPK.IsValid)
			{
				var pivotCountQuery = new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, containerPK);
				pivotCountQuery.AddToFilter(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, SQLComparisonOperator.NotEqual, this.PK);
				result = Factory.LoadTop1<ManifestBase.AsycudaContainerBillOrPackageLink>(pivotCountQuery) != null;
			}
			return result;
		}

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(Schema.ContainerNumber);

		public const string BillContainerCode = "B";

		public override ZString ABL_GoodsLocation
		{
			get => base.ABL_GoodsLocation;
			set
			{
				var oldValue = base.ABL_GoodsLocation;
				base.ABL_GoodsLocation = value;
				if (!IsCopying && oldValue != ABL_GoodsLocation && IsChildMasterBill)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString ABL_LocationInformation
		{
			get => base.ABL_LocationInformation;
			set
			{
				var oldValue = base.ABL_LocationInformation;
				base.ABL_LocationInformation = value;
				if (!IsCopying && oldValue != ABL_LocationInformation && IsChildMasterBill)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString ABL_GoodsDescription
		{
			get => base.ABL_GoodsDescription;
			set
			{
				base.ABL_GoodsDescription = value;
				if (this.IsChildMasterBill)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("AsycudaBill.NatureOfBusiness", Caption = "Nature Of Business")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.NatureOfBusinessList))]
		[MaxLength(Schema.NatureOfBusinessMaxLength)]
		public ZString NatureOfBusiness
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.NatureOfBusiness);
			set
			{
				var oldValue = NatureOfBusiness;
				CheckMaximumLength(NatureOfBusinessInfo, value);
				this.SetSystemDefinedValue(Schema.NatureOfBusiness, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateNatureOfBusiness();
				}
				NatureOfBusinessInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo NatureOfBusinessInfo => GetZPropertyInfo(Schema.NatureOfBusiness);

		[ResourceStringData("AsycudaBill.ExemptionCode1", Caption = "Exemption Code 1")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ExemptionCodeList))]
		[MaxLength(Schema.ExemptionCode1MaxLength)]
		public ZString ExemptionCode1
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ExemptionCode1);
			set
			{
				var oldValue = ExemptionCode1;
				CheckMaximumLength(ExemptionCode1Info, value);
				this.SetSystemDefinedValue(Schema.ExemptionCode1, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateExemptionCode1();
					RegularBillValidation.ValidateExemptionCode2();
				}
				ExemptionCode1Info.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ExemptionCode1Info => GetZPropertyInfo(Schema.ExemptionCode1);

		[ResourceStringData("AsycudaBill.ExemptionCode2", Caption = "Exemption Code 2")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ExemptionCodeList))]
		[MaxLength(Schema.ExemptionCode2MaxLength)]
		public ZString ExemptionCode2
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ExemptionCode2);
			set
			{
				var oldValue = ExemptionCode2;
				CheckMaximumLength(ExemptionCode2Info, value);
				this.SetSystemDefinedValue(Schema.ExemptionCode2, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateExemptionCode1();
					RegularBillValidation.ValidateExemptionCode2();
				}
				ExemptionCode2Info.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ExemptionCode2Info => GetZPropertyInfo(Schema.ExemptionCode2);

		[ResourceStringData("AsycudaBill.DepartureCountry", Caption = "Departure Country")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CountryList))]
		[MaxLength(Schema.DepartureCountryMaxLength)]
		public ZString DepartureCountry
		{
			get => GetEffectiveValueToReturn(this.GetSystemDefinedValue<ZString>(Schema.DepartureCountry), Header.DepartureCountryCode);
			set
			{
				var oldValue = DepartureCountry;
				CheckMaximumLength(DepartureCountryInfo, value);
				this.SetSystemDefinedValue(Schema.DepartureCountry, GetEffectiveValueToSet(value, Header.DepartureCountryCode));
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateDepartureCountry();
				}
				DepartureCountryInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo DepartureCountryInfo => GetZPropertyInfo(Schema.DepartureCountry);

		[ResourceStringData("AsycudaBill.TradeCountry", Caption = "Trade Country")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CountryList))]
		[MaxLength(Schema.TradeCountryMaxLength)]
		public ZString TradeCountry
		{
			get => GetEffectiveValueToReturn(this.GetSystemDefinedValue<ZString>(Schema.TradeCountry), Header.DepartureCountryCode);
			set
			{
				var oldValue = TradeCountry;
				CheckMaximumLength(TradeCountryInfo, value);
				this.SetSystemDefinedValue(Schema.TradeCountry, GetEffectiveValueToSet(value, Header.DepartureCountryCode));
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateTradeCountry();
				}
				TradeCountryInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo TradeCountryInfo => GetZPropertyInfo(Schema.TradeCountry);

		[ResourceStringData("AsycudaBill.ExportCountry", Caption = "Export Country")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CountryList))]
		[MaxLength(Schema.ExportCountryMaxLength)]
		public ZString ExportCountry
		{
			get => GetEffectiveValueToReturn(this.GetSystemDefinedValue<ZString>(Schema.ExportCountry), Header.DepartureCountryCode);
			set
			{
				var oldValue = ExportCountry;
				CheckMaximumLength(ExportCountryInfo, value);
				this.SetSystemDefinedValue(Schema.ExportCountry, GetEffectiveValueToSet(value, Header.DepartureCountryCode));
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateExportCountry();
				}
				ExportCountryInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ExportCountryInfo => GetZPropertyInfo(Schema.ExportCountry);

		[BusinessObjectTestExclude]
		[ResourceStringData("AsycudaBill.ArrivalCountry", Caption = "Arrival Country")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CountryList))]
		[MaxLength(Schema.ArrivalCountryMaxLength)]
		public ZString ArrivalCountry
		{
			get => GetEffectiveValueToReturn(this.GetSystemDefinedValue<ZString>(Schema.ArrivalCountry), Header.AMA_RL_NKPortOfFirstArrival.SubstringSafe(0, Schema.ArrivalCountryMaxLength));
			set
			{
				var oldValue = ArrivalCountry;
				CheckMaximumLength(ArrivalCountryInfo, value);
				this.SetSystemDefinedValue(Schema.ArrivalCountry, GetEffectiveValueToSet(value, Header.AMA_RL_NKPortOfFirstArrival.SubstringSafe(0, Schema.ArrivalCountryMaxLength)));
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateArrivalCountry();
				}
				ArrivalCountryInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ArrivalCountryInfo => GetZPropertyInfo(Schema.ArrivalCountry);

		[ResourceStringData("AsycudaBill.PaymentMethod", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PaymentMethodList))]
		[MaxLength(Schema.PaymentMethodMaxLength)]
		public ZString PaymentMethod
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PaymentMethod);
			set
			{
				var oldValue = PaymentMethod;
				CheckMaximumLength(PaymentMethodInfo, value);
				this.SetSystemDefinedValue(Schema.PaymentMethod, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidatePaymentMethod();
				}
				PaymentMethodInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo PaymentMethodInfo => GetZPropertyInfo(Schema.PaymentMethod);

		[ResourceStringData("AsycudaBill.AccountantName", Caption = "Accountant")]
		[MaxLength(Schema.AccountantNameMaxLength)]
		public ZString AccountantName
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AccountantName);
			set
			{
				var oldValue = AccountantName;
				CheckMaximumLength(AccountantNameInfo, value);
				this.SetSystemDefinedValue(Schema.AccountantName, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateAccountantName();
				}
				AccountantNameInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AccountantNameInfo => GetZPropertyInfo(Schema.AccountantName);

		[ResourceStringData("TRAsycudaBill.AccountantVAT", Caption = "Accountant VAT/ID No", ShortCaption = "Acct. VAT/ID No")]
		[MaxLength(Schema.AccountantVATMaxLength)]
		public ZString AccountantVAT
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AccountantVAT);
			set
			{
				var oldValue = AccountantVAT;
				CheckMaximumLength(AccountantVATInfo, value);
				this.SetSystemDefinedValue(Schema.AccountantVAT, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateAccountantVAT();
				}
				AccountantVATInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AccountantVATInfo => GetZPropertyInfo(Schema.AccountantVAT);

		public void ReCalcStatisticalValue()
		{
			foreach (AsycudaPack pack in Packs)
			{
				pack.PackedItem?.ReCalcStatisticalValue();
			}
		}

		[ResourceStringData("AsycudaBill.ABL_OtherValue", Caption = "Domestic Expenditure Value", ShortCaption = "Dom. Exp. Value")]
		[DecimalPlaces(2)]
		public override ZDecimal ABL_OtherValue
		{
			get => base.ABL_OtherValue;
			set
			{
				var oldValue = ABL_OtherValue;
				base.ABL_OtherValue = value;
				if (oldValue != ABL_OtherValue && !IsCopying)
				{
					Header.ReCalcOtherValue();
				}
			}
		}

		[ReadOnlyMember(nameof(ABL_RX_NKOtherValueCurrencyReadOnly))]
		[ResourceStringData("AsycudaBill.ABL_RX_NKOtherValueCurrency", Caption = "")]
		[MaxLength(Schema.ABL_RX_NKOtherValueCurrencyMaxLength)]
		public override ZString ABL_RX_NKOtherValueCurrency
		{
			get => base.ABL_RX_NKOtherValueCurrency;
			set => base.ABL_RX_NKOtherValueCurrency = value;
		}
		public ZBool ABL_RX_NKOtherValueCurrencyReadOnly => ZBool.True;

		public override ZInt ABL_ManifestQty
		{
			get => base.ABL_ManifestQty;
			set
			{
				var oldValue = ABL_ManifestQty;
				base.ABL_ManifestQty = value;
				if (oldValue != ABL_ManifestQty && !IsCopying)
				{
					Header?.ReCalcTotalBoxQty();
				}
			}
		}

		[ResourceStringData("AsycudaBill.ABL_Procedure", Caption = "Procedure Code")]
		[MaxLength(Schema.ABL_ProcedureMaxLength)]
		[BusinessObjectTestExclude]
		public override ZString ABL_Procedure
		{
			get => IsChildMasterBill ? base.ABL_Procedure : GetEffectiveValueToReturn(base.ABL_Procedure, Header.MasterBill?.ABL_Procedure ?? ZString.Empty);
			set
			{
				var oldValue = ABL_Procedure;
				base.ABL_Procedure = GetEffectiveValueToSet(value, Header?.MasterBill?.ABL_Procedure ?? ZString.Empty);
				if (!IsCopying && oldValue != ABL_Procedure)
				{
					if (this.IsChildMasterBill)
					{
						Header?.Bills.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("TR.ETrade.AsycudaBill.BillStatus", Caption = "Inspection Line")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsStatusList))]
		[ReadOnlyMember(nameof(ABL_BillStatus_ReadOnly))]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set => base.ABL_BillStatus = value;
		}

		protected override bool ABL_BillStatus_ReadOnly => true;

		[ResourceStringData("TR.ETrade.AsycudaBill.CargoStatus", Caption = "Cargo Status")]
		[ReadOnly(true)]
		public override ZString ABL_CargoStatus
		{
			get => base.ABL_CargoStatus;
			set => base.ABL_CargoStatus = value;
		}

		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set
			{
				var oldValue = ABL_BolType;
				base.ABL_BolType = value;
				if (!IsCopying && oldValue != ABL_BolType)
				{
					if (this.IsChildMasterBill)
					{
						Header?.Bills.MarkAsNeedingValidation();
					}
				}
			}
		}

		ZString GetEffectiveValueToReturn(ZString originalValue, ZString backupValue)
		{
			return originalValue.IsEmpty ? backupValue : originalValue;
		}

		ZString GetEffectiveValueToSet(ZString valuePassed, ZString backupValue)
		{
			var result = valuePassed;
			if (valuePassed == backupValue && !IsChildMasterBill)
			{
				result = ZString.Empty;
			}
			return result;
		}

		[DecimalPlaces(2)]
		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set
			{
				var oldValue = ABL_CustomsValue;
				base.ABL_CustomsValue = value;
				if (oldValue != ABL_CustomsValue && !IsCopying)
				{
					Header?.ReCalcTotalCustomsValue();
				}
			}
		}

		protected override bool ABL_CustomsValue_ReadOnly => true;

		public override ZString ABL_RX_NKCustomsValueCurrency
		{
			get => base.ABL_RX_NKCustomsValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKCustomsValueCurrency;
				base.ABL_RX_NKCustomsValueCurrency = value;
				if (oldValue != ABL_RX_NKCustomsValueCurrency && !IsCopying)
				{
					Header?.ReCalcTotalCustomsValue();
				}
			}
		}

		protected override bool ABL_RX_NKCustomsValueCurrency_ReadOnly => true;

		[DecimalPlaces(2)]
		public override ZDecimal ABL_TransportValue
		{
			get => base.ABL_TransportValue;
			set
			{
				var oldValue = ABL_TransportValue;

				if (oldValue != value)
				{
					base.ABL_TransportValue = value;
					PrecedentFreightToDisplayInfo.RefreshBinding();
					Header.ReCalcFreightValue();
					CalculateCustomsValueFromGoodsValue();
				}
			}
		}

		public override ZString ABL_RX_NKTransportValueCurrency
		{
			get => base.ABL_RX_NKTransportValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKTransportValueCurrency;
				if (oldValue != value)
				{
					base.ABL_RX_NKTransportValueCurrency = value;
					Header.ReCalcFreightValue();
					CalculateCustomsValueFromGoodsValue();
				}
			}
		}

		[ResourceStringData("AsycudaBill.PrecedentFreightToDisplay", Caption = "Precedent Freight Cost", ShortCaption = "Precedent Freight")]
		[DecimalPlaces(2)]
		public ZDecimal PrecedentFreightToDisplay => ShouldAddPrecedentFreightCost() ? PrecedentFreightCost : ZDecimal.Zero;

		public ZPropertyInfo PrecedentFreightToDisplayInfo => GetZPropertyInfo(nameof(PrecedentFreightToDisplay));

		[ResourceStringData("AsycudaBill.PrecedentFreightCostCurrency", Caption = "")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsValueCurrencies))]
		public ZString PrecedentFreightCostCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		[DecimalPlaces(2)]
		public override ZDecimal ABL_InsuranceValue
		{
			get => base.ABL_InsuranceValue;
			set
			{
				var oldValue = ABL_InsuranceValue;

				if (oldValue != value)
				{
					base.ABL_InsuranceValue = value;
					Header.ReCalcInsuranceValue();
				}
			}
		}

		public override ZString ABL_RX_NKInsuranceValueCurrency
		{
			get => base.ABL_RX_NKInsuranceValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKInsuranceValueCurrency;
				if (oldValue != value)
				{
					base.ABL_RX_NKInsuranceValueCurrency = value;
					Header.ReCalcInsuranceValue();
				}
			}
		}

		[ResourceStringData("TR.ETrade.AsycudaBill.ABL_GoodsValue", Caption = "Goods Value")]
		[DecimalPlaces(2)]
		public override ZDecimal ABL_GoodsValue
		{
			get => base.ABL_GoodsValue;
			set
			{
				var oldValue = ABL_GoodsValue;
				base.ABL_GoodsValue = value;

				if (oldValue != value && !IsCopying && !ABL_GoodsValue.IsEmpty)
				{
					CalculateCustomsValueFromGoodsValue();
				}
			}
		}

		public override ZString ABL_RX_NKGoodsValueCurrency
		{
			get => base.ABL_RX_NKGoodsValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKGoodsValueCurrency;
				base.ABL_RX_NKGoodsValueCurrency = value;

				if (oldValue != value && !IsCopying)
				{
					CalculateCustomsValueFromGoodsValue();
					SyncPackItemGoodsValueCurrency();
				}
			}
		}

		[ResourceStringData("TR.ETrade.AsycudaBill.ABL_SpecialCargoCode", Caption = "Trade Type", ShortCaption = "Trade Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SpecialCargoCodes))]
		public override ZString ABL_SpecialCargoCode
		{
			get => base.ABL_SpecialCargoCode;
			set => base.ABL_SpecialCargoCode = value;
		}

		public override ZString ABL_Incoterm
		{
			get => base.ABL_Incoterm;
			set
			{
				var oldValue = ABL_Incoterm;
				base.ABL_Incoterm = value;

				if (oldValue != value && !IsCopying)
				{
					CalculateCustomsValueFromGoodsValue();
					PrecedentFreightToDisplayInfo.RefreshBinding();
					Header.ReCalcFreightValue();
				}
			}
		}

		void SyncPackItemGoodsValueCurrency()
		{
			foreach (AsycudaPack pack in Packs)
			{
				var packItem = pack.PackedItem;
				if (packItem != null)
				{
					packItem.API_RX_NKGoodsValueCurrency = ABL_RX_NKGoodsValueCurrency;
				}
			}
		}

		public void CalculateCustomsValueFromGoodsValue()
		{
			if (!ABL_GoodsValue.IsEmpty && !ABL_RX_NKGoodsValueCurrency.IsEmpty)
			{
				var customsValue = Header.ConvertUsingCustomsRate(Header.AMA_DateAtCustomsOffice, ABL_GoodsValue, ABL_RX_NKGoodsValueCurrency,
																  Core.Constants.CurrencyCodes.EuropeanUnion, Header.Branch?.Company);

				if (ABL_TransportValue.IsEmpty && IncoTermFreightIncluding.Contains(ABL_Incoterm))
				{
					customsValue += PrecedentFreightCost;
				}
				else if (IncoTermFreightNotIncluding.Contains(ABL_Incoterm))
				{
					customsValue += ABL_TransportValue.IsEmpty ? PrecedentFreightCost : Header.ConvertUsingCustomsRate(Header.AMA_DateAtCustomsOffice, ABL_TransportValue, ABL_RX_NKTransportValueCurrency,
																													   Core.Constants.CurrencyCodes.EuropeanUnion, Header.Branch?.Company);
				}

				ABL_CustomsValue = customsValue;
			}
		}

		bool ShouldAddPrecedentFreightCost()
		{
			return ABL_TransportValue.IsEmpty &&
				   (IncoTermFreightIncluding.Contains(ABL_Incoterm) || IncoTermFreightNotIncluding.Contains(ABL_Incoterm));
		}

		static readonly ImmutableHashSet<string> IncoTermFreightIncluding = new[] { "CFR", "CIF", "CPT", "CIP", "DAP", "DPU", "DDP" }.ToImmutableHashSet();

		static readonly ImmutableHashSet<string> IncoTermFreightNotIncluding = new[] { "FCA", "FAS", "FOB", "EXW" }.ToImmutableHashSet();

		#endregion

		public new AsycudaTaxCollection AsycudaTaxes => (AsycudaTaxCollection)base.AsycudaTaxes;
		protected override ASYCUDA.Business.IAsycudaTaxCollection<ASYCUDA.Business.AsycudaTax, ASYCUDA.Business.AsycudaBill> CreateNewAsycudaTaxCollection() => new AsycudaTaxCollection(this);
		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);

		#region Supporting Documents For Bill

		[ChildEditable(true)]
		public SupportingDocumentsCollection SupportingDocumentsForBill
		{
			get
			{
				if (supportingDocumentsForBill == null)
				{
					supportingDocumentsForBill = new SupportingDocumentsCollection(this);
					supportingDocumentsForBill.Load();
					RegisterEditableChildObject(supportingDocumentsForBill);
				}
				return supportingDocumentsForBill;
			}
		}

		SupportingDocumentsCollection supportingDocumentsForBill;

		#endregion

		#region Guarantee

		[ResourceStringData("AsycudaBill.GuaranteeType", Caption = "Guarantee Type")]
		[MaxLength(Schema.GuaranteeTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BondTypeList))]
		public ZString GuaranteeType
		{
			get { return Guarantee?.PW_BondType ?? ZString.Empty; }
			set
			{
				var oldValue = GuaranteeType;
				if (oldValue != value)
				{
					CheckMaximumLength(GuaranteeTypeInfo, value);
					var guarantee = Guarantee ?? CreateCusBondDetail();
					guarantee.PW_BondType = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateGuaranteeType();
					}
					GuaranteeTypeInfo.RefreshBinding(oldValue);
				}
				MarkAsNeedingValidation();
			}
		}

		public ZPropertyInfo GuaranteeTypeInfo => GetZPropertyInfo(Schema.GuaranteeType);

		[ResourceStringData("AsycudaBill.GuaranteeRefNo", Caption = "Guarantee Ref.No")]
		[MaxLength(Schema.GuaranteeRefNoMaxLength)]
		public ZString GuaranteeRefNo
		{
			get { return Guarantee?.PW_BondNumber ?? ZString.Empty; }
			set
			{
				var oldValue = GuaranteeRefNo;
				if (oldValue != value)
				{
					CheckMaximumLength(GuaranteeRefNoInfo, value);
					var guarantee = Guarantee ?? CreateCusBondDetail();
					guarantee.PW_BondNumber = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateGuaranteeRefNo();
					}
					GuaranteeRefNoInfo.RefreshBinding(oldValue);
				}
				MarkAsNeedingValidation();
			}
		}
		public ZPropertyInfo GuaranteeRefNoInfo => GetZPropertyInfo(Schema.GuaranteeRefNo);

		[ResourceStringData("AsycudaBill.GuaranteeAmount", Caption = "Guarantee Amount (TRY)", ShortCaption = "G.Amount (TRY)")]
		public ZDecimal GuaranteeAmount
		{
			get { return Guarantee?.PW_BondAmount ?? ZDecimal.Zero; }
			set
			{
				var oldValue = GuaranteeAmount;
				if (oldValue != value)
				{
					var guarantee = Guarantee ?? CreateCusBondDetail();
					guarantee.PW_BondAmount = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateGuaranteeAmount();
					}
					GuaranteeAmountInfo.RefreshBinding();
				}
				MarkAsNeedingValidation();
			}
		}
		public ZPropertyInfo GuaranteeAmountInfo => GetZPropertyInfo(Schema.GuaranteeAmount);

		[ChildEditable(true)]
		public CusBondDetailCollection<AsycudaBill> Guarantees
		{
			get
			{
				if (fBondDetails == null)
				{
					fBondDetails = new CusBondDetailCollection<AsycudaBill>(this);
					fBondDetails.Load();
					this.RegisterEditableChildObject(fBondDetails);
				}
				return fBondDetails;
			}
		}
		CusBondDetailCollection<AsycudaBill> fBondDetails;

		CusBondDetail Guarantee
		{
			get
			{
				if (fGuarantee == null || fGuarantee.IsDeleted)
				{
					fGuarantee = LoadCusBondDetail();
				}
				return fGuarantee;
			}
		}
		CusBondDetail fGuarantee;

		CusBondDetail LoadCusBondDetail()
		{
			return Guarantees.Cast<CusBondDetail>().FirstOrDefault(e => e.PW_ParentID == PK && !e.IsDeleted);
		}

		CusBondDetail CreateCusBondDetail()
		{
			using (this.SuspendSettingHasChanges())
			using (this.SuspendMarkingAsNeedingValidation())
			{
				return Guarantees.AddNew();
			}
		}

		#endregion

		#region SupplementaryDeclaration

		[ResourceStringData("AsycudaBill.SupplementaryDeclarationName", Caption = "Name")]
		[MaxLength(Schema.SupplementaryDeclarationNameMaxLength)]
		public ZString SupplementaryDeclarationName
		{
			get { return SupplementaryDeclaration?.CY_Data ?? ZString.Empty; }
			set
			{
				var oldValue = SupplementaryDeclarationName;
				if (oldValue != value)
				{
					CheckMaximumLength(SupplementaryDeclarationNameInfo, value);
					var supplementary = SupplementaryDeclaration ?? CreateSupplementaryDeclaration();
					supplementary.CY_Data = value;
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
					SupplementaryDeclarationNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SupplementaryDeclarationNameInfo => GetZPropertyInfo(Schema.SupplementaryDeclarationName);

		[ResourceStringData("AsycudaBill.SupplementaryDeclarationRegNoIdNo", Caption = "Reg No./ID No.")]
		[MaxLength(Schema.SupplementaryDeclarationRegNoIdNoMaxLength)]
		public virtual ZString SupplementaryDeclarationRegNoIdNo
		{
			get { return SupplementaryDeclaration?.CY_Code ?? ZString.Empty; }
			set
			{
				var oldValue = SupplementaryDeclarationRegNoIdNo;
				if (oldValue != value)
				{
					CheckMaximumLength(SupplementaryDeclarationRegNoIdNoInfo, value);
					var supplementary = SupplementaryDeclaration ?? CreateSupplementaryDeclaration();
					supplementary.CY_Code = value;
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}

					if (!IsValidationSuspended)
					{
						RegularBillValidation?.ValidateSupplementaryDeclarationRegNoIdNo();
					}
					SupplementaryDeclarationRegNoIdNoInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SupplementaryDeclarationRegNoIdNoInfo => GetZPropertyInfo(Schema.SupplementaryDeclarationRegNoIdNo);

		[ResourceStringData("AsycudaBill.SupplementaryDeclarationDeliveryDate", Caption = "Delivery Date")]
		public ZDateTime SupplementaryDeclarationDeliveryDate
		{
			get { return SupplementaryDeclaration?.CY_Date ?? ZDateTime.Empty; }
			set
			{
				var oldValue = SupplementaryDeclarationDeliveryDate;
				if (oldValue != value)
				{
					var supplementary = SupplementaryDeclaration ?? CreateSupplementaryDeclaration();
					supplementary.CY_Date = value;
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}

					if (!IsValidationSuspended)
					{
						RegularBillValidation?.ValidateSupplementaryDeclarationDeliveryDate();
					}
					SupplementaryDeclarationDeliveryDateInfo.RefreshBinding(value);
				}
			}
		}

		public ZPropertyInfo SupplementaryDeclarationDeliveryDateInfo => GetZPropertyInfo(Schema.SupplementaryDeclarationDeliveryDate);

		public ETradeData SupplementaryDeclaration
		{
			get
			{
				if (supplementaryDeclaration == null || supplementaryDeclaration.IsDeleted)
				{
					supplementaryDeclaration = LoadSupplementaryDeclaration();
				}
				return supplementaryDeclaration;
			}
		}
		ETradeData supplementaryDeclaration;

		ETradeData LoadSupplementaryDeclaration()
		{
			return ETradeBillDatas.Cast<ETradeData>().FirstOrDefault(e => e.CY_ParentID == PK && !e.IsDeleted && e.CY_Type == BillCYType);
		}

		ETradeData CreateSupplementaryDeclaration()
		{
			using (this.SuspendSettingHasChanges())
			using (this.SuspendMarkingAsNeedingValidation())
			{
				var newSupplementaryDeclaration = ETradeBillDatas.AddNew();
				newSupplementaryDeclaration.CY_Type = BillCYType;
				newSupplementaryDeclaration.CY_ParentID = this.PK;
				newSupplementaryDeclaration.CY_ParentTableCode = this.TablePrefix;
				return newSupplementaryDeclaration;
			}
		}

		[ChildEditable(true)]
		public ETradeDataCollection ETradeBillDatas
		{
			get
			{
				if (fETradeBillDatas == null)
				{
					fETradeBillDatas = new ETradeDataCollection(this, BillCYType);
					fETradeBillDatas.Load();
					RegisterEditableChildObject(fETradeBillDatas);
				}
				return fETradeBillDatas;
			}
		}
		ETradeDataCollection fETradeBillDatas;

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(BillCYType, typeof(ETradeData));
				return result;
			}
		}

		public const string BillCYType = "DPD";

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_RX_NKOtherValueCurrency = Core.Constants.CurrencyCodes.Turkey;
			ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			ABL_SpecialCargoCode = SpecialCargoCodes.Codes.ET;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ContainerNumber = ZString.Empty;
			}
			Guarantees.RemoveAndDeleteAll();
			base.Delete();
		}

		#region ICusSupportingInfoTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> { { SupportingDocuments.SupportingDocumentsType, typeof(SupportingDocuments) } };
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set
			{
				var oldValue = ABL_GrossWeight;
				base.ABL_GrossWeight = value;
				if (oldValue != ABL_GrossWeight && !IsCopying)
				{
					grossWeightInKGCached = null;
				}
			}
		}

		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set
			{
				var oldValue = ABL_GrossWeightUQ;
				base.ABL_GrossWeightUQ = value;
				if (oldValue != ABL_GrossWeightUQ && !IsCopying)
				{
					grossWeightInKGCached = null;
				}
			}
		}

		public ZDecimal GrossWeightInKG => CachedValueHelper.GetValue(ref grossWeightInKGCached, () => Core.Constants.Weight.ConvertSafe(ABL_GrossWeight, ABL_GrossWeightUQ, Constants.Weight.Kilograms));

		CachedValue<ZDecimal> grossWeightInKGCached;

		RefSysConfig.Loader RefSysConfigLoader
		{
			get
			{
				if (refSysConfigLoader == null)
				{
					refSysConfigLoader = new RefSysConfig.Loader(Factory);
				}
				return refSysConfigLoader;
			}
		}

		RefSysConfig.Loader refSysConfigLoader;

		public ZDecimal MaxImportGrossWeight
		{
			get
			{
				var effectiveDateForDutyRate = Header.EffectiveDateForDutyRate.Date;
				return Factory.GetCachedValue("TRETIGWMAX" + effectiveDateForDutyRate, () =>
				{
					return RefSysConfigLoader.Load("TRETIGWMAX", effectiveDateForDutyRate)?.ZRC_DecimalValue ?? ZDecimal.Zero;
				});
			}
		}

		public ZDecimal MaxExportGrossWeight
		{
			get
			{
				var effectiveDateForDutyRate = Header.EffectiveDateForDutyRate.Date;
				return Factory.GetCachedValue("TRETEGWMAX" + effectiveDateForDutyRate, () =>
				{
					return RefSysConfigLoader.Load("TRETEGWMAX", effectiveDateForDutyRate)?.ZRC_DecimalValue ?? ZDecimal.Zero;
				});
			}
		}

		public ZDecimal MaxImportCustomsValue
		{
			get
			{
				var effectiveDateForDutyRate = Header.EffectiveDateForDutyRate.Date;
				return Factory.GetCachedValue("TRETIGVMAX" + effectiveDateForDutyRate, () =>
				{
					return RefSysConfigLoader.Load("TRETIGVMAX", effectiveDateForDutyRate)?.ZRC_DecimalValue ?? ZDecimal.Zero;
				});
			}
		}

		public ZDecimal MaxExportCustomsValue
		{
			get
			{
				var effectiveDateForDutyRate = Header.EffectiveDateForDutyRate.Date;
				return Factory.GetCachedValue("TRETEGVMAX" + effectiveDateForDutyRate, () =>
				{
					return RefSysConfigLoader.Load("TRETEGVMAX", effectiveDateForDutyRate)?.ZRC_DecimalValue ?? ZDecimal.Zero;
				});
			}
		}

		public ZDecimal PrecedentFreightCost
		{
			get
			{
				var effectiveDateForDutyRate = Header.EffectiveDateForDutyRate.Date;
				return Factory.GetCachedValue("TREPREFRCO" + effectiveDateForDutyRate, () =>
				{
					return RefSysConfigLoader.Load("TREPREFRCO", effectiveDateForDutyRate)?.ZRC_DecimalValue ?? ZDecimal.Zero;
				});
			}
		}

		public ZString SCDRateFixForETrade
		{
			get
			{
				return Factory.GetCachedValue("TRSCDRATE" + Header.EffectiveDateForDutyRate.Date, () =>
				{
					return RefSysConfigLoader.Load("TRSCDRATE", Header.EffectiveDateForDutyRate.Date)?.ZRC_StringValue ?? ZString.Empty;
				});
			}
		}

		public override bool ABL_BillNumber_ReadOnly => ABL_MessageStatus == TRMessageStatusCodeList.Codes.Awaiting ? ZBool.True : ZBool.False;

		[ResourceStringData("AA92FEB2-E5EB-4751-A53E-1D4C402B02C5", Caption = "Separated")]
		public ZBool Separated
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.Separated);
			set
			{
				var oldValue = Separated;
				this.SetSystemDefinedValue(Schema.Separated, value);
				SeparatedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SeparatedInfo => GetZPropertyInfo(Schema.Separated);

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					result.AddRange(AsycudaTaxes.Cast<AsycudaTax>());
				}
				return result.ToArray();
			}
		}

		[ResourceStringData("C252EDEF-3C87-43D3-A85A-2B62668DF2D1", Caption = "Market Place")]
		public override ZGuid ABL_OA_NotifyParty { get => base.ABL_OA_NotifyParty; set => base.ABL_OA_NotifyParty = value; }
	}
}
