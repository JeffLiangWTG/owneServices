using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.Schema_New;


namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class StagingDataWrapper : IStagingDataWrapper
	{
		public StagingDataWrapper(IMetadataProvider metadataProvider, IStagingDataProvider stagingDataProvider, IOverlappingCalculator overlappingCalculator, IStagingDataWrapperFactory factory, object stagingObject)
		{
			Argument.NotNull(stagingObject, nameof(stagingObject));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(stagingDataProvider, nameof(stagingDataProvider));
			Argument.NotNull(factory, nameof(factory));

			this.stagingObject = stagingObject;
			this.metadataProvider = metadataProvider;
			this.stagingDataProvider = stagingDataProvider;
			this.factory = factory;
			this.overlappingCalculator = overlappingCalculator;
			propertiesAndValues = new Dictionary<string, object>();
			relatedWrappersByType = new Dictionary<Type, IEnumerable<IStagingDataWrapper>>();
		}

		readonly object stagingObject;
		readonly IMetadataProvider metadataProvider;
		readonly IStagingDataProvider stagingDataProvider;
		readonly IStagingDataWrapperFactory factory;
		readonly IOverlappingCalculator overlappingCalculator;
		readonly Dictionary<string, object> propertiesAndValues;
		readonly Dictionary<Type, IEnumerable<IStagingDataWrapper>> relatedWrappersByType;

		public object GetWrapperValue(string propertyName)
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			if (propertiesAndValues.ContainsKey(propertyName))
			{
				return propertiesAndValues[propertyName];
			}
			return stagingObject.GetValue(propertyName);
		}

		public T GetWrapperValue<T>(string propertyName)
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			var result = GetWrapperValue(propertyName);
			return result == null ? default(T) : (T)result;
		}

		public void SetWrapperValue(string propertyName, object value)
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			propertiesAndValues.Add(propertyName, value);
		}

		public Type GetStagingType()
		{
			return stagingObject.GetEntityType();
		}

		public bool IsData => metadataProvider.IsData(GetStagingTypeName());

		public IEnumerable<Tuple<string, string>> GetRelatedEntityTypesAndFKs(bool includeExpirableKeys = false)
		{
			var stagingType = GetStagingType();
			var result = stagingDataProvider.GetRelatedEntityTypes(GetStagingTypeName(), metadataProvider)
				?.Where(x => includeExpirableKeys || !factory.GetExpriableKeyRelatedStagingEntityTypes(stagingType).Contains(x))
				?.Select(x => Tuple.Create(x.Name, stagingDataProvider.GetFKPropertyName(x, stagingType)));
			return result ?? Enumerable.Empty<Tuple<string, string>>();
		}

		public string GetStagingTypeName()
		{
			var result = GetStagingType().Name;
			return result;
		}

		public void SetRelatedEntities(Type relatedType, IEnumerable<IStagingDataWrapper> relatedWrappers)
		{
			Argument.NotNull(relatedType, nameof(relatedType));
			relatedWrappersByType.Add(relatedType, relatedWrappers);
		}

		public IEnumerable<IStagingDataWrapper> GetRelatedEntities(string relatedTypeName)
		{
			Argument.NotNullOrEmpty(relatedTypeName, nameof(relatedTypeName));
			var relatedType = stagingDataProvider.GetTypeFromTypeName(relatedTypeName);
			var result = Enumerable.Empty<IStagingDataWrapper>();
			if (!relatedWrappersByType.TryGetValue(relatedType, out result))
			{
				result = ((IEnumerable)stagingDataProvider.InvokeGenericMethod(nameof(IStagingDataProvider.GetRelatedEntities),
					new[] { stagingObject.GetEntityType(), relatedType }, stagingObject))?.Cast<object>()?.SelectMany(x => factory.CreateWrapper(x));
			}
			return result;
		}

		public DateTimeRange GetDateTimeRange()
		{
			var tablePrefix = GetStagingType().GetTablePrefix();
			var startDatePropertyName = $"{tablePrefix}_{Constants.StartDatePropertySuffix}";
			var endDatePropertyName = $"{tablePrefix}_{Constants.EndDatePropertySuffix}";
			var startDate = stagingObject.GetValue(startDatePropertyName);
			var endDate = stagingObject.GetValue(endDatePropertyName);
			if (startDate != null && endDate != null)
			{
				return new DateTimeRange(Convert.ToDateTime(startDate, CultureInfo.InvariantCulture), Convert.ToDateTime(endDate, CultureInfo.InvariantCulture));
			}
			return null;
		}

		public DateTime GetWrapperStartDateTime(string tablePrefix)
		{
			var startDatePropertyName = $"{tablePrefix}_{Constants.StartDatePropertySuffix}";
			var startDate = GetWrapperValue(startDatePropertyName);
			if (startDate != null)
			{
				return Convert.ToDateTime(startDate, CultureInfo.InvariantCulture);
			}
			return DateTime.MinValue;
		}

		public DateTime GetWrapperEndDateTime(string tablePrefix)
		{
			var endDatePropertyName = $"{tablePrefix}_{Constants.EndDatePropertySuffix}";
			var endDate = GetWrapperValue(endDatePropertyName);
			if (endDate != null)
			{
				return Convert.ToDateTime(endDate, CultureInfo.InvariantCulture);
			}
			return DateTime.MaxValue;
		}

		public Guid GetWrapperOriginalPK()
		{
			if (stagingObject is RefCusRateApplicability rateApp)
			{
				return rateApp.OriginalRefCusApplicabilityPK;
			}
			else if (stagingObject is RefCusConditionApplicability condApp)
			{
				return condApp.OriginalRefCusApplicabilityPK;
			}
			return stagingObject.GetPKValue();
		}
	}
}
