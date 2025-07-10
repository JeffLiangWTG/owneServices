using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	public class CusPermitFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestAddAlwaysVisibleFilterStripsWithNoException()
		{
			var filterRules = new Dictionary<CodeDescriptionPair, ZString>()
			{
				{ new CodeDescriptionPair("CODE1", "CODE1 Description"), "VALUE1" },
				{ new CodeDescriptionPair("CODE2", "CODE2 Description"), "VALUE2" },
				{ new CodeDescriptionPair("CODE3", "CODE3 Description"), "VALUE3" }
			};
			var collection = PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, null, "", "", filterRules, ZDate.Today);
			var filterBizO = new CusPermitFilterStripBusinessObject();
			using (var form = new Form())
			{
				var filterControl = new CusPermitFilterControl_ForTest(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();
				var permitRuleFilters = filterControl.Strips.Where(x => x.CurrentDataItem.FilterDescription == CusPermitFilterStripBusinessObject.Schema.PermitRule).ToArray();
				AssertEquals("3 permit rule strips should exist", 3, permitRuleFilters.Length);
				AssertEquals("Property1 (Code) is set", "CODE1", ((PermitRuleModuleFilter)permitRuleFilters.First().CurrentDataItem.CurrentModuleFilter).Property1);
				AssertEquals("Property2 (VALUE) is set", "VALUE1", ((PermitRuleModuleFilter)permitRuleFilters.First().CurrentDataItem.CurrentModuleFilter).Property2);
			}

			filterBizO = new CusPermitFilterStripBusinessObject();
			using (var form = new Form())
			using (filterBizO.ParentModule = new CusPermitModule())
			{
				filterBizO.ParentModule.LimitedColumns = new ZArchitecture.Modules.ZLimitedColumnsProvider(CusPermitHeaderSchema.CPH_Number, CusPermitHeaderSchema.CPH_Number);
				var filterControl = new CusPermitFilterControl_ForTest(collection, filterBizO);
				form.Controls.Add(filterControl);
				form.Show();
				var permitRuleFilters = filterControl.Strips.Where(x => x.CurrentDataItem.FilterDescription == CusPermitFilterStripBusinessObject.Schema.PermitRule);
				AssertEquals("0 permit rule strips should exist", 0, permitRuleFilters.Count());
			}
		}

		[RequiresSTA]
		public void TestValueBalance()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertType<ZCalcEditColumnStyleInfo>(filterControl.Grid.GetColumnStyle(nameof(BaseCusPermitHeader.ValueBalance)));
			}
		}

		[RequiresSTA]
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGridColumnWidths()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		CusPermitFilterControl GetNewFilterControl()
		{
			var collection = new CusPermitHeaderCollection(Factory);
			var filterBizO = new CusPermitFilterStripBusinessObject();
			return new CusPermitFilterControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(nameof(BaseCusPermitHeader.PermitHolder) + "+" + nameof(OrgHeader.OH_Code), "Permit Holder", 100),
			(CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode, "Country Code", 100),
			(CusPermitHeaderSchema.Constants.CPH_Number, "Permit Number", 100),
			(CusPermitHeaderSchema.Constants.CPH_StartDate, "Start Date", 100),
			(CusPermitHeaderSchema.Constants.CPH_EndDate, "End Date", 100),
			(CusPermitHeaderSchema.Constants.CPH_Type, "Type", 80),
			(CusPermitHeaderSchema.Constants.CPH_SubType, "Sub Type", 90),
			(CusPermitHeaderSchema.Constants.CPH_QtyValIndicator, "Qty/Val Indicator", 100),
			(nameof(BaseCusPermitHeader.ValueBalance), "Remaining Value", 110),
			(nameof(BaseCusPermitHeader.QuantityBalance), "Remaining Quantity", 110),
			(nameof(BaseCusPermitHeader.AuthLatestValueBalanceWithMsg), "Auth. Latest Val", 110),
			(nameof(BaseCusPermitHeader.AuthLatestQuantityBalanceWithMsg), "Auth. Latest Qty", 90),
			(CusPermitHeaderSchema.Constants.CPH_UnitOfMeasure, "Unit of Measure", 110),
			(CusPermitHeaderSchema.Constants.CPH_IsClosed, "Closed", 110)
		};
	}

	internal class CusPermitFilterControl_ForTest : CusPermitFilterControl
	{
		public CusPermitFilterControl_ForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
		{
		}

		public new List<ZFilterStrip> Strips => base.Strips;
	}
}
