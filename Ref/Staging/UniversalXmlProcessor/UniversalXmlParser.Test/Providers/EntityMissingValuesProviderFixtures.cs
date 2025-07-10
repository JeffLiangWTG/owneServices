using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using NUnit.Framework;
using Unity;
using Unity.Resolution;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Providers
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class EntityMissingValuesProviderFixtures : BaseUnitTestFixture
	{
		#region Member Varieables

		readonly Dictionary<string, Type> refDataTypes = new Dictionary<string, Type>
		{
			{ "ZZ4", typeof(RefCarrierCode) },
			{ "ZZG", typeof(RefCarrierCodeAttribute) },
			{ "ZZQ", typeof(RefCarrierVesselPivot) },
			{ "ZZT", typeof(RefCusApplicability) },
			{ "ZZD", typeof(RefCusCodeList) },
			{ "ZZE", typeof(RefCusCodeListAttribute) },
			{ "ZXE", typeof(RefCusCodeListAttributeName) },
			{ "ZZU", typeof(RefCusCodeOrAttributeTransportMode) },
			{ "ZZK", typeof(RefCusCodeType) },
			{ "ZX1", typeof(RefCusCondition) },
			{ "ZX2", typeof(RefCusConditionType) },
			{ "ZX3", typeof(RefCusConditionValue) },
			{ "ZX4", typeof(RefCusConditionValueType) },
			{ "ZZC", typeof(RefCusExcludedTradeGroup) },
			{ "ZZM", typeof(RefCusMap) },
			{ "ZZP", typeof(RefCusMapType) },
			{ "ZZ5", typeof(RefCusNomenclatureGroup) },
			{ "ZZL", typeof(RefCusNomenclatureGroupNote) },
			{ "ZZ9", typeof(RefCusNomenclatureGroupType) },
			{ "ZX8", typeof(RefCusNomenclatureLanguage) },
			{ "ZZS", typeof(RefCusPreference) },
			{ "ZX9", typeof(RefCusPreferenceLanguage) },
			{ "ZZ6", typeof(RefCusProcedure) },
			{ "ZZ2", typeof(RefCusRate) },
			{ "ZY1", typeof(RefCusRateCode) },
			{ "ZZR", typeof(RefCusRateType) },
			{ "ZZ1", typeof(RefCusTariff) },
			{ "ZZ3", typeof(RefCusTariffAttribute) },
			{ "ZX7", typeof(RefCusTariffLanguage) },
			{ "ZZW", typeof(RefCusTariffNationalCode) },
			{ "ZZH", typeof(RefCusTariffRelationship) },
			{ "ZZI", typeof(RefCusTariffType) },
			{ "ZZ8", typeof(RefCusTariffUOM) },
			{ "ZZF", typeof(RefCusTaxOrFee) },
			{ "ZZA", typeof(RefCusTradeGroup) },
			{ "ZZB", typeof(RefCusTradeGroupCountry) },
			{ "ZX5", typeof(RefCusVATApplicability) },
			{ "ZZN", typeof(RefExchangeRateZZ) },
			{ "ZZO", typeof(RefVesselZZ) },
			{ "RCL", typeof(RefComplianceList) },
			{ "ZZJ", typeof(RefCusConfiguration) },
			{ "STL", typeof(RefStlScript) },
			{ "ZRC", typeof(RefSysConfig) },
			{ "R4", typeof(RefTimeZoneRule) }
		};

		#endregion

		[Test]
		public void TestSetPrimaryKeyValues()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			using var stagingRepository = new StagingRepository(TestConnectionString.GetAdmin(dbName));
			using var valuesProvider = new EntityValuesProvider(TestConnectionString.GetAdmin(dbName));

			var entityTypeHelper = Container.Resolve<IEntityTypeHelper>();
			var missingValueProvider = Container.Resolve<IEntityMissingValuesProvider>(new ParameterOverride("entityValuesProvider", valuesProvider));

			foreach (var refDataType in refDataTypes)
			{
				var instance = Activator.CreateInstance(refDataType.Value);

				var primaryKeyProperty = entityTypeHelper.GetPrimaryKeyProperty(refDataType.Value);
				Assert.AreEqual(Guid.Empty, primaryKeyProperty.GetValue(instance));

				missingValueProvider.SetPrimaryKey(instance);
				Assert.AreNotEqual(Guid.Empty, primaryKeyProperty.GetValue(instance));
			}
		}

		[Test]
		public void TestFillOutMissingValues()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			using var stagingRepository = new StagingRepository(TestConnectionString.GetAdmin(dbName));
			using var valuesProvider = new EntityValuesProvider(TestConnectionString.GetAdmin(dbName));
			var entityTypeHelper = Container.Resolve<IEntityTypeHelper>();
			var missingValueProvider = Container.Resolve<IEntityMissingValuesProvider>(new ParameterOverride("entityValuesProvider", valuesProvider));

			var excludedTypes = new[] { typeof(Guid) };

			foreach (var refDataType in refDataTypes)
			{
				var dateTimeNow = DateTime.Now;
				var instance = Activator.CreateInstance(refDataType.Value);

				missingValueProvider.SetPrimaryKey(instance);
				missingValueProvider.FillOutMissingValues(instance);

				var properties = entityTypeHelper.GetProperties(refDataType.Value);
				foreach (var propertyInfo in properties)
				{
					var type = propertyInfo.PropertyType;
					var propertyName = propertyInfo.Name;

					if (!excludedTypes.Contains(type) && type.IsSystemType())
					{
						var defaultValue = valuesProvider.GetDefaultValue(refDataType.Value.Name, propertyName);
						var value = propertyInfo.GetValue(instance);

						var isNullable = type.IsNullable();
						if (isNullable)
						{
							type = Nullable.GetUnderlyingType(type);
						}
						if (type.IsStringType())
						{
							if (defaultValue == null && !valuesProvider.IsNullable(refDataType.Value.Name, propertyName))
							{
								defaultValue = string.Empty;
							}
						}
						else if (type.IsIntegerType())
						{
							if (defaultValue == null)
							{
								defaultValue = 0;
							}
						}
						else if (type.IsBoolType() && isNullable)
						{
							defaultValue = true;
						}
						else if (type.IsBoolType())
						{
							defaultValue = false;
						}
						else if (type.IsDateTime())
						{
							if (defaultValue != null)
							{
								defaultValue = DateTime.Parse(defaultValue.ToString(), CultureInfo.CurrentCulture);
							}
							else if (!isNullable)
							{
								var diff = (DateTime)value - dateTimeNow;
								Assert.IsTrue(Math.Abs(diff.TotalMilliseconds) < 1e3);
								continue;
							}
						}
						else if (type.IsNumericType())
						{
							if (defaultValue == null)
							{
								defaultValue = 0.0f;
							}
						}
						Assert.AreEqual(defaultValue, value, $"{propertyName} default value set incorrectly.");
					}
				}
			}
		}
	}
}
