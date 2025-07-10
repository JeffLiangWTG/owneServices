using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class FilterIsIntegratedCountryConstraintTest : ConstraintTest<FilterIsIntegratedCountryConstraint>
	{
		public override void TestGetValue()
		{
			foreach (var countryCode in IntegratedCountryHelper.CustomsWareCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					AssertEquals("CustomsWare - " + countryCode, "Y", Constraint.GetValue());
				}
			}

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			foreach (var countryCode in IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Inner.DeleteValue(companyPK, Guid.Empty, Guid.Empty);
					var isUsedToBeBuiltInOnlyCountry = IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(countryCode);
					if (isUsedToBeBuiltInOnlyCountry)
					{
						AssertEquals(countryCode + " - No Config", "N", Constraint.GetValue());
					}
					else
					{
						AssertEquals(countryCode + " - No Config", "Y", Constraint.GetValue());
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals(countryCode + " - Yes Config", "Y", Constraint.GetValue());
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals(countryCode + " - BLT Config", "N", Constraint.GetValue());
					}
				}
			}
		}

		protected override string ExpectedName => "IsIntegratedCountry";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;
	}
}
