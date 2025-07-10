using System;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("Pave/Board")]
	[GlowTicketAuthentication]
	public class BoardController : BasePaveController<IBoardService>
	{
		public BoardController() : base(() => new BoardService())
		{
		}

		[Route("configuration/{boardPK}")]
		public IHttpActionResult GetConfiguration(Guid boardPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = Service.GetConfiguration(boardPK);

				return ToPaveResponseJson(result);
			}
		}
	}
}
