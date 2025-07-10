using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CertificateOfOriginCusSupportingValidation))]
	sealed class CertificateOfOriginCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCustomsRequirementSPartially()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200000", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "S*", tariff2);
			Factory.Save();
			var certificateOfOriginCusSupporting = invoiceLine.CertificateOfOriginCusSupporting;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.RunPreSaveValidation();
			AssertNoWarning(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, ValidationConstants.InvoiceLine.CustomsRequirementSPartially);
			invoiceLine.JI_Tariff = "2713200001";
			certificateOfOriginCusSupporting = invoiceLine.CertificateOfOriginCusSupporting;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			invoiceLine.RunPreSaveValidation();
			AssertHasWarning(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, ValidationConstants.InvoiceLine.CustomsRequirementSPartially);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			certificateOfOriginCusSupporting.CSI_LineNo = 1;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_LineNo = 1;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "01234657890123465789012346578901234";
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_LineNo = ZShort.Zero;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PrimaryPreference = Constants.PreferenceCodes.Preference2;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "01234657890123465789012346578901234";
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PrimaryPreference = Constants.PreferenceCodes.ProvisionalPreference2;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "01234657890123465789012346578901234";
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PrimaryPreference = Constants.PreferenceCodes.Preference;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "01234657890123465789012346578901234";
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining(certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_LineNo()
		{
			var targetInfo = certificateOfOriginCusSupporting.CSI_LineNoInfo;
			certificateOfOriginCusSupporting.CSI_LineNo = ZShort.Zero;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = ZString.Empty;
			certificateOfOriginCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "01234657890123465789012346578901234";
			certificateOfOriginCusSupporting.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			certificateOfOriginCusSupporting.CSI_LineNo = 1;
			certificateOfOriginCusSupporting.RunPreSaveValidation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation
		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
		CertificateOfOriginCusSupporting certificateOfOriginCusSupporting;
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			certificateOfOriginCusSupporting = invoiceLine.CertificateOfOriginCusSupportingCollection.AddNew();
		}
		#endregion
	}
}
