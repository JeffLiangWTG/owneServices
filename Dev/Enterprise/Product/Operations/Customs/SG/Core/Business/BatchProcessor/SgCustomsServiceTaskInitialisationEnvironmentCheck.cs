using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Customs.Business.BatchProcessor;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SgCustomsServiceTaskInitialisationEnvironmentCheck
	{
		public static string[] Validate()
		{
			var environmentFailureDescriptions = Array.Empty<string>();
			try
			{
				var environmentChecker = new BatchProcessorEnvironmentChecker();
				environmentFailureDescriptions = environmentChecker.CheckEverythingRequiredToRunIsInPlace();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				environmentFailureDescriptions = new[] { string.Format(CultureInfo.InvariantCulture, "{0} occured while checking the task requirements.  An issue has been raised.", ex.Message) };
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return environmentFailureDescriptions;
		}

		protected class BatchProcessorEnvironmentChecker : BaseEnvironmentChecker
		{
		}
	}
}
