using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Service.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using PermitCriteria = Enterprise.Customs.Business.PermitCriteria;
using PermitQtyValIndicatorList = Enterprise.Customs.Business.PermitQtyValIndicatorList;
using PermitService = Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.US.Business.Service.Testing
{
	sealed class FTZPermitWithdrawRequestProviderTest : TestCaseWithFactory
	{
		public void TestGetOutwardEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var factory = new BusinessObjectFactory();
				var owner = GetCRAHOU(factory);
				var warehouse = GetINTTEL(factory);
				warehouse.Header.OH_RL_NKClosestPort = "USLAX";
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				warehouse.OA_RL_NKRelatedPortCode = "USLAX";
				warehouse.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FM32");
				var dataHelper = new PermitTestDataHelper(factory);
				var header = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				dataHelper.CreatePermitRule(header, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				factory.Save();
				IPermitWithdrawRequestProvider ps = new FTZPermitWithdrawRequestProvider();
				AssertEquals("GetOutwardEntryNumber", (ZString)"SJ5-ENT3431", ps.GetOutwardEntryNumber(header));
				AssertEquals("GetOutwardEntryNumber", ZString.Empty, ps.GetOutwardEntryNumber(null));
			}
		}

		public void TestIsDomesticLine()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN123";
			owner.MainAddress.OA_Code = "OFC: OWNER ADDRESS";
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "WHS123";
			warehouse.MainAddress.OA_Code = "OFC: WAREHOUSE ADDRESS";
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MAN123";
			manufacturer.MainAddress.OA_Code = "OFC: MANUFACTURER ADDRESS";
			IPermitWithdrawRequestProvider ps = new FTZPermitWithdrawRequestProvider();
			AssertEquals(true, ps.IsExemptForMatchingPermit(new PermitTransactionDetailForTesting()
			{ ZoneStatus = ZoneStatusList.Codes.Domestic }));
			AssertEquals(false, ps.IsExemptForMatchingPermit(new PermitTransactionDetailForTesting()
			{ ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign }));
		}

		public void TestTryGerPermitWithDomesticInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var factory = new BusinessObjectFactory();
				var owner = GetCRAHOU(factory);
				var warehouse = GetINTTEL(factory);
				warehouse.Header.OH_RL_NKClosestPort = "USLAX";
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				warehouse.OA_RL_NKRelatedPortCode = "USLAX";
				warehouse.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FM32");
				var dataHelper = new PermitTestDataHelper(factory);
				var header1 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				dataHelper.CreatePermitRule(header1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				factory.Save();
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request1.AddInfo = "ZoneStatus=D*BOB=WHERE";
				request1.ZoneStatus = ZoneStatusList.Codes.Domestic;
				var permitService = ObjectFactory.Get<PermitService.IPermitService>();
				PermitService.IPermitWithdrawRequestResponseResult response;
				try
				{
					AssertEquals(true, header1.LockMutex);
					response = permitService.TryGetPermits(new PermitService.IPermitWithdrawRequest[] { request1 });
				}
				finally
				{
					header1.UnlockMutex();
				}

				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], PermitService.SuccessOrFailure.Success, null, null, null);
				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				var permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000M, 200m, ZString.Empty);
				request1.Tariff = "1020304050";
				request1.Qty = 80m;
				permitService = ObjectFactory.Get<PermitService.IPermitService>();
				response = permitService.TryGetPermits(new PermitService.IPermitWithdrawRequest[] { request1 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], PermitService.SuccessOrFailure.Success, null, null, null);
			}
		}

		[TestDate(2017, 9, 4)]
		public void TestGetMatchingCriteria()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWN123";
			owner.MainAddress.OA_Code = "OFC: OWNER ADDRESS";
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "WHS123";
			warehouse.MainAddress.OA_Code = "OFC: WAREHOUSE ADDRESS";
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MAN123";
			manufacturer.MainAddress.OA_Code = "OFC: MANUFACTURER ADDRESS";
			IPermitWithdrawRequestProvider provider = new FTZPermitWithdrawRequestProvider();
			var permitCriteria = new PermitCriteria(false, null, null, null, "KG", "NZ", "PART1", "10203040", ZString.Empty);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse is null\nOwner is null\nProduct (PART1)\nTariff (1020.30.40)\nUQ (KG)", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(true, null, null, null, "KG", "NZ", "PART1", "10203040", ZString.Empty);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse is null\nOwner is null\nManufacturer is null\nTariff (1020.30.40)\nUQ (KG)\nCountry Of Origin (NZ)\nZone Status ()", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(false, null, null, null, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse is null\nOwner is null\nProduct (PART1)\nTariff (1020.30.40)\nUQ (KG)", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(true, null, null, null, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse is null\nOwner is null\nManufacturer is null\nTariff (1020.30.40)\nUQ (KG)\nCountry Of Origin (NZ)\nZone Status (D)", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(false, owner.MainAddress, warehouse.MainAddress, manufacturer.MainAddress, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse (WHS123:OFC: WAREHOUSE ADDRESS, FIRMS:)\nOwner (OWN123:OFC: OWNER ADDRESS)\nProduct (PART1)\nTariff (1020.30.40)\nUQ (KG)", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(true, owner.MainAddress, warehouse.MainAddress, manufacturer.MainAddress, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse (WHS123:OFC: WAREHOUSE ADDRESS, FIRMS:)\nOwner (OWN123:OFC: OWNER ADDRESS)\nManufacturer (MAN123:OFC: MANUFACTURER ADDRESS)\nTariff (1020.30.40)\nUQ (KG)\nCountry Of Origin (NZ)\nZone Status (D)", provider.GetMatchingCriteria(permitCriteria));
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			permitCriteria = new PermitCriteria(false, owner.MainAddress, warehouse.MainAddress, manufacturer.MainAddress, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse (WHS123:OFC: WAREHOUSE ADDRESS, FIRMS:KD32)\nOwner (OWN123:OFC: OWNER ADDRESS)\nProduct (PART1)\nTariff (1020.30.40)\nUQ (KG)", provider.GetMatchingCriteria(permitCriteria));
			permitCriteria = new PermitCriteria(true, owner.MainAddress, warehouse.MainAddress, manufacturer.MainAddress, "KG", "NZ", "PART1", "10203040", ZoneStatusList.Codes.Domestic);
			AssertMultilineASCIIEquals("Type (FTZ)\nWarehouse (WHS123:OFC: WAREHOUSE ADDRESS, FIRMS:KD32)\nOwner (OWN123:OFC: OWNER ADDRESS)\nManufacturer (MAN123:OFC: MANUFACTURER ADDRESS)\nTariff (1020.30.40)\nUQ (KG)\nCountry Of Origin (NZ)\nZone Status (D)", provider.GetMatchingCriteria(permitCriteria));
		}

		public void TestMatchSimple()
		{
			var owner1 = Factory.New<OrgHeader>();
			var owner2 = Factory.New<OrgHeader>();
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.Australia);
			var manufacturer = Factory.New<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var header1Simple = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-1", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header1Simple, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header1Simple, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header2SimpleAU = helper.CreatePermitHeader(Core.Constants.CountryCodes.Australia, owner1.PK, "PERMIT-2", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header2SimpleAU, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header2SimpleAU, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header3SimpleDiffTariff = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-3", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header3SimpleDiffTariff, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header3SimpleDiffTariff, USPermitRuleCodeList.Codes.TAR, "2820", "2900");
			var header4SimpleOwner2 = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner2.PK, "PERMIT-4", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header4SimpleOwner2, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header4SimpleOwner2, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header5SimpleOld = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-5", ZDate.Today.AddMonths(-2), ZDate.Today.AddMonths(-1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header5SimpleOld, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header5SimpleOld, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header6SimpleTooNew = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-6", ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(2), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header6SimpleTooNew, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header6SimpleTooNew, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header7SimpleDiffUOM = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-7", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "PK");
			helper.CreatePermitRule(header7SimpleDiffUOM, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header7SimpleDiffUOM, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header8SimpleDiffFirm = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-8", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header8SimpleDiffFirm, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD67", "");
			helper.CreatePermitRule(header8SimpleDiffFirm, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var header9SimpleDiffProduct = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-9", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header9SimpleDiffProduct, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header9SimpleDiffProduct, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header9SimpleDiffProduct, USPermitRuleCodeList.Codes.PRD, "PART2", "");
			var header10SimpleWithProduct = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-10", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header10SimpleWithProduct, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header10SimpleWithProduct, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header10SimpleWithProduct, USPermitRuleCodeList.Codes.PRD, "PART1", "");
			var header11SimpleException = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-11", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(header11SimpleException, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			var header11SimpleExceptionTariffRule = helper.CreatePermitRule(header11SimpleException, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRuleException(header11SimpleExceptionTariffRule, "2710", "2800");
			var header12WithCountryOfOrigin = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-12", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header12WithCountryOfOrigin, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header12WithCountryOfOrigin, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header12WithCountryOfOrigin, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			var header13WithZoneStatus = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-13", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header13WithZoneStatus, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header13WithZoneStatus, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header13WithZoneStatus, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header14SimpleWithManufacturer = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-1", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header14SimpleWithManufacturer, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header14SimpleWithManufacturer, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			IPermitWithdrawRequestProvider provider = new FTZPermitWithdrawRequestProvider();
			var permitCriteria = new PermitCriteria(false, owner1.MainAddress, warehouse1.MainAddress, manufacturer.MainAddress, "NO", "#@", "PART1", "2720", ZoneStatusList.Codes.Domestic);
			var permits = provider.FindMatchingPermit(permitCriteria);
			AssertEquals(1, permits.Length);
			AssertEquals("Matching Product and Tariff", header10SimpleWithProduct, permits[0]);
			permitCriteria = new PermitCriteria(false, owner1.MainAddress, warehouse1.MainAddress, manufacturer.MainAddress, "NO", "#@", "PART3", "2720", ZoneStatusList.Codes.Domestic);
			permits = provider.FindMatchingPermit(permitCriteria);
			AssertEquals(1, permits.Length);
			AssertEquals("Matching Tariff Only", header1Simple, permits[0]);
		}

		public void TestFindMatchingPermit_WithClosedPermit()
		{
			var owner1 = Factory.New<OrgHeader>();
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.Australia);
			var manufacturer = Factory.New<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var activePermit = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-1", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, Enterprise.Customs.US.Business.PermitTypeList.Codes.FTZ, uom: "NO");
			helper.CreatePermitRule(activePermit, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(activePermit, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			var closedPermit = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-2", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, Enterprise.Customs.US.Business.PermitTypeList.Codes.FTZ, uom: "NO");
			closedPermit.CPH_IsClosed = true;
			helper.CreatePermitRule(closedPermit, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(closedPermit, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			IPermitWithdrawRequestProvider provider = new FTZPermitWithdrawRequestProvider();
			var permitCriteria = new PermitCriteria(false, owner1.MainAddress, warehouse1.MainAddress, manufacturer.MainAddress, "NO", "#@", "PART1", "2720", ZString.Empty);
			var permits = provider.FindMatchingPermit(permitCriteria);
			AssertEquals(1, permits.Length);
			AssertSame(permits[0], activePermit);
		}

		public void TestFineMatchWithApplicationCode()
		{
			var owner1 = Factory.New<OrgHeader>();
			owner1.OH_Code = "ON12";
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WH12";
			warehouse1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.OH_Code = "WH22";
			warehouse2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.Australia);
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MF12";
			var helper = new PermitTestDataHelper(Factory);
			var header1US = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMUS-1", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header1US, USPermitRuleCodeList.Codes.FRM, "KD32", "");
			helper.CreatePermitRule(header1US, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header1US, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header1US, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header2AU = helper.CreatePermitHeader(Core.Constants.CountryCodes.Australia, owner1.PK, "PERMAU-2", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.FRM, "KD32", "");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header3US = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMUS-3", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(-1), PermitQtyValIndicatorList.Codes.BTH, USPermitRuleCodeList.Codes.TAR, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header3US, USPermitRuleCodeList.Codes.FRM, "KD32", "");
			helper.CreatePermitRule(header3US, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header3US, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header3US, USPermitRuleCodeList.Codes.ZST, "D", "D");
			Factory.Save();
			IPermitWithdrawRequestProvider provider = new FTZPermitWithdrawRequestProvider();
			var permitCriteria = new PermitCriteria(true, owner1.MainAddress, warehouse1.MainAddress, manufacturer.MainAddress, "NO", "IT", "PART1", "2720", ZoneStatusList.Codes.Domestic);
			var permits = provider.FindMatchingPermit(permitCriteria);
			AssertEquals(1, permits.Length);
			AssertEquals("Matching Tariff Only", header1US, permits[0]);
			AssertEquals("Matched with ApplicationCode PER", CusPermitHeaderApplicationCodeList.Codes.Permit, permits[0].CPH_ApplicationCode);
			AssertEquals("Matched with Country US", Core.Constants.CountryCodes.UnitedStates, permits[0].CPH_RN_NKCountryCode);
			AssertEquals("Matched with Type FTZ", PermitTypeList.Codes.FTZ, permits[0].CPH_Type);
		}

		public void TestMatchDetail()
		{
			var owner1 = Factory.New<OrgHeader>();
			var owner2 = Factory.New<OrgHeader>();
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			var warehouse2 = Factory.New<OrgHeader>();
			warehouse2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.Australia);
			var manufacturer = Factory.New<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var header1 = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-1", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header1, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header1, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header1, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header1, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header2AU = helper.CreatePermitHeader(Core.Constants.CountryCodes.Australia, owner1.PK, "PERMIT-2", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header2AU, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header2AU, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header3DiffTariff = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-3", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header3DiffTariff, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header3DiffTariff, USPermitRuleCodeList.Codes.TAR, "2820", "2900");
			helper.CreatePermitRule(header3DiffTariff, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header3DiffTariff, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header4Owner2 = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner2.PK, "PERMIT-4", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header4Owner2, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header4Owner2, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header4Owner2, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header4Owner2, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header5Old = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-5", ZDate.Today.AddMonths(-2), ZDate.Today.AddMonths(-1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header5Old, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header5Old, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header5Old, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header5Old, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header6TooNew = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-6", ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(2), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header6TooNew, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header6TooNew, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header6TooNew, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header6TooNew, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header7DiffUOM = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-7", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "PK", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header7DiffUOM, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header7DiffUOM, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header7DiffUOM, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header7DiffUOM, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header8DiffFirm = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-8", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header8DiffFirm, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD67", "");
			helper.CreatePermitRule(header8DiffFirm, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header8DiffFirm, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header8DiffFirm, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header9DiffProduct = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-9", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header9DiffProduct, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header9DiffProduct, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header9DiffProduct, USPermitRuleCodeList.Codes.PRD, "PART2", "");
			helper.CreatePermitRule(header9DiffProduct, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header9DiffProduct, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header10WithProduct = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-10", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header10WithProduct, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header10WithProduct, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header10WithProduct, USPermitRuleCodeList.Codes.PRD, "PART1", "");
			helper.CreatePermitRule(header10WithProduct, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header10WithProduct, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header11Exception = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-11", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header11Exception, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header11Exception, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header11Exception, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header11SimpleExceptionTariffRule = helper.CreatePermitRule(header11Exception, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRuleException(header11SimpleExceptionTariffRule, "2710", "2800");
			var header12DiffCountryOfOrigin = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-12", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header12DiffCountryOfOrigin, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header12DiffCountryOfOrigin, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header12DiffCountryOfOrigin, USPermitRuleCodeList.Codes.COO, "DE", "DE");
			helper.CreatePermitRule(header12DiffCountryOfOrigin, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header13DiffZoneStatus = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-13", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header13DiffZoneStatus, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header13DiffZoneStatus, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header13DiffZoneStatus, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			helper.CreatePermitRule(header13DiffZoneStatus, USPermitRuleCodeList.Codes.ZST, "F", "F");
			var header14NoCountryOfOrigin = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-14", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header14NoCountryOfOrigin, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header14NoCountryOfOrigin, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header14NoCountryOfOrigin, USPermitRuleCodeList.Codes.ZST, "D", "D");
			var header15NoZoneStatus = helper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner1.PK, "PERMIT-15", ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1), PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.FTZ, uom: "NO", appliesToAddressPK: manufacturer.MainAddress.PK);
			helper.CreatePermitRule(header15NoZoneStatus, FTZPermitWithdrawRequestProvider.FirmRuleCode, "KD32", "");
			helper.CreatePermitRule(header15NoZoneStatus, USPermitRuleCodeList.Codes.TAR, "2020", "3000");
			helper.CreatePermitRule(header15NoZoneStatus, USPermitRuleCodeList.Codes.COO, "IT", "IT");
			IPermitWithdrawRequestProvider provider = new FTZPermitWithdrawRequestProvider();
			var permitCriteria = new PermitCriteria(true, owner1.MainAddress, warehouse1.MainAddress, manufacturer.MainAddress, "NO", "IT", "PART1", "2720", ZoneStatusList.Codes.Domestic);
			var permits = provider.FindMatchingPermit(permitCriteria);
			AssertEquals(1, permits.Length);
			AssertEquals("Matching Tariff Only", header1, permits[0]);
			AssertEquals("Matched with ApplicationCode PER", CusPermitHeaderApplicationCodeList.Codes.Permit, permits[0].CPH_ApplicationCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_ = Env.Registry.CanUserEditOrganisationCode;
			Env.Registry.CanUserEditOrganisationCode = false;
			var algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			_ = algorithm.RegenerateOrgCodeOnChanges;
			algorithm.RegenerateOrgCodeOnChanges = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
		}

		OrgAddress GetCRAHOU(BusinessObjectFactory factory) => GetAddress(factory, "CRAHOU");

		OrgAddress GetINTTEL(BusinessObjectFactory factory) => GetAddress(factory, "INTTEL");

		OrgAddress GetAddress(BusinessObjectFactory factory, ZString orgCode) => factory?.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode)?.MainAddress;

		void AssertResponse(PermitService.IPermitWithdrawRequestResponse response, PermitService.SuccessOrFailure successOrFailure, ZString failureReason, ZDecimal? qty, ZString? outwardEntryNumber)
		{
			AssertEquals("FailureReason", failureReason, response.FailureReason);
			AssertResponse(response, successOrFailure, qty, outwardEntryNumber);
		}

		void AssertPermit(BaseCusPermitHeader header, ZDecimal qty, ZDecimal value)
		{
			AssertEquals("QuantityBalance", qty, header.QuantityBalance);
			AssertEquals("ValueBalance", value, header.ValueBalance);
		}

		void AssertResponse(PermitService.IPermitWithdrawRequestResponse response, PermitService.SuccessOrFailure successOrFailure, ZDecimal? qty, ZString? outwardEntryNumber)
		{
			AssertEquals("SuccessOrFailure", successOrFailure, response.SuccessOrFailure);
			AssertEquals("AvailableQty", qty, response.AvailableQty);
			AssertEquals("OutwardEntryNumber", outwardEntryNumber, response.OutwardEntryNumber);
		}
	}
}
