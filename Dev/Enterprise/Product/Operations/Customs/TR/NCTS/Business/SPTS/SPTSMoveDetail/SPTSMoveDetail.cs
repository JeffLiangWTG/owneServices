using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSMoveDetail : CusInBondMoveDetail
	{
		public SPTSMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ContainerTypeCore => typeof(SPTSContainer);

		protected override Type MoveLineItemTypeCore => typeof(SPTSMoveLineItem);

		protected override Type PackTypeCore => typeof(CusInvPack);

		protected override ICusInBondContainerCollection GetContainersCollection() => new SPTSContainerCollection(this);

		#region Implementation
		public new SPTSBill Bill => (SPTSBill)base.Bill;

		public new SPTSDepartureMovementHeader MoveHeader => Factory.Load<SPTSDepartureMovementHeader>(B9_BM);
		#endregion
	}
}
