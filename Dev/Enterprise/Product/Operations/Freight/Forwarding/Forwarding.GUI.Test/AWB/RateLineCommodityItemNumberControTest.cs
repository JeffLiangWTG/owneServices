using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class RateLineCommodityItemNumberControTest : TestCaseWithFactory
	{
		public void TestSetControlType()
		{
			var line = Factory.New<ConsolExportAWBRateLine>();

			using (var form = new FormForTest(line))
			{
				form.Show();

				void assertSetCorrectControlFromRateClassType(string rateClass, bool isShowForTextBox = false, bool isShowForDropEdit = false, bool isShowForFindBox = false)
				{
					line.ER_RateClass = rateClass;
					AssertEquals(isShowForTextBox, form.CommodityNumberItemTextBox.Visible);
					AssertEquals(isShowForDropEdit, form.CommodityItemNumberDropEdit.Visible);
					AssertEquals(isShowForFindBox, form.CommodityItemNumberFindBox.Visible);
				}

				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.BasicCharge, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.ClassRateReduction, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.ClassRateSurcharge, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.InternationalPriorityService, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.MinimumCharge, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.NormalCharge, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.QuantityRate, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.RatePerKilogram, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.UnitLoadDeviceDiscount, isShowForTextBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.WeightIncrease, isShowForTextBox: true);

				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation, isShowForDropEdit: true);

				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.SpecificCommodityRate, isShowForFindBox: true);
				assertSetCorrectControlFromRateClassType(Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, isShowForFindBox: true);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new RateLineCommodityItemNumberControl();
				Controls.Add(Control);
			}

			public RateLineCommodityItemNumberControl Control;
			public ZTextBox CommodityNumberItemTextBox
			{
				get { return (ZTextBox)Control.Controls.Find("CommodityNumberItemTextBox", true).First(); }
			}

			public ZDropEdit CommodityItemNumberDropEdit
			{
				get { return (ZDropEdit)Control.Controls.Find("CommodityItemNumberDropEdit", true).First(); }
			}

			public ZCodeFindBox CommodityItemNumberFindBox
			{
				get { return (ZCodeFindBox)Control.Controls.Find("CommodityItemNumberFindBox", true).First(); }
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Control.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
