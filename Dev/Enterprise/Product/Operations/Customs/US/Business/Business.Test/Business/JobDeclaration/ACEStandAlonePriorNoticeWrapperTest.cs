using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class ACEStandAlonePriorNoticeWrapperTest : TestCaseWithFactory
	{
		public void TestPriorNoticeHeadersForEntryNumber()
		{
			Declaration.Bills.RemoveAndDeleteAll();
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);

			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("HumanReadableName", "Entry Number(XJ523648975)", priorNoticeHeader.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.ENT, priorNoticeHeader.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", "XJ5", priorNoticeHeader.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "23648975", priorNoticeHeader.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", ZString.Empty, priorNoticeHeader.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader.ImportingCarrierSCAC);
			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionFreeDutiable, priorNoticeHeader.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 0, priorNoticeHeader.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 1, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());

			Declaration.JE_MasterBillIssuerSCAC = "APLU";
			Declaration.JE_MasterBill = "MB11111111";
			Declaration.JE_HouseBillIssuerSCAC = "SXXX";
			Declaration.JE_HouseBill = "HB1111111";
			var houseBill = Declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.HouseBill);
			houseBill.US_UI_NKBillIssuerSCAC = "SXXX";
			houseBill.CU_BillNum = "HB222222";
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ChildBills.Count", 1, priorNoticeHeader.Bills.Count());

			Declaration.InvoiceLines.RemoveAndDeleteAll();
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 0, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());
		}

		public void TestPriorNoticeHeaderForSingleMasterBill()
		{
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);

			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("HumanReadableName", "Master Bill(APLUMB11111111)", priorNoticeHeader.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.BOL, priorNoticeHeader.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", "APLU", priorNoticeHeader.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "MB11111111", priorNoticeHeader.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", SEBillTypesList.Codes.MasterBill, priorNoticeHeader.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader.ImportingCarrierSCAC);
			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionFreeDutiable, priorNoticeHeader.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 1, priorNoticeHeader.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 1, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());

			Declaration.InvoiceLines.RemoveAndDeleteAll();
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 0, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());
		}

		public void TestPriorNoticeHeaderForMultipleMasterBills()
		{
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var masterBill1 = Declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			masterBill1.CU_BillNum = "MB22222222";
			var invoice1 = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = FDAPriorNoticeRequiredTariff.UE_Tariff;
			invoiceLine1.ACE_FDALines.AddNew();
			invoiceLine1.ACE_FDALines.AddNew();

			Declaration.Invoices[0].JZ_CU_RelatedHouseBill = Declaration.PrimaryHouseBill.PK;
			Declaration.Invoices[1].JZ_CU_RelatedHouseBill = masterBill1.PK;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(Declaration);
			AssertEquals(2, priorNoticeWrapper.PriorNoticeHeaders.Count);

			var priorNoticeHeader0 = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("HumanReadableName", "Master Bill(APLUMB11111111)", priorNoticeHeader0.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.BOL, priorNoticeHeader0.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", "APLU", priorNoticeHeader0.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "MB11111111", priorNoticeHeader0.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", SEBillTypesList.Codes.MasterBill, priorNoticeHeader0.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader0.ImportingCarrierSCAC);
			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionFreeDutiable, priorNoticeHeader0.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader0.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 1, priorNoticeHeader0.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 1, priorNoticeHeader0.ACEStandalonePriorNoticeLines.Count());

			var priorNoticeHeader1 = priorNoticeWrapper.PriorNoticeHeaders[1];
			AssertEquals("HumanReadableName", "Master Bill(MB22222222)", priorNoticeHeader1.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.BOL, priorNoticeHeader1.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", ZString.Empty, priorNoticeHeader1.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "MB22222222", priorNoticeHeader1.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", SEBillTypesList.Codes.RegularBill, priorNoticeHeader1.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader1.ImportingCarrierSCAC);
			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionFreeDutiable, priorNoticeHeader1.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader1.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 0, priorNoticeHeader1.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 2, priorNoticeHeader1.ACEStandalonePriorNoticeLines.Count());
		}

		public void TestPriorNoticeHeaderForFTZPGAWithAdmissionNumberType()
		{
			FTZDeclaraiton.Bills.RemoveAndDeleteAll();
			FTZDeclaraiton.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(FTZDeclaraiton);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);

			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("HumanReadableName", "FTZ Admission Number(21400001700000001)", priorNoticeHeader.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.FTZ, priorNoticeHeader.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", ZString.Empty, priorNoticeHeader.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "21400001700000001", priorNoticeHeader.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", ZString.Empty, priorNoticeHeader.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader.ImportingCarrierSCAC);
			AssertEquals("EntryType", "81", priorNoticeHeader.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 0, priorNoticeHeader.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 1, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());

			FTZDeclaraiton.JE_MasterBillIssuerSCAC = "APLU";
			FTZDeclaraiton.JE_MasterBill = "MB11111111";
			FTZDeclaraiton.JE_HouseBillIssuerSCAC = "SXXX";
			FTZDeclaraiton.JE_HouseBill = "HB1111111";
			var houseBill = FTZDeclaraiton.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.HouseBill);
			houseBill.US_UI_NKBillIssuerSCAC = "SXXX";
			houseBill.CU_BillNum = "HB222222";
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(FTZDeclaraiton);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ChildBills.Count", 1, priorNoticeHeader.Bills.Count());

			FTZDeclaraiton.InvoiceLines.RemoveAndDeleteAll();
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(FTZDeclaraiton);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 0, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());
		}

		public void TestPriorNoticeHeaderForFTZPGAWithBillNumberType()
		{
			FTZDeclaraiton.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(FTZDeclaraiton);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);

			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("HumanReadableName", "Master Bill(APLUMB11111111)", priorNoticeHeader.HumanReadableName);
			AssertEquals("ReferenceQualifierCode", PriorNoticeReferenceQualifierCodeList.Codes.BOL, priorNoticeHeader.ReferenceQualifierCode);
			AssertEquals("FilerOrIssuerCode", "APLU", priorNoticeHeader.FilerOrIssuerCode);
			AssertEquals("ReferenceIdentifierNumber", "MB11111111", priorNoticeHeader.ReferenceIdentifierNumber);
			AssertEquals("BillTypeIndicator", SEBillTypesList.Codes.MasterBill, priorNoticeHeader.BillTypeIndicator);
			AssertEquals("ImportingCarrierSCAC", "APLU", priorNoticeHeader.ImportingCarrierSCAC);
			AssertEquals("EntryType", "81", priorNoticeHeader.EntryType);
			AssertEquals("ModeOfTransportationCode", TransportModeCodes.Codes.VesselContainer, priorNoticeHeader.ModeOfTransportationCode);
			AssertEquals("ChildBills.Count", 1, priorNoticeHeader.Bills.Count());
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 1, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());

			FTZDeclaraiton.InvoiceLines.RemoveAndDeleteAll();
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(FTZDeclaraiton);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			AssertEquals("ACEStandalonePriorNoticeLines.Count", 0, priorNoticeHeader.ACEStandalonePriorNoticeLines.Count());
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.ImportEntryNumber = "23648975";
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableSPN = true;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					declaration.JE_MasterBillIssuerSCAC = "APLU";
					declaration.JE_MasterBill = "MB11111111";
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
					declaration.JE_HouseBillIssuerSCAC = "SXXX";
					declaration.JE_HouseBill = "HB1111111";

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = FDAPriorNoticeRequiredTariff.UE_Tariff;

					invoiceLine.ACE_FDALines.AddNew();
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		JobDeclaration FTZDeclaraiton
		{
			get
			{
				if (ftzDeclaration == null)
				{
					ftzDeclaration = Factory.New<JobDeclaration>();
					ftzDeclaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.FTZ;
					ftzDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					ftzDeclaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
					ftzDeclaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
					ftzDeclaration.US_EnableSPN = true;
					ftzDeclaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
					ftzDeclaration.JE_MasterBillIssuerSCAC = "APLU";
					ftzDeclaration.JE_MasterBill = "MB11111111";
					ftzDeclaration.JE_HouseBillIssuerSCAC = "SXXX";
					ftzDeclaration.JE_HouseBill = "HB1111111";
					ftzDeclaration.FTZAdmissionNumber = "2140000|17|00000001";

					var invoice = ftzDeclaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = FDAPriorNoticeRequiredTariff.UE_Tariff;
					invoiceLine.ACE_FDALines.AddNew();
				}

				return ftzDeclaration;
			}
		}
		JobDeclaration ftzDeclaration;

		USCTariff FDAPriorNoticeRequiredTariff
		{
			get
			{
				if (fdaPriorNoticeRequiredTariff == null)
				{
					fdaPriorNoticeRequiredTariff = Factory.New<USCTariff>();
					fdaPriorNoticeRequiredTariff.UE_Tariff = "2234567890";
					fdaPriorNoticeRequiredTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
					fdaPriorNoticeRequiredTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
					fdaPriorNoticeRequiredTariff.UE_PGACodes = "AP2FD4";
					fdaPriorNoticeRequiredTariff.UE_OGACodes = "AAAFD4";
				}
				return fdaPriorNoticeRequiredTariff;
			}
		}
		USCTariff fdaPriorNoticeRequiredTariff;

		#endregion
	}
}
