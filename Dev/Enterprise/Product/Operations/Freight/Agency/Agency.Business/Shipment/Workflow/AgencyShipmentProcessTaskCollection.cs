using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentProcessTaskCollection : ProcessTaskCollection
	{
		public AgencyShipmentProcessTaskCollection(AgencyShipment shipment)
			: base(shipment)
		{
		}

		public new AgencyShipmentProcessTask this[int index]
		{
			get { return (AgencyShipmentProcessTask)Elements[index]; }
		}

		public new AgencyShipmentProcessTask AddNew()
		{
			return (AgencyShipmentProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new AgencyShipmentProcessTaskCollection(Parent);
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.Origin == null ? ZString.Empty : Parent.Origin.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.Destination == null ? ZString.Empty : Parent.Destination.RL_RN_NKCountryCode; }
		}

		#endregion

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode)
		{
			switch (conditionCode)
			{
				case AgencyShipmentWorkflowCondition1CodeList.Codes.Import:
					return Parent.IsImport();
				case AgencyShipmentWorkflowCondition1CodeList.Codes.Export:
					return Parent.IsExport();
				case AgencyShipmentWorkflowCondition1CodeList.Codes.Domestic:
					return Parent.IsDomestic();

				case AgencyShipmentWorkflowCondition1CodeList.Codes.Confirmed:
					return Parent.JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed;
				case AgencyShipmentWorkflowCondition1CodeList.Codes.Booked:
					return Parent.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked;
				case AgencyShipmentWorkflowCondition1CodeList.Codes.WaitListed:
					return Parent.JS_ShipmentStatus == ShipmentStatusList.Codes.WaitListed;

				case AgencyShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad:
					return Parent.JS_RL_NKOrigin != Parent.JS_NKLoadPort;
				case AgencyShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge:
					return Parent.JS_RL_NKDestination != Parent.JS_NKDischargePort;

				default:
					return false;
			}
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			switch (conditionCode)
			{
				case AgencyShipmentWorkflowCondition2CodeList.Codes.Import:
					return Parent.IsImport();
				case AgencyShipmentWorkflowCondition2CodeList.Codes.Export:
					return Parent.IsExport();
				case AgencyShipmentWorkflowCondition2CodeList.Codes.Domestic:
					return Parent.IsDomestic();

				case AgencyShipmentWorkflowCondition2CodeList.Codes.LQD:
					return Parent.JS_PackingMode == Core.Constants.ContainerModes.Liquid;
				case AgencyShipmentWorkflowCondition2CodeList.Codes.FCL:
					return Parent.JS_PackingMode == Core.Constants.ContainerModes.FCL;
				case AgencyShipmentWorkflowCondition2CodeList.Codes.BLK:
					return Parent.JS_PackingMode == Core.Constants.ContainerModes.Bulk;
				case AgencyShipmentWorkflowCondition2CodeList.Codes.BBK:
					return Parent.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk;
				case AgencyShipmentWorkflowCondition2CodeList.Codes.ROR:
					return Parent.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff;

				default:
					return false;
			}
		}

		#endregion

		#region Implementation

		new AgencyShipment Parent
		{
			get { return (AgencyShipment)base.Parent; }
		}

		#endregion

	}
}






