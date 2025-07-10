using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingPackLineCollection : AgencyShipmentPackLineCollection
	{
		public BillOfLadingPackLineCollection(BillOfLading master) : base(master)
		{
		}

		public new BillOfLadingPackLine AddNew()
		{
			return (BillOfLadingPackLine)base.AddNew();
		}

		public new BillOfLadingPackLine this[int index]
		{
			get { return (BillOfLadingPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(BillOfLadingPackLine);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			PackLine packline = (PackLine)child;
			packline.JL_UnitOfDimension = AgencyRegistry.Instance.DefaultBillDimensionUnit.Value;
		}

		#region Implementation

		#endregion

	}
}
