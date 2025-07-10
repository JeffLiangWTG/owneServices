using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public delegate void AddCustomField((string Name, string Type) customField, string addOnRulesXml = null);

	public interface IDummyWithCustomFieldsFactory : IDisposable
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		BusinessObject GetDummyWithCustomFields(
			BusinessObjectFactory factory,
			out AddCustomField addCustomField,
			out Action<(string Name, string Type), IZType> setCustomFieldValue,
			out Func<(string Name, string Type), IZType> getCustomFieldValue,
			out Func<(string Name, string Type), string> getCustomFieldIdentifier);

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		BusinessObject GetDummyWithCustomFields(
			BusinessObjectFactory factory,
			string addOnRulesXml,
			out IEnumerable<ICustomAddOnRule> addOnRules,
			out AddCustomField addCustomField,
			out Action<(string Name, string Type), IZType> setCustomFieldValue,
			out Func<(string Name, string Type), IZType> getCustomFieldValue,
			out Func<(string Name, string Type), string> getCustomFieldIdentifier);
	}
}
