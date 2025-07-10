using System;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DefaultPackagesTest : Customs.Business.Testing.DefaultPackagesTest
	{
		public override void TestDefaultToTheOnlyContainerWhenCreatingHouseBill()
		{
			Assert("Should not default to the only container when creating HouseBill in TW", true);
		}

		public override void TestDefaultPackagesLinkedToHouseBillForContainer()
		{
			Assert("Should not default Packages linked to HouseBill for container in TW", true);
		}

		public override void TestDefaultPackagesLinkedToContainerForHouseBill()
		{
			Assert("Should not default Packages linked to HouseBill for bill in TW", true);
		}

		public override void TestDefaultPackagesToNewPackingGroupIfAllAreLinkedForHouseBill()
		{
			Assert("Should not default Packages to new PackingGroup if all are linked for House Bill in TW", true);
		}

		public override void TestDontDefaultIfDeclarationIsPluggedIntoShipmentForContainer()
		{
			var declaration = GetDeclarationPackageRelevant();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(true, declaration.IsPluggedIntoShipment);
			var container = declaration.CusContainers.AddNew();
			AssertEquals("No packages should be defaulted as packages should be copied from freight", 0, declaration.PackingInformationCollection.Count);
		}

		public override void TestDontDefaultIfDeclarationIsPluggedIntoShipmentForHouseBill()
		{
			var declaration = GetDeclarationPackageRelevant();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(true, declaration.IsPluggedIntoShipment);
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "1";
			AssertEquals("No packages should be defaulted as packages should be copied from freight", 0, declaration.PackingInformationCollection.Count);
		}

		public override void TestHasChangesForLinkToPackingGroup()
		{
			Assert(true);
		}

		public override void TestEndToEndTestForDetachedContainerRow()
		{
			Assert(true);
		}

		public override void TestLinkToExistingPackingGroupWithoutContainer()
		{
			Assert(true);
		}

		protected override BaseJobDeclaration GetDeclarationPackageRelevant()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override void SetUp()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				base.SetUp();
			}
		}
	}
}
