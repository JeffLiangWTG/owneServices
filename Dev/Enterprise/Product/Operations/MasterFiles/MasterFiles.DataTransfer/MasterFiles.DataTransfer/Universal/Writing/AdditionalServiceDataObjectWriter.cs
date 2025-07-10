using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AdditionalServiceDataObjectWriter : DataObjectWriter<JobService, AdditionalService>
	{
		public AdditionalServiceDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override AdditionalService PopulateDataObject(JobService serviceBO)
		{
			var serviceData = new AdditionalService();

			serviceData.ServiceCode = ListHelper.GetWithDescription<CodeDescriptionPair>(serviceBO.ES_ServiceCode, serviceBO.Lookups.JobServiceType_List);
			serviceData.Booked = serviceBO.ES_Booked;
			serviceData.Completed = serviceBO.ES_Completed;
			serviceData.Duration = serviceBO.ES_Duration;
			serviceData.ServiceCount = serviceBO.ES_ServiceCount;
			serviceData.ServiceNote = serviceBO.ES_ServiceNote;
			serviceData.References = serviceBO.ES_References;
			serviceData.MeasurementBasis = serviceBO.ES_MeasurementBasis;
			serviceData.ServiceRateCurrency = serviceBO.ES_RX_NKServiceRateCurrency;
			serviceData.ServiceRate = serviceBO.ES_ServiceRate;
			serviceData.ServiceId = serviceBO.ES_ServiceId;
			serviceData.ExternalServiceId = serviceBO.ES_ExternalServiceId;
			serviceData.SubLocation = serviceBO.ES_SubLocation;

			if (serviceBO.Contractor != null)
			{
				serviceData.Contractor = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Contractor)).GetDataObject(serviceBO.Contractor.MainAddress);
			}

			serviceData.Location = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Location)).GetDataObject(serviceBO.Location);

			return serviceData;
		}
	}
}
