using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonContainerCollection : DependentBusinessObjectCollection<CommonContainer, BusinessObject>
	{
		public CommonContainerCollection(CommonConsol parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			ParentConsol = parent;
			IsManagedForDataRefresh = true;
		}

		protected internal CommonContainerCollection(JobSailing parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			Sailing = parent;
		}

		public CommonContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected readonly CommonConsol ParentConsol;
		readonly JobSailing Sailing;

		#region GetColumnsToFetchForEnumerate

		protected override TableColumn[] GetColumnsToFetchForEnumerate()
		{
			return new[]
			{
				new TableColumn(JobContainerPackPivotSchema.Constants.TableName, JobContainerPackPivotSchema.Constants.J6_JC),
				new TableColumn(CusContainerSchema.Constants.TableName, CusContainerSchema.Constants.CO_JC),
				new TableColumn(JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.Constants.EW_JC_Container)
			};
		}

		#endregion

		#region CheckReJoinPackLines

		public bool CheckReJoinPackLines(PackLine line)
		{
			bool result = false;
			if (QueryReJoinPackLines != null)
			{
				QueryReJoinPackLinesEventArgs e = new QueryReJoinPackLinesEventArgs(line);
				QueryReJoinPackLines(this, e);
				result = !e.Cancel;
			}
			return result;
		}

		public event QueryReJoinPackLinesEventHandler QueryReJoinPackLines;

		public class QueryReJoinPackLinesEventArgs : CancelEventArgs
		{
			public QueryReJoinPackLinesEventArgs(PackLine line) : base(false)
			{
				this.Line = line;
			}

			public readonly PackLine Line;
		}
		public delegate void QueryReJoinPackLinesEventHandler(object sender, QueryReJoinPackLinesEventArgs e);

		#endregion

		#region Find

		public CommonContainer FindAnyByContainerNumber(ZString containerNumber)
		{
			return this.Cast<CommonContainer>().FirstOrDefault(container => container.JC_ContainerNum == containerNumber.ToUpper());
		}

		public bool HasContainer(ZString containerNumber)
		{
			return FindAnyByContainerNumber(containerNumber) != null;
		}

		#endregion

		#region Pack Line Updating

		/// <summary>
		/// If false, tells collection not to update packlines when a container is added to collection.
		/// Default is true, ie. Packlines will be updated when container is added to collection.
		/// </summary>
		public bool UpdatePackLineContainerOnAdd
		{
			get { return fUpdatePackLineContainerOnAdd; }
			set { fUpdatePackLineContainerOnAdd = value; }
		}
		bool fUpdatePackLineContainerOnAdd = true;

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var newChild = (CommonContainer)child;
			if (ParentConsol != null)
			{
				newChild.JC_IsCFSRegistered = ParentConsol.JK_IsCFS;
				newChild.JC_JX = ZGuid.Empty;
				newChild.JC_OH_CFSClient = (ImportExportHelper.IsBranchCountry(ParentConsol.JK_RL_NKLoadPort)
											|| ParentConsol.IsExport())
					? ParentConsol.SendingForwarderPK
					: ParentConsol.ReceivingForwarderPK;
			}

			if (Sailing != null)
			{
				newChild.JC_JX = Sailing.PK;
			}

			SetDefaultContainerMode(newChild);
			SetDefaultDeliveryMode(newChild);
			SetDefaultAddresses(newChild);
		}

		void SetDefaultContainerMode(CommonContainer newChild)
		{
			if (ParentConsol != null)
			{
				if (ParentConsol.IsAir)
				{
					newChild.JC_ContainerMode = Constants.ContainerModes.ULD;
				}
				else //IsSea
				{
					switch (ParentConsol.JK_ConsolMode)
					{
						case Constants.ContainerModes.LCL:
						case Constants.ContainerModes.FCL:
						case Constants.ContainerModes.Groupage:
						case Constants.ContainerModes.BuyersConsol:
						case Constants.ContainerModes.ShippersConsol:
						case Constants.ContainerModes.RollOnRollOff:
							newChild.JC_ContainerMode = ParentConsol.JK_ConsolMode;
							break;

						default:
							break;
					}
				}
			}

			if (Sailing != null)
			{
				if (Sailing.JX_TransportMode == Constants.TransportModes.Air)
				{
					newChild.JC_ContainerMode = Constants.ContainerModes.ULD;
				}
				else
				{
					newChild.JC_ContainerMode = Constants.ContainerModes.Groupage;
				}
			}
		}

		void SetDefaultDeliveryMode(CommonContainer newChild)
		{
			if (ParentConsol != null && ParentConsol.IsSea)
			{
				switch (ParentConsol.JK_ConsolMode)
				{
					case Constants.ContainerModes.FCL:
						newChild.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;
						break;

					case Constants.ContainerModes.Groupage:
					case Constants.ContainerModes.LCL:
						newChild.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
						break;

					case Constants.ContainerModes.BuyersConsol:
						newChild.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CY;
						break;

					case Constants.ContainerModes.ShippersConsol:
						newChild.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CFS;
						break;

					default:
						newChild.JC_DeliveryMode = "";
						break;
				}
			}
		}

		void SetDefaultAddresses(CommonContainer newChild)
		{
			if (ParentConsol != null)
			{
				newChild.JC_OA_DepartureContainerYardAddress = ParentConsol.JK_OA_ContainerYardEmptyPickupAddress;
				newChild.JC_OA_ArrivalContainerYardAddress = ParentConsol.JK_OA_ContainerYardEmptyReturnAddress;
			}
		}

		#endregion

		#region Added

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsLoading
				&& ParentConsol != null
				&& fUpdatePackLineContainerOnAdd
				&& ParentConsol.AutomaticallyUpdatePackLineContainers
				&& !IsUpdatingByDataRefreshBus)
			{
				if (Count == 1)
				{
					foreach (CommonShipment shipment in ParentConsol.Shipments)
					{
						bool previousPackLinesHasChanges = shipment.OuterPackLines.HasChanges;
						try
						{
							ParentConsol.AllocateShipment(shipment);
						}
						finally
						{
							var container = (CommonContainer)bizOAdded;
							container.JC_GrossWeightVerificationType = container.CalculateVerificationType(shipment);

							if (container.CreatedFromCusContainer && !previousPackLinesHasChanges)
							{
								foreach (PackLine line in shipment.OuterPackLines)
								{
									line.Containers.HasChanges = false;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Removed

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (ParentConsol != null)
			{
				foreach (CommonShipment shipment in ParentConsol.Shipments)
				{
					shipment.UnpackFromContainer(bizO.PK);
				}
			}

			CommonContainer removedContainer = bizO as CommonContainer;
			if (removedContainer != null)
			{
				removedContainer.PackLines.RemoveAll();
			}
		}

		#endregion

		#region AllowNew

		protected override bool AllowNewCore
		{
			get { return Sailing == null; }
		}

		#endregion

		#region TEUCount

		public ZDecimal TEUCount
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CommonContainer container in this)
				{
					result += container.JC_Calc_TEUCount;
				}

				return result;
			}
		}

		#endregion
	}
}
