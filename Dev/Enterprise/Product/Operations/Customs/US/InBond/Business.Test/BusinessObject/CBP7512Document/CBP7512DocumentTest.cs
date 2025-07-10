using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CBP7512Document))]
	sealed class CBP7512DocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExpDateAndRevDate()
		{
			var date = ZDateTime.UtcToday.AddDays(-3);
			var expExpDateType = Factory.New<RefSysConfigType>();
			expExpDateType.ZRT_ConfigCode = "CBP7512ED";
			expExpDateType.ZRT_Description = "Test CBP7512ED";
			expExpDateType.ZRT_LongDescription = "Test CBP7512ED";
			var expExpDate = Factory.New<RefSysConfig>();
			expExpDate.ZRC_ZRT_NKConfigCode = expExpDateType.ZRT_ConfigCode;
			expExpDate.ZRC_DecimalValue = 0m;
			expExpDate.ZRC_StartDate = date.AddDays(-10);
			expExpDate.ZRC_EndDate = date.AddDays(10);
			expExpDate.ZRC_StringValue = "20210601";
			var expRevDateType = Factory.New<RefSysConfigType>();
			expRevDateType.ZRT_ConfigCode = "CBP7512RD";
			expRevDateType.ZRT_Description = "Test CBP7512RD";
			expRevDateType.ZRT_LongDescription = "Test CBP7512RD";
			var expRevDate = Factory.New<RefSysConfig>();
			expRevDate.ZRC_ZRT_NKConfigCode = expRevDateType.ZRT_ConfigCode;
			expRevDate.ZRC_DecimalValue = 0m;
			expRevDate.ZRC_StartDate = date.AddDays(-10);
			expRevDate.ZRC_EndDate = date.AddDays(10);
			expRevDate.ZRC_StringValue = "20210602";
			Factory.Save();
			var print = new CBP7512Document(MoveHeader);
			var revDate = print.RevDate;
			AssertEquals("20210601", print.ExpDate);
			AssertEquals("20210602", print.RevDate);
		}

		public void TestBLNo()
		{
			var print = new CBP7512Document(MoveHeader);
			var moveDetail1 = MoveHeader.MovementDetails.AddNew();
			var moveDetail2 = MoveHeader.MovementDetails.AddNew();
			var bill1 = Header.Bills.AddNew();
			var bill2 = Header.Bills.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail2.B9_B0 = bill2.PK;
			bill1.B0_IssuerCode = "ISS1";
			bill2.B0_IssuerCode = "ISS2";
			bill1.B0_MasterBillNumber = "MASTERBILL01";
			bill2.B0_MasterBillNumber = "MASTERBILL02";
			bill1.B0_HouseBillNumber = "HOUSEBILL01";
			bill1.B0_HouseBillNumber = "HOUSEBILL02";
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_MasterBillNumber = "MASTERBILL01";
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_IssuerCode = "ISS1";
			AssertEquals("ISS1 MASTERBILL01", print.BLNo);
			bill2.B0_MasterBillNumber = "MASTERBILL02";
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_IssuerCode = "ISS2";
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_MasterBillNumber = "MASTERBILL01";
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_IssuerCode = "ISS1";
			AssertEquals("MASTERBILL01", print.BLNo);
			bill2.B0_MasterBillNumber = "MASTERBILL02";
			AssertEquals("MULTIPLE", print.BLNo);
		}

		public void TestFieldsForAir()
		{
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var print = new CBP7512Document(MoveHeader);
			MoveHeader.InBondNumber = "693548231";
			AssertEquals("QX10693548231", print.ITNoBarCode);
		}

		public void TestIVisualizerNoteSupporterMembers()
		{
			var print = new CBP7512Document(MoveHeader);
			IVisualizerNoteSupporter supporter = print;
			AssertEquals("supporter.PK", MoveHeader.PK, supporter.PK);
			AssertEquals("supporter.TableCode", MoveHeader.TablePrefix, supporter.TableCode);
		}

		public void TestImportedOnwhenAirMode()
		{
			var print = new CBP7512Document(MoveHeader);
			Header.BH_VoyageNumber = "432";
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			Header.BH_CarrierSCAC = "QF";
			AssertEquals("QF,432", print.ImportedOn);
			Header.ThreeLetterAirCarrierCode = "QFA";
			AssertEquals("Pre-condition - ImportedOn", "QFA,432", print.ImportedOn);
			MoveHeader.BM_SplitCarrierSCAC = "AB";
			MoveHeader.ThreeLetterSplitAirCarrierCode = "";
			MoveHeader.BM_SplitFlightNo = "1234";
			AssertEquals("AB,1234", print.ImportedOn);
			MoveHeader.BM_SplitCarrierSCAC = "A1";
			MoveHeader.ThreeLetterSplitAirCarrierCode = "ABC";
			MoveHeader.BM_SplitFlightNo = "1234";
			AssertEquals("ABC,1234", print.ImportedOn);
			MoveHeader.BM_SplitCarrierSCAC = "AB";
			MoveHeader.ThreeLetterSplitAirCarrierCode = "ABC";
			MoveHeader.BM_SplitFlightNo = "1234";
			AssertEquals("ABC,1234", print.ImportedOn);
		}

		public void TestClearedFor()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "6@19", "Stephen Day", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2@87", "Wonderful Day", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var print = new CBP7512Document(MoveHeader);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			Header.BH_ETA = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, print.DateImported);
			MoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			MoveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(-1);
			MoveHeader.BM_ForeignDestPortKCode = foreignPort.ZZD_Code;
			MoveHeader.BM_DestinationPortCode = "2@87";
			AssertEquals("6@19 Stephen Day", foreignPort.ZZD_Code + " " + foreignPort.ZZD_Description, print.ClearedFor);
			MoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			MoveHeader.BM_ForeignDestPortKCode = foreignPort.ZZD_Code;
			MoveHeader.BM_DestinationPortCode = "2@87";
			AssertEquals("Wonderful Day", print.ClearedFor);
		}

		public void TestArrivedDate()
		{
			var print = new CBP7512Document(MoveHeader);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			Header.BH_ETA = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, print.DateImported);
			MoveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertEquals(ZDateTime.BrettsBirthday, print.DateImported);
		}

		public void TestFields()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "!323Z", "STILL DON'T KNOW", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "20182", "NUEVO LAREDO, MEX.", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "!23Z", "WHAT PORT IS THIS?", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TST";
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var print = new CBP7512Document(MoveHeader);
			MoveHeader.BM_OA_InBondCarrier = org.MainAddress.PK;
			MoveHeader.BM_InBondCarrierID = "12-234-456";
			AssertEquals("12-234-456 BOB THE BUILDER", print.InBondVia);
			MoveHeader.BM_OA_InBondCarrier = ZGuid.Empty;
			AssertEquals("12-234-456", print.InBondVia);
			MoveHeader.BM_InBondCarrierID = "";
			AssertEquals("", print.InBondVia);
			MoveHeader.BM_OA_InBondCarrier = org.MainAddress.PK;
			AssertEquals("BOB THE BUILDER", print.InBondVia);
			AssertEquals("", print.ImportedOn);
			Header.BH_ImportConveyanceName = "THE TANK";
			Header.BH_InBondCarrierID = "69-549-789";
			Header.BH_VoyageNumber = "432";
			AssertEquals("THE TANK", print.ImportedOn);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			Header.BH_CarrierSCAC = "QF";
			AssertEquals("QF,432", print.ImportedOn);
			Header.ThreeLetterAirCarrierCode = "QFA";
			AssertEquals("Pre-condition - ImportedOn", "QFA,432", print.ImportedOn);
			Header.BH_VoyageNumber = "Q432";
			AssertEquals("Q432", print.ImportedOn);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertEquals("THE TANK,Q432", print.ImportedOn);
			AssertEquals(ZBool.False, print.SubmittedElectronically);
			MoveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(ZBool.True, print.SubmittedElectronically);
			MoveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureAmendment;
			AssertEquals(ZBool.True, print.SubmittedElectronically);
			MoveHeader.LogManager.CancelAll();
			MoveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			MoveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
			AssertEquals(ZBool.False, print.SubmittedElectronically);
			var moveDetail = MoveHeader.MovementDetails.AddNew();
			AssertEquals("", print.PreviousITType);
			moveDetail.B9_PreviousITType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals(InbondCommonTypeList.Codes._1ImmediateTransport, print.PreviousITType);
			AssertEquals("", print.PreviousITNumber);
			moveDetail.B9_PreviousITNumber = "4345333";
			AssertEquals("4345333", print.PreviousITNumber);
			AssertEquals("", print.PreviousITPort);
			moveDetail.B9_PreviousITPortDCode = "9865";
			AssertEquals("9865", print.PreviousITPort);
			AssertEquals(ZDateTime.Empty, print.PreviousITDate);
			moveDetail.B9_PreviousITDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, print.PreviousITDate);
			var moveDetail2 = MoveHeader.MovementDetails.AddNew();
			moveDetail2.B9_PreviousITNumber = "01987456";
			AssertEquals("MULTIPLE", print.PreviousITNumber);
			moveDetail2.B9_PreviousITNumber = "4345333";
			AssertEquals("4345333", print.PreviousITNumber);
			moveDetail2.B9_PreviousITNumber = ZString.Empty;
			AssertEquals("4345333", print.PreviousITNumber);
			moveDetail.B9_PreviousITNumber = ZString.Empty;
			AssertEquals(ZString.Empty, print.PreviousITNumber);
			AssertEquals("", print.FormattedEntryNumber);
			MoveHeader.InBondNumber = "693548231";
			AssertEquals("693548231", print.FormattedEntryNumber);
			AssertEquals("", print.EntryTypeCode);
			MoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals(InbondCommonTypeList.Codes._2TransportandExport, print.EntryTypeCode);
			AssertEquals("T&E", print.EntryTypeAbbreviation);
			MoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("IT", print.EntryTypeAbbreviation);
			MoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("IE", print.EntryTypeAbbreviation);
			MoveHeader.BM_InBondEntryType = "ZZ";
			AssertEquals("", print.EntryTypeAbbreviation);

			AssertEquals("", print.PortCode);
			AssertEquals("", print.PortName);
			MoveHeader.BM_PortOfPresentationCode = "!23Z";
			AssertEquals("!23Z", print.PortCode);
			AssertEquals("WHAT PORT IS THIS?", print.PortName);
			AssertEquals("", print.FirstUSPortOfUnlading);
			Header.BH_PortUnladingDCode = "!23Z";
			AssertEquals("!23Z WHAT PORT IS THIS?", print.FirstUSPortOfUnlading);
			AssertEquals(ZDateTime.Today, print.EntryDate);
			moveHeader.BM_EntryDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, print.EntryDate);
			AssertEquals("", print.EnteredOrImportedBy);
			Header.BH_OA_Importer = org.MainAddress.PK;
			AssertEquals(org.MainAddress.AddressAsASingleLine, print.EnteredOrImportedBy);
			AssertEquals("", print.ImporterIRSNo);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.SocialSecurityNumber, "999-9999", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("", print.ImporterIRSNo);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345-456", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("12345-456", print.ImporterIRSNo);
			AssertEquals("", print.CustomsPortDirector);
			MoveHeader.BM_DestinationPortCode = "!23Z";
			AssertEquals("!23Z WHAT PORT IS THIS?", print.CustomsPortDirector);
			AssertEquals("", print.FinalForeignDestination);
			MoveHeader.BM_ForeignDestPortKCode = foreignPort.ZZD_Code;
			AssertEquals("!323Z STILL DON'T KNOW", print.FinalForeignDestination);
			var foreignDest = Factory.New<RefUNLOCO>();
			foreignDest.RL_Code = "!ZZ14";
			foreignDest.RL_PortName = "UNLOCO PORT";
			MoveHeader.BM_RL_NKForeignDestPort = foreignDest.RL_Code;
			AssertEquals("!323Z STILL DON'T KNOW", print.FinalForeignDestination);
			MoveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			AssertEquals("!ZZ14 UNLOCO PORT", print.FinalForeignDestination);
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "NEW ADDRESS 1";
			var bill = Header.Bills.AddNew();
			bill.Consignee.E2_OA_Address = address2.PK;
			moveDetail.B9_B0 = bill.PK;
			AssertEquals(address2.AddressAsASingleLine, print.Consignee);
			bill.Consignee.E2_OA_Address = ZGuid.Empty;
			AssertEquals(org.MainAddress.AddressAsASingleLine, print.Consignee);
			Header.BH_OA_Importer = ZGuid.Empty;
			AssertEquals("", print.Consignee);
			AssertEquals("", print.ForeignPortLading);
			header.BH_ImportLoadPortKCode = "20182";
			AssertEquals("20182 NUEVO LAREDO, MEX.", print.ForeignPortLading);
			bill.B0_IssuerCode = ZString.Empty;
			AssertEquals("", print.BLNo);
			bill.B0_IssuerCode = "ADSD";
			bill.B0_MasterBillNumber = "MB32342342";
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("MB32342342", print.BLNo);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertEquals("ADSD MB32342342", print.BLNo);
			var bill2 = Header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "00945678123";
			moveDetail2.B9_B0 = bill2.PK;
			AssertEquals("MULTIPLE", print.BLNo);
			bill2.B0_MasterBillNumber = "MB32342342";
			AssertEquals("ADSD MB32342342", print.BLNo);
			bill2.B0_MasterBillNumber = ZString.Empty;
			AssertEquals("ADSD MB32342342", print.BLNo);
			moveDetail2.B9_B0 = bill.PK;
			AssertEquals("ADSD MB32342342", print.BLNo);
			AssertEquals(ZDateTime.Empty, print.DateOfSailing);
			header.BH_SailingDate = new ZDateTime(2010, 1, 23);
			AssertEquals(new ZDateTime(2010, 1, 23), print.DateOfSailing);
			Header.BH_ImportConveyanceCountry = "ZN";
			AssertEquals("ZN", print.Flag);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.RailContainer;
			AssertEquals("", print.Flag);
			AssertEquals(ZDateTime.Empty, print.DateImported);
			Header.BH_ETA = new ZDateTime(2010, 4, 23);
			AssertEquals(new ZDateTime(2010, 4, 23), print.DateImported);
			AssertEquals("", print.Via);
			MoveHeader.BM_Via = "!323Z";
			AssertEquals("!323Z STILL DON'T KNOW", print.Via);
			AssertEquals("", print.ExportedFrom);
			header.BH_RN_NKFirstExportCountry = Core.Constants.CountryCodes.Uganda;
			AssertEquals("UG (Uganda)", print.ExportedFrom);
			AssertEquals(ZDateTime.Empty, print.DateExported);
			header.BH_FirstExportDate = new ZDateTime(2010, 3, 23);
			AssertEquals(new ZDateTime(2010, 3, 23), print.DateExported);

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var firms = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Z!@Z", "FIRMS OF BOB", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			AssertEquals("", print.GoodsNowAt);
			Header.BH_FIRMS = "DZ!2";
			AssertEquals("DZ!2", print.GoodsNowAt);
			Header.BH_FIRMS = "Z!@Z";
			AssertEquals("FIRMS OF BOB Z!@Z", print.GoodsNowAt);
			AssertEquals("QP01693548231", print.ITNoBarCode);
			MoveHeader.InBondNumber = ZString.Empty;
			AssertEquals("", print.ITNoBarCode);
			AssertEquals(Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy), print.InBondEnteredOrImportedBy);
			Header.BH_GB = GlbBranch.CurrentBranch.PK;
			Header.Branch.GB_OH_OrgProxy = org.PK;
			AssertEquals(org, print.InBondEnteredOrImportedBy);
			AssertEquals(org.OH_FullName, print.EnteredOrWithdrawnBy);
			org.MainAddress.OA_Address2 = "ADDRESS 2";
			org.MainAddress.OA_City = "CITY";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2200";
			AssertEquals(org.MainAddress.OA_Address1, print.EnteredOrWithdrawnByAddress1);
			AssertEquals(org.MainAddress.OA_Address2, print.EnteredOrWithdrawnByAddress2);
			AssertEquals(org.MainAddress.OA_City + " " + org.MainAddress.OA_State + " " + org.MainAddress.OA_PostCode, print.EnteredOrWithdrawnByAddress3);
			org.MainAddress.OA_Address2 = ZString.Empty;
			AssertEquals(org.MainAddress.OA_Address1, print.EnteredOrWithdrawnByAddress1);
			AssertEquals(org.MainAddress.OA_City + " " + org.MainAddress.OA_State + " " + org.MainAddress.OA_PostCode, print.EnteredOrWithdrawnByAddress2);
			AssertEquals("", print.EnteredOrWithdrawnByAddress3);
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, print.AttorneyInFact);
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("No CusAgent entered", ZString.Empty, print.AttorneyInFact);
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_Code = "Z!Z";
			moveHeader.BM_GS_NKCusAgent = "Z!Z";
			AssertEquals("BOB THE BUILDER", print.AttorneyInFact);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("BOB THE BUILDER ATTY-IN-FACT", print.AttorneyInFact);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("BOB THE BUILDER", print.AttorneyInFact);
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("CargoWise Support ATTY-IN-FACT", print.AttorneyInFact);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("CargoWise Support", print.AttorneyInFact);
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_FullName = "IAN THE BUILDER";
			staff2.GS_Code = "I!C";
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, staff2.PK.ToGuid());
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("IAN THE BUILDER ATTY-IN-FACT", print.AttorneyInFact);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("IAN THE BUILDER", print.AttorneyInFact);
			Header.BH_JobReference = "IB32342";
			AssertEquals("Job Ref: IB32342", print.DeclarationReference);
			AssertEquals("!23Z WHAT PORT IS THIS?", print.CertificateOfLadingPort);
			AssertEquals("WHAT PORT IS THIS?", print.ClearedFor);
			MoveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertEquals("", print.CertificateOfLadingPort);
			AssertEquals("", print.ClearedFor);
			MoveHeader.BM_ExportDate = new ZDateTime(2010, 3, 23);
			AssertEquals(new ZDateTime(2010, 3, 23), print.ClearedOn);
			AssertEquals("", print.USSeal);
			MoveHeader.BM_Seals = "SEAL#$3";
			AssertEquals("SEAL#$3", print.USSeal);
			AssertEquals("", print.ExportLadenOn);
			MoveHeader.BM_ExportLadenOn = "VESSEL D1";
			AssertEquals("VESSEL D1", print.ExportLadenOn);
			MoveHeader.BM_InBondCarrierID = "12-234-456";
			AssertEquals("12-234-456 BOB THE BUILDER", print.AttorneyOrAgentOfCarrier);
			MoveHeader.BM_OA_InBondCarrier = ZGuid.Empty;
			AssertEquals("12-234-456", print.AttorneyOrAgentOfCarrier);
			MoveHeader.BM_InBondCarrierID = ZString.Empty;
			AssertEquals("", print.AttorneyOrAgentOfCarrier);
			AssertEquals("", print.GONumber);
			MoveHeader.BM_GONumber = "GO324";
			AssertEquals("GO324", print.GONumber);
			AssertEquals("", print.PedimentoNumber);
			MoveHeader.BM_PedimentoNumber = "PN324";
			AssertEquals("PN324", print.PedimentoNumber);
		}

		public void TestImportedOnForAir()
		{
			var print = new CBP7512Document(MoveHeader);
			Header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("Pre-condition - ImportedOn", ZString.Empty, print.ImportedOn);
			Header.BH_CarrierSCAC = "AA";
			Header.BH_VoyageNumber = "219";
			AssertEquals("Pre-condition - ImportedOn", "AA,219", print.ImportedOn);
			Header.ThreeLetterAirCarrierCode = "AAL";
			AssertEquals("Pre-condition - ImportedOn", "AAL,219", print.ImportedOn);
			Header.BH_VoyageNumber = "UA310";
			AssertEquals("Pre-condition - ImportedOn", "UA310", print.ImportedOn);
		}

		public void TestPedimentoNumber()
		{
			CBP7512Document print = new CBP7512Document(MoveHeader);
			AssertEquals("Pre-condition - Pedimento Number", ZString.Empty, print.PedimentoNumber);
			CusInBondBill bill = Factory.NewWithValidTestData<CusInBondBill>();
			CusInbondBillAddRef ref1 = bill.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			ref1.BR_ReferenceNum = "2011CQ003948458";
			var moveDetail = bill.MoveDetails.AddNew();
			MoveHeader.MovementDetails.Add(moveDetail);
			AssertEquals("Pedimento Number", "Pedimento: 2011CQ003948458", print.PedimentoNumber);
		}

		public void TestAuthorizedStatement()
		{
			MoveHeader.Header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var print = new CBP7512Document(MoveHeader);
			AssertEquals("Pre-condition - inbond not submitted & clear", ZBool.False, print.SubmittedElectronically);
			AssertEquals(ZString.Empty, print.AuthorizedStatement);
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var request = mock.Object;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbond;
			request.EM_MessageNum = "545011";
			request.EM_LinkedObject = moveHeader;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			request.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(-20);
			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondResponse;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "545011";
			response.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(-10);
			request.EM_MessageText = @"B013901SV9QX                                               115360               10A61333209214   TOWE5301     0023232335-215404600                              20AZ  40                         636         3901120211                         30 Y        05564654214                                                         Y  3901SV9QX00003";
			response.EM_MessageText = @"B013901SV9XT                                               115360               10A61333209214   TOWE5301     0023232335-215404600                              20AZ  40                         636         3901120211                         30 Y        05564654214                                                         9502358DATA ACCEPTANCE                                                          Y  3901SV9XT00003                                                               ";
			MoveHeader.Messages.Add(request);
			MoveHeader.Messages.Add(response);
			MoveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			print = new CBP7512Document(MoveHeader);
			AssertEquals(ZBool.True, print.SubmittedElectronically);
			AssertEquals("** QX IN-BOND AUTHORIZED **", print.AuthorizedStatement);
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			request.EM_MessageText = @"B015201EJEQP                                01             SEIMIAHAM_1134       
10A62040414371   RLSN2304971230035443376-055469900 N                            
Y  5201EJEQP00001";
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			response.EM_MessageText = "B018888XJ5QT                                               27458                " +
				"10A61045439100   APLU2704     0062500011-123456700 N                            " +
				"9502220 ACCEPTED                                                                " +
				"Y  8888XJ5QT00003";
			print = new CBP7512Document(MoveHeader);
			AssertEquals("** QP IN-BOND AUTHORIZED **", print.AuthorizedStatement);
			response.EM_MessageText = "B018888XJ5QT                                               27458                " +
				"10A61045439100   APLU2704     0062500011-123456700 N                            " +
				"9501109 INVALID BONDED CARRIER ID                                               " +
				"9501270 TRANSACTION DATA  REJECTED                                              " +
				"Y  8888XJ5QT00003";
			print = new CBP7512Document(MoveHeader);
			AssertEquals("", print.AuthorizedStatement);
			mock.Protected().Verify("GetNumberFountainNumbersAndFillInPlaceHolders", Times.Never());
		}

		public void TestEnteredOrImportedBy()
		{
			CBP7512Document print = new CBP7512Document(MoveHeader);
			AssertEquals("", print.EnteredOrImportedBy);
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "A&S FURNISHING CO LTD";
			org.MainAddress.OA_Address1 = "UNIT A1, 4TH FLOOR, PIONEER IND BLDG";
			org.MainAddress.OA_City = "HONG KONG";
			Header.BH_OA_Importer = org.MainAddress.PK;
			AssertEquals(org.MainAddress.AddressAsASingleLine, print.EnteredOrImportedBy);
			AssertEquals("A&S FURNISHING CO LTD UNIT A1, 4TH FLOOR, PIONEER IND BLDG HONG KONG", print.EnteredOrImportedBy);
			AssertEquals("Address fits in allocated spacing", ZString.Empty, print.EnteredOrImportedByOverflow);
			org.OH_FullName = "A&S FURNISHING CO LTD";
			org.MainAddress.OA_Address1 = "UNIT A1, 4TH FLOOR, PIONEER IND BLDG";
			org.MainAddress.OA_Address2 = "213 WAI YIP STREET, KWUN TONG, KOWLOON";
			org.MainAddress.OA_City = "HONG KONG";
			AssertEquals("A&S FURNISHING CO LTD UNIT A1, 4TH FLOOR, PIONEER IND BLDG 213 WAI YIP STREET, KWUN TONG, KOWLOON", print.EnteredOrImportedBy);
			AssertEquals("Address has overflowed allocated spacing", "HONG KONG", print.EnteredOrImportedByOverflow);
		}

		public void TestEnteredOrWithdrawnBy()
		{
			var print = new CBP7512Document(MoveHeader);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.OH_FullName, print.EnteredOrWithdrawnBy);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address1, print.EnteredOrWithdrawnByAddress1);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_Address2, print.EnteredOrWithdrawnByAddress2);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_City + " " + GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_State + " " + GlbBranch.CurrentBranch.OrgProxy.MainAddress.OA_PostCode, print.EnteredOrWithdrawnByAddress3);
			var customsAddrOfRecord = Factory.Load<OrgHeader>(Header.Branch.GB_OH_OrgProxy);
			customsAddrOfRecord.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
			customsAddrOfRecord.Addresses[0].OA_Address1 = "Nashville Branch Office";
			customsAddrOfRecord.Addresses[0].OA_Address2 = "115 JonesTown Street";
			customsAddrOfRecord.Addresses[0].OA_City = "Nashville";
			customsAddrOfRecord.Addresses[0].OA_PostCode = "55555";
			customsAddrOfRecord.Addresses[0].OA_State = "TN";
			customsAddrOfRecord.Addresses[0].OA_Phone = "+1 22 456 789";
			customsAddrOfRecord.Addresses[0].OA_Fax = "+1 22 456 799";
			customsAddrOfRecord.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals("Customs Brokers and Logistics Worldwide", print.EnteredOrWithdrawnBy);
			AssertEquals("Nashville Branch Office", print.EnteredOrWithdrawnByAddress1);
			AssertEquals("115 JonesTown Street", print.EnteredOrWithdrawnByAddress2);
			AssertEquals("Nashville TN 55555", print.EnteredOrWithdrawnByAddress3);
			customsAddrOfRecord.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
			customsAddrOfRecord.Addresses[0].OA_Address1 = "115 JonesTown Street";
			customsAddrOfRecord.Addresses[0].OA_Address2 = "";
			customsAddrOfRecord.Addresses[0].OA_City = "Nashville";
			customsAddrOfRecord.Addresses[0].OA_PostCode = "55555";
			customsAddrOfRecord.Addresses[0].OA_State = "TN";
			customsAddrOfRecord.Addresses[0].OA_Phone = "+1 22 456 789";
			customsAddrOfRecord.Addresses[0].OA_Fax = "+1 22 456 799";
			customsAddrOfRecord.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals("Customs Brokers and Logistics Worldwide", print.EnteredOrWithdrawnBy);
			AssertEquals("115 JonesTown Street", print.EnteredOrWithdrawnByAddress1);
			AssertEquals("Nashville TN 55555", print.EnteredOrWithdrawnByAddress2);
			AssertEquals("", print.EnteredOrWithdrawnByAddress3);
		}

		public void TestHandleNullReference()
		{
			CBP7512Document print = new CBP7512Document(Factory.GetNull<CusInBondMoveHeader>());
			var properties = print.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				if (typeof(IZType).IsAssignableFrom(property.PropertyType))
				{
					try
					{
						var data = property.GetValue(print, null);
					}
					catch (Exception e)
					{
						throw new Exception("Accessing " + property.Name, e);
					}
				}
			}

			Assert(true);
		}

		public void TestIParentDocManagerSupportMembers()
		{
			CBP7512Document print = new CBP7512Document(MoveHeader);
			IParentDocManagerSupport supporter = print;
			AssertEquals(Core.Constants.DocManagerCodes.InBond, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", Header.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", Header.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			CBP7512Document print = new CBP7512Document(MoveHeader);
			IDocumentDeliveredLogSupporter supporter = print;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusInBondHeader), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", Header.PK, supporter.Identifier);
		}

		protected override BusinessObject GetNewBusinessObject() => new CBP7512Document(Header.MovementHeaders.AddNew());

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());

		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeader MoveHeader => moveHeader ?? (moveHeader = Header.MovementHeaders.AddNew());
	}
}
