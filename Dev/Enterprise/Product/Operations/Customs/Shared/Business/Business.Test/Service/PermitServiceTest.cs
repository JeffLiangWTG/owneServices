using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Service.Testing
{
	sealed class PermitServiceTest : TestCaseWithFactory
	{
		public void TestPermitServiceCanBeInstantiatedByObjectFactory()
		{
			var result = ObjectFactory.Get<IPermitService>();
			AssertEquals(typeof(PermitService), result.GetType());
		}

		public void TestIsPermitAvailable()
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
				var header2 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-6), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3432");
				var header3 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT3", ZDate.Today.AddMonths(-7), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "KG", "SJ5-ENT3433");
				var header4 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, owner.OA_OH, "PERMIT4", ZDate.Today.AddMonths(-8), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3434");
				var header5 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT5", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3435");
				var header6 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT6", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3436");

				dataHelper.CreatePermitRule(header1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header2, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header3, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header4, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header5, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header6, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header2, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header3, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header4, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header5, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");
				dataHelper.CreatePermitRule(header6, BaseCusPermitRule.RuleCodes.Tariff, "3020304050", "3020304050");
				dataHelper.CreatePermitLineTransaction(header6, "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);
				factory.Save();
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				var request3 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-3");
				request3.Tariff = "2020304050";
				var request4 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-4");
				request4.Tariff = "3020304050";
				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request1, request2, request4 });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 3, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[2], SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits, "There is already a pending request for 'ORD3242-4'.", 150m, null);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				var permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 100m, 1000m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);

				header5 = factory.Load<BaseCusPermitHeader>(header5.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header5.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3435", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header6 = factory.Load<BaseCusPermitHeader>(header6.PK);
				AssertPermit(header6, 150m, 1500m);
				permitLines = header6.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3436", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 100m, 1000m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);

				header5 = factory.Load<BaseCusPermitHeader>(header5.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header5.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3435", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header6 = factory.Load<BaseCusPermitHeader>(header6.PK);
				AssertPermit(header6, 150m, 1500m);
				permitLines = header6.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3436", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);
			}
		}

		public void TestIsPermitAvailable_WithClosedPermit()
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

				var activePermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				var closedPermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3432");
				closedPermit.CPH_IsClosed = true;

				dataHelper.CreatePermitRule(activePermit, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(closedPermit, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(activePermit, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(closedPermit, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");

				factory.Save();

				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request1.Qty = 80;
				request1.Tariff = "1020304050";

				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				request2.Tariff = "2020304050";

				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = response.Responses.ToArray();

				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.Failure, null, null, null);
				AssertStartsWith("error message", System.Environment.NewLine + "Type (FTZ)", responses[1].FailureReason);
			}
		}

		public void TestPermitQuantityAndValue()
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

				var permit1 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-4), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 400m, "NO", "SJ5-ENT3431");
				var permit2 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-3), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 200m, "NO", "SJ5-ENT3432");

				dataHelper.CreatePermitRule(permit1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(permit2, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(permit1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(permit2, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");

				factory.Save();

				var request = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request.Qty = 300;
				request.Tariff = "1020304050";

				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request });
				var responses = response.Responses.ToArray();

				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Weekly Estimate match found but remaining quantity is insufficient.\r\nWeekly Estimate match found but remaining value is insufficient.\r\nMatches found:\r\nPermit number: PERMIT1, Quantity left: 200.00 NO, Value left: 400.00 USD\r\nPermit number: PERMIT2, Quantity left: 100.00 NO, Value left: 200.00 USD", 300, null);

				request.Qty = 200;

				response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();

				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Weekly Estimate match found but remaining value is insufficient.\r\nMatches found:\r\nPermit number: PERMIT1, Quantity left: 200.00 NO, Value left: 400.00 USD\r\nPermit number: PERMIT2, Quantity left: 100.00 NO, Value left: 200.00 USD", 300, null);

				request.Qty = 30;

				response = permitService.IsPermitAvailable(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();

				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
			}
		}

		[TestDate(2017, 9, 4)]
		public void TestTryGetPermits()
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
				var header2 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-6), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3432");
				var header3 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT3", ZDate.Today.AddMonths(-7), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "KG", "SJ5-ENT3433");
				var header4 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, owner.OA_OH, "PERMIT4", ZDate.Today.AddMonths(-8), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3434");
				var header5 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT5", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3435");
				var header6 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT6", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3436");
				var header7 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT7", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3437");

				dataHelper.CreatePermitRule(header1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header2, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header3, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header4, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header5, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header6, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header7, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header2, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header3, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header4, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header5, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");
				dataHelper.CreatePermitRule(header6, BaseCusPermitRule.RuleCodes.Tariff, "3020304050", "3020304050");
				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "7020304050", "7020304050");
				dataHelper.CreatePermitLineTransaction(header6, "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);
				factory.Save();
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				var request3 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-3");
				request3.Tariff = "2020304050";
				var request4 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-4");
				request4.Tariff = "3020304050";
				var request7 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-7");
				request7.Tariff = "7020304050";
				request7.AddInfo = "ZoneStatus=D*BOB=WHERE";
				request7.ZoneStatus = "D";

				var permitService = ObjectFactory.Get<IPermitService>();
				IPermitWithdrawRequestResponseResult response;
				try
				{
					AssertEquals(true, header5.LockMutex);
					response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2, request3 });
				}
				finally
				{
					header5.UnlockMutex();
				}

				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 3, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[2], SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits, 200m, null);
				AssertContains("responses[2].FailureReason", ") is in the middle of modifying this permit 'PERMIT5'.", responses[2].FailureReason);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				var permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 100m, 1000m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);

				header5 = factory.Load<BaseCusPermitHeader>(header5.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header5.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3435", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header6 = factory.Load<BaseCusPermitHeader>(header6.PK);
				AssertPermit(header6, 150m, 1500m);
				permitLines = header6.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3436", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2, request4 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 3, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[2], SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits, "There is already a pending request for 'ORD3242-4'.", 150m, null);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 100m, 1000m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);

				header5 = factory.Load<BaseCusPermitHeader>(header5.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header5.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 1, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3435", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);

				header6 = factory.Load<BaseCusPermitHeader>(header6.PK);
				AssertPermit(header6, 150m, 1500m);
				permitLines = header6.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3436", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-4", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				request1.Owner = Factory.New<OrgHeader>().MainAddress;
				request1.Qty = 10;
				request1.PermitTransactionRefNumber = "ORD3242-3";
				request2.PermitTransactionRefNumber = "ORD3242-4";
				request2.Qty = 150m;

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Owner.", null, null);
				AssertResponse(responses[1], SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits, null, 170m, null);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				request2.Qty = 80m;

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Owner.", null, null);
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				request1.Owner = GetCRAHOU(factory);
				request1.Warehouse = Factory.New<OrgHeader>().MainAddress;
				request1.Warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Warehouse.", null, null);
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request7 });
				responses = response.Responses.ToArray();
				AssertEquals("request7.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, null);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				var warehouse2 = GetWINACO(factory);
				warehouse2.Header.OH_RL_NKClosestPort = "ZAJNB";
				warehouse2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				warehouse2.OA_RL_NKRelatedPortCode = "ZAJNB";
				warehouse2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FM32");
				factory.Save();
				request1.Warehouse = warehouse2;

				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "No Provider found for Country 'ZA' and Type 'FTZ'.", null, null);
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-3");
				request1.Tariff = "2020202020";
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "\nType (FTZ)\nWarehouse (INTTEL:PST: PO BOX 1298 PRINCETO, FIRMS:FM32)\nOwner (CRAHOU:PST: PO BOX 2201)\nProduct (PART1)\nTariff (2020.20.2020)\nUQ (NO).", null, null);
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				request1.Tariff = "1020304050";
				request1.Qty = 80m;
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits, null, 90m, null);

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);
				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);
			}
		}

		public void TestTryGetPermits_WithClosedPermit()
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

				// - create 2 permits: 1 active and 1 closed with similar rules
				var activePermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				var closedPermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3432");
				closedPermit.CPH_IsClosed = true;

				dataHelper.CreatePermitRule(activePermit, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(closedPermit, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(activePermit, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(closedPermit, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");

				factory.Save();

				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request1.Qty = 80;
				request1.Tariff = "1020304050";

				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				request2.Tariff = "2020304050";

				var permitService = ObjectFactory.Get<IPermitService>();
				var serviceResponse1 = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1 });
				var serviceResponse2 = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request2 });
				var responses1 = serviceResponse1.Responses.ToArray();
				var responses2 = serviceResponse2.Responses.ToArray();

				// - check that only active permit was got successfully
				AssertEquals("responses1.Length", 1, responses1.Length);
				AssertEquals("responses2.Length", 1, responses2.Length);
				AssertResponse(responses1[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses2[0], SuccessOrFailure.Failure, null, null, null);
				AssertStartsWith("error message", System.Environment.NewLine + "Type (FTZ)", responses2[0].FailureReason);

				// - check that transactions were added only to the active permit
				Assert(activePermit.HasNonZeroOrderBalance());
				Assert(!closedPermit.HasNonZeroOrderBalance());
			}
		}

		void AssertPermit(BaseCusPermitHeader header, ZDecimal qty, ZDecimal value)
		{
			AssertEquals("QuantityBalance", qty, header.QuantityBalance);
			AssertEquals("ValueBalance", value, header.ValueBalance);
		}

		void AssertResponse(IPermitWithdrawRequestResponse response, SuccessOrFailure successOrFailure, ZString? failureReason, ZDecimal? qty, ZString? outwardEntryNumber)
		{
			if (failureReason.HasValue)
			{
				AssertMultilineASCIIEquals("FailureReason", failureReason.Value, response.FailureReason);
			}
			AssertResponse(response, successOrFailure, qty, outwardEntryNumber);
		}

		void AssertResponse(IPermitWithdrawRequestResponse response, SuccessOrFailure successOrFailure, ZDecimal? qty, ZString? outwardEntryNumber)
		{
			AssertEquals("SuccessOrFailure", successOrFailure, response.SuccessOrFailure);
			AssertEquals("AvailableQty", qty, response.AvailableQty);
			AssertEquals("OutwardEntryNumber", outwardEntryNumber, response.OutwardEntryNumber);
		}

		public void TestConfirmPermitTransactions()
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
				var header2 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-6), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3432");
				var header3 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-4), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 300m, 3000m, "NO", "SJ5-ENT3433");

				dataHelper.CreatePermitRule(header1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header2, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header3, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header2, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header3, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitLineTransaction(header3, "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m);
				factory.Save();
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				var permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				var permitTransaction1 = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-1",
					Qty = 50m,
					ReceiveTotalQty = 50m,
					ReceiveTotalCustomsValue = 500m
				};
				var permitTransaction2 = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-2",
					Qty = 90m,
					ReceiveTotalQty = 90m,
					ReceiveTotalCustomsValue = 900m
				};
				var permitTransaction3 = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-3",
					Qty = 120m,
					ReceiveTotalQty = 120m,
					ReceiveTotalCustomsValue = 1200m
				};
				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(false, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[]
				{
					permitTransaction1, permitTransaction2, permitTransaction3
				}));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(false, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction3 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(false, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction2 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(true, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction1 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Requested (Qty: 50, Value: 500) - Confirmed (Qty: 50, Value: 500)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitTransaction2.ReceiveTotalCustomsValue = 700m;
				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(false, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction2 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Requested (Qty: 50, Value: 500) - Confirmed (Qty: 50, Value: 500)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitTransaction2.ReceiveTotalCustomsValue = 900m;
				permitTransaction2.ReceiveTotalQty = 70m;
				permitTransaction2.Qty = 70m;
				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(false, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction2 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Requested (Qty: 50, Value: 500) - Confirmed (Qty: 50, Value: 500)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitTransaction2.ReceiveTotalCustomsValue = 700m;
				permitService = ObjectFactory.Get<IPermitService>();
				AssertEquals(true, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction2 }));

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 130m, 1300m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-2", "Requested (Qty: 80, Value: 800) - Confirmed (Qty: 70, Value: 700)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 100m, 10m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Requested (Qty: 50, Value: 500) - Confirmed (Qty: 50, Value: 500)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);
			}
		}

		public void TestConfirmPermitTransactions_WithClosedPermit()
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

				// - create 2 permits: 1 will remain active and 1 will be closed
				var activePermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				var closedPermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3432");

				dataHelper.CreatePermitRule(activePermit, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(closedPermit, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(activePermit, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(closedPermit, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");

				factory.Save();

				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request1.Qty = 80;
				request1.Tariff = "1020304050";

				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				request2.Tariff = "2020304050";

				var permitService = ObjectFactory.Get<IPermitService>();
				var serviceResponse = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = serviceResponse.Responses.ToArray();

				// - check that both active permits was got successfully
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");

				// - check that transactions were added only to both permits
				Assert(activePermit.HasNonZeroOrderBalance());
				Assert(closedPermit.HasNonZeroOrderBalance());

				// close one of the permits
				// note: Normally this should not happen. We do not allow users to send entry summary if there are pending transactions.
				closedPermit.Close();
				factory.Save();

				var activePermitTransaction = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-1",
					Qty = 80m,
					ReceiveTotalQty = 80m,
					ReceiveTotalCustomsValue = 80m
				};
				var closedPermitTransaction = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-2",
					Qty = 80m,
					ReceiveTotalQty = 80m,
					ReceiveTotalCustomsValue = 80m
				};

				// - check that only transaction in the active permit is confirmed
				Assert("active permit transaction was confirmed", permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { activePermitTransaction }));
				Assert("closed permit transaction was not confirmed", !permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { closedPermitTransaction }));

				// - check that both permits retain non-zero order balance
				Assert(activePermit.HasNonZeroOrderBalance());
				Assert(closedPermit.HasNonZeroOrderBalance());
			}
		}

		public void TestDomesticLineIsExcluded()
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
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.AddInfo = "ZoneStatus=D*BOB=WHERE";
				request2.ZoneStatus = "D";

				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, null);

				factory = new BusinessObjectFactory();
				header = factory.Load<BaseCusPermitHeader>(header.PK);
				AssertPermit(header, 150m, 1500m);
				var permitLines = header.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				var permitTransactionRefNumber1 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-1");
				var permitTransactionRefNumber2 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-2", "D");
				permitService.RelinquishPermitTransactions(new[]
				{
					permitTransactionRefNumber1, permitTransactionRefNumber2
				});

				factory = new BusinessObjectFactory();
				header = factory.Load<BaseCusPermitHeader>(header.PK);
				AssertPermit(header, 200m, 2000m);
				permitLines = header.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 500m, 50m, ZString.Empty);

				request1.Qty = 80m;
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, null);

				factory = new BusinessObjectFactory();
				header = factory.Load<BaseCusPermitHeader>(header.PK);
				AssertPermit(header, 120m, 1200m);
				permitLines = header.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 4, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 500m, 50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[3], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				var permitTransaction1 = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-1",
					Qty = 80m,
					ReceiveTotalQty = 100m,
					ReceiveTotalCustomsValue = 1000m
				};
				var permitTransaction2 = new PermitWithdrawalRequestDetailForTesting()
				{
					Warehouse = warehouse,
					PermitTransactionRefNumber = "ORD3242-2",
					Qty = 50m,
					ReceiveTotalQty = 100m,
					ReceiveTotalCustomsValue = 1000m,
					ZoneStatus = "D"
				};
				AssertEquals(true, permitService.ConfirmPermitTransactions(new IPermitWithdrawalRequestDetail[] { permitTransaction1, permitTransaction2 }));

				factory = new BusinessObjectFactory();
				header = factory.Load<BaseCusPermitHeader>(header.PK);
				AssertPermit(header, 120m, 1200m);
				permitLines = header.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 5, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 500m, 50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[3], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[4], "ORD3242-1", "Requested (Qty: 80, Value: 800) - Confirmed (Qty: 80, Value: 800)", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 0m, 0m, ZString.Empty);
			}
		}

		public void TestRelinquishPermitTransactions()
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
				var header2 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-6), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 100m, 1000m, "NO", "SJ5-ENT3432");
				var header3 = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-4), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 300m, 3000m, "NO", "SJ5-ENT3433");

				dataHelper.CreatePermitRule(header1, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header2, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(header3, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(header1, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header2, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(header3, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitLineTransaction(header3, "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m);
				factory.Save();
				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				var permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				var permitTransactionRefNumber1 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-1");
				var permitTransactionRefNumber2 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-2");
				var permitTransactionRefNumber3 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-3");
				permitService = ObjectFactory.Get<IPermitService>();
				permitService.RelinquishPermitTransactions(new[]
				{
					permitTransactionRefNumber1, permitTransactionRefNumber2, permitTransactionRefNumber3
				});

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				permitService.RelinquishPermitTransactions(new[] { permitTransactionRefNumber3 });

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 120m, 1200m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, PermitTransactionStatusList.Codes.Pending);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				permitService.RelinquishPermitTransactions(new[] { permitTransactionRefNumber2 });

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-2", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 800m, 80m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 50m, 500m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, PermitTransactionStatusList.Codes.Pending);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);

				permitService = ObjectFactory.Get<IPermitService>();
				permitService.RelinquishPermitTransactions(new[] { permitTransactionRefNumber1 });

				factory = new BusinessObjectFactory();
				header1 = factory.Load<BaseCusPermitHeader>(header1.PK);
				AssertPermit(header1, 200m, 2000m);
				permitLines = header1.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3431", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 2000m, 200m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-2", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -800m, -80m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-2", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 800m, 80m, ZString.Empty);

				header2 = factory.Load<BaseCusPermitHeader>(header2.PK);
				AssertPermit(header2, 100m, 1000m);
				permitLines = header2.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 3, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3432", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 1000m, 100m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-1", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -500m, -50m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[2], "ORD3242-1", "Relinquished", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, 500m, 50m, ZString.Empty);

				header3 = factory.Load<BaseCusPermitHeader>(header3.PK);
				AssertPermit(header3, 180m, 1800m);
				permitLines = header3.CusPermitLineTransactions.OrderBy(x => x.CPL_TransactionDate).ToArray();
				AssertEquals("permitLines.Length", 2, permitLines.Length);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[0], "SJ5-ENT3433", "", "", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL, 3000m, 300m, ZString.Empty);
				PermitTestDataHelper.AssertPermitLineTransaction(permitLines[1], "ORD3242-3", "Requested", PermitTransactionAppIdList.Codes.WarehouseOrder, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA, -1200m, -120m, ZString.Empty);
			}
		}

		public void TestRelinquishPermitTransactions_WithClosedPermit()
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

				// - create 2 permits: 1 will remain active and 1 will be closed
				var activePermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT1", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3431");
				var closedPermit = dataHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, owner.OA_OH, "PERMIT2", ZDate.Today.AddMonths(-5), ZDate.Today.AddYears(1), PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 200m, 2000m, "NO", "SJ5-ENT3432");

				dataHelper.CreatePermitRule(activePermit, "FRM", "FM32", "FM32");
				dataHelper.CreatePermitRule(closedPermit, "FRM", "FM32", "FM32");

				dataHelper.CreatePermitRule(activePermit, BaseCusPermitRule.RuleCodes.Tariff, "1020304050", "1020304050");
				dataHelper.CreatePermitRule(closedPermit, BaseCusPermitRule.RuleCodes.Tariff, "2020304050", "2020304050");

				factory.Save();

				var request1 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-1");
				request1.Qty = 80;
				request1.Tariff = "1020304050";

				var request2 = PermitWithdrawRequestForTesting.NewWithTestData(factory, "ORD3242-2");
				request2.Qty = 80;
				request2.Tariff = "2020304050";

				var permitService = ObjectFactory.Get<IPermitService>();
				var serviceResponse = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request1, request2 });
				var responses = serviceResponse.Responses.ToArray();

				// - check that both active permits was got successfully
				AssertEquals("responses.Length", 2, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Success, null, null, "SJ5-ENT3431");
				AssertResponse(responses[1], SuccessOrFailure.Success, null, null, "SJ5-ENT3432");

				// - check that transactions were added only to both permits
				Assert(activePermit.HasNonZeroOrderBalance());
				Assert(closedPermit.HasNonZeroOrderBalance());

				// close one of the permits
				// note: Normally this should not happen. We do not allow users to send entry summary if there are pending transactions.
				closedPermit.Close();
				factory.Save();
				var permitTransactionRefNumber1 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-1");
				var permitTransactionRefNumber2 = PermitTransactionDetailForTesting.NewWithTestData(factory, "ORD3242-2");

				// - check that only transaction in the active permit is relinquished
				AssertEquals("active permit transaction was relinquished", SuccessOrFailure.Success, permitService.RelinquishPermitTransactions(new[] { permitTransactionRefNumber1 }));
				AssertEquals("closed permit transaction was not relinquished", SuccessOrFailure.Failure, permitService.RelinquishPermitTransactions(new[] { permitTransactionRefNumber2 }));

				// - check that only closed permit retain non-zero order balance
				Assert(!activePermit.HasNonZeroOrderBalance());
				Assert(closedPermit.HasNonZeroOrderBalance());
			}
		}

		[TestDate(2017, 9, 4)]
		public void TestHandleUnmatchingOrganisation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var request = PermitWithdrawRequestForTesting.NewWithTestData(null);
				request.DetailedTrackingEnabled = true;
				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Owner.\r\nUnable to match Warehouse.", null, null);

				var factory = new BusinessObjectFactory();
				request.Owner = GetCRAHOU(factory);
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Warehouse.", null, null);

				var warehouse = GetINTTEL(factory);
				warehouse.Header.OH_RL_NKClosestPort = "USLAX";
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				warehouse.OA_RL_NKRelatedPortCode = "USLAX";
				request.Warehouse = warehouse;
				request.Manufacturer = Factory.New<OrgHeader>().MainAddress;
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "Unable to match Manufacturer.", null, null);

				request.Manufacturer = GetWINACO(factory);
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "\nType (FTZ)\nWarehouse (INTTEL:PST: PO BOX 1298 PRINCETO, FIRMS:)\nOwner (CRAHOU:PST: PO BOX 2201)\nManufacturer (WINACO:Pickup and Delivery Addre)\nTariff (1020.30.4050)\nUQ (NO)\nCountry Of Origin (AU)\nZone Status ().", null, null);
			}
		}

		public void TestNoSuportForPermitTypeAndCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var factory = new BusinessObjectFactory();
				var owner = GetCRAHOU(factory);
				var warehouse = GetINTTEL(factory);
				warehouse.Header.OH_RL_NKClosestPort = "AUSYD";
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				warehouse.OA_RL_NKRelatedPortCode = "AUSYD";
				warehouse.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FM32");
				factory.Save();
				var request = PermitWithdrawRequestForTesting.NewWithTestData(Factory);
				request.Warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				var permitService = ObjectFactory.Get<IPermitService>();
				var response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				var responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "No Provider found for Country 'AU' and Type 'FTZ'.", null, null);

				warehouse.Header.OH_RL_NKClosestPort = "USLAX";
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				warehouse.OA_RL_NKRelatedPortCode = "USLAX";
				factory.Save();
				request.Warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				request.PermitType = PermitType.Unknown;
				permitService = ObjectFactory.Get<IPermitService>();
				response = permitService.TryGetPermits(new IPermitWithdrawRequest[] { request });
				responses = response.Responses.ToArray();
				AssertEquals("responses.Length", 1, responses.Length);
				AssertResponse(responses[0], SuccessOrFailure.Failure, "No Provider found for Country 'US' and Type 'Unknown'.", null, null);
			}
		}

		internal static OrgAddress GetCRAHOU(BusinessObjectFactory factory)
		{
			return GetAddress(factory, "CRAHOU");
		}

		protected override void SetUp()
		{
			base.SetUp();
			canUserEditOrganisationCodeOldValue = Env.Registry.CanUserEditOrganisationCode;
			Env.Registry.CanUserEditOrganisationCode = false;

			var algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			regenerateOrgCodeOnChangesOldValue = algorithm.RegenerateOrgCodeOnChanges;
			algorithm.RegenerateOrgCodeOnChanges = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
		}
		bool canUserEditOrganisationCodeOldValue;
		bool regenerateOrgCodeOnChangesOldValue;

		protected override void TearDown()
		{
			Env.Registry.CanUserEditOrganisationCode = canUserEditOrganisationCodeOldValue;
			var algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.RegenerateOrgCodeOnChanges = regenerateOrgCodeOnChangesOldValue;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			base.TearDown();
		}

		internal static OrgAddress GetINTTEL(BusinessObjectFactory factory)
		{
			return GetAddress(factory, "INTTEL");
		}

		internal static OrgAddress GetWINACO(BusinessObjectFactory factory)
		{
			return GetAddress(factory, "WINACO");
		}

		internal static OrgAddress GetAddress(BusinessObjectFactory factory, ZString orgCode)
		{
			return factory?.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode)?.MainAddress;
		}
	}
}
