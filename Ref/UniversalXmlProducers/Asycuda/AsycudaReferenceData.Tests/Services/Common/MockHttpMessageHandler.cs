using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services;

public class MockHttpMessageHandler : HttpMessageHandler
{
	private readonly string _responseContent;
	private readonly HttpStatusCode _statusCode;

	public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		_responseContent = responseContent;
		_statusCode = statusCode;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var response = new HttpResponseMessage(_statusCode)
		{
			Content = new StringContent(_responseContent)
		};
		return Task.FromResult(response);
	}
}
