using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsLithiumBatteryControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCreateInstance()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			line.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI968;

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();
				ZDropEdit dropEdit = (ZDropEdit)form.Control.Controls.Find("lithiumBatteryTypeDropEdit", true).FirstOrDefault();
				AssertEquals(Constants.AWB.LithiumBatteryTypes.Codes.PI968, dropEdit.CodeBox.Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsLithiumBatteryControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsLithiumBatteryControl Control;

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
