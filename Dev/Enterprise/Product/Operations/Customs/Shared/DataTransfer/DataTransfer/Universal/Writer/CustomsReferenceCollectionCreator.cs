using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class CustomsReferenceCollectionCreator
	{
		public static List<UniversalCustoms.CustomsReference> CreateCollection(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext = "")
		{
			var result = new List<UniversalCustoms.CustomsReference>();
			if (bizObj != null)
			{
				ZGuid bizObjPK = bizObj.PK;
				if (bizObjPK.IsValid && bizObj is ICusCodeDataTypeSupporter cusCodeDataTypeSupporter)
				{
					ZString tablePrefix = bizObj.TablePrefix;
					var validTypes = cusCodeDataTypeSupporter.GetCusCodeDataTypes();
					var supportedTypes = helper.GetSupportedCusCodeDataCY_TypesFor(tablePrefix, dataContext)?.Where(x => validTypes.ContainsKey(x)).ToArray();
					if (supportedTypes != null && supportedTypes.Length > 0)
					{
						var query = new ZQuery(CusCodeDataSchema.CY_ParentID, bizObjPK);
						query.AddToFilter(CusCodeDataSchema.CY_Type, supportedTypes);
						var customsReferenceCollection = helper.Load<CusCodeData>(query);
						if (customsReferenceCollection.Length > 0)
						{
							ICodeDescriptionPairList typeList = helper.GetCusCodeDataCY_TypeList(tablePrefix, dataContext);
							ICodeDescriptionPairList codeList = helper.GetCusCodeDataCY_CodeList(tablePrefix, dataContext);
							foreach (CusCodeData cusCodeData in customsReferenceCollection)
							{
								var data = new UniversalCustoms.CustomsReference();
								data.Type = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(cusCodeData.CY_Type, typeList);
								data.SubType = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair35Char>(cusCodeData.CY_Code, codeList);
								data.Reference = cusCodeData.CY_Data;
								data.IsOverridden = cusCodeData.CY_IsOverridden;
								data.Order = cusCodeData.CY_Order;
								if (data.DateCollection == null)
								{
									data.DateCollection = new List<UniversalShipment.Date>();
								}
								data.DateCollection.Add(new UniversalShipment.Date
								{
									Type = UniversalShipment.DateType.DateAtOffice,
									Value = cusCodeData.CY_Date
								});
								var referencedEntityDescription = helper.GetReferencedEntityDescriptionForCusCodeData(cusCodeData);
								if (referencedEntityDescription.HasValue)
								{
									data.ReferencedEntityDescription = referencedEntityDescription.Value;
								}

								result.Add(data);
							}
						}
					}
				}
			}

			var additional = helper.GetAdditionalCustomsReferenceDataFor(bizObj, writeManager, dataContext);
			if (additional != null)
			{
				result.AddRange(additional);
			}

			return result.Count == 0 ? null : result;
		}
	}
}
