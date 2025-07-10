using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HTransportEquipmentTest : TestCaseWithFactory
	{
		public void TestCharacteristicCode()
		{
			AssertEquals("40GP", node.CharacteristicCode);
		}

		public void TestID()
		{
			AssertEquals("1234567890", node.ID);
		}

		public void TestUsedCapacityCode()
		{
			AssertEquals("1", node.UsedCapacityCode);
		}

		public void TestSeals()
		{
			AssertContainsExactElementsInExactOrder(new string[] { "Seal1", "Seal2", "Seal3" }, node.Seals);
		}

		protected override void SetUp()
		{
			var bill = Factory.New<AsycudaBill>();

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40GP";

			var container = Factory.New<AsycudaContainer>();
			container.ACN_ContainerNumber = "1234567890";
			container.ACN_EmptyFullIndicator = "1";
			container.ACN_Seal1 = "Seal1";
			container.ACN_Seal2 = "Seal2";
			container.ACN_Seal3 = "Seal3";
			container.ACN_RC_ContainerType = refContainer.PK;

			bill.LinkContainer(container.PK);

			var billContainer = new AsycudaBillLinkAsycudaContainer(bill, container);

			node = new N5101HTransportEquipment(billContainer);
		}

		ITransportEquipment node;
	}
}
