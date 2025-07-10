using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCodePropertyChangedNotifier
	{
		public BaseSupplementaryCodePropertyChangedNotifier(BaseSupplementaryCode parentSupplementaryCode)
		{
			Parent = Argument.NotNull(parentSupplementaryCode, nameof(parentSupplementaryCode));
			notifyActions = RegisterActions().ToDictionary(x => x.PropertyName, x => x.Action);
		}
		readonly Dictionary<ZString, Action<ZString, ZString>> notifyActions;

		protected BaseSupplementaryCode Parent { get; }

		public void NotifyChange(ZString propertyName, ZString oldValue, ZString newValue)
		{
			if (notifyActions.ContainsKey(propertyName))
			{
				notifyActions[propertyName](oldValue, newValue);
			}
		}

		protected virtual void NotifyPropertyCY_CodeChanged(ZString oldValue, ZString newValue)
		{
			var parent = Parent;

			if (parent.Parent is ICusCodeDataWithOrderSupporter orderSupporter)
			{
				orderSupporter.OnCodesChanged();
			}
			if (parent.Parent is ISupplementaryCodeSupporter codeSupporter)
			{
				parent.CY_Data = codeSupporter.GetCountryCodeFromAdditionalCode(newValue);
			}
		}

		IEnumerable<(ZString PropertyName, Action<ZString, ZString> Action)> RegisterActions()
		{
			yield return (BaseSupplementaryCode.Schema.CY_Code, NotifyPropertyCY_CodeChanged);
		}
	}
}
