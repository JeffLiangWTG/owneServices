using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public abstract class AWBFieldControl : CompositeControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBFieldControl()
			: base()
		{
			CaptionVisible = true;
			FieldCaptionVisible = true;
		}

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(HorizontalAlign.Left), Browsable(true)]
		public HorizontalAlign CaptionAlign { get; set; }

		[Category("Appearance"), DefaultValue(HorizontalAlign.Left), Browsable(true)]
		public HorizontalAlign DataFieldAlign { get; set; }

		[Category("Appearance"), DefaultValue(true), Browsable(true)]
		public bool CaptionVisible { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string FieldCssClass { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string FieldCaption { get; set; }

		[Category("Appearance"), DefaultValue(true), Browsable(true)]
		public bool FieldCaptionVisible { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string Caption { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			FieldBox = GetNewFieldBox();
			FieldBox.ID = (NoResString)"Field";

			FieldCaptionLabel = new ZTextLabel();
			FieldCaptionLabel.ID = "FieldCaption";

			ControlCaption = new ZTextLabel();
			ControlCaption.ID = (NoResString)"Caption";
		}

		protected abstract WebControl GetNewFieldBox();

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			if (FieldBox is ISelfBindingWebControl)
			{
				((ISelfBindingWebControl)FieldBox).BindTo = BindTo;
			}
			FieldBox.CssClass = FieldCssClass;
			FieldCaptionLabel.Text = FieldCaption;
			FieldCaptionLabel.Visible = FieldCaptionVisible;

			ControlCaption.Text = Caption;
			ControlCaption.Visible = CaptionVisible;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			TableRow rowCaption = new TableRow();
			TableCell cellCaption = new TableCell();
			cellCaption.Controls.Add(ControlCaption);
			cellCaption.CssClass = "AWBSectionTitle";
			cellCaption.HorizontalAlign = CaptionAlign;
			rowCaption.Cells.Add(cellCaption);
			layoutTable.Rows.Add(rowCaption);
			rowCaption.Visible = CaptionVisible;

			TableRow row1 = new TableRow();
			TableCell cell11 = new TableCell();
			cell11.CssClass = "AWBCaption";
			cell11.Controls.Add(FieldCaptionLabel);
			TableCell cell12 = new TableCell();
			cell12.HorizontalAlign = DataFieldAlign;
			cell12.CssClass = "AWBSectionData";
			cell12.Controls.Add(FieldBox);
			if (FieldCaptionVisible)
			{
				cellCaption.ColumnSpan = 2;
				row1.Cells.Add(cell11);
			}
			else
			{
				cell12.CssClass = "AWBSectionDataNoCaption";
			}
			row1.Cells.Add(cell12);
			layoutTable.Rows.Add(row1);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		protected WebControl FieldBox;
		protected ZTextLabel FieldCaptionLabel;

		ZTextLabel ControlCaption;

		#endregion

		#region ISelfBindingWebControl Members

		public virtual bool IsBindable(object dataSource)
		{
			return ((!string.IsNullOrEmpty(BindTo) && dataSource != null && dataSource is BusinessObject));
		}

		public void Bind(object dataSource)
		{
			UnBind();
			if (dataSource is IBusiness)
			{
				new ZWebControlBinder((IBusiness)dataSource).Bind(Controls);
			}
			DataBind();
		}

		public void UnBind()
		{
			BusinessEntity = null;
			DataBind();
		}

		protected ExportAWBHeader BusinessEntity
		{
			get { return businessEntity; }
			set { businessEntity = value; }
		}

		ExportAWBHeader businessEntity;

		#endregion

		#region IBindTo Members

		public string BindTo { get; set; }

		#endregion

#if DEBUG
		public void InitializeForTesting()
		{
			OnInit(new EventArgs());
			InitializeChildControlsForTesting(Controls);
			CreateChildControls();
		}

		void InitializeChildControlsForTesting(ControlCollection controls)
		{
			foreach (Control childControl in controls)
			{
				if (childControl is AWBFieldControl)
				{
					((AWBFieldControl)childControl).InitializeForTesting();
				}
				if (childControl is AWBBaseControl)
				{
					((AWBBaseControl)childControl).InitializeForTesting();
				}
				InitializeChildControlsForTesting(childControl.Controls);
			}
		}
#endif
	}
}
