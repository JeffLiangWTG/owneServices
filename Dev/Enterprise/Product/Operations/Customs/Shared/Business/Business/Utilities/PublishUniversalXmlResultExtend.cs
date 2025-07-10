using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public static class PublishUniversalXmlResultExtend
	{
		public static void AddDataExportFailureLogIfNeeded(this PublishUniversalXmlResult result, IStmALogParent logParent, DataContextType dataContextType, Func<PublishToUniversalResult, KeyValuePair<string, string>[]> buildParameters = null)
		{
			if (result == null || logParent == null)
			{
				return;
			}

			var publishToUniversalResult = PublishToUniversalResult.New(result, dataContextType, ZString.Empty);
			if (publishToUniversalResult != null && publishToUniversalResult.ResultType == UniversalResult.HadErrors)
			{
				var parameters = buildParameters?.Invoke(publishToUniversalResult) ?? Array.Empty<KeyValuePair<string, string>>();
				logParent.Logs.AddNew(Events.DataExportFailure, ZDateTimeOffset.Now, parameters);
			}
		}
	}
}
