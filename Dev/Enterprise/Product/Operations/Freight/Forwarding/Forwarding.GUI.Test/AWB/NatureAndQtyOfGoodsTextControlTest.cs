using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsTextControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCreateInstance()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			line.NatureAndQtyOfGoodsText.Text = "frozen ducks";

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();
				ZTextBox textBox = (ZTextBox)form.Control.Controls.Find("textBox", true).FirstOrDefault();
				AssertEquals("frozen ducks", textBox.Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsTextControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsTextControl Control;

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
