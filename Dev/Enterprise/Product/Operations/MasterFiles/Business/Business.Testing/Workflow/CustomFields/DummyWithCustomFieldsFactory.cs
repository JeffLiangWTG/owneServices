using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyWithCustomFieldsFactory : IDummyWithCustomFieldsFactory
	{
		Action dispose;

		public BusinessObject GetDummyWithCustomFields(
			BusinessObjectFactory factory,
			out AddCustomField addCustomField,
			out Action<(string Name, string Type), IZType> setCustomFieldValue,
			out Func<(string Name, string Type), IZType> getCustomFieldValue,
			out Func<(string Name, string Type), string> getCustomFieldIdentifier)
		{
			return GetDummyWithCustomFields(
				factory,
				null,
				out _,
				out addCustomField,
				out setCustomFieldValue,
				out getCustomFieldValue,
				out getCustomFieldIdentifier);
		}

		public BusinessObject GetDummyWithCustomFields(
			BusinessObjectFactory factory,
			string addOnRulesXml,
			out IEnumerable<ICustomAddOnRule> addOnRules,
			out AddCustomField addCustomField,
			out Action<(string Name, string Type), IZType> setCustomFieldValue,
			out Func<(string Name, string Type), IZType> getCustomFieldValue,
			out Func<(string Name, string Type), string> getCustomFieldIdentifier)
		{
			var customAddOnRule = (addOnRulesXml != null) ? CustomFieldProcessTaskTemplateTestHelper.AddAddOnRules(factory, Guid.NewGuid().ToString("N"), addOnRulesXml) : null;

			var template = CustomFieldProcessTaskTemplateTestHelper.GetTemplate(factory);
			var dummy = CustomFieldProcessTaskTemplateTestHelper.GetDummy(factory);

			addOnRules = customAddOnRule?.GetRules();
			addCustomField = (field, addOnRuleXml) =>
			{
				if (addOnRuleXml != null)
				{
					customAddOnRule = CustomFieldProcessTaskTemplateTestHelper.AddAddOnRules(factory, Guid.NewGuid().ToString("N"), addOnRuleXml);
				}

				CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, field.Name, field.Type, customAddOnRule);
				factory.Save();
			};

			setCustomFieldValue = (field, value) => dummy.GetCustomFieldAccessor(field.Name, field.Type)?.SetValue(value);
			getCustomFieldValue = (field) => dummy.GetCustomFieldAccessor(field.Name, field.Type)?.GetValue();
			getCustomFieldIdentifier = (field) => dummy.GetCustomFieldAccessor(field.Name, field.Type)?.PropertyName;

			dispose = () =>
			{
				template.P0_IsActive = false;
				factory.Save();
			};

			return dummy;
		}

		public void Dispose()
		{
			dispose?.Invoke();
		}
	}
}
