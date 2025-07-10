using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class FilterIsInEuropeanCustomsUnionOrInheritsFromEUConstraintTest : ConstraintTest<FilterIsInEuropeanCustomsUnionOrInheritsFromEUConstraint>
	{
		public override void TestGetValue()
		{
			foreach (var countryCode in new EuropeanUnionCustomsMembersProvider().GetCountriesInEuropeanCustomsUnionOrInheritsFromEU())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals("Y", Constraint.GetValue());
				}
			}

			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals("Y", Constraint.GetValue());
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("N", Constraint.GetValue());
			}
		}

		#region Implementation

		protected override string ExpectedName => "IsInEuropeanCustomsUnionOrInheritsFromEU";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
