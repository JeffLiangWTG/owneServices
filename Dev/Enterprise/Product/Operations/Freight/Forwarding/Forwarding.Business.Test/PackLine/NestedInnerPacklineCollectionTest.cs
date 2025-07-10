using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(NestedInnerPackLineCollection))]
	sealed class NestedInnerPacklineCollectionTest : ActiveBusinessObjectCollectionTestCase<NestedInnerPackLineCollection>
	{
		public void TestParentVolumeBindingRefreshed_AddNew()
		{
			var collection = GetCollectionToTest();
			bool bindingRefreshed = false;
			parent.InnerPackTotalVolumeInfo.ValueChanged += (sender, e) => bindingRefreshed = true;

			var newPackLine = collection.AddNew();
			AssertEquals("Binding should have refreshed and triggered OnValueChanged", true, bindingRefreshed);
		}

		public void TestParentWeightBindingRefreshed_AddNew()
		{
			var collection = GetCollectionToTest();
			bool bindingRefreshed = false;
			parent.InnerPackTotalWeightInfo.ValueChanged += (sender, e) => bindingRefreshed = true;

			var newPackLine = collection.AddNew();
			AssertEquals("Binding should have refreshed and triggered OnValueChanged", true, bindingRefreshed);
		}

		public void TestParentPackTypeBindingRefreshed_AddNew()
		{
			var collection = GetCollectionToTest();
			bool bindingRefreshed = false;
			parent.InnerPackTypeInfo.ValueChanged += (sender, e) => bindingRefreshed = true;

			var newPackLine = collection.AddNew();
			AssertEquals("Binding should have refreshed and triggered OnValueChanged", true, bindingRefreshed);
		}

		public void TestParentPackCountBindingRefreshed_AddNew()
		{
			var collection = GetCollectionToTest();
			bool bindingRefreshed = false;
			parent.InnerPackCountInfo.ValueChanged += (sender, e) => bindingRefreshed = true;

			var newPackLine = collection.AddNew();
			AssertEquals("Binding should have refreshed and triggered OnValueChanged", true, bindingRefreshed);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_JS = shipment.PK;
			packline.JL_JL_OuterPackLine = parent.PK;
			return packline;
		}

		protected override NestedInnerPackLineCollection GetCollectionToTest()
		{
			return new NestedInnerPackLineCollection(parent);
		}

		protected override void SetUp()
		{
			base.SetUp();

			parent = Factory.New<ForwardingPackLine>();
			shipment = Factory.New<ForwardingShipment>();
			parent.JL_JS = shipment.PK;
			parent.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			parent.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
		}

		ForwardingPackLine parent;
		ForwardingShipment shipment;
	}
}
