using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageCustomLabelsProvider : ICustomLabelsProvider
	{
		public CusPackageCustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
		{
			fConfigOrgProvider = configOrgProvider;
		}

		protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;

		ICustomLabelsConfigOrgProvider ICustomLabelsProvider.ConfigOrgProvider => fConfigOrgProvider;

		CustomLabelInfoList customFieldsCached;

		CustomLabelInfoList ICustomLabelsProvider.GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			if (configOrg != null)
			{
				var dictionary = configOrg.Factory.GetCachedValue("CusPackage_CustomLabelsProvider", () => new Dictionary<ZGuid, CustomLabelInfoList>());
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
			var customFields = new CustomLabelInfoList(typeof(CusPackage), configOrg, ResString.GetMultilingualString("27b9400b-80c0-4c2f-852e-80c573277cdb", "Supplier on the declaration"), factory)
				{
					{ Constants.CustomLabels.CusPackage.CustomAttribute1, CusPackage.Schema.CustomAttribute1, Constants.CustomLabels.Descriptions.CustomAttribute(1) },
					{ Constants.CustomLabels.CusPackage.CustomAttribute2, CusPackage.Schema.CustomAttribute2, Constants.CustomLabels.Descriptions.CustomAttribute(2) },
					{ Constants.CustomLabels.CusPackage.CustomFlag1, CusPackage.Schema.CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1) },
					{ Constants.CustomLabels.CusPackage.CustomFlag2, CusPackage.Schema.CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2) },
					{ Constants.CustomLabels.CusPackage.CustomDate1, CusPackage.Schema.CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1) },
					{ Constants.CustomLabels.CusPackage.CustomDate2, CusPackage.Schema.CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2) },
					{ Constants.CustomLabels.CusPackage.CustomDecimal1, CusPackage.Schema.CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1) },
					{ Constants.CustomLabels.CusPackage.CustomDecimal2, CusPackage.Schema.CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2) }
				};
			return customFields;
		}
	}
}
