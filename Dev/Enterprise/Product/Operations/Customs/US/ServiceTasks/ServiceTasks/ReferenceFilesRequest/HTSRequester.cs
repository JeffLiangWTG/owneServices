using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.HTS,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.HTS,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.HTSRequester),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Day",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "5hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	[NeedsDataRefresh]
	public class HTSRequester : ReferenceFilesRequesterWithDataVersion
	{
		protected override void DoSendRequest()
		{
			bool sendMessage = false;
			ZInt nextAttempt = 0;
			ZInt lastHTSAttempt = DataVersion.UZ_Version;
			ZString lastHTSResult = DataVersion.UZ_Note;

			TariffRequestAttemptSender sender = new TariffRequestAttemptSender(lastHTSAttempt, DataVersion.Factory);

			if (lastHTSResult.StartsWith(ExtractReferenceFilesResult.Pending))
			{
				lastHTSResult = ExtractReferenceFilesResult.Failure;
			}

			if (lastHTSResult.StartsWith(ExtractReferenceFilesResult.Success))
			{
				nextAttempt = sender.NextAttempt;
				sendMessage = true;
			}
			else if (lastHTSResult.StartsWith(ExtractReferenceFilesResult.Failure))
			{
				nextAttempt = lastHTSAttempt;
				sendMessage = true;
			}
			else if (lastHTSResult.IsEmpty || DataVersion.UZ_UpdateTime == ZDateTime.MinSmallDateTimeValue)
			{
				if (sender.CurrentYearNotEqualAttemptYear)
				{
					nextAttempt = sender.NextAttemptForYearCutover;
				}
				else
				{
					nextAttempt = lastHTSAttempt;
				}

				sendMessage = true;
			}

			if (sendMessage)
			{
				sender.SendMessageIfAllowed(sendMessage, nextAttempt, DataVersion);

				ServiceLogger.Log(LogType.Information, TaskDescription + " successfully sent.");
			}
		}

		protected override string TaskDescription
		{
			get { return ServiceTaskApplicationCodeList.Descriptions.HTS; }
		}

		protected override USCDataVersion GetOrCreate(BusinessObjectFactory factory)
		{
			return USCDataVersion.GetLastHTSAttempt(factory);
		}

		protected override string ReferenceFileRequesterVersionName
		{
			get { return USCDataVersion.Constant.LastHTSAttempt; }
		}
	}
}
