using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbCompanyController))]
	sealed class GlbCompanyControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbCompany;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			RefCountry aCountry = Factory.LoadTop1<RefCountry>(new ZQuery());
			company.GC_RN_NKCountryCode = aCountry.Code;

			RefCurrency aCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			company.GC_RX_NKLocalCurrency = aCurrency.RX_Code;

			company.GC_StartDate = Env.Time.CurrentLocalDateTime;
			Factory.Save();
			return company;
		}
	}
}
