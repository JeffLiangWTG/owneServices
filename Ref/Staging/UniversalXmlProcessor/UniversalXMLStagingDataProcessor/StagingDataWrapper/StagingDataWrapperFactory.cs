using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class StagingDataWrapperFactory : IStagingDataWrapperFactory
	{
		public StagingDataWrapperFactory(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, IStagingDataProvider stagingProvider, IOverlappingCalculator overlappingCalculator)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(stagingProvider, nameof(stagingProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(overlappingCalculator, nameof(overlappingCalculator));

			this.safeDataProvider = safeDataProvider;
			this.metadataProvider = metadataProvider;
			this.stagingProvider = stagingProvider;
			this.overlappingCalculator = overlappingCalculator;
		}

		readonly ISafeDataProvider safeDataProvider;
		readonly IStagingDataProvider stagingProvider;
		readonly IMetadataProvider metadataProvider;
		readonly IOverlappingCalculator overlappingCalculator;

		public IEnumerable<Type> GetExpriableKeyRelatedStagingEntityTypes(Type stagingType)
		{
			foreach (var relatedEntityType in stagingProvider.GetRelatedEntityTypes(stagingType.Name, metadataProvider))
			{
				var relatedSafeEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedEntityType.Name);
				if (metadataProvider.IsKeyProperty(stagingType.Name, relatedEntityType.Name) && DataProviderHelper.IsExpirableType(relatedSafeEntityType))
				{
					yield return relatedEntityType;
				}
			}
		}

		public IEnumerable<IStagingDataWrapper> CreateWrapper(object stagingObject)
		{
			var wrapperList = new List<IStagingDataWrapper>();
			var stagingType = stagingObject.GetEntityType();
			foreach (var relatedEntityType in GetExpriableKeyRelatedStagingEntityTypes(stagingType))
			{
				var relatedEntities = GetRelatedEntities(stagingObject, relatedEntityType).SelectMany(x => CreateWrapper(x)).ToArray();
				Array.ForEach(relatedEntities, x =>
				{
					var wrapper = new StagingDataWrapper(metadataProvider, stagingProvider, overlappingCalculator, this, stagingObject);
					wrapper.SetRelatedEntities(relatedEntityType, new[] { x });
					wrapperList.Add(wrapper);
				});
			}
			if (wrapperList.Count == 0)
			{
				wrapperList.Add(new StagingDataWrapper(metadataProvider, stagingProvider, overlappingCalculator, this, stagingObject));
			}
			var result = wrapperList.ToArray();
			var safeRelatedTypeAndNKProperties = safeDataProvider.GetRelatedTypeAndNKPropertyNames(stagingType.Name, metadataProvider);
			foreach (var safeRelatedTypeAndNKProperty in safeRelatedTypeAndNKProperties)
			{
				var relatedEntityType = safeRelatedTypeAndNKProperty.Item1;
				var nkPropertiesAndValues = (object)safeRelatedTypeAndNKProperty.Item3.Select(x => (x, stagingObject.GetValue(x))).ToArray();
				var relatedEntity = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetRelatedEntity), relatedEntityType, nkPropertiesAndValues);
				if (relatedEntity != null)
				{
					var relatedPKPropertyName = relatedEntityType.GetPKPropertyName();
					var relatedEntityPK = relatedEntity.GetValue(relatedPKPropertyName);
					Array.ForEach(result, x => x.SetWrapperValue(safeRelatedTypeAndNKProperty.Item2, relatedEntityPK));
				}
			}
			return result;
		}

		IEnumerable<object> GetRelatedEntities(object stagingObject, Type relatedType)
		{
			Argument.NotNull(stagingObject, nameof(stagingObject));
			Argument.NotNull(relatedType, nameof(relatedType));
			var result = ((IEnumerable)stagingProvider.InvokeGenericMethod(nameof(IStagingDataProvider.GetRelatedEntities),
				new[] { stagingObject.GetEntityType(), relatedType }, stagingObject))?.Cast<object>();
			return result;
		}
	}
}
