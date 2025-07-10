using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class SharedHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetVoyageFlightNo()
		{
			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions("Import and Air", () =>
			{
				decl.JE_TransportMode = TransportTypeList.Codes.Air;
				decl.JE_VoyageFlightNo = "CI 0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "JourneyID should be NIL");
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl, false), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "JourneyID should be empty");

				decl.JE_VoyageFlightNo = "CI0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "CI00080";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI00080").Using(CustomComparers.TypeComparison), "JourneyID should be CI00080");

				decl.JE_VoyageFlightNo = "CI123";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 123").Using(CustomComparers.TypeComparison), "JourneyID should be CI 123");
			});

			CombineAssertions("Import and Sea", () =>
			{
				decl.JE_TransportMode = TransportTypeList.Codes.Sea;
				decl.JE_VoyageFlightNo = "CI 0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "JourneyID should be NIL");
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl, false), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "JourneyID should be empty");

				decl.JE_VoyageFlightNo = "CI0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI0008");

				decl.JE_VoyageFlightNo = "CI00080";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI00080").Using(CustomComparers.TypeComparison), "JourneyID should be CI00080");

				decl.JE_VoyageFlightNo = "CI123";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI123").Using(CustomComparers.TypeComparison), "JourneyID should be CI123");
			});

			decl.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions("Export and Air", () =>
			{
				decl.JE_TransportMode = TransportTypeList.Codes.Air;
				decl.JE_VoyageFlightNo = "CI 0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "JourneyID should be empty");

				decl.JE_VoyageFlightNo = "CI0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "CI00080";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI00080").Using(CustomComparers.TypeComparison), "JourneyID should be CI00080");

				decl.JE_VoyageFlightNo = "CI123";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 123").Using(CustomComparers.TypeComparison), "JourneyID should be CI 123");
			});

			CombineAssertions("Export and Sea", () =>
			{
				decl.JE_TransportMode = TransportTypeList.Codes.Sea;
				decl.JE_VoyageFlightNo = "CI 0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI 0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI 0008");

				decl.JE_VoyageFlightNo = "";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "JourneyID should be empty");

				decl.JE_VoyageFlightNo = "CI0008";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI0008").Using(CustomComparers.TypeComparison), "JourneyID should be CI0008");

				decl.JE_VoyageFlightNo = "CI00080";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI00080").Using(CustomComparers.TypeComparison), "JourneyID should be CI00080");

				decl.JE_VoyageFlightNo = "CI123";
				NUnit.Framework.Assert.That(SharedHelper.GetVoyageFlightNo(decl), NUnit.Framework.Is.EqualTo("CI123").Using(CustomComparers.TypeComparison), "JourneyID should be CI123");
			});
		}

		[ExpectNoExceptions]
		public void TestGetTransportID()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VSCD";
			vessel.RV_LloydsNumber = "123456";
			vessel.RV_RadioCallSign = "654321";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "CI 0008";
			declaration.JE_VesselName = "VSCD";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(SharedHelper.GetTransportID(declaration), NUnit.Framework.Is.EqualTo("654321").Using(CustomComparers.TypeComparison), "RV_RadioCallSign");

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				NUnit.Framework.Assert.That(SharedHelper.GetTransportID(declaration), NUnit.Framework.Is.EqualTo(SharedHelper.GetVoyageFlightNo(declaration)), "JE_VoyageFlightNo");
			});
		}

		[ExpectNoExceptions]
		public void TestGetCustomsOfficeName()
		{
			var codeDescriptionPairList = new TaiwanCustomsDistrictList();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(SharedHelper.GetCustomsOfficeName("AA"), NUnit.Framework.Is.EqualTo(codeDescriptionPairList.GetDescriptionFromCode("A")).Using(CustomComparers.TypeComparison), "A-基隆關");
				NUnit.Framework.Assert.That(SharedHelper.GetCustomsOfficeName("BC"), NUnit.Framework.Is.EqualTo(codeDescriptionPairList.GetDescriptionFromCode("B")).Using(CustomComparers.TypeComparison), "B-高雄關");
				NUnit.Framework.Assert.That(SharedHelper.GetCustomsOfficeName("CF"), NUnit.Framework.Is.EqualTo(codeDescriptionPairList.GetDescriptionFromCode("C")).Using(CustomComparers.TypeComparison), "C-台北關");
				NUnit.Framework.Assert.That(SharedHelper.GetCustomsOfficeName("DE"), NUnit.Framework.Is.EqualTo(codeDescriptionPairList.GetDescriptionFromCode("D")).Using(CustomComparers.TypeComparison), "D-台中關");
				NUnit.Framework.Assert.That(SharedHelper.GetCustomsOfficeName(""), NUnit.Framework.Is.EqualTo(ZString.Empty), "Empty");
			});
		}

		[ExpectNoExceptions]
		public void TestGetFunctionalReferenceIDPlaceHolderWithPK()
		{
			var licensingHeaderPK = new ZGuid();
			NUnit.Framework.Assert.That(SharedHelper.GetFunctionalReferenceIDPlaceHolderWithPK(licensingHeaderPK), NUnit.Framework.Is.EqualTo($"<<FUNCTIONAL REFERENCE ID PLACE HOLDER {licensingHeaderPK}>>").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetFunctionalReferenceIDPlaceHolderWithPKHtml()
		{
			var licensingHeaderPK = new ZGuid();
			NUnit.Framework.Assert.That(SharedHelper.GetFunctionalReferenceIDPlaceHolderWithPKHtml(licensingHeaderPK), NUnit.Framework.Is.EqualTo($"&lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER {licensingHeaderPK}&gt;&gt;").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetIDStartWithNO()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(SharedHelper.GetIDStartWithNO("111", PartyIdentifierCodeList.Codes._53), NUnit.Framework.Is.EqualTo("NO111").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(SharedHelper.GetIDStartWithNO("222", PartyIdentifierCodeList.Codes._58), NUnit.Framework.Is.EqualTo("222").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(SharedHelper.GetIDStartWithNO("333", PartyIdentifierCodeList.Codes._174), NUnit.Framework.Is.EqualTo("333").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestGetSuffixLetters()
		{
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.Taiwan), NUnit.Framework.Is.EqualTo("TWAEO-").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.Singapore), NUnit.Framework.Is.EqualTo("AEOSG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.China), NUnit.Framework.Is.EqualTo("AEOCN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.Israel), NUnit.Framework.Is.EqualTo("ILAEO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.KoreaSouth), NUnit.Framework.Is.EqualTo("KRAEO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.Australia), NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.India), NUnit.Framework.Is.EqualTo("IN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetSuffixLetters(Core.Constants.CountryCodes.UnitedStates).ToString(), NUnit.Framework.Is.Null.Or.Empty, "Should be empty. - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		public void TestCharacteristicCode()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";
			var cusContainer = Factory.NewWithValidTestData<CusContainer>();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			cusContainer.CO_RC = refContainer.PK;
			var cusInContainer = Factory.NewWithValidTestData<CusInBondContainer>();
			cusInContainer.BC_ContainerNum = "UUUU1234567";
			cusInContainer.BC_RC = refContainer.PK;
			var twMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			twMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			twMap.RCM_RC_Container = refContainer.PK;
			twMap.RCM_Code = "TW1";
			refContainer.CodeMapCollection.Load();
			NUnit.Framework.Assert.That(cusContainer.GetCharacteristicCode(), NUnit.Framework.Is.EqualTo("TW1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusInContainer.GetCharacteristicCode(), NUnit.Framework.Is.EqualTo("TW1").Using(CustomComparers.TypeComparison));
			twMap.Delete();
			refContainer.CodeMapCollection.Load();
			NUnit.Framework.Assert.That(cusContainer.GetCharacteristicCode(), NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cusInContainer.GetCharacteristicCode(), NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUsedCapacityCode()
		{
			var cusContainer = Factory.NewWithValidTestData<CusContainer>();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Groupage;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = true;
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison));
			cusContainer.CO_IsPart = false;
			NUnit.Framework.Assert.That(cusContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
			var cusInBondContainer = Factory.NewWithValidTestData<CusInBondContainer>();
			cusInBondContainer.BC_Mode = Core.Constants.ContainerModes.Empty;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_IsPart = true;
			cusInBondContainer.BC_Mode = Core.Constants.ContainerModes.FCL;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_IsPart = false;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_Mode = Core.Constants.ContainerModes.Groupage;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_Mode = Core.Constants.ContainerModes.LCL;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_IsPart = true;
			cusInBondContainer.BC_Mode = Core.Constants.ContainerModes.BuyersConsol;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison));
			cusInBondContainer.BC_IsPart = false;
			NUnit.Framework.Assert.That(cusInBondContainer.GetUsedCapacityCode(), NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetSeals()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";
			var cusContainer = Factory.NewWithValidTestData<CusContainer>();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			cusContainer.CO_RC = refContainer.PK;
			cusContainer.CO_Seal = "XXX1";
			cusContainer.CO_SecondSeal = "XXX2";
			var seals = cusContainer.GetSeals().ToList();
			NUnit.Framework.Assert.That(seals.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(seals[0], NUnit.Framework.Is.EqualTo("XXX1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seals[1], NUnit.Framework.Is.EqualTo("XXX2").Using(CustomComparers.TypeComparison));
			cusContainer.CO_Seal = "XXX1";
			cusContainer.CO_SecondSeal = ZString.Empty;
			seals = cusContainer.GetSeals().ToList();
			NUnit.Framework.Assert.That(seals.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(seals[0], NUnit.Framework.Is.EqualTo("XXX1").Using(CustomComparers.TypeComparison));
			cusContainer.CO_Seal = ZString.Empty;
			cusContainer.CO_SecondSeal = "XXX2";
			seals = cusContainer.GetSeals().ToList();
			NUnit.Framework.Assert.That(seals.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(seals[0], NUnit.Framework.Is.EqualTo("XXX2").Using(CustomComparers.TypeComparison));
			cusContainer.CO_Seal = ZString.Empty;
			cusContainer.CO_SecondSeal = ZString.Empty;
			seals = cusContainer.GetSeals().ToList();
			NUnit.Framework.Assert.That(seals.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			ITransportContractDocument GetDocument(ZString id, ZString typeCode) => new TransportContractDocumentsForTest(id, typeCode);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "69517920011";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "741" && x.ID == "NIL"), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is True and JE_TransportMode is Air");
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "704" && x.ID == declaration.JE_MasterBill), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is False and JE_TransportMode is Sea");
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "741" && x.ID == "695-17920011"), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is False and JE_TransportMode is Air");
			declaration.JE_HouseBill = "HH111111";
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "703" && x.ID == declaration.JE_HouseBill), NUnit.Framework.Is.True);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "714" && x.ID == declaration.JE_HouseBill), NUnit.Framework.Is.True);
			var cnBill1 = declaration.Bills.AddNew();
			cnBill1.CU_BillType = BillTypeList.Codes.ContainerNote;
			cnBill1.CU_BillNum = "CN00001";
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "976" && x.ID == cnBill1.CU_BillNum), NUnit.Framework.Is.True);
			var cnBill2 = declaration.Bills.AddNew();
			cnBill2.CU_BillType = BillTypeList.Codes.ContainerNote;
			cnBill2.CU_BillNum = "CN00002";
			NUnit.Framework.Assert.That(declaration.GetTransportContractDocumentsWithMasterBillSegmentID(GetDocument).Any(x => x.TypeCode == "976" && x.ID == cnBill2.CU_BillNum), NUnit.Framework.Is.True);
			var transportContractDocuments = SharedHelper.GetTransportContractDocuments(GetDocument, true, "1", "2", new List<ZString> { "3", "4" });
			NUnit.Framework.Assert.That(transportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "741" && x.ID == "1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "703" && x.ID == "2"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == "3"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == "4"), NUnit.Framework.Is.True);
			transportContractDocuments = SharedHelper.GetTransportContractDocuments(GetDocument, false, "1", "2", new List<ZString> { "3", "4" });
			NUnit.Framework.Assert.That(transportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "704" && x.ID == "1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "714" && x.ID == "2"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == "3"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == "4"), NUnit.Framework.Is.True);
			transportContractDocuments = SharedHelper.GetTransportContractDocuments(GetDocument, false, "1", "2");
			NUnit.Framework.Assert.That(transportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.ID == "1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(transportContractDocuments.Any(x => x.ID == "2"), NUnit.Framework.Is.True);
			transportContractDocuments = SharedHelper.GetTransportContractDocuments(GetDocument, false, "1", "");
			NUnit.Framework.Assert.That(transportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(1));
			transportContractDocuments = SharedHelper.GetTransportContractDocuments(GetDocument, false, "", "");
			NUnit.Framework.Assert.That(transportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		public void TestGetTransportContractDocumentsFromMovementBill()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.MovementHeader.BM_ExportLadenOn = "Test Export Laden On";
			var movementBill = header.MovementBill;
			movementBill.B0_MasterBillNumber = "69517920011";
			movementBill.B0_HouseBillNumber = "H000001";
			var transportContractDocumentsByMovementBill = header.GetTransportContractDocuments(movementBill, (id, typeCode) => new TransportContractDocumentsForTest(id, typeCode));
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(new[] { "69517920011", "H000001" }, transportContractDocumentsByMovementBill.Select(x => x.ID));
				AssertContainsExactElementsInExactOrder(new[] { "704", "714" }, transportContractDocumentsByMovementBill.Select(x => x.TypeCode));
			});

			header.MovementHeader.BM_ExportLadenOn = "";
			movementBill = header.MovementBill;
			movementBill.B0_MasterBillNumber = "69517920011";
			movementBill.B0_HouseBillNumber = "H000001";
			transportContractDocumentsByMovementBill = header.GetTransportContractDocuments(movementBill, (id, typeCode) => new TransportContractDocumentsForTest(id, typeCode));
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(new[] { "695-17920011", "H000001" }, transportContractDocumentsByMovementBill.Select(x => x.ID));
				AssertContainsExactElementsInExactOrder(new[] { "741", "703" }, transportContractDocumentsByMovementBill.Select(x => x.TypeCode));
			});
		}

		public void TestGetTransportContractDocumentsFromArrivalBill()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			var arrivalBill = header.ArrivalBill;
			arrivalBill.B0_MasterBillNumber = "70628031122";
			arrivalBill.B0_HouseBillNumber = "I111112";
			var transportContractDocumentsByArrivalBill = header.GetTransportContractDocuments(arrivalBill, (id, typeCode) => new TransportContractDocumentsForTest(id, typeCode));
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(new[] { "70628031122", "I111112" }, transportContractDocumentsByArrivalBill.Select(x => x.ID));
				AssertContainsExactElementsInExactOrder(new[] { "704", "714" }, transportContractDocumentsByArrivalBill.Select(x => x.TypeCode));
			});

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			arrivalBill = header.ArrivalBill;
			arrivalBill.B0_MasterBillNumber = "70628031122";
			arrivalBill.B0_HouseBillNumber = "I111112";
			transportContractDocumentsByArrivalBill = header.GetTransportContractDocuments(arrivalBill, (id, typeCode) => new TransportContractDocumentsForTest(id, typeCode));
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(new[] { "706-28031122", "I111112" }, transportContractDocumentsByArrivalBill.Select(x => x.ID));
				AssertContainsExactElementsInExactOrder(new[] { "741", "703" }, transportContractDocumentsByArrivalBill.Select(x => x.TypeCode));
			});
		}

		[ExpectNoExceptions]
		public void TestGetClassifications()
		{
			IClassification GetClassification(ZString hazMatCode, ZString idTypeCode) => new ClassificationForTest(hazMatCode, idTypeCode);
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87123456";
			invoiceLine.JI_HazMatCode = "";
			var classifications = entryLine.GetClassifications(GetClassification, false);
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(1), "Commodity.Classifications.Cound() should be ");
			NUnit.Framework.Assert.That(classifications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("87123456").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].ID should be ");
			NUnit.Framework.Assert.That(classifications.ElementAt(0).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("HS").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].IdentificationTypeCode should be ");
			invoiceLine.JI_HazMatCode = "1100";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			classifications = entryLine.GetClassifications(GetClassification, false);
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(2), "Commodity.Classifications.Cound() should be ");
			NUnit.Framework.Assert.That(classifications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("1100").Using(CustomComparers.TypeComparison), "Commodity.Classifications[1].ID should be ");
			NUnit.Framework.Assert.That(classifications.ElementAt(1).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "Commodity.Classifications[1].IdentificationTypeCode should be ");
			invoiceLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1100", "", "IMO").First().PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			classifications = entryLine.GetClassifications(GetClassification, false);
			NUnit.Framework.Assert.That(classifications.ElementAt(1).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("SSO").Using(CustomComparers.TypeComparison), "Commodity.Classifications[1].IdentificationTypeCode should be ");
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Load();
			classifications = entryLine.GetClassifications(GetClassification, false);
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == ""), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "1100"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "87123456"), NUnit.Framework.Is.True);
			invoiceLine1.JI_Tariff = "87123456";
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "1100"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "87123456"), NUnit.Framework.Is.True);
			invoiceLine1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1110", "", "IMO").First().PK;
			classifications = entryLine.GetClassifications(GetClassification, false);
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(!classifications.Any(x => x.ID == "1110"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "1100"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "87123456"), NUnit.Framework.Is.True);
			classifications = entryLine.GetClassifications(GetClassification, true);
			NUnit.Framework.Assert.That(classifications.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "1100"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(classifications.Any(x => x.ID == "87123456"), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestCusEntryHeaderGetDutyOtherTaxFees()
		{
			CreateNewCusRateCode();
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var charge = header.Charges.AddNew();
			charge.C1_ChargeType = "DTA";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "DTS";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "CTA";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "CTS";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "TAT";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "HWS";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "XX0";
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = "XX1";
			var dutyOtherTaxFees = header.GetDutyOtherTaxFees();
			NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(6));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "DTA"), NUnit.Framework.Is.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "DTS"), NUnit.Framework.Is.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "CTA"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "CTS"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "TAT"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "HWS"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "XX0"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.C1_ChargeType == "XX1"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryHeaderCharges)));
		}

		[ExpectNoExceptions]
		public void TestCusEntryLineGetDutyOtherTaxFees()
		{
			CreateNewCusRateCode();
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "DTA";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "DTS";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "ADD";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "CVD";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "ADT";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "RTD";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "CTA";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "CTS";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "TAT";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "HWS";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "XX0";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 1M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "XX1";
			lineFee.CF_MethodOfCalculation = "";
			lineFee.CF_Rate = 1M;
			var dutyOtherTaxFees = line.GetDutyOtherTaxFees();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(10));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "DTA"), NUnit.Framework.Is.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "DTS"), NUnit.Framework.Is.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "ADD"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "CVD"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "ADT"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "RTD"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "CTA"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "CTS"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "TAT"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "HWS"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "XX0"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.CF_ChargeType == "XX1"), NUnit.Framework.Is.Not.EqualTo(default(CusEntryLineFee)));
			});
		}

		[ExpectNoExceptions]
		public void TestGetRateCodesByType()
		{
			CreateNewCusRateCode();
			var rateCodeList = SharedHelper.GetRateCodesByType(Factory, "DTY");
			NUnit.Framework.Assert.That(rateCodeList.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "DTA"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "DTS"), NUnit.Framework.Is.True);
			rateCodeList = SharedHelper.GetRateCodesByType(Factory, "COM");
			NUnit.Framework.Assert.That(rateCodeList.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "CTA"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "CTS"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "TAT"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(rateCodeList.Any(x => x == "HWS"), NUnit.Framework.Is.True);
			rateCodeList = SharedHelper.GetRateCodesByType(Factory, "XXX");
			NUnit.Framework.Assert.That(rateCodeList.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		void CreateNewCusRateCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateCusRateType("TW", "DTY");
			rateType.ZZR_CustomsValueFormula = "CV";
			var comRateType = helper.CreateCusRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS";
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CTA", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CTS", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "TAT", comRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "HWS", comRateType.PK);
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestGetMasterBillSegmentID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.EntryNumber = "Test001";
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_MasterBill = "010-9999999";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_WHSMonth = ZString.Empty;
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = header.PK;
			importerDocumentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(importerDocumentaryAddress.IsFreeTradeZone, NUnit.Framework.Is.True);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_WHSMonth = "1";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			address.CustomsCodes.DeleteAll();
			NUnit.Framework.Assert.That(!importerDocumentaryAddress.IsFreeTradeZone, NUnit.Framework.Is.True);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_WHSMonth = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = header.PK;
			supplierDocumentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(supplierDocumentaryAddress.IsFreeTradeZone, NUnit.Framework.Is.True);

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("Test001").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			address.CustomsCodes.DeleteAll();
			NUnit.Framework.Assert.That(!supplierDocumentaryAddress.IsFreeTradeZone, NUnit.Framework.Is.True);
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999999").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(MessageConstants.TransportContractDocumentTypeCodes.NIL).Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");
			declaration.JE_MasterBill = "010-9999998";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo("010-9999998").Using(CustomComparers.TypeComparison), "The method of GetMasterBillSegmentID should be");

			declaration.JE_MasterBill = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The method of GetMasterBillSegmentID should be");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			NUnit.Framework.Assert.That(declaration.GetMasterBillSegmentID(), NUnit.Framework.Is.EqualTo(ZString.Empty), "The method of GetMasterBillSegmentID should be");
		}

		class TransportContractDocumentsForTest : ITransportContractDocument
		{
			public TransportContractDocumentsForTest(string id, string typeCode)
			{
				ID = id;
				TypeCode = typeCode;
			}

			public ZString ID
			{
				get;
				private set;
			}

			public ZString TypeCode
			{
				get;
				private set;
			}

			public IPartyDetails Deconsolidator => null;
		}

		class ClassificationForTest : IClassification
		{
			readonly ZString id;
			readonly ZString identificationTypeCode;
			public ClassificationForTest(ZString id, ZString identificationTypeCode)
			{
				this.id = id;
				this.identificationTypeCode = identificationTypeCode;
			}

			ZString IClassification.ID => id;
			ZString IClassification.IdentificationTypeCode => identificationTypeCode;
		}

		[ExpectNoExceptions]
		public void TestExtractSubBoxID()
		{
			var subBoxId = SharedHelper.ExtractSubBoxID("123548-Z");
			NUnit.Framework.Assert.That(subBoxId, NUnit.Framework.Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
			subBoxId = SharedHelper.ExtractSubBoxID("123548Z");
			NUnit.Framework.Assert.That(subBoxId, NUnit.Framework.Is.EqualTo(ZString.Empty));
			subBoxId = SharedHelper.ExtractSubBoxID("123548-Z-A");
			NUnit.Framework.Assert.That(subBoxId, NUnit.Framework.Is.EqualTo(ZString.Empty));
			subBoxId = SharedHelper.ExtractSubBoxID(ZString.Empty);
			NUnit.Framework.Assert.That(subBoxId, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDoesNotHaveCCPAndCPWNumbers()
		{
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "123", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.DoesNotHaveCCPAndCPWNumbers(testAddress), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "AA111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.DoesNotHaveCCPAndCPWNumbers(testAddress), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.DeleteAll();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BB111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.DoesNotHaveCCPAndCPWNumbers(testAddress), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetCustomsControlID()
		{
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "AA111", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BB111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress), NUnit.Framework.Is.EqualTo(default(OrgCusCode)));
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "123", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CodeType, NUnit.Framework.Is.EqualTo("CBF").Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.DeleteAll();
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CodeType, NUnit.Framework.Is.EqualTo("EPZ").Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.DeleteAll();
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "789", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CustomsRegNo, NUnit.Framework.Is.EqualTo("789").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetCustomsControlID(testAddress).OK_CodeType, NUnit.Framework.Is.EqualTo("FTZ").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2020, 07, 18)]
		[ExpectNoExceptions]
		public void TestGetPOCorPOADocNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AC";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 7, 16);
			entryInstruction.CEI_BoxNumber = "123";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "T1";
			orgHeader.OH_FullName = "test org";
			var pofDoc = orgHeader.RequiredDocuments.AddNew();
			pofDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			pofDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			pofDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			pofDoc.EQ_DateReceived = new ZDateTimeOffset(2019, 7, 17);
			pofDoc.EQ_ValidToDate = new ZDateTime(2020, 7, 17);
			pofDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			pofDoc.EQ_DocNumber = "333333";
			var pofBoxNumberAttr = pofDoc.Attributes.AddNew();
			pofBoxNumberAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			pofBoxNumberAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(!pofDoc.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CustomsDistrict), NUnit.Framework.Is.True);
			var docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "POF doc will not be selected");
			var poaDoc = orgHeader.RequiredDocuments.AddNew();
			poaDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_DateReceived = new ZDateTimeOffset(2019, 7, 17);
			poaDoc.EQ_ValidToDate = new ZDateTime(2020, 7, 17);
			poaDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaDocAttr.D0_AttribValue = "A";
			var poaBoxNumberAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaBoxNumberAttr.D0_AttribValue = "123";
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：111111\r\n起：108年07月17日\r\n迄：109年07月17日").Using(CustomComparers.TypeComparison), "POA doc will be selected");
			poaDocAttr.D0_AttribValue = "B";
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "POA doc will not be selected if customs district does not match");
			poaDocAttr.D0_AttribValue = "A";
			poaDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "POA doc will not be selected if there is no attribute with matching customs district");
			poaDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			var pocDoc = orgHeader.RequiredDocuments.AddNew();
			pocDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			pocDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			pocDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			pocDoc.EQ_DateReceived = new ZDateTimeOffset(2019, 7, 17);
			pocDoc.EQ_ValidToDate = new ZDateTime(2020, 7, 17);
			pocDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			pocDoc.EQ_DocNumber = "222222";
			var pocDocAttr = pocDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			pocDocAttr.D0_AttribValue = "A";
			var pocBoxNumberAttr = pocDoc.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			pocBoxNumberAttr.D0_AttribValue = "123";
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：222222\r\n起：108年07月17日\r\n迄：109年07月17日").Using(CustomComparers.TypeComparison), "POC doc should take priority over POA doc");
			pocDocAttr.D0_AttribValue = "B";
			poaDocAttr.D0_AttribValue = "B";
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "POC doc will not be selected if customs district does not match");
			pocDocAttr.D0_AttribValue = "A";
			pocDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader);
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "POC doc will not be selected if there is no attribute with matching customs district");

			var poaDoc2 = orgHeader.RequiredDocuments.AddNew();
			poaDoc2.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc2.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc2.EQ_DateReceived = new ZDateTimeOffset(2019, 7, 17);
			poaDoc2.EQ_ValidToDate = new ZDateTime(2020, 7, 17);
			poaDoc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc2.EQ_DocNumber = "666666";
			var poaCustomsDistrictDocAttr2 = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaCustomsDistrictDocAttr2.D0_AttribValue = "A";
			var poaCustomsDistrictAttr = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaCustomsDistrictAttr.D0_AttribValue = "123";
			var poaBondedIDAttr = poaDoc2.Attributes[JobRequiredDocAttribTypeList.Codes.BondedID];
			poaBondedIDAttr.D0_AttribValue = "886";
			docNumber = SharedHelper.GetPOCorPOADocNumber(entryInstruction, orgHeader, "886");
			NUnit.Framework.Assert.That(docNumber, NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：666666\r\n起：108年07月17日\r\n迄：109年07月17日").Using(CustomComparers.TypeComparison), "POA doc will be selected");
		}

		[ExpectNoExceptions]
		public void TestGetFirstLastLetterFromWord()
		{
			NUnit.Framework.Assert.That("HELLO".GetFirstLastLetterFromWord(), NUnit.Framework.Is.EqualTo("HO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That("H".GetFirstLastLetterFromWord(), NUnit.Framework.Is.EqualTo("HH").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That("HI".GetFirstLastLetterFromWord(), NUnit.Framework.Is.EqualTo("HI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That("".GetFirstLastLetterFromWord(), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetWords()
		{
			ZString statement = "THIS IS WTG";
			var words = statement.GetWords();
			NUnit.Framework.Assert.That(words.Count(), NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(words.ElementAt(0), NUnit.Framework.Is.EqualTo("THIS"));
			NUnit.Framework.Assert.That(words.ElementAt(1), NUnit.Framework.Is.EqualTo("IS"));
			NUnit.Framework.Assert.That(words.ElementAt(2), NUnit.Framework.Is.EqualTo("WTG"));
			statement = "Wisetech Global";
			words = statement.GetWords();
			NUnit.Framework.Assert.That(words.ElementAt(0), NUnit.Framework.Is.EqualTo("Wisetech"));
			NUnit.Framework.Assert.That(words.ElementAt(1), NUnit.Framework.Is.EqualTo("Global"));
			statement = "Type A company";
			words = statement.GetWords();
			NUnit.Framework.Assert.That(words.ElementAt(0), NUnit.Framework.Is.EqualTo("Type"));
			NUnit.Framework.Assert.That(words.ElementAt(1), NUnit.Framework.Is.EqualTo("A"));
			NUnit.Framework.Assert.That(words.ElementAt(2), NUnit.Framework.Is.EqualTo("company"));
			statement = "Black & Gold Foods";
			words = statement.GetWords();
			NUnit.Framework.Assert.That(words.ElementAt(0), NUnit.Framework.Is.EqualTo("Black"));
			NUnit.Framework.Assert.That(words.ElementAt(1), NUnit.Framework.Is.EqualTo("Gold"));
			NUnit.Framework.Assert.That(words.ElementAt(2), NUnit.Framework.Is.EqualTo("Foods"));
			statement = "Ariston Pty. Ltd";
			words = statement.GetWords();
			NUnit.Framework.Assert.That(words.ElementAt(0), NUnit.Framework.Is.EqualTo("Ariston"));
			NUnit.Framework.Assert.That(words.ElementAt(1), NUnit.Framework.Is.EqualTo("Pty"));
			NUnit.Framework.Assert.That(words.ElementAt(2), NUnit.Framework.Is.EqualTo("Ltd"));
		}

		[ExpectNoExceptions]
		public void TestGetLetterFromEnglishName()
		{
			NUnit.Framework.Assert.That(SharedHelper.GetLetterFromEnglishName("Wisetech Global"), NUnit.Framework.Is.EqualTo("WHGL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetLetterFromEnglishName("Type A company"), NUnit.Framework.Is.EqualTo("TEAACO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetLetterFromEnglishName("Black & Gold Foods"), NUnit.Framework.Is.EqualTo("BKGDFS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SharedHelper.GetLetterFromEnglishName("Ariston Pty. Ltd"), NUnit.Framework.Is.EqualTo("ANPYLD").Using(CustomComparers.TypeComparison));
		}

		#region AdditionalInformations
		[ExpectNoExceptions]
		public void TestGetAdditionalInformations()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			NUnit.Framework.Assert.That(jobDeclaration.GetAdditionalInformations(), NUnit.Framework.Is.EqualTo(default(IEnumerable<IAdditionalInformation>)));
			TestTWCreator.AddDeclarationReservedFields(jobDeclaration);
			var elements = jobDeclaration.GetAdditionalInformations();
			AssertAdditionalInformations(elements);
			while (jobDeclaration.ReservedFields.Count < 11)
			{
				jobDeclaration.ReservedFields.AddNew();
			}

			NUnit.Framework.Assert.That(jobDeclaration.ReservedFields.Count, NUnit.Framework.Is.EqualTo(11));
			NUnit.Framework.Assert.That(jobDeclaration.GetAdditionalInformations().Count(), NUnit.Framework.Is.EqualTo(10));
			jobDeclaration.ReservedFields.AddNew();
			NUnit.Framework.Assert.That(jobDeclaration.GetAdditionalInformations().Count(), NUnit.Framework.Is.EqualTo(10));
			var invoiceline = Factory.NewWithValidTestData<JobComInvoiceLine>();
			NUnit.Framework.Assert.That(invoiceline.GetAdditionalInformations(), NUnit.Framework.Is.EqualTo(default(IEnumerable<IAdditionalInformation>)));
			TestTWCreator.AddInvoiceLineReservedFields(invoiceline);
			elements = invoiceline.GetAdditionalInformations();
			AssertAdditionalInformations(elements);
			while (invoiceline.ReservedFields.Count < 11)
			{
				invoiceline.ReservedFields.AddNew();
			}

			NUnit.Framework.Assert.That(invoiceline.ReservedFields.Count, NUnit.Framework.Is.EqualTo(11));
			NUnit.Framework.Assert.That(invoiceline.GetAdditionalInformations().Count(), NUnit.Framework.Is.EqualTo(10));
			invoiceline.ReservedFields.AddNew();
			NUnit.Framework.Assert.That(invoiceline.GetAdditionalInformations().Count(), NUnit.Framework.Is.EqualTo(10));
		}

		[ExpectNoExceptions]
		internal static void AssertAdditionalInformations(IEnumerable<IAdditionalInformation> elements)
		{
			NUnit.Framework.Assert.That(elements.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(elements.ElementAt(0).StatementCode, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(1).StatementCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(2).StatementCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(3).StatementCode, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(0).StatementDescription, NUnit.Framework.Is.EqualTo("00").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(1).StatementDescription, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(2).StatementDescription, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(3).StatementDescription, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison));
		}

		#endregion
		[ExpectNoExceptions]
		public void TestGetPreviousDocument()
		{
			NUnit.Framework.Assert.That(SharedHelper.GetPreviousDocument(ZString.Empty, 5), NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			NUnit.Framework.Assert.That(SharedHelper.GetPreviousDocument(ZString.Empty, 0), NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			NUnit.Framework.Assert.That(SharedHelper.GetPreviousDocument("AA", 5), NUnit.Framework.Is.Not.EqualTo(default(IPreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestGetTypeCodeForDutyTaxFee()
		{
			AssertTypeCodeValue("DTA", "CAS", "A10");
			AssertTypeCodeValue("DTS", "CAS", "A10");
			AssertTypeCodeValue("CVD", "CAS", "A20");
			AssertTypeCodeValue("ADD", "CAS", "A30");
			AssertTypeCodeValue("RTD", "CAS", "A40");
			AssertTypeCodeValue("ADT", "CAS", "A50");
			AssertTypeCodeValue("TAT", "CAS", "B31");
			AssertTypeCodeValue("TT", "CAS", "B31");
			AssertTypeCodeValue("AT", "CAS", "B31");
			AssertTypeCodeValue("CTA", "CAS", "B10");
			AssertTypeCodeValue("CTS", "CAS", "B10");
			AssertTypeCodeValue("CT", "CAS", "B10");
			AssertTypeCodeValue("HWS", "CAS", "B32");
			AssertTypeCodeValue("VAT", "CAS", "B40");
			AssertTypeCodeValue("TPF", "CAS", "B51", true);
			AssertTypeCodeValue("TPF", "CAS", "B52");
			AssertTypeCodeValue("SSG", "CAS", "B60");
			AssertTypeCodeValue("SS", "CAS", "B60");
			AssertTypeCodeValue("DDF", "CAS", "C10");
			AssertTypeCodeValue("DTA", "DEF", "A19");
			AssertTypeCodeValue("DTS", "DEF", "A19");
			AssertTypeCodeValue("CTD", "DEF", "");
			AssertTypeCodeValue("ATD", "DEF", "");
			AssertTypeCodeValue("RET", "DEF", "");
			AssertTypeCodeValue("ADD", "DEF", "");
			AssertTypeCodeValue("TAT", "DEF", "B69");
			AssertTypeCodeValue("TT", "DEF", "B69");
			AssertTypeCodeValue("AT", "DEF", "B69");
			AssertTypeCodeValue("CTA", "DEF", "B19");
			AssertTypeCodeValue("CTS", "DEF", "B19");
			AssertTypeCodeValue("CT", "DEF", "B19");
			AssertTypeCodeValue("HWS", "DEF", "B79");
			AssertTypeCodeValue("VAT", "DEF", "B49");
			AssertTypeCodeValue("TPF", "DEF", "B59");
			AssertTypeCodeValue("SSG", "DEF", "B89");
			AssertTypeCodeValue("SS", "DEF", "B89");
			AssertTypeCodeValue("DDF", "DEF", "");
		}

		[ExpectNoExceptions]
		void AssertTypeCodeValue(ZString chargeType, ZString chargeMethodOfPayment, ZString expectedResult, bool declarationIsImport = false)
		{
			NUnit.Framework.Assert.That(SharedHelper.GetTypeCodeForDutyTaxFee(chargeType, chargeMethodOfPayment, declarationIsImport), NUnit.Framework.Is.EqualTo(expectedResult), "GoodsShipmentDutyTaxFee.TypeCode should be");
		}

		[ExpectNoExceptions]
		public void TestGetEnglishLanguageCodes()
		{
			var expected = new ZString[] { Core.SharedConstants.Languages.English, Core.SharedConstants.Languages.EnglishAmerican, Core.SharedConstants.Languages.EnglishBritish };
			var codes = SharedHelper.GetEnglishLanguageCodes();
			NUnit.Framework.Assert.That(codes, NUnit.Framework.Is.EquivalentTo(expected));
		}
	}
}
