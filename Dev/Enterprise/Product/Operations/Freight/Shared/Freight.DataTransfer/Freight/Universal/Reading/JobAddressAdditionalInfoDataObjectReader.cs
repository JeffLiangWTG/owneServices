using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Freight.Universal
{
	public class JobAddressAdditionalInfoDataObjectReader : DataObjectReader<AdditionalAddressInfo, JobAddressAdditionalInfo>
	{
		readonly IJobAddressAdditionalInfoSupport parent;
		readonly ZString addressTypeCode;
		readonly bool isEmptyAddressType;
		readonly bool isInvalidAddressType;

		public JobAddressAdditionalInfoDataObjectReader(AdditionalAddressInfo dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IJobAddressAdditionalInfoSupport parent)
			: base(dataObject, logger, factory)
		{
			this.parent = parent;
			addressTypeCode = ZString.Empty;
			isEmptyAddressType = false;
			isInvalidAddressType = false;

			if (!string.IsNullOrEmpty(dataObject.AddressType))
			{
				if (Enum.TryParse(dataObject.AddressType, out DocAddressType parsedDocAddressType))
				{
					addressTypeCode = DocAddressTypes.GetCode(factory.BOFactory, parsedDocAddressType);
				}
				else
				{
					isInvalidAddressType = true;
				}
			}
			else
			{
				isEmptyAddressType = true;
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobAddressAdditionalInfo targetBO)
		{
			var infoString = "AddressAdditionalInfo";
			if (isEmptyAddressType)
			{
				return Res.GetString("4fc4ea96-857c-4fda-8163-08365066ad38", "Empty or Null {0} provided in {1}.", nameof(dataObject.AddressType), infoString);
			}
			if (isInvalidAddressType)
			{
				return Res.GetString("455c39ec-de0c-4d4a-908e-58f62f46d3db", "Invalid {0} '{1}' provided in {2}.", nameof(dataObject.AddressType), dataObject.AddressType, infoString);
			}
			return ZString.Empty;
		}

		protected override JobAddressAdditionalInfo GetExistingBusinessObject()
		{
			if (parent != null && !addressTypeCode.IsEmpty)
			{
				return parent.JobAddressAdditionalInfoCollection.Get(addressTypeCode) as JobAddressAdditionalInfo;
			}
			return null;
		}

		protected override void PopulateBusinessObject(JobAddressAdditionalInfo targetBO)
		{
			if (targetBO != null && dataObject != null)
			{
				if (!addressTypeCode.IsEmpty)
				{
					targetBO.JAI_AddressType = addressTypeCode;
					parent.SetTransportMode(addressTypeCode, dataObject.TransportMode?.Code.GetValueOrDefault());
				}
			}
		}

		protected override JobAddressAdditionalInfo GetNewBusinessObject()
		{
			if (parent != null && !addressTypeCode.IsEmpty)
			{
				var newInfo = parent.JobAddressAdditionalInfoCollection.GetOrCreate(addressTypeCode) as JobAddressAdditionalInfo;
				return newInfo;
			}
			return null;
		}
	}
}
