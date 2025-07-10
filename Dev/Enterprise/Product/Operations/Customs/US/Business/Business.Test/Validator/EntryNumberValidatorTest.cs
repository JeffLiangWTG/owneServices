using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryNumberValidatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var bo = Factory.New<DummyBusinessObject>();
			using (bo.SuspendValidationTesting())
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = company.Branches.AddNew();
				branch.FillWithValidTestData();
				Factory.Save();
				USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler { EntryFilerCode = "XJ5" });
				bo.Z0_Description = "XJ5";
				EntryNumberValidator.ValidateFormatAndCheckDigit(bo.Z0_DescriptionInfo, branch);
				AssertHasMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.EntryNumberFormat);
				AssertNoMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.InvalidCheckDigit + 1);
				bo.Z0_Description = "XJ560011282";
				EntryNumberValidator.ValidateFormatAndCheckDigit(bo.Z0_DescriptionInfo, branch);
				AssertNoMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.EntryNumberFormat);
				AssertHasMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.InvalidCheckDigit + 1);
				bo.Z0_Description = "XJ560011281";
				EntryNumberValidator.ValidateFormatAndCheckDigit(bo.Z0_DescriptionInfo, branch);
				AssertNoMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.EntryNumberFormat);
				AssertNoMessageError(bo.Z0_DescriptionInfo, EntryNumberValidator.InvalidCheckDigit + 1);
				USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler { EntryFilerCode = "SV9" });
				bo.Z0_Description = "XJ560011282";
				EntryNumberValidator.ValidateFormatAndCheckDigit(bo.Z0_DescriptionInfo, branch);
				AssertHasWarning(bo.Z0_DescriptionInfo, EntryNumberValidator.InvalidCheckDigitOtherFiler + 1);
				bo.Z0_Description = "XJ560011281";
				EntryNumberValidator.ValidateFormatAndCheckDigit(bo.Z0_DescriptionInfo, branch);
				AssertNoWarning(bo.Z0_DescriptionInfo, EntryNumberValidator.InvalidCheckDigitOtherFiler + 1);
			}
		}
	}
}
