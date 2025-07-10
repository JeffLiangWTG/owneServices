using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageLegForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		public CartageLegForm(CommonCartageLeg cartageLeg)
			: base(cartageLeg)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			WorkflowTabPage.Initialize(cartageLeg);

			SetupForm();

			RunSheetSecurityGUIProvider.Register(cartageLeg.Factory);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetReadOnlyIfCartageLegIsNotRoot();
		}

		void SetReadOnlyIfCartageLegIsNotRoot()
		{
			if (CartageLeg != null && CartageLeg.IsRoot)
			{
				CartageLeg?.UpdateReadOnlyForWhenCancelled();
			}
		}

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("9ab0c15e-c1f4-4fe8-851d-a0fecf55e395", "Port Transport Leg");
				if (!CartageLeg.UniqueIDWithJobNumber.IsEmpty)
				{
					caption += " - " + CartageLeg.UniqueIDWithJobNumber;
				}
				return caption;
			}
		}

		protected override IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return CartageLeg;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			CartageLeg.Cartage.CheckTotalsDiffer();

			return base.ShowPreSaveDialogs();
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				UnHookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void SetupForm()
		{
			HookEvents();
		}

		void HookEvents()
		{
			if (CartageLeg.Cartage != null)
			{
				CartageLeg.Cartage.UpdateCartageTotalsPackQuantityVariation += new CancelEventHandler(Cartage_CheckUpdateCartageTotals);
			}
		}

		void UnHookEvents()
		{
			if (CartageLeg.Cartage != null)
			{
				CartageLeg.Cartage.UpdateCartageTotalsPackQuantityVariation -= new CancelEventHandler(Cartage_CheckUpdateCartageTotals);
			}
		}

		void Cartage_CheckUpdateCartageTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				string caption = Res.GetString("28f21203-05ef-4bb0-bf41-b20dbf4e6223", "Totals do not match");
				string message = Res.GetString("2b820961-34ee-4fb4-948d-4c593b4ada1e", "Total packs, weight and volume do not match the Port Transport total. Would you like to update the Port Transport to match the packline totals?");
				DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result != DialogResult.Yes;
			}
		}

		CommonCartageLeg CartageLeg
		{
			get { return (CommonCartageLeg)BusinessEntity; }
		}

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			QueryUserMsgBoxEventArgs msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No)) == DialogResult.Yes;
			}
		}
	}
}
