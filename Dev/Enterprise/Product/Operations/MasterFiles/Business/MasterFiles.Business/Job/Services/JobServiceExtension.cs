using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class JobServiceExtension
	{
		public static void UpdateExternalServiceId(this JobService service, JobService otherService, bool twoWay = true)
		{
			if (otherService == null)
			{
				return;
			}

			UpdateExternalServiceId(service, otherService.ES_ServiceId, otherService.ES_ExternalServiceId, twoWay);
		}

		public static void UpdateExternalServiceId(this JobService service, IXmlEventValueObjectContextValueList otherService, bool twoWay = true)
		{
			if (otherService == null)
			{
				return;
			}

			UpdateExternalServiceId(service, otherService.ServiceId.GetValueOrDefault(), otherService.ExternalServiceId, twoWay);
		}

		public static void UpdateExternalServiceId(this JobService service, AdditionalService otherService, bool twoWay = true)
		{
			if (otherService == null)
			{
				return;
			}

			UpdateExternalServiceId(service, otherService.ServiceId.GetValueOrDefault(), otherService.ExternalServiceId, twoWay);
		}

		static void UpdateExternalServiceId(JobService service, ZString otherServiceId, ZString? otherExternalServiceId, bool twoWay)
		{
			if (service == null)
			{
				return;
			}

			if (otherServiceId.IsEmpty)
			{
				service.ShouldPopulateServiceId = false;
			}
			else
			{
				JobService.PopulateServiceIdIfNeeded(service);
				if (service.ES_ServiceId == otherServiceId)
				{
					SetIfNotNull(service, JobServiceSchema.ES_ExternalServiceId, otherExternalServiceId);
				}
				else
				{
					SetIfNotNull(service, JobServiceSchema.ES_ExternalServiceId, otherServiceId);
					if (twoWay && service.ES_ServiceId != otherExternalServiceId.GetValueOrDefault())
					{
						var dataObjectServiceBO = service.Factory.LoadFromUniqueKey<JobService>(JobServiceSchema.ES_ServiceId, otherServiceId);
						if (dataObjectServiceBO != null)
						{
							SetIfNotNull(dataObjectServiceBO, JobServiceSchema.ES_ExternalServiceId, service.ES_ServiceId);
						}
					}
				}
			}
		}

		public static void SetIfNotNull(this JobService service, SchemaColumn column, IZType value)
		{
			if (service != null && value != null)
			{
				service[column] = value;
			}
		}
	}
}
