using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class WhsDataTestHelper : Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, OrgSupplierPart, CusClassification, CusClassPartPivot>
	{
		public WhsDataTestHelper()
		{
		}

		public WhsDataTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobDeclaration GetNewDeclaration(ZString entryType, ZString declarationReference, ZString entryFilerCode, ZString entryNumber, ZDecimal quantity)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = WhsWarehouse.WW_OA_WarehouseAddress;

			declaration.US_EnableENS = true;
			declaration.US_EntryType = entryType;
			declaration.Invoices.DeleteAll();
			if (declaration.IsExWarehouseEntryType)
			{
				declaration.US_WHSEntryFilerCode = entryFilerCode;
				declaration.US_WHSEntryNumber = entryNumber;
			}
			else if (!declaration.IsENSFormalImportAndConsumptionFTZ)
			{
				declaration.US_EntryFilerCode = entryFilerCode;
				declaration.ImportEntryNumber = entryNumber;
			}
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AddInvoiceLine(invoice, Part, quantity, entryFilerCode, entryNumber, 1);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, OrgSupplierPart part, ZDecimal quantity, ZString entryFilerCode, ZString entryNumber, ZShort whsEntryLineNo)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			var declaration = invoice.JobDeclaration;
			if (declaration.IsExWarehouseEntryType)
			{
				invoiceLine.US_WHSEntryLineNo = whsEntryLineNo;
			}
			else if (declaration.IsENSFormalImportAndConsumptionFTZ)
			{
				invoiceLine.US_WHSEntryNumber = entryFilerCode + "-" + entryNumber;
				invoiceLine.US_WHSEntryLineNo = whsEntryLineNo;
			}
			else
			{
				var whsPack = declaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 10;
				var whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = invoiceLine.PK;
			}
			return invoiceLine;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override ZString PivotChildType
		{
			get { return ClassificationTypeList.Codes.HTI; }
		}
	}
}
