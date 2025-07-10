using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(TradersRemarkCollection))]
	class TradersRemarkCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<TradersRemark>
	{
		public void TestSetDefaultsForNewChild()
		{
			var tradersRemark1 = TradersRemarkCollection.AddNew();
			AssertEquals(1, tradersRemark1.CSI_LineNo);
			var tradersRemark2 = TradersRemarkCollection.AddNew();
			AssertEquals(2, tradersRemark2.CSI_LineNo);

			tradersRemark2.CSI_LineNo = 3;
			var tradersRemark3 = TradersRemarkCollection.AddNew();
			AssertEquals(4, tradersRemark3.CSI_LineNo);
		}

		protected override CusSupportingInfoCollection<TradersRemark> GetCusSupportingInfoCollection() => TradersRemarkCollection;

		TradersRemarkCollection TradersRemarkCollection => tradersRemarkCollection ?? (tradersRemarkCollection = new TradersRemarkCollection(Factory.New<JobDeclaration>()));
		TradersRemarkCollection tradersRemarkCollection;
	}
}
