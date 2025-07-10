using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class JobDocsAndCartageLookups : AutoJobDocsAndCartageLookups
	{
		public JobDocsAndCartageLookups(AutoJobDocsAndCartage parent)
			: base(parent)
		{
		}

		#region Implementation

		public new JobDocsAndCartage Parent
		{
			get { return (JobDocsAndCartage)base.Parent; }
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region Equipment

		public CodeDescriptionPairList PickupEquipmentNeededList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent.Parent == null)
				{
					result = FCLAndLCLAIREquipmentNeededList;
				}
				else if (Parent.Parent.ContainerMode == Core.Constants.ContainerModes.BuyersConsol)
				{
					result = LCLAIREquipmentNeededList;
				}
				else
				{
					result = EquipmentNeededList;
				}
				return result;
			}
		}

		public CodeDescriptionPairList DeliveryEquipmentNeededList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent.Parent == null)
				{
					result = FCLAndLCLAIREquipmentNeededList;
				}
				else if (Parent.Parent.ContainerMode == Core.Constants.ContainerModes.BuyersConsol &&
						!(Parent.Parent.TransportMode == Core.Constants.TransportModes.Air || Parent.Parent.TransportMode == Core.Constants.TransportModes.SeaAir))
				{
					result = FCLEquipmentNeededList;
				}
				else
				{
					result = EquipmentNeededList;
				}
				return result;
			}
		}

		protected CodeDescriptionPairList EquipmentNeededList
		{
			get
			{
				ZString containerMode = Parent.Parent.ContainerMode;
				CodeDescriptionPairList result;

				if (Parent.Parent.TransportMode == Core.Constants.TransportModes.Air
					|| Parent.Parent.TransportMode == Core.Constants.TransportModes.AirSea
					|| Parent.Parent.TransportMode == Core.Constants.TransportModes.Courier
					|| containerMode == Core.Constants.ContainerModes.LCL
					|| containerMode == Core.Constants.ContainerModes.LTL
					|| containerMode == Core.Constants.ContainerModes.BreakBulk
					|| containerMode == Core.Constants.ContainerModes.NonContainerised
					|| containerMode == Core.Constants.ContainerModes.Bulk
					|| containerMode == Core.Constants.ContainerModes.Liquid
					|| containerMode == Core.Constants.ContainerModes.RollOnRollOff)
				{
					result = LCLAIREquipmentNeededList;
				}
				else if (Parent.Parent.TransportMode == Core.Constants.TransportModes.SeaAir || containerMode == Core.Constants.ContainerModes.FCL)
				{
					result = FCLEquipmentNeededList;
				}
				else if (containerMode == Core.Constants.ContainerModes.Containerised)
				{
					result = FCLEquipmentNeededList;

					foreach (IContainer container in Parent.Parent.GetContainers())
					{
						if (container.ContainerMode == Core.Constants.ContainerModes.FCL)
						{
							result = FCLEquipmentNeededList;
							break;
						}

						if (container.ContainerMode == Core.Constants.ContainerModes.LCL)
						{
							result = LCLAIREquipmentNeededList;
						}
					}
				}
				else
				{
					result = new CodeDescriptionPairList();
				}

				return result;
			}
		}

		protected FCLEquipmentNeededList FCLEquipmentNeededList
		{
			get { return Factory.GetCachedValue("JobDocsAndCartage.FCLEquipmentNeededList", () => new FCLEquipmentNeededList()); }
		}

		protected LCLAIREquipmentNeededList LCLAIREquipmentNeededList
		{
			get { return Factory.GetCachedValue("JobDocsAndCartage.LCLAIREquipmentNeededList", () => new LCLAIREquipmentNeededList()); }
		}

		protected CodeDescriptionPairList FCLAndLCLAIREquipmentNeededList
		{
			get
			{
				return Factory.GetCachedValue("JobDocsAndCartage.FCLAndLCLAIREquipmentNeededList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(LCLAIREquipmentNeededList);
					result.AddRangeOverwriteIfExists(FCLEquipmentNeededList);
					return result;
				});
			}
		}

		#endregion

		#region Exporter Statements

		public CodeDescriptionPairList JP_ExportStatementList
		{
			get
			{
				CodeDescriptionPairList result = null;
				CommonShipment shipmentInParent = Parent.ShipmentInParent;
				if (shipmentInParent != null)
				{
					result = FreightDataRegistry.Instance.ExportStatementSettings.Value.GetExportStatementDescriptionPairListForCountry(shipmentInParent.JS_RL_NKOrigin.Left(2));
				}
				else
				{
					result = new CodeDescriptionPairList();
				}

				if (result.Count == 0)
				{
					result.AddPair("DEF", Res.GetString("ed6c74c6-bdcf-447f-a16f-2ff0b4dac5d2", "Exporter Statements are defined in the System Registry located at Registry --> Freight --> Shipments --> Export Statements."));
				}

				return result;
			}
		}

		#endregion

		#region Local Transport Organisations

		public LocalTransportCollection LocalTransport_List
		{
			get { return BindingLists.OrgMiscServLocalTransport_List; }
		}

		public LocalTransportCollection LocalTransport_ListForPickup
		{
			get
			{
				CommonShipment shipment = Parent.Parent as CommonShipment;
				if (shipment != null)
				{
					return shipment.Lookups.LocalTransportAtOrigin_List;
				}
				return LocalTransport_List;
			}
		}

		public LocalTransportCollection LocalTransport_ListForDelivery
		{
			get
			{
				CommonShipment shipment = Parent.Parent as CommonShipment;
				if (shipment != null)
				{
					return shipment.Lookups.LocalTransportAtDestination_List;
				}
				return LocalTransport_List;
			}
		}

		#endregion
	}
}
