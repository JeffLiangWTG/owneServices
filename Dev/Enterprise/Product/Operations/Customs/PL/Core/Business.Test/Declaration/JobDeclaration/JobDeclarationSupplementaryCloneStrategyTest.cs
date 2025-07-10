using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

internal class JobDeclarationSupplementaryCloneStrategyTest : TestCaseWithFactory
{
	public void TestCopyItineraryCountries()
	{
		var declaration = Factory.New<JobDeclaration>();
		var transport1 = declaration.ItineraryCountries.AddNew();
		transport1.CY_Code = CountryCodes.Poland;

		var transport2 = declaration.ItineraryCountries.AddNew();
		transport2.CY_Code = CountryCodes.Germany;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var supplementaryCloneStrategy = new JobDeclarationSupplementaryCloneStrategy(declaration, CloneType.DeepTemplateCopy, Factory);
		var supplementaryDeclaration = (JobDeclaration)supplementaryCloneStrategy.Clone();

		AssertContainsExactElementsInAnyOrder(
		[CountryCodes.Poland, CountryCodes.Germany],
		supplementaryDeclaration.ItineraryCountries.Select<string>(x => x.CY_Code));
	}
}
