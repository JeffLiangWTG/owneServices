using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Data;
using Enterprise.TransportConsignment.DataTransfer.Universal.Helpers;
using Enterprise.ZArchitecture.Web.Business;
namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/CarrierBooking")]
	public class CarrierBookingController : ApiController
	{
		const string DefaultMediaType = "text/html";

		public CarrierBookingController()
		{
			WebAppEnvironment.Setup();
		}

		[Route("BookCarrierWithCarrierMessagingBuss/{parentTableCode}/{parentPK}")]
		[HttpPost]
		public async Task<IHttpActionResult> BookCarrierWithCarrierMessagingBuss([FromUri] Guid parentPK, [FromUri] string parentTableCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var carrierMessagingBussClientProvider = new CarrierMessagingBussClientProvider();
					await carrierMessagingBussClientProvider.SendBookingRequest(parentPK).ConfigureAwait(true);
				}
				catch (InvalidOperationException exception)
				{
					return Content(HttpStatusCode.InternalServerError, Res.GetString("3E8008E8-AE3C-4ED8-9D01-B906E49F51A9", "Unexpected error happened: {0}", exception.Message));
				}

				return Ok();
			}
		}

		#region Override

		protected override NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value)
		{
			return new CBNegotiatedContentResult<T>(statusCode, value, this, DefaultMediaType);
		}

		protected override OkNegotiatedContentResult<T> Ok<T>(T content)
		{
			return new CBOkNegotiatedContentResult<T>(content, this, DefaultMediaType);
		}
		#endregion

	} 

	class CBNegotiatedContentResult<T> : NegotiatedContentResult<T>
	{
		string MediaType { get; }

		public CBNegotiatedContentResult(HttpStatusCode statusCode, T content, ApiController controller, string mediaType = "text/plain") : base(statusCode, content, controller)
		{
			MediaType = mediaType;
		}

		public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage()
			{
				StatusCode = StatusCode,
				Content = new StringContent(Content.ToString(), Encoding.UTF8, MediaType)
			};

			return Task.FromResult(response);
		}
	}

	class CBOkNegotiatedContentResult<T> : OkNegotiatedContentResult<T>
	{
		string MediaType { get; }

		public CBOkNegotiatedContentResult(T content, ApiController controller, string mediaType = "text/plain") : base(content, controller)
		{
			MediaType = mediaType;
		}

		public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(Content.ToString(), Encoding.UTF8, MediaType)
			};

			return Task.FromResult(response);
		}
	}
}
