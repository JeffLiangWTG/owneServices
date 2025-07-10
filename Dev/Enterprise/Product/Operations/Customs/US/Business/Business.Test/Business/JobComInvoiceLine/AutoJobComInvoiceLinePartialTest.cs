using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoJobComInvoiceLinePartialTest : TestCaseWithFactory
	{
		public void TestHasEmptySupTariff()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(true, invoiceLine.HasEmptySupTariff);

			invoiceLine.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			AssertEquals(true, invoiceLine.HasEmptySupTariff);

			invoiceLine.US_SupTariff = "9999999999";
			AssertEquals(false, invoiceLine.HasEmptySupTariff);
		}

		public void TestDefaultCountryOfOriginFromManufacturer()
		{
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			OrgHeader org = Factory.New<OrgHeader>();
			org.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotEquals(Core.Constants.CountryCodes.NewZealand, invoiceLine.US_UC_NKCountryOfOrigin);
			invoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK = org.PK;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, invoiceLine.US_UC_NKCountryOfOrigin);
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = ZString.Empty;
			invoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK = org.PK;
			AssertNotEquals(Core.Constants.CountryCodes.NewZealand, invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestShowManufacturerIDInAddressList()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, invoiceLine.JI_OA_ManufacturerAddress_ZAddress);
		}

		public void TestIsSetXAndV()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			Assert(!invoiceLine1.IsSetXLine);
			Assert(!invoiceLine2.IsSetVLine);

			invoiceLine1.US_SecondarySPI = ZString.Empty;
			invoiceLine2.US_SecondarySPI = ZString.Empty;
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			Assert(invoiceLine1.IsSetXLine);
			Assert(invoiceLine2.IsSetVLine);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine1.US_SecondarySPI = ZString.Empty;
			invoiceLine2.US_SecondarySPI = ZString.Empty;
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			Assert(!invoiceLine1.IsSetXLine);
			Assert(!invoiceLine2.IsSetVLine);

			invoiceLine1.US_SetInd = ZString.Empty;
			invoiceLine2.US_SetInd = ZString.Empty;
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			Assert(invoiceLine1.IsSetXLine);
			Assert(invoiceLine2.IsSetVLine);
		}

		public void TestTSCAFieldsAreReadonlyForPGATracking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("US_TSCAODSCertIndividual", true, invoiceLine.US_TSCAODSCertIndividualInfo.ReadOnly);
			AssertEquals("US_TSCACertification", true, invoiceLine.US_TSCACertificationInfo.ReadOnly);
			AssertEquals("US_FDAContactEmail", true, invoiceLine.US_FDAContactEmailInfo.ReadOnly);
			AssertEquals("US_FDAContactName", true, invoiceLine.US_FDAContactNameInfo.ReadOnly);
			AssertEquals("US_FDAContactPhoneNo", true, invoiceLine.US_FDAContactPhoneNoInfo.ReadOnly);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("US_TSCAODSCertIndividual", false, invoiceLine.US_TSCAODSCertIndividualInfo.ReadOnly);
			AssertEquals("US_TSCACertification", false, invoiceLine.US_TSCACertificationInfo.ReadOnly);
			AssertEquals("US_FDAContactEmail", false, invoiceLine.US_FDAContactEmailInfo.ReadOnly);
			AssertEquals("US_FDAContactName", false, invoiceLine.US_FDAContactNameInfo.ReadOnly);
			AssertEquals("US_FDAContactPhoneNo", false, invoiceLine.US_FDAContactPhoneNoInfo.ReadOnly);
			invoiceLine.US_TSCATrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals("US_TSCAODSCertIndividual", true, invoiceLine.US_TSCAODSCertIndividualInfo.ReadOnly);
			AssertEquals("US_TSCACertification", true, invoiceLine.US_TSCACertificationInfo.ReadOnly);
			AssertEquals("US_FDAContactEmail", true, invoiceLine.US_FDAContactEmailInfo.ReadOnly);
			AssertEquals("US_FDAContactName", true, invoiceLine.US_FDAContactNameInfo.ReadOnly);
			AssertEquals("US_FDAContactPhoneNo", true, invoiceLine.US_FDAContactPhoneNoInfo.ReadOnly);
		}
	}
}
