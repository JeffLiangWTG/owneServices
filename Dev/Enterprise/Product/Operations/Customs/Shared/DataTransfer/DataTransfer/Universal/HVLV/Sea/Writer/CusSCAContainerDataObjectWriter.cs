using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	internal class CusSCAContainerDataObjectWriter : CusSCAContainerDataObjectWriter<BaseCusSCAContainer>
	{
		public CusSCAContainerDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{ }

		protected override ICodeDescriptionPairList GetContainerModes(BaseCusSCAContainer bizObj) { return null; }
	}

	public abstract class CusSCAContainerDataObjectWriter<TCusSCAContainer> : DataObjectWriter<TCusSCAContainer, UniversalXml.Container>
		where TCusSCAContainer : BaseCusSCAContainer
	{
		protected CusSCAContainerDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{ }

		protected override UniversalXml.Container PopulateDataObject(TCusSCAContainer sourceBO)
		{
			var data = new UniversalXml.Container(writeManager.WriterStrategy);
			PopulateDataObject(sourceBO, data, false);
			return data;
		}

		protected BusinessObjectFactory Factory
		{
			get { return writeManager.Action.FactoryForProcessing; }
		}

		void PopulateDataObject(TCusSCAContainer bizObj, UniversalXml.Container data, bool keepExistingData)
		{
			var bizObjRow = (IColumnIndexer)bizObj;
			data.ContainerNumber = PopulateValue(data.ContainerNumber, keepExistingData, () => bizObjRow.GetValue(CusSCAContainerSchema.CN_ContainerNumber));
			data.FCL_LCL_AIR = PopulateValue(data.FCL_LCL_AIR, keepExistingData,
				() => ListHelper.GetWithDescription<UniversalXml.ContainerMode>(bizObjRow.GetValue(CusSCAContainerSchema.CN_ContainerMode), GetContainerModes(bizObj)));
			data.ContainerType = PopulateValue(data.ContainerType, keepExistingData, () => GetContainerType(bizObj));
			data.Seal = PopulateValue(data.Seal, keepExistingData, () => bizObjRow.GetValue(CusSCAContainerSchema.CN_SealNumber));
			PopulateOrganizationAddresses(bizObj, data, keepExistingData);

			PopulateCountrySpecificData(bizObj, data, keepExistingData);
		}

		protected virtual UniversalXml.ContainerType GetContainerType(TCusSCAContainer bizObj)
		{
			return UniversalXml.ContainerType.New(bizObj.ContainerType);
		}

		protected virtual void PopulateCountrySpecificData(TCusSCAContainer bizObj, UniversalXml.Container data, bool keepExistingData) { }

		protected abstract ICodeDescriptionPairList GetContainerModes(TCusSCAContainer bizObj);

		void PopulateOrganizationAddresses(TCusSCAContainer bizObj, UniversalXml.Container data, bool keepExistingData)
		{
			var packLocationOrgAddress = Factory.Load<OrgAddress>(bizObj.CN_OA_PackLocation);
			if (packLocationOrgAddress != null)
			{
				data.SetOrganizationAddressCollection(() =>
				{
					var packLocationOrganizationAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ContainerPacking)).GetDataObject(packLocationOrgAddress);
					return data.OrganizationAddressCollection.MergeCollection(new[] { packLocationOrganizationAddress }, keepExistingData, UniversalDataObjectReaderHelper.IsOrganizationAddressTypeMatched);
				});
			}
		}
	}
}

// Tested in CusSCAOceanBillDataObjectWriter.cs
