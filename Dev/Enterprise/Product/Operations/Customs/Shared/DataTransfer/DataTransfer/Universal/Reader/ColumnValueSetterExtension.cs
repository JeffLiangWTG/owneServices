using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class ColumnValueSetterExtension
	{
		public static void SetValueOnSetterSupenderParentInSpecificOrder(this Dictionary<string, ValueSetter> dictionary, SetterSuspender setterSuspender, IEnumerable<ZString> matchingKeysInSettingOrder)
		{
			if (dictionary != null)
			{
				var setters = new Dictionary<string, ValueSetter>(dictionary);

				if (matchingKeysInSettingOrder != null)
				{
					foreach (var matchingKey in matchingKeysInSettingOrder)
					{
						ValueSetter setter;

						if (dictionary.TryGetValue(matchingKey, out setter))
						{
							setters.Remove(matchingKey);
							SetValueWithSettingAction(setterSuspender, setter);
						}
					}
				}

				foreach (var pair in setters)
				{
					SetValueWithSettingAction(setterSuspender, pair.Value);
				}
			}
		}

		static void SetValueWithSettingAction(SetterSuspender setterSuspender, ValueSetter setter)
		{
			if (setter != null)
			{
				var columnName = (setter as IColumnValueSetterInfo)?.Column?.Name;

				var settingAction = (setterSuspender?.IsSetterSuspended(columnName) ?? false)
					? setterSuspender.ResumeSetting(columnName)
					: DisposableAction.NoAction;

				using (settingAction)
				{
					setter.SetValue();
				}
			}
		}
	}
}
