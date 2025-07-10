using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingQuotationsColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingQuotationsColumnProviderTest : GridColumnProviderTest
	{
		public override void TestFixOldLayout()
		{
			string cachedRegistryValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			base.TestFixOldLayout();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingQuotations.QuoteNumber],
				TestProvider[WebTracker.Grids.TrackingQuotations.WeightUnit],
				TestProvider[WebTracker.Grids.TrackingQuotations.Volume],
				TestProvider[WebTracker.Grids.TrackingQuotations.TransportMode]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZLinkButtonColumn("Quote #", RatingHeaderSchema.Constants.TH_QuoteNumber)
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteNumber,
				Command = "ViewQuote"
			});

			AddDefaultsColumn(new ZTextEditColumn("Company", TrackingQuote.Constants.CompanyName) { ColumnKey = WebTracker.Grids.TrackingQuotations.Company });
			AddDefaultsColumn(new ZTextEditColumn("Status", "QuoteStatus") { ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteStatus });
			AddDefaultsColumn(new ZDateTimeColumn("Quote Date", RatingHeaderSchema.Constants.TH_QuoteDate)
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.QuoteDate,
				DateTimeFormat = ZDateTimePickerFormat.Short
			});

			AddDefaultsColumn(new ZDateTimeColumn("Expiry Date", RatingHeaderSchema.Constants.TH_QuoteEndDate)
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.ExpiryDate,
				DateTimeFormat = ZDateTimePickerFormat.Short
			});

			AddColumn(new ZDropDownListColumn("Transport Mode", TrackingQuote.Constants.TransportMode, TrackingQuote.Constants.TransportModes)
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.TransportMode,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Origin", TrackingQuote.Constants.Origin, TrackingQuote.Constants.Origins, typeof(TrackingQuote))
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.Origin,
				SortExpression = TrackingQuote.Constants.Origin,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Destination", TrackingQuote.Constants.Destination, TrackingQuote.Constants.Destinations, typeof(TrackingQuote))
			{
				ColumnKey = WebTracker.Grids.TrackingQuotations.Destination,
				SortExpression = TrackingQuote.Constants.Destination,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZCalcEditColumn("Volume", TrackingQuote.Constants.Volume) { ColumnKey = WebTracker.Grids.TrackingQuotations.Volume });
			AddColumn(new ZTextEditColumn("Vol. Units", TrackingQuote.Constants.VolumeUnits) { ColumnKey = WebTracker.Grids.TrackingQuotations.VolumeUnit });
			AddColumn(new ZCalcEditColumn("Weight", TrackingQuote.Constants.Weight) { ColumnKey = WebTracker.Grids.TrackingQuotations.Weight });
			AddColumn(new ZTextEditColumn("Weight Units", TrackingQuote.Constants.WeightUnits) { ColumnKey = WebTracker.Grids.TrackingQuotations.WeightUnit });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingQuotations.Destination
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingQuotationsColumnProvider();
		}
	}
}
