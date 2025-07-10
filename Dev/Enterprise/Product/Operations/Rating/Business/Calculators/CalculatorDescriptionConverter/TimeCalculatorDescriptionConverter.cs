using System.Collections.Generic;

namespace Enterprise.Rating.Business
{
	public class TimeCalculatorDescriptionConverter : BaseCombinedCalculatorDescriptionConverter<TimeCalculator>
	{
		public TimeCalculatorDescriptionConverter(TimeCalculator calculator)
			: base(calculator)
		{
		}

		protected override string CreateNonBasMinusPlusOperatorLinesDescription()
		{
			var nonBasMinusPlusOperatorLinesDescriptions = new List<string>();

			var linesDescription = base.CreateNonBasMinusPlusOperatorLinesDescription();
			if (!string.IsNullOrEmpty(linesDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(linesDescription);
			}

			var excludeConditionDescription = ConvertExcludeConditionDescription();
			if (!string.IsNullOrEmpty(excludeConditionDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(excludeConditionDescription);
			}

			return string.Join(", ", nonBasMinusPlusOperatorLinesDescriptions);
		}

		string ConvertExcludeConditionDescription()
		{
			switch (calculator.ExcludeHolidays)
			{
				case Calculator.Items.ExcludeWeekendsAndPublicHolidays:
					return Res.GetString("d36245c7-d2a3-44ed-acd7-8659609c189a", "Exclude Weekends and Public Holidays");
				case Calculator.Items.ExcludeSundaysAndPublicHolidays:
					return Res.GetString("3c9913ec-10e1-4bb2-92f6-bee4228e3d8a", "Exclude Sundays and Public Holidays");
				default:
					return string.Empty;
			}
		}
	}
}
