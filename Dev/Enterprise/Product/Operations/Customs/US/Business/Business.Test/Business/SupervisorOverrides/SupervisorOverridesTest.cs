using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(SupervisorOverrides))]
	sealed class SupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContextIsUnknown()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.Unknown);
			Assert(!supervisorOverrides.ContextIsSendingPaymentAuthorizationMessage);
		}

		public void TestContextIsSavingDeclaration()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SavingDeclaration);
			Assert(!supervisorOverrides.ContextIsSendingPaymentAuthorizationMessage);
		}

		public void TestContextIsSendingMessages()
		{
			SetSupervisorOverrides(SupervisorOverridesContext.SendingMessages);
			Assert(!supervisorOverrides.ContextIsSendingPaymentAuthorizationMessage);
		}

		void SetSupervisorOverrides(string context)
		{
			supervisorOverrides = new SupervisorOverrides(Declaration, context);
		}
		SupervisorOverrides supervisorOverrides;

		public void TestContextIsSendingPaymentAuthorizationMessage()
		{
			SupervisorOverrides supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SendingPaymentAuthorizationMessage);
			Assert(!supervisorOverrides.ContextIsSavingDeclaration);
			Assert(!supervisorOverrides.ContextIsSendingMessages);
			Assert(supervisorOverrides.ContextIsSendingPaymentAuthorizationMessage);
			Assert(!supervisorOverrides.ContextIsUnknown);
		}

		public void TestHasStatementAsBusinessEntity()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			StatementPaymentAction statement = new StatementPaymentAction(statementHeader);
			SupervisorOverrides supervisorOverrides = new SupervisorOverrides(statement, SupervisorOverridesContext.SendingPaymentAuthorizationMessage);
			Assert("Business entity is StatementPaymentAction", supervisorOverrides.HasStatementAsBusinessEntity);
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			supervisorOverrides = new SupervisorOverrides(invoice, SupervisorOverridesContext.SavingDeclaration);
			Assert("Business entity is not StatementPaymentAction", !supervisorOverrides.HasStatementAsBusinessEntity);
		}

		public void TestOverrideData()
		{
			using (RegistrySetup regSetupper = new RegistrySetup(TargetInRegistry.AllowMessageErrors))
			{
				SupervisorOverrides supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
				AssertNotNull(supervisorOverrides.OverrideData);
				AssertEquals(typeof(SupervisorOverrideData), supervisorOverrides.OverrideData.GetType());
				AssertEquals("OverrideData should return 0 NominatedMessageErrors from registry", 0, supervisorOverrides.OverrideData.NominatedMessageErrors.Count);
			}
		}

		public void TestSupervisorShouldApproveChanges()
		{
			ResetDeclaration();
			var supervisorOverrides = new SupervisorOverridesForTesting(Declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.AddMessageLogForTesting("CD1", "Test Message 1", false);
			Assert("Supervisor should check message and make decision.", supervisorOverrides.SupervisorShouldApproveChanges);

			using (var regSetupper = new RegistrySetup(TargetInRegistry.All))
			{
				ResetDeclaration();
				supervisorOverrides = new SupervisorOverridesForTesting(Declaration, SupervisorOverridesContext.SavingDeclaration);
				Assert("There is nothing to approve", !supervisorOverrides.SupervisorShouldApproveChanges);

				ResetDeclaration();
				supervisorOverrides = new SupervisorOverridesForTesting(Declaration, SupervisorOverridesContext.SavingDeclaration);

				supervisorOverrides.AddMessageLogForTesting("CD1", "Test Message 1", false);
				Assert("Supervisor should check message and make decision", supervisorOverrides.SupervisorShouldApproveChanges);
			}
		}

		public void TestSupervisorApprovalForReconciliation()
		{
			ResetDeclaration();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity.NAFTARecon);
			var supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			Assert("Declaration of wrong type so no supervisor override required", !supervisorOverrides.SupervisorShouldApproveChanges);

			ResetDeclaration();
			SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity.OtherRecon);
			Assert("Declaration of wrong type so no supervisor override required", !supervisorOverrides.SupervisorShouldApproveChanges);

			var helper = new DeclarationTestHelper(Factory);
			var supplier1 = helper.CreateOrganisation("SUPPLIER 1", "AUSYD");
			supplier1.OH_IsConsignor = true;
			var importer1 = helper.CreateOrganisation("IMPORTER 1", "USLAX");
			importer1.OH_IsConsignee = true;

			var supplierBuyerLink1 = supplier1.BuyerLinks.AddNew(importer1);
			supplierBuyerLink1.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var buyerSupplierLink1AddInfo = supplierBuyerLink1.GetAddInfo();
			buyerSupplierLink1AddInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			Factory.Save();
			using (var regSetupper = new RegistrySetup(TargetInRegistry.ReconIssue))
			{
				var originalAllowed = Env.Security.USReconIssueDefault.IsAllowed;
				try
				{
					Env.Security.USReconIssueDefault.IsAllowed = false;
					Factory.Save();
					var newFactory = new BusinessObjectFactory();
					ResetDeclaration(newFactory);

					supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;

					Declaration.JE_OH_Supplier = supplier1.PK;
					Declaration.JE_OH_Importer = importer1.PK;
					Declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;

					supervisorOverrides.CreateMessages();
					Assert("Changes should be approved, Other Recon Indicator is different from Supplier/Buyer relation",
						supervisorOverrides.SupervisorShouldApproveChanges);

					Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
					supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					Assert(
						"No actions required because declaration is FTZ and field is not relevant. Moreover, Entry Type for FTZ is 26, which is not valid entry type for reconciliation. Supervisor check will not run",
						!supervisorOverrides.SupervisorShouldApproveChanges);
				}
				finally
				{
					Env.Security.USReconIssueDefault.IsAllowed = originalAllowed;
				}
			}
		}

		public void TestLogSupervisorActions_ForDeclaration()
		{
			TestLogSupervisorActions_ForDeclarationWithSpecifiedContext(SupervisorOverridesContext.Unknown, new[] { Env.Security.SupervisorOverrides });
			TestLogSupervisorActions_ForDeclarationWithSpecifiedContext(SupervisorOverridesContext.SavingDeclaration, new[] { Env.Security.MergeByDefault, Env.Security.USFTAReconDefault, Env.Security.USPaymentTypeDefault, Env.Security.USReconIssueDefault, Env.Security.USAllRequestedIndicator });
			TestLogSupervisorActions_ForDeclarationWithSpecifiedContext(SupervisorOverridesContext.SendingMessages, new[] { Env.Security.AllowMessageErrors });
			TestLogSupervisorActions_ForDeclarationWithSpecifiedContext(SupervisorOverridesContext.SendingPaymentAuthorizationMessage, new[] { Env.Security.USPAYERAccountNumberDefault });
		}

		void TestLogSupervisorActions_ForDeclarationWithSpecifiedContext(string context, SecurityCheckpoint[] checkpoints)
		{
			var dic = new Dictionary<int, bool>();
			for (int i = 0; i < checkpoints.Length; i++)
			{
				var checkpoint = checkpoints[i];
				dic.Add(i, checkpoint.IsAllowed);
				checkpoint.IsAllowed = false;
			}

			try
			{
				using (var regSetupper = new RegistrySetup(TargetInRegistry.All))
				{
					Factory.Save();
					ResetDeclaration();
					var supervisorOverrides = new SupervisorOverrides(Declaration, context);
					supervisorOverrides.SupervisorName = regSetupper.User.GS_Code;

					SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity.All);
					supervisorOverrides.CreateMessages();

					if (supervisorOverrides.SupervisorShouldApproveChanges)
					{
						AssertAllLogs_ForDeclaration(supervisorOverrides, true, SupervisorOverrides.Constants.AffirmationCodeLocationForDeclaration);
					}
					else
					{
						Assert("Supervisor should allow changes because there are differences between Declaration and Importer of Record. If Context is not suitable for declaration there are no issues to allow", !(supervisorOverrides.ContextIsSavingDeclaration || supervisorOverrides.ContextIsSendingMessages));
					}

					ResetDeclaration();
					supervisorOverrides = new SupervisorOverrides(Declaration, context);
					supervisorOverrides.SupervisorName = regSetupper.User.GS_Code;

					SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity.NoTarget);

					supervisorOverrides.CreateMessages();
					if (!supervisorOverrides.SupervisorShouldApproveChanges)
					{
						AssertAllLogs_ForDeclaration(supervisorOverrides, false, SupervisorOverrides.Constants.AffirmationCodeLocationForDeclaration);
					}
					else
					{
						Fail("Supervisor should NOT allow changes because there are NO differences between Declaration and Importer of Record");
					}
				}
			}
			finally
			{
				foreach (var key in dic.Keys)
				{
					checkpoints[key].IsAllowed = dic[key];
				}
			}
		}

		public void TestLogSupervisorActions_ForAIIRequested()
		{
			using (var regSetupper = new RegistrySetup(TargetInRegistry.AIIRequestedIndicator))
			{
				var originalAllowed = Env.Security.USAllRequestedIndicator.IsAllowed;
				try
				{
					Env.Security.USAllRequestedIndicator.IsAllowed = false;
					Factory.Save();
					ResetDeclaration();
					var supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;
					supervisorOverrides.CreateMessages();
					Assert("Supervisor approval is not requied because Declaration is not saved yet",
						!supervisorOverrides.SupervisorShouldApproveChanges);

					supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;

					Declaration.US_IsAIIRequested = true;
					supervisorOverrides.CreateMessages();
					Assert("Supervisor approval is required, becuase Declaration is not saved, but AII Requested flag is true",
						supervisorOverrides.SupervisorShouldApproveChanges);

					var logs = new Logs(new BusinessObjectFactory().New<JobDeclaration>());
					supervisorOverrides.LogSupervisorActions(logs);
					var expectedLog = string.Format(SupervisorOverrides.Constants.AIIRequestedIndicator, Declaration.US_IsAIIRequested);
					AssertLog(supervisorOverrides.SupervisorName, expectedLog, true, logs);

					Declaration.Factory.Save();
					supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;
					Declaration.US_IsAIIRequested = false;
					supervisorOverrides.CreateMessages();

					Assert("AII Requested Indicator has been saved and then changed, Supervisor should approve changes",
						supervisorOverrides.SupervisorShouldApproveChanges);

					logs = new Logs(new BusinessObjectFactory().New<JobDeclaration>());
					supervisorOverrides.LogSupervisorActions(logs);

					expectedLog = string.Format(SupervisorOverrides.Constants.AIIRequestedIndicator, Declaration.US_IsAIIRequested);
					AssertLog(supervisorOverrides.SupervisorName, expectedLog, true, logs);

					ResetDeclaration();
					Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

					supervisorOverrides = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;
					Declaration.US_IsAIIRequested = true;
					supervisorOverrides.CreateMessages();
					Assert("Supervisor approval is not required, because Declaration is FTZ declaration and field is not relevant",
						!supervisorOverrides.SupervisorShouldApproveChanges);
				}
				finally
				{
					Env.Security.USAllRequestedIndicator.IsAllowed = originalAllowed;
				}
			}
		}

		public void TestLogSupervisorActions_ForStatement()
		{
			using (RegistrySetup regSetupper = new RegistrySetup(TargetInRegistry.All))
			{
				var originalAllowed = Env.Security.USPAYERAccountNumberDefault.IsAllowed;
				try
				{
					Env.Security.USPAYERAccountNumberDefault.IsAllowed = false;
					Factory.Save();
					SupervisorOverrides supervisorOverrides = new SupervisorOverrides(Statement, SupervisorOverridesContext.SendingPaymentAuthorizationMessage);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;

					SetDataToStatementPaymentActionAndImporterThatWillDiffer(TargetEntity.PayerAccountNumber);

					supervisorOverrides.CreateMessages();
					if (supervisorOverrides.SupervisorShouldApproveChanges)
					{
						supervisorOverrides.LogSupervisorActions(Statement.StatementHeader.Logs);

						string expectedLog = string.Format(SupervisorOverrides.Constants.PayerAccountNumber, Statement_ImporterWrapper.ZO_AccountNo, Statement.PayerUnitNo);
						AssertLog(supervisorOverrides.SupervisorName, expectedLog, true, Statement.StatementHeader.Logs);
					}
					else
					{
						Fail("Supervisor should allow changes because there are differences between Statement and Importer");
					}

					supervisorOverrides = new SupervisorOverrides(Statement, SupervisorOverridesContext.SendingPaymentAuthorizationMessage);
					supervisorOverrides.SupervisorName = GlbStaff.CurrentUser.GS_Code;

					SetDataToStatementPaymentActionAndImporterThatWillDiffer(TargetEntity.NoTarget);
					supervisorOverrides.CreateMessages();
					if (!supervisorOverrides.SupervisorShouldApproveChanges)
					{
						supervisorOverrides.LogSupervisorActions(Statement.StatementHeader.Logs);

						string expectedLog = string.Format(SupervisorOverrides.Constants.PayerAccountNumber, Statement_ImporterWrapper.ZO_AccountNo, Statement.PayerUnitNo);
						AssertLog(supervisorOverrides.SupervisorName, expectedLog, false, Statement.StatementHeader.Logs);
					}
					else
					{
						Fail("Supervisor should NOT allow changes because there are NO differences between Statement and Importer");
					}
				}
				finally
				{
					Env.Security.USPAYERAccountNumberDefault.IsAllowed = originalAllowed;
				}
			}
		}

		public void TestSupervisorMessage()
		{
			AssertReturnMessageResultForAllTargets(Declaration);
			AssertReturnMessageResultForAllTargets(Statement);

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertReturnMessageResultForAllTargets(invoiceHeader);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			using (var registrySetup = new RegistrySetup(TargetInRegistry.All))
			{
				for (int i = 0; i < targetEntityArray.Length; i++)
				{
					var originalAllowed = Env.Security.AllowMessageErrors.IsAllowed;
					var originalMerged = Env.Security.MergeByDefault.IsAllowed;
					try
					{
						Env.Security.AllowMessageErrors.IsAllowed = false;

						var target = targetEntityArray[i];
						var supervisorOverrides = new SupervisorOverridesForTesting(Declaration, SupervisorOverridesContext.SendingMessages);
						SetDataToDeclarationAndImporterOfRecordThatWillDiffer(target);
						supervisorOverrides.CreateMessages();

						if (target == TargetEntity.All || target == TargetEntity.AllowMessageErrors)
						{
							AssertNotNull(supervisorOverrides.UnAuthorisedMessagesForLog.FirstOrDefault(x => ((MessageLog)x).Message.Contains(AMessageError.Message)));
						}

						Env.Security.MergeByDefault.IsAllowed = false;
						supervisorOverrides = new SupervisorOverridesForTesting(Declaration, SupervisorOverridesContext.SavingDeclaration);
						supervisorOverrides.CreateMessages();

						if (target == TargetEntity.All || target == TargetEntity.MergeBy)
						{
							AssertNotNull(supervisorOverrides.UnAuthorisedMessagesForLog.FirstOrDefault(x => ((MessageLog)x).Message.Contains(string.Format(SupervisorOverrides.Constants.MergeBy, OrgConstants.MergeInvoiceLines.PartNumber, Declaration.JE_MergeBy))));
						}

						if (target == TargetEntity.All || target == TargetEntity.NAFTARecon)
						{
							//field is not relevant for FTZ and should not be supervised 
							AssertNull(supervisorOverrides.UnAuthorisedMessagesForLog.FirstOrDefault(x => ((MessageLog)x).Message.Contains(string.Format(SupervisorOverrides.Constants.FTARecon, Declaration.IORWrapper.ZO_NAFTAReconIndicator, Declaration.US_NAFTAReconIndicator))));
						}

						if (target == TargetEntity.All || target == TargetEntity.PaymentType)
						{
							//field is not relevant for FTZ and should not be supervised 
							AssertNull(supervisorOverrides.UnAuthorisedMessagesForLog.FirstOrDefault(x => ((MessageLog)x).Message.Contains(string.Format(SupervisorOverrides.Constants.PaymentType, Declaration.IORWrapper.ZO_PaymentType, Declaration.US_PaymentType))));
						}

						if (target == TargetEntity.All || target == TargetEntity.OtherRecon)
						{
							//field is not relevant for FTZ and should not be supervised 
							AssertNull(supervisorOverrides.UnAuthorisedMessagesForLog.FirstOrDefault(x => ((MessageLog)x).Message.Contains(string.Format(SupervisorOverrides.Constants.ReconIssue, Declaration.IORWrapper.ZO_OtherReconIndicator, Declaration.US_OtherReconIndicator))));
						}
					}
					finally
					{
						Env.Security.AllowMessageErrors.IsAllowed = originalAllowed;
						Env.Security.MergeByDefault.IsAllowed = originalMerged;
					}
				}
			}
		}

		public void TestSupervisorMessage_IsNotEmptyOnlyIfValuesAreChanged()
		{
			ResetDeclaration();

			using (RegistrySetup registrySetup = new RegistrySetup(TargetInRegistry.All))
			{
				Declaration.US_IsAIIRequested = true;
				SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity.All);
				Declaration.IOR.OH_Code = "ABC";
				Factory.Save();
				SupervisorOverrides supervisorOverrides1 = new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
				Assert("Declaration has been saved, so there are no any changes", !supervisorOverrides1.SupervisorShouldApproveChanges);
			}
		}

		#region AssertLog

		void AssertAllLogs_ForDeclaration(SupervisorOverrides supervisorOverrides, bool shouldBeFound, string affirmationCodeLocation)
		{
			var logs = new Logs(new BusinessObjectFactory().New<JobDeclaration>());
			supervisorOverrides.LogSupervisorActions(logs);

			AssertLog(supervisorOverrides.SupervisorName, AMessageError.Message, shouldBeFound && supervisorOverrides.ContextIsSendingMessages, logs);

			var expectedLog = string.Format(SupervisorOverrides.Constants.MergeBy, OrgConstants.MergeInvoiceLines.PartNumber, Declaration.JE_MergeBy);
			AssertLog(supervisorOverrides.SupervisorName, expectedLog, shouldBeFound && supervisorOverrides.ContextIsSavingDeclaration, logs);

			expectedLog = string.Format(SupervisorOverrides.Constants.FTARecon, Declaration.IORWrapper.ZO_NAFTAReconIndicator, Declaration.US_NAFTAReconIndicator);
			AssertLog(supervisorOverrides.SupervisorName, expectedLog, shouldBeFound && supervisorOverrides.ContextIsSavingDeclaration, logs);

			expectedLog = string.Format(SupervisorOverrides.Constants.ReconIssue, Declaration.IORWrapper.ZO_OtherReconIndicator, Declaration.US_OtherReconIndicator);
			AssertLog(supervisorOverrides.SupervisorName, expectedLog, shouldBeFound && supervisorOverrides.ContextIsSavingDeclaration, logs);

			expectedLog = string.Format(SupervisorOverrides.Constants.PaymentType, Declaration.IORWrapper.ZO_PaymentType, Declaration.US_PaymentType);
			AssertLog(supervisorOverrides.SupervisorName, expectedLog, shouldBeFound && supervisorOverrides.ContextIsSavingDeclaration, logs);
		}

		void AssertLog(string expectedUserCode, string expectedMessage, bool shouldBeFound, Logs logs)
		{
			var filter = new ZDBOnlyQuery(typeof(StmALog));
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			filter.AddToFilter(StmALogSchema.SL_GS_NKUser, expectedUserCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, expectedMessage);
			var foundLogs = logs.Find(filter);

			AssertNotNull(foundLogs);

			if (shouldBeFound)
			{
				AssertEquals("1 Log should be found!", 1, foundLogs.Length);
			}
			else
			{
				AssertEquals("No Logs should be found!", 0, foundLogs.Length);
			}
		}

		#endregion

		#region AssertReturnMessageResultForAllTargets

		readonly TargetEntity[] targetEntityArray = new TargetEntity[]
		{
			TargetEntity.DefaultsAreEmpty,
			TargetEntity.All,
			TargetEntity.AllowMessageErrors,
			TargetEntity.MergeBy,
			TargetEntity.NAFTARecon,
			TargetEntity.NoTarget,
			TargetEntity.OtherRecon,
			TargetEntity.PayerAccountNumber,
			TargetEntity.PaymentType,
			TargetEntity.AIIRequestedIndicator
		};

		void AssertReturnMessageResultForAllTargets(IBusiness businessEntity)
		{
			AssertReturnMessageResultForAllTargetsWithSpecifiedContext(businessEntity, SupervisorOverridesContext.SavingDeclaration, new[] { Env.Security.MergeByDefault, Env.Security.USFTAReconDefault, Env.Security.USPaymentTypeDefault, Env.Security.USReconIssueDefault, Env.Security.USAllRequestedIndicator });
			AssertReturnMessageResultForAllTargetsWithSpecifiedContext(businessEntity, SupervisorOverridesContext.SendingMessages, new[] { Env.Security.AllowMessageErrors });
			AssertReturnMessageResultForAllTargetsWithSpecifiedContext(businessEntity, SupervisorOverridesContext.SendingPaymentAuthorizationMessage, new[] { Env.Security.USPAYERAccountNumberDefault });
			AssertReturnMessageResultForAllTargetsWithSpecifiedContext(businessEntity, SupervisorOverridesContext.Unknown, new[] { Env.Security.SupervisorOverrides });
		}

		void AssertReturnMessageResultForAllTargetsWithSpecifiedContext(IBusiness businessEntity, string context, SecurityCheckpoint[] checkpoints)
		{
			var dic = new Dictionary<int, bool>();
			for (int i = 0; i < checkpoints.Length; i++)
			{
				var checkpoint = checkpoints[i];
				dic.Add(i, checkpoint.IsAllowed);
				checkpoint.IsAllowed = false;
			}

			try
			{
				using (RegistrySetup registrySetup = new RegistrySetup(TargetInRegistry.All))
				{
					for (int i = 0; i < targetEntityArray.Length; i++)
					{
						SupervisorOverrides supervisorOverrides = new SupervisorOverrides(businessEntity, context);

						if (supervisorOverrides.HasDeclarationAsBusinessEntity)
						{
							SetDataToDeclarationAndImporterOfRecordThatWillDiffer(targetEntityArray[i]);
						}
						else if (supervisorOverrides.HasStatementAsBusinessEntity)
						{
							SetDataToStatementPaymentActionAndImporterThatWillDiffer(targetEntityArray[i]);
						}

						supervisorOverrides.CreateMessages();
						AssertReturnMessageResult(supervisorOverrides, targetEntityArray[i]);
					}
				}
			}
			finally
			{
				foreach (var key in dic.Keys)
				{
					checkpoints[key].IsAllowed = dic[key];
				}
			}
		}

		void AssertCollectionHasMessage(SupervisorOverrides supervisorOverrides, string message)
		{
			foreach (MessageLog messageLog in supervisorOverrides.UnAuthorisedMessagesForLog)
			{
				if (messageLog.Message.ToUpper().Contains(message.ToUpper()))
				{
					return;
				}
			}

			Fail("MessagesForLog should contain the following message: " + message);
		}

		void AssertCollectionHasNoMessage(SupervisorOverrides supervisorOverrides, string message)
		{
			foreach (MessageLog messageLog in supervisorOverrides.UnAuthorisedMessagesForLog)
			{
				if (messageLog.Message.ToUpper() == message.ToUpper())
				{
					Fail("MessagesForLog should NOT contain the following message: " + message);
				}
			}
		}

		void AssertReturnMessageResult(SupervisorOverrides supervisorOverrides, TargetEntity target)
		{
			#region AllowMessageErrors

			if ((target == TargetEntity.All || target == TargetEntity.AllowMessageErrors) && supervisorOverrides.HasDeclarationAsBusinessEntity && supervisorOverrides.ContextIsSendingMessages)
			{
				AssertCollectionHasMessage(supervisorOverrides, AMessageError.Message);
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, AMessageError.Message);
			}

			#endregion

			#region MergeBy

			if ((target == TargetEntity.All || target == TargetEntity.MergeBy) && supervisorOverrides.HasDeclarationAsBusinessEntity && supervisorOverrides.ContextIsSavingDeclaration)
			{
				AssertCollectionHasMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.MergeBy, OrgConstants.MergeInvoiceLines.PartNumber, Declaration.JE_MergeBy));
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.MergeBy, OrgConstants.MergeInvoiceLines.Tariff, Declaration.JE_MergeBy));
			}

			#endregion
			#region NAFTARecon

			if ((target == TargetEntity.All || target == TargetEntity.NAFTARecon) && supervisorOverrides.HasDeclarationAsBusinessEntity && supervisorOverrides.ContextIsSavingDeclaration)
			{
				AssertCollectionHasMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.FTARecon, Declaration.IORWrapper.ZO_NAFTAReconIndicator, Declaration.US_NAFTAReconIndicator));
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.FTARecon, Declaration.IORWrapper.ZO_NAFTAReconIndicator, Declaration.US_NAFTAReconIndicator));
			}

			#endregion

			#region PaymentType

			if ((target == TargetEntity.All || target == TargetEntity.PaymentType) && supervisorOverrides.HasDeclarationAsBusinessEntity && supervisorOverrides.ContextIsSavingDeclaration)
			{
				AssertCollectionHasMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.PaymentType, Declaration.IORWrapper.ZO_PaymentType, Declaration.US_PaymentType));
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.PaymentType, Declaration.IORWrapper.ZO_PaymentType, Declaration.US_PaymentType));
			}

			#endregion

			#region OtherRecon

			if ((target == TargetEntity.All || target == TargetEntity.OtherRecon) && supervisorOverrides.HasDeclarationAsBusinessEntity && supervisorOverrides.ContextIsSavingDeclaration)
			{
				AssertCollectionHasMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.ReconIssue, Declaration.IORWrapper.ZO_OtherReconIndicator, Declaration.US_OtherReconIndicator));
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.ReconIssue, Declaration.IORWrapper.ZO_OtherReconIndicator, Declaration.US_OtherReconIndicator));
			}

			#endregion

			#region PayerAccountNumber

			if ((target == TargetEntity.All || target == TargetEntity.PayerAccountNumber) && supervisorOverrides.HasStatementAsBusinessEntity && supervisorOverrides.ContextIsSendingPaymentAuthorizationMessage)
			{
				AssertCollectionHasMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.PayerAccountNumber, Statement_ImporterWrapper.ZO_AccountNo, Statement.PayerUnitNo));
			}
			else
			{
				AssertCollectionHasNoMessage(supervisorOverrides, string.Format(SupervisorOverrides.Constants.PayerAccountNumber, Statement.PayerUnitNo, Statement_ImporterWrapper.ZO_AccountNo));
			}

			#endregion
		}

		#endregion

		#region SetDataToDeclarationAndImporterOfRecord

		enum TargetEntity
		{
			DefaultsAreEmpty,
			NoTarget,
			All,
			AllowMessageErrors,
			MergeBy,
			NAFTARecon,
			PaymentType,
			OtherRecon,
			PayerAccountNumber,
			AIIRequestedIndicator
		}

		void SetDataToDeclarationAndImporterOfRecordThatWillDiffer(TargetEntity target)
		{
			#region AllowMessageErrors

			if (target == TargetEntity.All || target == TargetEntity.AllowMessageErrors)
			{
				if (!((IMessageNotificationsProvider)Declaration).HasMessageErrors)
				{
					Declaration.AddRowNotification(AMessageError);
				}
				Assert("Precondition: Declaration has MessageErrors", ((IMessageNotificationsProvider)Declaration).HasMessageErrors);
			}
			else
			{
				if (((IMessageNotificationsProvider)Declaration).HasMessageErrors)
				{
					Declaration.RemoveRowNotification(AMessageError);
				}
				Assert("Precondition: Declaration has no MessageErrors", !((IMessageNotificationsProvider)Declaration).HasMessageErrors);
			}

			#endregion

			#region MergeBy

			if (target == TargetEntity.All || target == TargetEntity.MergeBy)
			{
				Declaration.JE_OH_Importer = Declaration.IOROrgPK;
				Declaration.Importer.OH_RL_NKClosestPort = "XXXX";
				Declaration.Importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.PartNumber;

				Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				Assert("Precondition: Declaration.JE_MergeBy is NOT equals to OrgConstants.MergeInvoiceLines.Tariff", Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.Tariff);
			}
			else
			{
				Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				Assert("Precondition: Declaration.JE_MergeBy is equals to OrgConstants.MergeInvoiceLines.Tariff", Declaration.JE_MergeBy == OrgConstants.MergeInvoiceLines.Tariff);
			}

			#endregion

			var wrapper = Declaration.IORWrapper;

			#region NAFTARecon
			Declaration.US_NAFTAReconIndicator = true;
			if (target == TargetEntity.All || target == TargetEntity.NAFTARecon)
			{
				wrapper.ZO_NAFTAReconIndicator = false;
				Assert("Precondition: Declaration.US_NAFTAReconIndicator is NOT equals to importerOfRecordWrapper.ZO_NAFTAReconIndicator", Declaration.US_NAFTAReconIndicator != wrapper.ZO_NAFTAReconIndicator);
			}
			else
			{
				wrapper.ZO_NAFTAReconIndicator = true;
				Assert("Precondition: Declaration.US_NAFTAReconIndicator is equals to importerOfRecordWrapper.ZO_NAFTAReconIndicator", Declaration.US_NAFTAReconIndicator == wrapper.ZO_NAFTAReconIndicator);
			}

			#endregion

			#region PaymentType

			Declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;

			if (target == TargetEntity.All || target == TargetEntity.PaymentType)
			{
				wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
				Assert("Precondition: Declaration.US_PaymentType is NOT equals to importerOfRecordWrapper.ZO_PaymentType", Declaration.US_PaymentType != wrapper.ZO_PaymentType);
			}
			else if (target == TargetEntity.DefaultsAreEmpty)
			{
				wrapper.ZO_PaymentType = ZString.Empty;
				AssertEquals("Precondition: IORWrapper.ZO_PaymentType is empty", ZString.Empty, wrapper.ZO_PaymentType);
			}
			else
			{
				wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
				Assert("Precondition: Declaration.US_PaymentType is equals to importerOfRecordWrapper.ZO_PaymentType", Declaration.US_PaymentType == wrapper.ZO_PaymentType);
			}

			#endregion

			#region OtherRecon

			var otherReconIndicatorCode = ReconIssueCodeList.Codes.ClassRecon; //any code except NAFTA
			Declaration.US_OtherReconIndicator = otherReconIndicatorCode;

			if (target == TargetEntity.All || target == TargetEntity.OtherRecon)
			{
				wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClassRecon; //any code except NAFTA
				Assert("Precondition: Declaration.US_OtherReconIndicator is NOT equals to importerOfRecordWrapper.ZO_OtherReconIndicator", Declaration.US_OtherReconIndicator != wrapper.ZO_OtherReconIndicator);
			}
			else if (target == TargetEntity.DefaultsAreEmpty)
			{
				wrapper.ZO_OtherReconIndicator = ZString.Empty;
				AssertEquals("Precondition: IORWrapper.ZO_OtherReconIndicator is empty", ZString.Empty, wrapper.ZO_OtherReconIndicator);
			}
			else
			{
				wrapper.ZO_OtherReconIndicator = otherReconIndicatorCode;
				Assert("Precondition: Declaration.US_OtherReconIndicator is equals to importerOfRecordWrapper.ZO_OtherReconIndicator", Declaration.US_OtherReconIndicator == wrapper.ZO_OtherReconIndicator);
			}

			#endregion

			#region AII Requested Indicator

			if (target != TargetEntity.NoTarget)
			{
				Declaration.US_IsAIIRequested = true;
			}

			#endregion
		}

		#endregion

		#region SetDataToStatementPaymentActionAndImporter

		void SetDataToStatementPaymentActionAndImporterThatWillDiffer(TargetEntity target)
		{
			string accountNo = "654321";

			Statement.PayerUnitNo = accountNo;

			if (target == TargetEntity.All || target == TargetEntity.PayerAccountNumber)
			{
				Statement_ImporterWrapper.ZO_AccountNo = "123456";
				Assert("Precondition: Statement.AccountNo is NOT equal to importerOfRecordWrapper.ZO_AccountNo", Statement.PayerUnitNo != Statement_ImporterWrapper.ZO_AccountNo);
			}
			else if (target == TargetEntity.DefaultsAreEmpty)
			{
				Statement_ImporterWrapper.ZO_AccountNo = ZString.Empty;
				AssertEquals("Precondition: importerOfRecordWrapper.ZO_AccountNo is empty", ZString.Empty, Statement_ImporterWrapper.ZO_AccountNo);
			}
			else
			{
				Statement_ImporterWrapper.ZO_AccountNo = accountNo;
				Assert("Precondition: Statement.AccountNo is equals to importerOfRecordWrapper.ZO_AccountNo", Statement.PayerUnitNo == Statement_ImporterWrapper.ZO_AccountNo);
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupervisorOverrides(Declaration, SupervisorOverridesContext.SavingDeclaration);
		}

		#endregion

		#region Declaration

		INotification AMessageError
		{
			get { return fAMessageError ?? (fAMessageError = new Notification(CargoWise.EntityFramework.NotificationType.MessageError, "message errors")); }
		}
		INotification fAMessageError;

		JobDeclaration Declaration
		{
			get
			{
				return Declaration_Mock.Object;
			}
		}

		Mock<JobDeclaration> Declaration_Mock
		{
			get
			{
				if (mockForDeclaration == null)
				{
					mockForDeclaration = CurrentFactory.NewMoq<JobDeclaration>();
					mockForDeclaration.Object.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					mockForDeclaration.Object.SuspendValidation();
					mockForDeclaration.Object.JE_MessageType = JobMessageTypeList.Codes.Import;
					mockForDeclaration.Object.IOROrgPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
					mockForDeclaration.Object.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
				}

				return mockForDeclaration;
			}
		}

		Mock<JobDeclaration> mockForDeclaration;

		void ResetDeclaration(BusinessObjectFactory factory = null)
		{
			currentFactory = factory;
			mockForDeclaration = null;
		}

		BusinessObjectFactory currentFactory;
		BusinessObjectFactory CurrentFactory
		{
			get
			{
				if (currentFactory == null)
				{
					currentFactory = Factory;
				}
				return currentFactory;
			}
		}

		#endregion

		#region Statement

		StatementPaymentAction Statement
		{
			get
			{
				if (statement == null)
				{
					var cusStatementHeader = Factory.New<CusStatementHeader>();
					statement = new StatementPaymentAction(cusStatementHeader);
					cusStatementHeader.B2_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				}
				return statement;
			}
		}
		StatementPaymentAction statement;

		OrgHeaderWrapper Statement_ImporterWrapper
		{
			get { return Statement.StatementHeader.ImporterWrapper; }
		}

		#endregion
		#endregion
	}
}
