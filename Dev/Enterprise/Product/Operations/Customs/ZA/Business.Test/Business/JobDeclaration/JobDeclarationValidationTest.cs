using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public override void TestCheckJE_OH_ImporterUsingCustomsRule()
		{
			// Please modify this test when Customs Rule applies to ZA Customs.
			var declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.CustomsRule);
		}

		public void TestCheckJE_OH_AgentOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions("CheckJE_OH_AgentOverride", () =>
			{
				declaration.JE_OH_AgentOverride = ZGuid.Empty;
				AssertHasMessageErrorContaining(declaration.JE_OH_AgentOverrideInfo, MandatoryValidation.YouHaveNotEntered);
				OrgHeader agent2 = OrgHeader.New(Factory);
				agent2.SetAgentCode(declaration.Branch.Country, "123");
				declaration.JE_OH_AgentOverride = agent2.PK;
				AssertNoMessageErrorContaining(declaration.JE_OH_AgentOverrideInfo, "The selected agent does not have an Agent Code.");
				OrgHeader agent = OrgHeader.New(Factory);
				declaration.JE_OH_AgentOverride = agent.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_AgentOverrideInfo, "The selected agent does not have an Agent Code.");
				AssertNoMessageErrorContaining(declaration.JE_OH_AgentOverrideInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestJE_ApplicationCode()
		{
			CombineAssertions("Check JE_ApplicationCode", () =>
			{
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertHasError("JE_ApplicationCode is blank", declaration.JE_ApplicationCodeInfo, "You must select a Message Mode.");
				declaration.JE_ApplicationCode = "123";
				AssertHasMessageError("JE_ApplicationCode does not exit in the field Master list", declaration.JE_ApplicationCodeInfo, "The code you have selected is not in the list.");
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				AssertNoMessageErrors("JE_ApplicationCode is BLT", declaration.JE_ApplicationCodeInfo);
			});
		}

		public void TestCheckJE_MessageType_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_MessageTypeInfo);
		}

		public void TestCheckJE_MessageType_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_MessageTypeInfo, "123", ZAJobMessageTypeList.Codes.Import);
		}

		public void TestCheckJE_TransportMode()
		{
			CombineAssertions("Check JE_TransportMode", () =>
			{
				var errorMessage = "Enter a valid Mode of Transportation.";
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = ZString.Empty;
				AssertNoMessageErrors("IMP, transport field is blank", declaration.JE_TransportModeInfo);
				declaration.JE_TransportMode = "123";
				AssertHasError("IMP, transport field does not exist in the field Master list", declaration.JE_TransportModeInfo, errorMessage);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertNoMessageErrors("IMP, transport field exists in the field Master list", declaration.JE_TransportModeInfo);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = ZString.Empty;
				AssertNoMessageErrors("EXP, transport field is blank", declaration.JE_TransportModeInfo);
				declaration.JE_TransportMode = "123";
				AssertHasError("EXP, transport field does not exit in the field Master list", declaration.JE_TransportModeInfo, errorMessage);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertNoMessageErrors("EXP, transport field exists in the field Master list", declaration.JE_TransportModeInfo);
			});
		}

		public void TestCheckJE_CustomsOffice()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			Factory.Save();
			declaration.JE_CustomsOffice = ZString.Empty;
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, "You have not entered a Customs Office.");
			declaration.JE_CustomsOffice = "XXX";
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, "The code you have selected is not in the list.");
			declaration.JE_CustomsOffice = "JHB";
			AssertNoNotifications(declaration.JE_CustomsOfficeInfo);
		}

		public void TestCheckJE_ValuationDate()
		{
			const string requiredMessage = "Exchange Rate Date is required";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				declaration.Validation.ValidateAll();
				AssertHasErrorContaining("Import without ValuationDate", declaration.JE_ValuationDateInfo, requiredMessage);

				declaration.JE_ValuationDate = ZDate.Today;
				AssertNoErrorContaining("Import with ValuationDate", declaration.JE_ValuationDateInfo, requiredMessage);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_ValuationDate = ZDate.Empty;
				AssertNoErrorContaining("Export without ValuationDate", declaration.JE_ValuationDateInfo, requiredMessage);
			});
		}

		public void TestCheckJE_OH_Supplier()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions("Check JE_OH_Supplier", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_OH_Supplier = ZGuid.Empty;
				AssertHasMessageError("supplier is blank", declaration.JE_OH_SupplierInfo, "You have not entered a Supplier.");
				declaration.JE_OH_Supplier = supplier.PK;
				AssertNoMessageErrors("IMP, supplier is not blank", declaration.JE_OH_SupplierInfo);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = ZGuid.Empty;
				AssertHasMessageError("EXP, supplier is blank", declaration.JE_OH_SupplierInfo, "You have not entered a Supplier.");
				declaration.JE_OH_Supplier = supplier.PK;
				AssertNoMessageError("EXP, supplier is not blank", declaration.JE_OH_SupplierInfo, "You have not entered a Supplier.");
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Supplier = ZGuid.Empty;
				AssertNoMessageErrors("EXW, supplier is blank", declaration.JE_OH_SupplierInfo);
				declaration.JE_OH_Supplier = supplier.PK;
				AssertNoMessageErrors("EXW, supplier is not blank", declaration.JE_OH_SupplierInfo);
			});
		}

		public void TestCheckJE_LocationOfGoods()
		{
			declaration.JE_LocationOfGoods = ZString.Empty;
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.Export };
			var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					declaration.Validation.ValidateJE_LocationOfGoods();
					AssertHasMessageError(declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.LocationOfGoodsMandatory);
				}
			}

			var districtOffice = Factory.New<ZZRefCusCodeListCombined>();
			districtOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			districtOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			districtOffice.ZZD_Code = "S#2";
			districtOffice.ZZD_StartDate = ZDateTime.Today;
			districtOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var facility1 = Factory.New<ZZRefCusCodeListCombined>();
			facility1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			facility1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility1.ZZD_Code = "S#";
			facility1.ZZD_StartDate = ZDateTime.Today;
			facility1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var attribute = facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.DistrictOffices, "S#2");
			var facility2 = Factory.New<ZZRefCusCodeListCombined>();
			facility2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			facility2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility2.ZZD_Code = "S%";
			facility2.ZZD_StartDate = ZDateTime.Today;
			facility2.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.DistrictOffices, "S%2");
			var facility3 = Factory.New<ZZRefCusCodeListCombined>();
			facility3.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			facility3.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			facility3.ZZD_Code = "S*";
			facility3.ZZD_StartDate = ZDateTime.Today;
			facility3.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			facility3.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.DistrictOffices, "S&3");
			Factory.Save();
			declaration.JE_CustomsOffice = "S#2";
			declaration.JE_LocationOfGoods = "S#";
			var messageError = "Enter a valid Location Of Goods.";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, messageError);
			AssertNoWarnings(declaration.JE_LocationOfGoodsInfo);
			declaration.JE_LocationOfGoods = "S%";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, messageError);
			AssertHasWarning(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice("S%", "S#2"));
			declaration.JE_CustomsOffice = "S%2";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, messageError);
			AssertNoWarnings(declaration.JE_LocationOfGoodsInfo);
			declaration.JE_LocationOfGoods = "S*";
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, messageError);
			AssertHasWarning(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice("S*", "S%2"));
			declaration.JE_LocationOfGoods = "S^";
			AssertHasMessageError(declaration.JE_LocationOfGoodsInfo, messageError);
			AssertNoWarnings(declaration.JE_LocationOfGoodsInfo);
			attribute.ZZE_ZXE_NKName = "D!D";
			var attribute2 = facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.DistrictOffices, "S#3");
			declaration.JE_CustomsOffice = "S#2";
			declaration.JE_LocationOfGoods = "S#";
			AssertHasWarning(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice("S#", "S#2"));
			declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageError(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsNotForContainerisedCargo);
			facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.DepotType, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Containerised);
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsNotForContainerisedCargo);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var typelist = helper.CreateCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var list = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "S!", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(list.PK, RefCusCodeListAttributeTypes.Codes.DistrictOffices, "S!3");
			helper.CreateTransportModeForCusCodeList(list.PK, Core.Constants.TransportModes.Air);
			Factory.Save();
			declaration.JE_LocationOfGoods = "S!";
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageError(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsNotForTransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsNotForTransportMode);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageError(declaration.JE_LocationOfGoodsInfo, ValidationConstants.Declaration.LocationOfGoodsNotForTransportMode);
		}

		public void TestMessageErrorIfContainerisedCargoAndDepotButInvalidCCP()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			OrgAddress depotAddress = GetNewDepotAddressWithCCP("555");
			AssertEquals(false, depotAddress.DepotLocalControlledPremisesID.IsEmpty);
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			AssertHasMessageErrors(declaration.DepotDocAddress.E2_OA_AddressInfo);
		}

		public void TestMessageErrorForInvalidDepot()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.DepotDocAddress.E2_OA_Address = GetNewDepotAddressWithCCP("5BB55").PK;
			AssertHasMessageErrors(declaration.DepotDocAddress.E2_OA_AddressInfo);
		}

		public void TestDepotCCPLongerThanTwoCharactersHasMessageError()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.DepotDocAddress.E2_OA_Address = GetNewDepotAddressWithCCP("").PK;
			AssertNoMessageError(declaration.DepotDocAddress.E2_OA_AddressInfo, "Invalid CPD (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.");
			declaration.DepotDocAddress.E2_OA_Address = GetNewDepotAddressWithCCP("555").PK;
			AssertHasMessageError(declaration.DepotDocAddress.E2_OA_AddressInfo, "Invalid CPD (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.");
		}

		public void TestCheckJE_CarrierCode()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.Export };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				declaration.JE_CarrierCode = ZString.Empty;
				switch (shipmentType)
				{
					case ZAJobMessageTypeList.Codes.Import:
						AssertHasMessageError(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_CarrierCodeInfo.HumanReadableName.ToString()));
						break;
					case ZAJobMessageTypeList.Codes.Export:
						AssertNoMessageError(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_CarrierCodeInfo.HumanReadableName.ToString()));
						break;
				}

				declaration.JE_CarrierCode = "0000";
				AssertHasMessageError(declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_CarrierCode = ZString.Empty;
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_CarrierCodeInfo.HumanReadableName.ToString()));
			declaration.JE_CarrierCode = "0000";
			AssertNoMessageError(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_CarrierCodeInfo.HumanReadableName.ToString()));
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.Export };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				declaration.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageError(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_RL_NKOriginInfo.HumanReadableName.ToString()));
				declaration.JE_RL_NKOrigin = "ZZZZZ";
				AssertHasMessageError(declaration.JE_RL_NKOriginInfo, ListValidation.InvalidCodeMessageError);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertNoMessageError(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_RL_NKOriginInfo.HumanReadableName.ToString()));
			declaration.JE_RL_NKOrigin = "ZZZZZ";
			AssertNoMessageError(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.JE_RL_NKOriginInfo.HumanReadableName.ToString()));
		}

		public void TestCheckJE_GoodsOrigin()
		{
			declaration.JE_GoodsOrigin = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_GoodsOriginInfo);
			declaration.JE_GoodsOrigin = "!@";
			AssertHasMessageErrorContaining(declaration.JE_GoodsOriginInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(declaration.JE_GoodsOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestNoMessageErrorWhenEmptyMasterBillAndExWarehouse()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_MasterBillInfo);
		}

		public void TestNoMessageErrorWhenEmptyMasterBillAndIMX()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_MasterBillInfo);
		}

		public void TestCheckJE_MasterBillForAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "12345678";
			AssertNoWarningContaining(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillShouldNotPrefixByThreeZeros);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "00012345";
			AssertHasWarningContaining(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillShouldNotPrefixByThreeZeros);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "00012345";
			AssertNoWarningContaining(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillShouldNotPrefixByThreeZeros);
		}

		public void TestCheckJE_TotalWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TotalWeight = 0;
			AssertHasMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TotalWeight = 10;
			AssertNoMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TotalWeight = 0;
			AssertHasMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TotalWeight = 10;
			AssertNoMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_TotalWeight = 0;
			AssertNoMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TotalWeight = 10;
			AssertNoMessageErrorContaining(declaration.JE_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUnregisteredTrader()
		{
			var gb = RefCountry.LoadFromCountryCode(Factory, "GB");
			OrgHeader importer = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode);
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "");
			importer.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gb, "");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.TaxFileCode, "");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "QWERTY123");
			importer.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gb, "ASDFGH456");
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, false))
			{
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDOrGTXCode("Importer"));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
			{
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDOrGTXCode("Importer"));
				importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "");
				importer.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gb, "");
				declaration.Validation.ValidateJE_OH_Importer();
				AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDPassportOrGTXCode("Importer"));
				importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "QWERTY123");
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDPassportOrGTXCode("Importer"));
				importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "");
				importer.SetCustomsCode(OrgCusCode.CodeTypes.PassportID, gb, "ASDFGH456");
				declaration.Validation.ValidateJE_OH_Importer();
				AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDPassportOrGTXCode("Importer"));
			}
		}

		public void TestMarksAndNumbers_BlankLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			const string expectedMessage = "Too many Line Feed and Carriage Return characters in this text field are known to cause EDI Gateway errors";
			CombineAssertions(() =>
			{
				// MarksAndNumbers stored in StmNote which is set from JE_MarksAndNumbers and partially displayed on the screen as JE_MarksAndNumbersShort, since the GUI is bound to StmNote when using "More"
				// the bindings & validation refresh can only be triggered in the GUI when the StmNote screen is closed hence the validation call in this test
				declaration.JE_MarksAndNumbers = "\nStarting with a line feed\nshould result\nin a message error";
				declaration.Validation.ValidateJE_MarksAndNumbersShort();
				AssertHasMessageError("With Line Feed", declaration.JE_MarksAndNumbersShortInfo, expectedMessage);
				declaration.JE_MarksAndNumbers = "\rStarting with a carriage return";
				declaration.Validation.ValidateJE_MarksAndNumbersShort();
				AssertHasMessageError("With carriage return", declaration.JE_MarksAndNumbersShortInfo, expectedMessage);
				declaration.JE_MarksAndNumbers = "\r\nStarting with a carriage return and line feed";
				declaration.Validation.ValidateJE_MarksAndNumbersShort();
				AssertHasMessageError("With CR LF", declaration.JE_MarksAndNumbersShortInfo, expectedMessage);
				declaration.JE_MarksAndNumbers = "Starting without a blank line\nshould not result\nin a message error";
				declaration.Validation.ValidateJE_MarksAndNumbersShort();
				AssertNoMessageError("Without Blank Line", declaration.JE_MarksAndNumbersShortInfo, expectedMessage);
			});
		}

		public void TestCheckJE_Carrier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_Carrier = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_Carrier = "#$$#";
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertHasMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_Carrier = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_Carrier = "#$$#";
			AssertNoMessageErrorContaining(declaration.JE_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_CarrierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_RadioCallSign()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_VesselName = "Black Pearl";
			declaration.JE_RadioCallSign = "JDK";
			AssertNoMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_VesselName = string.Empty;
			AssertEquals(ZString.Empty, declaration.JE_RadioCallSign);
			AssertHasMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertNoMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertHasMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			AssertNoMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoMessageErrorContaining(declaration.JE_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJE_RL_NKMasterBillIssuedAtHasWarningWhenItsCountryDiffersFromCountryOfOrigin()
		{
			RefUNLOCO originNLOCO = Factory.New<RefUNLOCO>();
			originNLOCO.RL_Code = "ZAAM";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKOrigin = originNLOCO.RL_Code;
			declaration.JE_RL_NKMasterBillIssuedAt = "XXXX";
			AssertHasWarnings(declaration.JE_RL_NKMasterBillIssuedAtInfo);
		}

		public void TestMasterBillIssuedAtWhenImportOrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			string[] messageTypes = new string[] { ZAJobMessageTypeList.Codes.Export, ZAJobMessageTypeList.Codes.Import };
			foreach (string messageType in messageTypes)
			{
				declaration.JE_MessageType = messageType;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "1";
				declaration.JE_RL_NKMasterBillIssuedAt = "";
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertNoMessageErrors(declaration.JE_RL_NKMasterBillIssuedAtInfo);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertNoMessageErrors(declaration.JE_RL_NKMasterBillIssuedAtInfo);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertNoMessageErrors(declaration.JE_RL_NKMasterBillIssuedAtInfo);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

				declaration.JE_TransportMode = "";
				declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
				AssertNoMessageErrors(declaration.JE_RL_NKMasterBillIssuedAtInfo);
			}
		}

		public void TestMasterBillIssuedDateWhenImportOrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			string[] messageTypes = new string[] { ZAJobMessageTypeList.Codes.Export, ZAJobMessageTypeList.Codes.Import };
			string[] transportModes = new string[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Road,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Mail
			};
			foreach (string messageType in messageTypes)
			{
				foreach (string transpotMode in transportModes)
				{
					declaration.JE_MessageType = messageType;
					declaration.JE_TransportMode = transpotMode;
					declaration.JE_MasterBill = "1";
					declaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
					AssertHasMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedDateCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));

					declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 6, 16);
					AssertNoMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedDateCannotBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
				}
			}
		}

		public void TestMasterCargoCarrierCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var carrierCode = helper.CreateCarrierCode("BHPB", "BHP Billiton", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, RefCarrierTypeList.Codes.Master, RefCarrierTypeList.Codes.Master);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Sea);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_CarrierCode = "XXXX";
			AssertNoNotifications(declaration.JE_CarrierCodeInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CarrierCode = "BHPB";
			AssertNoNotifications(declaration.JE_CarrierCodeInfo);
			declaration.JE_CarrierCode = "XXXX";
			AssertHasMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CarrierCode = "";
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);
			declaration.JE_CarrierCode = "BHPB";
			AssertNoNotifications(declaration.JE_CarrierCodeInfo);
		}

		public void TestCheckJE_VATClaimBackIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_VATClaimBackIndicator = "";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_VATClaimBackIndicator = "N";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_VATClaimBackIndicator = "Y";
			AssertNoMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_VATClaimBackIndicator = "A";
			AssertHasMessageErrorContaining(declaration.JE_VATClaimBackIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_ROOType()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry("EUR");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ROOCert = "123";
			AssertHasMessageErrorContaining(declaration.JE_ROOTypeInfo, ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
			declaration.JE_ROOType = "@@";
			AssertHasMessageErrorContaining(declaration.JE_ROOTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ROOType = "EUR";
			AssertNoNotifications(declaration.JE_ROOTypeInfo);
		}

		public void TestCheckJE_Trailer2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_Trailer1 = ZString.Empty;
			declaration.JE_Trailer2 = "AAA";
			AssertHasMessageError(declaration.JE_Trailer2Info, JobDeclarationValidation.Trailer1NotEntered);
			declaration.JE_Trailer2 = ZString.Empty;
			AssertNoMessageError(declaration.JE_Trailer2Info, JobDeclarationValidation.Trailer1NotEntered);
		}

		public void TestCountryOfOrigin()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var originNLOCO = Factory.New<RefUNLOCO>();
			originNLOCO.RL_Code = "ZAAM";
			declaration.JE_RL_NKOrigin = originNLOCO.RL_Code;
			var countryOfOrigin = declaration.Validation.MessageKeyFactor.CountryOfOrigin;
			AssertEquals("ZA", countryOfOrigin.Code);
		}

		public void TestCheckJE_RemovalTransportCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_RemovalTransportCode = ZString.Empty;

			Factory.Save();

			declaration.Validation.ValidateJE_RemovalTransportCode();

			AssertHasWarning("Should have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");

			declaration.JE_RL_NKFinalDestination = "DEFRA";
			declaration.Validation.ValidateJE_RemovalTransportCode();
			AssertNoWarning("Should NOT have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");

			declaration.JE_RL_NKFinalDestination = "LSMSU";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.Validation.ValidateJE_RemovalTransportCode();
			AssertHasWarning("Should have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_RemovalTransportCode();
			AssertNoWarning("Should NOT have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_RemovalTransportCode();
			AssertHasWarning("Should have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");

			declaration.JE_RemovalTransportCode = "A";
			declaration.Validation.ValidateJE_RemovalTransportCode();
			AssertNoWarning("Should NOT have warning", declaration.JE_RemovalTransportCodeInfo, "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");
		}

		public void TestCheckJE_RL_NKMasterBillIssuedAt()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKMasterBillIssuedAt = "CNGG";
			AssertHasNotifications(declaration.JE_RL_NKMasterBillIssuedAtInfo);
			declaration.JE_RL_NKMasterBillIssuedAt = "CNXGK";
			AssertNoNotifications(declaration.JE_RL_NKMasterBillIssuedAtInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		new JobDeclaration declaration;

		OrgAddress GetNewDepotAddressWithCCP(string cCP)
		{
			OrgHeader depotOrg = OrgHeader.New(Factory);
			OrgAddress depotAddress = depotOrg.Addresses.AddNew();
			OrgCusCode cusCode = depotOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusCode.OK_CustomsRegNo = cCP;
			cusCode.OK_OA_PremisesAddress = depotAddress.PK;
			return depotAddress;
		}
	}
}
