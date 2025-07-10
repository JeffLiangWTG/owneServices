using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackingListCustomLabelsProvider : ICustomLabelsProvider
	{
		public CusPackingListCustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
		{
			fConfigOrgProvider = configOrgProvider;
		}

		ICustomLabelsConfigOrgProvider ICustomLabelsProvider.ConfigOrgProvider => fConfigOrgProvider;

		protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;

		CustomLabelInfoList customFieldsCached;

		CustomLabelInfoList ICustomLabelsProvider.GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			if (configOrg != null)
			{
				var dictionary = configOrg.Factory.GetCachedValue("CusPackingList_CustomLabelsProvider", () => new Dictionary<ZGuid, CustomLabelInfoList>());
				if (!dictionary.TryGetValue(configOrg.PK, out var result))
				{
					result = GetCustomFieldsCore(configOrg, factory);
					dictionary.Add(configOrg.PK, result);
				}
				return result;
			}
			return customFieldsCached ?? (customFieldsCached = GetCustomFieldsCore(configOrg, factory));
		}

		CustomLabelInfoList GetCustomFieldsCore(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			var customFields = new CustomLabelInfoList(typeof(CusPackingList), configOrg, ResString.GetMultilingualString("58f78d0d-5488-4dab-b4fd-605085511f6e", "Supplier on the declaration"), factory)
				{
					{ Constants.CustomLabels.CustomsPackingList.CustomAttribute1, CusPackingList.Schema.CUL_CustomAttribute1, Constants.CustomLabels.Descriptions.CustomAttribute(1), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomAttribute2, CusPackingList.Schema.CUL_CustomAttribute2, Constants.CustomLabels.Descriptions.CustomAttribute(2), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomFlag1, CusPackingList.Schema.CUL_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomFlag2, CusPackingList.Schema.CUL_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomDate1, CusPackingList.Schema.CUL_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomDate2, CusPackingList.Schema.CUL_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomDecimal1, CusPackingList.Schema.CUL_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1), CustomLabelStyles.ShowByDefault },
					{ Constants.CustomLabels.CustomsPackingList.CustomDecimal2, CusPackingList.Schema.CUL_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2), CustomLabelStyles.ShowByDefault }
				};
			return customFields;
		}
	}
}
