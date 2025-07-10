using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsJobInvoicingSupporter<TBusinessObject> : JobInvoicingSupporter
		where TBusinessObject : BusinessObject, IJobInvoicingPlugIn
	{
		protected WhsJobInvoicingSupporter(TBusinessObject parent)
			: base(parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

		protected readonly TBusinessObject Parent;

		#region DeleteRelatedJobHeaders

		public void DeleteRelatedJobHeaders()
		{
			JobHeader.DeleteAllJobs(Parent);
		}

		#endregion

		#region ConsolType

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		#endregion
	}
}
