using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5301EDIMessageDocumentWrapper))]
	internal sealed class N5301EDIMessageDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			header.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(Wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AB  07094AD515").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTranshipmentNo()
		{
			header.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(Wrapper.TranshipmentNo, NUnit.Framework.Is.EqualTo("AB/  /07/094/AD515").Using(CustomComparers.TypeComparison), "when length of TranshipmentNo is more than 13");
			header.EntryNumber = "AB  07094AD51";
			NUnit.Framework.Assert.That(Wrapper.TranshipmentNo, NUnit.Framework.Is.EqualTo(ZString.Empty), "when length of TranshipmentNo is 13");
			header.EntryNumber = "AB  07094AD5";
			NUnit.Framework.Assert.That(Wrapper.TranshipmentNo, NUnit.Framework.Is.EqualTo(ZString.Empty), "when length of TranshipmentNo less than 13");
		}

		[ExpectNoExceptions]
		public void TestRegistrationNo()
		{
			header.BH_VoyageNumber = "07AM04";
			NUnit.Framework.Assert.That(Wrapper.RegistrationNo, NUnit.Framework.Is.EqualTo("07AM04").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestManifestSerialNumber()
		{
			header.ArrivalBill.B0_ReferenceID = "2014";
			NUnit.Framework.Assert.That(Wrapper.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("2014").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMasterBill()
		{
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			header.ArrivalBill.B0_MasterBillNumber = "M000001";
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaPackedSundryGoods;
			NUnit.Framework.Assert.That(Wrapper.MasterBill, NUnit.Framework.Is.EqualTo("M000001").Using(CustomComparers.TypeComparison), "Master Bill Number is 'M00-0001' when type code is 704");

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			header.ArrivalBill.B0_MasterBillNumber = "M000002";
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.AirNotExpressDelivery;
			NUnit.Framework.Assert.That(Wrapper.MasterBill, NUnit.Framework.Is.EqualTo("M00-0002").Using(CustomComparers.TypeComparison), "Master Bill Number is 'M00-0002' when type code is 741");
		}

		[ExpectNoExceptions]
		public void TestHouseBill()
		{
			header.ArrivalBill.B0_HouseBillNumber = "H000001";
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaPackedSundryGoods;
			NUnit.Framework.Assert.That(Wrapper.HouseBill, NUnit.Framework.Is.EqualTo("H000001").Using(CustomComparers.TypeComparison), "House Bill Number is 'H000001' when type code is 714");
			header.ArrivalBill.B0_HouseBillNumber = "H000002";
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.AirNotExpressDelivery;
			NUnit.Framework.Assert.That(Wrapper.HouseBill, NUnit.Framework.Is.EqualTo("H000002").Using(CustomComparers.TypeComparison), "House Bill Number is 'H000002' when type code is 703");
		}

		[ExpectNoExceptions]
		public void TestVesselRegistrationNumber()
		{
			header.MovementHeader.BM_TransportAtDeparture = "07AM68";
			NUnit.Framework.Assert.That(Wrapper.VesselRegistrationNumber, NUnit.Framework.Is.EqualTo("07AM68").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShippingOrderNumber()
		{
			header.MovementBill.B0_ReferenceID = "9014";
			NUnit.Framework.Assert.That(Wrapper.ShippingOrderNumber, NUnit.Framework.Is.EqualTo("9014").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportMasterBill()
		{
			var movementHeader = header.MovementHeader;
			header.MovementBill.B0_MasterBillNumber = "M000001";
			movementHeader.BM_ExportLadenOn = "Test";
			NUnit.Framework.Assert.That(Wrapper.TransportMasterBill, NUnit.Framework.Is.EqualTo("M000001").Using(CustomComparers.TypeComparison), "Transport Master Bill Number is 'M000001' when type code is 704");
			header.MovementBill.B0_MasterBillNumber = "M000002";
			movementHeader.BM_ExportLadenOn = "";
			NUnit.Framework.Assert.That(Wrapper.TransportMasterBill, NUnit.Framework.Is.EqualTo("M00-0002").Using(CustomComparers.TypeComparison), "Transport Master Bill Number is 'M00-0002' when type code is 741");
		}

		[ExpectNoExceptions]
		public void TestTransportHouseBill()
		{
			var movementHeader = header.MovementHeader;
			header.MovementBill.B0_HouseBillNumber = "H000001";
			movementHeader.BM_ExportLadenOn = "Test";
			NUnit.Framework.Assert.That(Wrapper.TransportHouseBill, NUnit.Framework.Is.EqualTo("H000001").Using(CustomComparers.TypeComparison), "Transport House Bill Number is 'H000001' when type code is 714");
			header.MovementBill.B0_HouseBillNumber = "H000002";
			movementHeader.BM_ExportLadenOn = "";
			NUnit.Framework.Assert.That(Wrapper.TransportHouseBill, NUnit.Framework.Is.EqualTo("H000002").Using(CustomComparers.TypeComparison), "Transport House Bill Number is 'H000002' when type code is 703");
		}

		[ExpectNoExceptions]
		public void TestAgentID()
		{
			header.TW_BoxNumber = "094";
			NUnit.Framework.Assert.That(Wrapper.AgentID, NUnit.Framework.Is.EqualTo("094").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			header.BH_CustomsProfile = "CBK1123-Z";
			NUnit.Framework.Assert.That(Wrapper.SubBoxID, NUnit.Framework.Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRepresentativePersonName()
		{
			var borker = Factory.NewWithValidTestData<GlbStaff>();
			var brkCertificate = borker.Certificates.AddNew();
			brkCertificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			brkCertificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			brkCertificate.XZ_RefNumber = "00D85";
			header.BH_GS_NKCusAgent = borker.GS_Code;
			NUnit.Framework.Assert.That(Wrapper.RepresentativePersonName, NUnit.Framework.Is.EqualTo("00D85").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBrokerStaffName()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "徐元昉";
			staff.GS_Code = "AQE";
			NUnit.Framework.Assert.That(Wrapper.BrokerStaffName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			header.BH_GS_NKCusAgent = staff.GS_Code;
			NUnit.Framework.Assert.That(Wrapper.BrokerStaffName, NUnit.Framework.Is.EqualTo("徐元昉").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApplicantInformation()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_FullName = "importer01";
			var importerAddress = importerOrg.Addresses.AddNew();
			importerAddress.Address1 = "ad1222222";
			importerAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			importerAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			importerAddress.OA_IsActive = true;
			importerAddress.OA_Language = Core.SharedConstants.Languages.English;
			importerAddress.OA_CompanyNameOverride = "TONGLIT LOGISTICS CO., LTD";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var importerTranslatedAddress = importerAddress.TranslatedAddresses.AddNew();
			importerTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			importerTranslatedAddress.Address1 = "otaaddress 111";
			importerTranslatedAddress.CompanyName = "東立物流股份有限公司";
			importerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "GB98222221365", Core.Constants.CountryCodes.Taiwan);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "80279759", Core.Constants.CountryCodes.Taiwan);
			header.BH_OA_Importer = importerAddress.PK;
			Factory.Save();

			NUnit.Framework.Assert.That(Wrapper.ApplicantName, NUnit.Framework.Is.EqualTo("TONGLIT LOGISTICS CO., LTD").Using(CustomComparers.TypeComparison), "ApplicantName");
			NUnit.Framework.Assert.That(Wrapper.ApplicantID, NUnit.Framework.Is.EqualTo("80279759").Using(CustomComparers.TypeComparison), "ApplicantID");
		}

		[ExpectNoExceptions]
		public void TestDeclarationChineseName()
		{
			var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			proxyOrg.OH_FullName = "proxy01";
			var proxyAddress = proxyOrg.Addresses.AddNew();
			proxyAddress.Address1 = "xx222222";
			proxyAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			proxyAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			proxyAddress.OA_IsActive = true;
			proxyAddress.OA_Language = Core.SharedConstants.Languages.English;
			proxyAddress.OA_CompanyNameOverride = "company 1";
			proxyAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var proxyTranslatedAddress = proxyAddress.TranslatedAddresses.AddNew();
			proxyTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			proxyTranslatedAddress.Address1 = "otaaddress 111";
			proxyTranslatedAddress.CompanyName = "公司1";
			proxyAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "GB98222221365", Core.Constants.CountryCodes.Taiwan);
			proxyOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID68955222", Core.Constants.CountryCodes.Taiwan);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = proxyOrg.PK;
			Factory.Save();

			NUnit.Framework.Assert.That(Wrapper.DeclarationChineseName, NUnit.Framework.Is.EqualTo("公司1").Using(CustomComparers.TypeComparison), "DeclarationChineseName");
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			header.BH_ImportTransportMode = "1";
			NUnit.Framework.Assert.That(Wrapper.BorderTransportMeans, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "type is sea");
			header.BH_ImportTransportMode = "4";
			NUnit.Framework.Assert.That(Wrapper.BorderTransportMeans, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison), "type is air");
		}

		[ExpectNoExceptions]
		public void TestTranshipmentType()
		{
			header.MovementHeader.BM_InBondEntryType = "T2";
			NUnit.Framework.Assert.That(Wrapper.TranshipmentType, NUnit.Framework.Is.EqualTo("T2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			header.BH_ETA = new ZDateTime(2018, 8, 3);
			NUnit.Framework.Assert.That(Wrapper.ArrivalDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2018, 8, 3)));
		}

		[TestDate(2009, 1, 2, 13, 20, 20)]
		[TestUtcOffset(8, 0, 0)]
		[ExpectNoExceptions]
		public void TestCloseDateTime()
		{
			tWMessage.EM_SystemCreateTimeUtc = new ZDateTime(2019, 9, 3);
			NUnit.Framework.Assert.That(Wrapper.CloseDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 9, 3).AddHours(8)), "There is a message on the CusInBondHeader");
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			var wrapper2 = new N5301EDIMessageDocumentWrapper(header2);
			NUnit.Framework.Assert.That(wrapper2.CloseDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Invalid), "There is no message on the CusInBondHeader");
		}

		[ExpectNoExceptions]
		public void TestLoadingLocationID()
		{
			moveHeader.BM_PlaceOfLoading = "CNFOC";
			NUnit.Framework.Assert.That(Wrapper.LoadingLocationID, NUnit.Framework.Is.EqualTo("CNFOC").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2017, 12, 26)]
		[ExpectNoExceptions]
		public void TestLoadingLocationName()
		{
			moveHeader.BM_PlaceOfLoading = "CNFOC";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Loading Location");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "CNFOC", "東立物流連鎖倉", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			Factory.Save();
			NUnit.Framework.Assert.That(Wrapper.LoadingLocationName, NUnit.Framework.Is.EqualTo("東立物流連鎖倉").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocationID()
		{
			moveHeader.BM_RL_NKForeignDestPort = "XXXXX";
			NUnit.Framework.Assert.That(Wrapper.UnloadingLocationID, NUnit.Framework.Is.EqualTo("XXXXX").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2017, 12, 26)]
		[ExpectNoExceptions]
		public void TestUnloadingLocationName()
		{
			moveHeader.BM_RL_NKForeignDestPort = "XXXXX";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Unloading Location");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXXX", "東立物流連鎖倉2", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			Factory.Save();
			NUnit.Framework.Assert.That(Wrapper.UnloadingLocationName, NUnit.Framework.Is.EqualTo("東立物流連鎖倉2").Using(CustomComparers.TypeComparison));
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "XXXXX";
			unloco.RL_PortName = "Canillo";
			Factory.Save();
			NUnit.Framework.Assert.That(Wrapper.UnloadingLocationName, NUnit.Framework.Is.EqualTo("Canillo").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeansName()
		{
			header.BH_ImportConveyanceName = "DREAM DIVA";
			NUnit.Framework.Assert.That(Wrapper.BorderTransportMeansName, NUnit.Framework.Is.EqualTo("DREAM DIVA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeansID()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "X1";
			vessel.RV_LloydsNumber = "3EKW6";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			header.BH_ImportConveyanceName = "X1";
			NUnit.Framework.Assert.That(Wrapper.BorderTransportMeansID, NUnit.Framework.Is.EqualTo("3EKW6").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeansJourneyID()
		{
			header.BH_UniqueVoyageIdentifier = "11";
			NUnit.Framework.Assert.That(Wrapper.BorderTransportMeansJourneyID, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeansName()
		{
			moveHeader.BM_ExportLadenOn = "VIKING SEA";
			NUnit.Framework.Assert.That(Wrapper.DepartureTransportMeansName, NUnit.Framework.Is.EqualTo("VIKING SEA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeansID()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "9V8009";
			vessel.RV_LloydsNumber = "12345";
			vessel.RV_RN_NKCountryOfReg = "CN";
			Factory.Save();
			moveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaPackedSundryGoods;
			moveHeader.BM_ExportLadenOn = "9V8009";
			NUnit.Framework.Assert.That(Wrapper.DepartureTransportMeansID, NUnit.Framework.Is.EqualTo("12345").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeansJourneyID()
		{
			moveHeader.BM_ConveyanceNumber = "2";
			NUnit.Framework.Assert.That(Wrapper.DepartureTransportMeansJourneyID, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalPackageQuantity()
		{
			header.ArrivalBill.B0_ManifestQty = 6;
			NUnit.Framework.Assert.That(Wrapper.TotalPackageQuantity, NUnit.Framework.Is.EqualTo(6).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			header.ArrivalBill.B0_ManifestUQ = "UNT";
			NUnit.Framework.Assert.That(Wrapper.Packaging, NUnit.Framework.Is.EqualTo("UNT").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMarksNumbersAndContainer()
		{
			var expected = @"APLU1234567/42G1/0";
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "42G1";
			var container = moveDetail.Containers.AddNew();
			container.BC_RC = refContainer1.PK;
			container.BC_ContainerNum = "APLU1234567";
			container.BC_Mode = CusInBondContainerModeList.Codes.EmptyContainer;
			container.BC_IsPart = true;
			NUnit.Framework.Assert.That(Wrapper.Container, NUnit.Framework.Is.EqualTo(expected).Using(CustomComparers.TypeComparison), "Container");
			moveHeader.InBondMoveDetail.InBondMoveLineItem.BI_MarksAndNumbers = "Marks and Numbers";
			NUnit.Framework.Assert.That(Wrapper.MarksNumbers, NUnit.Framework.Is.EqualTo("Marks and Numbers").Using(CustomComparers.TypeComparison), "Marks Numbers");
		}

		[TestDate(2019, 8, 16)]
		[ExpectNoExceptions]
		public void TestOtherRecordedItems()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			header.ReceiptOffice = "AB";
			header.TW_BoxNumber = "123";
			header.BH_OA_Importer = orgAddress.PK;

			var jobRequiredDocumentForPOC = GetJobRequiredDocument(orgHeader);
			jobRequiredDocumentForPOC.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			jobRequiredDocumentForPOC.EQ_DocNumber = "333";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo("委任書號:333").Using(CustomComparers.TypeComparison), "when EQ_DocType is 'POF'");

			var jobRequiredDocumentForPOF = GetJobRequiredDocument(orgHeader);
			jobRequiredDocumentForPOF.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocumentForPOF.EQ_DocNumber = "222";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo("委任書號:222").Using(CustomComparers.TypeComparison), "when EQ_DocType is 'POA'");

			var jobRequiredDocumentForPOA = GetJobRequiredDocument(orgHeader);
			jobRequiredDocumentForPOA.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			jobRequiredDocumentForPOA.EQ_DocNumber = "111";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo("委任書號:111").Using(CustomComparers.TypeComparison), "when EQ_DocType is 'POC'");

			header.ReceiptOffice = "BA";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo(ZString.Empty), "The first letter of ReceiptOffice is not equal to attribute value 'A', so return empty.");

			header.ReceiptOffice = "AC";
			header.TW_BoxNumber = "123";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo("委任書號:111").Using(CustomComparers.TypeComparison), "The first letter of ReceiptOffice is A as well.");

			header.TW_BoxNumber = "12";
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo(ZString.Empty), "TW_BoxNumber is not equal to the Box Number set in the JobRequiredDocAttrib.");

			header.TW_BoxNumber = "123";
			orgHeader.RequiredDocuments.Cast<JobRequiredDocument>().ForEach(x => x.EQ_DocUsage = JobRequiredDocument.DocUsage.Carrier);
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo(ZString.Empty), "All the RequiredDocuments are not Broker.");

			orgHeader.RequiredDocuments.Cast<JobRequiredDocument>().ForEach(x =>
			{
				x.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
				x.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			});
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo(ZString.Empty), "All countries of the RequiredDocuments are not Taiwan.");

			orgHeader.RequiredDocuments.Cast<JobRequiredDocument>().ForEach(x =>
			{
				x.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
				x.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
			});
			NUnit.Framework.Assert.That(Wrapper.OtherRecordedItems, NUnit.Framework.Is.EqualTo(ZString.Empty), "All the RequiredDocument settings have expired.");
		}

		JobRequiredDocument GetJobRequiredDocument(OrgHeader orgHeader)
		{
			var jobRequiredDocument = orgHeader.RequiredDocuments.AddNew();
			jobRequiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			jobRequiredDocument.EQ_ValidToDate = ZDateTime.Today;
			var jobRequiredDocAttrib = jobRequiredDocument.Attributes.AddNew();
			jobRequiredDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			jobRequiredDocAttrib.D0_AttribValue = "A";

			jobRequiredDocAttrib = jobRequiredDocument.Attributes.AddNew();
			jobRequiredDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			jobRequiredDocAttrib.D0_AttribValue = "123";
			return jobRequiredDocument;
		}

		[ExpectNoExceptions]
		public void TestBarcode()
		{
			header.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(Wrapper.Barcode, NUnit.Framework.Is.EqualTo("*AB07094AD515*").Using(CustomComparers.TypeComparison), "MID(FunctionCode,3,2) is not empty");
			header.EntryNumber = "AB07094AD515AA";
			NUnit.Framework.Assert.That(Wrapper.Barcode, NUnit.Framework.Is.EqualTo("*AB07094AD515AA*").Using(CustomComparers.TypeComparison), "MID(FunctionCode,3,2) is empty");
		}

		[ExpectNoExceptions]
		public void TestBarcodeSplitWithSpace()
		{
			header.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(Wrapper.BarcodeSplitWithSpace, NUnit.Framework.Is.EqualTo("* A B 0 7 0 9 4 A D 5 1 5 *").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCargoDescription()
		{
			header.MovementHeader.InBondMoveDetail.InBondMoveLineItem.BI_Description = "T/S CARGO EX DEAM DIVA V.11FROM JANGO TO";
			NUnit.Framework.Assert.That(Wrapper.CargoDescription, NUnit.Framework.Is.EqualTo("T/S CARGO EX DEAM DIVA V.11FROM JANGO TO").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasureTariffQuantity()
		{
			header.MovementHeader.InBondMoveDetail.InBondMoveLineItem.BI_QuantityUQ = "UNT";
			header.MovementHeader.InBondMoveDetail.InBondMoveLineItem.BI_Quantity = 8m;
			NUnit.Framework.Assert.That(Wrapper.GoodsMeasureTariffQuantity, NUnit.Framework.Is.EqualTo(8m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasureUnitCode()
		{
			header.MovementHeader.InBondMoveDetail.InBondMoveLineItem.BI_Quantity = 8m;
			header.MovementHeader.InBondMoveDetail.InBondMoveLineItem.BI_QuantityUQ = "UNT";
			NUnit.Framework.Assert.That(Wrapper.GoodsMeasureUnitCode, NUnit.Framework.Is.EqualTo("UNT").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalGrossMassMeasure()
		{
			header.ArrivalBill.B0_Weight = 19010m;
			NUnit.Framework.Assert.That(Wrapper.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(19010m).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.New<EDIMessage>();
			header = Factory.New<CusInBondHeader>();
			header.TW_BoxNumber = "094";
			message.EM_MessageType = MessageTypeList.Codes.TRA;
			message.EM_LinkTable = "CusInBondHeader";
			message.EM_LinkUniqueID = header.PK;
			header.Messages.Add(message);
			return new N5301EDIMessageDocumentWrapper(header);
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		TWMessage tWMessage;
		N5301EDIMessageDocumentWrapper Wrapper => new N5301EDIMessageDocumentWrapper(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusInBondHeader>();
			moveHeader = header.MovementHeader;
			tWMessage = header.Messages.AddNew();
			tWMessage.EM_MessageType = "TRA";
		}
	}
}
