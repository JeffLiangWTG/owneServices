using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using DeclarationForProductCreationHelper = Enterprise.Customs.Business.DeclarationForProductCreationHelper;
using ProductRelationDefaultOption = Enterprise.Customs.Business.ProductRelationDefaultOption;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	sealed class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		public void TestAddingNewPartExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			invoiceLine.US_DDTCITARExemptionNo = "EX1";
			invoiceLine.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.No;
			invoiceLine.US_DDTCRegistrationNo = "RG3234";
			invoiceLine.US_DDTCUSMLCategoryCode = "C3";
			invoiceLine.US_DDTCUnit = "U3";
			invoiceLine.US_ECCN = "EC32";
			invoiceLine.US_ExportCode = "E2";
			invoiceLine.US_LicenseNo = "LIC32";
			invoiceLine.US_LicenseType = "L3";
			invoiceLine.UnitPrice = 14.25m;

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			var part = collection.AddNew();
			var pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.SHB, "0121323122");
			AssertEquals("RG3234", pivot.CD_DDTCRegoNo);
			AssertEquals("U3", pivot.CD_DDTCUnit);
			AssertEquals("C3", pivot.CD_DDTCUSMLCategoryCode);
			AssertEquals("EC32", pivot.CD_ECCN);
			AssertEquals("E2", pivot.CD_ExportCode);
			AssertEquals("EX1", pivot.CD_ITARExemptionNo);
			AssertEquals("L3", pivot.CD_LicenceType);
			AssertEquals("Y", pivot.CD_MilitaryEquipInd);
			AssertEquals("LIC32", pivot.CD_LicenceNo);
			AssertEquals("D", pivot.CD_OriginIndicator);
			AssertEquals("N", pivot.CD_PartyCertInd);
			AssertEquals(ZDecimal.Zero, pivot.CD_PerUnitCost);
			AssertEquals(ZString.Empty, pivot.CD_RX_NKPerUnitCostCurr);

			declaration.US_TariffType = "HTS";
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			part = collection.AddNew();
			pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.HTE, "0121323122");
			AssertEquals("RG3234", pivot.CD_DDTCRegoNo);
			AssertEquals("U3", pivot.CD_DDTCUnit);
			AssertEquals("C3", pivot.CD_DDTCUSMLCategoryCode);
			AssertEquals("EC32", pivot.CD_ECCN);
			AssertEquals("E2", pivot.CD_ExportCode);
			AssertEquals("EX1", pivot.CD_ITARExemptionNo);
			AssertEquals("L3", pivot.CD_LicenceType);
			AssertEquals("Y", pivot.CD_MilitaryEquipInd);
			AssertEquals("LIC32", pivot.CD_LicenceNo);
			AssertEquals("D", pivot.CD_OriginIndicator);
			AssertEquals("N", pivot.CD_PartyCertInd);
			AssertEquals(ZDecimal.Zero, pivot.CD_PerUnitCost);
			AssertEquals(ZString.Empty, pivot.CD_RX_NKPerUnitCostCurr);
		}

		public void TestAdditionalLineDetailsIMP()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "S!@#123";
			supplier1.OH_FullName = "THE BUILDER";
			supplier1.OH_IsConsignor = true;

			var owner1 = Factory.New<OrgHeader>();
			owner1.OH_Code = "I!@#222";
			owner1.OH_FullName = "KJ THE DESTROYER";
			owner1.OH_IsConsignee = true;

			var classification1 = Factory.New<CusClassification>();
			classification1.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification1.CC_Description = "CUCKOO SQUEAKERS";
			classification1.CC_LookupCode = "CKSQKS";
			classification1.CC_TariffNum = "2010101010";

			var classification2 = Factory.New<CusClassification>();
			classification2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification2.CC_Description = "CUCKOO SQUEAKERS 2";
			classification2.CC_LookupCode = "CKSQKS2";
			classification2.CC_TariffNum = "3010101010";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_OH_Importer = owner1.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "NEWKJ1";
			line1.JI_Description = "KJ NEW TEST";
			line1.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Dozen;
			line1.US_SupTariff = "9800101010";
			line1.US_WoolLicenceNo = "LIC123";
			AssertEquals("line1.JI_CC", ZGuid.Empty, line1.JI_CC);

			var line2 = line1.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "3020102010";
			line2.JI_Description = "WEDNY TEST";
			line2.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			line2.US_SupTariff = "9910101010";
			line2.US_WoolLicenceNo = "LIC456";
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, line1, ZBool.False);
			var part = collection.AddNew();
			var pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.HTI, "", "9800101010");
			AssertEquals(1, pivot.Children.Count);
			var pivotChild = pivot.Children[0];
			AssertEquals("pivotChild.CI_ChildType", ClassificationChildTypeList.Codes.COMPONENT, pivotChild.CI_ChildType);
			AssertEquals("pivotChild.CI_CC", ZGuid.Empty, pivotChild.CI_CC);
			AssertEquals("pivotChild.CI_SupplementalTariff", "9910101010", pivotChild.CI_SupplementalTariff);
			AssertEquals("pivotChild.CI_TariffNum", "3020102010", pivotChild.CI_TariffNum);
			AssertEquals("pivotChild.CD_WoolLicenceNo", "LIC456", pivotChild.CD_WoolLicenceNo);
		}

		public override void TestAdditionalAddNewByOrgHeader()
		{
			Assert("It just test in Base class for create new product", true);
		}

		public override void TestAddingNewPart()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST NAME";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "0121323122";

			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.GetAddInfo().LoadPropertiesFromString("EntryType=01*UC_NKCountryOfOrigin=AU*BasisUnit=1*CBTPACertificateNo=CBCERT*HazMatDesc=AMYL ACETATES*TradeBrandName=BRAND NAME*WoolLicenceNo=WOOLICENC*ZoneStatus=N", false);
			invoiceLine.JI_OA_ManufacturerAddress = org.MainAddress.PK;
			invoiceLine.JI_PartAttrib1 = "AT1";
			invoiceLine.JI_PartAttrib2 = "AT2";
			invoiceLine.JI_PartAttrib3 = "AT3";
			invoiceLine.JI_SerialNumber = "SN";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Antarctica;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.BritishIndianOceanTerritory;
			invoiceLine.US_ADDCaseNo = "A123456";
			invoiceLine.US_ADD_NA = true;
			invoiceLine.US_ADDDepositValue = 100m;
			invoiceLine.US_IsBondedADD = false;
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_CVDCaseNo = "C123456";
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_CVDDepositValue = 120m;
			invoiceLine.US_IsBondedCVD = true;
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.JI_LinePrice = 500m;

			AddFDAToInvoiceLine(invoiceLine, "FDA1PC", 1m);
			AssertEquals("Precondition: value in inv. currency defaulted to FDA line", 500m, invoiceLine.FDAs[0].US_InvCurrFDAValue);

			AddFDAToInvoiceLine(invoiceLine, "FDA2PC", 2m);
			AddFCCToInvoiceLine(invoiceLine, "FCC1CD", "FCC1ID", 1.5m);
			AddFCCToInvoiceLine(invoiceLine, "FCC2CD", "FCC2ID", 10m);
			AddDOTToInvoiceLine(invoiceLine, "01", "E");
			AddDOTToInvoiceLine(invoiceLine, "03", "T");
			AddPGAFDAToInvoiceLine(invoiceLine, "FDA2PC", "ABC", "C", "SP", "CA");
			AddNHTSAToInvoiceLine(invoiceLine, NHTSAProgramCodeList.Codes.MVS, DepartmentOfTransportBoxNumberList.Codes._2A);
			AddNHTSAToInvoiceLine(invoiceLine, NHTSAProgramCodeList.Codes.REI, DepartmentOfTransportBoxNumberList.Codes._03);
			AddATFToInvoiceLine(invoiceLine, ATFCategoryCodeList.Codes.API, "FFLN1", "FFLE1", "FELN1", "PN1");
			AddATFToInvoiceLine(invoiceLine, ATFCategoryCodeList.Codes.AR, "FFLN2", "FFLE2", "FELN2", "PN2");
			AddPSTToInvoiceLine(invoiceLine, "IUC1", "PS3", "Brand1");
			AddPSTToInvoiceLine(invoiceLine, "IUC2", "PS3", "Brand2");
			AddOMCToInvoiceLine(invoiceLine, 100m);
			AddVNEToInvoiceLine(invoiceLine, "FT1", "VM1", "IC1", "2015");
			AddVNEToInvoiceLine(invoiceLine, "FT2", "VM2", "IC2", "2016");
			AddTTBToInvoiceLine(invoiceLine, "IRC1", "PMT1", TTBProgramCodeList.Codes.Beverage);
			AddTTBToInvoiceLine(invoiceLine, "IRC2", "PMT2", TTBProgramCodeList.Codes.Tobacco);
			AddCPSCToInvoiceLine(invoiceLine, "IUC3", "Brand3");
			AddDEAToInvoiceLine(invoiceLine, "CA", "1234567", "123456789", "DEA-35");
			AddAPHISToInvoiceLine(invoiceLine, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.LiveAnimals);
			AddAPHISToInvoiceLine(invoiceLine, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts);
			AddAMSToInvoiceLine(invoiceLine, "TEST AMS", "AMS");
			AddDDTCToInvoiceLine(invoiceLine, "DDTC61", "S61", "123.11B");
			AddHFCToInvoiceLine(invoiceLine, EntityRoleCodeList.Codes.Consignee, 50m, true, "001", 100m, "KG");
			AddFWSToInvoiceLine(invoiceLine, "PSC", "IDT", "HY");
			AddNMFSToInvoiceLine(invoiceLine, NMFSProgramCodeList.Codes._370, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Russia);
			AddNMFSToInvoiceLine(invoiceLine, NMFSProgramCodeList.Codes.COA, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Afghanistan);
			AddNMFSToInvoiceLine(invoiceLine, NMFSProgramCodeList.Codes.HMS, NMFS370DocumentIdentifierList.Codes.CaptainStatement, Core.Constants.CountryCodes.Croatia);
			AddNMFSToInvoiceLine(invoiceLine, NMFSProgramCodeList.Codes.AMR, NMFS370DocumentIdentifierList.Codes.ObserverStatement, Core.Constants.CountryCodes.Mozambique);

			var pga1 = invoiceLine.LaceyActLines.AddNew();
			pga1.US_PGACommercialDescription = "DESC1";
			pga1.US_InvCurrPGAValue = 10m;
			pga1.US_PGALineValue = 10m;

			AddElementToPGA(pga1, "EL1", "N1");
			AddScientificDataToElement(pga1.PG04ConstituentElements[0], "GN1", "A1");
			AddScientificDataToElement(pga1.PG04ConstituentElements[0], "GN2", "A2");

			AddElementToPGA(pga1, "EL2", "N2");
			AddScientificDataToElement(pga1.PG04ConstituentElements[1], "GN3", "A3");
			AddScientificDataToElement(pga1.PG04ConstituentElements[1], "GN4", "A4");

			var pga2 = invoiceLine.LaceyActLines.AddNew();
			pga2.US_PGACommercialDescription = "DESC2";
			pga2.US_InvCurrPGAValue = 12m;
			AddElementToPGA(pga2, "EL3", "N3");
			AddScientificDataToElement(pga2.PG04ConstituentElements[0], "GN5", "A5");

			var childLine1 = invoiceLine.AddSecondaryInvoiceLine();
			childLine1.JI_Tariff = "2010304050";
			childLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
			childLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			childLine1.JI_InvoiceQuantity = 10m;
			childLine1.JI_NetWeight = 13.6m;
			childLine1.JI_Weight = 15500m;
			childLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Austria;
			childLine1.US_ADD_NA = true;
			childLine1.US_ADDCaseNo = "A3003";
			childLine1.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			childLine1.US_ADDDepositValue = 45.3m;
			childLine1.US_IsBondedADD = true;
			childLine1.US_CVD_NA = true;
			childLine1.US_CVDCaseNo = "C12456";
			childLine1.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			childLine1.US_CVDDepositValue = 4m;
			childLine1.US_IsBondedCVD = true;

			AddFDAToInvoiceLine(childLine1, "FDA1CH", 1m);
			AddFCCToInvoiceLine(childLine1, "FCC1CH", "FCC1CH", 12.5m);
			AddDOTToInvoiceLine(childLine1, "03", "C");
			AddOMCToInvoiceLine(childLine1, 100m);
			AddNHTSAToInvoiceLine(childLine1, NHTSAProgramCodeList.Codes.REI, DepartmentOfTransportBoxNumberList.Codes._2B);
			AddATFToInvoiceLine(childLine1, ATFCategoryCodeList.Codes.API, "FFLN1", "FFLE1", "FELN1", "PN1");
			AddPSTToInvoiceLine(childLine1, "IUC3", "PT3", "Brand3");
			AddVNEToInvoiceLine(childLine1, "FT3", "VM3", "IC3", "2014");
			AddTTBToInvoiceLine(childLine1, "IRC3", "PMT3", TTBProgramCodeList.Codes.Wine);
			AddCPSCToInvoiceLine(childLine1, "IUC3", "Brand3");
			AddDEAToInvoiceLine(childLine1, "CA", "1234567", "123456789", "DEA-35");
			AddAPHISToInvoiceLine(childLine1, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts);
			AddAMSToInvoiceLine(childLine1, "CHILD AMS", "CHD");
			AddDDTCToInvoiceLine(childLine1, "DDTC62", "S62", "123.22B");
			AddFWSToInvoiceLine(childLine1, "CD1", "I1", "H1");
			AddHFCToInvoiceLine(childLine1, EntityRoleCodeList.Codes.CertifyingOfficial, 5m, false, "002", 1m, "HH");
			AddNMFSToInvoiceLine(childLine1, NMFSProgramCodeList.Codes._370, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.EastTimor);
			AddNMFSToInvoiceLine(childLine1, NMFSProgramCodeList.Codes.COA, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Albania);
			AddNMFSToInvoiceLine(childLine1, NMFSProgramCodeList.Codes.HMS, NMFS370DocumentIdentifierList.Codes.CaptainStatement, Core.Constants.CountryCodes.Dominica);
			AddNMFSToInvoiceLine(childLine1, NMFSProgramCodeList.Codes.AMR, NMFS370DocumentIdentifierList.Codes.ObserverStatement, Core.Constants.CountryCodes.SanMarino);

			var child1pga1 = childLine1.LaceyActLines.AddNew();
			child1pga1.US_PGACommercialDescription = "CHILD1";
			child1pga1.US_InvCurrPGAValue = 4m;
			AddElementToPGA(child1pga1, "CH1", "C1");
			AddScientificDataToElement(child1pga1.PG04ConstituentElements[0], "CH1", "C1");

			var childLine2 = invoiceLine.AddProductRelatedInvoiceLine();
			childLine2.JI_Tariff = "3020405010";
			childLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			childLine2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			childLine2.JI_InvoiceQuantity = 100m;
			childLine2.JI_NetWeight = 13600m;
			childLine2.JI_Weight = 14.5m;

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();

			var importPivot = part.PivotsForBinding.GetMatch(ClassificationTypeList.Codes.HTI, org.PK, ZGuid.Empty, true);
			AssertEquals(cusClass.PK, importPivot.CI_CC);
			AssertImportPivot(importPivot, org);
			AssertPivotAttributes(importPivot, invoiceLine);

			invoiceLine.JI_CC = ZGuid.Empty;
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			part = collection.AddNew();

			importPivot = part.PivotsForBinding.GetMatch(ClassificationTypeList.Codes.HTI, org.PK, ZGuid.Empty, true);
			AssertEquals(ZGuid.Empty, importPivot.CI_CC);
			AssertImportPivot(importPivot, org);
			AssertPivotAttributes(importPivot, invoiceLine);
		}

		public void TestTransformDataForLaceyAfterCopyFromInvoice()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST NAME";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_CC = cusClass.PK;

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var pGA = invoiceLine.LaceyActLines.AddNew();
			pGA.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;

			var constituentElement1 = pGA.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "T1";
			constituentElement1.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";

			var scientificData1 = constituentElement1.ScientificDataCollection.AddNew();
			scientificData1.US_PGAScientificGenusName = "123";
			scientificData1.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.Recycled;
			scientificData1.US_PGACountryCode = "MX";

			var constituentElement2 = pGA.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "T2";
			constituentElement2.US_PGAQuantityOfConstituentElement = 120m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			var scientificData2 = constituentElement2.ScientificDataCollection.AddNew();
			scientificData2.US_PGAScientificGenusName = "896";
			scientificData2.US_PGAScientificSpeciesName = PGALaceySpeciesNameCodeList.Codes.SPF;
			scientificData2.US_PGACountryCode = "AU";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var part = collection.AddNew();

			var importPivot = part.PivotsForBinding.GetImportMatch(org.PK, ZGuid.Empty);
			AssertEquals(importPivot.PGAs.Count, 1);
			var newPGA = importPivot.PGAs[0];
			AssertEquals(ZString.Empty, newPGA.US_TrackingStatus);
			AssertEquals("newPGA.PG04ConstituentElements.Count", 2, newPGA.PG04ConstituentElements.Count);
			AssertEquals(0, newPGA.PG04ConstituentElements[0].ScientificDataCollection.Count);
			AssertEquals(0, newPGA.PG04ConstituentElements[1].ScientificDataCollection.Count);
			AssertEquals(1, newPGA.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "MX" && x.US_GenusName == "123"));
			AssertEquals(1, newPGA.PG04ConstituentElements.Cast<ConstituentElement>().Count(x => x.US_UnknownBreakdownCountryCode == "AU" && x.US_GenusName == "896"));
		}

		public void TestActivateInactiveShouldMatchExactInvoiceLineDetail()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUP!@#123";
			supplier1.OH_FullName = "BOB THE BUILDER";
			supplier1.OH_IsConsignor = true;

			var owner1 = Factory.New<OrgHeader>();
			owner1.OH_Code = "IMP!@#123";
			owner1.OH_FullName = "WENDY THE DESTROYER";
			owner1.OH_IsConsignee = true;

			var classification1 = Factory.New<CusClassification>();
			classification1.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification1.CC_Description = "CUCKOO SQUEAKERS";
			classification1.CC_LookupCode = "CKSQKS";
			classification1.CC_TariffNum = "2010101010";

			var classification2 = Factory.New<CusClassification>();
			classification2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification2.CC_Description = "CUCKOO SQUEAKERS 2";
			classification2.CC_LookupCode = "CKSQKS2";
			classification2.CC_TariffNum = "3010101010";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST1";
			part.OP_Desc = "TEST PRODUCT";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Box;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "4010201020";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			part.RelatedOrganisations.AddOwner(owner1);
			part.RelatedOrganisations.AddSupplier(supplier1);
			part.OP_IsActive = false;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_OH_Importer = owner1.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "TEST1";
			line1.JI_CC = classification1.PK;
			line1.JI_Description = "BOB TEST";
			line1.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Dozen;
			line1.US_SupTariff = "9800101010";
			line1.US_WoolLicenceNo = "LIC123";
			line1.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = line1.FDAs.AddNew();
			fda.US_FDAProductCode = "AB3234";
			AssertEquals("line1.JI_OP", ZGuid.Empty, line1.JI_OP);

			var line2 = line1.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "3020102010";
			line2.JI_Description = "WEDNY TEST";
			line2.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			line2.US_SupTariff = "9910101010";
			line2.US_WoolLicenceNo = "LIC456";
			line2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var line2FDA = line2.FDAs.AddNew();
			line2FDA.US_FDAProductCode = "4379AD";
			Factory.Save();

			declaration.SaveNewProductsorActivateInactiveOnes(ProductRelationDefaultOption.None, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals("declaration.InvoiceLines.Count", 2, declaration.InvoiceLines.Count);
			AssertCollectionContains(line1, declaration.InvoiceLines);
			AssertCollectionContains(line2, declaration.InvoiceLines);
			AssertEquals("line1.IsDeleted", false, line1.IsDeleted);
			AssertEquals("line1.JI_PartNo", "TEST1", line1.JI_PartNo);
			AssertEquals("line1.JI_OP", part.PK, line1.JI_OP);
			AssertEquals("line1.JI_CC", classification1.PK, line1.JI_CC);
			AssertEquals("line1.JI_Tariff", "2010101010", line1.JI_Tariff);
			AssertEquals("line1.JI_Description", "BOB TEST", line1.JI_Description);
			AssertEquals("line1.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.Dozen, line1.JI_InvoiceUQ);
			AssertEquals("line1.US_SupTariff", "9800101010", line1.US_SupTariff);
			AssertEquals("line1.US_WoolLicenceNo", "LIC123", line1.US_WoolLicenceNo);
			AssertEquals("line1.FDAs.Count", 1, line1.FDAs.Count);
			AssertCollectionContains(fda, line1.FDAs);
			AssertEquals("fda.IsDeleted", false, fda.IsDeleted);
			AssertEquals("fda.US_FDAProductCode", "AB3234", fda.US_FDAProductCode);

			AssertEquals("line2.IsDeleted", false, line2.IsDeleted);
			AssertEquals("line2.JI_PartNo", "TEST1", line2.JI_PartNo);
			AssertEquals("line2.JI_OP", ZGuid.Empty, line2.JI_OP);
			AssertEquals("line2.JI_CC", ZGuid.Empty, line2.JI_CC);
			AssertEquals("line2.JI_Tariff", "3020102010", line2.JI_Tariff);
			AssertEquals("line2.JI_Description", "WEDNY TEST", line2.JI_Description);
			AssertEquals("line2.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.Number, line2.JI_InvoiceUQ);
			AssertEquals("line2.US_SupTariff", "9910101010", line2.US_SupTariff);
			AssertEquals("line2.US_WoolLicenceNo", "LIC456", line2.US_WoolLicenceNo);
			AssertEquals("line2.FDAs.Count", 1, line2.FDAs.Count);
			AssertCollectionContains(line2FDA, line2.FDAs);
			AssertEquals("line2FDA.IsDeleted", false, line2FDA.IsDeleted);
			AssertEquals("line2FDA.US_FDAProductCode", "4379AD", line2FDA.US_FDAProductCode);

			AssertEquals(true, part.OP_IsActive);
			AssertEquals(true, ((IBusinessObjectState)part).HasChangesNotIncludingChildren);
			AssertEquals("part.OP_Desc", "BOB TEST", part.OP_Desc);
			AssertEquals("part.OP_StockKeepingUnit", ABIUnitOfMeasureList.Codes.Dozen, part.OP_StockKeepingUnit);
			AssertEquals("part.RelatedOrganisations.Count", 1, part.RelatedOrganisations.Count);
			AssertNotNull("owner1", part.RelatedOrganisations.FindByOrganisationAndRelationship(owner1, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
			AssertEquals("part.PivotsForBinding.Count", 1, part.PivotsForBinding.Count);
			pivot = part.PivotsForBinding[0];
			AssertEquals("pivot.CI_ChildType", ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals("pivot.CI_CC", classification1.PK, pivot.CI_CC);
			AssertEquals("pivot.CI_TariffNum", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("pivot.CI_SupplementalTariff", "9800101010", pivot.CI_SupplementalTariff);
			AssertEquals("pivot.CD_WoolLicenceNo", "LIC123", pivot.CD_WoolLicenceNo);

			AssertEquals("pivot.Children.Count", 1, pivot.Children.Count);
			var pivotChild = pivot.Children[0];
			AssertEquals("pivotChild.CI_ChildType", ClassificationChildTypeList.Codes.COMPONENT, pivotChild.CI_ChildType);
			AssertEquals("pivotChild.CI_CC", ZGuid.Empty, pivotChild.CI_CC);
			AssertEquals("pivotChild.CI_SupplementalTariff", "9910101010", pivotChild.CI_SupplementalTariff);
			AssertEquals("pivotChild.CI_TariffNum", "3020102010", pivotChild.CI_TariffNum);
			AssertEquals("pivotChild.CD_WoolLicenceNo", "LIC456", pivotChild.CD_WoolLicenceNo);

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_TariffNum = "4010203010";
			part.OP_IsActive = false;
			invoiceHeader.JobComInvoiceLines.RemoveAndDeleteAll();

			line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_PartNo = "TEST1";
			line1.JI_CC = classification2.PK;
			line1.JI_Description = "BOB TEST 2";
			line1.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.DozenPairs;
			line1.US_SupTariff = "9820101010";
			line1.US_WoolLicenceNo = "LIC123A";
			line1.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			fda = line1.FDAs.AddNew();
			fda.US_FDAProductCode = "AB3234A";
			AssertEquals("line1.JI_OP", ZGuid.Empty, line1.JI_OP);

			line2 = line1.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "3030102010";
			line2.JI_Description = "WEDNY TEST 2";
			line2.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.NumberOfJewels;
			line2.US_SupTariff = "9920101010";
			line2.US_WoolLicenceNo = "LIC456A";
			line2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			line2FDA = line2.FDAs.AddNew();
			line2FDA.US_FDAProductCode = "439ADA";

			var line3 = line1.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "4030102010";
			line3.JI_Description = "JACK TEST 2";
			line3.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Milligram;
			line3.US_SupTariff = "9940101010";
			line3.US_WoolLicenceNo = "LIC789A";
			line3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var line3FDA = line3.FDAs.AddNew();
			line3FDA.US_FDAProductCode = "842ADA";

			var line4 = invoiceHeader.InvoiceLines.AddNew();
			line4.JI_PartNo = "TEST1";
			line4.JI_CC = classification1.PK;
			line4.JI_Description = "BOB TEST 3";
			line4.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Meters;
			line4.US_SupTariff = "9830101010";
			line4.US_WoolLicenceNo = "LIC123B";
			line4.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var line4FDA = line4.FDAs.AddNew();
			line4FDA.US_FDAProductCode = "AB3234B";
			AssertEquals("line4.JI_OP", ZGuid.Empty, line4.JI_OP);

			var line5 = line4.AddSecondaryInvoiceLine();
			line5.JI_Tariff = "3040102010";
			line5.JI_Description = "WEDNY TEST 3";
			line5.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.OuncesWeightAvdp;
			line5.US_SupTariff = "9930101010";
			line5.US_WoolLicenceNo = "LIC456B";
			line5.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var line5FDA = line5.FDAs.AddNew();
			line5FDA.US_FDAProductCode = "439ADB";

			declaration.SaveNewProductsorActivateInactiveOnes(ProductRelationDefaultOption.OptionForImporter, new DeclarationForProductCreationHelper(invoiceHeader));
			AssertEquals("declaration.InvoiceLines.Count", 6, declaration.InvoiceLines.Count);
			AssertCollectionContains(line1, declaration.InvoiceLines);
			AssertCollectionContains(line2, declaration.InvoiceLines);
			AssertCollectionContains(line3, declaration.InvoiceLines);
			AssertCollectionContains(line4, declaration.InvoiceLines);
			AssertCollectionNotContains(line5, declaration.InvoiceLines);
			AssertEquals("line1.IsDeleted", false, line1.IsDeleted);
			AssertEquals("line1.JI_PartNo", "TEST1", line1.JI_PartNo);
			var part2 = line1.Part;
			AssertNotEquals(part2, part);
			AssertEquals("line1.JI_OP", part2.PK, line1.JI_OP);
			AssertEquals("line1.JI_CC", classification2.PK, line1.JI_CC);
			AssertEquals("line1.JI_Tariff", "3010101010", line1.JI_Tariff);
			AssertEquals("line1.JI_Description", "BOB TEST 2", line1.JI_Description);
			AssertEquals("line1.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.DozenPairs, line1.JI_InvoiceUQ);
			AssertEquals("line1.US_SupTariff", "9820101010", line1.US_SupTariff);
			AssertEquals("line1.US_WoolLicenceNo", "LIC123A", line1.US_WoolLicenceNo);
			AssertEquals("line1.FDAs.Count", 1, line1.FDAs.Count);
			AssertCollectionContains(fda, line1.FDAs);
			AssertEquals("fda.IsDeleted", false, fda.IsDeleted);
			AssertEquals("fda.US_FDAProductCode", "AB3234A", fda.US_FDAProductCode);

			AssertEquals("line2.IsDeleted", false, line2.IsDeleted);
			AssertEquals("line2.JI_PartNo", "TEST1", line2.JI_PartNo);
			AssertEquals("line2.JI_OP", ZGuid.Empty, line2.JI_OP);
			AssertEquals("line2.JI_CC", ZGuid.Empty, line2.JI_CC);
			AssertEquals("line2.JI_Tariff", "3030102010", line2.JI_Tariff);
			AssertEquals("line2.JI_Description", "WEDNY TEST 2", line2.JI_Description);
			AssertEquals("line2.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.NumberOfJewels, line2.JI_InvoiceUQ);
			AssertEquals("line2.US_SupTariff", "9920101010", line2.US_SupTariff);
			AssertEquals("line2.US_WoolLicenceNo", "LIC456A", line2.US_WoolLicenceNo);
			AssertEquals("line2.FDAs.Count", 1, line2.FDAs.Count);
			AssertCollectionContains(line2FDA, line2.FDAs);
			AssertEquals("line2FDA.IsDeleted", false, line2FDA.IsDeleted);
			AssertEquals("line2FDA.US_FDAProductCode", "439ADA", line2FDA.US_FDAProductCode);

			AssertEquals("line3.IsDeleted", false, line3.IsDeleted);
			AssertEquals("line3.JI_PartNo", "TEST1", line3.JI_PartNo);
			AssertEquals("line3.JI_OP", ZGuid.Empty, line3.JI_OP);
			AssertEquals("line3.JI_CC", ZGuid.Empty, line3.JI_CC);
			AssertEquals("line3.JI_Tariff", "4030102010", line3.JI_Tariff);
			AssertEquals("line3.JI_Description", "JACK TEST 2", line3.JI_Description);
			AssertEquals("line3.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.Milligram, line3.JI_InvoiceUQ);
			AssertEquals("line3.US_SupTariff", "9940101010", line3.US_SupTariff);
			AssertEquals("line3.US_WoolLicenceNo", "LIC789A", line3.US_WoolLicenceNo);
			AssertEquals("line3.FDAs.Count", 1, line3.FDAs.Count);
			AssertCollectionContains(line3FDA, line3.FDAs);
			AssertEquals("line3FDA.IsDeleted", false, line3FDA.IsDeleted);
			AssertEquals("line3FDA.US_FDAProductCode", "842ADA", line3FDA.US_FDAProductCode);

			AssertEquals("line4.IsDeleted", false, line4.IsDeleted);
			AssertEquals("line4.JI_PartNo", "TEST1", line4.JI_PartNo);
			AssertEquals("line4.JI_OP", part2.PK, line4.JI_OP);
			AssertEquals("line4.JI_CC", classification2.PK, line4.JI_CC);
			AssertEquals("line4.JI_Tariff", "3010101010", line4.JI_Tariff);
			AssertEquals("line4.JI_Description", "BOB TEST 3", line4.JI_Description);
			AssertEquals("line4.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.DozenPairs, line4.JI_InvoiceUQ);
			AssertEquals("line4.US_SupTariff", "9820101010", line4.US_SupTariff);
			AssertEquals("line4.US_WoolLicenceNo", "LIC123A", line4.US_WoolLicenceNo);

			var line4Children = line4.ChildLines.ToArray();
			AssertEquals("line4Children.Length", 2, line4Children.Length);
			AssertEquals("line5.IsDeleted", true, line5.IsDeleted);
			line5 = line4Children[0];
			var line6 = line4Children[1];
			if (line6.JI_Tariff == "3030102010")
			{
				line5 = line4Children[1];
				line6 = line4Children[0];
			}
			AssertEquals("line5.JI_PartNo", "TEST1", line5.JI_PartNo);
			AssertEquals("line5.JI_OP", ZGuid.Empty, line5.JI_OP);
			AssertEquals("line5.JI_CC", ZGuid.Empty, line5.JI_CC);
			AssertEquals("line5.JI_Tariff", "3030102010", line5.JI_Tariff);
			AssertEquals("line5.JI_Description", "BOB TEST 3", line5.JI_Description);
			AssertEquals("line5.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.DozenPairs, line5.JI_InvoiceUQ);
			AssertEquals("line5.US_SupTariff", "9920101010", line5.US_SupTariff);
			AssertEquals("line5.US_WoolLicenceNo", "LIC456A", line5.US_WoolLicenceNo);
			AssertEquals("line5.FDAs.Count", 0, line5.FDAs.Count);
			AssertEquals("line5FDA.IsDeleted", true, line5FDA.IsDeleted);

			AssertEquals("line6.JI_PartNo", "TEST1", line6.JI_PartNo);
			AssertEquals("line6.JI_OP", ZGuid.Empty, line6.JI_OP);
			AssertEquals("line6.JI_CC", ZGuid.Empty, line6.JI_CC);
			AssertEquals("line6.JI_Tariff", "4030102010", line6.JI_Tariff);
			AssertEquals("line6.JI_Description", "BOB TEST 3", line6.JI_Description);
			AssertEquals("line6.JI_InvoiceUQ", ABIUnitOfMeasureList.Codes.DozenPairs, line6.JI_InvoiceUQ);
			AssertEquals("line6.US_SupTariff", "9940101010", line6.US_SupTariff);
			AssertEquals("line6.US_WoolLicenceNo", "LIC789A", line6.US_WoolLicenceNo);

			AssertEquals(false, part.OP_IsActive);
			AssertEquals("part.OP_Desc", "BOB TEST", part.OP_Desc);
			AssertEquals("part.OP_StockKeepingUnit", ABIUnitOfMeasureList.Codes.Dozen, part.OP_StockKeepingUnit);
			AssertEquals("part.RelatedOrganisations.Count", 1, part.RelatedOrganisations.Count);
			AssertNotNull("owner1", part.RelatedOrganisations.FindByOrganisationAndRelationship(owner1, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("part.PivotsForBinding.Count", 2, part.PivotsForBinding.Count);
			AssertEquals("pivot.IsDeleted", false, pivot.IsDeleted);
			AssertCollectionContains(pivot, part.PivotsForBinding);
			AssertEquals("pivot.CI_ChildType", ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			AssertEquals("pivot.CI_CC", classification1.PK, pivot.CI_CC);
			AssertEquals("pivot.CI_TariffNum", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("pivot.CI_SupplementalTariff", "9800101010", pivot.CI_SupplementalTariff);
			AssertEquals("pivot.CD_WoolLicenceNo", "LIC123", pivot.CD_WoolLicenceNo);

			AssertEquals("pivot.Children.Count", 1, pivot.Children.Count);
			AssertEquals("pivotChild.IsDeleted", false, pivotChild.IsDeleted);
			AssertCollectionContains(pivotChild, pivot.Children);
			AssertEquals("pivotChild.CI_ChildType", ClassificationChildTypeList.Codes.COMPONENT, pivotChild.CI_ChildType);
			AssertEquals("pivotChild.CI_CC", ZGuid.Empty, pivotChild.CI_CC);
			AssertEquals("pivotChild.CI_SupplementalTariff", "9910101010", pivotChild.CI_SupplementalTariff);
			AssertEquals("pivotChild.CI_TariffNum", "3020102010", pivotChild.CI_TariffNum);
			AssertEquals("pivotChild.CD_WoolLicenceNo", "LIC456", pivotChild.CD_WoolLicenceNo);

			AssertEquals("pivot2.IsDeleted", false, pivot2.IsDeleted);
			AssertCollectionContains(pivot2, part.PivotsForBinding);
			AssertEquals("pivot2.CI_ChildType", ClassificationTypeList.Codes.HTE, pivot2.CI_ChildType);
			AssertEquals("pivot2.CI_TariffNum", "4010203010", pivot2.CI_TariffNum);

			AssertEquals(true, part2.OP_IsActive);
			AssertEquals("part2.OP_Desc", "BOB TEST 2", part2.OP_Desc);
			AssertEquals("part2.OP_StockKeepingUnit", ABIUnitOfMeasureList.Codes.DozenPairs, part2.OP_StockKeepingUnit);
			AssertEquals("part2.RelatedOrganisations.Count", 1, part2.RelatedOrganisations.Count);
			AssertNotNull("owner1", part2.RelatedOrganisations.FindByOrganisationAndRelationship(owner1, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("part2.PivotsForBinding.Count", 1, part2.PivotsForBinding.Count);
			var part2Pivot = part2.PivotsForBinding[0];
			AssertEquals("part2Pivot.CI_ChildType", ClassificationTypeList.Codes.HTI, part2Pivot.CI_ChildType);
			AssertEquals("part2Pivot.CI_CC", classification2.PK, part2Pivot.CI_CC);
			AssertEquals("part2Pivot.CI_TariffNum", ZString.Empty, part2Pivot.CI_TariffNum);
			AssertEquals("part2Pivot.CI_SupplementalTariff", "9820101010", part2Pivot.CI_SupplementalTariff);
			AssertEquals("part2Pivot.CD_WoolLicenceNo", "LIC123A", part2Pivot.CD_WoolLicenceNo);

			AssertEquals("part2Pivot.Children.Count", 2, part2Pivot.Children.Count);
			var part2PivotChild1 = part2Pivot.Children[0];
			var part2PivotChild2 = part2Pivot.Children[1];
			if (part2PivotChild2.CI_TariffNum == "3030102010")
			{
				part2PivotChild1 = part2Pivot.Children[1];
				part2PivotChild2 = part2Pivot.Children[0];
			}
			AssertEquals("part2PivotChild1.CI_ChildType", ClassificationChildTypeList.Codes.COMPONENT, part2PivotChild1.CI_ChildType);
			AssertEquals("part2PivotChild1.CI_CC", ZGuid.Empty, part2PivotChild1.CI_CC);
			AssertEquals("part2PivotChild1.CI_SupplementalTariff", "9920101010", part2PivotChild1.CI_SupplementalTariff);
			AssertEquals("part2PivotChild1.CI_TariffNum", "3030102010", part2PivotChild1.CI_TariffNum);
			AssertEquals("part2PivotChild1.CD_WoolLicenceNo", "LIC456A", part2PivotChild1.CD_WoolLicenceNo);

			AssertEquals("part2PivotChild2.CI_ChildType", ClassificationChildTypeList.Codes.COMPONENT, part2PivotChild2.CI_ChildType);
			AssertEquals("part2PivotChild2.CI_CC", ZGuid.Empty, part2PivotChild2.CI_CC);
			AssertEquals("part2PivotChild2.CI_SupplementalTariff", "9940101010", part2PivotChild2.CI_SupplementalTariff);
			AssertEquals("part2PivotChild2.CI_TariffNum", "4030102010", part2PivotChild2.CI_TariffNum);
			AssertEquals("part2PivotChild2.CD_WoolLicenceNo", "LIC789A", part2PivotChild2.CD_WoolLicenceNo);
		}

		public void TestDataRefreshDoesNotDuplicateData()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST NAME";
			org.OH_Code = "CODE";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_LookupCode = "TestLookup";
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.JI_PartNo = "PART";
			invoiceLine.JI_Description = "LINE DESC";

			AddFDAToInvoiceLine(invoiceLine, "FDA1PC", 1m);
			AddFDAToInvoiceLine(invoiceLine, "FDA2PC", 2m);

			AddFCCToInvoiceLine(invoiceLine, "FCC1CD", "FCC1ID", 10m);
			AddFCCToInvoiceLine(invoiceLine, "FCC2CD", "FCC2ID", 12m);

			AddDOTToInvoiceLine(invoiceLine, "01", ClarificationCodeList.Codes.Equipment);
			AddDOTToInvoiceLine(invoiceLine, "03", ClarificationCodeList.Codes.Tire);
			Factory.Save();
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var part = collection.AddNew();
			part.OP_PartNum = invoiceLine.JI_PartNo;
			Factory.Save();
			var bestMatch = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);

			AssertEquals("still 2 FDAs", 2, invoiceLine.FDAs.Count);
			AssertEquals("still 2 FCCs", 2, invoiceLine.FCCs.Count);
			AssertEquals("still 2 DOTs", 2, invoiceLine.DOTs.Count);
			var factory2 = new BusinessObjectFactory();
			var part2 = factory2.Load<OrgSupplierPart>(part.PK);
			part2.OP_Desc = "XXX";
			factory2.Save();
			AssertEquals("still 2 FDAs", 2, invoiceLine.FDAs.Count);
			AssertEquals("still 2 FCCs", 2, invoiceLine.FCCs.Count);
			AssertEquals("still 2 DOTs", 2, invoiceLine.DOTs.Count);
		}

		public void TestCopyPGAFDADocAddressDetailsForImportProductFromInvoiceLine()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				var cusClass = Factory.New<CusClassification>();
				cusClass.CC_LookupCode = "TestLookup";
				cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				cusClass.CC_TariffNum = "111222";
				line.JI_CC = cusClass.PK;
				line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
				var fda = line.ACE_FDALines.AddNew();
				fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;

				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "ABC";
				org1.OH_FullName = "ABC INC.";
				org1.MainAddress.Address1 = "ABC AVENUE";
				fda.DocAddresses.AddNew(org1.MainAddress, DocAddressType.FSVPImporter);

				var org2 = fda.DocAddresses.AddNew(DocAddressType.Manufacturer);
				org2.E2_AddressOverride = true;
				org2.E2_CompanyName = "XYZ INC.";
				org2.E2_Address1 = "XYZ AVENUE";

				Factory.Save();

				var collection = new OrgSupplierPartCollection(Factory, line, false);
				var part = collection.AddNew();
				var pivot = part.PivotsForBinding[0];
				var pivotFda = pivot.ACEFDAs[0];
				AssertEquals(2, pivotFda.DocAddresses.Count);

				var pivotOrg1 = pivotFda.DocAddresses.FindByDocAddressType(DocAddressType.FSVPImporter);
				AssertEquals(org1.MainAddress.PK, pivotOrg1.E2_OA_Address);

				var pivotOrg2 = pivotFda.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
				AssertEquals(org2.AddressAsASingleLine, pivotOrg2.AddressAsASingleLine);
			}
		}

		public void TestCopyPGADetailsForImportProductFromInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_LookupCode = "TestLookup";
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "111222";
			invoiceLine1.JI_CC = cusClass.PK;
			invoiceLine1.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var cpscLine1 = invoiceLine1.CPSCHeaders.AddNew();
			cpscLine1.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			cpscLine1.US_ProductIDType = ProductIDTypeCodeList.Codes.AI;
			cpscLine1.US_ProductID = "AAA";
			cpscLine1.US_IntendedUseCode = "980.000";
			cpscLine1.US_IntendedUseDescription = "TEST AAA";
			cpscLine1.US_ProductName = "LIGHT BULB";
			var lot = cpscLine1.Lots.AddNew();
			lot.B7_AddInfoData = "LotNumber=12345*LotNumberType=1*StartDate=2024-01-01 00:00:00.000*EndDate=2024-05-01 00:00:00.000";
			var lab = cpscLine1.RuleAndLabs.AddNew();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			lab.US_OA_SafetyTestLocationAddress = org.MainAddress.PK;
			lab.US_CPSCAccreditedLabID = "11111";
			lab.US_RuleCodes = "BBBBB";
			lab.US_PreviousInspectionDate = ZDateTime.BrettsBirthday;
			var labReportInfo = lab.ReportAndLabs.AddNew();
			labReportInfo.US_RemarksText = "11111";
			labReportInfo.US_RemarksType = LabReportInformationTypeList.Codes.CP1;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = cusClass.PK;
			invoiceLine2.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
			var cpscLine2 = invoiceLine2.CPSCHeaders[0];
			cpscLine2.US_IntendedUseCode = "980.000";
			cpscLine2.US_IntendedUseDescription = "TEST BBB";
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine1, false);
			var part = collection.AddNew();
			var pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.HTI, "111222");
			AssertEquals("CPSC Indicator", OGAIndicatorList.Codes.Declared, pivot.CD_CPSCIndicator);
			AssertEquals("CPSC data should carry over", 1, pivot.CPSCLines.Count);
			var cpscPivot = pivot.CPSCLines[0];
			AssertEquals("ProcessingCode", CPSCProcessingCodeList.Codes.FGC, cpscPivot.US_ProcessingCode);
			AssertEquals("ProductIDType", ProductIDTypeCodeList.Codes.AI, cpscPivot.US_ProductIDType);
			AssertEquals("ProductID", "AAA", cpscPivot.US_ProductID);
			AssertEquals("IntendedUseCode", "980.000", cpscPivot.US_IntendedUseCode);
			AssertEquals("IntendedUseDescription", "TEST AAA", cpscPivot.US_IntendedUseDescription);
			AssertEquals("ProductName", "LIGHT BULB", cpscPivot.US_ProductName);
			AssertEquals("CPSC Lots data should NOT carry over", 0, cpscPivot.Lots.Count);
			AssertEquals("CPSC Labs and Rules data should carry over", 1, cpscPivot.RuleAndLabs.Count);
			var cpscLabsAndRules = cpscPivot.RuleAndLabs[0];
			AssertEquals("SafetyTestLocationAddress", org.MainAddress.PK, cpscLabsAndRules.US_OA_SafetyTestLocationAddress);
			AssertEquals("CPSCAccreditedLabID", "11111", lab.US_CPSCAccreditedLabID);
			AssertEquals("RuleCodes", "BBBBB", lab.US_RuleCodes);
			AssertEquals("PreviousInspectionDate", ZDateTime.BrettsBirthday, lab.US_PreviousInspectionDate);
			AssertEquals("Additional Lab Report Information should carry over", 1, cpscLabsAndRules.ReportAndLabs.Count);
			var report = cpscLabsAndRules.ReportAndLabs[0];
			AssertEquals("RemarksText", labReportInfo.US_RemarksText, report.US_RemarksText);
			AssertEquals("RemarksType", labReportInfo.US_RemarksType, report.US_RemarksType);

			collection = new OrgSupplierPartCollection(Factory, invoiceLine2, false);
			part = collection.AddNew();
			pivot = part.PivotsForBinding[0];
			AssertEquals("CPSC Indicator", OGAIndicatorList.Codes.Disclaimed, pivot.CD_CPSCIndicator);
			AssertEquals("CPSC Disclaim Reason", PGADisclaimReasonList.Codes.A, pivot.CD_CPSCDisclaimReason);
			AssertEquals("CPSC data should carry over", 1, pivot.CPSCLines.Count);
			cpscPivot = pivot.CPSCLines[0];
			AssertEquals("IntendedUseCode", "980.000", cpscPivot.US_IntendedUseCode);
			AssertEquals("IntendedUseDescription", "TEST BBB", cpscPivot.US_IntendedUseDescription);
		}

		public void TestCopyPGADetailsForExportProductFromInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_LookupCode = "TestLookup";
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ExportCertificateNo = "123456";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_EPAConsentNumber = "100900056";
			invoiceLine.US_HazWasteTrackingNo = "123456789ABC";
			invoiceLine.US_EPANetQty = 100m;
			invoiceLine.US_EPANetQtyUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			var invoiceNMFSHMS = invoiceLine.NMFSLines.AddNew();
			invoiceNMFSHMS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			invoiceNMFSHMS.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			invoiceNMFSHMS.US_VesselCountry = Core.Constants.CountryCodes.Canada;
			invoiceNMFSHMS.US_HarvestedCountry = "ZZ";
			invoiceNMFSHMS.US_GeographicLocation = OceanGeographicAreaCodeList.Codes.A;
			var invoiceNMFSAMR = invoiceLine.NMFSLines.AddNew();
			invoiceNMFSAMR.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			invoiceNMFSAMR.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			invoiceNMFSAMR.US_IFTPPermitNumber = "SE52103";
			invoiceNMFSAMR.US_Quantity = 1000m;
			invoiceNMFSAMR.US_UnitOfMeasure = Core.Constants.Weight.Kilograms;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportATF.US_Quantity = 3412m;
			invoiceLine.ExportATF.US_CategoryCode = ATFCategoryCodeList.Codes.AW;
			invoiceLine.ExportATF.US_FFLNumber = "TEST NUMBE";
			invoiceLine.ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._1;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var invoiceDEAHeader0 = invoiceLine.DEAHeaders.AddNew();
			invoiceDEAHeader0.US_DrugCode = "DRUG";
			invoiceDEAHeader0.US_Weight = 111.11m;
			invoiceDEAHeader0.US_UnitOfMeasure = Core.Constants.Weight.Grams;
			invoiceDEAHeader0.US_PermitNumber = "123456";
			var invoiceDEAHeader1 = invoiceLine.DEAHeaders.AddNew();
			invoiceDEAHeader1.US_DrugCode = "TEST";
			invoiceDEAHeader1.US_RegistrationNumber = "83741FL34";
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.ExportFWS.US_ConfirmationNum = "2015AA1234567";
			invoiceLine.ExportFWS.US_TaxonomicSerialNumber = "SN18383801234567890";
			invoiceLine.ExportFWS.US_PurposeCode = FWSPurposeCodeList.Codes.BiomedicalResearch;
			invoiceLine.ExportFWS.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			invoiceLine.ExportFWS.US_WildlifeSource = FWSWildlifeSourceList.Codes.R;
			invoiceLine.ExportFWS.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Amphibians;
			invoiceLine.ExportFWS.US_CertificationCode = FWSCertificationCodeList.Codes.CertificationOfNoWildlife;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			var invoiceTTB0 = invoiceLine.TTBLines.AddNew();
			invoiceTTB0.US_NumberForIRC = "DH-388-I9-974AI";
			invoiceTTB0.US_Date = new ZDate(2016, 12, 8);
			invoiceTTB0.US_SerialNumber = "09876543211";
			var invoiceTTB1 = invoiceLine.TTBLines.AddNew();
			invoiceTTB1.US_NumberForIRC = "DHA-38-974AI";
			invoiceTTB1.US_Date = new ZDate(2016, 12, 31);
			invoiceTTB1.US_SerialNumber = "09876543212";
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var part = collection.AddNew();
			var pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.SHB, "0121323122");
			CombineAssertions(delegate
			{
				AssertEquals("pivot.CD_AMSIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_AMSIndicator);
				AssertEquals("pivot.CD_ExportCertificateNo", "123456", pivot.CD_ExportCertificateNo);
				AssertEquals("pivot.CD_PSTIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_PSTIndicator);
				AssertEquals("pivot.CD_EPAConsentNumber", "100900056", pivot.CD_EPAConsentNumber);
				AssertEquals("pivot.CD_HazWasteTrackingNo", "123456789ABC", pivot.CD_HazWasteTrackingNo);
				AssertEquals("pivot.CD_EPANetQty is not visible on product", ZDecimal.Zero, pivot.CD_EPANetQty);
				AssertEquals("pivot.CD_EPANetQtyUQ is not visible on product", ZString.Empty, pivot.CD_EPANetQtyUQ);
				AssertEquals("pivot.CD_NMFSHMSIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_NMFSHMSIndicator);
				AssertEquals("pivot.NMFSLines.Count", 2, pivot.NMFSLines.Count);
				var productNMFSHMS = pivot.NMFSLines[0];
				AssertEquals("productNMFSHMS.US_ProgramType", NMFSProgramCodeList.Codes.HMS, productNMFSHMS.US_ProgramType);
				AssertEquals("productNMFSHMS.US_ProcessingType", NMFSProductCategoryCodeList.Codes.Dressed, productNMFSHMS.US_ProcessingType);
				AssertEquals("productNMFSHMS.US_VesselCountry", Core.Constants.CountryCodes.Canada, productNMFSHMS.US_VesselCountry);
				AssertEquals("productNMFSHMS.US_HarvestedCountry", "ZZ", productNMFSHMS.US_HarvestedCountry);
				AssertEquals("productNMFSHMS.US_GeographicLocation", OceanGeographicAreaCodeList.Codes.A, productNMFSHMS.US_GeographicLocation);
				var productNMFSAMR = pivot.NMFSLines[1];
				AssertEquals("productNMFSAMR.US_ProgramType", NMFSProgramCodeList.Codes.AMR, productNMFSAMR.US_ProgramType);
				AssertEquals("productNMFSAMR.US_DocumentType", NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument, productNMFSAMR.US_DocumentType);
				AssertEquals("productNMFSAMR.US_IFTPPermitNumber", "SE52103", productNMFSAMR.US_IFTPPermitNumber);
				AssertEquals("productNMFSAMR.US_Quantity is not visible on product", ZDecimal.Zero, productNMFSAMR.US_Quantity);
				AssertEquals("productNMFSAMR.US_UnitOfMeasure is not visible on product", ZString.Empty, productNMFSAMR.US_UnitOfMeasure);
				AssertEquals("pivot.CD_ATFIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_ATFIndicator);
				AssertEquals("pivot.ATFLines.Count", 1, pivot.ATFLines.Count);
				AssertEquals("pivot.ExportATF.US_Quantity is not visible on product", ZDecimal.Zero, pivot.ExportATF.US_Quantity);
				AssertEquals("pivot.ExportATF.US_CategoryCode", ATFCategoryCodeList.Codes.AW, pivot.ExportATF.US_CategoryCode);
				AssertEquals("pivot.ExportATF.US_FFLNumber", "TEST NUMBE", pivot.ExportATF.US_FFLNumber);
				AssertEquals("pivot.ExportATF.US_PermitExemptionCode", ExemptionCodesCodeList.Codes._1, pivot.ExportATF.US_PermitExemptionCode);
				AssertEquals("pivot.CD_DEAIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_DEAIndicator);
				AssertEquals("pivot.DEAHeaders.Count", 2, pivot.DEAHeaders.Count);
				var productDEAHeader0 = pivot.DEAHeaders[0];
				AssertEquals("productDEAHeader0.US_DrugCode", "DRUG", productDEAHeader0.US_DrugCode);
				AssertEquals("productDEAHeader0.US_Weight is not visible on product", ZDecimal.Zero, productDEAHeader0.US_Weight);
				AssertEquals("productDEAHeader0.US_UnitOfMeasure is not visible on product", ZString.Empty, productDEAHeader0.US_UnitOfMeasure);
				AssertEquals("productDEAHeader0.US_PermitNumber", "123456", productDEAHeader0.US_PermitNumber);
				var productDEAHeader1 = pivot.DEAHeaders[1];
				AssertEquals("productDEAHeader1.US_DrugCode", "TEST", productDEAHeader1.US_DrugCode);
				AssertEquals("productDEAHeader1.US_RegistrationNumber", "83741FL34", invoiceDEAHeader1.US_RegistrationNumber);
				AssertEquals("pivot.CD_FWSIndicator", OGAIndicatorList.Codes.Declared, pivot.CD_FWSIndicator);
				AssertEquals("pivot.FWSLines.Count", 1, pivot.FWSLines.Count);
				AssertEquals("pivot.ExportFWS.US_ConfirmationNum", "2015AA1234567", pivot.ExportFWS.US_ConfirmationNum);
				AssertEquals("pivot.ExportFWS.US_TaxonomicSerialNumber", "SN18383801234567890", pivot.ExportFWS.US_TaxonomicSerialNumber);
				AssertEquals("pivot.ExportFWS.US_PurposeCode", FWSPurposeCodeList.Codes.BiomedicalResearch, pivot.ExportFWS.US_PurposeCode);
				AssertEquals("pivot.ExportFWS.US_WildlifeDescriptionCode", FWSWildlifeDescriptionCodesList.Codes.BAL, pivot.ExportFWS.US_WildlifeDescriptionCode);
				AssertEquals("pivot.ExportFWS.US_WildlifeSource", FWSWildlifeSourceList.Codes.R, pivot.ExportFWS.US_WildlifeSource);
				AssertEquals("pivot.ExportFWS.US_WildlifeCategoryCode", FWSWildlifeCategoryCodesList.Codes.Amphibians, pivot.ExportFWS.US_WildlifeCategoryCode);
				AssertEquals("pivot.ExportFWS.US_CertificationCode", FWSCertificationCodeList.Codes.CertificationOfNoWildlife, pivot.ExportFWS.US_CertificationCode);
				AssertEquals("pivot.CD_TTBIndicator", OGAIndicatorList.Codes.Disclaimed, pivot.CD_TTBIndicator);
				AssertEquals("pivot.TTBLines.Count", 2, pivot.TTBLines.Count);
				var productTTB0 = pivot.TTBLines[0];
				AssertEquals("productTTB0.US_NumberForIRC", "DH-388-I9-974AI", productTTB0.US_NumberForIRC);
				AssertEquals("productTTB0.US_Date", new ZDate(2016, 12, 8), productTTB0.US_Date);
				AssertEquals("productTTB0.US_SerialNumber", "09876543211", productTTB0.US_SerialNumber);
				var productTTB1 = pivot.TTBLines[1];
				AssertEquals("productTTB1.US_NumberForIRC", "DHA-38-974AI", productTTB1.US_NumberForIRC);
				AssertEquals("productTTB1.US_Date", new ZDate(2016, 12, 31), productTTB1.US_Date);
				AssertEquals("productTTB1.US_SerialNumber", "09876543212", productTTB1.US_SerialNumber);
			});
		}

		public void TestCopyAdditionalTariffsForImportProductFromInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_FormattedTariff = "8101.00.0010";
			invoiceLine1.SupTariffFormatted = "9903.88.01";
			invoiceLine1.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine1.SupFormattedAdditionalTariff2 = "9903.88.03";
			invoiceLine1.SupFormattedAdditionalTariff3 = "9903.88.04";
			invoiceLine1.SupFormattedAdditionalTariff4 = "9903.88.05";
			invoiceLine1.SupFormattedAdditionalTariff5 = "9903.88.06";
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine1, false);
			var part = collection.AddNew();
			var pivot = part.PivotsForBinding[0];
			AssertPivot(pivot, ClassificationTypeList.Codes.HTI, "8101000010");
			AssertEquals("CI_TariffNum", "8101000010", pivot.CI_TariffNum);
			AssertEquals("CI_SupplementalTariff", "99038801", pivot.CI_SupplementalTariff);
			AssertEquals("CI_SupAdditionalTariff1", "99038802", pivot.CI_SupAdditionalTariff1);
			AssertEquals("CI_SupAdditionalTariff2", "99038803", pivot.CI_SupAdditionalTariff2);
			AssertEquals("CI_SupAdditionalTariff3", "99038804", pivot.CI_SupAdditionalTariff3);
			AssertEquals("CI_SupAdditionalTariff4", "99038805", pivot.CI_SupAdditionalTariff4);
			AssertEquals("CI_SupAdditionalTariff5", "99038806", pivot.CI_SupAdditionalTariff5);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgSupplierPartCollection(Factory);

		void AddFDAToInvoiceLine(JobComInvoiceLine invoiceLine, ZString productCode, ZDecimal qty)
		{
			var fda1 = invoiceLine.FDAs.AddNew();
			fda1.US_FDAProductCode = productCode;
			fda1.US_FDAQty1 = qty;
		}

		void AddPGAFDAToInvoiceLine(JobComInvoiceLine invoiceLine, ZString productCode, ZString affdata, ZString lotdata, ZString constituentElementData, ZString scientificDetailsdata)
		{
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			fda1.US_ProductCode = productCode;
			fda1.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			var affcode = fda1.AffirmationCodes.AddNew();
			affcode.CY_Code = affdata;

			var lot = fda1.Lots.AddNew();
			lot.US_DegreeType = lotdata;

			var constituentElements = fda1.ProductConstituentElements.AddNew();
			constituentElements.US_SpeciesName = constituentElementData;
		}

		void AddFCCToInvoiceLine(JobComInvoiceLine invoiceLine, ZString commercialDesc, ZString iD, ZDecimal qty)
		{
			var fcc1 = invoiceLine.FCCs.AddNew();
			fcc1.US_FCCCommercialDesc = commercialDesc.Left(fcc1.US_FCCCommercialDescInfo.MaxLength);
			fcc1.US_FCCID = iD;
			fcc1.US_FCCQty = qty;
		}

		void AddDOTToInvoiceLine(JobComInvoiceLine invoiceLine, ZString boxNo, ZString clarCode)
		{
			var dot1 = invoiceLine.DOTs.AddNew();
			dot1.US_DOTBoxNo = boxNo;
			dot1.US_DOTClarCode = clarCode;
		}

		void AddFWSToInvoiceLine(JobComInvoiceLine invoiceLine, ZString processingCode, ZString idendityType, ZString hybrid)
		{
			var fws1 = invoiceLine.FWSHeaders.AddNew();
			fws1.US_ProcessingCode = processingCode;
			fws1.US_Hybrid = hybrid;
		}

		void AddHFCToInvoiceLine(JobComInvoiceLine invoiceLine, ZString certifyingIndividual, ZDecimal netWeight, ZBool imageSent, ZString lpcoNumber, ZDecimal percentage, ZString nameOfActiveIngredient)
		{
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader.US_CertifyingIndividual = certifyingIndividual;
			hfcHeader.US_NetWeight = netWeight;
			hfcHeader.US_HFCImageSent = imageSent;
			var hfcDetail = hfcHeader.USHFCDetails.AddNew();
			hfcDetail.US_LPCONumber = lpcoNumber;
			hfcDetail.US_ActiveIngredientPercentage = percentage;
			hfcDetail.US_NameOfActiveIngredient = nameOfActiveIngredient;
		}

		void AddNMFSToInvoiceLine(JobComInvoiceLine invoiceLine, ZString programtype, ZString documentType, ZString countryCode)
		{
			var nmfsHMS = invoiceLine.NMFSLines.AddNew();
			nmfsHMS.US_ProgramType = programtype;
			nmfsHMS.US_DocumentType = documentType;
			nmfsHMS.HarvestingDetails.RemoveAndDeleteAll();

			var detail = nmfsHMS.HarvestingDetails.AddNew();
			detail.US_HarvestedCountry = countryCode;
		}

		void AddTTBToInvoiceLine(JobComInvoiceLine invoiceLine, ZString numberForIRC, ZString permitNumber, ZString programCode)
		{
			var ttb = invoiceLine.TTBLines.AddNew();
			ttb.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			ttb.US_NumberForIRC = numberForIRC;
			ttb.US_PermitNumber = permitNumber;
			ttb.US_ProgramCode = programCode;
		}

		void AddAMSToInvoiceLine(JobComInvoiceLine invoiceLine, ZString desc, ZString program)
		{
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_CommercialDescription = desc;
			ams.US_Program = program;

			var lines = ams.AMSLines.AddNew();
			lines.US_ProductNumber = "ABC";
			lines.US_CertNumber = "CertNum";
			lines.US_NetWeight = 5m;
			lines.US_NetWeightUQ = "KG";

			var lotCode = ams.LotCodes.AddNew();
			lotCode.CY_Code = "C";
			lotCode.CY_Data = "D";
		}

		void AddElementToPGA(PGA pga, ZString nameOfTheConstituentElement, ZString uQ)
		{
			var element = pga.PG04ConstituentElements.AddNew();
			element.US_PGANameOfTheConstituentElement = nameOfTheConstituentElement;
			element.US_PGAUnitOfMeasure = uQ;
		}

		void AddScientificDataToElement(ConstituentElement element, ZString scientificGenusName, ZString countryCode)
		{
			var data = element.ScientificDataCollection.AddNew();
			data.US_PGAScientificGenusName = scientificGenusName;
			data.US_PGACountryCode = countryCode;
		}

		void AddNHTSAToInvoiceLine(JobComInvoiceLine invoiceLine, ZString programCode, ZString boxNumber)
		{
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			nhtsa.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			nhtsa.US_NHTProgramCode = programCode;
			nhtsa.US_NHTBoxNumber = boxNumber;
			nhtsa.US_NHTElectronicImage = true;
		}

		void AddOMCToInvoiceLine(JobComInvoiceLine invoiceLine, ZDecimal netWeigth)
		{
			var omc = invoiceLine.OMCHeaders.AddNew();
			omc.US_NetWeight = netWeigth;
		}

		void AddATFToInvoiceLine(JobComInvoiceLine invoiceLine, ZString category, ZString fFLNumber, ZString fFLExempt, ZString fELNumber, ZString fELExempt)
		{
			var atf = invoiceLine.ATFLines.AddNew();
			atf.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			atf.US_CategoryCode = category;
			atf.US_FFLNumber = fFLNumber;
			atf.US_FFLExemptionCode = fFLExempt;
			atf.US_FELNumber = fELNumber;
			atf.US_FELExemptionCode = fELExempt;
		}

		void AddPSTToInvoiceLine(JobComInvoiceLine invoiceLine, ZString intendedUseCode, ZString productType, ZString brandName)
		{
			var pst = invoiceLine.PSTLines.AddNew();
			pst.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			pst.US_ProductType = productType;
			pst.US_IntendedUseCode = intendedUseCode;
			pst.US_BrandName = brandName;
		}

		void AddCPSCToInvoiceLine(JobComInvoiceLine invoiceLine, ZString intendedUseCode, ZString brandName)
		{
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_IntendedUseCode = intendedUseCode;
			cpsc.US_TradeBrandName = brandName;
		}

		void AddVNEToInvoiceLine(JobComInvoiceLine invoiceLine, ZString formType, ZString vehicleModel, ZString importCode, ZString modelYear)
		{
			var vne = invoiceLine.VehicleLines.AddNew();
			vne.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			vne.US_FormType = formType;
			vne.US_VehicleModel = vehicleModel;
			vne.US_ImportCode = importCode;
			vne.US_ModelYear = modelYear;
		}

		void AddDEAToInvoiceLine(JobComInvoiceLine invoiceLine, ZString countryOfShipment, ZString permitNumber, ZString registrant, ZString formID)
		{
			var dea = invoiceLine.DEAHeaders.AddNew();
			dea.US_CountryOfShipment = countryOfShipment;
			dea.US_PermitNumber = permitNumber;
			dea.US_RegistrationNumber = registrant;
			dea.US_FormID = formID;
		}

		void AddAPHISToInvoiceLine(JobComInvoiceLine invoiceLine, ZString programType, ZString categoryType)
		{
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = programType;
			aphisHeader.US_CategoryType = categoryType;
			var aphisInspection = aphisHeader.Inspections.AddNew();
			aphisInspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyScheduled;
			var aphisSource = aphisHeader.Sources.AddNew();
			aphisSource.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			var aphisRouting = aphisHeader.Routings.AddNew();
			aphisRouting.US_Type = RoutingTypeList.Codes.PlaceOfTransshipment;
		}

		void AddDDTCToInvoiceLine(JobComInvoiceLine invoiceLine, ZString regNumber, ZString licenseType, ZString exemptionCode)
		{
			invoiceLine.US_DDTCExemptionCode = exemptionCode;
			invoiceLine.US_DDTCLicenseType = licenseType;
			invoiceLine.US_DDTCRegistrationNo = regNumber;
		}

		void AssertImportPivot(CusClassPartPivot importPivot, OrgHeader org)
		{
			AssertPivot(importPivot, ClassificationTypeList.Codes.HTI, "0121323122");
			AssertEquals("ADD Case Number", "A123456", importPivot.CD_ADDCaseNo);
			AssertEquals("US_ADD_NA", true, importPivot.CD_ADDApplicable);
			AssertEquals("US_IsBondedADD", false, importPivot.CD_ADDBonded);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, importPivot.CD_ADDDepositRateInd);
			AssertEquals("US_CVDCaseNo", "C123456", importPivot.CD_CVDCaseNo);
			AssertEquals("US_CVD_NA", true, importPivot.CD_ADDApplicable);
			AssertEquals("US_IsBondedCVD", true, importPivot.CD_CVDBonded);
			AssertEquals("US_CVDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, importPivot.CD_CVDDepositRateInd);

			AssertEquals("2 NHTSA", 2, importPivot.NHTSALines.Count);
			var nhtsa1 = importPivot.NHTSALines.OfType<NHTSAHeader>().FirstOrDefault(x => x.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._2A);
			var nhtsa2 = importPivot.NHTSALines.OfType<NHTSAHeader>().FirstOrDefault(x => x.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._03);
			AssertNHTSA(nhtsa1, NHTSAProgramCodeList.Codes.MVS, DepartmentOfTransportBoxNumberList.Codes._2A);
			AssertNHTSA(nhtsa2, NHTSAProgramCodeList.Codes.REI, DepartmentOfTransportBoxNumberList.Codes._03);

			AssertEquals("2 TTB", 2, importPivot.TTBLines.Count);
			var ttb1 = importPivot.TTBLines.OfType<TTBLine>().FirstOrDefault(x => x.US_ProgramCode == TTBProgramCodeList.Codes.Beverage);
			var ttb2 = importPivot.TTBLines.OfType<TTBLine>().FirstOrDefault(x => x.US_ProgramCode == TTBProgramCodeList.Codes.Tobacco);
			AssertTTB(ttb1, "IRC1", "PMT1", TTBProgramCodeList.Codes.Beverage);
			AssertTTB(ttb2, "IRC2", "PMT2", TTBProgramCodeList.Codes.Tobacco);

			AssertEquals("1 FWS", 1, importPivot.FWSLines.Count);
			var fwsLine = importPivot.FWSLines.OfType<FWSHeader>().FirstOrDefault(x => x.US_ProcessingCode == "PSC");
			AssertFWS(fwsLine, "PSC", "IDT", "HY");

			var nmfs370 = importPivot.NMFS370Lines;
			AssertEquals("1 370", 1, nmfs370.Count());
			var line370 = nmfs370.FirstOrDefault();
			AssertNMFS(line370, NMFSProgramCodeList.Codes._370, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Russia);

			var nmfsCOA = importPivot.NMFSCOALines;
			AssertEquals("1 COA", 1, nmfsCOA.Count());
			var lineCOA = nmfsCOA.FirstOrDefault();
			AssertNMFS(lineCOA, NMFSProgramCodeList.Codes.COA, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Afghanistan);

			var nmfsAMR = importPivot.NMFSAMRLines;
			AssertEquals("1 AMR", 1, nmfsAMR.Count());
			var lineAMR = nmfsAMR.FirstOrDefault();
			AssertNMFS(lineAMR, NMFSProgramCodeList.Codes.AMR, NMFS370DocumentIdentifierList.Codes.ObserverStatement, Core.Constants.CountryCodes.Mozambique);

			var nmfsHMS = importPivot.NMFSHMSLines;
			AssertEquals("1 HMS", 1, nmfsHMS.Count());
			var lineHMS = nmfsHMS.FirstOrDefault();
			AssertNMFS(lineHMS, NMFSProgramCodeList.Codes.HMS, NMFS370DocumentIdentifierList.Codes.CaptainStatement, Core.Constants.CountryCodes.Croatia);

			AssertEquals("1 PST", 1, importPivot.OMCHeaders.Count);
			var omc1 = importPivot.OMCHeaders.OfType<OMCHeader>().FirstOrDefault(x => x.US_NetWeight == 100m);
			AssertOMC(omc1, 100m);

			AssertEquals("2 ATF", 2, importPivot.ATFLines.Count);
			var atf1 = importPivot.ATFLines.OfType<ATF>().FirstOrDefault(x => x.US_CategoryCode == ATFCategoryCodeList.Codes.API);
			var atf2 = importPivot.ATFLines.OfType<ATF>().FirstOrDefault(x => x.US_CategoryCode == ATFCategoryCodeList.Codes.AR);
			AssertATF(atf1, ATFCategoryCodeList.Codes.API, "FFLN1", "FFLE1", "FELN1", "PN1");
			AssertATF(atf2, ATFCategoryCodeList.Codes.AR, "FFLN2", "FFLE2", "FELN2", "PN2");

			AssertEquals("2 PST", 2, importPivot.PSTLines.Count);
			var pst1 = importPivot.PSTLines.OfType<Pesticide>().FirstOrDefault(x => x.US_IntendedUseCode == "IUC1");
			var pst2 = importPivot.PSTLines.OfType<Pesticide>().FirstOrDefault(x => x.US_IntendedUseCode == "IUC2");
			AssertPST(pst1, "IUC1", "PS3", "Brand1");
			AssertPST(pst2, "IUC2", "PS3", "Brand2");

			AssertEquals("1 PST", 1, importPivot.CPSCLines.Count);
			var cpsc1 = importPivot.CPSCLines.OfType<CPSCHeader>().FirstOrDefault(x => x.US_IntendedUseCode == "IUC3");
			AssertCPSC(cpsc1, "IUC3", "Brand3");

			AssertEquals("2 VNE", 2, importPivot.VehicleLines.Count);
			var vne1 = importPivot.VehicleLines.OfType<Vehicle>().FirstOrDefault(x => x.US_FormType == "FT1");
			var vne2 = importPivot.VehicleLines.OfType<Vehicle>().FirstOrDefault(x => x.US_FormType == "FT2");
			AssertVNE(vne1, "FT1", "VM1", "IC1", "2015");
			AssertVNE(vne2, "FT2", "VM2", "IC2", "2016");

			AssertEquals("2 PGAs", 2, importPivot.PGAs.Count);
			var pga1 = importPivot.PGAs.OfType<PGA>().FirstOrDefault(x => x.US_PGACommercialDescription == "DESC1");
			var pga2 = importPivot.PGAs.OfType<PGA>().FirstOrDefault(x => x.US_PGACommercialDescription == "DESC2");
			AssertPGA(pga1, "DESC1", "PGACommercialDescription=DESC1");
			AssertPGA(pga2, "DESC2", "PGACommercialDescription=DESC2");

			AssertEquals(2, pga1.PG04ConstituentElements.Count);
			var element1 = pga1.PG04ConstituentElements.OfType<ConstituentElement>().FirstOrDefault(x => x.US_PGANameOfTheConstituentElement == "EL1");
			var element2 = pga1.PG04ConstituentElements.OfType<ConstituentElement>().FirstOrDefault(x => x.US_PGANameOfTheConstituentElement == "EL2");
			AssertConstituentElement(element1, "EL1", "N1");
			AssertConstituentElement(element2, "EL2", "N2");

			AssertEquals(2, element1.ScientificDataCollection.Count);
			var data1 = element1.ScientificDataCollection.OfType<ScientificData>().FirstOrDefault(x => x.US_PGAScientificGenusName == "GN1");
			var data2 = element1.ScientificDataCollection.OfType<ScientificData>().FirstOrDefault(x => x.US_PGAScientificGenusName == "GN2");
			AssertScientificData(data1, "GN1", "A1");
			AssertScientificData(data2, "GN2", "A2");

			AssertEquals(2, element2.ScientificDataCollection.Count);
			var data3 = element2.ScientificDataCollection.OfType<ScientificData>().FirstOrDefault(x => x.US_PGAScientificGenusName == "GN3");
			var data4 = element2.ScientificDataCollection.OfType<ScientificData>().FirstOrDefault(x => x.US_PGAScientificGenusName == "GN4");
			AssertScientificData(data3, "GN3", "A3");
			AssertScientificData(data4, "GN4", "A4");

			AssertEquals(1, pga2.PG04ConstituentElements.Count);
			var element3 = pga2.PG04ConstituentElements[0];
			AssertConstituentElement(element3, "EL3", "N3");

			AssertEquals(1, element3.ScientificDataCollection.Count);
			data3 = element3.ScientificDataCollection[0];
			AssertScientificData(data3, "GN5", "A5");

			AssertEquals(2, importPivot.APHISHeaders.Count);
			var aphisHeader1 = importPivot.APHISHeaders.OfType<APHISHeader>().FirstOrDefault(x => x.US_CategoryType == APHISCategoryTypeCodeList.Codes.LiveAnimals);
			var aphisHeader2 = importPivot.APHISHeaders.OfType<APHISHeader>().FirstOrDefault(x => x.US_CategoryType == APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts);
			AssertAPHISHeader(aphisHeader1, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.LiveAnimals, 2, 1, 2, 1);
			AssertAPHISHeader(aphisHeader2, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts, 2, 1, 1, 1);

			AssertEquals(1, importPivot.HFCHeaders.Count);
			var hfc = importPivot.HFCHeaders.OfType<USHFCHeader>().FirstOrDefault();
			AssertHFC(hfc, EntityRoleCodeList.Codes.Consignee, 50m, false, "001", 100m, "KG");

			AssertEquals(1, importPivot.AMSLines.Count);
			var ams = importPivot.AMSLines.OfType<AMS>().FirstOrDefault();
			AssertAMS(ams);

			AssertEquals(2, importPivot.Children.Count);
			var importPivotChild1 = importPivot.Children.OfType<CusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT);
			var importPivotChild2 = importPivot.Children.OfType<CusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationChildTypeList.Codes.Related);
			AssertPivot(importPivotChild1, ClassificationChildTypeList.Codes.COMPONENT, "2010304050");
			AssertEquals("Country Of Export", Core.Constants.CountryCodes.Austria, importPivotChild1.CD_UC_NKCountryOfExport);
			AssertEquals("US_ADD_NA", true, importPivotChild1.CD_ADDApplicable);
			AssertEquals("US_ADDCaseNo", "A3003", importPivotChild1.CD_ADDCaseNo);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, importPivotChild1.CD_ADDDepositRateInd);
			AssertEquals("US_IsBondedADD", true, importPivotChild1.CD_ADDBonded);
			AssertEquals("US_CVD_NA", true, importPivotChild1.CD_CVDApplicable);
			AssertEquals("US_CVDCaseNo", "C12456", importPivotChild1.CD_CVDCaseNo);
			AssertEquals("US_ADDDepositRateIndicator", DepositRateIndicatorList.Codes.AdValorem, importPivotChild1.CD_CVDDepositRateInd);
			AssertEquals("US_IsBondedADD", true, importPivotChild1.CD_CVDBonded);

			AssertPivot(importPivotChild2, ClassificationChildTypeList.Codes.Related, "3020405010");
			AssertEquals(0.145m, importPivotChild2.CD_GrossWeight);
			AssertEquals(0.136m, importPivotChild2.CD_NetWeight);
			AssertEquals("USD", importPivotChild2.CD_RX_NK9802ValuePerUnitCurr);
			AssertEquals("KG", importPivotChild2.CD_WeightUQ);

			AssertEquals(1, importPivot.ACEFDAs.Count);
			AssertEquals(0, importPivotChild1.ACEFDAs.Count);
			var pgafda = importPivot.ACEFDAs[0];
			AssertPGAFDA(pgafda, "FDA2PC", "ABC", "SP", "CA");

			AssertEquals(1, importPivotChild1.NHTSALines.Count);
			AssertEquals(0, importPivotChild2.NHTSALines.Count);
			var child1NHTSA1 = importPivotChild1.NHTSALines[0];
			AssertNHTSA(child1NHTSA1, NHTSAProgramCodeList.Codes.REI, DepartmentOfTransportBoxNumberList.Codes._2B);

			AssertEquals(1, importPivotChild1.ATFLines.Count);
			AssertEquals(0, importPivotChild2.ATFLines.Count);
			var child1ATF1 = importPivotChild1.ATFLines[0];
			AssertATF(child1ATF1, ATFCategoryCodeList.Codes.API, "FFLN1", "FFLE1", "FELN1", "PN1");

			AssertEquals(1, importPivotChild1.PSTLines.Count);
			AssertEquals(0, importPivotChild2.PSTLines.Count);
			var child1PST1 = importPivotChild1.PSTLines[0];
			AssertPST(child1PST1, "IUC3", "PT3", "Brand3");

			AssertEquals(1, importPivotChild1.VehicleLines.Count);
			AssertEquals(0, importPivotChild2.VehicleLines.Count);
			var child1VNE1 = importPivotChild1.VehicleLines[0];
			AssertVNE(child1VNE1, "FT3", "VM3", "IC3", "2014");

			AssertEquals(1, importPivotChild1.TTBLines.Count);
			var child1TTB = importPivotChild1.TTBLines[0];
			AssertTTB(child1TTB, "IRC3", "PMT3", TTBProgramCodeList.Codes.Wine);

			AssertEquals(1, importPivotChild1.FWSLines.Count);
			var child1FWS = importPivotChild1.FWSLines[0];
			AssertFWS(child1FWS, "CD1", "I1", "H1");

			var nmfs370child = importPivotChild1.NMFS370Lines;
			AssertEquals("1 nmfs370child", 1, nmfs370child.Count());
			var line370childe = nmfs370child.FirstOrDefault();
			AssertNMFS(line370childe, NMFSProgramCodeList.Codes._370, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.EastTimor);

			var nmfsCOAchild = importPivotChild1.NMFSCOALines;
			AssertEquals("1 nmfsCOAchild", 1, nmfsCOAchild.Count());
			var lineCOAchilde = nmfsCOAchild.FirstOrDefault();
			AssertNMFS(lineCOAchilde, NMFSProgramCodeList.Codes.COA, NMFS370DocumentIdentifierList.Codes.NOAAForm370, Core.Constants.CountryCodes.Albania);

			var nmfsAMRchild = importPivotChild1.NMFSAMRLines;
			AssertEquals("1 nmfsAMRchild", 1, nmfsAMRchild.Count());
			var lineAMRchild = nmfsAMRchild.FirstOrDefault();
			AssertNMFS(lineAMRchild, NMFSProgramCodeList.Codes.AMR, NMFS370DocumentIdentifierList.Codes.ObserverStatement, Core.Constants.CountryCodes.SanMarino);

			var nmfsHMSchild = importPivotChild1.NMFSHMSLines;
			AssertEquals("1 nmfsHMSchild", 1, nmfsHMSchild.Count());
			var lineHMSchild = nmfsHMSchild.FirstOrDefault();
			AssertNMFS(lineHMSchild, NMFSProgramCodeList.Codes.HMS, NMFS370DocumentIdentifierList.Codes.CaptainStatement, Core.Constants.CountryCodes.Dominica);

			AssertEquals(1, importPivotChild1.PGAs.Count);
			AssertEquals(0, importPivotChild2.PGAs.Count);
			var child1pga1 = importPivotChild1.PGAs[0];
			AssertPGA(child1pga1, "CHILD1", "PGACommercialDescription=CHILD1");

			AssertEquals(1, child1pga1.PG04ConstituentElements.Count);
			var child1element1 = child1pga1.PG04ConstituentElements[0];
			AssertConstituentElement(child1element1, "CH1", "C1");

			AssertEquals(1, child1element1.ScientificDataCollection.Count);
			var child1data1 = child1element1.ScientificDataCollection[0];
			AssertScientificData(child1data1, "CH1", "C1");

			AssertEquals("1 DEA", 1, importPivotChild1.DEAHeaders.Count);
			var dea = importPivotChild1.DEAHeaders.OfType<DEAHeader>().FirstOrDefault();
			AssertDEA(dea, "CA", "1234567", "123456789", "DEA-35");

			AssertEquals(1, importPivotChild1.APHISHeaders.Count);
			AssertEquals(0, importPivotChild2.APHISHeaders.Count);
			var childAPHISHeader1 = importPivotChild1.APHISHeaders[0];
			AssertAPHISHeader(childAPHISHeader1, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts, 2, 1, 1, 1);

			AssertEquals("123.11B", importPivot.CD_ITARExemptionNo);
			AssertEquals("DDTC61", importPivot.CD_DDTCRegoNo);
			AssertEquals("S61", importPivot.Details.CD_DDTCLicenceType);

			AssertEquals("123.22B", importPivotChild1.CD_ITARExemptionNo);
			AssertEquals("DDTC62", importPivotChild1.CD_DDTCRegoNo);
			AssertEquals("S62", importPivotChild1.Details.CD_DDTCLicenceType);

			AssertEquals(1, importPivotChild1.HFCHeaders.Count);
			var childHFC = importPivotChild1.HFCHeaders.OfType<USHFCHeader>().FirstOrDefault();
			AssertHFC(childHFC, EntityRoleCodeList.Codes.CertifyingOfficial, 5m, false, "002", 1m, "HH");

			AssertEquals(1, importPivotChild1.AMSLines.Count);
			var childAMS = importPivotChild1.AMSLines.OfType<AMS>().FirstOrDefault();
			AssertAMS(childAMS);
		}

		void AssertPivotAttributes(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
		{
			if (!invoiceLine.JI_PartAttrib1.IsEmpty)
			{
				Assert(invoiceLine.JI_PartAttrib1, pivot.Attributes1.HasValue1(invoiceLine.JI_PartAttrib1));
			}

			if (!invoiceLine.JI_PartAttrib2.IsEmpty)
			{
				Assert(invoiceLine.JI_PartAttrib2, pivot.Attributes2.HasValue1(invoiceLine.JI_PartAttrib2));
			}

			if (!invoiceLine.JI_PartAttrib3.IsEmpty)
			{
				Assert(invoiceLine.JI_PartAttrib3, pivot.Attributes3.HasValue1(invoiceLine.JI_PartAttrib3));
			}
		}

		void AssertPivot(CusClassPartPivot pivot, ZString childType, ZString tariff, String provTariff = null)
		{
			AssertEquals("Child Type", childType, pivot.CI_ChildType);
			AssertEquals("Tariff Number", tariff, pivot.TariffNumber);
			if (!String.IsNullOrEmpty(provTariff))
			{
				AssertEquals("Tariff Number", provTariff, pivot.CI_SupplementalTariff);
			}
		}

		void AssertScientificData(ScientificData data, ZString scientificGenusName, ZString countryCode)
		{
			AssertEquals(scientificGenusName, data.US_PGAScientificGenusName);
			AssertEquals(countryCode, data.US_PGACountryCode);
		}

		void AssertOMC(OMCHeader omc, ZDecimal netWeight)
		{
			AssertNotNull(omc);
			AssertEquals(netWeight, omc.US_NetWeight);
		}

		void AssertConstituentElement(ConstituentElement element, ZString nameOfTheConstituentElement, ZString unitOfMeasure)
		{
			AssertEquals(nameOfTheConstituentElement, element.US_PGANameOfTheConstituentElement);
			AssertEquals(unitOfMeasure, element.US_PGAUnitOfMeasure);
		}

		void AssertPGA(PGA pga, ZString commercialDescription, ZString expectedAddInfo)
		{
			AssertEquals(ZString.Empty, pga.US_TrackingStatus);
			AssertEquals(commercialDescription, pga.US_PGACommercialDescription);
			AssertEquals("PGA line value should not be copied from invoice line", 0m, pga.US_InvCurrPGAValue);
			AssertEquals("PGA line value should not be copied from invoice line", 0m, pga.US_PGALineValue);
			AssertContains("AddInfo", expectedAddInfo, pga.B7_AddInfoData);
		}

		void AssertPGAFDA(ACEFDA newFDA, ZString productCode, ZString affdata, ZString constituentElementData, ZString scientificDetailsdata)
		{
			AssertEquals(ZString.Empty, newFDA.US_TrackingStatus);
			AssertEquals(newFDA.US_ProductCode, productCode);
			AssertEquals(newFDA.AffirmationCodes.Count, 1);
			AssertEquals(newFDA.AffirmationCodes[0].CY_Code, affdata);

			AssertEquals(newFDA.Lots.Count > 0, true);

			AssertEquals(newFDA.ProductConstituentElements.Count, 1);
			AssertEquals(newFDA.ProductConstituentElements[0].US_SpeciesName, constituentElementData);
		}

		void AssertNHTSA(NHTSAHeader nhtsa, ZString programCode, ZString boxNumber)
		{
			AssertNotNull(nhtsa);
			AssertEquals(ZString.Empty, nhtsa.US_TrackingStatus);
			AssertEquals(programCode, nhtsa.US_NHTProgramCode);
			AssertEquals(boxNumber, nhtsa.US_NHTBoxNumber);
			AssertEquals(false, nhtsa.US_NHTElectronicImage);
		}

		void AssertHFC(USHFCHeader hfcHeader, ZString certifyingIndividual, ZDecimal netWeight, ZBool imageSent, ZString lpcoNumber, ZDecimal percentage, ZString nameOfActiveIngredient)
		{
			AssertNotNull(hfcHeader);
			AssertEquals(certifyingIndividual, hfcHeader.US_CertifyingIndividual);
			AssertEquals(netWeight, hfcHeader.US_NetWeight);
			AssertEquals(imageSent, hfcHeader.US_HFCImageSent);
			var hfcDetail = (USHFCDetail)hfcHeader.USHFCDetails.FirstOrDefault();
			AssertNotNull(hfcDetail);
			AssertEquals(lpcoNumber, hfcDetail.US_LPCONumber);
			AssertEquals(percentage, hfcDetail.US_ActiveIngredientPercentage);
			AssertEquals(nameOfActiveIngredient, hfcDetail.US_NameOfActiveIngredient);
		}

		void AssertAMS(AMS amsHeader)
		{
			AssertNotNull(amsHeader);
			AssertEquals(1, amsHeader.AMSLines.Count);
			var amsLine = amsHeader.AMSLines[0];
			AssertEquals("ABC", amsLine.US_ProductNumber);
			AssertEquals("CertNum", amsLine.US_CertNumber);
			AssertEquals(5m, amsLine.US_NetWeight);
			AssertEquals("KG", amsLine.US_NetWeightUQ);
			AssertEquals(1, amsHeader.LotCodes.Count);
			var lotCode = amsHeader.LotCodes[0];
			AssertEquals("C", lotCode.CY_Code);
			AssertEquals("D", lotCode.CY_Data);
		}

		void AssertATF(ATF atf, ZString category, ZString fFLNumber, ZString fFLExempt, ZString fELNumber, ZString fELExempt)
		{
			AssertNotNull(atf);
			AssertEquals(ZString.Empty, atf.US_TrackingStatus);
			AssertEquals(category, atf.US_CategoryCode);
			AssertEquals(fFLNumber, atf.US_FFLNumber);
			AssertEquals(fFLExempt, atf.US_FFLExemptionCode);
			AssertEquals(fELNumber, atf.US_FELNumber);
			AssertEquals(fELExempt, atf.US_FELExemptionCode);
		}

		void AssertPST(Pesticide pst, ZString intendedUseCode, ZString productType, ZString brandName)
		{
			AssertNotNull(pst);
			AssertEquals(ZString.Empty, pst.US_TrackingStatus);
			AssertEquals(intendedUseCode, pst.US_IntendedUseCode);
			AssertEquals(productType, pst.US_ProductType);
			AssertEquals(brandName, pst.US_BrandName);
		}

		void AssertCPSC(CPSCHeader cpsc, ZString intendedUseCode, ZString brandName)
		{
			AssertNotNull(cpsc);
			AssertEquals(intendedUseCode, cpsc.US_IntendedUseCode);
			AssertEquals(brandName, cpsc.US_TradeBrandName);
		}

		void AssertVNE(Vehicle vne, ZString formType, ZString vehicleModel, ZString importCode, ZString modelYear)
		{
			AssertNotNull(vne);
			AssertEquals(ZString.Empty, vne.US_TrackingStatus);
			AssertEquals(formType, vne.US_FormType);
			AssertEquals(vehicleModel, vne.US_VehicleModel);
			AssertEquals(importCode, vne.US_ImportCode);
			AssertEquals(modelYear, vne.US_ModelYear);
		}

		void AssertFWS(FWSHeader fws, ZString processingCode, ZString identitytype, ZString hybrid)
		{
			AssertNotNull(fws);
			AssertEquals(processingCode, fws.US_ProcessingCode);
			AssertEquals(hybrid, fws.US_Hybrid);
		}

		void AssertNMFS(NMFSLine nmfs, ZString programType, ZString documentType, ZString harvestedCountry)
		{
			AssertNotNull(nmfs);
			AssertEquals(programType, nmfs.US_ProgramType);
			AssertEquals(documentType, nmfs.US_DocumentType);
			AssertEquals(harvestedCountry, nmfs.US_HarvestedCountry);
		}

		void AssertTTB(TTBLine ttb, ZString numberForIRC, ZString permitNumber, ZString programCode)
		{
			AssertNotNull(ttb);
			AssertEquals(ZString.Empty, ttb.US_TrackingStatus);
			AssertEquals(numberForIRC, ttb.US_NumberForIRC);
			AssertEquals(permitNumber, ttb.US_PermitNumber);
			AssertEquals(programCode, ttb.US_ProgramCode);
		}

		void AssertDEA(DEAHeader dea, ZString countryOfShipment, ZString permitNumber, ZString registrant, ZString formID)
		{
			AssertNotNull(dea);
			AssertEquals(countryOfShipment, dea.US_CountryOfShipment);
			AssertEquals("Permit Number not for product, so when copy data, we clear this data", ZString.Empty, dea.US_PermitNumber);
			AssertEquals(registrant, dea.US_RegistrationNumber);
			AssertEquals(formID, dea.US_FormID);
		}

		void AssertAPHISHeader(APHISHeader aphisHeader, ZString programType, ZString categoryType, int inspectionCount, int sourceCount, int routingCount, int documentCount)
		{
			AssertNotNull(aphisHeader);
			AssertEquals(programType, aphisHeader.US_ProgramType);
			AssertEquals(categoryType, aphisHeader.US_CategoryType);
			AssertEquals(inspectionCount, aphisHeader.Inspections.Count);
			AssertEquals(sourceCount, aphisHeader.Sources.Count);
			AssertEquals(routingCount, aphisHeader.Routings.Count);
		}
	}
}
