using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class AdditionalService
	{
		public static IReadOnlyCollection<AdditionalService> Create(IContext context, JobServiceDependentCollection services)
		{
			if (context == null || services == null)
			{
				return System.Array.Empty<AdditionalService>();
			}

			return services
				.OfType<JobService>()
				.Select(service =>
				{
					return new AdditionalService()
					{
						ServiceCode = new CodeDescription(service.Lookups.JobServiceType_List)
						{
							Code = service.ES_ServiceCode
						},
						Booked = service.ES_Booked,
						Completed = service.ES_Completed,
						Duration = service.ES_Duration,
						ServiceCount = service.ES_ServiceCount,
						ServiceNote = service.ES_ServiceNote,
						References = service.ES_References,
						Contractor = AddressBuilder.Create(context, service.Contractor?.MainAddress),
						Location = AddressBuilder.Create(context, service.Location)
					};
				}).ToArray();
		}
	}
}
