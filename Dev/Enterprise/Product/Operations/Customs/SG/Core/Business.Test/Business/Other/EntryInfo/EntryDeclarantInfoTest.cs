using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class EntryDeclarantInfoTest : TestCaseWithFactory
	{
		public void TestDeclarant_SG4()
		{
			var testDeclarant = Factory.NewWithValidTestData<GlbStaff>();
			testDeclarant.GS_FullName = "Gwok Hue Wee";
			testDeclarant.GS_WorkPhone = "88432984";
			var wrapper = SGGlbStaffWrapper.Get(testDeclarant);
			wrapper.Tradenetv4Password.GP_UserID = "v13t001";
			wrapper.Tradenetv4Password.GP_MailBoxID = "89135675C";
			testDeclarant.GS_Passport = "10008437593M";
			Factory.Save();
			var validDeclarant = new EntryDeclarantInfo(testDeclarant);
			AssertEquals("Declarant Name", "Gwok Hue Wee", validDeclarant.Name);
			AssertEquals("Declarant Phone No", "88432984", validDeclarant.Phone);
			AssertEquals("Declarant ID", "v13t001", validDeclarant.EntityIdentifier);
			AssertEquals("Declarant Code", "89135675C", validDeclarant.Code);
			AssertEquals("Declarant Passport", "10008437593M", validDeclarant.Passport);
		}
	}
}
