using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetail))]
	sealed class CusInBondMoveDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusInBondContainerType()
		{
			var supporter = movementDetail as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(CusInBondContainer), supporter.ContainerType);
			AssertEquals(typeof(CusInBondContainer), movementDetail.ContainerType);
		}

		public void TestCusInBondMoveLineItemCollection()
		{
			var inBondMoveLineItem = movementDetail.CusInBondMoveLineItemCollection.AddNew();
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusInBondMoveLineItem));
			query.AddToFilter(CusInBondMoveLineItemSchema.BI_B9, movementDetail.PK);
			var cusInBondMoveLineItem = Factory.Load(typeof(CusInBondMoveLineItem), query);
			AssertEquals(1, cusInBondMoveLineItem.Length);
		}

		public void TestInBondMoveLineItem()
		{
			var inBondMoveLineItem = movementDetail.InBondMoveLineItem;
			inBondMoveLineItem.BI_Weight = 2m;
			AssertSame(inBondMoveLineItem, movementDetail.CusInBondMoveLineItemCollection.FirstOrDefault());
			AssertEquals(Core.Constants.PkgUnit.Piece, inBondMoveLineItem.BI_QuantityUQ);
		}

		CusInBondHeader header;
		CusInBondMoveHeader movementHeader;
		CusInBondMoveDetail movementDetail;
		protected override void SetUp()
		{
			base.SetUp();
			CreateNewBusinessObject();
		}

		void CreateNewBusinessObject()
		{
			header = Factory.New<CusInBondHeader>();
			movementHeader = header.MovementHeaders.AddNew();
			movementDetail = movementHeader.MovementDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CreateNewBusinessObject();
			return movementDetail;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CreateNewBusinessObject();
			return movementDetail;
		}
	}
}
