using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
#pragma warning disable CW1147 //Baseline WI00653447
	public class ZReportFilterRow : HtmlTableRow
	{
		public ZReportFilterRow(FilterField filter)
		{
			Filter = filter;
			ID = Filter.PK.ToString();
		}

		#region CreateFilterControls

		public void CreateFilterControls()
		{
			if (Filter != null)
			{
				Cells.Add(LabelCell);
				Cells.Add(FilterCell);

				((Label)LabelCell.Controls[0]).Text = Filter.DisplayNameLocalized;
				AddFilterCellControls();
			}
		}

		void AddFilterCellControls()
		{
			if (FilterCell.Controls.Count == 0)
			{
				var controls = ZReportFilterControlProvider.CreateFilterControls(Filter);

				foreach (var control in controls)
				{
					FilterCell.Controls.Add(control);
					RegisterAsyncPostBackForUpdatingFilterControls(control);
				}
			}
		}

		void RegisterAsyncPostBackForUpdatingFilterControls(Control control)
		{
			var listControl = control as ListControl;
			if (listControl != null && Filter.IsUsedToDetermineReadOnlyOfRelatedFilter)
			{
				var scriptManager = ScriptManager.GetCurrent(Page);
				scriptManager.RegisterAsyncPostBackControl(listControl);
				listControl.AutoPostBack = true;
				listControl.SelectedIndexChanged += ListControl_SelectedIndexChanged;
			}
		}

		void ListControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			FireOnSelectedIndexChangedOfListControl();
		}

		#region FireOnSelectedIndexChangedOfListControl

		void FireOnSelectedIndexChangedOfListControl()
		{
			if (OnSelectedIndexChangedOfListControl != null)
			{
				OnSelectedIndexChangedOfListControl(this, EventArgs.Empty);
			}
		}

		internal event EventHandler OnSelectedIndexChangedOfListControl;

		#endregion

		#endregion

		public void BindFilterControls()
		{
			if (Filter != null)
			{
				foreach (Control control in FilterCell.Controls)
				{
					ISelfBindingWebControl bindControl = control as ISelfBindingWebControl;

					if (bindControl != null)
					{
						bindControl.Bind(Filter);
					}
				}
			}
		}

		#region LabelCell

		public string LabelCssClass
		{
			get { return LabelCell.Attributes["class"]; }
			set { LabelCell.Attributes["class"] = value; }
		}

		HtmlTableCell LabelCell
		{
			get
			{
				if (fLabelCell == null)
				{
					fLabelCell = new HtmlTableCell() { VAlign = (NoResString)"middle" };
					fLabelCell.Controls.Add(FilterLabel);
				}

				return fLabelCell;
			}
		}
		HtmlTableCell fLabelCell;

		Label FilterLabel
		{
			get { return fFilterLabel ?? (fFilterLabel = new Label()); }
		}
		Label fFilterLabel;

		#endregion

		public string FilterCssClass
		{
			get { return FilterCell.Attributes["class"]; }
			set { FilterCell.Attributes["class"] = value; }
		}

		#region FilterCell

		HtmlTableCell FilterCell
		{
			get { return fFilterCell ?? (fFilterCell = new HtmlTableCell() { VAlign = (NoResString)"middle" }); }
		}
		HtmlTableCell fFilterCell;

		#endregion

		#region FilterField

		protected FilterField Filter { get; private set; }

		#endregion

		#region Dispose

		public override void Dispose()
		{
			base.Dispose();
			UnhookOnListControlSelectedIndexChanged();
		}

		void UnhookOnListControlSelectedIndexChanged()
		{
			if (FilterCell != null && Filter != null)
			{
				foreach (Control control in FilterCell.Controls)
				{
					var listControl = control as ListControl;
					if (listControl != null && Filter.IsUsedToDetermineReadOnlyOfRelatedFilter)
					{
						listControl.SelectedIndexChanged -= ListControl_SelectedIndexChanged;
					}
				}
			}
		}

		#endregion
	}
#pragma warning restore CW1147
}
