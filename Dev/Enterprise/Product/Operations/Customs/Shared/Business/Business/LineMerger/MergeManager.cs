using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	public class MergeManager
	{
		public MergeManager(BaseJobDeclaration declaration)
		{
			this.Declaration = declaration;
		}

		public bool Execute() => Execute(Declaration.MessageInitiator);

		public bool Execute(ISendsMessagesToCustoms notifier)
		{
			var result = ExecuteCore(notifier);
			requiresMergeCached = null;
			return result;
		}

		public virtual ZString CheckAndGetPrerequisiteConditions(ISendsMessagesToCustoms notifier)
		{
			CreateDummyInvoiceLinesForMergeIfNeeded();

			var result = GetReasonCannotMerge();

			if (!string.IsNullOrEmpty(result))
			{
				notifier.NotifyUserOfAnInvalidOperation(result);
			}
			else
			{
				var abnormalDataMessage = GetAbnormalDataWarningMessage();
				if (!string.IsNullOrEmpty(abnormalDataMessage) && !notifier.ContinueWithAction(abnormalDataMessage, Res.GetString("100aefe3-50de-4155-8014-f79cd01a89a3", "Continue merge with abnormal data")))
				{
					result = abnormalDataMessage;
				}
				else if (!string.IsNullOrEmpty(abnormalDataMessage))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Declaration.Logs.AddNew(Events.EditedARecord, "User chose to continue with merge despite abnormal data");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			return result;
		}

		protected virtual bool ExecuteCore(ISendsMessagesToCustoms notifier)
		{
			fInvalidOperationText = CheckAndGetPrerequisiteConditions(notifier);

			if (InvalidOperationText.IsEmpty)
			{
				Declaration.AddMergeFetchHints();

				if (Declaration.ApportionmentDirty)
				{
					Declaration.ResumeApportionment();
				}

				var suspendedThingys = new List<IDisposable>();
				suspendedThingys.Add(Declaration.CustomsEntryHeaders.SuspendListChanged());
				foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
				{
					suspendedThingys.Add(entryHeader.MergedLines.SuspendListChanged());
				}

				try
				{
					using (Declaration.GetValidationSuspender())
					{
						var support = new BaseJobDeclaration.InvoicesOverrideDeclarationSupporter(Declaration);
						try
						{
							var merger = GetNewLineMerger();
							merger.DoMerge();
						}
						finally
						{
							support.Dispose();
						}
					}
				}
				finally
				{
					foreach (IDisposable thingy in suspendedThingys)
					{
						thingy.Dispose();
					}
				}

				Declaration.DeriveDeclarationStatus();
			}

			if (InvalidOperationText.IsEmpty)
			{
				NotifyThatDeclarationIsInAMergedState();

				OnMerged();
			}

			return InvalidOperationText.IsEmpty;
		}

		internal void CreateDummyInvoiceLinesForMergeIfNeeded()
		{
			if (Declaration.ShouldCreateDummyInvoiceLinesForMerge)
			{
				foreach (var invoice in Declaration.Invoices)
				{
					BaseJobComInvoiceLine dummyInvoiceLine = null;
					if (invoice.JobComInvoiceLines.Count == 1)
					{
						dummyInvoiceLine = invoice.JobComInvoiceLines[0];
					}
					else
					{
						if (invoice.JobComInvoiceLines.Count > 1)
						{
							invoice.JobComInvoiceLines.RemoveAndDeleteAll();
						}
						dummyInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					}
					dummyInvoiceLine.JI_LinePrice = dummyInvoiceLine.JI_Calc_LinesTotal;
				}
			}
		}

		protected void NotifyThatDeclarationIsInAMergedState()
		{
			HasChangesSinceMergeHunter.Mark();
		}

		public class ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge : IDisposable
		{
			public ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(BaseJobDeclaration declaration)
			{
				needsMergeStateAtInstantion = declaration.MergeManager.RequiresMerge && declaration.MergeManager.SupportsAutoMerge;
				this.declaration = declaration;
			}

			readonly BaseJobDeclaration declaration;

			public void Dispose()
			{
				if (!needsMergeStateAtInstantion)
				{
					declaration.MergeManager.NotifyThatDeclarationIsInAMergedState();
				}
			}

			readonly bool needsMergeStateAtInstantion;
		}

		public LineMerger GetNewLineMerger()
		{
			var merger = GetNewLineMergerCore();
			merger.SupportsAmendments = SupportsAmendments;
			return merger;
		}

		protected virtual LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected virtual string GetReasonCannotMerge()
		{
			string result = "";
			if (ShouldCheckExistenceOfInvoices && Declaration.Invoices.Count == 0)
			{
				result = Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders;
			}
			else if (ShouldCheckExistenceOfInvoiceLineForAllInvoices &&
				!Declaration.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader)
			{
				result = Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines;
			}
			return result;
		}

		protected virtual string GetAbnormalDataWarningMessage() => string.Empty;

		protected virtual bool ShouldCheckExistenceOfInvoices => true;

		protected virtual bool ShouldCheckExistenceOfInvoiceLineForAllInvoices => true;

		public ZString InvalidOperationText => fInvalidOperationText;

		public bool MergeSuccessful => InvalidOperationText.IsEmpty;

		public ZString HumanReadableNameForMerge => HumanReadableNameForMergeCore;

		protected virtual ZString HumanReadableNameForMergeCore => Res.GetString("d94d9d8a-6141-421a-8fe9-7246aafdb107", "merge");

		public bool RequiresMerge
		{
			get
			{
#if DEBUG
				if (mergeRequirementDisabledForTesting)
				{
					return false;
				}
#endif

				if (requiresMergeCalculatedValue.HasValue)
				{
					return requiresMergeCalculatedValue.Value;
				}
				else
				{
					if (requiresMergeCached == null)
					{
						requiresMergeCached = new CachedProperty<bool>(Declaration.Factory, () =>
						{
							bool result = false;

							if (Declaration == null || !Declaration.IsDeclarationIntegrated)
							{
								result = RequiresMergeCore;
							}

							return result;
						});
					}
					return requiresMergeCached.Value;
				}
			}
		}
		protected CachedProperty<bool> requiresMergeCached;
		bool? requiresMergeCalculatedValue;

		public IDisposable SuspendRequiresMergeCalculation()
		{
			requiresMergeCalculatedValue = null; // disable caching
			requiresMergeCalculatedValue = RequiresMerge;
			return new DisposableAction(() => requiresMergeCalculatedValue = null);
		}

		public bool RequiresMergeBeforeSave => SupportsAutoMerge && SupportsAmendments && !PersistsMergeState && RequiresMerge;

		public bool SupportsAutoMerge => SupportsAutoMergeCore;

		protected virtual bool SupportsAutoMergeCore => Declaration == null || !Declaration.IsDeclarationIntegrated;

		protected virtual bool SupportsAmendments => true;

		protected virtual bool PersistsMergeState => false;

		protected virtual bool RequiresMergeCore => Declaration.CustomsEntryHeaders.Count > 0 && HasChangesSinceMergeHunter.HasChangesSinceLastMark;

		/// <summary>
		/// This is after merge is marked as DONE. Should not cause any new HasChanges.
		/// Refresh Validations
		/// </summary>
		protected virtual void OnMerged()
		{
			//ValidateCH_BGMReference needs to be fired for active and inactive entries after merge is done
			//it has a post-merge checks
			foreach (CusEntryHeader entry in Declaration.CustomsEntryHeaders)
			{
				entry.Validation.ValidateCH_BGMReference();
			}
			Declaration.CustomsEntryInstructions?.OfType<CusEntryInstruction>().ForEach(x => x.Validation.ValidateAgainstLinkedEntryHeaders());
		}

#if DEBUG
		bool mergeRequirementDisabledForTesting;
		public void DisablePreSaveMergeRequirementForTesting()
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("Testing only");
			}

			mergeRequirementDisabledForTesting = true;
		}
#endif

		protected HasChangesHunter HasChangesSinceMergeHunter
		{
			get
			{
				if (fMergeHasChangesHunter == null)
				{
					List<HasChangesHunterExclusionDetails> typesNotToAffectMerge = GetTypesWhichDoNotEffectMerge();
					fMergeHasChangesHunter = new HasChangesHunter(Declaration, typesNotToAffectMerge.ToArray());
					if (ExcludeNonCustomsNamespaces)
					{
						fMergeHasChangesHunter.AddIncludedNamespacePrefix("Enterprise.Customs");
						fMergeHasChangesHunter.AddIncludedNamespacePrefix("Enterprise.MasterFiles.Business.CustomValues");
					}
				}
				return fMergeHasChangesHunter;
			}
		}
		protected HasChangesHunter fMergeHasChangesHunter;

		public IBusiness LastEntityToCauseMergeToBeRequiredForDebugging => HasChangesSinceMergeHunter.LastEntityFoundWithChanges;

		protected virtual bool ExcludeNonCustomsNamespaces => true;

		protected virtual List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge()
		{
			var result = new List<HasChangesHunterExclusionDetails>();
			result.Add(new HasChangesHunterExclusionDetails(typeof(GenCustomAddOnValue)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(StmALog)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryHeaderCollection<CusEntryHeader>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryLineCollection<CusEntryLine>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryHeader)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(EDIMessage)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryNumber)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryHeaderCharges)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryLineFee)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(BaseCusEntryCPDec)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(BasePackage)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(BasePackageCollection)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(BaseDeclarationLevelPackageCollection<BasePackage>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryPayInfo)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(CusEntryPayInfoCollection<CusEntryPayInfo>)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(ProcessTask)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AdditionalInvoiceLineEntryLineLink)));
			result.Add(new HasChangesHunterExclusionDetails(typeof(AdditionalLineLinkInvoiceLineCollection)));

			if (!ShouldBillsBeTypesThatAffectMerge)
			{
				result.Add(new HasChangesHunterExclusionDetails(typeof(Bill)));
				result.Add(new HasChangesHunterExclusionDetails(typeof(BillCollection<Bill, BaseJobDeclaration>)));
				result.Add(new HasChangesHunterExclusionDetails(typeof(BillTypeViewCollection<Bill, BaseJobDeclaration>)));
				result.Add(new HasChangesHunterExclusionDetails(typeof(ChildBillCollection<Bill, BaseJobDeclaration>)));
			}

			if (!ShouldPackingGroupsBeTypesThatAffectMerge)
			{
				result.Add(new HasChangesHunterExclusionDetails(typeof(BasePackingGroup)));
				result.Add(new HasChangesHunterExclusionDetails(typeof(BasePackingGroupCollection)));
				result.Add(new HasChangesHunterExclusionDetails(typeof(BaseDeclarationLevelPackingGroupCollection)));
			}

			if (!ShouldContainersBeTypesThatAffectMerge)
			{
				result.Add(new HasChangesHunterExclusionDetails(typeof(BaseCusContainer)));
			}

			return result;
		}

		protected virtual bool ShouldBillsBeTypesThatAffectMerge => false;

		protected virtual bool ShouldPackingGroupsBeTypesThatAffectMerge => false;

		protected virtual bool ShouldContainersBeTypesThatAffectMerge => true;

		#region Implementation

		protected readonly BaseJobDeclaration Declaration;
		ZString fInvalidOperationText;
		#endregion
	}
}
