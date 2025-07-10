using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class AdditionalServiceDataObjectWriter : DataObjectWriter<JobService, AdditionalService>
	{
		internal AdditionalServiceDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override AdditionalService PopulateDataObject(JobService serviceBO)
		{
			var serviceData = new AdditionalService();

			serviceData.ServiceCode = ListHelper.GetWithDescription<CodeDescriptionPair>(serviceBO.ES_ServiceCode, serviceBO.Lookups.JobServiceType_List);
			serviceData.Booked = serviceBO.ES_Booked;
			serviceData.Completed = serviceBO.ES_Completed;
			serviceData.ServiceId = serviceBO.ES_ServiceId;
			serviceData.ExternalServiceId = serviceBO.ES_ExternalServiceId;

			if (serviceBO.Contractor != null)
			{
				serviceData.Contractor = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Contractor)).GetDataObject(serviceBO.Contractor.MainAddress);
			}

			return serviceData;
		}
	}
}
