using System;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class UniversalPublisherControllerTest : TestCase
	{
		#region TestUniversalPublisher_PublishUniversal

		public void TestUniversalPublisher_PublishUniversal()
		{
			var jobPK1 = Guid.NewGuid();
			var jobPK2 = Guid.NewGuid();
			var universalService = new Mock<ITransitUniversalService>();
			(string ErrorType, string Message) error = (MessageTypes.Error, "Error Message");
			universalService.Setup(x => x.PublishUniversal(new ZGuid[] { jobPK1, jobPK2 }, WhsItemReceiveConsignmentSchema.Constants.TableName)).Returns(error).Verifiable();

			ObjectFactory.Substitute("ITransitUniversalService", universalService.Object);

			var controller = new UniversalPublisherController();
			controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://www.goofygoober/cw1api/Universal/PublishUniversal/");
			var actionResult = controller.PublishUniversal(new Guid[] { jobPK1, jobPK2 }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assert(response.IsSuccessStatusCode);
			AssertEquals("{\"messageType\":\"Error\",\"message\":\"Error Message\"}", content);
		}

		#endregion
	}
}
