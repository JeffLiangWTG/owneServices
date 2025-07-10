using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class AddInfoGroupCollectionCreator
	{
		public static UniversalCustoms.AddInfoGroup Create(CusAddInfo cusAddInfo, UniversalDataObjectWriterHelper helper, ICodeDescriptionPairList typeList, IDataWritingManager writeManager, string dataContext = "")
		{
			var addInfo = new UniversalCustoms.AddInfoGroup(writeManager.WriterStrategy)
			{
				Type = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(cusAddInfo.B7_Type, typeList),
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(cusAddInfo.B7_AddInfoData),
				AddInfoGroupCollection = AddInfoGroupCollectionCreator.CreateCollection(helper, cusAddInfo, writeManager, dataContext),
				CustomsReferenceCollection = CustomsReferenceCollectionCreator.CreateCollection(helper, cusAddInfo, writeManager, dataContext)
			};
			helper.UpdateOrganizationAddressCollection(addInfo, cusAddInfo, writeManager);
			writeManager.NotifyExported(addInfo, cusAddInfo);
			return addInfo;
		}

		public static List<UniversalCustoms.AddInfoGroup> CreateCollection(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext = "")
		{
			var result = new List<UniversalCustoms.AddInfoGroup>();
			if (bizObj != null)
			{
				var cusAddInfoTypeSupporter = bizObj as ICusAddInfoTypeSupporter;
				if (cusAddInfoTypeSupporter != null)
				{
					ZGuid bizObjPK = bizObj.PK;
					ZString tablePrefix = bizObj.TablePrefix;
					if (bizObjPK.IsValid)
					{
						var validTypes = cusAddInfoTypeSupporter.GetCusAddInfoTypes();
						var supportedTypes = helper.GetSupportedCusAddInfoB7_TypesFor(tablePrefix, dataContext)?.Where(x => validTypes.ContainsKey(x)).ToArray();
						if (supportedTypes != null && supportedTypes.Length > 0)
						{
							var query = new ZQuery(CusAddInfoSchema.B7_ParentID, bizObjPK);
							query.AddToFilter(CusAddInfoSchema.B7_Type, supportedTypes);
							var addInfoGroups = helper.Load<CusAddInfo>(query);
							if (addInfoGroups.Length > 0)
							{
								var typeList = helper.GetCusAddInfoB7_TypeList(tablePrefix, dataContext);
								foreach (var cusAddInfo in addInfoGroups)
								{
									var data = Create(cusAddInfo, helper, typeList, writeManager);
									result.Add(data);
								}
							}
						}
					}
				}
				var additionalAddInfoGroupCollectionSupport = helper.GetAdditionalAddInfoGroupCollectionSupportFor(bizObj, writeManager);
				if (additionalAddInfoGroupCollectionSupport != null)
				{
					result.AddRange(additionalAddInfoGroupCollectionSupport.CreateCollection());
				}
			}
			return result.Count == 0 ? null : result;
		}
	}
}
