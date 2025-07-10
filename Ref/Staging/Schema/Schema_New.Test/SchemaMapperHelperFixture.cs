using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class SchemaMapperHelperFixture
	{
		[TestCaseSource(nameof(GetMapperAndRelatedClassType))]
		public void ValidatedPropertiesInMapper_NoLost(ISchemaMapper schemaMapper, Type classType)
		{
			var properties = classType.GetProperties()
				.Where(o => (!o.PropertyType.IsClass || o.PropertyType == typeof(string)) &&
							o.PropertyType != typeof(Guid) &&
							o.PropertyType != typeof(Nullable<Guid>))
				.Where(p => !typeof(ICollection).IsAssignableFrom(p.PropertyType)
														&& !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
				.Select(p => p.Name);

			var transformProperties = schemaMapper.PropertyMappers.Select(x => x.originalProperty);

			var nonTransformProperties = properties.Except(transformProperties);
			Assert.That(!nonTransformProperties.Any(), $"In {classType.Name}, should add into SchemaMapper.PropertyMappers:\r\n{string.Join(",", nonTransformProperties)}");
		}

		[TestCaseSource(nameof(GetMapperAndRelatedClassType))]
		public void ValidatePropertiesInMapper_Matched(ISchemaMapper schemaMapper, Type classType)
		{
			var properties = classType.GetProperties().Select(p => p.Name).ToHashSet();

			Assert.Multiple(() =>
			{
				foreach (var propertyGroup in schemaMapper.PropertyMappers)
				{
					Assert.That(properties.Contains(propertyGroup.originalProperty), $"{propertyGroup.originalProperty} should IN {classType.Name}");

					Assert.That(propertyGroup.originalProperty.Substring(propertyGroup.originalProperty.Length - 3),
						Is.EqualTo(propertyGroup.transformedProperty.Substring(propertyGroup.transformedProperty.Length - 3)),
						$"In {schemaMapper.GetType().Name}, {propertyGroup.originalProperty} should NOT be transformed to {propertyGroup.transformedProperty}");
				}
			});
		}

		[Test]
		public void GetSchemaMappers()
		{
			var types = Assembly.LoadFrom("CargoWise.RefDbRepo.Staging.Schema_New.dll").GetTypes();
			var mappers = types.Where(x => x.GetInterface(nameof(ISchemaMapper)) != null).Select(x => x.Name);
			var mappersFromHelper = SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup()
				.Union(SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup()).Select(x => x.GetType().Name).Distinct();
			CollectionAssert.AreEquivalent(mappers, mappersFromHelper);
		}

		[TestCaseSource(nameof(GetTablePrefixMapperTestCase))]
		public (string, string) GetTablePrefixMappers(ISchemaMapper schemaMapper)
		{
			return schemaMapper.TablePrefixMapper;
		}

		[TestCaseSource(nameof(GetNameMapperTestCase))]
		public (string, string) GetNameMappers(ISchemaMapper schemaMapper)
		{
			return schemaMapper.NameMapper;
		}

		[Test]
		public void SchemaMapperCount()
		{
			var rateAndAppGroups = SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup();
			Assert.That(rateAndAppGroups.Length, Is.EqualTo(4));
			var condAndAppGroups = SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup();
			Assert.That(condAndAppGroups.Length, Is.EqualTo(5));
		}

		[Test]
		public void GetSchemaMappers_RateWithoutApp()
		{
			var mapper = SchemaMapperHelper.RateWithoutApplicabilityNameMapper;
			Assert.That(mapper.originalName, Is.EqualTo("RefCusRate"));
			Assert.That(mapper.transformedName, Is.EqualTo("RefCusRateWithoutApplicability"));
		}

		[Test]
		public void GetSchemaMappers_CondWithoutApp()
		{
			var mapper = SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper;
			Assert.That(mapper.originalName, Is.EqualTo("RefCusCondition"));
			Assert.That(mapper.transformedName, Is.EqualTo("RefCusConditionWithoutApplicability"));
		}

		static IEnumerable GetMapperAndRelatedClassType()
		{
			var types = Assembly.LoadFrom("CargoWise.RefDbRepo.Staging.Schema_New.dll").GetTypes();
			foreach (var mapper in types.Where(x => x.GetInterface(nameof(ISchemaMapper)) != null))
			{
				var mapperInstance = (ISchemaMapper)Activator.CreateInstance(mapper);
				var classType = types.Single(x => x.Name == mapperInstance.NameMapper.originalName);
				yield return new TestCaseData(mapperInstance, classType);
			}
		}

		static IEnumerable GetTablePrefixMapperTestCase()
		{
			yield return new TestCaseData(new TransformRefCusApplicabilityToRefCusConditionApplicability())
			{
				ExpectedResult = ("ZZT", "S07")
			};
			yield return new TestCaseData(new TransformRefCusApplicabilityToRefCusRateApplicability())
			{
				ExpectedResult = ("ZZT", "S01")
			};
			yield return new TestCaseData(new TransformRefCusConditionLanguageToRefCusConditionApplicabilityLanguage())
			{
				ExpectedResult = ("ZXJ", "S09")
			};
			yield return new TestCaseData(new TransformRefCusConditionToRefCusConditionApplicability())
			{
				ExpectedResult = ("ZX1", "S07")
			};
			yield return new TestCaseData(new TransformRefCusConditionValueToRefCusConditionApplicabilityValue())
			{
				ExpectedResult = ("ZX3", "S08")
			};
			yield return new TestCaseData(new TransformRefCusExcludedTradeGroupToRefCusExcludedTradeGroupNew())
			{
				ExpectedResult = ("ZZC", "S03")
			};
			yield return new TestCaseData(new TransformRefCusRateToRefCusRateApplicability())
			{
				ExpectedResult = ("ZZ2", "S01")
			};
			yield return new TestCaseData(new TransformRefCusRateUOMToRefCusRateApplicabilityUOM())
			{
				ExpectedResult = ("ZXG", "S02")
			};
		}

		static IEnumerable GetNameMapperTestCase()
		{
			yield return new TestCaseData(new TransformRefCusApplicabilityToRefCusConditionApplicability())
			{
				ExpectedResult = (nameof(RefCusApplicability), nameof(RefCusConditionApplicability))
			};
			yield return new TestCaseData(new TransformRefCusApplicabilityToRefCusRateApplicability())
			{
				ExpectedResult = (nameof(RefCusApplicability), nameof(RefCusRateApplicability))
			};
			yield return new TestCaseData(new TransformRefCusConditionLanguageToRefCusConditionApplicabilityLanguage())
			{
				ExpectedResult = (nameof(RefCusConditionLanguage), nameof(RefCusConditionApplicabilityLanguage))
			};
			yield return new TestCaseData(new TransformRefCusConditionToRefCusConditionApplicability())
			{
				ExpectedResult = (nameof(RefCusCondition), nameof(RefCusConditionApplicability))
			};
			yield return new TestCaseData(new TransformRefCusConditionValueToRefCusConditionApplicabilityValue())
			{
				ExpectedResult = (nameof(RefCusConditionValue), nameof(RefCusConditionApplicabilityValue))
			};
			yield return new TestCaseData(new TransformRefCusExcludedTradeGroupToRefCusExcludedTradeGroupNew())
			{
				ExpectedResult = (nameof(RefCusExcludedTradeGroup), nameof(RefCusExcludedTradeGroupNew))
			};
			yield return new TestCaseData(new TransformRefCusRateToRefCusRateApplicability())
			{
				ExpectedResult = (nameof(RefCusRate), nameof(RefCusRateApplicability))
			};
			yield return new TestCaseData(new TransformRefCusRateUOMToRefCusRateApplicabilityUOM())
			{
				ExpectedResult = (nameof(RefCusRateUOM), nameof(RefCusRateApplicabilityUOM))
			};
		}
	}
}
