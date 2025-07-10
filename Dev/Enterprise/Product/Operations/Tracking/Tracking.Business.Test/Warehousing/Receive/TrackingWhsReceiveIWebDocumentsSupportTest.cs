using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceive))]
	[SetGlobalsIsWeb]
	sealed class TrackingWhsReceiveIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
		}

		new TrackingWhsReceive BizObj
		{
			get { return base.BizObj as TrackingWhsReceive; }
		}

		public void TestDocumentReference()
		{
			BizObj.WhsReceive.WD_DocketID = "W000000001";
			BizObj.WhsReceive.WD_ExternalReference = "W000000002";

			AssertEquals("Reference is DocketID", BizObj.WhsReceive.WD_DocketID, BizObj.Reference);
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}
	}
}
