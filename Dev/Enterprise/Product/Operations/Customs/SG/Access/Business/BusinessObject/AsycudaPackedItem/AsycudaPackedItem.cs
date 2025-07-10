using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	[SystemDefinedValues]
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
		, Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaPackedItem.Schema
		{
			public const string GoodsType = "GoodsType";
			public const string GSTPaid = "GSTPaid";

			public const int GoodsTypeMaxLength = 10;
			public const int GSTPaidMaxLength = 1;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.GoodsTypeList))]
		[MaxLength(Schema.GoodsTypeMaxLength)]
		[ResourceStringData("SGAccess.AsycudaPackedItem|GoodsType", Caption = "Goods Type")]
		[BusinessObjectTestExclude]
		public ZString GoodsType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.GoodsType);
			set
			{
				var oldValue = GoodsType;
				CheckMaximumLength(GoodsTypeInfo, value);
				if (value.IsEmpty)
				{
					value = GetGoodsTypeFromPartyStatus(Pack?.Bill);
				}
				this.SetSystemDefinedValue(Schema.GoodsType, value);
				GoodsTypeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGoodsType();
				}
			}
		}

		public ZPropertyInfo GoodsTypeInfo => GetZPropertyInfo(Schema.GoodsType);

		public bool ShowGoodsType => Lookups.GoodsTypeList.Count > 0;

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.YesNoList))]
		[ResourceStringData("SGAccess.AsycudaPackedItem|GSTPaid", Caption = "GST Paid")]
		[MaxLength(Schema.GSTPaidMaxLength)]
		public ZString GSTPaid
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.GSTPaid); }
			set
			{
				var oldValue = GSTPaid;
				CheckMaximumLength(GSTPaidInfo, value);
				this.SetSystemDefinedValue(Schema.GSTPaid, value);
				GSTPaidInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation?.ValidateGSTPaid();
				}
			}
		}

		public ZPropertyInfo GSTPaidInfo => GetZPropertyInfo(Schema.GSTPaid);

		[ResourceStringData("SGAccess.AsycudaPackedItem|API_CustomsQty", Caption = "Customs Qty")]
		public override ZDecimal API_CustomsQty { get => base.API_CustomsQty; set => base.API_CustomsQty = value; }

		[ResourceStringData("SGAccess.AsycudaPackedItem|API_MessageStatus", Caption = "Msg. Status")]
		public override ZString API_MessageStatus { get => base.API_MessageStatus; set => base.API_MessageStatus = value; }

		[ResourceStringData("SGAccess.AsycudaPackedItem|API_PackStatus", Caption = "Pack Status")]
		public override ZString API_PackStatus { get => base.API_PackStatus; set => base.API_PackStatus = value; }

		public override ZDecimal API_CustomsValue
		{
			get => base.API_CustomsValue;
			set
			{
				var oldValue = API_CustomsValue;
				base.API_CustomsValue = value;
				if (!IsCopying && oldValue != API_CustomsValue)
				{
					CalculateGST();
				}
			}
		}

		public override ZDecimal API_DutyAmount
		{
			get => base.API_DutyAmount;
			set
			{
				var oldValue = API_DutyAmount;
				base.API_DutyAmount = value;
				if (!IsCopying && oldValue != API_DutyAmount)
				{
					CalculateGST();
				}
			}
		}

		public override ZString API_Tariff
		{
			get => base.API_Tariff;
			set
			{
				var oldValue = API_Tariff;
				base.API_Tariff = value;
				if (!IsCopying && oldValue != API_Tariff)
				{
					SGDefaults(Pack);
				}
			}
		}

		public TariffView Tariff => UniversalReferenceDataHelper.LoadBestMatch(Factory, API_Tariff, EffectiveDateForDutyRate);

		public void DefaultCustomsQtyAndUnitQtyFromPack(AsycudaPack pack)
		{
			if (pack != null)
			{
				var customsPackDetails = SGPackQuantityAndUnitConverter.GetCustomsPackDetails(Factory, UnitConverter, Tariff, new ZLong(pack.APA_PackQty), pack.APA_PackUQ);
				API_CustomsQty = customsPackDetails.Quantity;
				API_CustomsUQ = customsPackDetails.UnitOfQuantity;
			}
		}

		public void DefaultCustomsValueFromPack(AsycudaPack pack)
		{
			if (pack != null)
			{
				var packLinePriceCurrency = pack.LinePriceCurrency;
				if (!packLinePriceCurrency.IsEmpty)
				{
					var packLinePrice = pack.LinePrice;
					if (packLinePrice.IsEmpty || packLinePriceCurrency == Core.Constants.CurrencyCodes.Singapore)
					{
						SetCustomsValue(packLinePrice);
					}
					else
					{
						var packBill = pack.Bill;
						if (packBill != null)
						{
							var dateForRate = packBill.Header?.ValuationDate ?? ZDate.Today;
							var originalAmount = new Money(pack.LinePrice, pack.RefLinePriceCurrency);
							var packLocalCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Singapore);
							var cC = CurrencyConverter.New(Factory, dateForRate, ZArchitecture.Core.ExchangeRateType.Customs, 0);
							SetCustomsValue(cC.ConvertExact(originalAmount, packLocalCurrency, false).Amount);
						}
					}
				}
			}
		}

		protected override int LocalCurrencyDecimals => 2;

		internal void SetCustomsValue(ZDecimal value)
		{
			API_CustomsValue = value > ZDecimal.Zero ? Math.Max(value.Round(LocalCurrencyDecimals), customsValueMinimumValue) : decimal.Zero;
		}
		readonly decimal customsValueMinimumValue = 0.01m;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPack Pack => (AsycudaPack)base.Pack;
		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;
		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);
		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;
		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackedItemFetchStrategy(this);

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		protected override void DefaultFromCore(ManifestBase.AsycudaPack basePack)
		{
			base.DefaultFromCore(basePack);

			var pack = (AsycudaPack)basePack;
			API_GoodsDescription = pack.APA_GoodsDescription;
			SGDefaults(pack);

			var header = Header;
			if (header != null)
			{
				API_RN_NKGoodsOrigin = header.AMA_RL_NKPortOfLoading.Substring(0, 2);
			}
		}

		void CalculateGST()
		{
			var valuationDate = ZDateTime.Today;
			var isImportManifest = Pack?.Bill?.Header?.IsImport ?? false;
			if (isImportManifest)
			{
				valuationDate = Pack?.Bill?.CycleDate ?? ZDateTime.Today;
			}
			var gstRate = RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, valuationDate)?.ZZF_Value ?? ZDecimal.Zero;

			ZDecimal gstCalculated = (API_CustomsValue + API_DutyAmount) * gstRate;
			API_TaxAmount = gstCalculated.Round(2);
		}

		protected override void MakeBillApportionmentDirtyIfNeed()
		{
			Pack?.Bill?.MarkApportionmentDirty();
		}

		void SGDefaults(AsycudaPack pack)
		{
			var tariff = Tariff;
			API_CustomsUQ = tariff != null ? tariff.ZZ1_ZZ8_UQ1 : ZString.Empty;

			var bill = pack?.Bill;
			if (bill?.IsImport ?? false)
			{
				GoodsType = GetDefaultImportGoodsType(tariff, bill);
			}
			else
			{
				GoodsType = tariff?.GetSGDefaultExportGoodsType(EffectiveDateForDutyRate) ?? ZString.Empty;
				GSTPaid = ZString.Empty;
			}

			DefaultCustomsQtyAndUnitQtyFromPack(pack);
			DefaultCustomsValueFromPack(pack);
		}

		ZString GetGoodsTypeFromPartyStatus(AsycudaBill bill)
		{
			return (bill?.SG_PartyStatus).GetSGGoodsTypeFromPartyStatus();
		}

		#region GetDefaultImportGoodsType
		public void DefaultImportGoodsType()
		{
			GoodsType = GetDefaultImportGoodsType(Tariff, Pack?.Bill);
		}

		ZString GetDefaultImportGoodsType(TariffView tariff, AsycudaBill billCountry)
		{
			return tariff?.GetSGDefaultImportGoodsType(billCountry?.SG_PartyStatus, billCountry?.DutyAmount ?? ZDecimal.Zero, EffectiveDateForDutyRate) ?? ZString.Empty;
		}

		#endregion

		public SGPackQuantityAndUnitConverter SGPackQuantityAndUnitConverter => fSGPackQuantityAndUnitConverter ?? (fSGPackQuantityAndUnitConverter = new SGPackQuantityAndUnitConverter());
		SGPackQuantityAndUnitConverter fSGPackQuantityAndUnitConverter;

		RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader => refCusTaxOrFeeLoader ?? (refCusTaxOrFeeLoader = new RefCusTaxOrFee.Loader(Factory));
		RefCusTaxOrFee.Loader refCusTaxOrFeeLoader;

		protected override void SetNewCountryMessagingStatusAdditionalAction(ZString newStatus, ZString oldStatusForRollback)
		{
			if (oldStatusForRollback != newStatus)
			{
				Pack?.Bill?.SetNewCountryMessagingStatus(newStatus);
			}
		}
	}
}
