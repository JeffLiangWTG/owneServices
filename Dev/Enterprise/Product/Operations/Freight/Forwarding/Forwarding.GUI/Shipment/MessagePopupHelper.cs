using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class MessagePopupHelper
	{
		#region Inspection Status

		#region Prompt Reason For Changing Inspection

		internal static void PromptReasonForChangingSecurityInspectionStatusEventHandler(object sender, ReasonForChangingSecurityInspectionStatusEventArgs e)
		{
			var userResponseArgs = new UserResponseArgument
			{
				Caption = Res.GetString("d14f212c-c238-4708-b243-e1bd25b9a2c9", "Change Reason"),
				Message = Res.GetString("21e2b04d-9bb4-444b-93ca-709c7e960632", "Enter the reason for changing the Security Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference."),
				Buttons = ZMessageBoxButtons.OK,
				DefaultButton = ZMessageBoxDefaultButton.Button1,
				UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal,
				Icon = ZMessageBoxIcon.Information,
				MinimumResponseLength = 1
			};

			e.Reason = Globals.Message.QueryUserResponse(userResponseArgs).Trim();
		}

		#endregion

		#region Prompt Reason For Changing Additional Inspection

		internal static void PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler(object sender, ReasonForChangingSecurityInspectionStatusEventArgs e)
		{
			var userResponseArgs = new UserResponseArgument
			{
				Caption = Res.GetString("7130AAAC-9F9D-4DA0-A5B9-E0305A569B57", "Change Reason"),
				Message = Res.GetString("9125D13F-5397-4CA9-A607-51B515DF9873", "Enter the reason for changing the Security Additional Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference."),
				Buttons = ZMessageBoxButtons.OK,
				DefaultButton = ZMessageBoxDefaultButton.Button1,
				UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal,
				Icon = ZMessageBoxIcon.Information,
				MinimumResponseLength = 1
			};

			e.Reason = Globals.Message.QueryUserResponse(userResponseArgs).Trim();
		}

		#endregion

		#region Pack Level Screening

		internal static void CheckRedefaultPackLineInspectionTypeCodesFromShipment(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				var shipment = sender as ForwardingShipment;
				if (shipment != null && shipment.IsRedefaultingInspectionTypeCodesSuspended)
				{
					e.Cancel = true;
				}
				else
				{
					var result = Globals.Message.Show(Res.GetString("d71e80e8-6be7-4def-90f7-e6df539d667f", "Do you want to apply this Inspection Type to all Pack Lines on this Shipment?"),
						Res.GetString("19ccbb83-eacb-43b7-991b-2746623dd522", "Update Pack Lines"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);
					e.Cancel = result != DialogResult.Yes;
				}
			}
		}

		#endregion

		#region Sub HVL Shipment Inspection Type Updating

		internal static void CheckUpdateSubHVLShipmentInspectionTypeFromShipment(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("af13fb89-3831-41e9-88e8-450433130c8f", "Do you want to apply this Inspection Type to all sub HVL shipments?"),
				Res.GetString("38c91f7e-681b-4a04-a3fb-14cb678cf23d", "Update Sub HVL Shipment"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
			e.Cancel = result != DialogResult.Yes;
		}

		#endregion

		#endregion

		#region Is High Risk

		internal static void CheckRedefaultPackLineIsHighRiskFromShipment(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				var shipment = sender as ForwardingShipment;
				if (shipment != null && shipment.IsRedefaultingInspectionTypeCodesSuspended)
				{
					e.Cancel = true;
				}
				else
				{
					string message = shipment.JS_IsHighRisk
					? Res.GetString("77a37155-851d-4bf8-9060-63af60125ea5", "Do you want to apply ‘Is High Risk’ to all packlines on this Shipment?")
					: Res.GetString("bcdc2b36-d2b9-4839-9d66-99aa4ac35c4d", "Do you want to remove ‘Is High Risk’ from all packlines on this Shipment?");

					var result = Globals.Message.Show(
						message,
						Res.GetString("5964ac5e-a73d-4b2e-bf88-0b2fc13fe919", "Update Pack Lines"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question
					);

					e.Cancel = result != DialogResult.Yes;
				}
			}
		}

		#endregion

		#region Additional Inspection Status

		internal static void CheckRedefaultPackLineAdditionalInspectionTypeCodesFromShipment(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				var shipment = sender as ForwardingShipment;
				if (shipment != null && shipment.IsRedefaultingAdditionalInspectionTypeCodesSuspended)
				{
					e.Cancel = true;
				}
				else
				{
					var result = Globals.Message.Show(Res.GetString("ee81e7ca-d9d1-4329-9058-6099341984f6", "Do you want to apply this Inspection Type to all packlines on this Shipment?"),
						Res.GetString("00be720d-8768-4605-b290-24d07323ecc2", "Update Pack Lines"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);
					e.Cancel = result != DialogResult.Yes;
				}
			}
		}

		#endregion

		#region Delivery Due Date

		public static void PromptReasonForChangingDeliveryDueDateEventHandler(object sender, ReasonForChangingDeliveryDueDateEventArgs e)
		{
			var userResponseArgs = new UserResponseArgument
			{
				Caption = Res.GetString("c20ee88a-7a5f-4863-ad61-d21fceb1c36e", "Change Reason"),
				Message = Res.GetString("11b4c0cd-1e44-471b-a173-f5e5ac655c66", "Enter the reason for changing the Delivery Due Date.\r\nThe reason will be recorded on the DDE-Delivery Date Updated event for future reference."),
				Buttons = ZMessageBoxButtons.OK,
				DefaultButton = ZMessageBoxDefaultButton.Button1,
				UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal,
				Icon = ZMessageBoxIcon.Information,
				MinimumResponseLength = 1
			};

			e.Reason = Globals.Message.QueryUserResponse(userResponseArgs).Trim();
		}

		public static void NotifyDeliveryDueDateNotChangedInManualCalculation(object sender, EventArgs eventArgs)
		{
			var caption = Res.GetString("5BD89817-911A-4BE7-B847-164892F3B3DF", "DDD Calculation Result");
			var message = Res.GetString("0A4818C3-3F68-4554-A40E-D25A2864BF12", "The Delivery Due Date (DDD) has not been changed based on data entered.");

			Globals.Message.ShowInformation(message, caption);
		}

		#endregion
	}
}
