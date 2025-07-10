using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class DocketLine : IPartAttributes
	{
		public DocketLine(ZGuid pk, ZGuid productPK, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZString palletID, ZDecimal quantity, ZString packType)
		{
			PK = pk;
			ProductPK = productPK;
			PartAttrib1 = partAttrib1;
			PartAttrib2 = partAttrib2;
			PartAttrib3 = partAttrib3;
			SerialNumber = serialNumber;
			PalletID = palletID;
			Quantity = quantity;
			PackType = packType;
		}

		public readonly ZGuid PK;
		public readonly ZGuid ProductPK;
		public readonly ZDecimal Quantity;

		public ZString PartAttrib1 { get; }

		public ZString PartAttrib2 { get; }

		public ZString PartAttrib3 { get; }

		public ZString SerialNumber { get; }

		public ZString PalletID { get; }

		public ZString PackType { get; }

		// not necessary for our use
		public ZDate ExpiryDate => throw new NotImplementedException();
		public ZDate PackingDate => throw new NotImplementedException();
	}
}
