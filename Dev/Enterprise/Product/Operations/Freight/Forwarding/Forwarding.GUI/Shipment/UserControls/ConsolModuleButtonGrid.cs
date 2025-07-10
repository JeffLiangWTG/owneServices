using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolModuleButtonGrid : ZModuleButtonGrid
	{
		public ConsolModuleButtonGrid()
			: base()
		{
			InitializeComponent();
		}

		protected CommonShipment ParentShipment
		{
			get { return ((ZForm)ParentForm).BusinessEntity as CommonShipment; }
		}

		#region Buttons

		protected override void NewButton_Click(object sender, EventArgs e)
		{
			string errorMessage;
			if (!FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAddNewConsol(out errorMessage, ParentShipment))
			{
				Globals.Message.ShowError(errorMessage);
			}
			else if (ParentShipment.CoLoadMasterShipment != null && !AskUserToContinue(GetMessage_AddConsolToSubShipment()))
			{
				return;
			}
			else
			{
				base.NewButton_Click(sender, e);
			}
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(ParentShipment, null);
			if (!attachRequest.Errors.IsEmpty)
			{
				Globals.Message.ShowError(attachRequest.Errors);
			}
			else if (ParentShipment.CoLoadMasterShipment != null && !AskUserToContinue(GetMessage_AddConsolToSubShipment()))
			{
				return;
			}
			else
			{
				base.AttachButton_Click(sender, e);
			}
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			if (ParentShipment != null && InnerGrid.SelectedElements != null && InnerGrid.SelectedElements.Length > 0)
			{
				var selectedConsols = InnerGrid.SelectedElements.Cast<ForwardingConsol>();
				if (!ShipmentVsConsolGUIMessageHelper.Instance.IsAllowedToDetachConsols(FreightShipmentVsConsolMessageHelper.Instance, ParentShipment, selectedConsols))
				{
					return;
				}
			}

			base.DetachButton_Click(sender, e);
		}

		#endregion

		#region Implementation

		bool AskUserToContinue(string message)
		{
			string caption = Res.GetString("56291dd0-6b79-4457-afee-3836c6de174d", "Shipment Consols");
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK;
		}

		string GetMessage_AddConsolToSubShipment()
		{
			return Res.GetString("3ce0b60f-c9bf-4fe8-ad48-9d1133cdc065",
				"The shipment {0} is a sub-shipment of the master/lead shipment {1}. A new consol will be attached to this shipment only, without attaching to its master.\r\n\r\nIf you would like to attach a new consol to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new consol to the master/lead shipment {1} instead.",
				ParentShipment.JS_UniqueConsignRef,
				ParentShipment.CoLoadMasterShipment.JS_UniqueConsignRef);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new ConsolModuleButtonGridAttacher(ParentShipment, destinationCollection, findBoxList, moduleID);
		}

		protected override IBusiness GetNewBusinessEntity(ZController controller)
		{
			var shipment = DataSource as ForwardingShipment;
			var newConsol = (ForwardingConsol)base.GetNewBusinessEntity(controller);
			ConsolStandardAloneShipmentRelationshipHelper.MakeConsolFromStandaloneShipment(newConsol, shipment);
			return newConsol;
		}

		#region Reqired by issue 00195904. Please clean up when issue 00195904 is closed.

		protected override void Edit(BusinessObject selected, object sender)
		{
			var shipment = DataSource as ForwardingShipment;
			var consol = selected as ForwardingConsol;
			var issueIsPossible = shipment != null && consol != null;

			if (issueIsPossible)
			{
				shipment.HasChangesChanged += ShipmentOrConsolChanged;
				consol.HasChangesChanged += ShipmentOrConsolChanged;
			}

			try
			{
				base.Edit(selected, sender);
			}
			finally
			{
				if (issueIsPossible)
				{
					shipment.HasChangesChanged -= ShipmentOrConsolChanged;
					consol.HasChangesChanged -= ShipmentOrConsolChanged;
				}
			}
		}

		void ShipmentOrConsolChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (DataSource != null && ((IBusinessObjectFactoryInternals)DataSource.Factory).IsProcessingOnAllTransactionsCommitted && e.ObjectJustWasChanged)
			{
				if (!Globals.IsTest)
				{
					ErrorReporter.ReportOnce("HasChangesShouldNotBeSetWhileTransactionIsCommiting_2", "HasChanges should not be set after factory transaction is committed. Inform IL team");
				}
			}
		}

		#endregion

		internal class ConsolModuleButtonGridAttacher : ZRecordAttacher
		{
			public ConsolModuleButtonGridAttacher(CommonShipment shipment, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.shipment = shipment;
			}

			readonly CommonShipment shipment;

			protected override bool CheckAttaching(List<BusinessObject> businessObjectsToAttach)
			{
				bool result = base.CheckAttaching(businessObjectsToAttach);

				if (result)
				{
					result = AttachConsolToShipmentHelper.CheckAttaching(businessObjectsToAttach, shipment);

					// Note: Standalone shipment needs to be added to consol first before default behaviour of adding the shipment to consol in RecordAttacher.
					if (result)
					{
						var consols = businessObjectsToAttach.OfType<CommonConsol>().ToList();
						ConsolStandardAloneShipmentRelationshipHelper.AddStandAloneShipmentToConsols(consols, shipment);
					}
				}

				return result;
			}
		}

		#endregion
	}
}
