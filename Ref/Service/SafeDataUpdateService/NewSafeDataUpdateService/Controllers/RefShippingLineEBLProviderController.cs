using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class RefShippingLineEBLProviderController
	{
		public RefShippingLineEBLProviderController(IReferenceDataRepository repo)
		{
			Argument.NotNull(repo, nameof(repo));
			this.repo = repo;
		}

		readonly IReferenceDataRepository repo;

		[HttpGet]
		public IQueryable<string> GetDistinctNames()
		{
			return repo.Get<RefShippingLineEBLProvider>().Select(x => x.RSE_Name).Distinct();
		}
	}
}
