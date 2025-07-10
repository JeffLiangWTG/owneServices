using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.OData.ModelBuilder;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	public class ModelConfigFixture
	{
		[Test]
		public void RegisterModelsTest()
		{
			var loadedTypes = new HashSet<string>();
			var missingTypes = new HashSet<string>();
			foreach (var entitySet in builder.EntitySets)
			{
				var schemaPackage = ", CargoWise.RefDbRepo.Staging.Schema_New";
				var type = Type.GetType(entitySet.ClrType.FullName + schemaPackage);
				loadedTypes.Add(entitySet.ClrType.Name);
				if (missingTypes.Contains(entitySet.ClrType.Name))
				{
					missingTypes.Remove(entitySet.ClrType.Name);
				}
				foreach (var item in type.GetProperties().Where(x => (!x.PropertyType.Namespace.StartsWith("System") || x.PropertyType.IsGenericType)
				&& !x.CustomAttributes.Select(y => y.AttributeType == typeof(NotMappedAttribute)).FirstOrDefault()
					))
				{
					if (item.PropertyType.IsGenericType)
					{
						var itemType = item.PropertyType;
						if (itemType.GetGenericTypeDefinition() == typeof(ICollection<>))
						{
							var generic = item.PropertyType;
							var requiredType = generic.GetGenericArguments()[0].Name;
							if (!loadedTypes.Contains(requiredType))
							{
								missingTypes.Add(requiredType);
							}
						}
					}
					else
					{
						var requiredType = item.PropertyType.Name;
						if (!loadedTypes.Contains(requiredType))
						{
							missingTypes.Add(requiredType);
						}
					}
				}
			}
			var missingTypesResult = missingTypes.Except(new string[]
			{
				"RefCusRateApplicability",
				"RefCusConditionApplicability",
				"RefCusRateWithoutApplicability",
				"RefCusConditionWithoutApplicability",
				"QRTZ_TRIGGERS",
				"Geometry",
			});
			Assert.That(!missingTypesResult.Any(), $@"Missing Types in ModelConfig.cs ({string.Join(",", missingTypesResult)})");
		}

		[SetUp]
		public void Setup()
		{
			builder = new ODataConventionModelBuilder();
			ModelConfig.RegisterModels(builder);
		}

		ODataConventionModelBuilder builder;
	}
}
