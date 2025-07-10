using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:EventsControl runat=server></{0}:EventsControl>")]
	public class EventsControl : CompositeControl, ISelfBindingWebControl, IPostBackDataHandler
	{
		#region Constructors

		public EventsControl()
			: base()
		{ }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue("Events"), Browsable(true)]
		public string Caption
		{
			get { return caption; }
			set { caption = value; }
		}
		string caption = Res.GetString("118f5606-217b-4c75-b6f5-f0b535506b75", "Events");

		[Category("Appearance"), DefaultValue("DetailsCell"), Browsable(true)]
		public string ItemCssClass
		{
			get { return itemCssClass; }
			set { itemCssClass = value; }
		}
		string itemCssClass = "DetailsCell";

		[Category("Appearance"), DefaultValue("DetailsHeader"), Browsable(true)]
		public string HeaderCssClass
		{
			get { return headerCssClass; }
			set { headerCssClass = value; }
		}
		string headerCssClass = "DetailsHeader";

		public override string CssClass
		{
			get
			{
				return cssClass;
			}
			set
			{
				cssClass = value;
			}
		}
		string cssClass = "DetailsTable";

		[Category("Appearance"), DefaultValue("SectionTitle"), Browsable(true)]
		public string PanelCssClass
		{
			get { return panelCssClass; }
			set { panelCssClass = value; }
		}
		string panelCssClass = "SectionTitle";

		[Category("Appearance"), DefaultValue("False"), Browsable(true)]
		public bool DisplayInPanel
		{
			get { return displayInPanel; }
			set { displayInPanel = value; }
		}
		bool displayInPanel;

		#endregion

		#region Declaration

		ZGrid eventsGrid;
		ZCollapsablePanel OutterPanel;

		#endregion

		#region Overrides

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (Controls.Count > 0)
			{
				Controls[0].RenderControl(writer);
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			eventsGrid.Visible = BusinessEntity != null && BusinessEntity.CanViewTrackingEvents;
			if (DisplayInPanel)
			{
				OutterPanel.Visible = eventsGrid.Visible;
			}
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Clear();
			if (DisplayInPanel)
			{
				OutterPanel.Controls.Add(eventsGrid);
				Controls.Add(OutterPanel);
			}
			else
			{
				Controls.Add(eventsGrid);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			OutterPanel = new ZCollapsablePanel();
			OutterPanel.ID = ID + (NoResString)"Panel";
			OutterPanel.Label = Caption;
			OutterPanel.CssClass = PanelCssClass;
			OutterPanel.DisableCollapsing = true;

			eventsGrid = new ZGrid();
			eventsGrid.ID = ID + (NoResString)"Grid";
			eventsGrid.BindTo = BindTo;
			eventsGrid.AllowAdd = false;
			eventsGrid.AllowEdit = false;
			eventsGrid.AllowDelete = false;
			eventsGrid.Caption = String.Empty;
			eventsGrid.CssClass = CssClass;
			eventsGrid.AutoGenerateColumns = false;
			eventsGrid.DisableCollapsing = true;
			eventsGrid.ItemStyle.CssClass = ItemCssClass;
			eventsGrid.HeaderStyle.CssClass = HeaderCssClass;
			eventsGrid.Header.Visible = false;
			eventsGrid.ShowCustomizeColumnsButton = true;
		}

		#endregion

		#region ISelfBindingWebControl Members

		public void ReBind()
		{
			Bind(BusinessEntity);
		}

		public bool IsBindable(object dataSource)
		{
			return eventsGrid.IsBindable(dataSource);
		}

		public void Bind(object dataSource)
		{
			if (BusinessEntity == null)
			{
				eventsGrid.ColumnProvider = new EventsColumnProvider();
			}
			if (IsBindable(dataSource))
			{
				if (dataSource is ITrackingEventsProvider)
				{
					BusinessEntity = (ITrackingEventsProvider)dataSource;
				}
				eventsGrid.Bind(dataSource);
			}
		}

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		public void UnBind()
		{
			eventsGrid.UnBind();
		}

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		#endregion

		#region Implementation

		protected ITrackingEventsProvider BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}
		ITrackingEventsProvider fBusinessEntity;

		#endregion

		#region IPostBackDataHandler Members

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			throw new NotImplementedException();
		}

		public void RaisePostDataChangedEvent()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
