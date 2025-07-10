using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		[TestDate(2018, 11, 20)]
		public void TestCheckUS_ReleaseEntryNumber()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_ConsolACE = true;
			declaration.US_SchDEntry = "2222";
			declaration.IOROrgPK = importer.PK;
			declaration.ConsigneeAddressOrgPK = consignee.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "Cargo123";
			declaration.JE_VoyageFlightNo = "VG123";
			declaration.US_EntryFilerCode = "SV9";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.US_ReleaseEntryNumber = ZString.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "66666666";
			entry.CusEntryNumber.CE_EntryType = "ENS";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var releaseDeclaration1 = Factory.New<JobDeclaration>();
			releaseDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration1.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			releaseDeclaration1.JE_DeclarationReference = "B00001111";
			releaseDeclaration1.US_EntryFilerCode = "SV9";
			releaseDeclaration1.US_SchDEntry = "1111";
			releaseDeclaration1.JE_TransportMode = TransportTypeList.Codes.Air;
			var releaseEntry1 = releaseDeclaration1.CustomsEntryHeaders.AddNew();
			releaseEntry1.EntryNumber = "12345678";
			releaseEntry1.CusEntryNumber.CE_EntryType = "ENS";
			releaseEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var releaseEntry11 = Factory.New<CusEntryHeaderForTesting>();
			releaseEntry11.CH_JE = releaseDeclaration1.PK;
			releaseEntry11.EntryNumber = "12345678";
			releaseEntry11.CusEntryNumber.CE_EntryType = "ENS";
			releaseEntry11.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			releaseEntry11.HasBeenLodgedAtCustomsReturns = true;
			var releaseDeclaration2 = Factory.New<JobDeclaration>();
			releaseDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDeclaration2.JE_DeclarationReference = "B00002222";
			releaseDeclaration2.US_EntryFilerCode = "SV9";
			releaseDeclaration2.US_SchDEntry = "2222";
			releaseDeclaration2.IOROrgPK = importer.PK;
			releaseDeclaration2.ConsigneeAddressOrgPK = consignee.PK;
			releaseDeclaration2.JE_TransportMode = TransportTypeList.Codes.Air;
			releaseDeclaration2.US_UI_NKCarrierSCAC = "ZZZZ";
			releaseDeclaration2.JE_EntryAuthorisationDate = new ZDateTime(2018, 11, 1);
			releaseDeclaration2.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			var releaseEntry2 = releaseDeclaration2.CustomsEntryHeaders.AddNew();
			releaseEntry2.EntryNumber = "87654321";
			releaseEntry2.CusEntryNumber.CE_EntryType = "ENS";
			releaseEntry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var releaseEntry22 = Factory.NewMoq<CusEntryHeader>();
			releaseEntry22.Object.CH_JE = releaseDeclaration2.PK;
			releaseEntry22.Object.EntryNumber = "87654321";
			releaseEntry22.Object.CusEntryNumber.CE_EntryType = "ENS";
			releaseEntry22.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			releaseEntry22.Setup(m => m.HasBeenLodgedAtCustoms).Returns(false);
			Factory.Save();
			invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EntryFilerEntryNumberMandatory);
			invoice.US_ReleaseEntryNumber = "SV911111111";
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EntryFilerEntryNumberMandatory);
			AssertHasMessageErrorContaining(invoice.US_ReleaseEntryNumberInfo, "does not exist");
			invoice.US_ReleaseEntryNumber = "SV912345678";
			AssertNoMessageErrorContaining(invoice.US_ReleaseEntryNumberInfo, "does not exist");
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.PortOfEntryShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ImporterOfRecordShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ConsigneeShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.TransportModeShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseDateShouldExist);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EntryTypeNotAccepted);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.MessageTypeNotAccepted);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseStatusNotAccepted);
			releaseDeclaration1.JE_TransportMode = TransportTypeList.Codes.Sea;
			releaseDeclaration1.JE_VesselName = "Cargo123";
			releaseDeclaration1.JE_VoyageFlightNo = "VG456";
			Factory.Save();
			invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.TransportModeShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.VesselVoyageShouldMatch);
			releaseDeclaration1.JE_VoyageFlightNo = "VG123";
			Factory.Save();
			invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.VesselVoyageShouldMatch);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			Factory.Save();
			invoice.US_ReleaseEntryNumber = "SV987654321";
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.PortOfEntryShouldMatch);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ImporterOfRecordShouldMatch);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ConsigneeShouldMatch);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseDateShouldExist);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EntryTypeNotAccepted);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseStatusNotAccepted);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.MessageTypeNotAccepted);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.AirCarrierCodeShouldMatch);
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseDatePastLimit);
			releaseDeclaration2.US_UI_NKCarrierSCAC = "UNKN";
			releaseDeclaration2.JE_EntryAuthorisationDate = new ZDateTime(2018, 11, 8);
			Factory.Save();
			invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.AirCarrierCodeShouldMatch);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.ReleaseDatePastLimit);
			AssertNoMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EnterEntryNumberFromReleaseOnlyDeclaration);
			invoice.US_ReleaseEntryNumber = "SV966666666";
			AssertHasMessageError(invoice.US_ReleaseEntryNumberInfo, AddInfoJobComInvoiceHeaderValidation.EnterEntryNumberFromReleaseOnlyDeclaration);
		}

		public void TestCheckUS_ReleaseEntryNumber_WhenCreatedFromUSLowValue_ShouldNotValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_ConsolACE = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.US_ReleaseEntryNumber = ZString.Empty;

			declaration.Logs.AddNew(AutoEvents.Transferred,
			[
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "USLV"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "LV001")
			]);

			invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
			AssertNoMessageErrors(invoice.US_ReleaseEntryNumberInfo);
		}

		public void TestCheckUS_UltimateDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			var org01 = Factory.New<OrgHeader>();
			org01.FillWithValidTestData();
			org01.OH_RL_NKClosestPort = "USTES";
			var org02 = Factory.New<OrgHeader>();
			org02.FillWithValidTestData();
			org02.OH_RL_NKClosestPort = "CATES";
			declaration.JE_OH_Importer = org01.PK;
			var invoice01 = declaration.Invoices.AddNew();
			var invoice02 = declaration.Invoices.AddNew();
			var invoice03 = declaration.Invoices.AddNew();
			var invoice04 = declaration.Invoices.AddNew();
			var line01 = invoice01.InvoiceLines.AddNew();
			var line02 = invoice02.InvoiceLines.AddNew();
			var line03 = invoice03.InvoiceLines.AddNew();
			var line04 = invoice04.InvoiceLines.AddNew();
			invoice01.JZ_OH_Buyer = org01.PK;
			invoice02.JZ_OH_Buyer = org02.PK;
			invoice03.JZ_OH_Buyer = org02.PK;
			invoice04.JZ_OH_Buyer = org01.PK;
			invoice03.US_UltimateDestinationCountry = "XX";
			invoice04.US_UltimateDestinationCountry = "XX";
			invoice01.AddInfoValidation.ValidateUS_UltimateDestinationCountry();
			invoice02.AddInfoValidation.ValidateUS_UltimateDestinationCountry();
			invoice03.AddInfoValidation.ValidateUS_UltimateDestinationCountry();
			invoice04.AddInfoValidation.ValidateUS_UltimateDestinationCountry();
			AssertNoWarnings(invoice01.US_UltimateDestinationCountryInfo);
			AssertHasWarningContaining(invoice02.US_UltimateDestinationCountryInfo, ZString.Format(UltimateDestinationCountryUpdatedWarning, Core.Constants.CountryCodes.Canada));
			AssertHasWarningContaining(invoice03.US_UltimateDestinationCountryInfo, ZString.Format(UltimateDestinationCountryUpdatedWarning, "XX"));
			AssertHasMessageErrorContaining(invoice03.US_UltimateDestinationCountryInfo, UltimateDestinationCountryUpdatedMessageError);
			AssertHasMessageErrorContaining(invoice04.US_UltimateDestinationCountryInfo, UltimateDestinationCountryUpdatedMessageError);
			invoice02.UltimateConsigneeDocAddress.E2_AddressOverride = true;
			invoice02.AddInfoValidation.ValidateUS_UltimateDestinationCountry();
			AssertNoWarningContaining(invoice02.US_UltimateDestinationCountryInfo, ZString.Format(UltimateDestinationCountryUpdatedWarning, Core.Constants.CountryCodes.Canada));
		}

		internal const string UltimateDestinationCountryUpdatedWarning = "Ult. Country of Destination: '{0}' has been automatically updated to match the current Consignee - you may manually change this if required.";
		internal const string UltimateDestinationCountryUpdatedMessageError = "The code you have selected is not in the list.";

		sealed class CusEntryHeaderForTesting : CusEntryHeader
		{
			public CusEntryHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool HasBeenLodgedAtCustomsReturns { get; set; }

			public override bool HasBeenLodgedAtCustoms => HasBeenLodgedAtCustomsReturns;
		}
	}
}
