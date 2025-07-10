using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed partial class ContainerManagerMovementsControl : ZUserControl
	{
		public ContainerManagerMovementsControl()
		{
			InitializeComponent();
		}

		#region Operations

		public void PerformAttachClick()
		{
			attachButton.PerformClick();
		}

		public void PerformDetachClick()
		{
			detachButton.PerformClick();
		}

		public void PerformDefaultFromBookingRefClick()
		{
			defaultFromBookingRefButton.PerformClick();
		}

		public void PerformDefaultFromBolClick()
		{
			defaultFromBolButton.PerformClick();
		}

		#endregion

		#region Implementation

		void DoAttach()
		{
			string dialogCaption = Res.GetString("8B4ED11C-622D-4696-A25B-14301E92FD4F", "Attach");

			ContainerMovement[] movements;

			if (!Stock.IsInDatabase)
			{
				Globals.Message.Show(NotSavedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if ((movements = movementsGrid.GetSelectedElements<ContainerMovement>()).Length == 0)
			{
				Globals.Message.Show(NoMovementsSelectedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Array.TrueForAll(movements, (m) => m.E9_JV.IsEmpty))
			{
				string messageText = Res.GetString("d327cfc1-3516-4a72-8b8e-db4ec62e333a", "At least one selected movement is already attached.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Array.TrueForAll(movements, (m) => m.E9_NC.IsEmpty))
			{
				string messageText = Res.GetString("0d57b0f8-4072-4762-9790-83cdb4a178f1", "Cannot attach movements that are attached to a detention job.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else
			{
				VoyageIFindBox.SelectSeaVoyage((ZForm)FindForm(), Stock.Factory, delegate(ZGuid voyagePK)
				{
					foreach (ContainerMovement movement in movements)
					{
						movement.E9_JV = voyagePK;
					}
				});
			}
		}

		void DoDetach()
		{
			string dialogCaption = Res.GetString("323aeaf0-eb80-4d53-b17b-e05901135691", "Detach");

			ContainerMovement[] movements;

			if (!Stock.IsInDatabase)
			{
				Globals.Message.Show(NotSavedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if ((movements = movementsGrid.GetSelectedElements<ContainerMovement>()).Length == 0)
			{
				Globals.Message.Show(NoMovementsSelectedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Array.TrueForAll(movements, (m) => m.E9_NC.IsEmpty))
			{
				string messageText = Res.GetString("5f4712b2-72fe-478b-9092-47a8bafc1cf9", "Can't detach movements with a detention job attached.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (DialogResult.Yes == Globals.Message.Show(
				Res.GetString("4e2387ca-b5ab-4f5c-b2dd-71e85424653d", "Detach {0} movements?", movements.Length),
				dialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes))
			{
				foreach (ContainerMovement movement in movements)
				{
					movement.E9_JV = ZGuid.Empty;
				}
			}
		}

		void DoDefaulting(Converter<string, ZQuery> filterDelegate)
		{
			string dialogCaption = Res.GetString("60645117-5577-4B2D-A119-7AF02E76C88C", "Responsible Party and Principal Defaulting");

			ContainerMovement[] movements;
			BillOfLading[] shipments;

			if (Stock.Filter.ReferenceNumber.IsEmpty)
			{
				string messageText = Res.GetString("2caa8ae9-d85c-415a-8f61-0dbe9340b69f", "No reference number entered.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Stock.IsInDatabase)
			{
				Globals.Message.Show(NotSavedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if ((movements = movementsGrid.GetSelectedElements<ContainerMovement>()).Length == 0)
			{
				Globals.Message.Show(NoMovementsSelectedMsg, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if ((shipments = Stock.Factory.Load<BillOfLading>(filterDelegate(Stock.Filter.ReferenceNumber))).Length == 0)
			{
				string messageText = Res.GetString("e92e5669-cef0-4ec6-b9b1-53ee14671e71", "No matching bills/bookings were found.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (!Array.TrueForAll(movements, (m) => m.E9_NC.IsEmpty))
			{
				string messageText = Res.GetString("1fecbd00-6f9e-41c8-964f-b558296a07a1", "Cannot default movements that are attached to a detention job.");
				Globals.Message.Show(messageText, dialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, DialogResult.OK);
			}
			else if (DialogResult.Yes == Globals.Message.Show(
				Res.GetString("c1f88f13-8657-47e5-b2c7-6191a3d96e81", "Default the principal and responsible party overrides for {0} movements from {1}.", movements.Length, shipments[0].JS_UniqueConsignRef),
				dialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes))
			{
				AgencyShipment shipment = shipments[0];
				ZGuid principalPK = GetPrincipalPK(shipment);
				ZGuid clientPK = GetClientPK(shipment);

				foreach (ContainerMovement movement in movements)
				{
					if (!principalPK.IsEmpty)
					{
						movement.E9_OH_Principal = principalPK;
					}

					if (!clientPK.IsEmpty)
					{
						movement.E9_OH_ResponsibleParty = clientPK;
					}
				}
			}
		}

		ZGuid GetPrincipalPK(AgencyShipment shipment)
		{
			return shipment == null ? ZGuid.Empty : shipment.JS_OH_DeliveryAgent;
		}

		ZGuid GetClientPK(AgencyShipment shipment)
		{
			JobHeader header = new JobHeader.Loader(shipment).Load();
			return header == null ? ZGuid.Empty : header.LocalChargesPK;
		}

		string NotSavedMsg
		{
			get { return Res.GetString("76109419-4ace-46d0-b2ab-4b0485a18a01", "This container has not yet been saved."); }
		}

		string NoMovementsSelectedMsg
		{
			get { return Res.GetString("534bbb46-3d7b-44f6-8801-63c72c9b182c", "No movements selected."); }
		}

		RefContainerStock Stock
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RefContainerStock)base.CurrentDataItem; }
		}

		#endregion

		#region Events

		void resetFiltersButton_Click(object sender, EventArgs e)
		{
			Stock.Filter.Reset();
		}

		void findButton_Click(object sender, EventArgs e)
		{
			IBusiness filteredMovements = Stock.Filter.Movements;

			if (filteredMovements.HasChanges)
			{
				Globals.Message.Show(Res.GetString("11770b65-0c8d-4230-8db6-3bd4a0b32c50", "Some movements have changes, please save and try again."), Res.GetString("30d524a6-4992-4aae-980f-28a74fca2adb", "Cannot Find"), MessageBoxButtons.OK, DialogResult.OK);
			}
			else
			{
				Stock.Filter.Find();
			}
		}

		void defaultFromBookingRefButton_Click(object sender, EventArgs e)
		{
			DoDefaulting((refNum) =>
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				query.AddToFilter(JobShipmentSchema.JS_CFSReference, refNum);
				return query;
			});
		}

		void defaultFromBolButton_Click(object sender, EventArgs e)
		{
			DoDefaulting((refNum) =>
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				query.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
				query.AddToFilter(JobShipmentSchema.JS_HouseBill, refNum);
				return query;
			});
		}

		void attachButton_Click(object sender, EventArgs e)
		{
			DoAttach();
		}

		void detachButton_Click(object sender, EventArgs e)
		{
			DoDetach();
		}

		#endregion
	}
}


