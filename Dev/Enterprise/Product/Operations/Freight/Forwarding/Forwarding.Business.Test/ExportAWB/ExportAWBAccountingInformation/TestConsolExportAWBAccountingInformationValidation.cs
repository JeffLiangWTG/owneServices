using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class TestConsolExportAWBAccountingInformationValidation : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			AccountingInformation = Factory.New<ConsolExportAWBAccountingInformation>();
			Validation = new ConsolExportAWBAccountingInformationValidation(AccountingInformation);
		}

		ConsolExportAWBAccountingInformation AccountingInformation;
		ConsolExportAWBAccountingInformationValidation Validation;

		public void TestCheckEA_Information()
		{
			Validation.ValidateEA_Information();
			Assert(Validation.Parent.EA_InformationInfo.HasWarnings());

			AccountingInformation.EA_Information = "GEN";
			Validation.ValidateEA_Information();
			Assert(!Validation.Parent.EA_InformationInfo.HasWarnings());
		}

		public void TestCheckEA_InformationID()
		{
			Validation.ValidateEA_InformationID();
			Assert(Validation.Parent.EA_InformationIDInfo.HasWarnings());

			AccountingInformation.EA_InformationID = "GEN";
			Validation.ValidateEA_InformationID();
			Assert(!Validation.Parent.EA_InformationIDInfo.HasWarnings());

			AccountingInformation.EA_Information = "ThisIsATextWithMoreThan34Characters";
			Validation.ValidateEA_Information();
			Assert(Validation.Parent.EA_InformationInfo.HasWarnings());

			AccountingInformation.EA_Information = "ThisTextIsLessThan34Characters";
			Validation.ValidateEA_Information();
			Assert(!Validation.Parent.EA_InformationInfo.HasWarnings());

			AccountingInformation.EA_InformationID = ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA;
			Validation.ValidateEA_InformationID();
			Assert(!Validation.Parent.EA_InformationIDInfo.HasNotifications());

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";
			Validation.ValidateEA_InformationID();
			Assert(!Validation.Parent.EA_InformationIDInfo.HasNotifications());

			AccountingInformation.EA_InformationID = ExportAWBAccountingInformationLookups.IssuedByIVA;
			Validation.ValidateEA_InformationID();
			Assert(!Validation.Parent.EA_InformationIDInfo.HasNotifications());

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			Validation.ValidateEA_InformationID();
			Assert(!Validation.Parent.EA_InformationIDInfo.HasNotifications());
		}
	}
}
