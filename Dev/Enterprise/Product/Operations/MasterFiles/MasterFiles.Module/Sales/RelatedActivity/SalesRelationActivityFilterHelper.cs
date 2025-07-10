using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	public static class SalesRelationActivityFilterHelper
	{
		#region Module Filters

		public static void AddAllModuleFilters(BusinessObjectFactory factory, ModuleFilterCollection filters, ITableSchema schema, ZString tableCode, SchemaDateTimeColumn activityLastEditColumn, Type elementType)
		{
			Argument.NotNull(filters, "filters");
			Argument.NotNull(schema, "schema");
			Argument.NotNull(elementType, "elementType");

			foreach (var moduleFilter in GetAllModuleFilters(factory, schema, tableCode, activityLastEditColumn, elementType))
			{
				filters.AddFilter(moduleFilter);
			}
		}

		public static IEnumerable<ModuleFilter> GetAllModuleFilters(BusinessObjectFactory factory, ITableSchema schema, ZString tableCode, SchemaDateTimeColumn activityLastEditColumn, Type elementType)
		{
			var subGroup = new SalesRelationNodeSubGroup(schema, elementType);
			yield return GetRecentActivityDateFilter(string.Empty, subGroup, tableCode, activityLastEditColumn, elementType);
			yield return GetHasSalesRelationFilter(factory, string.Empty, subGroup, elementType);
		}

		public static IEnumerable<ModuleFilter> GetAllModuleFilters(string descriptionPrefix, BusinessObjectFactory factory, ITableSchema schema, ZString tableCode, ITableSchema viewSchema, SchemaColumn foreignKey, SchemaDateTimeColumn activityLastEditColumn, Type viewElementType)
		{
			var subGroup = new SalesRelationNodeSubGroup(schema, viewElementType, viewSchema, foreignKey);
			yield return GetRecentActivityDateFilter(descriptionPrefix, subGroup, tableCode, activityLastEditColumn, viewElementType);
			yield return GetHasSalesRelationFilter(factory, descriptionPrefix, subGroup, viewElementType);
		}

		#region RecentActivityDate Filter

		static RecentActivityDateFilter GetRecentActivityDateFilter(string descriptionPrefix, ModuleFilterSubGroup subGroup, ZString tableCode, SchemaDateTimeColumn activityLastEditColumn, Type elementType)
		{
			var filter = new RecentActivityDateFilter(GetConcatenatedDescription(descriptionPrefix, FilterDescription.RecentActivityDate), tableCode, activityLastEditColumn, elementType);
			filter.MultilingualDescription = ResString.GetMultilingualString("4b8f4d14-bf09-4b52-a298-735b59edff7b", "Sales Relations Last Edit Time");
			filter.SubGroup = subGroup;
			return filter;
		}

		#endregion

		#region HasSalesRelation Filter

		static ActivityHasSalesRelationFilter GetHasSalesRelationFilter(BusinessObjectFactory factory, string descriptionPrefix, ModuleFilterSubGroup subGroup, Type elementType)
		{
			var filter = new ActivityHasSalesRelationFilter(factory, GetConcatenatedDescription(descriptionPrefix, FilterDescription.HasSalesRelation), elementType);
			filter.MultilingualDescription = ResString.GetMultilingualString("7700ca4f-6342-44df-8bd8-c1f35b7d57ac", "Has Sales Relation");
			filter.SubGroup = subGroup;
			return filter;
		}

		#endregion

		static string GetConcatenatedDescription(string prefix, string description)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				return description;
			}
			else
			{
				return string.Join("_", prefix, description);
			}
		}

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string RecentActivityDate = "Recent Activity Date";
			public const string HasSalesRelation = "Has Sales Relation";

			#endregion
		}

		#endregion

		#region SubGroups

		internal class SalesRelationNodeSubGroup : ModuleFilterSubGroup
		{
			public SalesRelationNodeSubGroup(ITableSchema schema, Type elementType) : this(schema, elementType, null, null)
			{
			}

			public SalesRelationNodeSubGroup(ITableSchema schema, Type viewElementType, ITableSchema viewSchema, SchemaColumn foreignKey)
			{
				this.schema = schema;
				this.elementType = viewElementType;
				this.viewSchema = viewSchema;
				this.foreignKey = foreignKey;
			}

			readonly ITableSchema schema;
			readonly Type elementType;
			readonly ITableSchema viewSchema;
			readonly SchemaColumn foreignKey;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(elementType);
				if (viewSchema == null && foreignKey == null)
				{
					var sql = string.Format(@"
{0}
IN
(
	SELECT
		Master.{0}
	FROM
		{1} Master
		LEFT JOIN dbo.vw_SalesRelationNode MasterNode ON Master.{0} = ActivityID
	WHERE
		{2}
)",
						schema.PK.Name,
						schema.TableName,
						filter.FilterString);

					var parameters = new ZSqlParameterCollection();
					parameters.AddRange(filter.Params);
					result.AddFilterAndZSQLParameterCollection(sql, parameters);
				}
				else
				{
					var sql = string.Format(@"
{0}
IN
(
	SELECT
		{0}
	FROM
		{1}
		LEFT JOIN {2} Master ON {0} = {3}
		LEFT JOIN dbo.vw_SalesRelationNode MasterNode ON Master.{4} = ActivityID
	WHERE
		{5}
)",
						viewSchema.PK.Name,
						viewSchema.TableName,
						schema.TableName,
						foreignKey.Name,
						schema.PK.Name,
						filter.FilterString);

					var parameters = new ZSqlParameterCollection();
					parameters.AddRange(filter.Params);
					result.AddFilterAndZSQLParameterCollection(sql, parameters);
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Filter Control

		public static void AddAllFilterControlColumnsAndBuilders(IFilterControl filterControl)
		{
			Argument.NotNull(filterControl, "filterControl");

			AddAllGridColumns(filterControl);
			AddAllFilterControlBuilders(filterControl);
		}

		#region Grid Columns

		static void AddAllGridColumns(IFilterControl filterControl)
		{
			// ADL: Currently columns very slow. Temporarily disable while we investigate how to make faster
			//filterControl.FilteredGrid.ColumnStyles.Add(GetRecentActivityDateColumn());
			//filterControl.FilteredGrid.ColumnStyles.Add(GetHasSalesRelationColumn());
		}

		#endregion

		#region Filter Control Builders

		static void AddAllFilterControlBuilders(IFilterControl filterControl)
		{
			filterControl.FilterStripAdding += FilterControl_FilterStripAdding;
		}

		static void FilterControl_FilterStripAdding(object sender, ZFilterStripEventArgs e)
		{
			e.Strip.AddCustomFilterControlsBuilder(new RecentActivityDateFilterControlBuilder());
			e.Strip.AddCustomFilterControlsBuilder(new HasSalesRelationFilterControlBuilder());
		}

		#region RecentActivityDateFilterControlBuilder

		public class RecentActivityDateFilterControlBuilder : ZFilterStrip.CustomFilterControlsBuilder
		{
			public override bool Handles(ModuleFilter moduleFilter)
			{
				return moduleFilter is RecentActivityDateFilter;
			}

			public override Control[] GetFilterControls(ZFilterStrip parentStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource)
			{
				var dateRangeControl = new ZDateRangeControl(parentStrip);
				ControlDpiScalingHelper.SetHeight(ref dateRangeControl, parentStrip.Height, false);
				dateRangeControl.TabIndex = 2;
				bindingSource.SetBindingMember(dateRangeControl, ".");

				var dropEdit = new ZDropEdit();
				dropEdit.CaptionResourceString = Res.GetData("28c30fef-fb59-4ab9-8adc-ab9b917bfa9f", "Type");
				dropEdit.CharacterCasing = CharacterCasing.Normal;
				ControlDpiScalingHelper.SetTop(ref dropEdit, dateRangeControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ControlDpiScalingHelper.SetLeft(ref dropEdit, parentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetWidth(ref dropEdit, parentStrip.FilterControlBoxWidth, true);
				ControlDpiScalingHelper.SetWidth(dropEdit.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
				dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				dropEdit.ShowDescriptionBox = true;
				dropEdit.TabIndex = 3;
				dropEdit.BindTo = "TypeProperty";
				bindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);

				parentStrip.SetHeight(dropEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2));

				parentStrip.Controls.Find("GripHolderPanel", false).Single().AllowOverlap(dateRangeControl);
				parentStrip.Controls.Find("FilterDescriptionDropEdit", false).Single().AllowOverlap(dateRangeControl);
				parentStrip.Controls.Find("DeleteStripButton", false).Single().AllowOverlap(dateRangeControl);

				return new Control[] { dropEdit, dateRangeControl };
			}
		}

		#endregion

		#region HasSalesRelationFilterControlBuilder

		public class HasSalesRelationFilterControlBuilder : ZFilterStrip.CustomFilterControlsBuilder
		{
			public override bool Handles(ModuleFilter moduleFilter)
			{
				return moduleFilter is ActivityHasSalesRelationFilter;
			}

			public override Control[] GetFilterControls(ZFilterStrip parentStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource)
			{
				var checkBox = new ZCheckBox();
				checkBox.CaptionResourceString = Res.GetData("16c1d565-fd02-4029-848b-c9e8b1a27ef1", "Yes");
				checkBox.AutoSize = true;
				ControlDpiScalingHelper.SetTop(ref checkBox, parentStrip.LabelTop, false);
				ControlDpiScalingHelper.SetLeft(ref checkBox, parentStrip.FilterControlsStart, true);
				checkBox.BindTo = "BoolProperty";
				checkBox.TabIndex = 1;
				bindingSource.SetBindingMember(checkBox, checkBox.BindTo);

				dropEdit = new ZDropEdit();
				dropEdit.CharacterCasing = CharacterCasing.Normal;
				ControlDpiScalingHelper.SetTop(ref dropEdit, parentStrip.FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref dropEdit, parentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetWidth(ref dropEdit, parentStrip.FilterControlBoxWidth, true);
				ControlDpiScalingHelper.SetWidth(dropEdit.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
				dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				dropEdit.ShowDescriptionBox = true;
				dropEdit.TabIndex = 2;
				dropEdit.BindTo = "TypeProperty";
				bindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);

				guidFindBox = new ZGuidFindBox();
				guidFindBox.AllowDrop = true;
				guidFindBox.CaptionResourceString = Res.GetData("efdb21f8-b570-4109-872a-1548cf0cc262", "ID");
				ControlDpiScalingHelper.SetTop(ref guidFindBox, dropEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ControlDpiScalingHelper.SetLeft(ref guidFindBox, parentStrip.FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetWidth(ref guidFindBox, parentStrip.FilterControlBoxWidth, true);
				ControlDpiScalingHelper.SetWidth(guidFindBox.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
				guidFindBox.ShowDescriptionBox = true;
				guidFindBox.TabIndex = 3;
				guidFindBox.BindTo = "BizObjPK";
				bindingSource.SetBindingMember(guidFindBox, guidFindBox.BindTo);

				parentFilterStrip = parentStrip;

				var hasSalesRelationFilter = (ActivityHasSalesRelationFilter)moduleFilter;
				hasSalesRelationFilter.TypePropertyInfo.ValueChanged += TypePropertyInfo_ValueChanged;
				RefreshControls(hasSalesRelationFilter);

				return new Control[] { guidFindBox, dropEdit, checkBox };
			}

			void TypePropertyInfo_ValueChanged(object sender, EventArgs e)
			{
				var hasSalesRelationFilter = (ActivityHasSalesRelationFilter)sender;
				RefreshControls(hasSalesRelationFilter);
			}

			void RefreshControls(ActivityHasSalesRelationFilter hasSalesRelationFilter)
			{
				guidFindBox.Visible = hasSalesRelationFilter.TypeProperty != AnySalesRelationTypeCode && hasSalesRelationFilter.TypeList.ContainsCode(hasSalesRelationFilter.TypeProperty);
				parentFilterStrip.SetHeight(guidFindBox.Visible ? guidFindBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2) : dropEdit.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2));
				guidFindBox.ModuleID = hasSalesRelationFilter.ModuleId;
			}

			ZFilterStrip parentFilterStrip;
			ZGuidFindBox guidFindBox;
			ZDropEdit dropEdit;
		}

		#endregion

		#endregion

		#endregion

		#region Lists

		public const string AnySalesRelationTypeCode = "ANY";

		public static ICodeDescriptionPairList GetSalesRelationTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AnySalesRelationTypeCode, ResString.GetMultilingualString("13427661-66a9-419a-bed6-cd06e149b620", "Any Sales Relation"));
			result.AddRange(SalesRelationTypeList.New());
			return result;
		}

		#endregion
	}
}
