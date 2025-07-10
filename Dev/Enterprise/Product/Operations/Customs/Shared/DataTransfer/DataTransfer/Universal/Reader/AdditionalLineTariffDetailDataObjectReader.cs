using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AdditionalLineTariffDetailDataObjectReader : DataObjectReader<UniversalCustoms.AdditionalLineTariffDetail, CusLineTariffDetail>
	{
		public AdditionalLineTariffDetailDataObjectReader(UniversalCustoms.AdditionalLineTariffDetail dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, IAdditionalLineTariffDetailParent parent) : base(dataObject, logger, factory)
		{
			Parent = Argument.NotNull(parent, "parent");
			this.helper = helper;
		}
		protected readonly IAdditionalLineTariffDetailParent Parent;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected override CusLineTariffDetail GetNewBusinessObject()
		{
			return Parent.CusLineTariffDetails?.AddNew();
		}

		protected override CusLineTariffDetail GetExistingBusinessObject()
		{
			CusLineTariffDetail result = null;
			var sourceType = dataObject.Type?.Code ?? ZString.Empty;
			var sourceTariff = dataObject.Tariff ?? ZString.Empty;
			var targetCollection = Parent.CusLineTariffDetails?.OfType<CusLineTariffDetail>();
			if (targetCollection != null)
			{
				result = targetCollection.FirstOrDefault(x => x.BZ_Type == sourceType && x.BZ_Tariff == sourceTariff);
				result = result ?? targetCollection.FirstOrDefault(x => x.BZ_Type == sourceType && x.BZ_Tariff.IsEmpty);
			}
			return result;
		}

		protected override void PopulateBusinessObject(CusLineTariffDetail targetBO)
		{
			var indexer = GetColumnIndexer(targetBO);
			var addInfoManager = targetBO as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;

			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}

				SetValue(indexer, CusLineTariffDetailSchema.BZ_Type, dataObject.Type);
				SetValue(indexer, CusLineTariffDetailSchema.BZ_Tariff, dataObject.Tariff);
				SetValue(indexer, CusLineTariffDetailSchema.BZ_Value, dataObject.Value);
				SetValue(indexer, CusLineTariffDetailSchema.BZ_Qty1, dataObject.CustomsQuantity);
				SetValue(indexer, CusLineTariffDetailSchema.BZ_UQ1, dataObject.CustomsQuantityUnit);

				if (addInfoManager != null)
				{
					FillAddInfos(addInfoManager, indexer);
					if (IsDefaultingEnabled)
					{
						addInfoManager.UpdateRelatedPropertyInfo();
					}
				}
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(indexer.GetValue(CusLineTariffDetailSchema.BZ_NAddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
			}
		}

		protected void FillAddInfos(IAddInfoManager addInfoManager, IColumnIndexer indexer)
		{
			AddInfoDataObjectReader.New(addInfoManager as BusinessObject, logger, helper, CusLineTariffDetailSchema.BZ_NAddInfo).ReadIntoRow(addInfoManager, indexer, dataObject, null);
		}
	}
}
