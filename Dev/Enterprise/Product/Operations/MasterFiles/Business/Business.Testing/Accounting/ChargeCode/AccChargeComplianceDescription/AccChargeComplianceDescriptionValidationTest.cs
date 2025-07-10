using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeComplianceDescriptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIsUsedBy()
		{
			AssertType<AccChargeComplianceDescriptionValidation>(Factory.New<AccChargeComplianceDescription>().Validation);
		}

		[ExpectNoExceptions]
		public void TestCheckADE_JobType()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var jobTypeHelperMock = new Mock<IJobTypeConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			jobConfigurationHelperFactoryMock.Setup(x => x.GetJobTypeHelper(complianceDescription)).Returns(jobTypeHelperMock.Object);

			complianceDescription.Validation.ValidateADE_JobType();

			jobTypeHelperMock.Verify(x => x.Validate(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestCheckADE_TransportMode()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var transportModeConfigHelperMock = new Mock<ITransportModeConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			jobConfigurationHelperFactoryMock.Setup(x => x.GetTransportModeHelper(complianceDescription)).Returns(transportModeConfigHelperMock.Object);

			complianceDescription.Validation.ValidateADE_TransportMode();

			transportModeConfigHelperMock.Verify(x => x.Validate(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestCheckADE_SupplyType()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var jobConfigHelperMock = new Mock<IJobConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			jobConfigurationHelperFactoryMock.Setup(x => x.GetSupplyTypeHelper(complianceDescription)).Returns(jobConfigHelperMock.Object);

			complianceDescription.Validation.ValidateADE_SupplyType();

			jobConfigHelperMock.Verify(x => x.Validate(), Times.Once);
		}

		public void TestADE_Description_CheckEnteredValidation()
		{
			ComplianceDescription.ADE_Description = ZString.Empty;
			ComplianceDescription.RunPreSaveValidation();
			AssertHasError(ComplianceDescription.ADE_DescriptionInfo, "Please enter a Sell Compliance Description.");

			ComplianceDescription.ADE_Description = "TEST";
			ComplianceDescription.RunPreSaveValidation();
			AssertNoErrors(ComplianceDescription.ADE_DescriptionInfo);
		}

		public void TestADE_Description_CheckAllAlphaNumericAndSpecialCharactersAllowed()
		{
			ComplianceDescription.ADE_Description = "T4à_!#$%&)*,-/0;<=@[^`{|}~+ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			ComplianceDescription.RunPreSaveValidation();
			AssertNoErrors(ComplianceDescription.ADE_DescriptionInfo);
		}

		public void TestSeveralChargesCodesCanUseSameADE_Description()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			var chargeCode2 = Factory.New<AccChargeCode>();
			var chargeComplianceDescription1 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			var chargeComplianceDescription2 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			chargeComplianceDescription1.ADE_AC = chargeCode1.PK;
			chargeComplianceDescription1.ADE_Description = "Test";

			chargeComplianceDescription2.ADE_AC = chargeCode2.PK;
			chargeComplianceDescription2.ADE_Description = "Test";
			chargeComplianceDescription1.RunPreSaveValidation();
			chargeComplianceDescription2.RunPreSaveValidation();
			AssertNoErrors(chargeComplianceDescription1.ADE_DescriptionInfo);
			AssertNoErrors(chargeComplianceDescription2.ADE_DescriptionInfo);
		}

		public void TestSeveralRowsCanHaveSameADE_Description()
		{
			ComplianceDescription.ADE_Description = "Test";

			var chargeComplianceDescription1 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			chargeComplianceDescription1.ADE_AC = ComplianceDescription.ADE_AC;
			chargeComplianceDescription1.ADE_Description = "Test";

			ComplianceDescription.RunPreSaveValidation();
			chargeComplianceDescription1.RunPreSaveValidation();

			AssertNoErrors(ComplianceDescription.ADE_DescriptionInfo);
			AssertNoErrors(chargeComplianceDescription1.ADE_DescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ChargeCode = Factory.New<AccChargeCode>();
			ComplianceDescription = ChargeCode.ChargeComplianceDescriptions.AddNew();
		}

		AccChargeCode ChargeCode;
		AccChargeComplianceDescription ComplianceDescription;
	}
}
