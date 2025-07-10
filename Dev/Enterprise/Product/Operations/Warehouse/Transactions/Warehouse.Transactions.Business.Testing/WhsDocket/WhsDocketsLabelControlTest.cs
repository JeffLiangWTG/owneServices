using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketsLabelControl))]
	public class WhsDocketsLabelControlTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestLines()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var order2 = Factory.NewWithValidTestData<WhsOrder>();
			var pickableCollection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));

			var lines = new DeliveryLabelLineCollection(pickableCollection, Factory);
			var control = new WhsDocketsLabelControl(lines);
			AssertEquals("No lines", 0, control.Lines.Count);

			lines.Add(new WhsDocketLabelLine(order, 10, 10));
			lines.Add(new WhsDocketLabelLine(order2, 5, 7));
			AssertEquals("2 lines", 2, control.Lines.Count);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsDocketsLabelControl(Lines);
		}

		WhsOrder Order1
		{
			get
			{
				if (order1 == null)
				{
					order1 = Factory.NewWithValidTestData<WhsOrder>();
					order1.WD_ExternalReference = "Order1";
					order1.WD_PackagesSent = 2;
					order1.Lines.AddNew().WE_PackQuantity = 2;
					order1.Lines.AddNew().WE_PackQuantity = 1;
				}
				return order1;
			}
		}
		WhsOrder order1;

		WhsOrder Order2
		{
			get
			{
				if (order2 == null)
				{
					order2 = Factory.NewWithValidTestData<WhsOrder>();
					order2.WD_ExternalReference = "order2";
					order2.WD_PackagesSent = 4;
					order2.Lines.AddNew().WE_PackQuantity = 2;
					order2.Lines.AddNew().WE_PackQuantity = 1;
					order2.Lines.AddNew().WE_PackQuantity = 3;
				}
				return order2;
			}
		}
		WhsOrder order2;

		WhsPickableDocketCollection PickableCollection
		{
			get
			{
				if (pickableCollection == null)
				{
					pickableCollection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
					pickableCollection.Add(Order1);
					pickableCollection.Add(Order2);
				}
				return pickableCollection;
			}
		}
		WhsPickableDocketCollection pickableCollection;

		WhsDocketLabelLineCollection Lines
		{
			get { return lines ?? (lines = new DeliveryLabelLineCollection(PickableCollection, Factory)); }
		}
		WhsDocketLabelLineCollection lines;

		#endregion
	}
}
