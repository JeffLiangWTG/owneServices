using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NCTSMessageProviderTestHelper : IDisposable
	{
		public NCTSMessageProviderTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			Setup();
		}

		readonly BusinessObjectFactory factory;
		GlbStaff loggedInUser;
		IDisposable tempUserContext;

		void IDisposable.Dispose()
		{
			tempUserContext.Dispose();
		}

		void Setup()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "TR", "052", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			factory.Save();

			loggedInUser = factory.NewWithValidTestData<GlbStaff>();
			loggedInUser.GS_Code = "LPK";

			tempUserContext = Env.SetTemporaryUserContext(new UserContext(loggedInUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		public NctsHeader GetProviderNCTSHeader()
		{
			GlbStaff.CurrentUser.GS_Code = "LPK";
			GlbStaff.CurrentUser.GS_FullName = "ilker pakten";

			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_JobReference = "202200000231";
			nctsHeader.BH_RL_NKImportLoadPort = "TR";
			nctsHeader.Trailer1 = "AB1234CD";
			nctsHeader.Trailer2 = "AB5678CD";
			nctsHeader.StampDutyStatus = "5";
			nctsHeader.StampDuty = 150.55m;

			var longString = new string('x', 20);
			nctsHeader.Branch.Company.GC_Name = "ULUKOM LOGISTICS" + longString;
			nctsHeader.Branch.Company.GC_Address1 = "1. TAŞOCAĞI CAD. BURÇ SK.  ";
			nctsHeader.Branch.Company.GC_Address2 = "ULUKOM İŞ  " + longString;
			nctsHeader.Branch.Company.GC_City = "İSTANBUL";
			nctsHeader.Branch.Company.GC_BusinessRegNo = "8890376405";

			var message = nctsHeader.Messages.AddNew();
			message.EM_MessageType = "TRN";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_Status = "QUE";
			message.EM_MessageNum = "12";

			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "ABCD1234560";

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "ABCD1234561";

			var container3 = nctsHeader.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "ABCD1234562";

			var manifestToOpen = nctsHeader.ManifestsToOpenList.AddNew();
			manifestToOpen.IsPartial = true;
			manifestToOpen.CSI_ReferenceNumber2 = "Manifest Dec.No";
			manifestToOpen.AtWarehouse = true;
			manifestToOpen.CSI_ReferenceNumber = "Bill No";
			manifestToOpen.CSI_CustomsOffice = "A0001";
			manifestToOpen.CSI_LineNo = 1;
			manifestToOpen.CSI_Quantity3 = 200;

			var manifestToOpen2 = nctsHeader.ManifestsToOpenList.AddNew();
			manifestToOpen2.IsPartial = false;
			manifestToOpen2.CSI_ReferenceNumber2 = "Manifest Dec.No 2";
			manifestToOpen2.AtWarehouse = false;
			manifestToOpen2.CSI_ReferenceNumber = "Bill No 2";
			manifestToOpen2.CSI_CustomsOffice = "A0002";
			manifestToOpen2.CSI_LineNo = 2;
			manifestToOpen2.CSI_Quantity3 = 400;

			var departureMovement = nctsHeader.MovementHeader;

			departureMovement.BM_InBondEntryType = "A";
			departureMovement.BM_RL_NKDestinationPort = "TR";
			departureMovement.BM_LocationOfGoodsCode = "CCC";
			departureMovement.BM_LocationOfGoods = "DDD";
			departureMovement.BM_ExportTransportMode = "2";
			departureMovement.BM_PlaceOfLoading = "ISTANBUL";
			departureMovement.BM_InlandTransportMode = "3";
			departureMovement.BM_TransportAtDeparture = "EEE";
			departureMovement.BM_RN_NKTransportAtDepartureCountry = "GB";
			departureMovement.BM_TOLCarrierID = "YGT";
			departureMovement.BM_TOLCarrierCode = "L";
			departureMovement.BM_RL_NKForeignDestPort = "TRIST";
			departureMovement.BM_MethodOfPayment = "X";
			departureMovement.BM_AdditionalText = "additional text";
			departureMovement.BM_BTAIndicator = "1";
			departureMovement.BM_ConveyanceNumber = "4";
			departureMovement.BM_PlaceOfUnloading = "Test";
			departureMovement.BM_CustomsSubPlace = "12345678901234567";
			var nctsDepartureCargoDesc1 = departureMovement.GoodsItems.AddNew();

			var containerPivots1 = nctsDepartureCargoDesc1.ContainersPivots;
			containerPivots1[0].ContainerSelected = true;
			containerPivots1[2].ContainerSelected = true;

			nctsDepartureCargoDesc1.BY_LineNo = 1;
			nctsDepartureCargoDesc1.BY_FormattedHarmonisedTariff = "690320";
			nctsDepartureCargoDesc1.BY_Description = "AKSAM PARCA";
			nctsDepartureCargoDesc1.BY_GrossWeight = 120.55m;
			nctsDepartureCargoDesc1.BY_NetWeight = 110.24m;
			nctsDepartureCargoDesc1.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Turkey;
			nctsDepartureCargoDesc1.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
			nctsDepartureCargoDesc1.BY_MonetaryValue = 0m;
			nctsDepartureCargoDesc1.BY_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var nctsDepartureCargoDesc2 = departureMovement.GoodsItems.AddNew();

			var containerPivots2 = nctsDepartureCargoDesc2.ContainersPivots;
			containerPivots2[1].ContainerSelected = true;
			containerPivots2[2].ContainerSelected = true;

			nctsDepartureCargoDesc2.BY_GrossWeight = 130.12m;

			var packages1 = nctsDepartureCargoDesc1.Packages.AddNew();
			packages1.B5_MarksAndNumbers = "ABCD1234560";
			packages1.B5_UnitType = "BI";
			packages1.B5_UnitCount = 10;

			var packages2 = nctsDepartureCargoDesc1.Packages.AddNew();
			packages2.B5_MarksAndNumbers = "ABCD1234561";
			packages2.B5_UnitType = "KG";
			packages2.B5_UnitCount = 11;

			var warehouseToOpen = nctsDepartureCargoDesc1.PreviousDocuments.AddNew();
			warehouseToOpen.CSI_Code = "ANT";
			warehouseToOpen.CSI_ReferenceNumber = "22068888AN123456";
			warehouseToOpen.CSI_LineNo = 1;
			warehouseToOpen.CSI_Quantity = 20m;
			warehouseToOpen.CSI_Description = "Explanation";
			warehouseToOpen.CSI_Value = 100m;
			warehouseToOpen.CSI_RX_NKCurrency = "TRY";
			warehouseToOpen.Incoterm = "FOB";
			warehouseToOpen.CSI_SubType = "1";
			warehouseToOpen.CSI_Procedure = "11";
			warehouseToOpen.CSI_RN_NKCountryCode = "TR";

			CreateCompanies(nctsHeader, nctsDepartureCargoDesc1);
			CreateCustomsOffices(nctsHeader);

			var guarantees = nctsHeader.Guarantees.AddNew();
			guarantees.PW_BondType = "D";
			guarantees.PW_BondNumber = "DENIZ";
			guarantees.PW_BondAmount = 1;
			guarantees.PW_Password = "1234";

			Documents(nctsDepartureCargoDesc1, nctsDepartureCargoDesc2);

			return nctsHeader;
		}

		void CreateCustomsOffices(NctsHeader nctsHeader)
		{
			var customsOffice1 = nctsHeader.CustomsOffices.FirstOrDefault();
			customsOffice1.CY_ParentID = nctsHeader.PK;
			customsOffice1.CY_Code = "DEP";
			customsOffice1.CY_Data = "TR066666";

			var customsOffice2 = nctsHeader.CustomsOffices.AddNew();
			customsOffice2.CY_Code = "TRA";
			customsOffice2.CY_Data = "TR66667";

			var customsOffice3 = nctsHeader.CustomsOffices.AddNew();
			customsOffice3.CY_Code = "TXT";
			customsOffice3.CY_Data = "TR310100";

			var customsOffice4 = nctsHeader.CustomsOffices?.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == "DES") ?? nctsHeader.CustomsOffices.AddNew();
			customsOffice4.CY_Code = "DES";
			customsOffice4.CY_Data = "TR350300";
		}

		void CreateCompanies(NctsHeader nctsHeader, NctsDepartureCargoDesc nctsDepartureCargoDesc1)
		{
			var longString = new string('x', 20);

			var orgPrincipal = factory.New<OrgHeader>();
			orgPrincipal.OH_Code = "XYZAAA";
			orgPrincipal.OH_FullName = "xPrincipal Company Name" + longString;
			orgPrincipal.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890");
			orgPrincipal.OH_Language = "TR-IST";

			var addressPrinciple = orgPrincipal.MainAddress;
			addressPrinciple.OA_OH = orgPrincipal.PK;
			addressPrinciple.CompanyName = "xPrincipal Company Name";
			addressPrinciple.Address1 = "xAdress1";
			addressPrinciple.Address2 = "xAdress2" + longString;
			addressPrinciple.City = "IST";
			addressPrinciple.Postcode = "340300";
			addressPrinciple.OA_RN_NKCountryCode = "TR";
			nctsHeader.Principal.E2_OA_Address = addressPrinciple.PK;

			var orgConsignor = factory.New<OrgHeader>();
			orgConsignor.OH_Code = "XYZAAB";
			orgConsignor.OH_FullName = "xConsignor Company Name" + longString;
			orgConsignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567891");
			orgConsignor.OH_Language = "TR-IST";

			var addressConsignor = orgConsignor.MainAddress;
			addressConsignor.OA_OH = orgConsignor.PK;
			addressConsignor.CompanyName = "xConsignor Company Name";
			addressConsignor.Address1 = "xyAdress1";
			addressConsignor.Address2 = "xyAdress2" + longString;
			addressConsignor.City = "IST";
			addressConsignor.Postcode = "340301";
			addressConsignor.OA_RN_NKCountryCode = "TR";
			nctsHeader.Consignor.E2_OA_Address = addressConsignor.PK;

			var orgConsignee = factory.New<OrgHeader>();
			orgConsignee.OH_Code = "XYZAAC";
			orgConsignee.OH_FullName = "xConsignee Company Name" + longString;
			orgConsignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567892");
			orgConsignee.OH_Language = "TR-IST";

			var addressConsignee = orgConsignee.MainAddress;
			addressConsignee.OA_OH = orgConsignee.PK;
			addressConsignee.CompanyName = "xConsignee Company Name";
			addressConsignee.Address1 = "xyzAdress1";
			addressConsignee.Address2 = "xyzAdress2" + longString;
			addressConsignee.City = "IST";
			addressConsignee.Postcode = "340302";
			addressConsignee.OA_RN_NKCountryCode = "TR";
			nctsHeader.Consignee.E2_OA_Address = addressConsignee.PK;

			var orgConsignor2 = factory.New<OrgHeader>();
			orgConsignor2.OH_Code = "XYZAAB";
			orgConsignor2.OH_FullName = "xConsignor Company Name 2" + longString;
			orgConsignor2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567891");
			orgConsignor2.OH_Language = "TR-IST";

			var addressConsignor2 = orgConsignor2.MainAddress;
			addressConsignor2.OA_OH = orgConsignor2.PK;
			addressConsignor2.CompanyName = "xConsignor Company Name 2";
			addressConsignor2.Address1 = "xyAdress1";
			addressConsignor2.Address2 = "xyAdress2" + longString;
			addressConsignor2.City = "IST";
			addressConsignor2.Postcode = "340301";
			addressConsignor2.OA_RN_NKCountryCode = "TR";
			nctsDepartureCargoDesc1.Consignor.E2_OA_Address = addressConsignor2.PK;

			var orgConsignee2 = factory.New<OrgHeader>();
			orgConsignee2.OH_Code = "XYZAAC";
			orgConsignee2.OH_FullName = "xConsignee Company Name 2" + longString;
			orgConsignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567892");
			orgConsignee2.OH_Language = "TR-IST";

			var addressConsignee2 = orgConsignee2.MainAddress;
			addressConsignee2.OA_OH = orgConsignee2.PK;
			addressConsignee2.CompanyName = "xConsignee Company Name 2";
			addressConsignee2.Address1 = "xyzAdress1";
			addressConsignee2.Address2 = "xyzAdress2" + longString;
			addressConsignee2.City = "IST";
			addressConsignee2.Postcode = "340302";
			addressConsignee2.OA_RN_NKCountryCode = "TR";
			nctsDepartureCargoDesc1.Consignee.E2_OA_Address = addressConsignee2.PK;

			var orgCarrier = factory.New<OrgHeader>();
			orgCarrier.OH_Code = "XYZAAC";
			orgCarrier.OH_FullName = "xCarrier Company Name" + longString;
			orgCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567892");
			orgCarrier.OH_Language = "TR-IST";

			var addressCarrier = orgCarrier.MainAddress;
			addressCarrier.OA_OH = orgCarrier.PK;
			addressCarrier.CompanyName = "xCarrier Company Name";
			addressCarrier.Address1 = "xyzAdress1";
			addressCarrier.Address2 = "xyzAdress2" + longString;
			addressCarrier.City = "IST";
			addressCarrier.Postcode = "340302";
			addressCarrier.OA_RN_NKCountryCode = "TR";
			nctsHeader.MovementHeader.Carrier.E2_OA_Address = addressCarrier.PK;
		}

		void Documents(NctsDepartureCargoDesc nctsDepartureCargoDesc1, NctsDepartureCargoDesc nctsDepartureCargoDesc2)
		{
			var supportingDocuments1 = nctsDepartureCargoDesc1.SupportingDocuments.AddNew();
			supportingDocuments1.CSI_Code = "705";
			supportingDocuments1.CSI_ReferenceNumber = "MSCIST67676";
			supportingDocuments1.CSI_Description = "19/02/2012";

			var supportingDocuments2 = nctsDepartureCargoDesc1.SupportingDocuments.AddNew();
			supportingDocuments2.CSI_Code = "706";
			supportingDocuments2.CSI_ReferenceNumber = "MSCIST67676";
			supportingDocuments2.CSI_Description = "19/02/2012";

			var supportingDocuments3 = nctsDepartureCargoDesc2.SupportingDocuments.AddNew();
			supportingDocuments3.CSI_Code = "707";
			supportingDocuments3.CSI_ReferenceNumber = "MSCIST67676";
			supportingDocuments3.CSI_Description = "19/02/2012";

			var previousDocuments1 = nctsDepartureCargoDesc1.PreviousDocuments.AddNew();
			previousDocuments1.CSI_Code = "708";
			previousDocuments1.CSI_ReferenceNumber = "MSCIST67677";
			previousDocuments1.CSI_Description = "19/02/2013";

			var previousDocuments2 = nctsDepartureCargoDesc1.PreviousDocuments.AddNew();
			previousDocuments2.CSI_Code = "709";
			previousDocuments2.CSI_ReferenceNumber = "MSCIST67677";
			previousDocuments2.CSI_Description = "19/02/2013";

			var previousDocuments3 = nctsDepartureCargoDesc2.PreviousDocuments.AddNew();
			previousDocuments3.CSI_Code = "710";
			previousDocuments3.CSI_ReferenceNumber = "MSCIST67677";
			previousDocuments3.CSI_Description = "19/02/2013";

			var additionalDocuments1 = nctsDepartureCargoDesc1.AdditionalInfos.AddNew();
			additionalDocuments1.CSI_Code = "711";
			additionalDocuments1.CSI_ReferenceNumber = "MSCIST67678";
			additionalDocuments1.CSI_Description = "19/02/2014";
			additionalDocuments1.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;

			var additionalDocuments2 = nctsDepartureCargoDesc1.AdditionalInfos.AddNew();
			additionalDocuments2.CSI_Code = "712";
			additionalDocuments2.CSI_ReferenceNumber = "MSCIST67678";
			additionalDocuments2.CSI_Description = "19/02/2014";
			additionalDocuments2.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;

			var additionalDocument3 = nctsDepartureCargoDesc2.AdditionalInfos.AddNew();
			additionalDocument3.CSI_Code = "713";
			additionalDocument3.CSI_ReferenceNumber = "MSCIST67678";
			additionalDocument3.CSI_Description = "19/02/2014";
			additionalDocument3.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
		}
	}
}
