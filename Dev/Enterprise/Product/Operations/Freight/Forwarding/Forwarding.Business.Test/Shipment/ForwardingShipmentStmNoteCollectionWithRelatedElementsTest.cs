using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentStmNoteCollectionWithRelatedElements))]
	sealed class ForwardingShipmentStmNoteCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions()]
		public override void TestLoad()
		{
			try
			{
				var top1Filter = new ZQuery(StmNoteSchema.ST_Table, Shipment.TableName);
				top1Filter.MaximumRows = 1;
				Collection.Load(top1Filter);
			}
			catch (NotSupportedException) // Load not supported for this collection
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingShipmentStmNoteCollectionWithRelatedElements(Shipment);
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
