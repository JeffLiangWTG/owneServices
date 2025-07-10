using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsEntryPGAStatusColumnProvider))]
	sealed class CustomsEntryPGAStatusColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Agency Code", CusDisposition.Schema.CDI_StatusKey) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.AgencyCode });
			AddDefaultsColumn(new ZTextEditColumn("Status Code", CusDisposition.Schema.CDI_Status) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusCode });
			AddDefaultsColumn(new ZTextEditColumn("Status Description", CusDisposition.Schema.StatusDescription) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusDescription });
			AddDefaultsColumn(new ZDateTimeColumn("Status Date", CusDisposition.Schema.CDI_StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusDate });
			AddDefaultsColumn(new ZTextEditColumn("Notes", CusDisposition.Schema.CDI_Notes) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.Notes });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsEntryPGAStatusColumnProvider();
		}
	}
}
