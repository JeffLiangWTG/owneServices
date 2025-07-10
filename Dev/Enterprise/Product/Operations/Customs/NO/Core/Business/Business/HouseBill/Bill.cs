using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class Bill : Customs.Business.Bill, Integration.Customs.NO.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		[ChildEditable(true)]
		public new BillContainerCollection Containers => (BillContainerCollection)base.Containers;

		public new BillLookups Lookups => (BillLookups)base.Lookups;

		public new BillValidation Validation => (BillValidation)base.Validation;

		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new Bill GetBillOfType(ZString billType) => (Bill)base.GetBillOfType(billType);

		public new Bill ParentBill => (Bill)base.ParentBill;

		public new ChildBillCollection<Bill, JobDeclaration> ChildBills => (ChildBillCollection<Bill, JobDeclaration>)base.ChildBills;

		public new Bill Clone() => (Bill)base.Clone();

		protected override CusDecHouseBillLookups GetNewLookups() => new BillLookups(this);

		protected override CusDecHouseBillValidation GetNewValidation() => new BillValidation(this);

		protected override BaseBillContainerCollection GetContainerCollection() => new BillContainerCollection(this);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewHouseBillLevelInvoiceCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IChildBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewChildBills() => new ChildBillCollection<Bill, JobDeclaration>(this, Declaration);
	}
}
