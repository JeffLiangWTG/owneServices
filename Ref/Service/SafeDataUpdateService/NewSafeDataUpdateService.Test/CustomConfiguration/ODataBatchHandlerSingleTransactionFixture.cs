using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class ODataBatchHandlerSingleTransactionFixture
	{
		[Test]
		public void CheckODataBatchHandlerSingleTransaction_Quotas()
		{
			var handler = new ODataBatchHandlerSingleTransaction(() => repositoryMock.Object);
			Assert.AreEqual(65000, handler.MessageQuotas.MaxOperationsPerChangeset);
			Assert.AreEqual(65000, handler.MessageQuotas.MaxPartsPerBatch);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task ExecuteRequestMessagesAsync(bool succeed)
		{
			var response = new Mock<HttpResponse>();
			response.Setup(x => x.StatusCode).Returns((int)(succeed ? HttpStatusCode.OK : HttpStatusCode.InternalServerError));
			var context1 = new Mock<HttpContext>();
			context1.Setup(x => x.Response).Returns(response.Object);
			var changeSetResponse = new ChangeSetResponseItem(new[] { context1.Object });

			var defaultContext = new DefaultHttpContext();
			var operationRequestItem = new Mock<OperationRequestItem>(defaultContext);
			operationRequestItem.Setup(x => x.SendRequestAsync(It.IsAny<RequestDelegate>())).Returns(Task.FromResult((ODataBatchResponseItem)changeSetResponse));
			var changeSetRequestMock = new Mock<ChangeSetRequestItem>(new[] { defaultContext }.AsEnumerable());
			changeSetRequestMock.Setup(x => x.SendRequestAsync(It.IsAny<RequestDelegate>()))
				.Returns(Task.FromResult((ODataBatchResponseItem)changeSetResponse));

			var next = new RequestDelegate(async (context) => await Task.CompletedTask);
			var batchHandler = new ODataBatchHandlerSingleTransaction(() => repositoryMock.Object);
			var result = await batchHandler.ExecuteRequestMessagesAsync(new[] { operationRequestItem.Object }, next);
			operationRequestItem.Verify(x => x.SendRequestAsync(next), Times.Once);
			Assert.AreEqual(changeSetResponse, result.First());

			result = await batchHandler.ExecuteRequestMessagesAsync(new[] { changeSetRequestMock.Object }, next);
			changeSetRequestMock.Verify(x => x.SendRequestAsync(next), Times.Once);
			if (succeed)
			{
				repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), true), Times.Once);
			}
			Assert.AreEqual(Tuple.Create(repositoryMock.Object, true), defaultContext.GetContext());
		}

		Mock<IReferenceDataRepository> repositoryMock = new Mock<IReferenceDataRepository>();
	}
}
