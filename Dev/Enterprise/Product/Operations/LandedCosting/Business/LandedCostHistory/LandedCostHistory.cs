using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(LandedCostHeader), "Histories")]
	public class LandedCostHistory : AutoLandedCostHistory, Integration.LandedCosting.ILandedCostHistory, IClusterKeyWorker
	{
		public const int PerUnitDecimals = 4;

		public LandedCostHistory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		// Expose individual LandedLineCostItems using the OLD LH_XXX names.  Means we have few changes when vertically partiioning these properties onto LandedLineCostItem.
		public ZDecimal LH_LandedCostGroup1 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup1); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup1); } }
		public ZDecimal LH_LandedCostGroup2 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup2); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup2); } }
		public ZDecimal LH_LandedCostGroup3 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup3); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup3); } }
		public ZDecimal LH_LandedCostGroup4 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup4); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup4); } }
		public ZDecimal LH_LandedCostGroup5 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup5); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup5); } }
		public ZDecimal LH_LandedCostGroup6 { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroup6); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroup6); } }
		public ZDecimal LH_LandedCostGroupMisc { get { return GetLineValue(LandedLineCostType.Codes.LandedCostGroupMisc); } set { SetLineValue(value, LandedLineCostType.Codes.LandedCostGroupMisc); } }

#if DEBUG
		internal ZPropertyInfo LH_LandedCostGroup1InfoForTesting
		{
			get { return GetZPropertyInfo(nameof(LH_LandedCostGroup1)); }
		}
#endif

		public ZDecimal GetLineValue(string code)
		{
			var item = LandedLineCostItems[code];
			return item == null ?
							new ZDecimal(0) :  // old LH_Blah columnns are not nullable
							LandedLineCostItems[code].LZ_CostAmount;
		}

		public void SetLineValue(ZDecimal newValue, string code)
		{
			if (!String.IsNullOrEmpty(code))
			{
				var item = LandedLineCostItems[code];  // find but do not create new
				if (item == null && !newValue.IsEmpty)
				{
					item = LandedLineCostItems.AddNew(code);
				}

				if (!newValue.IsEmpty)
				{
					item.LZ_CostAmount = newValue;
				}
				else
				{
					if (item != null)
					{
						item.Delete();  // If setting to 0, remove whole item row. 
					}
				}
			}
		}

		public override void Delete()
		{
			LandedLineCostItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#region Constants

		public new class Schema : AutoLandedCostHistory.Schema
		{
			public const string HumanReadableUltimateDistributeeCode = "HumanReadableUltimateDistributeeCode";
			public const string RoundedPerUnitCustomsDisbursementCharges = "RoundedPerUnitCustomsDisbursementCharges";
			public const string RoundedPerUnitLandingCost = "RoundedPerUnitLandingCost";
			public const string RoundedPerUnitTotalCost = "RoundedPerUnitTotalCost";
			public const string RoundedSellPrice1ExGST = "RoundedSellPrice1ExGST";
			public const string RoundedSellPrice2ExGST = "RoundedSellPrice2ExGST";
			public const string RoundedSellPrice3ExGST = "RoundedSellPrice3ExGST";
			public const string RoundedSellPrice1IncGST = "RoundedSellPrice1IncGST";
			public const string RoundedSellPrice2IncGST = "RoundedSellPrice2IncGST";
			public const string RoundedSellPrice3IncGST = "RoundedSellPrice3IncGST";
			public const string InvoiceCostInLocalCurrency = "InvoiceCostInLocalCurrency";
			public const string UnitPriceInLocalCurrency = "UnitPriceInLocalCurrency";
			public const string DateOfProcessing = "DateOfProcessing";
			public const string UniqueReferenceNumber = "UniqueReferenceNumber";
			public const string EffectiveMarkUpPercentage1 = "EffectiveMarkUpPercentage1";
			public const string EffectiveMarkUpPercentage2 = "EffectiveMarkUpPercentage2";
			public const string EffectiveMarkUpPercentage3 = "EffectiveMarkUpPercentage3";
			public const string TotalCost = "TotalCost";
		}

		#endregion

		[RelatedBusinessObject("SupplierPart")]
		[List("Lookups.SupplierParts")]
		[ReadOnly(true)]  // For validation 
		public override ZGuid LH_OP
		{
			get { return base.LH_OP; }
			set { base.LH_OP = value; }
		}

		[List("Lookups.LineTypeList")]
		[ReadOnlyMember(nameof(LH_LandedCostHistoryLineType_Readonly))]
		public override ZString LH_LandedCostHistoryLineType
		{
			get => base.LH_LandedCostHistoryLineType;
			set => base.LH_LandedCostHistoryLineType = value;
		}

		public ZBool SupportsNoCostApportionmentItem => LCHeader?.Parent?.SupportsNoCostApportionmentItem ?? ZBool.False;

		public ZBool LH_LandedCostHistoryLineType_Readonly => !SupportsNoCostApportionmentItem;

		public ZBool IsNoCostApportionmentItem => LH_LandedCostHistoryLineType == LandedCostType.NoCostApportionmentItem;

		#region Rounded Figures

		public ZDecimal CustomsValue
		{
			get { return UltimateDistributee != null ? UltimateDistributee.CustomsValue : ZDecimal.Zero; }
		}

		public int LocalCurrencyDecimals
		{
			get { return LCHeader.LocalCurrencyDecimals; }
		}

		public ZDecimal RoundedGroup1
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup1, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroup2
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup2, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroup3
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup3, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroup4
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup4, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroup5
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup5, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroup6
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroup6, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedGroupMisc
		{
			get { return LCHeader.RoundingHelper.Round(LH_LandedCostGroupMisc, LocalCurrencyDecimals); }
		}

		public ZDecimal RoundedPerUnitCustomsDisbursementCharges
		{
			get { return LCHeader.RoundingHelper.Round(PerUnitCustomsDisbursementCharges, PerUnitDecimals); }
		}

		public ZPropertyInfo RoundedPerUnitCustomsDisbursementChargesInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedPerUnitCustomsDisbursementCharges); }
		}

		public ZDecimal RoundedPerUnitLandingCost
		{
			get { return LCHeader.RoundingHelper.Round(PerUnitLandingCost, PerUnitDecimals); }
		}

		public ZPropertyInfo RoundedPerUnitLandingCostInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedPerUnitLandingCost); }
		}

		public ZDecimal GetRoundedLineValue(string code)
		{
			return LCHeader.RoundingHelper.Round(GetLineValue(code), LocalCurrencyDecimals);
		}

		public
#if DEBUG
		 virtual
#endif
		 ZDecimal RoundedPerUnitTotalCost
		{
			get { return LCHeader.RoundingHelper.Round(PerUnitTotalCost, PerUnitDecimals); }
		}

		public ZPropertyInfo RoundedPerUnitTotalCostInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedPerUnitTotalCost); }
		}

		public ZDecimal RoundedSellPrice1ExGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice1ExGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice1ExGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice1ExGST); }
		}

		public ZDecimal RoundedSellPrice2ExGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice2ExGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice2ExGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice2ExGST); }
		}

		public ZDecimal RoundedSellPrice3ExGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice3ExGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice3ExGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice3ExGST); }
		}

		public ZDecimal RoundedSellPrice1IncGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice1IncGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice1IncGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice1IncGST); }
		}

		public ZDecimal RoundedSellPrice2IncGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice2IncGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice2IncGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice2IncGST); }
		}

		public ZDecimal RoundedSellPrice3IncGST
		{
			get { return LCHeader.RoundingHelper.Round(SellPrice3IncGST, LocalCurrencyDecimals); }
		}

		public ZPropertyInfo RoundedSellPrice3IncGSTInfo
		{
			get { return GetZPropertyInfo(Schema.RoundedSellPrice3IncGST); }
		}

		#endregion

		#region Calculated Fields

		public
#if DEBUG
			virtual
#endif
			ZDecimal LandedCostPercentage
		{
			get
			{
				ZDecimal invoiceCostInLocalCurrencyCached = InvoiceCostInLocalCurrency;
				return (!invoiceCostInLocalCurrencyCached.IsEmpty) ? TotalLandingCost / invoiceCostInLocalCurrencyCached * 100.00m : 0m;
			}
		}

		public ZDecimal LandedTotalCostPercentage
		{
			get
			{
				var invoiceCostInLocalCurrencyCached = InvoiceCostInLocalCurrency;
				var totalCosts = TotalLandingCost + CustomsDisbursementCharges;

				return (!invoiceCostInLocalCurrencyCached.IsEmpty) ? totalCosts / invoiceCostInLocalCurrencyCached * 100.00m : 0m;
			}
		}

		public ZDecimal InvoiceCostInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null)
				{
					result = ultimateDistributeeCached.CostInLocalCurrency;
				}
				return result;
			}
		}

		public ZPropertyInfo InvoiceCostInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceCostInLocalCurrency); }
		}

		public ZDecimal UnitPriceInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null)
				{
					result = ultimateDistributeeCached.UnitPriceInInvoiceCurrency;
				}
				return result;
			}
		}

		public ZDecimal UnitPriceInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null && ultimateDistributeeCached.ItemCount != 0)
				{
					result = ultimateDistributeeCached.CostInLocalCurrency / ultimateDistributeeCached.ItemCount;
				}
				return result;
			}
		}

		public ZPropertyInfo UnitPriceInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.UnitPriceInLocalCurrency); }
		}

		public virtual ZDecimal CustomsDisbursementCharges
		{
			get { return LCHeader.CustomsChargeLCItemSettings.Sum(x => GetRoundedLineValue(x.CostType)); }
		}

		public virtual ZDecimal TotalLandingCost
		{
			get
			{
				return RoundedGroup1
					+ RoundedGroup2
					+ RoundedGroup3
					+ RoundedGroup4
					+ RoundedGroup5
					+ RoundedGroup6
					+ RoundedGroupMisc;
			}
		}

		public virtual ZDecimal TotalCost
		{
			get
			{
				ZDecimal result = CustomsDisbursementCharges + TotalLandingCost;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null)
				{
					result += ultimateDistributeeCached.CostInLocalCurrency;
				}
				return result;
			}
		}

		public virtual ZDecimal SellPrice1ExGST
		{
			get { return RoundedPerUnitTotalCost * (1 + EffectiveMarkUpPercentage1 / 100); }
		}

		public virtual ZDecimal SellPrice2ExGST
		{
			get { return RoundedPerUnitTotalCost * (1 + EffectiveMarkUpPercentage2 / 100); }
		}

		public virtual ZDecimal SellPrice3ExGST
		{
			get { return RoundedPerUnitTotalCost * (1 + EffectiveMarkUpPercentage3 / 100); }
		}

		public ZDecimal SellPrice1IncGST
		{
			get { return RoundedSellPrice1ExGST * (1 + GSTVATRate); }
		}

		public ZDecimal SellPrice2IncGST
		{
			get { return RoundedSellPrice2ExGST * (1 + GSTVATRate); }
		}

		public ZDecimal SellPrice3IncGST
		{
			get { return RoundedSellPrice3ExGST * (1 + GSTVATRate); }
		}

		public ZDecimal TotalCostWithMarkup1Applied
		{
			get { return LCHeader.RoundingHelper.Round(TotalCost * (1 + EffectiveMarkUpPercentage1 / 100), LocalCurrencyDecimals); }
		}

		/// <summary>
		/// This is to display GST/VAT amount from entry, but should not be part of the final cost.
		/// </summary>
		public ZDecimal EntryGSTVATAmount
		{
			get
			{
				ZDecimal result = 0m;

				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null)
				{
					result = ultimateDistributeeCached.GSTVATAmount;
				}
				return result;
			}
		}

		public virtual ZDecimal GSTVATRate
		{
			get
			{
				if (UltimateDistributee != null && UltimateDistributee.IsCapableOfCalculatingOwnGstVatRate)
				{
					return UltimateDistributee.CalculateOwnGstVatRate();
				}
				else
				{
					var company = LCHeader != null ? LCHeader.Company : GlbCompany.CurrentCompany;

					var mainGSTTaxID = new ZGuid(AccountingConfigurationRegistry.Instance.MainGSTTaxID.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
					var accTaxRate = mainGSTTaxID.IsValid ? Factory.Load<AccTaxRate>(mainGSTTaxID) : null;
					return accTaxRate == null ? ZDecimal.Zero : new ZDecimal(accTaxRate.GetRate(ZDate.Today) / 100m);
				}
			}
		}

		public virtual ZDecimal PerUnitCustomsDisbursementCharges
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null && ultimateDistributeeCached.ItemCount != 0)
				{
					result = CustomsDisbursementCharges / ultimateDistributeeCached.ItemCount;
				}
				return result;
			}
		}

		public virtual ZDecimal PerUnitLandingCost
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null && ultimateDistributeeCached.ItemCount != 0)
				{
					result = TotalLandingCost / ultimateDistributeeCached.ItemCount;
				}
				return result;
			}
		}

		public ZDecimal PerUnitTotalCost
		{
			get
			{
				ZDecimal result = 0m;
				IUltimateDistributee ultimateDistributeeCached = UltimateDistributee;
				if (ultimateDistributeeCached != null && ultimateDistributeeCached.ItemCount != 0)
				{
					result = TotalCost / ultimateDistributeeCached.ItemCount;
				}
				return result;
			}
		}

		#endregion

		#region New bindable properties

		public ZDateTime DateOfProcessing
		{
			get { return LCHeader == null ? ZDateTime.Empty : LCHeader.LT_DateOfProcessing; }
		}

		public ZPropertyInfo DateOfProcessingInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfProcessing); }
		}

		public ZString UniqueReferenceNumber
		{
			get { return LCHeader == null || LCHeader.Parent == null ? ZString.Empty : LCHeader.Parent.UniqueReferenceNumber; }
		}

		public ZPropertyInfo UniqueReferenceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.UniqueReferenceNumber); }
		}

		public ZString HumanReadableUltimateDistributeeCode
		{
			get { return UltimateDistributee == null ? ZString.Empty : UltimateDistributee.HumanReadableCode; }
		}

		public ZPropertyInfo HumanReadableUltimateDistributeeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.HumanReadableUltimateDistributeeCode); }
		}

		public ZDecimal EffectiveMarkUpPercentage1
		{
			get { return MarginPercentages.LCMarginPercentage1; }
		}

		public ZPropertyInfo EffectiveMarkUpPercentage1Info
		{
			get { return GetZPropertyInfo(Schema.EffectiveMarkUpPercentage1); }
		}

		public ZDecimal EffectiveMarkUpPercentage2
		{
			get { return MarginPercentages.LCMarginPercentage2; }
		}

		public ZPropertyInfo EffectiveMarkUpPercentage2Info
		{
			get { return GetZPropertyInfo(Schema.EffectiveMarkUpPercentage2); }
		}

		public ZDecimal EffectiveMarkUpPercentage3
		{
			get { return MarginPercentages.LCMarginPercentage3; }
		}

		public ZPropertyInfo EffectiveMarkUpPercentage3Info
		{
			get { return GetZPropertyInfo(Schema.EffectiveMarkUpPercentage3); }
		}

		public ZString InvoiceNumber
		{
			get { return UltimateDistributee != null ? UltimateDistributee.InvoiceNumber : ZString.Empty; }
		}

		public ZString SupplierName
		{
			get { return UltimateDistributee != null ? UltimateDistributee.SupplierName : ZString.Empty; }
		}

		public ZString InvoiceCurrencyCode
		{
			get { return UltimateDistributee != null ? UltimateDistributee.InvoiceCurrencyCode : ZString.Empty; }
		}

		public ZString OrderNumber
		{
			get { return UltimateDistributee != null ? UltimateDistributee.OrderNumber : ZString.Empty; }
		}

		public ZInt OrderLineNumber
		{
			get { return UltimateDistributee != null ? UltimateDistributee.OrderLineNumber : ZInt.Zero; }
		}

		public ZString LineDescription
		{
			get { return UltimateDistributee != null ? UltimateDistributee.LineDescription : ZString.Empty; }
		}

		public ZString InvoiceNumberSupplierNameGroupByString
		{
			get { return InvoiceNumber + "_" + SupplierName; }
		}

		public ZDecimal LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency
		{
			get { return UltimateDistributee != null ? UltimateDistributee.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency : ZDecimal.Zero; }
		}

		public ZDecimal InvoiceQuantity
		{
			get { return UltimateDistributee != null ? UltimateDistributee.ItemCount : ZDecimal.Zero; }
		}

		public ZString InvoiceUQ
		{
			get { return UltimateDistributee != null ? UltimateDistributee.InvoiceUQ : ZString.Empty; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return UltimateDistributee != null ? UltimateDistributee.CustomsQuantity : ZDecimal.Zero; }
		}

		public ZString CustomsUQ
		{
			get { return UltimateDistributee != null ? UltimateDistributee.CustomsUQ : ZString.Empty; }
		}

		public ZString TariffNumber
		{
			get { return UltimateDistributee != null ? UltimateDistributee.TariffNumber : ZString.Empty; }
		}

		public ZString CountryOfOriginCode
		{
			get { return UltimateDistributee != null ? UltimateDistributee.CountryOfOriginCode : ZString.Empty; }
		}

		public ZString DutyRateDescription
		{
			get { return UltimateDistributee != null ? UltimateDistributee.DutyRateDescription : ZString.Empty; }
		}

		public ZDecimal Weight
		{
			get { return UltimateDistributee != null ? UltimateDistributee.Weight : ZDecimal.Zero; }
		}

		public ZString WeightUQ
		{
			get { return UltimateDistributee != null ? UltimateDistributee.WeightUQ : ZString.Empty; }
		}

		public ZDecimal Volume
		{
			get { return UltimateDistributee != null ? UltimateDistributee.Volume : ZDecimal.Zero; }
		}

		public ZString VolumeUQ
		{
			get { return UltimateDistributee != null ? UltimateDistributee.VolumeUQ : ZString.Empty; }
		}

		public ZDecimal LinePriceInInvoiceCurrency
		{
			get { return UltimateDistributee != null ? UltimateDistributee.LinePriceInInvoiceCurrency : ZDecimal.Zero; }
		}

		public ZDecimal LinePriceInLocalCurrency
		{
			get { return UltimateDistributee != null ? UltimateDistributee.CostInLocalCurrency : ZDecimal.Zero; }
		}

		public ZString ProductCode
		{
			get { return UltimateDistributee != null ? UltimateDistributee.ProductCode : ZString.Empty; }
		}

		public ZString ProductDepartment
		{
			get { return UltimateDistributee != null ? UltimateDistributee.ProductDepartment : ZString.Empty; }
		}

		public ZString ProductDivision
		{
			get { return UltimateDistributee != null ? UltimateDistributee.ProductDivision : ZString.Empty; }
		}

		#endregion

		#region New Methods

		public LCMarginPercentages GetLCMarginPercentagesForFallBack()
		{
			LCMarginPercentages result = new LCMarginPercentages();
			result.LCMarginPercentage1 = LH_LandedCostMarginPercent1;
			result.LCMarginPercentage2 = LH_LandedCostMarginPercent2;
			result.LCMarginPercentage3 = LH_LandedCostMarginPercent3;
			return result;
		}

		public bool HasItsOwnMarginPercentages
		{
			get
			{
				return !LH_LandedCostMarginPercent1.IsEmpty
					|| !LH_LandedCostMarginPercent2.IsEmpty
					|| !LH_LandedCostMarginPercent3.IsEmpty;
			}
		}

		#endregion

		#region Related Objects

		LandedLineCostItemCollection landedLineCostItems;
		[ChildEditable(true)]
		public LandedLineCostItemCollection LandedLineCostItems
		{
			get
			{
				if (landedLineCostItems == null)
				{
					landedLineCostItems = GetNewLandedLineCostItemCollection();
					landedLineCostItems.Load();
					RegisterEditableChildObject(landedLineCostItems);
				}
				return landedLineCostItems;
			}
		}

		protected virtual LandedLineCostItemCollection GetNewLandedLineCostItemCollection()
		{
			return new LandedLineCostItemCollection(this);
		}

		public LandedCostHeader LCHeader
		{
			get
			{
				if (fLCHeader == null)
				{
					fLCHeader = (LandedCostHeader)Factory.Load(typeof(LandedCostHeader), LH_LT);
				}
				return fLCHeader == null || fLCHeader.IsDeleted ? null : fLCHeader;
			}
		}
		LandedCostHeader fLCHeader;

		public IUltimateDistributee UltimateDistributee
		{
			get
			{
				if (ultimateDistributee == null)
				{
					ultimateDistributee = (IUltimateDistributee)UltimateDistributeeLoader.LoadBusinessObject(Factory, LH_ParentTableCode, LH_ParentID);
				}
				return ultimateDistributee;
			}
#if DEBUG
			set
			{
				ultimateDistributee = value;
			}
#endif
		}
		IUltimateDistributee ultimateDistributee;

		TypeLoaderCollection UltimateDistributeeLoader => ultimateDistributeeLoader ??= GetUltimateDistributeeLoaders();
		TypeLoaderCollection ultimateDistributeeLoader;

		protected virtual TypeLoaderCollection GetUltimateDistributeeLoaders()
		{
			var loader = new TypeLoaderCollection();
			loader.Add(ObjectFactory.GetType<Integration.Customs.IBaseJobComInvoiceLine>());
			loader.Add(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());

			return loader;
		}

		LCMarginPercentages MarginPercentages
		{
			get { return LCMarginPercentFallBackCalculator.GetLCMarginPercentagesForFallBack(); }
		}

		LCMarginPercentFallBackCalculator LCMarginPercentFallBackCalculator
		{
			get
			{
				if (fLCMarginPercentFallBackCalculator == null)
				{
					fLCMarginPercentFallBackCalculator = new LCMarginPercentFallBackCalculator(this);
				}
				return fLCMarginPercentFallBackCalculator;
			}
		}
		LCMarginPercentFallBackCalculator fLCMarginPercentFallBackCalculator;

		public LandedCostingCustomsFeeCollection CustomsFees
		{
			get
			{
				if (customsFees == null)
				{
					customsFees = new LandedCostingCustomsFeeCollection(Factory);

					if (UltimateDistributee != null)
					{
						customsFees.LoadCollection(UltimateDistributee.Fees);
					}
				}
				return customsFees;
			}
		}
		LandedCostingCustomsFeeCollection customsFees;

		OrgSupplierPart Product
		{
			get { return Factory.Load<OrgSupplierPart>(LH_OP); }
		}

		#endregion

		#region Calculate Duty

		internal void CalculateDutyAmountIfNecessary(string dutyType)
		{
			if (UltimateDistributee != null && ShouldCalculateDutyFromLH_DutyPercent)
			{
				SetLineValue(LH_DutyPercent * UltimateDistributee.CustomsValue / 100, dutyType);
			}
		}

		internal bool ShouldCalculateDutyFromLH_DutyPercent
		{
			get { return IsEstimatedLC && !IsDutyRateKnownFromProduct; }
		}

		public bool LH_DutyPercent_ReadOnly
		{
			get { return !IsEstimatedLC || IsDutyRateKnownFromProduct; }
		}

		bool IsDutyRateKnownFromProduct
		{
			get
			{
				return LCHeader != null
					&& CountriesWithDutyCalculationForEstimatedLC.Contains(LCHeader.Company.GC_RN_NKCountryCode)
					&& Product != null
					&& Product.IsClassifiedFor(LCHeader.Company.GC_RN_NKCountryCode, new ZString[] { Customs.Common.ClassificationType.IMP, Customs.Common.ClassificationType.Both });
			}
		}

		static List<string> CountriesWithDutyCalculationForEstimatedLC
		{
			get
			{
				if (countriesWithDutyCalculationForEstimatedLC == null)
				{
					countriesWithDutyCalculationForEstimatedLC = new List<string>();
					countriesWithDutyCalculationForEstimatedLC.Add(Core.Constants.CountryCodes.Australia);
				}
				return countriesWithDutyCalculationForEstimatedLC;
			}
		}
		[ThreadStatic]
		static List<string> countriesWithDutyCalculationForEstimatedLC;

		public void DefaultDutyRateFromHeaderIfNecessary()
		{
			if (IsEstimatedLC && LCHeader != null && !IsDutyRateKnownFromProduct)
			{
				LH_DutyPercent = LCHeader.LT_DefaultEstimatedDutyRate;
			}
		}

		public bool IsEstimatedLC
		{
			get { return LH_LandedCostHistoryLineType == LandedCostType.Estimated; }
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new LandedCostHistoryFetchStrategy(this);
		}

		#region Implementation of IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)LH_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(LandedCostHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)LH_LTInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(LandedLineCostItem), LandedLineCostItemSchema.LZ_LH);
			}
		}

		#endregion
	}
}
