using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public abstract class BaseIDtbMasterBookingEntityTest : TestCaseWithFactory
	{
		protected IDtbMasterBookingEntity MasterBookingEntity { get; set; }

		protected abstract IEnumerable<SchemaColumn> ReplicatedColumns { get; }
		protected abstract IEnumerable<SchemaColumn> NonReplicatedColumns { get; }
		protected abstract ITableSchema tableSchema { get; }

		public void TestAllDBFieldsAreAccountedForInReplicationTest()
		{
			var physicalFieldNames = Schema.CsvColumnList(tableSchema).Split(',').ToArray();
			var replicatedAndNonReplicatedFieldNames = ReplicatedColumns.Concat(NonReplicatedColumns).Select(c => c.Name).ToArray();

			AssertContainsExactElementsInAnyOrder("All physical fields should be accounted for in either ReplicationFieldInfos or NonReplicatedFields", replicatedAndNonReplicatedFieldNames, physicalFieldNames);
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		TransportBookingTestHelper helper;

		protected TransportBookingTestHelper GetNewTestHelper()
		{
			return new TransportBookingTestHelper(Factory);
		}
	}
}
