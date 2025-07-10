using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReleaseEntryInvoiceRetriever
	{
		public ReleaseEntryInvoiceRetriever(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public int ImportDeclarations(JobDeclarationCollection importDeclarations)
		{
			var declarationCount = 0;
			foreach (JobDeclaration declarationToImport in importDeclarations)
			{
				var invoiceCountImported = ImportInvoice(declarationToImport, null);
				if (invoiceCountImported > 0)
				{
					declarationCount++;
				}
			}
			return declarationCount;
		}

		public int ImportInvoice(JobDeclaration declarationToImport, JobComInvoiceHeader invoiceToImportFrom)
		{
			var countImported = 0;
			var entryFilerAndEntryNumber = declarationToImport.EntryFilerCode + declarationToImport.ImportEntryNumber;
			var filteredInvoices = declaration.FilteredInvoices;
			if (invoiceToImportFrom != null && filteredInvoices.IsNonCommittedElementExposed(invoiceToImportFrom) && !declaration.Invoices.Contains(invoiceToImportFrom))
			{
				((ICancelAddNew)filteredInvoices).EndNew(((IList)filteredInvoices).IndexOf(invoiceToImportFrom));
			}
			ImportGroupInvoiceHeaderCharge(declarationToImport);

			var invoices = declarationToImport.Invoices.ToArray();
			foreach (JobComInvoiceHeader header in invoices)
			{
				if (!CheckIfInvoiceIsAlreadyRetrieved(header.JZ_InvoiceNumber, entryFilerAndEntryNumber))
				{
					var clonedHeader = (JobComInvoiceHeader)header.Clone(new BusinessObjectCloneArgs(declaration.Factory, new string[] { JobComInvoiceHeader.Schema.JZ_JE }, typeof(JobComInvoiceHeader), true));
					declaration.Invoices.Add(clonedHeader);
					declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Add(clonedHeader);
					clonedHeader.GetAddInfo().LoadPropertiesFromString(clonedHeader.JZ_AddInfo);
					SetAdditionalColumnInfoForInvoiceHeader(header, clonedHeader);
					SetAdditionalAddInfoForInvoiceHeader(header, clonedHeader, entryFilerAndEntryNumber);
					ImportInvoiceHeaderCharge(header, clonedHeader);
					ImportInvoiceLines(header, clonedHeader.PK);
					countImported++;
				}
			}
			return countImported;
		}

		void SetAdditionalColumnInfoForInvoiceHeader(JobComInvoiceHeader headerToCopy, JobComInvoiceHeader clonedHeader)
		{
			clonedHeader.JZ_OH_Buyer = headerToCopy.JZ_OH_Buyer;
			clonedHeader.JZ_OH_Supplier = headerToCopy.JZ_OH_Supplier;
			clonedHeader.JZ_OA_SupplierAddress = headerToCopy.JZ_OA_SupplierAddress;
			clonedHeader.JZ_OA_ManufacturerAddress = headerToCopy.JZ_OA_ManufacturerAddress;
			clonedHeader.JZ_OA_ConsigneeAddress = headerToCopy.JZ_OA_ConsigneeAddress;
			clonedHeader.JZ_OA_SoldToPartyAddress = headerToCopy.JZ_OA_SoldToPartyAddress;
			clonedHeader.JZ_Weight = headerToCopy.JZ_Weight;
			clonedHeader.JZ_IncoTerm = headerToCopy.JZ_IncoTerm;
		}

		void SetAdditionalAddInfoForInvoiceHeader(JobComInvoiceHeader headerToCopy, JobComInvoiceHeader clonedHeader, ZString entryFilerAndEntryNumber)
		{
			clonedHeader.IsRetrievingInvoiceFromReleaseEntry = false;
			clonedHeader.US_ReleaseEntryNumber = entryFilerAndEntryNumber;
			clonedHeader.IsRetrievingInvoiceFromReleaseEntry = true;
			clonedHeader.US_DateOfExport = headerToCopy.US_DateOfExport;
			clonedHeader.US_UC_NKCountryOfExport = headerToCopy.US_UC_NKCountryOfExport;
			clonedHeader.US_TransactionsRelated = headerToCopy.US_TransactionsRelated;
			clonedHeader.US_FirstSale = headerToCopy.US_FirstSale;
		}

		void ImportGroupInvoiceHeaderCharge(JobDeclaration declarationToImport)
		{
			var charges = declarationToImport.JobComInvoiceGroupHeaders[0].Charges.ToArray();
			foreach (GroupInvoiceCharge charge in charges)
			{
				var clonedGroupCharge = (GroupInvoiceCharge)charge.Clone(new BusinessObjectCloneArgs(declaration.Factory, new string[] { GroupInvoiceCharge.Schema.J7_ParentID }, typeof(GroupInvoiceCharge), true));
				declaration.JobComInvoiceGroupHeaders[0].Charges.Add(clonedGroupCharge);
			}
		}

		void ImportInvoiceHeaderCharge(JobComInvoiceHeader headerToImport, JobComInvoiceHeader clonedHeader)
		{
			var charges = headerToImport.Charges.ToArray();
			foreach (InvoiceCharge charge in charges)
			{
				var clonedCharge = (InvoiceCharge)charge.Clone(new BusinessObjectCloneArgs(declaration.Factory, new string[] { InvoiceCharge.Schema.J7_ParentID }, typeof(InvoiceCharge), true));
				clonedHeader.Charges.Add(clonedCharge);
			}
		}

		public void ImportInvoiceLines(JobComInvoiceHeader headerToImport, ZGuid headerPK)
		{
			var invoiceLines = headerToImport.InvoiceLines.ToArray();
			var originalLinePKToClonedLinePK = new Dictionary<ZGuid, ZGuid>();
			var clonedLineToOriginalLine = new Dictionary<JobComInvoiceLine, JobComInvoiceLine>();
			foreach (JobComInvoiceLine line in invoiceLines)
			{
				var clonedLine = line.Clone(new BusinessObjectCloneArgs(declaration.Factory, new string[] { JobComInvoiceLine.Schema.JI_JZ }, typeof(JobComInvoiceLine), true));
				clonedLine.JI_JZ = headerPK;
				clonedLine.JI_RH_NKCommodity_Code = line.JI_RH_NKCommodity_Code;
				clonedLine.GetAddInfo().LoadPropertiesFromString(clonedLine.JI_AddInfo);
				clonedLine.US_UC_NKCountryOfOrigin = line.US_UC_NKCountryOfOrigin;
				declaration.InvoiceLines.Add(clonedLine);
				clonedLine.US_DestinationState = line.US_DestinationState;
				ImportInvoiceLineCharge(line, clonedLine);
				ImportInvoiceAdditionalTariffs(line, clonedLine);

				originalLinePKToClonedLinePK.Add(line.PK, clonedLine.PK);
				clonedLineToOriginalLine.Add(clonedLine, line);
			}

			foreach (var clonedLine in clonedLineToOriginalLine.Keys)
			{
				var originalLineParentID = clonedLineToOriginalLine[clonedLine].JI_ParentID;
				if (originalLineParentID.IsValid)
				{
					clonedLine.JI_ParentID = originalLinePKToClonedLinePK[originalLineParentID];
				}
			}
		}

		void ImportInvoiceLineCharge(JobComInvoiceLine lineToImport, JobComInvoiceLine clonedLine)
		{
			var charges = lineToImport.Charges.ToArray();
			foreach (InvoiceLineCharge charge in charges)
			{
				var clonedLineCharge = (InvoiceLineCharge)charge.Clone(new BusinessObjectCloneArgs(declaration.Factory, new string[] { InvoiceLineCharge.Schema.J7_ParentID }, typeof(InvoiceLineCharge), true));
				clonedLine.Charges.Add(clonedLineCharge);
			}
		}

		void ImportInvoiceAdditionalTariffs(JobComInvoiceLine lineToImport, JobComInvoiceLine clonedLine)
		{
			if (lineToImport.IsImport)
			{
				clonedLine.SupFormattedAdditionalTariff1 = lineToImport.SupFormattedAdditionalTariff1;
				clonedLine.SupFormattedAdditionalTariff2 = lineToImport.SupFormattedAdditionalTariff2;
				clonedLine.SupFormattedAdditionalTariff3 = lineToImport.SupFormattedAdditionalTariff3;
				clonedLine.SupFormattedAdditionalTariff4 = lineToImport.SupFormattedAdditionalTariff4;
				clonedLine.SupFormattedAdditionalTariff5 = lineToImport.SupFormattedAdditionalTariff5;
			}
		}

		bool CheckIfInvoiceIsAlreadyRetrieved(ZString invoiceNumber, ZString entryFilerAndEntryNumber)
		{
			var duplicatedInvoice = declaration.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault(x => x.US_ReleaseEntryNumber == entryFilerAndEntryNumber && x.JZ_InvoiceNumber == invoiceNumber);
			return (duplicatedInvoice != null);
		}
	}
}
