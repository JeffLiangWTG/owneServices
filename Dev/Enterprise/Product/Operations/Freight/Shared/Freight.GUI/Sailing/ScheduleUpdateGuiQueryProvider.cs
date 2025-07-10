using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI
{
	public sealed class ScheduleUpdateGuiQueryProvider : IScheduleUpdateQueryProvider
	{
		public ScheduleUpdateGuiQueryProvider(string additionalETDUpdateMsg = null, string additionalETAUpdateMsg = null)
		{
			this.additionalETDUpdateMsg = additionalETDUpdateMsg;
			this.additionalETAUpdateMsg = additionalETAUpdateMsg;
		}

		readonly string additionalETDUpdateMsg;
		readonly string additionalETAUpdateMsg;

		public static void Set(BusinessObjectFactory factory, string additionalETDUpdateMsg, string additionalETAUpdateMsg)
		{
			ScheduleUpdateQueryProviderFactory.Set(factory, () => new ScheduleUpdateGuiQueryProvider(additionalETDUpdateMsg, additionalETAUpdateMsg));
		}

		public bool ShouldUpdateAgencyShipmentDatesFromATD
		{
			get
			{
				if (shouldUpdateAgencyShipmentDatesFromATD == null)
				{
					shouldUpdateAgencyShipmentDatesFromATD = AskShouldUpdateAgencyShipmentDatesFromATD();
				}
				return shouldUpdateAgencyShipmentDatesFromATD.Value;
			}
		}

		public bool ShouldUpdateRelatedShipmentsETD
		{
			get
			{
				if (shouldUpdateRelatedShipmentsETD == null)
				{
					shouldUpdateRelatedShipmentsETD = AskShouldUpdateRelatedShipmentsETD();
				}
				return shouldUpdateRelatedShipmentsETD.Value;
			}
		}

		public bool ShouldUpdateRelatedShipmentsETA
		{
			get
			{
				if (shouldUpdateRelatedShipmentsETA == null)
				{
					shouldUpdateRelatedShipmentsETA = AskShouldUpdateRelatedShipmentsETA();
				}
				return shouldUpdateRelatedShipmentsETA.Value;
			}
		}

		public bool ShouldSendDelayAlerts(bool hasDelayedImportsVessels, bool hasDelayedExportsVessels)
		{
			bool result = false;
			if (hasDelayedImportsVessels || hasDelayedExportsVessels)
			{
				string message;
				if (hasDelayedImportsVessels && hasDelayedExportsVessels)
				{
					message = Res.GetString("bdd32e67-c60e-4579-938b-7297f8286945", "Do you wish to send a delay alert document to each affected importer and exporter?");
				}
				else if (hasDelayedImportsVessels)
				{
					message = Res.GetString("91ff193d-2a76-4369-a615-55419664d089", "Do you wish to send a delay alert document to each affected importer?");
				}
				else
				{
					message = Res.GetString("712906e4-dd2b-4532-b5a1-021013a9cb85", "Do you wish to send a delay alert document to each affected exporter?");
				}

				DialogResult userResponse = Globals.Message.Show(
					message,
					Res.GetString("dbc7bb49-cde8-470d-99d7-a8d1cd903e97", "Send Delay Alert Documents"),
					MessageBoxButtons.YesNo,
					DialogResult.Yes);

				result = userResponse == DialogResult.Yes;
			}

			return result;
		}

		public void ShowInformation(ZString message)
		{
			//MessageBox.Show(message, Res.GetString("c97b92b1-02ca-4e3a-a36b-6123e27fe28f", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);// this is a work around for problem with the datetime popup focus.
			Globals.Message.ShowInformation(message);
		}

		bool AskShouldUpdateAgencyShipmentDatesFromATD()
		{
			string caption = Res.GetString("69a0e261-6d7a-45fe-a8d1-13cd0d35987e", "Confirmation");
			string message = Res.GetString("7f807ead-2078-441f-b1be-9f2b1b299fa2", "There are some shipping manager shipments departing this port that do not have a shipped on board date and/or issue date set, would you like to default the shipped on board and issued dates for these shipments?");

			DialogResult result = Globals.IsUserInteractive
				? Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				: DialogResult.None;
			return result == DialogResult.Yes;
		}

		bool AskShouldUpdateRelatedShipmentsETD()
		{
			string caption = Res.GetString("69a0e261-6d7a-45fe-a8d1-13cd0d35987e", "Confirmation");
			string message = Res.GetString("843718a2-0df8-4a05-b296-78fd141ee03e", @"You have updated Port of Loading Date.

Do you want the system to automatically update Origin ETD for Shipments and Declarations on the same schedule?

	Click""Yes"" - if you want to update Shipments and Declarations.
	Click""No"" - if you do not want Shipments and Declarations updated.");

			if (!string.IsNullOrEmpty(additionalETDUpdateMsg))
			{
				message += "\r\n\r\n" + additionalETDUpdateMsg;
			}

			DialogResult result = Globals.IsUserInteractive
				? Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				: DialogResult.None;

			if (result == DialogResult.Yes)
			{
				return true;
			}

			return false;
		}

		bool AskShouldUpdateRelatedShipmentsETA()
		{
			string caption = Res.GetString("69a0e261-6d7a-45fe-a8d1-13cd0d35987e", "Confirmation");
			string message = Res.GetString("ede0a69e-1a01-42b4-9c7c-2b01b2f90e6b", @"You have updated Port of Discharge Date.

Do you want the system to automatically update Destination ETA for Shipments and Declarations on the same Schedule?

	Click""Yes"" - if you want to update Shipments and Declarations.
	Click""No"" - if you do not want Shipments and Declarations updated.");

			if (!string.IsNullOrEmpty(additionalETAUpdateMsg))
			{
				message += "\r\n\r\n" + additionalETAUpdateMsg;
			}

			DialogResult result = Globals.IsUserInteractive
				? Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				: DialogResult.None;
			if (result == DialogResult.Yes)
			{
				return true;
			}

			return false;
		}

		bool? shouldUpdateAgencyShipmentDatesFromATD;
		bool? shouldUpdateRelatedShipmentsETD;
		bool? shouldUpdateRelatedShipmentsETA;
	}
}
