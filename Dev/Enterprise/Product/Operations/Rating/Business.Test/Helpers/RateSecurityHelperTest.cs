using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class RateSecurityHelperTest : RatingTestCase
	{
		public void TestGetFirstDeniedSecurityCheckPoint()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				// Test With No Headers and Entries
				AssertNull(RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(System.Array.Empty<ZGuid>(), System.Array.Empty<ZGuid>(), Factory));

				// Test Headers
				var quote = Helper.NewQuote(setupResult.AllowedOrg);
				var clientRate = Helper.NewClientRate(setupResult.AllowedOrg);

				Factory.Save();
				AssertNull(RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(System.Array.Empty<ZGuid>(), new ZGuid[] { quote.PK, clientRate.PK }, Factory));

				quote.TH_OH = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(System.Array.Empty<ZGuid>(), new ZGuid[] { quote.PK, clientRate.PK }, new BusinessObjectFactory()).SecurityCheckPoint);

				quote.TH_OH = setupResult.AllowedOrg.PK;
				clientRate.TH_OH = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(System.Array.Empty<ZGuid>(), new ZGuid[] { quote.PK, clientRate.PK }, new BusinessObjectFactory()).SecurityCheckPoint);

				// Test Entries
				var entry1 = quote.AddRateEntry("AIR", "LSE", "AU", "ZA");
				entry1.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				entry1.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				entry1.TI_OH_Supplier = setupResult.AllowedOrg.PK;

				clientRate.TH_OH = setupResult.AllowedOrg.PK;
				var entry2 = clientRate.AddRateEntry("AIR", "LSE", "AU", "ZA");
				entry2.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				entry2.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				entry2.TI_OH_Supplier = setupResult.AllowedOrg.PK;

				Factory.Save();
				AssertNull(RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new ZGuid[] { entry1.PK, entry2.PK }, System.Array.Empty<ZGuid>(), new BusinessObjectFactory()));

				entry2.TI_OH_Consignee = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new ZGuid[] { entry1.PK, entry2.PK }, System.Array.Empty<ZGuid>(), new BusinessObjectFactory()).SecurityCheckPoint);

				entry2.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				entry2.TI_OH_Consignor = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new ZGuid[] { entry1.PK, entry2.PK }, System.Array.Empty<ZGuid>(), new BusinessObjectFactory()).SecurityCheckPoint);

				entry2.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				entry2.TI_OH_Supplier = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new ZGuid[] { entry1.PK, entry2.PK }, System.Array.Empty<ZGuid>(), new BusinessObjectFactory()).SecurityCheckPoint);
			}
		}
	}
}
