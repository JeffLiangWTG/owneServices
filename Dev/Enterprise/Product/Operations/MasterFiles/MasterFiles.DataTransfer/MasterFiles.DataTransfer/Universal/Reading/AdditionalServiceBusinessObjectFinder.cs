using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AdditionalServiceBusinessObjectFinder<T> : MatchingBusinessObjectFinder<AdditionalService, T> where T : JobService
	{
		public AdditionalServiceBusinessObjectFinder(AdditionalService dataObject)
			: base(dataObject)
		{
		}

		protected override T FindCore(IEnumerable<T> services) => services.GetService(dataObject);
	}
}
