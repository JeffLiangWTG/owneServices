using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class MultiJobDeclarationCollection : BusinessObjectCollection<BaseJobComInvoiceLine>
	{
		public MultiJobDeclarationCollection(BaseJobDeclaration parentDeclaration, BusinessObjectFactory factoryToSave)
			: base(factoryToSave)
		{
			this.parentDeclaration = parentDeclaration;
			if (parentDeclaration.Factory == factoryToSave)
			{
				throw new ArgumentException("parentDeclaration Factory must be different to factoryToSave");
			}
			parentDeclaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
			parentDeclaration.JE_TransportModeInfo.ValueChanged += new EventHandler(JE_TransportModeInfo_ValueChanged);
		}
		readonly BaseJobDeclaration parentDeclaration;

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			foreach (BaseJobComInvoiceLine invoiceLine in this)
			{
				invoiceLine.Declaration.JE_TransportMode = parentDeclaration.JE_TransportMode;
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			foreach (BaseJobComInvoiceLine invoiceLine in this)
			{
				invoiceLine.Declaration.JE_MessageType = parentDeclaration.JE_MessageType;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			BaseJobDeclaration lastDeclaration = parentDeclaration;
			if (Count > 0)
			{
				lastDeclaration = this[Count - 1].Declaration;
			}
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			parentDeclaration.RegisterEditableChildObject(declaration);
			declaration.CopyPersistentValuesFrom(lastDeclaration);
			declaration.JE_MessageType_ReadOnly = true;

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.IsSingleLine = true;

			base.SetDefaultsForNewChild(child);
			BaseJobComInvoiceLine invoiceLine = (BaseJobComInvoiceLine)child;
			invoiceLine.JI_JZ = invoice.PK;
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			BaseJobDeclaration declaration = ((BaseJobComInvoiceLine)child).Declaration;
			base.RemoveCollectionRelationshipsCore(child, forDelete);
			if (declaration != null)
			{
				declaration.Delete();
			}
		}
	}
}
