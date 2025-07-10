using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Workflow;

namespace Enterprise.Workflow.Business
{
	sealed partial class ProcessFieldOnChangeHook
	{
		static void AddMultipleFieldChanges(
			EventLogReferenceBuilder eventLogReferenceBuilder,
			IEnumerable<TrackingState.PropertyChange> changedProperties,
			int remainingSpace)
		{
			if (remainingSpace < (FieldChangeParameterKey.Length + 5))
			{
				// No space to add the parameter
				return;
			}

			var sb = new StringBuilder();
			var fieldChanges = new List<FieldChange>();
			changedProperties.ForEachWithBetween(
				(propertyChange) =>
				{
					var fieldChange = new FieldChange(
						propertyChange.PropertyType,
						propertyChange.Name,
						!propertyChange.OriginalValue.Equals(propertyChange.DefaultValue) ? ConvertValueToCultureInvariantString(propertyChange.OriginalValue) : "",
						ConvertValueToCultureInvariantString(propertyChange.CurrentValue));

					fieldChanges.Add(fieldChange);
					sb.Append(fieldChange.PropertyName);
					sb.Append(" [");
					sb.Append(fieldChange.OldValue);
					sb.Append("]");
					sb.Append("->[");
					sb.Append(fieldChange.NewValue);
					sb.Append("]");
				},
				() => sb.Append(", "));

			var totalSize = fieldChanges.Sum(x => x.Length) + (fieldChanges.Count / 2) * 2;
			if (totalSize <= remainingSpace)
			{
				eventLogReferenceBuilder.AddMandatory(FieldChangeParameterKey, sb.ToString());
			}
			else
			{
				eventLogReferenceBuilder.AddMandatory(FieldChangeParameterKey, TruncateFieldChangeValues(fieldChanges, remainingSpace - 5));
			}
		}

		static string TruncateFieldChangeValues(List<FieldChange> fieldChanges, int remainingSpace)
		{
			// Sort by length descending
			fieldChanges.Sort((a, b) => a.Length.CompareTo(b.Length));
			var totalSize = fieldChanges.Sum(x => x.Length) + (fieldChanges.Count / 2) * 2;

			// Shrink the strings, biggest to smallest
			for (int i = fieldChanges.Count - 1; i >= 0; --i)
			{
				var fieldChange = fieldChanges[i];

				if (fieldChange.PropertyType != typeof(ZString))
				{
					break;
				}

				var originalSize = fieldChange.Length;
				if (!string.IsNullOrEmpty(fieldChange.OldValue))
				{
					fieldChange.OldValue = Res.GetString("2D771A8E-D75C-4E88-9B1E-492D109CB409", "{0} bytes", fieldChange.OldValue.Length);
				}

				fieldChange.NewValue = Res.GetString("2D771A8E-D75C-4E88-9B1E-492D109CB409", "{0} bytes", fieldChange.NewValue.Length);

				totalSize -= originalSize - fieldChange.Length;

				if (totalSize <= remainingSpace)
				{
					return ConvertFieldChangesToString(fieldChanges);
				}
			}

			// Still doesn't fit, so strip the values
			var sb = new StringBuilder();
			totalSize = 0;

			for (int i = 0; i < fieldChanges.Count; ++i)
			{
				var fieldChange = fieldChanges[i];
				totalSize += fieldChange.PropertyName.Length + 2;
				if (totalSize <= remainingSpace - 3)
				{
					sb.Append(fieldChange.PropertyName);
					sb.Append(", ");
				}
				else if (i == fieldChanges.Count - 1 && totalSize + fieldChange.PropertyName.Length + 2 <= remainingSpace)
				{
					sb.Append(fieldChange.PropertyName);
					return sb.ToString();
				}
				else
				{
					sb.Append("...");
					return sb.ToString();
				}
			}

			sb.Length -= 2;
			return sb.ToString();
		}

		static string ConvertFieldChangesToString(List<FieldChange> fieldChanges)
		{
			if (fieldChanges.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			fieldChanges.ForEachWithBetween(
				(fieldChange) =>
				{
					sb.Append(fieldChange.PropertyName);
					sb.Append(" [");
					sb.Append(fieldChange.OldValue);
					sb.Append("]");
					sb.Append("->[");
					sb.Append(fieldChange.NewValue);
					sb.Append("]");
				},
				() => sb.Append(", "));

			return sb.ToString();
		}

		static string ConvertValueToCultureInvariantString(IZType value)
		{
			if (value is ZBlob blobValue)
			{
				return Res.GetString("2D771A8E-D75C-4E88-9B1E-492D109CB409", "{0} bytes", blobValue.Length);
			}

			if (value is ZString stringValue)
			{
				if (!stringValue.IsWesternEuropeanOrEmpty)
				{
					// probably unicode
					return Res.GetString("2D771A8E-D75C-4E88-9B1E-492D109CB409", "{0} bytes", stringValue.Length);
				}
			}

			if (value is IFormattable formattable)
			{
				return formattable.ToString(null, CultureInfo.InvariantCulture);
			}

			return TypeDescriptor.GetConverter(value.BaseDataType).ConvertToInvariantString(value);
		}
	}
}
