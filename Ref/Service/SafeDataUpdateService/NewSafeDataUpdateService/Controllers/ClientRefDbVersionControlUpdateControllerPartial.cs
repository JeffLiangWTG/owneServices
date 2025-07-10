using Microsoft.AspNetCore.Cors;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[EnableCors(CorsPolicyConfig.StagingServiceCorsPolicy)]
	public partial class ClientRefDbVersionControlUpdateController
	{
	}
}
