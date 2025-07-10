using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DeferredSubmissionHelperTest : TestCaseWithFactory
	{
		public void TestIsDeclarationDeferrable()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6", "Rejected");
			Factory.Save();

			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
			var helper = new DeferredSubmissionHelper(declaration);
			Assert("Declaration is deferrable", helper.IsDeclarationDeferrable);

			declaration.JE_CustomsOffice = "ABC";
			helper.RefreshData();
			Assert("Declaration is not deferrable when customs office does not have a mapping to a deferrable account", !helper.IsDeclarationDeferrable);

			declaration.JE_CustomsOffice = "DFM";
			helper.RefreshData();
			Assert("precondition", helper.IsDeclarationDeferrable);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			helper.RefreshData();
			Assert("Declaration is not deferrable for exports", !helper.IsDeclarationDeferrable);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			helper.RefreshData();
			Assert("precondition", helper.IsDeclarationDeferrable);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			helper.RefreshData();
			Assert("Declaration is deferrable for air", helper.IsDeclarationDeferrable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			helper.RefreshData();
			Assert("Declaration is not deferrable for road", !helper.IsDeclarationDeferrable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.RefreshData();
			Assert("precondition", helper.IsDeclarationDeferrable);
			declaration.JE_DateOfArrival = ZDate.Empty;
			helper.RefreshData();
			Assert("Declaration is not deferrable when arrival date is empty", !helper.IsDeclarationDeferrable);

			declaration.JE_DateOfArrival = ZDate.Today.AddDays(1);
			helper.RefreshData();
			Assert("precondition", helper.IsDeclarationDeferrable);

			var entryHeader = helper.FirstDutiableEntryHeader;
			entryHeader.MovementReferenceNumberSetter("123", ZDateTime.Now);
			helper.RefreshData();
			Assert("Declaration is not deferrable when MRN exists", !helper.IsDeclarationDeferrable);

			entryHeader.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Empty);
			helper.RefreshData();
			Assert("precondition", helper.IsDeclarationDeferrable);

			entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			entryHeader.CH_EntryStatus = ZString.Empty;
			helper.RefreshData();
			AssertEquals("Declaration is not deferrable when has messages unless message is rejected", false, helper.IsDeclarationDeferrable);

			entryHeader.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			helper.RefreshData();
			AssertEquals("Declaration is not deferrable when has messages unless message is rejected", false, helper.IsDeclarationDeferrable);

			entryHeader.CH_EntryStatus = "6";
			helper.RefreshData();
			AssertEquals("Declaration is deferrable when message is rejected", true, helper.IsDeclarationDeferrable);

			entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			entryHeader.CH_EntryStatus = ZString.Empty;

			var heldMessage = Factory.New<CUSDECEDIMessage>();
			heldMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			heldMessage.EM_Status = EDIMessage.Status.Queued;
			heldMessage.EM_HeldUntilDate = ZDateTime.Today.AddDays(1);
			entryHeader.Messages.Add(heldMessage);

			helper.RefreshData();
			AssertEquals("Can cancel when has unsent held messages", true, helper.CanCancelDeferredMessages);
			AssertEquals("Declaration is not deferrable when has messages unless message is rejected", false, helper.IsDeclarationDeferrable);

			helper.CancelDeferredMessages();
			helper.RefreshData();
			AssertEquals("Declaration is deferrable when message is cancelled", true, helper.IsDeclarationDeferrable);
		}

		public void TestDeferralDate()
		{
			var declaration = Factory.New<JobDeclaration>();

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 2 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.JE_TransportMode = "SEA";
				declaration.JE_DateOfArrival = ZDate.Empty;
				var helper = new DeferredSubmissionHelper(declaration);
				AssertEquals(ZDate.Today, helper.DeferralDate);

				declaration.JE_DateOfArrival = ZDate.Today.AddDays(1);
				helper.RefreshData();
				AssertEquals(ZDate.Today, helper.DeferralDate);

				declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);
				helper.RefreshData();
				AssertEquals(ZDate.Today.AddDays(2), helper.DeferralDate);

				declaration.JE_TransportMode = "AIR";
				helper.RefreshData();
				AssertEquals(ZDate.Today, helper.DeferralDate);
			}

			messageDeferralSettings.DaysBeforeETA = 0;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.JE_TransportMode = "SEA";
				declaration.JE_DateOfArrival = ZDate.Today.AddDays(4);
				var helper = new DeferredSubmissionHelper(declaration);
				AssertEquals(ZDate.Today, helper.DeferralDate);
			}
		}

		public void TestIsDeferringMessagesEnabled_SubmissionDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			var helper = new DeferredSubmissionHelper(declaration);
			var submission = new DeferredSubmission(helper);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertCanDeferMessages(false);
			}

			messageDeferralSettings.AllowAutomaticDeferredSelection = true;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertCanDeferMessages(false);
			}

			messageDeferralSettings.DaysBeforeETA = 1;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertCanDeferMessages(true);
			}

			messageDeferralSettings.AllowAutomaticDeferredSelection = false;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertCanDeferMessages(true);
			}

			void AssertCanDeferMessages(bool expectedResult)
			{
				helper.RefreshData();
				AssertEquals("Can Defer Messages", expectedResult, helper.IsDeferringMessagesEnabled);

				declaration.JE_DateOfArrival = ZDateTime.Empty;
				helper.RefreshData();
				AssertEquals(ZDateTime.Today, submission.SubmissionDate);

				declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-2);
				helper.RefreshData();
				AssertEquals(ZDateTime.Today, submission.SubmissionDate);

				declaration.JE_DateOfArrival = ZDateTime.Today;
				helper.RefreshData();
				AssertEquals(ZDateTime.Today, submission.SubmissionDate);

				declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
				helper.RefreshData();
				var expectedDate = expectedResult ? ZDateTime.Today.AddDays(1) : ZDateTime.Today;
				AssertEquals(expectedDate, submission.SubmissionDate);
			}
		}

		public void TestIsDeferringPaymentsEnabled()
		{
			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			var helper = new DeferredSubmissionHelper(declaration);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertEquals("Cannot Defer Payments", false, helper.IsDeferringPaymentsEnabled);
			}

			messageDeferralSettings.AllowAutomaticDeferredSelection = true;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				AssertEquals("Can Defer Payments", true, helper.IsDeferringPaymentsEnabled);
			}
		}

		[TestDate(2016, 04, 01)]
		public void TestCanAlterMessageSubmitDateOrPaymentDetails()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1, agent2);
			var helper = new DeferredSubmissionHelper(declaration);
			var entryHeader = helper.FirstDutiableEntryHeader;
			AssertEquals(PaymentMethodCodeList.Codes.Defer, entryHeader.CH_PaymentMethod);

			var maps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var mapping3 = maps.AddNew();
			mapping3.AccountStartDay = 3;
			mapping3.OrganizationPK = agent1.PK;
			mapping3.CreditorPK = agent1.PK;
			mapping3.FinancialAccountNumber = "3333333333";
			mapping3.Cash = true;
			var mapping4 = maps.AddNew();
			mapping4.AccountStartDay = 3;
			mapping4.OrganizationPK = agent2.PK;
			mapping4.CreditorPK = agent2.PK;
			mapping4.FinancialAccountNumber = "4444444444";
			mapping4.Cash = true;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = false, DaysBeforeETA = 0 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				AssertEquals("Cant alter when not enabled", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);
			}

			messageDeferralSettings.DaysBeforeETA = 1;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				helper.RefreshData();
				AssertEquals("Declaration is not deferrable for exports", false, helper.IsDeclarationDeferrable);
				AssertEquals("Cant alter date when Declaration is not deferrable", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				helper.RefreshData();
				AssertEquals("Declaration is deferrable for imports", true, helper.IsDeclarationDeferrable);
				AssertEquals("Can alter date when Declaration is deferrable", true, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				helper.RefreshData();
				AssertEquals("Can alter date of a VAT message", true, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				helper.RefreshData();
				AssertEquals("Cant alter date of a Cash message", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);
			}

			entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
			messageDeferralSettings.DaysBeforeETA = 0;
			messageDeferralSettings.AllowAutomaticDeferredSelection = true;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				helper.RefreshData();
				AssertEquals("Declaration is not deferrable for exports", false, helper.IsDeclarationDeferrable);
				AssertEquals("Cant alter when Declaration is not deferrable", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				helper.RefreshData();
				AssertEquals("Declaration is deferrable for imports", true, helper.IsDeclarationDeferrable);
				AssertEquals("Can alter when Declaration is deferrable", true, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				helper.RefreshData();
				AssertEquals("Can alter VAT", true, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				helper.RefreshData();
				AssertEquals("Cant alter Cash when multiple Cash accounts exist", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				maps.Remove(mapping4);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();
				AssertEquals("Cant alter Cash when only 1 Cash account exists", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);

				maps.RemoveAll();
				var mapping1 = maps.AddNew();
				mapping1.AccountStartDay = 5;
				mapping1.OrganizationPK = agent1.PK;
				mapping1.CreditorPK = agent1.PK;
				mapping1.CustomsOfficeCode = "DFM";
				mapping1.Cash = false;
				mapping1.ImporterPays = false;
				mapping1.FinancialAccountNumber = "1111111111";
				maps.Add(mapping3);
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				helper.RefreshData();
				AssertEquals("Cant alter with one deferrable account and a Cash account", false, helper.CanAlterMessageSubmitDateOrPaymentDetails);
			}
		}

		public void TestCashFANPortMapsCollection()
		{
			var declaration = GetDeclarationForPortMapTests();
			var helper = new DeferredSubmissionHelper(declaration);

			AssertNull("Deferrable mapping 1111111111 is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "1111111111"));
			AssertNull("Deferrable mapping 2222222222 is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "2222222222"));
			AssertNotNull("Cash mapping 3333333333 is included", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "3333333333"));
			AssertNull("Importer mapping 4444444444 is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));

			var maps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var mapping4 = maps.Cast<FinancialAccountNumberPortMap>().FirstOrDefault(m => m.FinancialAccountNumber == "4444444444");
			mapping4.Cash = true;
			mapping4.CustomsOfficeCode = "";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			helper.RefreshData();
			AssertNotNull("Importer mapping 4444444444 with Cash enabled and Org same as Importer is included", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));

			mapping4.Cash = false;
			mapping4.CustomsOfficeCode = "DFM";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			helper.RefreshData();
			AssertNull("Importer mapping 4444444444 not cash with customs office same as on dec and Org same as Importer is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));
			AssertNull("Importer mapping 5555555555 not cash with Org not same as Importer is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "5555555555"));

			var mapping5 = maps.Cast<FinancialAccountNumberPortMap>().FirstOrDefault(m => m.FinancialAccountNumber == "5555555555");
			mapping5.Cash = true;
			mapping5.CustomsOfficeCode = "";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			helper.RefreshData();
			AssertNull("Importer mapping 5555555555 with Cash and Org not same as Importer is excluded", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "5555555555"));

			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			agent4.CompanyData.OB_IsCreditor = true;
			agent4.OH_FullName = "AGENT4";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var mapping6 = maps.AddNew();
			mapping6.AccountStartDay = 6;
			mapping6.OrganizationPK = agent4.PK;
			mapping6.CreditorPK = agent4.PK;
			mapping6.Cash = true;
			mapping6.ImporterPays = false;
			mapping6.FinancialAccountNumber = "6666666666";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			AssertEquals(6, ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.Value.Count);
			AssertNull("Deferrable mapping 6666666666 is unknown due to caching", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "6666666666"));

			helper.RefreshData();
			AssertNotNull("Deferrable mapping 6666666666 is included", helper.CashFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "6666666666"));
		}

		public void TestDeferrableFANPortMapsCollection()
		{
			var declaration = GetDeclarationForPortMapTests();
			var helper = new DeferredSubmissionHelper(declaration);

			AssertNotNull("Deferrable mapping 1111111111 with customs office same as on dec is included", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "1111111111"));
			AssertNull("Deferrable mapping 2222222222 with customs office different to that on dec is excluded", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "2222222222"));
			AssertNull("Cash mapping 3333333333 is excluded", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "3333333333"));
			AssertNull("Importer mapping 4444444444 with customs office different to that on dec is excluded", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));

			var maps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var mapping4 = maps.Cast<FinancialAccountNumberPortMap>().FirstOrDefault(m => m.FinancialAccountNumber == "4444444444");
			mapping4.Cash = true;
			mapping4.CustomsOfficeCode = "";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			helper.RefreshData();
			AssertNull("Importer mapping 4444444444 with Cash enabled is excluded", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));

			mapping4.Cash = false;
			mapping4.CustomsOfficeCode = "DFM";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			helper.RefreshData();
			AssertNotNull("Importer mapping 4444444444 with customs office same as on dec and Org same as Importer is included", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "4444444444"));
			AssertNull("Importer mapping 5555555555 with Org not same as Importer is excluded", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "5555555555"));

			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			agent4.CompanyData.OB_IsCreditor = true;
			agent4.OH_FullName = "AGENT4";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var mapping6 = maps.AddNew();
			mapping6.AccountStartDay = 6;
			mapping6.OrganizationPK = agent4.PK;
			mapping6.CreditorPK = agent4.PK;
			mapping6.CustomsOfficeCode = "DFM";
			mapping6.Cash = false;
			mapping6.ImporterPays = false;
			mapping6.FinancialAccountNumber = "6666666666";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			AssertEquals(6, ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.Value.Count);
			AssertNull("Deferrable mapping 6666666666 with customs office same as on dec is unknown due to caching", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "6666666666"));

			helper.RefreshData();
			AssertNotNull("Deferrable mapping 6666666666 with customs office same as on dec is included", helper.DeferrableFANPortMaps.FirstOrDefault(m => m.FinancialAccountNumber == "6666666666"));
		}

		[TestDate(2016, 03, 7)]
		public void TestGetDeferredAccounts()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CompanyData.OB_IsCreditor = true;
			agent3.OH_FullName = "AGENT3";
			agent3.OH_Code = "AG3";
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			agent4.CompanyData.OB_IsCreditor = true;
			agent4.OH_FullName = "AGENT4";
			agent4.OH_Code = "AG4";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1, agent2);
				declaration.JE_DateOfArrival = new ZDateTime(2016, 03, 14);

				var helper = new DeferredSubmissionHelper(declaration);
				Assert("Declaration is deferrable", helper.IsDeclarationDeferrable);
				AssertEquals("Deferral Date is 1 day before Arrival Date", new ZDateTime(2016, 03, 13), helper.DeferralDate);

				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

				var mapping1 = maps.AddNew();
				mapping1.AccountStartDay = 1;
				mapping1.OrganizationPK = agent1.PK;
				mapping1.CreditorPK = agent1.PK;
				mapping1.CustomsOfficeCode = "DFM";
				mapping1.Cash = false;
				mapping1.ImporterPays = false;
				mapping1.FinancialAccountNumber = "1111111111";
				mapping1.DutyDefermentAmount = 100;

				var mapping2 = maps.AddNew();
				mapping2.AccountStartDay = 15;
				mapping2.OrganizationPK = agent2.PK;
				mapping2.CreditorPK = agent2.PK;
				mapping2.CustomsOfficeCode = "DFM";
				mapping2.Cash = false;
				mapping2.ImporterPays = false;
				mapping2.FinancialAccountNumber = "2222222222";
				mapping2.DutyDefermentAmount = 100;

				var mapping3 = maps.AddNew();
				mapping3.AccountStartDay = 10;
				mapping3.OrganizationPK = agent3.PK;
				mapping3.CreditorPK = agent3.PK;
				mapping3.CustomsOfficeCode = "DFM";
				mapping3.Cash = false;
				mapping3.ImporterPays = false;
				mapping3.FinancialAccountNumber = "3333333333";
				mapping3.DutyDefermentAmount = 100;

				var mapping4 = maps.AddNew();
				mapping4.AccountStartDay = 3;
				mapping4.OrganizationPK = agent4.PK;
				mapping4.CreditorPK = agent4.PK;
				mapping4.FinancialAccountNumber = "4444444444";
				mapping4.Cash = true;

				var mapping5 = maps.AddNew();
				mapping5.AccountStartDay = 3;
				mapping5.OrganizationPK = agent4.PK;
				mapping5.CreditorPK = agent4.PK;
				mapping5.CustomsOfficeCode = "DFM";
				mapping5.Cash = false;
				mapping5.ImporterPays = true;
				mapping5.FinancialAccountNumber = "5555555555";
				mapping5.DutyDefermentAmount = 100;

				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
				var accounts = helper.GetDeferrableAccounts().ToArray();

				AssertEquals(3, accounts.Length);

				var accountA = accounts[0];
				AssertEquals("3333333333", accountA.FinancialAccountNumberPortMap.FinancialAccountNumber);
				AssertEquals("After start so in next period (deferred)", new ZDateTime(2016, 03, 10), accountA.StartDate);
				AssertEquals(new ZDateTime(2016, 04, 10), accountA.DueDate);

				var accountB = accounts[1];
				AssertEquals("1111111111", accountB.FinancialAccountNumberPortMap.FinancialAccountNumber);
				AssertEquals("After start so in current period", new ZDateTime(2016, 03, 01), accountB.StartDate);
				AssertEquals(new ZDateTime(2016, 04, 01), accountB.DueDate);

				var accountC = accounts[2];
				AssertEquals("2222222222", accountC.FinancialAccountNumberPortMap.FinancialAccountNumber);
				AssertEquals("Before start so in previous period, least desirable.", new ZDateTime(2016, 02, 15), accountC.StartDate);
				AssertEquals(new ZDateTime(2016, 03, 15), accountC.DueDate);
			}
		}

		[TestDate(2016, 02, 24)]
		public void TestGetDeferredAccounts_LeapYear()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1);
				declaration.JE_DateOfArrival = new ZDateTime(2016, 03, 01);

				var helper = new DeferredSubmissionHelper(declaration);
				Assert("Declaration is deferrable", helper.IsDeclarationDeferrable);
				AssertEquals("Deferral Date is 1 day before Arrival Date", new ZDateTime(2016, 02, 29), helper.DeferralDate);

				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

				var mapping1 = maps.AddNew();
				mapping1.AccountStartDay = 31;
				mapping1.OrganizationPK = agent1.PK;
				mapping1.CreditorPK = agent1.PK;
				mapping1.CustomsOfficeCode = "DFM";
				mapping1.Cash = false;
				mapping1.ImporterPays = false;
				mapping1.FinancialAccountNumber = "1111111111";
				mapping1.DutyDefermentAmount = 100;

				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
				var accounts = helper.GetDeferrableAccounts().ToArray();

				AssertEquals(1, accounts.Length);
				AssertEquals(new ZDateTime(2016, 02, 29), accounts[0].StartDate);
				AssertEquals(new ZDateTime(2016, 03, 31), accounts[0].DueDate);
			}
		}

		[TestDate(2016, 03, 24)]
		public void TestSelectFirstAccountWithSufficientBalance()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "11223344DFM20160312";
			var payInfo1a = entry1.EntryPayInfos.AddNew();
			payInfo1a.C9_PaymentAmount = 20.0m;
			payInfo1a.C9_PaymentDate = new ZDateTime(2016, 03, 17);
			payInfo1a.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.Duty;
			payInfo1a.C9_PaymentParty = "D";
			var payInfo1b = entry1.EntryPayInfos.AddNew();
			payInfo1b.C9_PaymentAmount = 1000m;
			payInfo1b.C9_PaymentDate = new ZDateTime(2016, 03, 17);
			payInfo1b.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax;
			payInfo1b.C9_PaymentParty = "F"; // should be ignored
			var payInfo1c = entry1.EntryPayInfos.AddNew();
			payInfo1c.C9_PaymentAmount = 1000m;
			payInfo1c.C9_PaymentDate = new ZDateTime(2016, 03, 17);
			payInfo1c.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.Pending; // should be ignored
			payInfo1c.C9_PaymentParty = "D";

			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "22334455DFM20160312";
			var payInfo2a = entry2.EntryPayInfos.AddNew();
			payInfo2a.C9_PaymentAmount = 20.0m;
			payInfo2a.C9_PaymentDate = new ZDateTime(2016, 03, 17);
			payInfo2a.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.Duty;
			payInfo2a.C9_PaymentParty = "D";
			var payInfo2b = entry2.EntryPayInfos.AddNew();
			payInfo2b.C9_PaymentAmount = 5.0m;
			payInfo2b.C9_PaymentDate = new ZDateTime(2016, 03, 17);
			payInfo2b.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax;

			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			entry3.CH_BGMReference = "33445566DFM20160312";
			var payInfo3a = entry3.EntryPayInfos.AddNew();
			payInfo3a.C9_PaymentAmount = 20.0m;
			payInfo3a.C9_PaymentDate = new ZDateTime(2016, 03, 1);
			payInfo3a.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.Duty;
			payInfo3a.C9_PaymentParty = "D";
			var payInfo3b = entry3.EntryPayInfos.AddNew();
			payInfo3b.C9_PaymentAmount = 5.0m;
			payInfo3b.C9_PaymentDate = new ZDateTime(2016, 03, 1);
			payInfo3b.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax;

			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			var entry4 = dec4.CustomsEntryHeaders.AddNew();
			entry4.CH_BGMReference = "33445566DFM20160312";
			var payInfo4a = entry4.EntryPayInfos.AddNew();
			payInfo4a.C9_PaymentAmount = 20.0m;
			payInfo4a.C9_PaymentDate = new ZDateTime(2016, 03, 14);
			payInfo4a.C9_TransactionType = UniversalReferenceConstants.CusEntryPayTypes.Duty;
			payInfo4a.C9_PaymentParty = "D";

			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CompanyData.OB_IsCreditor = true;
			agent3.OH_FullName = "AGENT3";
			agent3.OH_Code = "AG3";
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			agent4.CompanyData.OB_IsCreditor = true;
			agent4.OH_FullName = "AGENT4";
			agent4.OH_Code = "AG4";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);

			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1, agent2);

			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 15;
			mapping1.OrganizationPK = agent1.PK;
			mapping1.CreditorPK = agent1.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.ImporterPays = false;
			mapping1.VatDefermentAmount = 50m;
			mapping1.FinancialAccountNumber = "1111111111";
			mapping1.DutyDefermentAmount = 80; // -20D = 60

			var mapping2 = maps.AddNew();
			mapping2.AccountStartDay = 10;
			mapping2.OrganizationPK = agent2.PK;
			mapping2.CreditorPK = agent2.PK;
			mapping2.CustomsOfficeCode = "DFM";
			mapping2.Cash = false;
			mapping2.ImporterPays = false;
			mapping2.VatDefermentAmount = 10m;
			mapping2.FinancialAccountNumber = "2222222222";
			mapping2.DutyDefermentAmount = 100; // -20D, -5V = 85

			var mapping3 = maps.AddNew();
			mapping3.AccountStartDay = 1;
			mapping3.OrganizationPK = agent3.PK;
			mapping3.CreditorPK = agent3.PK;
			mapping3.CustomsOfficeCode = "DFM";
			mapping3.Cash = false;
			mapping3.ImporterPays = false;
			mapping3.FinancialAccountNumber = "3333333333";
			mapping3.DutyDefermentAmount = 100; // -40D, -5V = 60

			var mapping4 = maps.AddNew();
			mapping4.AccountStartDay = 1;
			mapping4.OrganizationPK = agent4.PK;
			mapping4.CreditorPK = agent4.PK;
			mapping4.CustomsOfficeCode = "DFM";
			mapping4.Cash = false;
			mapping4.ImporterPays = false;
			mapping4.VatDefermentAmount = 15m;
			mapping4.FinancialAccountNumber = "4444444444";
			mapping4.DutyDefermentAmount = 80; // -0D = 95

			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			var accountsInMappingOrder = new List<FinancialAccountNumberPortMapWrapper>();
			foreach (FinancialAccountNumberPortMap map in maps)
			{
				var fanPortMapWrapper = new FinancialAccountNumberPortMapWrapper(map);
				fanPortMapWrapper.StartDate = new ZDateTime(2016, 03, map.AccountStartDay);
				fanPortMapWrapper.DueDate = new ZDateTime(2016, 04, map.AccountStartDay);
				accountsInMappingOrder.Add(fanPortMapWrapper);
			}

			var helper = new DeferredSubmissionHelper(declaration);

			var account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 80, 15);
			AssertNotEquals("Insufficient when pay info is taken into account", "1111111111", account.FANMap.FinancialAccountNumber);
			AssertNotEquals("Insufficient when pay infos and VAT are taken into account", "2222222222", account.FANMap.FinancialAccountNumber);
			AssertNotEquals("Insufficient when pay info VAT is applied correctly", "3333333333", account.FANMap.FinancialAccountNumber);
			AssertNotEquals("Incorrect Customs Office Code", "5555555555", account.FANMap.FinancialAccountNumber);
			AssertEquals("Has enough balance to cover pay infos, duty and VAT", "4444444444", account.FANMap.FinancialAccountNumber);
			AssertEquals("Is Deferring", PaymentMethodCodeList.Codes.Defer, account.PaymentMethod);

			account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 0, 15);
			AssertEquals("Has enough balance to cover duty and VAT", "1111111111", account.FANMap.FinancialAccountNumber);
			AssertEquals("Is Deferring", PaymentMethodCodeList.Codes.Defer, account.PaymentMethod);

			account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 80, 0);
			AssertEquals("Has enough balance to cover pay infos, duty and VAT", "2222222222", account.FANMap.FinancialAccountNumber);
			AssertEquals("Is Deferring", PaymentMethodCodeList.Codes.Defer, account.PaymentMethod);

			account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 200, 200);
			AssertEquals("First account that can cover VAT", "3333333333", account.FANMap.FinancialAccountNumber);
			AssertEquals("Is VAT Only", PaymentMethodCodeList.Codes.VATOnly, account.PaymentMethod);

			mapping3.VatDefermentAmount = 0m;
			account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 200, 200);
			AssertNull("There is no account that can cover both values", account.FANMap);
			AssertEquals("Is Deferring", PaymentMethodCodeList.Codes.Defer, account.PaymentMethod);

			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			// Only have a single mapping
			mapping1.VatDefermentAmount = FinancialAccountNumberPortMap.DefaultVatDefermentAmount;
			mapping1.DutyDefermentAmount = 10; // -20D = 60
			accountsInMappingOrder = new List<FinancialAccountNumberPortMapWrapper>()
			{
				new FinancialAccountNumberPortMapWrapper(mapping1)
				{
					StartDate = new ZDateTime(2016, 03, mapping1.AccountStartDay),
					DueDate = new ZDateTime(2016, 04, mapping1.AccountStartDay)
				}
			};

			account = helper.SelectFirstAccountWithSufficientBalance(accountsInMappingOrder, 80, 15);
			AssertNotNull("There is only one account", account.FANMap);
			AssertEquals("Is VAT Only if account balance goes negative and !IncludeVat", PaymentMethodCodeList.Codes.VATOnly, account.PaymentMethod);
		}

		[TestDate(2016, 03, 7)]
		public void TestCalculateDeferralAccountAndPaymentMethod()
		{
			var zzrefHelper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var procedure2 = zzrefHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "12", "00", "", "", ZAJobMessageTypeList.Codes.Import);

			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CompanyData.OB_IsCreditor = true;
			agent3.OH_FullName = "AGENT3";
			agent3.OH_Code = "AG3";
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			agent4.CompanyData.OB_IsCreditor = true;
			agent4.OH_FullName = "AGENT4";
			agent4.OH_Code = "AG4";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "44556677", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory, agent1, agent2);
				declaration.JE_DateOfArrival = new ZDateTime(2016, 03, 14);

				var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction2.CEI_Style = "12";
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = "FOB";
				invoiceHeader.JZ_InvoiceAmount = 75m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction2.PK;
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
				invoiceLine.JI_Tariff = "99999";
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				invoiceLine.JI_LinePrice = 75m;
				invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
				invoiceLine.JI_InvoiceQuantity = 1.0m;
				invoiceLine.JI_InvoiceUQ = "KG";
				declaration.DoMerge();
				Factory.Save();

				AssertEquals("Entries", 2, declaration.ActiveEntryHeaders.Count);
				var header1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "11");
				AssertEquals("header1.CustomsDuty", 22.5m, header1.CustomsDuty);
				AssertEquals("header1.ValueAddedTax", 14.7m, header1.ValueAddedTax);
				AssertEquals("header1.UZ_PaymentMethod", "D", header1.CH_PaymentMethod);
				var header2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == "12");
				AssertEquals("header2.CustomsDuty", 22.5m, header2.CustomsDuty);
				AssertEquals("header2.ValueAddedTax", 14.7m, header2.ValueAddedTax);
				AssertEquals("header2.UZ_PaymentMethod", "D", header2.CH_PaymentMethod);
				// total duty = 45, total vat = 29.4

				var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

				var mapping1 = maps.AddNew();
				mapping1.AccountStartDay = 10;
				mapping1.OrganizationPK = agent1.PK;
				mapping1.CreditorPK = agent1.PK;
				mapping1.CustomsOfficeCode = "DFM";
				mapping1.Cash = false;
				mapping1.ImporterPays = true;
				mapping1.FinancialAccountNumber = "1111111111";
				mapping1.DutyDefermentAmount = 100;

				var mapping2 = maps.AddNew();
				mapping2.AccountStartDay = 12;
				mapping2.OrganizationPK = agent2.PK;
				mapping2.CreditorPK = agent2.PK;
				mapping2.CustomsOfficeCode = "DFM";
				mapping2.Cash = false;
				mapping2.ImporterPays = false;
				mapping2.FinancialAccountNumber = "2222222222";
				mapping2.DutyDefermentAmount = 50;

				var mapping3 = maps.AddNew();
				mapping3.AccountStartDay = 10;
				mapping3.OrganizationPK = agent3.PK;
				mapping3.CreditorPK = agent3.PK;
				mapping3.CustomsOfficeCode = "DFM";
				mapping3.Cash = false;
				mapping3.ImporterPays = false;
				mapping3.VatDefermentAmount = 30m;
				mapping3.FinancialAccountNumber = "3333333333";
				mapping3.DutyDefermentAmount = 100;

				var mapping4 = maps.AddNew();
				mapping4.AccountStartDay = 10;
				mapping4.OrganizationPK = agent4.PK;
				mapping4.CreditorPK = agent4.PK;
				mapping4.Cash = true;
				mapping4.ImporterPays = false;
				mapping4.FinancialAccountNumber = "4444444444";

				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

				var importerPk = declaration.JE_OH_Importer;
				declaration.JE_OH_Importer = agent1.PK;

				var helper = new DeferredSubmissionHelper(declaration);
				Assert("CanCalculateDeferralAccountAndPaymentMethod", helper.CanCalculateDeferralAccountAndPaymentMethod);

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected match Importer, finds Imp Pays - Agent1", "11223344", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				declaration.JE_OH_Importer = importerPk;
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected Latest account, Deferred - Agent2", "22334455", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				mapping2.DutyDefermentAmount = 25;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected Deferred + VAT - Agent3", "33445566", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				mapping3.DutyDefermentAmount = 25;
				mapping3.VatDefermentAmount = 25;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected VAT Only - Agent2", "22334455", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.VATOnly, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.VATOnly, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				mapping2.VatDefermentAmount = 25;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.ActiveEntryHeaders[0].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				declaration.ActiveEntryHeaders[1].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected fallback to Cash - Agent4", "44556677", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.Cash, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.Cash, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				maps.Remove(mapping4);
				declaration.JE_OH_AgentOverride = ZGuid.Empty;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.ActiveEntryHeaders[0].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				declaration.ActiveEntryHeaders[1].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected no suitable account, agent not set", "", declaration.AgentCode);
				AssertEquals(ZString.Empty, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(ZString.Empty, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);

				//mapping2.OrganizationPK = ZGuid.Empty;
				mapping2.DutyDefermentAmount = 100;
				mapping2.VatDefermentAmount = FinancialAccountNumberPortMap.DefaultVatDefermentAmount;
				mapping3.DutyDefermentAmount = 100;
				mapping3.VatDefermentAmount = 30;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
				declaration.ActiveEntryHeaders[0].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				declaration.ActiveEntryHeaders[1].CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				agent2.Delete();
				Factory.Save();
				helper.RefreshData();

				helper.CalculateDeferralAccountAndPaymentMethod();
				AssertEquals("Expected Act 2 invalid, Deferred + VAT - Agent3", "33445566", declaration.AgentCode);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[0].CH_PaymentMethod);
				AssertEquals(PaymentMethodCodeList.Codes.Defer, declaration.ActiveEntryHeaders[1].CH_PaymentMethod);
			}
		}

		public void TestCanCalculateDeferralAccountAndPaymentMethod()
		{
			var declaration = DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory);
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			var helper = new DeferredSubmissionHelper(declaration);

			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				helper.RefreshData();
				AssertEquals("Deferring of Payments is Enabled", true, helper.IsDeferringPaymentsEnabled);
				AssertEquals("Declaration is not deferrable", false, helper.IsDeclarationDeferrable);
				AssertEquals("CanCalculateDeferralAccountAndPaymentMethod", false, helper.CanCalculateDeferralAccountAndPaymentMethod);

				declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
				helper.RefreshData();
				AssertEquals("Deferring of Payments is Enabled", true, helper.IsDeferringPaymentsEnabled);
				AssertEquals("Declaration is deferrable", true, helper.IsDeclarationDeferrable);
				AssertEquals("CanCalculateDeferralAccountAndPaymentMethod", true, helper.CanCalculateDeferralAccountAndPaymentMethod);
			}

			messageDeferralSettings.AllowAutomaticDeferredSelection = false;
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				Factory.ClearCachedValue<AutomaticDeferredSelection>(DeferredSubmissionHelper.MessageDeferralSettingsCacheKey);
				helper.RefreshData();
				AssertEquals("Deferring of Payments is Disabled", false, helper.IsDeferringPaymentsEnabled);
				AssertEquals("Declaration is deferrable", true, helper.IsDeclarationDeferrable);
				AssertEquals("CanCalculateDeferralAccountAndPaymentMethod", false, helper.CanCalculateDeferralAccountAndPaymentMethod);

				declaration.JE_DateOfArrival = ZDateTime.Empty;
				helper.RefreshData();
				AssertEquals("Deferring of Payments is Disabled", false, helper.IsDeferringPaymentsEnabled);
				AssertEquals("Declaration is not deferrable", false, helper.IsDeclarationDeferrable);
				AssertEquals("CanCalculateDeferralAccountAndPaymentMethod", false, helper.CanCalculateDeferralAccountAndPaymentMethod);
			}
		}

		public void TestCanCancelDeferredMessages()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<CUSDECEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_HeldUntilDate = ZDateTime.Today.AddDays(1);

			AssertEquals("Has messages", true, entryHeader.Messages.Any());
			var helper = new DeferredSubmissionHelper(declaration);
			AssertEquals("Can cancel when has unsent held messages", true, helper.CanCancelDeferredMessages);

			message.EM_HeldUntilDate = ZDateTime.Empty;
			AssertEquals("Cannot cancel when message is not held", false, helper.CanCancelDeferredMessages);

			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_HeldUntilDate = ZDateTime.Today;
			AssertEquals("Cannot cancel when message has been sent", false, helper.CanCancelDeferredMessages);
		}

		public void TestCancelDeferredMessages()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var normalMessage = Factory.New<CUSDECEDIMessage>();
			normalMessage.EM_LinkedObject = entryHeader;
			normalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			normalMessage.EM_Status = EDIMessage.Status.Queued;
			normalMessage.EM_HeldUntilDate = ZDateTime.Empty;

			var heldMessage1 = Factory.New<CUSDECEDIMessage>();
			heldMessage1.EM_LinkedObject = entryHeader;
			heldMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			heldMessage1.EM_Status = EDIMessage.Status.Queued;
			heldMessage1.EM_HeldUntilDate = ZDateTime.Today.AddDays(1);
			var vocValueAfter = heldMessage1.VoucherOfCorrectionValueAfters.AddNew();
			vocValueAfter.CY_Code = "VAT";
			vocValueAfter.CY_Data = "100";

			var sentMessage = Factory.New<CUSDECEDIMessage>();
			sentMessage.EM_LinkedObject = entryHeader;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_HeldUntilDate = ZDateTime.Today;

			var heldMessage2 = Factory.New<CUSDECEDIMessage>();
			heldMessage2.EM_LinkedObject = entryHeader;
			heldMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			heldMessage2.EM_Status = EDIMessage.Status.Queued;
			heldMessage2.EM_HeldUntilDate = ZDateTime.Today;
			var vocValueAfter2 = heldMessage1.VoucherOfCorrectionValueAfters.AddNew();
			vocValueAfter2.CY_Code = "DUTY";
			vocValueAfter2.CY_Data = "200";

			var receivedMessage = Factory.New<CUSRESEDIMessage>();
			receivedMessage.EM_LinkedObject = entryHeader;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_HeldUntilDate = ZDateTime.Today;

			Factory.Save();

			AssertEquals("Has 5 messages", 5, entryHeader.Messages.Count);
			var helper = new DeferredSubmissionHelper(declaration);
			AssertEquals("Can cancel when has unsent held messages", true, helper.CanCancelDeferredMessages);
			var count = helper.CancelDeferredMessages();
			AssertEquals("Cancelled 2 messages", 2, count);

			Factory.Save();
			AssertEquals("Does not delete the messages", 5, entryHeader.Messages.Count);

			var queuedMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Queued && msg.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			AssertCollectionContains("Does not alter unheld messages", normalMessage, queuedMessages);

			var sentMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Sent && msg.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			AssertCollectionContains("Does not alter sent messages", sentMessage, sentMessages);

			var cancelledMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Cancelled);
			AssertCollectionContains("Cancels held message1", heldMessage1, cancelledMessages);
			AssertCollectionContains("Cancels held message2", heldMessage2, cancelledMessages);
			Assert("held message1 contains voc after values", !heldMessage1.VoucherOfCorrectionValueAfters.Any());
			Assert("held message2 contains voc after values", !heldMessage2.VoucherOfCorrectionValueAfters.Any());

			AssertEquals("Has only 1 queued message", 1, queuedMessages.Count());
			AssertEquals("Has only 1 sent message", 1, sentMessages.Count());
			AssertEquals("Has 2 cancelled messages", 2, cancelledMessages.Count());
			AssertEquals("Status unchanged when there are sent messages", ZAMessageStatusList.Codes.AwaitingResponse, declaration.JE_MessageStatus);
			AssertEquals("Cannot cancel when there are no held messages", false, helper.CanCancelDeferredMessages);
		}

		public void TestCancelDeferredMessagesClearsStatus()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageStatus = ZAMessageStatusList.Codes.Error;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var failedMessage = Factory.New<CUSDECEDIMessage>();
			failedMessage.EM_LinkedObject = entryHeader;
			failedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			failedMessage.EM_Status = EDIMessage.Status.Sent;
			failedMessage.EM_HeldUntilDate = ZDateTime.Empty;
			failedMessage.EM_MessageNum = "1";

			var heldMessage = Factory.New<CUSDECEDIMessage>();
			heldMessage.EM_LinkedObject = entryHeader;
			heldMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			heldMessage.EM_Status = EDIMessage.Status.Queued;
			heldMessage.EM_HeldUntilDate = ZDateTime.Today.AddDays(1);
			heldMessage.EM_MessageNum = "2";

			var normalMessage = Factory.New<CUSDECEDIMessage>();
			normalMessage.EM_LinkedObject = entryHeader;
			normalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			normalMessage.EM_Status = EDIMessage.Status.Queued;
			normalMessage.EM_HeldUntilDate = ZDateTime.Empty;
			normalMessage.EM_MessageNum = "3";

			entryHeader.CH_Status = "AWA";
			entryHeader.CH_EntryStatus = "";
			Factory.Save();

			AssertEquals("Has messages", 3, entryHeader.Messages.Count);
			var helper = new DeferredSubmissionHelper(declaration);
			AssertEquals("Can cancel when has unsent held messages", true, helper.CanCancelDeferredMessages);
			var count = helper.CancelDeferredMessages();
			AssertEquals("Cancelled 1 messages", 1, count);
			AssertEquals("Does not delete the messages", 3, entryHeader.Messages.Count);

			var sentMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Sent);
			AssertCollectionContains("Does not alter sent messages", failedMessage, sentMessages);

			var queuedMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Queued);
			AssertCollectionContains("Does not alter unheld messages", normalMessage, queuedMessages);

			var cancelledMessages = entryHeader.Messages.Find(msg => msg.EM_Status == EDIMessage.Status.Cancelled);
			AssertCollectionContains("Cancels held message", heldMessage, cancelledMessages);

			AssertEquals("Has only 1 queued message", 1, queuedMessages.Count());
			AssertEquals("Has 1 cancelled messages", 1, cancelledMessages.Count());
			AssertEquals("Entry Status is cleared", "", entryHeader.CH_Status);
			AssertEquals("Cannot cancel when there are no held messages", false, helper.CanCancelDeferredMessages);

			var logEntry = declaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_Reference.Contains("Held messages cancelled."));
			AssertNotNull("Expected a log entry", logEntry);
			AssertEquals("Logged as Edit", AutoEvents.EditedARecord.Code, logEntry.Event.SE_Code);
		}

		JobDeclaration GetDeclarationForPortMapTests()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DFM");
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_IsCreditor = true;
			agent.OH_FullName = "AGENT1";
			agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.CompanyData.OB_IsCreditor = true;
			agent2.OH_FullName = "AGENT2";
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CompanyData.OB_IsCreditor = true;
			agent3.OH_FullName = "AGENT3";
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "33445566", Core.Constants.CountryCodes.SouthAfrica);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CompanyData.OB_IsCreditor = true;
			importer.OH_Code = "IMP1";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "99999999", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 1;
			mapping1.OrganizationPK = agent.PK;
			mapping1.CreditorPK = agent.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.ImporterPays = false;
			mapping1.FinancialAccountNumber = "1111111111";
			var mapping2 = maps.AddNew();
			mapping2.AccountStartDay = 2;
			mapping2.OrganizationPK = agent.PK;
			mapping2.CreditorPK = agent.PK;
			mapping2.CustomsOfficeCode = "JHB";
			mapping2.Cash = false;
			mapping2.ImporterPays = false;
			mapping2.FinancialAccountNumber = "2222222222";
			var mapping3 = maps.AddNew();
			mapping3.AccountStartDay = 3;
			mapping3.OrganizationPK = agent.PK;
			mapping3.CreditorPK = agent.PK;
			mapping3.Cash = true;
			mapping3.ImporterPays = false;
			mapping3.FinancialAccountNumber = "3333333333";
			var mapping4 = maps.AddNew();
			mapping4.AccountStartDay = 4;
			mapping4.OrganizationPK = importer.PK;
			mapping4.CreditorPK = importer.PK;
			mapping4.CustomsOfficeCode = "JHB";
			mapping4.Cash = false;
			mapping4.ImporterPays = true;
			mapping4.FinancialAccountNumber = "4444444444";
			var mapping5 = maps.AddNew();
			mapping5.AccountStartDay = 5;
			mapping5.OrganizationPK = agent3.PK;
			mapping5.CreditorPK = agent3.PK;
			mapping5.CustomsOfficeCode = "DFM";
			mapping5.Cash = false;
			mapping5.ImporterPays = true;
			mapping5.FinancialAccountNumber = "5555555555";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsOffice = "DFM";
			declaration.JE_OH_Importer = importer.PK;

			return declaration;
		}
	}
}
