using System.Web.Http;

namespace Enterprise.Rating.Web.Controllers
{
	/// <summary>
	/// WTGStatusCheckController
	/// </summary>
	[RoutePrefix("wtg")]
	public class WTGStatusCheckController : ApiController
	{
		[HttpGet]
		[Route("status")]
		public IHttpActionResult DBCheck()
		{
			using (var cx = CargoWise.Data.Db.NewExtraConnectionToMainDb())
			{
				cx.EnsureIsOpen();

				return Ok();
			}
		}
	}
}
