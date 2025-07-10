using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel
{
	public class RefCusCondition
	{
		[XmlElement(ElementName = "ZX1_StartDate")]
		public string StartDate { get; set; }

		[XmlElement(ElementName = "ZX1_EndDate")]
		public string EndDate { get; set; }

		[XmlElement(ElementName = "ZX1_Comment")]
		public string Comment { get; set; }

		[XmlElement(ElementName = "ZX1_ZX2_NKConditionType")]
		public string ConditionType { get; set; }

		[XmlElement(ElementName = "RefCusApplicability")]
		public RefCusApplicability Applicability { get; set; }

		[XmlElement(ElementName = "RefCusConditionValue")]
		public List<RefCusConditionValue> ConditionValue { get; set; }

		internal RefCusCondition()
		{ }

		public RefCusCondition(string conditionType, string comment, DateTime startDate, DateTime endDate, RefCusApplicability applicability, List<RefCusConditionValue> conditionValues)
		{
			Argument.NotNull(conditionType, nameof(conditionType));
			Argument.NotNull(comment, nameof(comment));

			StartDate = startDate.ToString("s");
			EndDate = endDate.ToString("s");
			Comment = comment;
			ConditionType = conditionType;
			Applicability = applicability;
			ConditionValue = conditionValues;
		}
	}
}
