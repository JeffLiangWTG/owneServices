using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CartageZoneDistanceControl : BaseCombinedCalculatorControl
	{
		public CartageZoneDistanceControl()
		{
			InitializeComponent();
			ConvFactorDropEdit.DropDownClosed += new EventHandler(ConversionFactorDropEdit_DropDownClosed);
			ConvFactorDropEdit.Enter += new EventHandler(ConversionFactorDropEdit_Enter);
		}

		#region Custom Conversion Factor

		string OldText;
		void ConversionFactorDropEdit_Enter(object sender, EventArgs e)
		{
			OldText = ConvFactorDropEdit.Text;
		}

		void ConversionFactorDropEdit_DropDownClosed(object sender, EventArgs e)
		{
			if (OldText == ConvFactorDropEdit.Text)
			{
				return;
			}

			if (ConvFactorDropEdit.Text == "CUSTOM")
			{
				RateLine line = null;
				foreach (Binding binding in ConvFactorDropEdit.DataBindings)
				{
					if (binding.BindingManagerBase.GetCurrent() is RateLine)
					{
						line = binding.BindingManagerBase.GetCurrent() as RateLine;
						break;
					}
				}

				using (CustomConversionFactorForm form = new CustomConversionFactorForm(line))
				{
					Point location = ConvFactorDropEdit.Parent.PointToScreen(ConvFactorDropEdit.Location);
					ControlDpiScalingHelper.SetTop(form, location.Y + ConvFactorDropEdit.Size.Height, false);
					ControlDpiScalingHelper.SetLeft(form, location.X, false);
					var result = form.ShowDialog(this);
					if (result == DialogResult.OK)
					{
						ConvFactorDropEdit.Text = form.ConversionFactor;
					}
					else if (result == DialogResult.Cancel)
					{
						ConvFactorDropEdit.Text = OldText;
					}
					else if (line != null)
					{
						ConvFactorDropEdit.Text = line.ConversionFactorForBinding.ConversionFactorString;
					}
				}
			}

			OldText = ConvFactorDropEdit.Text;
		}

		#endregion

		#region Binding

		protected override void SetBindings()
		{
			SetGridBinding(CartagePanel.ZoneRateLineItemsGrid, "CartageZones.ZoneRateLineItems");
			SetGridBinding(CartagePanel.CartageZonesGrid, "CartageZones");

			SetBinding(EquipmentDropEdit, "String1");
			SetListBinding(EquipmentDropEdit, "List1");
			SetBinding(ConvFactorDropEdit, "String2");
			SetListBinding(ConvFactorDropEdit, "List2");

			SetBinding(UseAccumulatedCheckbox, "Bool1");
			SetBinding(HigherChargeableLowerRateCheckBox, "Bool2");
			SetBinding(UseInclusiveBreaksCheckBox, "Bool3");
			SetBinding(ACIZoneCheckBox, "Bool4");

			SetBinding(BreaksPerDropDown, "String3");
			SetListBinding(BreaksPerDropDown, "List3");
		}

		internal override ZGrid GetRateLineItemsGrid()
		{
			return CartagePanel.ZoneRateLineItemsGrid;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

#if DEBUG
		internal CartageCalculatorPanel CartagePanelForTest
		{
			get { return CartagePanel; }
		}
#endif
	}
}

