using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.Business
{
	public static class JobServiceCollectionExtension
	{
		public static T GetService<T>(this IEnumerable<T> services, T service) where T : JobService
		{
			if (service == null)
			{
				return null;
			}

			JobService.PopulateServiceIdIfNeeded(service);
			return GetService(services, service.ES_ServiceId, service.ES_ServiceCode);
		}

		public static T GetService<T>(this IEnumerable<T> services, AdditionalService service) where T : JobService
		{
			if (service == null)
			{
				return null;
			}

			return GetService(services, service.ServiceId.GetValueOrDefault(), (service.ServiceCode?.Code).GetValueOrDefault().ToUpper());
		}

		static T GetService<T>(IEnumerable<T> services, ZString serviceId, ZString serviceCode) where T : JobService
		{
			if (services != null)
			{
				if (serviceId.IsEmpty)
				{
					services = services.Where(s => !s.HaveServiceId);
					if (!serviceCode.IsEmpty)
					{
						return services.FirstOrDefault(s => s.ES_ServiceCode == serviceCode && s.ServiceTypeNeedsToBeUnique);
					}
				}
				else
				{
					services = services.Where(s => s.HaveServiceId);
					return services.FirstOrDefault(s => s.ES_ServiceId == serviceId)
						?? services.FirstOrDefault(s => s.ES_ExternalServiceId == serviceId);
				}
			}

			return null;
		}
	}
}
