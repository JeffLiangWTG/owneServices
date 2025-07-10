using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CompanyCredentialsPlugInTest : TestCaseWithFactory
	{
		public void TestEnable()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				using (var plugin = new CompanyCredentialsPlugIn(company))
				{
					AssertEquals(true, plugin.Enabled);
				}

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;

				using (var plugin = new CompanyCredentialsPlugIn(company))
				{
					AssertEquals(false, plugin.Enabled);
				}
			}
		}

		public void TestMutex()
		{
			var company = Factory.New<GlbCompany>();

			using (var plugin = new CompanyCredentialsPlugIn(company))
			{
				AssertEquals(MutexIDs.CompanyCredentialsPlugInBeingCreated, plugin.Mutex.MutexID);
				AssertEquals(company.PK.ToString(), plugin.Mutex.RecordIdentifier);
			}
		}

		public void TestName()
		{
			var company = Factory.New<GlbCompany>();

			using (var plugin = new CompanyCredentialsPlugIn(company))
			{
				AssertEquals("Credentials", plugin.Name);
			}
		}
	}
}
