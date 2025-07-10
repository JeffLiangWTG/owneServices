using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobDeclarationValidationBaseOnlyTest : AddInfoJobDeclarationValidationAbstractTest
	{
		public void TestCheckUS_EntryTypeForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_EntryType = "!";
			AssertNoNotifications(declaration.US_EntryTypeInfo);
			declaration.US_EntryType = ZString.Empty;
			AssertNoNotifications(declaration.US_EntryTypeInfo);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoNotifications(declaration.US_EntryTypeInfo);
		}

		public void TestCheckUS_EntryTypeForInBond()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateTransportation;
			AssertHasMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_EnableSPN = true;
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateTransportation;
			AssertHasMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
			declaration.US_EntryTypeInfo.ClearAllNotifications();
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateTransportation;
			AssertNoMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_EnableSPN = false;
			declaration.US_SPNIDType = ZString.Empty;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AssertNoMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._61;
			AssertNoMessageError(declaration.US_EntryTypeInfo, USAddInfoValidation.InBondEntryTypeShouldBeEnteredInInBondType);
		}

		public void TestCheckUS_EntryTypeInvalidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "61", "TFTEA 1313(b)(p) - TFTEA Manufactured petroleum derivatives", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
			var message = "Please enter a valid Entry Type Code";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.BargeMovement;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateTransportation;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
		}

		public void TestCheckUS_FSISInspec()
		{
			var refCusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeList.ZZD_Code = "Code1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_FSISInspec = "XXX";
			AssertHasMessageErrorContaining(declaration.US_FSISInspecInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_InspecFirms()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Code", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InspecFirms = "XXX";
			AssertHasMessageErrorContaining(declaration.US_InspecFirmsInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_InspecFirms = "Code";
			AssertNoMessageErrorContaining(declaration.US_InspecFirmsInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_InspecPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InspecPort = "XXX";
			AssertHasMessageErrorContaining(declaration.US_InspecPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_InspecPort = "1234";
			AssertNoMessageErrorContaining(declaration.US_InspecPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_SchDExportExportValidationIsNotRun()
		{
			CreateTestLocoMapping();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_SchDExport = "";
			Declaration.US_RL_NKPortOfExport = "USLAX";
			Assert(!Declaration.US_SchDExportInfo.HasMessageErrors());
		}

		public void TestCheckUS_SchDLoading()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "24155", "DUMMY 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2792", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			SetMessageTypeAndTestCheckUS_SchDLoading(JobMessageTypeList.Codes.Import, "24155");
			SetMessageTypeAndTestCheckUS_SchDLoading(JobMessageTypeList.Codes.Export, "2792");
			SetMessageTypeAndTestCheckUS_SchDLoading(JobMessageTypeList.Codes.Miscellaneous, "24155");
		}

		public void TestSchDLoadingDoesNotHaveMultiPortsMessageWhenPortOfLoadingCleared()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "56507", "56507 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "56592", "56592 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2777", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2778", "TEST NAME", startDate, endDate);
			Factory.Save();

			CreateLocoMapIfNotExists("2777", "USLAX", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMapIfNotExists("2778", "USLAX", USLocoMapSystemUsageList.Codes.Sea, false);
			Factory.Save();
			var messageError = string.Format(AddInfoJobDeclarationValidation.MultipleMatches, Schedule.K);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.JE_RL_NKPortOfLoading = "PHLIM";
			Declaration.US_SchDLoading = "";
			AssertHasMessageError(Declaration.US_SchDLoadingInfo, messageError);
			Declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			Declaration.US_SchDLoading = "";
			AssertHasMessageError(Declaration.US_SchDLoadingInfo, messageError);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.US_SchDLoading = "";
			AssertNoMessageError(Declaration.US_SchDLoadingInfo, messageError);
			messageError = string.Format(AddInfoJobDeclarationValidation.MultipleMatches, Schedule.D);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_RL_NKPortOfLoading = "USLAX";
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.US_SchDLoading = "";
			AssertNoMessageError(Declaration.US_SchDLoadingInfo, messageError);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_SchDLoading = "";
			AssertHasMessageError(Declaration.US_SchDLoadingInfo, messageError);
			Declaration.JE_RL_NKPortOfLoading = "";
			AssertNoMessageError("US_SchDLoading should not keep multi port message if Multi Port of Loading is cleared", Declaration.US_SchDLoadingInfo, messageError);
		}

		public void TestCheckUS_SchDArrivalForSelectSpecificLoadingMsg()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1001", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1002", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1003", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1004", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1006", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1007", "TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1008", "TEST NAME", startDate, endDate);
			newFactory.Save();

			string loco1 = "UMULT";
			CreateLocoIfNotExists(loco1, "US");
			CreateLocoMapIfNotExists("1001", loco1, USLocoMapSystemUsageList.Codes.Sea, true);
			CreateLocoMapIfNotExists("1002", loco1, USLocoMapSystemUsageList.Codes.SCD, true);
			CreateLocoMapIfNotExists("1003", loco1, USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMapIfNotExists("1004", loco1, USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMapIfNotExists("1006", loco1, USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMapIfNotExists("1007", loco1, USLocoMapSystemUsageList.Codes.All, true);
			CreateLocoMapIfNotExists("1008", loco1, USLocoMapSystemUsageList.Codes.All, true);
			Factory.Save();
			string messageError = string.Format(AddInfoJobDeclarationValidation.MultipleMatches, Schedule.D);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, messageError);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_SchDArrival = "";
			Declaration.JE_RL_NKPortOfArrival = loco1;
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, messageError);
		}

		public void TestSchDArrivalDoesNotHaveMultiPortsMessageWhenPortOfArrivalCleared()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2973", "2973 TEST NAME", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2970", "2970 TEST NAME", startDate, endDate);
			newFactory.Save();

			CreateTestLocoMapping();
			string messageError = string.Format(AddInfoJobDeclarationValidation.MultipleMatches, Schedule.D);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, messageError);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_SchDArrival = "";
			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, messageError);
			Declaration.JE_RL_NKPortOfArrival = "";
			AssertNoMessageError("US_SchDArrival should not keep multi port message if Multi Port of Arrival is cleared", Declaration.US_SchDArrivalInfo, messageError);
		}

		public void TestCheckUS_7501Purchased()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_7501PurchasedInfo, "You have not entered a Purchased declaration, which is required for Entry Summary printing.");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_7501Purchased = "";
			AssertHasMessageError(Declaration.US_7501PurchasedInfo, "You have not entered a Purchased declaration, which is required for Entry Summary printing.");
			Declaration.US_7501Purchased = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(Declaration.US_7501PurchasedInfo, "You have not entered a Purchased declaration, which is required for Entry Summary printing.");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Declaration.US_7501Purchased = "";
			AssertNoMessageError(Declaration.US_7501PurchasedInfo, "You have not entered a Purchased declaration, which is required for Entry Summary printing.");
		}

		public void TestUS_SchKINBFinalForeignDest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "01", "Test", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SchKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.ScheduleKCodeInvalid);
			Declaration.US_SchKINBFinalForeignDest = "97101";
			AssertHasMessageError(Declaration.US_SchKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.ScheduleKCodeInvalid);
			Declaration.US_SchKINBFinalForeignDest = "01";
			AssertNoMessageError(Declaration.US_SchKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.ScheduleKCodeInvalid);
		}

		public void TestUS_SpecialKINBFinalForeignDest()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.SpecialKCodeInvalid);
			Declaration.US_SpecialKINBFinalForeignDest = "80115";
			AssertHasMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.SpecialKCodeInvalid);
			Declaration.US_SpecialKINBFinalForeignDest = "80112"; // Newfoundland
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.SpecialKCodeInvalid);
		}

		public void TestOneForeignDest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00013", "Common", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationRequired);
			Declaration.US_InbondType = EntryTypeList.Codes.ImmediateExportation;
			Declaration.US_SchKINBFinalForeignDest = "";
			AssertHasMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationRequired);
			Declaration.US_SchKINBFinalForeignDest = "00013";
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationRequired);
			Declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			Declaration.US_SchKINBFinalForeignDest = "";
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationRequired);
		}

		public void TestOnlyOneForeignDest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00013", "Common", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationOnlyOneRequired);
			Declaration.US_InbondType = EntryTypeList.Codes.ImmediateExportation;
			Declaration.US_SchKINBFinalForeignDest = "";
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationOnlyOneRequired);
			Declaration.US_SchKINBFinalForeignDest = "00013";
			AssertNoMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationOnlyOneRequired);
			Declaration.US_SpecialKINBFinalForeignDest = "80109";
			AssertHasMessageError(Declaration.US_SpecialKINBFinalForeignDestInfo, AddInfoJobDeclarationValidation.FinalForeignDestinationOnlyOneRequired);
		}

		public void TestSchDExportDoesNotHaveMultiPortsMessageWhenPortOfEportCleared()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2970", "2970 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2973", "2973 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			CreateTestLocoMapping();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SchDExportInfo, AddInfoJobDeclarationValidation.ExportMultipleMatches);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_SchDExport = "";
			Declaration.US_RL_NKPortOfExport = "USLAX";
			AssertHasMessageError(Declaration.US_SchDExportInfo, AddInfoJobDeclarationValidation.ExportMultipleMatches);
			Declaration.US_RL_NKPortOfExport = "";
			AssertNoMessageError("US_SchDExport should not keep multi port message if Multi Port of Export is cleared", Declaration.US_SchDExportInfo, AddInfoJobDeclarationValidation.ExportMultipleMatches);
		}

		public void TestUS_SchDArrivalDoNotNeedToBeEnteredWhenItIsAirExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "29213", "29213 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "29201", "29201 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			CreateTestLocoMapping();
			string messageError = string.Format(AddInfoJobDeclarationValidation.MultipleMatches, Schedule.K);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			Declaration.JE_RL_NKPortOfArrival = "BBBGI";
			Declaration.US_SchDArrival = ZString.Empty;
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, messageError);
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.AddInfoValidation.ValidateUS_SchDArrival();
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, messageError);
		}

		public void TestCheckUS_UI_NKCarrierSCAC_SCACMisMatchWarning()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "KDHS", Core.Constants.CountryCodes.UnitedStates);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.JE_OH_ShippingLine = org.PK;
			dec.US_UI_NKCarrierSCAC = "KDHS";
			AssertHasWarning(dec.US_UI_NKCarrierSCACInfo, "The value entered for Carrier SCAC is not the same as that recorded for the Carrier Organization entered ('ABCD')");
			dec.US_UI_NKCarrierSCAC = "ABCD";
			AssertNoWarnings(dec.US_UI_NKCarrierSCACInfo);
			dec.JE_TransportMode = TransportTypeList.Codes.Truck;
			dec.US_UI_NKCarrierSCAC = "KDHS";
			AssertNoWarnings(dec.US_UI_NKCarrierSCACInfo);
			dec.US_UI_NKCarrierSCAC = "ABCD";
			AssertHasWarning(dec.US_UI_NKCarrierSCACInfo, "The value entered for Carrier SCAC is not the same as that recorded for the Carrier Organization entered ('KDHS')");
		}

		public void TestCheckUS_UI_NKCarrierSCAC_SCAC_Drawback()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.US_UI_NKCarrierSCAC = "MPXX";
			AssertHasMessageErrorContaining(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCAC);
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_UI_NKCarrierSCAC = "MPXY";
			AssertNoMessageErrors(dec.US_UI_NKCarrierSCACInfo);
		}

		public void TestCheckUS_UI_NKCarrierSCAC_SCAC_CBP()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.US_UI_NKCarrierSCAC = "MPXX";
			AssertNoMessageError(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);
			dec.US_UI_NKCarrierSCAC = "MPX";
			AssertHasMessageError(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.US_UI_NKCarrierSCAC = "CBPX";
			AssertHasMessageError(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);
			dec.US_UI_NKCarrierSCAC = "CBP";
			AssertNoMessageError(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);
			dec.US_UI_NKCarrierSCAC = "CB";
			AssertNoMessageError(dec.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);
		}

		void SetMessageTypeAndTestCheckUS_SchDLoading(ZString messageType, string usSchDLoading)
		{
			Declaration.JE_MessageType = messageType;
			string expectedMessageError = string.Format(AddInfoJobDeclarationValidation.ScheduleDPortCodeInvalid, Declaration.IsExport ? Schedule.D : Schedule.K);
			Declaration.US_SchDLoading = "WW";
			AssertHasMessageError(Declaration.US_SchDLoadingInfo, expectedMessageError);
			Declaration.US_SchDLoading = usSchDLoading;
			AssertNoMessageError(Declaration.US_SchDLoadingInfo, expectedMessageError);
		}

		void CreateLocoIfNotExists(string locoCode, string country = "US")
		{
			ZQuery codeFilter = new ZQuery(RefUNLOCOSchema.RL_Code, locoCode);
			var unLoco = Factory.LoadTop1<RefUNLOCO>(codeFilter);
			if (unLoco == null)
			{
				var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
				testUSLoco.RL_Code = locoCode;
				testUSLoco.RL_PortName = "TEST Port - " + locoCode;
				testUSLoco.RL_IsSystem = true;
				testUSLoco.RL_HasAirport = true;
				testUSLoco.RL_HasSeaport = true;
				testUSLoco.RL_RN_NKCountryCode = country;
				Factory.Save();
			}
		}
	}
}
