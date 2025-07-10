using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusAuthorisationRuleRequirementTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<Exception>("Null string", () => new CusAuthorisationRuleRequirement(null, 1, 1));
			AssertExceptionThrown<Exception>("Empty string", () => new CusAuthorisationRuleRequirement(string.Empty, 1, 1));
			AssertExceptionThrown<ArgumentNullException>("Null notificationType", () => new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Location, 1, 1, null, v => "test", notificationType: null));
		}

		public void TestAdditionalValidatorOnValueCollection()
		{
			var authorisationRuleRequirement = new CusAuthorisationRuleRequirement("LOC", 1, 1);
			AssertNotNull("AdditionalValidatorOnValueCollection", authorisationRuleRequirement.AdditionalValidatorOnValueCollection);

			void AssertAdditionalValidatorOnValue(string expectedValidationMessage, INotificationType expectedNotificationType)
			{
				AssertNotNull("AdditionalValidatorOnValueCollection", authorisationRuleRequirement.AdditionalValidatorOnValueCollection);
				AssertEquals("AdditionalValidatorOnValueCollection count", 1, authorisationRuleRequirement.AdditionalValidatorOnValueCollection.Count());

				var additionalValidatorOnValue = authorisationRuleRequirement.AdditionalValidatorOnValueCollection.Single();
				CombineAssertions("Assert AdditionalValidatorOnValueCollection", () =>
				 {
					 var result = additionalValidatorOnValue("");
					 AssertNotNull("result should not be null", result);
					 AssertEquals("ValidationMessage", expectedValidationMessage, result.ValidationMessage);
					 AssertEquals("NotificationType", expectedNotificationType.EnumValueName, result.NotificationType.EnumValueName);
				 });
			}

			authorisationRuleRequirement = new CusAuthorisationRuleRequirement("LOC", 1, 1, null, (v) => "test", NotificationType.Information);
			AssertAdditionalValidatorOnValue("test", NotificationType.Information);

			authorisationRuleRequirement = new CusAuthorisationRuleRequirement("LOC", 1, 1, null, (v) => "test2", NotificationType.Warning);
			AssertAdditionalValidatorOnValue("test2", NotificationType.Warning);
		}

		public void TestAddAdditionalValidatorOnValue()
		{
			var authorisationRuleRequirement = new CusAuthorisationRuleRequirement("LOC", 1, 1);
			AssertNotNull("AdditionalValidatorOnValueCollection", authorisationRuleRequirement.AdditionalValidatorOnValueCollection);
			AssertEquals("AdditionalValidatorOnValueCollection count", 0, authorisationRuleRequirement.AdditionalValidatorOnValueCollection.Count());

			authorisationRuleRequirement.AddAdditionalValidatorOnValue(x => ("test", NotificationType.Error));
			AssertEquals("AdditionalValidatorOnValueCollection count", 1, authorisationRuleRequirement.AdditionalValidatorOnValueCollection.Count());
		}

		public void TestAttributeRepeats()
		{
			var attribute1 = new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Location, 1, 1);
			var attribute2 = new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Location, 1, 1);
			var attribute3 = new CusAuthorisationRuleRequirement("ZZZ", 1, 1);

			CombineAssertions(() =>
			{
				AssertEquals("Equals Override", true, attribute1.Equals(attribute2));
				AssertEquals("== Operator", true, attribute1 == attribute2);
				AssertEquals("!= Operator", true, attribute1 != attribute3);
				AssertEquals("GetHashCode()", attribute2.GetHashCode(), attribute1.GetHashCode());
			});
		}
	}
}
