using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	sealed class ProviderTestHelper : IDisposable
	{
		public ProviderTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			Setup();
		}
		readonly BusinessObjectFactory factory;
		GlbStaff loggedInUser;
		IDisposable tempUserContext;

		void Setup()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "TR", "052", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "KR", "728", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			factory.Save();

			loggedInUser = factory.NewWithValidTestData<GlbStaff>();
			loggedInUser.GS_Code = "ULU";
			loggedInUser.GS_FullName = "test ulukom";

			tempUserContext = Env.SetTemporaryUserContext(new UserContext(loggedInUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		void IDisposable.Dispose()
		{
			tempUserContext.Dispose();
		}

		public AsycudaManifestHeader GetProviderHeader()
		{
			var header = factory.New<AsycudaManifestHeader>();

			header.AMA_JobReference = "MAN0000001";
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = "DENITH";
			header.TransportType = "10";
			header.AMA_Nature = "IMP";
			header.AMA_AgentType = "AGT";
			header.AMA_ContainerMode = "BBK";
			header.AMA_VesselName = "VESSEL NO";
			header.AMA_LloydsNumber = "IMO NO";
			header.AMA_Voyage = "VOYAGE";
			header.AMA_RN_NKConveyanceNationality = "KR";
			header.AMA_RL_NKPortOfLoading = "TRALI";
			header.AMA_CustomsLoadPort = "TRALI-007";
			header.TR_GM_PresentationCustomsOffice = "TR041600";
			header.AMA_MasterBill = "BOL";
			header.AMA_RL_NKPortOfDischarge = "TRALA";
			header.AMA_CustomsDischargePort = "TRMRA-009";
			header.TIRNumber = "TIR NO";
			header.AMA_CustomsOffice = "TR042200";
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.AMA_Trailer1RegNo = "34 XXX 123";
			header.AMA_RN_NKTrailer1RegCountry = "TR";
			header.AMA_Trailer2RegNo = "06 XXX 456";
			header.AMA_RN_NKTrailer2RegCountry = "TR";
			header.AMA_ManifestDescription = "Test Message for manifest descriptions";

			var glbBranch = factory.NewWithValidTestData<GlbBranch>();
			var glbCompany = factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			glbCompany.CompanyName = "TestName";
			glbCompany.Address1 = "Test adress 1";
			glbCompany.Address2 = "Test adress 2";
			glbCompany.GC_BusinessRegNo = "test567890";
			header.AMA_GB = glbBranch.PK;
			glbCompany.Branches.Add(glbBranch);

			var extPassword = factory.New<GlbExternalPassword_TR>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TRK;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_UserID = "1234test12";
			extPassword.GP_GS = loggedInUser.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var orgCarrier = factory.New<OrgHeader>();
			orgCarrier.OH_Code = "XYZAAA";
			orgCarrier.OH_FullName = "xCarrier Company Name";
			orgCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890");
			var addressCarrier = orgCarrier.MainAddress;
			addressCarrier.OA_OH = orgCarrier.PK;
			addressCarrier.CompanyName = "xCarrier Company Name";
			addressCarrier.Address1 = "xAdress1";
			addressCarrier.Address2 = "xAdress2";
			addressCarrier.OA_Phone = "02122122692";
			addressCarrier.OA_Fax = "02122122692";
			addressCarrier.City = "IST";
			addressCarrier.Postcode = "340300";
			addressCarrier.OA_RN_NKCountryCode = "TR";
			header.AMA_OA_Carrier = addressCarrier.PK;

			#region Bill1
			var bill1 = header.Bills.AddNew();
			var orgContainerAgent = factory.New<OrgHeader>();
			orgContainerAgent.OH_Code = "TEYT";
			orgContainerAgent.OH_FullName = "xContainer Agent Name";
			orgContainerAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567892");
			var addressContainerAgent = orgContainerAgent.MainAddress;
			bill1.ABL_OA_ContainerAgent = addressContainerAgent.PK;
			bill1.ABL_ConsigneeName = "xConsignee Name";
			bill1.ABL_ConsigneeRegNo = "1234567890";
			bill1.IsToOrder = true;
			bill1.NotOwned = false;
			bill1.ABL_NotifyPartyName = "xNotify Name";
			bill1.ABL_NotifyPartyRegNo = "1234567892";
			bill1.ABL_RL_NKOrigin = "TRIST";
			bill1.ABL_LocationInformation = "GEMI";
			bill1.ABL_RX_NKFreightValueCurrency = "USD";
			bill1.ABL_FreightValue = 1500;
			bill1.ABL_ShipperName = "xShipper Name";
			bill1.ABL_BillNumber = "ABC111222333";
			bill1.ABL_ShipperRegNo = "1234567893";
			bill1.ABL_RX_NKTransportValueCurrency = "EUR";
			bill1.ABL_TransportValue = 500;
			bill1.PaymentType = "B";
			bill1.CustomsEntryNumber = "55555";
			bill1.TransshipmentType = "1";

			var refContainer = factory.New<RefContainer>();
			refContainer.RC_Code = "40DC";
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CNTR000001";
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_Seal1 = "Seal 1";
			container.Relation = "Local";
			container.ACN_EmptyFullIndicator = "MT";
			var pack = bill1.Packs.AddNew();
			pack.APA_PackQty = 15;
			pack.APA_LineNo = 1;
			pack.ContainerPK = container.PK;

			var packedItems = pack.PackedItems.AddNewPackedItem();
			packedItems.API_GrossWeight = Convert.ToDecimal(50);
			packedItems.API_Tariff = "123411223344";
			packedItems.API_FormattedTariff = "1234.12.12.12.12";
			packedItems.API_GoodsDescription = "xGoods Description";
			packedItems.API_RX_NKGoodsValueCurrency = "EUR";

			var uNDGs = packedItems.UNDGs.AddNew();
			uNDGs.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

			var packedItems2 = pack.PackedItems.AddNewPackedItem();
			packedItems2.API_GrossWeight = Convert.ToDecimal(60);
			packedItems2.API_Tariff = "222211223344";
			packedItems2.API_FormattedTariff = "2222.12.12.12.12";
			packedItems2.API_GoodsDescription = "2 Goods Description";
			packedItems2.API_RX_NKGoodsValueCurrency = "USD";

			var uNDGs2 = packedItems2.UNDGs.AddNew();
			uNDGs2.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

			var relatedDeclarationforExport = bill1.RelatedDeclarationForExports.AddNew();
			relatedDeclarationforExport.CSI_Quantity2 = Convert.ToDecimal(123.89);
			relatedDeclarationforExport.CSI_Quantity = Convert.ToInt32(20);
			relatedDeclarationforExport.CSI_ReferenceNumber = "340300IM123456";
			relatedDeclarationforExport.CSI_Procedure = "TCGB";
			#endregion

			#region Bill2
			var bill2 = header.Bills.AddNew();
			bill2.ABL_OA_ContainerAgent = addressContainerAgent.PK;
			bill2.ABL_ConsigneeName = "xConsignee Name";
			bill2.ABL_ConsigneeRegNo = "1234567890";
			bill2.IsToOrder = false;
			bill2.NotOwned = true;
			bill2.ABL_NotifyPartyName = "xNotify Name";
			bill2.ABL_NotifyPartyRegNo = "1234567892";
			bill2.ABL_RL_NKOrigin = "TRIST";
			bill2.ABL_LocationInformation = "GEMI";
			bill2.ABL_RX_NKFreightValueCurrency = "USD";
			bill2.ABL_FreightValue = 1500;
			bill2.ABL_ShipperName = "xShipper Name";
			bill2.ABL_BillNumber = "ABC111222333";
			bill2.ABL_ShipperRegNo = "1234567893";
			bill2.ABL_RX_NKTransportValueCurrency = "EUR";
			bill2.ABL_TransportValue = 500;
			bill2.PaymentType = "B";
			bill2.CustomsEntryNumber = "55555";
			bill2.TransshipmentType = "1";

			var packedItems3 = pack.PackedItems.AddNewPackedItem();
			packedItems3.API_GrossWeight = Convert.ToDecimal(50);
			packedItems3.API_Tariff = "123411223344";
			packedItems3.API_FormattedTariff = "1234.12.12.12.12";
			packedItems3.API_GoodsDescription = "xGoods Description";
			packedItems3.API_RX_NKGoodsValueCurrency = "EUR";

			var uNDGs3 = packedItems.UNDGs.AddNew();
			uNDGs3.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

			var relatedDeclarationforExport2 = bill2.RelatedDeclarationForExports.AddNew();
			relatedDeclarationforExport2.CSI_Quantity2 = Convert.ToDecimal(123.89);
			relatedDeclarationforExport2.CSI_Quantity = Convert.ToInt32(20);
			relatedDeclarationforExport2.CSI_ReferenceNumber = "340300IM123456";
			relatedDeclarationforExport2.CSI_Procedure = "OZBY";
			#endregion

			#region Bill3
			var bill3 = header.Bills.AddNew();
			bill3.ABL_OA_ContainerAgent = addressContainerAgent.PK;
			bill3.ABL_ConsigneeName = "xConsignee Name";
			bill3.ABL_ConsigneeRegNo = "1234567890";
			bill3.IsToOrder = false;
			bill3.NotOwned = false;
			bill3.ABL_NotifyPartyName = "xNotify Name";
			bill3.ABL_NotifyPartyRegNo = "1234567892";
			bill3.ABL_RL_NKOrigin = "TRIST";
			bill3.ABL_LocationInformation = "GEMI";
			bill3.ABL_RX_NKFreightValueCurrency = "USD";
			bill3.ABL_FreightValue = 1500;
			bill3.ABL_ShipperName = "xShipper Name";
			bill3.ABL_BillNumber = "ABC111222333";
			bill3.ABL_ShipperRegNo = "1234567893";
			bill3.ABL_RX_NKTransportValueCurrency = "EUR";
			bill3.ABL_TransportValue = 500;
			bill3.PaymentType = "B";
			bill3.CustomsEntryNumber = "55555";
			bill3.TransshipmentType = "1";

			var packedItems4 = pack.PackedItems.AddNewPackedItem();
			packedItems4.API_GrossWeight = Convert.ToDecimal(50);
			packedItems4.API_Tariff = "123411223344";
			packedItems4.API_FormattedTariff = "1234.12.12.12.12";
			packedItems4.API_GoodsDescription = "xGoods Description";
			packedItems4.API_RX_NKGoodsValueCurrency = "EUR";

			var uNDGs4 = packedItems.UNDGs.AddNew();
			uNDGs4.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

			var relatedDeclarationforExport3 = bill3.RelatedDeclarationForExports.AddNew();
			relatedDeclarationforExport3.CSI_Quantity2 = Convert.ToDecimal(123.89);
			relatedDeclarationforExport3.CSI_Quantity = Convert.ToInt32(20);
			relatedDeclarationforExport3.CSI_ReferenceNumber = "340300IM123456";
			relatedDeclarationforExport3.CSI_Procedure = "OZBY";
			#endregion

			#region Bill4
			var bill4 = header.Bills.AddNew();
			var orgContainerAgent2 = factory.New<OrgHeader>();
			orgContainerAgent2.OH_Code = "TEYT2";
			orgContainerAgent2.OH_FullName = "xContainerAgentName2";
			var addressContainerAgent2 = orgContainerAgent2.MainAddress;
			bill4.ABL_OA_ContainerAgent = addressContainerAgent2.PK;
			bill4.ABL_ConsigneeName = "xConsignee Name2";
			bill4.ABL_ConsigneeRegNo = "1234567890";
			bill4.IsToOrder = true;
			bill4.NotOwned = false;
			bill4.ABL_NotifyPartyName = "xNotify Name";
			bill4.ABL_NotifyPartyRegNo = "1234567892";
			bill4.ABL_RL_NKOrigin = "TRIST";
			bill4.ABL_LocationInformation = "GEMI";
			bill4.ABL_RX_NKFreightValueCurrency = "USD";
			bill4.ABL_FreightValue = 1500;
			bill4.ABL_ShipperName = "xShipper Name";
			bill4.ABL_BillNumber = "ABC111222334";
			bill4.ABL_ShipperRegNo = "1234567893";
			bill4.ABL_RX_NKTransportValueCurrency = "EUR";
			bill4.ABL_TransportValue = 500;
			bill4.PaymentType = "B";
			bill4.CustomsEntryNumber = "55555";
			bill4.TransshipmentType = "1";

			var refContainer2 = factory.New<RefContainer>();
			refContainer2.RC_Code = "40DC";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "CNTR00002";
			container2.ACN_RC_ContainerType = refContainer.PK;
			container2.ACN_Seal1 = "Seal 2";
			container2.Relation = "Local";
			var pack2 = bill4.Packs.AddNew();
			pack2.APA_PackQty = 15;
			pack2.APA_LineNo = 1;
			pack2.ContainerPK = container.PK;

			var packedItems5 = pack2.PackedItems.AddNewPackedItem();
			packedItems5.API_GrossWeight = Convert.ToDecimal(50);
			packedItems5.API_Tariff = "123411223344";
			packedItems5.API_FormattedTariff = "1234.12.12.12.12";
			packedItems5.API_GoodsDescription = "xGoods Description";
			packedItems5.API_RX_NKGoodsValueCurrency = "EUR";

			var uNDGs5 = packedItems5.UNDGs.AddNew();
			uNDGs5.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

			var relatedDeclarationforExport4 = bill4.RelatedDeclarationForExports.AddNew();
			relatedDeclarationforExport4.CSI_Quantity2 = Convert.ToDecimal(123.89);
			relatedDeclarationforExport4.CSI_Quantity = Convert.ToInt32(20);
			relatedDeclarationforExport4.CSI_ReferenceNumber = "340300IM123456";
			relatedDeclarationforExport4.CSI_Procedure = "TCGB";
			#endregion

			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			#region VisitedPorts
			var visitedPorts = header.VisitedPorts.AddNew();
			visitedPorts.CY_Code = "TRIST";
			visitedPorts.CY_Date = ZDateTime.Today;
			#endregion

			#region OpenManifest
			var manifestToOpenList1 = header.ManifestsToOpenList.AddNew();
			manifestToOpenList1.CSI_ParentID = header.PK;
			manifestToOpenList1.CSI_SubType = SubTypeListForManifestToOpen.Codes.Manifestlevel;
			manifestToOpenList1.CSI_Status = "N";
			manifestToOpenList1.CSI_ReferenceNumber2 = "manif111";
			manifestToOpenList1.CSI_Description = "Manifestlevel";

			var manifestToOpenList2 = header.ManifestsToOpenList.AddNew();
			manifestToOpenList2.CSI_ParentID = header.PK;
			manifestToOpenList2.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
			manifestToOpenList2.CSI_Status = "N";
			manifestToOpenList2.CSI_ReferenceNumber2 = "manif222";
			manifestToOpenList2.CSI_Description = "Billlevel";
			manifestToOpenList2.CSI_ReferenceNumber = "111111111";

			var manifestToOpenList3 = header.ManifestsToOpenList.AddNew();
			manifestToOpenList3.CSI_ParentID = header.PK;
			manifestToOpenList3.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
			manifestToOpenList3.CSI_Status = "N";
			manifestToOpenList3.CSI_ReferenceNumber2 = "manif333";
			manifestToOpenList3.CSI_Description = "Billlinelevel";
			manifestToOpenList3.CSI_ReferenceNumber = "222222222";
			manifestToOpenList3.CSI_CustomsOffice = "123456";
			manifestToOpenList3.CSI_Quantity2 = 200;
			manifestToOpenList3.CSI_LineNo = 1;
			#endregion

			return header;
		}
	}
}
