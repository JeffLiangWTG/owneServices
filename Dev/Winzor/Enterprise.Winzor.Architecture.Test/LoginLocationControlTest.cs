using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Startup.Login;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class LoginLocationControlTest
{
	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task SwitchCompanyShouldChangeBranchAccordingly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var company1 = factory.New<GlbCompany>();
			company1.GC_Code = "TST";
			var company2 = factory.New<GlbCompany>();
			company2.GC_Code = "DMO";

			var branch1 = factory.New<GlbBranch>();
			branch1.GB_Code = "TBN";
			branch1.GB_GC = company1.PK;
			var branch2 = factory.New<GlbBranch>();
			branch2.GB_Code = "TDM";
			branch2.GB_GC = company2.PK;
			factory.Save();
			return new LoginLocationControl();
		});
		var company = page.GetByRole(AriaRole.Textbox).Nth(0);
		var branch = page.GetByRole(AriaRole.Textbox).Nth(2);
		//Selecting the complete text before filling new text since the textbox has limit of 3 characters
		await company.DblClickAsync();
		await company.FillAsync("TST");
		await Assertions.Expect(company).ToHaveValueAsync("TST");
		await company.PressAsync("Tab");
		await Assertions.Expect(branch).ToHaveValueAsync("TBN");

		await company.DblClickAsync();
		await company.FillAsync("DMO");
		await Assertions.Expect(company).ToHaveValueAsync("DMO");
		await company.PressAsync("Tab");
		await Assertions.Expect(branch).ToHaveValueAsync("TDM");
	}
}
