using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketLabelLine))]
	public class WhsDocketLabelLineTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Constructor

		[ExpectNoExceptions()]
		public void TestEmptyOrderNumber()
		{
			WhsDocketLabelLine line = new WhsDocketLabelLine(Factory.NewWithValidTestData<WhsOrder>(), 2, 2);
			AssertNotNull("Valid ctor.", line);

			try
			{
				line = new WhsDocketLabelLine(null, 2, 2);
			}
			catch (ArgumentException ex)
			{
				AssertEquals("Same exception", "ArgumentNullException", ex.GetType().Name);
			}
		}

		[ExpectNoExceptions()]
		public void TestPositiveDefaultLabels()
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			WhsDocketLabelLine line = new WhsDocketLabelLine(order, 2, 2);
			AssertNotNull("Valid ctor.", line);

			try
			{
				line = new WhsDocketLabelLine(order, -2, 2);
			}
			catch (ArgumentException ex)
			{
				AssertEquals("Same exception", "ArgumentOutOfRangeException", ex.GetType().Name);
			}
		}

		[ExpectNoExceptions()]
		public void TestPositiveTotalLabels()
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			WhsDocketLabelLine line = new WhsDocketLabelLine(order, 2, 2);
			AssertNotNull("Valid ctor.", line);

			try
			{
				line = new WhsDocketLabelLine(order, 2, -2);
			}
			catch (ArgumentException ex)
			{
				AssertEquals("Same exception", "ArgumentOutOfRangeException", ex.GetType().Name);
			}
		}

		public void TestRangeRestrictedDefaultLabelCount()
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			WhsDocketLabelLine line = new WhsDocketLabelLine(order, 1, 2);
			AssertEquals("Default label count unchanged", 1, line.NumberOfLabelsToPrint);

			line = new WhsDocketLabelLine(order, 2, 2);
			AssertEquals("Default label count unchanged", 2, line.NumberOfLabelsToPrint);

			line = new WhsDocketLabelLine(order, 3, 2);
			AssertEquals("Default label count changed", 2, line.NumberOfLabelsToPrint);
		}

		#endregion

		#region Schema

		public void TestSchema()
		{
			AssertEquals("OrderNumber", WhsDocketLabelLine.Schema.OrderNumber);
			AssertEquals("NumberOfLabelsToPrint", WhsDocketLabelLine.Schema.NumberOfLabelsToPrint);
			AssertEquals("TotalNumberOfLabels", WhsDocketLabelLine.Schema.TotalNumberOfLabels);
		}

		#endregion

		#region Test Properties

		public void TestDocket()
		{
			AssertEquals("Same docket", FirstOrder, FirstLine.Docket);
		}

		public void TestOrderNumber()
		{
			AssertEquals("First Order Number", FirstOrder.WD_ExternalReference, FirstLine.OrderNumber);
			AssertEquals("Second Order Number", SecondOrder.WD_ExternalReference, SecondLine.OrderNumber);
		}

		public void TestOrderNumberInfo()
		{
			AssertEquals(WhsDocketLabelLine.Schema.OrderNumber, FirstLine.OrderNumberInfo.Name);
		}

		public void TestNumberOfLabelsToPrint()
		{
			AssertEquals("Line 1 default Number of Labels To Print", 4, FirstLine.NumberOfLabelsToPrint);

			FirstLine.NumberOfLabelsToPrint = 3;
			AssertEquals("Now 3 labels", 3, FirstLine.NumberOfLabelsToPrint);

			FirstLine.NumberOfLabelsToPrint = 0;
			AssertEquals("OK to have 0 labels", 0, FirstLine.NumberOfLabelsToPrint);

			FirstLine.NumberOfLabelsToPrint = -3;
			AssertHasError("Can't have negative numbers", FirstLine.NumberOfLabelsToPrintInfo, "Please enter the number of labels to print.  It can be 0 or greater.");

			FirstLine.NumberOfLabelsToPrint = 333;
			AssertHasError("Can't print more labels then maximum", FirstLine.NumberOfLabelsToPrintInfo, "You cannot print more labels than available.");

			AssertEquals("Line 2 default Number of Labels To Print", 3, SecondLine.NumberOfLabelsToPrint);
		}

		public void TestNumberOfLabelsToPrintInfo()
		{
			AssertEquals(WhsDocketLabelLine.Schema.NumberOfLabelsToPrint, FirstLine.NumberOfLabelsToPrintInfo.Name);
		}

		public void TestTotalNumberOfLabels()
		{
			AssertEquals("TotalNumberOfLabels", 7, FirstLine.TotalNumberOfLabels);
			AssertEquals("TotalNumberOfLabels", 11, SecondLine.TotalNumberOfLabels);
		}

		public void TestTotalNumberOfLabelsInfo()
		{
			AssertEquals(WhsDocketLabelLine.Schema.TotalNumberOfLabels, FirstLine.TotalNumberOfLabelsInfo.Name);
		}

		public void TestLegacyDocketLabelControl()
		{
			AssertLegacyDocketLabelControlEquals("First legacy docket label control", new WhsDocketLabelControl(FirstOrder, 4), FirstLine.LegacyDocketLabelControl);
			AssertLegacyDocketLabelControlEquals("First legacy docket label control", new WhsDocketLabelControl(SecondOrder, 3), SecondLine.LegacyDocketLabelControl);
		}

		void AssertLegacyDocketLabelControlEquals(string p, WhsDocketLabelControl expectedWhsDocketLabelControl, WhsDocketLabelControl actualWhsDocketLabelControl)
		{
			AssertEquals("Docket", expectedWhsDocketLabelControl.Docket.PK, actualWhsDocketLabelControl.Docket.PK);
			AssertEquals("Number Of Labels T oPrint", expectedWhsDocketLabelControl.NumberOfLabelsToPrint, actualWhsDocketLabelControl.NumberOfLabelsToPrint);
			AssertEquals("Number Of Labels", expectedWhsDocketLabelControl.NumberOfLabels, actualWhsDocketLabelControl.NumberOfLabels);
		}

		#endregion

		#region Validation

		public void TestValidationType()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var line = new WhsDocketLabelLine(order, 1, 2);
			AssertEquals(typeof(WhsDocketLabelLineValidation), line.Validation.GetType());
		}

		#endregion

		#region Implementation

		WhsOrder FirstOrder
		{
			get { return firstOrder ?? (firstOrder = Factory.NewWithValidTestData<WhsOrder>()); }
		}
		WhsOrder firstOrder;

		WhsOrder SecondOrder
		{
			get { return secondOrder ?? (secondOrder = Factory.NewWithValidTestData<WhsOrder>()); }
		}
		WhsOrder secondOrder;

		WhsDocketLabelLine FirstLine
		{
			get { return firstLine ?? (firstLine = new WhsDocketLabelLine(FirstOrder, 4, 7)); }
		}
		WhsDocketLabelLine firstLine;

		WhsDocketLabelLine SecondLine
		{
			get { return secondLine ?? (secondLine = new WhsDocketLabelLine(SecondOrder, 3, 11)); }
		}

		WhsDocketLabelLine secondLine;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsDocketLabelLine(Factory.NewWithValidTestData<WhsOrder>(), 10, 10);
		}

		#endregion
	}
}
