using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsOriginControlTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;
			line.NatureAndQtyOfGoodsOrigin.Country = Core.Constants.CountryCodes.Australia;

			using (FormForTest form = new FormForTest(line))
			{
				form.Show();

				ZCodeFindBox countryCodeFindBox = (ZCodeFindBox)form.Control.Controls.Find("countryCodeFindBox", true).FirstOrDefault();
				AssertEquals(Core.Constants.CountryCodes.Australia, countryCodeFindBox.Text);
			}
		}

		class FormForTest : ZForm
		{
			public FormForTest(ExportAWBRateLine line)
				: base(line)
			{
				Control = new NatureAndQtyOfGoodsOriginControl();
				Controls.Add(Control);
			}

			public NatureAndQtyOfGoodsOriginControl Control;

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
