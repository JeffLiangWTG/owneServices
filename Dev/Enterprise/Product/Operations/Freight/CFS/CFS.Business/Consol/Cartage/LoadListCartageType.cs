using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.CFS.Business
{
	public abstract class LoadListCartageType : CartageType
	{
		protected LoadListCartageType(CFSLoadListConsol loadList) : base(loadList) { }

		protected CFSLoadListConsol LoadListParent
		{
			get { return (CFSLoadListConsol)CartageParent; }
		}

		public override void CartageAdvised(BusinessObjectFactory factoryToSaveIn)
		{
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get { return (ICartageContainer[])LoadListParent.Containers.ToArray(typeof(ICartageContainer)); }
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return (ICartageLooseCargo[])LoadListParent.RelatedPackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		public override ZString PortOfLoading
		{
			get { return LoadListParent.JK_RL_NKLoadPort; }
		}

		public override ZString PortOfDischarge
		{
			get { return LoadListParent.JK_RL_NKDischargePort; }
		}

		public override ZString Vessel
		{
			get { return LoadListParent.JK_JX_JV_NKVessel; }
		}

		public override ZString VoyageFlight
		{
			get { return LoadListParent.JK_JX_JV_VoyageFlight; }
		}

		public override ZDateTime E_ARV
		{
			get { return LoadListParent.JK_JX_JB_E_ARV; }
		}

		public override ZDateTime E_DEP
		{
			get { return LoadListParent.JK_JX_JA_E_DEP; }
		}

		public override ZDateTime A_ARV
		{
			get { return LoadListParent.JK_JX_JB_A_ARV; }
		}

		public override ZDateTime A_DEP
		{
			get { return LoadListParent.JK_JX_JA_A_DEP; }
		}

		public override ZDateTime FCLAvailabilityDate
		{
			get { return LoadListParent.JK_CTOAvailabilityDate; }
		}

		public override ZDateTime FCLStorageDate
		{
			get { return LoadListParent.JK_CTOStorageDate; }
		}

		public override ZDateTime LCLAvailabilityDate
		{
			get { return LoadListParent.JK_DepotAvailabilityDate; }
		}

		public override ZDateTime LCLStorageDate
		{
			get { return LoadListParent.JK_DepotStorageDate; }
		}

		public override ZDateTime FCLCutOff
		{
			get { return LoadListParent.JK_CTOCutOff; }
		}

		public override ZDateTime FCLReceivalCommences
		{
			get { return LoadListParent.JK_CTOReceivalCommences; }
		}

		public override ZDateTime LCLCutOff
		{
			get { return LoadListParent.JK_DepotCutOff; }
		}

		public override ZDateTime LCLReceivalCommences
		{
			get { return LoadListParent.JK_DepotReceivalCommences; }
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return ZDateTime.Empty; }
		}

		protected void AddOrSetCompletedConfirm(CommonPickupDeliveryConfirmCollection confirms, ZDateTime timeOut)
		{
			if (confirms.Count > 1)
			{
				return;
			}

			CommonPickupDeliveryConfirm confirm = confirms.Count == 1 ? confirms[0] : confirms.AddNew();
			confirm.EU_PickupDeliveryTime = timeOut;
		}
	}
}
