using System;
using System.Threading;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/trackingMap")]
	[GlowTicketAuthentication]
	public sealed class TrackingMapController : ApiController
	{
		[Route("url")]
		[HttpGet]
		public IHttpActionResult GetUrl(string entityType, Guid pk, CancellationToken ct)
		{
			if (string.IsNullOrEmpty(entityType) || pk == Guid.Empty)
			{
				return BadRequest();
			}

			TrackingMapUrlResult result;

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				switch (entityType.ToUpperInvariant())
				{
					case "IJOBSHIPMENT":
						result = GetForwardingShipmentMapUrlResult(pk, ct);
						break;
					case "IJOBCONTAINER":
						result = GetContainerMapUrlResult(pk, ct);
						break;
					case "IJOBDECLARATION":
						result = GetDeclarationMapUrlResult(pk, ct);
						break;
					case "IJOBORDERHEADER":
						result = GetOrderMapUrlResult(pk, ct);
						break;
					default:
						result = new TrackingMapUrlResult
						{
							Errors = new[] { Res.GetString("C47E3390-3407-4598-8E68-B8FB17447C59", "{0} is not supported.", entityType) }
						};
						break;
				}

				return Json(result);
			}
		}

		TrackingMapUrlResult GetForwardingShipmentMapUrlResult(Guid shipmentPk, CancellationToken ct)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = GetNewBusinessObjectFactory();
				var shipment = factory.Load<ForwardingShipment>(shipmentPk);

				if (shipment == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { Res.GetString("2CE3E9B1-8382-4E16-86A0-C0C4E9BDA71C", "Shipment not found in database. If this is a new shipment, save your changes first.") }
					};
				}

				var helper = new TransportOrderHelper(shipment.TransportsIncludingRelated);
				var contact = GetContact(factory);

				return GetTransportLocationMapUrlResult(helper, contact, ct);
			}
		}

		TrackingMapUrlResult GetContainerMapUrlResult(Guid containerPk, CancellationToken ct)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = GetNewBusinessObjectFactory();
				var container = factory.Load<CommonContainer>(containerPk);

				if (container == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { Res.GetString("D657451F-7455-41ED-B20B-EFD60D9B65C1", "Container not found in database. If this is a new container, save your changes first.") }
					};
				}

				if (container.Consol == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { Res.GetString("5B3B56DC-AA42-4D68-8C73-09F4B9B427A3", "Consol not found for container.") }
					};
				}

				var helper = new TransportOrderHelper(container.Consol.Transports);
				var contact = GetContact(factory);

				return GetTransportLocationMapUrlResult(helper, contact, ct);
			}
		}

		TrackingMapUrlResult GetDeclarationMapUrlResult(Guid declarationPk, CancellationToken ct)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = GetNewBusinessObjectFactory();
				var declaration = factory.Load<BaseJobDeclaration>(declarationPk);

				if (declaration == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { Res.GetString("963F1C73-7382-41DE-86CA-A5D03E1027AA", "Declaration not found in database. If this is a new declaration, save your changes first.") }
					};
				}

				var helper = new TransportOrderHelper(declaration.TransportsIncludingRelated);
				var contact = GetContact(factory);

				return GetTransportLocationMapUrlResult(helper, contact, ct);
			}
		}

		static TrackingMapUrlResult GetTransportLocationMapUrlResult(TransportOrderHelper helper, OrgContact contact, CancellationToken ct)
		{
			var service = ObjectFactory.Get<ITrackingMapUrlService>();
			var (mapUrl, errorMessage) = service.GetActiveTransportMapUrl(helper, contact, ct);

			if (mapUrl == null)
			{
				return new TrackingMapUrlResult
				{
					Errors = new[] { errorMessage }
				};
			}

			return new TrackingMapUrlResult
			{
				Url = mapUrl.ToString()
			};
		}

		TrackingMapUrlResult GetOrderMapUrlResult(Guid orderPk, CancellationToken ct)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = GetNewBusinessObjectFactory();
				var order = factory.Load<Order>(orderPk);

				if (order == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { Res.GetString("70315007-2002-41CB-9D98-D0125D5F1B94", "Order not found in database. If this is a new order, save your changes first.") }
					};
				}

				var contact = GetContact(factory);
				var service = ObjectFactory.Get<ITrackingMapUrlService>();
				var (mapUrl, errorMessage) = service.GetOrderMapUrl(order, contact, ct);

				if (mapUrl == null)
				{
					return new TrackingMapUrlResult
					{
						Errors = new[] { errorMessage }
					};
				}

				return new TrackingMapUrlResult
				{
					Url = mapUrl.ToString()
				};
			}
		}

		OrgContact GetContact(BusinessObjectFactory factory)
		{
			var identity = User?.Identity as IGlowAuthenticationTicketIdentity;

			return identity?.GetContact(factory);
		}

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = "VesselMovementsController Factory" };
	}
}
