using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Business;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class MeasureProcessor
	{
		[SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
		public Measure ConvertXElementToModel(XElement element)
		{
			var model = new Measure
			{
				AdditionalCodeId = element.Attribute(at + "additionalCodeId")?.Value ?? string.Empty,
				AdditionalCodeType = element.Attribute(at + "additionalCodeType")?.Value ?? string.Empty,
				ChangeType = element.Attribute(at + "changeType")?.Value ?? string.Empty,
				DateStart = GetDateTimeFromElement(element, at + "dateStart"),
				GeographicalAreaId = element.Attribute(at + "geographicalAreaId")?.Value ?? string.Empty,
				GoodsNomenclatureCode = element.Attribute(at + "goodsNomenclatureCode")?.Value ?? string.Empty,
				MeasureType = element.Attribute(at + "measureType")?.Value ?? string.Empty,
				National = element.Attribute(at + "national")?.Value ?? string.Empty,
				RegulationId = element.Attribute(at + "regulationId")?.Value ?? string.Empty,
				RegulationRoleType = element.Attribute(at + "regulationRoleType")?.Value ?? string.Empty,
				SID = element.Attribute(at + "SID")?.Value ?? string.Empty,
				SIDAdditionalCode = element.Attribute(at + "SIDAdditionalCode")?.Value ?? string.Empty,
				SIDGeographicalArea = element.Attribute(at + "SIDGeographicalArea")?.Value ?? string.Empty,
				SIDGoodsNomenclature = element.Attribute(at + "SIDGoodsNomenclature")?.Value ?? string.Empty,
				StoppedFlag = element.Attribute(at + "stoppedFlag")?.Value ?? string.Empty,
				Expression = element.Attribute(at + "expression")?.Value ?? string.Empty,
				DateEnd = GetDateTimeFromElement(element, at + "dateEnd"),
			};

			var compList = new List<MeasureComponent>();
			foreach (var mcElement in element.Elements(xmlns + "measureComponent"))
			{
				var newItem = ConvertXElementToMeasureComponent(mcElement);
				compList.Add(newItem);
			}

			var measureConditionList = element.Elements(xmlns + "measureCondition");
			foreach (var measureCondition in measureConditionList)
			{
				var measureConditionComponentList = measureCondition.Elements(xmlns + "measureConditionComponent");
				foreach (var measureConditionComponent in measureConditionComponentList)
				{
					var newItem = ConvertXElementToMeasureComponent(measureConditionComponent);
					compList.Add(newItem);
				}
			}

			model.Components = compList;

			return model;
		}

		static IEnumerable<string> GetUnitsOfMeasure(string expression)
		{
			var UOMList = new List<string>();
			var baseCode = FindBaseCodeFromExpression(expression, "1");
			if (baseCode.Length > 0)
			{
				UOMList.Add(baseCode);
			}
			baseCode = FindBaseCodeFromExpression(expression, "2");
			if (baseCode.Length > 0 && !UOMList.Contains(baseCode))
			{
				UOMList.Add(baseCode);
			}
			baseCode = FindBaseCodeFromExpression(expression, "3");
			if (baseCode.Length > 0 && !UOMList.Contains(baseCode))
			{
				UOMList.Add(baseCode);
			}

			return UOMList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
		MeasureComponent ConvertXElementToMeasureComponent(XElement element)
		{
			var mc = new MeasureComponent
			{
				DutyExpressionId = element.Attribute(at + "dutyExpressionId")?.Value ?? string.Empty,
				DutyAmount = GetDecimalFromElement(element, at + "dutyAmount"),
				MeasurementUnitCode = element.Attribute(at + "measurementUnitCode")?.Value ?? string.Empty,
				MonetaryUnitCode = element.Attribute(at + "monetaryUnitCode")?.Value ?? string.Empty,
				National = element.Attribute(at + "national")?.Value ?? string.Empty,
			};

			return mc;
		}

		static DateTime? GetDateTimeFromElement(XElement element, XName attributeName)
		{
			DateTime? dt = null;

			if (element?.Attribute(attributeName)?.Value != null)
			{
				dt = DateTime.Parse(element.Attribute(attributeName)?.Value, CultureInfo.CurrentCulture);
			}

			return dt;
		}

		static decimal? GetDecimalFromElement(XElement element, XName attributeName)
		{
			decimal? dt = null;

			if (element?.Attribute(attributeName)?.Value != null)
			{
				dt = decimal.Parse(element.Attribute(attributeName)?.Value, CultureInfo.InvariantCulture);
			}

			return dt;
		}

		public List<Measure> VATModels { get; set; }
		public List<Measure> DutiesModels { get; set; }

		public void LoadData(string filename)
		{
			VATModels = new List<Measure>();
			DutiesModels = new List<Measure>();

			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				using (var xmlReader = XmlReader.Create(fileStream))
				{
					if (xmlReader.ReadToFollowing(ParentElement))
					{
						using (var detailReader = xmlReader.ReadSubtree())
						{
							while (detailReader.ReadToFollowing(ElementName))
							{
								if (XNode.ReadFrom(detailReader) is XElement element)
								{
									var model = ConvertXElementToModel(element);

									if (!string.IsNullOrEmpty(model.GoodsNomenclatureCode) && model.National == FilterNational && (!model.DateEnd.HasValue || model.DateEnd >= DateTime.Now))
									{
										if (model.MeasureType == ServiceConstants.MeasureFilters.FilterMeasureTypeVAT)
										{
											VATModels.Add(model);
										}
										else if (ServiceConstants.MeasureFilters.FilterMeasureTypeDuties.Contains(model.MeasureType) &&
												!model.AdditionalCodeId.EndsWith("00", StringComparison.InvariantCulture))
										{
											DutiesModels.Add(model);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		const string ParentElement = "items";
		const string ElementName = "measure";
		const string FilterNational = "1";

		public void UpdateModels()
		{
			if (VATModels != null)
			{
				foreach (var model in VATModels)
				{
					UpdateModelCommon(model);
					model.TaxOrFeeCode = GetTaxOrFeeCode(model);
				}
			}

			if (DutiesModels != null)
			{
				foreach (var model in DutiesModels)
				{
					UpdateModelCommon(model);
					model.Formula = GenerateFormula(model);
					if (!string.IsNullOrEmpty(model.AdditionalCode))
					{
						model.RateCode = GetRateCode(model);
						model.RateType = GetRateType(model.RateCode);
					}
				}
			}
		}

		static void UpdateModelCommon(Measure model)
		{
			model.UnitOfMeasure = GetUnitsOfMeasure(model.Expression);
			model.CleanId = CleanCommodityCode(model.GoodsNomenclatureCode);
			model.AdditionalCode = model.AdditionalCodeType + model.AdditionalCodeId;
		}

		static string GetTaxOrFeeCode(Measure model)
		{
			var component = model.Components?.FirstOrDefault();
			var convertedCode = string.Empty;

			if (component is MeasureComponent)
			{
				switch (component.DutyAmount)
				{
					case 0m:
						convertedCode = VatTypes.Zero;
						break;
					case 9m:
						convertedCode = VatTypes.Low;
						break;
					case 21m:
						convertedCode = VatTypes.Standard;
						break;
				}
			}

			return convertedCode;
		}

		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
		static string GenerateFormula(Measure model)
		{
			var component = model.Components?.FirstOrDefault();
			var expression = model.Expression;
			var formula = string.Empty;

			if (component != null)
			{
				formula = component.DutyAmount?.ToString(CultureInfo.InvariantCulture).Replace(',', '.') + " * [" + component.MeasurementUnitCode + "]";
			}
			else if (!string.IsNullOrEmpty(expression))
			{
				var multiple = expression.Contains("$RATE2");
				if (!multiple)
				{
					var baseCode = FindBaseCodeFromExpression(expression, "1");
					string divide = FindDivideFromExpression(expression);
					string rate = FindRateFromExpression(expression, "1");

					formula = rate + " * [" + baseCode + "] / " + divide;
				}
				else
				{
					var function = "MAX";
					var divide = "100";
					var base1 = FindBaseCodeFromExpression(expression, "1");
					var rate1 = FindRateFromExpression(expression, "1");
					var base2 = FindBaseCodeFromExpression(expression, "2");
					var rate2 = FindRateFromExpression(expression, "2");
					var base3 = FindBaseCodeFromExpression(expression, "3");
					var rate3 = FindRateFromExpression(expression, "3");

					formula = function + "([" + base1 + "] * " + rate1 + " / " + divide + " + [" + base2 + "] * " + rate2 + ", [" + base3 + "] * " + rate3 + ")";
				}
			}

			return formula;
		}

		static string FindRateFromExpression(string expression, string sequenceOfRate)
		{
			var searchExpression = string.Empty;
			var baseCode = string.Empty;
			char endSearchChar = ';';

			if (expression.Contains("$RATE" + sequenceOfRate + "=AMOUNT("))
			{
				searchExpression = "$RATE" + sequenceOfRate + "=AMOUNT(";
				endSearchChar = ',';
			}

			else if (expression.Contains("AMOUNT(", StringComparison.InvariantCultureIgnoreCase))
			{
				searchExpression = "AMOUNT(";
				endSearchChar = ',';
			}
			else if (expression.Contains("$RATE" + sequenceOfRate  + "=", StringComparison.InvariantCultureIgnoreCase))
			{
				searchExpression = "$RATE" + sequenceOfRate + "=";
				endSearchChar = ';';
			}

			if (searchExpression.Length > 0)
			{
				var position = expression.IndexOf(searchExpression, StringComparison.InvariantCultureIgnoreCase);
				var expressionTrimmed = expression.Substring(position + searchExpression.Length);
				var endPosition = expressionTrimmed.IndexOf(endSearchChar);
				baseCode = expressionTrimmed.Substring(0, endPosition);
			}

			return baseCode;
		}

		static string FindDivideFromExpression(string expression)
		{
			var divide = "100";

			var position = expression.IndexOf('/');

			if (position != -1)
			{
				var expressionTrimmed = expression.Substring(position);
				var endPosition = expressionTrimmed.IndexOf(';');
				divide = expressionTrimmed.Substring(1, endPosition -1);
			}

			return divide;
		}

		static string FindBaseCodeFromExpression(string expression, string sequenceOfBase)
		{
			var searchExpression = string.Empty;
			var baseCode = string.Empty;

			if (expression != null && expression.Length > 0)
			{
				if (expression.Contains("$Base=?", StringComparison.InvariantCultureIgnoreCase))
				{
					searchExpression = "$Base=?";
				}
				else if (expression.Contains("$BASE" + sequenceOfBase + "=?", StringComparison.InvariantCultureIgnoreCase))
				{
					searchExpression = "$BASE" + sequenceOfBase + "=?";
				}

				if (searchExpression.Length > 0)
				{
					var position = expression.IndexOf(searchExpression, StringComparison.InvariantCultureIgnoreCase);
					var expressionTrimmed = expression.Substring(position + searchExpression.Length);
					var endPosition = expressionTrimmed.IndexOf(';');
					baseCode = expressionTrimmed.Substring(0, endPosition);
				}
			}

			return baseCode;
		}

		static string CleanCommodityCode(string itemId)
		{
			if (itemId != null && itemId.Length > 3 && itemId.Length % 2 == 0)
			{
				while (itemId.Length > 3 && itemId.EndsWith("00", StringComparison.Ordinal))
				{
					itemId = itemId.Substring(0, itemId.Length - 2);
				}
			}
			return itemId;
		}

		static string GetRateType(string rateCode)
		{
			var rateType = string.Empty;

			if (rateCode != null && rateCode.Length > 0)
			{
				switch (rateCode)
				{
					case RateCodes.ResourceStockLevyLeadedLightOil:
					case RateCodes.ResourceStockLevyUnLeadedLightOil:
					case RateCodes.ResourceStockLevyMediumHeavyOil:
					case RateCodes.ResourceStockLevyGasOil:
					case RateCodes.ResourceStockLevyLiquifiedPetroliumGas:
						rateType = RateTypes.Levy;
						break;
					case RateCodes.ConsumptionTax:
					case RateCodes.ExciseMineralOils:
					case RateCodes.EnergyTax:
					case RateCodes.CoalTax:
					case RateCodes.StockDuty:
						rateType = RateTypes.Miscellaneous;
						break;
					case RateCodes.ExciseTobacco:
					case RateCodes.ExciseBeer:
					case RateCodes.ExciseWine:
					case RateCodes.ExciseIntermediateProducts:
					case RateCodes.ExciseOtherAlcoholicProducts:
						rateType = RateTypes.Excise;
						break;
					case RateCodes.SuspendedAntiDumpingDuty:
						rateType = RateTypes.NationalSecurity;
						break;
				}
			}

			return rateType;
		}

		static string GetRateCode(Measure measure)
		{
			var rateCode = RateCodes.Default;
			var lookupCode = measure.AdditionalCodeId.Substring(1,2);
			var measureType = measure.MeasureType;

			if(lookupCode != null && measureType.Length > 0)
			{
				switch (measureType)
				{
					case "NLACC":
						rateCode = FindRateCodeNLACC(lookupCode);
						break;
					case "NLACVH":
						rateCode = FindRateCodeNLACVH(lookupCode);
						break;
					case "NLVBB":
						rateCode = FindRateCodeNLVBB(lookupCode);
						break;
					case "NLKOBE":
						rateCode = FindRateCodeNLKOBE(lookupCode);
						break;
				}
			}

			return rateCode;
		}

		static string FindRateCodeNLKOBE(string lookupCode)
		{
			return RateCodes.CoalTax;
		}

		static string FindRateCodeNLVBB(string lookupCode)
		{
			var returnValue = RateCodes.Default;

			switch (lookupCode)
			{
				case "80":
				case "81":
				case "82":
				case "83":
				case "85":
				case "89":
					returnValue = RateCodes.ConsumptionTax;
					break;
			}

			return returnValue;
		}

		static string FindRateCodeNLACVH(string lookupCode)
		{
			var returnValue = RateCodes.Default;

			switch (lookupCode)
			{
				case "41":
					returnValue = RateCodes.ResourceStockLevyLeadedLightOil;
					break;
				case "42":
					returnValue = RateCodes.ResourceStockLevyUnLeadedLightOil;
					break;
				case "44":
					returnValue = RateCodes.ResourceStockLevyMediumHeavyOil;
					break;
				case "46":
					returnValue = RateCodes.ResourceStockLevyGasOil;
					break;
				case "69":
					returnValue = RateCodes.ResourceStockLevyLiquifiedPetroliumGas;
					break;
			}

			return returnValue;
		}

		static string FindRateCodeNLACC(string lookupCode)
		{
			var returnValue = RateCodes.Default;

			switch (lookupCode)
			{
				case "01":
				case "02":
				case "06":
				case "07":
				case "09":
				case "11":
				case "13":
				case "19":
					returnValue = RateCodes.ExciseBeer;
					break;
				case "21":
				case "23":
					returnValue = RateCodes.ExciseWine;
					break;
				case "36":
				case "38":
					returnValue = RateCodes.ExciseIntermediateProducts;
					break;
				case "40":
					returnValue = RateCodes.ExciseOtherAlcoholicProducts;
					break;
				case "41":
				case "42":
				case "44":
				case "46":
				case "66":
				case "69":
					returnValue = RateCodes.ExciseMineralOils;
					break;
				case "70":
				case "75":
				case "79":
					returnValue = RateCodes.ExciseTobacco;
					break;
			}

			return returnValue;
		}

		readonly XNamespace at = "http://www.arcticgroup.se/tariff/arctictariff/export";
		readonly XNamespace xmlns = "http://www.arcticgroup.se/tariff/arctictariff/export";

	}

	static class RateCodes
	{
		public const string Default = "";
		public const string ConsumptionTax = "028";
		public const string ExciseMineralOils = "030";
		public const string ExciseTobacco = "032";
		public const string ExciseBeer = "035";
		public const string ExciseWine = "036";
		public const string EnergyTax = "049";
		public const string CoalTax = "050";
		public const string SuspendedAntiDumpingDuty = "060";
		public const string ExciseIntermediateProducts = "065";
		public const string ExciseOtherAlcoholicProducts = "066";
		public const string ResourceStockLevyLeadedLightOil = "080";
		public const string ResourceStockLevyUnLeadedLightOil = "081";
		public const string ResourceStockLevyMediumHeavyOil = "082";
		public const string ResourceStockLevyGasOil = "083";
		public const string ResourceStockLevyLiquifiedPetroliumGas = "084";
		public const string StockDuty = "087";
	}

	static class RateTypes
	{
		public const string Default = "";
		public const string Miscellaneous = "MSC";
		public const string Excise = "EXC";
		public const string NationalSecurity = "NSC";
		public const string Levy = "LVY";
	}

	static class VatTypes
	{
		public const string Zero = "ZER";
		public const string Low = "LOW";
		public const string Standard = "STD";
	}
}
