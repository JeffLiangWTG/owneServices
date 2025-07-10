using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Utilities
{
	[TestFixture]
	public class EntityTypeHelperFixtures : BaseUnitTestFixture
	{
		#region Member Variables

		readonly Dictionary<string, Type> refDataTypes = new Dictionary<string, Type>
		{
			{ "ZZ4", typeof(RefCarrierCode) },
			{ "ZZG", typeof(RefCarrierCodeAttribute) },
			{ "ZZQ", typeof(RefCarrierVesselPivot) },
			{ "ZZT", typeof(RefCusApplicability) },
			{ "ZZD", typeof(RefCusCodeList) },
			{ "ZZE", typeof(RefCusCodeListAttribute) },
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
			{ "R2", typeof(RefTimeZone) },
			{ "R4", typeof(RefTimeZoneRule) }
		};

		#endregion

		[Test]
		public void TestGetTableCode()
		{
			var helper = Container.Resolve<IEntityTypeHelper>();
			foreach (var refDatType in refDataTypes)
			{
				Assert.AreEqual(refDatType.Key, helper.GetTableCode(refDatType.Value));
			}
		}

		[Test]
		public void TestGetPrimaryKeyProperty()
		{
			var helper = Container.Resolve<IEntityTypeHelper>();
			foreach (var refDatType in refDataTypes)
			{
				var primaryKey = helper.GetPrimaryKeyProperty(refDatType.Value);
				Assert.IsNotNull(primaryKey);
				Assert.IsTrue(primaryKey.Name.StartsWith(refDatType.Key));
			}
		}

		[Test]
		public void TestGetProperty()
		{
			var helper = Container.Resolve<IEntityTypeHelper>();
			foreach (var refDatType in refDataTypes)
			{
				var properties = helper.GetProperties(refDatType.Value);
				foreach (var propertyInfo in properties)
				{
					var property = helper.GetProperty(refDatType.Value, propertyInfo.Name);
					Assert.IsNotNull(property);
				}
			}
		}

		[Test]
		public void TestGetForeignKeyProperty()
		{
			var helper = Container.Resolve<IEntityTypeHelper>();
			var childParentTableCodes = new Dictionary<string, string>
			{
				{ "ZZG", "ZZ4" },
				{ "ZZQ", "ZZO" },
				{ "ZZT", "ZZA" },
				{ "ZZD", "ZZK" },
				{ "ZZE", "ZZD" },
				{ "ZZU", "ZZD" },
				{ "ZX1", "ZZ1" },
				{ "R4", "R2" }
			};

			foreach (var childParentTableCode in childParentTableCodes)
			{
				var childType = refDataTypes[childParentTableCode.Key];
				var parentType = refDataTypes[childParentTableCode.Value];
				var fkProperty = helper.GetForeignKeyProperty(childType, helper.GetTableCode(childType), helper.GetTableCode(parentType));
				Assert.IsNotNull(fkProperty);
			}
		}
	}
}
