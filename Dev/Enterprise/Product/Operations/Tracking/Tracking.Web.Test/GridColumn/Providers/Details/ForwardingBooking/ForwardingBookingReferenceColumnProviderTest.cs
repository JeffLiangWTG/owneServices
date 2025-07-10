using Enterprise.Customs.Common;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingBookingReferenceColumnProvider))]
	sealed class ForwardingBookingReferenceColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Country/Region", CusEntryNumber.Schema.CE_RN_NKCountryCode) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Country });
			AddDefaultsColumn(new ZTextEditColumn("Number Type", CusEntryNumber.Schema.CE_EntryType) { ColumnKey = WebTracker.Grids.ReferenceNumbers.NumberType });
			AddDefaultsColumn(new ZTextEditColumn("Number", CusEntryNumber.Schema.CE_EntryNum) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Number });
			AddDefaultsColumn(new ZTextEditColumn("Type Description", CusEntryNumber.Schema.AdditionalReferenceNumberTypeDescription) { ColumnKey = WebTracker.Grids.ReferenceNumbers.TypeDescription });
			AddColumn(new ZDateTimeColumn("Issue Date", CusEntryNumber.Schema.CE_IssueDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ReferenceNumbers.IssueDate });
			AddColumn(new ZTextEditColumn("Information", CusEntryNumber.Schema.CE_EntryLineReference) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Information });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingBookingReferenceColumnProvider();
		}
	}
}
