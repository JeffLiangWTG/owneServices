using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Integration
{
	public class CalculationLog : IXmlSerializable
	{
		#region Schema

		public static class Schema
		{
			public const string XmlElementName = "CalculationLog";
			public const string CalculationSteps = "CalculationSteps";

			public const string IsCosting = "IsCosting";
			public const string CalculatorCode = "CalculatorCode";
			public const string ChargeCode = "ChargeCode";
			public const string Currency = "Currency";
			public const string Weight = "Weight";
			public const string Unit = "Unit";
			public const string DeprecatedBreakUnit = "BreakUnit";
			public const string Chargeable = "Chargeable";
			public const string ChargeableUnit = "ChargeableUnit";

			public const string CommodityCode = "CommodityCode";
			public const string RateMode = "RateMode";
			public const string ContainerCode = "ContainerCode";

			public const string BaseRate = "BaseRate";
			public const string Minimum = "Minimum";
			public const string Maximum = "Maximum";
		}

		#endregion

		public ZBool IsCosting { get; set; }
		public ZString CalculatorCode { get; set; }
		public ZString ChargeCode { get; set; }
		public ZString Currency { get; set; }

		public ZDecimal Weight { get; set; }
		public ZString Unit { get; set; }
		public ZDecimal Chargeable { get; set; }
		public ZString ChargeableUnit { get; set; }

		public ZString CommodityCode { get; set; }
		public ZString RateMode { get; set; }
		public ZString ContainerCode { get; set; }

		public ZDecimal BaseRate { get; set; }
		public ZDecimal Minimum { get; set; }
		public ZDecimal Maximum { get; set; }

		public ZDecimal Result
		{
			get { return Steps.Sum(x => x.Result); }
		}

		public List<CalculationStep> Steps
		{
			get { return steps ?? (steps = new List<CalculationStep>()); }
		}
		List<CalculationStep> steps;

		#region Calculated properties

		public bool IsEmpty
		{
			get { return Steps.Count == 0 && !HasBaseRate && !HasMaximum && !HasMinimum; }
		}

		public bool HasBaseRate
		{
			get { return BaseRate != 0m; }
		}

		public bool HasMinimum
		{
			get { return Minimum != 0m; }
		}

		public bool HasMaximum
		{
			get { return Maximum != 0m; }
		}

		#endregion

		#region Add Calculation Steps

		public CalculationStep AddPerContainerUnitCalculation(ZDecimal unitCount, ZDecimal unitPrice)
		{
			var result = AddPerUnitCalculation(unitCount, QuantityUnit.CN, unitPrice);

			return result;
		}

		public CalculationStep AddPerUnitCalculation(ZDecimal unitCount, ZString unit, ZDecimal unitPrice)
		{
			CalculationStep result = AddCalculationStep();
			result.CalculationType = CalculationStep.Constants.CalculationType.PerUnit;
			result.UnitCount = unitCount;
			result.UnitPrice = unitPrice;
			result.Unit = unit;
			result.Result = unitCount * unitPrice;

			return result;
		}

		public CalculationStep AddFlatAmountToLastCalculation(ZDecimal flatAmount)
		{
			CalculationStep calculationStep = Steps.LastOrDefault();
			if (calculationStep == null || !calculationStep.IsPerUnit)
			{
				calculationStep = AddPerUnitCalculation(0m, ZString.Empty, 0m);
			}
			calculationStep.Flat += flatAmount;
			calculationStep.Result += flatAmount;

			return calculationStep;
		}

		public CalculationStep AddPercentageCalculation(ZDecimal value, ZDecimal percentage)
		{
			CalculationStep result = AddCalculationStep();
			result.CalculationType = CalculationStep.Constants.CalculationType.Percentage;
			result.UnitCount = value;
			result.Percentage = percentage;
			result.Result = value * percentage / 100m;

			return result;
		}

		public CalculationStep AddCalculationStep()
		{
			CalculationStep result = new CalculationStep();
			Steps.Add(result);

			return result;
		}

		#endregion

		public override bool Equals(object obj)
		{
			var other = obj as CalculationLog;
			if (other == null)
			{
				return false;
			}

			var arePropertiesEqual = IsCosting == other.IsCosting
				&& CalculatorCode == other.CalculatorCode
				&& ChargeCode == other.ChargeCode
				&& Currency == other.Currency
				&& Weight == other.Weight
				&& Unit == other.Unit
				&& Chargeable == other.Chargeable
				&& ChargeableUnit == other.ChargeableUnit
				&& CommodityCode == other.CommodityCode
				&& RateMode == other.RateMode
				&& BaseRate == other.BaseRate
				&& Minimum == other.Minimum
				&& Maximum == other.Maximum
				&& Steps.Count == other.Steps.Count;

			if (!arePropertiesEqual)
			{
				return false;
			}

			var otherSteps = other.Steps.ToList();
			foreach (var step in Steps)
			{
				var otherStep = otherSteps.FirstOrDefault(s => s.Equals(step));
				if (otherStep == null)
				{
					return false;
				}

				otherSteps.Remove(otherStep);
			}

			return true;
		}

		public override int GetHashCode()
		{
			return 23 * (int)Result + 14 * (int)Weight;
		}

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.XmlElementName);

			WriteMainProperties(writer);
			WriteCalculationSteps(writer);

			writer.WriteEndElement();

			writer.Flush();
		}

		void WriteMainProperties(XmlWriter writer)
		{
			writer.WriteElementString(Schema.IsCosting, IsCosting.ToString());
			writer.WriteElementString(Schema.CalculatorCode, CalculatorCode);
			writer.WriteElementString(Schema.ChargeCode, ChargeCode);
			writer.WriteElementString(Schema.Currency, Currency);
			writer.WriteElementString(Schema.Weight, SerializationHelper.DecimalToInvariantString(Weight));
			writer.WriteElementString(Schema.Unit, Unit);
			writer.WriteElementString(Schema.Chargeable, SerializationHelper.DecimalToInvariantString(Chargeable));
			writer.WriteElementString(Schema.ChargeableUnit, ChargeableUnit);

			writer.WriteElementString(Schema.CommodityCode, CommodityCode);
			writer.WriteElementString(Schema.RateMode, RateMode);
			writer.WriteElementString(Schema.ContainerCode, ContainerCode);

			writer.WriteElementString(Schema.BaseRate, SerializationHelper.DecimalToInvariantString(BaseRate));
			writer.WriteElementString(Schema.Minimum, SerializationHelper.DecimalToInvariantString(Minimum));
			writer.WriteElementString(Schema.Maximum, SerializationHelper.DecimalToInvariantString(Maximum));
		}

		void WriteCalculationSteps(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.CalculationSteps);
			foreach (CalculationStep calculationStep in Steps)
			{
				((IXmlSerializable)calculationStep).WriteXml(writer);
			}
			writer.WriteEndElement();
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();

			ReadMainProperties(reader);
			ReadCalculationSteps(reader);

			reader.ReadEndElement();
		}

		void ReadMainProperties(XmlReader reader)
		{
			IsCosting = new ZBool(reader.ReadElementString(Schema.IsCosting));
			CalculatorCode = reader.ReadElementString(Schema.CalculatorCode);
			ChargeCode = reader.ReadElementString(Schema.ChargeCode);
			Currency = reader.ReadElementString(Schema.Currency);
			Weight = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Weight));
			Unit = reader.ReadElementString(Schema.Unit);

			// This remains for backwards compatibility.
			SerializationHelper.SkipDeprecated(reader, Schema.DeprecatedBreakUnit);

			Chargeable = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Chargeable));
			ChargeableUnit = reader.ReadElementString(Schema.ChargeableUnit);

			CommodityCode = reader.ReadElementString(Schema.CommodityCode);
			RateMode = reader.ReadElementString(Schema.RateMode);
			ContainerCode = reader.ReadElementString(Schema.ContainerCode);

			BaseRate = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.BaseRate));
			Minimum = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Minimum));
			Maximum = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Maximum));
		}

		void ReadCalculationSteps(XmlReader reader)
		{
			reader.ReadStartElement(Schema.CalculationSteps);

			if (reader.NodeType != XmlNodeType.EndElement) // CalculationSteps node is not empty; if it's empty, we shouldn't call ReadEndElement()
			{
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					CalculationStep calculationStep = new CalculationStep();
					((IXmlSerializable)calculationStep).ReadXml(reader);

					Steps.Add(calculationStep);
				}

				reader.ReadEndElement();
			}
		}

		#endregion
	}
}
