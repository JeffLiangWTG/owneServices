using System;
using System.Net;
using System.Text;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Login page for the web site
	/// </summary>
	public partial class Login : BasePage
	{
		public Login()
		{
			base.EnableViewState = false;
			LoginHelper = new TrackingLoginHelper(this);
		}

		readonly TrackingLoginHelper LoginHelper;

		public string DefaultTextBox { get; private set; }

		protected override bool IsDataSourceInSession { get { return false; } }

		protected override bool Cacheable => false;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			TrackingLoginManager result = new TrackingLoginManager();
			PresetUserNameAndCompany(result, WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.Value);

			return result;
		}

		void PresetUserNameAndCompany(TrackingLoginManager loginMan, bool isCompanyCodeRequired)
		{
			var sessionRefKey = HttpContext.Current.Request.QueryString[TrackingConstants.QueryStringKeys.RefKey];
			if (!string.IsNullOrEmpty(sessionRefKey))
			{
				var session = HttpContext.Current.Session;
				if (session != null && session[sessionRefKey] is LoginManager sessionLoginMan)
				{
					loginMan.CompanyCode = sessionLoginMan.CompanyCode;
					loginMan.UserName = sessionLoginMan.UserName;
					isCompanyCodeRequired = true;
				}
			}

			loginMan.IsCompanyCodeRequired = isCompanyCodeRequired || !string.IsNullOrEmpty(CompanyCodeTextBox.Text);
		}

		protected TrackingLoginManager LoginMan
		{
			get { return DataSource as TrackingLoginManager; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			if ((!IsPostBack && IsLogOffRequest) || SiteUser?.IsShipmentQuickViewUser == true)
			{
				LogOff();
			}

			base.OnLoad(e);

			LoginHelper.LoadLoginHashCookie();

			if (IsQuickViewContainerRequest)
			{
				LoginMan.ContainerQuickViewNumber = HttpContext.Current.Request.QueryString["ContainerQuickViewNumber"];
				if (!LoginHelper.TryContainertQuickView())
				{
					LogOff();
				}
			}

			string loginInstruction = WebDataRegistry.Instance.WebTrackerLoginPageInstruction.Value;
			if (!string.IsNullOrEmpty(loginInstruction))
			{
				LoginInstructionContent.Visible = true;
				LoginInstructionLabel.Text = loginInstruction.IndexOf("<br") == -1 ? loginInstruction.Replace(System.Environment.NewLine, @"<br />") : loginInstruction;
			}
			else
			{
				LoginInstructionContent.Visible = false;
			}

			QuickViewDetails.Visible = WebDataRegistry.Instance.WebTrackerShipmentQuickView.Value || WebDataRegistry.Instance.WebTrackerContainerQuickView.Value;
			ViewShipmentDetails.Visible = WebDataRegistry.Instance.WebTrackerShipmentQuickView.Value;
			ViewContainerDetails.Visible = WebDataRegistry.Instance.WebTrackerContainerQuickView.Value;

			CompanyCode.Visible = LoginMan.IsCompanyCodeRequired;
			DefaultTextBox = GetDefaultTextBoxClientID();
			ContainerNumberTextBox.MaxLength = LoginMan.ContainerQuickViewNumberInfo.MaxLength;

			if (SiteUser?.IsLoggedIn == true)
			{
				LoginHelper.RedirectToDefault();
			}
		}

		string GetDefaultTextBoxClientID()
		{
			if (LoginMan.IsCompanyCodeRequired && string.IsNullOrEmpty(CompanyCodeTextBox.Text))
			{
				return CompanyCodeTextBox.ClientID;
			}

			if (string.IsNullOrEmpty(LoginNameTextBox.Text))
			{
				return LoginNameTextBox.ClientID;
			}

			return PasswordTextBox.ClientID;
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			if (!IsPostBack && LoginMan != null && !IsLogOffRequest)
			{
				if ((LoginMan.HasErrors || LoginMan.UserName.IsEmpty) && !LoginMan.QuickViewNumber.IsEmpty)
				{
					NotificationFlags.DisplayErrors = false;
				}
				else if (LoginMan.HasErrors)
				{
					NotificationFlags.DisplayErrors = true;
				}
			}
		}

		bool IsQuickViewContainerRequest
		{
			get
			{
				return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ContainerQuickViewNumber"]);
			}
		}

		bool IsLogOffRequest
		{
			get
			{
				return HttpContext.Current.Request.QueryString["ClearSaved"] != null;
			}
		}

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			CompanyCodeTextBox.BindTo = "CompanyCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingLoginManager)(null)).CompanyCode)));
			LoginNameTextBox.BindTo = "UserName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingLoginManager)(null)).UserName)));
			PasswordTextBox.BindTo = "Password";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingLoginManager)(null)).Password)));
			Message.BindTo = "LoginErrorMsg";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingLoginManager)(null)).LoginErrorMsg)));
			QuickViewMessage.BindTo = "QuickViewErrorMsg";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingLoginManager)(null)).QuickViewErrorMsg)));
			this.DataSourceAssemblyName = "Enterprise.Tracking.Web";
			this.DataSourceTypeName = "Enterprise.Tracking.Web.TrackingLoginManager";
		}
		#endregion

		protected void SigninBtn_Click(object sender, EventArgs e)
		{
			this.NotificationFlags.DisplayErrors = true;

			if (WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.Value)
			{
				if (LoginMan.CompanyCode.IsEmpty || LoginMan.UserName.IsEmpty || LoginMan.Password.IsEmpty)
				{
					LoginMan.LoginErrorMsg = Res.GetString("69B105E6-C871-40C2-A22B-08515C3827EC", "Company code or Username or Password is empty!");
				}
			}
			else if (!WebDataRegistry.Instance.WebTrackerLoginRequiresCompanyCode.Value)
			{
				if (LoginMan.UserName.IsEmpty || LoginMan.Password.IsEmpty)
				{
					LoginMan.LoginErrorMsg = Res.GetString("7985A817-EA2E-4229-8FEA-28260A2181D9", "Username or Password is empty!");
				}
			}

			if (!LoginMan.HasErrors)
			{
				LoginHelper.SignInViaRouting();
			}
		}

		protected void FindBtn_Click(object sender, EventArgs e)
		{
			this.NotificationFlags.DisplayErrors = false;

			if (!LoginMan.QuickViewNumber.IsEmpty & !LoginMan.ContainerQuickViewNumber.IsEmpty)
			{
				if (!LoginHelper.TryShipmentQuickView(false) || !LoginHelper.TryContainertQuickView(false))
				{
					LoginMan.QuickViewErrorMsg = Res.GetString("64C27F53-B905-4A7F-8A90-CC61469AD1C5", "Shipment or container not Found!");
					DefaultTextBox = "ShipmentHousebillNumberTextbox";
				}
				else
				{
					var shipmentPk = LoginHelper.GetShipmentPk(LoginMan.QuickViewNumber);
					if (LoginHelper.AreShipmentAndContainerRelated(LoginMan.ContainerQuickViewNumber, shipmentPk))
					{
						if (!LoginHelper.TryShipmentQuickView())
						{
							DefaultTextBox = "ShipmentHousebillNumberTextbox";
						}
					}
					else
					{
						LoginMan.QuickViewErrorMsg = Res.GetString("9D855509-7EAF-42DE-9EBD-840F26F76558", "Container {0} is not registered against Shipment {1}. Please try another search or try to search only by Shipment or only by Container number.", LoginMan.ContainerQuickViewNumber, LoginMan.QuickViewNumber);
						DefaultTextBox = "ContainerNumberTextBox";
					}
				}
			}
			else if (!LoginMan.QuickViewNumber.IsEmpty && LoginMan.ContainerQuickViewNumber.IsEmpty)
			{
				if (!LoginHelper.TryShipmentQuickView())
				{
					DefaultTextBox = "ShipmentHousebillNumberTextbox";
				}
			}
			else if (!LoginMan.ContainerQuickViewNumber.IsEmpty && WebDataRegistry.Instance.WebTrackerContainerQuickView.Value && LoginMan.QuickViewNumber.IsEmpty)
			{
				if (!LoginHelper.TryContainertQuickView())
				{
					DefaultTextBox = "ContainerNumberTextBox";
				}
			}
			else
			{
				LoginMan.QuickViewErrorMsg = Res.GetString("BE5281DE-0C56-4761-892B-0B3022907A80", "Please enter either Shipment Number or Container Number.");
				DefaultTextBox = "ShipmentHousebillNumberTextbox";
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			Message.Bind(LoginMan);
			QuickViewMessage.Bind(LoginMan);

			if (Page != null)
			{
				((ZPage)Page).DisablePageBusinessObjectValidation = true;
			}

			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SigninBtn.ClientID);
			DisabledSubmitButtons.Add(FindBtn.ClientID);
		}

		protected void PasswordReminderPrerender(object sender, EventArgs e)
		{
			StringBuilder url = new StringBuilder(AppInstance.ForgotPasswordPage);
			if (!LoginMan.UserName.Trim().IsEmpty)
			{
				url.AppendFormat((NoResString)"?Email={0}", WebUtility.UrlEncode((LoginMan.UserName.Trim()))); // Partial URL
			}
			PasswordReminder.NavigateUrl = url.ToString();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.Login;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LoginPage;
		}
	}
}
