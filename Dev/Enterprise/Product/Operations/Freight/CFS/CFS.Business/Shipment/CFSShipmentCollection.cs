using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	[ModuleID(ModuleId.ShipmentReceival)]
	public class CFSShipmentCollection : ConsolShipmentCollection
	{
		public CFSShipmentCollection(CFSLoadListConsol parent)
			: base(parent)
		{
			Sort(CFSShipment.Schema.JS_InterimReceipt, ListSortDirection.Descending);
		}

		protected new CFSLoadListConsol ParentConsol
		{
			get { return (CFSLoadListConsol)base.ParentConsol; }
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var shipmentChild = (CFSShipment)child;
			using (shipmentChild.SuppressForwarderCalculation())
			{
				SetValuesFromParent(shipmentChild);
			}
		}

		public void SetValuesFromParent()
		{
			if (ParentConsol != null)
			{
				foreach (CFSShipment shipment in this)
				{
					SetValuesFromParent(shipment);
				}
			}
		}

		void SetValuesFromParent(CFSShipment shipment)
		{
			if (ParentConsol != null)
			{
				if (shipment.JS_JS_ColoadMasterShipment.IsEmpty && shipment.JS_OH_HandledOnBehalfOfForwarder == ParentConsol.OldForwarder)
				{
					shipment.JS_OH_HandledOnBehalfOfForwarder = ParentConsol.JK_OH_Forwarder;
				}

				if (shipment.Consols.Count == 1 && !ParentConsol.JK_AgentsReference.IsEmpty && shipment.JS_ConsolReference.IsEmpty)
				{
					shipment.JS_ConsolReference = ParentConsol.JK_AgentsReference.SubstringSafe(0, JobShipmentSchema.JS_ConsolReference.MaxLength);
				}
			}
		}

		#endregion

		#region Add

		public override void Add(BusinessObject businessObject)
		{
			var cfsShipment = businessObject as CFSShipment;

			if (cfsShipment != null)
			{
				if (ParentConsol != null && !IsLoading && !shipmentsBeingAdded.Contains(cfsShipment))
				{
					try
					{
						shipmentsBeingAdded.Add(cfsShipment);

						if (cfsShipment.Consols.Count == 0)
						{
							SetValuesFromParent(cfsShipment);
						}

						CheckIfShipmentCanBeAdded(cfsShipment);
						if (!ParentConsol.ContinueWithChanging)
						{
							if (ParentConsol.IsPackLoadList && cfsShipment.IsExport())
							{
								cfsShipment.JS_TranshipToOtherCFS = false;
							}

							return;
						}
						ParentConsol.ContinueWithChanging = false;

						if (cfsShipment.JS_IsBooking && cfsShipment.JS_IsCFSRegistered && !cfsShipment.JS_IsForwardRegistered)
						{
							CFSBuildConsolHelper helper = new CFSBuildConsolHelper();
							helper.AddBookingToCFSLoadList(ParentConsol, cfsShipment.PK);
							return;
						}
					}
					finally
					{
						shipmentsBeingAdded.Remove(cfsShipment);
					}
				}

				base.Add(businessObject);
			}
		}

		readonly List<CFSShipment> shipmentsBeingAdded = new List<CFSShipment>();

		void CheckIfShipmentCanBeAdded(CFSShipment shipment)
		{
			ParentConsol.ContinueWithChanging = true;

			if (((ISupportDataImporting)ParentConsol).IsImportingData)
			{
				return;
			}

			if (shipment.IsAtLeastPartiallyDelivered && shipment.IsExport() && shipment.JS_TranshipToOtherCFS && ParentConsol.IsPackLoadList)
			{
				ParentConsol.MessageCaption = Res.GetString("8f99fe1b-48ef-47da-a865-a82771edb122", "Attach Shipment");
				ParentConsol.ErrorMessage = Res.GetString("3cbf0894-79c0-4ffd-a419-a302229ef2de", "This shipment has already been gate passed - you cannot export it.");
				ParentConsol.WarningMessage = "";
				ParentConsol.ContinueWithChanging = false;
			}
			else if (shipment.Consols.Count == 0 && shipment.JS_JX.IsValid && shipment.JS_JX != ParentConsol.JK_JX_Sailing)
			{
				ParentConsol.MessageCaption = Res.GetString("8f99fe1b-48ef-47da-a865-a82771edb122", "Attach Shipment");
				ParentConsol.WarningMessage = Res.GetString("47719ecd-6123-439f-8433-d3225bc4084d", "Shipment sailing details are not the same as the Load List Sailing Details.\r\nIf you attach the shipment the Load List sailing details will be defaulted to the shipment.\r\nProceed with shipment attachment?");
				ParentConsol.ErrorMessage = "";
			}
			else if (shipment.Consols.Count > 0)
			{
				foreach (CFSLoadListConsol loadList in shipment.Consols)
				{
					if (loadList.PK != ParentConsol.PK && loadList.JK_JX_JA_RL_NKPortOfLoading == ParentConsol.JK_JX_JA_RL_NKPortOfLoading && loadList.JK_JX_JB_RL_NKPortOfDischarge == ParentConsol.JK_JX_JB_RL_NKPortOfDischarge)
					{
						ParentConsol.MessageCaption = Res.GetString("8f99fe1b-48ef-47da-a865-a82771edb122", "Attach Shipment");
						ParentConsol.ErrorMessage = Res.GetString("4d16c015-1411-4a90-89a8-363b0d1b6d5c", "A shipment cannot be linked to multiple Load Lists with the same port of loading and discharge.");
						ParentConsol.WarningMessage = "";
						ParentConsol.ContinueWithChanging = false;
						break;
					}
				}
			}
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			if (ParentConsol != null)
			{
				bool isPackedShipment = false;

				CFSShipment shipment = (CFSShipment)elementToRemove;
				foreach (CFSPackLine line in shipment.OuterPackLines)
				{
					if (line.GetContainer(ParentConsol) != null)
					{
						isPackedShipment = true;
						break;
					}
				}

				if (isPackedShipment)
				{
					ParentConsol.ContinueWithChanging = true;
					ParentConsol.MessageCaption = Res.GetString("d3ecb992-a767-4411-adb4-8fba080e5aaa", "Detach Shipment");
					ParentConsol.WarningMessage = Res.GetString("a927e868-7849-4b99-be62-908d3478ad5f", "This shipment has been packed into a container.\r\nDetaching the shipment will unpack the shipment from the container.\r\nDo you want to proceed with Detach?");
					ParentConsol.WarningMessage = "";
					if (!ParentConsol.ContinueWithChanging)
					{
						return;
					}
					ParentConsol.ContinueWithChanging = false;
				}

				foreach (CFSPackLine line in shipment.OuterPackLines.ToArray())
				{
					foreach (CFSContainer container in ParentConsol.Containers.ToArray())
					{
						if (container.PackLines.Contains(line.PK))
						{
							container.PackLines.Remove(line);
							break;
						}
					}
				}
			}

			base.Remove(elementToRemove);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (ParentConsol != null && !IsLoading)
			{
				CFSShipment shipment = (CFSShipment)bizO;

				if (shipment.Consols.Contains(ParentConsol.PK))
				{
					shipment.Consols.Remove(ParentConsol);
				}
			}
		}

		#endregion

		#region Implementation

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, ZBool.True);
			return result;
		}

		public new CFSShipment this[int index]
		{
			get { return (CFSShipment)Elements[index]; }
		}

		public new CFSShipment AddNew()
		{
			return (CFSShipment)base.AddNew();
		}

		#endregion
	}
}
