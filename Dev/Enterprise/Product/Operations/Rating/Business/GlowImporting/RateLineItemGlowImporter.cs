using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	abstract class RateLineItemGlowImporter
	{
		public abstract bool Import(
			RateLine rateLine,
			ValueObjectImportContext context,
			Dictionary<string, string> directValues,
			Dictionary<string, string> relationshipValues = null);
	}

	/// <summary>
	/// Handles the importing of RateLineItems directly to the RatelineItems table.
	/// Calculators must extend this and do their own importing direct to the calculator or use
	/// this basic functionality to import directly to the RateLineItems table.
	///
	/// Nb: It's marked abstract to dissuade people from using it directly. It should only be used
	/// as part of a calculator doing its importing.
	/// </summary>
	abstract class RateLineItemGlowImporter<TCalculator, TColumn> : RateLineItemGlowImporter
		where TCalculator : Calculator
		where TColumn : struct, Enum
	{
		protected abstract bool ImportCore(
			TCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<TColumn, string> directValues,
			Dictionary<string, string> relationshipValues = null);

		public override bool Import(
			RateLine rateLine,
			ValueObjectImportContext context,
			Dictionary<string, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			var values = ConvertForCalculator<TColumn>(directValues);
			var calculator = rateLine.GetCalculator<TCalculator>();

			return ImportCore(calculator, context, values, relationshipValues);
		}

		protected virtual bool ImportRateLineItem(
			RateLine rateLine,
			ValueObjectImportContext context,
			Dictionary<TColumn, string> directValues,
			Dictionary<string, string> relationshipValues,
			params IRateLineItemImportData[] importMapping
		)
		{
			return ImportRateLineItem(rateLine, context, directValues, relationshipValues, false, importMapping);
		}

		/// <summary>
		/// Will create a new RateLineItem row in the given RateLine and will then populate it with
		/// the directValues and relationshipValues provided.
		///
		/// It will only look at the TM_* keys (from the RateLineItem schema) in the directValues
		/// and will only look for certain relationships that it supports importing.
		/// </summary>
		/// <returns>true if success, false if failure.</returns>
		protected bool ImportRateLineItem(
			RateLine rateLine,
			ValueObjectImportContext context,
			Dictionary<TColumn, string> directValues,
			Dictionary<string, string> relationshipValues,
			bool replaceExisting,
			params IRateLineItemImportData[] importMapping
		)
		{
			var mappingsManyToSingle = importMapping
				.OfType<IRateLineItemImportDataMany>()
				.SelectMany(r => r.ToSingles());

			var mappingsAllSingle = importMapping
				.OfType<IRateLineItemImportDataSingle>()
				.Union(mappingsManyToSingle)
				.ToArray();

			var rateLineItem = GetRateLineItem(rateLine, replaceExisting, directValues, mappingsAllSingle, out var undoAction);
			var somethingWasSet = false;

			// Any relationships need to be imported before other fields are set.
			// Other fields may depend on the relationship being set such as the TransportZone
			// needs to be set before the call for pricing on the CTZ calculator otherwise
			// it may put the new break in a different zone and keep call for pricing readonly.
			var success = TrySetRelationships(context, relationshipValues, rateLineItem, ref somethingWasSet);

			if (success)
			{
					SetDatabaseColumnValue(rateLineItem, context, directValues, mappingsAllSingle, ref somethingWasSet, ref success);
			}

			if (!success || !somethingWasSet)
			{
				undoAction();
			}

			return success;
		}

		/// <summary>
		/// Sets the columns of a RateLineItem given the directValues and the importMapping
		/// If it successfully set a field then somethingWasSet is set to true.
		/// Otherwise somethingWasSet remains unchanged.
		/// </summary>
		static void SetDatabaseColumnValue(RateLineItem rateLineItem, ValueObjectImportContext context, Dictionary<TColumn, string> directValues, IRateLineItemImportDataSingle[] importMapping, ref bool somethingWasSet, ref bool success)
		{
			var rateLineItemType = rateLineItem.GetType();
			foreach (var data in importMapping)
			{
				if (!data.TryGetValue(directValues, out var columnValue) || string.IsNullOrEmpty(columnValue))
				{
					// This is not an error. It means the user did not map this value or it is empty
					// and thus we'll skip it.
					continue;
				}

				// Note: This may fail if there is no ZPropertyInfo for a given column that is
				// being imported. However, chances are low since each column gets its own
				// ZPropertyInfo automatically
				var property = rateLineItemType.GetProperty(data.BizoColumnName + "Info");
				var zProperty = property.GetValue(rateLineItem) as ZPropertyInfo;

				// before actually setting the value into the RateLineItem column, we try to convert
				// to see if it would pass the conversion. If we didn't do this then the Bizo will
				// simply set it to a default value silently and we wouldn't be able to alert
				// the user. This show throw an exception if it cannot be converted to that type.
				ConvertToType(zProperty.PropertyType, columnValue);

				// We do not check whether the property is read only when importing. This is intentional
				// If we were to check, then the VED calculator will not be importable since it
				// has a non-empty read only field which needs a value. If we don't check then the
				// CST/CTB calculator can allow the user to enter a restricted reason for the first
				// item in the row even though it was readonly.
				// This will be fixed on a later WI. 
				try
				{
					var hasError = context.SetPropertyInfoValueAndAddErrorIfNotWithinMaxLength(zProperty, columnValue);

					if (!data.IsIgnorable)
					{
						somethingWasSet = true;
					}

					if (hasError)
					{
						success = false;
					}
				}
				catch (FormatException ex)
				{
					throw new FormatConversionException(ex, columnValue);
				}
			}
		}

		/// <summary>
		/// Tries to set the relationship values.
		///
		/// If a relationship value was set successfully then somethingWasSet is set to true and true is returned.
		/// If a relationship value was not set successfully somethingWasSet is unchanged and false is returned.
		/// if there was no relationship to be set then somethingWasSet is unchanged and true is returned.
		/// </summary>
		bool TrySetRelationships(ValueObjectImportContext context, Dictionary<string, string> relationshipValues, RateLineItem rateLineItem, ref bool somethingWasSet)
		{
			var failure = false;
			if (relationshipValues != null)
			{
				failure = failure || !ImportTransportZoneRelationship(context, rateLineItem, relationshipValues, ref somethingWasSet);
				failure = failure || !ImportChargeCodeRelationship(context, rateLineItem, relationshipValues, ref somethingWasSet);
			}

			return !failure;
		}

		/// <summary>
		/// Gets the RateLineItem to use for importing the values into.
		/// </summary>
		/// <param name="shouldReuse">
		/// true if it should find an existing RateLineItem to reuse (and make a new if not found)
		/// false if it makes a new one regardless of existing RateLineItems
		/// </param>
		/// <param name="undoAction">
		/// An action for canceling the importing. This will delete the RateLineItem if it was
		/// created by this function. This will do nothing if the function returned an existing one.
		/// </param>
		RateLineItem GetRateLineItem(RateLine rateLine, bool shouldReuse, Dictionary<TColumn, string> directValues, IRateLineItemImportDataSingle[] mapping, out Action undoAction)
		{
			RateLineItem result = null;
			if (shouldReuse)
			{
				var foundMapping = mapping.SingleOrDefault(d => d.BizoColumnName == RateLineItemsSchema.TM_Type.Name);
				var foundTmType = string.Empty;

				if (foundMapping?.TryGetValue(directValues, out foundTmType) ?? false)
				{
					result = rateLine.RateLineItems.FindByTM_Type(foundTmType);
				}
			}

			var created = result == null;

			result = result ?? rateLine.RateLineItems.AddNew();
			if (created)
			{
				undoAction = () => rateLine.RateLineItems.RemoveAndDelete(result);
			}
			else
			{
				undoAction = () => { };
			}
			return result;
		}

		bool ImportChargeCodeRelationship(ValueObjectImportContext context, RateLineItem rateLineItem, Dictionary<string, string> relationshipValues, ref bool somethingWasSet)
		{
			var chargeCodeRelationship = $"{nameof(AccChargeCode)}.{AccChargeCodeSchema.Constants.AC_Code}";  // Dictionary key

			if (!relationshipValues.TryGetValue(chargeCodeRelationship, out var chargeCodeName) || chargeCodeName.IsNullOrEmpty())
			{
				// When the user configures their GLOW/ADAW mapping they may not prepare a mapping
				// for all the columns that a calculator offers.
				//
				// So the 'relationshipValues' (which is the mapping/value that the user is importing)
				// may not always have the key we're looking for - and this is okay.
				// It just means that there is data to import this relationship and so, we won't.
				return true;
			}

			var chargeCode = GetChargeCode(rateLineItem.Factory, chargeCodeName);
			if (chargeCode == null)
			{
				context.Notifications.AddError(ResString.GetMultilingualString("cb33a541-594c-4c95-ad0b-aa4b4d557ef5", "Charge code with name '{0}' not found.", chargeCodeName));
				return false;
			}
			else
			{
				rateLineItem.TM_AC = chargeCode.PK;
				somethingWasSet = true;
				return true;
			}
		}

		protected AccChargeCode GetChargeCode(BusinessObjectFactory factory, string chargeCodeName)
		{
			// Although it may seem silly to query twice, there is no other way.
			// A charge code may exist for many companies, and optionally also globally.
			// We must always return the charge code specific to the current company, and only if
			// there is none, we can then search for a global one.
			//
			// Putting these two in a single query with an 'or' statement will not work as it will
			// sometimes return the global one instead of the local one.
			return
				GetChargeCode(factory, chargeCodeName, Env.CurrentCompanyPK)
				??
				GetChargeCode(factory, chargeCodeName, null);
		}

		AccChargeCode GetChargeCode(BusinessObjectFactory factory, string chargeCodeName, Guid? companyPK)
		{
			var queryChargeCode = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCodeName)
				.AddToFilter(new ZQuery(AccChargeCodeSchema.AC_GC, companyPK));
			var chargeCode = factory.LoadTop1<AccChargeCode>(queryChargeCode);

			return chargeCode;
		}

		bool ImportTransportZoneRelationship(ValueObjectImportContext context, RateLineItem rateLineItem, Dictionary<string, string> relationshipValues, ref bool somethingWasSet)
		{
			var zoneNameRelationship = $"DomesticZone.{RateTransportZonesSchema.Constants.TZ_ZoneName}";  // Dictionary key

			if (!relationshipValues.TryGetValue(zoneNameRelationship, out var transportZoneName) || transportZoneName.IsNullOrEmpty())
			{
				// When the user configures their GLOW/ADAW mapping they may not prepare a mapping
				// for all the columns that a calculator offers.
				//
				// So the 'relationshipValues' (which is the mapping/value that the user is importing)
				// may not always have the key we're looking for - and this is okay.
				// It just means that there is no data to import to this relationship and so, we won't.
				return true;
			}

			var factory = rateLineItem.Factory;
			var zone = factory.LoadTop1<RateTransportZone>(new ZQuery(RateTransportZonesSchema.TZ_ZoneName, transportZoneName));
			if (zone == null)
			{
				context.Notifications.AddError(ResString.GetMultilingualString("56c6fe94-90c5-4416-a9f1-c38eca842f2c", "Transport Zone with name '{0}' not found.", transportZoneName));
				return false;
			}

			rateLineItem.TM_TZ_DomesticZone = zone.PK;
			somethingWasSet = true;
			return true;
		}

		#region Helper functions

		static Dictionary<TColumn2, string> ConvertForCalculator<TColumn2>(Dictionary<string, string> calculatorValues)
			where TColumn2 : struct, Enum
		{
			var newDictionary = calculatorValues
				.Select(calculatorValue =>
				{
					if (Enum.TryParse<TColumn2>(calculatorValue.Key, out var result))
					{
						return new
						{
							Column = result,
							Value = calculatorValue.Value
						};
					}
					return null;
				})
				.WhereNotNull() // Skipping the ones where TryParse failed. Those are the ones that
								// are unrelated to calculator T.
				.ToDictionary(x => x.Column, x => x.Value);

			return newDictionary;
		}

		/// <returns>true if a non-blank value was set, false if no value was set.</returns>
		static bool TrySet<TColumn2, TCalculator2>(
			TColumn2 calculatorValueColumn,
			Dictionary<TColumn2, string> calculatorValues,
			TCalculator2 calculator,
			string calculatorPropertyName
		) where TCalculator2 : Calculator
		{
			if (calculatorValues.TryGetValue(calculatorValueColumn, out string value) && !value.IsNullOrEmpty())
			{
				var zPropertyInfo = calculator.GetRelatedToInfo(calculatorPropertyName);

				if (!zPropertyInfo.ReadOnly)
				{
					var type = calculator.GetType();
					var prop = type.GetProperty(calculatorPropertyName);
					var convertedValue = ConvertToType(prop.PropertyType, value);

					prop.SetValue(calculator, convertedValue);
					return true;
				}
				return false;
			}

			return false;
		}

		protected static bool TryGet<TColumn2, TZType>(TColumn2 column, Dictionary<TColumn2, string> calculatorValues, out TZType result)
		{
			if (calculatorValues.TryGetValue(column, out string value) && !value.IsNullOrEmpty())
			{
				Type type = typeof(TZType);

				result = (TZType)ConvertToType(type, value);

				return true;
			}

			result = default;
			return false;
		}

		static bool SetCalculatorProperty<TColumn2, TCalculator2>(Dictionary<TColumn2, string> calculatorValues, TCalculator2 calculator, bool isAgencyRate, params (TColumn2, string)[] columnAndPropertyName)
			where TCalculator2 : Calculator
			where TColumn2 : Enum
		{
			var someValueSet = false;
			var line = calculator.Line;

			using (new DisposableAction(
				() => line.SetViewAgentRatesWithoutRefreshBinding(isAgencyRate),
				() => line.SetViewAgentRatesWithoutRefreshBinding(false)))
			{
				foreach (var pair in columnAndPropertyName)
				{
					someValueSet |= TrySet(pair.Item1, calculatorValues, calculator, pair.Item2);
				}
			}

			return someValueSet;
		}

		/// <returns>true if a non-blank value was set, false if no value was set.</returns>
		protected static bool SetCalculatorProperty<T, Y>(Dictionary<T, string> calculatorValues, Y calculator, params (T, string)[] columnsAndInstanceNames)
			where Y : Calculator
			where T : Enum
			=> SetCalculatorProperty(calculatorValues, calculator, false, columnsAndInstanceNames);

		/// <returns>true if a non-blank value was set, false if no value was set.</returns>
		protected static bool SetCalculatorPropertyForAgencyRates<T, Y>(Dictionary<T, string> calculatorValues, Y calculator, params (T, string)[] columnsAndInstanceNames)
			where Y : Calculator
			where T : Enum
			=> SetCalculatorProperty(calculatorValues, calculator, true, columnsAndInstanceNames);

		static object ConvertToType(Type currentValue, string stringData)
		{
			try
			{
				var typeConverter = TypeDescriptor.GetConverter(currentValue);
				return typeConverter.ConvertFromString(stringData);
			}
			catch (Exception ex) when (ex is FormatException || ex is ZTypeValueException)
			{
				throw new FormatConversionException(ex, stringData);
			}
		}

		protected static bool HasContent(Dictionary<TColumn, string> directValues, TColumn column)
			=> directValues.TryGetValue(column, out var columnContent) && !columnContent.IsNullOrEmpty();

		#endregion
	}
}
