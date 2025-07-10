using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceHeaderProcessTask : ProcessTask
	{
		public BaseJobComInvoiceHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(BaseJobComInvoiceHeader); }
		}

		public new BaseJobComInvoiceHeader Parent
		{
			get { return (BaseJobComInvoiceHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get
			{
				return Parent != null && Parent.IsAttachedToPersistentDeclaration ? ControllerIDs.Customs.JobDeclaration : ControllerIDs.CommercialInvoice;
			}
		}
	}
}
