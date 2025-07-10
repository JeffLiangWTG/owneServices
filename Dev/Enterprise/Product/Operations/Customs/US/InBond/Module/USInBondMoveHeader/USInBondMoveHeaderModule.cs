using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Module
{
	public class USInBondMoveHeaderModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public USInBondMoveHeaderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.InBondMoveHeader;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USInBond;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.US.InBondMoveHeader);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new USInBondMoveHeaderFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new USInBondMoveHeaderFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new USInBondMoveHeaderCollection(Factory);
		}

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => new USInBondMoveHeaderOperationalActionSupporter();

		#endregion

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var bulkSendMessagesMenuItem = new ZMenuItem("Bulk Send Messages");
			bulkSendMessagesMenuItem.MenuItems.Add(new ZMenuItem("Send Arrival Messages", HandleSendArrivalMessagesClick));
			bulkSendMessagesMenuItem.MenuItems.Add(new ZMenuItem("Send Export Messages", HandleSendExportationMessagesClick));

			var allocatePedimentoNumberMenuItem = new ZMenuItem("Allocate Pedimento Number", HandleAllocatePedimentoNumberClick);
			var bulkPrintDocumentMenuItem = new ZMenuItem("Bulk Print 7512 Departure Document", HandleBulkPrintDocumentNumberClick);

			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(bulkSendMessagesMenuItem);
			menuItems.Add(allocatePedimentoNumberMenuItem);
			menuItems.Add(bulkPrintDocumentMenuItem);
			return menuItems.ToArray();
		}

		void HandleSendArrivalMessagesClick(object sender, EventArgs eventArgs)
		{
			HandleBulkSendMessagesClick(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
		}

		void HandleSendExportationMessagesClick(object sender, EventArgs eventArgs)
		{
			HandleBulkSendMessagesClick(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
		}

		void HandleBulkSendMessagesClick(InBondMenuItemMessageData.InBondMenuItemMessageTypes messageType)
		{
			if (!Env.Security.USInBondMessaging.IsAllowed)
			{
				Env.Security.USInBondMessaging.ShowError();
			}
			else
			{
				var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, SelectedBusinessObjects.OfType<USInBondMoveHeader>().Select(s => s.MoveHeader).Where(x => !x.Header.IsDocumentOnly && !x.IsWaitingForResponse).ToList(), messageType);
				ZFormModaliser.ShowDialogAndDispose(new SendInBondMessageMenuItemForm(inBondMenuItemMessageData));
			}
		}

		void HandleAllocatePedimentoNumberClick(object sender, EventArgs eventArgs)
		{
			if (Env.Security.USInBondEdit.IsAllowed)
			{
				ClickedToShowSendInBondMessageForm(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			}
			else
			{
				Env.Security.USInBondEdit.ShowError();
			}
		}

		void HandleBulkPrintDocumentNumberClick(object sender, EventArgs eventArgs)
		{
			ClickedToShowSendInBondMessageForm(InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument);
		}

		void ClickedToShowSendInBondMessageForm(InBondMenuItemMessageData.InBondMenuItemMessageTypes messageType)
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, SelectedBusinessObjects.OfType<USInBondMoveHeader>().Select(s => s.MoveHeader).ToList(), messageType);
			ZFormModaliser.ShowDialogAndDispose(new SendInBondMessageMenuItemForm(inBondMenuItemMessageData));
		}
	}
}
