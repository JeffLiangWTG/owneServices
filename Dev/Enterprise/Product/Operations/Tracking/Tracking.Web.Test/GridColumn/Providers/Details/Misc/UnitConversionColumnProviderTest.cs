using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(UnitConversionColumnProvider))]
	sealed class UnitConversionColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZCalcEditColumn("Qty in Parent", OrgPartUnitSchema.OF_QuantityInParent.Name) { ColumnKey = WebTracker.Grids.UnitConversion.QuantityInParent });
			AddDefaultsColumn(new ZTextEditColumn("Package", OrgPartUnitSchema.OF_PackType.Name) { ColumnKey = WebTracker.Grids.UnitConversion.Package });
			AddDefaultsColumn(new ZTextEditColumn("Parent Package", OrgPartUnitSchema.OF_ParentPackType.Name) { ColumnKey = WebTracker.Grids.UnitConversion.ParentPackage });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new UnitConversionColumnProvider();
		}
	}
}
