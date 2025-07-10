using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Quotes;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class QuotationTestClass : Quotation
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		public QuotationTestClass()
		{
			EmailNotice = new HtmlGenericControl();
			ComparisonQuotes = new ComparisonQuoteWebControl();
			ComparisonQuotes.BindTo = "ComparisonQuoteResults";

			ClearEstimates = new Button();
			EditControls = new HtmlGenericControl();

			PreviewQuote = new Button();
			SaveQuote = new Button();

			ServiceLevelComparison = new ZMultiColumnCheckBoxSelection();
			TansportModeComparison = new ZMultiColumnCheckBoxSelection();
			VolumeCalculatorGridAddOn = new VolumeCalculatorDataGridAddOn
				(RateOneOffPackLine.Schema.TPL_PackLineCount,
				RateOneOffPackLine.Schema.TPL_Length,
				RateOneOffPackLine.Schema.TPL_Width,
				RateOneOffPackLine.Schema.TPL_Height,
				RateOneOffPackLine.Schema.TPL_DimensionUQ,
				RateOneOffPackLine.Schema.TPL_Volume,
				RateOneOffPackLine.Schema.TPL_VolumeUQ);
		}

		protected internal override bool ShowImperialUnits
		{
			get { return fShowImperial; }
		}

		internal ZDataGrid LooseCargoDataGridInternal
		{
			get { return LooseCargoDataGrid; }

			set { LooseCargoDataGrid = value; }
		}

		internal ZDropDownList VolumeDropDownInternal => VolumeDropDown;

		internal Button PreviewQuoteInternal => PreviewQuote;

		internal HtmlGenericControl EmailNoticeInternal => EmailNotice;

		internal Button SaveQuoteInternal => SaveQuote;

		internal void LoadOrCreateDataSourceInternal()
		{
			LoadOrCreateDataSource();
		}

		public void SetTestValueForShowImperialUnits(bool value)
		{
			fShowImperial = value;
		}

		bool fShowImperial;

		protected override BusinessObject GetNewDataSource()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;

			var spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, SiteUser);
			spotQuote.Mode = "LSE";
			spotQuote.Origin = "HKHKG";
			spotQuote.Destination = "AUSYD";
			spotQuote.VolumeUnit = Core.Constants.Volume.Litre;
			spotQuote.Volume = 20m;
			spotQuote.ClientPK = consignor.PK;
			spotQuote.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			Factory.Save();

			return spotQuote;
		}
	}
}
