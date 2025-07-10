using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGA))]
	public class PGATest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<PGA>
	{
		public void TestDefaultPGAContactInfoForCommercialInvoice_CurrentDepartmentImportIsFalse()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(importer, "FIRST", "LAST", "123456", "ior@ian.com", null);
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			var pga = pivot.PGAs.AddNew();
			pga.US_PGACommercialDescription = "TST";
			pga.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			Factory.Save();

			var invoice = Factory.New<JobComInvoiceHeader>();
			var declaration = new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Buyer = importer.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			var laceyActLine = invoiceLine.LaceyActLines[0];
			AssertNotNull(laceyActLine);
			AssertEquals("FIRST LAST", laceyActLine.US_PGAContactName);
			AssertEquals("123456", laceyActLine.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", laceyActLine.US_PGAContactEmail);
			Factory.Save();

			GlbDepartment.CurrentDepartment.GE_Import = false;
			var newFactory = new BusinessObjectFactory();
			var invoice1 = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var declaration1 = new FakeDeclarationCreatorForInvoice(invoice1).HeaderData;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = product.OP_PartNum;
			var laceyActLine1 = invoiceLine1.LaceyActLines[0];
			AssertNotNull(laceyActLine1);
			AssertEquals("FIRST LAST", laceyActLine1.US_PGAContactName);
			AssertEquals("123456", laceyActLine1.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", laceyActLine1.US_PGAContactEmail);
		}

		public void TestPGALineReadOnly()
		{
			PGA.US_PGACommercialDescription = "COMDES";
			PGA.US_InvCurrPGAValue = 5m;
			PGA.US_UnknownBreakdown = ZBool.True;
			Factory.Save();
			PGA.OnLoaded();
			Assert(!PGA.ReadOnly);

			PGA.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			PGA.OnLoaded();
			Assert(PGA.ReadOnly);

			PGA.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			PGA.OnLoaded();
			Assert(!PGA.ReadOnly);

			PGA.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			PGA.OnLoaded();
			Assert(PGA.ReadOnly);

			PGA.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			PGA.OnLoaded();
			Assert(PGA.ReadOnly);
		}

		public void TestUS_CertifyingIndividual()
		{
			var iorOrgHeader = Factory.New<OrgHeader>();
			iorOrgHeader.OH_Code = "TESTIOR";

			DeclarationTestHelper.AddPGAContact(iorOrgHeader, "FIRST", "LAST", "123456", "ior@ian.com", null);
			PGA.US_CertifyingIndividual = ZString.Empty;
			var declaration = PGA.InvoiceLine.Declaration;
			declaration.IOROrgPK = iorOrgHeader.PK;

			var invoice = PGA.InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "Broker NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@ian.com";

			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "TESTOWN";
			DeclarationTestHelper.AddPGAContact(ownerOrgHeader, "OWNER", "TEST", "234567", "owner@ian.com", null);

			PGA.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			AssertEquals("FIRST LAST", PGA.US_PGAContactName);
			AssertEquals("123456", PGA.US_PGAContactPhoneNo);
			AssertEquals("ior@ian.com", PGA.US_PGAContactEmail);

			PGA.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertEquals("Broker NAME", PGA.US_PGAContactName);
			AssertEquals("2345678", PGA.US_PGAContactPhoneNo);
			AssertEquals("broker@ian.com", PGA.US_PGAContactEmail);
		}

		public void TestTransformDataACEToACS()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var constituentElement1 = PGA.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "T1";
			constituentElement1.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";
			constituentElement1.US_GenusName = "123";
			constituentElement1.US_UnknownBreakdownCountryCode = "MX";
			constituentElement1.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.Composite;

			var constituentElement2 = PGA.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "T2";
			constituentElement2.US_PGAQuantityOfConstituentElement = 120m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			constituentElement2.US_GenusName = "896";
			constituentElement2.US_UnknownBreakdownCountryCode = "AU";
			constituentElement2.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.SPF;

			var pga2 = invoiceLine.LaceyActLines.AddNew();
			var constituentElement3 = pga2.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "T3";
			constituentElement3.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_GenusName = "ABC";
			constituentElement3.US_UnknownBreakdownCountryCode = "US";
			constituentElement3.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.PreAmendment;

			var constituentElement4 = pga2.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "T3";
			constituentElement4.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement4.US_PGAUnitOfMeasure = "M3";
			constituentElement4.US_GenusName = "ABC";
			constituentElement4.US_UnknownBreakdownCountryCode = "CA";
			constituentElement4.US_SpecialUseDesignation = true;
			constituentElement4.US_SpeciesName = PGALaceySpeciesNameCodeList.Codes.Recycled;

			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			Assert(PGA.PG04ConstituentElements.Count == 2);
			Assert(PGA.PG04ConstituentElements[0].ScientificDataCollection.Count == 1);
			Assert(PGA.PG04ConstituentElements[1].ScientificDataCollection.Count == 1);

			Assert(pga2.PG04ConstituentElements.Count == 2);
			Assert(pga2.PG04ConstituentElements[0].ScientificDataCollection.Count == 1);
			Assert(pga2.PG04ConstituentElements[0].ScientificDataCollection.Cast<ScientificData>().Any(x => x.US_PGACountryCode == "US" || x.US_PGACountryCode == "CA"));

			Assert(pga2.PG04ConstituentElements[1].ScientificDataCollection.Count == 1);
			Assert(pga2.PG04ConstituentElements[1].ScientificDataCollection.Cast<ScientificData>().Any(x => x.US_PGACountryCode == "US" || x.US_PGACountryCode == "CA"));
		}

		public void TestTransformDataACSToACE()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var constituentElement1 = PGA.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "T1";
			constituentElement1.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";

			var scientificData1 = constituentElement1.ScientificDataCollection.AddNew();
			scientificData1.US_PGAScientificGenusName = "123";
			scientificData1.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.Recycled;
			scientificData1.US_PGACountryCode = "MX";

			var constituentElement2 = PGA.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "T2";
			constituentElement2.US_PGAQuantityOfConstituentElement = 120m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			var scientificData2 = constituentElement2.ScientificDataCollection.AddNew();
			scientificData2.US_PGAScientificGenusName = "896";
			scientificData2.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.SPF;
			scientificData2.US_PGACountryCode = "AU";

			var pga2 = invoiceLine.LaceyActLines.AddNew();
			var constituentElement3 = pga2.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "T3";
			constituentElement3.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";

			var scientificData3 = constituentElement3.ScientificDataCollection.AddNew();
			scientificData3.US_PGAScientificGenusName = "ABC";
			scientificData3.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.PreAmendment;
			scientificData3.US_PGACountryCode = "US";

			var scientificData4 = constituentElement3.ScientificDataCollection.AddNew();
			scientificData4.US_PGAScientificGenusName = "SPECIAL";
			scientificData4.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.Composite;
			scientificData4.US_PGACountryCode = "CA";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			AssertEquals(2, PGA.PG04ConstituentElements.Count);
			AssertEquals(0, PGA.PG04ConstituentElements[0].ScientificDataCollection.Count);
			AssertEquals(0, PGA.PG04ConstituentElements[1].ScientificDataCollection.Count);
			AssertEquals(1, PGA.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "MX" && x.US_GenusName == "123"));
			AssertEquals(1, PGA.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "AU" && x.US_GenusName == "896"));

			AssertEquals(2, pga2.PG04ConstituentElements.Count);
			AssertEquals(0, pga2.PG04ConstituentElements[0].ScientificDataCollection.Count);
			AssertEquals(0, pga2.PG04ConstituentElements[1].ScientificDataCollection.Count);
			AssertEquals(1, pga2.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "US" && x.US_GenusName == "ABC"));
			AssertEquals(1, pga2.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "CA" && x.US_GenusName == "SPECIAL"));
		}

		public void TestCopyFromProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;
			var pga = pivot.PGAs.AddNew();
			pga.US_PGACommercialDescription = "TST";
			var constituentElement1 = pga.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "ABC";
			constituentElement1.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";
			constituentElement1.US_GenusName = "1";
			constituentElement1.US_SpeciesName = "1";
			constituentElement1.US_UnknownBreakdownCountryCode = "AW";

			var constituentElement2 = pga.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "ABC";
			constituentElement2.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_GenusName = "2";
			constituentElement2.US_SpeciesName = "2";
			constituentElement1.US_UnknownBreakdownCountryCode = "SG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
			var laceys = invoiceLine.LaceyActLines;
			var pga2 = laceys[0];

			AssertEquals(pga2.PG04ConstituentElements.Count, 2);
			AssertEquals("1", pga2.PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("", pga2.PG04ConstituentElements[0].US_UnknownBreakdownCountryCode);
			AssertEquals("2", pga2.PG04ConstituentElements[1].ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("", pga2.PG04ConstituentElements[1].US_UnknownBreakdownCountryCode);
		}

		public void TestCopyToACEFromProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;

			var pga = pivot.PGAs.AddNew();
			pga.US_PGACommercialDescription = "TST";

			var constituentElement1 = pga.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "ABC";
			constituentElement1.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";
			constituentElement1.US_GenusName = "1";
			constituentElement1.US_SpeciesName = "1";
			constituentElement1.US_UnknownBreakdownCountryCode = "AW";

			var constituentElement2 = pga.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "DEF";
			constituentElement2.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_GenusName = "2";
			constituentElement2.US_SpeciesName = "2";
			constituentElement2.US_UnknownBreakdownCountryCode = "SG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";

			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
			var laceys = invoiceLine.LaceyActLines;
			var pga2 = laceys[0];

			Assert(pga2.PG04ConstituentElements.Count == 2);
			Assert(pga2.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "AW" && x.US_PGANameOfTheConstituentElement == "ABC") == 1);
			Assert(pga2.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "SG" && x.US_PGANameOfTheConstituentElement == "DEF") == 1);
		}

		public void TestIPGACommonMembers()
		{
			PGA.US_PGACommercialDescription = "commerc description";
			PGA.US_PGALineItemNumber = 2;
			PGA.US_PGALineValue = 15.50m;
			AssertEquals(16m, PGA.US_PGALineValue);

			ConstituentElement constituentElement = PGA.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "name1";
			constituentElement.US_PGAPercentOfConstituentElement = 7.56m;
			constituentElement.US_PGAQuantityOfConstituentElement = 65.55m;
			constituentElement.US_PGAUnitOfMeasure = "KG";

			AssertEquals(PGA.US_PGACommercialDescription, IPGACommon.CommercialDescription);
			AssertEquals(PGA.US_PGALineItemNumber, IPGACommon.PGALineItemNumber);
			AssertEquals(PGA.US_PGALineValue, IPGACommon.PGALineValue);

			var result = new List<IConstituentElement>(IPGACommon.ConstituentElements);

			AssertEquals(PGA.PG04ConstituentElements[0].US_PGANameOfTheConstituentElement, result[0].Name);
			AssertEquals(PGA.PG04ConstituentElements[0].US_PGAPercentOfConstituentElement, result[0].Percent);
			AssertEquals(PGA.PG04ConstituentElements[0].US_PGAQuantityOfConstituentElement, result[0].Quantity);
			AssertEquals(PGA.PG04ConstituentElements[0].US_PGAUnitOfMeasure, result[0].UnitOfMeasure);
		}

		public void TestNoValidationIsCalledOnNonCommittedElement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var pga = (PGA)((IBindingList)invoiceLine.LaceyActLines).AddNew();
			AssertNoRowMessageErrors(pga);
			var element = (ConstituentElement)((IBindingList)pga.PG04ConstituentElements).AddNew();
			AssertNoRowMessageErrors(pga);
			((ICancelAddNew)invoiceLine.LaceyActLines).CancelNew(0);
			AssertEquals(true, element.IsDeleted);
			pga = invoiceLine.LaceyActLines.AddNew();
			AssertNoRowMessageError(pga, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);
			element = pga.PG04ConstituentElements.AddNew();
			AssertNoRowMessageError(pga, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);
			element.Delete();
			AssertHasRowMessageError(pga, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);
			element = pga.PG04ConstituentElements.AddNew();
			AssertNoRowMessageError(pga, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			ICusAddInfoTypeSupporter supporter = PGA;
			supporter.AssertType(typeof(ConstituentElement), CusAddInfoTypeAttribute.Codes.USPGA);
			supporter.AssertType(null, "ZZ!");

			var element = PGA.PG04ConstituentElements.AddNew();
			element.US_PGANameOfTheConstituentElement = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(element.PK);
			AssertEquals(typeof(ConstituentElement), addInfo.GetType());

			var country = PGA.LaceyCountries.AddNew();
			country.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			newFactory = new BusinessObjectFactory();
			addInfo = newFactory.Load<CusAddInfo>(country.PK);
			AssertEquals(typeof(LaceyCountry), addInfo.GetType());
		}

		public void TestShouldSendPG15PG16Records()
		{
			PGA.PG04ConstituentElements.AddNew();
			AssertEquals(false, IPGACommon.ShouldSendPG15PG16Records);

			PGA.PG04ConstituentElements.AddNew();
			AssertEquals(true, IPGACommon.ShouldSendPG15PG16Records);
		}

		public void TestClone()
		{
			PGA.PG04ConstituentElements.AddNew();
			PGA.PG04ConstituentElements.AddNew();
			var licenses1 = PGA.Licenses.AddNew();
			licenses1.US_Type = "AB";
			var license2 = PGA.Licenses.AddNew();
			license2.US_Number = "12";
			var country1 = PGA.LaceyCountries.AddNew();
			country1.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			var country2 = PGA.LaceyCountries.AddNew();
			country2.US_CountryCode = Core.Constants.CountryCodes.Australia;

			PGA newPGA = (PGA)PGA.Clone();
			AssertEquals(2, newPGA.PG04ConstituentElements.Count);
			AssertEquals(2, newPGA.Licenses.Count);
			AssertEquals("Should be copied", "AB", newPGA.Licenses[0].US_Type);
			AssertEquals("Should be copied", "12", newPGA.Licenses[1].US_Number);
			newPGA.B7_ParentID = PGA.B7_ParentID;
			newPGA.B7_ParentTableCode = PGA.B7_ParentTableCode;

			var invoiceLine = PGA.InvoiceLine;

			var pga = invoiceLine.LaceyActLines.AddNew();
			pga.US_PGACommercialDescription = "COMDES";
			pga.US_InvCurrPGAValue = 5m;
			pga.US_UnknownBreakdown = ZBool.True;

			var element = pga.PG04ConstituentElements.AddNew();
			element.US_PGANameOfTheConstituentElement = "CONST";
			element.US_PGAQuantityOfConstituentElement = 5m;
			element.US_PGAUnitOfMeasure = "KG";
			element.US_PGAPercentOfConstituentElement = 12m;
			element.US_UnknownBreakdownCountryCode = "CO";
			element.US_SpecialUseDesignation = ZBool.False;
			element.US_GenusName = "GNS";
			element.US_SpeciesName = "SPC";

			var license = pga.Licenses.AddNew();
			license.US_TransType = "3";
			license.US_Type = "A11";
			license.US_Number = "A45";
			license.US_DateQualifier = "3";
			license.US_Date = new ZDateTime(2016, 4, 13);

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var pgaNew = invoiceLine.LaceyActLines.AddNew();

			AssertEquals("PGACommercialDescription must be equal", pga.US_PGACommercialDescription, pgaNew.US_PGACommercialDescription);
			AssertEquals("UnknownBreakdown must be equal", pga.US_UnknownBreakdown, pgaNew.US_UnknownBreakdown);

			AssertEquals("ConstituentElement: PGANameOfTheConstituentElement must be equal", pga.PG04ConstituentElements[0].US_PGANameOfTheConstituentElement, pgaNew.PG04ConstituentElements[0].US_PGANameOfTheConstituentElement);
			AssertEquals("ConstituentElement: PGAQuantityOfConstituentElement must be equal", pga.PG04ConstituentElements[0].US_PGAQuantityOfConstituentElement, pgaNew.PG04ConstituentElements[0].US_PGAQuantityOfConstituentElement);
			AssertEquals("ConstituentElement: PGAUnitOfMeasure must be equal", pga.PG04ConstituentElements[0].US_PGAUnitOfMeasure, pgaNew.PG04ConstituentElements[0].US_PGAUnitOfMeasure);
			AssertEquals("ConstituentElement: PGAPercentOfConstituentElement must be equal", pga.PG04ConstituentElements[0].US_PGAPercentOfConstituentElement, pgaNew.PG04ConstituentElements[0].US_PGAPercentOfConstituentElement);
			AssertEquals("ConstituentElement: UnknownBreakdownCountryCode must be equal", pga.PG04ConstituentElements[0].US_UnknownBreakdownCountryCode, pgaNew.PG04ConstituentElements[0].US_UnknownBreakdownCountryCode);
			AssertEquals("ConstituentElement: SpecialUseDesignation must be equal", pga.PG04ConstituentElements[0].US_SpecialUseDesignation, pgaNew.PG04ConstituentElements[0].US_SpecialUseDesignation);
			AssertEquals("ConstituentElement: GenusName must be equal", pga.PG04ConstituentElements[0].US_GenusName, pgaNew.PG04ConstituentElements[0].US_GenusName);
			AssertEquals("ConstituentElement: SpeciesName must be equal", pga.PG04ConstituentElements[0].US_SpeciesName, pgaNew.PG04ConstituentElements[0].US_SpeciesName);

			AssertEquals("License: TransType must be equal", pga.Licenses[0].US_TransType, pgaNew.Licenses[0].US_TransType);
			AssertEquals("License: Type must be equal", pga.Licenses[0].US_Type, pgaNew.Licenses[0].US_Type);
			AssertEquals("License: Number must be equal", pga.Licenses[0].US_Number, pgaNew.Licenses[0].US_Number);
			AssertEquals("License: DateQualifier must be equal", pga.Licenses[0].US_DateQualifier, pgaNew.Licenses[0].US_DateQualifier);
			AssertEquals("License: Date must be equal", pga.Licenses[0].US_Date, pgaNew.Licenses[0].US_Date);
		}

		public void TestDelete()
		{
			var element = PGA.PG04ConstituentElements.AddNew();
			var country = PGA.LaceyCountries.AddNew();
			PGA.Delete();

			Assert(element.IsDeleted);
			Assert(country.IsDeleted);
		}

		public void TestAddedOrDeletedElementChangedValidation()
		{
			PGA.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError("Should be notification - Constituent Element required for PGA", PGA, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);

			var constituentElement = PGA.PG04ConstituentElements.AddNew();
			AssertNoRowMessageError(PGA, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);

			PGA.PG04ConstituentElements[0].Delete();
			AssertHasRowMessageError("Should be notification - Constituent Element required for PGA", PGA, USPGAAddInfoValidation.AtLeastOneConstElementShouldExist);
		}

		public void TestPGAContainerPivotIsLoaded()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "SD@";
			refContainer.SetCountrySpecificContainerCode("20", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var invoiceLine = (JobComInvoiceLine)PGA.Parent;
			var declaration = invoiceLine.Declaration;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_RC = refContainer.PK;
			container1.CO_ContainerNumber = "CON32342";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_RC = refContainer.PK;
			container2.CO_ContainerNumber = "CON32346";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			CusContainerInvoiceLinePivot pivot = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].Pivot;
			var pgaContainersForInvoiceLine = PGA.ContainersForInvoiceLine;
			pgaContainersForInvoiceLine[0].IsForPGALine = true;
			pgaContainersForInvoiceLine[1].IsForPGALine = true;
			AssertEquals(2, PGA.ContainersForPGALine.Count);
			PGARelatedContainersGenPivot pgaPivot = PGA.ContainersForPGALine[0];
			AssertEquals(container1, pgaPivot.Container);
			AssertEquals(pivot, pgaPivot.Relation2Object);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var pga = newFactory.Load<PGA>(PGA.PK);
			pgaContainersForInvoiceLine = pga.ContainersForInvoiceLine;
			AssertEquals(2, pgaContainersForInvoiceLine.Count);
			AssertEquals(true, pgaContainersForInvoiceLine[0].IsForPGALine);
			AssertEquals(true, pgaContainersForInvoiceLine[1].IsForPGALine);

			//ACE Lacey Act
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			container1 = declaration.CusContainers.AddNew();
			container1.CO_RC = refContainer.PK;
			container1.CO_ContainerNumber = "CON32342";

			container2 = declaration.CusContainers.AddNew();
			container2.CO_RC = refContainer.PK;
			container2.CO_ContainerNumber = "CON32346";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			var aceLacey = invoiceLine.LaceyActLines.AddNew();
			AssertEquals(2, ((ILaceyActCommon)aceLacey).ContainerNumbers.Count());
		}

		public void TestPGALineValue()
		{
			Assert(PGA.US_PGALineValueInfo.ReadOnly);

			var invoiceLine = PGA.InvoiceLine;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine.InvoiceHeader.JZ_IncoTerm = "FOB";

			var declaration = invoiceLine.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			invoiceLine.JI_LinePrice = 1000m;
			PGA.US_InvCurrPGAValue = 1000m;
			AssertEquals("Not merged yet", "...", PGA.PGAValue);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merged now", "1000", PGA.PGAValue);
		}

		public void TestDefaultPGAContactPhoneNumber()
		{
			var ior = Factory.New<OrgHeader>();
			ior.OH_Code = "TESTIMP";
			DeclarationTestHelper.AddPGAContact(ior, "TEST", "IMPORTER", "11231231231231389789", "ABC@TEST.COM", null);

			PGA.InvoiceLine.Declaration.IOROrgPK = ior.PK;
			PGA.US_CertifyingIndividual = ZString.Empty;
			AssertNoExceptionThrown(delegate
			{
				PGA.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
				AssertEquals("TEST IMPORTER", PGA.US_PGAContactName);
				AssertEquals("112312312312313", PGA.US_PGAContactPhoneNo);
				AssertEquals("ABC@TEST.COM", PGA.US_PGAContactEmail);
			});
		}

		public void TestCertifySignatureDate()
		{
			PGA.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((ILaceyActCommon)PGA).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			PGA.InvoiceHeader.US_LACEYACTSignDate = expectedDate;
			AssertEquals(expectedDate, ((ILaceyActCommon)PGA).CertifySignatureDate);

			((ILaceyActCommon)PGA).CertifySignatureDate = new ZDate(2017, 01, 02);
			AssertEquals(new ZDate(2017, 01, 02), ((ILaceyActCommon)PGA).CertifySignatureDate);
		}

		public void TestDeclarationCertificate()
		{
			PGA.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Empty;
			AssertEquals("", ((ILaceyActCommon)PGA).DeclarationCertificate);

			PGA.InvoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;
			AssertEquals("Y", ((ILaceyActCommon)PGA).DeclarationCertificate);
		}

		#region Overrides

		protected override IEnumerable<PGA> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (PGA)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.LaceyActLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return PGA;
		}

		#endregion

		#region Implementation

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					if (declaration == null)
					{
						declaration = Factory.New<JobDeclaration>();
					}
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}
				return pga;
			}
		}
		PGA pga;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;

		ILaceyActCommon IPGACommon
		{
			get { return PGA; }
		}

		#endregion
	}
}
