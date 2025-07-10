using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.Testing
{
	[TestedType(typeof(CMDInboundInterchangeProcessorServiceTask))]
	sealed class CMDInboundInterchangeProcessorServiceTaskTest : ServiceTaskTestCase<CMDInboundInterchangeProcessorServiceTask>
	{
		[TestDate(2019, 1, 1, 0, 0, 0)]
		public void TestRunTask()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeCMD;
			interchange.EI_IsActive = true;
			var company = new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Singapore)[0];
			interchange.EI_GB = company.FirstActiveBranch.PK;
			Factory.Save();
			var logger = new TestServiceLogger();
			var task = new CMDInboundInterchangeProcessorServiceTask()
			{ ServiceLogger = logger };
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			AssertContains("Information|SGI Processing for Company SIN.", logger.ToString());
			AssertContains("Warning|\tDid not find exactly one consol dated within the MAWB recycle period. Found 0. MAWB# was .", logger.ToString());
			AssertContains("Warning|\tCould not find originating message for inbound '' message for MAWB '' and HAWB ''. Ignoring.", logger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"Cargo Manifest Declaration (SG) interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SingaporeCMD),
				};
			}
		}
	}
}
