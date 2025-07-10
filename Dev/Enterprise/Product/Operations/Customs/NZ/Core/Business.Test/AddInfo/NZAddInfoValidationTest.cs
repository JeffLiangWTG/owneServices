using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.MasterFiles.Business;

	internal class TestNZAddInfoValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckZN_IsZeroRatedAll()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			string flagMessage = NZAddInfoValidation.MessageErrorMustHaveValidZeroRatedAllFlag;

			addInfo.ZN_IsZeroRatedAll = "";
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = "B";
			AssertHasMessageError(addInfo.ZN_IsZeroRatedAllInfo, flagMessage);

			OrgHeader bondStore = OrgHeader.New(Factory);
			bondStore.OH_Code = "ZZAKBOND";
			bondStore.OH_FullName = "AUCKLAND BOND STORE";
			bondStore.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			bondStore.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			bondStore.OH_RL_NKClosestPort = "NZAKL";

			OrgCusCode cusCode = bondStore.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = bondStore.MainAddress.PK;
			cusCode.OK_CustomsRegNo = "1234Z";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			declaration.WarehouseDocAddress.E2_OA_Address = bondStore.MainAddress.PK;

			addInfo.ZN_IsZeroRatedAll = "";
			AssertHasMessageError(addInfo.ZN_IsZeroRatedAllInfo, NZAddInfoValidation.MessageErrorMustHaveZeroRatedAllFlagWhenBondedWarehouseSet);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = "B";
			AssertHasMessageError(addInfo.ZN_IsZeroRatedAllInfo, flagMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			addInfo.ZN_IsZeroRatedAll = "";
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
			addInfo.ZN_IsZeroRatedAll = "B";
			AssertNoMessageErrors(addInfo.ZN_IsZeroRatedAllInfo);
		}

		public void TestCheckZN_GoodsLocatedAtForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			AssertNoMessageErrors(addInfo.ZN_GoodsLocatedAtInfo);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			addInfo.ZN_GoodsLocatedAt = "";
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationRequiredForAirExport);

			addInfo.ZN_GoodsLocatedAt = "XX";
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, "The code you have selected is not in the list.");

			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, "Bonded Warehouse selected for Goods Location has not been entered.\r\nFor Air - Must be entered to state the cargo terminal operator / consolidator / freight forwarder responsible for export loading.");
		}

		public void TestCheckSoldOrConsigned()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			addInfo.ZN_SoldOrConsigned = ZString.Empty;
			AssertNoMessageErrors(addInfo.ZN_SoldOrConsignedInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			addInfo.ZN_SoldOrConsigned = ZString.Empty;
			AssertHasMessageErrors(addInfo.ZN_SoldOrConsignedInfo);

			addInfo.ZN_SoldOrConsigned = TermsOfSaleList.Codes.Sold;
			AssertNoMessageErrors(addInfo.ZN_SoldOrConsignedInfo);
		}

		public void TestCheckZN_GoodsLocatedAtForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			AssertNoMessageErrors(addInfo.ZN_GoodsLocatedAtInfo);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			addInfo.ZN_GoodsLocatedAt = "";
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationRequiredForAir);

			addInfo.ZN_GoodsLocatedAt = "XX";
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, "The code you have selected is not in the list.");

			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, "Bonded Warehouse selected for Goods Location has not been entered.\r\nFor Air - Must be entered to state the cargo terminal operator / consolidator / freight forwarder where the goods are located.");

			var wareHouseOrg = Factory.New<OrgHeader>();
			wareHouseOrg.OH_Code = "WAREHOUSE";
			declaration.WarehouseDocAddress.E2_OA_Address = wareHouseOrg.MainAddress.PK;
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.PremiseIDNotEnteredOnOrg);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			addInfo.ZN_GoodsLocatedAt = "";
			AssertHasMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationRequiredForPost);

			var ctOrg = Factory.New<OrgHeader>();
			ctOrg.OH_Code = "CTCODE";
			ctOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1795K", Core.Constants.CountryCodes.NewZealand);

			var ctOrgAddr2 = ctOrg.Addresses.AddNew();
			ctOrgAddr2.OA_Address1 = "CTO alternate address";
			ctOrgAddr2.OA_Code = "CTO2";
			Factory.Save();

			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.ContainerTerminalOperatorDocAddress.OrganisationPK = ctOrg.PK;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrgAddr2.PK;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
			AssertNoMessageError("While an organisation has a CCP code NOT associated to any specific address, it is the default code for all addresses without specific CCP code", addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationAddressNotLinkedToCode);

			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrg.MainAddress.PK;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
			AssertNoMessageError("Default code is obtained also if CCP is associated to MainAddress", addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationAddressNotLinkedToCode);

			ctOrg.CustomsCodes[0].OK_OA_PremisesAddress = ctOrgAddr2.PK;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrg.MainAddress.PK;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
			AssertHasMessageError("While an organisation only has CCP codes associated with specific addresses not being MainAddress, specific addresses with CCP code must be chosen", addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationAddressNotLinkedToCode);

			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrgAddr2.PK;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
			AssertNoMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.LocationAddressNotLinkedToCode);
		}

		public void TestCheckZN_GoodsLocatedAtForImportSeaFinalDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			AssertNoMessageErrors(addInfo.ZN_GoodsLocatedAtInfo);

			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DES;
			AssertHasMessageError("Goods located at Final Destination - final destination not entered", addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.FinalDestinatonRequiredForGoodsLocation);
			declaration.JE_RL_NKFinalDestination = "FJNAD";

			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DES;
			AssertNoMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.FinalDestinatonRequiredForGoodsLocation);
			AssertHasMessageError("Goods located at Final Destination - final destination port must be a NZ port", addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.FinalDestinatonMustBeNZPort);

			declaration.JE_RL_NKFinalDestination = "NZWLG";
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DES;
			AssertNoMessageError(addInfo.ZN_GoodsLocatedAtInfo, NZAddInfoValidation.FinalDestinatonMustBeNZPort);
		}

		public void TestCheckConcessionCode_UseRefDb()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
				pivot.CI_TariffNum = "123456789";
				var addInfo = ((IHaveNZAddInfo)pivot).AddInfo;

				pivot.CI_ConcessionCode = "INVALID";
				AssertHasWarningContaining(addInfo.ZN_ConcessionCodeInfo, "Concession Code [INVALID] not recognized");
				pivot.CI_ConcessionCode = "100001A";
				AssertNoWarnings(addInfo.ZN_ConcessionCodeInfo);
			}
		}

		public void TestRL_NKProcessingPortValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			declaration.JE_RL_NKProcessingPort = ZString.Empty;
			AssertHasMessageError(declaration.JE_RL_NKProcessingPortInfo, NZAddInfoValidation.ExciseEntryMissingProcessingPort);
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_RL_NKProcessingPortInfo.ClearAllNotifications();
				AssertNoNotifications(declaration.JE_RL_NKProcessingPortInfo);
				declaration.RunPreSaveValidation();
				AssertHasMessageError(declaration.JE_RL_NKProcessingPortInfo, NZAddInfoValidation.ExciseEntryMissingProcessingPort);
			}
		}

		public void TestCheckZN_TransactionNature()
		{
			var declaration = Factory.New<JobDeclaration>();
			NZAddInfo addInfo = ((IHaveNZAddInfo)declaration).AddInfo;
			AssertNoMessageErrors("Export entry should not validate this", addInfo.ZN_TransactionNatureInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageErrors("Import entry when TSW not on should not validate this either", addInfo.ZN_TransactionNatureInfo);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			addInfo.ZN_TransactionNature = "";
			AssertNoMessageErrors("Non TSW Import entry should not validate this either", addInfo.ZN_TransactionNatureInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			addInfo.ZN_TransactionNature = "";
			AssertHasMessageError("Import entry when TSW turned on should now validate this field - requires entry", addInfo.ZN_TransactionNatureInfo, NZAddInfoValidation.NatureOfTransactionRequired);

			addInfo.ZN_TransactionNature = "85";
			AssertHasMessageError("Import entry - Invalid Nature of Transaction code", addInfo.ZN_TransactionNatureInfo, NZAddInfoValidation.NatureOfTransactionInvalid);

			addInfo.ZN_TransactionNature = NatureOfTransactionList.Codes.N10;
			AssertNoMessageErrors("Import entry with valid Nature of Transaction code", addInfo.ZN_TransactionNatureInfo);
		}

		public void TestCheckZN_RN_NKCountryOfOrigin()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			NZAddInfo addInfo = ((IHaveNZAddInfo)partPivot).AddInfo;
			AssertNoErrors("System should not validate Country/Region of Origin if it is not entered", addInfo.ZN_RN_NKCountryOfOriginInfo);

			addInfo.ZN_RN_NKCountryOfOrigin = "XX";
			AssertHasErrors("List validation of Country/Region of Origin should occur when entered", addInfo.ZN_RN_NKCountryOfOriginInfo);

			addInfo.ZN_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertNoErrors("List validation of Country/Region of Origin should pass with valid country/region code", addInfo.ZN_RN_NKCountryOfOriginInfo);

			var supplierPart = Factory.New<MasterFiles.OrgSupplierPart>();
			var pivot = supplierPart.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountryOfOrigin = "XX";
			AssertHasErrors("List validation of Country/Region of Origin should occur when entered", pivot.CI_RN_NKCountryOfOriginInfo);

			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			AssertNoErrors("List validation of Country/Region of Origin should pass with valid country code", pivot.CI_RN_NKCountryOfOriginInfo);
		}

		#region Implementation

		#region Container
		protected CusContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Declaration.CusContainers.AddNew();
				}
				return fContainer;
			}
		}
		CusContainer fContainer;
		#endregion

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#endregion
	}
}
