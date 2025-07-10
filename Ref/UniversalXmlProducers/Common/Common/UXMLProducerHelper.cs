using System;
using System.Linq;

namespace CargoWise.RefDbRepo.XmlProducer.Common
{
	public static class UXMLProducerHelper
	{
		public const string IssueReportingExitCodeDefault = "7";
		public const string SourceDataAppName = "SourceDataAppName";
		public const string SubSource = "SubSource";
		public const string AppContext = "AppContext";
		public const string AppException = "AppException";
		public const string UnhandledException = "UnhandledException";
		public const string SqlPerformance = "SqlPerformance";

		public static bool ExitWIthProducerStatus(int exitCode)
		{
			var producerStatusValues = Enum.GetValues(typeof(ProducerStatus)).Cast<ProducerStatus>()
				.Select(x => (int)x);
			return producerStatusValues.Contains(exitCode);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute")]
	public enum ProducerStatus
	{
		Success = 0,
		Failure = 1,
		ParseFailure = 2,
		MergeFailure = 4
	}
}
