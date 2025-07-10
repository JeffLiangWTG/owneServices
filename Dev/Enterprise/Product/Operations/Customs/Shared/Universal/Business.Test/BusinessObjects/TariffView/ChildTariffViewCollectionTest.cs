using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ChildTariffViewCollection))]
	public class ChildTariffViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetNewCollection_FilterBusinessObjectDefaults()
		{
			var dataGroupingCode = Core.Constants.CountryCodes.Eritrea;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeEXC = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "EXC");
			var tariffTypeIMP = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "IMP");
			var tariffTypeRandom = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "LEV");
			Factory.Save();
			var collection = ChildTariffViewCollection.GetNewCollection(Factory, dataGroupingCode, "IMP", new ZDateTime(2019, 3, 15), Array.Empty<KeyValuePair<ZString, ZString>>());
			var tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			var effectiveDate = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.EffectiveDate + ":Property1"];
			AssertEquals("effectiveDate", new ZDateTime(2019, 3, 15), effectiveDate.Value);
			var tariffTypeCountry = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property1"];
			AssertEquals("tariffTypeCountry", (ZString)Core.Constants.CountryCodes.Eritrea, tariffTypeCountry.Value);
			var tariffTypeType = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property2"];
			AssertEquals("tariffTypeType", (ZString)"IMP", tariffTypeType.Value);
			collection = ChildTariffViewCollection.GetNewCollection(Factory, dataGroupingCode, ZString.Empty, ZDateTime.Invalid, Array.Empty<KeyValuePair<ZString, ZString>>());
			tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			AssertEquals("effectiveDate", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.EffectiveDate + ":Property1"));
			AssertEquals("tariffTypeCountry", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property1"));
			AssertEquals("tariffTypeType", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property2"));
		}

		public void TestGetNewCollection()
		{
			var dataGroupingCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeEXC = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "EXC");
			var tariffTypeIMP = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "IMP");
			var tariffTypeRandom = helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "LEV");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariffEXC = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff1 = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "1111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "2222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff3 = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "3333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff4 = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "4444", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff5 = helper.CreateTariff(dataGroupingCode, tariffTypeRandom.PK, "5555", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff6 = helper.CreateTariff(dataGroupingCode, tariffTypeEXC.PK, "6666", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddYears(-1));
			var tariffRelationship1 = helper.CreateTariffRelationship(tariff1.PK, tariffTypeIMP.PK, "08091998");
			var tariffRelationship2 = helper.CreateTariffRelationship(tariff2.PK, tariffTypeIMP.PK, "0809");
			var tariffRelationship3 = helper.CreateTariffRelationship(tariff3.PK, tariffTypeIMP.PK, "5454");
			var tariffRelationship4 = helper.CreateTariffRelationship(tariff4.PK, tariffTypeRandom.PK, "08091998");
			var tariffRelationship5 = helper.CreateTariffRelationship(tariff5.PK, tariffTypeIMP.PK, "08091998");
			var tariffRelationship6 = helper.CreateTariffRelationship(tariff6.PK, tariffTypeIMP.PK, "08091998");
			Factory.Save();
			CombineAssertions(() =>
			{
				var kvp = new KeyValuePair<ZString, ZString>("IMP", "08091998");
				var collection = ChildTariffViewCollection.GetNewCollection(Factory, dataGroupingCode, "EXC", ZDate.Today, new KeyValuePair<ZString, ZString>[] { kvp });
				var filter = collection.relationshipFilter;
				AssertEquals("Tariff 1111 should be a part of the collection, as it has the whole tariff code", true, tariff1.MatchesFilter(filter));
				AssertEquals("Tariff 2222 should be a part of the collection, as it has part of the tariff code", true, tariff2.MatchesFilter(filter));
				AssertEquals("Tariff 4444 shouldn't be a part of the collection, as the tariffType is not of type 'IMP'", false, tariff4.MatchesFilter(filter));
				AssertEquals("Tariff 5555 shouldn't be a part of the collection, as the relatedTariff is not of type 'EXC'", false, tariff5.MatchesFilter(filter));
				AssertEquals("Tariff 6666 shouldn't be a part of the collection, as it is out of date", false, tariff6.MatchesFilter(filter));
				AssertEquals("Tariff 3333 shouldn't be a part of the collection, as it does not have the correct tariff code ('5454')", false, tariff3.MatchesFilter(filter));
				collection = ChildTariffViewCollection.GetNewCollection(Factory, dataGroupingCode, "EXC", ZDate.Today, new KeyValuePair<ZString, ZString>[] { kvp, new KeyValuePair<ZString, ZString>("IMP", "54") });
				AssertEquals("Tariff 3333 should be a part of the collection, as the applicable tariffPair has code ('54')", false, tariff3.MatchesFilter(collection.relationshipFilter));
			}

			);
		}

		public void TestMandatoryAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var attrName1 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "ATT1", "Attribute Name 1", "TP1", Core.Constants.CountryCodes.China, "1P1");
			var attrName2 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "ATT2", "Attribute Name 2", "TP2", Core.Constants.CountryCodes.China, "2P1");

			var collection = new ChildTariffViewCollection(Factory, Core.Constants.CountryCodes.China, "1P1", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>() });

			CombineAssertions(() =>
			{
				AssertEquals("Should be readonly including Items.", true, collection.MandatoryAttributeNames.ReadOnly);
				AssertEquals("Should have the only on matched AttrName.", "ATT1", collection.MandatoryAttributeNames.Single().ZY6_Name);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ChildTariffViewCollection(Factory, "CN", "CIQ", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>("CN", "CIQ") });
	}
}
