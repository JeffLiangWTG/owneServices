using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business;

sealed record FiledToDisplayValidationStaticInfo(
	Type BusinessObjectType,
	ZString FieldName,
	Type FieldValueType,
	bool IsCustomField)
{
	public Type BusinessObjectType { get; } = BusinessObjectType;
	public ZString FieldName { get; } = FieldName;
	public bool IsCustomField { get; } = IsCustomField;
	public Type FieldValueType { get; } = FieldValueType;

	public ZBool IsZPropertyInfo => typeof(IZType).IsAssignableFrom(FieldValueType);

	public ZString GetTableName()
	{
		if (!typeof(BusinessObject).IsAssignableFrom(BusinessObjectType))
		{
			return ZString.Empty;
		}

		return BusinessObjectFactory.GetTableNameFromType(BusinessObjectType);
	}

	public ZPropertyInfo GetZPropertyInfo(IBusiness entity)
	{
		if (entity is null)
		{
			return null;
		}
		var zPropertyInfo = new ZPropertyInfoFinder(entity).GetZPropertyInfo(FieldName, IsCustomField);
		return zPropertyInfo;
	}
}
