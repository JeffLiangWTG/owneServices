using System.Linq;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class IDataContextDataObjectExtensions
	{
		public static bool ContainsHoldCode(this IDataContextDataObject dataContext)
		{
			return dataContext?.RecipientRoleCollection?.Any(r => r.ServiceCode == ServiceCodeType.HLD) ?? false;
		}

		public static bool IsWarehouseBondedChangeOfOwnership(this IDataContextDataObject dataContext)
		{
			return dataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.BCO) ?? false;
		}

		public static bool IsWarehouseBondedChangeOfRegime(this IDataContextDataObject dataContext)
		{
			return dataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.BCR) ?? false;
		}

		public static bool IsWarehouseBondedChangeOfInventory(this IDataContextDataObject dataContext)
		{
			return dataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.BCO || r.Code == RecipientRoleType.BCR) ?? false;
		}
	}
}
