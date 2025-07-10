using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Immutable]
	abstract class DataFormat<T> where T : struct
	{
		public abstract ZString Format(T data, FormattingResult formattingResult);
	}

	class FormattingResult
	{
		public FormattingResult()
		{
			IsFormattedCorrectly = true;
		}

		public bool IsFormattedCorrectly { get; set; }
		public string ErrorMessage { get; set; }
	}
}
