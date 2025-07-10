using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.AntiDumping,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.AntiDumping,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.AntiDumpingRequester),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Day",
	DefaultScheduleRunEvery = "52weeks",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "18hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class AntiDumpingRequester : ReferenceFilesRequesterWithDataVersion
	{
		protected override void DoSendRequest()
		{
			var factory = new BusinessObjectFactory();
			USCCountryCollection countries = new USCCountryCollection(factory);
			countries.Load();

			var messageBuilder = new ACEACQueryMessageBuilder();
			var queryInput = new ACEACCaseQueryInput();
			queryInput.CaseStatus = "B";

			foreach (USCCountry country in countries)
			{
				queryInput.CountryCode = country.UC_Code;
				messageBuilder.Generate(factory, queryInput);
			}

			factory.Save();
		}

		protected override string TaskDescription
		{
			get { return ServiceTaskApplicationCodeList.Descriptions.AntiDumping; }
		}

		protected override string ReferenceFileRequesterVersionName
		{
			get { return USCDataVersion.Constant.RefFileRequestAntiDumpingTimeStamp; }
		}
	}
}
