using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	[ModuleID(ModuleId.LoadListConsol)]
	public class CFSLoadListConsolManyToManyCollection : ConsolCollection
	{
		public CFSLoadListConsolManyToManyCollection(CFSShipment shipment)
			: base(shipment)
		{
			ParentShipment = shipment;
		}

		public new CFSLoadListConsol this[int index]
		{
			get { return (CFSLoadListConsol)(Elements[index]); }
		}

		public new CFSLoadListConsol AddNew()
		{
			return (CFSLoadListConsol)base.AddNew();
		}

		public new CFSLoadListConsol GetEarliestConsol()
		{
			return base.GetEarliestConsol() as CFSLoadListConsol;
		}

		public new CFSLoadListConsol GetLatestConsol()
		{
			return base.GetLatestConsol() as CFSLoadListConsol;
		}

		#region Overrides

		public override void Add(BusinessObject bizO)
		{
			CFSLoadListConsol loadList = bizO as CFSLoadListConsol;

			if (loadList == null)
			{
				if (bizO is CommonConsol)
				{
					loadList = Factory.Load<CFSLoadListConsol>(bizO.PK);
				}
				else
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture, "You cannot add a '{0}' business object to CFSLoadListConsolManyToManyCollection.",
						(bizO == null) ? "null" : bizO.GetType().FullName));
				}
			}

			if (loadList != null && ParentShipment != null && !IsLoading)
			{
				if (!IsUpdatingByDataRefreshBus
					&& ParentShipment.Consols.Count == 0
					&& ParentShipment.JS_JS_ColoadMasterShipment.IsEmpty
					&& ParentShipment.JS_OH_HandledOnBehalfOfForwarder.IsEmpty)
				{
					ParentShipment.JS_OH_HandledOnBehalfOfForwarder = loadList.JK_OH_Forwarder;
				}

				CheckIfLoadListCanBeAdded(loadList);
				if (!ParentShipment.ContinueWithChanging)
				{
					if (loadList.IsPackLoadList && ParentShipment.IsExport())
					{
						ParentShipment.JS_TranshipToOtherCFS = false;
					}

					return;
				}
				ParentShipment.ContinueWithChanging = false;
			}

			base.Add(bizO);

			if (loadList != null && ParentShipment != null && ParentShipment.Consols.Count == 1 && ParentShipment.JS_ConsolReference.IsEmpty && !loadList.JK_AgentsReference.IsEmpty)
			{
				ParentShipment.JS_ConsolReference = loadList.JK_AgentsReference.SubstringSafe(0, JobShipmentSchema.JS_ConsolReference.MaxLength);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (ParentShipment != null && !IsLoading)
			{
				CFSLoadListConsol loadList = (CFSLoadListConsol)bizOAdded;
				if (!loadList.Shipments.Contains(ParentShipment.PK))
				{
					loadList.Shipments.Add(ParentShipment);
				}
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			CFSLoadListConsol loadList = (CFSLoadListConsol)elementToRemove;

			if (ParentShipment != null)
			{
				bool isPackedShipment = false;

				foreach (CFSPackLine line in ParentShipment.OuterPackLines)
				{
					if (line.GetContainer(loadList) != null)
					{
						isPackedShipment = true;
						break;
					}
				}

				if (isPackedShipment)
				{
					ParentShipment.ContinueWithChanging = true;
					ParentShipment.MessageCaption = Res.GetString("8977835e-0930-46b1-9d09-2a6108115396", "Detach Load List");
					ParentShipment.WarningMessage = Res.GetString("2a921fc8-d254-453e-ae4e-73c9d746d556", @"This Load List has a container into which this shipment has been packed.
Detaching the Load List will unpack the shipment from the container.
Do you want to proceed with Detach?");
					ParentShipment.WarningMessage = "";
					if (!ParentShipment.ContinueWithChanging)
					{
						return;
					}
					ParentShipment.ContinueWithChanging = false;
				}
			}

			base.Remove(elementToRemove);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = base.CreateAdditionalFilter();
			query.AddToFilter(JobConsolSchema.JK_IsCFS, ZBool.True);
			return query;
		}

		#endregion

		#region Implementation

		new readonly CFSShipment ParentShipment;

		#region CheckIfLoadListCanBeAdded

		void CheckIfLoadListCanBeAdded(CFSLoadListConsol loadList)
		{
			ParentShipment.ContinueWithChanging = true;

			if (ParentShipment.IsAtLeastPartiallyDelivered && ParentShipment.IsExport() && ParentShipment.JS_TranshipToOtherCFS && loadList.IsPackLoadList)
			{
				ParentShipment.MessageCaption = Res.GetString("a5f4dddd-0de7-4490-ab95-90928a920421", "Attach Load List");
				ParentShipment.ErrorMessage = Res.GetString("684adb80-74ef-426a-b4b4-965c4394a2ef", "This shipment has already been gate passed - you cannot export it.");
				ParentShipment.WarningMessage = "";
				ParentShipment.ContinueWithChanging = false;
			}
			else if (Count == 0 && ParentShipment.JS_JX.IsValid && ParentShipment.JS_JX != loadList.JK_JX_Sailing)
			{
				ParentShipment.MessageCaption = Res.GetString("a5f4dddd-0de7-4490-ab95-90928a920421", "Attach Load List");
				ParentShipment.WarningMessage = Res.GetString("555fe1d0-09c4-4a06-abe5-feb39a5e3126", "Shipment sailing details are not the same as the Load List Sailing Details.\r\nIf you attach the shipment the Load List sailing details will be defaulted to the shipment.\r\nProceed with shipment attachment?");
				ParentShipment.WarningMessage = "";
			}
			else if (Count > 0)
			{
				foreach (CFSLoadListConsol otherLoadList in this)
				{
					if (otherLoadList.PK != loadList.PK && otherLoadList.JK_JX_JA_RL_NKPortOfLoading == loadList.JK_JX_JA_RL_NKPortOfLoading && otherLoadList.JK_JX_JB_RL_NKPortOfDischarge == loadList.JK_JX_JB_RL_NKPortOfDischarge)
					{
						ParentShipment.MessageCaption = Res.GetString("a5f4dddd-0de7-4490-ab95-90928a920421", "Attach Load List");
						ParentShipment.ErrorMessage = Res.GetString("8bb1e184-8ada-4af0-8783-9d9bb5aa1c85", "A shipment cannot be linked to multiple Load Lists with the same port of loading and discharge.");
						ParentShipment.ErrorMessage = "";
						ParentShipment.ContinueWithChanging = false;
						break;
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
