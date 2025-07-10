using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitKnownConsignorHelperTest : TransitUniversalTestCase
	{
		#region IsTransportCompanyKnown

		public void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_DoesNotExist()
		{
			Data.SetupForForwardingImport();
			var rtu = Factory.BOFactory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_WW_Warehouse = Data.Warehouse.PK;
			rtu.WRH_ReferenceNumber = "RTU001";
			AssertEquals(false, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(rtu));
		}

		public void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_DoesNotHaveCountryCode()
		{
			Data.SetupForForwardingImport();
			var rtu = Factory.BOFactory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_WW_Warehouse = Data.Warehouse.PK;
			rtu.WRH_ReferenceNumber = "RTU001";

			var docAddress = Factory.BOFactory.New<JobDocAddress>();
			var orgAddress = Factory.BOFactory.New<OrgAddress>();
			docAddress.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			rtu.DocAddresses.Add(docAddress);

			AssertEquals(false, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(rtu));
		}

		public void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_MissingDriverSignature()
		{
			Data.SetupForForwardingImport();
			var rtu = Factory.BOFactory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_WW_Warehouse = Data.Warehouse.PK;
			rtu.WRH_ReferenceNumber = "RTU001";

			var docAddress = Factory.BOFactory.New<JobDocAddress>();
			var orgAddress = Factory.BOFactory.New<OrgAddress>();
			docAddress.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			rtu.DocAddresses.Add(docAddress);

			var org = Data.Orgs.WUFSHIJNB;
			orgAddress.OA_OH = org.PK;

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rtu.WRH_SignedBy = ZString.Empty;

			AssertEquals(false, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(rtu));
		}

		public void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_BKGCertificateIsInvalid() => TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_CertificateIsInvalid("BKG");

		public void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_DTACertificateIsInvalid() => TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_CertificateIsInvalid("DTA");

		protected void TestIsTransportCompanyKnown_TransportCompanyDocumentaryAddress_CertificateIsInvalid(string certificateType)
		{
			Data.SetupForForwardingImport();
			var rtu = Factory.BOFactory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_WW_Warehouse = Data.Warehouse.PK;
			rtu.WRH_ReferenceNumber = "RTU001";

			var docAddress = Factory.BOFactory.New<JobDocAddress>();
			var orgAddress = Factory.BOFactory.New<OrgAddress>();
			docAddress.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			rtu.DocAddresses.Add(docAddress);

			var org = Data.Orgs.WUFSHIJNB;
			orgAddress.OA_OH = org.PK;

			var driver = org.Contacts.AddNew();
			driver.OC_ContactName = "Test Driver";
			var cert = driver.Certificates.AddNew();
			cert.XZ_Type = certificateType;

			var otherType = certificateType == "BKG" ? "DTA" : "BKG";
			var cert2 = driver.Certificates.AddNew();
			cert2.XZ_Type = otherType;
			cert2.XZ_ExpiryOrDueDate = ZDateTime.Now.AddDays(2);

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rtu.WRH_SignedBy = driver.OC_ContactName;

			AssertEquals(false, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(rtu));

			cert.XZ_ExpiryOrDueDate = ZDateTime.Now.AddDays(-2);
			AssertEquals(false, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(rtu));
		}

		#endregion
	}
}
