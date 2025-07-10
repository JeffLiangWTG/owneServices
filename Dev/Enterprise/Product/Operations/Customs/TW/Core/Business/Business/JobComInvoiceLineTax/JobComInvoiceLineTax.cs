using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineTax : Customs.Business.JobComInvoiceLineTax,
		Integration.Customs.TW.IJobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool SupportsCloneCore() => true;

		public void SetDefaultPaymentMethod()
		{
			var result = ZString.Empty;
			var cusProcedure = InvoiceLine?.CusProcedure;
			if (cusProcedure != null && !JLT_Type.IsEmpty)
			{
				var paymentMethod = DutyCalculationHelper.GetPaymentMethod(cusProcedure, TariffRate, EffectiveAssessmentDate);
				result = paymentMethod;
			}
			JLT_MethodOfPayment = result;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_Type", Caption = "Type", FullDescription = "The types of the duties other than tariffs.")]
		public override ZString JLT_Type
		{
			get => base.JLT_Type;
			set
			{
				var oldValue = JLT_Type;
				base.JLT_Type = value;
				if (!IsCopying && oldValue != JLT_Type)
				{
					DefaultTariff();
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null && !invoiceLine.IsValidationSuspended)
					{
						invoiceLine.Validation.ValidateJI_Tariff();
					}
				}
			}
		}

		#region Tariff Code Defaulting from Tariff Type

		void DefaultTariff()
		{
			if (!IsTariffDefaultingSuspended)
			{
				var tariff = ZString.Empty;
				var tariffType = UniversalTariffType;
				var parentInvoiceLine = InvoiceLine;
				if (tariffType != null && parentInvoiceLine != null)
				{
					var tariffsRelated = new List<TariffView>();
					var relatedTariffsFound = parentInvoiceLine.GetValidRefCusTariffSortedDictionary().TryGetValue(tariffType, out tariffsRelated);
					if (relatedTariffsFound && tariffsRelated.Count == 1)
					{
						tariff = tariffsRelated[0].ZZ1_TariffCode.Left(JLT_TariffInfo.MaxLength);
					}
				}
				JLT_Tariff = tariff;
			}
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_TypeDescription", Caption = "Type Desc.")]
		public ZString JLT_TypeDescription => Lookups.TypeList.GetDescriptionFromCode(JLT_Type) ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.TariffCollection))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_Tariff", Caption = "Tariff", FullDescription = "The types of the goods belonging to the duties other than tariffs.")]
		public override ZString JLT_Tariff
		{
			get => base.JLT_Tariff;
			set
			{
				var oldValue = base.JLT_Tariff;
				base.JLT_Tariff = value;
				if (!IsCopying && oldValue != JLT_Tariff)
				{
					JLT_BaseQuantityUQ = !JLT_Tariff.IsEmpty ? (UniversalTariff?.GetSpecificUOM(Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType) ?? ZString.Empty) : ZString.Empty;
					SetDefaultPaymentMethod();
					SetDefaultQuantity(EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(InvoiceLine));
				}
			}
		}

		public TariffView UniversalTariff
		{
			get
			{
				var type = JLT_Type;
				var tariff = JLT_Tariff;
				return !type.IsEmpty && !tariff.IsEmpty ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CustomsCountryCode, type, tariff, EffectiveAssessmentDate) : null;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_TariffDesc", Caption = "Tariff Desc.")]
		public ZString JLT_TariffDesc => UniversalTariff?.FullTariffDescription(EffectiveAssessmentDate, false, false) ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_MethodOfPayment", Caption = "Payment Method", FullDescription = "The payment method for the duties other than tariffs.")]
		public override ZString JLT_MethodOfPayment { get => base.JLT_MethodOfPayment; set => base.JLT_MethodOfPayment = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|PaymentMethodDescription", Caption = "Description")]
		public ZString PaymentMethodDescription => Lookups.MOPList.GetDescriptionFromCode(JLT_MethodOfPayment) ?? ZString.Empty;

		public ZDateTime EffectiveAssessmentDate => InvoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Today;

		ZString CustomsCountryCode => Core.Constants.CountryCodes.Taiwan;

		public void SetDefaultQuantity(Dictionary<string, decimal> unitOfMeasureValueList)
		{
			var unitQty = JLT_BaseQuantityUQ;
			if (unitOfMeasureValueList != null && !unitQty.IsEmpty && unitOfMeasureValueList.ContainsKey(unitQty))
			{
				JLT_BaseQuantity = unitOfMeasureValueList[unitQty];
			}
		}

		[ReadOnlyMember(nameof(JLT_BaseQuantityReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_BaseQuantity", Caption = "Quantity", FullDescription = "The quantity of specific tax rate for the duties other than tariffs.")]
		public override ZDecimal JLT_BaseQuantity { get => base.JLT_BaseQuantity; set => base.JLT_BaseQuantity = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|JLT_BaseQuantityUQ", Caption = "UQ", FullDescription = "The UQ of specific tax rate for the duties other than tariffs.")]
		public override ZString JLT_BaseQuantityUQ { get => base.JLT_BaseQuantityUQ; set => base.JLT_BaseQuantityUQ = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLineTax|FormattedTariffRate", Caption = "Rate", FullDescription = "The Ad Valorem Tax rate for the duties other than tariffs.")]
		public ZString FormattedTariffRate => TariffRate?.ZZ2_RateFormula ?? ZString.Empty;

		public ZPropertyInfo FormattedTariffRateInfo => GetZPropertyInfo(nameof(FormattedTariffRate));

		RateView TariffRate => Factory.GetValue(ref rateViewCached, delegate
					{
						var criteria = new SpecificRateSelectionCriteria(InvoiceLine?.JI_CountryOfOrigin ?? ZString.Empty, CustomsCountryCode, "", "", new HashSet<ZString>(), EffectiveAssessmentDate, "", "");
						var rateView = UniversalTariff?.GetApplicableRates(criteria).FirstOrDefault();
						return rateView;
					});

		CachedProperty<RateView> rateViewCached;

		public bool JLT_BaseQuantityReadOnly
		{
			get
			{
				var secondaryTariffUnit = JLT_BaseQuantityUQ;
				var result = secondaryTariffUnit.IsEmpty;
				if (!secondaryTariffUnit.IsEmpty)
				{
					var unitList = new List<ZString>();
					AddUnitOfMeasureTypes(unitList, InvoiceLine.JI_CustomsUnitQty);
					AddUnitOfMeasureTypes(unitList, InvoiceLine.JI_CustomsSecondUnitQty);
					if (unitList.Contains(secondaryTariffUnit))
					{
						result = true;
					}
				}
				return result;
			}
		}

		static void AddUnitOfMeasureTypes(List<ZString> unitList, ZString unit)
		{
			if (!unit.IsEmpty && !unitList.Contains(unit))
			{
				unitList.Add(unit);
				unit = UnitConverter.GetInterchangeableUnit(unit);
				if (!unit.IsEmpty && !unitList.Contains(unit))
				{
					unitList.Add(unit);
				}
			}
		}

		AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList()
		{
			return new AdditionalDutiesTariffTypeList(Factory, CustomsCountryCode);
		}

		public RefCusTariffType UniversalTariffType
		{
			get
			{
				var tariffType = JLT_Type;
				return !tariffType.IsEmpty ? GetAdditionalDutiesTariffTypeList()[tariffType] : null;
			}
		}

		internal bool IsRorType => JLT_Type == Constants.UniversalReferenceConstants.RefCusRateTypes.CT || JLT_Type == Constants.UniversalReferenceConstants.RefCusRateTypes.SS;

		#region Implementation

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups()
		{
			return new JobComInvoiceLineTaxLookups(this);
		}

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation()
		{
			return new JobComInvoiceLineTaxValidation(this);
		}
		#endregion

		#region Suspend Tariff Defaulting

		int tariffDefaultingSuspenderIndex;

		internal bool IsTariffDefaultingSuspended => tariffDefaultingSuspenderIndex > 0;

		internal IDisposable SuspendInvoiceLineTaxDefaulting()
		{
			return new TariffDefaultingSuspender(this);
		}

		class TariffDefaultingSuspender : IDisposable
		{
			public TariffDefaultingSuspender(JobComInvoiceLineTax invoiceLineTax)
			{
				this.invoiceLineTax = invoiceLineTax;
				this.invoiceLineTax.tariffDefaultingSuspenderIndex++;
			}

			readonly JobComInvoiceLineTax invoiceLineTax;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLineTax.tariffDefaultingSuspenderIndex--;
			}

			#endregion
		}
		#endregion
	}
}
