using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	static class DataModelHelper
	{
		internal static void ReportDataModelErrorIfNeeded(this BusinessObject bizObj, ZPropertyInfo propertyInfo, ZString newValue)
		{
			var oldValue = (ZString)propertyInfo.Value;
			if (!oldValue.IsEmpty && oldValue != newValue && bizObj.IsInDatabase)
			{
				ErrorReporter.ReportOnce($"{propertyInfo.Name} can only be set once.");
			}
			if (newValue.IsEmpty && bizObj is IBusinessObjectInternals bizObjInternals && !bizObjInternals.IsCopying)
			{
				ErrorReporter.ReportOnce($"{propertyInfo.Name} should not be set to empty.");
			}
		}

		public static void PopulateDataModelFromParentIfNeeded(this IDataModelSupporter bizObj, IDataModelSupporter parentBizObj)
		{
			if (bizObj.DataModel.IsEmpty && !bizObj.IsDeleted && parentBizObj != null)
			{
				parentBizObj.PopulateDataModelIfNeeded();
				bizObj.DataModel = parentBizObj.DataModel;
			}
		}

		public static void PopulateDataModelFromCountryCodeIfNeeded(this IDataModelSupporter bizObj, ZString countryCode)
		{
			if (bizObj.DataModel.IsEmpty && !bizObj.IsDeleted)
			{
				bizObj.DataModel = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			}
		}
	}
}
