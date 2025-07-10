using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class FilterIsUnderFrenchCustomsJurisdictionConstraintTest : ConstraintTest<FilterIsUnderFrenchCustomsJurisdictionConstraint>
	{
		public override void TestGetValue()
		{
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals("Y", Constraint.GetValue());
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				AssertEquals("N", Constraint.GetValue());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("N", Constraint.GetValue());
			}
		}

		protected override string ExpectedName => "IsUnderFrenchCustomsJurisdiction";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;
	}
}
