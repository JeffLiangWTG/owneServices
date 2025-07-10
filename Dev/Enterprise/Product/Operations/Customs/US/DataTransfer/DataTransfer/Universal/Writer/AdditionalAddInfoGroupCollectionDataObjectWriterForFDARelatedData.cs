using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	class AdditionalAddInfoGroupCollectionDataObjectWriterForFDARelatedData : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public AdditionalAddInfoGroupCollectionDataObjectWriterForFDARelatedData(UniversalDataObjectWriterHelper helper, FDA fda)
		{
			this.helper = Argument.NotNull(helper, "helper");
			this.fda = Argument.NotNull(fda, "fda");
		}
		readonly UniversalDataObjectWriterHelper helper;
		readonly FDA fda;

		#region IAdditionalAddInfoGroupCollectionDataObjectWriter Members

		public IEnumerable<UniversalCustoms.AddInfoGroup> CreateCollection()
		{
			foreach (var relatedBillAddInfo in GetRelatedBillAddInfo())
			{
				yield return relatedBillAddInfo;
			}
			foreach (var relatedContainerAddInfo in GetRelatedContainerAddInfo())
			{
				yield return relatedContainerAddInfo;
			}
		}

		IEnumerable<UniversalCustoms.AddInfoGroup> GetRelatedContainerAddInfo()
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, fda.TablePrefix);
			query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusContainerInvoiceLinePivotSchema.Constants.Prefix);
			foreach (var pivot in helper.Load<GenPivot>(query))
			{
				var containerPivot = helper.Load<CusContainerInvoiceLinePivot>(pivot.XX_Relation2ID);
				if (containerPivot != null)
				{
					var container = helper.Load<CusContainer>(containerPivot.C2_CO);
					if (container != null)
					{
						var addInfoGroup = new UniversalCustoms.AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.Type, Description = "FDA Related Container" },
							AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>()
						};
						AddRelatedDetail(addInfoGroup.AddInfoCollection, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.ContainerNumber, container.CO_ContainerNumber, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.SealNumber, container.CO_Seal);
						yield return addInfoGroup;
					}
				}
			}
		}

		IEnumerable<UniversalCustoms.AddInfoGroup> GetRelatedBillAddInfo()
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, fda.TablePrefix);
			query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusDecHouseBillSchema.Constants.Prefix);
			foreach (var pivot in helper.Load<GenPivot>(query))
			{
				var bill = helper.Load<Bill>(pivot.XX_Relation2ID);
				if (bill != null)
				{
					Bill houseBill = bill.CU_BillType == BillTypeList.Codes.HouseBill ? bill : null;
					Bill masterBill = houseBill == null ? bill.CU_BillType == BillTypeList.Codes.MasterBill ? bill : null : helper.Load<Bill>(houseBill.CU_CU_ParentBill);
					if (houseBill != null || masterBill != null)
					{
						var addInfoGroup = new UniversalCustoms.AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.Type, Description = "FDA Related Bill" },
							AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>()
						};
						if (masterBill != null)
						{
							AddRelatedDetail(addInfoGroup.AddInfoCollection, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.MasterBillIssuerCode, masterBill.US_UI_NKBillIssuerSCAC, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.MasterBill, masterBill.CU_BillNum);
						}
						if (houseBill != null)
						{
							AddRelatedDetail(addInfoGroup.AddInfoCollection, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.HouseBillIssuerCode, houseBill.US_UI_NKBillIssuerSCAC, CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.HouseBill, houseBill.CU_BillNum);
						}
						yield return addInfoGroup;
					}
				}
			}
		}

		void AddRelatedDetail(List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfos, ZString key1, ZString value1, ZString key2, ZString value2)
		{
			addInfos.Add(CreateAddInfo(key1, value1));
			addInfos.Add(CreateAddInfo(key2, value2));
		}

		UniversalDataBuss.DataObjects.Universal.AddInfo CreateAddInfo(ZString key, ZString value)
		{
			return new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = value };
		}

		#endregion
	}
}
