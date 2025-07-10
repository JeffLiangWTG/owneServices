using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccountingMasterFilesUtilsTest : TestCaseWithFactory
	{
		public void TestGetMaxLocationDateTimeByCountryCodeUsesIndex()
		{
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var dateTime = AccountingMasterFilesUtils.GetMaxLocationDateTimeByCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains("RefUNLOCO") && t.Item1.Contains("MAX(RL_PK)"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				CombineAssertions(() =>
				{
					Assert("Clustered index NR_UC__RL_Code must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_UC__RL_Code"));
					Assert("There must have no Table Scan been used.", !queryPlanAnalyzer.TableScans.Any());
					Assert("There must have no Index Scan", !queryPlanAnalyzer.IndexScans.Any());
				});
			}
		}

		public void TestNotAllowedForDissectionControlAccount()
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingMasterFilesRegistry.Instance.TaxTransactionPrepaidAssetControlAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionRemittanceLiabilityControlAccount,
				AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount,
				AccountingMasterFilesRegistry.Instance.PendingTaxTransactionRemittanceLiabilityControlAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionExpenseAccount,
				AccountingMasterFilesRegistry.Instance.TaxTransactionNegativeRevenueAccount,
				AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount,
			};

			var account = ZGuid.NewZGuid();
			Assert(!AccountingMasterFilesUtils.IsNotAllowedForDissectionControlAccount(account));

			foreach (var registryItem in registryItemsToCheck)
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account.ToGuid());
				Assert(AccountingMasterFilesUtils.IsNotAllowedForDissectionControlAccount(account));
			}
		}

		public void TestGetMaxRequiredAuthorizationDueToExceedingCreditLimit()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_IsCreditor = true;
			org1.CompanyData.OB_ARCreditLimit = 400M;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_ARCreditLimit = 200M;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.CompanyData.OB_IsDebtor = true;
			org3.CompanyData.OB_IsCreditor = true;
			org3.CompanyData.OB_ARCreditLimit = 700M;

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.CompanyData.OB_IsDebtor = true;
			org4.CompanyData.OB_IsCreditor = true;
			org4.CompanyData.OB_ARCreditLimit = 1200M;

			OrgHeader org5 = null;

			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			collection.Add(CreateNewSettings(500, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var arInvoice1 = GetNewTransactionHeader(org1, TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 300M); //org1 not over credit limit
			var arInvoice2 = GetNewTransactionHeader(org2, TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 400M); //org2 is over credit limit, level 1 authorization required
			var arInvoice3 = GetNewTransactionHeader(org3, TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 1201M); //org3 is over credit limit, level 2 authorization required
			var arInvoice4 = GetNewTransactionHeader(org4, TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 2201M); //org4 is over credit limit, level 3 authorization required

			Factory.Save();

			int actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org1, org5);
			AssertEquals("No authorization required", 0, actualResult);

			actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org1, org2, org5);
			AssertEquals("Level 1 authorization is expected", 1, actualResult);

			actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org1, org2, org3, org5);
			AssertEquals("Level 2 authorization is expected", 2, actualResult);

			actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org1, org2, org3, org4, org5);
			AssertEquals("Level 3 authorization is expected", 3, actualResult);

			actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org5);
			AssertEquals("No authorization required", 0, actualResult);

			actualResult = AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(org4, org3, org2, org1);
			AssertEquals("Level 3 authorization is expected", 3, actualResult);
		}

		public void TestGetAuthorisationRequirementWeight()
		{
			AssertEquals(0, AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			AssertEquals(1, AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			AssertEquals(2, AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			AssertEquals(3, AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			AssertEquals(10, AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.MissingExRate));
		}

		public void TestGetOnCreditHoldControllerSecurityCheckPoint()
		{
			var expectedSecurityCheckpoint = Env.Security.OnCreditHoldControllerFirstLevel;
			AssertEquals(expectedSecurityCheckpoint, AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(1));

			expectedSecurityCheckpoint = Env.Security.OnCreditHoldControllerSecondLevel;
			AssertEquals(expectedSecurityCheckpoint, AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(2));

			expectedSecurityCheckpoint = Env.Security.OnCreditHoldControllerThirdLevel;
			AssertEquals(expectedSecurityCheckpoint, AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(3));
		}

		public void TestIsHighestOnCreditHoldControllerSecurityLevelCorrect()
		{
			var nextOnCreditHoldControllerSecurityLevel = AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(AccountingMasterFilesUtils.HighestOnCreditHoldControllerSecurityLevel + 1);
			AssertNull("Next Level higher than the Highest On Credit Hold Controller Security Level does not exist", nextOnCreditHoldControllerSecurityLevel);

			var currentOnCreditHoldControllerSecurityLevel = AccountingMasterFilesUtils.GetOnCreditHoldControllerSecurityCheckPoint(AccountingMasterFilesUtils.HighestOnCreditHoldControllerSecurityLevel);
			AssertEquals("Current Highest On Credit Hold Controller Security Level exists", Env.Security.OnCreditHoldControllerThirdLevel, currentOnCreditHoldControllerSecurityLevel);
		}

		public void TestIsForeignToLocalConversionCorrectWhenExchangeRateIsOne()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";
			Assert(AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("AUD", 123.4m, 123m));
			Assert(AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("AUD", 120m, 123m));
			Assert(!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("JPY", 123.4m, 123m));
			Assert(!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("JPY", 120m, 123m));
		}

		public void TestOtherCompanyIsForeignToLocalConversionCorrectWhenExchangeRateIsOne()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "USD";
			Assert(AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("USD", 535.01m, 3399.99m, "CNY"));
			Assert(!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("USD", 123.4m, 123m, null));
			Assert(AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency("USD", 123m, 123m, null));
		}

		public void TestIsTermWithoutDays()
		{
			Assert(AccountingMasterFilesUtils.IsTermWithoutDays(AccountingMasterFilesConstants.DefaultInvoiceTerm));
			foreach (var field in typeof(InvoiceTermsList).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				var term = (CodeDescriptionPair)field.GetValue(null);
				AssertEquals(InvoiceTerm.GetIsTermWithoutDays(term.Code), AccountingMasterFilesUtils.IsTermWithoutDays(term.Code));
			}
		}

		public void TestShouldPreventCreateCreditNote()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, ZGuid.Empty));
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, Env.CurrentCompany.PK));

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, ZGuid.Empty));
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.UnapprovedPayableTransactions, Env.CurrentCompany.PK));
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.TransactionsPendingAllocation, Env.CurrentCompany.PK));
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.IncompleteTransactions, Env.CurrentCompany.PK));
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.UnapprovedPayableTransactions, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.TransactionsPendingAllocation, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.IncompleteTransactions, Env.CurrentCompany.PK));
		}

		public void TestShouldPreventCreateCreditNote_InvalidLedgerType()
		{
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.General, Env.CurrentCompany.PK));
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.CashBook, Env.CurrentCompany.PK));
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.JobCosting, Env.CurrentCompany.PK));
		}

		public void TestShouldPreventInvoiceReversing_InvalidLedgerType()
		{
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.General, Env.CurrentCompany.PK));
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.CashBook, Env.CurrentCompany.PK));
			AssertExceptionThrown<NotSupportedException>("Unsupported LedgerTypes", () => AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.JobCosting, Env.CurrentCompany.PK));
		}

		public void TestShouldPreventInvoiceReversing()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsReceivable, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsReceivable, ZGuid.Empty));
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsReceivable, Env.CurrentCompany.PK));

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsPayable, Env.CurrentCompany.PK));
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsPayable, ZGuid.Empty));
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsPayable, Env.CurrentCompany.PK));
		}

		public void TestGetCountryCodeFromRegistryFallBackLevel()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;
			var fallbackLevel = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals(Core.Constants.CountryCodes.Afghanistan, AccountingMasterFilesUtils.GetCountryCodeFromRegistryFallBackLevel(Factory, fallbackLevel));
		}

		public void TestGetCountryCodeFromCompanyPK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals(Core.Constants.CountryCodes.Singapore, Env.CurrentCompany.Country.Code);

				using (Db.Connection.TrackExecutedCommands())
				{
					AssertEquals(Core.Constants.CountryCodes.Singapore, AccountingMasterFilesUtils.GetCountryCodeFromCompanyPK(Factory, Env.CurrentCompanyPK));
					AssertEquals("Should not load GlbCompany and use Env.CurrentCompany", false,
						Db.Connection.ExecutedCommands.Any(x => x.Contains("GlbCompany")));
				}

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Gabon;
				AssertEquals(Core.Constants.CountryCodes.Gabon, AccountingMasterFilesUtils.GetCountryCodeFromCompanyPK(Factory, company.PK.ToGuid()));

				AssertEquals(Core.Constants.CountryCodes.Singapore, AccountingMasterFilesUtils.GetCountryCodeFromCompanyPK(Factory, Guid.Empty));
			}
		}

		public void TestGetTaxBranchResetValue()
		{
			var boolList = new[] { false, true };

			foreach (var isGSTRegistered in boolList)
			{
				foreach (var enableTaxBranchReporting in boolList)
				{
					GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
					AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting);

					if (isGSTRegistered && enableTaxBranchReporting)
					{
						AssertEquals("When TaxBranch is enabled, TaxBranch reset value should be current branch when otherCondition is true.", GlbBranch.CurrentBranch.PK, AccountingMasterFilesUtils.GetTaxBranchResetValue(true));
						AssertEquals("When TaxBranch is enabled, TaxBranch reset value should be empty when otherCondition is false.", ZGuid.Empty, AccountingMasterFilesUtils.GetTaxBranchResetValue(false));
					}
					else
					{
						AssertEquals("When TaxBranch is disabled, TaxBranch should be reset to empty.", ZGuid.Empty, AccountingMasterFilesUtils.GetTaxBranchResetValue(true));
					}
				}
			}
		}

		public void TestGetIsAllowedWithConstraint()
		{
			var securityCheckpointSupportConstraint = Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxBranch;
			var securityCheckpointStandard = Env.Security.ConsolBulkPost;

			AssertEquals("Pre-condition", false, securityCheckpointStandard is ISupportAllowWithConstraint);
			AssertEquals("Pre-condition", true, securityCheckpointSupportConstraint is ISupportAllowWithConstraint);

			AssertEquals("Security checkpoint does not support constraint", true, securityCheckpointStandard.IsAllowedWithConstraint());

			securityCheckpointStandard.IsAllowed = false;
			AssertEquals("Security checkpoint does not support constraint", false, securityCheckpointStandard.IsAllowedWithConstraint());

			AssertEquals("Security checkpoint supports constraint and has constraint", false, securityCheckpointSupportConstraint.IsAllowedWithConstraint());

			securityCheckpointStandard.IsAllowed = false;
			AssertEquals("Security checkpoint supports constraint and has constraint", false, securityCheckpointStandard.IsAllowedWithConstraint());

			var service = ObjectFactory.Get<IAccountingRegistryProvider>();
			service.EnableTaxBranchFeature = true;

			securityCheckpointStandard.IsAllowed = true;
			AssertEquals("Security checkpoint supports constraint and has no constraint", true, securityCheckpointStandard.IsAllowedWithConstraint());

			securityCheckpointStandard.IsAllowed = false;
			AssertEquals("Security checkpoint supports constraint and has no constraint", false, securityCheckpointStandard.IsAllowedWithConstraint());
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount, bool isCancelled = false)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = transactionType;
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_InvoiceAmount = outstandingAmount;
			invoice.AH_OSTotal = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_IsCancelled = isCancelled;
			return invoice;
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		public void TestIsValidSIC_HasSICInFeatureControl()
		{
			var featureControlData = new AccRegistrationNumberFeatureControlData
			{
				RegistrationNumbers =
				[
					new()
					{
						Country = "MY",
						Type = "SIC",
						Numbers = new List<NumberTuple> { new NumberTuple { Number = "12345" } }
					},

					new()
					{
						Country = "US",
						Type = "SIC",
						Numbers = new List<NumberTuple> { new NumberTuple { Number = "67890" } }
					}
				]
			};

			AssertIsValidSIC(featureControlData, () =>
			{
				Assert(AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "12345"));
				Assert(!AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
			});
		}

		public void TestIsValidSIC_HasNotSICInFeatureControl()
		{
			var featureControlData = new AccRegistrationNumberFeatureControlData
			{
				RegistrationNumbers =
				[
					new()
					{
						Country = "US",
						Type = "MAB",
						Numbers = new List<NumberTuple> { new NumberTuple { Number = "67890" } }
					}
				]
			};

			AssertIsValidSIC(featureControlData, () =>
			{
				Assert(!AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
			});
		}

		public void TestIsValidSIC_NoException()
		{
			var featureControlData = new AccRegistrationNumberFeatureControlData
			{
				RegistrationNumbers = null
			};

			AssertIsValidSIC(featureControlData, () =>
			{
				AssertNoExceptionThrown(() => AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
				Assert(!AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
			});

			featureControlData = new AccRegistrationNumberFeatureControlData
			{
				RegistrationNumbers =
				[
					new()
					{
						Country = "US",
						Type = "MAB",
						Numbers = null
					}
				]
			};

			AssertIsValidSIC(featureControlData, () =>
			{
				AssertNoExceptionThrown(() => AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
				Assert(!AccountingMasterFilesUtils.IsValidSIC(Core.Constants.CountryCodes.Malaysia, "67890"));
			});
		}

		void AssertIsValidSIC(AccRegistrationNumberFeatureControlData featureControlData, Action assertAction)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out featureControlData)).Returns(true);
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingRegistrationNumberFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				assertAction();
			}
		}

		public void TestUpdateStmLinkDescription()
		{
			var stmLink = Factory.NewWithValidTestData<StmLink>();
			stmLink.STL_ModuleID = ModuleIDs.AlternateGLAccounts.Name;
			stmLink.STL_ItemDescription = "Before";
			stmLink.STL_ItemPK = Guid.NewGuid();
			stmLink.STL_LinkType = "FLF";
			stmLink.STL_GS_NKUser = "~BP";
			stmLink.STL_GC_LogonCompany = GlbCompany.CurrentCompany.PK;
			stmLink.STL_LastUsedDateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var stmLinkQuery = new ZQuery();
			stmLinkQuery.AddToFilter(StmLinkSchema.STL_ModuleID, ModuleIDs.AlternateGLAccounts.Name);
			var stmLinks = Factory.Load<StmLink>(stmLinkQuery);
			AssertEquals(1, stmLinks.Length);
			AssertEquals("Before", stmLinks.FirstOrDefault().STL_ItemDescription);

			var dictionary = new Dictionary<ZGuid, ZString>
			{
				{ stmLink.STL_ItemPK, "After" },
			};

			AccountingMasterFilesUtils.UpdateStmLinkDescription(ModuleIDs.AlternateGLAccounts.Name, dictionary);

			stmLinks = Factory.Load<StmLink>(stmLinkQuery);
			AssertEquals(1, stmLinks.Length);
			AssertEquals("After", stmLinks.FirstOrDefault().STL_ItemDescription);
		}
	}
}
