using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	public partial class RefCusTariffNationalCode
	{
		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; } = new HashSet<RefCusRateApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusRateWithoutApplicability> RefCusRateWithoutApplicabilities { get; set; } = new HashSet<RefCusRateWithoutApplicability>();

		public void BuildNonPersistentObjects()
		{
			RefCusRateApplicabilities = RefCusRates?.SelectMany(x => x.RefCusApplicabilities.Select(y => new RefCusRateApplicability(x, y))).ToHashSet()
				?? new HashSet<RefCusRateApplicability>();
			RefCusRateWithoutApplicabilities = RefCusRates?.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusRateWithoutApplicability(x)).ToHashSet()
				?? new HashSet<RefCusRateWithoutApplicability>();
		}
	}
}
