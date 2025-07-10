using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineViewCollection : InvoiceLineViewCollection<JobComInvoiceLine>, IImportWizardProvider
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = false;
			if (Declaration.IsRecon)
			{
				JobComInvoiceLine invoiceLine = (JobComInvoiceLine)element;

				result = Declaration.ReconDeclaration.SelectedOriginalEntry.IsEmpty ||
				invoiceLine.InvoiceHeader != null && invoiceLine.InvoiceHeader.US_CH_ReconEntry == Declaration.ReconDeclaration.SelectedOriginalEntry;
			}
			else
			{
				result = base.IsThisPartOfTheCollection(element);
			}

			return result;
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			if (Declaration.IsPGAEntryHasBeenLodgedAtCustoms)
			{
				var invoiceLine = (JobComInvoiceLine)bizOAdded;
				if (invoiceLine.HasPGAIndicators)
				{
					Declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			JobComInvoiceLine previousLine = Count > 0 ? this[Count - 1] : null;
			JobComInvoiceLine newLine = (JobComInvoiceLine)child;
			JobComInvoiceHeader invoiceHeader = null;
			if (previousLine != null)
			{
				if (newLine.InvoiceHeader == previousLine.InvoiceHeader)
				{
					if (previousLine.IsSetXLine)
					{
						newLine.JI_ParentID = previousLine.PK;
					}
					else if (previousLine.IsSetVLine)
					{
						var hasProductSet = previousLine.JI_PartNoInfo.ReadOnly && !previousLine.JI_PartNo.IsEmpty;
						if (!hasProductSet)
						{
							newLine.JI_ParentID = previousLine.JI_ParentID;
						}
					}
				}
				else
				{
					invoiceHeader = previousLine.InvoiceHeader;
				}

				if (CopyLastLineDetailsToNewLines)
				{
					CopyCollectionsFromPreviousLineToNewLine(previousLine, newLine);
				}
			}
			if (Declaration.IsRecon && newLine.InvoiceHeader == null)
			{
				if (invoiceHeader == null)
				{
					ReconOriginalEntryHeader entryHeader = Declaration.ReconDeclaration.OriginalEntries.Count > 0 ? Declaration.ReconDeclaration.OriginalEntries[0] : Declaration.ReconDeclaration.OriginalEntries.AddNew();
					invoiceHeader = entryHeader.Invoice;
				}
				newLine.JI_JZ = invoiceHeader.PK;
			}
			if (Declaration.IsDrawback)
			{
				newLine.US_DRWExpNoticeInd = Declaration.US_DRWExamWitness;
			}

			newLine.SetDefaultAccountingMethod();
		}

		protected override void CopyLastLineDetailsToNewLinesIfEnabled(JobComInvoiceLine newLine, JobComInvoiceLine previousLine)
		{
			if (previousLine?.HasChanges ?? false)
			{
				previousLine.GetAddInfo().UpdateRelatedPropertyInfo();
			}
			base.CopyLastLineDetailsToNewLinesIfEnabled(newLine, previousLine);
			var hasProductSet = previousLine.JI_PartNoInfo.ReadOnly && !previousLine.JI_PartNo.IsEmpty;
			if (hasProductSet && !newLine.JI_ParentID.IsEmpty)
			{
				newLine.JI_ParentID = CargoWise.Types.ZGuid.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CopyCollectionsFromPreviousLineToNewLine(JobComInvoiceLine previousLine, JobComInvoiceLine newLine)
		{
			foreach (CusContainerInvoiceLinePivot pivot in previousLine.ContainersPivot)
			{
				if (!newLine.ContainersPivot.Contains(pivot.Container))
				{
					newLine.ContainersPivot.AddPivotFor(pivot.Container);
				}
			}

			foreach (InvoiceLineCharge charge in previousLine.Charges)
			{
				newLine.Charges.Add(charge.Clone());
			}

			foreach (FeeCusCodeData fee in previousLine.FeeCusCodes)
			{
				newLine.FeeCusCodes.Add(fee.Clone());
			}

			foreach (DOT dot in previousLine.DOTs)
			{
				newLine.DOTs.Add((DOT)dot.Clone());
			}

			foreach (FCC fcc in previousLine.FCCs)
			{
				newLine.FCCs.Add((FCC)fcc.Clone());
			}

			foreach (FDA fda in previousLine.FDAs)
			{
				var fdaCloned = (FDA)fda.Clone();
				newLine.FDAs.Add(fdaCloned);

				CloneContainerPivotsForFDA(newLine, fda, fdaCloned);
			}

			foreach (AMS ams in previousLine.AMSLines)
			{
				newLine.AMSLines.Add((AMS)ams.Clone());
			}

			foreach (PGA pga in previousLine.LaceyActLines)
			{
				newLine.LaceyActLines.Add((PGA)pga.Clone());
			}

			foreach (Pesticide pst in previousLine.PSTLines)
			{
				newLine.PSTLines.Add((Pesticide)pst.Clone());
			}

			foreach (NHTSAHeader nhtsa in previousLine.NHTSALines)
			{
				newLine.NHTSALines.Add((NHTSAHeader)nhtsa.Clone());
			}

			foreach (CPSCHeader cpsc in previousLine.CPSCHeaders)
			{
				newLine.CPSCHeaders.Add((CPSCHeader)cpsc.Clone());
			}

			foreach (DEAHeader dEA in previousLine.DEAHeaders)
			{
				newLine.DEAHeaders.Add((DEAHeader)dEA.Clone());
			}
		}

		void CloneContainerPivotsForFDA(JobComInvoiceLine newLine, FDA previousFDA, FDA newFDA)
		{
			foreach (FDARelatedContainer container in previousFDA.ContainersForInvoiceLine)
			{
				if (container.IsForFDALine)
				{
					var cusContainer = container.ContainerInvoiceLinePivot != null ? container.ContainerInvoiceLinePivot.Container : null;
					if (cusContainer != null)
					{
						var invoiceLineContainerPivot = newLine.ContainersPivot.GetRelatedPivot(cusContainer);
						if (invoiceLineContainerPivot != null)
						{
							newFDA.ContainersForFDALine.AddPivotFor(invoiceLineContainerPivot);
						}
					}
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsRebuilding && !IsLoading)
			{
				var parentLine = ((JobComInvoiceLine)bizOAdded).ParentTariffLine;
				if (parentLine != null)
				{
					parentLine.RefreshChildLines();
				}

				var productParentTariffLine = ((JobComInvoiceLine)bizOAdded).ProductParentTariffLine;
				if (productParentTariffLine != null)
				{
					productParentTariffLine.RefreshProductRelatedLines();
				}
			}
		}

		ImportWizard IImportWizardProvider.GetImportWizard(IImportCollectionInfo collectionInfo,
			ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			return Declaration.IsFormalImport
				? new InvoiceLineImportWizard(new ImportInvoiceLineImportCollectionInfo(collectionInfo), settingsStorage, fileMapper)
				: (Declaration.IsDrawback
					? new DrawbackInvoiceLineImportWizard(collectionInfo, settingsStorage, fileMapper)
					: new ImportWizard(collectionInfo, settingsStorage, fileMapper));
		}
	}
}
