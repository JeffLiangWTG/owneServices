using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BillOfLadingContainersSubGridSectionTest : TestCase
	{
		public void TestEstimatedFullDeliveryEditBoxBinding()
		{
			using (var section = new BillOfLadingContainersSubGridSection())
			{
				var collection = section.Controls.Find("estimatedFullDeliveryDateEdit", true);
				AssertEquals("estimatedFullDeliveryDateEdit should be found", 1, collection.Length);
				var bindingMember = section.BindingSource.GetBindingMember(collection[0]);
				AssertEquals("BindingMember", "FCLContainers.JC_ArrivalEstimatedDelivery", bindingMember);
			}
		}
	}
}
