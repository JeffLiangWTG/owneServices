using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class WarehousePackCalculator : Calculator
	{
		public WarehousePackCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.WarehousePack;

		#region Implementation

		public override string DefaultWeightVolume
		{
			get { return Constants.PkgUnit.Unit; }
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var measure = parameters.GetMeasureTypes(Line).FirstOrDefault();
			switch (measure)
			{
				case MeasureType.Package:
					CalculateForPackages(calcOutput);
					break;
				default:
					CalculateForUnits(calcOutput);
					break;
			}
		}

		#region CalculateForPackages

		void CalculateForPackages(CalculatorOutput calcOutput)
		{
			var paymentBases = new List<PaymentBasis>();

			var parameters = calcOutput.Parameters;
			var groupedPackages = parameters.Criteria.JobMeasures.GetGroupedPackages(Line.TL_UnitFactor == UnitFactorList.Codes.LoadedPackagesOnly);
			if (groupedPackages != null)
			{
				var notAutoratedPackages = new ZStringBuilder();
				foreach (var packageGroup in groupedPackages.OrderBy(p => p.Key)) // order by Package Type to make it consistent
				{
					var rateItemForPackage = GetPackageItem(packageGroup.Key);
					if (rateItemForPackage != null)
					{
						var paymentBasis = GetPaymentBasis(parameters, rateItemForPackage, packageGroup.Value);
						if (paymentBasis != null)
						{
							paymentBases.Add(paymentBasis.Value);
						}
						else if (packageGroup.Value > 0)
						{
							notAutoratedPackages.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", packageGroup.Value, packageGroup.Key));
						}
					}
					else if (packageGroup.Value > 0)
					{
						notAutoratedPackages.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", packageGroup.Value, packageGroup.Key));
					}
				}

				if (notAutoratedPackages.Length > 0)
				{
					var remainder = Res.GetString("7D8A1F14-3C54-4430-B784-5A2BF827E869", "There is a remainder of {0} that could not be rated. No rates for the Package Type(s) were found", notAutoratedPackages.ToStringWithDelimiterBetweenAppends(", "));
					AddRemainderToPaymentBases(parameters, paymentBases, remainder, remainder);
				}
			}

			calcOutput.Add(paymentBases);
		}

		#endregion

		#region CalculateForUnits

		void CalculateForUnits(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var chargeable = ChargeableAmount(parameters);
			var numUnits = RoundAmount(chargeable);
			var product = parameters.ProductFilter;

			if (product != null)
			{
				var conversionsList = new List<PackTypeConversionItem>();
				var hasFailed = false;

				try
				{
					CalculateRecursive(conversionsList, product.PartUnits, product.OP_StockKeepingUnit, "", 1m, ref hasFailed);
				}
				catch (CalculationException ex)
				{
					calcOutput.FailureMessage = ex.Message;
				}

				ApplyRating(calcOutput, product, conversionsList, numUnits.Amount, hasFailed);
			}
		}

		#region CalculateRecursive

		void CalculateRecursive(List<PackTypeConversionItem> conversionsList, OrgPartUnitCollection partUnits, ZString packageType, ZString previousPackageType, ZDecimal unitsInPackageType, ref bool hasFailed)
		{
			if (!hasFailed)
			{
				if (AddConversionItem(conversionsList, packageType, unitsInPackageType, ref hasFailed))
				{
					if (hasFailed)
					{
						return;
					}

					CalculateRecursiveForward(conversionsList, partUnits, packageType, previousPackageType, unitsInPackageType, ref hasFailed);
					CalculateRecursiveBackward(conversionsList, partUnits, packageType, previousPackageType, unitsInPackageType, ref hasFailed);
				}
			}
		}

		bool AddConversionItem(List<PackTypeConversionItem> conversionsList, ZString packageType, ZDecimal unitsInPackageType, ref bool hasFailed)
		{
			var conversionItemExist = false;

			if (unitsInPackageType == 0)
			{
				hasFailed = true;
				return false;
			}

			foreach (var item in conversionsList)
			{
				if (packageType.EqualsIgnoringCase(item.PackageType))
				{
					if (item.UnitsInStockKeepingUnits != unitsInPackageType)
					{
						hasFailed = true;
					}
					conversionItemExist = true;
					break;
				}
			}

			if (!conversionItemExist)
			{
				var newItem = new PackTypeConversionItem();
				newItem.PackageType = packageType;
				newItem.UnitsInStockKeepingUnits = unitsInPackageType;

				conversionsList.Add(newItem);
			}

			return !conversionItemExist;
		}

		void CalculateRecursiveForward(List<PackTypeConversionItem> conversionsList, OrgPartUnitCollection partUnits, ZString packageType, ZString previousPackageType, ZDecimal unitsInPackageType, ref bool hasFailed)
		{
			foreach (OrgPartUnit partUnit in partUnits)
			{
				if (partUnit.OF_PackType.EqualsIgnoringCase(packageType) && !partUnit.OF_ParentPackType.EqualsIgnoringCase(previousPackageType))
				{
					ZDecimal unitsInParent = unitsInPackageType * partUnit.OF_QuantityInParent;
					CalculateRecursive(conversionsList, partUnits, partUnit.OF_ParentPackType, packageType, unitsInParent, ref hasFailed);
				}
			}
		}

		void CalculateRecursiveBackward(List<PackTypeConversionItem> conversionsList, OrgPartUnitCollection partUnits, ZString packageType, ZString previousPackageType, ZDecimal unitsInPackageType, ref bool hasFailed)
		{
			foreach (OrgPartUnit partUnit in partUnits)
			{
				if (partUnit.OF_ParentPackType.EqualsIgnoringCase(packageType) && !partUnit.OF_PackType.EqualsIgnoringCase(previousPackageType))
				{
					if (partUnit.OF_QuantityInParent.IsEmpty)
					{
						throw new CalculationException(Res.GetString("7DB68994-D925-4d65-8FEF-6CD7828DB26E", "Could not convert between {0} and parent pack {1} for this product as the Quantity in Parent is zero.", partUnit.OF_PackType, partUnit.OF_ParentPackType));
					}
					ZDecimal unitsInParent = unitsInPackageType / partUnit.OF_QuantityInParent;
					CalculateRecursive(conversionsList, partUnits, partUnit.OF_PackType, packageType, unitsInParent, ref hasFailed);
				}
			}
		}

		#region PackTypeConvestionItem class

		class PackTypeConversionItem : IComparable<PackTypeConversionItem>
		{
			public string PackageType { get; set; }
			public ZDecimal UnitsInStockKeepingUnits { get; set; }

			#region IComparable<PackTypeConvertionItem> Members

			public int CompareTo(PackTypeConversionItem other)
			{
				return other.UnitsInStockKeepingUnits.CompareTo(this.UnitsInStockKeepingUnits);
			}

			#endregion
		}

		#endregion

		#endregion

		#region ApplyRating

		void ApplyRating(CalculatorOutput calcOutput, OrgSupplierPart product, List<PackTypeConversionItem> conversionsList, ZDecimal numberofUnits, bool hasFailed)
		{
			var parameters = calcOutput.Parameters;
			var paymentBases = new List<PaymentBasis>();
			var decimals = product.OP_CountDecimalPlaces;

			if (hasFailed)
			{
				paymentBases.Add(parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, Line.TL_RX_NKCurrency), default, null, Res.GetString("234e1e00-f585-4e35-a936-170ee212ed9c", "Product {0} - Calculation not complete, please check the package types definition", product.OP_PartNum)));
			}
			else
			{
				conversionsList.Sort(); // make sure that biggest packages are at the top

				foreach (var conversionItem in conversionsList)
				{
					RateLineItem rateItemForPackage = null;

					if (numberofUnits > 0 && (rateItemForPackage = GetPackageItem(conversionItem.PackageType)) != null)
					{
						ZDecimal packageQuantity = product.OP_StockKeepingUnit.EqualsIgnoringCase(conversionItem.PackageType)
							? Utilities.Round(numberofUnits, decimals)
							: Math.Truncate(numberofUnits / conversionItem.UnitsInStockKeepingUnits);

						numberofUnits = numberofUnits - (packageQuantity * conversionItem.UnitsInStockKeepingUnits);
						var paymentBasis = GetPaymentBasis(parameters, rateItemForPackage, packageQuantity);
						if (paymentBasis != null)
						{
							paymentBases.Add(paymentBasis.Value);
						}
					}
				}

				var productDescription = Res.GetString("7463bab6-8fb5-48cb-b1bf-f7eb9e8da977", "Product {0}", product.OP_PartNum) + (paymentBases.Count > 1 ? " -" : string.Empty);
				paymentBases = paymentBases.GetWithUpdatedChargeableDescription(productDescription).ToList();

				if (numberofUnits != 0)
				{
					var remainder = Res.GetString("349effd4-3d99-4914-a496-50288342f02c", "There is a remainder of {0} {1} that could not be rated. No rates for product stock keeping unit ({1}) were found and no conversions were possible", numberofUnits.ToString(decimals), product.OP_StockKeepingUnit);
					var remainderWithNoOtherBases = Res.GetString("7e5620db-3086-45c1-9df2-21b8a04911c3", "Product {0} - {1}", product.OP_PartNum, remainder);
					AddRemainderToPaymentBases(parameters, paymentBases, remainder, remainderWithNoOtherBases);
				}
			}

			calcOutput.Add(paymentBases);
		}

		PaymentBasis? GetPaymentBasis(AutoRatingCalculatorParameters parameters, RateLineItem rateItemForPackage, ZDecimal packageQuantity)
		{
			PaymentBasis? result = null;

			var rateForPackageType = rateItemForPackage.TM_RelevantValue;
			var packageAmount = packageQuantity * rateForPackageType;

			if (packageQuantity != 0 && packageAmount > 0)
			{
				var packageTypeCode = rateItemForPackage.TM_Type.ToUpperInvariant();
				var rateInfoUNT = RateInfo.CreateUNT(rateForPackageType, packageTypeCode, Line.TL_RX_NKCurrency, packageTypeCode, unitMultiplier: UnitMultiplier);
				var chargeableQuantity = new Quantity(packageQuantity, packageTypeCode);
				result = parameters.Criteria.CreatePaymentBasis(rateInfoUNT, chargeableQuantity, rateItemForPackage.WarehousePackageTypeDesc);
			}

			return result;
		}

		RateLineItem GetPackageItem(ZString packageType)
		{
			foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
			{
				if (packageType.EqualsIgnoringCase(item.TM_Type))
				{
					return item;
				}
			}

			return null;
		}

		void AddRemainderToPaymentBases(AutoRatingCalculatorParameters parameters, List<PaymentBasis> paymentBases, string remainder, string remainderWithNoOtherBases)
		{
			if (paymentBases.Any())
			{
				var lastBasis = paymentBases.Last();
				var changedBasis = lastBasis.UpdateRateInfo(RateInfo.GetWithNewRateInfoDescription(lastBasis.RateInfo, remainder));
				paymentBases.Remove(lastBasis);
				paymentBases.Insert(paymentBases.Count, changedBasis);
			}
			else
			{
				paymentBases.Add(parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, Line.TL_RX_NKCurrency), default, null, remainderWithNoOtherBases));
			}
		}

		#endregion

		#endregion

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (RateLineBizO.RateLineItems.Any())
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));

				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, item.WarehousePackageTypeDesc, (NoResString)ZString.Empty));
				}
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			if (RateLineBizO.RateLineItems.Any())
			{
				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					var unit = new DocLineAmount();
					unit.SetUnit(Line.TL_RX_NKCurrency, item.WarehousePackageTypeDesc, item.TM_RelevantValue);
					result = result + unit;
				}
			}

			return result;
		}

		#endregion

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			MandatoryValidation.CheckEntered(lineItem.TM_TypeInfo);
			ListValidation.ErrorIfInvalidCode(lineItem.TM_TypeInfo, lineItem.Lookups.WarehousePackagesTypes);
			ValidateTM_TypeDuplicates(lineItem, lineItem.TM_Type, Res.GetString("3bf0d9d8-edc5-49c6-ae48-cf9b1812e66f", "You can only specify one rate for each package type. You should not specify the same package type more than once."));
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(
			CompanyTariffOrCostBasedCalculator ctbCalc,
			RateLine clone,
			RateLineItem.RateTypeToUpdate rateTypeToUpdate) =>
			CalculatorHelper.CloneAndUpdateRateLineItemsWithPerUnitPercentAndBaseRate<WarehousePackCalculator>(ctbCalc, clone, RateLineItems, rateTypeToUpdate);

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

