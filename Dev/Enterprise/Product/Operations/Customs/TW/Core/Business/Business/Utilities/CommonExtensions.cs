using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public static class CommonExtensions
	{
		public static void RemoveAndDeleteAllIfReadOnly(this BusinessObjectCollection collection)
		{
			if (collection.ReadOnly && collection.Any())
			{
				collection.RemoveAndDeleteAll();
			}
		}

		public static void ClearValueIfReadOnly(this ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.ReadOnly && !propertyInfo.Value.IsEmpty)
			{
				propertyInfo.ClearValue();
			}
		}
	}
}
