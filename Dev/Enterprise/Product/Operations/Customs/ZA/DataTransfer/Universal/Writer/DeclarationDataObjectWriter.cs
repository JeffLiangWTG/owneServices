using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : Customs.DataTransfer.Universal.DeclarationDataObjectWriter
	{
		internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoiceLinePK);
		}

		protected override IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetEntryInstructionRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryInstructionPK = row.GetValue(CusEntryInstructionSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryInstructionPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryLinePK);
		}

		protected override DataObjectList<CommercialInvoiceHeader> GetCommercialInvoiceCollection(BaseJobComInvoiceHeader[] invoiceBOs, Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter commercialInvoiceHeaderDataObjectWriter)
		{
			if (invoiceBOs != null && invoiceBOs.Length > 0)
			{
				var destinationCountryCode = ZString.Empty;
				var companyCountryCode = ZString.Empty;

				var randomHeader = invoiceBOs.FirstOrDefault();
				BusinessObjectFactory factory = null;
				var headerDeclaration = randomHeader?.JobDeclaration;
				if (headerDeclaration != null)
				{
					factory = headerDeclaration.Factory;
					destinationCountryCode = headerDeclaration.JE_RL_NKFinalDestination.Left(2);
					companyCountryCode = headerDeclaration.CountryCode;
				}

				foreach (var header in invoiceBOs)
				{
					var supplier = header.Supplier_Effective;
					var buyer = header.Importer_Effective;

					if (supplier != null && buyer != null)
					{
						var buyerCountry = RefCountry.OrganisationCountry(buyer);
						var buyerCountryCode = buyerCountry == null ? ZString.Empty : buyerCountry.RN_Code;

						if (!destinationCountryCode.IsEmpty)
						{
							AddOrgSupplierBuyerLinkFetchHint(factory, OrgSupplierBuyerLink.GetQueryForExistingOrgSupplierBuyerLink(supplier, buyer, destinationCountryCode));
						}
						if (!companyCountryCode.IsEmpty && companyCountryCode != destinationCountryCode)
						{
							AddOrgSupplierBuyerLinkFetchHint(factory, OrgSupplierBuyerLink.GetQueryForExistingOrgSupplierBuyerLink(supplier, buyer, companyCountryCode));
						}
						if (!buyerCountryCode.IsEmpty && buyerCountryCode != destinationCountryCode && buyerCountryCode != companyCountryCode)
						{
							AddOrgSupplierBuyerLinkFetchHint(factory, OrgSupplierBuyerLink.GetQueryForExistingOrgSupplierBuyerLink(supplier, buyer, buyerCountryCode));
						}
					}
				}
			}
			return base.GetCommercialInvoiceCollection(invoiceBOs, commercialInvoiceHeaderDataObjectWriter);
		}

		static void AddOrgSupplierBuyerLinkFetchHint(BusinessObjectFactory factory, ZQuery query)
		{
			if (factory != null && query != null && query != ZQuery.NoResultQuery)
			{
				factory.AddFetchHint(OrgSupplierBuyerLinkSchema.Instance, query);
			}
		}

		protected override void PopulateAdditionalInfoForAdditionalBill(Bill billBO, AdditionalBill additionalBill)
		{
			var zaBill = (Business.Bill)billBO;
			if (zaBill.CU_BillType == BillTypeList.Codes.MasterBill)
			{
				additionalBill.IssueDate = zaBill.Declaration?.JE_MasterBillIssuedDate;
			}
		}
	}
}
