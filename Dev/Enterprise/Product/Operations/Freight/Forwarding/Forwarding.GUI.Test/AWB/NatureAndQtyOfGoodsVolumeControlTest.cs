using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsVolumeControlTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			line.NatureAndQtyOfGoodsVolume.Volume = 1.23m;
			line.NatureAndQtyOfGoodsVolume.Unit = Core.Constants.Volume.CubicFeet;

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();
				ZCalcDropEdit calcDropEdit = (ZCalcDropEdit)form.Control.Controls.Find("volumeCalcDropEdit", true).FirstOrDefault();
				AssertEquals("1.23", calcDropEdit.Controls.Find("AmountCalcEdit", true).First().Text);
				AssertEquals("CF", calcDropEdit.Controls.Find("UnitDropEdit", true).First().Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsVolumeControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsVolumeControl Control;

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
