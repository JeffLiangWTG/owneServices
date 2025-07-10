using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(DocketsLabelOptionForm))]
	public class DocketsLabelOptionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var order = Factory.New<WhsOrder>();
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			collection.Add(order);
			var lines = new DeliveryLabelLineCollection(collection, Factory);
			var docketLabel = new WhsDocketsLabelControl(lines);
			return new DocketsLabelOptionForm(docketLabel);
		}
	}
}
