using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsDimensionsControlTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			line.NatureAndQtyOfGoodsDimensions.Length = 1;
			line.NatureAndQtyOfGoodsDimensions.Width = 2;
			line.NatureAndQtyOfGoodsDimensions.Height = 3;
			line.NatureAndQtyOfGoodsDimensions.Unit = Core.Constants.Length.Metres;
			line.NatureAndQtyOfGoodsDimensions.Count = 4;

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();

				ZCalcEdit lengthCalcEdit = (ZCalcEdit)form.Control.Controls.Find("lengthCalcEdit", true).FirstOrDefault();
				ZCalcEdit widthCalcEdit = (ZCalcEdit)form.Control.Controls.Find("widthCalcEdit", true).FirstOrDefault();
				ZCalcEdit heightCalcEdit = (ZCalcEdit)form.Control.Controls.Find("heightCalcEdit", true).FirstOrDefault();
				ZDropEdit unitDropEdit = (ZDropEdit)form.Control.Controls.Find("unitDropEdit", true).FirstOrDefault();
				ZCalcEdit countCalcEdit = (ZCalcEdit)form.Control.Controls.Find("countCalcEdit", true).FirstOrDefault();

				AssertEquals("1", lengthCalcEdit.Text);
				AssertEquals("2", widthCalcEdit.Text);
				AssertEquals("3", heightCalcEdit.Text);
				AssertEquals("M", unitDropEdit.Text);
				AssertEquals("4", countCalcEdit.Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsDimensionsControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsDimensionsControl Control;

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
