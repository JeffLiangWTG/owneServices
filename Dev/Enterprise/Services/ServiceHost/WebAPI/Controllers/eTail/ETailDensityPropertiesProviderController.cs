using System;
#if NETFRAMEWORK
using System.Web.Http;
using Route = System.Web.Http.RouteAttribute;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
#if NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NET
	[Route("api/eTail")]
#elif NETFRAMEWORK
	[RoutePrefix("api/eTail")]
#endif
	public class ETailDensityPropertiesProviderController : ControllerBase
	{
		public ETailDensityPropertiesProviderController()
		{
		}

		[Route("chargeable/{entityTableCode}/{entityPK:Guid}")]
		[HttpGet]
		public string GetChargeable(string entityTableCode, Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = string.Empty;

				if (entityTableCode == HVLVConsignmentSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVConsignment>(entityTableCode, entityPK, out var consignment))
					{
						consignment.CalculateChargeable();
						result = consignment.ChargeableForDisplay;
					}
				}
				else if (entityTableCode == HVLVItemSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVItem>(entityTableCode, entityPK, out var item))
					{
						item.CalculateChargeableInformation();
						result = item.ChargeableForDisplay;
					}
				}

				return result;
			}
		}

		[Route("volumeWeight/{entityTableCode}/{entityPK:Guid}")]
		[HttpGet]
		public string GetVolumeWeight(string entityTableCode, Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = string.Empty;

				if (entityTableCode == HVLVConsignmentSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVConsignment>(entityTableCode, entityPK, out var consignment))
					{
						result = consignment.VolumeWeightForDisplay;
					}
				}
				else if (entityTableCode == HVLVItemSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVItem>(entityTableCode, entityPK, out var item))
					{
						result = item.VolumeWeightForDisplay;
					}
				}

				return result;
			}
		}

		[Route("densityFactor/{entityTableCode}/{entityPK:Guid}")]
		[HttpGet]
		public string GetDensityFactor(string entityTableCode, Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = string.Empty;

				if (entityTableCode == HVLVConsignmentSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVConsignment>(entityTableCode, entityPK, out var consignment))
					{
						result = consignment.DensityFactor.ToString("F2");
					}
				}
				else if (entityTableCode == HVLVItemSchema.Constants.Prefix)
				{
					if (TryGetBusinessObject<HVLVItem>(entityTableCode, entityPK, out var item))
					{
						result = item.DensityFactor.ToString("F2");
					}
				}

				return result;
			}
		}

		bool TryGetBusinessObject<T>(string entityTableCode, Guid entityPK, out T bizo) where T : BusinessObject
		{
			bizo = new BusinessObjectFactory().Load<T>(entityTableCode, entityPK);

			return bizo != null;
		}
	}
}
