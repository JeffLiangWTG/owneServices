using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Freight.Business
{
	public static class DateTimeOffsetRelatedPortHelper
	{
		public static void HookDateTimeOffsetProperties(BusinessObject bizObj)
		{
			if (bizObj is null)
			{
				return;
			}

			foreach (ZPropertyInfo info in bizObj.ZPropertyInfoHash)
			{
				if (info.PropertyDescriptor.Attributes[typeof(DateTimeOffsetRelatedPortAttribute)] is DateTimeOffsetRelatedPortAttribute attribute)
				{
					var relatedPortPropertyName = attribute.RelatedPortProperty;
					var relatedPortPropertyInfo = bizObj.ZPropertyInfoHash.GetPropertySafe(relatedPortPropertyName);
					if (relatedPortPropertyInfo != null)
					{
						relatedPortPropertyInfo.ValueChanged += (sender, args) => RelatedPortPropertyInfoOnValueChanged(info, args);
						info.ValueChanged += (sender, args) => DateTimeOffsetPropertyInfoOnValueChanged(relatedPortPropertyName, sender, args);

						if (attribute.IsRelatedPortMandatory)
						{
							info.AdditionalValidation += () => DateTimeOffsetAdditionalValidation(info, bizObj, relatedPortPropertyName);
						}
					}
				}
			}
		}

		static void DateTimeOffsetPropertyInfoOnValueChanged(string relatedPortPropertyName, object sender, EventArgs e)
		{
			if (sender is BusinessObject bizObj
				&& e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var relatedPort = (ZString)bizObj[relatedPortPropertyName];
				SetOffsetValueBasedOnPort(valueChangedEventArgs.Info, relatedPort);
			}
		}

		static void DateTimeOffsetAdditionalValidation(ZPropertyInfo info, BusinessObject bizObj, string relatedPortPropertyName)
		{
			if (bizObj[relatedPortPropertyName] is ZString relatedPort && relatedPort.IsEmpty)
			{
				info.AddError(Res.GetString("5ddae5ff-57ae-4ed8-ab4d-1372fc7d845c", "{0} is required", relatedPortPropertyName));
			}
		}

		static void RelatedPortPropertyInfoOnValueChanged(ZPropertyInfo dateTimePropInfo, EventArgs e)
		{
			if (e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var newPort = (ZString)valueChangedEventArgs.NewValue;
				SetOffsetValueBasedOnPort(dateTimePropInfo, newPort);
			}
		}

		static void SetOffsetValueBasedOnPort(ZPropertyInfo info, string unloco)
		{
			if (info.Value is ZDateTimeOffset value && value.IsValid)
			{
				var offset = TimeSpan.Zero;
				var dateTimePart = value.ToDateTimeOffset().DateTime;
				if (!string.IsNullOrEmpty(unloco))
				{
					offset = GetUtcOffsetForPort(unloco, dateTimePart);
				}

				info.Value = new ZDateTimeOffset(dateTimePart, offset);
			}
		}

		static TimeSpan GetUtcOffsetForPort(string unloco, DateTime localDateTime)
		{
			var result = TimeSpan.Zero;
			if (unloco == null)
			{
				return result;
			}

			return Env.Time.GetUtcOffsetBasedOnLocal(unloco, localDateTime);
		}
	}
}
