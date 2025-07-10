using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.GUI.OrderManager.Orders;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	public class OrderLineToleranceControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShowControl_ShouldBindCorrectly()
		{
			using (var form = new ZForm())
			using (var control = new OrderLineToleranceControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();
			}
		}

		public void TestDefaultValues()
		{
			using (var control = new OrderLineToleranceControlForTest())
			{
				AssertEquals(ToleranceModifiers.None, control.ToleranceModifier);
				AssertEquals(ToleranceTypes.Percent, control.ToleranceType);
			}
		}

		public void TestToleranceBounds_Days()
		{
			using (var control = new OrderLineToleranceControlForTest())
			{
				control.ToleranceType = ToleranceTypes.Days;
				control.ToleranceModifier = ToleranceModifiers.Positive;

				CombineAssertions("It should allow whole numbers from 0 - 99.999", () =>
				{
					AssertEquals(0, control.ToleranceCalcEdit.Decimals);
					AssertEquals(99.999m, control.ToleranceCalcEdit.MaxValue);
				});

				control.ToleranceModifier = ToleranceModifiers.Negative;

				CombineAssertions("It should not change for negative tolerance", () =>
				{
					AssertEquals(0, control.ToleranceCalcEdit.Decimals);
					AssertEquals(99.999m, control.ToleranceCalcEdit.MaxValue);
				});
			}
		}

		public void TestToleranceBounds_Percent()
		{
			using (var control = new OrderLineToleranceControlForTest())
			{
				control.ToleranceType = ToleranceTypes.Percent;
				control.ToleranceModifier = ToleranceModifiers.Positive;

				CombineAssertions("It should allow up to 999.999% for positive percentages", () =>
				{
					AssertEquals(3, control.ToleranceCalcEdit.Decimals);
					AssertEquals(999.999m, control.ToleranceCalcEdit.MaxValue);
				});

				control.ToleranceModifier = ToleranceModifiers.Negative;

				CombineAssertions("It should allow up to 99.999% for negative percentages", () =>
				{
					AssertEquals(3, control.ToleranceCalcEdit.Decimals);
					AssertEquals(99.999m, control.ToleranceCalcEdit.MaxValue);
				});
			}
		}

		#region Implementation

		public class OrderLineToleranceControlForTest : OrderLineToleranceControl
		{
			public ZCalcEdit ToleranceCalcEdit => Controls.Find("ToleranceCalcEdit", true).OfType<ZCalcEdit>().First();
		}

		#endregion
	}
}
