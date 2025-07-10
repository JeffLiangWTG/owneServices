using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondContainer))]
	sealed class CusInBondContainerTest : Customs.Business.Testing.CusInBondContainerTest<CusInBondContainer>
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(
				Factory.New<CusInBondContainer>(),
				"TWCusInBondContainer");
		}

		public void TestIsPartReadOnly()
		{
			var cusInBondContainer = Factory.New<CusInBondContainer>();
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.EmptyContainer;
			Assert("IsPartReadOnly is True", cusInBondContainer.IsPartReadOnly);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.LCL;
			Assert("IsPartReadOnly is True", cusInBondContainer.IsPartReadOnly);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.FCL;
			Assert("IsPartReadOnly is False", !cusInBondContainer.IsPartReadOnly);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.GRP;
			Assert("IsPartReadOnly is True", cusInBondContainer.IsPartReadOnly);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			Assert("IsPartReadOnly is False", !cusInBondContainer.IsPartReadOnly);
		}

		public void TestBC_Mode()
		{
			var cusInBondContainer = Factory.New<CusInBondContainer>();
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.FCL;
			cusInBondContainer.BC_IsPart = true;
			Assert(cusInBondContainer.BC_IsPart);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.EmptyContainer;
			Assert(!cusInBondContainer.BC_IsPart);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			cusInBondContainer.BC_IsPart = true;
			Assert(cusInBondContainer.BC_IsPart);
			cusInBondContainer.BC_Mode = CusInBondContainerModeList.Codes.GRP;
			Assert(!cusInBondContainer.BC_IsPart);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return moveDetail.Containers.AddNew();
		}
	}
}
