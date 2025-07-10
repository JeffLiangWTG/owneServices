using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TranCircumstancePropertyChangedNotifier
{
	public TranCircumstancePropertyChangedNotifier(TranCircumstance parentTranCircumstance)
	{
		Parent = Argument.NotNull(parentTranCircumstance, nameof(parentTranCircumstance));
		notifyActions = RegisterActions().ToDictionary(x => x.PropertyName, x => x.Action);
	}
	readonly Dictionary<ZString, Action<ZString, ZString>> notifyActions;

	protected TranCircumstance Parent { get; }

	public void NotifyChange(ZString propertyName, ZString oldValue, ZString newValue)
	{
		if (notifyActions.ContainsKey(propertyName))
		{
			notifyActions[propertyName](oldValue, newValue);
		}
	}

	protected virtual void NotifyPropertyCY_CodeChanged(ZString oldValue, ZString newValue)
	{
		var jobComInvoiceHeader = Parent?.Parent as JobComInvoiceHeader;
		if (jobComInvoiceHeader != null)
		{
			foreach (var item in jobComInvoiceHeader.TranCircumstances)
			{
				item.Validation.ValidateCY_Code();
			}
			jobComInvoiceHeader.Validation.ValidateAdditionalTranCircumstanceCodesAsString();
		}
	}

	IEnumerable<(ZString PropertyName, Action<ZString, ZString> Action)> RegisterActions()
	{
		yield return (TranCircumstance.Schema.CY_Code, NotifyPropertyCY_CodeChanged);
	}
}
