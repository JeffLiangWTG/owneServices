using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	public partial class RefClientUpdateController
	{
		[InternalDataSetActionFilter]
		public override IQueryable<RefClient> Get()
		{
			return base.Get();
		}

		[InternalDataSetActionFilter]
		public override IQueryable<RefClient> Get(Guid key)
		{
			return base.Get(key);
		}
	}
}
