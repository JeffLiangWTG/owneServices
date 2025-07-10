using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
#pragma warning disable CW1147 //Baseline WI00653447
	public partial class ReportFilterControl : BaseUserControl, ISelfBindingWebControl
	{
		public void SetupFilterControl(Report report)
		{
			Report = report;

			if (Report != null)
			{
				ID = Report.PK.ToString();
				AddFilterFields();
			}
			else
			{
				Visible = false;
			}
		}

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			ColumnConfigLabel.Text = Res.GetString("7baacd36-29ce-4dfe-b731-2fdb2dde7e45", "Select a Report Configuration");
			SortLabel.Text = Res.GetString("6aa33cb7-0511-4df8-943e-65505fafccad", "Sort the report in this order");
			GroupByLabel.Text = Res.GetString("c4dbef15-9618-45a3-ae34-f1b715ec591e", "Group Bys in the report");
			PageBreakOnNewGroup.Text = Res.GetString("3dbbe0eb-9750-4fb4-ac19-9a0b464a0c10", "Page Break on New group");
			OptionalTemplateLabel.Text = Res.GetString("e01c8057-ce66-4757-8a8f-63667052c662", "Optional templates");
			FormatTypeLabel.Text = Res.GetString("8ef7acc3-adcd-4165-944f-731cfc4722d8", "Format type");
			ReportLanguageLabel.Text = Res.GetString("098a3e6e-b084-4fc4-a78d-7be686afa695", "Report Language");
			RunReportButton.Text = Res.GetString("920e2a2d-7212-44b4-a6c2-03b103e0f7fd", "Run Report");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Page.NotificationFlags.DisplayAll = false;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			BindFilters();
		}

		#endregion

		#region Data Source

		Report Report { get; set; }

		#endregion

		#region Styles

		const string FilterGroupTitleCssClass = "GroupTitleClass";
		const string FilterLabelCssClass = "LabelClass";
		const string FilterValuesCssClass = "ValuesClass";

		public string GroupTitleCssClass
		{
			get { return ViewState[FilterGroupTitleCssClass] as string; }
			set { ViewState[FilterGroupTitleCssClass] = value; }
		}

		public string LabelCssClass
		{
			get { return ViewState[FilterLabelCssClass] as string; }
			set { ViewState[FilterLabelCssClass] = value; }
		}

		public string ValuesCssClass
		{
			get { return ViewState[FilterValuesCssClass] as string; }
			set { ViewState[FilterValuesCssClass] = value; }
		}

		#endregion

		#region ISelfBindingWebControl

		public string BindTo { get; set; }

		public bool IsBindable(object dataSource)
		{
			return Report != null;
		}

		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				BindColumnLayout();
				BindFilters();
				BindSortOrder();
				BindGroupBys();
				BindOptionalTemplates();
				BindFormatType();
				BindLanguages();
			}
		}

		public void UnBind()
		{
			Report = null;
		}

		#endregion

		#region AddFilterFields

		void AddFilterFields()
		{
			if (Report != null)
			{
				ZReportFilterControlProvider.SetWebUserRestrictingValues(Report, (OrgContactWebUser)Page.SiteUser);

				var fieldsGroupedByTab = new Dictionary<ZString, List<FilterField>>();

				foreach (FilterField filter in Report.FilterCollection)
				{
					if (!fieldsGroupedByTab.ContainsKey(filter.GroupName))
					{
						fieldsGroupedByTab.Add(filter.GroupName, new List<FilterField>());
					}

					fieldsGroupedByTab[filter.GroupName].Add(filter);
				}

				UpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
				UpdatePanel.ContentTemplateContainer.Controls.Add(FilterTable);
				Controls.AddAt(0, UpdatePanel);

				foreach (var tabName in fieldsGroupedByTab.Keys)
				{
					AddHeadingRow(tabName);

					foreach (var filter in fieldsGroupedByTab[tabName])
					{
						if (!ZReportFilterControlProvider.ShouldBeHiddenOnWeb(Report, filter) && ZReportFilterControlProvider.IsSupportedOnWeb(filter))
						{
							var row = new ZReportFilterRow(filter) { LabelCssClass = this.LabelCssClass, FilterCssClass = this.ValuesCssClass };
							FilterTable.Rows.Add(row);
							row.CreateFilterControls();
							row.OnSelectedIndexChangedOfListControl += OnSelectedIndexChangedOfListControl;
						}
					}
				}
			}
		}

		void AddHeadingRow(ZString heading)
		{
			ZString labelText = !heading.IsEmpty ? heading : (ZString)Res.GetString("0046173f-5828-46c0-8c53-87a9f1179507", "Primary Filter Options");

			if (!labelText.IsEmpty)
			{
				HtmlTableCell blankCell = new HtmlTableCell();
				blankCell.Controls.Add(new ZTextLabelWhiteSpace(1));
				HtmlTableRow blankRow = new HtmlTableRow();
				blankRow.Cells.Add(blankCell);

				HtmlTableCell cell = new HtmlTableCell() { ColSpan = 2 };
				cell.Controls.Add(new ZTextLabel { BindTo = null, Text = labelText, CssClass = this.GroupTitleCssClass });
				HtmlTableRow row = new HtmlTableRow();
				row.Cells.Add(cell);

				FilterTable.Rows.Add(blankRow);
				FilterTable.Rows.Add(row);
			}
		}

		void OnSelectedIndexChangedOfListControl(object sender, EventArgs e)
		{
			UpdateFilterControls();
		}

		void UpdateFilterControls()
		{
			UpdatePanel.Update();
		}

		#region UpdatePanel

		UpdatePanel UpdatePanel
		{
			get { return updatePanel ?? (updatePanel = new UpdatePanel()); }
		}

		UpdatePanel updatePanel;

		#endregion

		#endregion

		#region BindControls

		void BindColumnLayout()
		{
			((IWebReport)Report).CurrentOrgPK = ((OrgContactWebUser)Page.SiteUser).CurrentOrg;
			ColumnConfigLabel.CssClass = GroupTitleCssClass;
			ColumnConfigDropDownList.BindToList = "ColumnLayoutList";
			ColumnConfigDropDownList.BindTo = "SelectedColumnLayout";
			ColumnConfigDropDownList.DataTextField = (NoResString)"Description";
			ColumnConfigDropDownList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			ColumnConfigDropDownList.DataValueField = (NoResString)"Code";
			ColumnConfigDropDownList.CssClass = ValuesCssClass;
			ColumnConfigDropDownList.Bind(Report);
			ColumnConfigDropDownList.Enabled = ColumnConfigDropDownList.Items.Count > 1;
			ColumnConfigDIV.Visible = !((IWebReport)Report).IsOnlyCompanyDefaultLayout;
		}

		void BindFilters()
		{
			foreach (HtmlTableRow row in FilterTable.Rows)
			{
				ZReportFilterRow filterRow = row as ZReportFilterRow;

				if (filterRow != null)
				{
					filterRow.BindFilterControls();
				}
			}
		}

		void BindSortOrder()
		{
			SortDIV.Visible = Report.SortOrderCollection.Count > 0;
			SortLabel.CssClass = GroupTitleCssClass;
			SortOrderList.BindTo = "SortOrderCollection";
			SortOrderList.CssClass = ValuesCssClass;
			SortOrderList.Bind(Report);
		}

		void BindGroupBys()
		{
			GroupByDIV.Visible = Report.GroupByCollection.Count > 0;
			GroupByLabel.CssClass = GroupTitleCssClass;
			GroupBysList.BindTo = "GroupByCollection";
			GroupBysList.CssClass = this.ValuesCssClass;
			GroupBysList.Bind(Report);

			PageBreakOnNewGroup.BindTo = "BreakPageOverride";
			PageBreakOnNewGroup.CssClass = ValuesCssClass;
			PageBreakOnNewGroup.Bind(Report.GroupByCollection);
		}

		void BindOptionalTemplates()
		{
			OptionalTemplateDIV.Visible = Report.OptionalTemplateSheetCollection.Count > 0;
			OptionalTemplateLabel.CssClass = GroupTitleCssClass;
			OptionalTemplateList.BindTo = "OptionalTemplateSheetCollection";
			OptionalTemplateList.CssClass = ValuesCssClass;
			OptionalTemplateList.Bind(Report);
		}

		void BindFormatType()
		{
			FormatTypeLabel.CssClass = GroupTitleCssClass;
			FormatTypeDropDownList.BindToList = "FormatType_List";
			FormatTypeDropDownList.BindTo = "SelectedFormatType";
			FormatTypeDropDownList.DataTextField = (NoResString)"Description";
			FormatTypeDropDownList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			FormatTypeDropDownList.DataValueField = (NoResString)"Code";
			FormatTypeDropDownList.CssClass = ValuesCssClass;
			FormatTypeDropDownList.Bind(Report);
		}

		void BindLanguages()
		{
			ReportLanguageLabel.CssClass = GroupTitleCssClass;
			LanguageDropDownList.BindToList = "Languages";
			LanguageDropDownList.BindTo = "SelectedLanguage";
			LanguageDropDownList.DataTextField = (NoResString)"Description";
			LanguageDropDownList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			LanguageDropDownList.DataValueField = (NoResString)"Code";
			LanguageDropDownList.CssClass = ValuesCssClass;
			LanguageDropDownList.Bind(Report);

			var currentLanguage = Res.CurrentLanguage;
			if (Res.IsEnglish(currentLanguage))
			{
				currentLanguage = Core.Constants.Languages.EnglishAmerican;
			}

			LanguageDropDownList.SelectedValue = Report.Languages[currentLanguage, StringComparison.OrdinalIgnoreCase]?.Code;
		}

		#endregion

		#region Run Report Click

		protected void RunReportButton_Click(object sender, EventArgs e)
		{
			if (Page == null)
			{
				return;
			}

			try
			{
				BindFilters();
				Page.NotificationFlags.DisplayAll = true;
				Page.Validate();

				if (Report != null && !Report.HasErrors)
				{
					if (Report.Parent != null)
					{
						Report.Parent.Language = Report.SelectedLanguage;
					}

					EventLogHelper eventLogHelper = new EventLogHelper();
					eventLogHelper.CreateLogForReportPrinted(SiteUser as OrgContactWebUser, Report.Name);

					HttpContext.Current.Session[Report.PK.ToString()] = Report;
					Response.Redirect(ReportRequestHandler.RequestHelper.GetHandlerUrl(new[] { Report.PK }));
				}
			}
			catch (NullReferenceException)
			{
				var message = FormattableString.Invariant($@"There's a null reference exception occurred while running the report below:
Report Name: {Report.Name}
Report Path: {Report.MenuItem?.SU_MenuPath}
Report Filter: {Report.MenuItem?.SU_FilterList}
Is System Defined: {Report.MenuItem?.SU_IsSystemDefined}
Is Client Specific: {Report.MenuItem?.SU_IsClientSpecific}
Is FilterTable Null: {FilterTable == null}
Is Page Null: {Page == null}
Is Current Context Null: {HttpContext.Current == null}
Is Session Null: {HttpContext.Current?.Session == null}
");
				throw new InvalidOperationException(message);
			}
		}

		#endregion

		#region Dispose

		public override void Dispose()
		{
			base.Dispose();
			UnhookOnSelectedIndexChangedOfListControl();
		}

		void UnhookOnSelectedIndexChangedOfListControl()
		{
			if (FilterTable != null)
			{
				foreach (HtmlTableRow row in FilterTable.Rows)
				{
					var reportFilterRow = row as ZReportFilterRow;
					if (reportFilterRow != null)
					{
						reportFilterRow.OnSelectedIndexChangedOfListControl -= OnSelectedIndexChangedOfListControl;
					}
				}
			}
		}

		#endregion
	}
#pragma warning restore CW1147
}
