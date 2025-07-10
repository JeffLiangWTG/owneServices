using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:QuoteComparisonWebControl runat=server></{0}:QuoteComparisonWebControl>")]
	public class ComparisonQuoteWebControl : CompositeControl, ISelfBindingPostbackWebControl
	{
		#region Constructors

		public ComparisonQuoteWebControl()
			: base()
		{
			fHasChanges = false;
			fBindTo = "";
		}

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue("ComparisonQuotes"), Browsable(true)]
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
		string cssClass = "ComparisonQuotes";

		[Category("Appearance"), DefaultValue("ComparisonQuoteEstimateCaption"), Browsable(true)]
		public string EstimateCaptionCssClass
		{
			get
			{
				return estimateCaptionCssClass;
			}
			set
			{
				estimateCaptionCssClass = value;
			}
		}
		string estimateCaptionCssClass = "ComparisonQuoteEstimateCaption";

		[Category("Appearance"), DefaultValue("ComparisonQuoteEstimate"), Browsable(true)]
		public string EstimateCssClass
		{
			get
			{
				return estimateCssClass;
			}
			set
			{
				estimateCssClass = value;
			}
		}
		string estimateCssClass = "ComparisonQuoteEstimate";

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string Caption
		{
			get
			{
				return caption;
			}
			set
			{
				caption = value;
			}
		}
		string caption = string.Empty;

		[Category("Appearance"), DefaultValue("SectionTitle"), Browsable(true)]
		public string CaptionCssClass
		{
			get
			{
				return captionCssClass;
			}
			set
			{
				captionCssClass = value;
			}
		}
		string captionCssClass = "SectionTitle";

		#endregion

		#region Overrides

		#region ZPage

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		#endregion

		public string AnchorName
		{
			get
			{
				return Anchor.ID;
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (DataSourceCollection.Count > 0)
			{
				Anchor.RenderControl(writer);
			}
			base.RenderControl(writer);
		}

		Panel PutInNoWrapPanel(WebControl control)
		{
			Panel result = new Panel();
			result.Style.Add(HtmlTextWriterStyle.WhiteSpace, (NoResString)"nowrap");
			result.Controls.Add(control);
			return result;
		}

		TableCell GetNewEmptyCell()
		{
			TableCell result = new TableCell();
			result.Text = "&nbsp;";
			return result;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Controls.Clear();

			if (DataSourceCollection != null)
			{
				if (DataSourceCollection.Count > 0)
				{
					DataSourceCollection.Sort<ComparisonQuoteResult>(ComparerForDisplay);
					Table controlTable = new Table();
					controlTable.CssClass = CssClass;

					#region Caption

					Label captionLabel = new Label();
					captionLabel.CssClass = CaptionCssClass;
					captionLabel.Text = Caption;

					TableRow captionRow = new TableRow();
					TableCell captionCell = new TableCell();

					captionCell.Controls.Add(PutInNoWrapPanel(captionLabel));
					captionRow.Cells.Add(captionCell);
					controlTable.Rows.Add(captionRow);

					#endregion
					foreach (ZString transportMode in DataSourceCollection.Modes)
					{
						ComparisonQuoteResultCollection transportModeResuts = DataSourceCollection.ModeSpecific(transportMode);
						if (transportModeResuts.Count > 0)
						{
							TableRow transportModeCaptionRow = new TableRow();
							TableCell transportModeCaptionCell = new TableCell();
							transportModeCaptionCell.CssClass = EstimateCaptionCssClass;
							transportModeCaptionCell.Text = ComparerForDisplay.GetTransportModeDescription(transportMode);
							transportModeCaptionRow.Cells.Add(transportModeCaptionCell);

							controlTable.Rows.Add(transportModeCaptionRow);
							foreach (ZString serviceLevel in transportModeResuts.ServiceLevels)
							{
								ComparisonQuoteResultCollection serviceLevelResults = transportModeResuts.ServiceLevelSpecific(serviceLevel);
								if (serviceLevelResults.Count > 0)
								{
									foreach (ComparisonQuoteResult comparisonQuote in serviceLevelResults)
									{
										if (InternalControls.ContainsKey(comparisonQuote))
										{
											Table estimateTable = new Table();
											estimateTable.CssClass = EstimateCssClass;

											TableRow estimateCaptionRow = new TableRow();

											TableCell selectionCell = new TableCell();
											selectionCell.Controls.Add(InternalControls[comparisonQuote]);
											estimateCaptionRow.Cells.Add(selectionCell);

											TableCell estimateCaptionCell = new TableCell();
											estimateCaptionCell.CssClass = EstimateCaptionCssClass;
											estimateCaptionCell.Text = ComparerForDisplay.GetServiceLevelDescription(serviceLevel);
											estimateCaptionCell.ColumnSpan = 2;
											estimateCaptionRow.Cells.Add(estimateCaptionCell);

											if (comparisonQuote.ShowLocalCurrency)
											{
												TableCell quoteCurrencyCaptionCell = new TableCell();
												quoteCurrencyCaptionCell.Text = Res.GetString("7bb1a4b3-dc77-4411-ae54-0eac147534a4", "Quote Currency");
												quoteCurrencyCaptionCell.HorizontalAlign = HorizontalAlign.Right;
												estimateCaptionRow.Cells.Add(quoteCurrencyCaptionCell);
											}

											estimateTable.Rows.Add(estimateCaptionRow);

											if (comparisonQuote.Carrier != null)
											{
												estimateCaptionCell.Text += (string.IsNullOrEmpty(estimateCaptionCell.Text) ? "" : " - ") + comparisonQuote.Carrier.OH_FullName;
											}

											if (string.IsNullOrEmpty(estimateCaptionCell.Text))
											{
												estimateCaptionCell.Text = ComparerForDisplay.GetTransportModeDescription(comparisonQuote.Mode);
											}

											List<ZString> currencies = comparisonQuote.Charges.Currencies;
											foreach (ZString currency in currencies)
											{
												ComparisonQuoteChargeCollection currencyCharges = comparisonQuote.Charges.GetChargesBasedOnCurrency(currency);

												foreach (ComparisonQuoteCharge charge in currencyCharges)
												{
													TableRow chargeRow = new TableRow();

													chargeRow.Cells.Add(GetNewEmptyCell());

													TableCell chargeDescriptionCell = new TableCell();
													chargeDescriptionCell.Text = charge.Description;

													TableCell chargeAmountCell = new TableCell();
													chargeAmountCell.CssClass = "ComparisonQuoteEstimateAmount";
													chargeAmountCell.Text = charge.OSSellAmount.ToString();
													chargeAmountCell.HorizontalAlign = HorizontalAlign.Right;

													chargeRow.Cells.Add(chargeDescriptionCell);
													chargeRow.Cells.Add(chargeAmountCell);

													if (comparisonQuote.ShowLocalCurrency)
													{
														TableCell quoteCurrencyAmountCell = new TableCell();
														quoteCurrencyAmountCell.CssClass = "ComparisonQuoteEstimateAmount";
														quoteCurrencyAmountCell.Text = charge.LocalSellAmount.ToString();
														quoteCurrencyAmountCell.HorizontalAlign = HorizontalAlign.Right;
														chargeRow.Cells.Add(quoteCurrencyAmountCell);
													}

													estimateTable.Rows.Add(chargeRow);
												}
												TableRow currencyChargesTotalRow = new TableRow();

												TableCell currencyChargesTotalDescriptionCell = new TableCell();
												currencyChargesTotalDescriptionCell.Text = Res.GetString("83a79a43-95a0-4e65-b61f-ca394e1ebfa9", "Sub total({0})", currency);
												currencyChargesTotalDescriptionCell.HorizontalAlign = HorizontalAlign.Right;

												TableCell currencyChargesTotalAmountCell = new TableCell();
												currencyChargesTotalAmountCell.Text = currencyCharges.TotalOSSellAmount.ToString();
												currencyChargesTotalAmountCell.HorizontalAlign = HorizontalAlign.Right;
												currencyChargesTotalAmountCell.Style.Add("border-top", (NoResString)"2px solid");
												currencyChargesTotalAmountCell.Style.Add("font-weight", (NoResString)"bold");

												currencyChargesTotalRow.Cells.Add(GetNewEmptyCell());
												currencyChargesTotalRow.Cells.Add(currencyChargesTotalDescriptionCell);
												currencyChargesTotalRow.Cells.Add(currencyChargesTotalAmountCell);
												currencyChargesTotalRow.Cells.Add(GetNewEmptyCell());
												estimateTable.Rows.Add(currencyChargesTotalRow);
											}

											if (comparisonQuote.ShowLocalCurrency)
											{
												TableRow totalChargesRow = new TableRow();
												totalChargesRow.Cells.Add(GetNewEmptyCell());

												TableCell totalChargesCaptionCell = new TableCell();
												totalChargesCaptionCell.Text = Res.GetString("093dd73e-0dc6-4358-ade8-29f1a4841c83", "TOTAL CHARGES:");
												totalChargesCaptionCell.Style.Add("font-weight", Res.GetString("5a527da5-13ca-4f84-86d3-9a34b47fe182", "bold"));
												totalChargesRow.Cells.Add(totalChargesCaptionCell);

												TableCell totalChargesCurrencyCell = new TableCell();
												totalChargesCurrencyCell.Text = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
												totalChargesCurrencyCell.HorizontalAlign = HorizontalAlign.Right;
												totalChargesCurrencyCell.Style.Add("font-weight", (NoResString)"bold");
												totalChargesRow.Cells.Add(totalChargesCurrencyCell);

												TableCell totalChargesAmountCell = new TableCell();
												totalChargesAmountCell.Text = comparisonQuote.Charges.TotalLocalSellAmount.ToString();
												totalChargesAmountCell.HorizontalAlign = HorizontalAlign.Right;
												totalChargesAmountCell.Style.Add("font-weight", (NoResString)"bold");
												totalChargesRow.Cells.Add(totalChargesAmountCell);

												estimateTable.Rows.Add(totalChargesRow);
											}

											TableRow estimateQuoteRow = new TableRow();
											TableCell estimateQuoteCell = new TableCell();
											estimateQuoteCell.Controls.Add(estimateTable);
											estimateQuoteRow.Cells.Add(estimateQuoteCell);
											controlTable.Rows.Add(estimateQuoteRow);
										}
									}
								}
							}
						}
					}

					Controls.Add(controlTable);
				}
			}
		}

		#endregion

		#region ISelfBindingPostbackWebControl Members

		public bool HasChanges
		{
			get
			{
				return fHasChanges;
			}
			set
			{
				fHasChanges = value;
			}
		}
		bool fHasChanges;

		#endregion

		#region ISelfBindingWebControl Members

		public void Bind(object dataSource)
		{
			UnBind();
			BusinessEntity = dataSource;
			if (dataSource != null)
			{
				DataSourceCollection = (ComparisonQuoteResultCollection)ZPropertyAccessor.Get(dataSource, BindTo);
				DataSourceCollection.Sort<ComparisonQuoteResult>(ComparerForDisplay);
				if (DataSourceCollection != null)
				{
					int radioButtonID = 0;
					foreach (ComparisonQuoteResult comparisonQuote in DataSourceCollection)
					{
						ZRadioButton radioButton = new ZRadioButton();
						radioButton.GroupName = "ComparisonQuoteRB";
						radioButton.BindTo = "Selected";
						radioButton.ID = (radioButtonID++).ToString();
						radioButton.Attributes.Add((NoResString)"onclick", (NoResString)"javascript:document.getElementById('SaveQuote').disabled= false;"); // it is java script!
						radioButton.Bind(comparisonQuote);
						InternalControls.Add(comparisonQuote, radioButton);
					}
				}
			}
			CreateChildControls();
		}

		public bool IsBindable(object dataSource)
		{
			return ((!string.IsNullOrEmpty(BindTo) && dataSource != null && dataSource is BusinessObject));
		}

		public void UnBind()
		{
			DataSourceCollection = null;
			BusinessEntity = null;
			foreach (ZRadioButton radioButton in InternalControls.Values)
			{
				radioButton.UnBind();
			}
			InternalControls.Clear();
		}

		protected ComparisonQuoteResultCollection DataSourceCollection
		{
			get
			{
				return fDataSourceCollection;
			}
			set
			{
				fDataSourceCollection = value;
			}
		}
		ComparisonQuoteResultCollection fDataSourceCollection;

		protected QuotedBooking SpotQuote
		{
			get
			{
				return BusinessEntity as QuotedBooking;
			}
		}

		protected object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

		protected Dictionary<ComparisonQuoteResult, ZRadioButton> InternalControls
		{
			get
			{
				if (fInternalControls == null)
				{
					fInternalControls = new Dictionary<ComparisonQuoteResult, ZRadioButton>();
				}
				return fInternalControls;
			}
		}

		Dictionary<ComparisonQuoteResult, ZRadioButton> fInternalControls;

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get
			{
				return fBindTo;
			}
			set
			{
				fBindTo = value;
			}
		}
		string fBindTo;

		#endregion

		#region IPostBackDataHandler Members

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			return false;
		}

		public void RaisePostDataChangedEvent()
		{
		}

		#endregion

		#region Implementation

		protected HyperLink Anchor
		{
			get
			{
				if (fAnchor == null)
				{
					fAnchor = new HyperLink();
					fAnchor.NavigateUrl = "";
					fAnchor.ID = "ComparisonQuoteResultsAnchor";
				}
				return fAnchor;
			}
		}

		HyperLink fAnchor;

		protected ComparisonQuoteResultComparerForDisplay ComparerForDisplay
		{
			get
			{
				if (fComparerForDisplay == null || fComparerForDisplay.SpotQuote != SpotQuote)
				{
					fComparerForDisplay = new ComparisonQuoteResultComparerForDisplay(SpotQuote);
				}
				return fComparerForDisplay;
			}
		}
		ComparisonQuoteResultComparerForDisplay fComparerForDisplay;

		#endregion

		#region Comparers

		protected class ComparisonQuoteResultComparerForDisplay : Comparer<ComparisonQuoteResult>
		{
			#region Constructors

			public ComparisonQuoteResultComparerForDisplay(QuotedBooking spotQuote)
			{
				this.fSpotQuote = spotQuote;
			}

			#endregion

			#region Overrides

			public override int Compare(ComparisonQuoteResult x, ComparisonQuoteResult y)
			{
				if (GetTransportModeDescription(x.Mode).CompareTo(GetTransportModeDescription(y.Mode)) != 0)
				{
					return GetTransportModeDescription(x.Mode).CompareTo(GetTransportModeDescription(y.Mode));
				}

				if (GetServiceLevelDescription(x.ServiceLevel).CompareTo(GetServiceLevelDescription(y.ServiceLevel)) != 0)
				{
					return GetServiceLevelDescription(x.ServiceLevel).CompareTo(GetServiceLevelDescription(y.ServiceLevel));
				}

				ZString carrierX = (x.Carrier != null ? x.Carrier.OH_FullName : ZString.Empty);
				ZString carrierY = (y.Carrier != null ? y.Carrier.OH_FullName : ZString.Empty);
				return carrierX.CompareTo(carrierY);
			}

			#endregion

			#region Properties

			public QuotedBooking SpotQuote
			{
				get
				{
					return fSpotQuote;
				}
			}

			#endregion

			#region Methods

			public ZString GetTransportModeDescription(ZString transportModeCode)
			{
				ZString result = transportModeCode;
				if (SpotQuote != null)
				{
					if (!string.IsNullOrEmpty(SpotQuote.Modes.GetDescriptionFromCode(transportModeCode)))
					{
						result = SpotQuote.Modes.GetDescriptionFromCode(transportModeCode);
					}
				}
				return result;
			}

			public ZString GetServiceLevelDescription(ZString serviceLevelCode)
			{
				ZString result = serviceLevelCode;
				if (SpotQuote != null)
				{
					foreach (RefServiceLevel serviceLevel in SpotQuote.ServiceLevels)
					{
						if (serviceLevel.RS_Code == serviceLevelCode)
						{
							result = serviceLevel.RS_DescriptionMultilingual;
						}
					}
				}
				return result;
			}

			#endregion

			#region Implementation

			readonly QuotedBooking fSpotQuote;

			#endregion
		}

		#endregion
	}
}
