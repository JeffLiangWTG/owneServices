using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.SharedConstants;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill,
		Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaBill,
		ICurrencyConverterDataProvider,
		ISupportMultipleResourceStringData
	{
		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string Remarks = nameof(AsycudaBill.Remarks);
			public const string ExchangeRate = nameof(AsycudaBill.ExchangeRate);
			public const string ShipperBondedIDType = nameof(AsycudaBill.ShipperBondedIDType);
			public const string ShipperBondedID = nameof(AsycudaBill.ShipperBondedID);
		}

		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsImport => Header?.IsImport ?? false;

		public override bool IsExport => Header?.IsExport ?? false;

		[DecimalPlaces(2)]
		[ResourceStringData("8177CB98-80A4-42C6-9003-A6F3E4C8E56B", Caption = "Deductions")]
		public override ZDecimal ABL_OtherDeductions
		{
			get => base.ABL_OtherDeductions;
			set
			{
				var oldValue = ABL_OtherDeductions;
				base.ABL_OtherDeductions = value;
				if (!IsCopying && oldValue != ABL_OtherDeductions)
				{
					CalculateCustomsValue();
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("0D239F4F-E420-4A4C-A885-ABA633D69A05", Caption = "Deductions Currency", ShortCaption = "Curr.")]
		public override ZString ABL_RX_NKOtherDeductionsCurrency { get => base.ABL_RX_NKOtherDeductionsCurrency; set => base.ABL_RX_NKOtherDeductionsCurrency = value; }

		[DecimalPlaces(2)]
		[ResourceStringData("773D05EB-0464-427A-9179-C328BFA74071", Caption = "Freight")]
		public override ZDecimal ABL_FreightValue
		{
			get => base.ABL_FreightValue;
			set
			{
				var oldValue = ABL_FreightValue;
				base.ABL_FreightValue = value;
				if (!IsCopying && oldValue != ABL_FreightValue)
				{
					CalculateCustomsValue();
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("2464BFAA-4808-48C6-BD0E-3AE136CC5EBD", Caption = "Freight Currency", ShortCaption = "Curr.")]
		public override ZString ABL_RX_NKFreightValueCurrency { get => base.ABL_RX_NKFreightValueCurrency; set => base.ABL_RX_NKFreightValueCurrency = value; }

		[DecimalPlaces(2)]
		public override ZDecimal ABL_InsuranceValue
		{
			get => base.ABL_InsuranceValue;
			set
			{
				var oldValue = ABL_InsuranceValue;
				base.ABL_InsuranceValue = value;
				if (!IsCopying && oldValue != ABL_InsuranceValue)
				{
					CalculateCustomsValue();
				}
			}
		}

		[ReadOnly(true)]
		public override ZString ABL_RX_NKInsuranceValueCurrency { get => base.ABL_RX_NKInsuranceValueCurrency; set => base.ABL_RX_NKInsuranceValueCurrency = value; }

		[DecimalPlaces(2)]
		[ResourceStringData("341A4A7F-81F9-4627-BD19-C9F011434CA4", Caption = "Additions")]
		public override ZDecimal ABL_OtherValue
		{
			get => base.ABL_OtherValue;
			set
			{
				var oldValue = ABL_OtherValue;
				base.ABL_OtherValue = value;
				if (!IsCopying && oldValue != ABL_OtherValue)
				{
					CalculateCustomsValue();
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("048651CC-36FA-4B67-BAC7-9BEE2093DF9F", Caption = "Additions Currency", ShortCaption = "Curr.")]
		public override ZString ABL_RX_NKOtherValueCurrency { get => base.ABL_RX_NKOtherValueCurrency; set => base.ABL_RX_NKOtherValueCurrency = value; }

		[MaxLength(8)]
		public override ZInt ABL_ManifestQty
		{
			get => base.ABL_ManifestQty;
			set
			{
				var oldValue = ABL_ManifestQty;
				base.ABL_ManifestQty = value < ZInt.Zero ? ZInt.Zero : value;
				ABL_ManifestQtyInfo.RefreshBinding(oldValue);
			}
		}

		[ResourceStringData("D8784C7E-099A-41CA-B607-6779627BE7B1", Caption = "Weight", FullDescription = "Gross Weight")]
		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set
			{
				var oldValue = ABL_GrossWeight;
				base.ABL_GrossWeight = value < ZDecimal.Zero ? ZDecimal.Zero : value;
				ABL_GrossWeightInfo.RefreshBinding(oldValue);
			}
		}

		#region ABL_CustomsValue
		public const string ABL_CustomsValueIMPCaptionKey = "A7F0B314-2E34-4010-AE6B-22273138B47A";

		public const string ABL_CustomsValueEXPCaptionKey = "6E1587F1-A857-4376-B43F-42E749BBF05E";

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { IsImport ? ABL_CustomsValueIMPCaptionKey : ABL_CustomsValueEXPCaptionKey };

		[DecimalPlaces(2)]
		[ResourceStringData("9F72DD96-FFA4-4B47-BFB0-30D8D7C3AC7D", Caption = "Customs Value")]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business|ABL_CustomsValue|IMP", Caption = "Customs Value", MultipleKey = ABL_CustomsValueIMPCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business|ABL_CustomsValue|EXP", Caption = "FOB", MultipleKey = ABL_CustomsValueEXPCaptionKey)]
		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set
			{
				var oldValue = ABL_CustomsValue;
				base.ABL_CustomsValue = value < ZDecimal.Zero ? ZDecimal.Zero : value;
				ABL_CustomsValueInfo.RefreshBinding(oldValue);
			}
		}

		protected override bool ABL_CustomsValue_ReadOnly => true;
		#endregion

		protected override bool ABL_RX_NKCustomsValueCurrency_ReadOnly => true;

		[DecimalPlaces(2)]
		[ResourceStringData("48F04B60-D3E6-43F9-80E8-60307FC7F14E", Caption = "Total Invoice Amount", ShortCaption = "Total Inv. Amt")]
		public override ZDecimal ABL_GoodsValue
		{
			get => base.ABL_GoodsValue;
			set
			{
				var oldValue = ABL_GoodsValue;
				base.ABL_GoodsValue = value < ZDecimal.Zero ? ZDecimal.Zero : value;
				ABL_GoodsValueInfo.RefreshBinding(oldValue);
			}
		}

		[MaxLength(8)]
		[ResourceStringData("C095725D-1C96-4AFE-84D4-914329B1F2D77", Caption = "Goods Location")]
		public override ZString ABL_GoodsLocation { get => base.ABL_GoodsLocation; set => base.ABL_GoodsLocation = value; }

		[MaxLength(35)]
		[ResourceStringData("9CA60539-9670-457D-B23C-8AF155890204", Caption = "Bill Number", ShortCaption = "Bill No.")]
		public override ZString ABL_BillNumber { get => base.ABL_BillNumber; set => base.ABL_BillNumber = value; }

		[MaxLength(4)]
		[ResourceStringData("38D317B0-3916-44C5-A838-BD59C14C7E84", Caption = "Shipping Order")]
		public override ZString ABL_CarrierReference { get => base.ABL_CarrierReference; set => base.ABL_CarrierReference = value; }

		[ResourceStringData("7DEEAED2-96E2-4046-95D6-722E3ED9660D", Caption = "ETA")]
		public override ZDateTime ABL_E_ARV { get => base.ABL_E_ARV; set => base.ABL_E_ARV = value; }

		[MaxLength(1)]
		[ResourceStringData("64B02B25-F8E6-499E-8138-00CE6ADA06E0", Caption = "Examination Mode", ShortCaption = "Exam. Mode")]
		public override ZString ABL_Procedure { get => base.ABL_Procedure; set => base.ABL_Procedure = value; }

		[ResourceStringData("0B5C9588-AFE2-4D8A-9C02-E9AADAD011C2", Caption = "Unit Price Term", ShortCaption = "U/P. Term")]
		public override ZString ABL_Incoterm
		{
			get => base.ABL_Incoterm;
			set
			{
				var oldValue = ABL_Incoterm;
				base.ABL_Incoterm = value;
				if (!IsCopying && oldValue != ABL_Incoterm)
				{
					CalculateCustomsValue();
				}
			}
		}

		[ResourceStringData("D40FF9E0-694E-485B-B24C-918DACAB4684", Caption = "Total Invoice Amount Currency", MediumCaption = "Currency", ShortCaption = "Curr.")]
		public override ZString ABL_RX_NKGoodsValueCurrency
		{
			get => base.ABL_RX_NKGoodsValueCurrency;
			set
			{
				var oldValue = ABL_RX_NKGoodsValueCurrency;
				base.ABL_RX_NKGoodsValueCurrency = value;
				if (!IsCopying && oldValue != ABL_RX_NKGoodsValueCurrency)
				{
					PackedItems.Cast<AsycudaPackedItem>().ForEach(c => c.API_RX_NKGoodsValueCurrency = ABL_RX_NKGoodsValueCurrency);
					var goodsValueCurrency = ABL_RX_NKGoodsValueCurrency;
					ABL_RX_NKOtherDeductionsCurrency = goodsValueCurrency;
					ABL_RX_NKFreightValueCurrency = goodsValueCurrency;
					ABL_RX_NKInsuranceValueCurrency = goodsValueCurrency;
					ABL_RX_NKOtherValueCurrency = goodsValueCurrency;
					CalculateCustomsValue();
				}
			}
		}

		[ResourceStringData("8894F158-54AA-4FC3-A431-8A83D2D9B28E", Caption = "Port of Loading", FullDescription = "Place at which the goods (consignments) are loaded on to the active means of transport.")]
		public override ZString ABL_RL_NKPortOfLoading { get => base.ABL_RL_NKPortOfLoading; set => base.ABL_RL_NKPortOfLoading = value; }

		[ResourceStringData("72DA55FF-BAAF-4549-9D7F-5C5A6BD2F541", Caption = "Port of Discharge", FullDescription = "Place at which the goods (consignment) are unloaded from the active means of transport having been used for their carriage.")]
		public override ZString ABL_RL_NKPortOfDischarge { get => base.ABL_RL_NKPortOfDischarge; set => base.ABL_RL_NKPortOfDischarge = value; }

		[ResourceStringData("39D45A27-10FD-4B10-A6FC-1FCFF63528FD", Caption = "Final Destination", FullDescription = "The destination to which goods are to be delivered, if different from the buyer address and the consignee address.")]
		public override ZString ABL_LocationInformation { get => base.ABL_LocationInformation; set => base.ABL_LocationInformation = value; }

		[MaxLength(256)]
		[ResourceStringData("5575D033-2DAA-46E3-92D0-2C7C91D2B176", Caption = "Remarks")]
		public ZString Remarks
		{
			get { return base.ABL_Remarks; }
			set
			{
				CheckMaximumLength(RemarksInfo, value);
				base.ABL_Remarks = value;
				RemarksInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(nameof(Remarks));

		[ResourceStringData("D8754DB2-ADFC-4398-B099-16CFB0EB6BF2", Caption = "Exchange Rate")]
		public ZDecimal ExchangeRate
		{
			get
			{
				var exchangeRate = ZDecimal.Zero;
				if (GoodsValueCurrency is RefCurrency currency)
				{
					exchangeRate = (currency.RX_Code == LocalCurrencyCode) ? new ZDecimal(1m) : CurrencyConverter.GetExchangeRate(currency);
				}
				return exchangeRate;
			}
		}

		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(nameof(ExchangeRate));

		ZString LocalCurrencyCode => Company.Country.RN_RX_NKLocalCurrency;

		GlbCompany Company => Header?.Branch?.Company ?? GlbCompany.CurrentCompany;

		CurrencyConverter CurrencyConverter
		{
			get
			{
				if (currencyConverter == null)
				{
					currencyConverter = new CurrencyConverterWithDataProvider(Factory, this);
				}
				return currencyConverter;
			}
		}

		CurrencyConverter currencyConverter;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Consignors))]
		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var oldValue = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && oldValue != ABL_OA_Shipper && !ABL_OA_Shipper.IsEmpty)
				{
					if (Shipper?.GetTranslatedAddressInSpecificLanguage(Languages.ChineseTraditional) is OrgTranslatedAddress localAddress)
					{
						ABL_ShipperLocalNameInfo.Value = localAddress.CompanyName.Left(ABL_ShipperLocalNameInfo.MaxLength);
						ABL_ShipperLocalStreet1Info.Value = localAddress.Address1;
						ABL_ShipperLocalStreet2Info.Value = localAddress.Address2;
						ABL_ShipperLocalCityInfo.Value = localAddress.City;
						ABL_ShipperLocalStateInfo.Value = localAddress.StateCode;
					}
					else
					{
						ABL_ShipperLocalNameInfo.ClearValue();
						ABL_ShipperLocalStreet1Info.ClearValue();
						ABL_ShipperLocalStreet2Info.ClearValue();
						ABL_ShipperLocalCityInfo.ClearValue();
						ABL_ShipperLocalStateInfo.ClearValue();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Consignees))]
		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee && !ABL_OA_Consignee.IsEmpty)
				{
					if (Consignee?.GetTranslatedAddressInSpecificLanguage(Languages.ChineseTraditional) is OrgTranslatedAddress localAddress)
					{
						ABL_ConsigneeLocalNameInfo.Value = localAddress.CompanyName.Left(ABL_ShipperLocalNameInfo.MaxLength);
						ABL_ConsigneeLocalStreet1Info.Value = localAddress.Address1;
						ABL_ConsigneeLocalStreet2Info.Value = localAddress.Address2;
						ABL_ConsigneeLocalCityInfo.Value = localAddress.City;
						ABL_ConsigneeLocalStateInfo.Value = localAddress.StateCode;
					}
					else
					{
						ABL_ConsigneeLocalNameInfo.ClearValue();
						ABL_ConsigneeLocalStreet1Info.ClearValue();
						ABL_ConsigneeLocalStreet2Info.ClearValue();
						ABL_ConsigneeLocalCityInfo.ClearValue();
						ABL_ConsigneeLocalStateInfo.ClearValue();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ShipperBondedIDTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill|ShipperBondedIDType", Caption = "Bonded ID")]
		public ZString ShipperBondedIDType => ShipperCusCode?.OK_CodeType ?? ZString.Empty;
		public ZPropertyInfo ShipperBondedIDTypeInfo => GetZPropertyInfo(nameof(ShipperBondedIDType));

		public ZString ShipperBondedID => ShipperCusCode?.OK_CustomsRegNo ?? ZString.Empty;
		public ZPropertyInfo ShipperBondedIDInfo => GetZPropertyInfo(nameof(ShipperBondedID));

		OrgCusCode ShipperCusCode => Factory.GetValue(ref shipperCusCodeCached, () => OrgHeaderHelper.GetOrgCusCode(Shipper, OrgHeaderHelper.BCDExporterBondedIDCodeTypes));
		CachedProperty<OrgCusCode> shipperCusCodeCached;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups()
		{
			return new AsycudaBillLookups(this);
		}

		protected override AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		protected override AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		public override ZString[] ShipperRegNoTypes() => CommonRegNoTypes;

		public override ZString[] ConsigneeRegNoTypes() => CommonRegNoTypes;

		ZString[] CommonRegNoTypes => new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID };

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_E_ARV = ZDateTime.Today;
		}

		protected override IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);

		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;

		protected override ASYCUDA.Business.IAsycudaTaxCollection<ASYCUDA.Business.AsycudaTax, ASYCUDA.Business.AsycudaBill> CreateNewAsycudaTaxCollection() => new AsycudaTaxCollection(this);

		public new AsycudaTaxCollection AsycudaTaxes => (AsycudaTaxCollection)base.AsycudaTaxes;

		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		#region ICurrencyConverterDataProvider members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation => Header?.DeclarationDate ?? ZDateTime.Today;

		ExchangeRateType ICurrencyConverterDataProvider.RateType => Header?.IsImport ?? false ? ExchangeRateType.Customs : ExchangeRateType.CustomsSecondary;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => 7;

		GlbCompany ICurrencyConverterDataProvider.Company => Company;

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride => LocalCurrencyCode;

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride => null;

		#endregion

		public void CalculateCustomsValue()
		{
			var goodsValue = CalculateBillGoodsValue();
			ABL_GoodsValue = goodsValue;
			var customsValue = CalculateBillCustomsValue(goodsValue);
			ABL_CustomsValue = decimal.Round(ExchangeRate * customsValue, 0);
			CalculatePackedItemCustomsValue(customsValue, goodsValue);
		}

		ZDecimal CalculateBillGoodsValue() => PackedItems.Cast<AsycudaPackedItem>().Sum(packedItem => packedItem.API_GoodsValue);
		ZDecimal CalculateBillCustomsValue(ZDecimal goodsValue)
		{
			var goodsValueCurrency = RefCurrency.LoadFromCurrencyCode(Factory, ABL_RX_NKGoodsValueCurrency);

			var cifOrFobValue = ZDecimal.Zero;
			var freightValueAmount = CurrencyConverter.ConvertExact(new Money(ABL_FreightValue, FreightValueCurrency), goodsValueCurrency).Amount.Round(2);
			var insuranceValueAmount = CurrencyConverter.ConvertExact(new Money(ABL_InsuranceValue, InsuranceValueCurrency), goodsValueCurrency).Amount.Round(2);
			if (Header.IsExport)
			{
				switch (ABL_Incoterm)
				{
					case BriefCustomsDeclarationIncotermList.Codes.CostInsuranceAndFreight:
						cifOrFobValue = goodsValue - freightValueAmount - insuranceValueAmount;
						break;
					case BriefCustomsDeclarationIncotermList.Codes.CostAndFreight:
						cifOrFobValue = goodsValue - freightValueAmount;
						break;
					case BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance:
						cifOrFobValue = goodsValue - insuranceValueAmount;
						break;
					default:
						cifOrFobValue = goodsValue;
						break;
				}
			}
			else if (Header.IsImport)
			{
				switch (ABL_Incoterm)
				{
					case BriefCustomsDeclarationIncotermList.Codes.CostInsuranceAndFreight:
						cifOrFobValue = goodsValue;
						break;
					case BriefCustomsDeclarationIncotermList.Codes.CostAndFreight:
						cifOrFobValue = goodsValue + insuranceValueAmount;
						break;
					case BriefCustomsDeclarationIncotermList.Codes.CostAndInsurance:
						cifOrFobValue = goodsValue + freightValueAmount;
						break;
					default:
						cifOrFobValue = goodsValue + freightValueAmount + insuranceValueAmount;
						break;
				}
			}

			var otherValueAmount = CurrencyConverter.ConvertExact(new Money(ABL_OtherValue, OtherValueCurrency), goodsValueCurrency).Amount.Round(2);
			var otherDeductionsAmount = CurrencyConverter.ConvertExact(new Money(ABL_OtherDeductions, OtherDeductionsCurrency), goodsValueCurrency).Amount.Round(2);
			return cifOrFobValue + otherValueAmount - otherDeductionsAmount;
		}

		void CalculateAndReconcileCustomsValue<T>(IEnumerable<T> collection, ZDecimal billCustomsValue, Action<T> action, Func<T, decimal> selector)
		{
			collection.ForEach(action);
			var actualTotal = collection.Sum(selector);
			ZDecimal discrepancy = billCustomsValue - actualTotal;
			if (!discrepancy.IsEmpty)
			{
				ReconcileHelper.ReconcileValues((IEnumerable<IReconcileCandidate>)collection, discrepancy.ToZInt());
			}
		}

		void CalculatePackedItemCustomsValue(ZDecimal billCustomsValue, ZDecimal goodsValue)
		{
			var packedItems = PackedItems;
			if (packedItems.Any())
			{
				var customsFactor = goodsValue == ZDecimal.Zero ? 1m : billCustomsValue / goodsValue;
				var exchangeRate = ExchangeRate;
				CalculateAndReconcileCustomsValue(packedItems, ABL_CustomsValue, x => x.API_CustomsValue = ((ZDecimal)(x.API_UnitPrice * x.API_CustomsQty * exchangeRate * customsFactor)).Round(0), x => x.API_CustomsValue);
				PackedItems.Cast<AsycudaPackedItem>().ForEach(x => x.API_CustomsValueInfo.RefreshBinding());
			}
		}

		ZDecimal TpfThreshold => Factory.GetValue(ref tpfThreshold, () => new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Taiwan, RefCusTaxOrFeeCodes.TPF, ZDateTime.Now)?.ZZF_Threshold ?? ZDecimal.Zero);
		CachedProperty<ZDecimal> tpfThreshold;

		ZDecimal TPFToltalAmount => Factory.GetValue(ref tpfToltalAmount, () => PackedItems.Cast<AsycudaPackedItem>().Sum(c => c.API_CustomsValue * TpfRate));
		CachedProperty<ZDecimal> tpfToltalAmount;

		public bool RequiresCalculateTPF => TPFToltalAmount >= TpfThreshold;

		public ZDecimal TpfRate => Factory.GetCachedValue($"{Core.Constants.CountryCodes.Taiwan}|{ChargeTypeOtherList.Codes.TPF}|{ZDateTime.Today}", () => new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Taiwan, ChargeTypeOtherList.Codes.TPF, ZDateTime.Now)?.ZZF_Value ?? ZDecimal.Zero);

		public ZDecimal VatRate => Factory.GetCachedValue($"{Core.Constants.CountryCodes.Taiwan}|{ChargeTypeOtherList.Codes.VAT}|{ZDateTime.Today}", () => new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Taiwan, ChargeTypeOtherList.Codes.VAT, ZDateTime.Now)?.ZZF_Value ?? ZDecimal.Zero);

		public void CalculateItemTaxes()
		{
			var packedItems = PackedItems.Cast<AsycudaPackedItem>().ToList();
			if (Header.RequiresDefaultPackitemTaxes)
			{
				packedItems.ForEach(packedItem =>
				{
					packedItem.DefaultPackedItemTaxFromCusTariffSortedDictionary();
					packedItem.DefaultDutyPackedItemTax();
				});
			}
			else
			{
				packedItems.ForEach(packedItem =>
				{
					packedItem.ClearPackedItemTax();
				});
			}
		}

		public void CalculateBillDuties()
		{
			AsycudaTaxes.RemoveAndDeleteAll();
			var list = new List<DutyCalculationIntermediateResult>();
			foreach (var packedItem in PackedItems)
			{
				packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>().Where(x => !x.AET_ChargeAmount.IsEmpty).ForEach(x => list.Add(new DutyCalculationIntermediateResult { RateCode = x.AET_ChargeType, Amount = x.AET_ChargeAmount, PaymentMethod = x.AET_MethodOfPayment }));
			}
			var dtaAmountLargeThanDtsAmount = list.Where(c => c.RateCode == ChargeTypeOtherList.Codes.DTA).Sum(c => c.Amount) > list.Where(c => c.RateCode == ChargeTypeOtherList.Codes.DTS).Sum(c => c.Amount);
			if (dtaAmountLargeThanDtsAmount)
			{
				list.RemoveAll(c => c.RateCode == ChargeTypeOtherList.Codes.DTS);
			}
			else
			{
				list.RemoveAll(c => c.RateCode == ChargeTypeOtherList.Codes.DTA);
			}
			var aggregateCharges = list.GetGroupedDuties(Header.IsImport).ToList();
			foreach (var charge in aggregateCharges)
			{
				CreateBillAsycudaTaxes(charge.TypeCode, charge.Amount, charge.PaymentMethod);
			}
		}

		void CreateBillAsycudaTaxes(ZString rateCode, ZDecimal amount, ZString paymentMethod)
		{
			var tax = AsycudaTaxes.AddNew();
			tax.AET_ChargeType = rateCode;
			tax.AET_ChargeAmount = amount;
			tax.AET_MethodOfPayment = paymentMethod;
		}

		public new IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (AsycudaBillPackedItemCollection)base.PackedItems;

		protected override IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection()
		{
			return new AsycudaBillPackedItemCollection(this);
		}
	}
}
