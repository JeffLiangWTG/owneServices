using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(Calculator.Items.Operator.BAS, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "BaseRate")]
	[CalculatorProperty(Calculator.Items.Operator.UNT, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", RelatedTo = "PerKG")]
	[CalculatorProperty(PackageCountCalculator.Items.FirstPackageRate, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal3", RelatedTo = "FirstPackageRate")]
	[CalculatorProperty(PackageCountCalculator.Items.AddtionalPackageRate, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal4", RelatedTo = "AddtionalPackageRate")]
	public class PackageCountCalculator : Calculator // A.K.A. ItalianAirportTaxCalculator
	{
		public PackageCountCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.PackageCount;

		#region Properties

		public ZDecimal BaseRate
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.BAS]; }
			set { this[Calculator.Items.Operator.BAS] = value; }
		}

		public ZDecimal PerKG
		{
			get { return (ZDecimal)this[Calculator.Items.Operator.UNT]; }
			set { this[Calculator.Items.Operator.UNT] = value; }
		}

		public ZDecimal FirstPackageRate
		{
			get { return (ZDecimal)this[PackageCountCalculator.Items.FirstPackageRate]; }
			set { this[PackageCountCalculator.Items.FirstPackageRate] = value; }
		}

		public ZDecimal AddtionalPackageRate
		{
			get { return (ZDecimal)this[PackageCountCalculator.Items.AddtionalPackageRate]; }
			set { this[PackageCountCalculator.Items.AddtionalPackageRate] = value; }
		}

		RateLineItem BaseRateItem
		{
			get { return FindRateLineItem(Calculator.Items.Operator.BAS); }
		}

		RateLineItem PerKGItem
		{
			get { return FindRateLineItem(Calculator.Items.Operator.UNT); }
		}

		RateLineItem FirstPackageRateItem
		{
			get { return FindRateLineItem(PackageCountCalculator.Items.FirstPackageRate); }
		}

		RateLineItem AddtionalPackageRateItem
		{
			get { return FindRateLineItem(PackageCountCalculator.Items.AddtionalPackageRate); }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
			result.Add(QuotationLine.BaseRate(Line, 0));
			result.Add(QuotationLine.NewWithValue(Line, 0, Calculator.Items.Operator.UNT, FlatRateText, PerKGText));
			result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, PackageCountCalculator.Items.FirstPackageRate, FirstPackageText, (NoResString)ZString.Empty));
			result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, PackageCountCalculator.Items.AddtionalPackageRate, AdditionalPackageText, PerPackageText));

			return result;
		}

		static string FlatRateText => Res.GetString("7e5aaba2-4432-4fa3-9670-bd3cba4c153a", "Flat Rate");

		static ResourceString PerKGText => ResString.GetMultilingualString("df9ad0bd-86c6-4a55-9143-94120fded70d", "per KG");

		static string FirstPackageText => Res.GetString("ca4b4b96-eb16-4f65-bc84-7dbbbe78043a", "First Package");

		static string AdditionalPackageText => Res.GetString("7b3e7921-8679-4ce2-9e93-b8636691faea", "Additional Packages");

		static ResourceString PerPackageText => ResString.GetMultilingualString("e21e60a9-f8e8-4603-95ef-fa8d878ebc61", "per Package");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var baseRate = QuotationLine.GetValue(Calculator.Items.Operator.BAS, Line.ChildRateLineItems);
			var unit = QuotationLine.GetValue(Calculator.Items.Operator.UNT, Line.ChildRateLineItems);
			var firstPackage = QuotationLine.GetValue(PackageCountCalculator.Items.FirstPackageRate, Line.ChildRateLineItems);
			var additionalPackage = QuotationLine.GetValue(PackageCountCalculator.Items.AddtionalPackageRate, Line.ChildRateLineItems);

			result.SetFlat(Line.TL_RX_NKCurrency, baseRate);
			result.SetUnit(Line.TL_RX_NKCurrency, Line.Calculator.UnitDescription(true), unit);
			result.SetFirstAddAdditional(Line.TL_RX_NKCurrency, PackageText, firstPackage, additionalPackage);

			return result;
		}

		static string PackageText => Res.GetString("0638B411-AAEE-40A2-ADFF-F13A0A0C583B", "Package");

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var criteria = parameters.Criteria;
			calcOutput.BaseRate = BaseRate;

			if (!PerKG.IsEmpty)
			{
				CalculatePerUnit(calcOutput, new Quantity(PerKG, Unit));
			}

			var packagesCount = criteria.JobMeasures.GetActual(MeasureType.Package);
			if (packagesCount > 0)
			{
				var results = new List<PaymentBasis>();
				var additionalPackagesCount = packagesCount - 1;
				var unit = QuantityUnit.PK;
				var unitDescription = UnitDescriptionInternal(unit);
				var packageText = Res.GetString("505fd9d7-fc33-4125-bba5-d50becdaa89f", "package");

				var firstPackageRate = RateInfo.CreateUNT(FirstPackageRate, unit, Line.TL_RX_NKCurrency, packageText, Res.GetString("6d83d740-914b-4030-bc3c-941c3505843d", "for 1st package"));
				results.Add(criteria.CreatePaymentBasis(firstPackageRate, new Quantity(1, unit), unitDescription));

				var rateInfoAdditional = RateInfo.CreateUNT(AddtionalPackageRate, unit, Line.TL_RX_NKCurrency, packageText);
				var chargeable = new Quantity(additionalPackagesCount, unit);
				results.Add(criteria.CreatePaymentBasis(rateInfoAdditional, chargeable, Res.GetString("3f374566-8fb6-4994-b666-cb4b86efe70d", "additional package(s)")));
				calcOutput.Add(results);
			}
		}

		protected internal override ZString Unit => QuantityUnit.KG;

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var clonedCalc = clone.Calculator as PackageCountCalculator;
			if (clonedCalc != null)
			{
				var args = new BusinessObjectCloneArgs(new[] { RateLineItemsSchema.Constants.PK, RateLineItemsSchema.Constants.TM_TL });
				clonedCalc.BaseRateItem.CopyPersistentValuesFrom(BaseRateItem, args);
				clonedCalc.PerKGItem.CopyPersistentValuesFrom(PerKGItem, args);
				clonedCalc.FirstPackageRateItem.CopyPersistentValuesFrom(FirstPackageRateItem, args);
				clonedCalc.AddtionalPackageRateItem.CopyPersistentValuesFrom(AddtionalPackageRateItem, args);

				clonedCalc.BaseRateItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.BaseRateWithApplicableIncrease, 2),
					rateTypeToUpdate);

				clonedCalc.PerKGItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.PerUnit, 2),
					rateTypeToUpdate);

				clonedCalc.FirstPackageRateItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100, 2),
					rateTypeToUpdate);

				clonedCalc.AddtionalPackageRateItem.UpdateRateValue(
					x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100, 2),
					rateTypeToUpdate);
			}
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

