using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PermitHelperTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			supplier = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "REF1";

			outgoingMessage = Factory.NewWithValidTestData<EDIMessage>();
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_MessageNum = "OU1";

			outgoingMessage2 = Factory.NewWithValidTestData<EDIMessage>();
			outgoingMessage2.EM_LinkedObject = entryHeader;
			outgoingMessage2.EM_MessageNum = "OU2";

			incomingMessage = Factory.NewWithValidTestData<EDIMessage>();
			incomingMessage.EM_MessageNum = "APPID";
		}

		[TestDate(2016, 6, 01)]
		public void TestRollbackPermitTransactionsIfSendingCancel()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, ZString.Empty, "IMP", "", 1000m, 1000m);
			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			Factory.Save();

			PermitHelper.RollbackPermitTransactionsIfSendingCancel(outgoingMessage, null, (msg) => msg.EM_MessageNum, "Procedure", Core.Constants.CountryCodes.France);

			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "OU1", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("one transaction for message (1 rollback)", 1, transactions.Length);
		}

		[TestDate(2016, 6, 01)]
		public void TestRollbackManagedPermitTransaction()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, ZString.Empty, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			Factory.Save();

			PermitHelper.RollbackPermitTransactions(incomingMessage, outgoingMessage, null, (msg) => msg.EM_MessageNum, "Procedure", Core.Constants.CountryCodes.France, false);

			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("one transaction for message (1x outoing + 0x rollback", 1, transactions.Length);

			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;

			Factory.Save();

			PermitHelper.RollbackPermitTransactions(incomingMessage, outgoingMessage, null, (msg) => msg.EM_MessageNum, "procedure", Core.Constants.CountryCodes.France, false);

			query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("one transaction for message (1x outoing + 1x rollback", 2, transactions.Length);
		}

		[TestDate(2016, 6, 01)]
		public void TestAddManagedPermitRecordsToCancel()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);

			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);
			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			var permit2 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP4321", startDate.Date, endDate.Date, ZString.Empty, "IMP", "", 1000m, 1000m);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit2, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			Factory.Save();

			var permitRecords = new List<PermitRecord>();
			PermitHelper.AddPermitRecordsToCancel(Factory, Core.Constants.CountryCodes.France, "REF1", 0, permitRecords);
			AssertEquals("Only one managed permit was cancelled.", 1, permitRecords.Count);
			AssertEquals("IMP1234", permitRecords[0].PermitHeader.CPH_Number);
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdateManagedPermitPendingTransactions()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);

			var permit2 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.France, permitHolder.PK, "IMP4321", startDate.Date, endDate.Date, ZString.Empty, "IMP", "", 1000m, 1000m);

			var transaction2 = permitHelper.CreatePermitLineTransaction(permit2, "REF2", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);

			Factory.Save();

			PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, (msg) => msg.EM_MessageNum, Core.Constants.CountryCodes.France, false, PermitTransactionStatusList.Codes.Deleted);

			var query1 = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var query2 = permitHelper.GetPermitLineTransactionQuery("REF2", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);

			Factory.Save();
			var transactions1 = Factory.Load<BaseCusPermitLineTransaction>(query1);
			var transactions2 = Factory.Load<BaseCusPermitLineTransaction>(query2);
			AssertEquals("Permit 1 is managed (with QtyValIndicator as BTH), the transactions can be updated.", PermitTransactionStatusList.Codes.Deleted, transactions1[0].CPL_TransactionStatus);
			AssertEquals("Permit 2 is not managed (with QtyValIndicator as empty), the transactions can not be updated.", PermitTransactionStatusList.Codes.Pending, transactions2[0].CPL_TransactionStatus);
		}

		[TestDate(2016, 6, 01)]
		public void TestRollbackPermitTransaction()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "CMT1", "", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var transaction3 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			Factory.Save();

			PermitHelper.RollbackPermitTransactions(incomingMessage, outgoingMessage, null, (msg) => msg.EM_MessageNum, "PROCEDURE", Core.Constants.CountryCodes.SouthAfrica, false);

			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("Two transactions for message (1x outoing + 1x rollback", 2, transactions.Length);
			AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
			AssertEquals("DTI2014/7656 Rollback Value = 50m", 50m, transactions[1].CPL_TranValue);
		}

		[TestDate(2016, 6, 01)]
		public void TestRollbackPermitTransactionsForReferenceWithAdditionalCriteria()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "CMT1", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var transaction3 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			Factory.Save();

			AutoCusPermitLineTransaction transaction = null;
			PermitHelper.RollbackPermitTransactionsForReferenceWithAdditionalCriteria(Factory, Core.Constants.CountryCodes.SouthAfrica, "REF1", "ROLLBACK", "APPID", "PROCEDURE", x => x.CPL_Comment == "CMT1", trans =>
			{
				transaction = trans;
			});

			AssertNotNull("Rollback Transaction Created", transaction);
			AssertEquals("CPL_TranValue", 50m, transaction.CPL_TranValue);
			AssertEquals("CPL_TranQty", 50m, transaction.CPL_TranQty);

			Factory.Save();

			var rollbackTransactions = Factory.Load<BaseCusPermitLineTransaction>(new ZQuery(CusPermitLineTransactionSchema.CPL_Comment, "ROLLBACK"));
			AssertEquals("Only One Rollback Transaction", 1, rollbackTransactions.Length);
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatePendingTransactions()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "CMT1", "", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction3 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);

			Factory.Save();

			PermitHelper.UpdatePendingTransactions(incomingMessage, outgoingMessage, (msg) => msg.EM_MessageNum, Core.Constants.CountryCodes.SouthAfrica, false, PermitTransactionStatusList.Codes.Confirmed);

			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactions[0].CPL_TransactionStatus);
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatePendingTransactionsFactoryParameterOverload()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "CMT1", "", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction3 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);

			Factory.Save();

			PermitHelper.UpdatePendingTransactions(Factory, outgoingMessage, (msg) => msg.EM_MessageNum, Core.Constants.CountryCodes.SouthAfrica, false, PermitTransactionStatusList.Codes.Confirmed);

			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "Customs Entry", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactions[0].CPL_TransactionStatus);
		}

		[TestDate(2016, 6, 01)]
		public void TestUpdatePendingTransactionsWithAdditionalCriteria()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "CMT-PND", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction3 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Pending);
			var transaction4 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "CMT-DEL", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m, PermitTransactionStatusList.Codes.Deleted);

			Factory.Save();

			PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(Factory, Core.Constants.CountryCodes.SouthAfrica, "REF1", x => x.CPL_Comment == "CMT-PND", PermitTransactionStatusList.Codes.Confirmed);
			PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(Factory, Core.Constants.CountryCodes.SouthAfrica, "REF1", x => x.CPL_Comment == "CMT-DEL", PermitTransactionStatusList.Codes.Confirmed);

			Factory.Save();
			var query = permitHelper.GetPermitLineTransactionQuery("REF1", "CMT-PND", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals("CON", transactions[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("REF1", "CMT-DEL", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals("DEL", transactions[0].CPL_TransactionStatus);
		}

		[TestDate(2016, 6, 01)]
		public void TestAddPermitRecordsToCancel()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "IMP", "", 1000m, 1000m);

			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, "REF2", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);

			Factory.Save();

			var permitRecords = new List<PermitRecord>();
			PermitHelper.AddPermitRecordsToCancel(Factory, Core.Constants.CountryCodes.SouthAfrica, "REF1", 0, permitRecords);
			AssertEquals("One permit record added", 1, permitRecords.Count);
			AssertEquals("Permit Header", permit1, permitRecords.First().PermitHeader);
			AssertEquals("Quantity", 0m, permitRecords.First().Quantity);
			AssertEquals("Value", 0m, permitRecords.First().Value);
		}

		[TestDate(2016, 6, 01)]
		public void TestIsPermitBeenUsed()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(Core.Constants.CountryCodes.UnitedStates, permitHolder.PK, "IMP1234", startDate.Date, endDate.Date, PermitQtyValIndicatorList.Codes.BTH, "FTZ", "", 1000m, 1000m);
			var transaction1 = permit1.CusPermitLineTransactions[0];
			transaction1.CPL_Reference = "XJ5-00003877";
			Factory.Save();

			Assert("Permit Hasn't Been Used", !PermitHelper.IsPermitInUsed(Factory, Core.Constants.CountryCodes.UnitedStates, "FTZ", "XJ5-00003877"));

			permitHelper.CreatePermitLineTransaction(permit1, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();

			Assert("Permit Has Been Used", PermitHelper.IsPermitInUsed(Factory, Core.Constants.CountryCodes.UnitedStates, "FTZ", "XJ5-00003877"));
		}

		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		EDIMessage outgoingMessage2;
		EDIMessage incomingMessage;
		OrgHeader importer;
		OrgHeader supplier;
	}
}
