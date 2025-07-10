using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ServiceTasks
{
	public abstract class ReferenceFilesRequesterWithDataVersion : ReferenceFilesRequester
	{
		protected ReferenceFilesRequesterWithDataVersion()
		{
		}

		public USCDataVersion DataVersion
		{
			get
			{
				if (dataVersion == null || dataVersion.IsDeleted)
				{
					var factory = new BusinessObjectFactory();
					dataVersion = GetOrCreate(factory);
				}
				return dataVersion;
			}
		}
		USCDataVersion dataVersion;

		protected virtual USCDataVersion GetOrCreate(BusinessObjectFactory factory)
		{
			return USCDataVersion.GetOrCreate(factory, ReferenceFileRequesterVersionName);
		}

		protected abstract string ReferenceFileRequesterVersionName { get; }

		protected override void PerformUpdate(Action updateAction)
		{
			try
			{
				DataVersion.PerformUpdate(updateAction);
			}
			finally
			{
				dataVersion = null;
			}
		}

		protected override void SaveChanges()
		{
			if (DataVersion.HasChanges)
			{
				DataVersion.Factory.Save();
			}
		}

		protected override void SetUpdateTime(ZDateTime updateTime)
		{
			DataVersion.UZ_UpdateTime = updateTime;
			DataVersion.SetNoteWithDatabaseDetail(DataVersion.GetNoteWithoutDatabaseDetail());
		}

		protected override ZDateTime GetLastUpdateTime()
		{
			return DataVersion.UZ_UpdateTime;
		}
	}

	public abstract class ReferenceFilesRequester : Customs.ServiceTasks.CustomsServiceTask
	{
		protected ReferenceFilesRequester()
		{
		}

		const int hoursToElapseTillNextRequest = 12;
		protected sealed override void RunTaskCore(CancellationToken token)
		{
			bool result = false;
			var hasABICertifiedUnitedStatesCompany = false;
			var countryCodes = new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico };

			foreach (string country in countryCodes)
			{
				token.ThrowIfCancellationRequested();
				foreach (var company in GlbCompany.GetActiveCompanies(country))
				{
					token.ThrowIfCancellationRequested();
					if (IsCompanyABICertified(company.PK))
					{
						hasABICertifiedUnitedStatesCompany = true;
						foreach (var branchPK in company.Branches.Where(x => x.GB_IsActive).Select(x => x.PK.ToGuid()))
						{
							token.ThrowIfCancellationRequested();
							using (DisposableEnvironment.ForBranch(branchPK))
							{
								if (!string.IsNullOrWhiteSpace(USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)))
								{
									result = true;
									var lastSentTimeStamp = GetLastUpdateTime();

									if (lastSentTimeStamp.IsValid && (ZDateTime.UtcNow - lastSentTimeStamp) < new TimeSpan(hoursToElapseTillNextRequest, 0, 0))
									{
										ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "The last request was made at '{0}'(UTC). Reference file requests will not be sent more frequently than once every {1} hours. This is to avoid clogging up of the customs queue with large responses that may impact system performance and normal operational activities.", lastSentTimeStamp.ToLongTimeString(), hoursToElapseTillNextRequest));
										break;
									}
									else
									{
										if (lastSentTimeStamp == GetLastUpdateTime()) // if timeStamp is different to before lock means that another system has update the data after this service task has loaded from db
										{
											PerformUpdate(() =>
											{
												RunTaskHandleEmailSendFailure(() =>
												{
													ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Start: {0}", TaskDescription));
													DoSendRequest();
													SetUpdateTime(ZDateTime.UtcNow);
													SaveChanges();
													ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "End: {0}", TaskDescription));
												});
											});
										}
									}
								}
							}
						}
						if (result)
						{
							break;
						}
						else
						{
							ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, NoProcessingDistrictPortForCompany, company.GC_Code));
						}
					}
				}

				if (result)
				{
					break;
				}
			}

			if (!hasABICertifiedUnitedStatesCompany)
			{
				ServiceLogger.Log(LogType.Error, NoABICertifiedUnitedStatesCompany);
			}
		}

		protected virtual void PerformUpdate(Action updateAction)
		{
			updateAction();
		}

		protected abstract void SaveChanges();
		protected abstract void SetUpdateTime(ZDateTime updateTime);
		protected abstract ZDateTime GetLastUpdateTime();

		public const string NoABICertifiedUnitedStatesCompany = "No company is ABI Certified and system could not send reference file update request messages.";
		public const string NoProcessingDistrictPortForCompany = "The reference file request will not be sent because Company '{0}' has no Branch with Processing District Port Code.";

		bool IsCompanyABICertified(ZGuid companyPK)
		{
			var entryFiler = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return entryFiler.EntryFilerCode != "" && entryFiler.IsABICertified;
		}

		protected virtual TimeSpan GetUtcOffset()
		{
			return TimeZoneInfo.Local.GetUtcOffset(ZDateTime.UtcNow.ToDateTime());
		}

		public TimeSpan GetEstimatedRunTime(TimeSpan dBStartTime)
		{
			// Utc Offset for current time
			TimeSpan usUtcOffset = GetUtcOffset();

			// US EDT difference with UTC 4 hours 
			TimeSpan timeDifference = usUtcOffset.Add(new TimeSpan(4, 0, 0));

			// add to start time difference
			TimeSpan estimatedRunTime = dBStartTime.Add(timeDifference);

			estimatedRunTime.Add(usUtcOffset); //This was previously being used as Utc. To have the equivalent Local time for the new Local field, we need to add the current offset.

			return estimatedRunTime;
		}

		protected abstract void DoSendRequest();
		protected abstract string TaskDescription { get; }
	}
}
