using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts
{
	[RoutePrefix("api/CCA/RouteSets")]
	[GlowTicketAuthentication]
	public class RouteSetsController : ApiController
	{
		[Route("getRouteSetsForConsols")]
		[HttpPost]
		public IHttpActionResult GetRouteSetsForConsols([FromBody] List<string> consolPKStrings)
		{
			if (consolPKStrings == null || consolPKStrings.Count == 0)
			{
				return BadRequest((NoResString)"Must specify at least one consolidation PK");
			}

			List<Guid> consolPKGuids;
			try
			{
				consolPKGuids = consolPKStrings.Select(pkString => new Guid(pkString)).ToList();
			} catch (Exception)
			{
				return BadRequest((NoResString)"One or more of the provided consolidation PKs are not valid guids");
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var query = new ZQuery(JobConsolSchema.PK, consolPKGuids);
				var consols = factory.Load<ForwardingConsol>(query);
				var allRouteSets = consols.ToDictionary(consol => consol.PK, GetRelatedLegPKLists);

				return Json(allRouteSets);
			}
		}

		[Route("getRouteSetNumberForConsolTransportLeg")]
		[HttpPost]
		public IHttpActionResult GetRouteSetNumberForConsolTransportLeg([FromBody] string transportPK)
		{
			if (transportPK.IsNullOrEmpty())
			{
				return BadRequest((NoResString)"Must specify transport PK.");
			}

			if (!Guid.TryParse(transportPK, out Guid transportPKGuid))
			{
				return BadRequest((NoResString)"The provided transport PK is not valid guid.");
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var query = new ZQuery(JobConsolTransportSchema.PK, transportPKGuid);
				query.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Consol);
				var transport = factory.LoadTop1<Transport>(query);
				if (transport == null)
				{
					return BadRequest((NoResString)"Consol Transport leg is not found.");
				}
				transport.ParentType = typeof(ForwardingConsol);
				return Content(HttpStatusCode.OK, new RouteSetNumberJsonResponse(transport.RouteSetNumber));
			}
		}

		IEnumerable<IEnumerable<ZGuid>> GetRelatedLegPKLists(ForwardingConsol consol)
		{
			var routeSets = ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets;
			return routeSets.Count == 0
				? consol.Transports.Select(transport => new[] { transport.PK })
				: routeSets.Select(set => set.Legs.Select(leg => leg.PK));
		}

		public class RouteSetNumberJsonResponse
		{
			public RouteSetNumberJsonResponse(int? routeSetNumber)
			{
				this.routeSetNumber = routeSetNumber;
			}
			public int? routeSetNumber { get; set; }
		}
	}
}
