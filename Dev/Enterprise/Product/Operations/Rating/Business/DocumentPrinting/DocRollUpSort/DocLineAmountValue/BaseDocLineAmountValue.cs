using System.Collections.Generic;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	abstract class BaseDocLineAmountValue
	{
		public BaseDocLineAmountValue((string, string)[] properties, (string, decimal)[] values)
		{
			Property = new DocLineAmountProperty(properties);

			Property[TypePropertyKey] = GetType().Name;

			Values = new Dictionary<string, decimal>();
			foreach (var value in values)
			{
				Values[value.Item1] = value.Item2;
			}
		}

		public BaseDocLineAmountValue(DocLineAmountProperty property)
		{
			Property = new DocLineAmountProperty(property);
			Values = new Dictionary<string, decimal>();
		}

		public Dictionary<string, decimal> Values { get; }

		public DocLineAmountProperty Property { get; }

		public abstract BaseDocLineAmountValue Add(BaseDocLineAmountValue value);

		public abstract QuotationLineList GetQuotationLineList(RateLine rateLine);

		#region Property

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		const string TypePropertyKey = "Type";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string CurrencyPropertyKey = "Currency";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string UnitPropertyKey = "Unit";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string ApplyToPropertyKey = "ApplyTo";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string OperatorBreakPropertyKey = "OperatorBreak";

		#endregion

		#region Value

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string AmountValueKey = "Amount";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected const string RateValueKey = "Rate";

		#endregion
	}
}
