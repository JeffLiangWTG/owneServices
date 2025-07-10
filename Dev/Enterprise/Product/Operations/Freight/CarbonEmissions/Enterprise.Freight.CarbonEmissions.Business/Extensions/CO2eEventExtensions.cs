using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class CO2eEventExtensions
	{
		public static void LogGHGEvent(this ICO2eProvider provider, CO2eEventType type, string extra = null, decimal previousCO2eValue = 0)
		{
			if (provider is not IStmALogProvider stmALogProvider)
			{
				return;
			}
			var parameters = new Dictionary<string, string>();
			parameters.Add(Params.Type, type.ToString());
			if (extra != null)
			{
				if (CO2eEventType.Updated.Equals(type))
				{
					parameters.Add(Params.New, extra);
					var roundedPreviousCo2eValue = Weight.ConvertSafe(previousCO2eValue, Weight.Kilograms, Weight.Kilograms, false);
					parameters.Add(Params.Old, roundedPreviousCo2eValue == 0 ? "NA" : roundedPreviousCo2eValue.ToString());
				}
				else if (CO2eEventType.Rejected.Equals(type)
					|| CO2eEventType.Unauthorized.Equals(type)
					|| CO2eEventType.NotRequired.Equals(type)
					|| CO2eEventType.ServiceUnavailable.Equals(type))
				{
					parameters.Add(Params.Reason, extra);
				}
			}

			stmALogProvider.Logs.AddNew(AutoEvents.GreenhouseGasEmissionsCalculation, ZDateTimeOffset.Now, parameters.ToArray());
		}

		public static void LogSTUEvent(this ICO2eProvider provider, string oldStatus, string newStatus, CO2eStatusChangedReason reason)
		{
			if (provider is not IStmALogProvider stmALogProvider || string.IsNullOrEmpty(reason.ToString()))
			{
				return;
			}

			var parameters = new Dictionary<string, string>
			{
				{ Params.Old, oldStatus },
				{ Params.New, newStatus },
				{ Params.Type, (NoResString)"CO2e Status" },
				{ Params.Reason, (NoResString)$"Input value(s) have changed: {reason}" }
			};

			stmALogProvider.Logs.AddNew(AutoEvents.StatusUpdated, ZDateTimeOffset.Now, parameters.ToArray());
		}
	}

	public struct CO2eStatusChangedReason
	{
		public static CO2eStatusChangedReason Empty = new CO2eStatusChangedReason("");

		public CO2eStatusChangedReason(ZPropertyInfo changedProperty, IZType oldPropertyValue = null)
			: this(changedProperty, oldPropertyValue, null, null, string.Empty, string.Empty)
		{
		}

		public CO2eStatusChangedReason(string freeTextReason)
			: this(null, null, null, null, string.Empty, freeTextReason)
		{
		}

		public CO2eStatusChangedReason(ValueChangedEventArgs valueChangedEventArgs)
			: this(null, null, valueChangedEventArgs, null, string.Empty, string.Empty)
		{
		}

		public CO2eStatusChangedReason(CollectionCountChangedEventArgs collectionCountChangedEventArgs, string elementName)
			: this(null, null, null, collectionCountChangedEventArgs, elementName, string.Empty)
		{
		}

		CO2eStatusChangedReason(ZPropertyInfo changedProperty, IZType oldPropertyValue, ValueChangedEventArgs valueChangedEventArgs, CollectionCountChangedEventArgs collectionCountChangedEventArgs, string elementName, string freeTextReason)
		{
			this.changedProperty = changedProperty;
			this.oldPropertyValue = oldPropertyValue;
			this.valueChangedEventArgs = valueChangedEventArgs;
			this.collectionCountChangedEventArgs = collectionCountChangedEventArgs;
			this.elementName = elementName;
			this.freeTextReason = freeTextReason;
		}

		readonly ZPropertyInfo changedProperty;
		readonly IZType oldPropertyValue;
		readonly ValueChangedEventArgs valueChangedEventArgs;
		readonly CollectionCountChangedEventArgs collectionCountChangedEventArgs;
		readonly string elementName;
		readonly string freeTextReason;

		public override string ToString()
		{
			if (changedProperty is null && valueChangedEventArgs is null && collectionCountChangedEventArgs is null && string.IsNullOrEmpty(freeTextReason))
			{
				return string.Empty;
			}

			if (changedProperty is not null)
			{
				return $"{changedProperty.Name} [{oldPropertyValue ?? changedProperty.OriginalValue}]->[{changedProperty.Value}]";
			}
			else if (valueChangedEventArgs is not null)
			{
				return $"{valueChangedEventArgs.Info.Name} [{valueChangedEventArgs.OldValue}]->[{valueChangedEventArgs.NewValue}]";
			}
			else if (collectionCountChangedEventArgs is not null)
			{
				var text = collectionCountChangedEventArgs.ItemAdded ? (NoResString)"added" : (NoResString)"removed";
				return $"{elementName} {text}";
			}
			else
			{
				return freeTextReason;
			}
		}
	}
}
