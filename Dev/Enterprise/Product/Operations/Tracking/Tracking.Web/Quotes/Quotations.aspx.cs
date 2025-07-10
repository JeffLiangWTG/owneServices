using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Rating.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Quotes
{
	public partial class Quotations : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		protected void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;
			searchControl.ModuleID = WebModuleIDs.TrackingQuotations;
			searchControl.NewButtonUrl = AppInstance.QuotationPage;
			searchControl.NewButtonText = Res.GetString("0d99121b-3ec5-48fb-8c81-8b2d17ca7d88", "Get a New Quotation");
			searchControl.IsNewButtonVisible = SiteUser.CanViewQuotations;
			searchControl.ViewButtonText = Res.GetString("11ca4e2f-aadc-4280-a720-211e132d0f48", "Print Selected Quotations");
			searchControl.IsViewButtonVisible = SiteUser.CanViewQuotations;
			searchControl.CustomViewButtonClick += new EventHandler(GetSelectedQuotesHandler);
			searchControl.CustomButton1Text = Res.GetString("02be8b65-2b79-42f3-aba3-2326071c010e", "Copy Selected Quotation");
			searchControl.IsCustomButton1Visible = SiteUser.CanViewQuotations;
			searchControl.CustomButton1Click += OnCopyQuoteClickHandler;
			SearchControlHolder.Controls.AddAt(0, searchControl);

			quoteDownloadLink = new HyperLink();
			quoteDownloadLink.ID = "QuoteDownloadLink";
			quoteDownloadLink.Style.Add((NoResString)"display", (NoResString)"none"); // css
			SearchControlHolder.Controls.Add(quoteDownloadLink);
		}

		HyperLink quoteDownloadLink;

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.Quotations;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewQuotations; }
		}

		protected override ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return Res.GetString("ba7ed920-d6c0-4104-bea3-791552398932", "Forwarding Quotations"); }
		}

		#endregion

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		public ISearchControl SearchControl => searchControl;

		ZSearchControl searchControl;

		protected void DisplayNoRatingDataMessage(ZString additionalInfo)
		{
			ZTextLabel quotationFailure = new ZTextLabel();
			quotationFailure.CssClass = "SectionTitle";
			quotationFailure.Text = Res.GetString("23a46ab7-39ce-433a-811e-c2d738d76244", "Unable to generate quotation");
			EmailNotice.Controls.Add(quotationFailure);
			HtmlGenericControl messageText = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			messageText.InnerHtml = additionalInfo.IsEmpty ? Res.GetString("0665da44-58b0-42ad-b71b-5d9f915f70bf", "The system was unable to generate a quotation for the provided details.") : additionalInfo.ToString();
			messageText.InnerHtml += "<br>";

			messageText.InnerHtml += Res.GetString("0cde2199-54a7-4585-b946-f3c918a66868", "Please contact a sales representative directly to request a quotation.");
			EmailNotice.Controls.Add(messageText);
			EmailNotice.Visible = true;
			EmailNotice.Style.Add("background-color", (NoResString)"lightyellow"); // programmatic constant
		}

		#region Data Source

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return searchControl.Module.CreateNewFilterBusinessObject();
		}

		#endregion Data Source

		#region Copy Quote

		protected void OnCopyQuoteClickHandler(object sender, EventArgs e)
		{
			var selected = searchControl.SearchResultsDataGrid.GetSelectedPKs();
			if (selected.Length == 1 && selected[0].IsValid)
			{
				var oldQuote = Factory.Load<Quote>(selected[0]);
				if (oldQuote != null)
				{
					var newQuote = ((ITemplateCopyable)oldQuote).TemplateCopy() as Quote;
					if (newQuote != null)
					{
						var newQuotedBooking = TrackingQuotedBooking.GetNewQuotation(newQuote, SiteUser);
						if (newQuotedBooking != null)
						{
							UpdateAutoCreatedLogReferenceAndSaveDataSource(newQuotedBooking.PK, newQuotedBooking);
							HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.QuotationPage, newQuotedBooking.PK, DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
						}
					}
				}
			}
			else
			{
				Session[zPageCustomAlertMessageIndexer] = Res.GetString("e557fd5c-dd41-4b22-97e0-548aa16ce4b8", "Before requesting a Quotation copy please select only one Quotation from the list.");
			}
		}

		#endregion

		#region View Quotes

		void GetSelectedQuotesHandler(object sender, EventArgs e)
		{
			ZGuid[] selected = searchControl.SearchResultsDataGrid.GetSelectedPKs();
			if (selected.Length == 0 || (selected.Length == 1 && selected[0].IsEmpty))
			{
				Session[zPageCustomAlertMessageIndexer] = Res.GetString("4ff77e6a-a4a8-492f-968d-95b70f0f197a", "Before requesting print for selected Quotations please select at least one Quotation.");
			}
			else
			{
				if (selected.Length > 0)
				{
					bool selectedMultipleCompanies = false;
					if (selected.Length > 1)
					{
						ZGuid cachedPk = new ZGuid();
						foreach (var pk in selected)
						{
							var quote = Factory.Load<Quote>(pk);
							if (quote != null)
							{
								var quotePk = quote.Company.PK;
								if (!cachedPk.IsEmpty && cachedPk != quotePk)
								{
									selectedMultipleCompanies = true;
									break;
								}
								cachedPk = quotePk;
							}
						}
					}

					if (selectedMultipleCompanies)
					{
						Session[zPageCustomAlertMessageIndexer] = Res.GetString("3ed6ea94-0f72-46ce-8e08-8d49dbc28c26", "Cannot combine Quotations from multiple companies.");
					}
					else
					{
						Response.Redirect(QuoteRequestHandler.RequestHelper.GetHandlerUrl(selected));
					}
				}
			}
		}

#if DEBUG
		protected
#endif
		void OnSearchResultsDataGridItemCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.CommandName == ViewQuoteCommand && source is ZDataGrid grid)
			{
				var quotePK = grid.GetPKByRowIndex(e.Item.ItemIndex);
				var url = QuoteRequestHandler.RequestHelper.GetHandlerUrl(quotePK);

				if (!string.IsNullOrEmpty(url))
				{
					ScriptManager.RegisterStartupScript(this, typeof(Quotations), "QuoteDownload", GetQuoteDownloadScript(url), false);
				}
			}
		}

#if DEBUG
		protected
#endif
		string GetQuoteDownloadScript(string url)
		{
			return $@"
					<script type=""text/javascript"">
					var downloadControl = $('{quoteDownloadLink.ClientID}');

					if (downloadControl)
					{{
						downloadControl.href = '{url}';
						downloadControl.click();
					}}
					</script>";// Javascript segment
		}

		const string ViewQuoteCommand = "ViewQuote";

		#endregion

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.QuotationsPage;
		}

		#region Automatically generated

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
			SetupSearchControl();
			searchControl.SearchResultsDataGrid.ItemCommand += new DataGridCommandEventHandler(OnSearchResultsDataGridItemCommand);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
		}

		#endregion

		#endregion
	}
}
