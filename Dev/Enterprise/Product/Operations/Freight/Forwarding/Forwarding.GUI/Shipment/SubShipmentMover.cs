using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class SubShipmentMover
	{
		#region Ctor

		public SubShipmentMover(Form parentForm, BusinessObjectFactory factory = null)
		{
			this.parentForm = parentForm;
			Factory = factory;
		}

		#endregion

		#region Properties

		public BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
			private set
			{
				factory = value;
			}
		}

		BusinessObjectFactory factory;

		readonly Form parentForm;

		public Action BeforeMoveShipmentsAction { get; set; }

		ForwardingShipment SourceShipment { get; set; }

		ForwardingShipment DestinationShipment { get; set; }

		List<ForwardingShipment> ShipmentsToMove { get; set; }

		List<Func<ForwardingShipment, string>> ShipmentChecks
		{
			get
			{
				if (shipmentChecks == null)
				{
					shipmentChecks = new List<Func<ForwardingShipment, string>>();
					shipmentChecks.Add(CheckAllowAttachSubShipmentToMasterShipment);
				}

				return shipmentChecks;
			}
		}
		List<Func<ForwardingShipment, string>> shipmentChecks;

		#endregion

		#region Implementation

		public void Execute(ForwardingShipment sourceShipment, List<ForwardingShipment> shipmentsToMove)
		{
			SourceShipment = sourceShipment;
			ShipmentsToMove = shipmentsToMove;

			if (!PreValidate())
			{
				return;
			}

			if (!WarnIfSubsAttached())
			{
				return;
			}

			ChooseDestinationShipment();
		}

		bool PreValidate()
		{
			string message = string.Empty;

			if (SourceShipment == null)
			{
				message = Res.GetString("17a3baa0-62de-4fda-965e-e5b6602dcba1", "Could not locate source shipment");
			}
			else if (!SourceShipment.IsInDatabase || SourceShipment.HasChanges)
			{
				message = Res.GetString("261891a3-716e-47d9-b39a-47b00e6d91a2", "Please save your changes before you continue");
			}
			else if (SourceShipment.IsStandardHouse)
			{
				message = Res.GetString("bd1288d0-fdad-4d93-a933-549bcdc49e90", "You cannot use this feature with standard shipments");
			}
			else if (ShipmentsToMove == null || ShipmentsToMove.Count == 0)
			{
				message = Res.GetString("7e4844a0-8ff2-4a88-bfd0-2c5fe5d56e8f", "Please select shipments you wish to move");
			}

			if (!String.IsNullOrEmpty(message))
			{
				Globals.Message.ShowError(message);
				return false;
			}
			else
			{
				return true;
			}
		}

		bool WarnIfSubsAttached()
		{
			List<ForwardingShipment> listShipmentsToMoveWithSubsAttached = (from shipment in ShipmentsToMove
																			where shipment.CoLoadShipments.Count > 0
																			select shipment).ToList();

			if (listShipmentsToMoveWithSubsAttached.Count > 0)
			{
				string[] shipmentListForMessage = (from shipment in listShipmentsToMoveWithSubsAttached
												   select Convert.ToString(shipment.JS_UniqueConsignRef, CultureInfo.InvariantCulture)).ToArray();

				string confirmationMessage = Res.GetString("d1a66f0b-3b22-4913-93cb-ab4e602f98eb",
					"The following shipments have sub shipments attached:\r\n{0}\r\nwhich will be moved as well.\r\n\r\nDo you want to continue?",
					String.Join("\r\n", shipmentListForMessage));

				return Globals.Message.Show(confirmationMessage, Res.GetString("f7903cbe-ee61-4210-89ab-f2b156c914d7", "Confirmation"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes;
			}

			return true;
		}

		void ChooseDestinationShipment()
		{
			ChildEditableService.SetState(this.factory, ChildEditableServiceStates.Shipment);
			ModuleShipmentCollection collection = new ModuleShipmentCollection(Factory);
			var query = new ZQuery(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, new[]
				{
					Core.Constants.ShipmentTypes.StandardHouse,
					Core.Constants.ShipmentTypes.HighVolumeLowValue,
					Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy
				});
			query.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, SourceShipment.PK);

			foreach (var shipmentToMove in ShipmentsToMove)
			{
				query.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, shipmentToMove.PK);
			}

			collection.AddRelationshipFilter(query);

			ZFilterGridModule shipmentModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment);

			BizObjectPopupFinder finder = new BizObjectPopupFinder(shipmentModule, collection);
			finder.DecisionProvider = new BizObjectPopupFinder.PopupDecisionProvider(finder, false, false, false);

			finder.SelectedBizObjAction = (selectedShipments) =>
			{
				DestinationShipment = selectedShipments[0] as ForwardingShipment;
				MoveShipments();
			};

			finder.ShowModal(parentForm);
		}

		void MoveShipments()
		{
			ZController shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);

			if (!ReloadShipmentsInNewFactory(shipmentController.Factory))
			{
				return;
			}

			if (!CheckCanMoveShipments())
			{
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("756a2edf-621f-4e9a-9e0a-fe59025796da",
				"You're about to move {0} sub shipment(s) from {1} to {2}.\r\n\r\nYou will see the shipment(s) attached in {2} shipment form, however the changes will be saved only when you save the form.",
				ShipmentsToMove.Count, SourceShipment.JS_UniqueConsignRef, DestinationShipment.JS_UniqueConsignRef));

			if (BeforeMoveShipmentsAction != null)
			{
				BeforeMoveShipmentsAction();
			}

			MoveShipmentsInternal();

			var newShipmentForm = shipmentController.ShowEditForm(DestinationShipment) as ShipmentForm;
			newShipmentForm.ActivateRelatedShipmentsTabPage();
		}

		bool ReloadShipmentsInNewFactory(BusinessObjectFactory newFactory)
		{
			SourceShipment = newFactory.Load<ForwardingShipment>(SourceShipment.PK);
			DestinationShipment = newFactory.Load<ForwardingShipment>(DestinationShipment.PK);
			ShipmentsToMove = new List<ForwardingShipment>(newFactory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, ShipmentsToMove.Select(s => s.PK).ToArray())));

			return SourceShipment != null && DestinationShipment != null && ShipmentsToMove.Count > 0;
		}

		bool CheckCanMoveShipments()
		{
			List<string> allMessages = new List<string>();
			foreach (var shipmentToMove in ShipmentsToMove)
			{
				string message = CheckCanMoveShipment(shipmentToMove);
				if (!String.IsNullOrEmpty(message))
				{
					allMessages.Add(message);
				}
			}

			foreach (var shipmentToMove in ShipmentsToMove)
			{
				var shipmentToCheck = DestinationShipment as CommonShipment;
				while (shipmentToCheck != null)
				{
					if (shipmentToMove.PK == shipmentToCheck.PK)
					{
						allMessages.Add(Res.GetString("ef20c268-9169-4add-adbe-3fbe62de256c",
							"The sub shipment {0} cannot be moved to {1} as it would create a circular reference.",
							shipmentToMove.JS_UniqueConsignRef,
							DestinationShipment.JS_UniqueConsignRef));

						shipmentToCheck = null;
					}
					else
					{
						shipmentToCheck = shipmentToCheck.CoLoadMasterShipment;
					}
				}
			}

			var detachedShipmentJobs = ShipmentsToMove.Where(shipment => shipment.JS_JS_ColoadMasterShipment != SourceShipment.PK).Select(shipment => shipment.JobNumber).ToArray();
			if (detachedShipmentJobs.Any())
			{
				allMessages.Add(Res.GetString("482764DD-84B0-4F43-B4F2-6EA7A4098CB3",
					"The sub shipment {0} was already moved to another shipment. Please reload the form and try again.",
					String.Join(",", detachedShipmentJobs)));
			}

			if (allMessages.Count > 0)
			{
				Globals.Message.ShowError(Res.GetString("88ffd719-8f59-434e-b0fe-870020597da0",
					"Operation failed due to the following reason(s):\r\n{0}",
					allMessages.ToArray()));
				return false;
			}

			return true;
		}

		string CheckCanMoveShipment(ForwardingShipment shipmentToMove)
		{
			string message = string.Empty;
			foreach (var check in ShipmentChecks)
			{
				message = check(shipmentToMove);
				if (!String.IsNullOrEmpty(message))
				{
					break;
				}
			}

			return message;
		}

		string CheckAllowAttachSubShipmentToMasterShipment(ForwardingShipment subShipment)
		{
			string errorMessage = String.Empty;
			FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachSubShipment(out errorMessage, DestinationShipment, subShipment);

			return errorMessage;
		}

		void MoveShipmentsInternal()
		{
			CommonConsol[] consolsToDetach = AskUserToDetachConsols();
			StringBuilder allMessages = new StringBuilder();

			DestinationShipment.IsRoot = true;
			bool haveErrors = false;
			foreach (var shipmentToMove in ShipmentsToMove)
			{
				using (shipmentToMove.GetValidationSuspender())
				{
					SourceShipment.CoLoadShipments.Remove(shipmentToMove.PK);

					if (consolsToDetach != null && consolsToDetach.Any())
					{
						var detachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToDetachConsols(shipmentToMove, consolsToDetach);
						if (!detachRequest.RestrictedMessage.IsEmpty)
						{
							haveErrors = true;
							allMessages.AppendLine(detachRequest.RestrictedMessage);
						}
						else
						{
							FreightShipmentVsConsolMessageHelper.Instance.DetachShipmentsFromConsols(new[] { shipmentToMove }, consolsToDetach);
						}
					}

					DestinationShipment.CoLoadShipments.Add(shipmentToMove);
				}
				shipmentToMove.Validation.ValidateJS_JS_ColoadMasterShipment();
			}

			if (haveErrors)
			{
				string caption = Res.GetString("16ea75c1-a3a9-4d5b-9e4c-e9116b41d199", "Consols to detach");
				Globals.Message.ShowInformation(allMessages.ToString(), caption);
			}
		}

		CommonConsol[] AskUserToDetachConsols()
		{
			string message;
			CommonConsol[] consolsToDetach = FreightShipmentVsConsolMessageHelper.Instance.GetConsolsToDetachFromSubShipments(out message, null, SourceShipment, DestinationShipment, ShipmentsToMove.Cast<CommonShipment>().ToList());
			if (consolsToDetach.Any())
			{
				string caption = Res.GetString("16ea75c1-a3a9-4d5b-9e4c-e9116b41d199", "Consols to detach");
				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.No)
				{
					consolsToDetach = Array.Empty<CommonConsol>();
				}
			}

			return consolsToDetach;
		}

		#endregion
	}
}
