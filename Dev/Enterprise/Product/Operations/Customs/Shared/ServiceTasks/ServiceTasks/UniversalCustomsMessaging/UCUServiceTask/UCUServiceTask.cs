using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UniversalCustomsMessagingConstants.ServiceTaskCodes.Unpacking,
	"UniversalCustomsMessaging Process:Unpack incoming EDIInterchange to EDIMessage",
	"CUS",
	typeof(Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.UCUServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	AllowsMultipleInstances = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCUServiceTask : UniversalCustomsMessagingInterchangeServiceTask<IUniversalCustomsInterchangeUnpacker>
	{
		[HostedServiceRequirement]
		public static string HasUniversalCustomsInterchangeUnpackers() =>
			UCUSubscribers.HasUniversalCustomsInterchangeUnpackers()
			? string.Empty
			: (NoResString)"There is no Customs message subscribed to Universal Customs Interchange Unpacker";

		public override void RunTask(CancellationToken token)
		{
			var logger = GetNewLogger(ServiceLogger);

			try
			{
				logger.Log("UCU Service Task start");

				var branch = GlbBranch.GetFirstActiveBranch();
				using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskCore(logger, token);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.LogWarning($"UCU Service Task Error. Message: {ex.Message}");
			}
		}

		protected override Dictionary<string, IUniversalCustomsInterchangeUnpacker> GetUCMPSubscribers()
			=> UCUSubscribers.GetApplicationCodes().ToDictionary(applicationCode => applicationCode, UCUSubscribers.GetInterchangeUnpacker);

		protected override IUniversalCustomsMessagingInterchangeProcessor GetProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsInterchangeUnpacker handler, CancellationToken token)
			=> new UCUProcessor(logger, applicationCode, handler, token);

		protected override string ServiceTaskCode => UniversalCustomsMessagingConstants.ServiceTaskCodes.Unpacking;
	}
}
