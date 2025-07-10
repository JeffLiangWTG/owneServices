using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestsSubclassesOf(typeof(GenPivotDualDirectionRelatedEntityFilter))]
	public abstract class GenPivotDualDirectionRelatedEntityFilterTestCase<TFilter, TMainBizoType, TRelatedBizoType> : ModuleFilterTestCase<TFilter>
		where TFilter : GenPivotDualDirectionRelatedEntityFilter
		where TMainBizoType : BusinessObject, IWorkTaskRelatedItemSource
		where TRelatedBizoType : BusinessObject, IWorkTaskRelatedItemSource
	{
		public void TestFilter_AnyMatch()
		{
			var descriptionFilter = filter.SelectedFilters.AddTextFilterStrip(DescriptionFilterStripName, "Be");
			AssertResults(mainBizo1, mainBizo2);

			descriptionFilter.Property = "Battlestar";
			AssertResults(mainBizo2);

			filter.Clear();
			filter.SelectedFilters.AddTextFilterStrip(DescriptionFilterStripName, "Battlestar");

			AssertResults(mainBizo2);
		}

		public void TestFilter_AllMatch()
		{
			mainBizo3 = Factory.NewWithValidTestData<TMainBizoType>();
			GetMainBizoDescriptionProperty(mainBizo3).Value = (ZString)"False.";
			Factory.Save();

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			AssertResults(mainBizo1, mainBizo2, mainBizo3);

			var descriptionFilter = filter.SelectedFilters.AddTextFilterStrip(DescriptionFilterStripName, "Be");
			AssertResults(mainBizo1, mainBizo3);

			descriptionFilter.Property = "B";
			AssertResults(mainBizo1, mainBizo2, mainBizo3);
		}

		public void TestFilter_NoneMatch()
		{
			mainBizo3 = Factory.NewWithValidTestData<TMainBizoType>();
			GetMainBizoDescriptionProperty(mainBizo3).Value = (ZString)"False.";
			Factory.Save();

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertResults(mainBizo3);

			var descriptionFilter = filter.SelectedFilters.AddTextFilterStrip(DescriptionFilterStripName, "Be");
			AssertResults(mainBizo3);

			descriptionFilter.Property = "Battlestar";
			AssertResults(mainBizo1, mainBizo3);
		}

		public void TestFilterSQL_ShouldUseUnionAndTableCodes_BecausePerformance()
		{
			var filter = GetNewModuleFilter();
			var sql = filter.Query.LiteralTextSqlFormatted;

			AssertContains("Query should use union all because it's much faster than an OR", "union all", sql, ignoreCase: true);

			AssertContains($"{GenPivotSchema.Constants.XX_Relation1TableCode} = '{mainBizo1.TablePrefix}'", sql);
			AssertContains($"{GenPivotSchema.Constants.XX_Relation1TableCode} = '{relatedBizo1.TablePrefix}'", sql);

			AssertContains($"{GenPivotSchema.Constants.XX_Relation2TableCode} = '{mainBizo1.TablePrefix}'", sql);
			AssertContains($"{GenPivotSchema.Constants.XX_Relation2TableCode} = '{relatedBizo1.TablePrefix}'", sql);
		}

		public void TestFilterSql_AllMatch_ShouldUseParametersAndNotLiterals()
		{
			var filter = GetNewModuleFilter();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			filter.SelectedFilters.AddTextFilterStrip(DescriptionFilterStripName, "Be");
			var sql = filter.Query.ParameterisedText.ParameterisedQueryText;

			AssertContains("The sub query should use parameters rather than string literals. SAD!" + System.Environment.NewLine + sql, "@FOREIGN", sql);

			var parameters = filter.Query.Params;
			AssertEquals("The renamed parameters must actually be declared, too, or the query obviously won't work. SAD!", true, parameters.Any(x => x.ParameterName.StartsWith("@FOREIGN")));
		}

		#region Test Implementation

		protected virtual void CreateRelationship(IWorkTaskRelatedItemSource fromBizo, IWorkTaskRelatedItemSource toBizo)
		{
			fromBizo.RelatedItems.Add((BusinessObject)toBizo);
		}

		protected virtual void AssertResults(params TMainBizoType[] expectedResults)
		{
			var expectedDescriptions = expectedResults.Select(b => GetMainBizoDescriptionProperty(b).Value.ToString());
			var results = Factory.Load<TMainBizoType>(mainFilterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(expectedDescriptions, results.Select(b => GetMainBizoDescriptionProperty(b).Value.ToString()));
		}

		protected abstract ZPropertyInfo GetMainBizoDescriptionProperty(TMainBizoType mainBizo);
		protected abstract ZPropertyInfo GetRelatedBizoDescriptionProperty(TRelatedBizoType relatedBizo);

		protected abstract FilterStripBusinessObject CreateMainFilterBusinessObject();
		protected abstract FilterStripBusinessObject CreateRelatedFilterBusinessObject();

		protected abstract ModuleIdentifier RelatedModuleID { get; }
		protected abstract string DescriptionFilterStripName { get; }
		protected abstract string RelatedEntityFilterStripName { get; }

		#endregion

		#region SetUp

		protected TFilter filter;
		protected FilterStripBusinessObject mainFilterBizo;
		protected TMainBizoType mainBizo1, mainBizo2, mainBizo3;
		protected TRelatedBizoType relatedBizo1, relatedBizo2, relatedBizo3;

		protected override void SetUp()
		{
			base.SetUp();

			mainBizo1 = Factory.NewWithValidTestData<TMainBizoType>();
			mainBizo2 = Factory.NewWithValidTestData<TMainBizoType>();

			relatedBizo1 = Factory.NewWithValidTestData<TRelatedBizoType>();
			relatedBizo2 = Factory.NewWithValidTestData<TRelatedBizoType>();
			relatedBizo3 = Factory.NewWithValidTestData<TRelatedBizoType>();

			CreateRelationship(mainBizo1, relatedBizo1);
			CreateRelationship(relatedBizo2, mainBizo2);
			CreateRelationship(relatedBizo3, mainBizo2);

			GetMainBizoDescriptionProperty(mainBizo1).Value = (ZString)"Question";
			GetMainBizoDescriptionProperty(mainBizo2).Value = (ZString)"What kind of bear is best?";

			GetRelatedBizoDescriptionProperty(relatedBizo1).Value = (ZString)"Bears";
			GetRelatedBizoDescriptionProperty(relatedBizo2).Value = (ZString)"Beets";
			GetRelatedBizoDescriptionProperty(relatedBizo3).Value = (ZString)"Battlestar Galactica";

			AssertEquals("Question", GetMainBizoDescriptionProperty(mainBizo1).Value);
			AssertEquals("What kind of bear is best?", GetMainBizoDescriptionProperty(mainBizo2).Value);

			var unrelatedBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var unrelatedBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();

			unrelatedBizo1.Z0_Description = "Unrelizo1";
			unrelatedBizo2.Z0_Description = "Unrelizo2";

			var pivot1 = Factory.New<GenPivot>();
			var pivot2 = Factory.New<GenPivot>();

			pivot1.XX_Relation1ID = mainBizo1.PK;
			pivot1.XX_Relation1TableCode = mainBizo1.TablePrefix;
			pivot1.XX_RelationType = Core.Constants.GenPivotTypes.ProcessManagement;
			pivot1.XX_Relation2ID = unrelatedBizo1.PK;
			pivot1.XX_Relation2TableCode = unrelatedBizo1.TablePrefix;

			pivot2.XX_Relation2ID = mainBizo1.PK;
			pivot2.XX_Relation2TableCode = mainBizo1.TablePrefix;
			pivot2.XX_RelationType = Core.Constants.GenPivotTypes.ProcessManagement;
			pivot2.XX_Relation1ID = unrelatedBizo2.PK;
			pivot2.XX_Relation1TableCode = unrelatedBizo2.TablePrefix;

			Factory.Save();

			mainFilterBizo = CreateMainFilterBusinessObject();
			filter = (TFilter)mainFilterBizo[RelatedEntityFilterStripName];
			filter.IsActive = true;
		}

		#endregion

		#region ModuleFilterTestCase Overrides

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("Filter uses 'any match' by default, which isn't empty", true);
		}

		protected abstract IBusinessObjectCollection CreateRelatedEntityCollection();

		#endregion
	}
}
