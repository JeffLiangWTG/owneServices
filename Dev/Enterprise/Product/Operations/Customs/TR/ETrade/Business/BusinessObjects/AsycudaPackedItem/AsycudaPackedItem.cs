using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
		, Integration.Customs.ASYCUDA.TRETrade.IAsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaPackedItem.Schema
		{
			public const string SerialNo = "SerialNo";
			public const string UsedGoodsCode = "UsedGoodsCode";
			public const string StatisticalValue = "StatisticalValue";
			public const string AgriculturePolicy = "AgriculturePolicy";
			public const string ValueDeclarationForm = "ValueDeclarationForm";
			public const string CalculationMethod = "CalculationMethod";
			public const string QuotaCheck = "QuotaCheck";
			public const string StatisticalValueCurrency = "StatisticalValueCurrency";
			public const string BanderolTariff = "BanderolTariff";

			public const int SerialNoMaxLength = 20;
			public const int UsedGoodsCodeMaxLength = 35;
			public const int API_CustomsQty2MaxLength = 8;
			public const int API_CustomsQty3MaxLength = 8;
			public const int AgriculturePolicyMaxLength = 35;
			public const int ValueDeclarationFormMaxLength = 35;
			public const int CalculationMethodMaxLength = 35;
			public const int StatisticalValueCurrencyMaxLength = 3;
			public const int BanderolTariffMaxLength = 16;

			public new const int API_GoodsDescriptionMaxLength = 120;
			public new const int API_BrandMaxLength = 20;
			public new const int API_ModelMaxLength = 20;

			public new const int API_CustomsUQ2MaxLength = 3;
			public new const int API_CustomsUQ3MaxLength = 3;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;
		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;
		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		#region Properties

		[ResourceStringData("AsycudaPackedItem.API_GoodsDescription", Caption = "Goods Description")]
		[MaxLength(Schema.API_GoodsDescriptionMaxLength)]
		public override ZString API_GoodsDescription
		{
			get => base.API_GoodsDescription;
			set => base.API_GoodsDescription = value;
		}

		[ResourceStringData("AsycudaPackedItem.BanderolTariff", Caption = "Band.Tariff", ShortCaption = "Band.Tariff")]
		[MaxLength(Schema.BanderolTariffMaxLength)]
		public ZString BanderolTariff
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.BanderolTariff); }
			set
			{
				var oldValue = BanderolTariff;
				CheckMaximumLength(BanderolTariffInfo, value);
				this.SetSystemDefinedValue(Schema.BanderolTariff, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBanderolTariff();
				}
				BanderolTariffInfo.RefreshBinding(oldValue);

				if (!IsCopying)
				{
					SetDefaultCustomsUQAfterBanderolTariffChanged();
				}
			}
		}

		public ZPropertyInfo BanderolTariffInfo => GetZPropertyInfo(Schema.BanderolTariff);

		public TariffViewCollection BanderolTariffList
		{
			get
			{
				var applicationBusinessProvider = Header.ApplicationBusinessProvider;
				var dataGrouping = applicationBusinessProvider.PackedItemTariffDataGrouping;
				var effectiveDate = EffectiveDateForDutyRate;
				var tariffType = TaxCodeList.RelatedMiscCodes.BanderolTariffType;
				return TariffViewCollection.GetCachedCollection(Factory, dataGrouping, tariffType, effectiveDate);
			}
		}

		void SetDefaultCustomsUQAfterBanderolTariffChanged()
		{
			var banderolTariff = BanderolTariff;
			if (!banderolTariff.IsEmpty)
			{
				var tariff = BanderolTariffView;
				if (tariff != null)
				{
					var rate = tariff.Rates.FirstOrDefault(x => x.CusRateType.ZZR_RateType == TaxCodeList.RelatedMiscCodes.BanderolRateType);
					if (rate != null)
					{
						var uq = rate.UnitsOfMeasure.FirstOrDefault()?.ZXG_UOM ?? ZString.Empty;
						if (!uq.IsEmpty)
						{
							API_CustomsUQ = uq;
						}
					}
				}
			}
		}

		TariffView GetTariffView(ZString tariffCode, ZQuery filter)
		{
			TariffView result = null;

			if (!tariffCode.IsEmpty)
			{
				filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.Equal, tariffCode);
				result = Factory.LoadTop1<TariffView>(filter);
			}
			return result;
		}

		public TariffView NormalTariffView
		{
			get
			{
				return Factory.GetCachedValue("TRNormalTariffView" + API_Tariff + EffectiveDateForDutyRate, delegate
				{
					return GetTariffView(API_Tariff, Lookups.TariffList.CompleteFilter);
				});
			}
		}

		public TariffView BanderolTariffView
		{
			get
			{
				return Factory.GetCachedValue("TRBanderolTariffView" + BanderolTariff + EffectiveDateForDutyRate, delegate
				{
					return GetTariffView(BanderolTariff, BanderolTariffList.CompleteFilter);
				});
			}
		}

		[ResourceStringData("AsycudaPackedItem.SerialNo", Caption = "Serial No")]
		[MaxLength(Schema.SerialNoMaxLength)]
		public ZString SerialNo
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SerialNo);
			set
			{
				var oldValue = SerialNo;
				CheckMaximumLength(SerialNoInfo, value);
				this.SetSystemDefinedValue(Schema.SerialNo, value);
				SerialNoInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo SerialNoInfo => GetZPropertyInfo(Schema.SerialNo);

		[ResourceStringData("AsycudaPackedItem.API_CustomsQty", Caption = "Customs Qty")]
		[DecimalPlaces(0)]
		public override ZDecimal API_CustomsQty
		{
			get => base.API_CustomsQty;
			set => base.API_CustomsQty = value;
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString API_CustomsUQ => "BI";

		[ResourceStringData("AsycudaPackedItem.API_Brand", Caption = "Brand")]
		[MaxLength(Schema.API_BrandMaxLength)]
		public override ZString API_Brand
		{
			get => base.API_Brand;
			set => base.API_Brand = value;
		}

		[ResourceStringData("AsycudaPackedItem.API_Model", Caption = "Model")]
		[MaxLength(Schema.API_ModelMaxLength)]
		public override ZString API_Model
		{
			get => base.API_Model;
			set => base.API_Model = value;
		}

		[ResourceStringData("AsycudaPackedItem.UsedGoodsCode", Caption = "Used Goods Code")]
		[MaxLength(Schema.UsedGoodsCodeMaxLength)]
		public ZString UsedGoodsCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.UsedGoodsCode);
			set
			{
				var oldValue = UsedGoodsCode;
				CheckMaximumLength(UsedGoodsCodeInfo, value);
				this.SetSystemDefinedValue(Schema.UsedGoodsCode, value);
				UsedGoodsCodeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo UsedGoodsCodeInfo => GetZPropertyInfo(Schema.UsedGoodsCode);

		[ResourceStringData("AsycudaPackedItem.API_CustomsQty2", Caption = "Supplementary Quantity 1", ShortCaption = "Supp. Qty. 1")]
		[MaxLength(Schema.API_CustomsQty2MaxLength)]
		[DecimalPlaces(3)]
		public override ZDecimal API_CustomsQty2
		{
			get => base.API_CustomsQty2;
			set => base.API_CustomsQty2 = value;
		}

		[ResourceStringData("AsycudaPackedItem.API_CustomsUQ2", Caption = "")]
		[MaxLength(Schema.API_CustomsUQ2MaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TRUOMCodeList))]
		public override ZString API_CustomsUQ2
		{
			get => base.API_CustomsUQ2;
			set => base.API_CustomsUQ2 = value;
		}

		[ResourceStringData("AsycudaPackedItem.API_CustomsQty3", Caption = "Supplementary Quantity 2", ShortCaption = "Supp. Qty. 2")]
		[MaxLength(Schema.API_CustomsQty3MaxLength)]
		[DecimalPlaces(3)]
		public override ZDecimal API_CustomsQty3
		{
			get => base.API_CustomsQty3;
			set => base.API_CustomsQty3 = value;
		}

		[ResourceStringData("AsycudaPackedItem.API_CustomsUQ3", Caption = "")]
		[MaxLength(Schema.API_CustomsUQ3MaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TRUOMCodeList))]
		public override ZString API_CustomsUQ3
		{
			get => base.API_CustomsUQ3;
			set => base.API_CustomsUQ3 = value;
		}

		[DecimalPlaces(2)]
		public override ZDecimal API_GoodsValue
		{
			get => base.API_GoodsValue;
			set
			{
				var oldValue = API_GoodsValue;
				base.API_GoodsValue = value;
				if (oldValue != API_GoodsValue && !IsCopying)
				{
					ReCalcStatisticalValue();

					if (!IsValidationSuspended)
					{
						Bill.Validation.ValidateABL_GoodsValue();
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString API_RX_NKGoodsValueCurrency
		{
			get => base.API_RX_NKGoodsValueCurrency;
			set
			{
				var oldValue = API_RX_NKGoodsValueCurrency;
				base.API_RX_NKGoodsValueCurrency = value;
				if (oldValue != API_RX_NKGoodsValueCurrency && !IsCopying)
				{
					ReCalcStatisticalValue();
				}
			}
		}

		[ResourceStringData("AsycudaPackedItem.StatisticalValue", Caption = "Statistical Value")]
		public ZDecimal StatisticalValue
		{
			get => CalcStatisticalValue;
		}
		public ZPropertyInfo StatisticalValueInfo => GetZPropertyInfo(Schema.StatisticalValue);

		ZDecimal CalcStatisticalValue => CachedValueHelper.GetValue(ref calcStatisticalValueCached, () => Header.ConvertUsingCustomsRate(Header.AMA_DateAtCustomsOffice, API_GoodsValue, API_RX_NKGoodsValueCurrency, Core.Constants.CurrencyCodes.UnitedStates, Header.Branch?.Company));

		CachedValue<ZDecimal> calcStatisticalValueCached;

		public void ReCalcStatisticalValue()
		{
			calcStatisticalValueCached = null;
			StatisticalValueInfo.RefreshBinding();
		}

		[ResourceStringData("AsycudaPackedItem.StatisticalValueCurrency", Caption = "")]
		[MaxLength(Schema.StatisticalValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.CurrencyList))]
		public ZString StatisticalValueCurrency => Core.Constants.CurrencyCodes.UnitedStates;

		public ZPropertyInfo StatisticalValueCurrencyInfo => GetZPropertyInfo(Schema.StatisticalValueCurrency);

		[ResourceStringData("AsycudaPackedItem.AgriculturePolicy", Caption = "Agriculture Policy")]
		[MaxLength(Schema.AgriculturePolicyMaxLength)]
		public ZString AgriculturePolicy
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AgriculturePolicy);
			set
			{
				var oldValue = AgriculturePolicy;
				CheckMaximumLength(AgriculturePolicyInfo, value);
				this.SetSystemDefinedValue(Schema.AgriculturePolicy, value);
				AgriculturePolicyInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo AgriculturePolicyInfo => GetZPropertyInfo(Schema.AgriculturePolicy);

		[ResourceStringData("AsycudaPackedItem.ValueDeclarationForm", Caption = "Value Declaration Form", ShortCaption = "Val. Dec. Form")]
		[MaxLength(Schema.ValueDeclarationFormMaxLength)]
		public ZString ValueDeclarationForm
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ValueDeclarationForm);
			set
			{
				var oldValue = ValueDeclarationForm;
				CheckMaximumLength(ValueDeclarationFormInfo, value);
				this.SetSystemDefinedValue(Schema.ValueDeclarationForm, value);
				ValueDeclarationFormInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ValueDeclarationFormInfo => GetZPropertyInfo(Schema.ValueDeclarationForm);

		[ResourceStringData("AsycudaPackedItem.CalculationMethod", Caption = "Calculation Method")]
		[MaxLength(Schema.CalculationMethodMaxLength)]
		public ZString CalculationMethod
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CalculationMethod);
			set
			{
				var oldValue = CalculationMethod;
				CheckMaximumLength(CalculationMethodInfo, value);
				this.SetSystemDefinedValue(Schema.CalculationMethod, value);
				CalculationMethodInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo CalculationMethodInfo => GetZPropertyInfo(Schema.CalculationMethod);

		[ResourceStringData("AsycudaPackedItem.QuotaCheck", Caption = "Quota")]
		public ZBool QuotaCheck
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.QuotaCheck);
			set
			{
				var oldValue = UsedGoodsCode;
				this.SetSystemDefinedValue(Schema.QuotaCheck, value);
				QuotaInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo QuotaInfo => GetZPropertyInfo(Schema.QuotaCheck);

		[ResourceStringData("AsycudaPackedItem.API_ChemicalSubstanceCode", Caption = "Additional Code", ShortCaption = "Add.Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TariffAdditionalCodeList))]
		[MaxLength(Schema.API_ChemicalSubstanceCodeMaxLength)]
		public override ZString API_ChemicalSubstanceCode
		{
			get => base.API_ChemicalSubstanceCode;
			set => base.API_ChemicalSubstanceCode = value;
		}

		#endregion
	}
}
