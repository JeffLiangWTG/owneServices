using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbInstructionTile : ZUserControl
	{
		public DtbInstructionTile()
		{
			InitializeComponent();
			ZoneLabel.AllowOverlap(DocAddressControl);
		}

		DtbConsignmentInstruction Instruction
		{
			get { return (DtbConsignmentInstruction)CurrentDataItem; }
		}

		#region Binding

		#region SetDataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnhookEvents();
			base.SetDataBinding(dataSource, dataMember);
			HookEvents();
		}

		#endregion

		#region BindToOrganisations

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToOrganisations
		{
			get { return DocAddressControl.BindToOrganisations; }
			set { DocAddressControl.BindToOrganisations = value; }
		}

		#endregion

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.KN_TZ_DomesticZoneInfo.ValueChanged += Instruction_KN_TZ_DomesticZoneChanged;
				instruction.Address.E2_AddressTypeInfo.ValueChanged += Instruction_E2_AddressTypeChanged;

				HideIsAuthorisedToLeaveCheckBox();
			}
		}

		void UnhookEvents()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.KN_TZ_DomesticZoneInfo.ValueChanged -= Instruction_KN_TZ_DomesticZoneChanged;

				if (!instruction.IsDeleted)
				{
					instruction.Address.E2_AddressTypeInfo.ValueChanged -= Instruction_E2_AddressTypeChanged;
				}
			}
		}

		void HideIsAuthorisedToLeaveCheckBox()
		{
			var hideIsAuthorisedToLeave = Instruction.KN_InstructionType == InstructionTypes.Codes.PickUp;
			IsAuthorisedToLeaveCheckBox.Visible = !hideIsAuthorisedToLeave;
		}

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateZoneLabelText();
		}

		#endregion

		#region Actions

		void Instruction_E2_AddressTypeChanged(object sender, EventArgs e)
		{
		}

		void Instruction_KN_TZ_DomesticZoneChanged(object sender, EventArgs e)
		{
			UpdateZoneLabelText();
		}

		#region Zone Captions

		void UpdateZoneLabelText()
		{
			MoveZoneLabelToRightOfAddressGroupBoxCaption();
			var instruction = Instruction;
			if (instruction != null)
			{
				ZoneLabel.Text = GetZoneLabelText(instruction);
			}
		}

		string GetZoneLabelText(DtbConsignmentInstruction instruction)
		{
			var result = (string)instruction.ZoneDescription;
			if (!string.IsNullOrEmpty(result))
			{
				var endOfGroupBox = DocAddressControl.ControlWidth;
				var positionOfZoneLabel = ZoneLabel.Location.X;

				// chop off characters that won't fit and append "..."
				result = result.TruncateToFit(ZoneLabel.Font, endOfGroupBox - positionOfZoneLabel, "...)");
			}

			return result;
		}

		void MoveZoneLabelToRightOfAddressGroupBoxCaption()
		{
			var spaceBetweenGroupBoxCaptionAndZoneLabel = ControlDpiScalingHelper.ScaleToCurrentDpiX(18) + GetWidthOfText(this, DocAddressControl.CaptionResourceString.Caption);
			var width = GetWidthOfText(DocAddressControl, DocAddressControl.CaptionResourceString.Caption);
			ZoneLabel.Location = ControlDpiScalingHelper.NewScaledPoint(width + spaceBetweenGroupBoxCaptionAndZoneLabel, ZoneLabel.Location.Y, false);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Creating a size with maximum integer values. Scaling would overflow")]
		[return: DpiState(DpiState.ScaleX)]
		int GetWidthOfText(Control control, string text)
		{
			using (var graphics = control.CreateGraphics())
			{
				return TextRenderer.MeasureText(graphics, text, control.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
			}
		}

		#endregion

		#endregion

		#region CaptionResourceString

		public override ResourceStringData CaptionResourceString
		{
			get { return DocAddressControl.CaptionResourceString; }
			set { DocAddressControl.CaptionResourceString = value; }
		}

		#endregion

		#region class ZPanelThatOverlapsDocAddress, ZLabelThatOverlapsDocAddress

		class ZPanelThatOverlapsDocAddress : ZPanel
		{
		}

		protected class ZLabelThatOverlapsDocAddress : ZLabel
		{
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
