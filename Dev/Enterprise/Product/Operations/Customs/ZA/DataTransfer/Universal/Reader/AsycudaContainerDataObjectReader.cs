using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaContainerDataObjectReader : DataObjectReader<Container, AsycudaContainer>
	{
		public AsycudaContainerDataObjectReader(Container containerData, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader headerBO, UniversalDataObjectReaderHelper helper)
			: base(containerData, logger, factory)
		{
			this.containerData = Argument.NotNull(containerData, "Container DataObject");
			Argument.NotNull(logger, "Logger");
			Argument.NotNull(factory, "Factory");
			this.headerBO = Argument.NotNull(headerBO, "AsycudeManifestHeader");
			this.helper = Argument.NotNull(helper, "Helper");
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AsycudaContainer containerBO)
		{
			var builder = new ZStringBuilder();
			if (!containerData.ContainerNumber.HasValue || containerData.ContainerNumber.Value.IsEmpty)
			{
				builder.Append(Res.GetString("442726B5-99D2-4551-B17D-5297E4632D09", "{0} must not be empty.", "ContainerNumber"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override AsycudaContainer GetExistingBusinessObject()
		{
			AsycudaContainer result = null;
			if (containerData.ContainerNumber.HasValue)
			{
				var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, headerBO.PK);
				query.AddToFilter(AsycudaContainerSchema.ACN_ContainerNumber, containerData.ContainerNumber.Value);
				query.FetchOnlyFromLocalCache = !headerBO.IsInDatabase;
				query.OrderBy = AsycudaContainer.Schema.PK;
				result = factory.LoadTop1<AsycudaContainer>(query);
			}
			return result;
		}

		protected override AsycudaContainer GetNewBusinessObject()
		{
			return headerBO.Containers.AddNew();
		}

		protected override void PopulateBusinessObject(AsycudaContainer containerBO)
		{
			var containerRow = GetColumnIndexer(containerBO);
			SetValue(containerRow, AsycudaContainerSchema.ACN_ContainerNumber, containerData.ContainerNumber);
			FillContainerType(containerRow);
			SetValue(containerRow, AsycudaContainerSchema.ACN_Seal1, containerData.Seal);
			SetValue(containerRow, AsycudaContainerSchema.ACN_EmptyFullIndicator, containerData.ContainerStatus?.Code);
			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(containerData.AddInfoCollection, helper.GetAsycudaContainerGenAddOnColumnList(containerBO), containerBO);
			SetValue(containerRow, AsycudaContainerSchema.ACN_SealingPartyType, containerData.SealPartyType?.Code);
		}

		void FillContainerType(IColumnIndexer containerRow)
		{
			if (containerData.ContainerType != null)
			{
				var containerTypeCode = containerData.ContainerType?.Code;
				RefContainer containerType = null;
				if (containerTypeCode.HasValue)
				{
					var queryContainerTypeCode = new ZQuery();
					queryContainerTypeCode.AddToFilter(RefContainerSchema.RC_Code, containerTypeCode);
					queryContainerTypeCode.OrderBy = RefContainer.Schema.RC_Code;
					containerType = factory.LoadTop1<RefContainer>(queryContainerTypeCode);
				}
				if (containerType != null)
				{
					SetValue(containerRow, AsycudaContainerSchema.ACN_RC_ContainerType, containerType.PK);
				}
			}
		}

		readonly Container containerData;
		readonly AsycudaManifestHeader headerBO;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
