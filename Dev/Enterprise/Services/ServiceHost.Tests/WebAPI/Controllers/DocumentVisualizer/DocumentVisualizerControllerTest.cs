using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Forwarding = Enterprise.Integration.Forwarding;

namespace Enterprise.Services.ServiceHost.Tests
{
	public sealed class DocumentVisualizerControllerTest : TestCaseWithFactory
	{
		const string CRESA_MessageID = "FR_CRESA";

		[ExpectNoExceptions]
		public void TestControllerDisposableManager()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			const string tablePrefix = DummyBusinessObject.Schema.TablePrefix;

			var url = $"http://www.goofygoober/cw1api/DocumentVisualizer/SendMessage/{CRESA_MessageID}/{tablePrefix}/{dummy.PK}";

			var mockSender = new Mock<IDocDataObjectWithoutUIMessageSender>();
			mockSender
				.Setup(s => s.SendMessage(It.IsAny<BusinessObject>(), It.IsAny<INotifications>()))
				.Callback<BusinessObject, INotifications>((biz, n) =>
				{
					biz.Factory.SubscribeForDispose(new DisposableAction(() =>
					{
						return;
					}));
				})
				.Returns(true);

			var mockSenders = new Hashtable
			{
				{ CRESA_MessageID, new TestObjectHandle(mockSender.Object) }
			};

			using (ObjectFactory.Substitute("DocDataObjectWithoutUIMessageSendersProvider", mockSenders))
			{
				var controller = new DocumentVisualizerController();
				controller.Request = new HttpRequestMessage(HttpMethod.Get, url);
				var actionResult = controller.SendMessage(CRESA_MessageID, tablePrefix, dummy.PK.ToGuid());
			}
		}

		#region Success'

		public void TestSendMessage_Success_NoMessages()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			const string tablePrefix = DummyBusinessObject.Schema.TablePrefix;

			var url = $"http://www.goofygoober/cw1api/DocumentVisualizer/SendMessage/{CRESA_MessageID}/{tablePrefix}/{dummy.PK}";

			var mockSender = new Mock<IDocDataObjectWithoutUIMessageSender>();
			mockSender
				.Setup(s => s.SendMessage(It.IsAny<BusinessObject>(), It.IsAny<INotifications>()))
				.Returns(true);

			var mockSenders = new Hashtable
			{
				{ CRESA_MessageID, new TestObjectHandle(mockSender.Object) }
			};

			using (ObjectFactory.Substitute("DocDataObjectWithoutUIMessageSendersProvider", mockSenders))
			{
				var controller = new DocumentVisualizerController();
				controller.Request = new HttpRequestMessage(HttpMethod.Get, url);
				var actionResult = controller.SendMessage(CRESA_MessageID, tablePrefix, dummy.PK.ToGuid());

				var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				Assert("should process message successfully", response.IsSuccessStatusCode);

				var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SendMessageResponse>(content);
				Assert("response indicates success", responseObj.Success);
				AssertNull("there's no validation messages", responseObj.Messages);
			}
		}

		public void TestSendMessage_Success_WithMessages()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			const string tablePrefix = DummyBusinessObject.Schema.TablePrefix;

			var url = $"http://www.goofygoober/cw1api/DocumentVisualizer/SendMessage/{CRESA_MessageID}/{tablePrefix}/{dummy.PK}";

			var mockSender = new Mock<IDocDataObjectWithoutUIMessageSender>();
			mockSender
				.Setup(s => s.SendMessage(It.IsAny<BusinessObject>(), It.IsAny<INotifications>()))
				.Callback<BusinessObject, INotifications>((biz, n) =>
				{
					n.AddWarning("some warning");
				})
				.Returns(true);

			var mockSenders = new Hashtable
			{
				{ CRESA_MessageID, new TestObjectHandle(mockSender.Object) }
			};

			using (ObjectFactory.Substitute("DocDataObjectWithoutUIMessageSendersProvider", mockSenders))
			{
				var controller = new DocumentVisualizerController();
				controller.Request = new HttpRequestMessage(HttpMethod.Get, url);
				var actionResult = controller.SendMessage(CRESA_MessageID, tablePrefix, dummy.PK.ToGuid());

				var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				Assert("should process message successfully", response.IsSuccessStatusCode);

				var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SendMessageResponse>(content);
				Assert("response indicates success", responseObj.Success);
				AssertEquals("there 1 validation message", 1, responseObj.Messages.Count);

				var validationMessage = responseObj.Messages.Single();
				AssertEquals("MessageType", "Warning", validationMessage.MessageType);
				AssertEquals("Message", "some warning", validationMessage.Message);
			}
		}

		#endregion

		#region Failures

		public void TestSendMessage_Failure_ForwardingShipment()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = "SEA";
			shipment[JobShipmentSchema.Constants.JS_RL_NKOrigin] = "AUSYD";
			shipment[JobShipmentSchema.Constants.JS_RL_NKDestination] = "FRPAR";
			Factory.Save();

			const string tablePrefix = JobShipmentSchema.Constants.Prefix;

			var url = $"http://www.goofygoober/cw1api/DocumentVisualizer/SendMessage/{CRESA_MessageID}/{tablePrefix}/{shipment.PK}";

			var controller = new DocumentVisualizerController();
			controller.Request = new HttpRequestMessage(HttpMethod.Get, url);
			var actionResult = controller.SendMessage(CRESA_MessageID, tablePrefix, shipment.PK.ToGuid());

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			Assert("should send message successfully", response.IsSuccessStatusCode);

			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<SendMessageResponse>(content);
			Assert("response indicates failure", !responseObj.Success);
			Assert("there's validation messages", responseObj.Messages.Count > 0);
		}

		public void TestSendMessage_Failure_BadRequest()
		{
			AssertFailedRequest(HttpStatusCode.BadRequest, string.Empty, JobShipmentSchema.Constants.Prefix, Guid.NewGuid());
			AssertFailedRequest(HttpStatusCode.BadRequest, CRESA_MessageID,string.Empty, Guid.NewGuid());
			AssertFailedRequest(HttpStatusCode.BadRequest, CRESA_MessageID, JobShipmentSchema.Constants.Prefix, Guid.Empty);
		}

		public void TestSendMessage_Failure_UnknownBusinessObject()
		{
			AssertFailedRequest(HttpStatusCode.NotFound, CRESA_MessageID, JobShipmentSchema.Constants.Prefix, Guid.NewGuid());
		}

		public void TestSendMessage_Failure_InvalidTablePrefix()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertFailedRequest(HttpStatusCode.NotFound, CRESA_MessageID, "BAD", dummy.PK.ToGuid());
			ErrorReporter.Clear();
		}

		public void TestSendMessage_Failure_UnsupportedMessage()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertFailedRequest(HttpStatusCode.NotFound, "Unsupported", DummyBusinessObject.Schema.TablePrefix, dummy.PK.ToGuid());
		}

		void AssertFailedRequest(HttpStatusCode expectedStatusCode, string messageID, string tablePrefix, Guid jobPK)
		{
			var url = $"http://www.goofygoober/cw1api/DocumentVisualizer/SendMessage/{CRESA_MessageID}/{tablePrefix}/{jobPK}";

			var mockSender = new Mock<IDocDataObjectWithoutUIMessageSender>();
			mockSender
				.Setup(s => s.SendMessage(It.IsAny<BusinessObject>(), It.IsAny<INotifications>()))
				.Returns(false);

			var mockSenders = new Hashtable
			{
				{ CRESA_MessageID, new TestObjectHandle(mockSender.Object) }
			};

			using (ObjectFactory.Substitute("DocDataObjectWithoutUIMessageSendersProvider", mockSenders))
			{
				var controller = new DocumentVisualizerController();
				controller.Configuration = new HttpConfiguration();
				controller.Request = new HttpRequestMessage(HttpMethod.Get, url);
				var actionResult = controller.SendMessage(messageID, tablePrefix, jobPK);

				var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				AssertEquals("expected status code", expectedStatusCode, response.StatusCode);
			}
		}

		#endregion
	}
}
