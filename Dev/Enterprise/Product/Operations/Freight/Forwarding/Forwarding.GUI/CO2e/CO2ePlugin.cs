using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Integration;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class CO2ePlugin : ZPlugIn, INotifications
	{
		readonly ICO2eCalculationSupporter hostBO;

		public CO2ePlugin(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			hostBO = hostBusinessEntity as ICO2eCalculationSupporter;
			if (hostBO == null)
			{
				Enabled = false;
				ErrorReporter.ReportOnce($"CO2e Plugin is not supported for type: {hostBusinessEntity.GetType()}");
			}
			else
			{
				CO2eRecalculationGUIChecker.Register(hostBO.Factory);
			}
		}

		public override string Name => Res.GetString("10594a86-c117-4cc6-89b6-76b23e96102a", "Greenhouse Gas Emissions");

		protected override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.AlwaysAllow;

		#region Menu Item

		protected override MenuItem GetNewTopLevelMenu()
		{
			var actionLevelMenu = Form?.Menu?.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			if (actionLevelMenu == null)
			{
				return null;
			}

			var calculateCO2eMenuItem = new ZMenuItem(ResString.GetMultilingualString("665e92ee-5297-4191-a72d-ada393d9ed9f", "Calculate Greenhouse Gas Emissions (CO2e)"));
			calculateCO2eMenuItem.Click += CalculateCO2eEmission_Click;
			actionLevelMenu.MenuItems.Add(calculateCO2eMenuItem);

			return actionLevelMenu;
		}

		async void CalculateCO2eEmission_Click(object sender, EventArgs e)
		{
			if (((IBusiness)hostBO).HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("e40cf895-5961-4bdf-aba3-86cc7c7f3f98", "Please save before calculating greenhouse gas emissions."), Res.GetString("a1952b7d-e88e-462e-a9ce-bb1775563429", "Request failed"));
				return;
			}

			var reasons = hostBO.ValidateInputsWithAdditionalSupporter();
			if (reasons.Count > 0)
			{
				Globals.Message.ShowError(
					string.Join("\n", reasons.Prepend(Res.GetString("a2bee59a-7813-487e-995e-812d0acfc76e", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:"))),
					Res.GetString("be1f722d-77b3-46d3-878d-d5d74cdd7a93", "Request failed"));
				return;
			}

			var result = await new CO2eRequestProcessor().SendRequestAsync(hostBO, this);

			if (result.Type is CO2eResultType.EhubSuccess)
			{
				Globals.Message.ShowInformation(
					Res.GetString("9f0ab8d3-c035-4db6-9cd7-44422702f78f", "The greenhouse gas emissions calculation has been requested."),
					Res.GetString("189b36a9-3350-4758-adf3-e6e07d7d3ee9", "Request sent"));
			}
			else if (result.Type is CO2eResultType.ApiError)
			{
				Globals.Message.ShowError(
					Res.GetString("343a78cc-dab3-4b97-87c0-a482c180cdd8", "Error requesting greenhouse gas emissions calculation service."),
					Res.GetString("8c8943a2-6ad1-44b7-b0f4-376c6e7a93ff", "Request failed"));
			}
			else if (result.Type is CO2eResultType.ApiFail)
			{
				Globals.Message.ShowError(
					Res.GetString("f9e40e88-e541-4e6f-80ce-132234d8cd6d", "Error requesting greenhouse gas emissions calculation: {0}", result.Message),
					Res.GetString("e4cc70e6-c78e-4a09-9585-386d0e291df3", "Request failed"));
			}
			else if (result.Type is CO2eResultType.Unauthorized)
			{
				Globals.Message.ShowError(
					Res.GetString("166C9304-7710-4719-B687-569308C551E1", "Error requesting greenhouse gas emissions calculation service: Unauthorized Request.\r\nEnsure you are using a registered version of CargoWise."),
					Res.GetString("741B6FCD-42FA-4E96-AA41-A46BB9FE1D55", "Request failed"));
			}
			else if (result.Type is CO2eResultType.ServiceUnavailable)
			{
				Globals.Message.ShowError(
					Res.GetString("3677a785-97b0-4049-95a6-066563a8eb5d", "Error requesting greenhouse gas emissions calculation service: Service Unavailable.\r\nIssue communicating with server. Please try again later."),
					Res.GetString("6d9fc810-a90a-4857-9110-2bcf15f805b9", "Request failed"));
			}
		}

		#endregion

		#region INotifications member

		public void Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion
	}
}
