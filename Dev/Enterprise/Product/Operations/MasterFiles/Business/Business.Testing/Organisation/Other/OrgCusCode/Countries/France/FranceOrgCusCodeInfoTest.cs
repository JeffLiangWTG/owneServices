using System.Linq;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class FranceOrgCusCodeInfoTest : BusinessObjectValidationTestCase
	{
		public void TestOrgCusCodeDependenciesROU()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			var dependencyProvider = OrgCusCodeCountryFactory.GetIOrgCusCodeDependencyProvider(CountryCodes.France);
			AssertNotNull(dependencyProvider);

			var orgCusCode = newOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.ROU, "12345", CountryCodes.France);
			var dependencies = dependencyProvider.GetOrgCusCodeDependencies(orgCusCode);
			AssertNotNull(dependencies);
			AssertEquals(1, dependencies.Count());

			var expectedDependencies = new OrgCusCodeDependency[]
			{
				new OrgCusCodeDependency(OrgCusCode.FranceCodeTypes.Siret, "It is only possible to record a Routage ID (ROU) in combination with a SIRET ID (SRT)")
			};
			AssertContainsExactElementsInAnyOrder(expectedDependencies, dependencies);
		}

		public void TestValidationROU()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var rouCode = newOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.ROU, "12345", CountryCodes.France);
			var expectedMessage = "It is only possible to record a Routage ID (ROU) in combination with a SIRET ID (SRT)";
			AssertHasError(rouCode.OK_CodeTypeInfo, expectedMessage);
			AssertNoWarnings(rouCode.OK_CodeTypeInfo);
			AssertNoErrors(rouCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(rouCode.OK_CustomsRegNoInfo);

			var siretCode = newOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET-ID-12345", CountryCodes.France);
			rouCode.Validation.ValidateAll();
			AssertNoErrors(rouCode.OK_CodeTypeInfo);
			AssertNoWarnings(rouCode.OK_CodeTypeInfo);
			AssertNoErrors(rouCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(rouCode.OK_CustomsRegNoInfo);

			rouCode.OK_CustomsRegNo = "";
			AssertNoErrors(rouCode.OK_CodeTypeInfo);
			AssertNoWarnings(rouCode.OK_CodeTypeInfo);
			AssertHasError(rouCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			AssertNoWarnings(rouCode.OK_CustomsRegNoInfo);

			rouCode.OK_CustomsRegNo = new string('1', 254);
			AssertNoErrors(rouCode.OK_CodeTypeInfo);
			AssertNoWarnings(rouCode.OK_CodeTypeInfo);
			AssertNoErrors(rouCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(rouCode.OK_CustomsRegNoInfo);

			newOrg.CustomsCodes.RemoveAndDelete(siretCode);
			Assert("ROU code should be marked for validation", rouCode.ShouldValidateOnSave);
			rouCode.Validation.ValidateAll();
			AssertHasError(rouCode.OK_CodeTypeInfo, expectedMessage);
			AssertNoWarnings(rouCode.OK_CodeTypeInfo);
			AssertNoErrors(rouCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(rouCode.OK_CustomsRegNoInfo);
		}

		public void TestValidationSUF()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var sufCode = newOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SUF, "12345", CountryCodes.France);
			AssertNoErrors(sufCode.OK_CodeTypeInfo);
			AssertNoWarnings(sufCode.OK_CodeTypeInfo);
			AssertNoErrors(sufCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(sufCode.OK_CustomsRegNoInfo);

			sufCode.OK_CustomsRegNo = "";
			AssertNoErrors(sufCode.OK_CodeTypeInfo);
			AssertNoWarnings(sufCode.OK_CodeTypeInfo);
			AssertHasError(sufCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			AssertNoWarnings(sufCode.OK_CustomsRegNoInfo);

			sufCode.OK_CustomsRegNo = new string('1', 254);
			AssertNoErrors(sufCode.OK_CodeTypeInfo);
			AssertNoWarnings(sufCode.OK_CodeTypeInfo);
			AssertNoErrors(sufCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(sufCode.OK_CustomsRegNoInfo);

			sufCode.OK_CustomsRegNo = "12345";
			var siretCode = newOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "SIRET-ID-12345", CountryCodes.France);
			var expectedMessage = "The following (FR) Registration codes cannot coexist: SUF, SRT";
			sufCode.Validation.ValidateAll();
			AssertHasError(sufCode.OK_CodeTypeInfo, expectedMessage);
			AssertNoWarnings(sufCode.OK_CodeTypeInfo);
			AssertNoErrors(sufCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(sufCode.OK_CustomsRegNoInfo);

			newOrg.CustomsCodes.Remove(siretCode);
			sufCode.Validation.ValidateAll();
			AssertNoErrors(sufCode.OK_CodeTypeInfo);
			AssertNoWarnings(sufCode.OK_CodeTypeInfo);
			AssertNoErrors(sufCode.OK_CustomsRegNoInfo);
			AssertNoWarnings(sufCode.OK_CustomsRegNoInfo);
		}
	}
}
