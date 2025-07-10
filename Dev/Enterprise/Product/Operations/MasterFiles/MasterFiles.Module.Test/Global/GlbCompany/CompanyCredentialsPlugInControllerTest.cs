using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CompanyCredentialsPlugInController))]
	sealed class CompanyCredentialsPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CompanyCredentialsPlugIn;
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals("View", Env.Security.Companies, Controller.CheckPointForViewExposedForTest);
			AssertEquals("New", Env.Security.Companies, Controller.CheckPointForNewExposedForTest);
			AssertEquals("Edit", Env.Security.Companies, Controller.CheckPointForEditExposedForTest);
			AssertEquals("Delete", Env.Security.Companies, Controller.CheckPointForDeleteExposedForTest);
		}

		[RequiresSTA]
		public void TestGetPlugIn()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var form = new ZForm(GlbCompany.CurrentCompany))
			{
				form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
				form.Show();

				var plugins = form.PlugIns.Instances;
				Assert(plugins.Any(p => p is Enterprise.Integration.Customs.AU.IAUCompanyCredentialsPlugIn));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var form = new ZForm(GlbCompany.CurrentCompany))
			{
				form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
				form.Show();

				var plugins = form.PlugIns.Instances;
				Assert(plugins.Any(p => p is Enterprise.Integration.Customs.GB.IGBCompanyCredentialsPlugIn));
			}
		}
	}
}
