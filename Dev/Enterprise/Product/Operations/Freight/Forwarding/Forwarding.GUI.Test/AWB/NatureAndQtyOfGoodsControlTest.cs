using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsControlTest : TestCaseWithFactory
	{
		public virtual void TestSetDetailsControl()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

			using (FormForTest form = new FormForTest(line, new NatureAndQtyOfGoodsControl()))
			{
				form.Show();
				AssertEquals("DetailsControl should be NatureAndQtyOfGoodsTextControl", typeof(NatureAndQtyOfGoodsTextControl), form.DetailControl.UserControlType);

				line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
				AssertEquals("DetailsControl should be NatureAndQtyOfGoodsTextControl regardless of line type", typeof(NatureAndQtyOfGoodsTextControl), form.DetailControl.UserControlType);
			}
		}

		protected class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line, NatureAndQtyOfGoodsControl control)
				: base(line)
			{
				Control = control;
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsControl Control;
			public ZDynamicControlCreationUserControl DetailControl
			{
				get { return (ZDynamicControlCreationUserControl)Control.Controls.Find("natureAndQtyOfGoodsDetails", true).First(); }
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
