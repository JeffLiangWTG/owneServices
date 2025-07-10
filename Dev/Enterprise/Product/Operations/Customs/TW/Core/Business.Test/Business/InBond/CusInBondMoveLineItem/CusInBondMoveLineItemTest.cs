using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveLineItem))]
	sealed class CusInBondMoveLineItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBI_Quantity_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(moveLine.GetType(), nameof(moveLine.BI_Quantity), false, attr => attr.DecimalPlaces == 4);
		}

		public void TestValidation()
		{
			AssertEquals("Validation", typeof(CusInBondMoveLineItemValidation), moveLine.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("Lookups", typeof(CusInBondMoveLineItemLookups), moveLine.Lookups.GetType());
		}

		public void TestMoveDetail()
		{
			AssertEquals(moveDetail.PK, moveLine.MoveDetail.PK);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Core.Constants.PkgUnit.Piece, moveLine.BI_QuantityUQ);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			header = factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveLine = moveDetail.CusInBondMoveLineItemCollection.AddNew();
			return moveLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			moveLine = moveDetail.CusInBondMoveLineItemCollection.AddNew();
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveDetail moveDetail;
		CusInBondMoveLineItem moveLine;
	}
}
