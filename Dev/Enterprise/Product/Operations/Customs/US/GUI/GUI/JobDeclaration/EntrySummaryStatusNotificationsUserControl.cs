using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class EntrySummaryStatusNotificationsUserControl : ZUserControl
	{
		public EntrySummaryStatusNotificationsUserControl()
		{
			InitializeComponent();
			InitializeGridLayoutCore();
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			RefreshContextMenu();
		}

		internal void RefreshContextMenu()
		{
			if (writeToLogMenu != null)
			{
				MQEDIMessage message = (MQEDIMessage)GetCurrentEDIMessage();
				writeToLogMenu.Enabled = message != null && !message.ActionAuthorised;
			}
		}

		void InitializeGridLayoutCore()
		{
			StatusNotificationsGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		void StatusNotificationsGrid_AfterBind(object sender, EventArgs e)
		{
			StatusNotificationsGrid.ListManager.PositionChanged += ListManager_PositionChanged;
			ListManager_PositionChanged(null, null);
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			SetQuotaInformationsGridVisible();
		}

		void SetQuotaInformationsGridVisible()
		{
			var currentErrorsRecord = (ErrorsRecord)StatusNotificationsGrid.ListManager.GetCurrent();
			var isVisible = currentErrorsRecord != null && currentErrorsRecord.QuotaInformations.Count > 0;
			QuotaInformationsGrid.Visible = isVisible;
		}

		IStmALogParent GetCurrentEDIMessage()
		{
			return GetEDIMessage(StatusNotificationsGrid.ListManager);
		}

		internal IStmALogParent GetEDIMessage(CurrencyManager currencyManager)
		{
			IStmALogParent result = null;

			if (currencyManager != null && currencyManager.List.Count > 0)
			{
				if (currencyManager.GetCurrent() != null)
				{
					ErrorsRecord record = (ErrorsRecord)currencyManager.GetCurrent();

					if (record != null)
					{
						result = record.message;
					}
				}
			}

			return result;
		}

		protected override void OnCurrentDataItemChanged(EventArgs eventArgs)
		{
			JobDeclaration declaration = DataSource as JobDeclaration;
			IStmALogParent parent = declaration == null ? DataSource as IStmALogParent : declaration.Shipment ?? (IStmALogParent)declaration;
			StatusNotificationsGrid.ColourDeciding -= StatusNotificationsGrid_ColourDeciding;

			if (writeToLogMenu != null)
			{
				StatusNotificationsGrid.ContextMenu.MenuItems.Remove(writeToLogMenu);
				writeToLogMenu = null;
			}

			if (DataSource != null)
			{
				writeToLogMenu = new WriteToLogMenuItem(parent, GetCurrentEDIMessage, Env.Security.CustomsDeclarationEnquiryEdit, StatusActionMenuItemCaption, new BusinessObjectLoggerOptions("", false, Enterprise.ZArchitecture.Business.Events.Authorised));
				writeToLogMenu.Logged += WriteToLogMenu_Logged;
				StatusNotificationsGrid.ContextMenu.MenuItems.Add(0, writeToLogMenu);
				StatusNotificationsGrid.ColourDeciding += StatusNotificationsGrid_ColourDeciding;
			}
		}

		void WriteToLogMenu_Logged(object sender, LoggedEventArgs e)
		{
			if (e.Succeeded)
			{
				if (DataSource != null)
				{
					if (DataSource is JobDeclaration declaration)
					{
						declaration.ReCalculateENSAction();
					}
					else if (DataSource is ReconDeclaration recon)
					{
						recon.ReconWrappedJobDeclaration.ReCalculateENSAction();
					}
				}
			}
		}

		void StatusNotificationsGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			e.Colour = ((ErrorsRecord)e.ObjectAtRow).IsFurtherActionRequiredButNotActionedYet ? System.Drawing.Color.Red : e.ReadOnlyColour;
		}

		public const string StatusActionMenuItemCaption = "Mark Actions Taken";
		public const string AcknowledgeActionMenuItemCaption = "Acknowledge Comments";
		WriteToLogMenuItem writeToLogMenu;
	}
}
