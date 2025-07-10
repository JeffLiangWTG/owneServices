using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[TestedType(typeof(ExportAWBSecurityStatusLineView))]
	sealed class ReadOnlyExportAWBSecurityStatusLineViewTest : BusinessObjectCollectionViewTestCase<ExportAWBSecurityStatusLineView>
	{
		#region TestFilter

		public void TestFilter()
		{
			var header = Factory.New<ExportAWBHeader>();
			var collection = new ExportAWBSecurityStatusLineCollection(header);

			var line1 = collection.AddNew();
			line1.EAS_ApprovalCategory = "KC";
			line1.EAS_ApprovalNumber = "1234";

			var line2 = collection.AddNew();
			line2.EAS_ScreeningMethod = "XRY";

			var line3 = collection.AddNew();
			line3.EAS_ExemptionGround = "MAI";

			var view = new ExportAWBSecurityStatusLineView(collection, SecurityStatusLineType.ScreeningMethod, false, false);
			view.Rebuild();

			AssertContainsExactElementsInAnyOrder("filter",
				new[] { line2 },
				view);
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			var header = Factory.New<ExportAWBHeader>();
			var collection = new ExportAWBSecurityStatusLineCollection(header);

			var view = new ExportAWBSecurityStatusLineView(collection, SecurityStatusLineType.KnownConsignor, true, false);

			AssertEquals("AllowNew", true, view.AllowNew);
		}

		#endregion

		#region TestAllowRemove

		public void TestAllowRemove()
		{
			var header = Factory.New<ExportAWBHeader>();
			var collection = new ExportAWBSecurityStatusLineCollection(header);

			var view = new ExportAWBSecurityStatusLineView(collection, SecurityStatusLineType.KnownConsignor, false, true);

			AssertEquals("AllowNew", true, view.AllowRemove);
		}

		#endregion

		#region Implementation

		protected override ExportAWBSecurityStatusLineView GetCollectionToTest()
		{
			var header = Factory.New<ExportAWBHeader>();
			var collection = new ExportAWBSecurityStatusLineCollection(header);

			return new ExportAWBSecurityStatusLineView(collection, SecurityStatusLineType.KnownConsignor, false, false);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ExportAWBSecurityStatusLine>();
		}

		#endregion
	}
}
