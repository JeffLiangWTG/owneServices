using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketLabelControl))]
	public class WhsOrderDocumentTest : NonPersistentBusinessObjectTestCase
	{
		#region Schema

		public void TestSchema()
		{
			AssertEquals("NumberOfLabels", WhsDocketLabelControl.Schema.NumberOfLabels);
			AssertEquals("NumberOfLabelsDescription", WhsDocketLabelControl.Schema.NumberOfLabelsDescription);
			AssertEquals("NumberOfLabelsToPrint", WhsDocketLabelControl.Schema.NumberOfLabelsToPrint);
		}

		#endregion

		#region Properties

		#region Labels

		public void TestNumberOfLabelsDescription()
		{
			AssertEquals("of 0", DocketLabel.NumberOfLabelsDescription);
			Order = Factory.New<WhsOrder>();
			DocketLabel = new WhsDocketLabelControl(Order, 10);
			AssertEquals("of 10", DocketLabel.NumberOfLabelsDescription);
		}

		public void TestNumberOfLabelsDescriptionInfo()
		{
			AssertEquals(WhsDocketLabelControl.Schema.NumberOfLabelsDescription, DocketLabel.NumberOfLabelsDescriptionInfo.Name);
		}

		public void TestNumberOfLabels()
		{
			DocketLabel = new WhsDocketLabelControl(Order, 10);
			AssertEquals(10, DocketLabel.NumberOfLabels);

			DocketLabel = new WhsDocketLabelControl(Order, 12);
			AssertEquals(12, DocketLabel.NumberOfLabels);
		}

		public void TestNumberOfLabelsInfo()
		{
			AssertEquals(WhsDocketLabelControl.Schema.NumberOfLabels, DocketLabel.NumberOfLabelsInfo.Name);
		}

		public void TestNumberOfLabelsToPrint()
		{
			DocketLabel = new WhsDocketLabelControl(Order, 10);
			AssertEquals(10, DocketLabel.NumberOfLabelsToPrint);

			DocketLabel.NumberOfLabelsToPrint = 5;
			AssertEquals(5, DocketLabel.NumberOfLabelsToPrint);
		}

		public void TestNumberOfLabelsToPrintInfo()
		{
			AssertEquals(WhsDocketLabelControl.Schema.NumberOfLabelsToPrint, DocketLabel.NumberOfLabelsToPrintInfo.Name);
		}

		#endregion

		#region PackageLabels

		public void TestNumberOfPackageLabels()
		{
			Order.Lines.AddNew().WE_PackQuantity = 10;
			DocketLabel = new WhsDocketLabelControl(Order, 1);
			AssertEquals(10, DocketLabel.NumberOfPackageLabels);

			Order.Lines[0].WE_PackQuantity = 12.2;
			DocketLabel = new WhsDocketLabelControl(Order, 1);
			AssertEquals(13, DocketLabel.NumberOfPackageLabels);

			Order.Lines[0].WE_PackQuantity = 10.2;
			Order.Lines.AddNew().WE_PackQuantity = 12.4;
			DocketLabel = new WhsDocketLabelControl(Order, 1);
			AssertEquals(11 + 13, DocketLabel.NumberOfPackageLabels);
		}

		public void TestNumberOfPackageLabelsCalculateOnlyParentLines()
		{
			var orderLine1 = Order.Lines.AddNew();
			orderLine1.WE_PackQuantity = 10m;
			AssertEquals(10, DocketLabel.NumberOfPackageLabels);

			var orderLine2 = Order.Lines.AddNew();
			orderLine2.WE_WE_ParentDocketLine = orderLine1.PK;
			orderLine2.WE_PackQuantity = 5m;

			var orderLine3 = Order.Lines.AddNew();
			orderLine3.WE_WE_ParentDocketLine = orderLine1.PK;
			orderLine3.WE_PackQuantity = 7m;
			AssertEquals(10, DocketLabel.NumberOfPackageLabels);
		}

		#endregion

		#endregion

		#region Validation

		public void TestValidateNumberOfLabelsToPrint()
		{
			ZString errorZeroOrLess = "Please enter the number of labels to print";
			ZString errorTooMany = "You cannot print more labels than available";

			DocketLabel = new WhsDocketLabelControl(Order, 0);
			DocketLabel.NumberOfLabelsToPrint = 0;
			AssertHasError(DocketLabel.NumberOfLabelsToPrintInfo, errorZeroOrLess);

			DocketLabel = new WhsDocketLabelControl(Order, 10);
			DocketLabel.NumberOfLabelsToPrint = -5;
			AssertHasError(DocketLabel.NumberOfLabelsToPrintInfo, errorZeroOrLess);

			DocketLabel.NumberOfLabelsToPrintInfo.ClearAllNotifications();
			DocketLabel.NumberOfLabelsToPrint = 12;
			AssertHasError(DocketLabel.NumberOfLabelsToPrintInfo, errorTooMany);

			DocketLabel.NumberOfLabelsToPrintInfo.ClearAllNotifications();
			DocketLabel.NumberOfLabelsToPrint = 10;
			AssertNoErrors(DocketLabel.NumberOfLabelsToPrintInfo);

			DocketLabel.NumberOfLabelsToPrint = 1;
			AssertNoErrors(DocketLabel.NumberOfLabelsToPrintInfo);

			DocketLabel.NumberOfLabelsToPrint = 5;
			AssertNoErrors(DocketLabel.NumberOfLabelsToPrintInfo);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsDocketLabelControl(Factory.New<WhsOrder>(), 0);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Order = Factory.New<WhsOrder>();
			DocketLabel = new WhsDocketLabelControl(Order, 0);
		}

		WhsOrder Order;
		WhsDocketLabelControl DocketLabel;

		#endregion
	}
}
