using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	[ModuleID(ModuleId.PackContainerRegistration)]
	public class CFSContainerCollection : CommonContainerCollection
	{
		public CFSContainerCollection(CFSLoadListConsol parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.Parent = parent;
			UpdatePackLineContainerOnAdd = CFSDataRegistry.Instance.AutopackContainers.Value;
		}

		public new CFSContainer this[int index]
		{
			get { return (CFSContainer)Elements[index]; }
		}

		public new CFSContainer AddNew()
		{
			return (CFSContainer)base.AddNew();
		}

		public new CFSContainer AddNew(Type bizoType)
		{
			return (CFSContainer)base.AddNew(bizoType);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Parent != null)
			{
				CFSContainer container = (CFSContainer)child;

				container.IsDefaultingFromLoadList = true;
				try
				{
					container.JC_TransportMode = Parent.JK_TransportMode;
					ZString newContainerMode = GetParentContainerMode();
					container.JC_ContainerMode = newContainerMode;
					container.JC_OH_CFSClient = Parent.JK_OH_Forwarder;
					container.JC_JK = Parent.PK;
				}
				finally
				{
					container.IsDefaultingFromLoadList = false;
				}
			}
		}

		#region SetTransportModeFromLoadList

		public void SetTransportModeFromLoadList()
		{
			if (Parent != null)
			{
				foreach (CFSContainer container in this)
				{
					container.JC_TransportMode = Parent.JK_TransportMode;
				}
			}
		}

		#endregion

		protected CFSLoadListConsol Parent;

		#region Business Object Overrides

		#region Get Parent Container Mode

		protected ZString GetParentContainerMode()
		{
			ZString result = Constants.ContainerModes.Groupage;
			if (Parent.IsAir)
			{
				result = Constants.ContainerModes.ULD;
			}
			else
			{
				if (Parent.JK_ConsolMode == Constants.ContainerModes.LCL)
				{
					result = Constants.ContainerModes.LCL;
				}
				else if (Parent.JK_ConsolMode == Constants.ContainerModes.FCL)
				{
					result = Constants.ContainerModes.FCL;
				}
			}
			return result;
		}

		#endregion

		#region Add

		public override void Add(BusinessObject businessObject)
		{
			var container = (CFSContainer)businessObject;

			if (Parent != null && !IsLoading && Parent != container.Consol)
			{
				CheckIfContainerCanBeAdded(container);
				if (!Parent.ContinueWithChanging)
				{ return; }

				Parent.ContinueWithChanging = false;
				if (!IsRefreshingByDataRefreshBus && container.Consol != null && container.Consol.Containers.Contains(container))
				{
					container.Consol.Containers.Remove(container);
				}

				container.JC_JK = Parent.PK;
			}

			base.Add(businessObject);
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			var hasPackedShipment = ((CFSContainer)elementToRemove).PackLines.OfType<CFSPackLine>().Any(line => Parent.Shipments.Contains(line.JL_JS));

			if (Parent == null || !hasPackedShipment || Factory.IsInTransaction || !StopRemovalOfContainersWithPackLine())
			{
				base.Remove(elementToRemove);
			}
		}

		bool StopRemovalOfContainersWithPackLine()
		{
			var args = new CancelEventArgs();

			if (OnRemovingContainerWithPackLines != null)
			{
				OnRemovingContainerWithPackLines(this, args);
			}

			return args.Cancel;
		}

		public event EventHandler<CancelEventArgs> OnRemovingContainerWithPackLines;

		#endregion

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((CFSContainer)bizOAdded).RefreshAllSailingFields();
		}

		#endregion

		#region OnRemoving

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			((CFSContainer)bizO).RefreshAllSailingFields();
		}

		#endregion

		#endregion

		#region Implementation

		#region CheckIfContainerCanBeAdded

		void CheckIfContainerCanBeAdded(CFSContainer container)
		{
			Parent.ContinueWithChanging = true;
			Parent.MessageCaption = Res.GetString("5f7f3ae0-b159-44b1-a754-83ef23c8e277", "Attach Container");

			if (container.JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage)
			{
				Parent.ErrorMessage = Res.GetString("f44cbec9-519e-4ae3-b855-39f02ebf1ae3", "A storage container cannot be attached to a Load List");
				Parent.ErrorMessage = "";
				Parent.ContinueWithChanging = false;
			}
			else
			{
				ZQuery filter = new ZQuery(JobContainerSchema.JC_IsCFSRegistered, ZBool.True);
				filter.AddToFilter(JobContainerSchema.JC_ContainerNum, container.JC_ContainerNum);
				filter.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, container.PK);
				filter.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.NotEqual, null);
				CFSContainer[] containers = (CFSContainer[])Factory.Load(typeof(CFSContainer), filter);

				foreach (CFSContainer otherContainer in containers)
				{
					var otherConsol = Factory.Load<CFSLoadListConsol>(otherContainer.JC_JK);
					if (otherConsol != null)
					{
						if (otherConsol.JK_OH_Forwarder == Parent.JK_OH_Forwarder && otherConsol.Schedule != null && Parent.Schedule != null && otherConsol.Schedule.PK == Parent.Schedule.PK)
						{
							Parent.ErrorMessage = Res.GetString("bc4695b7-4c82-4733-8ae3-7007af54fb02", "The container is already attached to Load List {0} on this sailing and client.\r\nIt cannot be attached to this Load List.", otherConsol.JK_UniqueConsignRef);
							Parent.ErrorMessage = "";
							Parent.ContinueWithChanging = false;
							break;
						}
					}
				}

				if (Parent.ContinueWithChanging)
				{
					var currentConsol = Factory.Load<CFSLoadListConsol>(container.JC_JK);
					if (currentConsol != null && currentConsol.PK != Parent.PK)
					{
						if (container.PackLines.Count > 0)
						{
							Parent.ErrorMessage = Res.GetString("917cd0c8-49f7-4313-8a3f-97672a5da306", "The container is already attached to Load List {0}.\r\nThe container has been packed therefore cannot be attached to this Load List.", currentConsol.JK_UniqueConsignRef);
							Parent.ErrorMessage = "";
							Parent.ContinueWithChanging = false;
						}
						else
						{
							Parent.WarningMessage = Res.GetString("8987fa43-40ca-4cae-ac96-6778d4c3eb23", "The container is already attached to Load List {0}.\r\nDo you want to detach it from that Load List and attach to this one?", currentConsol.JK_UniqueConsignRef);
							Parent.WarningMessage = "";
						}
					}

					if (Parent.ContinueWithChanging)
					{
						if (container.JC_OH_CFSClient != Parent.JK_OH_Forwarder || (Parent.Schedule != null && container.JC_JX != Parent.Schedule.PK))
						{
							Parent.WarningMessage = Res.GetString("9c4c431f-3396-48cf-a105-bc928edc75ca", "Container Client details are not the same as the Load List Client details.\r\nIf you attach the container the Load List Client details will be defaulted to the container registration.\r\nProceed with container attachment?");
							Parent.WarningMessage = "";
						}
					}
				}
			}
		}

		#endregion

		#endregion

	}
}
