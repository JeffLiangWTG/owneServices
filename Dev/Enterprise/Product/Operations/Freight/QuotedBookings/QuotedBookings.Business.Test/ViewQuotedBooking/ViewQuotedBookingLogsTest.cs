using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBookingLogs))]
	public class ViewQuotedBookingLogsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestRelationshipFilter

		public void TestRelationshipFilter()
		{
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			var logs = new ViewQuotedBookingLogsForTest(viewQuotedBooking);
			var relationshipQuery = logs.GetRelationshipFilter();

			var expectedQuery = new ZQuery(StmALogSchema.SL_Table, ViewQuotedBookingSchema.Constants.TableName);

			AssertCollectionContains(expectedQuery, relationshipQuery.GetAndParts());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ViewQuotedBookingLogs(Factory.New<ViewQuotedBooking>());
		}

		#endregion

		#region TestClass

		class ViewQuotedBookingLogsForTest : ViewQuotedBookingLogs
		{
			public ViewQuotedBookingLogsForTest(ViewQuotedBooking parent) : base(parent)
			{
			}

			public ZQuery GetRelationshipFilter()
			{
				return ElementsInternalFilter;
			}
		}

		#endregion
	}
}
