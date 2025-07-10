using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public class CalculationStep : IXmlSerializable
	{
		#region Constants

		public static class Constants
		{
			public static class CalculationType
			{
				public const string PerUnit = "UNT";
				public const string Percentage = "PER";
			}
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string XmlElementName = "CalculationStep";
			public const string CalculationType = "CalculationType";
			public const string IsSpecialCommodityRate = "IsSpecialCommodityRate";

			public const string UnitCount = "UnitCount";
			public const string UnitPrice = "UnitPrice";
			public const string Unit = "Unit";

			public const string Flat = "Flat";
			public const string Percentage = "Percentage";
			public const string Result = "Result";

			public const string DeprecatedContainerNumber = "ContainerNumber";
		}

		#endregion

		public ZString CalculationType { get; set; }
		public ZBool IsSpecialCommodityRate { get; set; }

		public ZDecimal UnitCount { get; set; }
		public ZDecimal UnitPrice { get; set; }
		public ZString Unit { get; set; }

		public ZDecimal Flat { get; set; }
		public ZDecimal Percentage { get; set; }
		public ZDecimal Result { get; set; }

		#region Calculated properties

		public bool IsPerUnit
		{
			get { return CalculationType == Constants.CalculationType.PerUnit; }
		}

		public bool IsPercentage
		{
			get { return CalculationType == Constants.CalculationType.Percentage; }
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.XmlElementName);
			WriteXmlCore(writer);
			writer.WriteEndElement();

			writer.Flush();
		}

		void WriteXmlCore(XmlWriter writer)
		{
			writer.WriteElementString(Schema.CalculationType, CalculationType);
			writer.WriteElementString(Schema.IsSpecialCommodityRate, IsSpecialCommodityRate.ToString());

			writer.WriteElementString(Schema.UnitCount, SerializationHelper.DecimalToInvariantString(UnitCount));
			writer.WriteElementString(Schema.UnitPrice, SerializationHelper.DecimalToInvariantString(UnitPrice));
			writer.WriteElementString(Schema.Unit, Unit);

			writer.WriteElementString(Schema.Flat, SerializationHelper.DecimalToInvariantString(Flat));
			writer.WriteElementString(Schema.Percentage, SerializationHelper.DecimalToInvariantString(Percentage));
			writer.WriteElementString(Schema.Result, SerializationHelper.DecimalToInvariantString(Result));
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			ReadXmlCore(reader);
			reader.ReadEndElement();
		}

		void ReadXmlCore(XmlReader reader)
		{
			CalculationType = reader.ReadElementString(Schema.CalculationType);
			IsSpecialCommodityRate = new ZBool(reader.ReadElementString(Schema.IsSpecialCommodityRate));

			UnitCount = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.UnitCount));
			UnitPrice = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.UnitPrice));
			Unit = reader.ReadElementString(Schema.Unit);

			Flat = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Flat));
			Percentage = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Percentage));
			Result = SerializationHelper.InvariantStringToDecimal(reader.ReadElementString(Schema.Result));

			// This remains for backwards compatibility.
			SerializationHelper.SkipDeprecated(reader, Schema.DeprecatedContainerNumber);
		}

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			CalculationStep anotherStep = obj as CalculationStep;
			return anotherStep != null
				&& CalculationType == anotherStep.CalculationType
				&& IsSpecialCommodityRate == anotherStep.IsSpecialCommodityRate
				&& UnitCount == anotherStep.UnitCount
				&& UnitPrice == anotherStep.UnitPrice
				&& Flat == anotherStep.Flat
				&& Percentage == anotherStep.Percentage
				&& Result == anotherStep.Result;
		}

		public override int GetHashCode()
		{
			return CalculationType.GetHashCode()
				^ IsSpecialCommodityRate.GetHashCode()
				^ UnitCount.GetHashCode()
				^ UnitPrice.GetHashCode()
				^ Flat.GetHashCode()
				^ Percentage.GetHashCode()
				^ Result.GetHashCode();
		}

		#endregion
	}
}
