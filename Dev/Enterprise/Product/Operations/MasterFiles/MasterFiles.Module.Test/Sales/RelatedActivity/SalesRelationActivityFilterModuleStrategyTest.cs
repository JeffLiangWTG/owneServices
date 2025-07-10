using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SalesRelationActivityFilterModuleStrategyTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRunOnFilterControlInitialisation_WithSalesRelationActivity()
		{
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripCommonControl(new OrgOpportunityFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var strategy = new SalesRelationActivityFilterModuleStrategy();
				strategy.RunOnFilterControlInitialisation(filterControl, new OrgOpportunityCollection(Factory));

				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<ZString>(),
					filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(info => info.ColumnName));

				var filterStrip = filterControl.AddNewFilterStrip();
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						typeof(SalesRelationActivityFilterHelper.RecentActivityDateFilterControlBuilder),
						typeof(SalesRelationActivityFilterHelper.HasSalesRelationFilterControlBuilder)
					},
					filterStrip.CustomFilterControlsBuilders.Select(builder => builder.GetType()));
			}
		}

		[RequiresSTA]
		public void TestRunOnFilterControlInitialisation_WithNonSalesRelationActivity()
		{
			using (var form = new ZForm())
			using (var filterControl = new ZFilterStripCommonControl(new OrgContactsFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var strategy = new SalesRelationActivityFilterModuleStrategy();
				strategy.RunOnFilterControlInitialisation(filterControl, new OrgContactCollection(Factory));

				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<string>(),
					filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(info => info.ColumnName));

				var filterStrip = filterControl.AddNewFilterStrip();
				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<ZFilterStrip.CustomFilterControlsBuilder>(),
					filterStrip.CustomFilterControlsBuilders.Select(builder => builder.GetType()));
			}
		}

		public void TestRunOnModuleFiltersCreated_WithSalesRelationActivity()
		{
			var moduleFilters = new ModuleFilterCollection();
			var strategy = new SalesRelationActivityFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(moduleFilters, typeof(OrgOpportunity), Factory);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate + FilterModuleStrategy.UniqueSuffix,
					SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation + FilterModuleStrategy.UniqueSuffix,
				},
				moduleFilters.Select(filter => filter.Description));
		}

		public void TestRunOnModuleFiltersCreated_WithNonSalesRelationActivity()
		{
			var moduleFilters = new ModuleFilterCollection();
			var strategy = new SalesRelationActivityFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(moduleFilters, typeof(OrgContact), Factory);

			AssertContainsExactElementsInAnyOrder(
				Enumerable.Empty<string>(),
				moduleFilters.Select(filter => filter.Description));
		}

		public void TestRunOnModuleFiltersCreated_SystemLastEditTimeUtc()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();
			TestConnection.ExecuteNonQuery($"update dbo.OrgOpportunity set P8_SystemLastEditTimeUtc = '2000-1-1', P8_SystemLastEditUser = 'E' where P8_PK = '{opp1.PK}';");
			TestConnection.ExecuteNonQuery($"update dbo.OrgOpportunity set P8_SystemLastEditTimeUtc = '{ZDateTime.Now.AddDays(3).SqlFormat}', P8_SystemLastEditUser = 'E' where P8_PK = '{opp2.PK}';");

			var moduleFilters = new ModuleFilterCollection();
			var strategy = new SalesRelationActivityFilterModuleStrategy();
			strategy.RunOnModuleFiltersCreated(moduleFilters, typeof(OrgOpportunity), Factory);

			var recentActivityDateFilter = moduleFilters.OfType<RecentActivityDateFilter>().Single();
			var activityLastEditColumn = recentActivityDateFilter.GetType().GetField("activityLastEditColumn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(recentActivityDateFilter) as SchemaDateTimeColumn;
			AssertEquals("P8_SystemLastEditTimeUtc", activityLastEditColumn.Name);
			recentActivityDateFilter.PropertySearch = ModuleDateFilter.Past;
			recentActivityDateFilter.IsActive = true;

			var query = moduleFilters.GetFilterQuery(new[] { recentActivityDateFilter });
			AssertEquals(opp1.PK, new BusinessObjectFactory().Load<OrgOpportunity>(query).Single().PK);
		}
	}
}
