using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	// Tested functionally through US Customs subclass
	public abstract class ClassificationDataLoad<T> : DataLoadWithFlexibleColumns where T : BaseCusClassification
	{
		protected ClassificationDataLoad()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		protected IEnumerable<string> FieldNames
		{
			get
			{
				if (fieldNames == null)
				{
					fieldNames = new List<string>();
					fieldNames.Add("Code");
					fieldNames.Add("Type");
					fieldNames.Add("Description");
					fieldNames.Add("Tariff");
					fieldNames.AddRange(GetCountrySpecificFieldNames());
				}
				return fieldNames;
			}
		}
		List<string> fieldNames;

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", FieldNames); }
		}

		int linesImported;
		protected override sealed void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				disposables = new List<IDisposable>();

				#region SuppressResourceStringsCheckRegion

				var data = GetClassificationDataToLoad();
				PopulatePartsDataToLoad(line, data);
				var lookupCode = data.Code;
				var classificationType = data.Type;
				var description = data.Description;
				var tariff = data.Tariff;

				#endregion

				if (!IsClassificationTypeValid(classificationType))
				{
					DisplayInvalidClassificationTypeMessage();
				}
				else
				{
					if (description.IsEmpty)
					{
						description = GetTariffDescription(tariff, classificationType);
					}

					T classification = LoadClassificationIfItExists(lookupCode, classificationType);

					if (classification == null)
					{
						classification = Factory.New<T>();
						SetValue(classification.CC_LookupCodeInfo, lookupCode);
						SetValue(classification.CC_DescriptionInfo, description);
						SetValue(classification.CC_ClassificationTypeInfo, classificationType);
						SetValue(classification.CC_TariffNumInfo, tariff);
						SetupCusClassificationSetterSuspenderIfNeeded(classification);
						ProcessCountrySpecificData(data, classification);
						classification.Validation.ValidateAll();
						if (classification.HasErrors)
						{
							DisplayExcludedRecordMessageCore(classification, Res.GetString("60c62d64-4d5a-4bbf-b520-43a9c8641458", ": Record Excluded - Tariff values provided are not valid"));
							classification.Delete();
						}
						else
						{
							RunCounters.RecsCreated++;
							RunCounters.RecsToUpdate++;
						}
					}
					else
					{
						DisplayExcludedRecordMessage(Res.GetString("3ab48179-e424-4b6e-9406-115feee14a80", ": Record Excluded - Classification '{0}' already exists", lookupCode));
					}
				}

				linesImported++;
				if (linesImported % 1000 == 0)
				{
					SaveAndRenewFactory();
				}
			}
			finally
			{
				disposables.ForEach(d => d.Dispose());
				disposables = null;
			}
		}
		List<IDisposable> disposables;

		protected void SetValue(ZPropertyInfo propertyInfo, ZString propertyValue)
		{
			propertyInfo.Value = propertyValue.Left(propertyInfo.MaxLength);
		}

		protected void AddToDisposableList(IDisposable disposable)
		{
			if (disposable != null)
			{
				disposables.Add(disposable);
			}
		}

		protected void SetupCusClassificationSetterSuspenderIfNeeded(BaseCusClassification classification)
		{
			var disposable = GetCusClassificationPropertiesToSuspendSetting(classification);
			if (disposable != null)
			{
				AddToDisposableList(disposable);
			}
		}

		protected virtual IDisposable GetCusClassificationPropertiesToSuspendSetting(BaseCusClassification classification)
		{
			return null;
		}

		protected virtual bool IsClassificationTypeValid(string type)
		{
			return type.Length <= BaseCusClassification.Schema.CC_ClassificationTypeMaxLength;
		}

		protected virtual void DisplayInvalidClassificationTypeMessage()
		{
			DisplayExcludedRecordMessage(Res.GetString("c227d130-8aed-4afe-ba16-ebfd777fe378", ": Record Excluded - Invalid classification type"));
		}

		/// <summary>
		/// If you want to use the boring default behavior of MasterFiles.Biz, override this and just call DisplayExcludedRecordMessage(defaultMessageToShow)
		/// </summary>
		protected virtual void DisplayExcludedRecordMessageCore(T c, string defaultMessageToShow)
		{
			ZStringBuilder errors = new ZStringBuilder();
			IEnumerable<ZPropertyInfo> infosToCheck = GetMainClassificationFieldsUponWhichToRunFriendlyValidation(c);

			foreach (ZPropertyInfo propInfo in infosToCheck)
			{
				if (propInfo.HasMessageErrors() || propInfo.HasErrors())
				{
					errors.Append(GetNotificationsAsOneString(propInfo.Notifications));
				}
			}

			DisplayExcludedRecordMessage(": " + errors.ToStringWithDelimiterBetweenAppends(" "));
		}

		/// <summary>
		/// This will give the main shared properties to validate, and will call GetCountrySpecificClassificationfieldsUponWhichToRunFriendlyValidation() to get extra country-specific ones
		/// </summary>
		protected virtual IEnumerable<ZPropertyInfo> GetMainClassificationFieldsUponWhichToRunFriendlyValidation(T classification)
		{
			ZPropertyInfo[] coreInfosArray = new ZPropertyInfo[] { classification.CC_FormattedTariffNumInfo, classification.CC_ClassificationTypeInfo, classification.CC_DescriptionInfo };
			List<ZPropertyInfo> coreInfosList = new List<ZPropertyInfo>();
			coreInfosList.AddRange(coreInfosArray);
			coreInfosList.AddRange(GetCountrySpecificClassificationFieldsUponWhichToRunFriendlyValidation(classification));
			return coreInfosList;
		}

		/// <summary>
		/// Override this to return a list of ZPropertyInfos that are defined in your own counry, e.g. classification.CC_ProcedureCodeInfo for GB/EU.
		/// </summary>
		protected virtual IEnumerable<ZPropertyInfo> GetCountrySpecificClassificationFieldsUponWhichToRunFriendlyValidation(T classification)
		{
			return Array.Empty<ZPropertyInfo>();
		}

		string GetNotificationsAsOneString(IEnumerable<INotification> warnings)
		{
			ZStringBuilder errors = new ZStringBuilder();
			foreach (INotification note in warnings)
			{
				errors.Append(note.Message);
			}
			return errors.ToStringWithDelimiterBetweenAppends("; ");
		}

		protected abstract T LoadClassificationIfItExists(ZString lookupCode, ZString type);
		protected abstract ZString GetTariffDescription(ZString tariff, ZString type);
		protected abstract IEnumerable<string> GetCountrySpecificFieldNames();
		protected abstract void ProcessCountrySpecificData(ClassificationDataToLoad dataToLoad, T classification);

		protected virtual ClassificationDataToLoad GetClassificationDataToLoad()
		{
			return new ClassificationDataToLoad();
		}

		void PopulatePartsDataToLoad(OCsvLine line, ClassificationDataToLoad dataToLoad)
		{
			foreach (var fieldName in FieldNamesMatchingColumn)
			{
				PopulateClassificationDataForProperty(line, dataToLoad, fieldName);
			}
		}

		IEnumerable<string> FieldNamesMatchingColumn => fieldNamesMatchingColumn ?? (fieldNamesMatchingColumn = FieldNames.Where(f => HasColumn(f)).ToArray());
		IEnumerable<string> fieldNamesMatchingColumn;

		protected void PopulateClassificationDataForProperty(OCsvLine line, ClassificationDataToLoad dataToLoad, string fieldName)
		{
			var propertyInfo = dataToLoad.GetType().GetProperty(fieldName);
			if (propertyInfo != null)
			{
				var value = GetValue(line, fieldName, propertyInfo.PropertyType);
				propertyInfo.SetValue(dataToLoad, value);
			}
		}
	}
}
