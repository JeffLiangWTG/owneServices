using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondHeaderImportedFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestImportBillsOfLadingLinkedToTheSameSailing()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA");
			AssertEquals(11, bill.B0_ManifestQty);
			bill.B0_ManifestQty = 12;

			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB");
			AssertEquals(3, bill.B0_ManifestQty);
			bill.B0_ManifestQty = 4;

			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC");
			AssertEquals(3, bill.B0_ManifestQty);
			bill.B0_ManifestQty = 5;

			bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "DDD";

			var billImportActions = new BillImportActionCollection(header.Bills);
			billImportActions.Cast<BillImportAction>().FirstOrDefault(x => x.BillNumber == "AAA").IsSelected = true;
			billImportActions.Cast<BillImportAction>().FirstOrDefault(x => x.BillNumber == "BBB").IsSelected = false;
			billImportActions.Cast<BillImportAction>().FirstOrDefault(x => x.BillNumber == "CCC").IsSelected = true;
			billImportActions.Cast<BillImportAction>().FirstOrDefault(x => x.BillNumber == "DDD").IsSelected = true;

			header.ImportBillsOfLadingLinkedToTheSameSailing(billImportActions);
			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA");
			AssertEquals(11, bill.B0_ManifestQty);

			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB");
			AssertEquals(4, bill.B0_ManifestQty);

			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC");
			AssertEquals(3, bill.B0_ManifestQty);

			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "DDD");
			AssertNull(bill);
		}
	}

	sealed class StandaloneCusInBondHeaderTest : TestCaseWithFactory
	{
		public void TestHumanReadableShortcutNameCusISFHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;

			var oceanBill = header.OceanBill;
			AssertNotNull("OceanBill", oceanBill);
			var oceanBillPTTMovement = header.OceanBillPTTMovement;
			AssertNotNull("oceanBillPTTMovement", oceanBillPTTMovement);
			Factory.Save();

			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "SCAC";
			bill1.B0_MasterBillNumber = "AAA";
			AssertEquals(1, header.Bills.Count);
			AssertEquals("AMS MVOCC Job# AAA", string.Format("AMS - {0} - {1}", header.BH_JobReference, header.Bills[0].B0_MasterBillNumber), header.HumanReadableShortcutName);

			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			header2.BH_VoyageNumber = "VVOOYY";

			AssertEquals("AMS MVOCC Job# VoyageNumber", string.Format("AMS - {0} - {1}", header2.BH_JobReference, header2.BH_VoyageNumber), header2.HumanReadableShortcutName);
		}

		public void TestSCACInCarrier()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = GlbBranch.CurrentBranch.GB_Code;
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			var existingCarrierCode = currentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			if (existingCarrierCode != null)
			{
				currentCompany.OrgProxy.CustomsCodes.RemoveAndDelete(existingCarrierCode);
			}
			var us = Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SC1Z", us);
			Factory.Save();

			AssertEquals("SC1Z", header.SCACInCarrier);
		}

		public void TestCheckBH_CarrierSCACFromBranch()
		{
			var orgProxyForCompany = Factory.New<OrgHeader>();
			orgProxyForCompany.OH_Code = "FCOM";
			var cusCode1 = orgProxyForCompany.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "CCCC";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var companyJim = Factory.NewWithValidTestData<GlbCompany>();
			companyJim.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			companyJim.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.HongKong;
			companyJim.GC_Code = "CMP";
			companyJim.GC_OH_OrgProxy = orgProxyForCompany.PK;
			var staffJim = Factory.NewWithValidTestData<GlbStaff>();
			staffJim.GS_LoginName = "JIM";

			var jimBranch = companyJim.Branches.AddNew();
			jimBranch.GB_Code = "BBB";
			jimBranch.GB_BranchName = "BBB NAME";
			jimBranch.GB_RL_NKHomePort = "AUDAR";
			jimBranch.GB_OH_OrgProxy = orgProxyForCompany.PK;
			jimBranch.GB_Phone = "7778889999";
			jimBranch.GB_State = "JIBS";

			var orgProxyForBranch = Factory.New<OrgHeader>();
			orgProxyForBranch.OH_Code = "FBRA";
			var cusCode2 = orgProxyForBranch.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "PROX";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			glbCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			glbCompany.GC_Code = "MMM";
			var glbBranch = glbCompany.Branches.AddNew();
			jimBranch.GB_Code = "XXX";
			jimBranch.GB_BranchName = "XXX NAME";
			jimBranch.GB_RL_NKHomePort = "AUDAR";
			jimBranch.GB_OH_OrgProxy = orgProxyForBranch.PK;
			jimBranch.GB_Phone = "1234567890";
			jimBranch.GB_State = "PANIC";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staffJim.GS_LoginName, Guid.Empty, companyJim.PK.ToGuid()))
			{
				using (Env.SetTemporaryUserContext(staffJim.GS_LoginName, jimBranch.PK.ToGuid(), glbCompany.PK.ToGuid()))
				{
					var header = Factory.New<CusInBondHeader>();
					AssertEquals(jimBranch.PK, header.BH_GB);
					AssertEquals("PROX", header.BH_CarrierSCAC);

					orgProxyForBranch.CustomsCodes.RemoveAll();
					AssertEquals("CCCC", header.SCACInCarrier);
				}
			}
		}

		public void TestSailingSynchronisationIsNotSaved()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_LloydsNumber = "12345";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Canada;
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			header.ChangeSailing(sailing.PK);
			AssertEquals("Pre-condition: synchronisation works as expected", "12345", header.BH_LloydsNumber);
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			header = anotherFactory.Load<CusInBondHeader>(header.PK);
			AssertEquals("It should be saved in db", "12345", header.BH_LloydsNumber);
		}

		public void TestResynchroniseVesselDetails()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_LloydsNumber = "12345";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Canada;
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var header = Factory.New<CusInBondHeader>();
			header.ChangeSailing(sailing.PK);
			AssertEquals("12345", header.BH_LloydsNumber);
			AssertEquals(Core.Constants.CountryCodes.Canada, header.BH_ImportConveyanceCountry);
			header.BH_OverrideFreightDefaults = true;
			header.BH_LloydsNumber = "54321";
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Cambodia;
			header.BH_OverrideFreightDefaults = false;
			AssertEquals("12345", header.BH_LloydsNumber);
			AssertEquals(Core.Constants.CountryCodes.Canada, header.BH_ImportConveyanceCountry);
		}

		public void TestNoOfBillsAndContainers()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Today.AddDays(10);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USLAX";
			destination1.JB_E_ARV = ZDateTime.Today.AddDays(20);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "CAAAD";
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(30);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "USCHI";
			destination2.JB_E_ARV = ZDateTime.Today.AddDays(40);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "USLAX");
			var sBill1 = Factory.New<BillOfLading>();
			sBill1.JS_JX = sailing.PK;
			sBill1.JS_HouseBill = "SCACAAA";
			var sBill2 = Factory.New<BillOfLading>();
			sBill2.JS_JX = sailing.PK;
			sBill2.JS_HouseBill = "SCACBBB";

			var sailing2 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "CAAAD" && x.JX_JB_RL_NKPortOfDischarge == "USCHI");
			var sBill3 = Factory.New<BillOfLading>();
			sBill3.JS_JX = sailing2.PK;
			sBill3.JS_HouseBill = "SCAZZZ";

			var header = Factory.New<CusInBondHeader>();
			header.ChangeSailing(sailing.PK);
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "SCAC";
			bill1.B0_MasterBillNumber = "AAA";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "SCAC";
			bill2.B0_MasterBillNumber = "BBB";
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "SCAC";
			bill3.B0_MasterBillNumber = "CCC";

			AssertEquals(3, header.BH_NoOfAMSBills);
			AssertEquals(2, header.BH_NoOfSailingBills);

			header.ChangeSailing(sailing2.PK);
			AssertEquals(1, header.BH_NoOfSailingBills);
		}

		public void TestIsNVOCCHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("IsNVOCCHeader", false, header.IsNVOCCHeader);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals("IsNVOCCHeader", true, header.IsNVOCCHeader);
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			AssertEquals("IsNVOCCHeader", false, header.IsNVOCCHeader);
		}

		public void TestOceanBillDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertNull("OceanBill", header.OceanBill);
			AssertNull("OceanBillPTTMovement", header.OceanBillPTTMovement);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var oceanBill = header.OceanBill;
			AssertNotNull("OceanBill", oceanBill);
			var oceanBillPTTMovement = header.OceanBillPTTMovement;
			AssertNotNull("oceanBillPTTMovement", oceanBillPTTMovement);
			Factory.Save();
			AssertEquals("oceanBill.IsDeleted", false, oceanBill.IsDeleted);
			AssertEquals("oceanBillPTTMovement.IsDeleted", false, oceanBillPTTMovement.IsDeleted);
			header.Delete();
			AssertEquals("oceanBill.IsDeleted", true, oceanBill.IsDeleted);
			AssertEquals("oceanBillPTTMovement.IsDeleted", true, oceanBillPTTMovement.IsDeleted);

			header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			oceanBill = header.OceanBill;
			AssertNotNull("OceanBill", oceanBill);
			oceanBillPTTMovement = header.OceanBillPTTMovement;
			AssertNotNull("oceanBillPTTMovement", oceanBillPTTMovement);
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			AssertNull("OceanBill", header.OceanBill);
			AssertNull("OceanBillPTTMovement", header.OceanBillPTTMovement);
			AssertEquals("oceanBill.IsDeleted", true, oceanBill.IsDeleted);
			AssertEquals("oceanBillPTTMovement.IsDeleted", true, oceanBillPTTMovement.IsDeleted);
		}

		public void TestPopulateJobReferenceIfNeeded_WhenConsolExists_PopulateFromConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertNotNull(header.Consol);

			header.PopulateJobReferenceIfNeeded();
			AssertEquals("Job reference", consol.JK_UniqueConsignRef, header.BH_JobReference);
		}

		[UseSnapshotProtection]
		public void TestPopulateJobReferenceIfNeeded_WhenNoConsol_FallbackToGenerateJobReference()
		{
			Env.NumberFountains.USAMSJobReference.SetNext(Factory, 888);
			var header = Factory.New<CusInBondHeader>();
			AssertNull(header.Consol);

			header.PopulateJobReferenceIfNeeded();
			AssertEquals("Job reference", "AMS0000888", header.BH_JobReference);
		}

		[UseSnapshotProtection]
		public void TestGenerateJobReference()
		{
			Env.NumberFountains.USAMSJobReference.SetNext(Factory, 999);
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			AssertEquals("Job reference", "AMS0000999", header.BH_JobReference);
		}

		public void TestAddAndRemoveSailing()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.ChangeSailing(sailing1.PK);
			AssertEquals(sailing1.PK, header.Sailing.PK);
			AssertEquals(1, header.Sailings.Count);

			header.ChangeSailing(sailing2.PK);
			AssertEquals(sailing2.PK, header.Sailing.PK);
			AssertEquals(1, header.Sailings.Count);

			header.ChangeSailing(ZGuid.Empty);
			AssertNull(header.Sailing);
			AssertEquals(0, header.Sailings.Count);
		}

		public void TestISailingParentFindBoxMembers()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_RL_NKPortUnlading = "USCHI";
			header.BH_OverrideFreightDefaults = true;
			header.ChangeSailing(sailing.PK);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;

			ISailingParentFindBox sailingParent = header;
			AssertEquals("USCHI", sailingParent.DischargePort);
			AssertEquals(ZString.Empty, sailingParent.LoadPort);
			AssertEquals(ZString.Empty, sailingParent.Origin);
			AssertEquals(sailing.PK, sailingParent.SailingPK);
			AssertEquals(Core.Constants.TransportModes.Sea, sailingParent.TransportMode);
		}
	}

	[TestedType(typeof(CusInBondHeader))]
	sealed class CusInBondHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLatestDispositionCodeAndDesc()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusInBondHeader), nameof(CusInBondHeader.BH_LatestDispositionCode), false, x => x.Caption == "Latest Disposition" && x.MediumCaption == "Latest Disp." && x.ShortCaption == "Disposition");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusInBondHeader), nameof(CusInBondHeader.BH_LatestDispositionCodeDescription), false, x => x.Caption == "Latest Disposition Description" && x.MediumCaption == "Latest Disp. Desc." && x.ShortCaption == "Disposition Desc.");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1W", "1W DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "83", "83 DESC", startDate, endDate);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			header.BH_PortUnladingDCode = "0012";

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			_ = bill2.DispositionCodes.AddNewIfNotExist(code1.ZZD_Code, new ZDateTime(2023, 12, 1));
			var bill3 = header.Bills.AddNew();
			_ = bill3.DispositionCodes.AddNewIfNotExist(code1.ZZD_Code, new ZDateTime(2023, 12, 2));
			var bill4 = header.Bills.AddNew();
			_ = bill4.DispositionCodes.AddNewIfNotExist(code1.ZZD_Code, new ZDateTime(2023, 12, 2));
			_ = bill4.DispositionCodes.AddNewIfNotExist(code2.ZZD_Code, new ZDateTime(2023, 12, 3));
			var bill5 = header.Bills.AddNew();
			_ = bill5.DispositionCodes.AddNewIfNotExist(code3.ZZD_Code, new ZDateTime(2023, 12, 4));
			_ = bill5.DispositionCodes.AddNewIfNotExist(code2.ZZD_Code, new ZDateTime(2023, 12, 3));
			var bill6 = header.Bills.AddNew();
			bill6.B0_ShipmentType = CusInBondBill.OceanBillType;
			_ = bill6.DispositionCodes.AddNewIfNotExist(code4.ZZD_Code, new ZDateTime(2023, 12, 3));
			Factory.Save();

			AssertContains(code1.ZZD_Code, header.BH_LatestDispositionCode);
			AssertContains(code2.ZZD_Code, header.BH_LatestDispositionCode);
			AssertContains(code3.ZZD_Code, header.BH_LatestDispositionCode);
			AssertNotContains(code4.ZZD_Code, header.BH_LatestDispositionCode);
			AssertContains(code1.ZZD_Description, header.BH_LatestDispositionCodeDescription);
			AssertContains(code2.ZZD_Description, header.BH_LatestDispositionCodeDescription);
			AssertContains(code3.ZZD_Description, header.BH_LatestDispositionCodeDescription);
			AssertNotContains(code4.ZZD_Description, header.BH_LatestDispositionCodeDescription);

			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_PortUnladingDCode = "0012";
			var header1bill1 = header1.Bills.AddNew();
			var header1bill2 = header1.Bills.AddNew();
			_ = header1bill2.DispositionCodes.AddNewIfNotExist(code1.ZZD_Code, new ZDateTime(2023, 12, 1));
			Factory.Save();
			AssertEquals(code1.ZZD_Code, header1.BH_LatestDispositionCode);
			AssertEquals(code1.ZZD_Description, header1.BH_LatestDispositionCodeDescription);
		}

		public void TestBH_JobReferenceIsAssignedFromConsolNumberWithAutoCreatedLog()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals("BH_JobReference", ZString.Empty, header.BH_JobReference);
			Factory.Save();
			AssertNotEquals("BH_JobReference", ZString.Empty, header.BH_JobReference);
			AssertEquals("BH_JobReference", consol.JK_UniqueConsignRef, header.BH_JobReference);
			AssertEquals(true, header.Logs.AutoCreatedLog != null);
		}

		public void TestPortOfDisChargeWithMultiPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0012", "US Port of Discharge", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0013", "US Port of Discharge", startDate, endDate);
			newFactory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "0012";

			var bill1 = header.Bills.AddNew();
			bill1.B0_InBondPortOfDestDCode = "0012";
			AssertEquals(false, header.HasMultiDischargePorts);
			AssertEquals("0012 US Port of Discharge", header.PortOfDischarge);

			var bill2 = header.Bills.AddNew();
			bill2.B0_InBondPortOfDestDCode = "0013";

			AssertEquals(true, header.HasMultiDischargePorts);
			AssertEquals("MULTI", header.PortOfDischarge);
		}

		public void TestDispositionCodeDescriptionList()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(true, Object.ReferenceEquals(Factory.GetCachedValue<ConveyanceEventCodeList>(), header.DispositionCodeDescriptionList));
		}

		public void TestIDispositionCodeColumnsForFastSearchProviderMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			IDispositionCodeColumnsForFastSearchProvider provider = header;
			var columns = provider.GetColumnsForFastSearch();
			AssertEquals(1, columns.Length);
			AssertEquals(USDispositionDataAddInfoSchema.US_Code, columns[0]);
		}

		public void TestMovementHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			AssertEquals("BM_SubApplicationCode", SubApplicationCodeList.Codes.AMS, moveHeader.BM_SubApplicationCode);
			var pttMoveHeader = header.PTTMovements.AddNew();
			AssertEquals("BM_SubApplicationCode", SubApplicationCodeList.Codes.PermitToTransfer, pttMoveHeader.BM_SubApplicationCode);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerInNewFactory = newFactory.Load<CusInBondHeader>(header.PK);
			var moveHeaderInNewFactory = headerInNewFactory.MovementHeader;
			AssertEquals("PK", moveHeader.PK, moveHeaderInNewFactory.PK);
			AssertEquals("BM_SubApplicationCode", SubApplicationCodeList.Codes.AMS, moveHeaderInNewFactory.BM_SubApplicationCode);
		}

		public void TestCanCancelInBond()
		{
			var header = Factory.New<CusInBondHeader>();
			Assert(string.IsNullOrEmpty(header.CanCancel()));

			var moveheader1 = header.MovementHeaders.AddNew();
			moveheader1.BM_CustomsStatus = AMSBillMessageStatusList.Codes.AwaitingDeparture;
			var moveheader2 = header.MovementHeaders.AddNew();
			moveheader2.BM_CustomsStatus = AMSBillMessageStatusList.Codes.ClearDeparture;
			var moveheader3 = header.MovementHeaders.AddNew();
			moveheader3.BM_CustomsStatus = AMSBillMessageStatusList.Codes.Error;

			AssertContains("This record cannot be deactivated as there are Movements that are still waiting for a response from Customs.", header.CanCancel());

			moveheader1.Delete();
			AssertContains("This record cannot be deactivated as there are Movements that have been accepted by Customs.", header.CanCancel());

			moveheader2.BM_CustomsStatus = AMSBillMessageStatusList.Codes.Deleted;
			Assert(string.IsNullOrEmpty(header.CanCancel()));

			moveheader2.Delete();
			Assert(string.IsNullOrEmpty(header.CanCancel()));
		}

		public void TestBooleanFlags()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("IsRail", true, header.IsRail);
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			AssertEquals("IsRail", false, header.IsRail);

			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals("IsSubsequentInBondValidationMode", false, header.IsSubsequentInBondValidationMode);
			header.ValidationModes = ValidationModes.SubsequentInBond;
			AssertEquals("IsSubsequentInBondValidationMode", true, header.IsSubsequentInBondValidationMode);
		}

		public void TestPortDefaulting()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_RL_NKPortUnlading = "USLAX";
			AssertEquals("2704", header.BH_PortUnladingDCode);

			header.BH_RL_NKPortUnlading = ZString.Empty;
			header.BH_PortUnladingDCode = "2705";
			header.BH_RL_NKPortUnlading = "USLAX";
			AssertEquals("2704", header.BH_PortUnladingDCode);

			header.BH_RL_NKPortUnlading = ZString.Empty;
			header.BH_PortUnladingDCode = "2705";
			header.BH_RL_NKPortUnlading = "USXXX";
			AssertEquals("2705", header.BH_PortUnladingDCode);
		}

		public void TestBH_VoyageNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_VoyageNumber = "234/S";
			AssertEquals("234S", header.BH_VoyageNumber);
			header.BH_VoyageNumber = "234\\S";
			AssertEquals("234S", header.BH_VoyageNumber);
		}

		public void TestBH_PortUnladingDCodeInfoHumanReadableName()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("Port of Unlading Schedule D", header.BH_PortUnladingDCodeInfo.HumanReadableName);
		}

		public void TestValidationModes()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(ValidationModes.InventoryRecord, header.ValidationModes);
			AssertEquals(true, header.IsInventoryRecordValidationMode);

			header.ValidationModes = ValidationModes.None;
			AssertEquals(false, header.IsInventoryRecordValidationMode);

			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.InventoryRecord, header.ValidationModes);
			AssertEquals(true, header.IsInventoryRecordValidationMode);

			var bill1 = header.Bills.AddNew();
			AssertEquals(ValidationModes.InventoryRecord, bill1.ValidationModes);
			bill1.ValidationModes = ValidationModes.None;
			var bill2 = header.Bills.AddNew();
			AssertEquals(ValidationModes.InventoryRecord, bill2.ValidationModes);
			header.ValidationModes = ValidationModes.UseParentValidateMode;
			AssertEquals(ValidationModes.None, bill1.ValidationModes);
			AssertEquals(ValidationModes.UseParentValidateMode, bill2.ValidationModes);
			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.None, bill1.ValidationModes);
			AssertEquals(ValidationModes.InventoryRecord, bill2.ValidationModes);
			header.ValidationModes = ValidationModes.None;
			AssertEquals(ValidationModes.None, bill1.ValidationModes);
			AssertEquals(ValidationModes.None, bill2.ValidationModes);
			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.InventoryRecord, bill1.ValidationModes);
			AssertEquals(ValidationModes.InventoryRecord, bill2.ValidationModes);
		}

		public void TestVesselUpdateWithLowercaseLettersData()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL 1";
			vessel.RV_LloydsNumber = "abcdef";
			vessel.RV_RN_NKCountryOfReg = "gh";

			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportConveyanceName = "VESS/E\\L 1";
			AssertEquals("VESSEL 1", header.BH_ImportConveyanceName);
			AssertEquals("ABCDEF", header.BH_LloydsNumber);
			AssertEquals("GH", header.BH_ImportConveyanceCountry);
		}

		public void TestVesselUpdate()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL 1";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Bahamas;

			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportConveyanceName = "VESS/E\\L 1";
			AssertEquals("VESSEL 1", header.BH_ImportConveyanceName);
			AssertEquals("1234567", header.BH_LloydsNumber);
			AssertEquals(Core.Constants.CountryCodes.Bahamas, header.BH_ImportConveyanceCountry);
			AssertEquals(ZString.Empty, header.BH_VoyageNumber);

			header.BH_VoyageNumber = "234U";
			AssertEquals("VESSEL 1", header.BH_ImportConveyanceName);
			AssertEquals("1234567", header.BH_LloydsNumber);
			AssertEquals(Core.Constants.CountryCodes.Bahamas, header.BH_ImportConveyanceCountry);
			AssertEquals("234U", header.BH_VoyageNumber);

			header.BH_ImportConveyanceName = ZString.Empty;
			AssertEquals(ZString.Empty, header.BH_ImportConveyanceName);
			AssertEquals(ZString.Empty, header.BH_LloydsNumber);
			AssertEquals(ZString.Empty, header.BH_ImportConveyanceCountry);
			AssertEquals(ZString.Empty, header.BH_VoyageNumber);

			header.BH_ImportConveyanceName = "VESSEL 2";
			AssertEquals("VESSEL 2", header.BH_ImportConveyanceName);
			AssertEquals(ZString.Empty, header.BH_LloydsNumber);
			AssertEquals(ZString.Empty, header.BH_ImportConveyanceCountry);
			AssertEquals(ZString.Empty, header.BH_VoyageNumber);
		}

		public void TestSetDefaultValues()
		{
			var org = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var scac1 = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCA1", Core.Constants.CountryCodes.Australia);
			var scac2 = org.CustomsCodes.AddNew();
			scac2.OK_CustomsRegNo = "SCA23";
			scac2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			scac2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			AssertEquals(CusInBondApplicationCodeList.Codes.AMS, header.BH_ApplicationCode);
			AssertEquals("SCA2", header.BH_CarrierSCAC);
			AssertEquals(DirectionTypeList.Codes.MVOCC, header.BH_TransitDirection);
		}

		[ExpectNoExceptions]
		public void TestDoesNotRaiseNotSupportedExceptionWhenAMSMessageIsCreated()
		{
			var header = Factory.New<CusInBondHeaderForTesting>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			var moveDetail1 = bill1.MovementDetail;
			var bill2 = header.Bills.AddNew();
			var moveDetail2 = bill2.MovementDetail;

			moveHeader.Messages.CountChanged += header.StopSynchronisationOnMessages_CountChanged_Exposed;
			var action = new MessageSendingAction(header, ActionCode.Creating);
			action.CreateAMSMessages();
		}

		public void TestSynchronisation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "USLAX";
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(false, header.ShouldSynchronise);
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ var a = header.Synchroniser; });

			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(true, header.ShouldSynchronise);
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			synchroniser.Synchronise(true);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchronise);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			header.BH_RL_NKPortUnlading = "USCHI";
			header.BH_OverrideFreightDefaults = false;
			AssertEquals(true, header.ShouldSynchronise);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			var consolSynchonisation = (ICusInBondHeaderWithConsolSynchonisation)header;
			header.BH_RL_NKPortUnlading = "USCHI";
			consolSynchonisation.SynchroniseWithConsolIfNeeded();
			AssertEquals(true, header.ShouldSynchronise);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			var message = header.MovementHeader.Messages.AddNew();
			AssertEquals(false, header.ShouldSynchronise);
			synchroniser = header.Synchroniser;
			AssertEquals(false, synchroniser.IsEnabled);

			consol.JK_RL_NKDischargePort = "USCHI";
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchronise);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			cancelOverrideFreightDefaultsForTesting = true;
			header.OnOverrideFreightDefaultsUnchecking += new CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
			header.BH_OverrideFreightDefaults = false;
			AssertEquals(true, header.BH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchronise);
			AssertEquals("USLAX", header.BH_RL_NKPortUnlading);

			cancelOverrideFreightDefaultsForTesting = false;
			header.BH_OverrideFreightDefaults = false;
			AssertEquals(false, header.BH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchronise);
			AssertEquals("USCHI", header.BH_RL_NKPortUnlading);
		}

		void header_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			e.Cancel = cancelOverrideFreightDefaultsForTesting;
		}
		bool cancelOverrideFreightDefaultsForTesting;

		public void TestSynchronisationViaDataRefreshBus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "AB1DHB1";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_HarmonisedCode = "1010.10.10";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("APLU", bill.B0_IssuerCode);
			AssertEquals("AB1DHB1", bill.B0_MasterBillNumber);
			var moveDetail = bill.MovementDetail;
			AssertEquals(1, moveDetail.Containers.Count);
			var billContainer = moveDetail.Containers[0];
			AssertEquals("CONT1", billContainer.BC_ContainerNum);
			AssertEquals(1, billContainer.Commodities.Count);
			var commodity = billContainer.Commodities[0];
			AssertEquals("10101010", commodity.BY_HarmonisedTariff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var containerInOtherFactory = newFactory.Load<ForwardingContainer>(container.PK);
			AssertEquals("CONT1", containerInOtherFactory.JC_ContainerNum);
			containerInOtherFactory.JC_ContainerNum = "CONT2";
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertEquals("AB1DHB1", shipmentInOtherFactory.JS_HouseBill);
			shipmentInOtherFactory.JS_HouseBill = "EFGHHB2";
			var packLineInOtherFactory = newFactory.Load<ForwardingPackLine>(packLine.PK);
			AssertEquals("1010.10.10", packLineInOtherFactory.JL_HarmonisedCode);
			packLineInOtherFactory.JL_HarmonisedCode = "2020.20.10";
			newFactory.Save();
			AssertEquals("EFGHHB2", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["APLU", "EFGHHB2"]);

			AssertEquals(false, moveDetail.IsDeleted);
			AssertEquals(moveDetail, bill.MovementDetail);

			AssertEquals("CONT2", container.JC_ContainerNum);
			AssertEquals(1, moveDetail.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, moveDetail.Containers["CONT2"]);

			AssertEquals("2020.20.10", packLine.JL_HarmonisedCode);
			AssertEquals(1, billContainer.Commodities.Count);
			AssertEquals(false, commodity.IsDeleted);
			AssertEquals(commodity, billContainer.Commodities["20202010"]);

			header.BH_OverrideFreightDefaults = true;
			Factory.Save();

			containerInOtherFactory.JC_ContainerNum = "CONT3";
			shipmentInOtherFactory.JS_HouseBill = "IJKLHB3";
			packLineInOtherFactory.JL_HarmonisedCode = "3020.10.10";
			newFactory.Save();

			AssertEquals("IJKLHB3", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["APLU", "EFGHHB2"]);

			AssertEquals(false, moveDetail.IsDeleted);
			AssertEquals(moveDetail, bill.MovementDetail);

			AssertEquals("CONT3", container.JC_ContainerNum);
			AssertEquals(1, moveDetail.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, moveDetail.Containers["CONT2"]);

			AssertEquals("3020.10.10", packLine.JL_HarmonisedCode);
			AssertEquals(1, billContainer.Commodities.Count);
			AssertEquals(false, commodity.IsDeleted);
			AssertEquals(commodity, billContainer.Commodities["20202010"]);

			header.BH_OverrideFreightDefaults = false;
			AssertEquals("IJKLHB3", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(true, bill.IsDeleted);
			bill = header.Bills["APLU", "IJKLHB3"];
			AssertNotNull(bill);

			AssertEquals("CONT3", container.JC_ContainerNum);
			AssertEquals(true, moveDetail.IsDeleted);
			moveDetail = bill.MovementDetail;
			AssertEquals(1, moveDetail.Containers.Count);

			AssertEquals(true, billContainer.IsDeleted);
			billContainer = moveDetail.Containers["CONT3"];
			AssertNotNull(billContainer);

			AssertEquals("3020.10.10", packLine.JL_HarmonisedCode);
			AssertEquals(true, commodity.IsDeleted);
			AssertEquals(1, billContainer.Commodities.Count);
			AssertEquals("30201010", billContainer.Commodities[0].BY_HarmonisedTariff);
		}

		public void TestMessageAttacheesRelatedRecords()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.Branch.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(0, header.MessageAttacheesRelatedRecords.Count);
			var moveHeader = header.MovementHeader;
			moveHeader.BM_MonetaryValue = 100m;
			AssertEquals(0, header.MessageAttacheesRelatedRecords.Count);
			var message = moveHeader.Messages.AddNew();
			message.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals(1, header.MessageAttacheesRelatedRecords.Count);
			AssertEquals(consol.JK_UniqueConsignRef, header.MessageAttacheesRelatedRecords[0].relatedRecord.RecordIdentifier);

			var pttMoveHeader1 = header.PTTMovements.AddNew();
			pttMoveHeader1.BM_InBondCarrierID = "PTT1";
			Factory.Save();
			AssertEquals(1, header.MessageAttacheesRelatedRecords.Count);
			AssertEquals(consol.JK_UniqueConsignRef, header.MessageAttacheesRelatedRecords[0].relatedRecord.RecordIdentifier);

			message = pttMoveHeader1.Messages.AddNew();
			message.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals(2, header.MessageAttacheesRelatedRecords.Count);
			var relatedRecord1 = header.MessageAttacheesRelatedRecords[0].relatedRecord;
			var relatedRecord2 = header.MessageAttacheesRelatedRecords[1].relatedRecord;
			if (relatedRecord2.RecordIdentifier == consol.JK_UniqueConsignRef)
			{
				relatedRecord1 = header.MessageAttacheesRelatedRecords[1].relatedRecord;
				relatedRecord2 = header.MessageAttacheesRelatedRecords[0].relatedRecord;
			}
			AssertEquals(consol.JK_UniqueConsignRef, relatedRecord1.RecordIdentifier);
			AssertEquals("Carrier: PTT1", relatedRecord2.RecordIdentifier);

			var pttMoveHeader2 = header.PTTMovements.AddNew();
			pttMoveHeader2.BM_InBondCarrierID = "PTT2";
			Factory.Save();
			AssertEquals(2, header.MessageAttacheesRelatedRecords.Count);
			relatedRecord1 = header.MessageAttacheesRelatedRecords[0].relatedRecord;
			relatedRecord2 = header.MessageAttacheesRelatedRecords[1].relatedRecord;
			if (relatedRecord2.RecordIdentifier == consol.JK_UniqueConsignRef)
			{
				relatedRecord1 = header.MessageAttacheesRelatedRecords[1].relatedRecord;
				relatedRecord2 = header.MessageAttacheesRelatedRecords[0].relatedRecord;
			}
			AssertEquals(consol.JK_UniqueConsignRef, relatedRecord1.RecordIdentifier);
			AssertEquals("Carrier: PTT1", relatedRecord2.RecordIdentifier);

			message = pttMoveHeader2.Messages.AddNew();
			message.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			Factory.Save();
			AssertEquals(3, header.MessageAttacheesRelatedRecords.Count);
			AssertEquals(consol.JK_UniqueConsignRef, header.MessageAttacheesRelatedRecords.GetElementWrapping(moveHeader).relatedRecord.RecordIdentifier);
			AssertEquals("Carrier: PTT1", header.MessageAttacheesRelatedRecords.GetElementWrapping(pttMoveHeader1).relatedRecord.RecordIdentifier);
			AssertEquals("Carrier: PTT2", header.MessageAttacheesRelatedRecords.GetElementWrapping(pttMoveHeader2).relatedRecord.RecordIdentifier);

			pttMoveHeader1.Delete();
			Factory.Save();
			AssertEquals(2, header.MessageAttacheesRelatedRecords.Count);
			AssertEquals(consol.JK_UniqueConsignRef, header.MessageAttacheesRelatedRecords.GetElementWrapping(moveHeader).relatedRecord.RecordIdentifier);
			AssertEquals("Carrier: PTT2", header.MessageAttacheesRelatedRecords.GetElementWrapping(pttMoveHeader2).relatedRecord.RecordIdentifier);

			var inBondMoveHeader1 = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader1.InBondNumber = "INB1";
			inBondMoveHeader1.BM_InBondEntryType = "61";
			message = inBondMoveHeader1.Messages.AddNew();
			message.EM_MessageText = "A " + AMSEDIMessage.AMSMessageNumberPlaceHolder + " B";
			Factory.Save();

			AssertEquals(3, header.MessageAttacheesRelatedRecords.Count);
			AssertEquals(consol.JK_UniqueConsignRef, header.MessageAttacheesRelatedRecords.GetElementWrapping(moveHeader).relatedRecord.RecordIdentifier);
			AssertEquals("Carrier: PTT2", header.MessageAttacheesRelatedRecords.GetElementWrapping(pttMoveHeader2).relatedRecord.RecordIdentifier);
			AssertEquals("In-Bond Number: INB1, Entry Type: 61", header.MessageAttacheesRelatedRecords.GetElementWrapping(inBondMoveHeader1).relatedRecord.RecordIdentifier);
		}

		public void TestPTTMovements()
		{
			var header = Factory.New<CusInBondHeader>();
			header.PTTMovements.AddNew();
			AssertEquals(1, header.PTTMovements.Count);
			AssertEquals(typeof(CusInBondMoveHeader), header.PTTMovements[0].GetType());
		}

		public void TestHasMoveHeaderEntryType62Or63()
		{
			var header = Factory.New<CusInBondHeader>();
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("Entry Type 62", true, header.HasInbondMoveHeaderEntryType62Or63);

			inbondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("Entry Type 61", false, header.HasInbondMoveHeaderEntryType62Or63);

			inbondMoveHeader.BM_InBondEntryType = ZString.Empty;
			AssertEquals("Entry Type empty", false, header.HasInbondMoveHeaderEntryType62Or63);

			inbondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("Entry Type 63", true, header.HasInbondMoveHeaderEntryType62Or63);

			inbondMoveHeader.Delete();
			AssertEquals("No In-Bond movement", false, header.HasInbondMoveHeaderEntryType62Or63);
		}

		public void Test1302Properties()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "0012";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_LloydsNumber = "12345";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Canada;
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "CAXAN";
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "CAXAN";
			unloco.RL_PortName = "XANCOUVER";

			var refloco = Factory.New<RefLocoMap>();
			refloco.RY_LocalPortCode = "00011";
			refloco.RY_RL_NKLocoPort = "CAXAN";
			refloco.RY_SystemUsage = "SCK";
			refloco.RY_RN = Core.Constants.CountryGuids.UnitedStates;

			header.BH_ParentID = sailing.PK;
			header.BH_ParentTableCode = JobSailingSchema.Constants.Prefix;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0012", "US Port of Discharge", startDate, endDate);
			newFactory.Save();

			var disp1 = header.DispositionCodes.AddNew();
			disp1.US_Code = "AAD";
			disp1.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 34, 0);
			var disp2 = header.DispositionCodes.AddNew();
			disp2.US_Code = "AAD";
			disp2.US_DispositionDate = new ZDateTime(2014, 3, 26, 9, 33, 0);
			var disp3 = header.DispositionCodes.AddNew();
			disp3.US_Code = "AAD";
			disp3.US_DispositionDate = new ZDateTime(2014, 3, 26, 17, 34, 0);

			ZString conveyanceOutput = @"AAD Arrival of vessel 03/26/2014 9:33:00 AM
AAD Arrival of vessel 03/26/2014 9:34:00 AM
AAD Arrival of vessel 03/26/2014 5:34:00 PM
";

			AssertEquals("0012 US Port of Discharge", header.PortOfDischarge);
			AssertEquals("00011 XANCOUVER", header.PortOfLoading);
			AssertEquals(conveyanceOutput, header.ConveyanceEvents);
		}

		public void TestUpdateATDAfterSailingAttached()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2015, 4, 8);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2015, 4, 10);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var header = Factory.New<CusInBondHeader>();
			header.ChangeSailing(sailing.PK);
			AssertEquals(new ZDateTime(2015, 4, 8), header.BH_SailingDate);

			header.ChangeSailing(ZGuid.Empty);
			AssertEquals(ZDateTime.Empty, header.BH_SailingDate);
		}

		public void TestUpdateATDAfterSendingMessage()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_SailingDate = new ZDateTime(2015, 4, 8);
			var moveHeader = header.MovementHeader;
			moveHeader.BM_CustomsStatus = "BDD";
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			Factory.Save();

			var actionCode = ActionCode.VesselDeparture;
			var sendingBill = new MessageSendingObject(moveDetail, ActionCode.VesselDeparture);
			sendingBill.MB_Date = new ZDateTime(2015, 4, 12);
			var messageSendingObjCollection = new MessageSendingObjectCollection(Factory);
			messageSendingObjCollection.Add(sendingBill);
			header.UpdateATDAfterSendingMessage(actionCode, messageSendingObjCollection);
			AssertEquals(new ZDateTime(2015, 4, 12), header.BH_SailingDate);
		}

		public void TestHumanReadableName()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "AMS11111111";
			AssertEquals("AMS AMS11111111", header.HumanReadableName);
		}

		public void TestBH_RL_NKPortOfLoading()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("BH_RL_NKPortOfLoading", ZString.Empty, header.BH_RL_NKPortOfLoading);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2015, 4, 8);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2015, 4, 10);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			header.ChangeSailing(sailing.PK);
			AssertEquals("BH_RL_NKPortOfLoading", origin.JA_RL_NKPortOfLoading, header.BH_RL_NKPortOfLoading);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(bill1, header.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains(bill2, header.BusinessObjectsWithRelatedEvents);
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("JobHeaderCompany", header.Company, ((IWorkflowTriggerEventSource)header).JobHeaderCompany);
			AssertEquals("ParentWorkflowProviders", consol, ((IWorkflowTriggerEventSource)header).ParentWorkflowProviders[0]);
		}

		public void TestActualArrivalDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var arrivalDetail0 = header.PortArrivalDetails.AddNew();
			arrivalDetail0.PortCode = "1101";
			arrivalDetail0.ActualArrivalDate = ZDate.Today.AddDays(-1);
			AssertEquals(ZDate.Today.AddDays(-1), header.NVOCCActualArrivalDate);

			var arrivalDetail1 = header.PortArrivalDetails.AddNew();
			arrivalDetail1.PortCode = "1102";
			arrivalDetail1.ActualArrivalDate = ZDate.Today.AddDays(-1);
			AssertEquals(ZDate.Today.AddDays(-1).ToZDateTime().ToLongTimeString(), header.ActualArrivalDate);
			AssertEquals(ZDateTime.Empty, header.NVOCCActualArrivalDate);

			arrivalDetail1.ActualArrivalDate = ZDate.Today.AddDays(1);
			AssertEquals("MULTIPLE", header.ActualArrivalDate);
			AssertEquals(ZDateTime.Empty, header.NVOCCActualArrivalDate);

			header.PortArrivalDetails.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, header.ActualArrivalDate);
			AssertEquals(ZDateTime.Empty, header.NVOCCActualArrivalDate);
		}

		public void TestCreateManifestMessageProcessor()
		{
			var header = Factory.New<CusInBondHeader>();
			var processor = ((ICustomsManifestMessageSupporter)header).CreateManifestMessageProcessor();
			AssertEquals("AutoSendUSAMSMessageProcessor", processor.GetType().Name);
		}

		public void TestCreateStmProcessQueueProcessor()
		{
			var header = Factory.New<CusInBondHeader>();
			var actionCode = WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage;

			var processor = ((IBaseAutoSendingMessageSupporter)header).CreateStmProcessQueueProcessor(null, actionCode);
			AssertEquals("CustomsStmProcessQueueCreatorProcessor", processor.GetType().Name);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			var header = factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			return header;
		}
		public void TestGetNewCusInBondHeaderProcessTaskCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>", typeof(ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		public void TestHasHVLVParent()
		{
			var header = Factory.New<CusInBondHeader>();
			Assert("Default should be HasHVLVParent is false", !header.HasHVLVParent);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			Assert("Non-shipment parent should return false for HasHVLVParent", !header.HasHVLVParent);

			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ParentID = shipment.PK;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			Assert("Precondition: shipment is not identified as HVLV", !shipment.IsHighVolumeLowValue);
			Assert("Non-HVLV shipment should return false for HasHVLVParent", !header.HasHVLVParent);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			Assert("Precondition: shipment is identified as HVLV", shipment.IsHighVolumeLowValue);
			Assert("HVLV shipment should return true for HasHVLVParent", header.HasHVLVParent);
			header.BH_ParentID = ZGuid.Empty;
			Assert("On ParentID changed to empty, should return false for HasHVLVParent", !header.HasHVLVParent);
		}
	}

	sealed class CusInBondHeaderForTesting : CusInBondHeader
	{
		public CusInBondHeaderForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public void StopSynchronisationOnMessages_CountChanged_Exposed(object sender, CollectionCountChangedEventArgs e)
		{
			StopSynchronisationOnMessages_CountChanged(sender, e);
		}
	}

	[TestedType(typeof(CusInBondHeader))]
	sealed class CusInBondHeaderWorkflowProviderTest : WorkflowProviderTest<CusInBondHeader, ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>>
	{
		public void TestGetTemplateSelectionCriteria()
		{
			var implementation = (IWorkflowProvider)CusInBondHeader;
			var ranker = (ColumnValueRanker)implementation.GetTemplateSelectionCriteria();
			AssertEquals("P0_GB", CusInBondHeader.BH_GB, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals("P0_OH_Client", ZGuid.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("P0_LoadPortCountry", "12!AU", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("P0_DischargePortCountry", "34!US", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!12";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.Australia;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var dischargePort = Factory.New<RefUNLOCO>();
			dischargePort.RL_Code = "US!34";
			var dischargePortMap = dischargePort.RefLocoMaps.AddNew();
			dischargePortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			dischargePortMap.RY_LocalPortCode = "34!US";
			dischargePortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "12!AU";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "34!US";
			destination.JB_E_ARV = ZDateTime.BrettsBirthday;

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			CusInBondHeader.ChangeSailing(sailing.PK);
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.USAMSWorkflowDescriptorCode; }
		}

		protected override CusInBondHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return CusInBondHeader;
		}

		CusInBondHeader CusInBondHeader
		{
			get { return cusInBondHeader ?? (cusInBondHeader = Factory.New<CusInBondHeader>()); }
		}
		CusInBondHeader cusInBondHeader;
	}
}
