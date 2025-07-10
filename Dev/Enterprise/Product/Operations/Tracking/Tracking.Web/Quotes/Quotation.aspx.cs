using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web.Quotes
{
	/// <summary>
	/// Summary description for Quotation.
	/// </summary>
	public partial class Quotation : BasePageWithAuthorisation
	{
		#region JobDocAddresControl Setup

		void SetupConsignorAddressControl()
		{
			ConsignorAddress.PostalCodeChanged += new EventHandler(ConsignorAddress_PostalCodeChanged);
		}

		void SetupConsigneeAddressControl()
		{
			ConsigneeAddress.PostalCodeChanged += new EventHandler(ConsigneeAddress_PostalCodeChanged);
		}

		#endregion
		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			SetupLooseCargoDataGrid();
			base.OnLoad(e);

			SetupGridsVisibility();
			IncoTermLabel.Text = SpotQuote.IsDomesticFreight ? Res.GetString("2399d4a5-92e9-4c80-bfa5-536fd641bef7", "Payment Term") : Res.GetString("bffe327e-706a-f789-422b-e35039eb8cc5", "Incoterm");

			EmailNotice.Visible = false;
			NotificationFlags.DisplayAll = false;

			if (IsPostBack && SpotQuote.Job != null)
			{
				Job spotQuoteJob = (Job)SpotQuote.Job;
				spotQuoteJob.SetDefaultBranch(SpotQuote);
				spotQuoteJob.SetDefaultDepartment(SpotQuote);
			}

			if (IsPostBack && (SpotQuote.IsCompareMode || SpotQuote.IsCompareServiceLevel))
			{
				SaveQuote.Enabled = false;
			}

			InsuranceValueRow.Visible = !WebDataRegistry.Instance.DisableQuoteInsuranceValue.Value;

			if (SpotQuote.ComparisonQuoteResults.Count == 1)
			{
				SpotQuote.ComparisonQuoteResults.RemoveAndDeleteAll();
			}

			SetupComparisonControlsVisibility();
			SetupClearEstimatesButton();
			SetupQuoteEstimatesControl();
		}

		void SetupQuoteEstimatesControl()
		{
			ComparisonQuotes.Visible = false;
			if (SpotQuote != null)
			{
				if (SpotQuote.ComparisonQuoteResults.Count > 1)
				{
					ComparisonQuotes.Visible = true;
				}
			}
		}

		void SetupClearEstimatesButton()
		{
			ClearEstimates.Visible = false;
			DisableControl(EditControls, false);
			if (SpotQuote != null)
			{
				if (SpotQuote.ComparisonQuoteResults.Count > 1)
				{
					ClearEstimates.Visible = true;
					DisableControl(EditControls, true);
				}
			}
		}

		void SetupComparisonControlsVisibility()
		{
			TansportModeComparison.Visible = false;
			ServiceLevelComparison.Visible = false;
			if (SpotQuote != null)
			{
				if (SpotQuote.IsCompareMode)
				{
					TansportModeComparison.Visible = true;
				}
				if (SpotQuote.IsCompareServiceLevel)
				{
					ServiceLevelComparison.Visible = true;
				}
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewQuotations; }
		}

		#endregion OnLoad

		#region OnPreRender

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RefreshChargeable();
			IsDomesticHiddenField.Value = SpotQuote.IsDomesticFreight ? "Y" : "N";
			if (SpotQuote.ComparisonQuoteResults.Count > 0)
			{
			}
		}

		#endregion OnPreRender

		#region Scripts

		protected override string GetPageAnchorName()
		{
			return ComparisonQuotes.AnchorName;
		}

		protected override void RenderPageSpecificScripts()
		{
			base.RenderPageSpecificScripts();

			RenderHelperFunctionsScript();
		}

		#region HelperFunctionsScript

		const string HelperFunctionsScriptKey = "Quotation_HelperFunctionsScript";

		void RenderHelperFunctionsScript()
		{
			AddHelperFunctionsScript();
		}

		protected virtual void AddHelperFunctionsScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), HelperFunctionsScriptKey))
			{
				string script = @"<SCRIPT>

						function getOriginPortValue()
						{
							var originPort = getElement('" + OriginPort.TextBoxControl.ClientID + @"');
							if (originPort)
							{
								return originPort.get('value');
							}
							return '';
						}

						function getDestinationPortValue()
						{
							var destinationPort = getElement('" + DestinationPort.TextBoxControl.ClientID + @"');
							if (destinationPort)
							{
								return destinationPort.get('value');
							}
							return '';
						}

						function getIsDomesticValue()
						{
							var isDomesticHiddenField = getElement('" + IsDomesticHiddenField.ClientID + @"');
							if (isDomesticHiddenField)
							{
								return (isDomesticHiddenField.get('value') == 'Y');
							}
							return false;
						}            

						function calculateIsDomesticFreight()
						{
							return getOriginPortValue().substr(0,2)==getDestinationPortValue().substr(0,2);
						}

						function portChangeRequiresPostBack()
						{
							return calculateIsDomesticFreight()!=getIsDomesticValue();
						}

						</SCRIPT>";

				ZClientScript.RegisterClientScriptBlock(GetType(), HelperFunctionsScriptKey, script);
			}
		}

		#endregion

		#endregion

		#region Controls With Postback Condition Setup

		protected override void SetupPostBackConditionControls()
		{
			base.SetupPostBackConditionControls();

			OriginPort.PostbackCondition = (NoResString)"return portChangeRequiresPostBack();"; // Javascript code
			DestinationPort.PostbackCondition = (NoResString)"return portChangeRequiresPostBack();"; // Javascript code
		}

		#endregion

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			QuotedBooking result = null;
			if (QuoteCompany.PK != GlbCompany.CurrentCompany.PK)
			{
				using (WebLoginBranch quoteBranch = new WebLoginBranch(QuoteCompany.Branches[0]))
				{
					result = TrackingQuotedBooking.GetNewQuotation(Factory, SiteUser);
				}
			}
			else
			{
				result = TrackingQuotedBooking.GetNewQuotation(Factory, SiteUser);
			}
			result.Quote.TH_GC = QuoteCompany.PK;

			return result;
		}

		const string QuoteCompanyKey = "QuoteCompany";

		GlbCompany QuoteCompany
		{
			get
			{
				if (quoteCompany == null)
				{
					quoteCompany = GlbCompany.CurrentCompany;
					if (HttpContext.Current.Request.QueryString[QuoteCompanyKey] != null)
					{
						ZGuid companyPK;
						if (ZGuid.TryParse(HttpContext.Current.Request.QueryString[QuoteCompanyKey], out companyPK) &&
							companyPK != GlbCompany.CurrentCompany.PK)
						{
							quoteCompany = Factory.Load<GlbCompany>(companyPK);
						}
					}
				}
				return quoteCompany;
			}
		}
		GlbCompany quoteCompany;

		protected internal QuotedBooking SpotQuote
		{
			get { return DataSource as QuotedBooking; }
		}

		TrackingQuotationHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TrackingQuotationHelper(Factory, SiteUser.LoggedInOrgContact, SpotQuote);
					fHelper.QuoteUpdated += new EventHandler(OneOffQuote_QuoteUpdated);
				}
				return fHelper;
			}
		}
		TrackingQuotationHelper fHelper;

		protected override BusinessObject BusinessObjectToValidate
		{
			get { return SpotQuote; }
		}

		#endregion DataSource

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			SetupConsignorAddressControl();
			SetupConsigneeAddressControl();

			IsCompareModeCheckBox.PostDataChanged += new EventHandler(TransportModeCompare_Click);
			IsCompareServiceLevelCheckBox.PostDataChanged += new EventHandler(ServiceLevelCompare_Click);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>l
		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
			ShipmentDetailsTitle.BindTo = null;
			GoodsDetailsTitle.BindTo = null;

			// This used to be done by the generator - now this is maintained manually
			// Compile time check for the BindTos. 
			// If this line fails, Modify this code, then check that the binding still works on the page.

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).Mode)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).ModeInfo)));

			ZBindToChecker.CheckBindTo(((INumericZType)(((QuotedBooking)(null)).Weight)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).WeightInfo)));

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).WeightUnit)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).WeightUnitInfo)));

			ZBindToChecker.CheckBindTo(((INumericZType)(((QuotedBooking)(null)).Volume)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).VolumeInfo)));

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).VolumeUnit)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).VolumeUnitInfo)));

			ZBindToChecker.CheckBindTo(((INumericZType)(((QuotedBooking)(null)).Chargeable)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).ChargeableInfo)));

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ChargeableUnit)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).ChargeableUnitInfo)));

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ServiceLevel)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).ServiceLevelInfo)));

			ZBindToChecker.CheckBindTo(((ZGuid)(((QuotedBooking)(null)).Quote.TH_GC)));
			ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((QuotedBooking)(null)).Quote.TH_GCInfo)));

			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBooking.Business";
			this.DataSourceTypeName = "QuotedBooking";
		}
		#endregion

		#region Data Grids Setup & Binding

		protected override void SetupGrids()
		{
			base.SetupGrids();
			SetupContainersGrid();
		}

		protected void SetupContainersGrid()
		{
			ContainersDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("9213a59e-c7c7-456d-afef-bafc0ca20713", "Count"), RateOneOffContainers.Schema.TC_ContainerCount));

			ZGuidDropDownListColumn typeColumn = new ZGuidDropDownListColumn(Res.GetString("dd760647-3632-4e6c-89d1-230f5f21b478", "Type"), RateOneOffContainers.Schema.TC_RC);
			typeColumn.BindToList = "Container_List";
			typeColumn.ValueFieldName = "PK";
			typeColumn.TextFieldName = MasterFiles.Business.RefContainer.Schema.RC_Code;
			ContainersDataGrid.Columns.Add(typeColumn);
		}

		protected internal void SetupLooseCargoDataGrid()
		{
			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("9213a59e-c7c7-456d-afef-bafc0ca20713", "Count"), RateOneOffPackLine.Schema.TPL_PackLineCount) { Decimals = 0 });
			LooseCargoDataGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("52ed2dbe-020a-4847-9c63-f6a524e4ac95", "Pack Type"), RateOneOffPackLine.Schema.TPL_F3_NKPackType) { ValueFieldName = RefPackTypeSchema.F3_Code.Name, TextFieldName = RefPackTypeSchema.F3_Description.Name + (NoResString)"Multilingual" });// property name constant

			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("c9ef5e9f-f6ce-438e-ac1c-6883afaf108c", "Length"), RateOneOffPackLine.Schema.TPL_Length));
			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("aee4deca-eb47-4337-a941-8d3531cdeea3", "Width"), RateOneOffPackLine.Schema.TPL_Width));
			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("fc548d13-fc31-488f-ae19-f3fe9280e65f", "Height"), RateOneOffPackLine.Schema.TPL_Height));
			LooseCargoDataGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("cba1f425-8a2a-4f6f-9ecf-e05b8dcfa529", "UD"), RateOneOffPackLine.Schema.TPL_DimensionUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("b5100612-09d2-449b-99b3-51d08a4c213a", "Weight"), RateOneOffPackLine.Schema.TPL_Weight));
			LooseCargoDataGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("b519e0e3-af56-42c0-a033-0b8f9531258a", "UW"), RateOneOffPackLine.Schema.TPL_WeightUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

			LooseCargoDataGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("611d2f33-9722-453e-801a-acbf719c1bf2", "Volume"), RateOneOffPackLine.Schema.TPL_Volume));
			LooseCargoDataGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("fbc0fca8-b03d-451a-9e0c-6701f4f3e9cf", "UV"), RateOneOffPackLine.Schema.TPL_VolumeUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });
			VolumeCalculatorGridAddOn.Grid = LooseCargoDataGrid;
		}

		protected override VolumeCalculatorDataGridAddOn GetNewVolumeCalculatorGirdAddOn()
		{
			return new VolumeCalculatorDataGridAddOn
				(RateOneOffPackLine.Schema.TPL_PackLineCount,
				RateOneOffPackLine.Schema.TPL_Length,
				RateOneOffPackLine.Schema.TPL_Width,
				RateOneOffPackLine.Schema.TPL_Height,
				RateOneOffPackLine.Schema.TPL_DimensionUQ,
				RateOneOffPackLine.Schema.TPL_Volume,
				RateOneOffPackLine.Schema.TPL_VolumeUQ);
		}

		protected virtual internal bool ShowImperialUnits
		{
			get
			{
				bool showImperialUnits = false;

				IWebControlWithPostbackNewValue volumeControl = VolumeDropDown as IWebControlWithPostbackNewValue;

				if (volumeControl != null)
				{
					showImperialUnits = Core.Constants.Volume.IsImperial(volumeControl.NewValue);
				}

				return showImperialUnits;
			}
		}

		void SetupGridsVisibility()
		{
			ZString containerMode = RatingConstants.GetContainerModeFromMode(SpotQuote.Mode);
			bool showLooseCargoGrid = Core.Constants.ContainerModes.IsLCLType(containerMode);
			bool showContainersGrid = Core.Constants.ContainerModes.IsContainerised(containerMode);

			if (SpotQuote.IsCompareMode)
			{
				showLooseCargoGrid = true;
				showContainersGrid = true;
			}

			SetGridVisibility(LooseCargoDataGrid, showLooseCargoGrid);
			SetGridVisibility(ContainersDataGrid, showContainersGrid);
		}

		void SetGridVisibility(ZDataGrid grid, bool visible)
		{
			if (grid.Visible != visible && grid.EditItemIndex != -1)
			{
				grid.EditItemIndex = -1;
				grid.RowAdded = false;
			}
			grid.Parent.Visible = visible;
		}

		#endregion Data Grid Setup

		#region Event Handlers

		protected void TransportModeCompare_Click(object sender, EventArgs e)
		{
			if (!SpotQuote.IsCompareMode)
			{
				SaveQuote.Enabled = true;
			}

			TransportMode.RaisePostDataChangedEventInternal();
		}

		protected void ServiceLevelCompare_Click(object sender, EventArgs e)
		{
			if (!SpotQuote.IsCompareServiceLevel)
			{
				SaveQuote.Enabled = true;
			}

			ServiceLevel.RaisePostDataChangedEventInternal();
		}

		protected internal void PreviewQuote_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayErrors = true;

			bool result = false;
			if (SpotQuote.Quote != null && SpotQuote.Quote.TH_IsLocked)
			{
				result = true;
			}
			else
			{
				try
				{
					result = Helper.Preview();
				}
				catch (ZSaveConcurrencyException ex)
				{
					HandleZSaveConcurrencyException(ex);

					return;
				}
				catch (AutoRaterException ex)
				{
					DisplayNoRatingDataMessage(ex.Message);

					return;
				}

				ComparisonQuotes.Bind(SpotQuote);
				SetupQuoteEstimatesControl();
				SetupClearEstimatesButton();
			}
			if (result && SpotQuote.Quote.IsInDatabase)
			{
				DisplayQuote();
			}
			else
			{
				if (SpotQuote.ComparisonQuoteResults.Count == 0 && !result)
				{
					if (SpotQuote.Quote.TH_IsLocked)
					{
						DisplayNoRatingDataMessage(Res.GetString("EBDC49E7-D1E0-4101-B9B3-C4C624E02DD1", "The quote has been accepted already so cannot be edited."));
					}
					else if (!Helper.HasFreightCharges || Helper.TotalSellAmount <= 0)
					{
						DisplayNoRatingDataMessage(Res.GetString("5d41a04d-9c0e-f8bc-46f3-24cfd918f61d", "Unable to calculate a Freight charge for the selected Incoterm."));
					}
					else
					{
						DisplayNoRatingDataMessage();
					}
				}
			}
		}
		protected void ClearEstimates_Click(object sender, EventArgs e)
		{
			if (SpotQuote != null)
			{
				SpotQuote.ComparisonQuoteResults.RemoveAndDeleteAll();
			}
			Bind();
			OnLoad(new EventArgs());
		}

		protected internal void SaveQuote_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayErrors = true;

			if (SaveQuote.Text == Res.GetString("6bfcf65a-5b34-4336-bcea-dab41201a18d", "New Quote"))
			{
				ClearCurrentDataSource();
				Response.Redirect(AppInstance.QuotationPage);
			}
			else
			{
				if (SiteUser == null)
				{
					ErrorReporter.ReportOnce("[WebTracker SaveQuoteClick] SiteUser is null", "At URL Quotation.aspx , the SiteUser is null");
				}
				else if (SiteUser.LoggedInUser == null)
				{
					ErrorReporter.ReportOnce("[WebTracker SaveQuoteClick] SiteUser.LoggedInUser is null", "At URL Quotation.aspx , SiteUser.LoggedInUser is null");
				}

				var result = false;
				try
				{
					result = Helper.Finalise();
				}
				catch (ZSaveConcurrencyException ex)
				{
					HandleZSaveConcurrencyException(ex);

					return;
				}
				catch (AutoRaterException ex)
				{
					DisplayNoRatingDataMessage(ex.Message);

					return;
				}

				if (result)
				{
					SpotQuote.ComparisonQuoteResults.RemoveAndDeleteAll();
					SetupComparisonControlsVisibility();
					SetupClearEstimatesButton();
					Bind();
					DisableControl(EditControls, true);
					DisplayEmailSentMessage();
				}
				else if (!Helper.HasFreightCharges || Helper.TotalSellAmount <= 0)
				{
					DisplayNoRatingDataMessage(Res.GetString("5d41a04d-9c0e-f8bc-46f3-24cfd918f61d", "Unable to calculate a Freight charge for the selected Incoterm."));
				}
				else
				{
					DisplayNoRatingDataMessage();
				}
			}
		}

		void HandleZSaveConcurrencyException(ZSaveConcurrencyException ex)
		{
			var notifier = new ZWebNotificationHandler(this);
			ZExceptionReporting.HandleZSaveConcurrencyException(ex, notifier);

			SpotQuote.Quote.AddRowError(notifier.Message);
		}

		void ConsignorAddress_PostalCodeChanged(object sender, EventArgs e)
		{
			SpotQuote.Origin = new PortLoader().GetClosestPortCode(SpotQuote.ConsignorDocumentaryAddress);
			OriginPort.Bind(SpotQuote);
		}

		void ConsigneeAddress_PostalCodeChanged(object sender, EventArgs e)
		{
			SpotQuote.Destination = new PortLoader().GetClosestPortCode(SpotQuote.ConsigneeDocumentaryAddress);
			DestinationPort.Bind(SpotQuote);
		}

		#endregion

		#region DisplayQuote

		protected void DisplayQuote()
		{
			UpdatePanelRedirect("DisplayQuote", QuoteRequestHandler.RequestHelper.GetHandlerUrl(SpotQuote.Quote.PK));
			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "DisplayQuotePendingDownload", displayQuotePendingMessageScript, true);
		}

		readonly string displayQuotePendingMessageScript = @"
			function DisplayQuotePendingMessage()
			{
				try
				{
					var pendingDownload = $('PendingDownload');
					if (pendingDownload)
					{
						pendingDownload.style.visibility = 'visible';
					}
				} catch(err) {}
			};

			DisplayQuotePendingMessage();";  // Javascript segment

		#endregion DisplayQuote

		#region DisplayNoRatingDataMessage

		protected void DisplayNoRatingDataMessage()
		{
			DisplayNoRatingDataMessage(ZString.Empty);
		}

		protected void DisplayNoRatingDataMessage(ZString additionalInfo)
		{
			DisplayNoRatingDataMessage(additionalInfo, true);
		}

		protected void DisplayNoRatingDataMessage(ZString additionalInfo, bool showContactSalesMessage)
		{
			ZTextLabel quotationFailure = new ZTextLabel();
			quotationFailure.CssClass = "SectionTitle";
			quotationFailure.Text = Res.GetString("4b483614-e16a-48c3-9695-911706ab8524", "Unable to generate quotation");
			EmailNotice.Controls.Add(quotationFailure);
			HtmlGenericControl messageText = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			messageText.InnerHtml = additionalInfo.IsEmpty ? Res.GetString("3439b3ca-c4e7-44db-9e23-1982f76c0f6c", "The system was unable to generate a quotation for the provided details.") : additionalInfo.ToString();
			messageText.InnerHtml += "<br>";

			if (showContactSalesMessage)
			{
				string salesRepLinkText = Res.GetString("5e8742b2-839d-4d13-a5e7-fb3b6fb85c23", "a sales representative");
				ZString emailAddresses = "";
				foreach (ZString address in Helper.SalesRepEmailAddresses)
				{
					emailAddresses += ((emailAddresses.IsEmpty) ? "" : ";");
					emailAddresses += address;
				}

				if (!emailAddresses.IsEmpty)
				{
					salesRepLinkText = string.Format("<a href=\"mailto:{0}\">{1}</a>", emailAddresses, salesRepLinkText);
				}

				messageText.InnerHtml += Res.GetString("49ba1aae-6ae7-4618-9c04-35992e06444a", "Please contact {0} directly to request a quotation.", salesRepLinkText);
			}

			EmailNotice.Controls.Add(messageText);
			EmailNotice.Visible = true;
			EmailNotice.Style.Add("background-color", (NoResString)"lightyellow");
		}

		#endregion DisplayNoRatingDataMessage

		#region DisplayEmailSentMessage

		protected void DisplayEmailSentMessage()
		{
			string contactEmail = SiteUser.LoggedInUser.OC_Email;
			ZTextLabel emailSentMessage = new ZTextLabel();
			emailSentMessage.CssClass = "SectionTitle";
			emailSentMessage.Text = Res.GetString("42846307-be31-4578-9890-097345138091", "Quotation {0} has been processed", SpotQuote.Quote.QuoteNumberWithoutAmendmentSuffix);
			EmailNotice.Controls.Add(emailSentMessage);
			HtmlGenericControl messageText = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			messageText.InnerHtml = Res.GetString("8d3c3a46-cad9-4f10-b40d-1bf873efbfc6", "Your quotation request has been processed. A copy of the quotation has been emailed your email address <b>{0}</b>.<br>", contactEmail);
			messageText.InnerHtml += Res.GetString("4e2a3e85-1eb8-4d65-be82-63d011773095", "Press \"View Quote\" to view or save the quotation document or \"New Quote\" to request a new quotation.");
			EmailNotice.Controls.Add(messageText);
			SaveQuote.Text = Res.GetString("6bfcf65a-5b34-4336-bcea-dab41201a18d", "New Quote");
			PreviewQuote.Text = Res.GetString("b8536275-9bee-44fb-8f05-d573de7d7dc7", "View Quote");
			EmailNotice.Visible = true;
			EmailNotice.Style.Add("background-color", (NoResString)"lightyellow");
		}

		#endregion DisplayEmailSentMessage

		#region DisableControls

		protected void DisableControl(Control parent, bool disable)
		{
			foreach (Control control in parent.Controls)
			{
				DisableControl(control, disable);
			}
			if (parent is HtmlControl)
			{
				((HtmlControl)parent).Disabled = disable;
			}
			else if (parent is WebControl)
			{
				((WebControl)parent).Enabled = !disable;
			}
		}

		#endregion DisableControls

		#region Label Refresh Helpers

		protected void RefreshChargeable()
		{
			VolumeAmount.Bind(DataSource);
			Chargeable.Bind(DataSource);
		}

		#endregion Label Refresh Helpers

		#region QuoteUpdated EventHandler

		void OneOffQuote_QuoteUpdated(object sender, EventArgs e)
		{
			QuoteRequestHandler.Instance.RemoveFromCache(SpotQuote.Quote.PK);
		}

		#endregion QuoteUpdated EventHandler

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.QuotationPage;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.Quotation;
		}
	}
}
