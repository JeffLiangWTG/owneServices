using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class CustomsSupportingInformationCollectionCreator
	{
		public static UniversalCustoms.CustomsSupportingInformation Create(CusSupportingInfo cusSupportingInfo, ICodeDescriptionPairList typeList, IDataWritingManager writeManager)
		{
			var lookups = cusSupportingInfo.Lookups;
			var supportingInfo = new UniversalCustoms.CustomsSupportingInformation()
			{
				Category = ListHelper.GetWithDescription<CodeDescriptionPair>(cusSupportingInfo.CSI_Type, typeList),
				Country = Country.NewOrEmpty(cusSupportingInfo.Country),
				CustomsOffice = ListHelper.GetWithDescription<CodeDescriptionPair10Char>(cusSupportingInfo.CSI_CustomsOffice, lookups.CustomsOfficeList),
				DateOfIssue = cusSupportingInfo.CSI_DateOfIssue.Date,
				Description = cusSupportingInfo.CSI_Description,
				LineNo = cusSupportingInfo.CSI_LineNo,
				Procedure = ListHelper.GetWithDescription<CodeDescriptionPair7Char>(cusSupportingInfo.CSI_Procedure, lookups.ProcedureList),
				Quantity = cusSupportingInfo.CSI_Quantity,
				Quantity2 = cusSupportingInfo.CSI_Quantity2,
				Quantity3 = cusSupportingInfo.CSI_Quantity3,
				ReferenceNumber = cusSupportingInfo.CSI_ReferenceNumber,
				ItemNumber = cusSupportingInfo.CSI_ItemNumber == 0 ? null : (ZShort?)cusSupportingInfo.CSI_ItemNumber,
				ReferenceNumberCollection = cusSupportingInfo.CSI_ReferenceNumber2.IsEmpty
					? null
					: new List<Reference>
					{
						new Reference
						{
							Type = new EntryType
							{
								Code = Constants.ReferenceNumberTypes.Codes.LocalReferenceNumber,
								Description = Constants.ReferenceNumberTypes.Descriptions.LocalReferenceNumber
							},
							ReferenceNumber = cusSupportingInfo.CSI_ReferenceNumber2
						}
					},
				Status = ListHelper.GetWithDescription<CodeDescriptionPair>(cusSupportingInfo.CSI_Status, lookups.StatusList),
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(cusSupportingInfo.CSI_SubType, lookups.SubTypeList),
				Tariff = cusSupportingInfo.CSI_Tariff,
				UnitOfQuantity = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cusSupportingInfo.CSI_UnitOfQuantity, lookups.UnitOfQuantityList),
				UnitOfQuantity2 = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cusSupportingInfo.CSI_UnitOfQuantity2, lookups.UnitOfQuantity2List),
				UnitOfQuantity3 = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cusSupportingInfo.CSI_UnitOfQuantity3, lookups.UnitOfQuantity3List),
				DateOfExpiry = cusSupportingInfo.CSI_DateOfExpiry.Date,
				ValueCurrency = Currency.New(cusSupportingInfo.Currency),
				Value = cusSupportingInfo.CSI_Value,
				AdditionalDescription = cusSupportingInfo.CSI_AdditionalDescription,
				IssuerType = ListHelper.GetWithDescription<CodeDescriptionPair10Char>(cusSupportingInfo.CSI_IssuerType, lookups.IssuerTypeList),
				PackQuantity = cusSupportingInfo.CSI_PackQty,
				PackUnitOfQuantity = ListHelper.GetWithDescription<CodeDescriptionPair>(cusSupportingInfo.CSI_PackType, lookups.PackTypeList),
			};

			if (lookups.CodeList is ICodeDescriptionPairList codeList)
			{
				supportingInfo.Type = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(cusSupportingInfo.CSI_Code, codeList);
			}
			else if (lookups.CodeList is IFindBoxListProvider listProvider)
			{
				supportingInfo.Type = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(cusSupportingInfo.CSI_Code, listProvider);
			}

			writeManager.NotifyExported(supportingInfo, cusSupportingInfo);
			return supportingInfo;
		}

		public static List<UniversalCustoms.CustomsSupportingInformation> CreateCollection(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext = "", ZString[] supportedTypes = null)
		{
			var result = new List<UniversalCustoms.CustomsSupportingInformation>();
			if (bizObj != null)
			{
				ZGuid bizObjPK = bizObj.PK;
				ZString tablePrefix = bizObj.TablePrefix;
				if (bizObjPK.IsValid)
				{
					supportedTypes = supportedTypes ?? helper.GetSupportedCusSupportingInfoCSI_TypesFor(tablePrefix, dataContext);
					if (supportedTypes != null && supportedTypes.Length > 0)
					{
						var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, bizObjPK);
						query.AddToFilter(CusSupportingInfoSchema.CSI_Type, supportedTypes);
						var supportingInfos = helper.Load<CusSupportingInfo>(query);
						if (supportingInfos.Length > 0)
						{
							var typeList = helper.GetCusSupportingInfoCSI_TypeList(tablePrefix, dataContext);
							foreach (var cusSupportingInfosGroup in supportingInfos.GroupBy(x => x.CSI_Type).OrderBy(x => x.Key))
							{
								foreach (var cusSupportingInfo in cusSupportingInfosGroup.OrderBy(x => x.CSI_SystemCreateTimeUtc))
								{
									var data = Create(cusSupportingInfo, typeList, writeManager);
									result.Add(data);
								}
							}
						}
					}
				}
			}
			return result.Count == 0 ? null : result;
		}
	}
}
