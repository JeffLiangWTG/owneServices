using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAContainerDataObjectReader<TCusSCAContainer> : DataObjectReader<UniversalContainer, TCusSCAContainer>
		where TCusSCAContainer : BaseCusSCAContainer
	{
		public CusSCAContainerDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, UniversalContainer data, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(data, logger, factory)
		{
			this.oceanBill = Argument.NotNull(oceanBill, "oceanBill");
			this.hvlvConsolidatorShipmentWrapper = hvlvConsolidatorShipmentWrapper;
		}
		readonly IColumnIndexer oceanBill;
		protected readonly HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper;

		protected override TCusSCAContainer GetExistingBusinessObject()
		{
			TCusSCAContainer result = null;
			var containerNo = dataObject.ContainerNumber.GetValueOrDefault().ToUpper();
			if (!containerNo.IsEmpty)
			{
				result = CusSCADataObjectHelper.LoadContainers<TCusSCAContainer>(oceanBill, factory.BOFactory)
					.FirstOrDefault(container => container.CN_ContainerNumber == containerNo);
			}
			return result;
		}

		protected sealed override void PopulateBusinessObject(TCusSCAContainer targetBO)
		{
			using (targetBO.SuspendMarkingAsNeedingValidation())
			{
				var valueSetters = GetValueSetters(targetBO);
				foreach (var delaySetter in valueSetters)
				{
					delaySetter.SetValue();
				}
			}
		}

		protected IEnumerable<ValueSetter> GetValueSetters(TCusSCAContainer targetBO)
		{
			var result = new Dictionary<string, ValueSetter>();
			var containerBO = GetColumnIndexer(targetBO);
			SetValue(containerBO, CusSCAContainerSchema.CN_CB, oceanBill.GetValue(CusSCAOceanBillSchema.PK), result);
			SetValue(containerBO, CusSCAContainerSchema.CN_ContainerNumber, dataObject.ContainerNumber, result);
			SetValue(containerBO, CusSCAContainerSchema.CN_ContainerMode, hvlvConsolidatorShipmentWrapper != null ? hvlvConsolidatorShipmentWrapper.ContainerMode.GetCodeAsUpperCase() : dataObject.FCL_LCL_AIR.GetCodeAsUpperCase(), result);
			SetValue(containerBO, CusSCAContainerSchema.CN_SealNumber, dataObject.Seal, result);

			if (dataObject.ContainerType != null)
			{
				var refContainerCode = dataObject.ContainerType.Code.GetValueOrDefault();
				var isValidRefContainerCode = !refContainerCode.IsEmpty && new RefContainer.Loader(factory.BOFactory).LoadFromCode(refContainerCode) != null;
				SetValue(containerBO, CusSCAContainerSchema.CN_RC_NKContainerType, isValidRefContainerCode ? refContainerCode : ZString.Empty, result);
			}

			PopulateOrganizationAddresses(targetBO, result);

			PopulateCountrySpecificData(targetBO, result);

			return result.Values;
		}

		void PopulateOrganizationAddresses(TCusSCAContainer targetBO, Dictionary<string, ValueSetter> valueSetter)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var packLocationDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerPacking));
				if (packLocationDataObject != null)
				{
					var packLocationAddress = new OrganisationDataObjectReader(packLocationDataObject, logger, factory).GetMatched();
					if (packLocationAddress != null)
					{
						SetValue(targetBO, CusSCAContainerSchema.CN_OA_PackLocation, packLocationAddress.PK, valueSetter);
					}
				}
			}
		}

		protected virtual void PopulateCountrySpecificData(TCusSCAContainer targetBO, Dictionary<string, ValueSetter> valueSetter) { }
	}
}

// Tested in CusSCAOceanBillDataObjectReader.cs
