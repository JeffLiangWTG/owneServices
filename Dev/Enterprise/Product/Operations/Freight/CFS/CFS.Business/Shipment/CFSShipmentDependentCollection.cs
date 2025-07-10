using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentDependentCollection : BusinessObjectCollection<CFSShipment>
	{
		public CFSShipmentDependentCollection(BusinessObjectFactory factory, CFSContainer parent)
			: base(factory)
		{
			fParent = parent;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CFSShipment newShipment = (CFSShipment)child;
			newShipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			newShipment.JS_PackingMode = fParent.JC_ContainerMode;
			newShipment.ParentContainerRegistration = fParent;
			newShipment.JS_OH_HandledOnBehalfOfForwarder = fParent.JC_OH_CFSClient;
			newShipment.JS_JX = ZGuid.Empty;
			newShipment.JS_RL_NKOrigin = fParent.JC_JA_NKPortOfLoading;
			newShipment.JS_RL_NKDestination = fParent.JC_JB_NKPortOfDischarge;
			newShipment.ValidateTotalsAgainstPackLines = ValidateTotalsAgainstPackLines;
			newShipment.AllowSurplusPacks = AllowSurplusPacks;
		}

		public override void Load()
		{
			base.Load();

			foreach (CFSShipment shipment in Elements)
			{
				shipment.ParentContainerRegistration = fParent;
			}
		}

		protected override BusinessObject AddNewCore()
		{
			var newShipment = (CFSShipment)base.AddNewCore();
			newShipment.ParentContainerRegistration = fParent;
			newShipment.DefaultPackLine.SetContainer(fParent.PK);
			return newShipment;
		}

		public override void Add(BusinessObject businessObject)
		{
			CFSShipment newShipment = (CFSShipment)businessObject;
			base.Add(newShipment);
			newShipment.ParentContainerRegistration = fParent;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();
			CFSPackLineManyToManyCollection packLines = new CFSPackLineManyToManyCollection(fParent);
			packLines.Load();

			if (packLines.Count == 0)
			{
				filter = ZQuery.NoResultQuery;
			}
			else
			{
				filter.AddToFilter(JobShipmentSchema.PK, Array.ConvertAll(packLines.ToArray<PackLine>(), packLine => packLine.JL_JS));
			}

			return filter;
		}

		public void SetContainerModeFromParent()
		{
			foreach (CFSShipment shipment in this)
			{
				switch (fParent.JC_ContainerMode)
				{
					case Constants.ContainerModes.Groupage:
						shipment.JS_PackingMode = Constants.ContainerModes.LCL;
						break;

					case Constants.ContainerModes.AIR:
						shipment.JS_PackingMode = Constants.ContainerModes.Loose;
						break;

					default:
						shipment.JS_PackingMode = fParent.JC_ContainerMode;
						break;
				}
			}
		}

		public void SetClient(ZGuid clientPK)
		{
			foreach (CFSShipment shipment in this)
			{
				shipment.JS_OH_HandledOnBehalfOfForwarder = clientPK;
			}
		}

		public ZInt TotalPacks
		{
			get
			{
				ZInt result = 0;
				foreach (CFSShipment shipment in this)
				{
					if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster || shipment.CoLoadShipments.Count == 0)
					{
						result += shipment.JS_OuterPacks;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal result = 0;
				foreach (CFSShipment shipment in this)
				{
					if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster || shipment.CoLoadShipments.Count == 0)
					{
						result += shipment.JS_ActualWeight;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalVolume
		{
			get
			{
				ZDecimal result = 0;
				foreach (CFSShipment shipment in this)
				{
					if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster || shipment.CoLoadShipments.Count == 0)
					{
						result += shipment.JS_ActualVolume;
					}
				}
				return result;
			}
		}

		protected bool fValidateTotalsAgainstPackLines;
		public bool ValidateTotalsAgainstPackLines
		{
			get { return fValidateTotalsAgainstPackLines; }
			set
			{
				fValidateTotalsAgainstPackLines = value;

				foreach (CFSShipment shipment in this)
				{
					shipment.ValidateTotalsAgainstPackLines = value;
				}
			}
		}

		protected bool fAllowSurplusPacks;
		public bool AllowSurplusPacks
		{
			get { return fAllowSurplusPacks; }
			set
			{
				fAllowSurplusPacks = value;

				foreach (CFSShipment shipment in this)
				{
					shipment.AllowSurplusPacks = value;
				}
			}
		}

		#region Implementation

		protected readonly CFSContainer fParent;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsLoading && !IsUpdatingByDataRefreshBus)
			{
				if (fParent.Consol != null)
				{
					fParent.Consol.Shipments.Add(bizOAdded);
				}
			}
		}

		#endregion
	}
}
