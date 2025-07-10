using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DtbInstructionStandardViewControl : ZUserControl
	{
		public DtbInstructionStandardViewControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			UnhookEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			currentBooking = (DtbBooking)CurrentDataItem;

			if (CurrentDataItem != null)
			{
				HookEvents();
			}

			RefreshInstructionTiles();
		}

		void HookEvents()
		{
			var booking = Booking;
			booking.Instructions.CountChanged += Instructions_CountChanged;
			booking.KM_KT_NKBookingTemplateInfo.ValueChanged += KM_KT_NKBookingTemplateInfo_ValueChanged;
		}

		void UnhookEvents()
		{
			var booking = Booking;
			if (booking != null)
			{
				booking.Instructions.CountChanged -= Instructions_CountChanged;
				booking.KM_KT_NKBookingTemplateInfo.ValueChanged -= KM_KT_NKBookingTemplateInfo_ValueChanged;
			}
		}

		void KM_KT_NKBookingTemplateInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshInstructionTiles();
		}

		void Instructions_CountChanged(object sender, EventArgs e)
		{
			RefreshInstructionTiles();
		}

		public void RefreshInstructionTiles()
		{
			int x = 0;

			var booking = Booking;
			if (booking != null)
			{
				if (booking.Instructions.Any())
				{
					var instructionsOrdered = booking.Instructions.Where(i => !i.IsDeleted).OrderBy(i => i.KN_Sequence).ToArray();
					for (x = 0; x < instructionsOrdered.Length; x++)
					{
						var instruction = instructionsOrdered[x];

						DtbInstructionControl instructionTile;

						if (InstructionTiles.Count > x)
						{
							instructionTile = InstructionTiles[x];
							instructionTile.Visible = true;
							instructionTile.SetDataBinding(instruction, "");
						}
						else
						{
							instructionTile = AddInstructionTile(instruction);
						}

						instructionTile.DtbInstructionGroupBox.Visible = x < instructionsOrdered.Length - 1;
						ControlDpiScalingHelper.SetLeft(ref instructionTile, x * instructionTile.Width, false);
						instruction.RefreshBindingIncludingChildren();
					}

					ComplexViewLabel.Visible = false;
				}
				else
				{
					ComplexViewLabel.Visible = true;
				}
			}

			for (int y = x; y < InstructionTiles.Count; y++)
			{
				var instructionTile = InstructionTiles[y];
				instructionTile.Visible = false;
				instructionTile.SetDataBinding(null, "");
			}
		}

		DtbInstructionControl AddInstructionTile(DtbBookingInstruction instruction)
		{
			var tile = new DtbInstructionControl();
			InstructionTiles.Add(tile);
			tile.SetDataBinding(instruction, "");
			Controls.Add(tile);
			return tile;
		}

		DtbBooking Booking
		{
			get { return currentBooking; }
		}
		DtbBooking currentBooking;

		List<DtbInstructionControl> InstructionTiles
		{
			get { return instructionTiles ?? (instructionTiles = new List<DtbInstructionControl>()); }
		}
		List<DtbInstructionControl> instructionTiles;
	}
}
