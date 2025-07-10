using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLineCollection : OuterPackLineCollection
	{
		public CFSPackLineCollection(CFSShipment master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		public new CFSPackLine AddNew()
		{
			return (CFSPackLine)base.AddNew();
		}

		public new CFSPackLine this[int index]
		{
			get { return (CFSPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CFSPackLine);
		}

		public ZString LargestLinesPackageType => Shipment.OuterPackLines.Cast<CFSPackLine>().Where(x => !x.JL_F3_NKPackType.IsEmpty).MaxBySafe(x => x.JL_PackageCount)?.JL_F3_NKPackType ?? ZString.Empty;

		#region Implementation

		protected new CFSShipment Shipment
		{
			get { return (CFSShipment)Master; }
		}

		#endregion
	}
}
