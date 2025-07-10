using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	sealed class MessageProcessorServiceTest : ServiceTaskTestCase<MessageProcessorService>
	{
		public void TestProcessMessages_WithUCMPEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			{
				var cusResMessage = Factory.New<CUSRESEDIMessage>();
				cusResMessage.EM_MessageNum = "1";
				cusResMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				cusResMessage.EM_Status = EDIMessage.Status.Queued;
				cusResMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();
				var cusCarMessage = Factory.New<CUSCAREDIMessage>();
				cusCarMessage.EM_MessageNum = "2";
				cusCarMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				cusCarMessage.EM_Status = EDIMessage.Status.Queued;
				cusCarMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();
				TestServiceLogger logger = new TestServiceLogger();
				MessageProcessorService serviceTask = new MessageProcessorService();
				serviceTask.ServiceLogger = logger;

				serviceTask.RunTask();

				cusResMessage.Reload();
				cusCarMessage.Reload();
				AssertEquals("cusResMessage.EM_Status", EDIMessage.Status.Queued, cusResMessage.EM_Status);
				AssertEquals("cusCarMessage.EM_Status", EDIMessage.Status.Queued, cusCarMessage.EM_Status);
			}
		}

		public void TestProcessMessages_WithUCMPDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			{
				var cusResMessage = Factory.New<CUSRESEDIMessage>();
				cusResMessage.EM_MessageNum = "1";
				cusResMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				cusResMessage.EM_Status = EDIMessage.Status.Queued;
				cusResMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();
				var cusCarMessage = Factory.New<CUSCAREDIMessage>();
				cusCarMessage.EM_MessageNum = "2";
				cusCarMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				cusCarMessage.EM_Status = EDIMessage.Status.Queued;
				cusCarMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();
				TestServiceLogger logger = new TestServiceLogger();
				MessageProcessorService serviceTask = new MessageProcessorService();
				serviceTask.ServiceLogger = logger;

				serviceTask.RunTask();

				cusResMessage.Reload();
				cusCarMessage.Reload();
				AssertNotEquals("cusResMessage.EM_Status", EDIMessage.Status.Queued, cusResMessage.EM_Status);
				AssertNotEquals("cusCarMessage.EM_Status", EDIMessage.Status.Queued, cusCarMessage.EM_Status);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"ZA Customs Response Messages",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.SouthAfricanCustoms),
				};
			}
		}
	}
}
