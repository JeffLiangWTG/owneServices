using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WF = CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class CreateEventRule : ICustomAddOnRule
	{
		public static string DefaultEventCode => Events.CustomisableEvent00Code;
		public string Name => Res.GetString("Enterprise.MasterFiles.Business.CustomValues.CreateEvent", "Create Event on Edit");

		public ZString EventCode { get; set; } = DefaultEventCode;
		public ZString EventReference { get; set; }
		public ZBool IsEstimate { get; set; }
		public bool IsEnabled { get; set; }

		public string Code => WF.CustomAddOnRuleTypes.CreateEvent;

		public bool CanBeApplied(Type type) => type != typeof(ZBool);

		public IEnumerable<DynamicMetaData> GetMetaData() => Enumerable.Empty<DynamicMetaData>();

		public Action<ZPropertyInfo> GetValidator() => null;

		public OnSet GetOnSetBehaviour()
		{
			return (bizo, info, uniqueIdentifier, args) =>
			{
				var stmALogParent = bizo as IStmALogParent;
				// UniqueIdentifier can be ZGuid.Empty in the case of setting part 2 of a combo box before part 1
				if (stmALogParent != null && uniqueIdentifier != ZGuid.Empty && Events.All.Contains(EventCode))
				{
					var eventCode = Events.All[EventCode];
					var reference = EventReference.IsEmpty ? GetEventReferenceValue(args, args.FieldName) : EventReference;
					stmALogParent.CreateOrReplaceEventFromSource(uniqueIdentifier, eventCode, reference,
						GetEventTime(args),IsEstimate);
				}
			};
		}

		public bool IsUpperCase => false;

		ZDateTimeOffset GetEventTime(CustomAddOnRuleArgs args)
		{
			if (!args.HasChanges)
			{
				// Do not save event when Old and New values are the same
				return ZDateTimeOffset.Empty;
			}

			if (args.NewValue is ZDateTime dateTime && args.NewValue.IsValid)
			{
				return new ZDateTimeOffset(dateTime);
			}

			return ZDateTimeOffset.Now;
		}

		ZString GetEventReferenceValue(CustomAddOnRuleArgs args, string fieldName)
		{
			var result = new StringBuilder();
			result.Append($"|NAM={fieldName}");

			switch (args.Type)
			{
				case AddOnColumnDataType.Codes.String when args.Rules?.FirstOrDefault(i => i.Code == WF.CustomAddOnRuleTypes.InvalidCode) is InvalidCodeRule listRule:
					result.Append(GetReferenceValueFromInvalidCodeRuleList(args.OldValue, args.NewValue, listRule));
					break;
				case AddOnColumnDataType.Codes.Datetime when args.Rules?.FirstOrDefault(i => i.Code == WF.CustomAddOnRuleTypes.DateTimeFormat) is DateTimeFormatRule dateRule:
					result.Append(GetReferenceValueFromDateTimeFormatRule(args.OldValue, args.NewValue, dateRule));
					break;
				default:
					result.Append(GetReferenceValueDefault(args));
					break;
			}

			return new ZString(result.ToString());
		}

		const string DefaultEmptyTextValue = "\u2000";
		const string OldParameterPrefix = "OLD";
		const string NewParameterPrefix = "NEW";

		string GetReferenceValueDefault(CustomAddOnRuleArgs args)
		{
			var oldValue = GetParameterText(OldParameterPrefix, args.OldValue, args.OldDescription);
			var newValue = GetParameterText(NewParameterPrefix, args.NewValue, args.NewDescription);

			return $"{newValue}{oldValue}";
		}

		string GetReferenceValueFromInvalidCodeRuleList(IZType oldValue, IZType newValue, InvalidCodeRule listRule)
		{
			var codeList = listRule.List.OfType<CodeDescriptionPair>().ToList();

			if (codeList.Count == 0)
			{
				return $"{GetParameterText(OldParameterPrefix, oldValue)}{GetParameterText(NewParameterPrefix, newValue)}";
			}

			var oldDesc = (ZString)codeList.FirstOrDefault(i => i.Code == oldValue.ToString())?.Description;
			var newDesc = (ZString)codeList.FirstOrDefault(i => i.Code == newValue.ToString())?.Description;

			var fOldValue = GetParameterText(OldParameterPrefix, oldValue, oldDesc);
			var fNewValue = GetParameterText(NewParameterPrefix, newValue, newDesc);

			return $"{fNewValue}{fOldValue}";
		}

		string GetReferenceValueFromDateTimeFormatRule(IZType oldValue, IZType newValue, DateTimeFormatRule dateFormatRule)
		{
			var ruleFormat = dateFormatRule.Format;
			var fOldValue = $"|{OldParameterPrefix}={DefaultEmptyTextValue}";
			var fNewValue = $"|{NewParameterPrefix}={DefaultEmptyTextValue}";

			var format = ZDateTime.BestReadableDateTimeFormat;

			if (ruleFormat == KDateTimeFormat.Short)
			{
				format = ZDateTime.ShortDateFormat;
			}
			else if (ruleFormat == KDateTimeFormat.Time)
			{
				format = ZDateTime.ShortTimeFormat;
			}

			if (oldValue is ZDateTime oldDateTime && oldValue.IsValid && !oldValue.IsEmpty)
			{
				fOldValue = $"|{OldParameterPrefix}={oldDateTime.ToString(format)}";
			}

			if (newValue is ZDateTime newDateTime && newValue.IsValid && !newValue.IsEmpty)
			{
				fNewValue = $"|{NewParameterPrefix}={newDateTime.ToString(format)}";
			}

			return $"{fNewValue}{fOldValue}";
		}

		string GetParameterText(string name, IZType value, IZType desc = null)
		{
			var fValue = value == null || value.IsEmpty ? GetDefault(value) : $"{value}";
			var fDesc = desc == null || desc.IsEmpty ? null : $"{desc}";
			var result = string.Join(":", new[] { fValue, fDesc }.Where(i => i != null));

			if (string.IsNullOrEmpty(result))
			{
				result = DefaultEmptyTextValue;
			}

			return $"|{name}={result}";
		}

		string GetDefault(IZType value)
		{
			if (value == null || value is ZString)
			{
				return null;
			}

			return value.Default.ToString();
		}
	}
}
