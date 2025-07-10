using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ISFAddressColumnProvider : GridColumnProvider
	{
		public ISFAddressColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("79322537-e636-4e73-9a9b-b4cadffc2625", "Address Type"), JobDocAddress.Schema.E2_AddressType, "Parent.SupportedDocAddressesList") { ColumnKey = WebTracker.Grids.TrackingAddress.AddressType });
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("012c9129-0f72-447d-8214-983b88dbec52", "OVR"), JobDocAddress.Schema.E2_AddressOverride) { ColumnKey = WebTracker.Grids.TrackingAddress.AddressOverride });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b112f477-cd12-4a5b-99cf-96bf30ed7d3d", "Company Name"), JobDocAddress.Schema.E2_CompanyName) { ColumnKey = WebTracker.Grids.TrackingAddress.CompanyName });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("cc7bd0b4-ede3-4433-814b-dd6951515919", "Address Line 1"), JobDocAddress.Schema.E2_Address1) { ColumnKey = WebTracker.Grids.TrackingAddress.AddressLine1 });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("3f3e745b-e451-449a-96ba-354eab5d80c5", "Ctry/Rgn."), JobDocAddress.Schema.E2_RN_NKCountryCode) { ColumnKey = WebTracker.Grids.TrackingAddress.Country });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("04f2e186-4f92-4357-b6f0-fb348c9e7fba", "State"), JobDocAddress.Schema.E2_State) { ColumnKey = WebTracker.Grids.TrackingAddress.State });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("05f29030-c3c9-4825-8f79-130515d7a11d", "City"), JobDocAddress.Schema.E2_City) { ColumnKey = WebTracker.Grids.TrackingAddress.City });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("0468e10a-7d5e-4369-91f6-59df587ec366", "Post"), JobDocAddress.Schema.E2_Postcode) { ColumnKey = WebTracker.Grids.TrackingAddress.Post });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d179d55f-144d-4053-97ce-97082fbb8716", "Contact"), JobDocAddress.Schema.E2_Contact) { ColumnKey = WebTracker.Grids.TrackingAddress.Contact });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("4e371b91-fb94-4374-a9cb-00865e5ae0e0", "Phone"), JobDocAddress.Schema.E2_Phone) { ColumnKey = WebTracker.Grids.TrackingAddress.Phone });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a9e92633-514b-493a-a134-24730ee1f405", "Fax"), JobDocAddress.Schema.E2_Fax) { ColumnKey = WebTracker.Grids.TrackingAddress.Fax });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a2a5b000-0650-45ed-96f2-120c4504b374", "Email"), JobDocAddress.Schema.E2_Email) { ColumnKey = WebTracker.Grids.TrackingAddress.Email });
		}
	}
}
