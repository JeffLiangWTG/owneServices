using System.Web.Http;
using System.Web.Http.Description;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Services.ServiceHost;

[GlowTicketAuthentication]
[RoutePrefix("api/rating/jobs")]
public class JobController : ApiController
{
	[HttpPost]
	[Route("")]
	[ResponseType(typeof(CreateJobResponseDto))]
	public IHttpActionResult CreateConsol([FromBody] CreateJobDto dto)
	{
		if (dto is null)
		{
			return BadRequest();
		}

		using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
		using (Db.DisposableActionForDbConnection())
		{
			var factory = new BusinessObjectFactory();
			var result = factory.New<RateSearchResult>();
			result.StoreDto(dto);
			factory.Save();
			return Json(new CreateJobResponseDto { RequestId = result.PK.ToGuid() });
		}
	}

	[HttpPost]
	[Route("autorate")]
	[ResponseType(typeof(CreateJobResponseDto))]
	public IHttpActionResult SelectRate([FromBody] SelectRateDto dto)
	{
		if (dto is null)
		{
			return BadRequest();
		}

		using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
		using (Db.DisposableActionForDbConnection())
		{
			var factory = new BusinessObjectFactory();
			var result = factory.New<RateSearchResult>();
			result.StoreDto(dto);
			factory.Save();
			return Json(new CreateJobResponseDto { RequestId = result.PK.ToGuid() });
		}
	}
}
