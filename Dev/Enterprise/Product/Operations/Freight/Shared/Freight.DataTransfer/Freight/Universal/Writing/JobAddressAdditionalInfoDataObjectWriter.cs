using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class JobAddressAdditionalInfoDataObjectWriter : DataObjectWriter<JobAddressAdditionalInfo, AdditionalAddressInfo>
	{
		public JobAddressAdditionalInfoDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AdditionalAddressInfo PopulateDataObject(JobAddressAdditionalInfo bizo)
		{
			var dataObject = new AdditionalAddressInfo();

			dataObject.AddressType = bizo.DocAddressType.ToString();
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(
				bizo.JAI_TransportMode,
				new JobAddressAdditionalInfoTransportModeCodeDescriptionPairList()
			);

			return dataObject;
		}
	}
}
