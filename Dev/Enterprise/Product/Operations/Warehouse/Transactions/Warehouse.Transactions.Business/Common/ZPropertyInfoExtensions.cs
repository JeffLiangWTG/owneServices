using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	public static class ZPropertyInfoExtensions
	{
		public static void ConvertOffsetPropertyToTimeZone(this ZPropertyInfo propertyToUpdate, ITimeZone calculationTimeZone)
		{
			if (propertyToUpdate is ZPropertyInfoDateTimeOffset offsetPropertyInfo)
			{
				if (PropertyHasChanged(offsetPropertyInfo) && offsetPropertyInfo.Value.IsValid && !offsetPropertyInfo.HasErrors())
				{
					var utcValue = offsetPropertyInfo.Value.ToUtcDateTime();
					if (calculationTimeZone != null)
					{
						offsetPropertyInfo.Value = new ZDateTimeOffset(calculationTimeZone.ToLocalTime(utcValue), calculationTimeZone.GetUtcOffsetBasedOnUtc(utcValue));
					}
					else
					{
						offsetPropertyInfo.Value = new ZDateTime(utcValue).UtcToDateTimeOffset();
					}
				}
			}
			else
			{
				throw new InvalidOperationException($"{propertyToUpdate.Name} is not a valid ZDateTimeOffset property.");
			}
		}

		static bool PropertyHasChanged(ZPropertyInfo property) => !property.BizObj.IsInDatabase || property.HasChanges;
	}
}
