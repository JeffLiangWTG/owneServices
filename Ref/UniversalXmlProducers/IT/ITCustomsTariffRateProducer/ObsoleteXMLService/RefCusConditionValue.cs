using System;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusConditionValue
	{
		[XmlElement(ElementName = "ZX3_Value")]
		public string Value { get; set; }

		[XmlElement(ElementName = "ZX3_ZX4_NKValueType")]
		public string ValueType { get; set; }

		internal RefCusConditionValue()
		{ }

		public RefCusConditionValue(string valueType, string value)
		{
			Argument.NotNull(valueType, nameof(valueType));
			Argument.NotNull(value, nameof(value));

			Value = value;
			ValueType = valueType;
		}
	}
}
