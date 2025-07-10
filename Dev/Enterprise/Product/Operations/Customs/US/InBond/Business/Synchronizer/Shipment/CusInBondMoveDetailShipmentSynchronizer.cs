using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailShipmentSynchronizer : BusinessObjectSynchroniser
	{
		public CusInBondMoveDetailShipmentSynchronizer(CusInBondMoveDetail destination, CusInBondBill source)
			: base(destination, source)
		{
		}

		protected new CusInBondMoveDetail Destination
		{
			get { return (CusInBondMoveDetail)base.Destination; }
		}

		protected CusInBondBill Bill
		{
			get { return (CusInBondBill)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.B9_B0Info, GetBillNumber, GetBillRelatedInfos));
			var consol = Bill.Header.Consol;
			if (consol != null)
			{
				Synchronisers.Add(new CusInBondContainerCollectionShipmentSynchroniser(Destination.Header.Parent as ForwardingShipment, Destination, consol, Bill.ShouldSynchronise, Destination.Containers));
			}
		}

		#region Bill Number

		IZType GetBillNumber()
		{
			return Bill.PK;
		}

		IEnumerable<ZPropertyInfo> GetBillRelatedInfos()
		{
			yield return Destination.B9_B0Info;
		}

		#endregion
	}
}
