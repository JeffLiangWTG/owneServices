using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceHeaderProcessTask : ProcessTask
	{
		public AccDraftInvoiceHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID => null;

		protected internal override Type ParentType => typeof(AccDraftInvoiceHeader);

		#endregion
	}
}
