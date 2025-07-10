using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsSLACControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCreateInstance()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			line.NatureAndQtyOfGoodsSLAC.Count = 5;

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();

				ZCalcEdit countCalcEdit = (ZCalcEdit)form.Control.Controls.Find("countCalcEdit", true).FirstOrDefault();
				AssertEquals("5", countCalcEdit.Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsSLACControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsSLACControl Control;

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
