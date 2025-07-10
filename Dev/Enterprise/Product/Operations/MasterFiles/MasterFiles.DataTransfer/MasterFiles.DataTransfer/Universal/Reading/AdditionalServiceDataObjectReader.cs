using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AdditionalServiceDataObjectReader : AdditionalServiceDataObjectReader<JobService>
	{
		public AdditionalServiceDataObjectReader(AdditionalService additionalServiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IHaveServices parent)
			: base(additionalServiceDataObject, logger, factory, parent)
		{
		}
	}

	public class AdditionalServiceDataObjectReader<T> : DataObjectReader<AdditionalService, T> where T : JobService
	{
		public AdditionalServiceDataObjectReader(AdditionalService additionalServiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IHaveServices parent)
			: base(additionalServiceDataObject, logger, factory)
		{
			this.parent = parent;
		}

		readonly IHaveServices parent;

		protected override T GetExistingBusinessObject()
		{
			return new AdditionalServiceBusinessObjectFinder<T>(dataObject).Find(parent?.Services?.Cast<T>());
		}

		protected override void PopulateBusinessObject(T serviceBO)
		{
			SetValue(serviceBO, JobServiceSchema.ES_Duration, dataObject.Duration);
			SetValue(serviceBO, JobServiceSchema.ES_OA_Location, null, dataObject.Location);
			SetValue(serviceBO, JobServiceSchema.ES_ServiceCode, dataObject.ServiceCode);
			SetValue(serviceBO, null, JobServiceSchema.ES_OH_Contractor, dataObject.Contractor);
			SetValue(serviceBO, JobServiceSchema.ES_References, dataObject.References);
			SetValue(serviceBO, JobServiceSchema.ES_ServiceCount, dataObject.ServiceCount);
			SetValue(serviceBO, JobServiceSchema.ES_ServiceNote, dataObject.ServiceNote);
			SetValue(serviceBO, JobServiceSchema.ES_Booked, dataObject.Booked);
			SetValue(serviceBO, JobServiceSchema.ES_Completed, dataObject.Completed);
			SetValue(serviceBO, JobServiceSchema.ES_MeasurementBasis, dataObject.MeasurementBasis);
			SetValue(serviceBO, JobServiceSchema.ES_RX_NKServiceRateCurrency, dataObject.ServiceRateCurrency);
			SetValue(serviceBO, JobServiceSchema.ES_ServiceRate, dataObject.ServiceRate);
			SetValue(serviceBO, JobServiceSchema.ES_SubLocation, dataObject.SubLocation);

			serviceBO.UpdateExternalServiceId(dataObject);
		}

		void SetValue(BusinessObject serviceBO, SchemaGuidColumn orgAddress, SchemaGuidColumn orgHeader, OrganizationAddress addressDataObject)
		{
			if (addressDataObject != null)
			{
				var addressBO = new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched();
				if (addressBO != null)
				{
					if (orgAddress != null)
					{
						serviceBO[orgAddress] = addressBO.PK;
					}

					if (orgHeader != null)
					{
						serviceBO[orgHeader] = addressBO.OA_OH;
					}
				}
			}
		}
	}
}
