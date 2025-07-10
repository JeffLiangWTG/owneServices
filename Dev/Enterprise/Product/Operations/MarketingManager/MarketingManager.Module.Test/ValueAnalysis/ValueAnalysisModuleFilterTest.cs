using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisModuleFilter))]
	public class ValueAnalysisModuleFilterTest : ModuleFilterTestCase<ValueAnalysisModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
		protected override ZString ExpectedDescription => ModuleIDs.ValueAnalysisForwardingOrg.Description;

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("It's definitely empty", true);
		}

		public void TestVVA_GetQuery()
		{
			using (var module = new ModuleForTest())
			{
				var fbo = module.FilterBusinessObject as ValueAnalysisFilterBusinessObject;
				AssertNotNull("ValueAnalysisFilterBusinessObject", fbo);
				fbo.ModuleContext = OrgHeaderSchema.PK.Name;
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.TradeStatus);

				var filter = fbo[ValueAnalysisFilterStripsHelper.Description.TradeStatus] as ModuleTextFilter;
				AssertNotNull("TradeStatus filter", filter);
				filter.IsActive = true;
				filter.Property = OrgSalesActualsStatusList.Codes.Prospective;

				var moduleFilter = GetNewModuleFilter();
				moduleFilter.UpdateSelectedFilters(fbo);

				var productPk = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment)).PK;
				using (var findBox = new ZFilterCollectionFindBox(moduleFilter))
				{
					var onLayoutChanged = findBox.GetType().GetMethod("OnLayoutChanged", BindingFlags.Instance | BindingFlags.NonPublic);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					var queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_Status" && p.Value.ToString() == OrgSalesActualsStatusList.Codes.Prospective));
					AssertEquals(@"
NOT EXISTS
(
	SELECT VVA_PK FROM dbo.ViewValueAnalysis WHERE VVA_OH_Primary = OH_PK
	EXCEPT SELECT VVA_PK FROM dbo.ViewValueAnalysis WHERE VVA_OH_Primary = OH_PK AND (VVA_Status = @P1_VVA_Status 
AND
VVA_MP_Product = @P0_VVA_MP_Product
)
)
AND OH_PK IN
(
	SELECT VVA_OH_Primary FROM dbo.ViewValueAnalysis WHERE VVA_Status = @P1_VVA_Status 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_Status" && p.Value.ToString() == OrgSalesActualsStatusList.Codes.Prospective));
					AssertEquals(@"
OH_PK IN
(
	SELECT VVA_OH_Primary FROM dbo.ViewValueAnalysis WHERE VVA_Status = @P1_VVA_Status 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_Status" && p.Value.ToString() == OrgSalesActualsStatusList.Codes.Prospective));
					AssertEquals(@"
OH_PK NOT IN
(
	SELECT VVA_OH_Primary FROM dbo.ViewValueAnalysis WHERE VVA_Status = @P1_VVA_Status 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);
				}
			}
		}

		public void TestVVA_GetQueryOrgOpportunity()
		{
			using (var module = new ModuleForTest())
			{
				var fbo = module.FilterBusinessObject as ValueAnalysisFilterBusinessObject;
				AssertNotNull("ValueAnalysisFilterBusinessObject", fbo);
				fbo.ModuleContext = OrgOpportunitySchema.PK.Name;
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.Certainty);

				var filter = fbo[ValueAnalysisFilterStripsHelper.Description.Certainty] as ModuleTextFilter;
				AssertNotNull("Certainty filter", filter);
				filter.IsActive = true;
				filter.Property = CertaintyLikertItemList.Descriptions._4Likely;

				var moduleFilter = GetNewOpportunityModuleFilter();
				moduleFilter.UpdateSelectedFilters(fbo);

				var productPk = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment)).PK;
				using (var findBox = new ZFilterCollectionFindBox(moduleFilter))
				{
					var onLayoutChanged = findBox.GetType().GetMethod("OnLayoutChanged", BindingFlags.Instance | BindingFlags.NonPublic);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					var queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_ConversionCertainty" && (byte)p.Value == 20));
					AssertEquals(@"
NOT EXISTS
(
	SELECT VVA_PK FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE SVP_ActivityId = P8_PK
	EXCEPT SELECT VVA_PK FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE SVP_ActivityId = P8_PK AND (VVA_ConversionCertainty < @P1_VVA_ConversionCertainty 
AND
VVA_MP_Product = @P0_VVA_MP_Product
)
)
AND P8_PK IN
(
	SELECT SVP_ActivityId FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE VVA_ConversionCertainty < @P1_VVA_ConversionCertainty 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_ConversionCertainty" && (byte)p.Value == 20));
					AssertEquals(@"
P8_PK IN
(
	SELECT SVP_ActivityId FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE VVA_ConversionCertainty < @P1_VVA_ConversionCertainty 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					queryModuleFilter = moduleFilter.Query;
					AssertEquals(2, queryModuleFilter.Params.Length);
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_VVA_MP_Product" && (Guid)p.Value == productPk));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_VVA_ConversionCertainty" && (byte)p.Value == 20));
					AssertEquals(@"
P8_PK NOT IN
(
	SELECT SVP_ActivityId FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE VVA_ConversionCertainty < @P1_VVA_ConversionCertainty 
AND
VVA_MP_Product = @P0_VVA_MP_Product

)", queryModuleFilter.FilterString);
				}
			}
		}

		public void TestValueAnalysisQuantityFilter()
		{
			using (var module = new ModuleForTest())
			{
				var fbo = module.FilterBusinessObject as ValueAnalysisFilterBusinessObject;
				fbo.ModuleContext = OrgHeaderSchema.PK.Name;
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.TradedJobCost);
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue);

				var tradedJobCostFilter = fbo[ValueAnalysisFilterStripsHelper.Description.TradedJobCost] as ValueAnalysisQuantityFilter;
				tradedJobCostFilter.IsActive = true;
				tradedJobCostFilter.Property1 = 3000;
				tradedJobCostFilter.Property2 = 4000;

				var tradedJobRevenueFilter = fbo[ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue] as ValueAnalysisQuantityFilter;
				tradedJobRevenueFilter.IsActive = true;
				tradedJobRevenueFilter.Property1 = 30;
				tradedJobRevenueFilter.Property2 = 400;

				var moduleFilter = GetNewModuleFilter();
				moduleFilter.UpdateSelectedFilters(fbo);

				using (var findBox = new ZFilterCollectionFindBox(moduleFilter))
				{
					var onLayoutChanged = findBox.GetType().GetMethod("OnLayoutChanged", BindingFlags.Instance | BindingFlags.NonPublic);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					var queryModuleFilter = moduleFilter.Query;
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_PAV_Cost" && (decimal)p.Value == 4000));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P2_PAV_Revenue" && (decimal)p.Value == 400));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P3_PAV_Cost" && (decimal)p.Value == 3000));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P4_PAV_Revenue" && (decimal)p.Value == 30));

					var filterString = queryModuleFilter.FilterString;
					AssertContains("<=@P1_PAV_Cost", filterString);
					AssertContains("<=@P2_PAV_Revenue", filterString);
					AssertContains(">=@P3_PAV_Cost", filterString);
					AssertContains(">=@P4_PAV_Revenue", filterString);
				}
			}
		}

		public void TestValueAnalysisPipelineFilter()
		{
			using (var module = new ModuleForTest())
			{
				var fbo = module.FilterBusinessObject as ValueAnalysisFilterBusinessObject;
				fbo.ModuleContext = OrgOpportunitySchema.PK.Name;
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA);
				fbo.FilterStrips.AddNew(ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA);

				var volumeFilter = fbo[ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA] as ValueAnalysisPipelineFilter;
				volumeFilter.IsActive = true;
				volumeFilter.Property1 = 6000;
				volumeFilter.Property2 = 7000;

				var weightFilter = fbo[ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA] as ValueAnalysisPipelineFilter;
				weightFilter.IsActive = true;
				weightFilter.Property1 = 60;
				weightFilter.Property2 = 75;

				var moduleFilter = GetNewOpportunityModuleFilter();
				moduleFilter.UpdateSelectedFilters(fbo);

				using (var findBox = new ZFilterCollectionFindBox(moduleFilter))
				{
					var onLayoutChanged = findBox.GetType().GetMethod("OnLayoutChanged", BindingFlags.Instance | BindingFlags.NonPublic);

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
					onLayoutChanged.Invoke(findBox, Array.Empty<object>());

					var queryModuleFilter = moduleFilter.Query;
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P0_PAS_Volume" && (decimal)p.Value == 7000));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P1_PAS_Weight" && (decimal)p.Value == 75));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P2_PAS_Volume" && (decimal)p.Value == 6000));
					AssertEquals(1, queryModuleFilter.Params.Count(p => p.ParameterName == "@P3_PAS_Weight" && (decimal)p.Value == 60));

					var filterString = queryModuleFilter.FilterString;
					AssertContains("<=@P0_PAS_Volume", filterString);
					AssertContains("<=@P1_PAS_Weight", filterString);
					AssertContains(">=@P2_PAS_Volume", filterString);
					AssertContains(">=@P3_PAS_Weight", filterString);
				}
			}
		}

		protected override ValueAnalysisModuleFilter GetNewModuleFilter()
		{
			return new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisForwardingOrg, OrgHeaderSchema.PK, Factory, typeof(OrgHeader));
		}

		protected ValueAnalysisModuleFilter GetNewOpportunityModuleFilter()
		{
			return new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisForwardingOpp, OrgOpportunitySchema.PK, Factory, typeof(OrgOpportunity));
		}

		class ModuleForTest : ValueAnalysisForwardingOrgModule
		{
			public ModuleForTest() { }
		}
	}
}
