using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusTariffNationalCode
	{
		public RefCusTariffNationalCode()
		{
			RefCusRateApplicabilities = new HashSet<RefCusRateApplicability>();
			RefCusRateWithoutApplicabilities = new HashSet<RefCusRateWithoutApplicability>();
		}

		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; }

		[IgnoreClientProperty]
		public ICollection<RefCusRateWithoutApplicability> RefCusRateWithoutApplicabilities { get; set; }

		public void BuildNonPersistentObjects(ISafeRepository repo)
		{
			RefCusRateApplicabilities = NonPersistentObjectHelper.Build(RefCusRates.Select(x => (x, x.RefCusApplicabilities.ToArray())), repo).Cast<RefCusRateApplicability>().ToHashSet();
			RefCusRateWithoutApplicabilities = RefCusRates.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusRateWithoutApplicability(x, repo)).ToHashSet();
		}
	}
}
