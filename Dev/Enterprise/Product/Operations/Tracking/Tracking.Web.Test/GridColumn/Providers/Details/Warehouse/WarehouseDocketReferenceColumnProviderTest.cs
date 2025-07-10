using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WarehouseDocketReferenceColumnProvider))]
	sealed class WarehouseDocketReferenceColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZDropDownListColumn("Ref Type", WhsDocketReferenceSchema.WX_RefType.Name, "Lookups.ReferenceTypes") { ColumnKey = WebTracker.Grids.WarehouseDocketReference.ReferenceType });
			AddDefaultsColumn(new ZTextEditColumn("Reference", WhsDocketReferenceSchema.WX_Reference.Name) { ColumnKey = WebTracker.Grids.WarehouseDocketReference.Reference });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new WarehouseDocketReferenceColumnProvider();
		}
	}
}
