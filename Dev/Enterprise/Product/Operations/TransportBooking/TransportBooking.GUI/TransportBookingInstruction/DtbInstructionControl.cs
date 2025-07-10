using System;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DtbInstructionControl : ZUserControl
	{
		public DtbInstructionControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Instruction != null)
			{
				Instruction.KN_SequenceInfo.ValueChanged -= new EventHandler(KN_SequenceInfo_ValueChanged);
				Instruction.KN_InstructionTypeInfo.ValueChanged -= new EventHandler(KN_InstructionTypeInfo_ValueChanged);
				Instruction.OrganisationTypeInfo.ValueChanged -= new EventHandler(OrganisationTypeInfo_ValueChanged);
			}

			base.SetDataBinding(dataSource as DtbBookingInstruction, dataMember); // weird issue where Architecture tries binding this control to DtbBooking top level datasource. This is a manually added control

			currentInstruction = (DtbBookingInstruction)CurrentDataItem;

			if (Instruction != null)
			{
				Instruction.KN_SequenceInfo.ValueChanged += new EventHandler(KN_SequenceInfo_ValueChanged);
				Instruction.KN_InstructionTypeInfo.ValueChanged += new EventHandler(KN_InstructionTypeInfo_ValueChanged);
				Instruction.OrganisationTypeInfo.ValueChanged += new EventHandler(OrganisationTypeInfo_ValueChanged);
			}

			RefreshCaptions();
		}

		void OrganisationTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptions();
		}

		void KN_InstructionTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptions();
		}

		void KN_SequenceInfo_ValueChanged(object sender, EventArgs e)
		{
			((DtbInstructionStandardViewControl)Parent).RefreshInstructionTiles();
		}

		void RefreshCaptions()
		{
			var addressCap = Instruction != null ? Instruction.Address.AddressCaption : ZString.Empty;
			var instructionCap = Instruction != null ? Instruction.Lookups.InstructionTypes.GetDescriptionFromCode(Instruction.KN_InstructionType) : "";

			DocAddressControl.Text = instructionCap + " - " + addressCap;

			if (Instruction != null)
			{
				if (Instruction.Confirmations.Count > 0)
				{
					var confirm = Instruction.Confirmations[0];
					ReqFromDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = GetPicDlvLabel(Instruction, confirm.RequiredFromLabel);
					ReqToDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = GetPicDlvLabel(Instruction, confirm.RequiredToLabel);

					EstimatedDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = GetPicDlvLabel(Instruction, Res.GetString("DtbBookingInstructionGUI|Estimated", "Estimated"));
					ActualDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = GetPicDlvLabel(Instruction, Res.GetString("DtbBookingInstructionGUI|Actual", "Actual"));
				}

				var orgType = Instruction.OrganisationType;
				var hideDropMode = new ZString[] { OrganisationTypesList.Codes.CTO, OrganisationTypesList.Codes.CYD }.Contains(orgType);
				var hideSignedBy = Instruction.KN_InstructionType == InstructionTypes.Codes.PickUp;
				var hideIsAuthorisedToLeave = Instruction.KN_InstructionType == InstructionTypes.Codes.PickUp;

				DropModeDropEdit.Visible = !hideDropMode;
				ReceivedByTextBox.Visible = !hideSignedBy;
				IsAuthorisedToLeaveCheckBox.Visible = !hideIsAuthorisedToLeave;
			}
		}

		static string GetPicDlvLabel(DtbBookingInstruction instruction, string label)
		{
			return instruction.KN_InstructionType == InstructionTypes.Codes.PickUp
				? Res.GetString("DtbBookingInstruction|Pic", "Pic. {0}", label)
				: Res.GetString("DtbBookingInstruction|Dlv", "Dlv. {0}", label);
		}

		DtbBookingInstruction Instruction
		{
			get { return currentInstruction; }
		}
		DtbBookingInstruction currentInstruction;
	}
}
