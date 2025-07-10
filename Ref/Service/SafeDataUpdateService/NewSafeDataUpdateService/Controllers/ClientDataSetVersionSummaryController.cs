using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	[EnableCors(CorsPolicyConfig.StagingServiceCorsPolicy)]
	public class ClientDataSetVersionSummaryController : ControllerBase
	{
		public ClientDataSetVersionSummaryController(IReferenceDataRepository repo, IClientLicenseInformationProvider licenseProvider)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(licenseProvider, nameof(licenseProvider));

			this.repo = repo;
			this.licenseProvider = licenseProvider;
		}

		readonly IReferenceDataRepository repo;
		readonly IClientLicenseInformationProvider licenseProvider;

		[HttpGet]
		public Dictionary<string, DateTime> GetDataSetsTimestamp()
		{
			return repo.Get<RefDataSetInformation>().Where(x => x.RDS_LastUpdatedUTC.HasValue)
				.ToDictionary(x => x.RDS_DataSetName, x => DateTime.SpecifyKind(x.RDS_LastUpdatedUTC.Value, DateTimeKind.Utc));
		}

		[HttpGet]
		public IQueryable<ClientDataSetVersionSummary> Get(DateTime? lastUpdatedUTCFrom)
		{
			var lastUpdatedUTCs = GetDataSetsTimestamp();

			var result = (from g in repo.Get<ClientRefDbVersionControl>()
							.Where(x => x.CVC_IsInUse == true && (!lastUpdatedUTCFrom.HasValue || x.CVC_LastUpdatedTimeUTC.Value > lastUpdatedUTCFrom.Value))
							.GroupBy(x => x.CVC_DataSet).AsEnumerable()
						  let lastDataSetChangedTime = lastUpdatedUTCs.ContainsKey(g.Key) ? lastUpdatedUTCs[g.Key] : DateTime.MinValue
						  let noOfCustomersUpdated = g.Count(x => x.CVC_DataSetTimestamp == lastDataSetChangedTime)
						  let noOfCustomersFailed = g.Count(x => x.CVC_DataSetTimestamp != lastDataSetChangedTime)
						  let noOfProductionCustomersFailed = g.Count(x => x.CVC_DataSetTimestamp != lastDataSetChangedTime && x.CVC_SystemType == "PRD")
						  select new ClientDataSetVersionSummary
						  {
							  DataSet = g.Key,
							  LastDataChangedTime = lastDataSetChangedTime,
							  NoOfCustomersUpdated = noOfCustomersUpdated,
							  NoOfCustomersFailed = noOfCustomersFailed,
							  NoOfProductionCustomersFailed = noOfProductionCustomersFailed
						  });

			return result.AsQueryable();
		}

		[HttpGet]
		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public async Task<int> RemoveInactiveClients()
		{
			var clientIds = await licenseProvider.GetLicenceInformationInactive();
			foreach (var clientVersion in repo.Get<ClientRefDbVersionControl>().Where(x => clientIds.Contains(x.CVC_ClientId)))
			{
				repo.Delete(clientVersion);
			}
			return await repo.SaveChangesAsync(null);
		}
	}
}
