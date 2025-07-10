using System;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Web
{
	public partial class ProductImage : WarehousingBasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SetupButtons();
			NotificationFlags.DisplayAll = false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPage();
		}

		void SetupPage()
		{
			var isProductValid = (Product != null);

			ProductImageContents.Visible = isProductValid;
			NotFoundError.Visible = !isProductValid;
			NotFoundLabel.Text = Res.GetString("8ecfedfa-9c87-4328-871d-3c1357e5cd46", "Product was not found or you don't have rights to view it.");

			var eDocImage = Product.ProductImage;
			if (eDocImage != null)
			{
				SetProductImageIfAny(eDocImage);
			}
		}

		void SetProductImageIfAny(IeDoc productImage)
		{
			this.ProductImageControl.ImageUrl = string.Format((NoResString)"{0}?Ref={1}&Doc={2}", Enterprise.DocumentScanning.Web.eDocsRequestHandler.RequestHelper.BaseUrl, Product.Part.PK.ToString(), productImage.UniqueKey.ToString());
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ProductImagePage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewWarehouseProducts; }
		}

		#region Overrides

		protected override string PageHeaderControlPath
		{
			get { return ""; }
		}

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		protected override bool ShowCloseWindowInPopup
		{
			get { return false; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(CloseWindow.ClientID);
		}

		#endregion

		#region Factory

		public new BusinessObjectFactory Factory
		{
			get
			{
				return ProductPK.IsValid && HttpContext.Current.Session[ProductPK.ToString()] != null ?
					((BusinessObject)HttpContext.Current.Session[ProductPK.ToString()]).Factory : base.Factory;
			}
		}

		#endregion

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingSupplierPart result = null;
			if (ProductPK.IsValid)
			{
				result = TrackingSupplierPart.FromPKFilteredByContact(Factory, ProductPK, SiteUser);
			}

			return result;
		}

		protected
#if DEBUG
 virtual
#endif
 TrackingSupplierPart Product
		{
			get { return DataSource as TrackingSupplierPart; }
		}

		#endregion BusinessObject

		#region ProductPK

		protected ZGuid ProductPK
		{
			get { return GetGuidFromParameter("Ref"); }
		}

		#endregion

		#region Setup Buttons

		void SetupButtons()
		{
			this.CloseWindow.OnClientClick = (NoResString)"javascript: self.close();"; // javascript code
		}

		#endregion

		#region Save Button Click Handlers

		protected void Save_Click(object sander, EventArgs e)
		{
			RegisterClosingScript();
		}

		void RegisterClosingScript()
		{
			if (
#if DEBUG
!Globals.IsTest &&
#endif
 Request.Browser.EcmaScriptVersion.Major >= 1 && !ZClientScript.IsClientForEventScriptBlockRegistered((NoResString)"window", (NoResString)"onload", GetType(), "OnLoad")) // Programmatic constant
			{
				ZClientScript.RegisterClientForEventScriptBlock((NoResString)"window", (NoResString)"onload", GetType(), "OnLoad", (NoResString)"javascript: self.close ()"); // Programmatic constant
			}
		}

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.ProductImage;
		}
	}
}
