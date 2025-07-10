using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInBondMoveDetail))]
	public abstract class CusInBondMoveDetailTest<T> : EnterpriseBusinessObjectTestCase where T : CusInBondMoveDetail
	{
		public virtual void TestLookups()
		{
			AssertEquals(true, typeof(CusInBondMoveDetailLookups).IsAssignableFrom(moveDetail.Lookups.GetType()));
		}

		public virtual void TestProperties()
		{
			AssertEquals(ZString.Empty, moveDetail.HumanReadableName);
			moveHeader.InBondNumber = "964397663";
			AssertEquals("964397663 / ", moveDetail.HumanReadableName);
			bill.B0_MasterBillNumber = "MB234422";
			AssertEquals("964397663 / MB234422", moveDetail.HumanReadableName);
			AssertEquals("MB234422", moveDetail.MasterBillNumber);
			moveHeader.InBondNumber = ZString.Empty;
			AssertEquals("MB234422", moveDetail.HumanReadableName);
		}

		public void TeststackTraceWhenB9_B0SetToEmpty()
		{
			moveDetail.B9_B0 = ZGuid.Empty;
			var expectStackTrace = "at Enterprise.Customs.US.Business.CusInBondMoveDetail.set_B9_B0(ZGuid value)";

			try
			{
				Factory.Save();
			}
			catch (CargoWise.EntityFramework.ZSaveException ex)
			{
				Assert(ex.Message.Contains(expectStackTrace));
			}
		}

		protected Customs.Business.CusInBondHeader header;
		protected Customs.Business.CusInBondBill bill;
		protected CusInBondMoveHeader moveHeader;
		protected CusInBondMoveDetail moveDetail;
		protected override void SetUp()
		{
			base.SetUp();
			header = CreateNewCusInBondHeader();
			bill = CreateNewCusInBondBill(header);
			moveHeader = CreateNewCusInBondMoveHeader(header);
			moveDetail = CreateNewCusInBondMoveDetail(moveHeader);
			moveDetail.B9_B0 = bill.PK;
		}

		protected abstract CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header);

		protected abstract Customs.Business.CusInBondHeader CreateNewCusInBondHeader();

		protected abstract Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header);

		protected abstract T CreateNewCusInBondMoveDetail(CusInBondMoveHeader moveHeader);
	}
}
