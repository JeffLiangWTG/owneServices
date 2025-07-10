using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class EntryCreationStrategy
	{
		public EntryCreationStrategy(BaseJobDeclaration declaration)
		{
			Declaration = declaration;
			mergeKeys = new Dictionary<BaseJobComInvoiceLine, MergeKey>();
			lineMergeKeysAtLastSave = new Dictionary<BaseJobComInvoiceLine, MergeKey>(declaration.InvoiceLines.Count);
			IsNoEntryInstruction = Declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction;
		}
		readonly protected bool IsNoEntryInstruction;

		public EntryCreationStrategy(BaseJobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader)
			: this(declaration)
		{
			this.CH_MessageTypeToNewEntryHeader = entryHeaderMessageTypeToNewEntryHeader;
		}

		readonly Dictionary<BaseJobComInvoiceLine, MergeKey> lineMergeKeysAtLastSave;

		protected BaseJobDeclaration Declaration { get; private set; }
		protected ZString CH_MessageTypeToNewEntryHeader { get; private set; }

		internal CusEntryLine GetOrCreateEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			return EntryManager.GetOrCreateEntryLine(invoiceLine);
		}

		internal void DiscardUnusedObjects()
		{
			EntryManager.DiscardUnusedEntryAndEntryLines();
			DiscardUnusedObjectsCore();
		}

		protected internal virtual void Initiate()
		{
		}

		protected internal virtual void DiscardUnusedObjectsCore()
		{
		}

		protected internal virtual IEnumerable<CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		}

		protected internal virtual IEnumerable<CusEntryLine> GetExistingEntryLinesCreatedThroughThisStrategy()
		{
			foreach (CusEntryHeader entry in GetExistingEntriesCreatedThroughThisStrategy())
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					yield return entryLine;
				}
			}
		}

		internal bool IsActive
		{
			get { return IsActiveCore; }
		}

		/// <summary>
		/// Eg. Is a certain messaging mode enabled? 
		/// </summary>
		protected virtual bool IsActiveCore
		{
			get { return true; }
		}

		/// <summary>
		/// Provides a location to attach invoice lines and entry lines in a non standard fashion.
		/// This would be overridden for each different creation strategy as there is only one link supported between line and entry by default.
		/// An example is the RIB message for South African Customs
		/// </summary>
		/// <param name="entryLine"></param>
		/// <param name="baseInvoiceLine"></param>
		protected internal virtual AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			baseInvoiceLine.JI_CL = entryLine.PK;
			return null;
		}

		protected internal virtual void AfterCreateOrGetEntryHeader(CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			// A different type of entry can get recycled and turn into a different type
			// US disables merge if an entry with active messages exist and messaging modes change but until the entry is withdrawn
			// The entry gets deactivated after withdrawn and deactivated entries are not to be recycled.
			if (!CH_MessageTypeToNewEntryHeader.IsEmpty)
			{
				entryHeader.CH_MessageType = CH_MessageTypeToNewEntryHeader;
			}
			entryHeader.CH_CEI_Instruction = IsNoEntryInstruction ? ZGuid.Empty : baseInvoiceLine.JI_CEI;
		}

		protected internal virtual void AfterCreateOrGetEntryLine(CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
		}

		/// <summary>
		/// Indicate whether or not the line meets the criteria to be included in the entry
		/// </summary>
		/// <param name="baseInvoiceLine"></param>
		/// <returns></returns>
		public virtual bool LineIsValidForMerge(BaseJobComInvoiceLine baseInvoiceLine)
		{
			return true;
		}

		protected internal virtual void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_CL = ZGuid.Empty;
		}

		protected internal virtual IComparer<BaseJobComInvoiceLine> GetInvoiceLineComparerForMerge(ReadOnlyBusinessObjectFactory cleanFactory)
		{
			return new InvoiceLineComparerForMerge(this, cleanFactory);
		}

		/// <summary>
		/// Return the key that defines the 'mergeability' of a particular entry header
		/// The invoice line is passed as some header merge details may be available in the line.
		/// An example of this is the purpose code for South African Customs
		/// </summary>
		/// <param name="invoiceLine"></param>
		/// <returns></returns>
		public MergeKey GetKeyForHeader(BaseJobComInvoiceLine invoiceLine)
		{
			MergeKey result = new MergeKey();
			result.Add(CH_MessageTypeToNewEntryHeader);
			result += GetKeyForHeaderCore(invoiceLine);
			return result;
		}

		protected virtual MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			MergeKey result = new MergeKey();
			var invoiceHeader = invoiceLine.InvoiceHeader;

			result.Add(IsValuationDatePartOfMergeKey && invoiceHeader != null ? invoiceHeader.EffectiveValuationDate.Date : ZDate.Empty);
			result.Add(IsNoEntryInstruction ? ZGuid.Empty : invoiceLine.JI_CEI);
			return result;
		}

		protected virtual bool IsValuationDatePartOfMergeKey => true;

		protected internal virtual CusEntryHeader GetExistingEntryHeader(BaseJobComInvoiceLine invoiceLine)
		{
			CusEntryLine entryLine = GetExistingEntryLine(invoiceLine);
			return entryLine == null ? null : entryLine.Header;
		}

		/// <summary>
		/// Return the key that defines the 'mergeability' of a particular entry line
		/// the base implementation should normally be called, as this implements the 'merge by' option
		/// </summary>
		/// <param name="baseInvoiceLine"></param>
		/// <returns></returns>
		public virtual MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			ZString partNumber = baseInvoiceLine.JI_PartNo;
			BaseCusClassification classification = baseInvoiceLine.Classification;
			ZString lookupCode = classification == null ? ZString.Empty : classification.CC_LookupCode;
			ZString tariff = baseInvoiceLine.JI_Tariff;
			ZGuid invoiceLinePK = baseInvoiceLine.PK;

			MergeKey result = new MergeKey();
			switch (GetMergeBy())
			{
				case OrgConstants.MergeInvoiceLines.PartNumber:
				case OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription:
					result.Add(partNumber);
					result.Add(lookupCode);
					result.Add(tariff);
					break;
				case OrgConstants.MergeInvoiceLines.Classification:
				case OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways:
					result.Add(lookupCode);
					result.Add(tariff);
					break;
				case OrgConstants.MergeInvoiceLines.TariffAndDescription:
					result.Add(GetTariffAndDescriptionKey(baseInvoiceLine));
					if (baseInvoiceLine.IsExtendedCommercialDescriptionEnabled)
					{
						result.Add(baseInvoiceLine.JI_ExtraInfoForClassification);
					}
					result.Add(tariff);
					break;
				case OrgConstants.MergeInvoiceLines.Tariff:
				case OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices:
					result.Add(tariff);
					break;
				default:
					result.Add(invoiceLinePK);
					break;
			}

			return GetKeyForHeader(baseInvoiceLine) + result;
		}

		protected virtual ZString GetTariffAndDescriptionKey(BaseJobComInvoiceLine baseInvoiceLine) => baseInvoiceLine.JI_Description;

		protected virtual ZString GetMergeBy() => Declaration.JE_MergeBy;

		/// <summary>
		/// Returns an existing entry line linked through JI_CL or AdditionalLineLink pivot
		/// </summary>
		protected internal virtual CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.CusEntryLine;
		}

		/// <summary>
		/// If customs amendment is a total replacement, then all is amendable, this should be empty
		/// But AU customs nature/transport mode are not amendable
		/// </summary>
		protected internal virtual string GetNonAmendableLineDetails(BaseJobComInvoiceLine invoiceLine)
		{
			return string.Empty;
		}

		protected internal virtual bool IsEntryLineValidToBeReused(CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			return true;
		}

		protected internal virtual bool EntryLineNeedsToBeSplit(CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine) => false;

		protected internal virtual CusEntryLine GetEntryLineFromSplitIfItCanBeReused(BaseJobComInvoiceLine invoiceLine, List<CusEntryLine> splitEntryLines) => null;

		protected internal virtual bool IsEntryHeaderValidToBeReused(CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine)
		{
			return entry.CH_JE == Declaration.PK;
		}

		protected internal virtual bool CreateANewEntryIfMaximumEntryLineExceeded
		{
			get { return false; }
		}

		/// <summary>
		/// Customs usually limits the number of entry lines per entry to a certain number. If more entry lines are created, then it creates a new entry and attach those entry lines to the new entry.
		/// </summary>
		protected internal virtual int MaxLinesBeforeNewEntry
		{
			get { return int.MaxValue; }
		}

		/// <summary>
		/// US secondary entry lines do not count as a normal entry line. US limits entry lines to 999 and secondary entry lines should not be counted for that. 
		/// </summary>
		protected internal virtual bool ShouldIncrementNumberOfEntryLinesForMaximumEntryLines(BaseJobComInvoiceLine invoiceLine)
		{
			return true;
		}

		public bool CanCreateEntryLine(CusEntryHeader entryHeader, BaseJobComInvoiceLine invoiceLine) => CanCreateEntryLineCore(entryHeader, invoiceLine);
		protected virtual bool CanCreateEntryLineCore(CusEntryHeader entryHeader, BaseJobComInvoiceLine invoiceLine) => true;

		readonly Dictionary<BaseJobComInvoiceLine, MergeKey> mergeKeys;

		internal
		//// for mocking	
#if DEBUG
		virtual
#endif
		bool HasMergeKeyChangeSinceLastSaving(BaseJobComInvoiceLine invoiceLine, ReadOnlyBusinessObjectFactory cleanFactory)
		{
			bool result = false;
			if (invoiceLine.IsInDatabase)
			{
				MergeKey currentKey;
				if (!mergeKeys.TryGetValue(invoiceLine, out currentKey))
				{
					currentKey = GetKeyForLine(invoiceLine);
					mergeKeys.Add(invoiceLine, currentKey);
				}

				MergeKey dbMergeKey;
				if (!lineMergeKeysAtLastSave.TryGetValue(invoiceLine, out dbMergeKey))
				{
					var dbInvoiceLine = cleanFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
					if (dbInvoiceLine != null && dbInvoiceLine.Declaration != null)
					{
						dbMergeKey = GetKeyForLine(dbInvoiceLine);
					}

					lineMergeKeysAtLastSave[invoiceLine] = dbMergeKey;
				}

				result = dbMergeKey != currentKey;
			}

			return result;
		}

		protected virtual EntryManager GetEntryManager()
		{
			return new EntryManager(Declaration, this);
		}

		EntryManager EntryManager
		{
			get
			{
				if (entryManager == null)
				{
					entryManager = GetEntryManager();
				}
				return entryManager;
			}
		}
		EntryManager entryManager;
	}
}
