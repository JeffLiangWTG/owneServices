using System.Windows.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class TopLevelPacksDetailsControlTest : BaseAgencyTest
	{
		public void TestHarmonisedCodeFindBox()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.TopLevelPacks.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();
				var harmonisedCodeControl = (TariffFindBox)form.HarmonisedCodeTextBox;
				AssertNotNull(harmonisedCodeControl);
				AssertNull(harmonisedCodeControl.GetCountryCode?.Invoke());
				AssertEquals("WCO", harmonisedCodeControl.GetDataGrouping());
				AssertEquals("HSN", harmonisedCodeControl.TariffType);
			}
		}

		public void TestDimensionsLabel()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.TopLevelPacks.AddNew();
			using (var form = new FormForTesting(shipment))
			{
				form.Show();
				Application.DoEvents();

				var lengthControl = (ZCalcEdit)form.LengthCalcEdit;
				AssertNotNull(lengthControl);
				AssertEquals("Length", lengthControl.CaptionResourceString.Caption);

				var widthControl = (ZCalcEdit)form.WidthCalcEdit;
				AssertNotNull(widthControl);
				AssertEquals("Width", widthControl.CaptionResourceString.Caption);

				var heightControl = (ZCalcEdit)form.HeightCalcEdit;
				AssertNotNull(heightControl);
				AssertEquals("Height", heightControl.CaptionResourceString.Caption);
			}
		}

		#region Implementation
		class FormForTesting : ZForm
		{
			public FormForTesting(BillOfLading billOfLading) : base(billOfLading)
			{
				control = new TopLevelPacksDetailsControl();
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(billOfLading, "TopLevelPacks");
				Controls.Add(control);
			}

			public Control HarmonisedCodeTextBox
			{
				get
				{
					var placeHolder = (Panel)control.Controls.Find("tariffTextBoxPlaceholder", true)[0];
					return placeHolder.Controls[0];
				}
			}

			public Control LengthCalcEdit
			{
				get
				{
					return control.Controls.Find("LengthCalcEdit", true)[0];
				}
			}

			public Control WidthCalcEdit
			{
				get
				{
					return control.Controls.Find("WidthCalcEdit", true)[0];
				}
			}

			public Control HeightCalcEdit
			{
				get
				{
					return control.Controls.Find("HeightCalcEdit", true)[0];
				}
			}

			readonly TopLevelPacksDetailsControl control;
		}
		#endregion
	}
}
