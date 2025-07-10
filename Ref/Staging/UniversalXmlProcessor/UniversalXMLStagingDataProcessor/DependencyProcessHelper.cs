using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class DependencyProcessHelper
	{
		public static DependencyProcessResult GetDependencyProcessResult(IStagingDataProvider stagingDataProvider, IEnumerable<Dependency> dependencies, bool isTimeOut)
		{
			if(dependencies is null || !dependencies.Any())
			{
				return new DependencyProcessResult(ProcessAction.ProcessPrimaryData, null);
			}

			var processInfos = dependencies.Select(x => GetDependencyProcessInfo(stagingDataProvider, x));

			var missingRequiredDependency = processInfos.FirstOrDefault(x =>
				x.Dependency.DependencyType == DependencyType.Required && (x.Status == ProcessStatus.Error || x.Status == ProcessStatus.Missing));
			if (missingRequiredDependency != null && isTimeOut)
			{
				return new DependencyProcessResult(ProcessAction.ReportError, missingRequiredDependency.Dependency);
			}
			if (missingRequiredDependency != null && !isTimeOut)
			{
				return new DependencyProcessResult(ProcessAction.WaitForDependency, missingRequiredDependency.Dependency);
			}

			var dependencyNotMerged = processInfos.FirstOrDefault(x => x.Status == ProcessStatus.NotMerged);
			if (dependencyNotMerged != null)
			{
				return new DependencyProcessResult(ProcessAction.WaitForDependency, dependencyNotMerged.Dependency);
			}

			return new DependencyProcessResult(ProcessAction.ProcessPrimaryData, null);
		}

		public static DependencyProcessInfo GetDependencyProcessInfo(IStagingDataProvider stagingDataProvider, Dependency dependency)
		{
			var processInfo = new DependencyProcessInfo { Dependency = dependency };

			var dependencySourceDatas = stagingDataProvider.GetDependencySourceData(dependency).ToArray();
			if (dependencySourceDatas is null || !dependencySourceDatas.Any())
			{
				processInfo.Status = ProcessStatus.Missing;
			}
			else if (dependencySourceDatas.All(x => x.IsSourceDataErrorStatus()))
			{
				processInfo.Status = ProcessStatus.Error;
			}
			else if (dependencySourceDatas.Any(x => x.IsSourceDataMergedStatus()))
			{
				processInfo.Status = ProcessStatus.Merged;
			}
			else
			{
				processInfo.Status = ProcessStatus.NotMerged;
			}

			return processInfo;
		}
	}
}
