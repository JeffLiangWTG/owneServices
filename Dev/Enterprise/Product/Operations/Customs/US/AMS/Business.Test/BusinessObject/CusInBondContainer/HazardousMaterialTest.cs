using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class HazardousMaterialTest : TestCaseWithFactory
	{
		public void TestIHazardousMaterials()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();

			var undg1 = container.UNDGs.AddNew();
			undg1.DI_DGFlashPoint = 23m;

			ICommonContainer icontainer = container;
			var undg = icontainer.HazardousMaterials.ToList<IHazardousMaterial>()[0];
			AssertEquals(HazMatQualifierList.Codes.UnitedNations, undg.HazMatQualifier);
		}
	}
}
