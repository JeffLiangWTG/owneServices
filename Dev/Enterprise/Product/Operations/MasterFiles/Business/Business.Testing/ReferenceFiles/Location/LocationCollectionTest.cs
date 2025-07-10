using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LocationCollection))]
	sealed class LocationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetDescriptionFromCode()
		{
			var collection = new LocationCollection(Factory);

			var description = collection.GetDescriptionFromCode(TestLOCO.Code);
			AssertEquals(description, "XXXXX Port");

			description = collection.GetDescriptionFromCode(TestZone.Code);
			AssertEquals(description, "XXXX Zone");

			description = collection.GetDescriptionFromCode(TestCountry.Code);
			AssertEquals(description, "XX Country");
		}

		public void TestGetEstimatedLoadCount()
		{
			try
			{
				Collection.GetEstimatedLoadCount(new ZQuery());
				Fail("Exception should have been thrown when calling GetEstimatedLoadCount without a Type");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("NotSupportedException Message", "GetEstimatedLoadCount must be provided a BizOType. Use override on LocationCollection that takes BizOType instead.", ex.Message);
			}

			var expCountryCount = Factory.GetDatabaseCount(typeof(RefCountry));
			AssertEquals("GetEstimatedLoadCount - RefCountry", expCountryCount, ((LocationCollection)Collection).GetEstimatedLoadCount(typeof(RefCountry), new ZQuery()));

			var expRegionCount = Factory.GetDatabaseCount(typeof(RefZoneHeader), new ZQuery());
			AssertEquals("GetEstimatedLoadCount - RefZoneHeader", expRegionCount, ((LocationCollection)Collection).GetEstimatedLoadCount(typeof(RefZoneHeader), new ZQuery()));

			var expUNLOCOCount = Factory.GetDatabaseCount(typeof(RefUNLOCO));
			AssertEquals("GetEstimatedLoadCount - RefUNLOCO", expUNLOCOCount, ((LocationCollection)Collection).GetEstimatedLoadCount(typeof(RefUNLOCO), new ZQuery()));
		}

		public void TestLoadUNLoco()
		{
			var locationCollection = new LocationCollection(Factory);
			var query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, "XXXXX");
			locationCollection.LoadUNLoco(query);
			AssertEquals("Count", 1, locationCollection.Count);
		}

		public void TestLoadZone()
		{
			var locationCollection = new LocationCollection(Factory);
			var query = new ZQuery(RefZoneHeaderSchema.FZ_Code, SQLComparisonOperator.Equal, "XXXX");
			locationCollection.LoadZone(query);
			AssertEquals("Count", 1, locationCollection.Count);

			locationCollection = new LocationCollection(Factory, false);
			query.AddToFilter(RefZoneHeaderSchema.FZ_Code, SQLComparisonOperator.Equal, "XXXX");
			locationCollection.LoadZone(query);
			AssertEquals("Nothing returned as regions not allowed", 0, locationCollection.Count);

			var requiredZoneTypes = new ZoneTypeList();
			requiredZoneTypes.Add(ZoneTypeCodeDescriptionPair.All);
			locationCollection = new LocationCollection(Factory, requiredZoneTypes);
			locationCollection.LoadZone(new ZQuery());
			AssertEquals("Count", 1, locationCollection.Count);
		}

		public void TestLoadCountry()
		{
			var locationCollection = new LocationCollection(Factory);
			var query = new ZQuery();
			query.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, "XX");
			locationCollection.LoadCountry(query);
			AssertEquals("Count", 1, locationCollection.Count);

			locationCollection = new LocationCollection(Factory, true, false);
			query.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, "XX");
			locationCollection.LoadCountry(query);
			AssertEquals("Nothing returned as countries not allowed", 0, locationCollection.Count);
		}

		public override void TestLoad()
		{
			var locationCollection = new LocationCollection(Factory);
			locationCollection.Load(new ZQuery());
			AssertEquals(0, locationCollection.Count);
		}

		public void TestPrimaryKeyFromCode()
		{
			var locationCollection = new LocationCollection(Factory);
			TestProvider = locationCollection;

			var unlocoPK = TestProvider.PrimaryKeyFromCode("XXXXX");
			AssertEquals("Correct PK", TestLOCO.PK, unlocoPK);

			var regionPK = TestProvider.PrimaryKeyFromCode("XXXX");
			AssertEquals("Correct PK", TestZone.PK, regionPK);

			var countryPK = TestProvider.PrimaryKeyFromCode("XX");
			AssertEquals("Correct PK", TestCountry.PK, countryPK);

			locationCollection = new LocationCollection(Factory, false);
			TestProvider = locationCollection;
			regionPK = TestProvider.PrimaryKeyFromCode("XXXX");
			AssertEquals("No PK as regions not allowed", ZGuid.Invalid, regionPK);

			locationCollection = new LocationCollection(Factory, zoneList);
			TestProvider = locationCollection;
			regionPK = TestProvider.PrimaryKeyFromCode("XXXX");
			AssertEquals("No PK as XXXX region is not allowed zone type", ZGuid.Invalid, regionPK);

			locationCollection = new LocationCollection(Factory, true, false);
			TestProvider = locationCollection;
			countryPK = TestProvider.PrimaryKeyFromCode("XX");
			AssertEquals("No PK as countries not allowed", ZGuid.Invalid, countryPK);
		}

		public void TestCodeFromPrimaryKey()
		{
			var locationCollection = new LocationCollection(Factory);
			TestProvider = locationCollection;

			ZString unlocoCode = TestProvider.CodeFromPrimaryKey(TestLOCO.PK);
			AssertEquals("Correct Code", unlocoCode, "XXXXX");

			ZString regionCode = TestProvider.CodeFromPrimaryKey(TestZone.PK);
			AssertEquals("Correct Code", regionCode, "XXXX");

			ZString countryCode = TestProvider.CodeFromPrimaryKey(TestCountry.PK);
			AssertEquals("Correct Code", countryCode, "XX");

			locationCollection = new LocationCollection(Factory, false);
			TestProvider = locationCollection;
			regionCode = TestProvider.CodeFromPrimaryKey(TestZone.PK);
			AssertEquals("No Code as regions are not allowed", regionCode, "");

			locationCollection = new LocationCollection(Factory, zoneList);
			TestProvider = locationCollection;
			regionCode = TestProvider.CodeFromPrimaryKey(TestZone.PK);
			AssertEquals("No code as XXXX region is not allowed zone type", string.Empty, regionCode);

			locationCollection = new LocationCollection(Factory, true, false);
			TestProvider = locationCollection;
			countryCode = TestProvider.CodeFromPrimaryKey(TestCountry.PK);
			AssertEquals("No Code as countries are not allowed", countryCode, "");
		}

		public void TestAutoCompleteOnCommit()
		{
			var locationCollection = new LocationCollection(Factory);
			AssertEquals(true, (((IFindBoxListProvider)locationCollection).AutoCompleteOnCommit));
		}

		public void TestDescriptionFromCode()
		{
			var locationCollection = new LocationCollection(Factory);
			TestProvider = locationCollection;

			var refUnlocoDescription = TestProvider.DescriptionFromCode(TestLOCO.RL_Code);
			AssertEquals("Description Code", "XXXXX Port", refUnlocoDescription);

			var internationalZoneDescription = TestProvider.DescriptionFromCode(TestZone.FZ_Code);
			AssertEquals("Description Code", "XXXX Zone", internationalZoneDescription);

			var countryDescription = TestProvider.DescriptionFromCode(TestCountry.RN_Code);
			AssertEquals("Description Code", "XX Country", countryDescription);

			AssertEquals("Australia", TestProvider.DescriptionFromCode("AU"));
			AssertNull("Should not be able to find anything", TestProvider.DescriptionFromCode("AU "));

			locationCollection = new LocationCollection(Factory, false);
			TestProvider = locationCollection;
			internationalZoneDescription = TestProvider.DescriptionFromCode(TestZone.FZ_Code);
			AssertNull("Description Code - regions not allowed so no description", internationalZoneDescription);

			locationCollection = new LocationCollection(Factory, zoneList);
			TestProvider = locationCollection;
			internationalZoneDescription = TestProvider.DescriptionFromCode(TestZone.FZ_Code);
			AssertNull("No description as XXXX zone is not allowed zone type", internationalZoneDescription);

			locationCollection = new LocationCollection(Factory, true, false);
			TestProvider = locationCollection;
			countryDescription = TestProvider.DescriptionFromCode(TestCountry.RN_Code);
			AssertNull("Description Code - countries not allowed so no description", countryDescription);
		}

		public void TestCodeFromDescription()
		{
			var locationCollection = new LocationCollection(Factory);
			var provider = (IFindBoxListProviderDescriptionEx)locationCollection;

			var lOCOCode = provider.CodeFromDescription(TestLOCO.Description);
			AssertEquals(TestLOCO.Code, lOCOCode);

			var regionCode = provider.CodeFromDescription(TestZone.Description);
			AssertEquals(TestZone.Code, regionCode);

			var countryCode = provider.CodeFromDescription(TestCountry.Description);
			AssertEquals(TestCountry.Code, countryCode);

			locationCollection = new LocationCollection(Factory, false);
			provider = locationCollection;
			regionCode = provider.CodeFromDescription(TestZone.Description);
			AssertNull("Regions not allowed so no code returned", regionCode);

			locationCollection = new LocationCollection(Factory, zoneList);
			provider = locationCollection;
			regionCode = provider.CodeFromDescription(TestZone.Description);
			AssertNull("No code as XXXX zone is not allowed zone type", regionCode);

			locationCollection = new LocationCollection(Factory, true, false);
			provider = locationCollection;
			countryCode = provider.CodeFromDescription(TestCountry.Description);
			AssertNull("Countries not allowed so no code returned", countryCode);
		}

		public void TestNearestMatch()
		{
			var locationCollection = new LocationCollection(Factory);
			TestProvider = locationCollection;

			var nearestMatch = TestProvider.NearestMatch("XX", true, -1).Item1;
			AssertEquals("Auto Completed", TestLOCO.RL_Code, nearestMatch);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
		}

		public void TestIFindBoxListProviderEx()
		{
			var collection = new LocationCollection(Factory);
			var findBoxListProviderEx = (IFindBoxListProviderEx)collection;

			var alternateKeys = findBoxListProviderEx.AlternateKeys;
			AssertEquals(1, alternateKeys.Count);
			AssertEquals("Description", alternateKeys[0].ColumnName);

			AssertEquals(TestLOCO.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXXX Port"));
			AssertEquals(TestZone.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXX Zone"));
			AssertEquals(TestCountry.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XX Country"));

			AssertEquals("XXXXX Port", findBoxListProviderEx.AlternateKeyFromPrimaryKey("Description", TestLOCO.PK));
			AssertEquals("XXXX Zone", findBoxListProviderEx.AlternateKeyFromPrimaryKey("Description", TestZone.PK));
			AssertEquals("XX Country", findBoxListProviderEx.AlternateKeyFromPrimaryKey("Description", TestCountry.PK));

			collection = new LocationCollection(Factory, false);
			findBoxListProviderEx = collection;

			AssertEquals(TestLOCO.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXXX Port"));
			AssertEquals(ZGuid.Empty, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXX Zone"));
			AssertEquals(TestCountry.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XX Country"));

			collection = new LocationCollection(Factory, true, false);
			findBoxListProviderEx = collection;

			AssertEquals(TestLOCO.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXXX Port"));
			AssertEquals(TestZone.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XXXX Zone"));
			AssertEquals(ZGuid.Empty, findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", (ZString)"XX Country"));
		}

		public void TestFindBoxListProvider_GetBusinessObjectFromCode_DbHitsForCountryCode()
		{
			using (RowFactory.SetCachedTables())
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var hits = new Dictionary<string, int> {
					{ RefUNLOCOSchema.Constants.TableName, 0 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefZoneHeaderSchema.Constants.TableName, 0 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(hits, newFactory))
				{
					var collection = new LocationCollection(newFactory);
					IFindBoxListProvider findBoxListProvider = collection;
					findBoxListProvider.GetBusinessObjectFromCode("AU");
					findBoxListProvider.PrimaryKeyFromCode("AU");
					findBoxListProvider.DescriptionFromCode("AU");
				}
			}
		}

		public void TestFindBoxListProvider_GetBusinessObjectFromCode_DbHitsForInvalidPortCode()
		{
			using (RowFactory.SetCachedTables())
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var hits = new Dictionary<string, int> {
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 0 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(hits, newFactory))
				{
					var collection = new LocationCollection(newFactory);
					IFindBoxListProvider findBoxListProvider = collection;
					findBoxListProvider.GetBusinessObjectFromCode("1-1-1");
					findBoxListProvider.PrimaryKeyFromCode("1-1-1");
					findBoxListProvider.DescriptionFromCode("1-1-1");
				}
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(RefUNLOCO));
		}

		IFindBoxListProvider TestProvider;

		RefUNLOCO TestLOCO;
		RefZoneHeader TestZone;
		RefCountry TestCountry;
		ZoneTypeList zoneList;

		protected override void SetUp()
		{
			base.SetUp();

			TestLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			TestLOCO.RL_Code = "XXXXX";
			TestLOCO.RL_PortName = "XXXXX Port";

			TestZone = Factory.NewWithValidTestData<RefZoneHeader>();
			TestZone.FZ_Code = "XXXX";
			TestZone.FZ_Description = "XXXX Zone";
			TestZone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.All.Code;

			TestCountry = Factory.NewWithValidTestData<RefCountry>();
			TestCountry.RN_Code = "XX";
			TestCountry.RN_Desc = "XX Country";

			zoneList = new ZoneTypeList();
			zoneList.Add(ZoneTypeCodeDescriptionPair.Rating);
		}

		#endregion
	}
}
