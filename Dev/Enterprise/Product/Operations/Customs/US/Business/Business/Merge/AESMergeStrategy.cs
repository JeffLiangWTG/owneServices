using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AESMergeStrategy : Customs.Business.EntryCreationStrategy
	{
		public AESMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.Export)
		{
		}

		protected override bool IsActiveCore
		{
			get { return Declaration.IsExport; }
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.GetKeyForLine(baseInvoiceLine);
			var line = baseInvoiceLine as JobComInvoiceLine;
			if (line != null)
			{
				var additionalDetailsKey = ShouldNotMergeThisLineWithOtherLines(line) ? line.PK : ZGuid.Empty;

				result.Add(line.US_ExportCode);
				result.Add(line.US_LicenseType);
				result.Add(line.US_LicenseNo);
				result.Add(line.US_AESOriginIndicator);
				result.Add(line.US_ECCN);
				result.Add(line.US_MarksAndNumbers);
				result.Add(line.US_DDTCITARExemptionNo);
				result.Add(line.US_DDTCMilitaryEquipmentIndicator);
				result.Add(line.US_DDTCPartyCertificationIndicator);
				result.Add(line.US_DDTCRegistrationNo);
				result.Add(line.US_DDTCUnit);
				result.Add(line.US_DDTCUSMLCategoryCode);
				result.Add(line.US_JurisdictionNumber);
				result.Add(line.US_IsUsedVehicle ? line.PK : ZGuid.Empty);
				result.Add(additionalDetailsKey);
				result.Add(line.US_AMSInd);
				result.Add(line.US_PSTIndicator);
				result.Add(line.US_NMFSHMSInd);
				result.Add(line.US_ATFInd);
				result.Add(line.US_DEAInd);
				result.Add(line.US_FWSInd);
			}
			return result;
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var result = new Customs.Business.MergeKey(12);
			var invoice = invoiceLine.InvoiceHeader as JobComInvoiceHeader;
			if (invoice != null)
			{
				result.Add(invoice.SupplierPickupAddress.E2_OA_Address);
				result.Add(invoice.SupplierPickupAddress.E2_Address1);
				result.Add(invoice.SupplierPickupAddress.E2_Address2);
				result.Add(invoice.SupplierPickupAddress.E2_City);
				result.Add(invoice.SupplierPickupAddress.E2_State);
				result.Add(invoice.SupplierPickupAddress.E2_Postcode);
				result.Add(invoice.SupplierPickupAddress.E2_RN_NKCountryCode);
				result.Add(invoice.USPPIDocAddress.E2_OA_Address);
				result.Add(invoice.USPPIDocAddress.E2_Address1);
				result.Add(invoice.USPPIDocAddress.E2_Address2);
				result.Add(invoice.USPPIDocAddress.E2_City);
				result.Add(invoice.USPPIDocAddress.E2_State);
				result.Add(invoice.USPPIDocAddress.E2_Postcode);
				result.Add(invoice.USPPIDocAddress.E2_RN_NKCountryCode);
				result.Add(invoice.USPPIDocAddress.E2_CompanyName);
				result.Add(invoice.JZ_OH_Consignee);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_OA_Address);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_Address1);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_Address2);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_City);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_State);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_Postcode);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_RN_NKCountryCode);
				result.Add(invoice.UltimateConsigneeDocAddress.E2_CompanyName);
				result.Add(invoice.US_TransactionsRelated);
				result.Add(invoice.US_HazardousCargo);
				result.Add(invoice.US_RoutedTransaction);
				result.Add(invoice.US_ImportEntryNo);
				result.Add(invoice.US_InbondType);
				result.Add(((JobComInvoiceLine)invoiceLine).US_DateOfExport.Date);
				result.Add(invoice.US_StateOfOrigin);
				result.Add(invoice.US_ForeignTradeZone);
				result.Add(invoice.US_UltimateConsigneeType);
				result.Add(invoice.US_UltimateDestinationCountry);
			}
			return result;
		}

		protected override System.Collections.Generic.IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
			{
				if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.Export)
				{
					yield return entry;
				}
			}
		}

		ZBool ShouldNotMergeThisLineWithOtherLines(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsAMSDeclared
				|| invoiceLine.IsExportEPADeclared
				|| invoiceLine.IsATFDeclared
				|| invoiceLine.IsFWSDeclared
				|| invoiceLine.HasDEAHeaders
				|| invoiceLine.HasNMFSLines
				|| invoiceLine.HasTTBLines;
		}
	}
}
