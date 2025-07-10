using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider, IWhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider
	{
		static class AdHocType
		{
			public const string Code = "ADH";
			public static MultilingualString Description { get { return ResString.GetMultilingualString("AdHocType|AdHocJob", "Ad Hoc Service Job"); } }
		}

		static class VASOrderType
		{
			public static MultilingualString Name => ResString.GetMultilingualString("VASOrder|Name", "VAS Order");
			public static MultilingualString Description => ResString.GetMultilingualString("VASOrder|Description", "Value-Added Service Order");
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (whsDocketTypeAndSubTypePairList == null)
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(GetCombinedDocketTypeSubTypeList(DocketType.Codes.Receive, DocketType.Descriptions.Receive, new ReceiveType(), ReceiveType.Codes.Receipt));
				list.AddRange(GetCombinedDocketTypeSubTypeList(DocketType.Codes.Order, DocketType.Descriptions.Order, new OrderType(), OrderType.Codes.Order));
				list.AddRange(GetCombinedDocketTypeSubTypeList(DocketType.Codes.Adjustment, DocketType.Descriptions.Adjustment, new AdjustmentType(), AdjustmentType.Codes.Adjustment));
				list.AddRange(GetCombinedDocketTypeSubTypeList(DocketType.Codes.Transfer, DocketType.Descriptions.Transfer, new TransferType(), TransferType.Codes.Internal));
				list.AddPair(AdHocType.Code, BuildListDescription(AdHocType.Description, AdHocType.Code, AdHocType.Description));
				list.AddPair(WhsVASOrderSchema.Constants.Prefix, BuildListDescription(VASOrderType.Name, WhsVASOrderSchema.Constants.Prefix, VASOrderType.Description));
				whsDocketTypeAndSubTypePairList = list;
			}

			return whsDocketTypeAndSubTypePairList;
		}

		ReadOnlyCodeDescriptionPairList whsDocketTypeAndSubTypePairList;

#if DEBUG
		public
#endif
		ReadOnlyCodeDescriptionPairList GetCombinedDocketTypeSubTypeList(string docketTypeCode, string docketTypeDescription, ReadOnlyCodeDescriptionPairList subTypeList, string defaultSubTypeCode)
		{
			var list = new CodeDescriptionPairList();
			if (subTypeList.ContainsCode(defaultSubTypeCode))
			{
				list.AddPair(docketTypeCode + defaultSubTypeCode, BuildListDescription(docketTypeDescription, defaultSubTypeCode, subTypeList.GetDescriptionFromCode(defaultSubTypeCode)));

				foreach (var subType in subTypeList.Cast<ICodeDescription>().Where(st => st.Code != defaultSubTypeCode))
				{
					list.AddPair(docketTypeCode + subType.Code, BuildListDescription(docketTypeDescription, subType.Code, subType.Description));
				}
			}
			else
			{
				throw new ArgumentException("defaultSubTypeCode is not in subTypeList.");
			}
			return list;
		}

		static string BuildListDescription(string description, string code, string infoDetail) => string.Format(Culture.Invariant, "{0} - {1} ({2})", description, code, infoDetail.ToUpper(Culture.Invariant));
	}
}
