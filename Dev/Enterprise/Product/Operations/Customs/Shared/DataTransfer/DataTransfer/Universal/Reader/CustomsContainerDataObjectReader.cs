using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsContainerDataObjectReader<TDeclaration, TContainer> : BaseContainerDataObjectReader<TContainer>
		where TDeclaration : BaseJobDeclaration
		where TContainer : BaseCusContainer
	{
		public CustomsContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, TDeclaration declaration, ILandedCostDataReader landedCostDataReader = null)
			: base(containerDataObject, logger, helper.Factory)
		{
			this.declaration = Argument.NotNull(declaration, "BaseJobDeclaration declaration");
			this.helper = helper;
			this.landedCostDataReader = landedCostDataReader;
		}
		readonly ILandedCostDataReader landedCostDataReader;
		protected readonly TDeclaration declaration;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected sealed override TContainer GetExistingBusinessObject()
		{
			var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();
			return !containerNumber.IsEmpty ? (TContainer)declaration.CusContainers.Find(containerNumber) : null;
		}

		protected sealed override TContainer GetNewBusinessObject()
		{
			// use this to handle correct type
			return (TContainer)declaration.CusContainers.AddNew();
		}

		protected sealed override void PopulateBusinessObject(TContainer container)
		{
			var containerRow = GetColumnIndexer(container);
			var addInfoManager = container as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}
				var containerPK = containerRow.GetValue(CusContainerSchema.PK);
				var containerIsInDatabase = container.IsInDatabase;
				SetValue(containerRow, CusContainerSchema.CO_RC, dataObject.ContainerType);
				SetValue(containerRow, CusContainerSchema.CO_ContainerNumber, dataObject.ContainerNumber);

				SetValue(containerRow, CusContainerSchema.CO_Weight, dataObject.GoodsWeight);
				SetValue(containerRow, CusContainerSchema.CO_WeightUQ, dataObject.WeightUnit);

				var jobContainer = container.JobContainer;
				if (jobContainer != null && (dataObject.GoodsWeight.HasValue || dataObject.WeightUnit != null && dataObject.WeightUnit.Code.HasValue))
				{
					SetValue(jobContainer, JobContainerSchema.JC_GrossWeight, jobContainer.JC_GrossWeight);
				}

				SetValue(containerRow, CusContainerSchema.CO_Seal, dataObject.Seal);
				SetValue(containerRow, CusContainerSchema.CO_SecondSeal, dataObject.SecondSeal);
				if (container.UseContainerSize)
				{
					SetValue(containerRow, CusContainerSchema.CO_ContainerSize, dataObject.CustomsContainerSize);
				}
				SetValue(containerRow, CusContainerSchema.CO_FCL_LCL_AIR, GetCustomsContainerMode(container, dataObject));
				if (helper.IsSourceAndTargetCountrySame)
				{
					if (addInfoManager != null)
					{
						GetNewAddInfoDataObjectReader(container).ReadIntoRow(addInfoManager, containerRow, dataObject, null);
						if (IsDefaultingEnabled)
						{
							addInfoManager.UpdateRelatedPropertyInfo();
						}
					}
					var tablePrefix = CusContainerSchema.Constants.Prefix;
					new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(containerPK, tablePrefix, containerIsInDatabase, dataObject);
					new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(containerPK, tablePrefix, containerIsInDatabase, dataObject);
				}
				new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(CusContainerSchema.Instance, containerRow, dataObject, helper.GetCusContainerCustomLabelsProvider(containerRow));

				if (jobContainer == null)
				{
					jobContainer = factory.New<CommonContainer>();
					SetValue(containerRow, CusContainerSchema.CO_JC, jobContainer.PK);
				}
				new Freight.DataTransfer.Universal.ContainerWithVGMDataObjectReader<CommonContainer>(dataObject, logger, factory, containerBizObjProvider: (x) => jobContainer).ReadIntoBusinessObject();

				FillCustomizedFields(container, dataObject);
				PopulateCountrySpecificData(container, dataObject);
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(containerRow.GetValue(CusContainerSchema.CO_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
			}
			if (landedCostDataReader != null)
			{
				landedCostDataReader.CollectTransportLogisticsCost(container, dataObject);
			}
		}

		protected virtual void FillCustomizedFields(TContainer container, Container dataObject)
		{
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReader(TContainer container)
		{
			return AddInfoDataObjectReader.New(container, logger, helper, CusContainerSchema.CO_AddInfo);
		}

		protected ContainerMode GetCustomsContainerMode(TContainer container, Container dataObject)
		{
			var converter = new CustomsContainerModeConverter<Container>(dataObject, container.Lookups.CO_FCL_LCL_NCT_List, dataObject.FCL_LCL_AIR, null, _ => false);
			return converter.Convert();
		}

		protected virtual void PopulateCountrySpecificData(TContainer container, Container dataObject)
		{
		}
	}
}
