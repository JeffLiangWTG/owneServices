using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJobReference : AutoAccDraftInvoiceJobReference
	{
		public AccDraftInvoiceJobReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AIR_Type = "JOB";
		}
#endif
	}
}
