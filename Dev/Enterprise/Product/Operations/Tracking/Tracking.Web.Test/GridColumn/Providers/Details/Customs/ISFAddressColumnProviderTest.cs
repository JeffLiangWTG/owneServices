using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ISFAddressColumnProvider))]
	sealed class ISFAddressColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZDropDownListColumn("Address Type", JobDocAddress.Schema.E2_AddressType, "Parent.SupportedDocAddressesList") { ColumnKey = WebTracker.Grids.TrackingAddress.AddressType });
			AddDefaultsColumn(new ZCheckBoxColumn("OVR", JobDocAddress.Schema.E2_AddressOverride) { ColumnKey = WebTracker.Grids.TrackingAddress.AddressOverride });
			AddDefaultsColumn(new ZTextEditColumn("Company Name", JobDocAddress.Schema.E2_CompanyName) { ColumnKey = WebTracker.Grids.TrackingAddress.CompanyName });
			AddDefaultsColumn(new ZTextEditColumn("Address Line 1", JobDocAddress.Schema.E2_Address1) { ColumnKey = WebTracker.Grids.TrackingAddress.AddressLine1 });
			AddDefaultsColumn(new ZTextEditColumn("Ctry/Rgn.", JobDocAddress.Schema.E2_RN_NKCountryCode) { ColumnKey = WebTracker.Grids.TrackingAddress.Country });
			AddDefaultsColumn(new ZTextEditColumn("State", JobDocAddress.Schema.E2_State) { ColumnKey = WebTracker.Grids.TrackingAddress.State });
			AddDefaultsColumn(new ZTextEditColumn("City", JobDocAddress.Schema.E2_City) { ColumnKey = WebTracker.Grids.TrackingAddress.City });
			AddDefaultsColumn(new ZTextEditColumn("Post", JobDocAddress.Schema.E2_Postcode) { ColumnKey = WebTracker.Grids.TrackingAddress.Post });
			AddDefaultsColumn(new ZTextEditColumn("Contact", JobDocAddress.Schema.E2_Contact) { ColumnKey = WebTracker.Grids.TrackingAddress.Contact });
			AddDefaultsColumn(new ZTextEditColumn("Phone", JobDocAddress.Schema.E2_Phone) { ColumnKey = WebTracker.Grids.TrackingAddress.Phone });
			AddDefaultsColumn(new ZTextEditColumn("Fax", JobDocAddress.Schema.E2_Fax) { ColumnKey = WebTracker.Grids.TrackingAddress.Fax });
			AddDefaultsColumn(new ZTextEditColumn("Email", JobDocAddress.Schema.E2_Email) { ColumnKey = WebTracker.Grids.TrackingAddress.Email });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ISFAddressColumnProvider();
		}
	}
}
