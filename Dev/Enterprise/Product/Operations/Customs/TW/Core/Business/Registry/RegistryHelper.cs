using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class RegistryHelper
	{
		#region BoxNumber
		public static CusBrokerageBoxNumberCollection CusBrokerageBoxNumbers => TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		internal static CodeDescriptionPairList GetBoxNumberList(BusinessObjectFactory factory) => factory.GetCachedValue("Enterprise.Customs.TW.Business.BoxNumberList|" + GlbCompany.CurrentCompany.PK.ToStringKey(), () =>
		{
			var result = new CodeDescriptionPairList();
			var boxNumbers = CusBrokerageBoxNumbers.Cast<CusBrokerageBoxNumber>().Select(x => x.BoxNumber).Distinct();
			foreach (var item in boxNumbers)
			{
				result.AddPair(item);
			}
			result.Sort();
			return result;
		});

		internal static CodeDescriptionPairList GetBoxNumberList(BusinessObjectFactory factory, ZString customsDistrict) => factory.GetCachedValue("Enterprise.Customs.TW.Business.BoxNumberList|" + GlbCompany.CurrentCompany.PK.ToStringKey() + customsDistrict, () =>
		{
			var result = new CodeDescriptionPairList();
			CusBrokerageBoxNumbers.Cast<CusBrokerageBoxNumber>().Where(x => x.CustomsOfficeArea == customsDistrict).Select(x => x.BoxNumber).Distinct().ForEach(x => result.AddPair(x));
			result.Sort();
			return result;
		});

		internal static ZString GetDefaultValueForBoxNumber(ZString receiptOffice) => GetDefaultBoxNumberByCustomsDistrict(receiptOffice.SubstringSafe(0, 1));

		internal static ZString GetDefaultBoxNumberByCustomsDistrict(ZString customsDistrict) => CusBrokerageBoxNumbers.Cast<CusBrokerageBoxNumber>().FirstOrDefault(x => x.CustomsOfficeArea == customsDistrict && x.IsDefaultBoxNumber)?.BoxNumber ?? ZString.Empty;
		#endregion

		#region GoodsLocation
		static CusGoodsLocationCollection CusGoodsLocations => TWCustomsDataRegistry.Instance.CusGoodsLocation.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public static ZString GetDefaultGoodsLocation(ZString customsOffice, ZString messageType) =>
			 CusGoodsLocations.Cast<CusGoodsLocation>().FirstOrDefault(x => x.CustomsOffice == customsOffice && x.MessageType == messageType)?.GoodsLocation ?? ZString.Empty;
		#endregion

		internal static CusBrokerStaff DefaultBrokerStaff => TWCustomsDataRegistry.Instance.CusBrokerStaff.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		internal static CusCustomsOffice DefaultCustomsOffice => TWCustomsDataRegistry.Instance.CusCustomsOffice.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static ZString DefaultCustomsOfficeCode => DefaultCustomsOffice?.CustomsOfficeCode ?? ZString.Empty;

		public static void NewOrUpdateIfExist<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key, TValue value)
		{
			if (map.ContainsKey(key))
			{
				map[key] = value;
			}
			else
			{
				map.Add(key, value);
			}
		}

		internal static TWNCATKClientSetting TWNCATKClientSetting => TWCustomsDataRegistry.Instance.TWNCATKClientSetting.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public static bool ValidateEntryNumber => TWCustomsDataRegistry.Instance.ValidateEntryNumber.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public static bool DefaultPrintingGoodsLocationDescription => TWCustomsDataRegistry.Instance.DefaultPrintingGoodsLocationDescription.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		#region NXM registry items
		public static bool EnableNX201_01 => TWCustomsDataRegistry.Instance.EnableNX201_01.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX201_07 => TWCustomsDataRegistry.Instance.EnableNX201_07.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX301 => TWCustomsDataRegistry.Instance.EnableNX301.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX301_AX => TWCustomsDataRegistry.Instance.EnableNX301_AX.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX301_DN => TWCustomsDataRegistry.Instance.EnableNX301_DN.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX401 => TWCustomsDataRegistry.Instance.EnableNX401.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX601 => TWCustomsDataRegistry.Instance.EnableNX601.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

		public static bool EnableNX603 => TWCustomsDataRegistry.Instance.EnableNX603.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
		#endregion
	}
}
