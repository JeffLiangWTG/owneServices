using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MergeManagerTestOnlyForBase : TestCaseWithFactory
	{
		//EndToEnd test in AU CusEntryHeaderValidation.cs which calls a concrete Execute()
		[ExpectNoExceptions]
		public void TestValidateCH_BGMReferenceFiredOnMerged()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);
			mockDeclaration.Setup(m => m.IsCustomsLineAmendmentATotalReplacement).Returns(false);

			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockDeclaration.Object.CustomsEntryHeaders.Add(mockEntry.Object);

			var mockValidation = new Mock<CusEntryHeaderValidation>(mockEntry.Object) { CallBase = true };
			mockValidation
				.Protected()
				.Setup("CheckCH_BGMReference");
			mockEntry
				.Protected()
				.Setup<CusEntryHeaderValidation>("GetNewValidation")
				.Returns(mockValidation.Object);

			var mergeManager = mockDeclaration.Object.MergeManager;
			var onMergedMethodInfo = typeof(MergeManager).GetMethod("OnMerged",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			onMergedMethodInfo.Invoke(mergeManager, Array.Empty<object>());

			mockValidation.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestValidateEntryInstructionsAgainstLinkedEntryHeadersFiredOnMerged()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration
				.Protected()
				.Setup<EntryInstructionProvider>("GetCustomsEntryInstructionProviderCore")
				.Returns(new EntryInstructionProvider(mockDeclaration.Object));

			var mockEntryInstruction = Factory.NewMoq<CusEntryInstruction>();
			mockDeclaration.Object.CustomsEntryInstructions.Add(mockEntryInstruction.Object);
			var mockEntryInstructionValidation = new Mock<CusEntryInstructionValidation>(mockEntryInstruction.Object) { CallBase = true };
			mockEntryInstructionValidation
				.Protected()
				.Setup("ValidateAgainstLinkedEntryHeadersCore");
			mockEntryInstruction
				.Protected()
				.Setup<CusEntryInstructionValidation>("GetNewValidation")
				.Returns(mockEntryInstructionValidation.Object);

			var mockEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockEntryHeader.Object.CH_CEI_Instruction = mockEntryHeader.Object.PK;
			mockDeclaration.Object.CustomsEntryHeaders.Add(mockEntryHeader.Object);

			var mergeManager = mockDeclaration.Object.MergeManager;
			var onMergedMethodInfo = typeof(MergeManager).GetMethod("OnMerged",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			onMergedMethodInfo.Invoke(mergeManager, Array.Empty<object>());

			mockEntryInstructionValidation.VerifyAll();
		}

		public void TestCheckAndGetPrerequisiteConditions()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertContains("There is no Invoice yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, errorCondition);

			declaration.Invoices.AddNew();
			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertContains("There is no Invoice line yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines, errorCondition);

			declaration.InvoiceLines.AddNew();
			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("There is now a invoice header and invoice line", ZString.Empty, errorCondition);
		}

		public void TestCreateDummyInvoiceLinesWhenPrerequisiteConditionsAreChecked()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(true);

			var declaration = mockDeclaration.Object;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;

			var manager = new MergeManager(declaration);

			var errorCondition = manager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("no invoice lines should not be a problem because system creates an invoice line automatically. AU SAC without lines", ZString.Empty, errorCondition);
		}

		public void TestCreateDummyInvoiceLinesBeforeMerge()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(true);

			var declaration = mockDeclaration.Object;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;

			var manager = new MergeManager(declaration);

			AssertEquals("PreCondition: No invoice lines", 0, invoice.JobComInvoiceLines.Count);
			manager.Execute(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One invoice line should have been created", 1, invoice.JobComInvoiceLines.Count);
			AssertEquals("Invoice should be balanced", 10000m, invoice.JobComInvoiceLines[0].JI_LinePrice);
		}

		public void TestExcludeNonCustomsNamespaces()
		{
			var expectedIncludedNonCustomsNamespaces = new[] { "Enterprise.Customs", "Enterprise.MasterFiles.Business.CustomValues" };

			var hasChangesSinceMergeHunterPropertyInfo = typeof(MergeManager).GetProperty("HasChangesSinceMergeHunter",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var hasChangesSinceMergeHunter = hasChangesSinceMergeHunterPropertyInfo.GetValue(new MergeManager(null)) as HasChangesHunter;

			AssertArrayEqualsByElements("Included namespaces", expectedIncludedNonCustomsNamespaces, hasChangesSinceMergeHunter.IncludedPrefixes);
		}

		public void TestOverridingExcludeNonCustomsNamespacesAndReturningFalseDisablesNonCustomsFiltering()
		{
			var mock = new Mock<MergeManager>(new object[] { null });
			mock.Protected().Setup<bool>("ExcludeNonCustomsNamespaces").Returns(false);
			mock.CallBase = true;
			var manager = mock.Object;

			var hasChangesSinceMergeHunterPropertyInfo = typeof(MergeManager).GetProperty("HasChangesSinceMergeHunter",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var hasChangesSinceMergeHunter = hasChangesSinceMergeHunterPropertyInfo.GetValue(manager) as HasChangesHunter;

			AssertEquals("ExcludeNonCustomsNamespaces = false should not restrict included prefixes", 0, hasChangesSinceMergeHunter.IncludedPrefixes.Length);
		}

		public void TestGetReasonCannotMerge()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var mergeManager = testDec.MergeManager;
			var getReasonCannotMergeMethodInfo = typeof(MergeManager).GetMethod("GetReasonCannotMerge",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());

			AssertEquals("There is no Invoice yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, reason);

			var topGroup = testDec.JobComInvoiceGroupHeaders[0];
			var subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();

			var invoice = subGroup.JobComInvoiceHeaders.AddNew();
			reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("There is no Invoice line yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines, reason);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			reason = (string)getReasonCannotMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
			AssertEquals("There is now a invoice header and invoice line", true, reason != Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders && reason != Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines);
		}

		public void TestGetTypesWhichDoNotEffectMerge()
		{
			var mergeManager = new MergeManager(Factory.New<BaseJobDeclaration>());
			var excludedTypes = MergeManagerTestHelper.GetTypesWhichDoNotEffectMerge(mergeManager);

			AssertNotNull("GenCustomAddOnValue exclusion item", excludedTypes.SingleOrDefault(x => x.TypeToExclude == typeof(GenCustomAddOnValue)));
		}

		public void TestGenAddOnColumnAffectsRequiresMerge()
		{
			var declaration = Factory.New<TestDeclarationWithGenAddOn>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.GenAddOnColumnForTest = "whatever";
			declaration.DoMerge();
			Factory.Save();
			Assert("PRE-Condition", !declaration.MergeManager.RequiresMerge);
			declaration.GenAddOnColumnForTest = "new value";
			Assert(declaration.MergeManager.RequiresMerge);
		}

		[SystemDefinedValues]
		class TestDeclarationWithGenAddOn : BaseJobDeclaration
		{
			public TestDeclarationWithGenAddOn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString GenAddOnColumnForTest
			{
				get => this.GetSystemDefinedValue<ZString>(nameof(GenAddOnColumnForTest));
				set => this.SetSystemDefinedValue(nameof(GenAddOnColumnForTest), value);
			}
		}
	}

	public class MergeManagerTest : TestCaseWithFactory
	{
		public void TestDefaultMergeInformation()
		{
			new DummyMergeManager(GetJobDeclaration()).AssertBaseDefaults();
		}

		[DeveloperOnlyTest]
		public void TestHasChanges_Performance()
		{
			var declaration = GetJobDeclaration();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			for (var i = 0; i < 100; i++)
			{
				var invoice = declaration.Invoices.AddNew();
				for (var j = 0; j < 100; j++)
				{
					invoice.InvoiceLines.AddNew();
				}
			}
			declaration.DoMerge();
			Factory.Save();
			var mergeManager = declaration.MergeManager;
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 10; i++)
			{
				_ = mergeManager.RequiresMerge;
				Factory.InvalidateCachedProperties();
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 1000);
		}

		public void TestSetsSupportsAmendmentsOnLineMerger()
		{
			var merger = new DummyMergeManager(GetJobDeclaration());
			merger.SupportsAmendmentsExposed = true;
			var lineMerger = merger.GetNewLineMerger();
			AssertEquals("LineMerger.SupportsAmendments", true, lineMerger.SupportsAmendments);

			merger.SupportsAmendmentsExposed = false;
			lineMerger = merger.GetNewLineMerger();
			AssertEquals("LineMerger.SupportsAmendments", false, lineMerger.SupportsAmendments);
		}

		public void TestLineMergerIsRightType()
		{
			var declaration = GetJobDeclaration();
			AssertEquals("LineMerger Type", GetLineMergerType(), declaration.MergeManager.GetNewLineMerger().GetType());
		}

		public virtual void TestHumanReadableNameForMerge()
		{
			var testDec = GetJobDeclaration();
			AssertEquals("HumanReadableNameForMerge", "merge", testDec.MergeManager.HumanReadableNameForMerge);
		}

		public virtual void TestShouldCheckExistenceOfInvoices()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("ShouldCheckExistenceOfInvoices", true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoices(manager));
		}

		public virtual void TestShouldCheckExistenceOfInvoiceLineForAllInvoices()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("ShouldCheckExistenceOfInvoiceLineForAllInvoices", true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoiceLineForAllInvoices(manager));
		}

		public virtual void TestPersistsMergeState()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("PersistsMergeState", false, MergeManagerTestHelper.GetPersistsMergeState(manager));
		}

		public virtual void TestSupportsAmendments()
		{
			var manager = GetJobDeclaration().MergeManager;
			AssertEquals("SupportsAmendments", true, MergeManagerTestHelper.GetSupportsAmendments(manager));
		}

		public virtual void TestRequiresMerge()
		{
			var dec = ImportJobDeclaration;

			var invoiceHeader = dec.Invoices.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_Tariff = "01010101";

			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();

			AssertEquals("RequiresMerge", !dec.IsDeclarationIntegrated, dec.MergeManager.RequiresMerge);
		}

		public virtual void TestSupportsAutoMerge()
		{
			var dec = ImportJobDeclaration;
			dec.ActiveEntryHeaders.AddNew();

			AssertEquals("SupportsAutoMerge", !dec.IsDeclarationIntegrated, dec.MergeManager.SupportsAutoMerge);
		}

		protected virtual Type GetLineMergerType() => typeof(LineMerger);

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			return declaration;
		}

		protected virtual BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = GetJobDeclaration();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				return result;
			}
		}

		protected virtual bool DoesHouseBillAffectMerge => false;
		protected virtual bool DoesPackingGroupAffectMerge => false;

		protected class DummyMergeManager : MergeManager
		{
			public DummyMergeManager(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public bool SupportsAutoMergeExposed;
			protected override bool SupportsAutoMergeCore
			{
				get { return SupportsAutoMergeExposed; }
			}

			public bool RequiresMergeExposed
			{
				get { return requiresMergeExposed; }
				set
				{
					requiresMergeExposed = value;
					requiresMergeCached = null;
				}
			}
			bool requiresMergeExposed;

			protected override bool RequiresMergeCore
			{
				get { return RequiresMergeExposed; }
			}

			public bool PersistsMergedStateExposed;
			protected override bool PersistsMergeState
			{
				get { return PersistsMergedStateExposed; }
			}

			public bool SupportsAmendmentsExposed;
			protected override bool SupportsAmendments
			{
				get { return SupportsAmendmentsExposed; }
			}

			public void AssertBaseDefaults()
			{
				AssertEquals("SupportsAutoMerge", !Declaration.IsDeclarationIntegrated, base.SupportsAutoMergeCore);
				AssertEquals("PersistsMergeState", false, base.PersistsMergeState);
				AssertEquals("SupportsAmendments", true, base.SupportsAmendments);
			}
		}
	}

	public class MergeManagerWhichSupportsAutoMergeTest : MergeManagerTest
	{
		public void TestAutoMergeWhenHouseMultiPackChanges()
		{
			var declaration = ImportJobDeclaration;
			declaration.FillWithValidTestData();
			declaration.DisableDefaultPackingInformation = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.JE_HouseBill = "HBL1010";
			declaration.JE_MasterBill = "master";
			declaration.DoMerge();
			AssertEquals("PreCondition", false, declaration.MergeManager.RequiresMerge);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C0987";
			declaration.DoMerge();
			AssertEquals("PreCondition", false, declaration.MergeManager.RequiresMerge);

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HBL1010";
			AssertEquals("housebill change cause re-merge?", DoesHouseBillAffectMerge, declaration.MergeManager.RequiresMerge);

			declaration.DoMerge();
			Assert("PreCondition", !declaration.MergeManager.RequiresMerge);

			var packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CU_HouseBill = houseBill.PK;
			packGroup.CR_CO_Container = container.PK;
			AssertEquals("packGroup change cause re-merge?", DoesPackingGroupAffectMerge, declaration.MergeManager.RequiresMerge);
		}

		public void TestApportionBeforeRunningValidation()
		{
			BaseJobDeclaration declaration = GetPreMergeDeclaration();

			declaration.JE_MergeBy = "";//causes an error
			declaration.ApportionmentDirty = true;
			AssertEquals("Apportionment is dirty", true, declaration.ApportionmentDirty);

			bool merged = declaration.DoMerge();
			AssertEquals("Merge-By validated", true, declaration.JE_MergeByInfo.HasErrors());
			AssertEquals("Apportionment is resumed before validatin runs", false, declaration.ApportionmentDirty);
		}

		public void TestChangingMergedDeclarationInAWayThatDoesNotRequireReMerge()
		{
			BaseJobDeclaration declaration = GetMergedDeclaration();
			AssertEquals("requires merge", false, declaration.MergeManager.RequiresMerge);
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_EntryStatus = "AAA";
			}
			AssertEquals("requires merge", false, declaration.MergeManager.RequiresMerge);
		}

		public void TestRequiresMergeBeforeSave()
		{
			var dummy = new DummyMergeManager(GetPreMergeDeclaration());

			dummy.SupportsAutoMergeExposed = true;
			dummy.SupportsAmendmentsExposed = true;
			dummy.PersistsMergedStateExposed = false;
			dummy.RequiresMergeExposed = true;
			AssertEquals("RequiresMergeBeforeSave", true, dummy.RequiresMergeBeforeSave);

			dummy.SupportsAutoMergeExposed = false;
			dummy.SupportsAmendmentsExposed = true;
			dummy.PersistsMergedStateExposed = false;
			dummy.RequiresMergeExposed = true;
			AssertEquals("RequiresMergeBeforeSave", false, dummy.RequiresMergeBeforeSave);

			dummy.SupportsAutoMergeExposed = true;
			dummy.SupportsAmendmentsExposed = false;
			dummy.PersistsMergedStateExposed = false;
			dummy.RequiresMergeExposed = true;
			AssertEquals("RequiresMergeBeforeSave", false, dummy.RequiresMergeBeforeSave);

			dummy.SupportsAutoMergeExposed = true;
			dummy.SupportsAmendmentsExposed = true;
			dummy.PersistsMergedStateExposed = true;
			dummy.RequiresMergeExposed = true;
			AssertEquals("RequiresMergeBeforeSave", false, dummy.RequiresMergeBeforeSave);

			dummy.SupportsAutoMergeExposed = true;
			dummy.SupportsAmendmentsExposed = true;
			dummy.PersistsMergedStateExposed = false;
			dummy.RequiresMergeExposed = false;
			AssertEquals("RequiresMergeBeforeSave", false, dummy.RequiresMergeBeforeSave);
		}

		public void TestRequiresMergeIsOnlyEffectedByRelevantChangesCountrySpecific()
		{
			BaseJobDeclaration declaration = GetMergedDeclaration();

			declaration.JE_DateAtOrigin = new ZDateTime(2000, 1, 1);
			CheckRequiresMergeThenMerge(declaration);

			declaration.Invoices[0].JZ_PaymentNo = "ABC";
			CheckRequiresMergeThenMerge(declaration);
		}

		public void TestRequiresMergeIsOnlyEffectedByRelevantChanges()
		{
			BaseJobDeclaration declaration = GetMergedDeclaration();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_TotalPaid = 12234;
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 34;
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			declaration.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Empty, true);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			CusEntryHeaderCharges headerCharge = entryHeader.Charges.AddNew();
			headerCharge.C1_ChargeAmount = 100m;
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			BaseCusEntryCPDec question = Factory.New<BaseCusEntryCPDec>();
			question.ON_CH = entryHeader.PK;
			question.ON_AnswerCode = "Y";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			CusEntryLineFee lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeAmount = 23;
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			BaseCusEntryCPDec lineQuestion = Factory.New<BaseCusEntryCPDec>();
			lineQuestion.ON_CH = entryLine.PK;
			lineQuestion.ON_AnswerCode = "Y";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			declaration.Invoices[0].JZ_PaymentNo = "ABC";
			AssertEquals("RequiresMerge", true, declaration.MergeManager.RequiresMerge);
		}

		protected void CheckRequiresMergeThenMerge(BaseJobDeclaration declaration)
		{
			AssertEquals("RequiresMerge", true, declaration.MergeManager.RequiresMerge);
			declaration.DoMerge();
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
		}

		public void TestNoMergeRequiredUntilAMergeHasBeenCarriedOut()
		{
			BaseJobDeclaration declaration = GetPreMergeDeclaration();
			declaration.Invoices[0].JZ_PaymentNo = "ABC";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			declaration.DoMerge();
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			declaration.Invoices[0].JZ_PaymentNo = "XYZ";
			AssertEquals("RequiresMerge", true, declaration.MergeManager.RequiresMerge);
		}

		protected BaseJobDeclaration GetPreMergeDeclaration()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			return declaration;
		}

		protected BaseJobDeclaration GetMergedDeclaration()
		{
			var declaration = GetPreMergeDeclaration();
			declaration.DoMerge();
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
			return declaration;
		}
	}

	public static class MergeManagerTestHelper
	{
		public static void InvokeNotifyThatDeclarationIsInAMergedState(MergeManager mergeManager)
		{
			var notifyThatDeclarationIsInAMergedStateMethodInfo = typeof(MergeManager).GetMethod("NotifyThatDeclarationIsInAMergedState",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			notifyThatDeclarationIsInAMergedStateMethodInfo.Invoke(mergeManager, Array.Empty<object>());
		}

		public static bool GetPersistsMergeState(MergeManager mergeManager)
		{
			var persistsMergeStatePropertyInfo = typeof(MergeManager).GetProperty("PersistsMergeState",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (bool)persistsMergeStatePropertyInfo.GetValue(mergeManager);
		}

		public static bool GetSupportsAmendments(MergeManager mergeManager)
		{
			var supportsAmendmentsPropertyInfo = typeof(MergeManager).GetProperty("SupportsAmendments",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (bool)supportsAmendmentsPropertyInfo.GetValue(mergeManager);
		}

		public static bool GetShouldCheckExistenceOfInvoices(MergeManager mergeManager)
		{
			var shouldCheckExistenceOfInvoicesPropertyInfo = typeof(MergeManager).GetProperty("ShouldCheckExistenceOfInvoices",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (bool)shouldCheckExistenceOfInvoicesPropertyInfo.GetValue(mergeManager);
		}

		public static bool GetShouldCheckExistenceOfInvoiceLineForAllInvoices(MergeManager mergeManager)
		{
			var shouldCheckExistenceOfInvoiceLineForAllInvoicesPropertyInfo = typeof(MergeManager).GetProperty("ShouldCheckExistenceOfInvoiceLineForAllInvoices",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (bool)shouldCheckExistenceOfInvoiceLineForAllInvoicesPropertyInfo.GetValue(mergeManager);
		}

		public static List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge(MergeManager mergeManager)
		{
			var getTypesWhichDoNotEffectMergeMethodInfo = typeof(MergeManager).GetMethod("GetTypesWhichDoNotEffectMerge",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			return (List<HasChangesHunterExclusionDetails>)getTypesWhichDoNotEffectMergeMethodInfo.Invoke(mergeManager, Array.Empty<object>());
		}
	}
}
