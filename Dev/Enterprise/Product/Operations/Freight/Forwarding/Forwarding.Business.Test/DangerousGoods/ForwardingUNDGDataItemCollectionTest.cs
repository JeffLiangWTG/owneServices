using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingUNDGDataItemCollection))]
	sealed class ForwardingUNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<ForwardingUNDGDataItemCollection>
	{
		#region Implementation

		protected override ForwardingUNDGDataItemCollection GetCollectionToTest()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var collection = new ForwardingUNDGDataItemCollection(packLine);
			collection.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First().PK;
			Factory.Save();

			return collection;
		}

		#endregion

		public void TestUNDGSubstances()
		{
			var expectedProperty = "Standard" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			{
				var collection = new ForwardingUNDGDataItemCollection(packLine);
				AssertEquals(true, collection.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
				AssertEquals(true, collection.AllUNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			{
				var collection = new ForwardingUNDGDataItemCollection(packLine);
				AssertEquals(true, collection.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
				AssertEquals(true, collection.AllUNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			{
				var collection = new ForwardingUNDGDataItemCollection(packLine);
				AssertEquals(true, collection.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
				AssertEquals(true, collection.AllUNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
			}

			{
				var collection = new ForwardingUNDGDataItemCollection(packLine);
				AssertEquals(true, collection.UNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
				AssertEquals(true, collection.AllUNDGSubstances.FilterBusinessObjectDefaults.ContainsDefaultFor(expectedProperty));
			}
		}

		public void TestItemDeletedWhenClassAndSubstanceAreBlanked_NoException_DI_DG()
		{
			var collection = CreatePacklineUNDGCollection();
			collection.FirstItemForBinding[0].DI_IMOClass = "";
			AssertNoExceptionThrown(() => collection.FirstItemForBinding[0].DI_DG = ZGuid.Empty);
		}

		public void TestItemDeletedWhenClassAndSubstanceAreBlanked_NoException_DI_IMOClass()
		{
			var collection = CreatePacklineUNDGCollection();
			collection.FirstItemForBinding[0].DI_DG = ZGuid.Empty;
			AssertNoExceptionThrown(() => collection.FirstItemForBinding[0].DI_IMOClass = "");
		}

		UNDGDataItemCollection CreatePacklineUNDGCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			var undgDataItem = Factory.NewWithValidTestData<ForwardingUNDGDataItem>();

			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.DI_IMOClass = "3";
			packLine.UNDGs.Add(undgDataItem);
			Factory.Save();

			return packLine.UNDGs;
		}
	}
}
