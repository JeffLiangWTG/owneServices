using System;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.Country,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.Country,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.CountryRequester),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Day",
	DefaultScheduleRunEvery = "4weeks",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "18hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class CountryRequester : ReferenceFilesRequesterWithDataVersion
	{
		protected override void DoSendRequest()
		{
			new ReferenceFileRequester().RequestCountry();
		}

		protected override string TaskDescription
		{
			get { return ServiceTaskApplicationCodeList.Descriptions.Country; }
		}

		protected override string ReferenceFileRequesterVersionName
		{
			get { return USCDataVersion.Constant.RefFileRequestCountryCodeTimeStamp; }
		}
	}
}
