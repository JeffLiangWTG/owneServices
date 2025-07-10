using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using InvoiceLineDependentCollection = Enterprise.Customs.Business.InvoiceLineDependentCollection;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.TR.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

		public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

		public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

		public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public override ZGuid JZ_JE
		{
			get => base.JZ_JE;
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;

				if (oldValue != JZ_JE && !IsCopying)
				{
					if (base.JobComInvoiceLines is JobComInvoiceLineViewCollection)
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							invoiceLine.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				var oldValue = base.JZ_RX_NKInvoice_Currency;
				base.JZ_RX_NKInvoice_Currency = value;
				if (oldValue != JZ_RX_NKInvoice_Currency && !IsCopying)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData($"{ResourceStringPrefix}JZ_RelatedIndicator", Caption = "Seller/Buyer Relation Code", ShortCaption = "S/B Rel.Code")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.RelationCodeList))]
		public override ZString JZ_RelatedIndicator { get => base.JZ_RelatedIndicator; set => base.JZ_RelatedIndicator = value; }

		public override ZDecimal JZ_InvoiceAmount
		{
			get => base.JZ_InvoiceAmount;
			set
			{
				var oldValue = JZ_InvoiceAmount;
				base.JZ_InvoiceAmount = value;
				if (oldValue != JZ_InvoiceAmount)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				var oldValue = JZ_ValuationDateOverride;
				base.JZ_ValuationDateOverride = value;
				if (!IsCopying && oldValue != JZ_ValuationDateOverride)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		protected override ZString GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge)
		{
			ZString result;
			if (ForeignTotalCharge is InvoiceCharge foreignTotalCharge && TRIncotermChargeCodeList.IsForeignCharge(chargeType.Code))
			{
				result = foreignTotalCharge.J7_RX_NKCurrency;
			}
			else
			{
				result = base.GetDefaultCurrencyCode(chargeType, charge);
			}

			return result;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new InvoiceChargeCollection(this);

		IJobComInvChargeCollection<BaseInvoiceCharge> fOtherCharges;
		[ChildEditable]
		public IJobComInvChargeCollection<BaseInvoiceCharge> OtherCharges
		{
			get
			{
				if (fOtherCharges == null)
				{
					fOtherCharges = new InvoiceChargeCollection(this, TRIncotermChargeCodeListStatic.IsOtherChargeThanLocalOrForeign);
					RegisterEditableChildObject(fOtherCharges);
				}
				return fOtherCharges;
			}
		}

		#region Local Charges

		IJobComInvChargeCollection<BaseInvoiceCharge> localCharges;
		[ChildEditable]
		public IJobComInvChargeCollection<BaseInvoiceCharge> LocalCharges
		{
			get
			{
				if (localCharges == null)
				{
					localCharges = new InvoiceChargeCollection(this, TRIncotermChargeCodeListStatic.IsLocalCharge);
					RegisterEditableChildObject(localCharges);
				}
				return localCharges;
			}
		}

		[ResourceStringData($"{ResourceStringPrefix}LocalChargesExpected", Caption = "Total Expected")]
		public ZDecimal LocalChargesExpected => LocalTotalCharge?.J7_Amount ?? ZDecimal.Zero;
		public ZPropertyInfo LocalChargesExpectedInfo => GetZPropertyInfo(nameof(LocalChargesExpected));

		internal void LocalChargesEnteredChanged()
		{
			fLocalChargesEntered = null;
		}

		[ResourceStringData($"{ResourceStringPrefix}LocalChargesEntered", Caption = "Lines Entered")]
		public ZDecimal LocalChargesEntered => (
			fLocalChargesEntered ?? (fLocalChargesEntered = Charges.Where(TRIncotermChargeCodeListStatic.IsLocalCharge).Sum(r => r.J7_Amount))
		).Value;
		public ZPropertyInfo LocalChargesEnteredInfo => GetZPropertyInfo(nameof(LocalChargesEntered));
		ZDecimal? fLocalChargesEntered;

		[ResourceStringData($"{ResourceStringPrefix}LocalChargesBalance", Caption = "Balance")]
		public ZDecimal LocalChargesBalance => LocalChargesExpected - LocalChargesEntered;
		public ZPropertyInfo LocalChargesBalanceInfo => GetZPropertyInfo(nameof(LocalChargesBalance));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		[ResourceStringData($"{ResourceStringPrefix}LocalChargesCurrency", Caption = "Local Currency")]
		public ZGuid LocalChargesCurrency => LocalTotalCharge?.Currency.PK ?? ZGuid.Empty;
		public ZPropertyInfo LocalChargesCurrencyInfo => GetZPropertyInfo(nameof(LocalChargesCurrency));

		#endregion

		#region Foreign Charges

		IJobComInvChargeCollection<BaseInvoiceCharge> foreignCharges;
		[ChildEditable]
		public IJobComInvChargeCollection<BaseInvoiceCharge> ForeignCharges
		{
			get
			{
				if (foreignCharges == null)
				{
					foreignCharges = new InvoiceChargeCollection(this, TRIncotermChargeCodeListStatic.IsForeignCharge);
					RegisterEditableChildObject(foreignCharges);
				}
				return foreignCharges;
			}
		}

		[ResourceStringData($"{ResourceStringPrefix}ForeignChargesExpected", Caption = "Total Expected")]
		public ZDecimal ForeignChargesExpected => ForeignTotalCharge?.J7_Amount ?? ZDecimal.Zero;
		public ZPropertyInfo ForeignChargesExpectedInfo => GetZPropertyInfo(nameof(ForeignChargesExpected));

		internal void ForeignChargesEnteredChanged()
		{
			fForeignChargesEntered = null;
		}

		[ResourceStringData($"{ResourceStringPrefix}ForeignChargesEntered", Caption = "Lines Entered")]
		public ZDecimal ForeignChargesEntered => (
			fForeignChargesEntered ?? (fForeignChargesEntered = CalculateForeignChargesEntered())
		).Value;
		ZDecimal? fForeignChargesEntered;

		public ZPropertyInfo ForeignChargesEnteredInfo => GetZPropertyInfo(nameof(ForeignChargesEntered));

		ZDecimal CalculateForeignChargesEntered()
		{
			var result = ZDecimal.Zero;
			if (ForeignTotalCharge is InvoiceCharge foreignTotalCharge && foreignTotalCharge.Currency is RefCurrency foreignTotalCurrency)
			{
				var localResult = new Money(0, LocalCurrency);
				var foreignCharges = Charges.Where(TRIncotermChargeCodeListStatic.IsForeignCharge).Where(charge => charge.J7_Amount != 0).ToArray();
				var currencyConverter = CurrencyConverter;
				foreach (var foreignCharge in foreignCharges)
				{
					localResult = currencyConverter.Add(localResult, foreignCharge.Money);
				}
				result = currencyConverter.ConvertExact(localResult, foreignTotalCurrency).Amount;
			}
			return result;
		}

		[ResourceStringData($"{ResourceStringPrefix}ForeignChargesBalance", Caption = "Balance")]
		public ZDecimal ForeignChargesBalance => ForeignChargesExpected - ForeignChargesEntered;
		public ZPropertyInfo ForeignChargesBalanceInfo => GetZPropertyInfo(nameof(ForeignChargesBalance));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		[ResourceStringData($"{ResourceStringPrefix}ForeignChargesCurrency", Caption = "Foreign Currency")]
		public ZGuid ForeignChargesCurrency => ForeignTotalCharge?.Currency?.PK ?? ZGuid.Empty;
		public ZPropertyInfo ForeignChargesCurrencyInfo => GetZPropertyInfo(nameof(ForeignChargesCurrency));

		#endregion

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			JobComInvoiceHeaderValidation result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderValidation(this);
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}
			return result;
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			JobComInvoiceHeaderLookups result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderLookups(this);
			}
			else
			{
				result = new JobComInvoiceHeaderLookups(this);
			}
			return result;
		}

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

		protected override ExchangeRateType RateTypeCore => IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Turkey;

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();

			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		const string ResourceStringPrefix = "Enterprise.Customs.TR.Business.Declaration.JobComInvoiceHeader|";

		internal void OnChargeTypeSet(InvoiceCharge chargeAdded)
		{
			if (chargeAdded.IsOtherChargeThanLocalOrForeign() && !OtherCharges.Contains(chargeAdded))
			{
				OtherCharges.Add(chargeAdded);
			}
			else if (chargeAdded.IsForeignCharge() && !ForeignCharges.Contains(chargeAdded))
			{
				ForeignCharges.Add(chargeAdded);
			}
			else if (chargeAdded.IsLocalCharge() && !LocalCharges.Contains(chargeAdded))
			{
				LocalCharges.Add(chargeAdded);
			}
		}

		CachedProperty<InvoiceCharge> localTotalChargeCache;
		InvoiceCharge LocalTotalCharge => Factory.GetValue(ref localTotalChargeCache, () => Charges.GetCharge(TRIncotermChargeCodeList.Codes.LocalTotalCharges).SingleOrDefault());

		CachedProperty<InvoiceCharge> foreignTotalChargeCache;
		InvoiceCharge ForeignTotalCharge => Factory.GetValue(ref foreignTotalChargeCache, () => Charges.GetCharge(TRIncotermChargeCodeList.Codes.TotalForeignCharges).SingleOrDefault());

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var result = ZDateTime.Today;

				if (JobDeclaration != null)
				{
					if (JobDeclaration.JE_ValuationDate.IsValid && !JobDeclaration.JE_ValuationDate.IsEmpty)
					{
						result = JobDeclaration.JE_ValuationDate;
					}
				}

				return result;
			}
		} 
	}
}
