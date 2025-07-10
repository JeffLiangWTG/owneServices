using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business;

sealed class FieldToDisplayValidationResolver(ZString fieldToDisplayValidation)
{
	readonly ZString fieldToDisplayValidation = Utilities.TrimExpression(fieldToDisplayValidation);

	public ZPropertyInfo GetFieldToDisplayValidationZPropertyInfo(IBusiness entity)
	{
		if (fieldToDisplayValidation.IsEmpty || entity is null)
		{
			return null;
		}

		var fieldValueInfo = new WorkflowMacroEvaluator().GetValue(entity, fieldToDisplayValidation, out _);
		if (fieldValueInfo is null)
		{
			return null;
		}

		var zPropertyInfo  = new ZPropertyInfoFinder(fieldValueInfo.BusinessObject).GetZPropertyInfo(fieldValueInfo.FieldName, fieldValueInfo.IsCustomField);
		return zPropertyInfo;
	}

	public FiledToDisplayValidationStaticInfo GetFieldToDisplayValidationStaticInfo(Type componentType) => GetFieldToDisplayValidationStaticInfoCore(componentType, fieldToDisplayValidation);

	FiledToDisplayValidationStaticInfo GetFieldToDisplayValidationStaticInfoCore(Type componentType, string propertyPath)
	{
		propertyPath = Utilities.TrimExpression(propertyPath);

		if (componentType == null || string.IsNullOrEmpty(propertyPath))
		{
			return null;
		}

		if (typeof(IBusinessObjectCollection).IsAssignableFrom(componentType))
		{
			new MacroClauseProcessor(null, null).ProcessPropertyAndValue(propertyPath, out var nextPath, out _);
			return GetFieldToDisplayValidationStaticInfoCore(BusinessObjectCollection.GetElementTypeFromCollectionType(componentType), nextPath);
		}

		var (customFieldName, customFieldType) = MacroHelper.GetCustomFieldNameAndType(propertyPath);
		var isCustomField = !customFieldName.IsEmpty;
		if (isCustomField)
		{
			var (customProperty, _) = MacroHelper.GetCustomProperty(new BusinessObjectFactory().GetNull(componentType), customFieldName, customFieldType);
			return customProperty is not null ? new FiledToDisplayValidationStaticInfo(componentType, customProperty.Identifier, customProperty.Info.Type, true) : null;
		}

		var nextPropertyInfo = new WorkflowMacroEvaluator().GetNextPropertyInfo(componentType, propertyPath, out string subPath);
		if (nextPropertyInfo == null)
		{
			return null;
		}

		return string.IsNullOrEmpty(subPath)
			? new FiledToDisplayValidationStaticInfo(componentType, propertyPath, nextPropertyInfo.PropertyType, false)
			: GetFieldToDisplayValidationStaticInfoCore(nextPropertyInfo.PropertyType, subPath);
	}
}
