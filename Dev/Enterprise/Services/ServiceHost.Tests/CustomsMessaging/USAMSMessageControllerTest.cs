using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Definitions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class USAMSMessageControllerTest : TestCase
	{
		public void TestSendManifestSupportsHttpPost()
		{
			var sendManifestInfo = typeof(USAMSMessageController).GetMethods().Single(x => x.Name == "SendManifest");
			AssertEquals(1, sendManifestInfo.GetCustomAttributes(typeof(HttpPostAttribute), false).Length);
		}

		public void TestSendManifest()
		{
			using (var controller = new USAMSMessageController())
			{
				var bills = new[] { new USAMSManifestBillAmendment(), new USAMSManifestBillAmendment() };
				messageSenderMock.Setup(x => x.SendManifest(headerPK, bills, false)).Returns(new Tuple<int, string>(1, string.Empty));

				var response = controller.SendManifest(headerPK, bills);
				var result = JsonConvert.DeserializeObject<CreatingMessagesResult>(response.Content.ReadAsStringAsync().Result);

				AssertEquals(1, result.NoOfMessagesCreated);
				AssertEquals(string.Empty, result.ErrorMessage);
			}
		}

		public void TestSendManifest_Error()
		{
			using (var controller = new USAMSMessageController())
			{
				messageSenderMock.Setup(x => x.SendManifest(headerPK, null, false)).Returns(new Tuple<int, string>(0, "No AMS Header has been created."));

				var response = controller.SendManifest(headerPK, null);
				var result = JsonConvert.DeserializeObject<CreatingMessagesResult>(response.Content.ReadAsStringAsync().Result);

				AssertEquals(0, result.NoOfMessagesCreated);
				AssertEquals("No AMS Header has been created.", result.ErrorMessage);
			}
		}

		public void TestSendManifestInAnotherThread()
		{
			Task.Run(() =>
			{
				using (var controller = new USAMSMessageController())
				{
					messageSenderMock.Setup(x => x.SendManifest(headerPK, null, false)).Returns(new Tuple<int, string>(1, string.Empty));

					var response = controller.SendManifest(headerPK, null);
					var result = JsonConvert.DeserializeObject<CreatingMessagesResult>(response.Content.ReadAsStringAsync().Result);

					AssertEquals(1, result.NoOfMessagesCreated);
					AssertEquals(string.Empty, result.ErrorMessage);
				}
			}).Wait();
		}

		protected override void SetUp()
		{
			headerPK = Guid.NewGuid();

			messageSenderMock = new Mock<Integration.Customs.US.USAMS.IUSAMSMessageSender>();
			ObjectFactory.Substitute("USAMS.IUSAMSMessageSender", messageSenderMock.Object);

			base.SetUp();
		}
		Guid headerPK;
		Mock<Integration.Customs.US.USAMS.IUSAMSMessageSender> messageSenderMock;
	}
}
