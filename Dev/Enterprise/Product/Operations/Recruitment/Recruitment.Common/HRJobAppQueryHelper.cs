using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Common
{
	public static class ApplicationQueryHelper
	{
		public static HRJobApplication[] WithinDates(ZDateTime newest, ZDateTime oldest)
			=> new BusinessObjectFactory().Load<HRJobApplication>(WithinDatesQuery(newest, oldest));

		public static ZQuery WithinDatesQuery(ZDateTime newest, ZDateTime oldest)
		{
			var query = new ZQuery
			{
				OrderBy = HRJobApplicationSchema.Constants.HP_SubmissionTimeUtc + OrderByClause.Descending,
			};
			_ = query.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, oldest);
			_ = query.AddToFilter(HRJobApplicationSchema.HP_SubmissionTimeUtc, SQLComparisonOperator.LessThan, newest);
			return query;
		}

		public static bool BatchConvertResumes(ILogger logger, Queue<HRJobApplication> applications, Action<HRJobApplication> postConversionFunc, CancellationToken token)
		{
			if (!RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.Value || applications == null || applications.Count == 0)
			{
				return true;
			}

			void LogInfo(string message) => logger?.Log(LogType.Information, $"[BatchConvertResumes] {message}");

			LogInfo(string.Format(
				CultureInfo.InvariantCulture,
				(NoResString)"Found {0} application{1} to convert",
				applications.Count,
				applications.Count > 1 ? (NoResString)"s" : string.Empty));

			var resumeConverter = ObjectFactory.Get<IResumeConverter>(nameof(IResumeConverter), logger);

			while (applications.Count > 0)
			{
				LogInfo(string.Format(CultureInfo.InvariantCulture, (NoResString)"Applications remaining={0}", applications.Count));
				if (token.IsCancellationRequested)
				{
					LogInfo((NoResString)"Cancellation requested, task is incomplete");
					return false;
				}

				var currentApplication = applications.Dequeue();

				try
				{
					resumeConverter.ConvertAvailableResumes(currentApplication);
					postConversionFunc?.Invoke(currentApplication);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					LogInfo(string.Format(CultureInfo.InvariantCulture, (NoResString)"ConvertApi error caught, continuing"));
				}
			}

			return true;
		}
	}
}
