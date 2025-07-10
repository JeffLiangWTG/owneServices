using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Universal.Testing
{
	public class UniversalRateDataForTest : IUniversalRateCalcData
	{
		protected QuestionForFormulaSpecificValue GetQuestionWithAnswer(string question, decimal answer)
		{
			var result = new QuestionForFormulaSpecificValue(question);
			result.SetUserAnswer(answer);
			return result;
		}

		#region IUniversalRateCalcData Members

		public DateTime DateOfValuation => new DateTime(2015, 01, 01);

		public decimal ValueForDuty => 1000M;

		public decimal CustomsValue => 1200M;

		public IDictionary<string, decimal> UnitOfMeasureValueList { get; } = new Dictionary<string, decimal>();

		public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		#endregion
	}
}
