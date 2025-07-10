using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	[DefaultField("LineNo")]
	public class CommercialInvoiceLineWrapper : DocumentWrapper
	{
		CommercialInvoiceLineWrapper(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(invoiceLine, factory)
		{
			invoiceLineBO = invoiceLine ?? factory.GetNull<JobComInvoiceLine>();
			invoiceHeaderBO = invoiceLineBO.InvoiceHeader;
			tWInvoiceLineBO = invoiceLineBO.AddInfoChild;
		}

		public static CommercialInvoiceLineWrapper New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			return new CommercialInvoiceLineWrapper(jobComInvoiceLine, factoryToWrap);
		}

		readonly JobComInvoiceHeader invoiceHeaderBO;

		readonly JobComInvoiceLine invoiceLineBO;

		readonly JobTWComInvoiceLine tWInvoiceLineBO;

		public ZShort LineNo => invoiceLineBO.JI_LineNo;

		public ZString GoodsDescription => invoiceLineBO.JI_DeclarationGoodsDescription;

		public ZString TwGroup => invoiceLineBO.JI_Group.Trim();

		public ZBool ShowFOC => invoiceLineBO.JI_Procedure == Constants.ProcedureCodes._04 || invoiceLineBO.JI_Procedure == Constants.ProcedureCodes._94;

		public ZDecimal UnitPrice => tWInvoiceLineBO.TWL_DocumentaryUnitPrice;

		public ZDecimal LinePrice => invoiceLineBO.JI_LinePrice;

		public ValueAndUnitWrapper LineQuantity
		{
			get
			{
				var decimalPlaces = invoiceHeaderBO?.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name) ?? 2;
				return new ValueAndUnitWrapper(tWInvoiceLineBO.TWL_DocumentaryQty, tWInvoiceLineBO.TWL_DocumentaryUQ, decimalPlaces, invoiceLineBO.Lookups.InvoiceUQList, Factory);
			}
		}
	}
}
