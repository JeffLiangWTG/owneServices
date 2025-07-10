using System.Globalization;
using System.Linq;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	internal abstract class OneStopLineRecord
	{
		public OneStopLineRecord(string[] lineRecord)
		{
			LineRecord = lineRecord;
		}

		protected string[] LineRecord { get; }

		protected void ReportErrorIfUAT(string message)
		{
			OneStopProcessorBase.ReportErrorIfUAT(ErrorKey, message + ": " + string.Join(",", LineRecord.Select(s => string.Format(CultureInfo.InvariantCulture, "\"{0}\"", s)).ToArray()));
		}

		protected abstract string ErrorKey { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Reporter Message")]
		protected const string InvalidDateTimeRecord = "One or more date time field values in a record are either of invalid format or out of smalldatetime range";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Reporter Message")]
		protected const string MaxLengthExceededErrorMessage = "One or more identifying fields have exceeded the maximum length.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Reporter Message")]
		protected const string InvalidNumberOfFieldsErrorMessage = "Invalid number of fields in a record.";
	}
}
