using System;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.ForeignPort,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.ForeignPort,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.ForeignPortCodeRequester),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Day",
	DefaultScheduleRunEvery = "1weeks",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "18hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class ForeignPortCodeRequester : ReferenceFilesRequesterWithDataVersion
	{
		protected override void DoSendRequest()
		{
			new ReferenceFileRequester().RequestForeignPortCodes();
		}

		protected override string TaskDescription
		{
			get { return ServiceTaskApplicationCodeList.Descriptions.ForeignPort; }
		}

		protected override string ReferenceFileRequesterVersionName
		{
			get { return USCDataVersion.Constant.RefFileRequestForeignPortCodeTimeStamp; }
		}
	}
}
