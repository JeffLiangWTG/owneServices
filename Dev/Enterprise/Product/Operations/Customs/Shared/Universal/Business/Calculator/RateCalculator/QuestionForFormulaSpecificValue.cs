
using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class QuestionForFormulaSpecificValue
	{
		public QuestionForFormulaSpecificValue(string question)
			: this(question, 0, 0)
		{
		}

		public QuestionForFormulaSpecificValue(string question, int precision, int scale)
		{
			Question = question;
			Precision = precision;
			Scale = scale;
		}

		public int Precision { get; private set; }
		public int Scale { get; private set; }
		public string Question { get; private set; }
		public decimal Answer { get; private set; }

		public void SetUserAnswer(decimal answer)
		{
			Answer = answer;
		}

		public bool IsDefaultPrecisionScale
		{
			get { return Precision == 0 && Scale == 0; }
		}

		public static string ADefaultFormulaSpecificValueIsRequired(int scale)
		{
			var defaultValue = "0" + (scale < 1 ? "" : ".".PadRight(scale + 1, '0'));
			return ResString.GetMultilingualString("{A3199064-E63F-4F85-8A03-0A85FDDC72C2}", "Value should be specified. The default value should be {0}.", defaultValue);
		}

		public static ZString GetDecimalFormat(ZString value, int precision, int scale, bool showGroupSeparators = true)
		{
			var decimalValue = Utilities.Round(Utilities.ConvertToDecimal(value.ToString()), scale);
			if (decimalValue > 0m)
			{
				decimalValue = Math.Min(GetMaximumValue(precision, scale), decimalValue);
			}
			return showGroupSeparators ? Utilities.FormatNumberWithGroupSeparators(decimalValue, scale, Culture.CurrentCompanyCountryCulture) : Utilities.FormatNumber(decimalValue, scale, Culture.CurrentCompanyCountryCulture);
		}

		public static ZDecimal GetMaximumValue(int precision, int scale)
		{
			return ZDecimal.ParseSafe("".PadRight(Math.Max(0, precision - scale), '9') + (scale > 0 ? "." + "".PadRight(scale, '9') : ""), ZDecimal.Zero);
		}
	}
}
