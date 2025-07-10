using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AgencyRequirementsValidationTest : TestCaseWithDummy
	{
		public void TestValidatePGAIndicatorAndData()
		{
			var bizObj = Factory.New<DummyRequirements>();
			bizObj.TestCase = Cases.ValidatePGAIndicatorAndData;
			bizObj.AgencyCode = GovernmentAgencyProgramCodeList.Codes.Lacey;
			bizObj.Z0_Description = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(bizObj.Z0_DescriptionInfo, "At least one Lacey Line is required when Lacey Indicator is 'Declared'.");
			bizObj.Z0_Description = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(bizObj.Z0_DescriptionInfo, "Lacey Lines may not be entered if Lacey Indicator is blank or 'Disclaimed'.");
		}

		public void TestValidatePGADisclaimedIndicator()
		{
			var bizObj = Factory.New<DummyRequirements>();
			bizObj.TestCase = Cases.ValidatePGADisclaimedIndicator;
			bizObj.AgencyCode = GovernmentAgencyProgramCodeList.Codes.Lacey;
			bizObj.IsRequired = true;
			bizObj.Z0_Description = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(bizObj.Z0_DescriptionInfo, "Disclaim is generally NOT allowed if the HTS tariff is flagged as 'Must Be provided'.");
			bizObj.Z0_Description = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(bizObj.Z0_DescriptionInfo, "Disclaim is generally NOT allowed if the HTS tariff is flagged as 'Must Be provided'.");
		}

		public void TestValidatePGADisclaimedIndicator_LVS()
		{
			var bizObj = Factory.New<DummyRequirements>();
			bizObj.TestCase = Cases.ValidatePGADisclaimedIndicatorForLVS;
			bizObj.AgencyCode = GovernmentAgencyProgramCodeList.Codes.Lacey;
			bizObj.IsRequired = true;
			bizObj.Z0_Description = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(bizObj.Z0_DescriptionInfo, "Disclaim is generally NOT allowed if the HTS tariff is flagged as 'Must Be provided'.");
		}

		sealed class DummyRequirements : DummyBusinessObject
		{
			public DummyRequirements(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string AgencyCode { get; set; }

			public string TestCase { get; set; }

			public bool IsRequired { get; set; }

			public OGAAgencyRequirementCollection AgencyRequirements { get; set; }

			protected override DummyBizoValidation GetNewValidation() => new DummyRequirementsValidation(this);
		}

		sealed class DummyRequirementsValidation : DummyBizoValidation
		{
			public DummyRequirementsValidation(DummyRequirements parent) : base(parent)
			{
				this.parent = parent;
			}
			readonly DummyRequirements parent;

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();
				switch (parent.TestCase)
				{
					case Cases.ValidatePGAIndicatorAndData:
						AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.Z0_DescriptionInfo, parent.AgencyCode, System.Array.Empty<IPGADataCorrection>());
						parent.Z0_Code = ZString.Empty;
						AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.Z0_DescriptionInfo, parent.AgencyCode, new IPGADataCorrection[] { PGADataCorrection });
						break;
					case Cases.ValidatePGADisclaimedIndicator:
						AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.Z0_DescriptionInfo, true);
						break;
					case Cases.ValidatePGADisclaimedIndicatorForLVS:
						AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.Z0_DescriptionInfo, "C", true);
						break;
					case Cases.ValidateOGAAgencyRequirements:
						AgencyRequirementsValidator.ValidateOGAAgencyRequirements(parent.AgencyCode, parent.AgencyRequirements);
						break;
					case Cases.ValidateOGAAgencyDisclaimReason:
						AgencyRequirementsValidator.ValidateOGAAgencyDisclaimReason(parent.AgencyCode, parent.AgencyRequirements);
						break;
					default:
						break;
				}
			}

			IPGADataCorrection PGADataCorrection
			{
				get
				{
					if (pGADataCorrectionMock == null)
					{
						pGADataCorrectionMock = new Mock<IPGADataCorrection>();
						pGADataCorrectionMock.Setup(m => m.TrackingStatusInfo).Returns(parent.Z0_CodeInfo);
					}

					return pGADataCorrectionMock.Object;
				}
			}
			Mock<IPGADataCorrection> pGADataCorrectionMock;
		}

		static class Cases
		{
			public const string ValidatePGAIndicatorAndData = "ValidatePGAIndicatorAndData";
			public const string ValidatePGADisclaimedIndicator = "ValidatePGADisclaimedIndicator";
			public const string ValidatePGADisclaimedIndicatorForLVS = "ValidatePGADisclaimedIndicatorForLVS";
			public const string ValidateOGAAgencyRequirements = "ValidateOGAAgencyRequirements";
			public const string ValidateOGAAgencyDisclaimReason = "ValidateOGAAgencyDisclaimReason";
		}
	}
}
