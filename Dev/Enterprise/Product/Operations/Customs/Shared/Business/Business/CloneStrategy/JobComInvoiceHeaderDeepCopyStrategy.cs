using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Copies invoice, its invoice lines and charges
	///  
	/// </summary>
	public class JobComInvoiceHeaderDeepCopyStrategy : CustomsBusinessObjectCloneStrategy
	{
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType)
			: this(invoice, cloneType, null, invoice.Factory, null)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: this(invoice, cloneType, clonedDeclaration, clonedDeclaration.Factory, pkPairsDictionaryCollection)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceHeaderDeepCopyStrategy(BaseJobComInvoiceHeader invoice, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoice, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
			this.clonedDeclaration = clonedDeclaration;
			this.pkPairsDictionaryCollection = pkPairsDictionaryCollection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection;

		protected BaseJobComInvoiceHeader InvoiceToClone
		{
			get { return (BaseJobComInvoiceHeader)bizObjToClone; }
		}

		protected BaseJobDeclaration clonedDeclaration;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (BaseJobComInvoiceHeader)base.CloneInternal(args);

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				LinkInvoiceToDeclaration(result);
				CloneExtraInvoiceDataInSpecificCountry(args, result);

				if (IsTemplateCopy)
				{
					CopyJobDocAddresses(result);
				}

				DeepCopyInvoiceLines(result);

				if (InvoiceToClone.JobDeclaration != null && InvoiceToClone.JobDeclaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
				{
					foreach (InvoiceHeaderPackagePivot invoiceHeaderPackagePivot in InvoiceToClone.PackagesPivot)
					{
						var package = invoiceHeaderPackagePivot.Package;
						if (package != null)
						{
							var clonedPackage = result.JobDeclaration != null ? result.JobDeclaration.Packages.Cast<BasePackage>().FirstOrDefault(x => x.CW_HouseBill.EqualsIgnoringCase(package.CW_HouseBill)
																																				&& x.CW_PackQty == package.CW_PackQty
																																				&& x.CW_PackType.EqualsIgnoringCase(package.CW_PackType)
																																				) : null;
							if (clonedPackage != null)
							{
								var clonedInvoiceHeaderPackagePivot = result.PackagesPivot.AddPivotFor(clonedPackage) as InvoiceHeaderPackagePivot;
								clonedInvoiceHeaderPackagePivot.CHZ_JE = invoiceHeaderPackagePivot.CHZ_JE;
								clonedInvoiceHeaderPackagePivot.CHZ_NumberOfPacks = invoiceHeaderPackagePivot.CHZ_NumberOfPacks;
							}
						}
					}
				}

				//copy invoice's chargest
				new JobComInvChargeCloneHelper().CopyCharges(InvoiceToClone, result, cloneType);
			}

			return result;
		}

		protected virtual void LinkInvoiceToDeclaration(BaseJobComInvoiceHeader clonedInvoice)
		{
			if (clonedDeclaration != null)
			{
				clonedInvoice.JZ_JE = clonedDeclaration.PK;
			}
		}

		void CopyJobDocAddresses(BaseJobComInvoiceHeader clonedInvoice)
		{
			if ((InvoiceToClone as IDocAddresses).SupportedAddressTypes.Any())
			{
				if (pkPairsDictionaryCollection == null)
				{
					pkPairsDictionaryCollection = new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>();
				}
				var jobDocAddressPKPairs = FreightUtilities.GetValueOrDefault(
						pkPairsDictionaryCollection,
						JobDeclarationDeepCloneStrategy.JobDocAddressPKPairsKey);
				if (jobDocAddressPKPairs == null)
				{
					jobDocAddressPKPairs = new Dictionary<ZGuid, ZGuid>();
					pkPairsDictionaryCollection.Add(JobDeclarationDeepCloneStrategy.JobDocAddressPKPairsKey, jobDocAddressPKPairs);
				}
				foreach (var keyValuePair in JobDeclarationDeepCloneStrategy.CopyJobDocAddresses(InvoiceToClone.DocAddresses, clonedInvoice.PK, clonedInvoice.DocAddresses, cloneType, alternativeFactoryToInstantiateCloneIn))
				{
					jobDocAddressPKPairs.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		protected virtual void CloneExtraInvoiceDataInSpecificCountry(BusinessObjectCloneArgs args, BaseJobComInvoiceHeader clonedInvoice)
		{
		}

		void DeepCopyInvoiceLines(BaseJobComInvoiceHeader clonedInvoice)
		{
			Dictionary<ZGuid, ZGuid> parentIDs = new Dictionary<ZGuid, ZGuid>();
			Dictionary<ZGuid, List<BaseJobComInvoiceLine>> listOfParenntWithChildLines = new Dictionary<ZGuid, List<BaseJobComInvoiceLine>>();

			InvoiceToClone.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.Constants.JI_LineNo, ListSortDirection.Ascending);

			foreach (BaseJobComInvoiceLine invoiceLineToClone in InvoiceToClone.JobComInvoiceLines)
			{
				if (invoiceLineToClone.ShouldClone)
				{
					BaseJobComInvoiceLine invoiceLine = (BaseJobComInvoiceLine)GetInvoiceLineCloneStrategy(invoiceLineToClone, clonedInvoice).Clone();
					parentIDs.Add(invoiceLineToClone.PK, invoiceLine.PK);
					if (!invoiceLineToClone.JI_ParentID.IsEmpty)
					{
						List<BaseJobComInvoiceLine> list;
						if (!listOfParenntWithChildLines.TryGetValue(invoiceLineToClone.JI_ParentID, out list))
						{
							list = new List<BaseJobComInvoiceLine>();
							listOfParenntWithChildLines.Add(invoiceLineToClone.JI_ParentID, list);
						}
						list.Add(invoiceLine);
					}
					invoiceLine.ResetValuesAfterClone();
				}
			}

			foreach (KeyValuePair<ZGuid, List<BaseJobComInvoiceLine>> pair in listOfParenntWithChildLines)
			{
				if (parentIDs.ContainsKey(pair.Key))
				{
					ZGuid newParentID = parentIDs[pair.Key];
					foreach (BaseJobComInvoiceLine invoiceLine in pair.Value)
					{
						invoiceLine.JI_ParentID = newParentID;
					}
				}
			}
		}

		protected virtual JobComInvoiceLineDeepCloneStrategy GetInvoiceLineCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, BaseJobComInvoiceHeader clonedInvoice)
		{
			return new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection);
		}
	}
}
