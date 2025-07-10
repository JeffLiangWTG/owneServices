using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public JobComInvoiceLineDeepCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice.Factory)
		{
			this.clonedInvoice = clonedInvoice;
			this.invoiceLineToClone = invoiceLineToClone;
			this.pkPairsDictionaryCollection = pkPairsDictionaryCollection;
		}

		protected readonly BaseJobComInvoiceHeader clonedInvoice;
		protected readonly BaseJobComInvoiceLine invoiceLineToClone;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected readonly Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseJobComInvoiceLine result = (BaseJobComInvoiceLine)base.CloneInternal(args);

			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.JI_JZ = clonedInvoice.PK;
				clonedInvoice.JobComInvoiceLines.Add(result);

				if (IsCountryToCountryCopy)
				{
					var pivot = result.Pivot;
					if (pivot != null && pivot.Classification != null)
					{
						result.JI_CC = pivot.Classification.PK;
					}
				}

				SetEntryInstructionForJobComInvoiceLine(result);

				ReloadPartSyncManagerPart(result);

				if (IsCountryToCountryCopy || IsDeepTemplateCopy)
				{
					CloneContainerPivots(result, invoiceLineToClone);

					if (invoiceLineToClone.Declaration != null && invoiceLineToClone.Declaration.SupportsChcPivotBetweenInvoiceLineAndPacking && result.Declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
					{
						foreach (InvoiceLinePackagePivot invoiceLinePackagePivot in invoiceLineToClone.PackagesPivot)
						{
							var package = invoiceLinePackagePivot.Package;
							if (package != null)
							{
								var clonedPackage = result.Declaration != null ? result.Declaration.Packages.Cast<BasePackage>().FirstOrDefault(x => x.CW_HouseBill.EqualsIgnoringCase(package.CW_HouseBill)
																																					&& x.CW_PackQty == package.CW_PackQty
																																					&& x.CW_PackType.EqualsIgnoringCase(package.CW_PackType)
																																					) : null;
								if (clonedPackage != null)
								{
									var clonedInvoiceLinePackagePivot = result.PackagesPivot.AddPivotFor(clonedPackage) as InvoiceLinePackagePivot;
									clonedInvoiceLinePackagePivot.CHC_NumberOfPacks = invoiceLinePackagePivot.CHC_NumberOfPacks;
								}
							}
						}
					}
				}
			}

			new JobComInvChargeCloneHelper().CopyCharges((BaseJobComInvoiceLine)bizObjToClone, result, cloneType);

			return result;
		}

		protected virtual void SetEntryInstructionForJobComInvoiceLine(BaseJobComInvoiceLine clonedInvoiceLine)
		{
			if (!invoiceLineToClone.JI_CEI.IsEmpty && pkPairsDictionaryCollection != null)
			{
				if (pkPairsDictionaryCollection.TryGetValue(JobDeclarationDeepCloneStrategy.CusEntryInstructionPKPairsKey, out var pkPairs) && pkPairs.TryGetValue(invoiceLineToClone.JI_CEI, out var ceiPK))
				{
					clonedInvoiceLine.JI_CEI = ceiPK;
				}
			}
		}

		void CloneContainerPivots(BaseJobComInvoiceLine resultInvoiceLine, BaseJobComInvoiceLine sourceInvoiceLine)
		{
			foreach (CusContainerInvoiceLinePivot invoiceLineContainerPivot in sourceInvoiceLine.ContainersPivot)
			{
				var container = invoiceLineContainerPivot.Container;
				if (container != null)
				{
					var clonedContainer = resultInvoiceLine.Declaration != null ? resultInvoiceLine.Declaration.CusContainers.Cast<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber.EqualsIgnoringCase(container.CO_ContainerNumber)) : null;
					if (clonedContainer != null)
					{
						var clonedInvoiceLineContainerPivot = resultInvoiceLine.ContainersPivot.AddPivotFor(clonedContainer);
						clonedInvoiceLineContainerPivot.C2_GrossWeight = invoiceLineContainerPivot.C2_GrossWeight;
						clonedInvoiceLineContainerPivot.C2_NetWeight = invoiceLineContainerPivot.C2_NetWeight;
						clonedInvoiceLineContainerPivot.C2_SplitValue = invoiceLineContainerPivot.C2_SplitValue;
						clonedInvoiceLineContainerPivot.C2_PackQty = invoiceLineContainerPivot.C2_PackQty;
					}
				}
			}
		}

		void ReloadPartSyncManagerPart(BaseJobComInvoiceLine clonedInvoiceLine)
		{
			if (clonedInvoiceLine.PartSyncManager != null)
			{
				clonedInvoiceLine.PartSyncManager.ReloadPart = true;
			}
		}
	}
}
