using System;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class Reports : BasePageWithAuthorisation
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				ReportsLabel.Text = Res.GetString("bc95e069-06d6-4f38-901c-7a92922f92c5", "Reports");
				if (HttpContext.Current.Request.QueryString.HasKeys())
				{
					if (ReportMode != WebReportModes.All)
					{
						WRHController.UpdateBindingMembers(ReportMode);
						ReportsDropDownList.Bind(DataSource);
						ReportsLabel.Text = WRHController.ReportsCaption(ReportMode);
					}
				}
			}
			ReportsDropDownList.EmptyItemText = Res.GetString("e4104cda-ed06-44e6-bab2-cab3204cd111", "Please select a Report");
			SetupFilterControl();
		}

		public new ZString ContentType
		{
			get
			{
				return HttpContext.Current.Request.QueryString[TrackingConstants.QueryStringKeys.ContentType];
			}
		}

		public WebReportModes ReportMode
		{
			get
			{
				return WRHController.ReportMode(ContentType);
			}
		}

		#region DataSource

		protected WebReportHolderController WRHController
		{
			get
			{
				if (fWRHController == null)
				{
					fWRHController = new WebReportHolderController(DataSource);
				}
				return fWRHController;
			}
		}
		WebReportHolderController fWRHController;

		protected new WebReportHolder DataSource
		{
			get { return base.DataSource as WebReportHolder; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new WebReportHolder(Factory, SiteUser);
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject BusinessObjectToValidate
		{
			get
			{
				return DataSource.SelectedReport;
			}
		}

		#endregion

		#region Overrides

		protected override bool CanAccessAuthorisedContent
		{
			get
			{
				return SiteUser.CanViewReports(ReportMode);
			}
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Forwarding Reports"; } // Event logging
		}

		#endregion

		#region Report Filter Control setup

		protected ReportFilterControl FilterControl;

		void SetupFilterControl()
		{
			FilterControl = GetNewFilterControl();
			FilterControlHolder.Controls.Clear();
			FilterControlHolder.Controls.AddAt(0, FilterControl);
			FilterControl.SetupFilterControl(DataSource.SelectedReport);
		}

		protected virtual ReportFilterControl GetNewFilterControl()
		{
			ReportFilterControl result = Page.LoadControl(FilterControlResource.FileName) as ReportFilterControl;
			result.GroupTitleCssClass = CssConstants.ReportFilterGroupTitle;
			result.LabelCssClass = CssConstants.ReportFilterLabel;
			result.ValuesCssClass = CssConstants.ReportFilterValues;
			return result;
		}

		#endregion

		#region FilterControlResource

		protected ZWebResource FilterControlResource
		{
			get
			{
				if (fFilterControlResource == null)
				{
					fFilterControlResource = new ZWebResource(typeof(Reports), "ReportFilterControl.ascx", this, "Enterprise.Tracking.Web", typeof(ReportFilterControl).Assembly);
				}
				return fFilterControlResource;
			}
		}
		ZWebResource fFilterControlResource;

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(FilterControlResource);
				return result;
			}
		}

		#endregion

		protected void ReportsDropDownList_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupFilterControl();
			FilterControl.Bind(DataSource.SelectedReport);
		}
	}
}
