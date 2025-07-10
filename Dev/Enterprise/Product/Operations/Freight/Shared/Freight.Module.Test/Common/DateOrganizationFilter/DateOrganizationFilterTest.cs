using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(DateOrganizationFilter))]
	sealed class DateOrganizationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Property1", ZDateTime.Empty, Filter.Property1);
				AssertEquals("Property2", ZDateTime.Empty, Filter.Property2);
				AssertEquals("OrganizationPK", ZGuid.Empty, Filter.OrganizationPK);
				AssertEquals("OrganizationComparisonOperator", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, Filter.OrganizationComparisonOperator);
			});
		}

		#region Validation

		public void TestValidateOrganizationPK()
		{
			var validOrg = Factory.NewWithValidTestData<OrgHeader>();
			validOrg.OH_IsMiscFreightServices = true;
			validOrg.OH_IsPackDepot = true;
			var validAddress = validOrg.Addresses.AddNew();
			validAddress.Address1 = "Valid address, exists in org list";

			var invalidOrg = Factory.NewWithValidTestData<OrgHeader>();
			var invalidAddress = invalidOrg.Addresses.AddNew();
			invalidAddress.Address1 = "Some invalid address";

			Filter.OrganizationPK = validOrg.PK;
			Filter.Validation.ValidateOrganizationPK();

			AssertNoErrors(Filter.OrganizationPKInfo);

			Filter.OrganizationPK = invalidOrg.PK;
			Filter.Validation.ValidateOrganizationPK();

			AssertHasError(Filter.OrganizationPKInfo, "Enter a valid selection.");
		}

		#endregion

		#region OrganizationComparisonOperator

		public void TestOrganizationComparisonOperator_List()
		{
			var expected = new HashSet<string>()
			{
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank,
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank,
			};

			var comparisonOperator_List = Filter.ComparisonOperator_List.GetAllCodes().ToHashSet();

			AssertEquals("Should only have 4 comparisons", comparisonOperator_List.Count, expected.Count);

			foreach (var comparisonConstant in expected)
			{
				Assert($"{comparisonConstant} should be in the list", comparisonOperator_List.Contains(comparisonConstant));
			}
		}

		public void TestSQLComparisonOperator()
		{
			var comparisonConstantToOperatorMapping = new Dictionary<string, SQLComparisonOperator>()
			{
				{ ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, SQLComparisonOperator.Equal },
				{ ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual, SQLComparisonOperator.NotEqual },
				{ ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, SQLComparisonOperator.IsBlank },
				{ ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, SQLComparisonOperator.IsNotBlank },
			};

			foreach (var mapping in comparisonConstantToOperatorMapping)
			{
				Filter.OrganizationComparisonOperator = mapping.Key;
				AssertEquals("SqlComparisonOperator should have mapped to:", mapping.Value, Filter.SqlComparisonOperator);
			}
		}

		public void TestOrganizationComparisonOperator()
		{
			var today = ZDateTime.Now;
			var (org, address1, _, _) = GetOrgWithAddressess();

			var someOtherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var someOtherOrgAddress = someOtherOrg.Addresses.AddNew();
			someOtherOrgAddress.Address1 = "Some other org";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_OA_PackDepotAddress = address1.PK;
			consol1.JK_PackDepotReceiptRequested = today.AddDays(-2);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_OA_PackDepotAddress = address1.PK;
			consol2.JK_PackDepotReceiptRequested = today.AddDays(-2);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_OA_PackDepotAddress = someOtherOrgAddress.PK;
			consol3.JK_PackDepotReceiptRequested = today.AddDays(-2);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_PackDepotReceiptRequested = today.AddDays(-2);

			Factory.Save();

			Filter.Property1 = ZDateTime.Now.AddDays(-5);
			Filter.Property2 = ZDateTime.Now.AddDays(5);
			Filter.OrganizationPK = org.PK;

			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return exact matches by default",
				expected: new[] { consol1, consol2 },
				actual: consolCollection
			);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return 'not equal' matches",
				expected: new[] { consol3 },
				actual: consolCollection
			);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;

			consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return any consol with null orgs",
				expected: new[] { consol4 },
				actual: consolCollection
			);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;

			consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return any consol with non-null orgs",
				expected: new[] { consol1, consol2, consol3 },
				actual: consolCollection
			);
		}

		#endregion

		#region GetQuery

		public void TestGetQuery_DateAndOrganisation_DifferentDateFilterTypes()
		{
			var today = ZDateTime.Now;
			var (org, address1, address2, address3) = GetOrgWithAddressess();

			var job1 = Factory.New<CommonConsol>();
			job1.JK_OA_PackDepotAddress = address1.PK;
			job1.JK_PackDepotReceiptRequested = today.AddDays(-3);

			var job2 = Factory.New<CommonConsol>();
			job2.JK_OA_PackDepotAddress = address2.PK;
			job2.JK_PackDepotReceiptRequested = today.AddDays(-12);

			var job3 = Factory.New<CommonConsol>();
			job3.JK_OA_PackDepotAddress = address3.PK;
			job3.JK_PackDepotReceiptRequested = ZDateTime.Empty;

			Factory.Save();

			Filter.OrganizationPK = org.PK;
			Filter.Property1 = ZDateTime.Now.AddDays(-3);
			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;

			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return matching with date entered",
				expected: new[] { job1, job2 },
				actual: consolCollection
			);

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return results with empty date time",
				expected: new[] { job3 },
				actual: consolCollection
			);

			Filter.Property1 = ZDateTime.Now.AddDays(-5);
			Filter.Property2 = ZDateTime.Now.AddDays(5);
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should return results within date range",
				expected: new[] { job1 },
				actual: consolCollection
			);
		}

		public void TestGetQuery_NoDateRangeEntered()
		{
			var today = ZDateTime.Now;
			var (org, address1, _, _) = GetOrgWithAddressess();

			var invalidOrg = Factory.NewWithValidTestData<OrgHeader>();
			var invalidAddress = invalidOrg.Addresses.AddNew();
			invalidAddress.Address1 = "Some other org we aren't searching for";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_OA_PackDepotAddress = address1.PK;
			consol1.JK_PackDepotReceiptRequested = today.AddDays(-1);

			var shouldNotAppear_DueToOrg = Factory.New<CommonConsol>();
			shouldNotAppear_DueToOrg.JK_OA_PackDepotAddress = invalidAddress.PK;
			shouldNotAppear_DueToOrg.JK_PackDepotReceiptRequested = today.AddDays(-10);

			Factory.Save();

			Filter.OrganizationPK = org.PK;

			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should include any consol regardless of dates -- aslong as they have the organisation we are searching for",
				expected: new[] { consol1 },
				actual: consolCollection
			);
		}

		public void TestGetQuery_NoOrganizationEntered()
		{
			var today = ZDateTime.Now;
			var (_, address1, _, _) = GetOrgWithAddressess();

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_OA_PackDepotAddress = address1.PK;
			consol1.JK_PackDepotReceiptRequested = today.AddDays(-1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_OA_PackDepotAddress = address1.PK;
			consol2.JK_PackDepotReceiptRequested = today.AddDays(-5);

			var shouldNotAppear_OutsideOfDateRange = Factory.New<CommonConsol>();
			shouldNotAppear_OutsideOfDateRange.JK_OA_PackDepotAddress = address1.PK;
			shouldNotAppear_OutsideOfDateRange.JK_PackDepotReceiptRequested = today.AddDays(-10);

			Factory.Save();

			Filter.Property1 = ZDateTime.Now.AddDays(-5);
			Filter.Property2 = ZDateTime.Now.AddDays(5);
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);

			AssertContainsExactElementsInAnyOrder(
				"Should include any consol regardless of organisation -- aslong as they are within the date range",
				expected: new[] { consol1, consol2 },
				actual: consolCollection
			);
		}

		#endregion

		#region ReadOnly

		public void TestOrganizationPK_ReadOnly()
		{
			AssertEquals("Editable by default", false, Filter.OrganizationPKInfo.ReadOnly);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			AssertEquals("ReadOnly when IsBlank selected", true, Filter.OrganizationPKInfo.ReadOnly);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertEquals("Editable when Exact selected", false, Filter.OrganizationPKInfo.ReadOnly);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			AssertEquals("ReadOnly when IsNotBlank selected", true, Filter.OrganizationPKInfo.ReadOnly);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			AssertEquals("Editable when NotEqual selected", false, Filter.OrganizationPKInfo.ReadOnly);
		}

		public void TestOrganizationPK_SetToEmpty_WhenIsBlankOrNotBlank()
		{
			var (org, _, _, _) = GetOrgWithAddressess();

			Filter.OrganizationPK = org.PK;
			AssertEquals("Pre-Condition: Has a value", org.PK, Filter.OrganizationPK);

			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			AssertEquals("Should clear value when `IsBlank`", ZGuid.Empty, Filter.OrganizationPK);

			Filter.OrganizationPK = org.PK;
			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertEquals("Shouldn't clear value when `Exact`", false, Filter.OrganizationPKInfo.ReadOnly);

			Filter.OrganizationPK = org.PK;
			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			AssertEquals("Should clear value when `IsNotBlank`", ZGuid.Empty, Filter.OrganizationPK);

			Filter.OrganizationPK = org.PK;
			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			AssertEquals("Shouldn't clear value when `NotEqual`", false, Filter.OrganizationPKInfo.ReadOnly);
		}

		#endregion

		#region Clear / IsEmpty

		public void TestClear()
		{
			var searchValue = "Today";
			var dateValue1 = new ZDateTime(2012, 05, 05);
			var dateValue2 = new ZDateTime(2012, 05, 06);
			var orgPK = ZGuid.NewZGuid();

			Filter.PropertySearch = searchValue;
			Filter.Property1 = dateValue1;
			Filter.Property2 = dateValue2;
			Filter.OrganizationPK = orgPK;
			Filter.OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			CombineAssertions("Pre-Conditions", () =>
			{
				AssertEquals(searchValue, Filter.PropertySearch);
				AssertEquals(dateValue1, Filter.Property1);
				AssertEquals(dateValue2, Filter.Property2);
				AssertEquals(orgPK, Filter.OrganizationPK);
				AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual, Filter.OrganizationComparisonOperator);
			});

			Filter.Clear();

			CombineAssertions("Should all be empty", () =>
			{
				AssertEquals(ZString.Empty, Filter.PropertySearch);
				AssertEquals(ZDateTime.Empty, Filter.Property1);
				AssertEquals(ZDateTime.Empty, Filter.Property2);
				AssertEquals(ZGuid.Empty, Filter.OrganizationPK);
				AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, Filter.OrganizationComparisonOperator);
			});
		}

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Today;
			Filter.Property2 = ZDateTime.Today;

			Filter.PropertySearch = ZString.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "crap data";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);
		}

		#endregion

		#region Serialisation

		public void TestSerialisation()
		{
			Filter.PropertySearch = "Tomorrow";
			Filter.Property1 = new ZDateTime(2008, 1, 1, 10, 0, 0);
			Filter.Property2 = new ZDateTime(2008, 2, 2, 10, 0, 0);
			Filter.OrganizationPK = new ZGuid("64bdf460-7704-4f01-ba24-7ea821e531f3");

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("DateLocationFilter Serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestSerialisationOffsetRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			Filter.Property1 = new ZDateTime(2020, 1, 1, 10, 0, 0);
			Filter.Property2 = new ZDateTime(2020, 2, 1, 10, 0, 0);
			Filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			Filter.PropertyDecimal1 = new ZDecimal(2.00);
			Filter.PropertyDecimal2 = new ZDecimal(10.00);
			Filter.OrganizationPK = new ZGuid("64bdf460-7704-4f01-ba24-7ea821e531f3");

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("DateLocationFilterWithOffsetRange Serialisation", SampleXml2, writer.ToString());
			}
		}

		public void TestDeserialisation()
		{
			using (StringReader reader = new StringReader(SampleXml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter"); // because we follow a broken pattern for reading xml.
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement(); // because we follow a broken pattern for reading xml.
			}

			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", "Tomorrow", Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2008, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2008, 02, 02, 10, 0, 0), Filter.Property2);
				AssertEquals("OrganizationPK", new ZGuid("64bdf460-7704-4f01-ba24-7ea821e531f3"), Filter.OrganizationPK);
			});
		}

		public void TestDeserialisationPreviousFilters()
		{
			using (StringReader reader = new StringReader(SampleXmlPrevious))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", "Tomorrow", Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2008, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2008, 02, 02, 10, 0, 0), Filter.Property2);
				AssertEquals("OrganizationPK", new ZGuid("64bdf460-7704-4f01-ba24-7ea821e531f3"), Filter.OrganizationPK);
			});
		}

		public void TestDeserialisationOffsetRange()
		{
			using (var reader = new StringReader(SampleXml2))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)Filter).ReadXml(xmlReader);
			}
			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", ModuleDateFilter.SpecifiedHourOffsetRange, Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2020, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2020, 02, 01, 10, 0, 0), Filter.Property2);
				AssertEquals("PropertyDecimal1", new ZDecimal(2.00), Filter.PropertyDecimal1);
				AssertEquals("PropertyDecimal2", new ZDecimal(10.00), Filter.PropertyDecimal2);
				AssertEquals("FilterOption", DateOffsetRangeFilterOptions.Codes.Future, Filter.FilterOption);
				AssertEquals("OrganizationPK", new ZGuid("64bdf460-7704-4f01-ba24-7ea821e531f3"), Filter.OrganizationPK);
			});
		}

		#endregion

		#region Implementation

		const string SampleXml =
			"<Filter>\r\n" +
			"  <SearchProperty>Tomorrow</SearchProperty>\r\n" +
			"  <Property1>2008-01-01 10:00:00.000</Property1>\r\n" +
			"  <Property2>2008-02-02 10:00:00.000</Property2>\r\n" +
			"  <FilterOption>Past</FilterOption>\r\n" +
			"  <PropertyDecimal1>0.00</PropertyDecimal1>\r\n" +
			"  <PropertyDecimal2>0.00</PropertyDecimal2>\r\n" +
			"  <OrganizationPK>64bdf460-7704-4f01-ba24-7ea821e531f3</OrganizationPK>\r\n" +
			"</Filter>\r\n" +
			"";

		const string SampleXml2 =
			"<Filter>\r\n" +
			"  <SearchProperty>Offset range</SearchProperty>\r\n" +
			"  <Property1>2020-01-01 10:00:00.000</Property1>\r\n" +
			"  <Property2>2020-02-01 10:00:00.000</Property2>\r\n" +
			"  <FilterOption>Future</FilterOption>\r\n" +
			"  <PropertyDecimal1>2.00</PropertyDecimal1>\r\n" +
			"  <PropertyDecimal2>10.00</PropertyDecimal2>\r\n" +
			"  <OrganizationPK>64bdf460-7704-4f01-ba24-7ea821e531f3</OrganizationPK>\r\n" +
			"</Filter>\r\n" +
			"";

		const string SampleXmlPrevious =
			 "<Filter>\r\n" +
			 "  <SearchProperty>Tomorrow</SearchProperty>\r\n" +
			 "  <Property1>2008-01-01 10:00:00.000</Property1>\r\n" +
			 "  <Property2>2008-02-02 10:00:00.000</Property2>\r\n" +
			 "  <OrganizationPK>64bdf460-7704-4f01-ba24-7ea821e531f3</OrganizationPK>\r\n" +
			 "</Filter>\r\n" +
			 "";

		DateOrganizationFilter Filter
		{
			get { return filter ?? (filter = (DateOrganizationFilter)GetNewBusinessObject()); }
		}
		DateOrganizationFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DateOrganizationFilter(
				"description",
				JobConsolSchema.JK_PackDepotReceiptRequested,
				GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_PackDepotAddress),
				BindToLists.GetCachedLists(Factory).PackDepot_List,
				null
			);
		}

		(OrgHeader org, OrgAddress address1, OrgAddress address2, OrgAddress address3) GetOrgWithAddressess()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			var address3 = org.Addresses.AddNew();
			address1.Address1 = "Some depot address";
			address2.Address1 = "Another depot address";
			address3.Address1 = "Another another depot address";

			return (org, address1, address2, address3);
		}

		GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(CommonConsol));
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn, notIn);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(orgAddressColumn, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				result.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			return result;
		}

		#endregion
	}
}
