using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[SingleObjectAroundARow]
	public class USImportEntryInvoiceLine : AutoUSImportEntryInvoiceLine, IInvoiceLine, IInvoiceHeader, ICurrencyConverterDataProviderWithFixedExRates, IDeclaration
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public USImportEntryInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SuspendValidation();
			Factory.AddFetchHint(USImportEntryInvoiceLineSchema.PK, USL_ParentID);
			Factory.AddFetchHint(USImportEntryInvoiceLineSchema.PK, USL_JI_ParentProduct);
		}

		public ImportInvoiceLineHelper ImportHelper => importHelper ?? (importHelper = new ImportInvoiceLineHelper(this));
		ImportInvoiceLineHelper importHelper;
		public USImportEntryInvoiceLine ParentTariffLine
		{
			get
			{
				if (parentTariffLineCached == null)
				{
					parentTariffLineCached = new CachedValue<USImportEntryInvoiceLine>(() => USL_ParentID == PK ? null : Factory.Load<USImportEntryInvoiceLine>(USL_ParentID));
				}
				return parentTariffLineCached.Value;
			}
		}
		CachedValue<USImportEntryInvoiceLine> parentTariffLineCached;

		public USImportEntryInvoiceLine ProductParentTariffLine
		{
			get
			{
				if (productParentTariffLineCached == null)
				{
					productParentTariffLineCached = new CachedValue<USImportEntryInvoiceLine>(() => USL_JI_ParentProduct == PK ? null : Factory.Load<USImportEntryInvoiceLine>(USL_JI_ParentProduct));
				}
				return productParentTariffLineCached.Value;
			}
		}
		CachedValue<USImportEntryInvoiceLine> productParentTariffLineCached;

		ZString IInvoiceLine.BaseJI_PartNo => USL_PartNo;

		public ZString JI_PartNo
		{
			get
			{
				if (!jI_PartNoCached.HasValue)
				{
					jI_PartNoCached = ImportHelper.GetPartNo();
				}
				return jI_PartNoCached.Value;
			}
		}
		ZString? jI_PartNoCached;

		public ZShort JI_LineNo => USL_LineNo;

		public ZString JI_Tariff => USL_Tariff;

		public ZString US_SupTariff => USL_SupTariff;

		public ZString US_SupAdditionalTariff1 => ZString.Empty;
		public ZString US_SupAdditionalTariff2 => ZString.Empty;
		public ZString US_SupAdditionalTariff3 => ZString.Empty;
		public ZString US_SupAdditionalTariff4 => ZString.Empty;
		public ZString US_SupAdditionalTariff5 => ZString.Empty;
		public ZDecimal US_SupAdditionalTariff1Qty => ZDecimal.Zero;
		public ZString US_SupAdditionalTariff1UQ => ZString.Empty;
		public ZDecimal US_SupAdditionalTariff2Qty => ZDecimal.Zero;
		public ZString US_SupAdditionalTariff2UQ => ZString.Empty;
		public ZDecimal US_SupAdditionalTariff3Qty => ZDecimal.Zero;
		public ZString US_SupAdditionalTariff3UQ => ZString.Empty;
		public ZDecimal US_SupAdditionalTariff4Qty => ZDecimal.Zero;
		public ZString US_SupAdditionalTariff4UQ => ZString.Empty;
		public ZDecimal US_SupAdditionalTariff5Qty => ZDecimal.Zero;
		public ZString US_SupAdditionalTariff5UQ => ZString.Empty;

		bool IInvoiceLine.HasEmptySupTariff => US_SupTariff.IsEmpty || US_SupTariff == TariffViewAsCodeDescription.NotApplicableCode;

		public ZString InvoiceNumber => USL_InvoiceNumber;

		#region IInvoiceHeader Members
		public IInvoiceHeader InvoiceHeader => this;
		public IDeclaration Declaration => this;
		ZGuid Customs.Business.IBaseInvoiceHeader.PK => USL_JZ;
		ZShort Customs.Business.IBaseInvoiceHeader.JZ_InvoiceDisplaySequence => USL_InvoiceDisplaySequence;
		ZString Customs.Business.IBaseInvoiceHeader.JZ_InvoiceNumber => InvoiceNumber;
		IComparable Customs.Business.IBaseInvoiceHeader.OrderByColumn => InvoiceNumber;
		Customs.Business.IBaseInvoiceHeader Customs.Business.IBaseInvoiceLine.InvoiceHeader => InvoiceHeader;
		#endregion
		public ZBool IsSetXLine
		{
			get
			{
				if (!isSetXLineCached.HasValue)
				{
					isSetXLineCached = ImportHelper.GetIsSetXLine();
				}
				return isSetXLineCached.Value;
			}
		}
		ZBool? isSetXLineCached;
		public ZBool IsSetVLine
		{
			get
			{
				if (!isSetVLineCached.HasValue)
				{
					isSetVLineCached = ImportHelper.GetIsSetVLine();
				}
				return isSetVLineCached.Value;
			}
		}
		ZBool? isSetVLineCached;
		public ZBool IsVParentLine
		{
			get
			{
				if (!isVParentLineCached.HasValue)
				{
					isVParentLineCached = ImportHelper.GetIsVParentLine();
				}
				return isVParentLineCached.Value;
			}
		}
		ZBool? isVParentLineCached;

		public ZDecimal US_SupQty1 => USL_SupQty1;

		public ZString US_SupUQ1 => USL_SupUQ1;

		bool IInvoiceLine.HasDeclaration => true;
		bool IInvoiceLine.IsRecon => false;
		IEnumerable<IInvoiceLine> IInvoiceLine.ChildLines => ChildLines;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryInvoiceLine[] ChildLines
		{
			get
			{
				if (childLines == null)
				{
					if (USL_IsParent)
					{
						var query = new ZQuery(USImportEntryInvoiceLineSchema.USL_ParentID, PK);
						query.AddToFilter(JoinCondition.And, USImportEntryInvoiceLineSchema.USL_JZ, SQLComparisonOperator.Equal, USL_JZ);
						query.AddToFilter(JoinCondition.And, USImportEntryInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, PK);
						childLines = Factory.Load<USImportEntryInvoiceLine>(query).OrderBy(x => x.USL_LineNo).ToArray();
					}
					else
					{
						childLines = Array.Empty<USImportEntryInvoiceLine>();
					}
				}
				return childLines;
			}
		}
		USImportEntryInvoiceLine[] childLines;

		IEnumerable<IInvoiceLine> IInvoiceLine.ChildVLines => ChildVLines;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryInvoiceLine[] ChildVLines => childVLines ?? (childVLines = ImportHelper.GetChildVLines().Cast<USImportEntryInvoiceLine>().ToArray());
		USImportEntryInvoiceLine[] childVLines;

		public ZString US_ZoneStatus => USL_ZoneStatus;
		public ZDateTime US_PrivilegedStatusDate => USL_PrivilegedStatusDate;
		public ZDecimal JI_CustomsQuantity => USL_CustomsQuantity;
		public ZString JI_CustomsUnitQty => USL_CustomsUnitQty;
		public ZDecimal JI_CustomsSecondQuantity => USL_CustomsSecondQuantity;
		public ZString JI_CustomsSecondUnitQty => USL_CustomsSecondUnitQty;
		public ZDecimal US_SupQty2 => USL_SupQty2;
		public ZString US_SupUQ2 => USL_SupUQ2;
		public ZDecimal US_SupQty3 => USL_SupQty3;
		public ZString US_SupUQ3 => USL_SupUQ3;
		public ZDecimal US_98GoodsValue => USL_98GoodsValue;
		public ZDecimal US_98ValueInvCurr => USL_98ValueInvCurr;
		public ZDecimal JI_CustomsThirdQuantity => USL_CustomsThirdQuantity;
		public ZString JI_CustomsThirdUnitQty => USL_CustomsThirdUnitQty;

		public ZBool IsVChildLine
		{
			get
			{
				if (!isVChildLineCached.HasValue)
				{
					isVChildLineCached = ImportHelper.GetIsVChildLine();
				}
				return isVChildLineCached.Value;
			}
		}
		ZBool? isVChildLineCached;

		ZDecimal IInvoiceLine.JI_CustomsValue => throw new NotSupportedException();

		public ZDecimal US_CustomsValue => USL_CustomsValue;

		public ZDecimal TotalOriginalGoodsValueInUSD
		{
			get
			{
				if (!totalOriginalGoodsValueInUSDCached.HasValue)
				{
					totalOriginalGoodsValueInUSDCached = ImportHelper.GetTotalOriginalGoodsValueInUSD();
				}
				return totalOriginalGoodsValueInUSDCached.Value;
			}
		}
		ZDecimal? totalOriginalGoodsValueInUSDCached;

		public override RefCurrency Invoice_Currency
		{
			get
			{
				if (invoice_CurrencyCached == null)
				{
					invoice_CurrencyCached = new CachedValue<RefCurrency>(() => base.Invoice_Currency);
				}
				return invoice_CurrencyCached.Value;
			}
		}
		CachedValue<RefCurrency> invoice_CurrencyCached;

		public CurrencyConverter CurrencyConverter => Factory.GetCachedValue("USCurrencyConverter" + USL_JZ.ToString(), () => new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this));

		public ZDate EffectiveDateForDutyRate
		{
			get
			{
				if (!effectiveDateForDutyRateCached.HasValue)
				{
					effectiveDateForDutyRateCached = ImportHelper.GetEffectiveDateForDutyRate();
				}
				return effectiveDateForDutyRateCached.Value;
			}
		}
		ZDate? effectiveDateForDutyRateCached;
		public ZDate FTZAdmissionEffectiveDateForDutyRate => Factory.GetCachedValue("USFTZAdmissionEffectiveDateForDutyRate" + USL_JE.ToString(), () => IsFTZAdmission && USL_DateOfArrival.IsValid ? USL_DateOfArrival.Date : ZDate.Invalid);
		public ZDate ImportEffectiveDateForDutyRate => Factory.GetCachedValue("ImportEffectiveDateForDutyRate" + USL_JE.ToString(), () => new DutyFeeDateCalculator().GetDutyFeeDate(this));
		public USCTariff ImportSupTariff => ImportHelper.ImportSupTariff;
		public USCTariff ImportSupAdditionalTariff1 => null;

		public ZString JZ_IncoTerm => USL_IncoTerm;

		#region ICurrencyConverterDataProviderWithFixedExRates Members
		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode => USL_IsJZ_InvoiceCurrExRateUserEnterable ? Invoice_Currency?.RX_Code ?? ZString.Empty : ZString.Empty;

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate => USL_InvoiceCurrExRate;

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation => USL_ValuationDate;

		ExchangeRateType ICurrencyConverterDataProvider.RateType => ExchangeRateType.Customs;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => 0;

		GlbCompany ICurrencyConverterDataProvider.Company => Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride => Core.Constants.CurrencyCodes.UnitedStates;

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride => true;

		#endregion
		ZGuid IDeclaration.PK => USL_JE;
		public ZDate US_DutyCalcDate => USL_DutyCalcDate;
		bool IDeclaration.ShouldCalculateMPFAndDutyDate => false;

		public ZBool IsExWarehouse => USL_IsExWarehouse;
		public bool IsFormalImport => USL_IsFormalImport;
		public bool IsACE => USL_IsACE;
		public bool IsQuota => USL_IsQuota;
		public bool IsACECargoCertificationMode => USL_IsACECargoCertificationMode;
		public bool IsFTZAdmission => USL_IsFTZAdmission;
		public bool IsBorderMovement => USL_IsBorderMovement;
		public ZDateTime US_ITDate => USL_ITDate;
		public ZDateTime US_EstimatedEntryDate => USL_EstimatedEntryDate;
		public ZDateTime US_PreliminaryStatementPrintDate => USL_PreliminaryStatementPrintDate;
		public ZDateTime US_EntryDate => USL_EntryDate;
		public ZDateTime US_PresentationDate => USL_PresentationDate;
		public ZDateTime JE_EntryAuthorisationDate => USL_EntryAuthorisationDate;
		public ZString US_SetInd => USL_SetInd;
		public ZString US_SecondarySPI => USL_SecondarySPI;
		IInvoiceLine IInvoiceLine.ParentTariffLine => ParentTariffLine;
		IInvoiceLine IInvoiceLine.ProductParentTariffLine => ProductParentTariffLine;

		public override void Delete()
		{
			throw new NotSupportedException("Cannot delete a view data");
		}
	}
}
