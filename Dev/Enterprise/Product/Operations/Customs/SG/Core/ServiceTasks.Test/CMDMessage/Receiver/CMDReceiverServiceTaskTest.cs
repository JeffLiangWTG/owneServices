using System.Collections.Generic;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.Testing
{
	[TestedType(typeof(CMDReceiverServiceTask))]
	sealed class CMDReceiverServiceTaskTest : ServiceTaskTestCase<CMDReceiverServiceTask>
	{
		public void TestRunTask()
		{
			var task = new CMDReceiverServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"Cargo Manifest Declaration (SG)",
						MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.CMDMessage)
				};
			}
		}
	}
}
