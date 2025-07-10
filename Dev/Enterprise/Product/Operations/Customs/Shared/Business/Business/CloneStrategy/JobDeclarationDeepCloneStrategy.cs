using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType)
			: this(declarationToClone, cloneType, declarationToClone.JE_GB, declarationToClone.Factory)
		{
		}

		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: this(declarationToClone, cloneType, declarationToClone.JE_GB, alternativeFactoryToInstantiateCloneIn)
		{
		}

		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, ZGuid jE_GBForCloneResult)
			: this(declarationToClone, cloneType, jE_GBForCloneResult, declarationToClone.Factory)
		{
		}

		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, ZGuid jE_GBForCloneResult, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
			this.jE_GBForCloneResult = jE_GBForCloneResult;
		}

		readonly ZGuid jE_GBForCloneResult;

		protected BaseJobDeclaration DeclarationToClone
		{
			get { return (BaseJobDeclaration)base.bizObjToClone; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection;
		protected const string ContainerPKPairsKey = "containerPKPairs";
		protected const string BillPKPairsKey = "billPKPairs";
		protected const string GroupInvoicePKPairsKey = "groupInvoicePKPairs";
		public const string JobDocAddressPKPairsKey = "jobDocAddressPKPairs";
		public const string CusEntryInstructionPKPairsKey = "cusEntryInstructionPKPairs";

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseJobDeclaration clonedResult = (BaseJobDeclaration)base.CloneInternal(args);

			using (clonedResult.SuspendMarkApportionmentDirty())
			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				clonedResult.JE_GB = jE_GBForCloneResult;

				if (DeclarationToClone.IsDocsAndCartageSet)
				{
					clonedResult.DocsAndCartage.CopyPersistentValuesFrom(DeclarationToClone.DocsAndCartage);
				}

				bool isDetailedCopy = (IsCountryToCountryCopy || IsDeepTemplateCopy);
				Dictionary<ZGuid, ZGuid> containerPKPairs = isDetailedCopy ? CopyContainers(clonedResult) : new Dictionary<ZGuid, ZGuid>();

				Dictionary<ZGuid, ZGuid> billPKPairs = isDetailedCopy ? DeepCopyBills(clonedResult, containerPKPairs) : new Dictionary<ZGuid, ZGuid>();

				Dictionary<ZGuid, ZGuid> groupInvoicePKPairs = DeepCopyGroupInvoices(clonedResult);

				Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs = null;

				pkPairsDictionaryCollection = new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>();
				pkPairsDictionaryCollection.Add(ContainerPKPairsKey, containerPKPairs);
				pkPairsDictionaryCollection.Add(BillPKPairsKey, billPKPairs);
				pkPairsDictionaryCollection.Add(GroupInvoicePKPairsKey, groupInvoicePKPairs);

				if (IsTemplateCopy)
				{
					if (DeclarationToClone.Shipment == null)
					{
						DeepCopyNotesCore(clonedResult, (StmNoteCollection)DeclarationToClone.Notes.GetAllNotes());
						jobDocAddressPKPairs = CopyJobDocAddresses(clonedResult);
						pkPairsDictionaryCollection.Add(JobDocAddressPKPairsKey, jobDocAddressPKPairs);
						if (IsDeepTemplateCopy)
						{
							CopyTransports(clonedResult);
						}
					}
					else if (DeclarationToClone.JE_OverrideFreightDefaults)
					{
						jobDocAddressPKPairs = CopyJobDocAddresses(clonedResult);
						pkPairsDictionaryCollection.Add(JobDocAddressPKPairsKey, jobDocAddressPKPairs);
					}
					else
					{
						jobDocAddressPKPairs = CopyCountrySpecificJobDocAddresses(clonedResult);
						pkPairsDictionaryCollection.Add(JobDocAddressPKPairsKey, jobDocAddressPKPairs);
					}
				}
				else if (IsCountryToCountryCopy && DeclarationToClone.Shipment == null)
				{
					CopyTransports(clonedResult);
				}

				CopyEntryInstructions(clonedResult);
				DeepCopyDeclarationCore(clonedResult);
				DeepCopyCountrySpecificDataCore(clonedResult);
				DeepCopyCusInvPackCore(clonedResult);
				DeepCopyInvoices(clonedResult);
				DeepCopyCusPackingList(clonedResult);

				clonedResult.IsCloning = false;
				clonedResult.MarkApportionmentDirty();
			}

			return clonedResult;
		}

		protected virtual void DeepCopyNotesCore(BaseJobDeclaration clonedResult, StmNoteCollection notes)
		{
			foreach (StmNote note in notes)
			{
				clonedResult.Notes.Add(note.Clone());
			}
		}

		void CopyEntryInstructions(BaseJobDeclaration clonedResult)
		{
			if (!clonedResult.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				if (DeclarationToClone.CustomsEntryInstructions != null && DeclarationToClone.CustomsEntryInstructions.Count > 0)
				{
					clonedResult.CustomsEntryInstructions.RemoveAndDeleteAll();
					var cusEntryInstructionPKPairs = new Dictionary<ZGuid, ZGuid>();
					foreach (CusEntryInstruction entIns in DeclarationToClone.CustomsEntryInstructions)
					{
						var entryInstructionCloneStrategy = GetCusEntryInstructionDeepCloneStrategy(entIns);
						var newEntIns = (CusEntryInstruction)entryInstructionCloneStrategy.Clone();
						clonedResult.CustomsEntryInstructions.Add(newEntIns);
						cusEntryInstructionPKPairs.Add(entIns.PK, newEntIns.PK);
					}
					pkPairsDictionaryCollection.Add(CusEntryInstructionPKPairsKey, cusEntryInstructionPKPairs);
				}
			}
		}

		void DeepCopyCusPackingList(BaseJobDeclaration clonedResult)
		{
			if (DeclarationToClone.SupportsCusPackingList && clonedResult.SupportsCusPackingList)
			{
				var packingListToClone = DeclarationToClone.LoadCusPackingList(DeclarationToClone.Factory);
				if (packingListToClone != null)
				{
					var cusPackingListCloneStrategy = new CusPackingListDeepCloneStrategy(packingListToClone, cloneType, clonedResult, alternativeFactoryToInstantiateCloneIn);
					cusPackingListCloneStrategy.Clone();
				}
			}
		}

		protected virtual void DeepCopyDeclarationCore(BaseJobDeclaration clonedResult)
		{
		}

		protected virtual void DeepCopyCountrySpecificDataCore(BaseJobDeclaration clonedResult)
		{
		}

		protected virtual void DeepCopyCusInvPackCore(BaseJobDeclaration clonedResult)
		{
		}

		void CopyTransports(BaseJobDeclaration clonedResult)
		{
			System.Type parentType = clonedResult.GetType();
			foreach (Transport transport in DeclarationToClone.Transports)
			{
				Transport clonedtransport = (Transport)new CustomsBusinessObjectCloneStrategy(transport, cloneType, alternativeFactoryToInstantiateCloneIn).Clone();

				using (clonedtransport.GetValidationSuspender())
				using (clonedtransport.SuspendSettingHasChanges())
				{
					clonedtransport.ParentType = parentType;
					clonedtransport.JW_ParentGUID = clonedResult.PK;
					clonedResult.Transports.Add(clonedtransport);
				}
			}
		}

		Dictionary<ZGuid, ZGuid> CopyCountrySpecificJobDocAddresses(BaseJobDeclaration clonedResult)
		{
			return CopyJobDocAddresses(CountrySpecificJobDocAddresses, clonedResult.PK, clonedResult.DocAddresses, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		protected virtual IEnumerable<JobDocAddress> CountrySpecificJobDocAddresses => Enumerable.Empty<JobDocAddress>();

		Dictionary<ZGuid, ZGuid> CopyJobDocAddresses(BaseJobDeclaration clonedResult)
		{
			return CopyJobDocAddresses(DeclarationToClone.DocAddresses, clonedResult.PK, clonedResult.DocAddresses, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		public static Dictionary<ZGuid, ZGuid> CopyJobDocAddresses(JobDocAddressDependentCollection docAddressesToClone, ZGuid clonedParentPK, JobDocAddressDependentCollection clonedDocAddresses, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			return CopyJobDocAddresses(docAddressesToClone.Cast<JobDocAddress>(), clonedParentPK, clonedDocAddresses, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		static Dictionary<ZGuid, ZGuid> CopyJobDocAddresses(IEnumerable<JobDocAddress> docAddressesToClone, ZGuid clonedParentPK, JobDocAddressDependentCollection clonedDocAddresses, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			var result = new Dictionary<ZGuid, ZGuid>();
			clonedDocAddresses.RemoveAndDeleteAll();
			foreach (JobDocAddress address in docAddressesToClone)
			{
				JobDocAddress clonedAddress = (JobDocAddress)new CustomsBusinessObjectCloneStrategy(address, cloneType, alternativeFactoryToInstantiateCloneIn).Clone();

				using (clonedAddress.GetValidationSuspender())
				using (clonedAddress.SuspendSettingHasChanges())
				{
					clonedAddress.E2_ParentID = clonedParentPK;
					clonedDocAddresses.Add(clonedAddress);
				}
				clonedAddress.HasChanges = !clonedAddress.IsEmpty;
				result.Add(address.PK, clonedAddress.PK);
			}
			return result;
		}

		Dictionary<ZGuid, ZGuid> DeepCopyGroupInvoices(BaseJobDeclaration clonedResult)
		{
			Dictionary<ZGuid, ZGuid> result = new Dictionary<ZGuid, ZGuid>();

			if (DeclarationToClone.JobComInvoiceGroupHeaders.Count > 0 && DeclarationToClone.JobComInvoiceGroupHeaders[0] != null)
			{
				clonedResult.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
				clonedResult.JobComInvoiceGroupHeaders.HasChanges = false;

				var groupHeaderCloned = (BaseJobComInvoiceGroupHeader)new JobComInvoiceGroupHeaderDeepCloneStrategy(
					result,
					DeclarationToClone.JobComInvoiceGroupHeaders[0],
					cloneType,
					clonedResult,
					null).Clone();
				clonedResult.JobComInvoiceGroupHeaders.Add(groupHeaderCloned);
			}
			else if (clonedResult.JobComInvoiceGroupHeaders.Count > 0 && clonedResult.JobComInvoiceGroupHeaders[0] != null)
			{
				result.Add(clonedResult.JobComInvoiceGroupHeaders[0].PK, clonedResult.PK);
			}

			return result;
		}

		void DeepCopyInvoices(BaseJobDeclaration clonedResult)
		{
			foreach (BaseJobComInvoiceHeader invoiceToClone in DeclarationToClone.Invoices)
			{
				var invoiceCopyStrategy = GetInvoiceDeepCopyStrategy(invoiceToClone, clonedResult);
				var clonedInvoice = (BaseJobComInvoiceHeader)invoiceCopyStrategy.Clone();

				using (clonedInvoice.GetValidationSuspender())
				using (clonedInvoice.SuspendSettingHasChanges())
				{
					if (!invoiceToClone.JZ_CU_RelatedHouseBill.IsEmpty)
					{
						ZGuid relatedBillPK;
						if (FreightUtilities.GetValueOrDefault(pkPairsDictionaryCollection, BillPKPairsKey).TryGetValue(invoiceToClone.JZ_CU_RelatedHouseBill, out relatedBillPK))
						{
							clonedInvoice.JZ_CU_RelatedHouseBill = relatedBillPK;
						}
					}

					if (!invoiceToClone.JZ_JZ_GroupInvoiceFK.IsEmpty)
					{
						ZGuid groupInvoicePK;
						if (FreightUtilities.GetValueOrDefault(pkPairsDictionaryCollection, GroupInvoicePKPairsKey).TryGetValue(invoiceToClone.JZ_JZ_GroupInvoiceFK, out groupInvoicePK))
						{
							clonedInvoice.JZ_JZ_GroupInvoiceFK = groupInvoicePK;
						}
					}
				}
			}
		}

		protected virtual JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCopyStrategy(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection);
		}

		Dictionary<ZGuid, ZGuid> CopyContainers(BaseJobDeclaration clonedResult)
		{
			Dictionary<ZGuid, ZGuid> result = new Dictionary<ZGuid, ZGuid>();

			if (IsCountryToCountryCopy || IsDeepTemplateCopy || DeclarationToClone.Shipment == null)
			{
				foreach (BaseCusContainer container in DeclarationToClone.CusContainers)
				{
					BaseCusContainer clonedContainer = (BaseCusContainer)new CustomsBusinessObjectCloneStrategy(container, cloneType, alternativeFactoryToInstantiateCloneIn).Clone();

					result.Add(container.PK, clonedContainer.PK);

					using (clonedContainer.GetValidationSuspender())
					using (clonedContainer.SuspendSettingHasChanges())
					{
						clonedContainer.CO_JE = clonedResult.PK;
						clonedResult.CusContainers.Add(clonedContainer);
					}
				}
			}

			return result;
		}

		Dictionary<ZGuid, ZGuid> DeepCopyBills(BaseJobDeclaration clonedResult, Dictionary<ZGuid, ZGuid> containerPKPairs)
		{
			Dictionary<ZGuid, ZGuid> result = new Dictionary<ZGuid, ZGuid>();

			Dictionary<Bill, ZGuid> clonedChildBills = new Dictionary<Bill, ZGuid>();
			foreach (Bill bill in DeclarationToClone.Bills)
			{
				Bill clonedBill = (Bill)new BillDeepCloneStrategy(bill, containerPKPairs, cloneType, clonedResult).Clone();
				if (bill.CU_CU_ParentBill.IsValid)
				{
					clonedChildBills.Add(clonedBill, bill.CU_CU_ParentBill);
				}
				result.Add(bill.PK, clonedBill.PK);
			}

			foreach (KeyValuePair<Bill, ZGuid> pair in clonedChildBills)
			{
				pair.Key.CU_CU_ParentBill = result[pair.Value];
			}

			return result;
		}

		protected virtual CusEntryInstructionDeepCloneStrategy GetCusEntryInstructionDeepCloneStrategy(CusEntryInstruction cusEntryInstructionToClone)
		{
			return new CusEntryInstructionDeepCloneStrategy(cusEntryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}
	}
}
