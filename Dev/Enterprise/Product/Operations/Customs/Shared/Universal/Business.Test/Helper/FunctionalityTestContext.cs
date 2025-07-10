using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing;

public class FunctionalityTestContext : IDisposable
{
	readonly record struct FunctionalityKey(string Code, string Grouping, string Attribute = null);

	readonly Dictionary<FunctionalityKey, IDisposable> settings = new();

	public void SetFunctionality(ZString code, ZString grouping, ZDateTime effectiveDate, bool enabled)
		=> SetFunctionalityCore(code, grouping, effectiveDate, enabled);

	protected virtual void SetFunctionalityCore(ZString code, ZString grouping, ZDateTime effectiveDate, bool enabled)
	{
		var key = new FunctionalityKey(code, grouping);
		if (settings.TryGetValue(key, out var alreadySetFunctionality))
		{
			settings.Remove(key);
			alreadySetFunctionality.Dispose();
		}
		var temporarySetFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(code, grouping, effectiveDate, enabled);
		settings.Add(key, temporarySetFunctionality);
	}

	public void SetFunctionalityAttribute(ZString code, ZString grouping, ZDateTime effectiveDate, ZString attribute, ZString value)
		=> SetFunctionalityAttributeCore(code, grouping, effectiveDate, attribute, value);

	protected virtual void SetFunctionalityAttributeCore(ZString code, ZString grouping, ZDateTime effectiveDate, ZString attribute, ZString value)
	{
		var key = new FunctionalityKey(code, grouping, attribute);
		if (settings.TryGetValue(key, out var alreadySetFunctionality))
		{
			settings.Remove(key);
			alreadySetFunctionality.Dispose();
		}
		var temporarySetFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(code, grouping, effectiveDate, attribute, value);
		settings.Add(key, temporarySetFunctionality);
	}

	public void Reset() => ResetCore();

	protected virtual void ResetCore()
	{
		settings.Values.ForEach(x => x.Dispose());
		settings.Clear();
	}

	public virtual void Dispose()
	{
		Reset();
	}
}
