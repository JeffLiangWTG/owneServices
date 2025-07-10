using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrder))]
	sealed class TrackingWhsOrderIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.New<WhsOrder>());
		}

		new TrackingWhsOrder BizObj
		{
			get { return base.BizObj as TrackingWhsOrder; }
		}

		public void TestDocumentReference()
		{
			BizObj.WhsOrder.WD_DocketID = "W000000001";
			BizObj.WhsOrder.WD_ExternalReference = "W000000002";

			AssertEquals("Reference is DocketID", BizObj.WhsOrder.WD_DocketID, BizObj.Reference);
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}
	}
}
