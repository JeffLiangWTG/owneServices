using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.BatchProcessor.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	sealed class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestMessagesCreatedFromInterchange()
		{
			ZACInterchangeTest.ZACInterchangeForTest interchange1;
			ZACInterchangeTest.ZACInterchangeForTest interchange2;
			ZACInterchangeTest.ZACInterchangeForTest interchange3;
			ZACInterchangeTest.SetUpTwoRealInterchangesForTest(Factory, out interchange1, out interchange2, out interchange3);
			interchange1.EI_Status = interchange2.EI_Status = interchange3.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			var log = InitialiseAndRunTaskSchedule(new MessageRetrieverService());
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.SouthAfricanCustoms);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange1.PK, interchange2.PK, interchange3.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "RES");
			var messages = Factory.Load<EDIMessage>(query);
			AssertEquals("Running the ZCR task should have created three 'RES' messages", 3, messages.Length);
			AssertEquals("... And the messages should be queued", ZAMessage.Status.Queued, messages[0].EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"ZA Customs interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SouthAfricanCustoms),
				};
			}
		}
	}
}
