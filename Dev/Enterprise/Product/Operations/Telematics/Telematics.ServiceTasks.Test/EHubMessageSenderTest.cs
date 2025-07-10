using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	class EHubMessageSenderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			eHubMessageSender = new EHubMessageSender();
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => eHubMessageSender.Send(null, Enumerable.Empty<string>(), Enumerable.Empty<string>()));
				AssertEquals("factory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => eHubMessageSender.Send(Factory, null, Enumerable.Empty<string>()));
				AssertEquals("messages", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => eHubMessageSender.Send(Factory, Enumerable.Empty<string>(), null));
				AssertEquals("recipients", result.ParamName);
			});
		}

		public void TestAddsMessagesForAllRecipients()
		{
			CombineAssertions(() =>
			{
				Test(new[] { "message1" }, new[] { "recipient1" });
				Test(new[] { "message1", "message2" }, new[] { "recipient1" });
				Test(new[] { "message1" }, new[] { "recipient1", "recipient2" });
			});

			void Test(string[] messages, string[] recipients)
			{
				// Arrange
				foreach (var ediInterchange in Factory.Load<EDIInterchange>(new ZQuery()))
				{
					ediInterchange.Delete();
				}
				ErrorReporter.Clear(); // Because of this: You are attempting to delete an EDIMessage that has been queued for the eHub Outbound Messages service task

				// Act
				eHubMessageSender.Send(Factory, messages, recipients);
				Factory.Save();

				// Assert
				var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(recipients, ediInterchanges.Select(interchange => interchange.EI_To));
				foreach (var ediInterchange in ediInterchanges)
				{
					AssertEquals(Env.CurrentCompany.GetLicenceCode(), ediInterchange.EI_From);
					AssertEquals(ReceiveTransmitList.Codes.Transmit, ediInterchange.EI_ReceiveTransmit);
					AssertEquals(ApplicationCodeList.Codes.Telematics, ediInterchange.EI_ApplicationCode);
					AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, ediInterchange.EI_Status);
					AssertEquals(EDIInterchangeTransportTypeList.Codes.eHub, ediInterchange.EI_TransportType);

					var ediMessages = ediInterchange.ContainedMessages.Cast<EDIMessage>();
					AssertContainsExactElementsInAnyOrder(messages, ediMessages.Select(message => message.EM_MessageText));
					foreach (var ediMessage in ediMessages)
					{
						AssertEquals(ApplicationCodeList.Codes.Telematics, ediMessage.EM_ApplicationCode);
						AssertEquals(TelematicsMessageList.Codes.TelematicsXmlData, ediMessage.EM_MessageSubType);
						AssertEquals(ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
						AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, ediMessage.EM_Status);
					}
				}
			}
		}

		public void TestAddsNothingOnEmptyMessages()
		{
			// Arrange
			foreach (var ediInterchange in Factory.Load<EDIInterchange>(new ZQuery()))
			{
				ediInterchange.Delete();
			}

			// Act
			eHubMessageSender.Send(Factory, Enumerable.Empty<string>(), new[] { "recipient1" });

			// Assert
			AssertEquals(0, Factory.Load<EDIInterchange>(new ZQuery()).Length);
		}

		public void TestAddsNothingOnEmptyRecipients()
		{
			// Arrange
			foreach (var ediInterchange in Factory.Load<EDIInterchange>(new ZQuery()))
			{
				ediInterchange.Delete();
			}

			// Act
			eHubMessageSender.Send(Factory, new[] { "message1" }, Enumerable.Empty<string>());

			// Assert
			AssertEquals(0, Factory.Load<EDIInterchange>(new ZQuery()).Length);
		}

		EHubMessageSender eHubMessageSender;
	}
}
