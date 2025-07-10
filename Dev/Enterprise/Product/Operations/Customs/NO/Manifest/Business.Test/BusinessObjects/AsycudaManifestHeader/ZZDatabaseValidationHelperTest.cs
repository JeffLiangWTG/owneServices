using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(ZZDatabaseValidationHelper))]
sealed class ZZDatabaseValidationHelperTest : TestCaseWithFactory
{
	public void TestMandatoryFieldsValues()
	{
		var validationHelper = new ZZDatabaseValidationHelperForTest(Factory.New<AsycudaManifestHeader>());
		var mandatoryFields = validationHelper.GetMandatoryFields();

		AssertEquals("[Pre-Condition] Validation rule for CONSIGNOR", true, mandatoryFields.ContainsKey(ManifestValidationRuleCodes.Consignor));

		CombineAssertions(() =>
		{
			var validationRule = mandatoryFields[ManifestValidationRuleCodes.Consignor];
			AssertEquals("NeedToCheck", true, validationRule.NeedsToCheck?.Invoke());
			AssertEquals("Error Message", "A Shipper is required", validationRule.ErrorMessage);
		});
	}

	class ZZDatabaseValidationHelperForTest : ZZDatabaseValidationHelper
	{
		public ZZDatabaseValidationHelperForTest(AsycudaManifestHeader header) : base(header)
		{
		}

		public Dictionary<string, MandatoryValidationRule> GetMandatoryFields() => base.GetMandatoryFieldsCore();
	}
}
