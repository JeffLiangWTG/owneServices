using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IGroupInvoiceOrInvoice
	{
		void Move(BaseJobComInvoiceGroupHeader origin, BaseJobComInvoiceGroupHeader destination);
		BaseJobComInvoiceGroupHeader ParentGroupInvoice { get; }
		BaseJobComInvoiceGroupHeader GroupInvoiceOfInvoiceOrGroupInvoiceItself { get; }
		ZString JZ_InvoiceNumber { get; set; }
		ZPropertyInfo JZ_InvoiceNumberInfo { get; }
		bool IsGroupInvoice { get; }
		IGroupInvoiceOrInvoice[] ChildGroupInvoices { get; }
		IGroupInvoiceOrInvoice[] ChildInvoices { get; }

		ZPropertyInfo JZ_JZ_GroupInvoiceFKInfo { get; }
	}
}
