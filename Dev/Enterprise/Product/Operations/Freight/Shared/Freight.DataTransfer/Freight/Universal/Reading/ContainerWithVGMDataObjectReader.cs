using System;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerWithVGMDataObjectReader<T> : ContainerDataObjectReader<T> where T : CommonContainer
	{
		public ContainerWithVGMDataObjectReader(Container containerDataObject,
			IXmlImportLogger logger,
			UniversalObjectFactory factory,
			Func<Container, T> containerBizObjProvider = null,
			Func<Container, T> containerBizObjCreator = null)
			: base(containerDataObject, logger, factory, containerBizObjProvider, containerBizObjCreator)
		{
		}

		protected override void ImportWeightRelatedInfo(T container)
		{
			var validationError = dataObject.ValidateVGMProperties();
			bool shouldImportVerifiedWeightRelatedValues = !container.IsGrossWeightVerified || dataObject.GrossWeightVerificationType != null;

			if (shouldImportVerifiedWeightRelatedValues && validationError.IsEmpty)
			{
				SetValue(container, JobContainerSchema.JC_GrossWeightVerificationType, dataObject.GrossWeightVerificationType);
				SetValue(container, JobContainerSchema.JC_GrossWeightVerificationDateTime, dataObject.GrossWeightVerificationDateTime);

				if (dataObject.OrganizationAddressCollection != null)
				{
					var grossWeightVerifiedByAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GrossWeightVerifiedBy));
					if (grossWeightVerifiedByAddress != null)
					{
						var grossWeightVerifiedByDocAddress = new OrganisationDataObjectReader(grossWeightVerifiedByAddress, logger, factory).GetMatchedOrNew(container);

						if (grossWeightVerifiedByDocAddress != null)
						{
							container.DocAddresses.Add(grossWeightVerifiedByDocAddress);
						}
					}
				}
			}
			else if (!shouldImportVerifiedWeightRelatedValues)
			{
				logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("5F328BC0-CC50-4184-ABB2-9F0B6FFB774F", "Gross Weight Verification Type not found in XML, Gross Weight, Unit, Verified By, Verified Date will not be imported."));
			}
			else
			{
				logger.Log(Enterprise.Integration.LogType.Warning, validationError);
			}

			if (container.IsGrossWeightVerified && dataObject.GrossWeightVerificationType != null && validationError.IsEmpty)
			{
				base.ImportWeightRelatedInfo(container);
			}
			else
			{
				SetValue(container, JobContainerSchema.JC_DunnageWeight, dataObject.DunnageWeight);
				SetValue(container, JobContainerSchema.JC_TareWeight, dataObject.TareWeight);
			}

			SetValue(container, JobContainerSchema.JC_GrossWeight, container.JC_GrossWeight); //hack: trigger SetValue to fix the value and provide logging message
		}
	}
}
