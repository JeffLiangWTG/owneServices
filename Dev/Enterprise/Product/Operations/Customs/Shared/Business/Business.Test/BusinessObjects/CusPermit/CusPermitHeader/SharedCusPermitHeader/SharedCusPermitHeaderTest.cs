using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitHeaderTest<TSharedCusPermitHeader> : EnterpriseBusinessObjectTestCase
			where TSharedCusPermitHeader : SharedCusPermitHeader
	{
		public void TestDoActionWithMutexLock_DoAction()
		{
			var mock = Factory.NewMoq<TSharedCusPermitHeader>();
			var countReloadLatestTransactions = 0;
			mock.Setup(m => m.ReloadLatestTransactions()).Callback(() => countReloadLatestTransactions++);
			var countDoAction = 0;
			var header = mock.Object;
			CombineAssertions(() =>
			{
				header.DoActionWithMutexLock(() =>
				{
					AssertEquals("HasLock", true, header.Mutex.HasLock);
					countDoAction++;
				}, null);
				AssertEquals("ReloadLatestTransactions Called", 1, countReloadLatestTransactions);
				AssertEquals("DoAction called", 1, countDoAction);
				AssertEquals("Unlocked", false, header.Mutex.IsLocked);
			});
		}

		public void TestDoActionWithMutexLock_UnlockMutexAnyWay()
		{
			try
			{
				PermitHeader.DoActionWithMutexLock(() => throw new Exception("BlaBla"), null);
			}
			catch (Exception)
			{
			}
			AssertEquals(false, PermitHeader.Mutex.IsLocked);
		}

		public void TestDoActionWithMutexLock_Notifier()
		{
			var header = PermitHeader;
			var countDoAction = 0;
			var message = string.Empty;
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "CPH" + header.PK))
			{
				mutex.Lock();
				header.DoActionWithMutexLock(() => countDoAction++, m => message = m);
				CombineAssertions(() =>
				{
					AssertEquals("DoAction not called", 0, countDoAction);
					AssertContains("Notifier", "is already in the process of adding transaction", message);
				});
			}
		}

		public void TestAddTransaction_WillNotCheckTransactionBursting_OBL()
		{
			var messages = new List<ZString>();
			void Notifier(ZString message, ZDecimal value)
			{
				messages.Add(message);
			}

			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL, notifier: Notifier);
			AssertEquals(0, messages.Count);
		}

		public void TestAddTransaction_CheckTransactionBursting_NotOBL()
		{
			var messages = new List<ZString>();
			void Notifier(ZString message, ZDecimal value)
			{
				messages.Add(message);
			}

			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, -100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.CUS, notifier: Notifier);
			AssertContains("will be exceeded by 100", messages[0]);
		}

		public void TestAddTransaction_IgnoreCheckTransactionBursting_NotOBL()
		{
			var messages = new List<ZString>();
			void Notifier(ZString message, ZDecimal value)
			{
				messages.Add(message);
			}

			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, -100m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.CUS, notifier: Notifier, checkBursting: false);
			AssertEquals(0, messages.Count);
		}

		public void TestMutexText()
		{
			var header = PermitHeader;
			header.CPH_Number = "12345";
			header.CPH_Type = "OBL";
			header.CPH_SubType = "JTQ";
			AssertContains($"is already in the process of adding transaction for this {ShortName}({ShortName} Holder: {PermitHeader.PermitHolder.OH_Code}, Country Code: {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}, Number: 12345, Start Date: 18-Sep-71, Type: OBL, Sub Type: JTQ). You cannot process with it until data is saved. Please try later.", header.MutexText);
		}

		public void TestIControllerIDProvider()
		{
			var header = PermitHeader;
			var controllerIDProvider = (IControllerIDProvider)header;
			AssertEquals("ControllerID", ExpectedControllerID, controllerIDProvider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK, controllerIDProvider.BusinessObjectPK);
		}

		protected abstract ControllerID ExpectedControllerID { get; }

		public void TestIHaveRequiredDocuments()
		{
			var header = PermitHeader;
			header.CPH_Number = "num1";
			header.RequiredDocuments.AddNew().EQ_DateReceived = new ZDateTimeOffset(2017, 1, 2);
			var requiredDocuments = (IHaveRequiredDocuments)header;
			AssertEquals("UniqueConsignRef", "num1", requiredDocuments.UniqueConsignRef);
			AssertEquals("HouseBill", "", requiredDocuments.HouseBill);
			AssertEquals("MasterBill", "", requiredDocuments.MasterBill);
			AssertNull("ExportBroker", requiredDocuments.ExportBroker);
			AssertEquals("TableCode", CusPermitHeaderSchema.Constants.Prefix, requiredDocuments.TableCode);
			AssertEquals("PK", header.PK, requiredDocuments.PK);
			AssertEquals("RequiredDocuments", 1, requiredDocuments.RequiredDocuments.Count);
			AssertEquals("EQ_DateReceived", new ZDateTimeOffset(2017, 1, 2), requiredDocuments.RequiredDocuments[0].EQ_DateReceived);
			AssertEquals("UltimateDocumentParent", header, requiredDocuments.UltimateDocumentParent);
			AssertEquals("Logs", header.Logs, requiredDocuments.Logs);
			AssertEquals("AdditionalRefTypes", 0, requiredDocuments.AdditionalRefTypes.Count);
		}

		public void TestDelete_NotSaved()
		{
			var header = PermitHeader;
			var rule = header.GetRulesCollection().AddNew();
			var transaction = header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero, transactionType: "");
			AssertEquals(false, rule.IsDeleted);
			AssertEquals(false, transaction.IsDeleted);
			header.Delete();
			AssertEquals(true, rule.IsDeleted);
			AssertEquals(true, transaction.IsDeleted);
		}

		public void TestDelete_IsInDatabase()
		{
			var header = PermitHeader;
			var rule = header.GetRulesCollection().AddNew();
			rule.FillWithValidTestData();
			var transaction = header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			Factory.Save();
			AssertEquals(false, rule.IsDeleted);
			AssertEquals(false, transaction.IsDeleted);
			AssertEquals(false, header.CanDelete);
			AssertExceptionThrown<CannotDeleteException>(() =>
			{
				header.Delete();
			});
		}

		public void TestReasonForNotAbleToDelete_IsInDatabase()
		{
			var header = PermitHeader;
			AssertEquals(true, header.CanDelete);
			var transaction = (AutoCusPermitLineTransaction)header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertEquals("", header.ReasonForNotAbleToDelete);
			Factory.Save();
			AssertEquals($"{PermitHeader.HumanReadableName} cannot be deleted because it has transaction allocated to it.", header.ReasonForNotAbleToDelete);
		}

		public void TestReasonForNotAbleToDelete_HasSystemGeneratedTransactions()
		{
			var header = PermitHeader;
			AssertEquals(true, header.CanDelete);
			var transaction = (AutoCusPermitLineTransaction)header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			AssertEquals($"{PermitHeader.HumanReadableName} cannot be deleted because it has transaction allocated to it.", header.ReasonForNotAbleToDelete);
		}

		public void TestCanDelete_IsInDatabase()
		{
			var header = PermitHeader;
			AssertEquals(true, header.CanDelete);
			var transaction = (AutoCusPermitLineTransaction)header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			AssertEquals(true, header.CanDelete);
			Factory.Save();
			AssertEquals(false, header.CanDelete);
		}

		public void TestCanDelete_HasSystemGeneratedTransactions()
		{
			var header = PermitHeader;
			AssertEquals(true, header.CanDelete);
			var transaction = (AutoCusPermitLineTransaction)header.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			transaction.FillWithValidTestData();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			AssertEquals(false, header.CanDelete);
		}

		public void TestReadOnlyFieldsWhenPermitHasTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var permit = Factory.New<BaseCusPermitHeader>();
				permit.CPH_Type = "RCC";
				GlbStaff.CurrentUser.GS_IsController = false;
				permit.CusPermitLineTransactions.AddNew().CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				AssertReadOnlyFields(permit, false);
				permit.CusPermitLineTransactions.AddNew().CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
				AssertReadOnlyFields(permit, false);

				var tran = permit.CusPermitLineTransactions.AddNew();
				tran.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
				AssertReadOnlyFields(permit, true);

				tran.Delete();
				AssertReadOnlyFields(permit, false);
				permit.CusPermitLineTransactions.AddNew().CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
				AssertReadOnlyFields(permit, true);
			}
		}

		void AssertReadOnlyFields(BaseCusPermitHeader permit, bool isReadOnly)
		{
			AssertEquals("CPH_OH_PermitHolderInfo.ReadOnly", isReadOnly, permit.CPH_OH_PermitHolderInfo.ReadOnly);
			AssertEquals("CPH_NumberInfo.ReadOnly", isReadOnly, permit.CPH_NumberInfo.ReadOnly);
			Assert("CPH_StartDateInfo.ReadOnly", !permit.CPH_StartDateInfo.ReadOnly);
			Assert("CPH_EndDateInfo.ReadOnly", !permit.CPH_EndDateInfo.ReadOnly);
			AssertEquals("CPH_TypeInfo.ReadOnly", isReadOnly, permit.CPH_TypeInfo.ReadOnly);
			AssertEquals("CPH_SubTypeInfo.ReadOnly", isReadOnly, permit.CPH_SubTypeInfo.ReadOnly);
			AssertEquals("CPH_QtyValIndicatorInfo.ReadOnly", isReadOnly, permit.CPH_QtyValIndicatorInfo.ReadOnly);
			AssertEquals("CPH_UnitOfMeasureInfo.ReadOnly", isReadOnly, permit.CPH_UnitOfMeasureInfo.ReadOnly);
			AssertEquals("CPH_OA_AppliesToInfo.ReadOnly", isReadOnly, permit.CPH_OA_AppliesToInfo.ReadOnly);
		}

		public void TestAuthLatestValueBalance()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 12.34m, ZDecimal.Zero, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today.AddDays(-1));
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 23.45m, ZDecimal.Zero, category: PermitTransactionCategoryList.Codes.VAL);
			AssertEquals(23.45m, PermitHeader.AuthLatestValueBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			AssertEquals(ZDecimal.Zero, PermitHeader.AuthLatestValueBalance);
		}

		public void TestAuthLatestValueBalanceWithMsg()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 12.34m, ZDecimal.Zero, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today.AddDays(-1));
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 23.45m, ZDecimal.Zero, category: PermitTransactionCategoryList.Codes.VAL);
			AssertEquals("23.45", PermitHeader.AuthLatestValueBalanceWithMsg);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			AssertEquals("Not Available", PermitHeader.AuthLatestValueBalanceWithMsg);
		}

		public void TestAuthLatestQuantityBalance()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 12.34m, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today.AddDays(-1));
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 23.45m, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today);
			AssertEquals(23.45m, PermitHeader.AuthLatestQuantityBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			AssertEquals(ZDecimal.Zero, PermitHeader.AuthLatestQuantityBalance);
		}

		public void TestAuthLatestQuantityBalanceWithMsg()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 12.34m, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today.AddDays(-1));
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 23.45m, category: PermitTransactionCategoryList.Codes.VAL, transactionDate: ZDateTime.Today);
			AssertEquals("23.45", PermitHeader.AuthLatestQuantityBalanceWithMsg);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			AssertEquals("Not Available", PermitHeader.AuthLatestQuantityBalanceWithMsg);
		}

		public void TestValueBalance()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 12.34m, ZDecimal.Zero);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 23.45m, ZDecimal.Zero, PermitTransactionStatusList.Codes.Confirmed);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 50.00m, ZDecimal.Zero, PermitTransactionStatusList.Codes.Deleted);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 100.00m, ZDecimal.Zero, PermitTransactionStatusList.Codes.Pending);
			AssertEquals(new ZDecimal(135.79), PermitHeader.ValueBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			AssertEquals(ZDecimal.Zero, PermitHeader.ValueBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			AssertEquals(new ZDecimal(135.79), PermitHeader.ValueBalance);
		}

		public void TestQuantityBalance()
		{
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 12.34m);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 23.45m, PermitTransactionStatusList.Codes.Confirmed);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 50.00m, PermitTransactionStatusList.Codes.Deleted);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, 100.00m, PermitTransactionStatusList.Codes.Pending);
			AssertEquals(new ZDecimal(135.79), PermitHeader.QuantityBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			AssertEquals(ZDecimal.Zero, PermitHeader.QuantityBalance);

			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			AssertEquals(new ZDecimal(135.79), PermitHeader.QuantityBalance);
		}

		public void TestSetDefaultValues()
		{
			var permitHeader = Factory.New<BaseCusPermitHeader_ForTest>();
			AssertEquals("Defaulting CountryCode", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), permitHeader.CPH_RN_NKCountryCode);
		}

		public void TestOnFactorySaving()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "HLDR1";
			var permitHeader = GetNewPermitHeader(Factory);
			FillCusPermitHeader(permitHeader, orgHeader);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var permitHeader2 = GetNewPermitHeader(newFactory);
			FillCusPermitHeader(permitHeader2, orgHeader);

			var exception = AssertExceptionThrown<ZCannotSaveException>(() =>
			{
				newFactory.Save();
			});
			AssertContains($"A {ShortName} with this combination already exists", exception.Message);
		}

		protected abstract ZString ShortName { get; }

		public void TestOnFactorySaving_CanHaveSameCombinationInDifferentCountries()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "HLDR1";
			var permitHeader = GetNewPermitHeader(Factory);
			FillCusPermitHeader(permitHeader, orgHeader);
			permitHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Factory.Save();

			var permitHeader2 = GetNewPermitHeader(Factory);
			FillCusPermitHeader(permitHeader2, orgHeader);
			permitHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		void FillCusPermitHeader(SharedCusPermitHeader permitHeader, OrgHeader orgHeader)
		{
			permitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			permitHeader.CPH_Number = "12345";
			permitHeader.CPH_StartDate = ZDate.BrettsBirthday;
			permitHeader.CPH_Type = "ADJ";
			permitHeader.CPH_SubType = ZString.Empty;
			permitHeader.CPH_QtyValIndicator = "BTH";
		}

		public void TestCPH_Calc_OpeningBalance()
		{
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1234.56m, ZDecimal.Zero, transactionType: PermitTransactionTypeList.Codes.OBL);
			PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 789m, ZDecimal.Zero);
			AssertEquals(new ZDecimal(1234.56), PermitHeader.CPH_Calc_OpeningBalance);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(BaseCusPermitHeaderTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.SharedCusPermitHeaderFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}
		public void TestIsTransactionsApplicable()
		{
			PermitHeader.CPH_QtyValIndicator = ZString.Empty;
			Assert(!PermitHeader.IsTransactionsApplicable());
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			Assert(PermitHeader.IsTransactionsApplicable());
		}
		protected abstract TSharedCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			PermitHeader = GetNewPermitHeader(Factory);
		}
		protected TSharedCusPermitHeader PermitHeader { get; set; }
	}
}
