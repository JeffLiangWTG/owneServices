using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class SafeDataClientFixture
	{
		[Test]
		public void CheckTypeConsistentWithModelConfigRegistrationInUpdateService()
		{
			var dataSetsDictionary = DataSetStructureProvider.StructuredDataSets.ToDictionary(k => k[0], v => v.Skip(1));
			var headerTables = dataSetsDictionary.Keys.OrderBy(x => x).Except(ExclusionList);
			var childTables = dataSetsDictionary.Values.SelectMany(x => x).OrderBy(x => x).Except(ExclusionList);
			var batchDeleteList = new List<string>();
			var batchInActiveList = new List<string>();
			var batchExpireList = new List<string>();
			var forceDeleteList = new List<string>();
			var getCreatedBetweenList = new List<string>();
			var getLastestCreatedTimeUTCList = new List<string>();
			var getLatestUpdatedTimeUTCList = new List<string>();
			var getModifiedBetweenList = new List<string>();
			var getWithOptimizedExpandList = new List<string>();

			var type = typeof(Default.ExtensionMethods);
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
			foreach (MethodInfo method in methods)
			{
				var parameterTypeName = method.GetParameters()[0].ParameterType.GetGenericArguments().FirstOrDefault().Name;
				if (!string.IsNullOrEmpty(parameterTypeName) && !parameterTypeName.EndsWith("View", StringComparison.OrdinalIgnoreCase) //Exclusion, revisit in WI00722940
					&& !ExclusionList.Contains(parameterTypeName))
				{
					if (method.Name is "BatchDelete")
					{
						batchDeleteList.Add(parameterTypeName);
					}
					if (method.Name == "BatchInActive")
					{
						batchInActiveList.Add(parameterTypeName);
					}
					if (method.Name == "BatchExpire")
					{
						batchExpireList.Add(parameterTypeName);
					}
					if (method.Name == "ForceDelete")
					{
						forceDeleteList.Add(parameterTypeName);
					}
					if (method.Name == "GetCreatedBetween")
					{
						getCreatedBetweenList.Add(parameterTypeName);
					}
					if (method.Name == "GetLastestCreatedTimeUTC")
					{
						getLastestCreatedTimeUTCList.Add(parameterTypeName);
					}
					if (method.Name == "GetLatestUpdatedTimeUTC")
					{
						getLatestUpdatedTimeUTCList.Add(parameterTypeName);
					}
					if (method.Name == "GetModifiedBetween")
					{
						getModifiedBetweenList.Add(parameterTypeName);
					}
					if (method.Name == "GetWithOptimizedExpand")
					{
						getWithOptimizedExpandList.Add(parameterTypeName);
					}
				}
			}

			Assert.True(headerTables.SequenceEqual(forceDeleteList.OrderBy(x => x)), "Header table should enable ForceDelete by default");
			Assert.True(headerTables.SequenceEqual(getCreatedBetweenList.OrderBy(x => x)), "Header table should enable GetCreatedBetween by default");
			Assert.True(headerTables.SequenceEqual(getLastestCreatedTimeUTCList.OrderBy(x => x)), "Header table should enable GetLastestCreatedTimeUTC by default");
			Assert.True(headerTables.SequenceEqual(getLatestUpdatedTimeUTCList.OrderBy(x => x)), "Header table should enable GetLatestUpdatedTimeUTC by default");
			Assert.True(headerTables.SequenceEqual(getModifiedBetweenList.OrderBy(x => x)), "Header table should enable GetModifiedBetween by default");
			Assert.True(headerTables.SequenceEqual(getWithOptimizedExpandList.Except(WithOptimizedExpandExclusionList).OrderBy(x => x)), "Header table should enable GetWithOptimizedExpand by default");
			Assert.True(headerTables.Concat(childTables).OrderBy(x => x).SequenceEqual(batchDeleteList.OrderBy(x => x)), "All business tables should enable BatchDelete by default");
			Assert.True(headerTables.Concat(childTables).OrderBy(x => x).SequenceEqual(batchInActiveList.OrderBy(x => x)), "All business tables should enable BatchInActive by default");
			Assert.True(headerTables.Concat(childTables).OrderBy(x => x).SequenceEqual(batchExpireList.OrderBy(x => x)), "All business tables should enable BatchExpire by default");
		}

		[Test]
		public void CheckSerializedGeometryPropertyIsRenamed()
		{
			var errorBuilder = new StringBuilder();
			var entitiesWithGeometry = typeof(RefCusTariff).Assembly.GetTypes()
				.Where(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => p.PropertyType == typeof(SerializedGeometry)));
			foreach (var entity in entitiesWithGeometry)
			{
				var properties = entity.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var geoProperties = properties.Where(x => x.PropertyType == typeof(SerializedGeometry));
				foreach (var geoProperty in geoProperties)
				{
					if (geoProperty.Name.EndsWith("_WKT", StringComparison.OrdinalIgnoreCase))
					{
						errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{geoProperty.Name} of {entity.Name} should be renamed to {geoProperty.Name.Substring(0, geoProperty.Name.Length - 4)}, please add it to Customization.GeometryPropertyNameMappings");
					}
				}
			}
			Assert.That(string.IsNullOrEmpty(errorBuilder.ToString()), errorBuilder.ToString());
		}

		static readonly string[] ExclusionList = new[] {
			"RefCusTaxOrFee",	//This is header in ttinclude, but not enable dataset function, revisit in WI00722940
			"RefCusTariffRule", "RefCusTariffAttributeRule", "RefCusTariffRelationshipRule", "RefCusTariffUOMRule", "RefCusRateRule", "ClientRefDbVersionControl"	//This doesn't include in ttinclude, revisit in WI00722940
		};
		static readonly string[] WithOptimizedExpandExclusionList = new[] { "RefUNLOCOUtcOffset", "RefLocoMap", "RefUNLOCORelatedPort" };   //revisit in WI00722940
	}
}
