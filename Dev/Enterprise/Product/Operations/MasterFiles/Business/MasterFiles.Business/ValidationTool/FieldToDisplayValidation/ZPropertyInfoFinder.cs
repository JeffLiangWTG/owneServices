using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business;

sealed class ZPropertyInfoFinder(IBusiness businessObject)
{
	public ZPropertyInfo GetZPropertyInfo(ZString propertyNameOrIdentifier, ZBool isCustom)
	{
		ZPropertyInfo zPropertyInfo;
		if (isCustom)
		{
			zPropertyInfo = businessObject switch
			{
				CustomBusinessObject customBusinessObject => GetCustomBusinessObject(customBusinessObject.Parent)?.ZPropertyInfoHash.GetPropertySafe(propertyNameOrIdentifier),
				BusinessObject entity => GetCustomBusinessObject(entity)?.ZPropertyInfoHash.GetPropertySafe(propertyNameOrIdentifier),
				_ => null
			};
		}
		else
		{
			zPropertyInfo = (businessObject as BusinessObject)?.FindPropertyInfo(propertyNameOrIdentifier);
		}

		return zPropertyInfo;
	}

	internal static CustomBusinessObject GetCustomBusinessObject(BusinessObject input) =>
		((IBusiness)input).Children.OfType<CustomBusinessObject>().FirstOrDefault() ??
		(input as ICustomFieldProvider)?.GetCustomBusinessObject() ??
		input?.GetCustomBusinessObject(true);
}
