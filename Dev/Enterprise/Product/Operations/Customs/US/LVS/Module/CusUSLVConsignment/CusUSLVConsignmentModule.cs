using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVConsignmentModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CusUSLVConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (IsEmbeddedControlConstructed && !EmbeddedControl.IsDisposed)
				{
					if (ViewMenuItem != null)
					{
						ViewMenuItem.Popup -= ViewMenuItem_Popup;
					}
					if (EditMenuItem != null)
					{
						EditMenuItem.Popup -= EditMenuItem_Popup;
					}
				}
			}
			base.Dispose(isDisposing);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject is USConsignmentCombined consignmentCombined && consignmentCombined.IsDeclaration)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntriesDeclaration);
			}

			return ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntriesBill);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<USConsignmentCombined>(Factory);

		protected override IFilterControl GetNewFilterControl() => new CusUSLVConsignmentFilterControl(GridCollection, (USConsignmentCombinedFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USConsignmentCombinedFilterBusinessObject();

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			if (ViewMenuItem != null)
			{
				ViewMenuItem.MenuItems.Add(viewBillCaption, HandleViewBillClick);
				viewLowValueEntriesMenuItem = ViewMenuItem.MenuItems.Add(viewLowValueEntriesCaption, HandleViewLowValueEntriesClick);

				ViewMenuItem.Popup -= ViewMenuItem_Popup;
				ViewMenuItem.Popup += ViewMenuItem_Popup;
			}

			if (EditMenuItem != null)
			{
				EditMenuItem.MenuItems.Add(editBillCaption, HandleEditBillClick);
				editLowValueEntriesMenuItem = EditMenuItem.MenuItems.Add(editLowValueEntriesCaption, HandleEditLowValueEntriesClick);

				EditMenuItem.Popup -= EditMenuItem_Popup;
				EditMenuItem.Popup += EditMenuItem_Popup;
			}

			var sendOriginalMessage = new ZMenuItem(sendOriginalMessageCaption);
			sendOriginalMessageConsignments = new ZMenuItem(sendOriginalMessageForConsignmentsCaption, HandleSendOriginalMessagesForConsignmentsClick);
			sendOriginalMessageLowValueEntries = new ZMenuItem(sendOriginalMessageForLowValueEntriesCaption, HandleSendOriginalMessageForLowValueEntriesClick);
			sendOriginalMessage.MenuItems.Add(sendOriginalMessageConsignments);
			sendOriginalMessage.MenuItems.Add(sendOriginalMessageLowValueEntries);

			sendOriginalMessage.Popup -= SendOriginalMessage_Popup;
			sendOriginalMessage.Popup += SendOriginalMessage_Popup;

			menuItems.Add(sendOriginalMessage);

			menuItems.Add(new ZMenuItem(convertPartyAddressesToNewOrganizationsCaption, HandleConvertPartyAddressesToNewOrgClick));

			menuItems.Add(ConvertToStandAloneDeclarationHelper.CreateConvertToStandAloneDeclarationMenuItem((_, __) =>
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				{
					var bizosToReload = SelectedBusinessObjects;
					ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(Factory, SelectedBusinessObjects, null, true);
					bizosToReload.ForEach(bizo => bizo.ReloadSafe());
				}
			}));

			return menuItems.ToArray();
		}

		MenuItem viewLowValueEntriesMenuItem;
		MenuItem editLowValueEntriesMenuItem;
		MenuItem sendOriginalMessageConsignments;
		MenuItem sendOriginalMessageLowValueEntries;

		void ViewMenuItem_Popup(object sender, EventArgs e)
		{
			if (viewLowValueEntriesMenuItem != null)
			{
				UpdateLowValueEntriesMenuItemVisibility(viewLowValueEntriesMenuItem);
			}
		}

		void EditMenuItem_Popup(object sender, EventArgs e)
		{
			if (editLowValueEntriesMenuItem != null)
			{
				UpdateLowValueEntriesMenuItemVisibility(editLowValueEntriesMenuItem);
			}
		}

		void SendOriginalMessage_Popup(object sender, EventArgs e)
		{
			UpdateSendOriginalMessageLowValueEntries();
		}

		void UpdateLowValueEntriesMenuItemVisibility(MenuItem lowValueEntriesMenuItem)
		{
			if (lowValueEntriesMenuItem != null)
			{
				if (SelectedBusinessObjects.Any(x => x is USConsignmentCombined view && view.UBV_JobType == USConsignmentCombinedJobTypes.Codes.Consignment))
				{
					lowValueEntriesMenuItem.Visible = true;
				}
				else
				{
					lowValueEntriesMenuItem.Visible = false;
				}
			}
		}

		void UpdateSendOriginalMessageLowValueEntries()
		{
			sendOriginalMessageLowValueEntries.Visible = Grid.SelectedElements.Length == 1;
		}

		readonly ZString largeNumberConsignmentsMessage = Res.GetString("8755e12f-ddf8-4e62-9b15-5ab5f4cc51c0", "You have selected a large number of Consignments, do you want to proceed?");
		readonly ZString largeNumberLowValueEntriesMessage = Res.GetString("128fc46c-b8ed-4a61-952a-fa2e2f5c8f51", "You have selected a large number of Low Value Entries jobs, do you want to proceed?");
		readonly ZString viewBillCaption = Res.GetString("d7bed3f9-422b-4fd4-a7f8-f757f448152b", "View Bill");
		readonly ZString viewLowValueEntriesCaption = Res.GetString("83312dc5-6213-44ef-8caf-06ec1a6e5039", "View Low Value Entries");
		readonly ZString editBillCaption = Res.GetString("64d14e81-3c5b-4857-b692-7d73a585d232", "Edit Bill");
		readonly ZString editLowValueEntriesCaption = Res.GetString("a1a5d86b-69e2-4bc0-b8c2-3ef18eee8310", "Edit Low Value Entries");
		readonly ZString sendOriginalMessageCaption = Res.GetString("44f1e071-ce68-4f59-bed9-3f0ab2fdb60b", "Send Original Messages");
		readonly ZString sendOriginalMessageForConsignmentsCaption = Res.GetString("8e71a162-ecf9-4f74-a4f9-51367b22f98a", "Consignments");
		readonly ZString sendOriginalMessageForLowValueEntriesCaption = Res.GetString("f47a8cd3-70c0-4ebe-aa58-3caf83e5af08", "Entire Low Value Entries");
		readonly ZString invalidElementForMessaging = Res.GetString("919597E1-3C8B-48F5-9AAF-4EB065BB0A25", "Invalid Element");
		readonly ZString convertPartyAddressesToNewOrganizationsCaption = Res.GetString("4f48a3ef-5d12-4c64-981b-98d9709098e3", "Convert Party Addresses to Organizations");

		void HandleViewBillClick(object sender, EventArgs eventArgs)
		{
			var selectedBills = SelectedBusinessObjects.OfType<USConsignmentCombined>();
			if (selectedBills.Count() > 10)
			{
				var dialogResult = Globals.Message.Show(largeNumberConsignmentsMessage, viewBillCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}

			ShowForms(selectedBills.ToArray(), true);
		}

		void HandleViewLowValueEntriesClick(object sender, EventArgs eventArgs)
		{
			var selectedLowValueEntriesJobs = SelectedBusinessObjects.OfType<USConsignmentCombined>()
				.Where(row => row.IsConsignment)
				.Select(row => row.Consignment.Shipment)
				.Distinct();
			if (selectedLowValueEntriesJobs.Count() > 10)
			{
				var dialogResult = Globals.Message.Show(largeNumberLowValueEntriesMessage, viewLowValueEntriesCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}

			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntries);
			foreach (var clearance in selectedLowValueEntriesJobs)
			{
				controller.ShowViewForm(clearance);
			}
		}

		void HandleEditBillClick(object sender, EventArgs eventArgs)
		{
			var selectedBills = SelectedBusinessObjects.OfType<USConsignmentCombined>();
			if (selectedBills.Count() > 10)
			{
				var dialogResult = Globals.Message.Show(largeNumberConsignmentsMessage, editBillCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}
			ShowForms(selectedBills.ToArray(), false);
		}

		void HandleEditLowValueEntriesClick(object sender, EventArgs eventArgs)
		{
			var selectedLowValueEntriesJobs = SelectedBusinessObjects.OfType<USConsignmentCombined>()
				.Where(row => row.IsConsignment)
				.Select(row => row.Consignment.Shipment)
				.Distinct();
			if (selectedLowValueEntriesJobs.Count() > 10)
			{
				var dialogResult = Globals.Message.Show(largeNumberLowValueEntriesMessage, editLowValueEntriesCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}

			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntries);
			foreach (var clearance in selectedLowValueEntriesJobs)
			{
				controller.ShowEditForm(clearance);
			}
		}

		void HandleSendOriginalMessagesForConsignmentsClick(object sender, EventArgs eventArgs)
		{
			if (CustomsSendMessageHelper.USLVClearanceImportMessagingIsAllowed())
			{
				var billsSelected = SelectedBusinessObjects.OfType<USConsignmentCombined>()
					.Where(row => row.IsConsignment)
					.Select(row => row.Consignment);
				var totalBills = billsSelected.Count();

				CustomsSendMessageHelper.InitMessagingAction(billsSelected, UpdateActionCode.Add);
				var billsToSend = CustomsSendMessageHelper.PrepareMessagesToSend(billsSelected, UpdateActionCode.Add);
				var billsActuallySent = SendToCustoms(billsToSend);
				Globals.Message.Show(Res.GetString("57f05743-e7fa-40d5-b067-97484d3af60c", "{0} original message(s) generated; {1} message(s) ignored.", billsActuallySent, totalBills - billsActuallySent), sendOriginalMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void HandleSendOriginalMessageForLowValueEntriesClick(object sender, EventArgs eventArgs)
		{
			var clearance = SelectedBusinessObjects.OfType<USConsignmentCombined>().Where(row => row.IsConsignment).Select(row => row.Consignment).FirstOrDefault()?.Shipment;
			if (clearance != null)
			{
				var billsSelected = clearance.CusUSLVConsignments.Cast<CusUSLVConsignment>().ToList();
				var totalBills = billsSelected.Count;

				CustomsSendMessageHelper.InitMessagingAction(billsSelected, UpdateActionCode.Add);
				var billsToSend = CustomsSendMessageHelper.PrepareMessagesToSend(billsSelected, UpdateActionCode.Add);
				var billsActuallySent = SendToCustoms(billsToSend);
				Globals.Message.Show(Res.GetString("57f05743-e7fa-40d5-b067-97484d3af60c", "{0} original message(s) generated; {1} message(s) ignored.", billsActuallySent, totalBills - billsActuallySent), sendOriginalMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				Globals.Message.Show(Res.GetString("43537041-AB21-4DCE-B530-C0163F5391E3", "This bill is from a Customs Declaration.\nPlease select a bill that belongs to Low Value Entries."), invalidElementForMessaging, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void HandleConvertPartyAddressesToNewOrgClick(object sender, EventArgs eventArgs)
		{
			var consignments = SelectedBusinessObjects.OfType<USConsignmentCombined>().ToList();
			if (!consignments.IsNullOrEmpty() && consignments.Any(c => c.IsConsignment && (!c.Consignment.ConsigneeIsOrganisation || !c.Consignment.ShipperIsOrganisation)))
			{
				var newFactoryInCaseUserCancelsConversion = new BusinessObjectFactory();
				var conversion = new FreeTextAddressConversion<CusUSLVConsignment>(newFactoryInCaseUserCancelsConversion);
				conversion.AddToList(consignments.Where(c => c.IsConsignment).Select(c => c.Consignment));

				using (var freeTextAddressConversionForm = new FreeTextAddressConversionForm<CusUSLVConsignment>(conversion, ignoreButtonVisible: false, freeTextAddressGridLayoutProvider: new FreeTextAddressGridLayoutProvider()))
				{
					ZFormModaliser.ShowDialogWithoutDispose(freeTextAddressConversionForm);
					if (freeTextAddressConversionForm.LastSaveSucceeded)
					{
						Globals.Message.Show(Res.GetString("97337af1-64c2-4967-88fc-0a1921cc6a8c", "New organization(s) have been linked to the selected consignments."));
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("bc4312e6-9bd4-4ad1-84f8-9ae024e6800f", "All consignments have linked to organizations"));
			}
		}

		int SendToCustoms(IEnumerable<CusUSLVConsignmentForMessaging> billsToSend)
		{
			var billsGroupedByClearance = new Dictionary<ZGuid, List<CusUSLVConsignmentForMessaging>>();
			foreach (var bill in billsToSend)
			{
				var currentClearancePK = bill.Consignment.Shipment.PK;
				if (!billsGroupedByClearance.ContainsKey(currentClearancePK))
				{
					billsGroupedByClearance.Add(currentClearancePK, new List<CusUSLVConsignmentForMessaging>());
				}
				billsGroupedByClearance[currentClearancePK].Add(bill);
			}

			var billsActuallySent = 0;
			foreach (var pk in billsGroupedByClearance.Keys)
			{
				var clearance = billsGroupedByClearance[pk][0].Consignment.Shipment;
				var reasonForUnableToAllocateEntryNumber = CustomsSendMessageHelper.AllocateEntryNumbers(clearance, billsGroupedByClearance[pk]);
				if (reasonForUnableToAllocateEntryNumber.IsEmpty)
				{
					billsActuallySent += CustomsSendMessageHelper.SendToCustoms(clearance, billsGroupedByClearance[pk], UpdateActionCode.Add);
				}
			}

			return billsActuallySent;
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USLowValueEntriesBill;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USLVConsignment;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.CoreCustomsModule;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public OperationalActionSupporter OperationalActionSupporter => new CusUSLVConsignmentOperationalActionSupporter();

		#region Module Decision Provider

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new USConsignmentCombinedModuleDecisionProvider(this);

		protected class USConsignmentCombinedModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public USConsignmentCombinedModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}

		#endregion
	}
}
