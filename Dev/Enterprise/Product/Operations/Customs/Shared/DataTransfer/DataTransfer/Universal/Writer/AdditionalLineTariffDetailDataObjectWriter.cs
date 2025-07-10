using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AdditionalLineTariffDetailDataObjectWriter : DataObjectWriter<CusLineTariffDetail, UniversalCustoms.AdditionalLineTariffDetail>
	{
		public AdditionalLineTariffDetailDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
			dataWritingManager = manager;
		}

		readonly IDataWritingManager dataWritingManager;

		protected override UniversalCustoms.AdditionalLineTariffDetail PopulateDataObject(CusLineTariffDetail sourceBO)
		{
			var result = new UniversalCustoms.AdditionalLineTariffDetail(dataWritingManager.WriterStrategy)
			{
				Type = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(sourceBO.BZ_Type, sourceBO.Lookups.TariffTypeList),
				Tariff = sourceBO.BZ_Tariff,
				Value = sourceBO.BZ_Value,
				CustomsQuantity = sourceBO.BZ_Qty1,
				CustomsQuantityUnit = SetCustomsQuantityUnit(),
			};
			result.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(sourceBO.BZ_NAddInfo));
			return result;

			CodeDescriptionPair SetCustomsQuantityUnit()
			{
				var collection = sourceBO.Lookups.QuantityUnitList;
				if (collection is IFindBoxListProvider findBoxCollection)
				{
					return ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.BZ_UQ1, findBoxCollection);
				}
				if (collection is ICodeDescriptionPairList codeDescriptionPairList)
				{
					return ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.BZ_UQ1, codeDescriptionPairList);
				}
				else
				{
					return new CodeDescriptionPair { Code = sourceBO.BZ_UQ1 };
				}
			}
		}
	}
}
