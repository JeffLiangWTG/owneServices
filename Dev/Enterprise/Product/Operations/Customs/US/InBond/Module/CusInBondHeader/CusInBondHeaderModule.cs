using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Module
{
	/// <summary>
	/// Module for CusInBondHeaders
	/// </summary>
	class CusInBondHeaderModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CusInBondHeaderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.US.InBond; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusInBondHeaderFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusInBondHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusInBondHeaderFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = null;
			CusInBondHeader inBondHeader = selectedBusinessObject as CusInBondHeader;

			if (inBondHeader != null)
			{
				if (inBondHeader.BH_ParentID.IsValid)
				{
					switch (inBondHeader.BH_ParentTableCode)
					{
						case JobShipmentSchema.Constants.Prefix:
							result = ZControllerFactory.Create(ControllerIDs.Customs.US.InBondPluggedIntoShipment);
							break;
						case JobDeclarationSchema.Constants.Prefix:
							result = ZControllerFactory.Create(ControllerIDs.Customs.US.InBondPluggedIntoDeclaration);
							break;
					}
				}
			}

			if (result == null)
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.US.InBond);
			}

			return result;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportBroker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.USInBond; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode; }
		}

		#region IOperationalActionSupportable

		public OperationalActionSupporter OperationalActionSupporter => new CusInBondHeaderOperationalActionSupporter();

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
				var query = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				var subQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
				subQuery.AddToFilter(CusInBondHeaderSchema.PK, SelectedBusinessObjects.Select(x => x.PK));
				subQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.NotEqual, InBondHeaderTypeList.Codes.DocumentOnly);
				query.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, subQuery, JoinCondition.And);

				var autualFactory = new BusinessObjectFactory();
				var movementHeaders = autualFactory.Load<CusInBondMoveHeader>(query);
				var inBondMenuItemMessageData = new InBondMenuItemMessageData(autualFactory, movementHeaders.Where(x => !x.IsWaitingForResponse).ToList(), messageType);
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
			var query = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			subQuery.AddToFilter(CusInBondHeaderSchema.PK, SelectedBusinessObjects.Select(x => x.PK));
			query.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, subQuery, JoinCondition.And);

			var autualFactory = new BusinessObjectFactory();
			var movementHeaders = autualFactory.Load<CusInBondMoveHeader>(query);
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(autualFactory, movementHeaders.ToList(), messageType);
			ZFormModaliser.ShowDialogAndDispose(new SendInBondMessageMenuItemForm(inBondMenuItemMessageData));
		}
	}
}
