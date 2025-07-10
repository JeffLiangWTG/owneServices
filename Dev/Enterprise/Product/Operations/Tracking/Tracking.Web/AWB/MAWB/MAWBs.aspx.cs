using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class MAWBs : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return searchControl.Module.CreateNewFilterBusinessObject();
		}

		#endregion

		#region Page Setup

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		#endregion

		#region SearchControl

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.TrackingMAWB;
			searchControl.IsNewButtonVisible = false;

			searchControl.CustomControl = UploadButton;

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected ZButton UploadButton
		{
			get
			{
				if (uploadButton == null)
				{
					uploadButton = new ZButton()
					{
						Text = Res.GetString("e02c0436-fae3-4095-b1f5-edb2f995f4a0", "New Upload"),
						// JavaScript Code
						OnClientClick = string.Format((NoResString)"javascript:window.open('{0}', '{1}', '{2}');return(false);", String.Format("{0}?{1}={2}", AppInstance.MAWBUploadPage, TrackingConstants.QueryStringKeys.PopupKey, "Y"), (NoResString)"_blank", Global.MAWBUploadPopupWindowStyle) // JavaScript Code
					};
				}
				return uploadButton;
			}
		}

		ZButton uploadButton;

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetControlKeyIdentifier(System.Web.UI.Control control)
		{
			return searchControl.ModuleID.Name;
		}

		#endregion

		#region Overrides

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
			SetupSearchControl();
		}

		void InitializeComponent()
		{
			PageTitleLabel.BindTo = null;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.MAWBs;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewMAWB; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return "TrackingMAWB"; } // Event logging
		}

		#endregion
	}
}
