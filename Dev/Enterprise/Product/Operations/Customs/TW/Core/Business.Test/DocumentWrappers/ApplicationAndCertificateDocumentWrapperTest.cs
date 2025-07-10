using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ApplicationAndCertificateDocumentWrapper))]
	sealed class ApplicationAndCertificateDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return new ApplicationAndCertificateDocumentWrapper(entryHeader, "A125677888", "", 0, 0, null);
		}

		[ExpectNoExceptions]
		public void TestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var comRateType = helper.CreateCusRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS";
			var rateCodeCTA = helper.LoadOrCreateNewCusRateCode(Factory, "CTA", comRateType.PK);
			var rateCodeCTS = helper.LoadOrCreateNewCusRateCode(Factory, "CTS", comRateType.PK);
			var rateCodeTAT = helper.LoadOrCreateNewCusRateCode(Factory, "TAT", comRateType.PK);
			var rateCodeHWS = helper.LoadOrCreateNewCusRateCode(Factory, "HWS", comRateType.PK);
			Factory.Save();
			SetupOrganizations();
			CreateUNLOCOForTest("TW001", "TAI BEI", CountryCodes.Taiwan);
			CreateUNLOCOForTest("TW002", "GAO XIONG", CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "VSCD";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "VF";
			declaration.JE_VesselName = "Vessel";
			declaration.JE_ExportDate = new ZDateTime(2019, 7, 16);
			declaration.JE_DateOfArrival = new ZDateTime(2019, 8, 20);
			declaration.JE_RL_NKPortOfLoading = "TW001";
			declaration.JE_RL_NKPortOfArrival = "TW002";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = "CTA";
			charge.C1_ChargeAmount = 100;
			charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeType = "TAT";
			charge.C1_ChargeAmount = 200;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "CTA";
			lineFee.CF_ChargeAmount = 100M;
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "TAT";
			lineFee.CF_ChargeAmount = 200M;
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "AA";
			lineFee.CF_ChargeAmount = 300M;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = entryHeader.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			entryHeader.CH_Status = "AWO";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_OH_Importer = organization1.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine.JI_Group = "group";
			invoiceLine.JI_Description = "description";
			invoiceLine.JI_ModelYear = 2015;
			invoiceLine.JI_CarType = "A1";
			invoiceLine.JI_Transmission = "B";
			invoiceLine.JI_Gears = 2;
			invoiceLine.JI_NumberOfDoor = 5;
			invoiceLine.JI_BrandName = "brandname";
			invoiceLine.JI_Model = "model";
			invoiceLine.JI_Cylinders = 3;
			invoiceLine.JI_Displacement = "500";
			invoiceLine.JI_LHD = "Y";
			invoiceLine.JI_EngineType = "CG";
			invoiceLine.JI_Seats = 5;
			invoiceLine.JI_CarCondition = "1";
			invoiceLine.JI_Transmission = "A";
			var chassis1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			var chassis2 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis2.JG_ReferenceNumber = "1234567";
			var chassis3 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis3.JG_ReferenceNumber = "1234567";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_NetWeight = 2000m;
			invoiceLine1.JI_NetWeightUQ = "G";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceQuantity = 200m;
			invoiceLine1.JI_InvoiceUQ = "AAA";
			invoiceLine.JI_InvoiceUQ = "AAA";
			var cusSupporting1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			cusSupporting1.CSI_ReferenceNumber = "ref1";
			cusSupporting1.CSI_LineNo = 1;
			var wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "A125677888", "", 100, 200, null);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.UnitCommodityTax, NUnit.Framework.Is.EqualTo(200M).Using(CustomComparers.TypeComparison), "CommodityTax");
				NUnit.Framework.Assert.That(wrapper.ChassisNumber, NUnit.Framework.Is.EqualTo("A125677888").Using(CustomComparers.TypeComparison), "ChassisNumber");
				NUnit.Framework.Assert.That(wrapper.ImporterChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "ImporterChineseName");
				NUnit.Framework.Assert.That(wrapper.BorderTransportMeans, NUnit.Framework.Is.EqualTo("Vessel").Using(CustomComparers.TypeComparison), "BorderTransportMeans");
				NUnit.Framework.Assert.That(wrapper.GoodsShipmentExitDateTimeYear, NUnit.Framework.Is.EqualTo("108").Using(CustomComparers.TypeComparison), "GoodsShipmentExitDateTimeYear");
				NUnit.Framework.Assert.That(wrapper.GoodsShipmentExitDateTimeMonth, NUnit.Framework.Is.EqualTo("7").Using(CustomComparers.TypeComparison), "GoodsShipmentExitDateTimeMonth");
				NUnit.Framework.Assert.That(wrapper.GoodsShipmentExitDateTimeDay, NUnit.Framework.Is.EqualTo("16").Using(CustomComparers.TypeComparison), "GoodsShipmentExitDateTimeDay");
				NUnit.Framework.Assert.That(wrapper.BorderTransportMeansArrivalDateTimeYear, NUnit.Framework.Is.EqualTo("108").Using(CustomComparers.TypeComparison), "BorderTransportMeansArrivalDateTimeYear");
				NUnit.Framework.Assert.That(wrapper.BorderTransportMeansArrivalDateTimeMonth, NUnit.Framework.Is.EqualTo("8").Using(CustomComparers.TypeComparison), "BorderTransportMeansArrivalDateTimeMonth");
				NUnit.Framework.Assert.That(wrapper.BorderTransportMeansArrivalDateTimeDay, NUnit.Framework.Is.EqualTo("20").Using(CustomComparers.TypeComparison), "BorderTransportMeansArrivalDateTimeDay");
				NUnit.Framework.Assert.That(wrapper.NetWeightInKG, NUnit.Framework.Is.EqualTo(100M).Using(CustomComparers.TypeComparison), "NetWeightMeasure");
				NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("NO1").Using(CustomComparers.TypeComparison), "DeclarationID");
				NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo("NO////").Using(CustomComparers.TypeComparison), "DeclarationIDFormatted");
				cusNum1.CE_EntryNum = "AA  0812300002";
				NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AA  0812300002").Using(CustomComparers.TypeComparison), "DeclarationID");
				NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo("AA/  /08/123/00002").Using(CustomComparers.TypeComparison), "DeclarationIDFormatted");
				NUnit.Framework.Assert.That(wrapper.PortOfLoadingName, NUnit.Framework.Is.EqualTo("TAI BEI").Using(CustomComparers.TypeComparison), "PortOfLoadingName");
				NUnit.Framework.Assert.That(wrapper.PortOfArrivalName, NUnit.Framework.Is.EqualTo("GAO XIONG").Using(CustomComparers.TypeComparison), "PortOfArrivalName");
				NUnit.Framework.Assert.That(wrapper.FirstAdditionalDocumentID, NUnit.Framework.Is.EqualTo(ZString.Empty), "FirstAdditionalDocumentID");
				NUnit.Framework.Assert.That(wrapper.DocumentRemark, NUnit.Framework.Is.EqualTo("此欄空白").Using(CustomComparers.TypeComparison), "DocumentRemark");
			}

			);
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "A125677888", "", 100, 200, new List<string> { "REF-1" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.FirstAdditionalDocumentID, NUnit.Framework.Is.EqualTo("REF-1").Using(CustomComparers.TypeComparison), "FirstAdditionalDocumentID");
				NUnit.Framework.Assert.That(wrapper.DocumentRemark, NUnit.Framework.Is.EqualTo("此欄空白").Using(CustomComparers.TypeComparison), "DocumentRemark");
			}

			);
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "A125677888", "", 100, 200, new List<string> { "REF1-1", "REF2-2", "REF3-3" });
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.FirstAdditionalDocumentID, NUnit.Framework.Is.EqualTo("REF1-1").Using(CustomComparers.TypeComparison), "FirstAdditionalDocumentID");
				NUnit.Framework.Assert.That(wrapper.DocumentRemark, NUnit.Framework.Is.EqualTo(@"REF2-2
REF3-3").Using(CustomComparers.TypeComparison), "DocumentRemark");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCustomsName()
		{
			var mapping = new Dictionary<string, string> { ["A001"] = SharedHelper.GetCustomsOfficeName("A001"), ["B001"] = SharedHelper.GetCustomsOfficeName("B001"), ["C001"] = SharedHelper.GetCustomsOfficeName("C001"), ["D001"] = SharedHelper.GetCustomsOfficeName("D001") };
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = CountryCodes.Taiwan;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (var map in mapping)
			{
				var chassisNumber = map.Key;
				var customsName = map.Value;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				cusNum1.CE_EntryNum = chassisNumber;
				cusNum1.CE_ParentID = entryHeader.PK;
				var wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, chassisNumber, "", 0, 0, null);
				NUnit.Framework.Assert.That(wrapper.ChassisNumber, NUnit.Framework.Is.EqualTo(chassisNumber).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CustomsName, NUnit.Framework.Is.EqualTo(customsName).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var goodsDescription = @"YEAR: 2016 CAR TYPE: WAGON DOOR: 5
BRAND: VOLVO MODEL: XC60 D4
DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
LEFT SIDE STEERING: YES
TRANSMISSION: AUTO (A8) (WITH AUTO SHIFT LOCK)
ENGINE TYPE: DIESEL
STANDARD EQUIPMENT WITH EGR & CATALYST CONVERTER: YES
NON CFC REFRIGERANT SYSTEM (R134A)
CHASSIS NO: YV1DZA8BDG2911087";
			var wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "YV1DZA8BDG2911087", goodsDescription, 0, 0, null);
			NUnit.Framework.Assert.That(wrapper.Lines.Count, NUnit.Framework.Is.EqualTo(1));
			goodsDescription = @"YEAR: 2016 CAR TYPE:WAGON DOOR: 5
BRAND: VOLVO MODEL: XC60 D4
DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
LEFT SIDE STEERING: YES
TRANSMISSION: AUTO (A8)
(WITH AUTO SHIFT LOCK)
ENGINE TYPE: DIESEL
STANDARD EQUIPMENT WITH EGR &
CATALYST CONVERTER: YES
NON CFC REFRIGERANT SYSTEM (R134A)
CHASSIS NO: YV1DZA8BDG2911087";
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "YV1DZA8BDG2911087", goodsDescription, 0, 0, null);
			NUnit.Framework.Assert.That(wrapper.Lines.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line12.ToString(), NUnit.Framework.Does.Contain("以下空白"));
			goodsDescription = @"YEAR: 2016 CAR TYPE:
WAGON DOOR: 5
BRAND: VOLVO MODEL: XC60 D4
DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
LEFT SIDE STEERING: YES
TRANSMISSION: AUTO (A8)
(WITH AUTO SHIFT LOCK)
ENGINE TYPE: DIESEL
STANDARD EQUIPMENT WITH EGR &
CATALYST CONVERTER: YES
NON CFC REFRIGERANT SYSTEM (R134A)
CHASSIS NO: YV1DZA8BDG2911087";
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "YV1DZA8BDG2911087", goodsDescription, 0, 0, null);
			NUnit.Framework.Assert.That(wrapper.Lines.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line1, NUnit.Framework.Is.EqualTo("YEAR: 2016 CAR TYPE:").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line2, NUnit.Framework.Is.EqualTo("WAGON DOOR: 5").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line3, NUnit.Framework.Is.EqualTo("BRAND: VOLVO MODEL: XC60 D4").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line4, NUnit.Framework.Is.EqualTo("DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line5, NUnit.Framework.Is.EqualTo("LEFT SIDE STEERING: YES").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line6, NUnit.Framework.Is.EqualTo("TRANSMISSION: AUTO (A8)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line7, NUnit.Framework.Is.EqualTo("(WITH AUTO SHIFT LOCK)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line8, NUnit.Framework.Is.EqualTo("ENGINE TYPE: DIESEL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line9, NUnit.Framework.Is.EqualTo("STANDARD EQUIPMENT WITH EGR &").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line10, NUnit.Framework.Is.EqualTo("CATALYST CONVERTER: YES").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line11, NUnit.Framework.Is.EqualTo("NON CFC REFRIGERANT SYSTEM (R134A)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line12, NUnit.Framework.Is.EqualTo("CHASSIS NO: YV1DZA8BDG2911087").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number1, NUnit.Framework.Is.EqualTo("1)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number2, NUnit.Framework.Is.EqualTo("2)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number3, NUnit.Framework.Is.EqualTo("3)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number4, NUnit.Framework.Is.EqualTo("4)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number5, NUnit.Framework.Is.EqualTo("5)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number6, NUnit.Framework.Is.EqualTo("6)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number7, NUnit.Framework.Is.EqualTo("7)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number8, NUnit.Framework.Is.EqualTo("8)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number9, NUnit.Framework.Is.EqualTo("9)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number10, NUnit.Framework.Is.EqualTo("10)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number11, NUnit.Framework.Is.EqualTo("11)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number12, NUnit.Framework.Is.EqualTo("12)").Using(CustomComparers.TypeComparison));
			goodsDescription = @"1). YEAR: 2016 CAR TYPE:
WAGON DOOR: 5
BRAND: VOLVO MODEL: XC60 D4

DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5

LEFT SIDE STEERING: YES

TRANSMISSION: AUTO (A8)
(WITH AUTO SHIFT LOCK)
 ENGINE TYPE: DIESEL
STANDARD EQUIPMENT WITH EGR &
CATALYST CONVERTER: YES
NON CFC REFRIGERANT SYSTEM (R134A)
CHASSIS NO: YV1DZA8BDG2911087";
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "YV1DZA8BDG2911087", goodsDescription, 0, 0, null);
			NUnit.Framework.Assert.That(wrapper.Lines.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line12, NUnit.Framework.Is.EqualTo("CHASSIS NO: YV1DZA8BDG2911087").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number12, NUnit.Framework.Is.EqualTo("12)").Using(CustomComparers.TypeComparison));
			goodsDescription = @"YEAR: 2016 CAR TYPE:
WAGON DOOR: 5
BRAND: VOLVO MODEL: XC60 D4
DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
LEFT SIDE STEERING: YES
TRANSMISSION: AUTO (A8)
(WITH AUTO SHIFT LOCK)
ENGINE TYPE: DIESEL
STANDARD EQUIPMENT WITH EGR &
CATALYST CONVERTER: YES
NON CFC REFRIGERANT SYSTEM (R134A)
NON CFC REFRIGERANT SYSTEM (R134A)
NON CFC REFRIGERANT SYSTEM (R134A)
CHASSIS NO: YV1DZA8BDG2911087";
			wrapper = new ApplicationAndCertificateDocumentWrapper(entryHeader, "YV1DZA8BDG2911087", goodsDescription, 0, 0, null);
			NUnit.Framework.Assert.That(wrapper.Lines.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Line12, NUnit.Framework.Is.EqualTo("NON CFC REFRIGERANT SYSTEM (R134A)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[0].Number12, NUnit.Framework.Is.EqualTo("12)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[1].Line2, NUnit.Framework.Is.EqualTo("CHASSIS NO: YV1DZA8BDG2911087").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[1].Number2, NUnit.Framework.Is.EqualTo("14)").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(wrapper.Lines[1].Line3.ToString(), NUnit.Framework.Does.Contain("以下空白"));
			NUnit.Framework.Assert.That(wrapper.Lines[1].Number3, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		OrgHeader organization1;
		OrgHeader organization2;
		OrgCusCode vATCusCode;
		OrgCusCode cCPCusCode;
		OrgCusCode pASCusCode;
		OrgCusCode pIDCusCode;
		void SetupOrganizations()
		{
			organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE";
			address1.OA_Email = "EMAIL";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.TPC, "TPCREGNO", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "96944490";
			cCPCusCode = address1.CustomsCodes.AddNew();
			cCPCusCode.OK_RN_NKCodeCountry = "TW";
			cCPCusCode.OK_CodeType = "CCP";
			cCPCusCode.OK_CustomsRegNo = "987654321";
			pASCusCode = organization1.CustomsCodes.AddNew();
			pASCusCode.OK_RN_NKCodeCountry = "TW";
			pASCusCode.OK_CodeType = "PAS";
			pASCusCode.OK_CustomsRegNo = "PASREGNO";
			pIDCusCode = organization1.CustomsCodes.AddNew();
			pIDCusCode.OK_RN_NKCodeCountry = "TW";
			pIDCusCode.OK_CodeType = "PID";
			pIDCusCode.OK_CustomsRegNo = "PIDREGNO";
			organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "Org2";
			organization2.OH_RL_NKClosestPort = "TW";
		}

		RefUNLOCO CreateUNLOCOForTest(ZString uNLOCOCode, ZString name, ZString countryCode)
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = uNLOCOCode;
			var country = RefCountry.LoadFromCountryCode(Factory, countryCode);
			uNLOCO.RL_RN_NKCountryCode = country.Code;
			uNLOCO.RL_PortName = name;
			return uNLOCO;
		}

		[ExpectNoExceptions]
		public void TestCutString()
		{
			var str = @"XXXXXXXX X XX XXXX XX";
			var lines = ApplicationAndCertificateDocumentWrapper.CutString(str, 5);
			NUnit.Framework.Assert.That(lines[0], NUnit.Framework.Is.EqualTo("XXXXX"));
			NUnit.Framework.Assert.That(lines[1], NUnit.Framework.Is.EqualTo("XXX X"));
			NUnit.Framework.Assert.That(lines[2], NUnit.Framework.Is.EqualTo(" XX"));
			NUnit.Framework.Assert.That(lines[3], NUnit.Framework.Is.EqualTo("XXXX"));
			NUnit.Framework.Assert.That(lines[4], NUnit.Framework.Is.EqualTo("XX"));
		}
	}
}
