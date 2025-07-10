using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationLockConfigHelper))]
	public class DeclarationLockConfigHelperTest : TestCaseWithFactory
	{
		public void TestGetCompanyCountry() => CombineAssertions(() =>
		{
			var company = (BusinessObject)Factory.New<IGlbCompany>();
			company[GlbCompanySchema.GC_Name] = "AT";
			company[GlbCompanySchema.GC_Code] = "AT";
			company[GlbCompanySchema.GC_RN_NKCountryCode] = "AT";
			Factory.Save();

			var template = new DeclarationLockConfig();

			template.CurrentFallbackLevel = null;
			AssertEquals("No FallbackLevel", string.Empty, template.GetCompanyCountry());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				template.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				AssertEquals("Selected CurrentCompany", "FR", template.GetCompanyCountry());

				template.CurrentFallbackLevel = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("Selected FallbackLevel", "AT", template.GetCompanyCountry());
			}
		});
	}
}
