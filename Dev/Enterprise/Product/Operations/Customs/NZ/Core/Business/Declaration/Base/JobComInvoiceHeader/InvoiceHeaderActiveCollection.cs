using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: base(declaration)
		{
			InvoiceDeleteEvent.AddInvoiceDeletedEventHandler(Factory, new EventHandler(RefreshWhenInvoiceDeleted));
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		public virtual new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		void RefreshWhenInvoiceDeleted(object sender, EventArgs e)
		{
			RefreshECI_InvoiceAmountIfECIWriteOff();
		}

		void RefreshECI_InvoiceAmountIfECIWriteOff()
		{
			if (Declaration != null && !Declaration.IsDeleted && Declaration.IsECIWriteoff)
			{
				Declaration.JE_ECI_InvoiceAmountInfo.RefreshBinding();//this also gets refreshed when JZ_JE changes
			}
		}

		protected override void OnAdded(BaseJobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);

			RefreshECI_InvoiceAmountIfECIWriteOff();
		}

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader invoice)
		{
			base.SetDefaultsForNewElementCore(invoice);

			invoice.JZ_OH_Buyer = Declaration.JE_OH_Importer;
		}
	}
}
