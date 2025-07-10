using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// PackLine specialised for making gate passes for CFS.
	/// </summary>
	public class GatePassPackLine : CFSPackLine
	{
		public GatePassPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			HasNewDeliveryDetails = false;
		}

		public bool HasNewDeliveryDetails;

		#region Additional Methods

		public bool MatchingDimensions(GatePassPackLine otherPack)
		{
			return otherPack.JL_F3_NKPackType == JL_F3_NKPackType &&
				otherPack.JL_Length == JL_Length &&
				otherPack.JL_Width == JL_Width &&
				otherPack.JL_Height == JL_Height &&
				otherPack.JL_UnitOfDimension == JL_UnitOfDimension;
		}

		#endregion

		#region Properties

		#region UseOutturn

		public override bool UseOutturn
		{
			get { return !(Shipment != null && Shipment.IsExport() && JL_Outturn == 0); }
		}

		#endregion

		#region printing

		public ZString PackTypeDescription
		{
			get { return JL_F3_NKPackType_List.GetDescriptionFromCode(JL_F3_NKPackType); }
		}

		#endregion

		#region JL_Calc_OutturnUndelivered

		//		public ZInt JL_Calc_OutturnUndelivered
		//		{
		//			get { return JL_Outturn - TotalDelivered; }
		//		}
		//
		//		public ZPropertyInfo JL_Calc_OutturnUndeliveredInfo
		//		{
		//			get { return GetZPropertyInfo(Schema.JL_Calc_OutturnUndelivered); }
		//		}

		#endregion

		#region IsFullyDelivered

		public override bool PerformWarehouseQuantityValidation
		{
			get { return false; }
		}

		#endregion

		#region Overidden for Light Validation

		#endregion

		#endregion

		#region Related Business Objects

		public new GatePassShipment Shipment
		{
			get { return (GatePassShipment)base.Shipment; }
		}

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<GatePassShipment>(JL_JS);
		}

		protected override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new GatePassContainerManyToManyCollection(this);
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<GatePassContainer>(containerPK);
		}

		#endregion

		#region Code lists

		public static RefPackTypeCollection PackType_List
		{
			get
			{
				return new RefPackTypeCollection(new BusinessObjectFactory());
			}
		}

		#endregion

		#region Implementation

		protected string CalcTransportMode
		{
			get { return Shipment == null ? "" : (string)Shipment.JS_TransportMode; }
		}

		#endregion
	}
}
