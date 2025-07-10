using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	internal class ViewQuotedBookingLogs : Logs
	{
		public ViewQuotedBookingLogs(IStmALogParent parent)
			: base(parent)
		{
		}

		public new ViewQuotedBooking Parent => (ViewQuotedBooking)base.Parent;

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			var additionalFilter = new ZQuery(StmALogSchema.SL_Table, ViewQuotedBookingSchema.Constants.TableName);
			return new StmALogDependentCollection(Parent, additionalFilter);
		}
	}
}
