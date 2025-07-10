using Enterprise.Customs.US.ISF.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ISFReferenceColumnProvider))]
	sealed class ISFReferenceColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Description", CusISFBill.Schema.BB_BillTypeDescription) { ColumnKey = WebTracker.Grids.ISFReference.Description });
			AddDefaultsColumn(new ZTextEditColumn("Bill Number", CusISFBill.Schema.BB_BillNum) { ColumnKey = WebTracker.Grids.ISFReference.BillNumber });
			AddDefaultsColumn(new ZTextEditColumn("Bill Status", CusISFBill.Schema.BB_CustomsStatusDescription) { ColumnKey = WebTracker.Grids.ISFReference.BillStatus });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ISFReferenceColumnProvider();
		}
	}
}
